using System;
using System.Diagnostics;
using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration
{
    /// <summary>
    /// 双语标题特性。
    /// </summary>
    [DontApplyToListElements]
    [AttributeUsage(AttributeTargets.All)]
    [Conditional("UNITY_EDITOR")]
    public class BilingualTitleAttribute : Attribute
    {
        public BilingualTitleAttribute(string chineseTitle,
            string englishTitle = null,
            string chineseSubTitle = null,
            string englishSubTitle = null,
            TitleAlignments titleAlignment = TitleAlignments.Left,
            bool horizontalLine = true,
            bool bold = true,
            bool beforeSpace = true)
        {
            TitleData = new BilingualData(chineseTitle, englishTitle);
            SubtitleData = new BilingualData(chineseSubTitle, englishSubTitle);
            TitleAlignment = titleAlignment;
            HorizontalLine = horizontalLine;
            Bold = bold;
            BeforeSpace = beforeSpace;
        }

        /// <summary>
        /// 是否在标题前添加空格
        /// </summary>
        public bool BeforeSpace { get; set; }

        /// <summary>
        /// 是否加粗
        /// </summary>
        public bool Bold { get; set; }

        /// <summary>
        /// 是否显示水平分割线
        /// </summary>
        public bool HorizontalLine { get; set; }

        /// <summary>
        /// 副标题数据
        /// </summary>
        public BilingualData SubtitleData { get; set; }

        /// <summary>
        /// 标题对齐方式
        /// </summary>
        public TitleAlignments TitleAlignment { get; set; }

        /// <summary>
        /// 标题数据
        /// </summary>
        public BilingualData TitleData { get; set; }
    }
}
