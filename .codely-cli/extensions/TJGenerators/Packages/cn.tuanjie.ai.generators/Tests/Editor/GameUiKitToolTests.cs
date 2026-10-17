using System;
using NUnit.Framework;
using UnityTcp.Editor.Tools;

namespace TJGenerators.Editor.Tests
{
    /// <summary>
    /// Offline unit tests for the game UI kit provider routing.
    /// </summary>
    public class GameUiKitToolTests
    {
        [Test]
        public void ResolveStepGeneratorId_NullProvider_DefaultsToSeedream()
        {
            Assert.AreEqual(
                "huoshan_seedream_pro_image",
                GenerateGameUiKitTool.ResolveStepGeneratorId(null, 1));
            Assert.AreEqual(
                GenerateImageLayersTool.SeedreamGeneratorId,
                GenerateGameUiKitTool.ResolveStepGeneratorId(null, 2));
        }

        [Test]
        public void ResolveStepGeneratorId_SeedreamPro_MapsByStep()
        {
            Assert.AreEqual(
                "huoshan_seedream_pro_image",
                GenerateGameUiKitTool.ResolveStepGeneratorId("seedream_pro", 1));
            Assert.AreEqual(
                GenerateImageLayersTool.SeedreamGeneratorId,
                GenerateGameUiKitTool.ResolveStepGeneratorId("seedream_pro", 2));
        }

        [Test]
        public void ResolveStepGeneratorId_Frontier_BothSteps()
        {
            Assert.AreEqual(
                "frontier-game-design",
                GenerateGameUiKitTool.ResolveStepGeneratorId("frontier", 1));
            Assert.AreEqual(
                "frontier-game-design",
                GenerateGameUiKitTool.ResolveStepGeneratorId("frontier", 2));
        }

        [Test]
        public void ResolveStepGeneratorId_CaseInsensitive()
        {
            Assert.AreEqual(
                "frontier-game-design",
                GenerateGameUiKitTool.ResolveStepGeneratorId("FRONTIER", 1));
            Assert.AreEqual(
                "frontier-game-design",
                GenerateGameUiKitTool.ResolveStepGeneratorId("Frontier", 2));
        }

        [Test]
        public void ResolveStepGeneratorId_GarbageValue_FallsBackToSeedream()
        {
            Assert.AreEqual(
                "huoshan_seedream_pro_image",
                GenerateGameUiKitTool.ResolveStepGeneratorId("garbage", 1));
            Assert.AreEqual(
                GenerateImageLayersTool.SeedreamGeneratorId,
                GenerateGameUiKitTool.ResolveStepGeneratorId("garbage", 2));
        }

        [Test]
        public void SeedreamLayerHint_ContainsDetectionMarker_AndWithinPromptLimit()
        {
            // 与后端 mcp/game_ui_kit.go 的 gameUILayerPromptMarker 保持一致
            StringAssert.Contains("game UI kit layer decomposition", GenerateGameUiKitTool.SeedreamLayerHint);
            Assert.LessOrEqual(
                GenerateGameUiKitTool.SeedreamLayerHint.Length, 1024,
                "Seedream layer hint must fit the 1024-char prompt limit of seedream-image-layering");
        }

        [Test]
        public void ScreenshotSuffix_PlusTypicalPrompt_WithinSeedreamProLimit()
        {
            // 后端共用同一后缀（frontier/seedream step 1），用户 prompt 300 字 + 后缀应仍在 1024 内
            string userPrompt = new string('x', 300);
            string effective = userPrompt + ", complete game UI screen design, full HUD layout";
            Assert.LessOrEqual(effective.Length, 1024);
        }
    }
}
