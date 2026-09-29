using System;
using System.Collections.Generic;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 泛型定位器接口。提供按类型注册、查询与获取对象实例的契约。
    /// </summary>
    /// <typeparam name="T">定位器管理的基类型，所有注册的实例必须可赋值给该类型。</typeparam>
    /// <remarks>
    ///     <para>
    ///     定位器的抽象契约，定义了注册、查询、获取与注销实例的标准接口。
    ///     <see cref="GenericLocator{T}" /> 是其默认实现，内部以 <see cref="Dictionary{TKey,TValue}" />
    ///     存储注册关系。
    ///     </para>
    ///     <para>
    ///     注册与查询须使用相同的类型参数。若以具体类型注册（如 <c>Register&lt;Sword&gt;</c>），
    ///     再以接口类型查询（如 <c>Get&lt;IWeapon&gt;</c>），将返回 <c>null</c>。
    ///     </para>
    /// </remarks>
    /// <seealso cref="GenericLocator{T}" />
    public interface IGenericLocator<T> : IDisposable where T : class
    {
        /// <summary>
        /// 释放定位器：清空全部注册（等价于清空容器，<see cref="AbstractContext{T}" /> 的收尾即依赖此语义），
        /// 不销毁被注册的实例——实例的释放由调用方（如 Context 逆序 Dispose 模块）负责。
        /// </summary>
        /// <remarks>
        /// 声明为继承 <see cref="IDisposable" /> 的目的是让"清空容器"成为契约的一部分：
        /// <see cref="AbstractContext{T}.Dispose" /> 只持有 <c>IGenericLocator&lt;T&gt;</c> 抽象，
        /// 需要经接口而非具体实现清空。
        /// </remarks>
        void Dispose();

        /// <summary>
        /// 注册实例，以 <c>typeof(TItem)</c> 作为键。重复注册将覆盖已有实例。
        /// </summary>
        /// <typeparam name="TItem">要注册的实例类型，必须为 <typeparamref name="T" /> 的子类型。</typeparam>
        /// <param name="instance">要注册的实例。</param>
        /// <exception cref="ArgumentNullException"><paramref name="instance" /> 为 null 时抛出。</exception>
        void Register<TItem>(TItem instance) where TItem : class, T;

        /// <summary>
        /// 注册实例，以 <see cref="Type" /> 作为键。重复注册将覆盖已有实例。
        /// </summary>
        /// <param name="type">注册时使用的键类型，实例必须可赋值给该类型。</param>
        /// <param name="instance">要注册的实例。</param>
        /// <exception cref="ArgumentNullException"><paramref name="type" /> 或 <paramref name="instance" /> 为 null 时抛出。</exception>
        /// <exception cref="ArgumentException">
        /// 当 <paramref name="instance" /> 不能赋值给 <paramref name="type" /> 时抛出。
        /// </exception>
        void Register(Type type, T instance);

        /// <summary>
        /// 获取已注册的实例，不存在则返回 null。
        /// </summary>
        /// <typeparam name="TItem">要获取的实例类型，必须为 <typeparamref name="T" /> 的子类型。</typeparam>
        /// <returns>已注册的实例；若未注册则返回 <c>null</c>。</returns>
        TItem Get<TItem>() where TItem : class, T;

        /// <summary>
        /// 尝试获取已注册的实例。返回是否成功找到对应类型的注册。
        /// </summary>
        /// <typeparam name="TItem">要获取的实例类型，必须为 <typeparamref name="T" /> 的子类型。</typeparam>
        /// <param name="instance">找到时输出已注册的实例；未找到时输出 <c>null</c>。</param>
        /// <returns>成功找到则返回 <c>true</c>；未注册则返回 <c>false</c>。</returns>
        bool TryGet<TItem>(out TItem instance) where TItem : class, T;

        /// <summary>
        /// 注销指定类型的注册。
        /// </summary>
        /// <typeparam name="TItem">要注销的实例类型，必须为 <typeparamref name="T" /> 的子类型。</typeparam>
        void Unregister<TItem>() where TItem : class, T;

        /// <summary>
        /// 按注册顺序获取所有已注册的实例。
        /// </summary>
        /// <returns>所有已注册实例的 <see cref="IEnumerable{T}" /> 集合，不含类型键，按注册顺序排列。</returns>
        /// <remarks>
        /// 返回调用时刻的完整快照：之后的注册/注销不影响已返回的枚举，
        /// 消费端可在枚举期间安全地修改定位器（例如模块初始化过程中动态注册新模块，不会抛"集合已修改"异常）。
        /// 与诊断成员 <see cref="GetAllEntries" /> 取同一份快照语义，两者修改安全性契约一致。
        /// </remarks>
        IEnumerable<T> GetAll();

        /// <summary>
        /// 按注册键获取所有已注册的键值对（诊断用途）。
        /// </summary>
        /// <returns>注册键 <see cref="Type" /> 与实例的键值对枚举，不保证顺序。</returns>
        /// <remarks>
        /// 正常查询请使用 <see cref="Get{TItem}" /> / <see cref="TryGet{TItem}" />。
        /// 此成员服务"近失识别"类诊断——例如按实现类注册、按接口查询失败时，
        /// 需要遍历注册键值对识别"已注册实例可赋值给查询类型"的近失情况并生成提示。
        /// <para>
        /// 与 <see cref="GetAll" /> 一致地返回调用时刻的物化快照：枚举期间修改定位器不会抛"集合已修改"异常。
        /// </para>
        /// </remarks>
        IEnumerable<KeyValuePair<Type, T>> GetAllEntries();
    }
}
