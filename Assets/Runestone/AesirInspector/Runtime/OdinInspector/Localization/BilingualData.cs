using System;

namespace Runestone.AesirInspector
{
    /// <summary>
    /// 双语数据结构体，存放中文和英文两个字段。
    /// </summary>
    public readonly struct BilingualData : IEquatable<BilingualData>
    {
        /// <summary>
        /// 空的 BilingualData 实例，中文和英文均为空字符串，类似于 string.Empty。
        /// </summary>
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
        public string GetChinese() => _chinese;

        /// <summary>
        /// 获取英文文本。
        /// </summary>
        public string GetEnglish() => _english;

        /// <summary>
        /// 判断是否相等。
        /// </summary>
        public bool Equals(BilingualData other) => _chinese == other._chinese && _english == other._english;

        /// <summary>
        /// 返回当前编辑器语言的文本或者回退到中文。
        /// </summary>
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
        public override string ToString() => GetCurrentOrFallback();

        /// <summary>
        /// 隐式类型转换，BilingualData 可以直接转换为 String。
        /// </summary>
        public static implicit operator string(BilingualData data) => data.GetCurrentOrFallback();
    }
}
