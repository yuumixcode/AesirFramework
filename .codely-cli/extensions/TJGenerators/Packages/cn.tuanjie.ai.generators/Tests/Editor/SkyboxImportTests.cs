using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Codely.Newtonsoft.Json.Linq;
using NUnit.Framework;
using TJGenerators;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityTcp.Editor.Tools;

namespace TJGenerators.Editor.Tests
{
    public class SkyboxImportTests
    {
        private string _folder;
        private string _session;
        private readonly List<string> _ids = new List<string>();

        [SetUp]
        public void SetUp()
        {
            _session = "skybox-import-test-" + Guid.NewGuid().ToString("N");
            _folder = "Assets/天空盒导入测试_" + Guid.NewGuid().ToString("N");
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var id in _ids) LocalTaskToolsTests.StoreAction(typeof(ImportImageTaskTracker), "RemoveTask", id);
            _ids.Clear();
            var history = TJGeneratorsHistoryManager.LoadHistory();
            if (history.RemoveAll(h => h.sessionId == _session) > 0) TJGeneratorsHistoryManager.SaveHistory(history);
            if (AssetDatabase.IsValidFolder(_folder)) AssetDatabase.DeleteAsset(_folder);
        }

        private static byte[] Panorama(Color color, int width = 64)
        {
            var texture = new Texture2D(width, width / 2, TextureFormat.RGB24, false);
            try
            {
                var pixels = new Color[width * width / 2];
                for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.Lerp(color, Color.white, (i % width) / (float)width);
                texture.SetPixels(pixels); texture.Apply();
                return texture.EncodeToPNG();
            }
            finally { UnityEngine.Object.DestroyImmediate(texture); }
        }

        private string Start(string url)
        {
            var result = (Dictionary<string, object>)ImportImageFromUrlTool.ImportImageFromUrl(new JObject
            {
                ["image_url"] = url, ["import_type"] = "skybox",
                ["output_path"] = _folder + "/日落.png", ["session_id"] = _session
            });
            Assert.IsTrue((bool)result["success"], result.ContainsKey("message") ? result["message"].ToString() : "");
            Assert.IsTrue((bool)result["submission_success"]);
            var id = (string)result["task_id"]; _ids.Add(id); return id;
        }

        private static IEnumerator Wait(string id)
        {
            double deadline = EditorApplication.timeSinceStartup + 20;
            while (EditorApplication.timeSinceStartup < deadline)
            {
                string status = ImportImageTaskTracker.GetTask(id).Status;
                if (status != "importing" && status != "recovering") yield break;
                yield return null;
            }
            Assert.Fail("Import did not reach a terminal state.");
        }

        [UnityTest]
        public IEnumerator OnlinePanorama_ImportsAsCubemap_ReplacementKeepsGuidsAndMaterialSettings()
        {
            var originalSky = RenderSettings.skybox;
#if UNITY_2021_2_OR_NEWER
            AssetDatabase.CreateFolder("Assets", Path.GetFileName(_folder));
            string unrelatedPath = _folder + "/unrelated.mat";
            var unrelated = new Material(Shader.Find("Skybox/Cubemap"));
            AssetDatabase.CreateAsset(unrelated, unrelatedPath);
            byte[] unrelatedBefore = File.ReadAllBytes(unrelatedPath);
            unrelated.SetColor("_Tint", Color.green);
            EditorUtility.SetDirty(unrelated);
#endif
            string id;
            using (var server = new ImageServer(Panorama(Color.blue)))
            {
                id = Start(server.Url);
                yield return Wait(id);
            }
            var result = (JObject)LocalTaskTools.QueryLocalTask(new JObject { ["task_id"] = id });
            Assert.AreEqual("completed", (string)result["status"], result.ToString());
            string texturePath = (string)result["texture_path"], materialPath = (string)result["material_path"];
            var cube = AssetDatabase.LoadAssetAtPath<Cubemap>(texturePath);
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            Assert.IsNotNull(cube); Assert.Greater(cube.width, 1);
            Assert.AreEqual("Skybox/Cubemap", material.shader.name);
            Assert.AreEqual(cube, material.GetTexture("_Tex"));
#if UNITY_2021_2_OR_NEWER
            CollectionAssert.AreEqual(unrelatedBefore, File.ReadAllBytes(unrelatedPath), "Import must not save unrelated dirty assets.");
            Assert.IsTrue(EditorUtility.IsDirty(unrelated));
#endif
            string textureGuid = AssetDatabase.AssetPathToGUID(texturePath), materialGuid = AssetDatabase.AssetPathToGUID(materialPath);
            material.SetFloat("_Exposure", 2.25f);
            EditorUtility.SetDirty(material);
#if UNITY_2021_2_OR_NEWER
            AssetDatabase.SaveAssetIfDirty(material);
#else
            AssetDatabase.SaveAssets();
#endif

            using (var server = new ImageServer(Panorama(Color.red, 128)))
            {
                id = Start(server.Url);
                Assert.AreEqual(2.25f, material.GetFloat("_Exposure"));
                yield return Wait(id);
            }
            result = (JObject)LocalTaskTools.QueryLocalTask(new JObject { ["task_id"] = id });
            Assert.AreEqual("completed", (string)result["status"], result.ToString());
            Assert.AreEqual(textureGuid, AssetDatabase.AssetPathToGUID(texturePath));
            Assert.AreEqual(materialGuid, AssetDatabase.AssetPathToGUID(materialPath));
            Assert.AreEqual(2.25f, AssetDatabase.LoadAssetAtPath<Material>(materialPath).GetFloat("_Exposure"));
            Assert.AreEqual(originalSky, RenderSettings.skybox, "Importing must not change the user's scene.");
        }

        [UnityTest]
        public IEnumerator InvalidDownload_PreservesExistingSkyboxBytesAndMaterial()
        {
            string id;
            using (var server = new ImageServer(Panorama(Color.blue))) { id = Start(server.Url); yield return Wait(id); }
            Assert.AreEqual("completed", ImportImageTaskTracker.GetTask(id).Status);
            string path = _folder + "/日落.png", matPath = _folder + "/日落_material.mat";
            byte[] before = File.ReadAllBytes(path), materialBefore = File.ReadAllBytes(matPath);
            string guid = AssetDatabase.AssetPathToGUID(path);
            bool ignored = LogAssert.ignoreFailingMessages;
            try
            {
                LogAssert.ignoreFailingMessages = true;
                using (var server = new ImageServer(Encoding.UTF8.GetBytes("not an image"))) { id = Start(server.Url); yield return Wait(id); }
                Assert.AreEqual("failed", ImportImageTaskTracker.GetTask(id).Status);
                CollectionAssert.AreEqual(before, File.ReadAllBytes(path));
                CollectionAssert.AreEqual(materialBefore, File.ReadAllBytes(matPath));
                Assert.AreEqual(guid, AssetDatabase.AssetPathToGUID(path));
            }
            finally { LogAssert.ignoreFailingMessages = ignored; }
        }

        [Test]
        public void SkyboxMode_RejectsMultipleImagesBeforeCreatingAssets()
        {
            var result = (Dictionary<string, object>)ImportImageFromUrlTool.ImportImageFromUrl(new JObject
            {
                ["image_urls"] = "https://example.com/a.png,https://example.com/b.png",
                ["import_type"] = "skybox", ["output_path"] = _folder
            });
            Assert.IsFalse((bool)result["success"]);
            Assert.IsFalse(Directory.Exists(_folder));
        }

        private sealed class ImageServer : IDisposable
        {
            private readonly TcpListener _listener;
            private readonly Task _serve;
            public string Url { get; }
            public ImageServer(byte[] bytes)
            {
                _listener = new TcpListener(IPAddress.Loopback, 0); _listener.Start();
                Url = "http://127.0.0.1:" + ((IPEndPoint)_listener.LocalEndpoint).Port + "/sky.png";
                _serve = Task.Run(async () =>
                {
                    try
                    {
                        using (var client = await _listener.AcceptTcpClientAsync())
                        using (var stream = client.GetStream())
                        {
                            await stream.ReadAsync(new byte[8192], 0, 8192);
                            byte[] header = Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\nContent-Type: image/png\r\nContent-Length: " + bytes.Length + "\r\nConnection: close\r\n\r\n");
                            await stream.WriteAsync(header, 0, header.Length);
                            await stream.WriteAsync(bytes, 0, bytes.Length);
                        }
                    }
                    catch (ObjectDisposedException) { }
                    catch (SocketException) { }
                });
            }
            public void Dispose() { _listener.Stop(); }
        }
    }
}
