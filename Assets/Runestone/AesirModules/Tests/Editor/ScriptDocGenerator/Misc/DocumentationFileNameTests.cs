using Runestone.AesirModules.ScriptDocGenerator;
using NUnit.Framework;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    public class DocumentationFileNameTests
    {
        [Test]
        public void TestGenericTypeUsesCurlyBraces()
        {
            // 花括号对应 C# XML 文档注释 cref 规范（List{T}），Windows 文件名与 Markdown 链接均安全
            Assert.AreEqual("TypeData{T}", TypeAnalyzerUtility.ConvertToDocumentationFileName("TypeData<T>"));
        }

        [Test]
        public void TestMultipleGenericParametersKeepNames()
        {
            Assert.AreEqual("Dictionary{Key,Value}",
                TypeAnalyzerUtility.ConvertToDocumentationFileName("Dictionary<Key,Value>"));
        }

        [Test]
        public void TestNonGenericTypeIsUnchanged()
        {
            Assert.AreEqual("TypeAnalyzerUtility",
                TypeAnalyzerUtility.ConvertToDocumentationFileName("TypeAnalyzerUtility"));
        }

        [Test]
        public void TestNullNameReturnsNull()
        {
            Assert.IsNull(TypeAnalyzerUtility.ConvertToDocumentationFileName(null));
        }
    }
}
