using System.Collections.Generic;
using Codely.Newtonsoft.Json.Linq;
using NUnit.Framework;
using TJGenerators.Utils;
using UnityTcp.Editor.Tools;

namespace TJGenerators.Editor.Tests
{
    /// <summary>
    /// Offline CustomTool input validation (no network / no credits).
    /// Covers required fields, prompt length caps, and missing query ids across modules.
    /// Video / audio / TTS / voice-clone generation validation moved to the MCP backend;
    /// Unity-side coverage = import_video_from_url / import_audio_from_url parameter checks.
    /// </summary>
    public class CustomToolValidationTests
    {
        #region Prompt required

        [Test]
        public void Import_RejectsMissingModelUrl()
        {
            AssertFailed(
                Import3DModelFromUrlTool.ImportModelFromUrl(new JObject()),
                messageContains: "'model_url' is required");
        }

        [Test]
        public void Import_RejectsUnsupportedUrlExtension()
        {
            AssertFailed(
                Import3DModelFromUrlTool.ImportModelFromUrl(new JObject
                {
                    ["model_url"] = "https://cdn.example.com/model.glb"
                }),
                messageContains: "Unsupported model_url extension");
        }

        [Test]
        public void Import_RejectsExtensionlessUrl()
        {
            AssertFailed(
                Import3DModelFromUrlTool.ImportModelFromUrl(new JObject
                {
                    ["model_url"] = "https://cdn.example.com/model?token=abc"
                }),
                messageContains: "no recognizable extension");
        }

        [Test]
        public void Import_RejectsUnsupportedOutputType()
        {
            AssertFailed(
                Import3DModelFromUrlTool.ImportModelFromUrl(new JObject
                {
                    ["model_url"] = "https://cdn.example.com/model.fbx",
                    ["output_type"] = "foo"
                }),
                messageContains: "Unsupported output_type");
        }

        [Test]
        public void Import_AddMotionRequiresDescription()
        {
            AssertFailed(
                Import3DModelFromUrlTool.ImportModelFromUrl(new JObject
                {
                    ["model_url"] = "https://cdn.example.com/model.fbx",
                    ["add_motion"] = true
                }),
                messageContains: "'motion_description' is required");
        }

        [Test]
        public void Import_AddMotionOnlyAllowedForStatic()
        {
            AssertFailed(
                Import3DModelFromUrlTool.ImportModelFromUrl(new JObject
                {
                    ["model_url"] = "https://cdn.example.com/model.fbx",
                    ["output_type"] = "rigged",
                    ["source_model_path"] = "Assets/Missing.fbx",
                    ["add_motion"] = true,
                    ["motion_description"] = "a walking cycle"
                }),
                messageContains: "add_motion is only supported with output_type=static");
        }

        [Test]
        public void Import_RiggedRequiresSourceModelPath()
        {
            AssertFailed(
                Import3DModelFromUrlTool.ImportModelFromUrl(new JObject
                {
                    ["model_url"] = "https://cdn.example.com/model.fbx",
                    ["output_type"] = "rigged"
                }),
                messageContains: "'source_model_path' is required");

            AssertFailed(
                Import3DModelFromUrlTool.ImportModelFromUrl(new JObject
                {
                    ["model_url"] = "https://cdn.example.com/model.fbx",
                    ["output_type"] = "rigged",
                    ["source_model_path"] = "Assets/TJGenerators/History/__missing_offline_test__.fbx"
                }),
                messageContains: "Source model not found");
        }

        [Test]
        public void Import_MotionRequiresRiggedModelPath()
        {
            AssertFailed(
                Import3DModelFromUrlTool.ImportModelFromUrl(new JObject
                {
                    ["model_url"] = "https://cdn.example.com/model.fbx",
                    ["output_type"] = "motion"
                }),
                messageContains: "'rigged_model_path' is required");

            AssertFailed(
                Import3DModelFromUrlTool.ImportModelFromUrl(new JObject
                {
                    ["model_url"] = "https://cdn.example.com/model.fbx",
                    ["output_type"] = "motion",
                    ["rigged_model_path"] = "Assets/TJGenerators/History/__missing_offline_test__.fbx"
                }),
                messageContains: "Rigged model not found");
        }

        [Test]
        public void Import_RejectsUnknownProvider()
        {
            AssertFailed(
                Import3DModelFromUrlTool.ImportModelFromUrl(new JObject
                {
                    ["model_url"] = "https://cdn.example.com/model.fbx",
                    ["provider"] = "not-a-provider"
                }),
                messageContains: "Unsupported provider");
        }

        [Test]
        public void ImportImage_RejectsMissingUrl()
        {
            AssertFailed(
                ImportImageFromUrlTool.ImportImageFromUrl(new JObject()),
                messageContains: "'image_url' or 'image_urls' is required");
        }

        [Test]
        public void ImportImage_RejectsUnsupportedExtension()
        {
            AssertFailed(
                ImportImageFromUrlTool.ImportImageFromUrl(new JObject
                {
                    ["image_url"] = "https://cdn.example.com/result.webp"
                }),
                messageContains: "Unsupported image URL extension");
        }

        [Test]
        public void ImportImageQuery_RejectsMissingTaskId()
        {
            AssertFailed(
                ImportImageFromUrlTool.QueryImportStatus(new JObject()),
                messageContains: "'task_id' parameter is required");
        }

        [Test]
        public void Sprite_RejectsEmptyPromptAndImage()
        {
            AssertFailed(
                GenerateSpriteTool.GenerateSprite(new JObject()),
                messageContains: "Either 'prompt' or 'image_path' must be provided");
        }

        [Test]
        public void Terrain_RejectsEmptyPromptAndImage()
        {
            AssertFailed(
                GenerateTerrainTool.GenerateTerrain(new JObject()),
                messageContains: "Either 'prompt' or 'image_path' must be provided");
        }

        [Test]
        public void Skybox_RejectsEmptyPromptAndImage()
        {
            AssertFailed(
                GenerateSkyboxTool.GenerateSkybox(new JObject()),
                messageContains: "Either 'prompt' or 'image_path' must be provided");
        }

        [Test]
        public void Material_RejectsEmptyInputs()
        {
            AssertFailed(
                GenerateMaterialTool.GenerateMaterial(new JObject()),
                messageContains: "At least one of 'prompt', 'preset_id', 'pattern_id', or 'image_path'");
        }

        #endregion

        #region Media import (import_video_from_url / import_audio_from_url)

        [Test]
        public void ImportVideo_RejectsMissingUrl()
        {
            AssertFailed(
                ImportMediaFromUrlTool.ImportVideoFromUrl(new JObject()),
                messageContains: "'video_url' is required");
        }

        [Test]
        public void ImportVideo_RejectsUnsupportedExtension()
        {
            AssertFailed(
                ImportMediaFromUrlTool.ImportVideoFromUrl(new JObject
                {
                    ["video_url"] = "https://cdn.example.com/clip.mov"
                }),
                messageContains: "Unsupported video URL extension");
        }

        [Test]
        public void ImportVideoQuery_RejectsMissingTaskId()
        {
            AssertFailed(
                ImportMediaFromUrlTool.QueryVideoImportStatus(new JObject()),
                messageContains: "'task_id' parameter is required");
        }

        [Test]
        public void ImportAudio_RejectsMissingUrl()
        {
            AssertFailed(
                ImportMediaFromUrlTool.ImportAudioFromUrl(new JObject()),
                messageContains: "'audio_url' is required");
        }

        [Test]
        public void ImportAudio_RejectsUnsupportedExtension()
        {
            AssertFailed(
                ImportMediaFromUrlTool.ImportAudioFromUrl(new JObject
                {
                    ["audio_url"] = "https://cdn.example.com/clip.aac"
                }),
                messageContains: "Unsupported audio URL extension");

            AssertFailed(
                ImportMediaFromUrlTool.ImportAudioFromUrl(new JObject
                {
                    ["audio_url"] = "https://cdn.example.com/clip.flac"
                }),
                messageContains: "Unsupported audio URL extension");
        }

        [Test]
        public void ImportAudioQuery_RejectsMissingTaskId()
        {
            AssertFailed(
                ImportMediaFromUrlTool.QueryAudioImportStatus(new JObject()),
                messageContains: "'task_id' parameter is required");
        }

        #endregion

        #region Prompt length

        // Image / video / audio / TTS prompt-length & generator_id validation moved to the
        // MCP backend (generate_image / generate_video / generate_music / generate_tts);
        // Unity-side import tools take no prompt limits or generator ids.

        [Test]
        public void Sprite_RejectsOverlongPrompt_ForFrontierEffect()
        {
            int maxLen = TJGeneratorsPromptLimits.GetMaxLength("frontier-effect");
            Assert.Greater(maxLen, 0);
            AssertFailed(
                GenerateSpriteTool.GenerateSprite(new JObject
                {
                    ["generator_id"] = "frontier-effect",
                    ["prompt"] = new string('a', maxLen + 1)
                }),
                errorCode: "INVALID_PARAMS",
                messageContains: "exceeds the");
        }

        [Test]
        public void Terrain_RejectsOverlongPrompt()
        {
            int maxLen = TJGeneratorsPromptLimits.GetMaxLength("frontier-effect");
            Assert.Greater(maxLen, 0);
            AssertFailed(
                GenerateTerrainTool.GenerateTerrain(
                    new JObject { ["prompt"] = new string('a', maxLen + 1) }),
                errorCode: "INVALID_PARAMS",
                messageContains: "exceeds the");
        }

        [Test]
        public void Sprite_RejectsOverlongPrompt_ForHuoshanSeedream()
        {
            int maxLen = TJGeneratorsPromptLimits.GetMaxLength("huoshan_seedream");
            Assert.AreEqual(1024, maxLen);
            AssertFailed(
                GenerateSpriteTool.GenerateSprite(new JObject
                {
                    ["prompt"] = new string('a', maxLen + 1)
                }),
                errorCode: "INVALID_PARAMS",
                messageContains: "exceeds the");
        }

        [Test]
        public void Material_RejectsOverlongPrompt()
        {
            int maxLen = TJGeneratorsPromptLimits.GetMaxLength("huoshan_seedream_material");
            Assert.AreEqual(1024, maxLen);
            AssertFailed(
                GenerateMaterialTool.GenerateMaterial(new JObject
                {
                    ["prompt"] = new string('a', maxLen + 1)
                }),
                errorCode: "PROMPT_TOO_LONG",
                messageContains: "exceeds the");
        }

        #endregion

        #region Invalid generator / missing query ids

        // Video / audio / TTS generator_id validation moved to the MCP backend (provider/model params).

        [Test]
        public void QueryTools_RejectMissingTaskId()
        {
            AssertFailed(
                GenerateImageTool.QueryImageStatus(new JObject()),
                messageContains: "'task_id' parameter is required");
            AssertFailed(
                GenerateSpriteTool.QuerySpriteStatus(new JObject()),
                messageContains: "'task_id' parameter is required");
            AssertFailed(
                GenerateTerrainTool.QueryTerrainStatus(new JObject()),
                messageContains: "'task_id' parameter is required");
            AssertFailed(
                GenerateSkyboxTool.QuerySkyboxStatus(new JObject()),
                messageContains: "'task_id' parameter is required");
            AssertFailed(
                GenerateMaterialTool.QueryMaterialStatus(new JObject()),
                messageContains: "'task_id' parameter is required");
            AssertFailed(
                Import3DModelFromUrlTool.QueryImportStatus(new JObject()),
                messageContains: "'task_id' parameter is required");
            AssertFailed(
                ImportImageFromUrlTool.QueryImportStatus(new JObject()),
                messageContains: "'task_id' parameter is required");
            AssertFailed(
                ImportMediaFromUrlTool.QueryVideoImportStatus(new JObject()),
                messageContains: "'task_id' parameter is required");
            AssertFailed(
                ImportMediaFromUrlTool.QueryAudioImportStatus(new JObject()),
                messageContains: "'task_id' parameter is required");
        }

        [Test]
        public void ListSessionAssets_RejectsMissingSessionId()
        {
            AssertFailed(
                ListSessionAssetsTool.ListSessionAssets(new JObject()),
                messageContains: "'session_id' parameter is required");
        }

        [Test]
        public void TerrainApply_RejectsMissingHeightmapPath()
        {
            AssertFailed(
                GenerateTerrainTool.ApplyTerrainHeightmap(new JObject()),
                messageContains: "'heightmap_path' parameter is required");
        }

        [Test]
        public void TerrainApply_RejectsMissingHeightmapFile()
        {
            AssertFailed(
                GenerateTerrainTool.ApplyTerrainHeightmap(new JObject
                {
                    ["heightmap_path"] = "Assets/TJGenerators/History/__missing_offline_test__.png"
                }),
                messageContains: "not found");
        }

        [Test]
        public void TerrainApplyQuery_RejectsMissingApplyTaskId()
        {
            AssertFailed(
                GenerateTerrainTool.QueryTerrainApplyStatus(new JObject()),
                messageContains: "'apply_task_id' is required");
        }

        #endregion

        #region PromptLimits table

        [Test]
        public void PromptLimits_MatchKnownGeneratorCaps()
        {
            Assert.AreEqual(4000, TJGeneratorsPromptLimits.GetMaxLength("frontier-game-design"));
            Assert.AreEqual(2000, TJGeneratorsPromptLimits.GetMaxLength("frontier-effect"));
            Assert.AreEqual(500, TJGeneratorsPromptLimits.GetMaxLength("sonilo-sfx"));
            Assert.AreEqual(2000, TJGeneratorsPromptLimits.GetMaxLength("sonilo-music"));
            Assert.AreEqual(10000, TJGeneratorsPromptLimits.GetMaxLength("minimax-tts"));
            Assert.AreEqual(1000, TJGeneratorsPromptLimits.GetMaxLength("tencent-generation"));
            Assert.AreEqual(1000, TJGeneratorsPromptLimits.GetMaxLength("rodin"));
            Assert.AreEqual(1024, TJGeneratorsPromptLimits.GetMaxLength("tripo-p1"));
            Assert.AreEqual(1024, TJGeneratorsPromptLimits.GetMaxLength("huoshan_seedream"));
            Assert.AreEqual(1024, TJGeneratorsPromptLimits.GetMaxLength("huoshan_seedream_image"));
            Assert.AreEqual(1024, TJGeneratorsPromptLimits.GetMaxLength("huoshan_seedream_pro"));
            Assert.AreEqual(1024, TJGeneratorsPromptLimits.GetMaxLength("huoshan_seedream_pro_image"));
            Assert.AreEqual(1024, TJGeneratorsPromptLimits.GetMaxLength("huoshan_seedream_material"));
            Assert.AreEqual(1024, TJGeneratorsPromptLimits.GetMaxLength("image-layering"));
            Assert.AreEqual(0, TJGeneratorsPromptLimits.GetMaxLength("unknown-generator"));
            Assert.AreEqual(0, TJGeneratorsPromptLimits.GetMaxLength(null));
        }

        #endregion

        static void AssertFailed(
            object result,
            string messageContains = null,
            string errorCode = null)
        {
            Assert.IsInstanceOf<Dictionary<string, object>>(result);
            var dict = (Dictionary<string, object>)result;
            Assert.IsTrue(dict.ContainsKey("success"), "result missing success");
            Assert.IsFalse((bool)dict["success"], dict.ContainsKey("message") ? dict["message"]?.ToString() : "expected failure");

            if (messageContains != null)
            {
                Assert.IsTrue(dict.ContainsKey("message"), "result missing message");
                StringAssert.Contains(messageContains, dict["message"]?.ToString() ?? "");
            }

            if (errorCode != null)
            {
                Assert.IsTrue(dict.ContainsKey("error_code"), "result missing error_code");
                Assert.AreEqual(errorCode, dict["error_code"]?.ToString());
            }
        }
    }
}
