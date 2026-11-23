using System;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 绑定信息基类：Script 订阅的公共部分（订阅者、优先级、事件类型与诊断成员名）。
    /// <para>
    /// internal 实现细节：订阅/退订必须经 <see cref="EventModule" /> 公开 API 走同一套注册表与死引用清理，
    /// 包外不可见；包内测试经 <c>InternalsVisibleTo</c> 访问。
    /// 注册表是异构列表（不同事件类型的绑定共存），由本基类提供统一的 <see cref="Invoke" /> 入口。
    /// </para>
    /// </summary>
    internal abstract class BindingInfo
    {
        protected BindingInfo(object subscriber,
            SubscriberPriority priority,
            Type eventType,
            string memberName)
        {
            Subscriber = subscriber;
            Priority = priority;
            EventType = eventType;
            MemberName = memberName;
        }

        /// <summary>订阅者对象。供死引用清理与按对象退订使用。</summary>
        public object Subscriber { get; }

        /// <summary>订阅优先级档位。</summary>
        public SubscriberPriority Priority { get; }

        /// <summary>本绑定监听的事件参数类型，同时是注册表键。</summary>
        public Type EventType { get; }

        /// <summary>委托方法名，供监视面板展示（Lambda 通常显示为编译器生成的名字）。</summary>
        public string MemberName { get; }

        /// <summary>
        /// 调用订阅委托。参数类型由注册表键保证（分发按运行期事件类型精确匹配），子类直接强转。
        /// </summary>
        public abstract void Invoke(AesirEventArgs args);
    }

    /// <summary>
    /// Script 订阅绑定。持有 <see cref="Action{T}" /> 委托，注册后直接调用——
    /// 无反射、无表达式树、无参数数组，分发热路径只是一次虚调用加一次委托调用。
    /// </summary>
    /// <remarks>
    /// 非泛型订阅（按参数实例推断类型）复用 <c>ScriptBindingInfo&lt;AesirEventArgs&gt;</c>
    /// 并显式传入运行期事件类型作为注册表键——键已保证分发参数的实际类型，
    /// 基类回调可直接接收，无需反射适配。
    /// </remarks>
    internal sealed class ScriptBindingInfo<TEventArgs> : BindingInfo where TEventArgs : AesirEventArgs
    {
        readonly Action<TEventArgs> _callback;

        public ScriptBindingInfo(Action<TEventArgs> callback,
            SubscriberPriority priority,
            object subscriber,
            Type eventType) : base(subscriber, priority, eventType, callback.Method.Name) =>
            _callback = callback;

        public override void Invoke(AesirEventArgs args) => _callback((TEventArgs)args);
    }
}
