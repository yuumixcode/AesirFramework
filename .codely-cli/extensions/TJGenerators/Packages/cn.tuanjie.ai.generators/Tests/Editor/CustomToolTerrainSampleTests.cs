using System.Collections;
using Codely.Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityTcp.Editor.Tools;

namespace TJGenerators.Editor.Tests
{
    /// <summary>
    /// Manual EditMode samples for Terrain window via CustomTool (non-GUI).
    /// Single chained case: generate → query → wait → apply → wait (one heightmap credit).
    /// </summary>
    [Explicit("Manual EditMode sample — select and Run in Test Runner after login")]
    public class CustomToolTerrainSampleTests : CustomToolSampleTestBase
    {
        private const string TerrainPrompt = "rolling hills with a river valley";
        private static readonly string[] HeightmapPathKeys = { "heightmap_path" };
        private static readonly string[] TerrainDataPathKeys = { "terrain_data_path" };

        [UnityTest]
        [Timeout(600000)]
        public IEnumerator Terrain_GenerateThenApply_WaitUntilCompleted()
        {
            var submit = GenerateTerrainTool.GenerateTerrain(new JObject
            {
                ["prompt"] = TerrainPrompt,
                ["resolution"] = "1K",
                ["output_path"] = SampleAssetPath("Terrain_GenerateThenApply", ".png")
            });

            AssertSubmitOk(submit);
            string genTaskId = GetTaskId(submit);
            AssertQueryFound(
                GenerateTerrainTool.QueryTerrainStatus(new JObject { ["task_id"] = genTaskId }),
                genTaskId);

            yield return WaitUntilTerminalStatus(
                genTaskId,
                id => GenerateTerrainTool.QueryTerrainStatus(new JObject { ["task_id"] = id }),
                HeightmapPathKeys);

            var query = AsDict(GenerateTerrainTool.QueryTerrainStatus(new JObject { ["task_id"] = genTaskId }));
            Assert.IsTrue(query.TryGetValue("heightmap_path", out var hmObj) && hmObj != null);
            string heightmapPath = hmObj.ToString();

            var apply = GenerateTerrainTool.ApplyTerrainHeightmap(new JObject
            {
                ["heightmap_path"] = heightmapPath,
                ["task_id"] = genTaskId,
                ["use_default_options"] = true,
                ["terrain_go_name"] = "TJGenerators Sample Terrain"
            });

            AssertSubmitOk(apply, "apply_task_id");
            string applyTaskId = GetTaskId(apply, "apply_task_id");

            yield return WaitUntilTerminalStatus(
                applyTaskId,
                id => GenerateTerrainTool.QueryTerrainApplyStatus(new JObject { ["apply_task_id"] = id }),
                TerrainDataPathKeys);
        }
    }
}
