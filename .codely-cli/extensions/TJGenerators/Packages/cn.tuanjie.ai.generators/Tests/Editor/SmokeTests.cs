using NUnit.Framework;
using TJGenerators.Utils;
using UnityEditor.PackageManager;

namespace TJGenerators.Editor.Tests
{
    public class SmokeTests
    {
        [Test]
        public void Package_IsDiscoverable()
        {
            var info = PackageInfo.FindForAssetPath(
                "Packages/cn.tuanjie.ai.generators/package.json");
            Assert.IsNotNull(info, "Expected cn.tuanjie.ai.generators package to be installed");
            Assert.AreEqual("cn.tuanjie.ai.generators", info.name);
            Assert.IsFalse(string.IsNullOrEmpty(info.version));
        }

        [Test]
        public void EditorAssembly_IsLoaded()
        {
            var asm = typeof(GenerationRequestOrigin).Assembly;
            Assert.IsNotNull(asm);
            Assert.IsTrue(asm.GetName().Name.Contains("tjgenerators"), asm.GetName().Name);
        }
    }
}
