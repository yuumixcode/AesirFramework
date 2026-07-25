using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// 特性信息数据抽象基类，定义面板所需的全部显示数据。
    /// </summary>
    [Summary("特性信息数据抽象基类，定义面板所需的全部显示数据")]
    public abstract class AbstractAttributeData
    {
        /// <summary>
        /// 顶部说明控件。
        /// </summary>
        [Summary("顶部说明控件")]
        public abstract BilingualHeaderControl BilingualHeaderControl { get; set; }

        /// <summary>
        /// 使用提示数组。
        /// </summary>
        [Summary("使用提示数组")]
        public abstract BilingualData[] UsageTips { get; set; }

        /// <summary>
        /// 特性参数数组。
        /// </summary>
        [Summary("特性参数数组")]
        public abstract ParameterValue[] AttributeParameters { get; set; }

        /// <summary>
        /// 被解析的字符串参数数组。
        /// </summary>
        [Summary("被解析的字符串参数数组")]
        public abstract ResolvedStringParameterValue[] ResolvedStringParameters { get; set; }

        /// <summary>
        /// 使用案例预览项数组。
        /// </summary>
        [Summary("使用案例预览项数组")]
        public abstract AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; }

        /// <summary>
        /// 获取初始显示的案例 ScriptableObject。
        /// </summary>
        [Summary("获取初始显示的案例 ScriptableObject")]
        public ScriptableObject GetInitialExample()
        {
            if (ExamplePreviewItems == null || ExamplePreviewItems.Length == 0)
            {
                return null;
            }

            var firstItem = ExamplePreviewItems[0];
            return firstItem.ExampleType == AttributeExampleType.OdinSerialized
                ? firstItem.OdinSerializedExample
                : firstItem.UnitySerializedExample;
        }
    }
}
