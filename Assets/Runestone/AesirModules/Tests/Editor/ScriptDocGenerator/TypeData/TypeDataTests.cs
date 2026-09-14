using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Runestone.AesirModules.ScriptDocGenerator;
using UnityEngine;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
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
                          "public class TypeDataTests : Runestone.AesirModules.Tests.Editor.ScriptDocGenerator.TestAbstractClass" ==
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

        #region 合成访问器过滤（IsSyntheticAccessor）

        class AccessorFilterFixture
        {
            public event TestDelegate SomethingHappened;

            public int Value { get; set; }

            /// <summary>用户以访问器前缀命名的普通方法——不应被误滤。</summary>
            public void get_Thing() { }

            /// <summary>用户以访问器前缀命名的普通方法——不应被误滤。</summary>
            public void add_Item() { }

            protected void Raise() => SomethingHappened?.Invoke();
        }

        [Test]
        public void SyntheticAccessors_Filtered_UserPrefixMethodsKept()
        {
            var typeData = new TypeData(typeof(AccessorFilterFixture));
            var signatures = new List<string>();
            foreach (var method in typeData.RuntimeReflectedMethodsData)
            {
                signatures.Add(method.Signature);
            }

            Assert.IsFalse(signatures.Exists(s => s.Contains("add_SomethingHappened")), "事件 add 访问器应被过滤");
            Assert.IsFalse(signatures.Exists(s => s.Contains("remove_SomethingHappened")), "事件 remove 访问器应被过滤");
            Assert.IsFalse(signatures.Exists(s => s.Contains("get_Value")), "属性 get 访问器应被过滤");
            Assert.IsFalse(signatures.Exists(s => s.Contains("set_Value")), "属性 set 访问器应被过滤");
            Assert.IsTrue(signatures.Exists(s => s.Contains("get_Thing")), "用户普通方法 get_Thing 不应被名称前缀误杀");
            Assert.IsTrue(signatures.Exists(s => s.Contains("add_Item")), "用户普通方法 add_Item 不应被名称前缀误杀");
        }

        #endregion
    }
}
