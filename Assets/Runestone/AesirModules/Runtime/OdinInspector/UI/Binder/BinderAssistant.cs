using System;
using System.Collections.Generic;
using System.IO;
using Runestone.AesirArchitecture;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
#endif

namespace Runestone.AesirModules
{
    /// <summary>
    /// Object Binder 核心组件。配置自动绑定信息并生成对应的脚本代码。
    /// <para>
    /// 工作流程：
    /// 1. 在需要自动绑定引用的子物体上添加 <see cref="BinderTag" /> 组件，设置绑定数量。
    /// 2. 在本组件上点击「构建绑定单元」扫描所有 BinderTag，生成 <see cref="BinderInfo" /> 列表。
    /// 3. 点击「生成脚本」生成两个 partial class 文件：
    /// - <c>*.generated.cs</c>：自动维护，包含字段声明和 <c>BindReferences()</c> 方法。
    /// - <c>*.cs</c>：开发者手动编写业务逻辑，仅生成一次。
    /// 4. 脚本编译后自动将生成的脚本组件挂载到当前 GameObject 并执行一次绑定。
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    public class BinderAssistant : AesirMonoBehaviour
    {
        [PropertyOrder(-5)]
        [HorizontalGroup("Bool")]
        [ToggleLeft]
        [LabelText("开启自动检查")]
        public bool OpenAutoValidate = true;

        [LabelText("命名空间: ")]
        [LabelWidth(100)]
        [InlineButton("DefaultNamespace", "默认命名空间")]
        public string TargetNamespace;

        [LabelText("脚本名: ")]
        [LabelWidth(100)]
        [InlineButton("DefaultScriptName", "默认脚本名")]
        public string ScriptName;

        [ValueDropdown(nameof(GetBaseTypes))]
        [LabelText("基类: ")]
        [LabelWidth(100)]
        public string BaseType;

        [FolderPath(RequireExistingPath = true)]
        [LabelText("目标文件夹路径: ")]
        [LabelWidth(100)]
        public string FolderPath;

        [Title("自动绑定列表")]
        public List<BinderInfo> Units;

        [TitleGroup("自定义命名空间", "示例: UnityEngine.UI")]
        public List<string> CustomNamespaces;

        [PropertyOrder(-3)]
        [HorizontalGroup("Bool")]
        [ToggleLeft]
        [LabelText("当前绑定信息有错误")]
        [ShowInInspector]
        public bool HasError { get; private set; }

        /// <summary>
        /// 当前物体在场景层级中的绝对路径
        /// </summary>
        public string HierarchyPath => BinderHierarchyUtility.GetAbsolutePath(transform);

        void Reset()
        {
            DefaultNamespace();
            DefaultScriptName();
            BaseType = nameof(MonoBehaviour);
        }

        void DefaultNamespace()
        {
            TargetNamespace = "Game";
        }

        void DefaultScriptName()
        {
            ScriptName = gameObject.name + "Presenter";
        }

        /// <summary>
        /// 可选基类下拉列表
        /// </summary>
        public ValueDropdownList<string> GetBaseTypes()
        {
            var typeStrings = new ValueDropdownList<string>
            {
                new ValueDropdownItem<string>(nameof(MonoBehaviour), typeof(MonoBehaviour).FullName)
            };
            return typeStrings;
        }

        [TitleGroup("按钮操作")]
        [Button("构建绑定单元")]
        void CreateUnits()
        {
            // 遍历所有子物体（含 inactive）上的 BinderTag
            var labels = transform.GetComponentsInChildren<BinderTag>(true);

            foreach (var label in labels)
            {
                // 每个 BinderTag 声明的组件数量即为需要生成的 BinderInfo 数量
                var number = label.ComponentNumber;

                // 先更新已存在的 BinderInfo 的路径（层级可能已变动）
                for (var i = 0; i < Units.Count; i++)
                {
                    var unit = Units[i];
                    if (unit.LabelObj != label.SelfObj)
                    {
                        continue;
                    }

                    unit.UpdatePath(this);
                    number--;
                }

                // 剩余数量即为新增的绑定单元
                while (number > 0)
                {
                    Units.Add(new BinderInfo(this, label));
                    number--;
                }
            }
        }

        [TitleGroup("按钮操作")]
        [Button("生成文件夹")]
        void CreateFolder()
        {
#if UNITY_EDITOR
            if (!AssetDatabase.IsValidFolder(FolderPath))
            {
                AesirModulesDebug.Log(AesirModulesDebug.ObjectBinderTag, "不存在该路径");
                var guid = AssetDatabase.CreateFolder("Assets", FolderPath.Replace("Assets/", ""));
                var newFolderPath = AssetDatabase.GUIDToAssetPath(guid);
                AesirModulesDebug.Log(AesirModulesDebug.ObjectBinderTag, newFolderPath);
            }
#endif
        }

        [TitleGroup("按钮操作")]
        [Button("生成脚本")]
        void GenerateCode()
        {
            var generatedPath = Path.Combine(FolderPath, ScriptName + ".generated.cs");
            var controllerPath = Path.Combine(FolderPath, ScriptName + ".cs");
            try
            {
                WriteGeneratedScript(generatedPath);
                WriteControllerScript(controllerPath);
            }
            catch (Exception ex)
            {
                AesirModulesDebug.LogError(AesirModulesDebug.ObjectBinderTag, $"生成脚本失败: {ex.Message}");
            }
#if UNITY_EDITOR

            AssetDatabase.ImportAsset(controllerPath);
            AssetDatabase.ImportAsset(generatedPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
#endif
        }

        /// <summary>
        /// 生成 Controller 脚本（开发者手动编辑区，仅生成一次）。
        /// 文件包含空 partial class 体，开发者在此编写业务逻辑。
        /// </summary>
        void WriteControllerScript(string controllerPath)
        {
            using (var writer = new StreamWriter(controllerPath))
            {
                writer.WriteLine("// * ---------------------------------------------");
                writer.WriteLine("// * Controller 脚本仅由 Object Binder 生成一次");
                writer.WriteLine("// * 生成时间: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                writer.WriteLine("// * ---------------------------------------------");
                writer.WriteLine("using UnityEngine;");
                foreach (var item in CustomNamespaces)
                {
                    item.TrimEnd(';');
                    writer.WriteLine("using " + item + ";");
                }

                writer.WriteLine();
                writer.WriteLine("namespace " + TargetNamespace);
                writer.WriteLine("{");
                writer.WriteLine("    public partial class " + ScriptName);
                writer.WriteLine("    {");
                writer.WriteLine();
                writer.WriteLine("    }");
                writer.WriteLine("}");
                writer.Flush();
                writer.Close();
            }
        }

        /// <summary>
        /// 生成 Generated 脚本（自动维护区，每次「生成脚本」时覆盖）。
        /// 文件包含：
        /// - 所有绑定字段的声明（带 Odin Inspector 属性标签）
        /// - <c>BindReferences()</c> 方法：通过 <c>transform.Find</c> 查找并赋值引用
        /// - <c>BindCommand()</c> 方法：供 <c>[ContextMenu]</c> 手动触发绑定
        /// </summary>
        void WriteGeneratedScript(string generatedPath)
        {
            using (var writer = new StreamWriter(generatedPath))
            {
                writer.WriteLine("// * ---------------------------------------------");
                writer.WriteLine("// * Generated 脚本由 Object Binder 自动生成，手动修改将会被覆盖");
                writer.WriteLine("// * 生成时间: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                writer.WriteLine("// * ---------------------------------------------");
                writer.WriteLine("using UnityEngine;");
                writer.WriteLine("using Sirenix.OdinInspector;");
                foreach (var item in CustomNamespaces)
                {
                    item.TrimEnd(';');
                    writer.WriteLine("using " + item + ";");
                }

                writer.WriteLine();
                writer.WriteLine("namespace " + TargetNamespace);
                writer.WriteLine("{");
                writer.WriteLine("    public partial class " + ScriptName + " : " + BaseType + ", " +
                                 typeof(IComponentBinder).FullName);
                writer.WriteLine("    {");
                // 声明绑定字段
                foreach (var unit in Units)
                {
                    writer.WriteLine("        [PropertyOrder(-1000)]");
                    writer.WriteLine("        [TitleGroup(\"自动绑定变量\")]");
                    writer.WriteLine("        [BoxGroup(\"自动绑定变量/Box\",ShowLabel = false)]");
                    writer.WriteLine("        [SerializeField]");
                    writer.WriteLine("        " + unit.ComponentFullName + " " + unit.FieldName + ";");
                }

                writer.WriteLine();
                // 生成 BindReferences 方法：通过 transform.Find 路径查找并赋值组件引用
                writer.WriteLine("        public void BindReferences()");
                writer.WriteLine("        {");
                foreach (var unit in Units)
                {
                    if (unit.ComponentFullName == typeof(GameObject).FullName)
                    {
                        // GameObject 类型需要额外调用 .gameObject
                        writer.WriteLine("            " + unit.FieldName + " = transform.Find(" + "\"" +
                                         unit.HierarchyPath + "\"" + ").gameObject;");
                    }
                    else
                    {
                        // 其他组件类型通过 GetComponent 获取
                        writer.WriteLine("            " + unit.FieldName + " = transform.Find(" + "\"" +
                                         unit.HierarchyPath + "\"" + ").GetComponent<" +
                                         unit.ComponentFullName + ">();");
                    }
                }

                writer.WriteLine("        }");
                writer.WriteLine();
                // 提供 ContextMenu 入口，方便在 Inspector 右键手动重新绑定
                writer.WriteLine("        [ContextMenu(\"绑定引用\", false)]");
                writer.WriteLine("        public void BindCommand()");
                writer.WriteLine("        {");
                writer.WriteLine("            BindReferences();");
                writer.WriteLine("        }");
                writer.WriteLine("    }");
                writer.WriteLine("}");
                writer.Flush();
                writer.Close();
            }
#if UNITY_EDITOR
            // 将当前 GameObject 和即将生成的脚本类型暂存到 EditorPrefs，
            // 脚本编译完成后由 AttachToGameObject() 读取并自动挂载组件
            EditorPrefs.SetInt("即将绑定脚本的物体 Id", gameObject.GetInstanceID());
            EditorPrefs.SetString("即将绑定的脚本类型",
                TargetNamespace + "." + ScriptName +
                ", Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
#endif

            AesirModulesDebug.Log(AesirModulesDebug.ObjectBinderTag, $"成功生成脚本: {generatedPath}");
        }

#if UNITY_EDITOR
        /// <summary>
        /// 脚本重编译后自动校验所有 BinderAssistant 的绑定信息是否有效。
        /// 检查项：引用是否丢失、层级路径是否与实际层级一致。
        /// </summary>
        [DidReloadScripts]
        static void CheckBinderUnit()
        {
            var assistants =
                FindObjectsByType<BinderAssistant>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var assistant in assistants)
            {
                assistant.HasError = false;
                if (!assistant.OpenAutoValidate)
                {
                    continue;
                }

                foreach (var unit in assistant.Units)
                {
                    // 引用丢失（物体被删除）
                    if (!unit.LabelObj)
                    {
                        assistant.HasError = true;
                    }

                    // 层级路径过期（物体被移动）
                    if (unit.HierarchyPath != BinderHierarchyUtility.GetRelativePath(assistant.HierarchyPath,
                            unit.LabelObj.GetComponent<BinderTag>().HierarchyPath))
                    {
                        assistant.HasError = true;
                    }
                }

                if (assistant.HasError)
                {
                    AesirModulesDebug.LogWarning(AesirModulesDebug.ObjectBinderTag,
                        assistant.name + " Binder Assistant 发现绑定错误，请重新生成绑定信息单元");
                }
            }
        }

        /// <summary>
        /// 脚本重编译后，将生成的脚本组件自动挂载到目标 GameObject 并执行一次绑定。
        /// 依赖 <see cref="WriteGeneratedScript" /> 中写入的 EditorPrefs 数据。
        /// </summary>
        [DidReloadScripts]
        static void AttachToGameObject()
        {
            if (!EditorPrefs.HasKey("即将绑定脚本的物体 Id"))
            {
                return;
            }

            var targetObj =
                EditorUtility.InstanceIDToObject(EditorPrefs.GetInt("即将绑定脚本的物体 Id")) as GameObject;
            if (!targetObj)
            {
                EditorPrefs.DeleteKey("即将绑定脚本的物体 Id");
                EditorPrefs.DeleteKey("即将绑定的脚本类型");
                return;
            }

            var scriptType = Type.GetType(EditorPrefs.GetString("即将绑定的脚本类型"));
            if (scriptType == null)
            {
                AesirModulesDebug.LogError(AesirModulesDebug.ObjectBinderTag, "即将绑定的脚本类型为空");
                return;
            }

            // 挂载生成的脚本组件（已有则跳过）
            if (!targetObj.GetComponent(scriptType))
            {
                targetObj.AddComponent(scriptType);
            }

            // 立即执行一次引用绑定，使新挂载的组件字段立即有值
            var script = targetObj.GetComponent(scriptType);
            if (script is IComponentBinder bindReferences)
            {
                bindReferences.BindComponents();
            }

            Selection.activeObject = targetObj;
            EditorPrefs.DeleteKey("即将绑定脚本的物体 Id");
            EditorPrefs.DeleteKey("即将绑定的脚本类型");
        }
#endif
    }
}
