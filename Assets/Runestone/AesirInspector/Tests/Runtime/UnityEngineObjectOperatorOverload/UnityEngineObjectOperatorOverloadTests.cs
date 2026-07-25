// ----------------------------------------------------------------------------
// MIT License
// 
// Copyright (c) 2026 Runestone - Yuumix
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ----------------------------------------------------------------------------

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Runestone.AesirInspector.Tests
{
    /// <summary>
    /// 关于 UnityEngine.Object 运算符重载的测试
    /// </summary>
    [Summary("关于 UnityEngine.Object 运算符重载的测试")]
    public class UnityEngineObjectOperatorOverloadTests
    {
        /// <summary>
        /// 测试直接继承 UnityEngine.Object 的自定义类 TempObject 是否具有 native C++ counterpart object
        /// </summary>
        [Summary("测试直接继承 UnityEngine.Object 的自定义类 TempObject 是否具有 native C++ counterpart object")]
        [UnityTest]
        public IEnumerator UnityEngineObject_NewDirectInheritedObjectNoHaveNativeCounterpartObject()
        {
            var temp = new TempObject(123);
            var flag1 = temp is not null;
            Debug.Log("new TempObject 对象后，temp is not null 的结果为: " + flag1);
            var flag2 = temp == null;
            Debug.Log("new TempObject 对象后，temp == null 的结果为: " + flag2);
            Debug.Log(flag1 && flag2);
            // flag1 为 true 说明 temp 的 C# 引用不为 null，存在 C# 实例。
            // flag2 为 true 说明 temp 没有 native C++ counterpart object
            Assert.IsTrue(flag1 && flag2);
            yield return null;
        }

        /// <summary>
        /// 测试销毁 MonoBehaviour 后，C# 引用是否被置空
        /// </summary>
        [Summary("测试销毁 MonoBehaviour 后，C# 引用是否被置空")]
        [UnityTest]
        public IEnumerator UnityEngineObject_DestroyMonoBehaviourNoSetNull()
        {
            var managedMonoBehaviour = new GameObject("ManagedMonoBehaviour")
                .AddComponent<UnityEngineObjectTempMonoBehaviour>();
            var flag1 = managedMonoBehaviour is not null;
            Debug.Log("managedMonoBehaviour is not null 的结果为: " + flag1);
            var flag2 = managedMonoBehaviour != null;
            Debug.Log("managedMonoBehaviour != null 的结果为: " + flag2);
            // flag1 为 true 说明 managedMonoBehaviour 的 C# 引用不为 null，存在 C# 实例。
            // flag2 为 true 说明 managedMonoBehaviour 存在 native C++ counterpart object。
            Debug.Log(flag1 && flag2);
            Object.Destroy(managedMonoBehaviour);
            yield return null;
            var flag3 = managedMonoBehaviour is not null;
            Debug.Log("managedMonoBehaviour is not null 的结果为: " + flag3);
            var flag4 = managedMonoBehaviour == null;
            Debug.Log("managedMonoBehaviour == null 的结果为: " + flag4);
            // flag3 为 true 说明 managedMonoBehaviour 的 C# 引用不为 null，存在 C# 实例。
            // flag4 为 true 说明 managedMonoBehaviour 不存在 native C++ counterpart object。
            Debug.Log(flag3 && flag4);
            Assert.IsTrue(flag1 && flag2 && flag3 && flag4);
            yield return null;
        }

        /// <summary>
        /// 用于测试的继承了 Object 的类
        /// </summary>
        [Summary("用于测试的继承了 Object 的类")]
        class TempObject : Object
        {
            public TempObject(int id) => ID = id;

            public int ID { get; }

            ~TempObject()
            {
                Debug.Log($"TempObject（ID：{ID}）的 C# 托管对象被 GC 回收了");
            }
        }
    }
}
