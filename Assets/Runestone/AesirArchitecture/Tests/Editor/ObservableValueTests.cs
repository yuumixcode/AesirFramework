using System.Collections.Generic;
using NUnit.Framework;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    /// <summary>
    /// 验证 <see cref="ObservableValue{T}" /> 的值比较、通知触发与静默设置行为。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     ObservableValue 是 Model 层向 View 层暴露只读订阅的响应式属性载体，
    ///     值变化时才触发通知（EqualityComparer&lt;T&gt;.Default 比较）是其核心契约。
    ///     </para>
    ///     <para>纯 C# 逻辑，EditMode 即可运行。</para>
    /// </remarks>
    /// <seealso cref="ObservableValue{T}" />
    public class ObservableValueTests
    {
        /// <summary>
        /// 验证默认构造使用类型默认值，带参构造使用指定初始值。
        /// </summary>
        [Test]
        public void Constructor_DefaultAndInitialValue()
        {
            var d = new ObservableValue<int>();
            var i = new ObservableValue<int>(7);

            Assert.AreEqual(0, d.Value, "默认构造应为类型默认值");
            Assert.AreEqual(7, i.Value, "带参构造应为指定初始值");
            AesirArchitectureDebug.LogTestInfo("构造: 默认值与初始值正确");
        }

        /// <summary>
        /// 验证赋相同值不触发通知，赋不同值触发一次通知且参数为新值。
        /// </summary>
        [Test]
        public void Value_SameValue_NoNotification_DifferentValue_NotifiesWithNewValue()
        {
            var observable = new ObservableValue<int>(1);
            var received = new List<int>();

            observable.AddListener(received.Add);

            observable.Value = 1;
            Assert.AreEqual(0, received.Count, "赋相同值不应触发通知");

            observable.Value = 5;
            Assert.AreEqual(1, received.Count, "赋不同值应触发一次通知");
            Assert.AreEqual(5, received[0], "通知参数应为新值");
            Assert.AreEqual(5, observable.Value, "新值应已写入");
            AesirArchitectureDebug.LogTestInfo("值比较: 相同值不通知，不同值通知且参数为新值");
        }

        /// <summary>
        /// 验证 SetValueSilently 更新值但不触发通知。
        /// </summary>
        [Test]
        public void SetValueSilently_UpdatesWithoutNotification()
        {
            var observable = new ObservableValue<int>(1);
            var received = new List<int>();

            observable.AddListener(received.Add);

            observable.SetValueSilently(9);

            Assert.AreEqual(9, observable.Value, "值应已更新");
            Assert.AreEqual(0, received.Count, "静默设置不应触发通知");
            AesirArchitectureDebug.LogTestInfo("SetValueSilently: 更新值但不通知");
        }

        /// <summary>
        /// 验证 AddListenerAndInvoke 添加监听后立即以当前值触发一次。
        /// </summary>
        [Test]
        public void AddListenerAndInvoke_FiresImmediatelyWithCurrentValue()
        {
            var observable = new ObservableValue<string>("init");
            var received = new List<string>();

            observable.AddListenerAndInvoke(received.Add);

            Assert.AreEqual(1, received.Count, "注册时应立即触发一次");
            Assert.AreEqual("init", received[0], "立即触发的参数应为当前值");
            AesirArchitectureDebug.LogTestInfo("AddListenerAndInvoke: 注册即同步当前状态");
        }

        /// <summary>
        /// 验证 InvokeEvent 强制触发通知（值未变也触发）。
        /// </summary>
        [Test]
        public void InvokeEvent_ForcesNotification()
        {
            var observable = new ObservableValue<int>(3);
            var received = new List<int>();

            observable.AddListener(received.Add);

            observable.InvokeEvent();

            Assert.AreEqual(1, received.Count, "值未变也应强制触发通知");
            Assert.AreEqual(3, received[0]);
            AesirArchitectureDebug.LogTestInfo("InvokeEvent: 强制刷新订阅方状态");
        }

        /// <summary>
        /// 验证 RemoveListener 与 Clear 后不再收到通知。
        /// </summary>
        [Test]
        public void RemoveListener_And_Clear_StopNotifications()
        {
            var observable = new ObservableValue<int>(0);
            var count = 0;

            void Callback(int _)
            {
                count++;
            }

            var handle = observable.AddListener(Callback);
            observable.Value = 1;
            Assert.AreEqual(1, count, "移除前应正常收到通知");

            handle.Dispose();
            observable.Value = 2;
            Assert.AreEqual(1, count, "句柄 Dispose 后不应再收到通知");

            observable.AddListener(Callback);
            observable.Clear();
            observable.Value = 3;
            Assert.AreEqual(1, count, "Clear 清空所有监听后不应再收到通知");
            AesirArchitectureDebug.LogTestInfo("RemoveListener/Clear: 正确停止通知");
        }

        /// <summary>
        /// 验证引用类型的值比较：赋同一引用不触发通知，赋新引用触发。
        /// </summary>
        [Test]
        public void Value_ReferenceType_UsesReferenceEquality()
        {
            var first = new object();
            var second = new object();
            var observable = new ObservableValue<object>(first);
            var received = new List<object>();

            observable.AddListener(received.Add);

            observable.Value = first;
            Assert.AreEqual(0, received.Count, "赋同一引用不应触发通知");

            observable.Value = second;
            Assert.AreEqual(1, received.Count, "赋新引用应触发通知");
            Assert.AreSame(second, received[0]);
            AesirArchitectureDebug.LogTestInfo("引用类型值比较: 按引用相等判断");
        }
    }
}
