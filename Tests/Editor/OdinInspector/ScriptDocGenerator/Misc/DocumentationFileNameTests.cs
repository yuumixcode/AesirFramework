using NUnit.Framework;
using Runestone.AesirModules.ScriptDocGenerator;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    public class DocumentationFileNameTests
    {
        [Test]
        public void TestGenericTypeUsesOfSuffix()
        {
            // "-of-" 形式同时满足 Windows 文件名、Markdown 链接与 IRI/URL 三处合法性（见被测方法注释）
            Assert.AreEqual("TypeData-of-T", TypeAnalyzerUtility.ConvertToDocumentationFileName("TypeData<T>"));
        }

        [Test]
        public void TestMultipleGenericParametersJoinedByHyphen()
        {
            Assert.AreEqual("Dictionary-of-Key-Value",
                TypeAnalyzerUtility.ConvertToDocumentationFileName("Dictionary<Key,Value>"));
            // 反射类型名参数间带空格，同样要归一化（空格在 IRI 中非法）
            Assert.AreEqual("Dictionary-of-Key-Value",
                TypeAnalyzerUtility.ConvertToDocumentationFileName("Dictionary<Key, Value>"));
        }

        [Test]
        public void TestNestedTypeSuffixesEachGenericSegment()
        {
            Assert.AreEqual("Dictionary-of-Key.Enumerator-of-Key",
                TypeAnalyzerUtility.ConvertToDocumentationFileName("Dictionary<Key>.Enumerator<Key>"));
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
