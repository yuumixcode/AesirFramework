using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// 特性信息数据抽象基类，定义面板所需的全部显示数据。
    /// </summary>
    public abstract class AbstractAttributeData
    {
        /// <summary>
        /// 顶部说明控件。
        /// </summary>
        public abstract BilingualHeaderControl BilingualHeaderControl { get; set; }

        /// <summary>
        /// 使用提示数组。
        /// </summary>
        public abstract BilingualData[] UsageTips { get; set; }

        /// <summary>
        /// 特性参数数组。
        /// </summary>
        public abstract ParameterValue[] AttributeParameters { get; set; }

        /// <summary>
        /// 被解析的字符串参数数组。
        /// </summary>
        public abstract ResolvedStringParameterValue[] ResolvedStringParameters { get; set; }

        /// <summary>
        /// 使用案例预览项数组。
        /// </summary>
        public abstract AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; }

        /// <summary>
        /// 获取初始显示的案例 ScriptableObject。
        /// </summary>
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
