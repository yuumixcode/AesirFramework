using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// Odin 语法高亮处理器可视化面板，基于 AesirCodeHighlighter 提供语法高亮测试功能
    /// </summary>
    public class OdinSyntaxHighlighterPanelSO : ScriptableObject
    {
        /// <summary>
        /// EditorBuildSettings 存储引用的 Key
        /// </summary>
        static readonly string ConfigName =
            typeof(OdinSyntaxHighlighterPanelSO).GetNiceFullName();

        [PropertyOrder(-100)]
        public BilingualHeaderControl bilingualHeader;

        [BilingualTitle("源码示例", "Source Code Example")]
        [HideLabel]
        [TextArea(10, 15)]
        public string exampleSourceCode = @"using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Example : ScriptableObject
{
    public int ExampleInt;
    public float ExampleFloat;
    public string ExampleString;
    public bool ExampleBool;
    public Vector3 ExampleVector3;
    public Color ExampleColor;
    public GameObject ExampleGameObject;
    public List<int> ExampleList;
    public Dictionary<string, int> ExampleDictionary; 
}";

        [PropertyOrder(-5)]
        public BilingualDisplayAsStringControl firstTip;

        [PropertyOrder(-5)]
        public BilingualDisplayAsStringControl secondTip;

        [PropertyOrder(-5)]
        public BilingualDisplayAsStringControl thirdTip;

        [PropertyOrder(-5)]
        public BilingualDisplayAsStringControl fourthTip;

        /// <summary>
        /// 获取 OdinSyntaxHighlighterSO 单例
        /// </summary>
        public static OdinSyntaxHighlighterPanelSO Instance =>
            ScriptableObjectSafeEditorUtility.GetOrCreateEditorScriptableObject<OdinSyntaxHighlighterPanelSO>(
                ConfigName, AesirInspectorPaths.MiniToolsAssetsFolderPath, "OdinSyntaxHighlighter");

        void OnEnable()
        {
            bilingualHeader = new BilingualHeaderControl("语法高亮处理器", "Syntax Highlighter",
                "获取 Odin 的语法高亮处理器，直接使用。", "Get Odin Inspector Syntax Highlighter, Directly Use.");
            firstTip = new BilingualDisplayAsStringControl("1.被处理的源代码中，不能包含有命名空间。",
                "1.Processed Code Cannot Contain Namespace.");
            secondTip = new BilingualDisplayAsStringControl("2.被处理的源代码中，不能包含有 $ 内插字符串。",
                "2.Processed Code Cannot Contain Interpolated Strings.");
            thirdTip = new BilingualDisplayAsStringControl("3.被处理的源代码需要提前格式化，保证合理的空格。",
                "3.Processed Code Needs To Be Formatted With Reasonable Spaces.");
            fourthTip = new BilingualDisplayAsStringControl("4.被处理的源代码要注意富文本标签的使用，失效时检查是否有此类原因。",
                "4.Processed Code Should Pay Attention To Rich Text Tag Usage, Check For Such Reasons When It Fails.");
        }

        /// <summary>
        /// 使用富文本标记进行脚本语法高亮。委托给 AesirCodeHighlighter 实现。
        /// </summary>
        public static string ApplyCodeHighlighting(string code) =>
            OdinCodeHighlighter.ApplyHighlighting(code);

        [PropertySpace(10)]
        [BilingualInfoBox("查看 Console 窗口输出", "See Console Window Output")]
        [BilingualInfoBox("使用 OdinSyntaxHighlighterSO.ApplyCodeHighlighting(sourceCode) 处理源代码",
            "Use OdinSyntaxHighlighterSO.ApplyCodeHighlighting(sourceCode)")]
        [BilingualButton("输出语法高亮结果", "Log SyntaxHighlighting Result", ButtonSizes.Large)]
        public void TestSyntaxHighlight()
        {
            Debug.Log(ApplyCodeHighlighting(exampleSourceCode));
        }

        [PropertyOrder(-10)]
        [BilingualTitle("语法高亮处理器具有局限性", "Syntax highlighting processor has certain limitations",
            TitleAlignment = TitleAlignments.Centered)]
        [OnInspectorGUI]
        void OnGUI1() { }
    }
}
