using System.Collections;
using Codely.Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityTcp.Editor.Tools;

namespace TJGenerators.Editor.Tests
{
    /// <summary>
    /// Manual EditMode samples for the MCP video flow landing (import_video_from_url, non-GUI).
    /// Generation now runs on the backend MCP (generate_video / generate_effect_video -> check_task
    /// -> output.data.result.video_url); this sample covers the Unity-side download + import
    /// (+ ChromaKey post-processing for green-screen effect videos). One import per scenario.
    /// </summary>
    [Explicit("Manual EditMode sample — select and Run in Test Runner after login")]
    public class CustomToolVideoSampleTests : CustomToolSampleTestBase
    {
        // 永久 CDN URL（MCP generate_video -> check_task 实测产物，字段 output.data.result.video_url）
        private const string SampleVideoUrl =
            "https://tuanjie-ai-prd.tos-cn-shanghai.volces.com/aigc/files/2f10d5e848575232d4c9c44742477bcb.mp4";

        private static readonly string[] VideoPathKeys = { "video_path" };
        private static readonly string[] EffectPathKeys = { "video_path", "material_path" };

        [UnityTest]
        [Timeout(600000)]
        public IEnumerator Video_Import_WaitUntilCompleted()
        {
            var submit = ImportMediaFromUrlTool.ImportVideoFromUrl(new JObject
            {
                ["video_url"] = SampleVideoUrl,
                ["prompt"] = "a gentle camera pan over a wooden chair in a sunny room",
                ["output_path"] = SampleAssetPath("Video_Import_WaitDone", ".mp4")
            });

            AssertSubmitOk(submit);
            string taskId = GetTaskId(submit);
            AssertQueryFound(
                ImportMediaFromUrlTool.QueryVideoImportStatus(new JObject { ["task_id"] = taskId }),
                taskId);

            yield return WaitUntilTerminalStatus(
                taskId,
                id => ImportMediaFromUrlTool.QueryVideoImportStatus(new JObject { ["task_id"] = id }),
                VideoPathKeys);
        }

        [UnityTest]
        [Timeout(600000)]
        public IEnumerator EffectVideo_ImportChromaKey_WaitUntilCompleted()
        {
            // 绿幕特效视频链路的机械验证：chroma_key=true 时下载后创建 ChromaKey 材质并布置特效播放器。
            // 开发者应换成 MCP generate_effect_video 产出的绿幕视频 URL（同为 result.video_url）。
            var submit = ImportMediaFromUrlTool.ImportVideoFromUrl(new JObject
            {
                ["video_url"] = SampleVideoUrl,
                ["prompt"] = "small fire explosion (chroma_key sample)",
                ["chroma_key"] = true,
                ["output_path"] = SampleAssetPath("EffectVideo_ImportChromaKey_WaitDone", ".mp4")
            });

            AssertSubmitOk(submit);
            string taskId = GetTaskId(submit);
            AssertQueryFound(
                ImportMediaFromUrlTool.QueryVideoImportStatus(new JObject { ["task_id"] = taskId }),
                taskId);

            yield return WaitUntilTerminalStatus(
                taskId,
                id => ImportMediaFromUrlTool.QueryVideoImportStatus(new JObject { ["task_id"] = id }),
                EffectPathKeys);
        }
    }
}
