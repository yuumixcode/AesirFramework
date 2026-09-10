using Runestone.AesirModules.ScriptDocGenerator;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    public class GeneratedInternalTypeFilterTests
    {
        [CompilerGenerated]
        class CompilerGeneratedDummyClass
        {
        }

        [Test]
        public void TestCompilerGeneratedAttributeTypeIsFiltered()
        {
            Assert.IsTrue(TypeAnalyzerUtility.IsGeneratedInternalType(typeof(CompilerGeneratedDummyClass)));
        }

        [Test]
        public void TestAngleBracketNameIsFiltered()
        {
            Assert.IsTrue(TypeAnalyzerUtility.IsGeneratedInternalTypeName(
                "<PrivateImplementationDetails>+__StaticArrayInitTypeSize=1373"));
            Assert.IsTrue(TypeAnalyzerUtility.IsGeneratedInternalTypeName("<>c__DisplayClass1"));
        }

        [Test]
        public void TestUnitySourceGeneratedNameIsFiltered()
        {
            Assert.IsTrue(TypeAnalyzerUtility.IsGeneratedInternalTypeName(
                "UnitySourceGeneratedAssemblyMonoScriptTypes_v1+MonoScriptData"));
        }

        [Test]
        public void TestNormalTypeIsNotFiltered()
        {
            Assert.IsFalse(TypeAnalyzerUtility.IsGeneratedInternalType(typeof(GeneratedInternalTypeFilterTests)));
            Assert.IsFalse(TypeAnalyzerUtility.IsGeneratedInternalType(typeof(List<int>)));
            Assert.IsFalse(TypeAnalyzerUtility.IsGeneratedInternalTypeName(
                "Runestone.AesirModules.Tests.Editor.ScriptDocGenerator.GeneratedInternalTypeFilterTests"));
        }

        [Test]
        public void TestOpenGenericAndNullTypeIsNotFiltered()
        {
            // 未闭合泛型（如 List<>）的 FullName 为 null，不应被误判为内部类型
            Assert.IsFalse(TypeAnalyzerUtility.IsGeneratedInternalType(typeof(List<>)));
            Assert.IsFalse(TypeAnalyzerUtility.IsGeneratedInternalType(null));
        }
    }
}
