using System.Collections;
using Codely.Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityTcp.Editor.Tools;

namespace TJGenerators.Editor.Tests
{
    /// <summary>
    /// Manual EditMode samples for image-family CustomTools (non-GUI).
    /// Image generation now runs on the MCP backend (generate_image -> check_task);
    /// Unity-side coverage = import_image_from_url (single / multi) + generate_sprite (not migrated).
    /// One submit per scenario (query + wait in the same test) to avoid burning credits.
    /// </summary>
    [Explicit("Manual EditMode sample — select and Run in Test Runner after login")]
    public class CustomToolImageSampleTests : CustomToolSampleTestBase
    {
        private const string ImagePrompt = "a cute cartoon fox icon on transparent background";
        private static readonly string[] ImagePathKeys = { "image_path" };
        private static readonly string[] SpritePathKeys = { "sprite_path" };
        private static readonly string[] Layer0Keys = { "layer_0_path" };

        // 永久 CDN URL（file_upload 产物，用于 import_image_from_url 样例）
        private const string SampleCdnUrlA =
            "https://tuanjie-ai-prd.tos-cn-shanghai.volces.com/mcp-upload/a1458a8423827fe0854bf198044b8d15/2025-07-16183254.png";
        private const string SampleCdnUrlB =
            "https://tuanjie-ai-prd.tos-cn-shanghai.volces.com/mcp-upload/beedb586a762783a7eab1e51cebbbe41/2025-07-16183330.png";

        [UnityTest]
        [Timeout(600000)]
        public IEnumerator ImportImage_Single_WaitUntilCompleted()
        {
            var submit = ImportImageFromUrlTool.ImportImageFromUrl(new JObject
            {
                ["image_url"] = SampleCdnUrlA,
                ["prompt"] = "import sample: sci-fi drone cockpit front view",
                ["output_path"] = SampleAssetPath("ImportImage_Single_WaitDone", ".png")
            });

            AssertSubmitOk(submit);
            string taskId = GetTaskId(submit);
            AssertQueryFound(
                ImportImageFromUrlTool.QueryImportStatus(new JObject { ["task_id"] = taskId }),
                taskId);

            yield return WaitUntilTerminalStatus(
                taskId,
                id => ImportImageFromUrlTool.QueryImportStatus(new JObject { ["task_id"] = id }),
                ImagePathKeys);
        }

        [UnityTest]
        [Timeout(600000)]
        public IEnumerator ImportImage_MultiLayers_WaitUntilCompleted()
        {
            var submit = ImportImageFromUrlTool.ImportImageFromUrl(new JObject
            {
                ["image_urls"] = SampleCdnUrlA + "," + SampleCdnUrlB,
                ["names"] = "layer_0,layer_1",
                ["prompt"] = "import sample: two views as pseudo layers",
                ["output_path"] = "Assets/TJGenerators/History/TestImportLayers_Sample"
            });

            AssertSubmitOk(submit);
            string taskId = GetTaskId(submit);
            AssertQueryFound(
                ImportImageFromUrlTool.QueryImportStatus(new JObject { ["task_id"] = taskId }),
                taskId);

            yield return WaitUntilTerminalStatus(
                taskId,
                id => ImportImageFromUrlTool.QueryImportStatus(new JObject { ["task_id"] = id }),
                Layer0Keys);
        }

        [UnityTest]
        [Timeout(600000)]
        public IEnumerator Sprite_Text_WaitUntilCompleted()
        {
            var submit = GenerateSpriteTool.GenerateSprite(new JObject
            {
                ["prompt"] = ImagePrompt,
                ["output_path"] = SampleAssetPath("Sprite_Text_WaitDone", ".png")
            });

            AssertSubmitOk(submit);
            string taskId = GetTaskId(submit);
            AssertQueryFound(
                GenerateSpriteTool.QuerySpriteStatus(new JObject { ["task_id"] = taskId }),
                taskId);

            yield return WaitUntilTerminalStatus(
                taskId,
                id => GenerateSpriteTool.QuerySpriteStatus(new JObject { ["task_id"] = id }),
                SpritePathKeys);
        }
    }
}
