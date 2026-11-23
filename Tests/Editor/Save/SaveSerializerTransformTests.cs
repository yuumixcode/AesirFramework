using System;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;
using UnityEngine;

namespace Runestone.AesirModules.Tests.Editor.Save
{
    /// <summary>
    /// 默认序列化器与内置变换的 EditMode 用例：UnityJson 往返、GZip/AES 往返、
    /// 错误密钥不可还原、随机 IV（spec「三轨序列化」默认轨 +「可选字节变换链」）。
    /// </summary>
    public class SaveSerializerTransformTests
    {
        [Test]
        public void UnityJson_RoundTripsPublicFieldsListAndVector3()
        {
            var serializer = new UnityJsonSerializer();
            var payload = SaveTestPayload.Sample();

            var restored = serializer.Deserialize<SaveTestPayload>(serializer.Serialize(payload));

            Assert.AreEqual(payload.Score, restored.Score);
            Assert.AreEqual(payload.Health, restored.Health);
            Assert.AreEqual(payload.Name, restored.Name);
            Assert.AreEqual(payload.Inventory, restored.Inventory);
            Assert.AreEqual(payload.Position, restored.Position);
        }

        [Test]
        public void GZip_RoundTrips()
        {
            var transform = new GZipSaveTransform();
            var data = Enumerable.Repeat((byte)7, 4096).ToArray();

            var encoded = transform.Encode(data);
            Assert.Less(encoded.Length, data.Length, "高重复数据压缩后应更小");
            Assert.AreEqual(data, transform.Decode(encoded));
        }

        [Test]
        public void Aes_RoundTrips()
        {
            var transform = new AesSaveTransform("correct-password");
            var data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

            Assert.AreEqual(data, transform.Decode(transform.Encode(data)));
        }

        [Test]
        public void Aes_WrongKey_CannotRecoverOriginal()
        {
            var data = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            var encoded = new AesSaveTransform("correct-password").Encode(data);

            // 错误密钥：解密抛 CryptographicException（padding 失败）或产出垃圾字节，两者都不可还原
            var recoverable = false;
            try
            {
                var decoded = new AesSaveTransform("wrong-password").Decode(encoded);
                recoverable = decoded.SequenceEqual(data);
            }
            catch (CryptographicException)
            {
                // 预期路径之一
            }

            Assert.IsFalse(recoverable, "错误密钥不得还原出原始数据");
        }

        [Test]
        public void Aes_Encode_UsesRandomIvPerCall()
        {
            var transform = new AesSaveTransform("password");
            var data = new byte[] { 1, 2, 3 };

            var first = transform.Encode(data);
            var second = transform.Encode(data);

            Assert.Greater(first.Length, 16, "输出应含 16 字节 IV 前缀");
            Assert.AreNotEqual(first.Take(16).ToArray(), second.Take(16).ToArray(),
                "每次保存应使用随机 IV（同明文两次加密的 IV 前缀不得相同）");
        }
    }
}