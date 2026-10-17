using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Codely.Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityTcp.Editor.Tools;

namespace TJGenerators.Editor.Tests
{
    public class ImageImportPreservationTests
    {
        [UnityTest]
        public IEnumerator FailedDownload_PreservesExistingImageAndGuid()
        {
            string folder = "Assets/TJGeneratorsImportTest_" + Guid.NewGuid().ToString("N");
            string path = folder + "/existing.png";
            bool ignoredLogs = LogAssert.ignoreFailingMessages;
            Directory.CreateDirectory(folder);
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            byte[] original;
            try
            {
                texture.SetPixels(new[] { Color.red, Color.green, Color.blue, Color.white });
                texture.Apply();
                original = texture.EncodeToPNG();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(texture);
            }

            try
            {
                File.WriteAllBytes(path, original);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                string originalGuid = AssetDatabase.AssetPathToGUID(path);
                LogAssert.ignoreFailingMessages = true; // The deliberately unreachable URL must fail.

                var result = (Dictionary<string, object>)ImportImageFromUrlTool.ImportImageFromUrl(
                    new JObject
                    {
                        ["image_url"] = "http://127.0.0.1:1/missing.png",
                        ["output_path"] = path,
                        ["session_id"] = "image-import-preservation-test"
                    });
                Assert.IsTrue((bool)result["success"]);
                Assert.IsTrue((bool)result["submission_success"],
                    "The CLI needs this marker to register task_id -> session_id for completion notifications.");
                string taskId = (string)result["task_id"];
                CollectionAssert.AreEqual(original, File.ReadAllBytes(path),
                    "Submitting a replacement must not replace an existing image with the placeholder.");

                double deadline = EditorApplication.timeSinceStartup + 15;
                var task = ImportImageTaskTracker.GetTask(taskId);
                while ((task.Status == "importing" || task.Status == "recovering") &&
                       EditorApplication.timeSinceStartup < deadline)
                {
                    yield return null;
                    task = ImportImageTaskTracker.GetTask(taskId);
                }

                Assert.AreEqual("failed", task.Status);
                CollectionAssert.AreEqual(original, File.ReadAllBytes(path));
                Assert.AreEqual(originalGuid, AssetDatabase.AssetPathToGUID(path));
                var imported = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                Assert.IsNotNull(imported);
                Assert.AreEqual(2, imported.width);
                Assert.AreEqual(2, imported.height);
            }
            finally
            {
                LogAssert.ignoreFailingMessages = ignoredLogs;
                AssetDatabase.DeleteAsset(folder);
            }
        }
    }
}
