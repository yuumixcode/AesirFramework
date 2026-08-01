using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace Runestone.AesirInspector.OdinIntegration
{
    /// <summary>
    /// 双语按钮特性。必须硬编码使用特性，不能使用 OdinAttributeProcessor 动态添加使用。
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = false)]
    [Conditional("UNITY_EDITOR")]
    public class BilingualButtonAttribute : ShowInInspectorAttribute
    {
        public BilingualButtonAttribute(string chineseName,
            string englishName = null,
            ButtonSizes buttonSize = ButtonSizes.Medium,
            ButtonStyle style = ButtonStyle.Box,
            SdfIconType icon = SdfIconType.None,
            IconAlignment buttonIconAlignment = IconAlignment.LeftOfText,
            int buttonHeight = -1,
            bool stretch = true,
            bool drawResult = true,
            bool expanded = false,
            float buttonAlignment = 0.5f,
            bool displayParameters = true,
            bool dirtyOnClick = true)
        {
            ChineseName = chineseName;
            EnglishName = englishName ?? chineseName;
            ButtonSize = buttonSize;
            ButtonHeight = buttonHeight;
            Style = style;
            Icon = icon;
            ButtonIconAlignment = buttonIconAlignment;
            Stretch = stretch;
            DrawResult = drawResult;
            Expanded = expanded;
            ButtonAlignment = buttonAlignment;
            DisplayParameters = displayParameters;
            DirtyOnClick = dirtyOnClick;
        }

        /// <summary>
        /// 按钮对齐方式 (0-1)
        /// </summary>
        public float ButtonAlignment { get; set; }

        /// <summary>
        /// 按钮高度
        /// </summary>
        public int ButtonHeight { get; set; }

        /// <summary>
        /// 图标对齐方式
        /// </summary>
        public IconAlignment ButtonIconAlignment { get; set; }

        /// <summary>
        /// 按钮大小
        /// </summary>
        public ButtonSizes ButtonSize { get; set; }

        /// <summary>
        /// 中文名称
        /// </summary>
        public string ChineseName { get; set; }

        /// <summary>
        /// 点击时是否标记为脏
        /// </summary>
        public bool DirtyOnClick { get; set; }

        /// <summary>
        /// 是否显示参数
        /// </summary>
        public bool DisplayParameters { get; set; }

        /// <summary>
        /// 是否绘制结果
        /// </summary>
        public bool DrawResult { get; set; }

        /// <summary>
        /// 英文名称
        /// </summary>
        public string EnglishName { get; set; }

        /// <summary>
        /// 是否展开
        /// </summary>
        public bool Expanded { get; set; }

        /// <summary>
        /// SDF 图标类型
        /// </summary>
        public SdfIconType Icon { get; set; }

        /// <summary>
        /// 是否拉伸
        /// </summary>
        public bool Stretch { get; set; }

        /// <summary>
        /// 按钮样式
        /// </summary>
        public ButtonStyle Style { get; set; }

        /// <summary>
        /// 创建 Odin 的 Button 特性。
        /// </summary>
        public ButtonAttribute CreateButton()
        {
            var button = new ButtonAttribute(ChineseName, ButtonSize)
            {
                Style = Style,
                Icon = Icon,
                IconAlignment = ButtonIconAlignment,
                ButtonAlignment = ButtonAlignment,
                Stretch = Stretch,
                DrawResult = DrawResult,
                Expanded = Expanded,
                DisplayParameters = DisplayParameters,
                DirtyOnClick = DirtyOnClick
            };
            // 如果 ButtonHeight 大于 ButtonSize 对应的高度，则覆盖
            if (ButtonHeight > (int)ButtonSize)
            {
                button.ButtonHeight = ButtonHeight;
            }

            return button;
        }
    }

#if UNITY_EDITOR
    internal sealed class BilingualAttributeProcessor<T> : OdinAttributeProcessor<T> where T : class
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty,
            MemberInfo member,
            List<Attribute> attributes)
        {
            if (member.MemberType == MemberTypes.Method &&
                member.GetCustomAttribute<BilingualButtonAttribute>() != null)
            {
                var button = member.GetCustomAttribute<BilingualButtonAttribute>();
                var chineseButton = button.CreateButton();
                attributes.Add(chineseButton);
            }
        }
    }

#endif
}
