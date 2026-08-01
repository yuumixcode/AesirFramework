using System;
using System.Diagnostics;
using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration
{
    /// <summary>
    /// 带有详细信息的双语信息框特性。
    /// </summary>
    [DontApplyToListElements]
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    [Conditional("UNITY_EDITOR")]
    public class DetailInfoBoxAttribute : Attribute
    {
        public DetailInfoBoxAttribute(string chinese,
            string english = null,
            string detailsChinese = null,
            string detailsEnglish = null,
            InfoMessageType infoMessageType = InfoMessageType.Info,
            string visibleIf = null,
            bool guiAlwaysEnabled = false)
        {
            BilingualData = new BilingualData(chinese, english);
            DetailsBilingualData = new BilingualData(detailsChinese, detailsEnglish);
            InfoMessageType = infoMessageType;
            VisibleIf = visibleIf;
            GUIAlwaysEnabled = guiAlwaysEnabled;
        }

        /// <summary>
        /// 双语主要信息数据
        /// </summary>
        public BilingualData BilingualData { get; set; }

        /// <summary>
        /// 双语详细信息数据
        /// </summary>
        public BilingualData DetailsBilingualData { get; set; }

        /// <summary>
        /// 信息消息类型
        /// </summary>
        public InfoMessageType InfoMessageType { get; set; }

        /// <summary>
        /// 显示条件表达式
        /// </summary>
        public string VisibleIf { get; set; }

        /// <summary>
        /// GUI 是否始终启用
        /// </summary>
        public bool GUIAlwaysEnabled { get; set; }
    }
}
