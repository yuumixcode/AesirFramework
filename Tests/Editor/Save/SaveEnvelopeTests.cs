using System;
using System.Linq;
using NUnit.Framework;

namespace Runestone.AesirModules.Tests.Editor.Save
{
    /// <summary>
    /// 信封与键校验的 EditMode 用例：二进制头往返、损坏检测（位翻转 / 截断 / magic / 版本）、
    /// 键白名单（spec「完整性校验」「存档键合法性」）。
    /// </summary>
    public class SaveEnvelopeTests
    {
        [Test]
        public void Wrap_ThenUnwrap_RoundTripsAllFields()
        {
            var payload = new byte[] { 1, 2, 3, 4, 5 };
            var transformIds = new[] { "gzip", "aes" };

            var file = SaveEnvelope.Wrap("unity-json", transformIds, 7, payload);
            Assert.IsTrue(SaveEnvelope.TryUnwrap(file, out var data, out var error), error);

            Assert.AreEqual("unity-json", data.SerializerId);
            Assert.AreEqual(transformIds, data.TransformIds);
            Assert.AreEqual(7, data.DataVersion);
            Assert.AreEqual(payload, data.Payload);
        }

        [Test]
        public void TryUnwrap_BitFlippedPayload_FailsChecksum()
        {
            var file = SaveEnvelope.Wrap("unity-json", Array.Empty<string>(), 0, new byte[] { 10, 20, 30 });
            file[file.Length - 1] ^= 0xFF;

            Assert.IsFalse(SaveEnvelope.TryUnwrap(file, out _, out var error));
            Assert.IsNotNull(error, "损坏描述不可为空（结果对象需携带错误信息）");
        }

        [Test]
        public void TryUnwrap_TruncatedFile_Fails()
        {
            var file = SaveEnvelope.Wrap("unity-json", Array.Empty<string>(), 0, new byte[] { 10, 20, 30 });
            var truncated = file.Take(file.Length / 2).ToArray();

            Assert.IsFalse(SaveEnvelope.TryUnwrap(truncated, out _, out _));
        }

        [Test]
        public void TryUnwrap_WrongMagic_Fails()
        {
            var file = SaveEnvelope.Wrap("unity-json", Array.Empty<string>(), 0, new byte[] { 1 });
            file[0] = (byte)'X';

            Assert.IsFalse(SaveEnvelope.TryUnwrap(file, out _, out _));
        }

        [Test]
        public void TryUnwrap_UnsupportedFormatVersion_Fails()
        {
            var file = SaveEnvelope.Wrap("unity-json", Array.Empty<string>(), 0, new byte[] { 1 });
            file[4] = 2; // 信封格式版本字段

            Assert.IsFalse(SaveEnvelope.TryUnwrap(file, out _, out _));
        }

        [TestCase("")]
        [TestCase("../escape")]
        [TestCase("a/b")]
        [TestCase("a b")]
        [TestCase("a.b")]
        [TestCase("slot\\0")]
        public void SaveKey_InvalidKey_ThrowsArgumentException(string key)
        {
            Assert.Throws<ArgumentException>(() => SaveKey.Validate(key));
        }

        [TestCase("slot_0")]
        [TestCase("Quicksave-1")]
        [TestCase("abc123")]
        public void SaveKey_ValidKey_Accepted(string key)
        {
            Assert.DoesNotThrow(() => SaveKey.Validate(key));
        }
    }
}