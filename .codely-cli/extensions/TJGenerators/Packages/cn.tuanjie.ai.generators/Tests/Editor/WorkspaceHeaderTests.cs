using System;
using NUnit.Framework;
using TJGenerators.Utils;
using UnityEngine.Networking;

namespace TJGenerators.Editor.Tests
{
    public class WorkspaceHeaderTests
    {
        [TestCase("中文工程 + 50% 🌌")]
        [TestCase("D:\\项目\\天空盒")]
        [TestCase("line\r\nnext")]
        public void UnsafeHeaderCharacters_AreEncodedAndRoundTrip(string name)
        {
            using (var request = new UnityWebRequest("http://127.0.0.1/"))
            {
                Assert.DoesNotThrow(() => GenerationRequestOrigin.ApplyWorkspaceNameHeader(request, name));
                string value = request.GetRequestHeader(GenerationRequestOrigin.WorkspaceNameHeaderName);
                Assert.IsTrue(System.Linq.Enumerable.All(value, c => c >= 32 && c <= 126));
                Assert.AreEqual(name, Uri.UnescapeDataString(value));
                Assert.AreEqual("uri", request.GetRequestHeader(GenerationRequestOrigin.WorkspaceNameEncodingHeaderName));
            }
        }

        [TestCase("game + 50%")]
        [TestCase("literal_%E4%B8%AD")]
        public void ExistingAsciiNames_AreUnchanged(string name)
        {
            using (var request = new UnityWebRequest("http://127.0.0.1/"))
            {
                GenerationRequestOrigin.ApplyWorkspaceNameHeader(request, name);
                Assert.AreEqual(name, request.GetRequestHeader(GenerationRequestOrigin.WorkspaceNameHeaderName));
                Assert.IsNull(request.GetRequestHeader(GenerationRequestOrigin.WorkspaceNameEncodingHeaderName));
            }
        }
    }
}
