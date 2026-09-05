using System;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 绑定信息基类。Attribute 订阅与 Script 订阅的共同部分。
    /// </summary>
    public abstract class BindingInfo
    {
        public string BindingKey { get; protected set; }
        public object Subscriber { get; protected set; }
        public SubscriberPriority Priority { get; protected set; }

        public abstract void Invoke(object[] args = null);
    }

    /// <summary>
    /// Attribute 订阅绑定信息。
    /// <para>
    /// 在注册时（冷路径）通过表达式树将 <see cref="MethodInfo" /> 编译为
    /// <see cref="Action" />（object target, object[] args）委托，
    /// 分发时（热路径）直接委托调用，避免每次反射。
    /// </para>
    /// </summary>
    public sealed class StaticBindingInfo : BindingInfo
    {
        /// <summary>
        /// 编译后的委托。参数 0 = 订阅者实例，参数 1 = 参数数组。
        /// </summary>
        readonly Action<object, object[]> _compiledInvoke;

        public StaticBindingInfo(string bindingKey,
            MethodInfo method,
            object subscriber,
            SubscriberPriority priority)
        {
            BindingKey = bindingKey;
            Subscriber = subscriber;
            Priority = priority;
            Method = method;
            _compiledInvoke = CompileMethod(method);
        }

        /// <summary>
        /// 原始方法信息。仅用于去重判断 <see cref="IsAlreadyBound" />。
        /// 不参与分发调用。
        /// </summary>
        public MethodInfo Method { get; }

        public override void Invoke(object[] args = null) => _compiledInvoke(Subscriber, args);

        /// <summary>
        /// 将 <see cref="MethodInfo" /> 编译为 <see cref="Action{T1, T2}" /> 委托。
        /// </summary>
        /// <param name="method">要编译的方法信息。</param>
        /// <returns>编译后的委托。参数 0 = 目标对象，参数 1 = 参数数组。</returns>
        /// <remarks>
        ///     <para>
        ///         <b>为什么性能好：</b>
        ///     </para>
        ///     <para>
        ///     <c>MethodInfo.Invoke</c> 每次调用都要做：参数类型检查、可见性检查、
        ///     安全栈遍历、装箱拆箱。约 200-400ns/次。
        ///     </para>
        ///     <para>
        ///     表达式树在注册时一次性把方法调用编译成等价的 IL 委托。
        ///     之后每次调用走 JIT 编译后的机器码，只有类型转换 + 数组索引 + callvirt，
        ///     约 8-15ns/次。比反射快 <b>20-40 倍</b>。
        ///     </para>
        ///     <para>
        ///     编译发生在 <c>Bind</c> 时（<c>OnEnable</c> 生命周期），属于冷路径，
        ///     一次性开销约 1-3ms，对运行时帧率无影响。
        ///     </para>
        ///     <para>
        ///         <b>有什么缺点：</b>
        ///     </para>
        ///     <list type="bullet">
        ///         <item>
        ///         <b>首次编译开销</b>：每个唯一 MethodInfo 需要 ~1-3ms 编译时间。
        ///         如果在运行时频繁绑定/解绑大量不同方法，编译开销会累积。
        ///         </item>
        ///         <item>
        ///         <b>内存占用增加</b>：每个编译后的委托占用约 100-200 bytes。
        ///         1000 个绑定约多占 100-200KB，通常可接受。
        ///         </item>
        ///         <item>
        ///         <b>不支持的边角情况</b>：ref/out 参数、指针类型等罕见签名
        ///         需要额外处理（本实现仅支持普通值类型和引用类型参数）。
        ///         </item>
        ///         <item>
        ///         <b>调试信息减少</b>：堆栈跟踪中显示的是编译委托而非原始方法名，
        ///         排查时不如 <c>MethodInfo.Invoke</c> 直观。
        ///         </item>
        ///     </list>
        /// </remarks>
        static Action<object, object[]> CompileMethod(MethodInfo method)
        {
            // 参数 1：目标对象（订阅者），类型为 object，需要转换为方法所属类型
            var targetParam = Expression.Parameter(typeof(object), "target");

            // 参数 2：参数数组，类型为 object[]
            var argsParam = Expression.Parameter(typeof(object[]), "args");

            // 构造方法调用的参数表达式
            var parameters = method.GetParameters();
            var callArgs = new Expression[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                // 从 object[] 中取出第 i 个元素，并转换为参数声明的类型
                // 等价于 C# 代码: (T)arg[i]
                callArgs[i] = Expression.Convert(Expression.ArrayIndex(argsParam, Expression.Constant(i)),
                    parameters[i].ParameterType);
            }

            // 构造调用表达式
            // 等价于 C# 代码: ((TargetType)target).Method((T0)args[0], (T1)args[1], ...)
            var body = Expression.Call(Expression.Convert(targetParam, method.DeclaringType), method,
                callArgs);

            // 编译为委托
            return Expression.Lambda<Action<object, object[]>>(body, targetParam, argsParam).Compile();
        }
    }

    /// <summary>
    /// Script 订阅绑定信息。通过 <see cref="Action{T}" /> 委托直接调用，无需表达式树。
    /// </summary>
    public sealed class DynamicBindingInfo<TEventArgs> : BindingInfo where TEventArgs : AesirEventArgs
    {
        readonly Action<TEventArgs> _callback;

        public DynamicBindingInfo(Action<TEventArgs> callback, SubscriberPriority priority, object subscriber)
        {
            BindingKey = AesirEventUtility.GetEventBindingKey<TEventArgs>();
            Subscriber = subscriber;
            Priority = priority;
            _callback = callback;
        }

        public override void Invoke(object[] args = null)
        {
            if (args is { Length: 1 } && args[0] is TEventArgs eventArg)
            {
                _callback(eventArg);
            }
            else
            {
                Debug.LogError($"DynamicBindingInfo: 参数类型不匹配，期望 {typeof(TEventArgs).Name}。");
            }
        }
    }
}
