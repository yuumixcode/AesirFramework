using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace Runestone.AesirArchitecture.Tests
{
    /// <summary>
    /// 验证 <see cref="UnityEngine.Object" /> 派生类在各种场景下的 null 检查语义与行为。
    /// <para>
    /// 测试覆盖三种场景：直接 new 的对象无 native counterpart、Destroy 后 native 销毁但 C# 引用存活、
    /// 以及丢弃 C# 引用但不 Destroy 时 native 对象仍存活。
    /// </para>
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     <see cref="UnityEngine.Object" /> 重载了 <c>==</c>/<c>!=</c> 运算符和隐式转换，
    ///     使 <c>obj == null</c> 不仅检查 C# 引用是否为 null，还检查底层 C++ native counterpart 是否已销毁。
    ///     这是 Unity 独有的行为，与纯 C# 的 null 语义不同，容易导致框架代码中出现隐蔽的空引用 bug。
    ///     </para>
    ///     <para>
    ///     Aesir Architecture 架构中的 View、Controller、Presenter 等角色经常持有 <see cref="UnityEngine.Object" /> 派生类型的引用，
    ///     理解这些 null 语义差异对于编写安全的框架代码至关重要——特别是在生命周期管理和对象销毁场景中。
    ///     </para>
    ///     <para>测试使用 <c>[UnityTest]</c> 而非 <c>[Test]</c>，因为部分场景需要跨帧等待（如 <c>Object.Destroy</c> 的延迟销毁）。</para>
    /// </remarks>
    /// <seealso cref="UnityEngine.Object" />
    public class UnityEngineObjectCheckNullTests
    {
        /// <summary>
        /// 验证直接 new 继承自 <see cref="UnityEngine.Object" /> 的 C# 对象时，不存在 native C++ counterpart object。
        /// <para>预期：<c>is not null</c> 为 <c>true</c>（C# 引用存在）；<c>== null</c> 为 <c>true</c>（无 native counterpart）。</para>
        /// </summary>
        /// <remarks>
        /// <see cref="UnityEngine.Object" /> 的 <c>==</c> 运算符会检查 native counterpart 的存在性。
        /// 直接 <c>new</c> 创建的派生对象没有经过 Unity 引擎的原生对象创建流程，因此没有 native counterpart，
        /// 导致 <c>== null</c> 返回 <c>true</c>——尽管 C# 引用本身不为 null。
        /// 这意味着 <c>obj == null</c> 为 true 时，对象未必真的"不存在"，框架代码不能仅依赖 <c>== null</c> 判断对象可用性。
        /// </remarks>
        /// <seealso cref="UnityEngine.Object" />
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
        /// 验证 <see cref="Object.Destroy" /> 一个 <see cref="MonoBehaviour" /> 后，C# 引用仍存在但 native C++ counterpart 已销毁。
        /// <para>
        /// Destroy 前：<c>is not null</c> 为 <c>true</c>，<c>!= null</c> 为 <c>true</c>（C# 引用和 native counterpart 均存在）；
        /// Destroy 后：<c>is not null</c> 为 <c>true</c>（C# 引用仍在），<c>== null</c> 为 <c>true</c>（native counterpart 已销毁）。
        /// </para>
        /// </summary>
        /// <remarks>
        /// <c>Object.Destroy</c> 不会立即释放 C# 托管对象——C# 引用仍指向内存中的托管对象，
        /// 但底层 C++ 对象在当前帧结束后被标记为销毁。此后 <c>== null</c> 返回 <c>true</c>，
        /// 而 <c>is not null</c>（纯 C# 检查）仍返回 <c>true</c>。
        /// <para>
        /// 这一行为对框架至关重要：如果 View 持有的 <see cref="GameObject" /> 引用被 Destroy 后未清理，
        /// 框架代码通过 <c>is not null</c> 检查会误认为对象仍然有效，继续访问已销毁的 native 对象将抛出异常。
        /// 架构必须使用 <c>== null</c> 或 <see cref="Object" /> 的隐式 bool 转换来正确检测 native 对象的销毁状态。
        /// </para>
        /// </remarks>
        /// <seealso cref="UnityEngine.Object" />
        /// <seealso cref="Object.Destroy" />
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
        /// 验证将 <see cref="UnityEngine.Object" /> 派生对象的 C# 引用置为 null 但不调用 <see cref="Object.Destroy" /> 时，native C++
        /// 对象仍然存活。
        /// <para>预期：C# 引用为 null，但通过 <c>Resources.FindObjectsOfTypeAll</c> 仍能找到 native 对象。</para>
        /// </summary>
        /// <remarks>
        /// 仅丢弃 C# 引用不会触发 native 对象的销毁，必须显式调用 <see cref="Object.Destroy" />。
        /// <para>
        /// 这意味着架构中的 View/Controller 如果仅将引用置为 null 而不调用 Destroy，
        /// native <see cref="GameObject" /> 及其组件将泄漏在场景中，持续占用内存并参与每帧更新，
        /// 最终导致性能下降或逻辑错误（如隐藏的 Collider 仍参与碰撞检测）。
        /// 此测试通过 <c>Resources.FindObjectsOfTypeAll</c> 重新查找泄漏对象来验证 native 对象的存活状态，
        /// 因为置 null 后已无法通过原引用检查 native counterpart。
        /// </para>
        /// </remarks>
        /// <seealso cref="UnityEngine.Object" />
        /// <seealso cref="Object.Destroy" />
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
        /// 表示一个临时对象，继承自 <see cref="UnityEngine.Object" />，用于验证 null 检查语义
        /// </summary>
        /// <remarks>
        /// 此类通过直接 <c>new</c> 创建（而非 <c>AddComponent</c> 或 <c>Instantiate</c>），
        /// 因此不经过 Unity 引擎的原生对象创建流程，没有 native counterpart。
        /// 这使得它成为验证 <c>== null</c> 与 <c>is not null</c> 语义差异的理想测试对象。
        /// </remarks>
        /// <seealso cref="UnityEngine.Object" />
        class TempObject : Object
        {
            /// <summary>
            /// 创建临时对象并指定 ID
            /// </summary>
            /// <param name="id">对象标识，用于析构日志中区分不同实例</param>
            public TempObject(int id) => ID = id;

            /// <summary>
            /// 对象标识
            /// </summary>
            int ID { get; }

            /// <summary>
            /// 析构函数，在 GC 回收托管对象时输出日志，用于验证 C# 对象的生命周期与 native 对象不同步
            /// </summary>
            ~TempObject()
            {
                AesirArchitectureDebug.LogTestInfo($"TempObject（ID：{ID}）的 C# 托管对象被 GC 回收了");
            }
        }
    }

    /// <summary>
    /// 测试用 <see cref="MonoBehaviour" />，用于验证 <see cref="Object.Destroy" /> 后的 null 检查行为
    /// </summary>
    /// <remarks>
    /// 此组件通过 <c>GameObject.AddComponent</c> 创建，具有完整的 native counterpart。
    /// 测试中对其调用 <see cref="Object.Destroy" /> 后，C# 引用仍存在但 native 对象已销毁，
    /// 用于验证 <see cref="UnityEngine.Object" /> 的 <c>== null</c> 运算符在 native 销毁场景下的行为。
    /// </remarks>
    /// <seealso cref="UnityEngine.Object" />
    /// <seealso cref="Object.Destroy" />
    public class UnityEngineObjectTempMonoBehaviour : MonoBehaviour
    {
        /// <summary>
        /// 测试用标识 ID
        /// </summary>
        [SerializeField]
        int id;
    }
}
