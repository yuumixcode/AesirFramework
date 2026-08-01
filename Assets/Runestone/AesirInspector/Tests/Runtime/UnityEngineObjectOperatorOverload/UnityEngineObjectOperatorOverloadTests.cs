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
    public class UnityEngineObjectOperatorOverloadTests
    {
        /// <summary>
        /// 测试直接继承 UnityEngine.Object 的自定义类 TempObject 是否具有 native C++ counterpart object
        /// </summary>
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
