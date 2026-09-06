using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("Runestone.AesirModules.Tests")]

namespace Runestone.AesirModules
{
    /// <summary>
    /// Binder 代码生成器。只做纯文本拼装与校验，不接触 Unity 对象，
    /// 由 <see cref="BinderAssistant" /> 收集场景数据后调用，便于单元测试。
    /// </summary>
    internal static class BinderCodeGenerator
    {
        /// <summary>
        /// 生成脚本中自动生成内容所在的 region 名称。
        /// </summary>
        internal const string BindFieldRegionName = "绑定字段（自动生成）";

        /// <summary>
        /// region 起始标记行。
        /// </summary>
        internal const string RegionStartMarker = "#region " + BindFieldRegionName;

        /// <summary>
        /// region 结束标记。
        /// </summary>
        internal const string RegionEndMarker = "#endregion";

        /// <summary>
        /// 生成脚本实现绑定能力的接口完整名称。
        /// </summary>
        internal const string InterfaceFullName = "Runestone.AesirModules.IComponentBinder";

        /// <summary>
        /// 生成代码的类成员缩进（4 空格）。
        /// </summary>
        internal const string MemberIndent = "    ";

        /// <summary>
        /// 单个绑定单元在代码生成阶段的只读描述。
        /// <para>
        /// <see cref="HierarchyPath" /> 为空字符串表示绑定 BinderAssistant 所在物体自身，
        /// 生成代码将直接调用 <c>GetComponent</c> 而不经过 <c>transform.Find</c>。
        /// </para>
        /// </summary>
        internal readonly struct BindUnit
        {
            /// <summary>组件类型完整名称（含命名空间，嵌套类型以 <c>+</c> 连接）。</summary>
            internal readonly string ComponentFullName;

            /// <summary>生成脚本中的字段名。</summary>
            internal readonly string FieldName;

            /// <summary>相对于 BinderAssistant 的 <c>transform.Find()</c> 路径；空字符串表示自身。</summary>
            internal readonly string HierarchyPath;

            internal BindUnit(string componentFullName, string fieldName, string hierarchyPath)
            {
                ComponentFullName = componentFullName;
                FieldName = fieldName;
                HierarchyPath = hierarchyPath;
            }
        }

        /// <summary>
        /// 一次代码生成的完整配置。
        /// </summary>
        internal readonly struct CodeGenConfig
        {
            /// <summary>生成脚本所在的命名空间。</summary>
            internal readonly string Namespace;

            /// <summary>生成的类名（即脚本文件名）。</summary>
            internal readonly string ScriptName;

            /// <summary>基类完整名称。</summary>
            internal readonly string BaseTypeFullName;

            /// <summary>发起生成的物体名，仅用于头部注释展示。</summary>
            internal readonly string SourceObjectName;

            /// <summary>用户额外追加的 using 命名空间。</summary>
            internal readonly IReadOnlyList<string> CustomNamespaces;

            /// <summary>
            /// 泛型基类的具体类型参数（逗号分隔）。基类候选不含 <c>&lt;T&gt;</c> 占位时忽略。
            /// </summary>
            internal readonly string BaseTypeArguments;

            /// <summary>
            /// partial 分部类模式下自动维护文件的后缀（含扩展名，如 <c>.designer.cs</c>）。
            /// 同一脚本增量模式忽略。
            /// </summary>
            internal readonly string AutoFileSuffix;

            /// <summary>绑定单元列表。</summary>
            internal readonly IReadOnlyList<BindUnit> Units;

            internal CodeGenConfig(string targetNamespace, string scriptName, string baseTypeFullName,
                string baseTypeArguments, string autoFileSuffix, string sourceObjectName,
                IReadOnlyList<string> customNamespaces, IReadOnlyList<BindUnit> units)
            {
                Namespace = targetNamespace;
                ScriptName = scriptName;
                BaseTypeFullName = baseTypeFullName;
                BaseTypeArguments = baseTypeArguments;
                AutoFileSuffix = autoFileSuffix;
                SourceObjectName = sourceObjectName;
                CustomNamespaces = customNamespaces ?? Array.Empty<string>();
                Units = units ?? Array.Empty<BindUnit>();
            }
        }

        /// <summary>
        /// 构建 partial 分部类模式的自动维护脚本 <c>*.generated.cs</c> 全文。
        /// <para>
        /// 内容包含：头注释、按需计算的 using、命名空间与类声明，
        /// 类体内为「绑定字段（自动生成）」region（字段 + <c>BindComponents()</c> 方法），每次生成整体覆盖。
        /// </para>
        /// </summary>
        internal static string BuildGeneratedScript(CodeGenConfig config)
        {
            return BuildScaffoldCore(config, new[]
            {
                "本文件由 Aesir Modules Binder 自动生成，请勿手动修改（重新生成时会被整体覆盖）",
                "",
                "面板对象: " + config.SourceObjectName,
                "绑定数量: " + config.Units.Count,
                "生成时间: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                "",
                "使用说明:",
                "1. 业务逻辑写在同目录的 " + config.ScriptName + ".cs（partial 类，仅首次生成，重新生成不会覆盖）",
                "2. 更新绑定: 在 BinderAssistant 的 Inspector 中先「构建绑定单元」再「生成脚本」",
                "3. 「" + BindFieldRegionName + "」region（字段与 BindComponents 方法）由生成器维护"
            });
        }

        /// <summary>
        /// 构建开发者手动编辑的 <c>*.cs</c> 脚本全文（partial 分部类模式下仅首次生成）。
        /// </summary>
        internal static string BuildControllerScript(CodeGenConfig config)
        {
            var builder = new StringBuilder();

            AppendHeaderComment(builder, new[]
            {
                "本文件由 Aesir Modules Binder 首次生成，之后由开发者维护（重新生成不会覆盖本文件）",
                "",
                "绑定字段与 BindComponents 方法在 " + config.ScriptName + config.AutoFileSuffix + " 的",
                "「" + BindFieldRegionName + "」region 中，本 partial 类可直接使用，",
                "例如: playButton.onClick.AddListener(OnPlayClicked);",
                "",
                "生成时间: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            });

            foreach (var usingNamespace in ComputeUsingDirectives(config.Units, config.CustomNamespaces))
            {
                builder.AppendLine("using " + usingNamespace + ";");
            }

            builder.AppendLine();
            if (!string.IsNullOrEmpty(config.Namespace))
            {
                builder.AppendLine("namespace " + config.Namespace);
                builder.AppendLine("{");
            }

            builder.AppendLine("public partial class " + config.ScriptName);
            builder.AppendLine("{");
            builder.AppendLine("}");
            if (!string.IsNullOrEmpty(config.Namespace))
            {
                builder.AppendLine("}");
            }

            return builder.ToString();
        }

        /// <summary>
        /// 构建同一脚本增量模式下目标脚本的初始脚手架全文（目标文件不存在时使用）。
        /// <para>
        /// 文件结构与 partial 模式的 generated 脚本一致，但文件名即 <c>*.cs</c> 本身，
        /// 之后重新生成时仅替换 region 内容，region 外归开发者所有。
        /// </para>
        /// </summary>
        internal static string BuildIncrementalScaffold(CodeGenConfig config)
        {
            return BuildScaffoldCore(config, new[]
            {
                "本文件由 Aesir Modules Binder 以「同一脚本增量」模式创建",
                "之后重新生成时仅替换「" + BindFieldRegionName + "」region 内的内容（含 BindComponents 方法），",
                "region 外的内容归开发者所有",
                "",
                "面板对象: " + config.SourceObjectName,
                "生成时间: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                "",
                "使用说明:",
                "1. 业务逻辑直接写在本文件 region 外的区域",
                "2. 更新绑定: 在 BinderAssistant 的 Inspector 中先「构建绑定单元」再「生成脚本」",
                "3. region 内的类型与特性均为全限定名，不依赖文件头部的 using 指令"
            });
        }

        /// <summary>
        /// 构建脚手架公共部分: 头注释 + using + 命名空间 + 类声明 + 自动生成 region。
        /// <para>
        /// namespace 内类型平铺（不额外缩进），类成员统一缩进 4 空格。
        /// </para>
        /// </summary>
        static string BuildScaffoldCore(CodeGenConfig config, string[] headerLines)
        {
            var builder = new StringBuilder();

            AppendHeaderComment(builder, headerLines);

            foreach (var usingNamespace in ComputeUsingDirectives(config.Units, config.CustomNamespaces))
            {
                builder.AppendLine("using " + usingNamespace + ";");
            }

            builder.AppendLine();
            if (!string.IsNullOrEmpty(config.Namespace))
            {
                builder.AppendLine("namespace " + config.Namespace);
                builder.AppendLine("{");
            }

            builder.AppendLine("/// <summary>");
            builder.AppendLine("/// 由 Binder 自动生成的绑定部分，与 " + config.ScriptName +
                               ".cs 中的 partial 合并为同一类。");
            builder.AppendLine("/// </summary>");
            builder.AppendLine("public partial class " + config.ScriptName + " : " +
                               BuildBaseTypeReference(config.BaseTypeFullName, config.BaseTypeArguments) +
                               ", " + InterfaceFullName);
            builder.AppendLine("{");
            builder.AppendLine(ApplyIndent(BuildRegionBlock(config), MemberIndent, "\n"));
            builder.AppendLine("}");
            if (!string.IsNullOrEmpty(config.Namespace))
            {
                builder.AppendLine("}");
            }

            return builder.ToString();
        }

        /// <summary>
        /// 构建「绑定字段（自动生成）」region 内容（字段 + <c>BindComponents()</c> 方法），行首无缩进。
        /// <para>
        /// 类型与特性均使用全限定名，使 region 块自包含，
        /// 同一脚本增量模式下替换进任意已有文件都不会因缺少 using 而编译失败。
        /// 绑定字段经 <c>TitleGroup</c> 归入同一分组，标注该分组由 Binder 自动生成维护。
        /// </para>
        /// </summary>
        internal static string BuildRegionBlock(CodeGenConfig config)
        {
            var builder = new StringBuilder();
            builder.AppendLine(RegionStartMarker);
            builder.AppendLine();
            foreach (var unit in config.Units)
            {
                builder.AppendLine("[Sirenix.OdinInspector.TitleGroup(\"" + BindFieldRegionName + "\")]");
                builder.AppendLine("[UnityEngine.SerializeField]");
                builder.AppendLine("private " + ToSourceTypeReference(unit.ComponentFullName) + " " +
                                   unit.FieldName + ";");
                builder.AppendLine();
            }

            builder.AppendLine("/// <summary>");
            builder.AppendLine("/// 绑定引用: 按 BinderAssistant 中配置的层级路径查找组件并赋值到绑定字段。");
            builder.AppendLine("/// </summary>");
            builder.AppendLine("[UnityEngine.ContextMenu(\"绑定引用\")]");
            builder.AppendLine("public void BindComponents()");
            builder.AppendLine("{");
            foreach (var unit in config.Units)
            {
                builder.AppendLine("    " + BuildBindStatement(unit));
            }

            builder.AppendLine("}");
            builder.Append(RegionEndMarker);

            return builder.ToString();
        }

        /// <summary>
        /// 在已有脚本内容中替换「绑定字段（自动生成）」region（从 <see cref="RegionStartMarker" /> 行起，
        /// 到其后第一个 <see cref="RegionEndMarker" /> 行止）。
        /// <para>
        /// 替换内容统一使用 4 空格缩进（生成器权威，与脚手架一致）；
        /// 自动适配文件的换行风格（CRLF / LF），region 外内容保持原样。
        /// </para>
        /// </summary>
        /// <returns>找到并完成替换返回 true；未找到 region 起始或结束标记返回 false。</returns>
        internal static bool TryReplaceRegion(string fileContent, string regionBlock, out string updatedContent)
        {
            updatedContent = null;
            if (string.IsNullOrEmpty(fileContent))
            {
                return false;
            }

            var markerIndex = fileContent.IndexOf(RegionStartMarker, StringComparison.Ordinal);
            if (markerIndex < 0)
            {
                return false;
            }

            var endregionIndex = fileContent.IndexOf(RegionEndMarker, markerIndex, StringComparison.Ordinal);
            if (endregionIndex < 0)
            {
                return false;
            }

            var regionLineStart = fileContent.LastIndexOf('\n', markerIndex) + 1;
            var regionLineEnd = endregionIndex + RegionEndMarker.Length;
            var newline = fileContent.Contains("\r\n") ? "\r\n" : "\n";

            updatedContent = fileContent
                .Remove(regionLineStart, regionLineEnd - regionLineStart)
                .Insert(regionLineStart, ApplyIndent(regionBlock, MemberIndent, newline));

            return true;
        }

        /// <summary>
        /// 计算默认字段名: 物体名 camelCase + 组件类型简称后缀（无下划线）；
        /// 物体名已以类型简称结尾（忽略大小写）时不再重复拼接（如 <c>ScoreText</c> + Text → <c>scoreText</c> 而非 <c>scoreTextText</c>）。
        /// </summary>
        internal static string ComposeDefaultFieldName(string objectName, string componentFullName)
        {
            var camelName = ToCamelCase(objectName);
            var typeShort = GetTypeShortName(componentFullName);

            return camelName.EndsWith(typeShort, StringComparison.OrdinalIgnoreCase)
                ? camelName
                : camelName + typeShort;
        }

        /// <summary>
        /// 计算 using 指令集合：UnityEngine 必需（MonoBehaviour/SerializeField/ContextMenu），
        /// 其余按绑定单元的组件类型命名空间推导（嵌套类型取外部类型命名空间），
        /// 再并入用户附加命名空间，最后按序去重输出。
        /// </summary>
        internal static List<string> ComputeUsingDirectives(IReadOnlyList<BindUnit> units,
            IReadOnlyList<string> customNamespaces)
        {
            var set = new SortedSet<string>(StringComparer.Ordinal) { "UnityEngine" };
            if (units != null)
            {
                foreach (var unit in units)
                {
                    var typeNamespace = GetTypeNamespace(unit.ComponentFullName);
                    if (!string.IsNullOrEmpty(typeNamespace))
                    {
                        set.Add(typeNamespace);
                    }
                }
            }

            if (customNamespaces != null)
            {
                foreach (var custom in customNamespaces)
                {
                    var trimmed = custom.Trim().TrimEnd(';').Trim();
                    if (trimmed.Length > 0)
                    {
                        set.Add(trimmed);
                    }
                }
            }

            return set.ToList();
        }

        /// <summary>
        /// 生成单条绑定赋值语句。GameObject 类型取 <c>.gameObject</c>，其余 <c>GetComponent</c>；
        /// 路径为空（绑定自身）时跳过 <c>transform.Find</c>。
        /// </summary>
        static string BuildBindStatement(BindUnit unit)
        {
            var typeReference = ToSourceTypeReference(unit.ComponentFullName);
            var isSelf = string.IsNullOrEmpty(unit.HierarchyPath);
            var lookup = isSelf ? "this.transform" : "transform.Find(\"" + EscapeStringLiteral(unit.HierarchyPath) + "\")";

            if (typeReference == "UnityEngine.GameObject")
            {
                return unit.FieldName + " = " + (isSelf ? "gameObject;" : lookup + ".gameObject;");
            }

            return unit.FieldName + " = " + (isSelf ? lookup + ".GetComponent<" + typeReference + ">();"
                : lookup + ".GetComponent<" + typeReference + ">();");
        }

        /// <summary>
        /// 追加统一格式的头注释块。
        /// </summary>
        static void AppendHeaderComment(StringBuilder builder, string[] lines)
        {
            builder.AppendLine("// * ------------------------------------------------------------------");
            foreach (var line in lines)
            {
                builder.AppendLine("// * " + line);
            }

            builder.AppendLine("// * ------------------------------------------------------------------");
        }

        /// <summary>
        /// 将类型完整名称转换为源代码可用的类型引用：嵌套类型的 <c>+</c> 分隔符替换为 <c>.</c>。
        /// </summary>
        static string ToSourceTypeReference(string componentFullName)
        {
            return (componentFullName ?? "").Replace('+', '.');
        }

        /// <summary>
        /// 按目标缩进与换行风格重排行块（空行保持为空）。
        /// </summary>
        static string ApplyIndent(string block, string indent, string newline)
        {
            var lines = block.Replace("\r\n", "\n").TrimEnd('\n').Split('\n');
            var builder = new StringBuilder();
            for (var i = 0; i < lines.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(newline);
                }

                builder.Append(lines[i].Length == 0 ? "" : indent + lines[i]);
            }

            return builder.ToString();
        }

        /// <summary>
        /// 物体名转 camelCase 字段名片段：首字母小写；非字母开头加 <c>_</c> 前缀；空名回退 <c>element</c>。
        /// </summary>
        internal static string ToCamelCase(string name)
        {
            var trimmed = (name ?? "").Trim();
            if (trimmed.Length == 0)
            {
                return "element";
            }

            if (!char.IsLetter(trimmed[0]))
            {
                return "_" + trimmed;
            }

            return char.ToLowerInvariant(trimmed[0]) + trimmed.Substring(1);
        }

        /// <summary>
        /// 取类型简称（最末段，支持嵌套类型的 <c>+</c> 分隔），空名回退 <c>Component</c>。
        /// </summary>
        internal static string GetTypeShortName(string componentFullName)
        {
            if (string.IsNullOrEmpty(componentFullName))
            {
                return "Component";
            }

            var separator = componentFullName.LastIndexOfAny(new[] { '.', '+' });
            return separator < 0 ? componentFullName : componentFullName.Substring(separator + 1);
        }

        /// <summary>
        /// 取类型所属命名空间（嵌套类型取最外层命名空间），无命名空间返回空字符串。
        /// </summary>
        internal static string GetTypeNamespace(string componentFullName)
        {
            if (string.IsNullOrEmpty(componentFullName))
            {
                return "";
            }

            var nested = componentFullName.IndexOf('+');
            var effective = nested >= 0 ? componentFullName.Substring(0, nested) : componentFullName;
            var dot = effective.LastIndexOf('.');
            return dot < 0 ? "" : effective.Substring(0, dot);
        }

        /// <summary>
        /// 把泛型类型的元数后缀转换为占位符形式:
        /// <c>Ns.View`1</c> → <c>Ns.View&lt;T&gt;</c>，<c>Ns.Pair`2</c> → <c>Ns.Pair&lt;T1,T2&gt;</c>；
        /// 非泛型名称原样返回。
        /// </summary>
        internal static string ConvertArityToPlaceholders(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
            {
                return fullName ?? "";
            }

            var backtickIndex = fullName.IndexOf('`');
            if (backtickIndex < 0)
            {
                return fullName;
            }

            var arityText = fullName.Substring(backtickIndex + 1);
            if (!int.TryParse(arityText, out var arity) || arity <= 0)
            {
                return fullName;
            }

            var placeholders = string.Join(",", Enumerable.Range(1, arity).Select(i => "T" + (arity == 1 ? "" : i.ToString())));
            return fullName.Substring(0, backtickIndex) + "<" + placeholders + ">";
        }

        /// <summary>
        /// 基类候选是否含 <c>&lt;T&gt;</c>/<c>&lt;T1,T2&gt;</c> 泛型占位。
        /// </summary>
        internal static bool HasGenericPlaceholder(string baseType)
        {
            return !string.IsNullOrEmpty(baseType) && baseType.Contains('<');
        }

        /// <summary>
        /// 基类候选的泛型占位元数（<c>&lt;T&gt;</c> → 1，<c>&lt;T1,T2&gt;</c> → 2），非泛型候选返回 0。
        /// </summary>
        internal static int GetGenericPlaceholderArity(string baseType)
        {
            if (!HasGenericPlaceholder(baseType))
            {
                return 0;
            }

            var angleIndex = baseType.IndexOf('<');
            var placeholder = baseType.Substring(angleIndex + 1, baseType.Length - angleIndex - 2);
            return placeholder.Split(',').Length;
        }

        /// <summary>
        /// 计算最终基类引用: 非泛型候选原样返回；
        /// 含 <c>&lt;T&gt;</c>/<c>&lt;T1,T2&gt;</c> 占位的泛型候选，用具体类型参数替换整个占位段。
        /// 占位与参数的元数一致性由 BinderAssistant 校验保证。
        /// </summary>
        internal static string BuildBaseTypeReference(string baseType, string genericArguments)
        {
            if (string.IsNullOrEmpty(baseType))
            {
                return baseType ?? "";
            }

            var angleIndex = baseType.IndexOf('<');
            if (angleIndex < 0)
            {
                return baseType;
            }

            return baseType.Substring(0, angleIndex) + "<" + (genericArguments ?? "").Trim() + ">";
        }

        /// <summary>
        /// 转义字符串字面量中的反斜杠与双引号，保证生成代码合法。
        /// </summary>
        internal static string EscapeStringLiteral(string value)
        {
            return (value ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        /// <summary>
        /// 是否为合法的 C# 标识符（关键字未做排除，字段名撞关键字由编译报错兜底）。
        /// </summary>
        internal static bool IsValidIdentifier(string value)
        {
            if (string.IsNullOrEmpty(value) || (!char.IsLetter(value[0]) && value[0] != '_'))
            {
                return false;
            }

            return value.All(c => char.IsLetterOrDigit(c) || c == '_');
        }

        /// <summary>
        /// 是否为合法的 C# 命名空间（非空，且每个 <c>.</c> 段都是合法标识符）。
        /// </summary>
        internal static bool IsValidNamespace(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            return value.Split('.').All(IsValidIdentifier);
        }

        /// <summary>
        /// 检查绑定单元字段名是否唯一，返回第一个重名字段。
        /// </summary>
        internal static bool TryFindDuplicateFieldName(IReadOnlyList<BindUnit> units, out string duplicate)
        {
            duplicate = null;
            if (units == null)
            {
                return false;
            }

            var seen = new HashSet<string>();
            foreach (var unit in units)
            {
                if (!seen.Add(unit.FieldName))
                {
                    duplicate = unit.FieldName;
                    return true;
                }
            }

            return false;
        }
    }
}
