using System;
using System.Diagnostics;
using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration
{
    /// <summary>
    /// 双语信息框特性。
    /// </summary>
    [DontApplyToListElements]
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    [Conditional("UNITY_EDITOR")]
    public class BilingualInfoBoxAttribute : Attribute
    {
        public BilingualInfoBoxAttribute(string chinese,
            string english = null,
            InfoMessageType infoMessageType = InfoMessageType.Info,
            SdfIconType icon = SdfIconType.None,
            string visibleIf = "",
            string iconColor = null,
            bool guiAlwaysEnabled = false)
        {
            BilingualData = new BilingualData(chinese, english);
            InfoMessageType = infoMessageType;
            Icon = icon;
            VisibleIf = visibleIf;
            IconColor = iconColor;
            GUIAlwaysEnabled = guiAlwaysEnabled;
        }

        /// <summary>
        /// GUI 是否始终启用
        /// </summary>
        public bool GUIAlwaysEnabled { get; set; }

        /// <summary>
        /// SDF 图标类型
        /// </summary>
        public SdfIconType Icon { get; set; }

        /// <summary>
        /// 图标颜色
        /// </summary>
        public string IconColor { get; set; }

        /// <summary>
        /// 信息消息类型
        /// </summary>
        public InfoMessageType InfoMessageType { get; set; }

        /// <summary>
        /// 显示条件表达式
        /// </summary>
        public string VisibleIf { get; set; }

        /// <summary>
        /// 双语数据
        /// </summary>
        public BilingualData BilingualData { get; set; }

        /// <summary>
        /// 是否定义了图标
        /// </summary>
        public bool HasDefinedIcon => Icon != SdfIconType.None;
    }
}
