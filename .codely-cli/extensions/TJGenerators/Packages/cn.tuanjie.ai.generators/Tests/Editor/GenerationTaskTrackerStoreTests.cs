using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TJGenerators.Utils;
using UnityEditor;

namespace TJGenerators.Editor.Tests
{
    public class GenerationTaskTrackerStoreTests
    {
        private readonly List<string> _createdTaskIds = new List<string>();
        private GenerationTaskTrackerStore<TestTaskInfo, TestPersistedTask> _store;

        [SetUp]
        public void SetUp()
        {
            _store = new GenerationTaskTrackerStore<TestTaskInfo, TestPersistedTask>(
                "TJGen_StoreTest",
                ToPersisted,
                FromPersisted);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (string taskId in _createdTaskIds)
                _store.RemoveTask(taskId);
            _createdTaskIds.Clear();
        }

        [Test]
        public void CreateRecoveredTask_DeduplicatesByBackendId()
        {
            string backendId = "test_dedupe_" + Guid.NewGuid().ToString("N");
            var first = Track(_store.CreateRecoveredTask(backendId, () => NewRecovered(backendId, "a")));
            var second = _store.CreateRecoveredTask(backendId, () => NewRecovered(backendId, "b"));

            Assert.AreEqual(first.TaskId, second.TaskId);
            Assert.AreEqual(first.BackendTaskId, second.BackendTaskId);
            Assert.AreEqual("a", first.Prompt);
        }

        [Test]
        public void ApplyTaskUpdate_PersistsMutations()
        {
            var task = Track(_store.CreateRecoveredTask(
                "test_apply_" + Guid.NewGuid().ToString("N"),
                () => NewRecovered("test_apply", "p")));

            _store.ApplyTaskUpdate(task, t =>
            {
                t.Status = "completed";
                t.Progress = 55;
                t.PreviewUrl = "https://example.com/preview.png";
                t.EndTime = DateTime.Now;
            });

            RemoveActiveTaskFromMemory(task.TaskId);
            var restored = _store.GetTask(task.TaskId);

            Assert.IsNotNull(restored);
            Assert.AreEqual("completed", restored.Status);
            Assert.AreEqual(55, restored.Progress);
            Assert.AreEqual("https://example.com/preview.png", restored.PreviewUrl);
            Assert.IsTrue(restored.EndTime.HasValue);
        }

        [Test]
        public void GetTask_RestoresFromSession_WhenNotInMemory()
        {
            var task = Track(_store.CreateRecoveredTask(
                "test_restore_" + Guid.NewGuid().ToString("N"),
                () => NewRecovered("test_restore", "hello")));

            RemoveActiveTaskFromMemory(task.TaskId);
            var restored = _store.GetTask(task.TaskId);

            Assert.IsNotNull(restored);
            Assert.AreEqual(task.TaskId, restored.TaskId);
            Assert.AreEqual("hello", restored.Prompt);
        }

        [Test]
        public void GetTask_DoesNotReconcileInterruptedStatusOnRestore()
        {
            var endTime = DateTime.Now.AddMinutes(-10);
            const string errorMessage = "already interrupted";
            var task = Track(_store.CreateRecoveredTask(
                "test_interrupted_" + Guid.NewGuid().ToString("N"),
                () => NewRecovered("test_interrupted", "keep")));
            _store.ApplyTaskUpdate(task, t =>
            {
                t.Status = "interrupted";
                t.ErrorMessage = errorMessage;
                t.EndTime = endTime;
            });

            RemoveActiveTaskFromMemory(task.TaskId);
            var restored = _store.GetTask(task.TaskId);

            Assert.IsNotNull(restored);
            Assert.AreEqual("interrupted", restored.Status);
            Assert.AreEqual(errorMessage, restored.ErrorMessage);
            Assert.AreEqual(endTime, restored.EndTime);
        }

        [Test]
        public void GetTaskByBackendId_FindsTaskRestoredFromSession()
        {
            string backendId = "test_by_backend_" + Guid.NewGuid().ToString("N");
            var task = Track(_store.CreateRecoveredTask(backendId, () => NewRecovered(backendId, "p")));

            RemoveActiveTaskFromMemory(task.TaskId);
            var found = _store.GetTaskByBackendId(backendId);

            Assert.IsNotNull(found);
            Assert.AreEqual(task.TaskId, found.TaskId);
        }

        [Test]
        public void AllocateTaskIdAndRegisterTask_Works()
        {
            string taskId = _store.AllocateTaskId("unit");
            var info = new TestTaskInfo
            {
                TaskId = taskId,
                Status = "generating",
                Progress = 0,
                Prompt = "hi",
                StartTime = DateTime.Now
            };
            _store.RegisterTask(taskId, info);
            Track(info);

            var got = _store.GetTask(taskId);
            Assert.IsNotNull(got);
            Assert.AreEqual("hi", got.Prompt);
            Assert.AreEqual("generating", got.Status);
        }

        [Test]
        public void CleanupCompletedTasks_RemovesExpiredTasksFromMemoryAndSession()
        {
            var expired = Track(_store.CreateRecoveredTask(
                "test_cleanup_expired_" + Guid.NewGuid().ToString("N"),
                () => NewRecovered("expired", "old")));
            _store.ApplyTaskUpdate(expired, t =>
            {
                t.Status = "completed";
                t.EndTime = DateTime.Now.AddHours(-2);
            });

            var recent = Track(_store.CreateRecoveredTask(
                "test_cleanup_recent_" + Guid.NewGuid().ToString("N"),
                () => NewRecovered("recent", "fresh")));
            _store.ApplyTaskUpdate(recent, t =>
            {
                t.Status = "completed";
                t.EndTime = DateTime.Now.AddMinutes(-5);
            });

            var active = Track(_store.CreateRecoveredTask(
                "test_cleanup_active_" + Guid.NewGuid().ToString("N"),
                () => NewRecovered("active", "running")));

            RemoveActiveTaskFromMemory(expired.TaskId);
            RemoveActiveTaskFromMemory(recent.TaskId);
            RemoveActiveTaskFromMemory(active.TaskId);

            _store.CleanupCompletedTasks(retainMinutes: 60);

            var allTasks = _store.GetAllTasks();
            Assert.IsFalse(allTasks.Any(t => t.TaskId == expired.TaskId));
            Assert.IsTrue(allTasks.Any(t => t.TaskId == recent.TaskId));
            Assert.IsTrue(allTasks.Any(t => t.TaskId == active.TaskId));
            Assert.IsNull(_store.GetTask(expired.TaskId));
        }

        [Test]
        public void GetTask_ErasesInvalidSessionPayload_WhenJsonEmptyObject()
        {
            var task = Track(_store.CreateRecoveredTask(
                "test_invalid_json_" + Guid.NewGuid().ToString("N"),
                () => NewRecovered("test_invalid_json", "bad")));
            string taskId = task.TaskId;

            RemoveActiveTaskFromMemory(taskId);
            SessionState.SetString("TJGen_StoreTest_" + taskId, "{}");

            Assert.IsNull(_store.GetTask(taskId));
            Assert.IsNull(_store.GetTask(taskId));
            Assert.IsFalse(_store.GetAllTasks().Any(t => t.TaskId == taskId));
            _createdTaskIds.Remove(taskId);
        }

        [Test]
        public void GetTask_ErasesInvalidSessionPayload_WhenTaskIdMissingInJson()
        {
            var task = Track(_store.CreateRecoveredTask(
                "test_missing_id_" + Guid.NewGuid().ToString("N"),
                () => NewRecovered("test_missing_id", "bad")));
            string taskId = task.TaskId;

            RemoveActiveTaskFromMemory(taskId);
            long endTicks = DateTime.Now.Ticks;
            SessionState.SetString("TJGen_StoreTest_" + taskId,
                "{\"backendTaskId\":\"x\",\"prompt\":\"p\",\"status\":\"completed\",\"progress\":100,\"startTimeTicks\":1,\"endTimeTicks\":" + endTicks + "}");

            var restored = _store.GetTask(taskId);

            Assert.IsNotNull(restored);
            Assert.AreEqual(taskId, restored.TaskId);
            Assert.AreEqual("completed", restored.Status);
            Assert.AreEqual("p", restored.Prompt);
        }

        private TestTaskInfo Track(TestTaskInfo task)
        {
            _createdTaskIds.Add(task.TaskId);
            return task;
        }

        private static TestTaskInfo NewRecovered(string backendId, string prompt)
        {
            return new TestTaskInfo
            {
                TaskId = $"recovered_{backendId}",
                BackendTaskId = backendId,
                Prompt = prompt ?? "",
                Status = "recovering",
                Progress = 0,
                StartTime = DateTime.Now
            };
        }

        private static TestPersistedTask ToPersisted(TestTaskInfo info)
        {
            return new TestPersistedTask
            {
                taskId = info.TaskId,
                backendTaskId = info.BackendTaskId ?? "",
                prompt = info.Prompt ?? "",
                status = info.Status,
                progress = info.Progress,
                errorMessage = info.ErrorMessage ?? "",
                previewUrl = info.PreviewUrl ?? "",
                startTimeTicks = info.StartTime.Ticks,
                endTimeTicks = info.EndTime?.Ticks ?? 0
            };
        }

        private static TestTaskInfo FromPersisted(TestPersistedTask p)
        {
            return new TestTaskInfo
            {
                TaskId = p.taskId,
                BackendTaskId = p.backendTaskId,
                Prompt = p.prompt,
                Status = p.status,
                Progress = p.progress,
                ErrorMessage = p.errorMessage,
                PreviewUrl = p.previewUrl,
                StartTime = new DateTime(p.startTimeTicks),
                EndTime = p.endTimeTicks > 0 ? (DateTime?)new DateTime(p.endTimeTicks) : null
            };
        }

        private void RemoveActiveTaskFromMemory(string taskId)
        {
            Assert.IsTrue(
                _store.RemoveActiveTaskFromMemoryOnly(taskId),
                "Expected task to exist in Store memory before removal");
        }

        [Serializable]
        private class TestPersistedTask
        {
            public string taskId;
            public string backendTaskId;
            public string prompt;
            public string status;
            public int progress;
            public string errorMessage;
            public string previewUrl;
            public long startTimeTicks;
            public long endTimeTicks;
        }

        private class TestTaskInfo : IGenerationTaskInfo
        {
            public string TaskId { get; set; }
            public string Status { get; set; }
            public int Progress { get; set; }
            public string BackendTaskId { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime? EndTime { get; set; }
            public string ErrorMessage { get; set; }
            public string PreviewUrl { get; set; }
            public string Prompt { get; set; }
        }
    }
}
