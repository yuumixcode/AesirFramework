using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    /// <summary>
    /// 验证 <see cref="MiniEvent" />、<see cref="MiniEvent{T}" /> 的原生事件语义与监听句柄行为。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     极简约定（2026-08-15 裁决）：事件调用为直接多播调用，零分配；
    ///     异常语义与原生 C# 事件一致——某个监听者抛出异常会中断后续监听者的执行并向上传播（fail-fast），
    ///     监听回调不应抛异常属框架约定，业务异常应在回调内部自行处理。
    ///     本测试锁定该语义，防止未来误加异常吞噬或快照分配。
    ///     </para>
    ///     <para>
    ///     <see cref="ObservableValue{T}" /> 内部经 <see cref="MiniEvent{T}" /> 派发通知，
    ///     语义随 MiniEvent 一致，其专项行为由 <c>ObservableValueTests</c> 覆盖。
    ///     </para>
    ///     <para>纯 C# 逻辑，EditMode 即可运行。</para>
    /// </remarks>
    /// <seealso cref="MiniEvent" />
    /// <seealso cref="AutoRemoveListenerHandle" />
    public class MiniEventTests
    {
        /// <summary>
        /// 验证无参事件按注册顺序通知所有监听者，句柄 Dispose 后不再收到通知。
        /// </summary>
        [Test]
        public void MiniEvent_Invoke_NotifiesAllListenersInOrder_HandleDisposeRemoves()
        {
            var evt = new MiniEvent();
            var order = new List<string>();

            var h1 = evt.AddListener(() => order.Add("first"));
            evt.AddListener(() => order.Add("second"));

            evt.Invoke();
            CollectionAssert.AreEqual(new[] { "first", "second" }, order, "两个监听者应按注册顺序被通知");

            h1.Dispose();
            order.Clear();

            evt.Invoke();
            CollectionAssert.AreEqual(new[] { "second" }, order, "句柄 Dispose 后对应监听者应被移除");
            AesirArchitectureDebug.LogTestInfo("MiniEvent: 按注册顺序通知，句柄 Dispose 正确移除监听者");
        }

        /// <summary>
        /// 验证单参事件将参数传递给所有监听者。
        /// </summary>
        [Test]
        public void MiniEventGeneric_Invoke_PassesArgumentToAllListeners()
        {
            var evt = new MiniEvent<int>();
            var received = new List<int>();

            evt.AddListener(received.Add);
            evt.AddListener(received.Add);

            evt.Invoke(42);

            CollectionAssert.AreEqual(new[] { 42, 42 }, received, "两个监听者都应收到同一参数");
            AesirArchitectureDebug.LogTestInfo("MiniEvent<T>: 参数正确传递给所有监听者");
        }

        /// <summary>
        /// 验证监听者抛出异常时中断同事件后续监听者并向上传播（原生 C# 事件语义，fail-fast）。
        /// </summary>
        /// <remarks>
        /// 此为极简裁决的锁定测试：框架不吞监听者异常。
        /// </remarks>
        [Test]
        public void MiniEvent_Invoke_ThrowingListener_InterruptsSubsequentAndPropagates()
        {
            var evt = new MiniEvent();
            var secondInvoked = false;

            evt.AddListener(() => throw new InvalidOperationException("boom"));
            evt.AddListener(() => secondInvoked = true);

            Assert.Throws<InvalidOperationException>(() => evt.Invoke(), "监听者异常应从 Invoke 向上传播（fail-fast）");
            Assert.IsFalse(secondInvoked, "首个监听者抛异常后，后续监听者不应被执行（原生多播语义）");
            AesirArchitectureDebug.LogTestInfo("MiniEvent 异常语义: 监听者异常中断后续并向上传播（fail-fast）");
        }

        /// <summary>
        /// 验证句柄重复 Dispose 安全，不会重复移除或抛异常。
        /// </summary>
        [Test]
        public void AutoRemoveListenerHandle_DisposeTwice_IsSafe()
        {
            var evt = new MiniEvent();
            var count = 0;

            var handle = evt.AddListener(() => count++);
            handle.Dispose();
            handle.Dispose();

            evt.Invoke();
            Assert.AreEqual(0, count, "重复 Dispose 后监听者已被移除且不产生副作用");
            AesirArchitectureDebug.LogTestInfo("AutoRemoveListenerHandle: 重复 Dispose 安全");
        }

        /// <summary>
        /// 验证 Dispose 清空所有监听引用。
        /// </summary>
        [Test]
        public void MiniEvent_Dispose_RemovesAllListeners()
        {
            var evt = new MiniEvent();
            var count = 0;

            evt.AddListener(() => count++);
            evt.AddListener(() => count++);
            evt.Dispose();

            evt.Invoke();
            Assert.AreEqual(0, count, "Dispose 后所有监听者不再被通知");
            Assert.AreEqual(0, evt.GetListeners().Length, "Dispose 后监听者列表应为空");
            AesirArchitectureDebug.LogTestInfo("MiniEvent Dispose: 清空所有监听引用");
        }
    }
}
