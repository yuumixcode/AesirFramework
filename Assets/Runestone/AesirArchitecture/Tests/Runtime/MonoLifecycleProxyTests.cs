using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Runestone.AesirArchitecture.Tests
{
    /// <summary>
    /// 验证 <see cref="MonoLifecycleProxy"/> 的订阅、排序和取消订阅行为。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 测试使用 <c>[UnityTest]</c> 在 PlayMode 下运行，因为 <see cref="MonoLifecycleProxy"/> 依赖
    /// Unity 生命周期回调（Update / FixedUpdate / LateUpdate）和 PlayerLoop 注入来触发事件，
    /// 纯 EditMode 无法驱动这些回调。
    /// </para>
    /// <para>
    /// 测试使用 <see cref="MonoLifecycleProxy.Instance"/> 全局单例，
    /// 每个测试在结束时移除所有已注册的监听者，确保测试间隔离。
    /// </para>
    /// <para>
    /// 测试覆盖三个维度：
    /// <list type="number">
    /// <item><b>订阅</b>：注册回调后，对应生命周期事件触发时回调被执行。</item>
    /// <item><b>排序</b>：多个回调按 order 升序执行，同 order 按注册顺序执行。</item>
    /// <item><b>取消订阅</b>：注销后回调不再被触发。</item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <seealso cref="MonoLifecycleProxy"/>
    /// <seealso cref="MonoLifecycleEvent"/>
    public class MonoLifecycleProxyTests
    {
        MonoLifecycleProxy _proxy;
        readonly List<string> _log = new List<string>();

        /// <summary>
        /// 获取全局单例并清空日志
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            _log.Clear();
            _proxy = MonoLifecycleProxy.Instance;
        }

        /// <summary>
        /// 清空所有监听者，确保测试间隔离
        /// </summary>
        [TearDown]
        public void TearDown()
        {
            _proxy?.ClearAllListeners();
            _log.Clear();
        }

        /// <summary>
        /// 验证订阅 Update 事件后，回调在 Update 阶段被触发，注销后不再触发。
        /// </summary>
        /// <remarks>
        /// 此测试同时覆盖订阅和取消订阅两个方面：
        /// <list type="number">
        /// <item>注册回调后等待一帧，断言回调被触发（订阅验证）。</item>
        /// <item>注销回调后等待一帧，断言回调不再触发（取消订阅验证）。</item>
        /// </list>
        /// </remarks>
        /// <seealso cref="MonoLifecycleProxy.AddListener"/>
        /// <seealso cref="MonoLifecycleProxy.RemoveListener"/>
        [UnityTest]
        public IEnumerator AddListener_Update_CallbackInvokedThenRemoved()
        {
            var called = false;

            _proxy.AddListener(MonoLifecycleEvent.Update, Callback);

            // 等待一帧，让 Update 触发
            yield return null;

            Assert.IsTrue(called, "订阅后回调应被触发");
            Assert.AreEqual(1, _log.Count, "回调应只被触发一次");
            Assert.AreEqual("Update callback invoked", _log[0]);
            AesirArchitectureDebug.LogTestInfo("订阅验证: Update 回调在帧中被成功触发");

            // 取消订阅
            called = false;
            _proxy.RemoveListener(MonoLifecycleEvent.Update, Callback);
            _log.Clear();

            // 等待一帧，确认回调不再触发
            yield return null;

            Assert.IsFalse(called, "注销后回调不应再被触发");
            Assert.AreEqual(0, _log.Count, "注销后不应有任何日志");
            AesirArchitectureDebug.LogTestInfo("取消订阅验证: 注销后回调不再被触发");
            yield break;

            void Callback()
            {
                called = true;
                _log.Add("Update callback invoked");
            }
        }

        /// <summary>
        /// 验证订阅 FixedUpdate 事件后，回调在 FixedUpdate 阶段被触发。
        /// </summary>
        /// <remarks>
        /// FixedUpdate 在固定时间步长执行，可能在一帧内触发多次或零次。
        /// 此测试等待足够长的帧数确保至少触发一次物理帧。
        /// </remarks>
        /// <seealso cref="MonoLifecycleProxy.AddListener"/>
        [UnityTest]
        public IEnumerator AddListener_FixedUpdate_CallbackInvoked()
        {
            var called = false;
            void Callback()
            {
                called = true;
                _log.Add("FixedUpdate callback invoked");
            }

            _proxy.AddListener(MonoLifecycleEvent.FixedUpdate, Callback);

            // 等待物理帧（Time.fixedDeltaTime 默认 0.02s，需要等待足够帧数）
            yield return new WaitForFixedUpdate();
            yield return null;

            _proxy.RemoveListener(MonoLifecycleEvent.FixedUpdate, Callback);

            Assert.IsTrue(called, "FixedUpdate 回调应被触发");
            Assert.GreaterOrEqual(_log.Count, 1, "至少应有一条日志");
            AesirArchitectureDebug.LogTestInfo($"订阅验证: FixedUpdate 回调被触发 {_log.Count} 次");
        }

        /// <summary>
        /// 验证订阅 LateUpdate 事件后，回调在 LateUpdate 阶段被触发，注销后不再触发。
        /// </summary>
        /// <seealso cref="MonoLifecycleProxy.AddListener"/>
        /// <seealso cref="MonoLifecycleProxy.RemoveListener"/>
        [UnityTest]
        public IEnumerator AddListener_LateUpdate_CallbackInvokedThenRemoved()
        {
            var called = false;
            void Callback()
            {
                called = true;
                _log.Add("LateUpdate callback invoked");
            }

            _proxy.AddListener(MonoLifecycleEvent.LateUpdate, Callback);

            // 等待一帧（LateUpdate 在 Update 之后执行）
            yield return null;

            Assert.IsTrue(called, "订阅后 LateUpdate 回调应被触发");
            Assert.AreEqual(1, _log.Count, "回调应只被触发一次");
            AesirArchitectureDebug.LogTestInfo("订阅验证: LateUpdate 回调在帧中被成功触发");

            // 取消订阅
            called = false;
            _log.Clear();
            _proxy.RemoveListener(MonoLifecycleEvent.LateUpdate, Callback);

            yield return null;

            Assert.IsFalse(called, "注销后回调不应再被触发");
            Assert.AreEqual(0, _log.Count, "注销后不应有任何日志");
            AesirArchitectureDebug.LogTestInfo("取消订阅验证: 注销后 LateUpdate 回调不再被触发");
        }

        /// <summary>
        /// 验证多个回调按 order 升序执行，相同 order 的回调按注册顺序执行。
        /// </summary>
        /// <remarks>
        /// 注册三个回调到 Update 事件，order 分别为 2、-1、1。
        /// 预期执行顺序：order=-1 → order=1 → order=2。
        /// 此测试覆盖排序验证。
        /// </remarks>
        /// <seealso cref="MonoLifecycleProxy.AddListener"/>
        [UnityTest]
        public IEnumerator AddListener_MultipleCallbacks_ExecutedByOrder()
        {
            _proxy.AddListener(MonoLifecycleEvent.Update, () => _log.Add("C"), 2);
            _proxy.AddListener(MonoLifecycleEvent.Update, () => _log.Add("A"), -1);
            _proxy.AddListener(MonoLifecycleEvent.Update, () => _log.Add("B"), 1);

            yield return null;

            // 预期顺序: A(order=-1) → B(order=1) → C(order=2)
            Assert.AreEqual(3, _log.Count, "三个回调都应被触发");
            Assert.AreEqual("A", _log[0], "order=-1 的回调应最先执行");
            Assert.AreEqual("B", _log[1], "order=1 的回调应第二个执行");
            Assert.AreEqual("C", _log[2], "order=2 的回调应最后执行");
            AesirArchitectureDebug.LogTestInfo(
                "排序验证: 回调按 order 升序执行 — A(-1) → B(1) → C(2)");
        }

        /// <summary>
        /// 验证相同 order 的回调按注册顺序执行（稳定排序）。
        /// </summary>
        /// <remarks>
        /// 注册三个回调到 Update 事件，order 均为 0。
        /// 预期执行顺序按注册顺序：First → Second → Third。
        /// <see cref="MonoLifecycleProxy"/> 使用 InsertionIndex 作为次级排序键实现稳定排序。
        /// </remarks>
        /// <seealso cref="MonoLifecycleProxy.AddListener"/>
        [UnityTest]
        public IEnumerator AddListener_SameOrder_ExecutedInRegistrationOrder()
        {
            _proxy.AddListener(MonoLifecycleEvent.Update, () => _log.Add("First"), 0);
            _proxy.AddListener(MonoLifecycleEvent.Update, () => _log.Add("Second"), 0);
            _proxy.AddListener(MonoLifecycleEvent.Update, () => _log.Add("Third"), 0);

            yield return null;

            Assert.AreEqual(3, _log.Count, "三个回调都应被触发");
            Assert.AreEqual("First", _log[0], "同 order 的回调应按注册顺序执行");
            Assert.AreEqual("Second", _log[1], "同 order 的回调应按注册顺序执行");
            Assert.AreEqual("Third", _log[2], "同 order 的回调应按注册顺序执行");
            AesirArchitectureDebug.LogTestInfo(
                "稳定排序验证: 相同 order 的回调按注册顺序执行 — First → Second → Third");
        }

        /// <summary>
        /// 验证 <see cref="AutoRemoveListenerHandle"/> Dispose 后回调不再被触发。
        /// </summary>
        /// <remarks>
        /// <see cref="MonoLifecycleProxy.AddListener"/> 返回 <see cref="AutoRemoveListenerHandle"/>，
        /// 调用 <see cref="AutoRemoveListenerHandle.Dispose"/> 应等效于 <see cref="MonoLifecycleProxy.RemoveListener"/>。
        /// 此测试覆盖通过句柄取消订阅的路径。
        /// </remarks>
        /// <seealso cref="AutoRemoveListenerHandle"/>
        [UnityTest]
        public IEnumerator AddListener_HandleDispose_CallbackRemoved()
        {
            var called = false;
            void Callback()
            {
                called = true;
                _log.Add("Handle callback invoked");
            }

            var handle = _proxy.AddListener(MonoLifecycleEvent.Update, Callback);

            yield return null;

            Assert.IsTrue(called, "订阅后回调应被触发");
            AesirArchitectureDebug.LogTestInfo("订阅验证: 通过 AddListener 返回的句柄注册成功");

            // 通过句柄取消订阅
            called = false;
            _log.Clear();
            handle.Dispose();

            yield return null;

            Assert.IsFalse(called, "句柄 Dispose 后回调不应再被触发");
            Assert.AreEqual(0, _log.Count, "句柄 Dispose 后不应有任何日志");
            AesirArchitectureDebug.LogTestInfo("取消订阅验证: AutoRemoveListenerHandle.Dispose 后回调不再被触发");
        }

        /// <summary>
        /// 验证 GetListenerCount 在订阅和取消订阅后正确反映监听者数量。
        /// </summary>
        /// <seealso cref="MonoLifecycleProxy.GetListenerCount"/>
        [UnityTest]
        public IEnumerator GetListenerCount_AddAndRemove_ReturnsCorrectCount()
        {
            Assert.AreEqual(0, _proxy.GetListenerCount(MonoLifecycleEvent.Update),
                "初始状态下不应有监听者");

            void Callback() => _log.Add("invoked");

            _proxy.AddListener(MonoLifecycleEvent.Update, Callback);
            Assert.AreEqual(1, _proxy.GetListenerCount(MonoLifecycleEvent.Update),
                "订阅后监听者数量应为 1");

            _proxy.AddListener(MonoLifecycleEvent.Update, () => _log.Add("other"));
            Assert.AreEqual(2, _proxy.GetListenerCount(MonoLifecycleEvent.Update),
                "第二个订阅后监听者数量应为 2");

            _proxy.RemoveListener(MonoLifecycleEvent.Update, Callback);
            Assert.AreEqual(1, _proxy.GetListenerCount(MonoLifecycleEvent.Update),
                "取消一个订阅后监听者数量应为 1");

            AesirArchitectureDebug.LogTestInfo(
                "监听者数量验证: 订阅增加计数，取消订阅减少计数，数量正确");

            yield return null;
        }

        /// <summary>
        /// 验证所有帧级生命周期事件按 Unity 执行顺序依次触发。
        /// </summary>
        /// <remarks>
        /// 订阅全部 5 个帧级事件（FixedUpdate → BeforeUpdate → Update → LateUpdate → AfterUpdate），
        /// 等待足够帧数捕获至少一个包含 FixedUpdate 的完整帧，
        /// 在日志中查找符合预期顺序的连续事件块。
        /// <para>
        /// FixedUpdate 按固定时间步长执行，编辑器帧率较高时可能间隔多帧才触发一次，
        /// 因此等待最多 30 帧确保至少捕获一个物理帧。
        /// </para>
        /// <para>
        /// 不包含 OnApplicationFocus / OnApplicationPause / OnApplicationQuit —
        /// 这三个应用级事件无法在 PlayMode 测试中可靠触发。
        /// </para>
        /// </remarks>
        /// <seealso cref="MonoLifecycleProxy.AddListener"/>
        /// <seealso cref="MonoLifecycleEvent"/>
        [UnityTest]
        public IEnumerator AllFrameEvents_ExecutedInCorrectOrder()
        {
            _proxy.AddListener(MonoLifecycleEvent.FixedUpdate, () => _log.Add("FixedUpdate"));
            _proxy.AddListener(MonoLifecycleEvent.BeforeUpdate, () => _log.Add("BeforeUpdate"));
            _proxy.AddListener(MonoLifecycleEvent.Update, () => _log.Add("Update"));
            _proxy.AddListener(MonoLifecycleEvent.LateUpdate, () => _log.Add("LateUpdate"));
            _proxy.AddListener(MonoLifecycleEvent.AfterUpdate, () => _log.Add("AfterUpdate"));

            // 等待两帧让残余事件触发，然后清空日志
            yield return null;
            yield return null;
            _log.Clear();

            string[] expected =
            {
                "FixedUpdate",
                "BeforeUpdate", "Update",
                "LateUpdate", "AfterUpdate"
            };

            // 等待最多 30 帧捕获至少一个包含 FixedUpdate 的完整帧
            var found = false;
            for (var i = 0; i < 30; i++)
            {
                yield return null;

                if (_log.Count < expected.Length)
                    continue;

                for (var j = 0; j <= _log.Count - expected.Length; j++)
                {
                    var match = true;
                    for (var k = 0; k < expected.Length; k++)
                    {
                        if (_log[j + k] != expected[k])
                        {
                            match = false;
                            break;
                        }
                    }

                    if (!match)
                        continue;
                    found = true;
                    break;
                }

                if (found)
                    break;
            }

            Assert.IsTrue(found,
                $"未找到符合预期顺序的连续事件块。预期: {string.Join(" → ", expected)}。" +
                $"日志: [{string.Join(", ", _log)}]");

            AesirArchitectureDebug.LogTestInfo(
                "全生命周期顺序验证: FixedUpdate → BeforeUpdate → Update → LateUpdate → AfterUpdate");
        }
    }
}
