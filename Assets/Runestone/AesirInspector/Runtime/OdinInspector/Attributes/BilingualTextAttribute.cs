using System;
using System.Diagnostics;
using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration
{
    /// <summary>
    /// 双语文本特性。
    /// </summary>
    [DontApplyToListElements]
    [AttributeUsage(AttributeTargets.All)]
    [Conditional("UNITY_EDITOR")]
    public class BilingualTextAttribute : Attribute
    {
        public BilingualTextAttribute(string chinese,
            string english = null,
            bool nicifyEnglishText = true,
            SdfIconType icon = SdfIconType.None,
            string iconColor = null)
        {
            BilingualData = new BilingualData(chinese, english);
            NicifyEnglishText = nicifyEnglishText;
            Icon = icon;
            IconColor = iconColor;
        }

        /// <summary>
        /// SDF 图标类型
        /// </summary>
        public SdfIconType Icon { get; set; }

        /// <summary>
        /// 图标颜色，支持 Odin Inspector 的颜色设置
        /// </summary>
        public string IconColor { get; set; }

        /// <summary>
        /// 是否美化英文文本
        /// </summary>
        public bool NicifyEnglishText { get; set; }

        /// <summary>
        /// 双语数据
        /// </summary>
        public BilingualData BilingualData { get; set; }
    }
}
