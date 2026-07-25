// ----------------------------------------------------------------------------
// MIT License
// 
// Copyright (c) 2026 Runestone - Yuumix
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ----------------------------------------------------------------------------

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
    [Summary("双语按钮特性。必须硬编码使用特性，不能使用 OdinAttributeProcessor 动态添加使用。")]
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
        [Summary("按钮对齐方式 (0-1)")]
        public float ButtonAlignment { get; set; }

        /// <summary>
        /// 按钮高度
        /// </summary>
        [Summary("按钮高度")]
        public int ButtonHeight { get; set; }

        /// <summary>
        /// 图标对齐方式
        /// </summary>
        [Summary("图标对齐方式")]
        public IconAlignment ButtonIconAlignment { get; set; }

        /// <summary>
        /// 按钮大小
        /// </summary>
        [Summary("按钮大小")]
        public ButtonSizes ButtonSize { get; set; }

        /// <summary>
        /// 中文名称
        /// </summary>
        [Summary("中文名称")]
        public string ChineseName { get; set; }

        /// <summary>
        /// 点击时是否标记为脏
        /// </summary>
        [Summary("点击时是否标记为脏")]
        public bool DirtyOnClick { get; set; }

        /// <summary>
        /// 是否显示参数
        /// </summary>
        [Summary("是否显示参数")]
        public bool DisplayParameters { get; set; }

        /// <summary>
        /// 是否绘制结果
        /// </summary>
        [Summary("是否绘制结果")]
        public bool DrawResult { get; set; }

        /// <summary>
        /// 英文名称
        /// </summary>
        [Summary("英文名称")]
        public string EnglishName { get; set; }

        /// <summary>
        /// 是否展开
        /// </summary>
        [Summary("是否展开")]
        public bool Expanded { get; set; }

        /// <summary>
        /// SDF 图标类型
        /// </summary>
        [Summary("SDF 图标类型")]
        public SdfIconType Icon { get; set; }

        /// <summary>
        /// 是否拉伸
        /// </summary>
        [Summary("是否拉伸")]
        public bool Stretch { get; set; }

        /// <summary>
        /// 按钮样式
        /// </summary>
        [Summary("按钮样式")]
        public ButtonStyle Style { get; set; }

        /// <summary>
        /// 创建 Odin 的 Button 特性。
        /// </summary>
        [Summary("创建 Odin 的 Button 特性")]
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
