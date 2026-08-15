using Runestone.AesirInspector;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Runestone.AesirInspector.Editor.Tests
{
    public interface ITestInterface { }

    [ReferenceLinkURL("https://learn.microsoft.com/en-us/dotnet/api/system.object?view=net-9.0")]
    public class TestClassWithAttribute { }

    public abstract class TestAbstractClass { }

    public sealed class TestSealedClass { }

    public static class TestStaticClass { }

    public class TestGenericClass<T> where T : class
    {
        public T Owner;
    }

    public delegate void TestDelegate();

    public delegate void TestDelegateHasParameters(int a, List<string> b);

    public delegate bool TestDelegateHasReturnType(float a, int[] b);

    public record TestRecord;

    public struct TestStruct { }

    public class TypeDataTests : TestAbstractClass
    {
        [Test]
        public void TestInterface()
        {
            var typeData = new TypeData(typeof(ITestInterface));
            Debug.Log(typeData.MemberType);
            Assert.IsTrue(typeData.MemberType == MemberTypes.TypeInfo);
            Debug.Log(typeData.FullDeclarationWithAttributes);
            Assert.AreEqual("public interface ITestInterface", typeData.FullDeclarationWithAttributes);
        }

        [Test]
        public void TestAbstractClass()
        {
            var typeData = new TypeData(typeof(TestAbstractClass));
            Debug.Log(typeData.MemberType);
            Assert.IsTrue(typeData.MemberType == MemberTypes.TypeInfo);
            Debug.Log(typeData.FullDeclarationWithAttributes);
            Assert.AreEqual("public abstract class TestAbstractClass",
                typeData.FullDeclarationWithAttributes);
        }

        [Test]
        public void TestSealedClass()
        {
            var typeData = new TypeData(typeof(TestSealedClass));
            Debug.Log(typeData.MemberType);
            Assert.IsTrue(typeData.MemberType == MemberTypes.TypeInfo);
            Debug.Log(typeData.FullDeclarationWithAttributes);
            Assert.AreEqual("public sealed class TestSealedClass", typeData.FullDeclarationWithAttributes);
        }

        [Test]
        public void TestStaticClass()
        {
            var typeData = new TypeData(typeof(TestStaticClass));
            Debug.Log(typeData.MemberType);
            Assert.IsTrue(typeData.MemberType == MemberTypes.TypeInfo);
            Debug.Log(typeData.FullDeclarationWithAttributes);
            Assert.AreEqual("public static class TestStaticClass", typeData.FullDeclarationWithAttributes);
        }

        [Test]
        public void TestTypeDataTests()
        {
            var typeData = new TypeData(typeof(TypeDataTests));
            Debug.Log(typeData.MemberType);
            Debug.Log(typeData.FullDeclarationWithAttributes);
            Assert.IsTrue(typeData.MemberType == MemberTypes.TypeInfo &&
                          "public class TypeDataTests : Runestone.AesirInspector.Editor.Tests.TestAbstractClass" ==
                          typeData.FullDeclarationWithAttributes);
        }

        [Test]
        public void TestGenericClass()
        {
            var typeData = new TypeData(typeof(TestGenericClass<>));
            Debug.Log(typeData.MemberType);
            Assert.IsTrue(typeData.MemberType == MemberTypes.TypeInfo);
            Debug.Log(typeData.FullDeclarationWithAttributes);
            Assert.AreEqual("public class TestGenericClass<T> where T : class",
                typeData.FullDeclarationWithAttributes);
        }

        [Test]
        public void TestClassWithAttribute()
        {
            var typeData = new TypeData(typeof(TestClassWithAttribute));
            Debug.Log(typeData.MemberType);
            Assert.IsTrue(typeData.MemberType == MemberTypes.TypeInfo);
            Debug.Log(typeData.FullDeclarationWithAttributes);
            Assert.AreEqual(
                @"[ReferenceLinkURL(""https://learn.microsoft.com/en-us/dotnet/api/system.object?view=net-9.0"")]
public class TestClassWithAttribute", typeData.FullDeclarationWithAttributes);
        }

        [Test]
        public void TestDelegate()
        {
            var typeData = new TypeData(typeof(TestDelegate));
            Debug.Log(typeData.MemberType);
            Assert.IsTrue(typeData.MemberType == MemberTypes.TypeInfo);
            Debug.Log(typeData.FullDeclarationWithAttributes);
            Assert.AreEqual("public delegate void TestDelegate()", typeData.FullDeclarationWithAttributes);
        }

        [Test]
        public void TestDelegateHasParameters()
        {
            var typeData = new TypeData(typeof(TestDelegateHasParameters));
            Debug.Log(typeData.MemberType);
            Assert.IsTrue(typeData.MemberType == MemberTypes.TypeInfo);
            Debug.Log(typeData.FullDeclarationWithAttributes);
            Assert.AreEqual("public delegate void TestDelegateHasParameters(int a, List<string> b)",
                typeData.FullDeclarationWithAttributes);
        }

        [Test]
        public void TestDelegateHasReturnType()
        {
            var typeData = new TypeData(typeof(TestDelegateHasReturnType));
            Debug.Log(typeData.MemberType);
            Assert.IsTrue(typeData.MemberType == MemberTypes.TypeInfo);
            Debug.Log(typeData.FullDeclarationWithAttributes);
            Assert.AreEqual("public delegate bool TestDelegateHasReturnType(float a, int[] b)",
                typeData.FullDeclarationWithAttributes);
        }

        [Test]
        public void TestRecord()
        {
            var typeData = new TypeData(typeof(TestRecord));
            Debug.Log(typeData.MemberType);
            Assert.IsTrue(typeData.MemberType == MemberTypes.TypeInfo);
            Debug.Log(typeData.FullDeclarationWithAttributes);
            Assert.AreEqual(@"[NullableContext]
[Nullable]
public record TestRecord : System.IEquatable<TestRecord>", typeData.FullDeclarationWithAttributes);
        }

        [Test]
        public void TestStruct()
        {
            var typeData = new TypeData(typeof(TestStruct));
            Debug.Log(typeData.MemberType);
            Assert.IsTrue(typeData.MemberType == MemberTypes.TypeInfo);
            Debug.Log(typeData.FullDeclarationWithAttributes);
            Assert.AreEqual("public struct TestStruct : System.ValueType",
                typeData.FullDeclarationWithAttributes);
        }

        [Test]
        public void TestScriptDocGeneratorTestEnum()
        {
            var typeData = new TypeData(typeof(ScriptDocGeneratorTestEnum));
            Debug.Log(typeData.MemberType);
            Assert.IsTrue(typeData.MemberType == MemberTypes.TypeInfo);
            Debug.Log(typeData.FullDeclarationWithAttributes);
            Assert.AreEqual(@"public enum ScriptDocGeneratorTestEnum : System.Enum, 
System.IFormattable, 
System.IComparable, 
System.IConvertible", typeData.FullDeclarationWithAttributes);
        }

        [Test]
        public void TestNestedClass()
        {
            var typeData = new TypeData(typeof(NestedClass));
            Debug.Log(typeData.MemberType);
            Assert.IsTrue(typeData.MemberType == MemberTypes.NestedType);
            Debug.Log(typeData.FullDeclarationWithAttributes);
            Assert.AreEqual("private class TypeDataTests.NestedClass",
                typeData.FullDeclarationWithAttributes);
        }

        #region Nested type: NestedClass

        class NestedClass { }

        #endregion
    }
}
