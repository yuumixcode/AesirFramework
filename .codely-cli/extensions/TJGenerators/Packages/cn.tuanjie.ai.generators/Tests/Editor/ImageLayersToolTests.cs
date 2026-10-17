using System.Collections.Generic;
using System.IO;
using Codely.Newtonsoft.Json.Linq;
using NUnit.Framework;
using TJGenerators;
using TJGenerators.Utils;
using UnityEditor;
using UnityEngine;
using UnityTcp.Editor.Tools;

namespace TJGenerators.Editor.Tests
{
    /// <summary>
    /// Offline unit tests for image-layering param parsing and sibling path collection.
    /// </summary>
    public class ImageLayersToolTests
    {
        private const string TestFolder = "Assets/TJGenerators/History/__image_layers_unit_test__";

        [TearDown]
        public void TearDown()
        {
            if (AssetDatabase.IsValidFolder(TestFolder))
                AssetDatabase.DeleteAsset(TestFolder);
        }

        [Test]
        public void CollectLayerPaths_FindsImageLayersIndexedSiblings()
        {
            EnsureTestFolder();

            string layer0 = TestFolder + "/ImageLayers_20260101_120000.png";
            string layer1 = TestFolder + "/ImageLayers_20260101_120000_1.png";
            string layer2 = TestFolder + "/ImageLayers_20260101_120000_2.png";
            WriteTinyPng(layer0);
            WriteTinyPng(layer1);
            WriteTinyPng(layer2);

            List<string> found = GenerateImageLayersTool.CollectLayerPaths(layer0, expectedCount: 4);
            Assert.AreEqual(3, found.Count);
            Assert.AreEqual(layer0, found[0]);
            Assert.AreEqual(layer1, found[1]);
            Assert.AreEqual(layer2, found[2]);
        }

        [Test]
        public void CollectIndexedSiblingPaths_StopsWhenGap()
        {
            EnsureTestFolder();

            string layer0 = TestFolder + "/ImageLayers_gap_test.png";
            string layer1 = TestFolder + "/ImageLayers_gap_test_1.png";
            // intentionally skip _2
            string layer3 = TestFolder + "/ImageLayers_gap_test_3.png";
            WriteTinyPng(layer0);
            WriteTinyPng(layer1);
            WriteTinyPng(layer3);

            List<string> found = GeneratedTextureImportUtils.CollectIndexedSiblingPaths(layer0, 8);
            Assert.AreEqual(2, found.Count);
            Assert.AreEqual(layer0, found[0]);
            Assert.AreEqual(layer1, found[1]);
        }

        [Test]
        public void CollectIndexedSiblingPaths_DoesNotMatchDoubleDigitIndexAsSingleDigit()
        {
            EnsureTestFolder();

            string layer0 = TestFolder + "/ImageLayers_index_test.png";
            string layer10 = TestFolder + "/ImageLayers_index_test_10.png";
            WriteTinyPng(layer0);
            WriteTinyPng(layer10);

            List<string> found = GeneratedTextureImportUtils.CollectIndexedSiblingPaths(layer0, 11);
            Assert.AreEqual(1, found.Count);
            Assert.AreEqual(layer0, found[0]);
        }

        [Test]
        public void MatchesIndexedSiblingFileName_AllowsUniqueAssetPathSuffix()
        {
            Assert.IsTrue(GeneratedTextureImportUtils.MatchesIndexedSiblingFileName(
                "ImageLayers_20260101_120000_1 1", "ImageLayers_20260101_120000", 1));
            Assert.IsFalse(GeneratedTextureImportUtils.MatchesIndexedSiblingFileName(
                "ImageLayers_20260101_120000_10", "ImageLayers_20260101_120000", 1));
        }

        [Test]
        public void CollectIndexedSiblingPaths_EmptyFirst_ReturnsEmpty()
        {
            Assert.AreEqual(0, GeneratedTextureImportUtils.CollectIndexedSiblingPaths(null, 4).Count);
            Assert.AreEqual(0, GeneratedTextureImportUtils.CollectIndexedSiblingPaths("", 4).Count);
        }

        [Test]
        public void ParseSeedreamSize_TiersAndAuto()
        {
            Assert.AreEqual("1K", GenerateImageLayersTool.ParseSeedreamSize(new JValue("1K")));
            Assert.AreEqual("1.5K", GenerateImageLayersTool.ParseSeedreamSize(new JValue("1.5K")));
            Assert.AreEqual("2K", GenerateImageLayersTool.ParseSeedreamSize(new JValue("2K")));
            // 空 / auto / 非法值 → 空串（跟随输入图）
            Assert.AreEqual("", GenerateImageLayersTool.ParseSeedreamSize(null));
            Assert.AreEqual("", GenerateImageLayersTool.ParseSeedreamSize(new JValue("auto")));
            Assert.AreEqual("", GenerateImageLayersTool.ParseSeedreamSize(new JValue("AUTO")));
            Assert.AreEqual("", GenerateImageLayersTool.ParseSeedreamSize(new JValue("4K")));
            Assert.AreEqual("", GenerateImageLayersTool.ParseSeedreamSize(new JValue("1024x1024")));
        }

        [Test]
        public void IsLayeringGenerator_MatchesBothLayeringGenerators()
        {
            Assert.IsTrue(TJGeneratorsImageWindow.IsLayeringGenerator("image-layering"));
            Assert.IsTrue(TJGeneratorsImageWindow.IsLayeringGenerator("seedream-image-layering"));
            Assert.IsFalse(TJGeneratorsImageWindow.IsLayeringGenerator("huoshan_seedream_image"));
            Assert.IsFalse(TJGeneratorsImageWindow.IsLayeringGenerator(null));
            Assert.IsFalse(TJGeneratorsImageWindow.IsLayeringGenerator(""));
        }

        [Test]
        public void IsSeedreamLayeringGenerator_OnlySeedreamId()
        {
            Assert.IsTrue(TJGeneratorsImageWindow.IsSeedreamLayeringGenerator("seedream-image-layering"));
            Assert.IsFalse(TJGeneratorsImageWindow.IsSeedreamLayeringGenerator("image-layering"));
            Assert.IsFalse(TJGeneratorsImageWindow.IsSeedreamLayeringGenerator(null));
        }

        [Test]
        public void CollectLayerPaths_Seedream17Cap_CollectsAllExistingSiblings()
        {
            EnsureTestFolder();

            // Seedream 自动分层最多 17 张（底图 + 16 层）；expectedCount 传 17，
            // CollectIndexedSiblingPaths 遇缺口自动停止，不会凭空造出多余条目
            string layer0 = TestFolder + "/ImageLayers_seedream_17.png";
            WriteTinyPng(layer0);
            for (int i = 1; i <= 16; i++)
                WriteTinyPng($"{TestFolder}/ImageLayers_seedream_17_{i}.png");

            List<string> found = GenerateImageLayersTool.CollectLayerPaths(
                layer0, GenerateImageLayersTool.SeedreamMaxLayerCount);
            Assert.AreEqual(17, found.Count);
            Assert.AreEqual(layer0, found[0]);
        }

        static void EnsureTestFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/TJGenerators"))
                AssetDatabase.CreateFolder("Assets", "TJGenerators");
            if (!AssetDatabase.IsValidFolder("Assets/TJGenerators/History"))
                AssetDatabase.CreateFolder("Assets/TJGenerators", "History");
            if (!AssetDatabase.IsValidFolder(TestFolder))
                AssetDatabase.CreateFolder("Assets/TJGenerators/History", "__image_layers_unit_test__");
        }

        static void WriteTinyPng(string assetPath)
        {
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, Color.magenta);
            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            Object.DestroyImmediate(tex);

            string abs = PathUtils.ToAbsoluteAssetPath(assetPath);
            Directory.CreateDirectory(Path.GetDirectoryName(abs));
            File.WriteAllBytes(abs, bytes);
            AssetDatabase.ImportAsset(assetPath);
        }
    }
}
