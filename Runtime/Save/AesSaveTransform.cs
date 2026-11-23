using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Runestone.AesirModules
{
    /// <summary>
    /// AES-256 加密变换（内置，默认不启用）。密钥由游戏侧代码注入（SHA256(password) 派生 256-bit），
    /// <b>不入配置资产、不入存档文件</b>；每次保存随机 IV 并前置 16 字节写入变换输出（IV 无需保密）。
    /// </summary>
    /// <remarks>
    /// 安全边界：加密只防 casual 修改（玩家用文本编辑器改档），不防逆向——密钥在客户端二进制内可被提取；
    /// 防作弊需服务端校验，超出模块范围。错误密钥加载时解密失败或产出垃圾字节，经校验和判定为损坏。
    /// 变换链中建议置于压缩之后（先压缩后加密，密文不可压缩）。
    /// </remarks>
    public class AesSaveTransform : ISaveTransform
    {
        const int IvLength = 16;

        readonly byte[] _key;

        /// <summary>构造 AES 变换。</summary>
        /// <param name="password">密钥口令（非空；经 SHA256 派生 256-bit 密钥，仅存在于运行时内存）。</param>
        /// <exception cref="ArgumentException">口令为空。</exception>
        public AesSaveTransform(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("加密口令不可为空", nameof(password));
            }

            using (var sha = SHA256.Create())
            {
                _key = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        /// <inheritdoc />
        public string Id => "aes";

        /// <inheritdoc />
        public byte[] Encode(byte[] data)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                aes.GenerateIV();

                using (var output = new MemoryStream())
                {
                    output.Write(aes.IV, 0, IvLength);
                    using (var crypto = new CryptoStream(output, aes.CreateEncryptor(),
                               CryptoStreamMode.Write, leaveOpen: true))
                    {
                        crypto.Write(data, 0, data.Length);
                    }

                    return output.ToArray();
                }
            }
        }

        /// <inheritdoc />
        public byte[] Decode(byte[] data)
        {
            if (data == null || data.Length < IvLength)
            {
                throw new CryptographicException("AES 存档数据过短，缺少 IV 前缀");
            }

            using (var aes = Aes.Create())
            {
                aes.Key = _key;
                var iv = new byte[IvLength];
                Buffer.BlockCopy(data, 0, iv, 0, IvLength);
                aes.IV = iv;

                using (var input = new MemoryStream(data, IvLength, data.Length - IvLength))
                using (var crypto = new CryptoStream(input, aes.CreateDecryptor(), CryptoStreamMode.Read))
                using (var output = new MemoryStream())
                {
                    crypto.CopyTo(output);
                    return output.ToArray();
                }
            }
        }
    }
}