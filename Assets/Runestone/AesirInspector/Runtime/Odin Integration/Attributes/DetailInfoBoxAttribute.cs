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
using System.Diagnostics;
using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration
{
    /// <summary>
    /// 带有详细信息的双语信息框特性。
    /// </summary>
    [Summary("带有详细信息的双语信息框特性")]
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
        [Summary("双语主要信息数据")]
        public BilingualData BilingualData { get; set; }

        /// <summary>
        /// 双语详细信息数据
        /// </summary>
        [Summary("双语详细信息数据")]
        public BilingualData DetailsBilingualData { get; set; }

        /// <summary>
        /// 信息消息类型
        /// </summary>
        [Summary("信息消息类型")]
        public InfoMessageType InfoMessageType { get; set; }

        /// <summary>
        /// 显示条件表达式
        /// </summary>
        [Summary("显示条件表达式")]
        public string VisibleIf { get; set; }

        /// <summary>
        /// GUI 是否始终启用
        /// </summary>
        [Summary("GUI 是否始终启用")]
        public bool GUIAlwaysEnabled { get; set; }
    }
}
