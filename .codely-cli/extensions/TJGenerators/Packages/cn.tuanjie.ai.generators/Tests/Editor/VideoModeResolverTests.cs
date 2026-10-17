using NUnit.Framework;
using TJGenerators.Utils;

namespace TJGenerators.Editor.Tests
{
    /// <summary>
    /// Offline VideoModeResolver checks (no network / no credits).
    /// </summary>
    public class VideoModeResolverTests
    {
        [Test]
        public void ReferenceImage_WithoutImage_FallsBackToTextToVideo()
        {
            Assert.AreEqual(
                "text_to_video",
                VideoModeResolver.Resolve("reference_image", hasImage: false));
        }

        [Test]
        public void TextToVideo_WithImage_UpgradesToReferenceImage()
        {
            Assert.AreEqual(
                "reference_image",
                VideoModeResolver.Resolve("text_to_video", hasImage: true));
        }

        [Test]
        public void NullMode_ResolvesByImagePresence()
        {
            Assert.AreEqual("text_to_video", VideoModeResolver.Resolve(null, hasImage: false));
            Assert.AreEqual("reference_image", VideoModeResolver.Resolve(null, hasImage: true));
        }

        [Test]
        public void Multimodal_WithoutVideo_FallsBackByImagePresence()
        {
            Assert.AreEqual(
                "text_to_video",
                VideoModeResolver.Resolve("multimodal", hasImage: false, hasReferenceVideo: false));
            Assert.AreEqual(
                "reference_image",
                VideoModeResolver.Resolve("multimodal", hasImage: true, hasReferenceVideo: false));
        }

        [Test]
        public void Multimodal_WithVideo_StaysMultimodal()
        {
            Assert.AreEqual(
                "multimodal",
                VideoModeResolver.Resolve("multimodal", hasImage: false, hasReferenceVideo: true));
        }

        [Test]
        public void FirstFrame_WithoutImage_RemainsFirstFrame_ForCallerValidation()
        {
            // Resolver does not soft-fallback; window must reject missing images.
            Assert.AreEqual(
                "first_frame",
                VideoModeResolver.Resolve("first_frame", hasImage: false));
        }

        [Test]
        public void FirstLastFrame_WithoutImage_RemainsFirstLastFrame_ForCallerValidation()
        {
            Assert.AreEqual(
                "first_last_frame",
                VideoModeResolver.Resolve("first_last_frame", hasImage: false));
        }
    }
}
