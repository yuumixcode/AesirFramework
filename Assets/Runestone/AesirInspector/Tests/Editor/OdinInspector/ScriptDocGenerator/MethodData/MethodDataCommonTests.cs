using Runestone.AesirInspector;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace Runestone.AesirInspector.Editor.Tests
{
    public class MethodDataCommonTests
    {
        static readonly MethodInfo[] MethodInfos = typeof(TestClass).GetRuntimeMethods().ToArray();

        static readonly IMethodData[] MethodDataArray = MethodInfos
            .Select(m => UnitTestAnalysisFactory.Default.CreateMethodData(m)).ToArray();

        static readonly Dictionary<string, List<string>> ExpectedMethodSignature =
            new Dictionary<string, List<string>>
            {
                {
                    nameof(TestClass.EmptyParamMethod),
                    new List<string>
                    {
                        "public void EmptyParamMethod",
                        "public void EmptyParamMethod()"
                    }
                },
                {
                    nameof(TestClass.OneIntParamMethod),
                    new List<string>
                    {
                        "public void OneIntParamMethod",
                        "public void OneIntParamMethod(int param)"
                    }
                },
                {
                    nameof(TestClass.TwoParamMethod),
                    new List<string>
                    {
                        "public void TwoParamMethod",
                        "public void TwoParamMethod(string param1, bool param2)"
                    }
                },
                {
                    nameof(TestClass.GenericMethod),
                    new List<string>
                    {
                        "public void GenericMethod<T>",
                        "public void GenericMethod<T>(T param)"
                    }
                },
                {
                    nameof(TestClass.TwoParamsGenericMethod),
                    new List<string>
                    {
                        "public void TwoParamsGenericMethod<T, T1>",
                        "public void TwoParamsGenericMethod<T, T1>(T param1, List<T1> param2)"
                    }
                }
            };

        [Test]
        public void TestEmptyParamMethod()
        {
            var methodData =
                MethodDataArray.First(f => ((MemberData)f).Name == nameof(TestClass.EmptyParamMethod));
            Debug.Log(methodData.SignatureWithoutParameters);
            Debug.Log(methodData.Signature);
            Assert.AreEqual(ExpectedMethodSignature[nameof(TestClass.EmptyParamMethod)][0],
                methodData.SignatureWithoutParameters);
            Assert.AreEqual(ExpectedMethodSignature[nameof(TestClass.EmptyParamMethod)][1],
                methodData.Signature);
        }

        [Test]
        public void TestOneIntParamMethod()
        {
            var methodData =
                MethodDataArray.First(f => ((MemberData)f).Name == nameof(TestClass.OneIntParamMethod));
            Debug.Log(methodData.SignatureWithoutParameters);
            Debug.Log(methodData.Signature);
            Assert.AreEqual(ExpectedMethodSignature[nameof(TestClass.OneIntParamMethod)][0],
                methodData.SignatureWithoutParameters);
            Assert.AreEqual(ExpectedMethodSignature[nameof(TestClass.OneIntParamMethod)][1],
                methodData.Signature);
        }

        [Test]
        public void TestTwoParamMethod()
        {
            var methodData =
                MethodDataArray.First(f => ((MemberData)f).Name == nameof(TestClass.TwoParamMethod));
            Debug.Log(methodData.SignatureWithoutParameters);
            Debug.Log(methodData.Signature);
            Assert.AreEqual(ExpectedMethodSignature[nameof(TestClass.TwoParamMethod)][0],
                methodData.SignatureWithoutParameters);
            Assert.AreEqual(ExpectedMethodSignature[nameof(TestClass.TwoParamMethod)][1],
                methodData.Signature);
        }

        [Test]
        public void TestGenericMethod()
        {
            var methodData =
                MethodDataArray.First(f => ((MemberData)f).Name == nameof(TestClass.GenericMethod));
            Debug.Log(methodData.SignatureWithoutParameters);
            Debug.Log(methodData.Signature);
            Assert.AreEqual(ExpectedMethodSignature[nameof(TestClass.GenericMethod)][0],
                methodData.SignatureWithoutParameters);
            Assert.AreEqual(ExpectedMethodSignature[nameof(TestClass.GenericMethod)][1],
                methodData.Signature);
        }

        [Test]
        public void TestTwoParamsGenericMethod()
        {
            var methodData = MethodDataArray.First(f =>
                ((MemberData)f).Name == nameof(TestClass.TwoParamsGenericMethod));
            Debug.Log(methodData.SignatureWithoutParameters);
            Debug.Log(methodData.Signature);
            Assert.AreEqual(ExpectedMethodSignature[nameof(TestClass.TwoParamsGenericMethod)][0],
                methodData.SignatureWithoutParameters);
            Assert.AreEqual(ExpectedMethodSignature[nameof(TestClass.TwoParamsGenericMethod)][1],
                methodData.Signature);
        }

        [Test]
        public void TestIndefiniteParamsMethod()
        {
            var methodData = MethodDataArray.First(f =>
                ((MemberData)f).Name == nameof(TestClass.IndefiniteParamsMethod));
            Debug.Log(methodData.SignatureWithoutParameters);
            Debug.Log(methodData.Signature);
            Assert.AreEqual("public void IndefiniteParamsMethod", methodData.SignatureWithoutParameters);
            Assert.AreEqual("public void IndefiniteParamsMethod(string param1, params bool[] param2)",
                methodData.Signature);
        }

        [Test]
        public void TestHasReturnStringMethod()
        {
            var methodData = MethodDataArray.First(f =>
                ((MemberData)f).Name == nameof(TestClass.HasReturnStringMethod));
            Debug.Log(methodData.SignatureWithoutParameters);
            Debug.Log(methodData.Signature);
            Assert.AreEqual("public string HasReturnStringMethod", methodData.SignatureWithoutParameters);
            Assert.AreEqual("public string HasReturnStringMethod(float param1)", methodData.Signature);
        }

        [Test]
        public void TestStaticMethod()
        {
            var methodData =
                MethodDataArray.First(f => ((MemberData)f).Name == nameof(TestClass.StaticMethod));
            Debug.Log(methodData.SignatureWithoutParameters);
            Debug.Log(methodData.Signature);
            Assert.AreEqual("public static void StaticMethod", methodData.SignatureWithoutParameters);
            Assert.AreEqual("public static void StaticMethod()", methodData.Signature);
        }

        [Test]
        public void TestHasReturnBoolMethodWithDefault()
        {
            var methodData = MethodDataArray.First(f =>
                ((MemberData)f).Name == nameof(TestClass.HasReturnBoolMethodWithDefault));
            Debug.Log(methodData.SignatureWithoutParameters);
            Debug.Log(methodData.Signature);
            Assert.AreEqual("public static bool HasReturnBoolMethodWithDefault",
                methodData.SignatureWithoutParameters);
            Assert.AreEqual(
                "public static bool HasReturnBoolMethodWithDefault(float param1 = 5f, bool param2 = false, string param3 = \"Hello World\", int param4 = 0, ScriptDocGeneratorTestEnum param5 = ScriptDocGeneratorTestEnum.Value3)",
                methodData.Signature);
        }

        [Test]
        public void TestAsyncMethod()
        {
            var methodData =
                MethodDataArray.First(f => ((MemberData)f).Name == nameof(TestClass.AsyncMethod));
            Debug.Log(methodData.SignatureWithoutParameters);
            Debug.Log(methodData.Signature);
            Assert.AreEqual("public static async Task AsyncMethod", methodData.SignatureWithoutParameters);
            Assert.AreEqual("public static async Task AsyncMethod()", methodData.Signature);
        }

        [Test]
        public void TestAsyncMethodWithReturnValue()
        {
            var methodData = MethodDataArray.First(f =>
                ((MemberData)f).Name == nameof(TestClass.AsyncMethodWithReturnValue));
            Debug.Log(methodData.SignatureWithoutParameters);
            Debug.Log(methodData.Signature);
            Assert.AreEqual("public async Task<int> AsyncMethodWithReturnValue",
                methodData.SignatureWithoutParameters);
            Assert.AreEqual("public async Task<int> AsyncMethodWithReturnValue()", methodData.Signature);
        }

        #region Nested type: TestClass

        class TestClass
        {
            public void EmptyParamMethod()
            {
                Debug.Log("EmptyParamMethod");
            }

            public void OneIntParamMethod(int param)
            {
                Debug.Log(param);
            }

            public void TwoParamMethod(string param1, bool param2)
            {
                Debug.Log(param1 + param2);
            }

            public void GenericMethod<T>(T param)
            {
                Debug.Log(param);
            }

            public void TwoParamsGenericMethod<T, T1>(T param1, List<T1> param2)
            {
                Debug.Log(param1);
                Debug.Log(param2);
            }

            public void IndefiniteParamsMethod(string param1, params bool[] param2)
            {
                Debug.Log(param1 + param2);
            }

            public string HasReturnStringMethod(float param1)
            {
                Debug.Log(param1);
                return "HasReturnStringMethod";
            }

            public static void StaticMethod()
            {
                Debug.Log("StaticMethod");
            }

            public static bool HasReturnBoolMethodWithDefault(float param1 = 5f,
                bool param2 = false,
                string param3 = "Hello World",
                int param4 = 0,
                ScriptDocGeneratorTestEnum param5 = ScriptDocGeneratorTestEnum.Value3)
            {
                Debug.Log(param3 + param4);
                return true;
            }

            public static async Task AsyncMethod()
            {
                await Task.Delay(1);
            }

            public async Task<int> AsyncMethodWithReturnValue()
            {
                await Task.Delay(1);
                return 42;
            }
        }

        #endregion
    }
}
