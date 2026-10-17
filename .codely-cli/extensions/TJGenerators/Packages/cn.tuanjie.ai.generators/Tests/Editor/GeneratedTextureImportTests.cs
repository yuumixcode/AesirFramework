using System;
using System.IO;
using NUnit.Framework;
using TJGenerators.Utils;
using UnityEditor;
using UnityEngine;

namespace TJGenerators.Editor.Tests
{
    public class GeneratedTextureImportTests
    {
        [Test]
        public void FolderImport_PreservesSpriteMetadataAndAnimationReferences()
        {
            string folder = "Assets/SpriteMetadataTest_" + Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", Path.GetFileName(folder));
            string path = folder + "/frame.png";
            string clipPath = folder + "/clip.anim";
            var source = new Texture2D(8, 8, TextureFormat.RGBA32, false);
            try
            {
                File.WriteAllBytes(PathUtils.ToAbsoluteAssetPath(path), source.EncodeToPNG());
                AssetDatabase.ImportAsset(path);
                GeneratedTextureImportUtils.ConfigureImportedTexture(path, TextureImporterType.Sprite);
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                Assert.NotNull(sprite);
                var clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, clipPath);
                var binding = TJGenerators.PostProcessing.SpriteSequencePostProcess.CreateSpriteRendererBinding();
                AnimationUtility.SetObjectReferenceCurve(clip, binding,
                    new[] { new ObjectReferenceKeyframe { time = 0, value = sprite } });
                EditorUtility.SetDirty(clip);
                AssetDatabase.SaveAssets();
                string spriteGuid = AssetDatabase.AssetPathToGUID(path);
                string clipGuid = AssetDatabase.AssetPathToGUID(clipPath);

                PathUtils.ImportAssetsUnderFolderAfterDiskWrite(folder);
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

                Assert.IsTrue(File.Exists(PathUtils.ToAbsoluteAssetPath(path) + ".meta"));
                Assert.AreEqual(TextureImporterType.Sprite, ((TextureImporter)AssetImporter.GetAtPath(path)).textureType);
                Assert.AreEqual(spriteGuid, AssetDatabase.AssetPathToGUID(path));
                Assert.AreEqual(clipGuid, AssetDatabase.AssetPathToGUID(clipPath));
                clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                Assert.AreEqual(AssetDatabase.LoadAssetAtPath<Sprite>(path),
                    AnimationUtility.GetObjectReferenceCurve(clip, binding)[0].value);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(source);
                AssetDatabase.DeleteAsset(folder);
            }
        }

        [TestCase(TextureImporterType.Default)]
        [TestCase(TextureImporterType.Sprite)]
        public void Configure_PreservesNonPowerOfTwoSourceDimensions(TextureImporterType type)
        {
            string folder = "Assets/TextureDimensionsTest_" + Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", Path.GetFileName(folder));
            string path = folder + "/layer.png";
            var source = new Texture2D(40, 24, TextureFormat.RGBA32, false);
            try
            {
                File.WriteAllBytes(PathUtils.ToAbsoluteAssetPath(path), source.EncodeToPNG());
                AssetDatabase.ImportAsset(path);
                GeneratedTextureImportUtils.ConfigureImportedTexture(path, type);
                var imported = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                Assert.AreEqual(40, imported.width);
                Assert.AreEqual(24, imported.height);
                Assert.AreEqual(TextureImporterNPOTScale.None, importer.npotScale);
                Assert.AreEqual(type, importer.textureType);
                Assert.AreEqual(TextureFormat.RGBA32, imported.format);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(source);
                AssetDatabase.DeleteAsset(folder);
            }
        }
    }
}
