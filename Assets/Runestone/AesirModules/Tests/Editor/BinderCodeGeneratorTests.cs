using System.Collections.Generic;
using NUnit.Framework;
using Runestone.AesirModules;

namespace Runestone.AesirModules.Tests.Editor
{
    /// <summary>
    /// <see cref="BinderCodeGenerator" /> 纯逻辑单元测试:
    /// 生成模板（partial / 同一脚本增量）、region 替换、using 计算、命名规则、标识符校验与重名检测。
    /// </summary>
    public class BinderCodeGeneratorTests
    {
        static BinderCodeGenerator.BindUnit Unit(string type, string field, string path)
        {
            return new BinderCodeGenerator.BindUnit(type, field, path);
        }

        static BinderCodeGenerator.CodeGenConfig Config(List<BinderCodeGenerator.BindUnit> units,
            string baseType = "Runestone.AesirModules.AesirBasePanel", string baseTypeArguments = null,
            string autoFileSuffix = ".designer.cs")
        {
            return new BinderCodeGenerator.CodeGenConfig(
                "Game.UI",
                "BattlePanel",
                baseType,
                baseTypeArguments,
                autoFileSuffix,
                "BattlePanelRoot",
                new List<string> { "System" },
                units);
        }

        [Test]
        public void BuildGeneratedScript_RegionContainsFieldsAndBindMethod()
        {
            var units = new List<BinderCodeGenerator.BindUnit>
            {
                Unit("UnityEngine.UI.Button", "playButton", "Panel/PlayButton")
            };

            var script = BinderCodeGenerator.BuildGeneratedScript(Config(units));

            Assert.That(script, Does.Contain("#region 绑定字段（自动生成）"));
            Assert.That(script, Does.Contain("#endregion"));
            // region 内类型与特性均为全限定名（自包含，同一脚本增量模式不依赖 using）
            Assert.That(script, Does.Contain("[UnityEngine.SerializeField]"));
            Assert.That(script, Does.Contain("private UnityEngine.UI.Button playButton;"));
            Assert.That(script, Does.Contain("[UnityEngine.ContextMenu(\"绑定引用\")]"));
            Assert.That(script, Does.Contain("public void BindComponents()"));
            Assert.That(script, Does.Contain("transform.Find(\"Panel/PlayButton\").GetComponent<UnityEngine.UI.Button>();"));
            // BindComponents 方法位于 region 内（#endregion 之前）
            Assert.That(script.IndexOf("public void BindComponents()", System.StringComparison.Ordinal),
                Is.LessThan(script.IndexOf("#endregion", System.StringComparison.Ordinal)));
        }

        [Test]
        public void BuildGeneratedScript_RegionFieldsUseSingleParameterTitleGroup()
        {
            var units = new List<BinderCodeGenerator.BindUnit>
            {
                Unit("UnityEngine.UI.Button", "playButton", "Panel/PlayButton")
            };

            var script = BinderCodeGenerator.BuildGeneratedScript(Config(units));

            Assert.That(script, Does.Contain(
                "    [Sirenix.OdinInspector.TitleGroup(\"绑定字段（自动生成）\")]"));
        }

        [Test]
        public void BuildGeneratedScript_ClassHeaderDeclaresBaseTypeAndInterface()
        {
            var script = BinderCodeGenerator.BuildGeneratedScript(Config(new List<BinderCodeGenerator.BindUnit>()));

            Assert.That(script, Does.Contain("public partial class BattlePanel : " +
                                            "Runestone.AesirModules.AesirBasePanel, Runestone.AesirModules.IComponentBinder"));
            Assert.That(script, Does.Contain("namespace Game.UI"));
        }

        [Test]
        public void BuildGeneratedScript_GameObjectBindingUsesDotGameObject()
        {
            var units = new List<BinderCodeGenerator.BindUnit>
            {
                Unit("UnityEngine.GameObject", "coin", "Panel/Coin")
            };

            var script = BinderCodeGenerator.BuildGeneratedScript(Config(units));

            Assert.That(script, Does.Contain("private UnityEngine.GameObject coin;"));
            Assert.That(script, Does.Contain("coin = transform.Find(\"Panel/Coin\").gameObject;"));
        }

        [Test]
        public void BuildGeneratedScript_SelfPathBindsDirectlyWithoutFind()
        {
            var units = new List<BinderCodeGenerator.BindUnit>
            {
                Unit("UnityEngine.Transform", "selfTransform", ""),
                Unit("UnityEngine.GameObject", "selfObject", "")
            };

            var script = BinderCodeGenerator.BuildGeneratedScript(Config(units));

            Assert.That(script, Does.Contain("selfTransform = this.transform.GetComponent<UnityEngine.Transform>();"));
            Assert.That(script, Does.Contain("selfObject = gameObject;"));
            Assert.That(script, Does.Not.Contain("transform.Find"));
        }

        [Test]
        public void BuildGeneratedScript_EscapesQuoteAndBackslashInPath()
        {
            var units = new List<BinderCodeGenerator.BindUnit>
            {
                Unit("UnityEngine.Transform", "tricky", "Pa\"th\\A")
            };

            var script = BinderCodeGenerator.BuildGeneratedScript(Config(units));

            Assert.That(script, Does.Contain("transform.Find(\"Pa\\\"th\\\\A\")"));
        }

        [Test]
        public void BuildGeneratedScript_UsingsContainTypeNamespacesAndCustom()
        {
            var units = new List<BinderCodeGenerator.BindUnit>
            {
                Unit("UnityEngine.UI.Button", "button", "Panel/Button"),
                Unit("TMPro.TextMeshProUGUI", "label", "Panel/Label"),
                Unit("Game.Logic.Outer+Inner", "nested", "Panel/Nested")
            };

            var script = BinderCodeGenerator.BuildGeneratedScript(Config(units));

            Assert.That(script, Does.Contain("using UnityEngine;"));
            Assert.That(script, Does.Contain("using UnityEngine.UI;"));
            Assert.That(script, Does.Contain("using TMPro;"));
            Assert.That(script, Does.Contain("using Game.Logic;"));
            Assert.That(script, Does.Contain("using System;"));
            // 嵌套类型的 + 分隔符需替换为源代码可用的 .
            Assert.That(script, Does.Contain("private Game.Logic.Outer.Inner nested;"));
        }

        [Test]
        public void BuildIncrementalScaffold_MarksIncrementalModeAndContainsRegion()
        {
            var units = new List<BinderCodeGenerator.BindUnit>
            {
                Unit("UnityEngine.UI.Button", "playButton", "Panel/PlayButton")
            };

            var scaffold = BinderCodeGenerator.BuildIncrementalScaffold(Config(units));

            Assert.That(scaffold, Does.Contain("同一脚本增量"));
            Assert.That(scaffold, Does.Contain("#region 绑定字段（自动生成）"));
            Assert.That(scaffold, Does.Contain("public partial class BattlePanel : " +
                                             "Runestone.AesirModules.AesirBasePanel, Runestone.AesirModules.IComponentBinder"));
            Assert.That(scaffold, Does.Contain("[UnityEngine.SerializeField]"));
            Assert.That(scaffold, Does.Contain("public void BindComponents()"));
        }

        [Test]
        public void BuildControllerScript_ContainsPartialClassWithoutGeneratedContent()
        {
            var units = new List<BinderCodeGenerator.BindUnit>
            {
                Unit("UnityEngine.UI.Button", "playButton", "Panel/PlayButton")
            };

            var script = BinderCodeGenerator.BuildControllerScript(Config(units));

            Assert.That(script, Does.Contain("public partial class BattlePanel"));
            Assert.That(script, Does.Contain("namespace Game.UI"));
            // 头注释引用实际后缀的自动维护文件名
            Assert.That(script, Does.Contain("BattlePanel.designer.cs 的"));
            Assert.That(script, Does.Not.Contain("#region"));
            Assert.That(script, Does.Not.Contain("public void BindComponents()"));
        }

        [Test]
        public void TryReplaceRegion_ReplacesRegionAndKeepsOutsideContent()
        {
            var fileContent = "using UnityEngine;\n\nnamespace Game\n{\n    public class MyPanel : MonoBehaviour\n    {\n" +
                              "        #region 绑定字段（自动生成）\n\n" +
                              "        [UnityEngine.SerializeField]\n" +
                              "        private UnityEngine.Transform oldField;\n\n" +
                              "        [UnityEngine.ContextMenu(\"绑定引用\")]\n" +
                              "        public void BindComponents()\n" +
                              "        {\n" +
                              "        }\n\n" +
                              "        #endregion\n\n" +
                              "        public void UserLogic()\n" +
                              "        {\n" +
                              "        }\n" +
                              "    }\n}\n";

            var units = new List<BinderCodeGenerator.BindUnit>
            {
                Unit("UnityEngine.Transform", "newField", "Child")
            };

            var replaced = BinderCodeGenerator.TryReplaceRegion(fileContent,
                BinderCodeGenerator.BuildRegionBlock(Config(units)), out var updated);

            Assert.That(replaced, Is.True);
            Assert.That(updated, Does.Contain("private UnityEngine.Transform newField;"));
            Assert.That(updated, Does.Not.Contain("oldField"));
            // region 外内容保持原样
            Assert.That(updated, Does.Contain("public void UserLogic()"));
            Assert.That(updated, Does.Contain("namespace Game"));
            Assert.That(updated, Does.Contain("using UnityEngine;"));
            // 替换内容统一归一化为 4 空格缩进（生成器权威，不随原文件缩进变化）
            Assert.That(updated, Does.Contain("    #region 绑定字段（自动生成）"));
            Assert.That(updated, Does.Contain("    [UnityEngine.SerializeField]"));
            // 新 region 含 TitleGroup 分组标注（自包含全限定，单参数）
            Assert.That(updated, Does.Contain(
                "    [Sirenix.OdinInspector.TitleGroup(\"绑定字段（自动生成）\")]"));
        }

        [Test]
        public void TryReplaceRegion_MissingMarkerReturnsFalse()
        {
            var fileContent = "public class MyPanel\n{\n    public void UserLogic()\n    {\n    }\n}\n";

            var replaced = BinderCodeGenerator.TryReplaceRegion(fileContent, "#region 绑定字段（自动生成）\n#endregion",
                out var updated);

            Assert.That(replaced, Is.False);
            Assert.That(updated, Is.Null);
        }

        [Test]
        public void TryReplaceRegion_MissingEndMarkerReturnsFalse()
        {
            var fileContent = "public class MyPanel\n{\n    #region 绑定字段（自动生成）\n}\n";

            var replaced = BinderCodeGenerator.TryReplaceRegion(fileContent, "#region 绑定字段（自动生成）\n#endregion",
                out var updated);

            Assert.That(replaced, Is.False);
            Assert.That(updated, Is.Null);
        }

        [TestCase("ScoreText", "UnityEngine.UI.Text", ExpectedResult = "scoreText")]
        [TestCase("ScoreText", "TMPro.TextMeshProUGUI", ExpectedResult = "scoreTextTextMeshProUGUI")]
        [TestCase("Panel", "UnityEngine.Transform", ExpectedResult = "panelTransform")]
        [TestCase("Button", "UnityEngine.UI.Button", ExpectedResult = "button")]
        [TestCase("3DText", "UnityEngine.UI.Text", ExpectedResult = "_3DText")]
        public string ComposeDefaultFieldName_AppendsSuffixWithoutUnderscore(string objectName, string componentFullName)
        {
            return BinderCodeGenerator.ComposeDefaultFieldName(objectName, componentFullName);
        }

        [TestCase("Runestone.AesirModules.AesirBasePanelView`1",
            ExpectedResult = "Runestone.AesirModules.AesirBasePanelView<T>")]
        [TestCase("Ns.Pair`2", ExpectedResult = "Ns.Pair<T1,T2>")]
        [TestCase("UnityEngine.MonoBehaviour", ExpectedResult = "UnityEngine.MonoBehaviour")]
        public string ConvertArityToPlaceholders_ConvertsGenericArity(string fullName)
        {
            return BinderCodeGenerator.ConvertArityToPlaceholders(fullName);
        }

        [TestCase("UnityEngine.MonoBehaviour", "Game.X", ExpectedResult = "UnityEngine.MonoBehaviour")]
        [TestCase("Ns.VC<T>", "Game.HUDContext", ExpectedResult = "Ns.VC<Game.HUDContext>")]
        [TestCase("Ns.P<T1,T2>", "A,B", ExpectedResult = "Ns.P<A,B>")]
        [TestCase("Ns.VC<T>", null, ExpectedResult = "Ns.VC<>")]
        public string BuildBaseTypeReference_SubstitutesGenericArguments(string baseType, string arguments)
        {
            return BinderCodeGenerator.BuildBaseTypeReference(baseType, arguments);
        }

        [TestCase("Ns.VC<T>", ExpectedResult = true)]
        [TestCase("Ns.VC<T1,T2>", ExpectedResult = true)]
        [TestCase("UnityEngine.MonoBehaviour", ExpectedResult = false)]
        [TestCase("", ExpectedResult = false)]
        [TestCase(null, ExpectedResult = false)]
        public bool HasGenericPlaceholder_ReturnsExpectedResults(string baseType)
        {
            return BinderCodeGenerator.HasGenericPlaceholder(baseType);
        }

        [TestCase("Ns.VC<T>", ExpectedResult = 1)]
        [TestCase("Ns.VC<T1,T2>", ExpectedResult = 2)]
        [TestCase("UnityEngine.MonoBehaviour", ExpectedResult = 0)]
        [TestCase(null, ExpectedResult = 0)]
        public int GetGenericPlaceholderArity_ReturnsExpectedResults(string baseType)
        {
            return BinderCodeGenerator.GetGenericPlaceholderArity(baseType);
        }

        [Test]
        public void BuildGeneratedScript_GenericBaseSubstitutesArguments()
        {
            var units = new List<BinderCodeGenerator.BindUnit>
            {
                Unit("UnityEngine.Transform", "hudRoot", "")
            };

            var script = BinderCodeGenerator.BuildGeneratedScript(Config(units,
                "Runestone.AesirModules.AesirBasePanelViewController<T>", "Game.HUDContext"));

            Assert.That(script, Does.Contain(
                "public partial class BattlePanel : " +
                "Runestone.AesirModules.AesirBasePanelViewController<Game.HUDContext>, " +
                "Runestone.AesirModules.IComponentBinder"));
            // 占位不允许出现在生成代码中
            Assert.That(script, Does.Not.Contain("<T>"));
        }

        [TestCase("abc", ExpectedResult = true)]
        [TestCase("_abc1", ExpectedResult = true)]
        [TestCase("1abc", ExpectedResult = false)]
        [TestCase("a-bc", ExpectedResult = false)]
        [TestCase("", ExpectedResult = false)]
        [TestCase(null, ExpectedResult = false)]
        public bool IsValidIdentifier_ReturnsExpectedResults(string value)
        {
            return BinderCodeGenerator.IsValidIdentifier(value);
        }

        [TestCase("Game", ExpectedResult = true)]
        [TestCase("Game.UI.Sub", ExpectedResult = true)]
        [TestCase("Game..UI", ExpectedResult = false)]
        [TestCase("1Game", ExpectedResult = false)]
        [TestCase("Game.2UI", ExpectedResult = false)]
        [TestCase("", ExpectedResult = false)]
        [TestCase(null, ExpectedResult = false)]
        public bool IsValidNamespace_ReturnsExpectedResults(string value)
        {
            return BinderCodeGenerator.IsValidNamespace(value);
        }

        [TestCase("PlayButton", ExpectedResult = "playButton")]
        [TestCase("button", ExpectedResult = "button")]
        [TestCase("3DText", ExpectedResult = "_3DText")]
        [TestCase("", ExpectedResult = "element")]
        [TestCase(null, ExpectedResult = "element")]
        public string ToCamelCase_ReturnsExpectedResults(string name)
        {
            return BinderCodeGenerator.ToCamelCase(name);
        }

        [TestCase("UnityEngine.UI.Button", ExpectedResult = "Button")]
        [TestCase("Game.Logic.Outer+Inner", ExpectedResult = "Inner")]
        [TestCase("Transform", ExpectedResult = "Transform")]
        [TestCase("", ExpectedResult = "Component")]
        [TestCase(null, ExpectedResult = "Component")]
        public string GetTypeShortName_ReturnsExpectedResults(string fullName)
        {
            return BinderCodeGenerator.GetTypeShortName(fullName);
        }

        [TestCase("UnityEngine.UI.Button", ExpectedResult = "UnityEngine.UI")]
        [TestCase("Game.Logic.Outer+Inner", ExpectedResult = "Game.Logic")]
        [TestCase("Transform", ExpectedResult = "")]
        [TestCase("", ExpectedResult = "")]
        [TestCase(null, ExpectedResult = "")]
        public string GetTypeNamespace_ReturnsExpectedResults(string fullName)
        {
            return BinderCodeGenerator.GetTypeNamespace(fullName);
        }

        [Test]
        public void TryFindDuplicateFieldName_DetectsDuplicates()
        {
            var duplicated = new List<BinderCodeGenerator.BindUnit>
            {
                Unit("UnityEngine.Transform", "same", "A"),
                Unit("UnityEngine.Transform", "same", "B")
            };

            Assert.That(BinderCodeGenerator.TryFindDuplicateFieldName(duplicated, out var duplicate), Is.True);
            Assert.That(duplicate, Is.EqualTo("same"));

            var unique = new List<BinderCodeGenerator.BindUnit>
            {
                Unit("UnityEngine.Transform", "first", "A"),
                Unit("UnityEngine.Transform", "second", "B")
            };

            Assert.That(BinderCodeGenerator.TryFindDuplicateFieldName(unique, out duplicate), Is.False);
            Assert.That(duplicate, Is.Null);
        }
    }
}
