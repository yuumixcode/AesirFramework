using NUnit.Framework;
using UnityTcp.Editor.Tools;

namespace TJGenerators.Editor.Tests
{
    /// <summary>
    /// Offline CustomTool parameter-mapping checks (no network / no credits).
    /// Video / audio / TTS generation parameter mapping moved to the MCP backend
    /// (generate_video / generate_music / generate_sound_effect / generate_tts);
    /// only the Unity-side import tools' deterministic path helpers remain testable offline.
    /// </summary>
    public class CustomToolParameterMappingTests
    {
        [Test]
        public void Import_ResolvesProviderConfigIds()
        {
            Assert.AreEqual("rodin", Import3DModelFromUrlTool.ResolveProviderConfigId(null));
            Assert.AreEqual("rodin", Import3DModelFromUrlTool.ResolveProviderConfigId(""));
            Assert.AreEqual("rodin", Import3DModelFromUrlTool.ResolveProviderConfigId("rodin"));
            Assert.AreEqual("tripo-p1", Import3DModelFromUrlTool.ResolveProviderConfigId("tripo"));
            Assert.AreEqual("tripo-p1", Import3DModelFromUrlTool.ResolveProviderConfigId("tripo-p1"));
            Assert.AreEqual("tripo-texture-model", Import3DModelFromUrlTool.ResolveProviderConfigId("tripo-texture"));
            Assert.AreEqual("rodin", Import3DModelFromUrlTool.ResolveProviderConfigId("hunyuan"));
            Assert.IsNull(Import3DModelFromUrlTool.ResolveProviderConfigId("not-a-provider"));
        }

        [Test]
        public void Import_UrlHashFileName_IsDeterministic()
        {
            string a = Import3DModelFromUrlTool.BuildUrlHashFileName("https://cdn.example.com/model.fbx", ".fbx");
            string b = Import3DModelFromUrlTool.BuildUrlHashFileName("https://cdn.example.com/model.fbx", ".fbx");
            Assert.AreEqual(a, b);
            StringAssert.EndsWith(".fbx", a);
            Assert.AreEqual(20, a.Length); // 16 个十六进制字符 + ".fbx"

            string c = Import3DModelFromUrlTool.BuildUrlHashFileName("https://cdn.example.com/other.fbx", ".obj");
            Assert.AreNotEqual(a, c);
            StringAssert.EndsWith(".obj", c);
        }

        [Test]
        public void Import_BuildSafeBaseName_MakesCompactSlugFromPrompt()
        {
            Assert.AreEqual("Import3D", Import3DModelFromUrlTool.BuildSafeBaseName(null));
            Assert.AreEqual("Import3D", Import3DModelFromUrlTool.BuildSafeBaseName("   "));
            Assert.AreEqual("Import3D", Import3DModelFromUrlTool.BuildSafeBaseName(",.!?"));

            // 去冠词 + 标点转下划线 + 24 字符词边界截断
            Assert.AreEqual("volleyball_spherical",
                Import3DModelFromUrlTool.BuildSafeBaseName("a volleyball, spherical sports ball, white with blue stripes"));
            Assert.AreEqual("simple_low_poly_wooden",
                Import3DModelFromUrlTool.BuildSafeBaseName("a simple low poly wooden crate, game asset, flat surfaces"));

            // 非法路径字符不残留；超长 prompt 截断到 24 内
            string slashy = Import3DModelFromUrlTool.BuildSafeBaseName("a/b:c*d?e f/g");
            Assert.IsFalse(slashy.Contains("/") || slashy.Contains(":") || slashy.Contains("*"));
            Assert.LessOrEqual(Import3DModelFromUrlTool.BuildSafeBaseName(new string('x', 200)).Length, 24);

            // 中文 prompt 原样保留（≤24 字符）
            Assert.AreEqual("一把木椅子", Import3DModelFromUrlTool.BuildSafeBaseName("一把木椅子"));
        }
    }
}
