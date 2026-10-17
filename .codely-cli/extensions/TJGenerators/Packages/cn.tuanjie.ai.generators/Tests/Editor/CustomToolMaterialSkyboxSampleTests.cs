using System.Collections;
using Codely.Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine.TestTools;
using UnityTcp.Editor.Tools;

namespace TJGenerators.Editor.Tests
{
    /// <summary>
    /// Manual EditMode samples for Material / Skybox windows via CustomTool (non-GUI).
    /// One submit per scenario (query + wait in the same test) to avoid burning credits.
    /// </summary>
    [Explicit("Manual EditMode sample — select and Run in Test Runner after login")]
    public class CustomToolMaterialSkyboxSampleTests : CustomToolSampleTestBase
    {
        private static readonly string[] MaterialPathKeys = { "texture_path", "material_path" };
        private static readonly string[] SkyboxPathKeys = { "texture_path" };

        [UnityTest]
        [Timeout(600000)]
        public IEnumerator Material_Text_WoodPreset_WaitUntilCompleted()
        {
            var submit = GenerateMaterialTool.GenerateMaterial(new JObject
            {
                ["prompt"] = "light oak wood grain",
                ["preset_id"] = "wood",
                ["output_path"] = SampleAssetPath("Material_Wood_WaitDone", ".png")
            });

            AssertSubmitOk(submit);
            string taskId = GetTaskId(submit);
            AssertQueryFound(
                GenerateMaterialTool.QueryMaterialStatus(new JObject { ["task_id"] = taskId }),
                taskId);

            yield return WaitUntilTerminalStatus(
                taskId,
                id => GenerateMaterialTool.QueryMaterialStatus(new JObject { ["task_id"] = id }),
                MaterialPathKeys);
        }

        [UnityTest]
        [Timeout(600000)]
        public IEnumerator Skybox_Text_WaitUntilCompleted()
        {
            var submit = GenerateSkyboxTool.GenerateSkybox(new JObject
            {
                ["prompt"] = "clear blue sky with soft white clouds",
                ["output_path"] = SampleAssetPath("Skybox_Text_WaitDone", ".png")
            });

            AssertSubmitOk(submit);
            string taskId = GetTaskId(submit);
            AssertQueryFound(
                GenerateSkyboxTool.QuerySkyboxStatus(new JObject { ["task_id"] = taskId }),
                taskId);

            yield return WaitUntilTerminalStatus(
                taskId,
                id => GenerateSkyboxTool.QuerySkyboxStatus(new JObject { ["task_id"] = id }),
                SkyboxPathKeys);
        }
    }
}
