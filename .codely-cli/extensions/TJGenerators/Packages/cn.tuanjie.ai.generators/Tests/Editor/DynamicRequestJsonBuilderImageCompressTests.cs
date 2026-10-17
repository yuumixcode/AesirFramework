using System.Text.RegularExpressions;
using NUnit.Framework;
using TJGenerators.Generators;
using UnityEngine;
using UnityEngine.TestTools;

namespace TJGenerators.Editor.Tests
{
    public class DynamicRequestJsonBuilderImageCompressTests
    {
        private const int MaxImageBytes = 10 * 1024 * 1024;

        [Test]
        public void CompressImageIfNeeded_UnderLimit_ReturnsSameReference()
        {
            byte[] small = CreateJpegBytes(64, 64);
            Assert.Less(small.Length, MaxImageBytes);

            byte[] result = DynamicRequestJsonBuilder.CompressImageIfNeeded(small, "small.jpg");

            Assert.AreSame(small, result);
        }

        [Test]
        public void CompressImageIfNeeded_OverLimit_ReturnsCompressedUnderQuota()
        {
            byte[] large = CreateOversizedPngBytes();
            Assert.Greater(large.Length, MaxImageBytes, "fixture must exceed 10MB");

            LogAssert.Expect(LogType.Warning, new Regex("超过 10MB"));

            byte[] result = DynamicRequestJsonBuilder.CompressImageIfNeeded(large, "huge.png");

            Assert.AreNotSame(large, result);
            Assert.LessOrEqual(result.Length, MaxImageBytes);
            Assert.IsTrue(CanLoadImage(result), "compressed bytes should decode as an image");
        }

        [Test]
        public void CompressImageIfNeeded_OverLimitButUndecodable_ReturnsOriginal()
        {
            byte[] garbage = new byte[MaxImageBytes + 1024];
            for (int i = 0; i < garbage.Length; i++)
                garbage[i] = (byte)(i % 251);

            LogAssert.Expect(LogType.Warning, new Regex("无法解码"));

            byte[] result = DynamicRequestJsonBuilder.CompressImageIfNeeded(garbage, "bad.bin");

            Assert.AreSame(garbage, result);
        }

        private static byte[] CreateJpegBytes(int width, int height)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            var pixels = new Color32[width * height];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = new Color32(200, 100, 50, 255);
            tex.SetPixels32(pixels);
            tex.Apply();
            byte[] bytes = tex.EncodeToJPG(90);
            UnityEngine.Object.DestroyImmediate(tex);
            return bytes;
        }

        /// <summary>
        /// 生成噪声 PNG，使文件体积超过 10MB（不依赖仓库内大图资源）。
        /// </summary>
        private static byte[] CreateOversizedPngBytes()
        {
            const int size = 4096;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];
            var rng = new System.Random(42);
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color32(
                    (byte)rng.Next(256),
                    (byte)rng.Next(256),
                    (byte)rng.Next(256),
                    255
                );
            }
            tex.SetPixels32(pixels);
            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(tex);

            if (bytes.Length <= MaxImageBytes)
            {
                Assert.Inconclusive(
                    $"PNG fixture only {bytes.Length} bytes; expected > {MaxImageBytes}. "
                        + "Increase texture size if this fails on a new Unity version."
                );
            }
            return bytes;
        }

        private static bool CanLoadImage(byte[] bytes)
        {
            var tex = new Texture2D(2, 2);
            bool ok = tex.LoadImage(bytes);
            UnityEngine.Object.DestroyImmediate(tex);
            return ok;
        }
    }
}
