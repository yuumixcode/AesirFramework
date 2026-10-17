using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using TJGenerators;
using TJGenerators.Config;
using TJGenerators.Generators;
using TJGenerators.Utils;
using Unity.UniAsset.Manager.Editor.InternalBridge;
using UnityEditor;
using UnityEngine;
using UnityTcp.Editor.Tools;

namespace TJGenerators.Editor.Tests
{
    /// <summary>
    /// Shared helpers for manual CustomTool EditMode samples (non-GUI).
    /// Subclasses should use [Explicit] and call RequireLogin from SetUp.
    /// </summary>
    public abstract class CustomToolSampleTestBase
    {
        /// <summary>
        /// Unity Test Framework default is 180s (<c>UnityWorkItem.k_DefaultTimeout</c>).
        /// Must be written onto UTF's internal <c>UnityTestExecutionContext</c> (not NUnit's context).
        /// </summary>
        public const int SampleUnityTestTimeoutMs = 600000;
        protected const string SamplePrefabRoot = "Assets/TJGenerators/History/TestSamples";
        protected const string DefaultPrompt = "a simple wooden chair";

        protected const float PollIntervalSeconds = 3f;
        protected const float CompletionTimeoutSeconds = 600f;

        private static readonly string UnityTestExecutionContextTypeName =
            "UnityEngine.TestRunner.NUnitExtensions.Runner.UnityTestExecutionContext";

        [SetUp]
        public virtual void SetUp()
        {
            ExtendUnityTestCaseTimeout();
            RequireLogin();
        }

        /// <summary>
        /// UTF times out via <c>UnityTestExecutionContext.TestCaseTimeout</c>. NUnit
        /// <c>TestExecutionContext</c> / <c>[Timeout]</c> do not reliably update that field for UnityTest.
        /// </summary>
        protected static void ExtendUnityTestCaseTimeout()
        {
            Type type = null;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(UnityTestExecutionContextTypeName, throwOnError: false);
                if (type != null)
                    break;
            }

            if (type == null)
                return;

            object current = type.GetProperty("CurrentContext", BindingFlags.Public | BindingFlags.Static)
                ?.GetValue(null);
            if (current == null)
                return;

            type.GetProperty("TestCaseTimeout", BindingFlags.Public | BindingFlags.Instance)
                ?.SetValue(current, SampleUnityTestTimeoutMs);
        }

        protected static void RequireLogin()
        {
            string token = UnityConnectSession.instance.GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                Assert.Ignore(
                    "No access token — log in to Codely/Unity Connect, then re-run the selected sample.");
            }
        }

        protected static IEnumerator GenerateAiSingleRef(Action<string> onPath, string prompt = null)
        {
            ExtendUnityTestCaseTimeout();

            string path = null;
            string error = null;
            string usePrompt = string.IsNullOrEmpty(prompt) ? DefaultPrompt : prompt;

            yield return AIReferenceImageGeneration.GenerateSingle(
                usePrompt,
                onSuccess: p => path = p,
                onError: err => error = err);

            Assert.IsNull(error, $"AI single reference generation failed: {error}");
            Assert.IsFalse(string.IsNullOrEmpty(path), "AI single reference path empty");
            Assert.IsTrue(File.Exists(path), $"AI single reference missing: {path}");
            onPath(path);
        }

        protected static IEnumerator GenerateAiMultiViewRefs(Action<string[]> onPaths, string prompt = null)
        {
            ExtendUnityTestCaseTimeout();

            string[] paths = null;
            string error = null;
            string usePrompt = string.IsNullOrEmpty(prompt) ? DefaultPrompt : prompt;

            yield return AIReferenceImageGeneration.GenerateMultiView(
                usePrompt,
                viewCount: 4,
                onSuccess: p => paths = p,
                onError: err => error = err,
                onProgress: (cur, total, label) =>
                    Debug.Log($"[CustomToolSample] AI multiview ref {cur}/{total} ({label})"));

            Assert.IsNull(error, $"AI multiview reference generation failed: {error}");
            Assert.IsNotNull(paths);
            Assert.AreEqual(4, paths.Length);
            for (int i = 0; i < paths.Length; i++)
            {
                Assert.IsFalse(string.IsNullOrEmpty(paths[i]), $"multiview path[{i}] empty");
                Assert.IsTrue(File.Exists(paths[i]), $"multiview path[{i}] missing: {paths[i]}");
            }

            onPaths(paths);
        }

        protected static Dictionary<string, object> AsDict(object result)
        {
            Assert.IsNotNull(result, "CustomTool returned null");
            Assert.IsInstanceOf<Dictionary<string, object>>(result);
            return (Dictionary<string, object>)result;
        }

        protected static void AssertSubmitOk(object result, string taskIdKey = "task_id")
        {
            var dict = AsDict(result);
            bool success = dict.TryGetValue("success", out var successObj) && successObj is bool b && b;
            Assert.IsTrue(success, FormatFailure(dict, "submit"));
            Assert.IsFalse(
                string.IsNullOrEmpty(GetTaskId(result, taskIdKey)),
                $"Expected non-empty {taskIdKey} after submit");
        }

        protected static void AssertQueryFound(object result, string expectedTaskId, string taskIdKey = "task_id")
        {
            var dict = AsDict(result);
            bool success = dict.TryGetValue("success", out var successObj) && successObj is bool b && b;
            Assert.IsTrue(success, FormatFailure(dict, "query"));
            Assert.IsTrue(dict.TryGetValue(taskIdKey, out var taskObj), $"Query result missing {taskIdKey}");
            Assert.AreEqual(expectedTaskId, taskObj?.ToString());
        }

        protected static void AssertSubmitFailed(object result)
        {
            var dict = AsDict(result);
            bool success = dict.TryGetValue("success", out var successObj) && successObj is bool b && b;
            Assert.IsFalse(success, "Expected submit to fail");
        }

        protected static IEnumerator WaitUntilTerminalStatus(
            string taskId,
            Func<string, object> queryByTaskId,
            string[] completionPathKeys,
            float timeoutSeconds = CompletionTimeoutSeconds)
        {
            ExtendUnityTestCaseTimeout();

            if (completionPathKeys == null || completionPathKeys.Length == 0)
                throw new ArgumentException("completionPathKeys must not be empty", nameof(completionPathKeys));

            double deadline = EditorApplication.timeSinceStartup + timeoutSeconds;
            string lastStatus = "";
            int lastProgress = -1;

            while (EditorApplication.timeSinceStartup < deadline)
            {
                var dict = AsDict(queryByTaskId(taskId));
                Assert.IsTrue(
                    dict.TryGetValue("success", out var successObj) && successObj is bool ok && ok,
                    FormatFailure(dict, "query"));

                string status = dict.TryGetValue("status", out var statusObj)
                    ? statusObj?.ToString() ?? ""
                    : "";
                int progress = 0;
                if (dict.TryGetValue("progress", out var progressObj) && progressObj != null)
                    int.TryParse(progressObj.ToString(), out progress);

                if (status != lastStatus || progress != lastProgress)
                {
                    Debug.Log($"[CustomToolSample] task={taskId} status={status} progress={progress}%");
                    lastStatus = status;
                    lastProgress = progress;
                }

                if (status == "completed")
                {
                    string foundKey = null;
                    string foundPath = null;
                    foreach (var key in completionPathKeys)
                    {
                        if (dict.TryGetValue(key, out var pathObj) &&
                            !string.IsNullOrEmpty(pathObj?.ToString()))
                        {
                            foundKey = key;
                            foundPath = pathObj.ToString();
                            break;
                        }
                    }

                    Assert.IsFalse(
                        string.IsNullOrEmpty(foundPath),
                        $"completed but none of [{string.Join(", ", completionPathKeys)}] is set");
                    Assert.IsTrue(
                        PathExistsOrIsAsset(foundPath),
                        $"{foundKey} not found: {foundPath}");
                    yield break;
                }

                if (status == "failed" || status == "interrupted" ||
                    status == "rigging_complete_motion_failed" ||
                    (status.Length > 0 && status.IndexOf("fail", StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    string error = dict.TryGetValue("error", out var errObj) ? errObj?.ToString() : null;
                    Assert.Fail($"Generation ended with status={status}. error={error ?? "(none)"}");
                }

                double waitUntil = EditorApplication.timeSinceStartup + PollIntervalSeconds;
                while (EditorApplication.timeSinceStartup < waitUntil)
                    yield return null;
            }

            Assert.Fail(
                $"Timed out after {timeoutSeconds}s waiting for task {taskId}. last_status={lastStatus}");
        }

        protected static string GetTaskId(object result, string taskIdKey = "task_id")
        {
            var dict = AsDict(result);
            return dict.TryGetValue(taskIdKey, out var taskObj) ? taskObj?.ToString() : null;
        }

        protected static string FormatFailure(Dictionary<string, object> dict, string phase)
        {
            string message = dict.TryGetValue("message", out var msg) ? msg?.ToString() : null;
            string errorCode = dict.TryGetValue("error_code", out var code) ? code?.ToString() : null;
            if (!string.IsNullOrEmpty(errorCode))
                return $"{phase} failed [{errorCode}]: {message}";
            return $"{phase} failed: {message ?? "(no message)"}";
        }

        protected static string SamplePrefabPath(string name)
        {
            PathUtils.EnsureAssetFolder(SamplePrefabRoot);
            return $"{SamplePrefabRoot}/{name}.prefab";
        }

        protected static string SampleAssetPath(string name, string ext)
        {
            PathUtils.EnsureAssetFolder(SamplePrefabRoot);
            if (string.IsNullOrEmpty(ext))
                ext = ".png";
            if (!ext.StartsWith("."))
                ext = "." + ext;
            return $"{SamplePrefabRoot}/{name}{ext}";
        }

        private static bool PathExistsOrIsAsset(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;
            if (File.Exists(path) || Directory.Exists(path))
                return true;
            return !string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(path));
        }

        /// <summary>
        /// 通过 DynamicGenerator + GenerationService 跑 Editor 窗口内的生成器（无 CustomTool 时使用）。
        /// </summary>
        protected static IEnumerator RunEditorGeneratorWaitUntilCompleted(
            string generatorId,
            string prefabSampleName,
            Action<DynamicGenerator> configureGenerator)
        {
            ExtendUnityTestCaseTimeout();

            var config = ConfigManager.GetGeneratorConfig(ConfigType.Generator, generatorId);
            Assert.IsNotNull(config, $"{generatorId} config missing");

            string prefabPath = SamplePrefabPath(prefabSampleName);
            string createdPrefab = Import3DModelFromUrlTool.CreateBlankPrefab(prefabPath);
            Assert.IsFalse(string.IsNullOrEmpty(createdPrefab), $"Failed to create prefab at {prefabPath}");

            var generator = new DynamicGenerator(config);
            configureGenerator?.Invoke(generator);

            var submitResult = TJGeneratorsGenerationService.SubmitTaskSync(generator);
            Assert.IsTrue(
                submitResult.Success,
                string.IsNullOrEmpty(submitResult.ErrorCode)
                    ? submitResult.Message
                    : $"[{submitResult.ErrorCode}] {submitResult.Message}");

            var handle = TJGeneratorsGenerationService.GenerateFromSubmittedTask(
                generator,
                new TJGeneratorsGenerationContext
                {
                    TargetAsset = TJGeneratorsAssetReference.FromPath(createdPrefab),
                    AutoCreateTargetPrefab = false
                },
                submitResult.BackendTaskId);

            yield return WaitUntilTaskHandleCompleted(handle);
        }

        protected static IEnumerator WaitUntilTaskHandleCompleted(
            TJGeneratorsTaskHandle handle,
            float timeoutSeconds = CompletionTimeoutSeconds)
        {
            ExtendUnityTestCaseTimeout();

            double deadline = EditorApplication.timeSinceStartup + timeoutSeconds;
            string lastStatus = "";
            int lastProgress = -1;

            while (EditorApplication.timeSinceStartup < deadline)
            {
                if (handle.IsCompleted)
                {
                    Assert.IsFalse(string.IsNullOrEmpty(handle.ModelPath), "completed but model path empty");
                    Assert.IsTrue(PathExistsOrIsAsset(handle.ModelPath), $"model path not found: {handle.ModelPath}");
                    yield break;
                }

                if (handle.IsFailed)
                    Assert.Fail($"Generation failed: status={handle.Status} error={handle.ErrorMessage ?? "(none)"}");

                if (handle.Status != lastStatus || handle.Progress != lastProgress)
                {
                    Debug.Log(
                        $"[CustomToolSample] backend={handle.BackendTaskId} status={handle.Status} progress={handle.Progress}%");
                    lastStatus = handle.Status;
                    lastProgress = handle.Progress;
                }

                double waitUntil = EditorApplication.timeSinceStartup + PollIntervalSeconds;
                while (EditorApplication.timeSinceStartup < waitUntil)
                    yield return null;
            }

            Assert.Fail($"Timed out after {timeoutSeconds}s. last_status={lastStatus}");
        }
    }
}
