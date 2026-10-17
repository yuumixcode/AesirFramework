using NUnit.Framework;
using TJGenerators.Utils;

namespace TJGenerators.Editor.Tests
{
    public class GenerationRequestOriginTests
    {
        [Test]
        public void HeaderConstants_AreStable()
        {
            Assert.AreEqual("fromMethod", GenerationRequestOrigin.HeaderName);
            Assert.AreEqual("X-Package-Version", GenerationRequestOrigin.PackageVersionHeaderName);
            Assert.AreEqual("X-Session-Id", GenerationRequestOrigin.SessionIdHeaderName);
            Assert.AreEqual("X-Workspace-Name", GenerationRequestOrigin.WorkspaceNameHeaderName);
            Assert.AreEqual("ui", GenerationRequestOrigin.Ui);
            Assert.AreEqual("agent", GenerationRequestOrigin.Agent);
        }

        [Test]
        public void SetWorkspaceName_CachesNonEmpty_AndIgnoresEmpty()
        {
            // 空值不能清掉已缓存的 workspace 名（旧 CLI 未注入时保留已有值）
            GenerationRequestOrigin.SetWorkspaceName("my-project");
            Assert.AreEqual("my-project", GenerationRequestOrigin.CurrentWorkspaceName);

            GenerationRequestOrigin.SetWorkspaceName("");
            Assert.AreEqual("my-project", GenerationRequestOrigin.CurrentWorkspaceName);

            GenerationRequestOrigin.SetWorkspaceName(null);
            Assert.AreEqual("my-project", GenerationRequestOrigin.CurrentWorkspaceName);

            // 非空值可更新
            GenerationRequestOrigin.SetWorkspaceName("other-project");
            Assert.AreEqual("other-project", GenerationRequestOrigin.CurrentWorkspaceName);
        }

        [Test]
        public void GetPackageVersion_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => GenerationRequestOrigin.GetPackageVersion());
        }
    }
}
