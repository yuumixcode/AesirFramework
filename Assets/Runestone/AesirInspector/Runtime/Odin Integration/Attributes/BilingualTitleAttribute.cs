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
    /// 双语标题特性。
    /// </summary>
    [Summary("双语标题特性")]
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
        [Summary("是否在标题前添加空格")]
        public bool BeforeSpace { get; set; }

        /// <summary>
        /// 是否加粗
        /// </summary>
        [Summary("是否加粗")]
        public bool Bold { get; set; }

        /// <summary>
        /// 是否显示水平分割线
        /// </summary>
        [Summary("是否显示水平分割线")]
        public bool HorizontalLine { get; set; }

        /// <summary>
        /// 副标题数据
        /// </summary>
        [Summary("副标题数据")]
        public BilingualData SubtitleData { get; set; }

        /// <summary>
        /// 标题对齐方式
        /// </summary>
        [Summary("标题对齐方式")]
        public TitleAlignments TitleAlignment { get; set; }

        /// <summary>
        /// 标题数据
        /// </summary>
        [Summary("标题数据")]
        public BilingualData TitleData { get; set; }
    }
}
