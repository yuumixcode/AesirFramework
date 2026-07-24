using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace Runestone.AesirArchitecture.Tests
{
    /// <summary>
    /// 验证 UnityEngine.Object 派生类在各种场景下的 null 检查语义与行为的测试类
    /// </summary>
    public class UnityEngineObjectCheckNullTests
    {
        /// <summary>
        /// 验证直接 new 继承自 UnityEngine.Object 的 C# 对象时，不存在 native C++ counterpart object。
        /// is not null 为 true → C# 引用存在；== null 为 true → 无 native counterpart（UnityEngine.Object 的 == 运算符重载行为）。
        /// </summary>
        [UnityTest]
        public IEnumerator NewUnityEngineObject_NoHaveNativeCounterpartObject()
        {
            var temp = new TempObject(123);
            var hasCsharpReference = temp is not null;
            var hasNoNativeCounterpart = temp == null;
            AesirArchitectureDebug.LogTestInfo("C# 引用不为 null: " + hasCsharpReference);
            AesirArchitectureDebug.LogTestInfo("C++ Native Counterpart 为 null: " + hasNoNativeCounterpart);
            Assert.IsTrue(hasCsharpReference && hasNoNativeCounterpart);
            AesirArchitectureDebug.LogTestInfo(
                "NewUnityEngineObject 测试结果: 直接 new 的 C# 对象 is not null 为 true，== null 也为 true（无 native counterpart）");
            yield return null;
        }

        /// <summary>
        /// 验证 Destroy 一个 MonoBehaviour 后，C# 引用仍存在但 native C++ counterpart 已销毁。
        /// Destroy 前：is not null 为 true，!= null 为 true（C# 引用和 native counterpart 均存在）；
        /// Destroy 后：is not null 为 true（C# 引用仍在），== null 为 true（native counterpart 已销毁）。
        /// </summary>
        [UnityTest]
        public IEnumerator UnityEngineObject_DestroyMonoBehaviourNoSetNull()
        {
            var managedMonoBehaviour = new GameObject("ManagedMonoBehaviour")
                .AddComponent<UnityEngineObjectTempMonoBehaviour>();
            var hasCsharpReferenceBefore = managedMonoBehaviour is not null;
            var hasNativeCounterpartBefore = managedMonoBehaviour != null;
            AesirArchitectureDebug.LogTestInfo("在 Mono 物体对象未执行 Destroy 之前，C# 引用不为 null: " +
                                               hasCsharpReferenceBefore);
            AesirArchitectureDebug.LogTestInfo("在 Mono 物体对象未执行 Destroy 之前，C++ Native Counterpart 不为 null: " +
                                               hasNativeCounterpartBefore);
            Object.Destroy(managedMonoBehaviour);
            yield return null;
            var hasCsharpReferenceAfter = managedMonoBehaviour is not null;
            var isNativeDestroyed = managedMonoBehaviour == null;
            AesirArchitectureDebug.LogTestInfo("在 Mono 物体对象执行 Object.Destroy 之后，C# 引用不为 null: " +
                                               hasCsharpReferenceAfter);
            AesirArchitectureDebug.LogTestInfo(
                "在 Mono 物体对象执行 Object.Destroy 之后，C++ Native Counterpart 为 null: " + isNativeDestroyed);
            Assert.IsTrue(hasCsharpReferenceBefore && hasNativeCounterpartBefore && hasCsharpReferenceAfter &&
                          isNativeDestroyed);
            AesirArchitectureDebug.LogTestInfo(
                "DestroyMonoBehaviour 测试结果: Destroy 后 C# 引用仍存在（is not null），但 == null 为 true（native counterpart 已销毁）");
            yield return null;
        }

        /// <summary>
        /// 验证将 UnityEngine.Object 派生对象的 C# 引用置为 null 但不调用 Destroy 时，native C++ 对象仍然存活。
        /// 仅丢弃 C# 引用不会触发 native 对象的销毁，必须显式调用 Object.Destroy。
        /// </summary>
        [UnityTest]
        public IEnumerator UnityEngineObject_SetReferenceNullWithoutDestroy_NativeObjectSurvives()
        {
            var tempObject = new GameObject("LeakedNativeObject")
                .AddComponent<UnityEngineObjectTempMonoBehaviour>();
            var instanceID = tempObject.GetInstanceID();
            // ReSharper disable once RedundantAssignment
            tempObject = null;
            var csharpReferenceIsNull = tempObject is null;
            // C# 引用置 null 后无法再通过 tempObject != null 检查 native counterpart，
            // 需通过 Resources.FindObjectsOfTypeAll 重新查找该对象
            UnityEngineObjectTempMonoBehaviour leaked = null;
            var allTempMonoArray = Resources.FindObjectsOfTypeAll<UnityEngineObjectTempMonoBehaviour>();
            foreach (var g in allTempMonoArray)
            {
                if (g.GetInstanceID() != instanceID)
                {
                    continue;
                }

                leaked = g;
                break;
            }

            var isNativeAlive = leaked != null;
            AesirArchitectureDebug.LogTestInfo("C# 引用置 null 后（未 Destroy）: native 对象仍存活: " + isNativeAlive);
            yield return null;
            Assert.IsTrue(csharpReferenceIsNull && isNativeAlive);
            AesirArchitectureDebug.LogTestInfo(
                "SetReferenceNullWithoutDestroy 测试结果: 仅丢弃 C# 引用不会触发 native 对象的销毁，必须显式调用 Object.Destroy");
            yield return null;
            if (leaked != null)
            {
                Object.Destroy(leaked);
            }
        }

        /// <summary>
        /// 表示一个临时对象，继承自 UnityEngine.Object，用于验证 null 检查语义
        /// </summary>
        class TempObject : Object
        {
            /// <summary>
            /// 创建临时对象并指定 ID
            /// </summary>
            public TempObject(int id) => ID = id;

            /// <summary>
            /// 对象标识
            /// </summary>
            int ID { get; }

            ~TempObject()
            {
                AesirArchitectureDebug.LogTestInfo($"TempObject（ID：{ID}）的 C# 托管对象被 GC 回收了");
            }
        }
    }

    /// <summary>
    /// 测试用 MonoBehaviour，用于验证 Destroy 后的 null 检查行为
    /// </summary>
    public class UnityEngineObjectTempMonoBehaviour : MonoBehaviour
    {
        /// <summary>
        /// 测试用标识 ID
        /// </summary>
        [SerializeField]
        int id;
    }
}
