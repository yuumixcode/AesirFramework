using System;
using NUnit.Framework;
using TJGenerators.Utils;

namespace TJGenerators.Editor.Tests
{
    public class PathUtilsRemoteUrlTests
    {
        [Test]
        public void NormalizeRemoteUrl_ReplacesBackslashes()
        {
            const string input = "https://cdn.example.com/uploads\\user\\image.png";
            Assert.AreEqual(
                "https://cdn.example.com/uploads/user/image.png",
                PathUtils.NormalizeRemoteUrl(input));
        }

        [Test]
        public void NormalizeRemoteUrl_LeavesForwardSlashesUnchanged()
        {
            const string input = "https://cdn.example.com/uploads/user/image.png";
            Assert.AreEqual(input, PathUtils.NormalizeRemoteUrl(input));
        }

        [Test]
        public void NormalizeRemoteUrl_HandlesNullAndEmpty()
        {
            Assert.IsNull(PathUtils.NormalizeRemoteUrl(null));
            Assert.AreEqual("", PathUtils.NormalizeRemoteUrl(""));
        }

        [Test]
        public void NormalizeUrlArray_NormalizesEachElement()
        {
            var input = new[]
            {
                "https://cdn.example.com/a\\b.png",
                "https://cdn.example.com/c/d.png",
            };
            var expected = new[]
            {
                "https://cdn.example.com/a/b.png",
                "https://cdn.example.com/c/d.png",
            };
            CollectionAssert.AreEqual(expected, PathUtils.NormalizeUrlArray(input));
        }

        [Test]
        public void NormalizeUrlArray_ReturnsNullOrEmptyAsIs()
        {
            Assert.IsNull(PathUtils.NormalizeUrlArray(null));
            CollectionAssert.AreEqual(Array.Empty<string>(), PathUtils.NormalizeUrlArray(Array.Empty<string>()));
        }
    }
}
