using System;
using System.Linq;
using System.Reflection;
using Codely.Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityTcp.Editor.Tools;

namespace TJGenerators.Editor.Tests
{
    public class LocalTaskToolsTests
    {
        private string _id;

        [SetUp]
        public void SetUp()
        {
            _id = "local_query_test_" + Guid.NewGuid().ToString("N");
            ImportImageTaskTracker.RegisterTask(new ImportImageTaskTracker.ImportImageTaskInfo
            {
                TaskId = _id, Status = "completed", StartTime = DateTime.Now,
                ImportType = "skybox", MaterialPath = "Assets/test_material.mat",
                ImageUrls = new System.Collections.Generic.List<string> { "https://example.com/sky.png" },
                ImagePaths = new System.Collections.Generic.List<string> { "Assets/test.png" }
            });
        }

        internal static void StoreAction(Type tracker, string action, string id)
        {
            var store = tracker.GetField("Store", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            store.GetType().GetMethod(action, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Invoke(store, new object[] { id });
        }

        [TearDown]
        public void TearDown()
        {
            StoreAction(typeof(ImportImageTaskTracker), "RemoveTask", _id);
            StoreAction(typeof(AudioImportTaskTracker), "RemoveTask", _id);
        }

        [Test]
        public void Query_RestoresSkyboxImportDetailsFromPersistedTask()
        {
            StoreAction(typeof(ImportImageTaskTracker), "RemoveActiveTaskFromMemoryOnly", _id);
            var result = (JObject)LocalTaskTools.QueryLocalTask(new JObject { ["task_id"] = _id });
            Assert.IsTrue((bool)result["success"]);
            Assert.AreEqual("image_import", (string)result["task_type"]);
            Assert.AreEqual("skybox", (string)result["import_type"]);
            Assert.AreEqual("Assets/test_material.mat", (string)result["material_path"]);
            Assert.AreEqual("Assets/test.png", (string)result["texture_path"]);
        }

        [Test]
        public void Query_AmbiguousRecoveredId_RequiresTypeAndReturnsCorrectStore()
        {
            AudioImportTaskTracker.RegisterTask(new AudioImportTaskTracker.AudioImportTaskInfo
            {
                TaskId = _id, Status = "completed", StartTime = DateTime.Now, AudioPath = "Assets/test.wav"
            });
            var ambiguous = (JObject)LocalTaskTools.QueryLocalTask(new JObject { ["task_id"] = _id });
            Assert.AreEqual("AMBIGUOUS_TASK_ID", (string)ambiguous["error_code"]);
            var audio = (JObject)LocalTaskTools.QueryLocalTask(new JObject { ["task_id"] = _id, ["task_type"] = "audio_import" });
            Assert.AreEqual("Assets/test.wav", (string)audio["audio_path"]);
        }

        [Test]
        public void List_FiltersPaginatesAndKeepsFullResultFields()
        {
            var result = (JObject)LocalTaskTools.ListLocalTasks(new JObject { ["task_type"] = "image_import", ["status"] = "completed", ["limit"] = 1000 });
            var rows = (JArray)result["tasks"];
            Assert.IsTrue(rows.All(t => (string)t["task_type"] == "image_import" && (string)t["status"] == "completed"));
            Assert.AreEqual(1, rows.Count(t => (string)t["task_id"] == _id));
            Assert.AreEqual("Assets/test_material.mat", (string)rows.Single(t => (string)t["task_id"] == _id)["material_path"]);
            var empty = (JObject)LocalTaskTools.ListLocalTasks(new JObject { ["task_type"] = "image_import", ["offset"] = int.MaxValue });
            Assert.AreEqual(0, ((JArray)empty["tasks"]).Count);
            Assert.AreEqual(JTokenType.Null, empty["next_offset"].Type);
        }

        [Test]
        public void MissingRemoteIdAndInvalidFilters_FailWithoutStartingWork()
        {
            var result = (JObject)LocalTaskTools.QueryLocalTask(new JObject { ["task_id"] = "mcp_id_not_in_local_stores" });
            Assert.AreEqual("TASK_NOT_FOUND", (string)result["error_code"]);
            StringAssert.Contains("MCP check_task", (string)result["message"]);
            Assert.IsFalse((bool)((JObject)LocalTaskTools.ListLocalTasks(new JObject { ["task_type"] = "unknown" }))["success"]);
            Assert.IsFalse((bool)((JObject)LocalTaskTools.ListLocalTasks(new JObject { ["limit"] = 0 }))["success"]);
        }

        [Test]
        public void TerrainApply_AcceptsLocalIdAndPreservesStageSpecificFields()
        {
            string id = TerrainApplyTaskTracker.CreateTask("Assets/height.png");
            try
            {
                TerrainApplyTaskTracker.MarkCompleted(id, "Assets/terrain.asset", "ExistingTerrain");
                var result = (JObject)LocalTaskTools.QueryLocalTask(new JObject { ["task_id"] = id });
                Assert.AreEqual(id, (string)result["apply_task_id"]);
                Assert.AreEqual("terrain_apply", (string)result["task_type"]);
                Assert.AreEqual("ExistingTerrain", (string)result["terrain_go_name"]);
            }
            finally
            {
                var tasks = (System.Collections.IDictionary)typeof(TerrainApplyTaskTracker).GetField("_tasks", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
                tasks.Remove(id);
            }
        }
    }
}
