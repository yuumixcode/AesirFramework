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
using UnityEngine;

namespace Runestone.AesirInspector
{
    /// <summary>
    /// 双语字符串显示控件，以字段的形式支持多语言。
    /// </summary>
    [Summary("双语字符串显示控件，以字段的形式支持多语言")]
    [Serializable]
    public class BilingualDisplayAsStringControl
    {
        public int fontSize = 13;
        public TextAlignment alignment = TextAlignment.Left;
        public bool enableRichText = true;
        public string format = "";
        public bool overflow;

        public BilingualDisplayAsStringControl(string chinese, string english = null)
        {
            ChineseDisplay = chinese;
            EnglishDisplay = english ?? chinese;
        }

        public string ChineseDisplay { get; set; }
        public string EnglishDisplay { get; set; }
    }
}
