using System;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 存档键校验：键直接映射存储文件名，白名单校验防止路径穿越写到存储目录之外。
    /// 非法键是编程错误，抛 <see cref="ArgumentException" />（fail-fast）。
    /// </summary>
    internal static class SaveKey
    {
        /// <summary>
        /// 校验键合法性：非空且仅由字母、数字、下划线、连字符组成。
        /// </summary>
        /// <param name="key">待校验键。</param>
        /// <exception cref="ArgumentException">键为空或含白名单外字符（路径分隔符 / 空白 / .. 等）。</exception>
        public static void Validate(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("存档键不可为空", nameof(key));
            }

            for (var i = 0; i < key.Length; i++)
            {
                var c = key[i];
                if (char.IsLetterOrDigit(c) || c == '_' || c == '-')
                {
                    continue;
                }

                throw new ArgumentException(
                    $"存档键含非法字符 '{c}'（键='{key}'）：仅允许字母、数字、下划线与连字符，防止路径穿越",
                    nameof(key));
            }
        }
    }
}