using System;
using NUnit.Framework;
using TJGenerators.Utils;

namespace TJGenerators.Editor.Tests
{
    public class PathUtilsTests
    {
        // -----------------------------------------------------------------------
        // SanitizeAssetFolderName
        // -----------------------------------------------------------------------

        [Test]
        public void SanitizeAssetFolderName_NullOrWhitespace_ReturnsModel()
        {
            Assert.AreEqual("Model", PathUtils.SanitizeAssetFolderName(null));
            Assert.AreEqual("Model", PathUtils.SanitizeAssetFolderName(""));
            Assert.AreEqual("Model", PathUtils.SanitizeAssetFolderName("   "));
        }

        [Test]
        public void SanitizeAssetFolderName_ValidName_Unchanged()
        {
            Assert.AreEqual("MyCharacter", PathUtils.SanitizeAssetFolderName("MyCharacter"));
            Assert.AreEqual("New Mesh 3D", PathUtils.SanitizeAssetFolderName("New Mesh 3D"));
        }

        [Test]
        public void SanitizeAssetFolderName_InvalidChars_ReplacedWithUnderscore()
        {
            // Must strip Windows-invalid chars even on Linux (Path.GetInvalidFileNameChars is OS-specific)
            string result = PathUtils.SanitizeAssetFolderName("Name/With:Invalid|Chars");
            Assert.AreEqual("Name_With_Invalid_Chars", result);
        }

        [Test]
        public void SanitizeAssetFolderName_LeadingAndTrailingSpaces_Trimmed()
        {
            Assert.AreEqual("trimmed", PathUtils.SanitizeAssetFolderName("  trimmed  "));
        }

        // -----------------------------------------------------------------------
        // NormalizeModelDirectory
        // -----------------------------------------------------------------------

        [Test]
        public void NormalizeModelDirectory_Null_ReturnsEmptyString()
        {
            Assert.AreEqual("", PathUtils.NormalizeModelDirectory(null));
        }

        [Test]
        public void NormalizeModelDirectory_EmptyString_ReturnsEmptyString()
        {
            Assert.AreEqual("", PathUtils.NormalizeModelDirectory(""));
        }

        [Test]
        public void NormalizeModelDirectory_NormalPath_Unchanged()
        {
            Assert.AreEqual("Assets/Models/Foo", PathUtils.NormalizeModelDirectory("Assets/Models/Foo"));
        }

        [Test]
        public void NormalizeModelDirectory_DuplicateAssetsPrefix_StrippedOnce()
        {
            Assert.AreEqual("Assets/Models/Foo", PathUtils.NormalizeModelDirectory("Assets/Assets/Models/Foo"));
        }

        [Test]
        public void NormalizeModelDirectory_TripleDuplicatePrefix_OnlyOneLayerStripped()
        {
            // Only the outermost duplicate is removed, leaving one
            Assert.AreEqual("Assets/Assets/Models", PathUtils.NormalizeModelDirectory("Assets/Assets/Assets/Models"));
        }

        [Test]
        public void NormalizeModelDirectory_Backslashes_Normalized()
        {
            Assert.AreEqual("Assets/Models/Foo", PathUtils.NormalizeModelDirectory("Assets\\Models\\Foo"));
        }

        // -----------------------------------------------------------------------
        // IsModelAssetPath
        // -----------------------------------------------------------------------

        [Test]
        public void IsModelAssetPath_NullOrEmpty_ReturnsFalse()
        {
            Assert.IsFalse(PathUtils.IsModelAssetPath(null));
            Assert.IsFalse(PathUtils.IsModelAssetPath(""));
        }

        [Test]
        public void IsModelAssetPath_FbxAndObj_ReturnsTrue()
        {
            Assert.IsTrue(PathUtils.IsModelAssetPath("Assets/Models/hero.fbx"));
            Assert.IsTrue(PathUtils.IsModelAssetPath("Assets/Models/hero.obj"));
        }

        [Test]
        public void IsModelAssetPath_CaseInsensitive()
        {
            Assert.IsTrue(PathUtils.IsModelAssetPath("Assets/Models/hero.FBX"));
            Assert.IsTrue(PathUtils.IsModelAssetPath("Assets/Models/hero.OBJ"));
        }

        [Test]
        public void IsModelAssetPath_UnsupportedExtension_ReturnsFalse()
        {
            Assert.IsFalse(PathUtils.IsModelAssetPath("Assets/Textures/tex.png"));
            Assert.IsFalse(PathUtils.IsModelAssetPath("Assets/Sounds/sfx.wav"));
            Assert.IsFalse(PathUtils.IsModelAssetPath("Assets/Models/hero.fbx.meta"));
        }

        [Test]
        public void IsModelAssetPath_NoExtension_ReturnsFalse()
        {
            Assert.IsFalse(PathUtils.IsModelAssetPath("Assets/Models/hero"));
        }

        // -----------------------------------------------------------------------
        // GetRaw  — 反射点路径取值（无 Unity 运行时依赖）
        // -----------------------------------------------------------------------

        private class FlatDto
        {
            public string title = "hello";
            public int count = 7;
            public string[] tags = { "alpha", "beta", "gamma" };
            public string TitleUpper => title.ToUpperInvariant();
        }

        private class NestedDto
        {
            public FlatDto inner = new FlatDto();
        }

        private class NullInnerDto
        {
            public FlatDto nullInner = null;
        }

        [Test]
        public void GetRaw_NullObjectOrEmptyPath_ReturnsNull()
        {
            Assert.IsNull(PathUtils.GetRaw(null, "title"));
            Assert.IsNull(PathUtils.GetRaw(new FlatDto(), null));
            Assert.IsNull(PathUtils.GetRaw(new FlatDto(), ""));
        }

        [Test]
        public void GetRaw_PublicField_ReturnsValue()
        {
            var dto = new FlatDto();
            Assert.AreEqual("hello", PathUtils.GetRaw(dto, "title"));
            Assert.AreEqual(7, PathUtils.GetRaw(dto, "count"));
        }

        [Test]
        public void GetRaw_PublicProperty_ReturnsValue()
        {
            var dto = new FlatDto();
            Assert.AreEqual("HELLO", PathUtils.GetRaw(dto, "TitleUpper"));
        }

        [Test]
        public void GetRaw_ArrayIndex_ReturnsElement()
        {
            var dto = new FlatDto();
            Assert.AreEqual("alpha", PathUtils.GetRaw(dto, "tags.0"));
            Assert.AreEqual("beta",  PathUtils.GetRaw(dto, "tags.1"));
            Assert.AreEqual("gamma", PathUtils.GetRaw(dto, "tags.2"));
        }

        [Test]
        public void GetRaw_ArrayOutOfRange_ReturnsNull()
        {
            var dto = new FlatDto();
            Assert.IsNull(PathUtils.GetRaw(dto, "tags.99"));
        }

        [Test]
        public void GetRaw_NestedPath_ReturnsDeepValue()
        {
            var dto = new NestedDto();
            Assert.AreEqual("hello", PathUtils.GetRaw(dto, "inner.title"));
            Assert.AreEqual("beta",  PathUtils.GetRaw(dto, "inner.tags.1"));
        }

        [Test]
        public void GetRaw_MissingSegment_ReturnsNull()
        {
            Assert.IsNull(PathUtils.GetRaw(new FlatDto(), "nonexistent"));
            Assert.IsNull(PathUtils.GetRaw(new NestedDto(), "inner.nonexistent"));
        }

        [Test]
        public void GetRaw_NullIntermediateValue_ReturnsNull()
        {
            // Field exists but its value is null — traversal must stop, not throw
            Assert.IsNull(PathUtils.GetRaw(new NullInnerDto(), "nullInner.title"));
        }

        [Test]
        public void GetRaw_NegativeArrayIndex_ReturnsNull()
        {
            // Negative index fails the idx >= 0 guard; should not wrap or throw
            Assert.IsNull(PathUtils.GetRaw(new FlatDto(), "tags.-1"));
        }

        // -----------------------------------------------------------------------
        // GetUrlString
        // -----------------------------------------------------------------------

        private class UrlDto
        {
            public string url = "https://cdn.example.com/a/b.glb";
            public string[] urls = { "https://cdn.example.com/first.glb", "https://cdn.example.com/second.glb" };
            public string[] emptyUrls = Array.Empty<string>();
            public string[] urlsWithNullFirst = { null, "https://cdn.example.com/second.glb" };
            public string backslashUrl = "https://cdn.example.com/a\\b.glb";
        }

        [Test]
        public void GetUrlString_ScalarField_ReturnsFieldValue()
        {
            var dto = new UrlDto();
            Assert.AreEqual("https://cdn.example.com/a/b.glb", PathUtils.GetUrlString(dto, "url"));
        }

        [Test]
        public void GetUrlString_BackslashField_Normalized()
        {
            var dto = new UrlDto();
            Assert.AreEqual("https://cdn.example.com/a/b.glb", PathUtils.GetUrlString(dto, "backslashUrl"));
        }

        [Test]
        public void GetUrlString_ArrayField_ReturnsFirstElement()
        {
            var dto = new UrlDto();
            Assert.AreEqual("https://cdn.example.com/first.glb", PathUtils.GetUrlString(dto, "urls"));
        }

        [Test]
        public void GetUrlString_EmptyArrayField_ReturnsNull()
        {
            Assert.IsNull(PathUtils.GetUrlString(new UrlDto(), "emptyUrls"));
        }

        [Test]
        public void GetUrlString_NullFirstElementInArray_ReturnsNull()
        {
            // arr.GetValue(0)?.ToString() is null; NormalizeRemoteUrl(null) must propagate null
            Assert.IsNull(PathUtils.GetUrlString(new UrlDto(), "urlsWithNullFirst"));
        }

        [Test]
        public void GetUrlString_MissingPath_ReturnsNull()
        {
            Assert.IsNull(PathUtils.GetUrlString(new UrlDto(), "missing"));
            Assert.IsNull(PathUtils.GetUrlString(null, "url"));
        }
    }
}
