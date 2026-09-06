using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Runestone.AesirArchitecture;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
#endif

namespace Runestone.AesirModules
{
    /// <summary>
    /// Binder 脚本生成模式。
    /// </summary>
    public enum BinderScriptMode
    {
        /// <summary>
        /// partial 分部类: 生成自动维护文件（后缀可选，默认 <c>.designer.cs</c>，整体覆盖）+
        /// 手写 partial <c>*.cs</c>（仅首次生成）。
        /// </summary>
        [InspectorName("Partial 分部类")]
        PartialClass,

        /// <summary>
        /// 同一脚本增量: 只替换目标脚本内「绑定字段（自动生成）」region 的内容（含 BindComponents 方法），
        /// region 外的内容归开发者所有。
        /// </summary>
        [InspectorName("同一脚本增量")]
        SameScriptIncrement
    }

    /// <summary>
    /// Object Binder 核心组件。挂载在根面板上，统一配置所有要绑定的子组件并一键生成绑定脚本。
    /// <para>
    /// 工作流程：
    /// 1. 在需要绑定引用的子物体上添加 <see cref="BinderTag" /> 组件标记（默认绑定 1 个组件），用数量声明要绑定的组件个数。
    /// 2. 在本组件上点击「构建绑定单元」——按标记<b>增量</b>更新 <see cref="BinderInfo" /> 列表
    ///    （新增缺失单元、刷新已有单元路径、移除标记已删除或数量缩减的单元），每个单元记录
    ///    组件类型、字段名、绑定路径等配置。
    /// 3. 点击「生成脚本」，按生成模式产出代码：
    ///    - <see cref="BinderScriptMode.PartialClass" />：<c>*.generated.cs</c>（自动维护，整体覆盖）+
    ///      <c>*.cs</c>（开发者手写业务逻辑的 partial 类，仅首次生成，<b>不会被覆盖</b>）；
    ///    - <see cref="BinderScriptMode.SameScriptIncrement" />：只替换目标脚本
    ///      <c>*.cs</c> 内「绑定字段（自动生成）」region 的内容（字段 + <c>BindComponents()</c> 方法），
    ///      region 外的内容归开发者所有；文件不存在时先创建脚手架。
    /// 4. 编译完成后自动把生成脚本挂载到当前 GameObject 并执行一次绑定。
    /// </para>
    /// <para>
    /// 生成脚本的基类可从「基类」下拉中选择：内置 <see cref="MonoBehaviour" />、
    /// 由 Binder 预选的 Aesir 面板家族（<see cref="AesirBasePanel" />、<c>AesirBasePanelView&lt;T&gt;</c>、
    /// <c>AesirBasePanelViewController&lt;T&gt;</c>——核心程序集无法反向引用 Odin 程序集标注
    /// <see cref="BinderBaseTypeAttribute" />，故经 typeof 直接内置），
    /// 以及所有被 <see cref="BinderBaseTypeAttribute" /> 标记的用户类
    /// （选择 Aesir 泛型面板基类后在「Context 类型」下拉中选择项目内的 AbstractContext 派生类，
    /// 占位不会写进生成代码）。
    /// </para>
    /// </summary>
    [DetailedInfoBox("Binder 使用说明（工作流 / 自动检查时机）",
        "【工作流】\n" +
        "① 在需要绑定引用的子物体上挂 BinderTag 标记（可通过层级右键菜单「GameObject/Aesir/添加 BinderTag 标记」快速添加），默认绑定 1 个组件，用「绑定组件数量」声明要绑定的组件个数；\n" +
        "② 选中根面板上的 BinderAssistant，点击「构建绑定单元」——按标记增量更新绑定列表（新增缺失单元、刷新已有单元路径、移除失效单元），每个单元记录组件类型、字段名、绑定路径；\n" +
        "③ 点击「生成脚本」——「同一脚本增量」模式（默认）只替换目标脚本内「绑定字段（自动生成）」region 的内容（字段 + BindComponents 方法），region 外归开发者所有，文件不存在时先创建脚手架；「Partial 分部类」模式生成自动维护文件（后缀可选，默认 .designer.cs）与手写 partial 文件（仅首次生成，不会被覆盖）；\n" +
        "④ 编译完成后自动把生成脚本挂载到当前物体并执行一次绑定。\n" +
        "\n【自动检查时机】\n" +
        "「开启自动检查」在以下时机执行：\n" +
        "① 脚本重编译完成后（每次进入 Play、修改脚本触发编译等都会重编译）；\n" +
        "② 点击「构建绑定单元」或「检查绑定」按钮时。\n" +
        "注意：在编辑器内移动物体层级不会实时触发检查——路径漂移会在下次重编译或手动点击「检查绑定」时提示。")]
    [DisallowMultipleComponent]
    public class BinderAssistant : AesirMonoBehaviour
    {
        /// <summary>EditorPrefs 键: 待自动挂载的物体 InstanceID。</summary>
        const string PendingBindInstanceIdKey = "AesirModules.BinderAssistant.PendingBind.InstanceId";

        /// <summary>EditorPrefs 键: 待自动挂载的脚本类型完整名称。</summary>
        const string PendingBindTypeKey = "AesirModules.BinderAssistant.PendingBind.TypeFullName";

        [PropertyOrder(-10)]
        [HorizontalGroup("状态")]
        [ToggleLeft]
        [LabelText("开启自动检查")]
        public bool OpenAutoValidate = true;

        [PropertyOrder(-9)]
        [HorizontalGroup("状态")]
        [ShowInInspector]
        [Sirenix.OdinInspector.ReadOnly]
        [LabelText("当前绑定信息有错误")]
        public bool HasError { get; private set; }

        [FoldoutGroup("生成配置")]
        [LabelText("生成模式")]
        public BinderScriptMode ScriptMode = BinderScriptMode.SameScriptIncrement;

        [FoldoutGroup("生成配置")]
        [LabelText("命名空间: ")]
        [InlineButton(nameof(DefaultNamespace), "默认")]
        public string TargetNamespace;

        [FoldoutGroup("生成配置")]
        [LabelText("脚本名: ")]
        [InlineButton(nameof(DefaultScriptName), "默认")]
        public string ScriptName;

        [FoldoutGroup("生成配置")]
        [ValueDropdown(nameof(GetBaseTypes))]
        [LabelText("基类: ")]
        public string BaseType;

        [FoldoutGroup("生成配置")]
        [ShowIf(nameof(IsAesirGenericPanelBase))]
        [ValueDropdown(nameof(GetContextTypeChoices))]
        [LabelText("Context 类型: ")]
        [InfoBox("$NoContextHint", InfoMessageType.Warning, nameof(NoContextAvailable))]
        public string ContextTypeName;

        [FoldoutGroup("生成配置")]
        [ShowIf(nameof(IsUserGenericBase))]
        [LabelText("泛型参数: ")]
        public string BaseTypeArguments;

        [FoldoutGroup("生成配置")]
        [ShowIf(nameof(IsPartialMode))]
        [ValueDropdown(nameof(GetPartialSuffixOptions))]
        [LabelText("生成文件后缀: ")]
        [Tooltip("Rider 中 .designer.cs 是默认折叠的，Rider 用户推荐使用")]
        [OnValueChanged(nameof(SaveDefaultPartialSuffix))]
        public string PartialSuffix = ".designer.cs";

        [FoldoutGroup("生成配置")]
        [ShowIf(nameof(IsPartialMode))]
        [LabelText("可选后缀列表（编辑器持久化）")]
        [ShowInInspector]
        [OnValueChanged(nameof(SavePartialSuffixes))]
        public List<string> PartialSuffixList => BinderEditorSettings.Settings.PartialSuffixes;

        [FoldoutGroup("生成配置")]
        [LabelText("目标文件夹: ")]
        [InlineButton(nameof(DefaultFolderPath), "默认")]
        [FolderPath]
        public string FolderPath;

        [FoldoutGroup("生成配置")]
        [LabelText("附加 using 命名空间")]
        public List<string> CustomNamespaces = new List<string>();

        [InfoBox("$ErrorHint", InfoMessageType.Error, nameof(HasError))]
        [Title("绑定单元列表")]
        [TableList(AlwaysExpanded = true, IsReadOnly = true)]
        public List<BinderInfo> Units = new List<BinderInfo>();

        /// <summary>最近一次校验的错误明细，供 Inspector 错误提示框展示。</summary>
        List<string> _lastValidationErrors;

        /// <summary>
        /// 当前物体在场景层级中的绝对路径
        /// </summary>
        public string HierarchyPath => BinderHierarchyUtility.GetAbsolutePath(transform);

        /// <summary>当前是否为 partial 分部类模式。</summary>
        bool IsPartialMode => ScriptMode == BinderScriptMode.PartialClass;

        /// <summary>
        /// 基类候选是否为 Aesir 泛型面板基类（AesirBasePanelView&lt;T&gt; / AesirBasePanelViewController&lt;T&gt;），
        /// 决定「Context 类型」下拉的显示。
        /// </summary>
        bool IsAesirGenericPanelBase =>
            BaseType != null && (BaseType.StartsWith("Runestone.AesirModules.AesirBasePanelView<") ||
                                 BaseType.StartsWith("Runestone.AesirModules.AesirBasePanelViewController<"));

        /// <summary>
        /// 基类候选是否为用户自定义泛型基类（非 Aesir 面板家族的泛型占位），决定「泛型参数」文本框的显示。
        /// </summary>
        bool IsUserGenericBase => BinderCodeGenerator.HasGenericPlaceholder(BaseType) && !IsAesirGenericPanelBase;

        /// <summary>
        /// 生成时实际使用的泛型类型参数: Aesir 泛型面板基类取「Context 类型」下拉，其余取「泛型参数」文本。
        /// </summary>
        string EffectiveBaseTypeArguments => IsAesirGenericPanelBase ? ContextTypeName : BaseTypeArguments;

        /// <summary>项目中是否不存在 AbstractContext 派生类（用于空列表提示）。</summary>
        bool NoContextAvailable => IsAesirGenericPanelBase && GetContextTypeChoices().Count == 0;

        string NoContextHint =>
            "项目中未找到 AbstractContext 派生类。请先创建 Context 类型，例如:\n" +
            "public class HUDContext : AbstractContext<HUDContext> { protected override void Configure() { } }";

        /// <summary>自动维护脚本的输出路径（partial 模式为 ScriptName + 所选后缀，增量模式为 *.cs）。</summary>
        string GeneratedScriptPath =>
            Path.Combine(FolderPath, ScriptName + (IsPartialMode ? PartialSuffix : ".cs"));

        /// <summary>开发者手写 partial 类的输出路径（仅 partial 模式使用）。</summary>
        string ControllerScriptPath => Path.Combine(FolderPath, ScriptName + ".cs");

        string ErrorHint => _lastValidationErrors != null && _lastValidationErrors.Count > 0
            ? $"绑定信息存在 {_lastValidationErrors.Count} 处错误，点击「检查绑定」在 Console 查看明细。"
            : null;

        void Reset()
        {
#if UNITY_EDITOR
            // 命名空间默认值取最近一次成功生成的命名空间（ScriptableSingleton 持久化）
            TargetNamespace = BinderEditorSettings.Settings.LastNamespace;
            PartialSuffix = BinderEditorSettings.Settings.DefaultPartialSuffix;
            ScriptMode = BinderScriptMode.SameScriptIncrement;
#endif
            DefaultScriptName();
            BaseType = typeof(MonoBehaviour).FullName;
            DefaultFolderPath();
        }

        void DefaultNamespace()
        {
#if UNITY_EDITOR
            TargetNamespace = BinderEditorSettings.Settings.LastNamespace;
#else
            TargetNamespace = "Game";
#endif
        }

        void DefaultScriptName()
        {
            ScriptName = gameObject.name + "Panel";
        }

        void DefaultFolderPath()
        {
            FolderPath = "Assets/Scripts";
        }

        /// <summary>
        /// 可选基类下拉列表: 内置 <see cref="MonoBehaviour" />，加上由 Binder 预选的 Aesir 面板家族
        /// （<see cref="AesirBasePanel" /> / <c>AesirBasePanelView&lt;T&gt;</c> /
        /// <c>AesirBasePanelViewController&lt;T&gt;</c>——核心程序集无法反向引用 Odin 程序集标注
        /// <see cref="BinderBaseTypeAttribute" />，故经 typeof 直接内置），
        /// 以及所有被 <see cref="BinderBaseTypeAttribute" /> 标记的 MonoBehaviour 派生类；
        /// 泛型基类以 <c>&lt;T&gt;</c> 占位形式提供，选择后需把占位替换为具体类型参数。
        /// </summary>
        public ValueDropdownList<string> GetBaseTypes()
        {
            var list = new ValueDropdownList<string>
            {
                new ValueDropdownItem<string>(nameof(MonoBehaviour), typeof(MonoBehaviour).FullName)
            };

            // 框架预选基类（Aesir 面板家族）: Odin 程序集单向引用核心程序集，直接 typeof 引用
            AddBaseTypeCandidate(list, typeof(AesirBasePanel));
            AddBaseTypeCandidate(list, typeof(AesirBasePanelView<>));
            AddBaseTypeCandidate(list, typeof(AesirBasePanelViewController<>));

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException)
                {
                    // 个别程序集类型加载失败时跳过，不影响下拉列表构建
                    continue;
                }

                foreach (var type in types)
                {
                    if (!typeof(MonoBehaviour).IsAssignableFrom(type) ||
                        !type.IsDefined(typeof(BinderBaseTypeAttribute), false))
                    {
                        continue;
                    }

                    AddBaseTypeCandidate(list, type);
                }
            }

            return list;
        }

        /// <summary>
        /// 向基类下拉列表追加一个候选: 泛型类型定义以 <c>&lt;T&gt;</c> 占位形式呈现，按值去重。
        /// </summary>
        static void AddBaseTypeCandidate(ValueDropdownList<string> list, Type type)
        {
            var value = type.IsGenericTypeDefinition
                ? BinderCodeGenerator.ConvertArityToPlaceholders(type.FullName)
                : type.FullName;

            if (list.All(item => item.Value != value))
            {
                var display = type.IsGenericTypeDefinition
                    ? BinderCodeGenerator.ConvertArityToPlaceholders(type.Name)
                    : type.Name;
                list.Add(new ValueDropdownItem<string>(display, value));
            }
        }

        /// <summary>已扫描的 AbstractContext 派生类缓存（域重载自动失效）。</summary>
        static List<ValueDropdownItem<string>> _contextTypeChoicesCache;

        /// <summary>
        /// 「Context 类型」下拉列表: 项目内所有具体的 AbstractContext 派生类（CRTP 闭合类型）。
        /// 被 <see cref="InternalContextAttribute" /> 标记的框架内部 Context（示例 / 测试）不会出现。
        /// </summary>
        public ValueDropdownList<string> GetContextTypeChoices()
        {
            if (_contextTypeChoicesCache != null)
            {
                var cached = new ValueDropdownList<string>();
                foreach (var item in _contextTypeChoicesCache)
                {
                    cached.Add(item);
                }

                return cached;
            }

            var list = new ValueDropdownList<string>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException)
                {
                    continue;
                }

                foreach (var type in types)
                {
                    if (!type.IsClass || type.IsAbstract || !DerivesFromAbstractContext(type))
                    {
                        continue;
                    }

                    // 框架内部 Context（示例 / 测试，[InternalContext] 标注）不出现在用户选择器
                    if (type.IsDefined(typeof(InternalContextAttribute), false))
                    {
                        continue;
                    }

                    if (list.All(item => item.Value != type.FullName))
                    {
                        list.Add(new ValueDropdownItem<string>(type.Name, type.FullName));
                    }
                }
            }

            _contextTypeChoicesCache = new List<ValueDropdownItem<string>>(list);
            return list;
        }

        /// <summary>
        /// 判断类型（沿基类链）是否派生自开放泛型 <see cref="AbstractContext{T}" />。
        /// </summary>
        static bool DerivesFromAbstractContext(Type type)
        {
            var baseType = type.BaseType;
            while (baseType != null)
            {
                if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(AbstractContext<>))
                {
                    return true;
                }

                baseType = baseType.BaseType;
            }

            return false;
        }

        /// <summary>
        /// 在所有已加载的非动态程序集中按完整名称解析类型（运行时可用，供校验与自动挂载共用）。
        /// </summary>
        static Type ResolveTypeByFullName(string typeFullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsDynamic)
                {
                    continue;
                }

                var type = assembly.GetType(typeFullName, false);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        /// <summary>
        /// 「生成文件后缀」下拉选项: 来自 ScriptableSingleton 持久化的后缀列表。
        /// </summary>
        public ValueDropdownList<string> GetPartialSuffixOptions()
        {
#if UNITY_EDITOR
            var list = new ValueDropdownList<string>();
            foreach (var suffix in BinderEditorSettings.Settings.PartialSuffixes)
            {
                list.Add(suffix, suffix);
            }

            return list;
#else
            return new ValueDropdownList<string> { { ".designer.cs", ".designer.cs" } };
#endif
        }

        void SaveDefaultPartialSuffix()
        {
#if UNITY_EDITOR
            BinderEditorSettings.Settings.SetDefaultPartialSuffix(PartialSuffix);
#endif
        }

        void SavePartialSuffixes()
        {
#if UNITY_EDITOR
            BinderEditorSettings.Settings.Save();
#endif
        }

        [ButtonGroup("操作")]
        [Button("构建绑定单元")]
        void CreateUnits()
        {
            var tags = GetComponentsInChildren<BinderTag>(true);

            // 第一步: 刷新已有单元路径并统计每个标记名下的单元数量；
            // 标记已被移除（或物体已销毁）的单元直接删除
            var unitCountPerTag = new Dictionary<UnityEngine.Object, int>();
            for (var i = Units.Count - 1; i >= 0; i--)
            {
                var unit = Units[i];
                if (unit.LabelObj && unit.LabelObj.TryGetComponent<BinderTag>(out var tag) && tags.Contains(tag))
                {
                    unit.UpdatePath(this);
                    unitCountPerTag[unit.LabelObj] = unitCountPerTag.GetValueOrDefault(unit.LabelObj) + 1;
                }
                else
                {
                    Units.RemoveAt(i);
                }
            }

            // 第二步: 按各标记声明的数量补齐差额或移除超额单元，保持列表与标记一致
            foreach (var label in tags)
            {
                var current = unitCountPerTag.GetValueOrDefault(label.SelfObj);
                while (current < label.ComponentNumber)
                {
                    var unit = new BinderInfo(this, label);
                    unit.FieldName = ComputeUniqueFieldName(unit);
                    Units.Add(unit);
                    current++;
                }

                while (current > label.ComponentNumber && RemoveLastUnitOf(label))
                {
                    current--;
                }
            }

            ValidateBindings();
        }

        [ButtonGroup("操作")]
        [Button("生成脚本")]
        void GenerateCode()
        {
            if (!ValidateBindings())
            {
                AesirModulesDebug.LogError(AesirModulesDebug.ObjectBinderTag, $"{name}: 存在校验错误，已取消生成脚本");
                return;
            }

            try
            {
#if UNITY_EDITOR
                EnsureFolderExists(FolderPath);
                // 记录最近使用的命名空间，作为新建 BinderAssistant 的默认值（ScriptableSingleton 持久化）
                BinderEditorSettings.Settings.SetLastNamespace(TargetNamespace);
#endif
                var config = BuildCodeGenConfig();

                if (IsPartialMode)
                {
                    WritePartialScripts(config);
                }
                else
                {
                    WriteIncrementalScript(config);
                }

#if UNITY_EDITOR
                // 暂存目标物体与脚本类型，编译完成后由 AttachToGameObject 自动挂载并绑定
                EditorPrefs.SetInt(PendingBindInstanceIdKey, gameObject.GetInstanceID());
                EditorPrefs.SetString(PendingBindTypeKey, TargetNamespace + "." + ScriptName);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
#endif
            }
            catch (Exception ex)
            {
                AesirModulesDebug.LogError(AesirModulesDebug.ObjectBinderTag, $"生成脚本失败: {ex.Message}");
            }
        }

        /// <summary>
        /// partial 分部类模式: generated 脚本整体覆盖，controller 脚本仅首次生成。
        /// </summary>
        void WritePartialScripts(BinderCodeGenerator.CodeGenConfig config)
        {
            var generatedPath = GeneratedScriptPath;
            var controllerPath = ControllerScriptPath;

            File.WriteAllText(generatedPath, BinderCodeGenerator.BuildGeneratedScript(config), new UTF8Encoding(false));

            // Controller 脚本（开发者手写区）仅生成一次，避免覆盖用户业务逻辑
            var controllerCreated = false;
            if (!File.Exists(controllerPath))
            {
                File.WriteAllText(controllerPath, BinderCodeGenerator.BuildControllerScript(config),
                    new UTF8Encoding(false));
                controllerCreated = true;
            }

#if UNITY_EDITOR
            AssetDatabase.ImportAsset(generatedPath);
            AssetDatabase.ImportAsset(controllerPath);
#endif
            AesirModulesDebug.Log(this, AesirModulesDebug.ObjectBinderTag,
                controllerCreated
                    ? $"成功生成脚本: {generatedPath}（含首次生成的 {controllerPath}）"
                    : $"成功生成脚本: {generatedPath}");
        }

        /// <summary>
        /// 同一脚本增量模式: 目标文件不存在时创建脚手架，存在时仅替换 region 内容。
        /// </summary>
        void WriteIncrementalScript(BinderCodeGenerator.CodeGenConfig config)
        {
            var scriptPath = GeneratedScriptPath;

            if (!File.Exists(scriptPath))
            {
                File.WriteAllText(scriptPath, BinderCodeGenerator.BuildIncrementalScaffold(config),
                    new UTF8Encoding(false));
                AesirModulesDebug.Log(this, AesirModulesDebug.ObjectBinderTag,
                    $"已创建同一脚本增量模式脚手架: {scriptPath}");
            }
            else
            {
                var content = File.ReadAllText(scriptPath);
                if (!BinderCodeGenerator.TryReplaceRegion(content, BinderCodeGenerator.BuildRegionBlock(config),
                        out var updated))
                {
                    AesirModulesDebug.LogError(AesirModulesDebug.ObjectBinderTag,
                        $"{name}: 未在 {scriptPath} 中找到「{BinderCodeGenerator.BindFieldRegionName}」region，" +
                        $"请在类体内添加 {BinderCodeGenerator.RegionStartMarker} … {BinderCodeGenerator.RegionEndMarker} 后重试");
                    return;
                }

                File.WriteAllText(scriptPath, updated, new UTF8Encoding(false));
                AesirModulesDebug.Log(this, AesirModulesDebug.ObjectBinderTag,
                    $"已增量更新绑定 region: {scriptPath}");
            }

#if UNITY_EDITOR
            AssetDatabase.ImportAsset(scriptPath);
#endif
        }

        [ButtonGroup("操作")]
        [Button("检查绑定")]
        void CheckBindings()
        {
            ValidateBindings();
        }

        /// <summary>
        /// 为新增绑定单元生成唯一的默认字段名: 以 BinderInfo 默认名为基底，重名时追加序号。
        /// </summary>
        string ComputeUniqueFieldName(BinderInfo unit)
        {
            var baseName = unit.FieldName;
            if (!IsFieldNameTaken(baseName, unit))
            {
                return baseName;
            }

            var suffix = 2;
            while (IsFieldNameTaken(baseName + suffix, unit))
            {
                suffix++;
            }

            return baseName + suffix;
        }

        bool IsFieldNameTaken(string fieldName, BinderInfo exclude)
        {
            return Units.Any(unit => unit != exclude && unit.FieldName == fieldName);
        }

        bool RemoveLastUnitOf(BinderTag label)
        {
            for (var i = Units.Count - 1; i >= 0; i--)
            {
                if (Units[i].LabelObj != label.SelfObj)
                {
                    continue;
                }

                Units.RemoveAt(i);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 校验生成配置与所有绑定单元，错误明细写入 Console 并刷新 <see cref="HasError" />。
        /// 命名空间与基类校验仅在脚本由生成器维护时执行
        /// （partial 模式，或增量模式下目标文件尚不存在需创建脚手架）。
        /// </summary>
        /// <returns>校验通过返回 true。</returns>
        bool ValidateBindings()
        {
            var errors = new List<string>();

            // 增量模式下目标文件已存在时，命名空间与基类由开发者脚本自行声明，跳过校验
            var scaffoldNeeded = IsPartialMode || !File.Exists(ControllerScriptPath);
            if (scaffoldNeeded)
            {
                if (!BinderCodeGenerator.IsValidNamespace(TargetNamespace))
                {
                    errors.Add($"命名空间 \"{TargetNamespace}\" 不是合法的 C# 命名空间");
                }

                if (string.IsNullOrEmpty(BaseType))
                {
                    errors.Add("基类不能为空");
                }
                else if (BaseType.Contains('`'))
                {
                    errors.Add("基类包含泛型元数后缀（`），请从「基类」下拉重新选择");
                }
                else if (BinderCodeGenerator.HasGenericPlaceholder(BaseType))
                {
                    var arguments = (EffectiveBaseTypeArguments ?? "").Trim();
                    if (arguments.Length == 0)
                    {
                        errors.Add(
                            $"泛型基类 \"{BaseType}\" 需填写具体类型参数" +
                            "（Context 类型需继承 AbstractContext<T>，如 Game.HUDContext）");
                    }
                    else if (IsAesirGenericPanelBase)
                    {
                        var contextType = ResolveTypeByFullName(arguments);
                        if (contextType == null)
                        {
                            errors.Add($"未找到 Context 类型 \"{arguments}\"");
                        }
                        else if (!DerivesFromAbstractContext(contextType))
                        {
                            errors.Add($"\"{arguments}\" 不是 AbstractContext 派生类");
                        }
                    }
                    else
                    {
                        var arity = BinderCodeGenerator.GetGenericPlaceholderArity(BaseType);
                        var argumentCount = arguments.Split(',').Length;
                        if (argumentCount != arity)
                        {
                            errors.Add($"泛型基类需要 {arity} 个类型参数，当前「泛型参数」填写了 {argumentCount} 个");
                        }
                    }
                }
            }

            if (!BinderCodeGenerator.IsValidIdentifier(ScriptName))
            {
                errors.Add($"脚本名 \"{ScriptName}\" 不是合法的 C# 标识符");
            }

            if (IsPartialMode)
            {
                if (string.IsNullOrEmpty(PartialSuffix))
                {
                    errors.Add("「生成文件后缀」不能为空");
                }
                else if (PartialSuffix == ".cs")
                {
                    errors.Add("「生成文件后缀」不能是 .cs（与手写 partial 文件冲突）");
                }
            }

            if (string.IsNullOrEmpty(FolderPath) || !FolderPath.StartsWith("Assets"))
            {
                errors.Add($"目标文件夹 \"{FolderPath}\" 必须位于 Assets/ 之下");
            }

            if (Units.Count == 0)
            {
                errors.Add("绑定单元列表为空，请先在子物体上挂 BinderTag 后点击「构建绑定单元」");
            }

            foreach (var unit in Units)
            {
                var displayName = unit.LabelObj ? unit.LabelObj.name : "<已丢失物体>";
                if (!unit.LabelObj)
                {
                    errors.Add($"[{displayName}] 绑定物体已丢失，请重新「构建绑定单元」");
                    continue;
                }

                if (!unit.LabelObj.TryGetComponent<BinderTag>(out _))
                {
                    errors.Add($"[{displayName}] 缺少 BinderTag 标记，请重新「构建绑定单元」");
                    continue;
                }

                // 空字符串表示绑定自身（标记与 Assistant 在同一物体上），是合法配置
                if (unit.HierarchyPath == null)
                {
                    errors.Add($"[{displayName}] 无法解析相对路径，请重新「构建绑定单元」");
                }

                if (!BinderCodeGenerator.IsValidIdentifier(unit.FieldName))
                {
                    errors.Add($"[{displayName}] 字段名 \"{unit.FieldName}\" 不是合法的 C# 标识符");
                }
            }

            if (BinderCodeGenerator.TryFindDuplicateFieldName(ToBindUnits(), out var duplicate))
            {
                errors.Add($"字段名 \"{duplicate}\" 重复，请修改绑定单元的字段名");
            }

            _lastValidationErrors = errors;
            HasError = errors.Count > 0;
            if (HasError)
            {
                foreach (var error in errors)
                {
                    AesirModulesDebug.LogWarning(AesirModulesDebug.ObjectBinderTag, $"{name}: {error}");
                }
            }

            return !HasError;
        }

        BinderCodeGenerator.CodeGenConfig BuildCodeGenConfig()
        {
            return new BinderCodeGenerator.CodeGenConfig(
                TargetNamespace,
                ScriptName,
                string.IsNullOrEmpty(BaseType) ? typeof(MonoBehaviour).FullName : BaseType,
                EffectiveBaseTypeArguments,
                PartialSuffix,
                name,
                CustomNamespaces,
                ToBindUnits());
        }

        List<BinderCodeGenerator.BindUnit> ToBindUnits()
        {
            return Units
                .Select(unit => new BinderCodeGenerator.BindUnit(unit.ComponentFullName, unit.FieldName,
                    unit.HierarchyPath))
                .ToList();
        }

#if UNITY_EDITOR
        /// <summary>
        /// 脚本重编译后自动校验所有 BinderAssistant 的绑定信息是否有效。
        /// 检查项: 引用是否丢失、标记是否缺失、层级路径是否与实际层级一致、配置与字段名是否合法。
        /// </summary>
        [DidReloadScripts]
        static void CheckBinderUnit()
        {
            var assistants = FindObjectsByType<BinderAssistant>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var assistant in assistants)
            {
                if (!assistant.OpenAutoValidate)
                {
                    continue;
                }

                assistant.ValidateBindings();
            }
        }

        /// <summary>
        /// 脚本重编译后，把刚生成的脚本组件自动挂载到目标 GameObject 并执行一次绑定。
        /// 依赖 <see cref="GenerateCode" /> 写入的 EditorPrefs 暂存数据。
        /// </summary>
        [DidReloadScripts]
        static void AttachToGameObject()
        {
            if (!EditorPrefs.HasKey(PendingBindInstanceIdKey))
            {
                return;
            }

            var instanceId = EditorPrefs.GetInt(PendingBindInstanceIdKey);
            var typeFullName = EditorPrefs.GetString(PendingBindTypeKey);
            EditorPrefs.DeleteKey(PendingBindInstanceIdKey);
            EditorPrefs.DeleteKey(PendingBindTypeKey);

            var targetObj = EditorUtility.InstanceIDToObject(instanceId) as GameObject;
            if (!targetObj)
            {
                AesirModulesDebug.LogWarning(AesirModulesDebug.ObjectBinderTag, "自动挂载失败: 目标物体已不存在");
                return;
            }

            var scriptType = ResolveGeneratedType(typeFullName);
            if (scriptType == null)
            {
                AesirModulesDebug.LogError(AesirModulesDebug.ObjectBinderTag,
                    $"自动挂载失败: 未找到类型 {typeFullName}，请确认脚本编译无错误");
                return;
            }

            var component = targetObj.GetComponent(scriptType);
            if (!component)
            {
                component = targetObj.AddComponent(scriptType);
            }

            if (component is IComponentBinder binder)
            {
                binder.BindComponents();
                EditorSceneManager.MarkSceneDirty(targetObj.scene);
            }

            Selection.activeObject = targetObj;
            AesirModulesDebug.Log(AesirModulesDebug.ObjectBinderTag,
                $"已自动挂载 {typeFullName} 并完成首次绑定: {targetObj.name}");
        }

        /// <summary>
        /// 在所有已加载的非动态程序集中按完整名称解析生成的脚本类型。
        /// 生成脚本可能位于 Assembly-CSharp 或任意自定义 asmdef 程序集，故不做单一程序集假设。
        /// </summary>
        static Type ResolveGeneratedType(string typeFullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.IsDynamic)
                {
                    continue;
                }

                var type = assembly.GetType(typeFullName, false);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        /// <summary>
        /// 确保目标文件夹存在（自 Assets 起逐级创建）。
        /// </summary>
        static void EnsureFolderExists(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            var parts = folderPath.Split('/');
            if (parts.Length == 0 || parts[0] != "Assets")
            {
                throw new InvalidOperationException($"目标文件夹必须位于 Assets/ 之下: {folderPath}");
            }

            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }
#endif
    }
}
