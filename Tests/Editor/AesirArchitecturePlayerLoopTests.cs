using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    /// <summary>
    /// 验证 <see cref="AesirArchitecturePlayerLoop"/> 与 <see cref="PlayerLoopUtility"/> 在自定义系统插入和生命周期回调方面的正确性。
    /// <para>测试覆盖三个维度：PlayerLoop 子系统的插入位置、描述输出准确性、以及回调注册/排序/清除的行为。</para>
    /// </summary>
    /// <remarks>
    /// <para>PlayerLoop 是 Unity 引擎每帧执行的核心调度结构，框架通过向其中注入自定义子系统来实现无需 MonoBehaviour 的帧级回调。
    /// 如果插入位置错误或回调排序不稳定，将导致架构逻辑在错误的帧阶段执行，引发难以排查的时序 bug。</para>
    /// <para>每个测试在 <see cref="SetUp"/> 中保存原始 PlayerLoop 快照，在 <see cref="TearDown"/> 中恢复，
    /// 确保测试间副作用隔离，不会污染全局 PlayerLoop 状态。</para>
    /// </remarks>
    /// <seealso cref="AesirArchitecturePlayerLoop"/>
    /// <seealso cref="PlayerLoopUtility"/>
    /// <seealso cref="AesirArchitectureLifecyclePhase"/>
    public class AesirArchitecturePlayerLoopTests
    {
        /// <summary>
        /// 框架注入的 BeforeUpdate 子系统类型名。与 <c>AesirArchitecturePlayerLoop</c> 内部私有嵌套结构体同名，
        /// 测试无法直接引用私有类型，故以名称匹配。若内部类型重命名，此常量需同步更新。
        /// </summary>
        const string BeforeUpdateSystemName = "AesirArchitectureScriptRunBeforeUpdate";

        /// <summary>
        /// 框架注入的 AfterUpdate 子系统类型名。与 <c>AesirArchitecturePlayerLoop</c> 内部私有嵌套结构体同名，
        /// 测试无法直接引用私有类型，故以名称匹配。若内部类型重命名，此常量需同步更新。
        /// </summary>
        const string AfterUpdateSystemName = "AesirArchitectureScriptRunAfterUpdate";

        PlayerLoopSystem _originalLoop;

        /// <summary>
        /// 保存当前 PlayerLoop 快照并重置架构钩子状态，为每个测试提供干净的初始环境
        /// </summary>
        /// <remarks>
        /// 保存原始 PlayerLoop 是因为被测方法会修改全局 PlayerLoop 状态（调用 <c>PlayerLoop.SetPlayerLoop</c>），
        /// 若不在 <see cref="TearDown"/> 中恢复，后续测试将基于被污染的 PlayerLoop 执行，导致断言失败或副作用累积。
        /// </remarks>
        [SetUp]
        public void SetUp()
        {
            _originalLoop = PlayerLoop.GetCurrentPlayerLoop();
            AesirArchitecturePlayerLoop.Reset();
        }

        /// <summary>
        /// 恢复测试前的 PlayerLoop 状态并清除架构钩子，防止测试副作用泄漏到后续测试
        /// </summary>
        /// <remarks>
        /// 测试中插入的自定义子系统如果不清理，会影响其他测试的 PlayerLoop 结构，
        /// 也可能导致编辑器在帧循环中持续触发已被测试销毁的回调委托。
        /// </remarks>
        [TearDown]
        public void TearDown()
        {
            PlayerLoop.SetPlayerLoop(_originalLoop);
            AesirArchitecturePlayerLoop.Reset();
        }

        /// <summary>
        /// 验证 <see cref="PlayerLoopUtility.InsertSystemBefore{TTarget}"/> 将自定义系统插入到目标系统之前。
        /// <para>预期：插入返回 <c>true</c>，且自定义系统在 PlayerLoop 中位于目标系统之前。</para>
        /// </summary>
        /// <remarks>
        /// 框架依赖 <c>InsertSystemBefore&lt;Update&gt;</c> 将 BeforeUpdate 回调入口注入到 Update 子系统之前，
        /// 如果插入位置错误（如插入到之后或未插入），架构的帧前逻辑将晚于 Update 执行，
        /// 导致依赖帧初始状态的逻辑读取到已被 Update 修改过的数据。
        /// </remarks>
        /// <seealso cref="PlayerLoopUtility.InsertSystemBefore{TTarget}"/>
        /// <seealso cref="AesirArchitecturePlayerLoop"/>
        [Test]
        public void InsertSystemBefore_TargetExists_InsertsBefore()
        {
            var markerType = typeof(TestMarkerBeforeUpdate);
            var inserted = PlayerLoopUtility.InsertSystemBefore<Update>(
                new PlayerLoopSystem { type = markerType });
            Assert.IsTrue(inserted);
            AssertIsBeforeInLoop(markerType, typeof(Update));
            AesirArchitectureDebug.LogTestInfo("InsertSystemBefore: 成功在 Update 之前插入了自定义系统");
        }

        /// <summary>
        /// 验证 <see cref="PlayerLoopUtility.InsertSystemAfter{TTarget}"/> 将自定义系统插入到目标系统之后。
        /// <para>预期：插入返回 <c>true</c>，且自定义系统在 PlayerLoop 中位于目标系统之后。</para>
        /// </summary>
        /// <remarks>
        /// 框架依赖 <c>InsertSystemAfter&lt;PostLateUpdate&gt;</c> 将 AfterUpdate 回调入口注入到 PostLateUpdate 子系统之后，
        /// 确保架构逻辑在每帧所有更新完成后执行。如果插入到 PostLateUpdate 之前，
        /// AfterUpdate 回调将读取到尚未完成 LateUpdate 的中间状态。
        /// </remarks>
        /// <seealso cref="PlayerLoopUtility.InsertSystemAfter{TTarget}"/>
        /// <seealso cref="AesirArchitecturePlayerLoop"/>
        [Test]
        public void InsertSystemAfter_TargetExists_InsertsAfter()
        {
            var markerType = typeof(TestMarkerAfterFixedUpdate);
            var inserted = PlayerLoopUtility.InsertSystemAfter<FixedUpdate>(
                new PlayerLoopSystem { type = markerType });

            Assert.IsTrue(inserted);
            AssertIsAfterInLoop(markerType, typeof(FixedUpdate));
            AesirArchitectureDebug.LogTestInfo("InsertSystemAfter: 成功在 FixedUpdate 之后插入了自定义系统");
        }

        /// <summary>
        /// 验证对同一目标系统连续两次 <see cref="PlayerLoopUtility.InsertSystemBefore{TTarget}"/> 时，两次插入均生效。
        /// <para>预期：两个自定义系统均存在于 PlayerLoop 中。</para>
        /// </summary>
        /// <remarks>
        /// 框架的 <c>Initialize()</c> 方法通过 <c>ContainsSystem&lt;T&gt;</c> 去重，但在测试和扩展场景中，
        /// 用户可能需要向同一目标位置插入多个自定义系统。如果后续插入覆盖了先前的插入，
        /// 将导致部分回调丢失。此测试确保插入操作是累加的而非替换的。
        /// </remarks>
        /// <seealso cref="PlayerLoopUtility.InsertSystemBefore{TTarget}"/>
        /// <seealso cref="PlayerLoopUtility.ContainsSystem{TTarget}"/>
        [Test]
        public void InsertSystemBefore_SameTargetTwice_InsertsTwoSystems()
        {
            var marker1 = typeof(TestMarkerTwice1);
            var marker2 = typeof(TestMarkerTwice2);

            PlayerLoopUtility.InsertSystemBefore<Update>(new PlayerLoopSystem { type = marker1 });
            PlayerLoopUtility.InsertSystemBefore<Update>(new PlayerLoopSystem { type = marker2 });

            var loop = PlayerLoop.GetCurrentPlayerLoop();
            Assert.IsTrue(ContainsType(ref loop, marker1), "First marker should exist");
            Assert.IsTrue(ContainsType(ref loop, marker2), "Second marker should exist");
            AesirArchitectureDebug.LogTestInfo("InsertSystemBefore(连续两次): 成功在 Update 中插入了两个自定义系统");
        }

        /// <summary>
        /// 验证 <see cref="PlayerLoopUtility.GetCurrentPlayerLoopDescription"/> 在默认 PlayerLoop 下包含核心系统名称。
        /// <para>预期：输出字符串中包含 <c>Update</c>、<c>FixedUpdate</c>、<c>PostLateUpdate</c> 等核心系统名。</para>
        /// </summary>
        /// <remarks>
        /// 描述输出是调试和验证 PlayerLoop 状态的主要手段。如果 <c>GetCurrentPlayerLoopDescription</c> 无法正确反映
        /// PlayerLoop 的实际结构，开发者将无法通过输出来确认注入是否成功，增加排查难度。
        /// 此测试以 Unity 引擎保证存在的核心系统作为基准，验证描述输出的基本可靠性。
        /// </remarks>
        /// <seealso cref="PlayerLoopUtility.GetCurrentPlayerLoopDescription"/>
        [Test]
        public void GetCurrentPlayerLoopDescription_DefaultLoop_ContainsCoreSystems()
        {
            var dump = PlayerLoopUtility.GetCurrentPlayerLoopDescription();

            Assert.IsTrue(dump.Contains("Update"), "Should contain Update");
            Assert.IsTrue(dump.Contains("FixedUpdate"), "Should contain FixedUpdate");
            Assert.IsTrue(dump.Contains("PostLateUpdate"), "Should contain PostLateUpdate");
            AesirArchitectureDebug.LogTestInfo(
                "GetCurrentPlayerLoopDescription(默认循环): 输出中包含 Update、FixedUpdate、PostLateUpdate 等核心系统");
        }

        /// <summary>
        /// 验证 <see cref="PlayerLoopUtility.GetCurrentPlayerLoopDescription"/> 在插入自定义系统后能输出该系统名称。
        /// <para>预期：描述字符串中包含被插入系统的类型名。</para>
        /// </summary>
        /// <remarks>
        /// 框架在 <c>Initialize()</c> 中注入的子系统以 <c>AesirArchitecture</c> 前缀命名，
        /// 描述输出会以 <c>[Aesir Architecture]</c> 标签标注。此测试验证注入后的子系统能被描述输出正确反映，
        /// 使开发者能够通过日志确认框架是否已成功接入 PlayerLoop。
        /// </remarks>
        /// <seealso cref="PlayerLoopUtility.GetCurrentPlayerLoopDescription"/>
        /// <seealso cref="PlayerLoopUtility.InsertSystemBefore{TTarget}"/>
        [Test]
        public void GetCurrentPlayerLoopDescription_AfterInsert_ShowsInsertedSystem()
        {
            var markerType = typeof(TestMarkerForDump);
            PlayerLoopUtility.InsertSystemBefore<Update>(new PlayerLoopSystem { type = markerType });

            var dump = PlayerLoopUtility.GetCurrentPlayerLoopDescription();

            Assert.IsTrue(dump.Contains(markerType.Name),
                $"Dump should contain inserted system '{markerType.Name}'");
            AesirArchitectureDebug.LogTestInfo(
                $"GetCurrentPlayerLoopDescription(插入后): 输出中包含插入的系统 '{markerType.Name}'");
        }

        /// <summary>
        /// 验证 <see cref="AesirArchitectureLifecyclePhase.BeforeUpdate"/> 和 <see cref="AesirArchitectureLifecyclePhase.AfterUpdate"/> 两个阶段的注册互不干扰。
        /// <para>预期：两个阶段各自计数为 1，注册到一阶段的回调不出现在另一阶段。</para>
        /// </summary>
        /// <remarks>
        /// 框架使用 <c>Dictionary&lt;AesirArchitectureLifecyclePhase, List&lt;HookEntry&gt;&gt;</c> 按阶段隔离回调。
        /// 如果阶段隔离失败（如键冲突或共享列表），注册到 BeforeUpdate 的回调可能在 AfterUpdate 阶段也被执行，
        /// 导致同一逻辑被重复调用或在不正确的帧阶段执行。
        /// </remarks>
        /// <seealso cref="AesirArchitecturePlayerLoop.Register"/>
        /// <seealso cref="AesirArchitecturePlayerLoop.GetHookCount"/>
        /// <seealso cref="AesirArchitectureLifecyclePhase"/>
        [Test]
        public void Register_BeforeAndAfterUpdate_AreDistinct()
        {
            AesirArchitecturePlayerLoop.Reset();

            AesirArchitecturePlayerLoop.Register(AesirArchitectureLifecyclePhase.BeforeUpdate, () => { });
            AesirArchitecturePlayerLoop.Register(AesirArchitectureLifecyclePhase.AfterUpdate, () => { });

            Assert.AreEqual(1,
                AesirArchitecturePlayerLoop.GetHookCount(AesirArchitectureLifecyclePhase.BeforeUpdate));
            Assert.AreEqual(1,
                AesirArchitecturePlayerLoop.GetHookCount(AesirArchitectureLifecyclePhase.AfterUpdate));
            AesirArchitectureDebug.LogTestInfo("Register(BeforeUpdate/AfterUpdate): 两个阶段注册互不干扰，各自计数为 1");
        }

        /// <summary>
        /// 验证 <see cref="AesirArchitecturePlayerLoop.Reset"/> 清除之前注册的所有回调。
        /// <para>预期：清除后回调计数归零。</para>
        /// </summary>
        /// <remarks>
        /// <c>Reset</c> 在 <c>[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]</c> 中被调用，
        /// 用于在域重载后清理 Disable Domain Reload 模式下残留的静态状态。
        /// 如果清理失败，旧的回调引用将指向已销毁的对象，在帧循环中触发空引用异常或调用已失效的逻辑。
        /// </remarks>
        /// <seealso cref="AesirArchitecturePlayerLoop.Reset"/>
        /// <seealso cref="AesirArchitecturePlayerLoop.Register"/>
        [Test]
        public void Clear_RemovesAllRegisteredHooks()
        {
            AesirArchitecturePlayerLoop.Register(AesirArchitectureLifecyclePhase.BeforeUpdate, () => { });
            Assert.AreEqual(1,
                AesirArchitecturePlayerLoop.GetHookCount(AesirArchitectureLifecyclePhase.BeforeUpdate));

            AesirArchitecturePlayerLoop.Reset();

            Assert.AreEqual(0,
                AesirArchitecturePlayerLoop.GetHookCount(AesirArchitectureLifecyclePhase.BeforeUpdate));
            AesirArchitectureDebug.LogTestInfo("Clear: 成功清除之前注册的所有回调");
        }

        /// <summary>
        /// 验证相同 order 的回调按注册顺序执行，不同 order 按 order 升序排列。
        /// <para>预期：执行顺序为 Order(-1) → Order(0, 按注册序) → Order(1)。</para>
        /// </summary>
        /// <remarks>
        /// 框架使用 <c>InsertionIndex</c> 作为次级排序键实现稳定排序。
        /// 如果排序不稳定（如相同 order 的回调执行顺序随机），将导致依赖执行顺序的逻辑产生不可预期的行为，
        /// 例如后注册的初始化回调先于先注册的执行，造成状态依赖断裂。
        /// 此测试直接调用 <c>OnBeforeUpdate</c>（通过 InternalsVisibleTo 可见）模拟帧触发，
        /// 避免依赖真实 PlayerLoop 帧循环的时间不确定性。
        /// </remarks>
        /// <seealso cref="AesirArchitecturePlayerLoop.Register"/>
        /// <seealso cref="AesirArchitecturePlayerLoop.OnBeforeUpdate"/>
        [Test]
        public void Register_SameOrder_ExecutesInRegistrationOrder()
        {
            var executionOrder = new List<int>();
            AesirArchitecturePlayerLoop.Register(AesirArchitectureLifecyclePhase.BeforeUpdate,
                () => executionOrder.Add(1));
            AesirArchitecturePlayerLoop.Register(AesirArchitectureLifecyclePhase.BeforeUpdate,
                () => executionOrder.Add(2));
            AesirArchitecturePlayerLoop.Register(AesirArchitectureLifecyclePhase.BeforeUpdate,
                () => executionOrder.Add(3), -1);
            AesirArchitecturePlayerLoop.Register(AesirArchitectureLifecyclePhase.BeforeUpdate,
                () => executionOrder.Add(4), 1);
            AesirArchitecturePlayerLoop.Register(AesirArchitectureLifecyclePhase.BeforeUpdate,
                () => executionOrder.Add(5));

            // 直接调用 internal 方法（通过 InternalsVisibleTo），替代反射
            AesirArchitecturePlayerLoop.OnBeforeUpdate();

            // 预期顺序: Order -1 → Order 0(按注册) → Order 1
            var expected = new[] { 3, 1, 2, 5, 4 };
            CollectionAssert.AreEqual(expected, executionOrder);
            AesirArchitectureDebug.LogTestInfo("Register(同order): 同 order 按注册顺序执行，不同 order 按升序排列");
        }

        /// <summary>
        /// 验证 <see cref="AesirArchitecturePlayerLoop.EnsureInjected"/> 在注入点被第三方覆盖后能重新补插缺失的子系统。
        /// <para>预期：注入成功后被模拟覆盖抹掉，调用 <c>EnsureInjected</c> 后两个子系统恢复存在。</para>
        /// </summary>
        /// <remarks>
        /// 第三方 SDK 若使用其缓存的 PlayerLoop 副本调用 <c>PlayerLoop.SetPlayerLoop</c>，会连同框架注入的子系统一起抹掉，
        /// 导致 BeforeUpdate / AfterUpdate 钩子静默失效。<c>EnsureInjected</c> 是框架的自愈入口：
        /// 通过 <c>ContainsSystem</c> 检测后仅补插缺失的子系统，且已存在时不重复插入（幂等）。
        /// <para>
        /// 测试通过根级过滤掉框架两个子系统来模拟第三方覆盖——框架的注入锚点（<c>Update</c> / <c>PostLateUpdate</c>）
        /// 均位于 PlayerLoop 根层级，注入点也在根层级。
        /// </para>
        /// </remarks>
        /// <seealso cref="AesirArchitecturePlayerLoop.EnsureInjected"/>
        [Test]
        public void EnsureInjected_AfterThirdPartyWipe_ReinjectsMissingSystems()
        {
            AesirArchitecturePlayerLoop.EnsureInjected();

            var loop = PlayerLoop.GetCurrentPlayerLoop();
            Assert.IsTrue(LoopContainsNamed(ref loop, BeforeUpdateSystemName), "注入后应包含 BeforeUpdate 子系统");
            Assert.IsTrue(LoopContainsNamed(ref loop, AfterUpdateSystemName), "注入后应包含 AfterUpdate 子系统");

            WipeAesirSystems();

            loop = PlayerLoop.GetCurrentPlayerLoop();
            Assert.IsFalse(LoopContainsNamed(ref loop, BeforeUpdateSystemName), "模拟覆盖后 BeforeUpdate 子系统应缺失");
            Assert.IsFalse(LoopContainsNamed(ref loop, AfterUpdateSystemName), "模拟覆盖后 AfterUpdate 子系统应缺失");

            AesirArchitecturePlayerLoop.EnsureInjected();

            loop = PlayerLoop.GetCurrentPlayerLoop();
            Assert.IsTrue(LoopContainsNamed(ref loop, BeforeUpdateSystemName), "自愈后应重新包含 BeforeUpdate 子系统");
            Assert.IsTrue(LoopContainsNamed(ref loop, AfterUpdateSystemName), "自愈后应重新包含 AfterUpdate 子系统");
            AesirArchitectureDebug.LogTestInfo("EnsureInjected(覆盖自愈): 第三方覆盖后成功补插缺失的注入点");
        }

        /// <summary>
        /// 验证 <see cref="AesirArchitecturePlayerLoop.Register"/> 在注入点被第三方覆盖后触发自愈补插。
        /// <para>预期：覆盖后调用 <c>Register</c>，两个注入点均恢复存在。</para>
        /// </summary>
        /// <remarks>
        /// <c>Register</c> 内部调用 <see cref="AesirArchitecturePlayerLoop.EnsureInjected"/>（注册即自愈），
        /// 覆盖"覆盖发生后、周期性检测到来之前"的时间窗口内新注册回调的场景。
        /// </remarks>
        /// <seealso cref="AesirArchitecturePlayerLoop.Register"/>
        [Test]
        public void Register_AfterThirdPartyWipe_HealsInjection()
        {
            AesirArchitecturePlayerLoop.EnsureInjected();
            WipeAesirSystems();

            AesirArchitecturePlayerLoop.Register(AesirArchitectureLifecyclePhase.BeforeUpdate, () => { });

            var loop = PlayerLoop.GetCurrentPlayerLoop();
            Assert.IsTrue(LoopContainsNamed(ref loop, BeforeUpdateSystemName),
                "Register 应触发自愈补插 BeforeUpdate 子系统");
            Assert.IsTrue(LoopContainsNamed(ref loop, AfterUpdateSystemName),
                "Register 应触发自愈补插 AfterUpdate 子系统");
            AesirArchitectureDebug.LogTestInfo("Register(覆盖自愈): 注册回调时成功补插缺失的注入点");
        }

        /// <summary>
        /// 模拟第三方 SDK 覆盖：从当前 PlayerLoop 根层级过滤掉框架注入的两个子系统后写回
        /// </summary>
        /// <remarks>
        /// 等价于第三方用"不含框架子系统的缓存副本"调用 <c>PlayerLoop.SetPlayerLoop</c>。
        /// 框架注入点位于根层级（插在根级 <c>Update</c> 之前、根级 <c>PostLateUpdate</c> 之后），根级过滤即可完整移除。
        /// </remarks>
        static void WipeAesirSystems()
        {
            var wiped = PlayerLoop.GetCurrentPlayerLoop();
            wiped.subSystemList = Array.FindAll(wiped.subSystemList,
                s => s.type?.Name != BeforeUpdateSystemName && s.type?.Name != AfterUpdateSystemName);
            PlayerLoop.SetPlayerLoop(wiped);
        }

        /// <summary>
        /// 递归遍历 PlayerLoop 子系统树，按类型名判断是否存在指定子系统
        /// </summary>
        /// <remarks>
        /// 框架注入的子系统类型为私有嵌套结构体，测试无法直接引用，故以 <c>type.Name</c> 字符串匹配。
        /// </remarks>
        static bool LoopContainsNamed(ref PlayerLoopSystem system, string typeName)
        {
            if (system.type?.Name == typeName)
            {
                return true;
            }

            if (system.subSystemList == null)
            {
                return false;
            }

            for (var i = 0; i < system.subSystemList.Length; i++)
            {
                if (LoopContainsNamed(ref system.subSystemList[i], typeName))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 断言 <paramref name="markerType"/> 在 PlayerLoop 中与 <paramref name="targetType"/> 处于同一父级，且排在 targetType 之前
        /// </summary>
        /// <remarks>
        /// 此方法通过递归遍历 <c>PlayerLoopSystem.subSystemList</c> 查找两个类型所在的兄弟索引，
        /// 仅当二者处于同一父级时才比较前后关系，确保插入位置断言的准确性。
        /// </remarks>
        /// <param name="markerType">被插入的自定义系统类型标识</param>
        /// <param name="targetType">目标系统类型标识，markerType 应排在其之前</param>
        static void AssertIsBeforeInLoop(Type markerType, Type targetType)
        {
            var loop = PlayerLoop.GetCurrentPlayerLoop();
            var found = TryFindSiblingIndices(ref loop, markerType, targetType, out var markerIdx,
                out var targetIdx);
            Assert.IsTrue(found, $"{markerType.Name} and {targetType.Name} should be siblings in the loop");
            Assert.Less(markerIdx, targetIdx, $"{markerType.Name} should be before {targetType.Name}");
        }

        /// <summary>
        /// 断言 <paramref name="markerType"/> 在 PlayerLoop 中与 <paramref name="targetType"/> 处于同一父级，且排在 targetType 之后
        /// </summary>
        /// <remarks>
        /// 此方法通过递归遍历 <c>PlayerLoopSystem.subSystemList</c> 查找两个类型所在的兄弟索引，
        /// 仅当二者处于同一父级时才比较前后关系，确保插入位置断言的准确性。
        /// </remarks>
        /// <param name="markerType">被插入的自定义系统类型标识</param>
        /// <param name="targetType">目标系统类型标识，markerType 应排在其之后</param>
        static void AssertIsAfterInLoop(Type markerType, Type targetType)
        {
            var loop = PlayerLoop.GetCurrentPlayerLoop();
            var found = TryFindSiblingIndices(ref loop, markerType, targetType, out var markerIdx,
                out var targetIdx);
            Assert.IsTrue(found, $"{markerType.Name} and {targetType.Name} should be siblings in the loop");
            Assert.Greater(markerIdx, targetIdx, $"{markerType.Name} should be after {targetType.Name}");
        }

        /// <summary>
        /// 递归遍历 PlayerLoop 子系统树，查找两个指定类型在同一层级中的兄弟索引位置
        /// </summary>
        /// <remarks>
        /// PlayerLoop 是树形结构，仅当两个类型处于同一父级的 <c>subSystemList</c> 中时，
        /// 其数组索引才能反映真实的执行先后顺序。此方法先在当前层级查找，
        /// 未找到则递归进入子节点继续搜索。
        /// </remarks>
        /// <param name="system">当前遍历到的 PlayerLoopSystem 节点</param>
        /// <param name="typeA">要查找的第一个类型</param>
        /// <param name="typeB">要查找的第二个类型</param>
        /// <param name="indexA">输出：typeA 在兄弟数组中的索引，未找到则为 -1</param>
        /// <param name="indexB">输出：typeB 在兄弟数组中的索引，未找到则为 -1</param>
        /// <returns>是否在同一层级中同时找到两个类型</returns>
        static bool TryFindSiblingIndices(ref PlayerLoopSystem system,
            Type typeA,
            Type typeB,
            out int indexA,
            out int indexB)
        {
            indexA = -1;
            indexB = -1;

            if (system.subSystemList == null)
            {
                return false;
            }

            for (var i = 0; i < system.subSystemList.Length; i++)
            {
                if (system.subSystemList[i].type == typeA)
                {
                    indexA = i;
                }

                if (system.subSystemList[i].type == typeB)
                {
                    indexB = i;
                }
            }

            if (indexA >= 0 && indexB >= 0)
            {
                return true;
            }

            for (var i = 0; i < system.subSystemList.Length; i++)
            {
                if (TryFindSiblingIndices(ref system.subSystemList[i], typeA, typeB, out indexA, out indexB))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 递归遍历 PlayerLoop 子系统树，判断是否包含指定类型的子系统
        /// </summary>
        /// <remarks>
        /// 用于验证插入操作是否成功：插入后通过此方法确认目标类型已存在于 PlayerLoop 中。
        /// 采用深度优先遍历，因为 PlayerLoop 的层级较浅（通常 2-3 层），递归开销可忽略。
        /// </remarks>
        /// <param name="system">当前遍历到的 PlayerLoopSystem 节点</param>
        /// <param name="targetType">要查找的目标类型</param>
        /// <returns>是否在 PlayerLoop 中找到目标类型</returns>
        static bool ContainsType(ref PlayerLoopSystem system, Type targetType)
        {
            if (system.type == targetType)
            {
                return true;
            }

            if (system.subSystemList == null)
            {
                return false;
            }

            for (var i = 0; i < system.subSystemList.Length; i++)
            {
                if (ContainsType(ref system.subSystemList[i], targetType))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 测试用空结构体，作为 <c>PlayerLoopSystem.type</c> 的类型标识，验证 InsertSystemBefore 在 Update 之前的插入
        /// </summary>
        struct TestMarkerBeforeUpdate { }

        /// <summary>
        /// 测试用空结构体，作为 <c>PlayerLoopSystem.type</c> 的类型标识，验证 InsertSystemAfter 在 FixedUpdate 之后的插入
        /// </summary>
        struct TestMarkerAfterFixedUpdate { }

        /// <summary>
        /// 测试用空结构体，作为 <c>PlayerLoopSystem.type</c> 的类型标识，验证连续两次插入的第一个系统
        /// </summary>
        struct TestMarkerTwice1 { }

        /// <summary>
        /// 测试用空结构体，作为 <c>PlayerLoopSystem.type</c> 的类型标识，验证连续两次插入的第二个系统
        /// </summary>
        struct TestMarkerTwice2 { }

        /// <summary>
        /// 测试用空结构体，作为 <c>PlayerLoopSystem.type</c> 的类型标识，验证描述输出中是否包含已插入的系统名称
        /// </summary>
        struct TestMarkerForDump { }
    }
}
