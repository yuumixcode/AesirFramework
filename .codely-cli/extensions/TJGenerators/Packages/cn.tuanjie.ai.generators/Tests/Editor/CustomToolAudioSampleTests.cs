using System.Collections;
using Codely.Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityTcp.Editor.Tools;

namespace TJGenerators.Editor.Tests
{
    /// <summary>
    /// Manual EditMode samples for the MCP audio flow landing (import_audio_from_url, non-GUI).
    /// Generation now runs on the backend MCP (generate_music / generate_sound_effect / generate_tts
    /// -> check_task -> output.data.audioUrl); this sample covers the Unity-side download + import,
    /// AudioSource re-binding and BGMPlayer wiring. One import per scenario.
    /// </summary>
    [Explicit("Manual EditMode sample — select and Run in Test Runner after login")]
    public class CustomToolAudioSampleTests : CustomToolSampleTestBase
    {
        // 永久 CDN URL（MCP generate_music / generate_sound_effect -> check_task 实测产物，字段 output.data.audioUrl）
        private const string SampleMusicWavUrl =
            "https://tuanjie-ai-prd.tos-cn-shanghai.volces.com/aigc/audio/1f3e7a3049c996f012dca3ab67de9c7d.wav";
        private const string SampleSfxMp3Url =
            "https://tuanjie-ai-prd.tos-cn-shanghai.volces.com/aigc/files/1c90bc0ff136f25ee9ab015f58158d60.mp3";

        private static readonly string[] AudioPathKeys = { "audio_path" };

        [UnityTest]
        [Timeout(600000)]
        public IEnumerator Music_ImportBgm_WaitUntilCompleted()
        {
            // is_bgm=true：完成时场景无引用则自动创建/复用 BGMPlayer（loop + 2D + playOnAwake）
            var submit = ImportMediaFromUrlTool.ImportAudioFromUrl(new JObject
            {
                ["audio_url"] = SampleMusicWavUrl,
                ["prompt"] = "calm ambient forest background music",
                ["is_bgm"] = true,
                ["output_path"] = SampleAssetPath("Music_ImportBgm_WaitDone", ".wav")
            });

            AssertSubmitOk(submit);
            string taskId = GetTaskId(submit);
            AssertQueryFound(
                ImportMediaFromUrlTool.QueryAudioImportStatus(new JObject { ["task_id"] = taskId }),
                taskId);

            yield return WaitUntilTerminalStatus(
                taskId,
                id => ImportMediaFromUrlTool.QueryAudioImportStatus(new JObject { ["task_id"] = id }),
                AudioPathKeys);
        }

        [UnityTest]
        [Timeout(600000)]
        public IEnumerator Sfx_ImportMp3_WaitUntilCompleted()
        {
            // .mp3 分支：占位是同名 .wav，真实 mp3 落盘后占位自动删除（query 返回 audio_path 为 .mp3）
            var submit = ImportMediaFromUrlTool.ImportAudioFromUrl(new JObject
            {
                ["audio_url"] = SampleSfxMp3Url,
                ["prompt"] = "short wooden door creak",
                ["output_path"] = SampleAssetPath("Sfx_ImportMp3_WaitDone", ".mp3")
            });

            AssertSubmitOk(submit);
            string taskId = GetTaskId(submit);
            AssertQueryFound(
                ImportMediaFromUrlTool.QueryAudioImportStatus(new JObject { ["task_id"] = taskId }),
                taskId);

            yield return WaitUntilTerminalStatus(
                taskId,
                id => ImportMediaFromUrlTool.QueryAudioImportStatus(new JObject { ["task_id"] = id }),
                AudioPathKeys);
        }
    }
}
