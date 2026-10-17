using System;
using System.Linq;
using System.Reflection;
using Codely.Newtonsoft.Json.Linq;
using NUnit.Framework;
using TJGenerators.Utils;
using UnityEditor;
using UnityTcp.Editor.Tools;

namespace TJGenerators.Editor.Tests
{
    public class AudioImportHistoryTests
    {
        private const string HistoryKey = "TJGeneratorsGenerationHistory";
        private string _folder;
        private string _session;
        private string _historyBefore;
        private bool _hadHistory;
        private string _taskId;

        [SetUp]
        public void SetUp()
        {
            _session = "audio-history-test-" + Guid.NewGuid().ToString("N");
            _folder = "Assets/" + _session;
            _hadHistory = EditorPrefs.HasKey(HistoryKey);
            _historyBefore = EditorPrefs.GetString(HistoryKey, "");
        }

        [TearDown]
        public void TearDown()
        {
            if (!string.IsNullOrEmpty(_taskId))
                LocalTaskToolsTests.StoreAction(typeof(AudioImportTaskTracker), "RemoveTask", _taskId);
            _taskId = null;
            if (_hadHistory) EditorPrefs.SetString(HistoryKey, _historyBefore);
            else EditorPrefs.DeleteKey(HistoryKey);
            if (AssetDatabase.IsValidFolder(_folder)) AssetDatabase.DeleteAsset(_folder);
        }

        private string AddHistory(string guid, string prompt)
        {
            return TJGeneratorsHistoryManager.AddGeneratingPlaceholder(
                prompt, null, "import_audio_from_url", true, guid, sessionId: _session);
        }

        private JObject SessionAssets()
        {
            return JObject.FromObject(ListSessionAssetsTool.ListSessionAssets(new JObject { ["session_id"] = _session }));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Completion_BindsRealAssetAndPreservesOtherHistory(bool samePath)
        {
            string placeholder = TJGeneratorsAudioUtils.CreateBlankAudioClip(_folder + "/placeholder.wav");
            string saved = samePath ? placeholder : TJGeneratorsAudioUtils.CreateBlankAudioClip(_folder + "/completed.wav");
            string oldGuid = AssetDatabase.AssetPathToGUID(placeholder);
            string newGuid = AssetDatabase.AssetPathToGUID(saved);
            string historyId = AddHistory(oldGuid, "current task");
            string otherId = AddHistory(oldGuid, "other task sharing target");
            _taskId = "audio-history-completion-" + Guid.NewGuid().ToString("N");
            var task = new AudioImportTaskTracker.AudioImportTaskInfo
            {
                TaskId = _taskId, SessionId = _session, Status = "importing",
                PlaceholderPath = placeholder, OutputPath = saved, HistoryTaskId = historyId,
                StartTime = DateTime.Now, AudioUrl = "https://example.com/audio.wav"
            };
            AudioImportTaskTracker.RegisterTask(task);
            var complete = typeof(ImportMediaFromUrlTool).GetMethod("CompleteAudioTask", BindingFlags.NonPublic | BindingFlags.Static);
            complete.Invoke(null, new object[] { task, saved });
            complete.Invoke(null, new object[] { task, saved });

            Assert.AreEqual("completed", task.Status);
            var history = TJGeneratorsHistoryManager.LoadHistory();
            Assert.AreEqual(newGuid, history.Single(h => h.taskId == historyId).assetGuid);
            Assert.AreEqual(oldGuid, history.Single(h => h.taskId == otherId).assetGuid);
            Assert.IsTrue(history.Single(h => h.taskId == otherId).isGenerating);
            Assert.AreEqual(samePath, AssetDatabase.LoadMainAssetAtPath(placeholder) != null);
            var row = ((JArray)SessionAssets()["assets"]).Single(a => (string)a["task_id"] == historyId);
            Assert.AreEqual(newGuid, (string)row["asset_guid"]);
            Assert.AreEqual(saved, (string)row["asset_path"]);
            Assert.IsTrue((bool)row["file_exists"]);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void List_RecoversDeletedTargetWithoutChangingHistoryOrValidTargets(bool deleteTarget)
        {
            string target = TJGeneratorsAudioUtils.CreateBlankAudioClip(_folder + "/target.wav");
            string saved = TJGeneratorsAudioUtils.CreateBlankAudioClip(_folder + "/result.wav");
            string oldGuid = AssetDatabase.AssetPathToGUID(target);
            string historyId = AddHistory(oldGuid, "legacy record");
            TJGeneratorsHistoryManager.CompletePlaceholder(historyId, saved);
            if (deleteTarget) AssetDatabase.DeleteAsset(target);
            string beforeList = EditorPrefs.GetString(HistoryKey);

            var row = ((JArray)SessionAssets()["assets"]).Single(a => (string)a["task_id"] == historyId);
            string expectedPath = deleteTarget ? saved : target;
            Assert.AreEqual(expectedPath, (string)row["asset_path"]);
            Assert.AreEqual(AssetDatabase.AssetPathToGUID(expectedPath), (string)row["asset_guid"]);
            Assert.AreEqual(beforeList, EditorPrefs.GetString(HistoryKey), "Listing must remain read-only.");
        }
    }
}
