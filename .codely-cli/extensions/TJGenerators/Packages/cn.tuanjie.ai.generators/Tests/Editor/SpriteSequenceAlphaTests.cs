using System;
using System.IO;
using NUnit.Framework;
using TJGenerators.PostProcessing;
using TJGenerators.Utils;
using UnityEditor;
using UnityEngine;

namespace TJGenerators.Editor.Tests
{
    public class SpriteSequenceAlphaTests
    {
        private string _folder;
        private string _output;

        [SetUp]
        public void SetUp()
        {
            string name = "AlphaSequenceTest_" + Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", name);
            _folder = "Assets/" + name;
        }

        [TearDown]
        public void TearDown()
        {
            if (!string.IsNullOrEmpty(_output)) AssetDatabase.DeleteAsset(_output);
            _output = null;
            AssetDatabase.DeleteAsset(_folder);
        }

        private Texture2D Slice(bool transparent, bool preserveAlpha)
        {
            var source = new Texture2D(16, 8, TextureFormat.RGBA32, false);
            string path = _folder + "/source.png";
            try
            {
                var pixels = new Color[128];
                for (int i = 0; i < pixels.Length; i++) pixels[i] = transparent ? Color.clear : Color.green;
                pixels[3 * 16 + 3] = transparent ? Color.green : Color.red;
                pixels[3 * 16 + 11] = transparent ? Color.green : Color.red;
                source.SetPixels(pixels);
                source.Apply();
                File.WriteAllBytes(PathUtils.ToAbsoluteAssetPath(path), source.EncodeToPNG());
            }
            finally { UnityEngine.Object.DestroyImmediate(source); }
            AssetDatabase.ImportAsset(path);
            var result = SpriteSequencePostProcess.CutoutAndSlice(path, 0.16f, 0.04f, 2, 1, 12, true,
                writeCutoutBackToAsset: false,
                preserveExistingAlpha: preserveAlpha);
            _output = result.OutputDirectory;
            Assert.AreEqual(2, result.ExportedCount);
            Assert.NotNull(AssetDatabase.LoadAssetAtPath<AnimationClip>(result.AnimationClipPath));
            return SpriteSequencePostProcess.LoadReadableTextureFromAssetPath(result.SpriteAssetPaths[0]);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ExistingAlpha_PreservesGreenForegroundOnlyWhenRequested(bool preserveAlpha)
        {
            var frame = Slice(true, preserveAlpha);
            try
            {
                Assert.AreEqual(0f, frame.GetPixel(0, 0).a);
                Assert.AreEqual(preserveAlpha ? 1f : 0f, frame.GetPixel(3, 3).a);
                if (preserveAlpha) Assert.AreEqual(Color.green, frame.GetPixel(3, 3));
            }
            finally { UnityEngine.Object.DestroyImmediate(frame); }
        }

        [Test]
        public void OpaqueGreenScreen_StillKeysBackgroundInAutomaticMode()
        {
            var frame = Slice(false, true);
            try
            {
                Assert.AreEqual(0f, frame.GetPixel(0, 0).a);
                Assert.AreEqual(Color.red, frame.GetPixel(3, 3));
            }
            finally { UnityEngine.Object.DestroyImmediate(frame); }
        }
    }
}
