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

namespace Runestone.AesirInspector
{
    /// <summary>
    /// 双语数据结构体，存放中文和英文两个字段。
    /// </summary>
    [Summary("双语数据结构体，存放中文和英文两个字段")]
    public readonly struct BilingualData : IEquatable<BilingualData>
    {
        /// <summary>
        /// 空的 BilingualData 实例，中文和英文均为空字符串，类似于 string.Empty。
        /// </summary>
        [Summary("空的 BilingualData 实例，中文和英文均为空字符串，类似于 string.Empty")]
        public static BilingualData Empty => new BilingualData(string.Empty, string.Empty);

        readonly string _chinese;
        readonly string _english;

        public BilingualData(string chinese, string english)
        {
            _chinese = chinese;
            _english = english;
        }

        /// <summary>
        /// 获取中文文本。
        /// </summary>
        [Summary("获取中文文本")]
        public string GetChinese() => _chinese;

        /// <summary>
        /// 获取英文文本。
        /// </summary>
        [Summary("获取英文文本")]
        public string GetEnglish() => _english;

        /// <summary>
        /// 判断是否相等。
        /// </summary>
        [Summary("判断是否相等")]
        public bool Equals(BilingualData other) => _chinese == other._chinese && _english == other._english;

        /// <summary>
        /// 返回当前编辑器语言的文本或者回退到中文。
        /// </summary>
        [Summary("返回当前编辑器语言的文本或者回退到中文")]
        public string GetCurrentOrFallback()
        {
            var provider = LanguageProviderLocator.Provider;
            if (provider != null && provider.IsEnglish && !string.IsNullOrWhiteSpace(_english))
            {
                return _english;
            }

            return _chinese;
        }

        /// <summary>
        /// 重写 ToString 方法。
        /// </summary>
        [Summary("重写 ToString 方法")]
        public override string ToString() => GetCurrentOrFallback();

        /// <summary>
        /// 隐式类型转换，BilingualData 可以直接转换为 String。
        /// </summary>
        [Summary("隐式类型转换，BilingualData 可以直接转换为 String")]
        public static implicit operator string(BilingualData data) => data.GetCurrentOrFallback();
    }
}
