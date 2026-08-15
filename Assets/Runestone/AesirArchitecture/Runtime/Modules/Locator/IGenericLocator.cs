using System;
using System.Collections.Generic;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 泛型定位器接口。提供按类型注册、查询与获取对象实例的契约。
    /// </summary>
    /// <typeparam name="T">定位器管理的基类型，所有注册的实例必须可赋值给该类型。</typeparam>
    /// <remarks>
    /// <para>
    /// 定位器的抽象契约，定义了注册、查询、获取与注销实例的标准接口。
    /// <see cref="GenericLocator{T}" /> 是其默认实现，内部以 <see cref="Dictionary{TKey, TValue}" />
    /// 存储注册关系。
    /// </para>
    /// <para>
    /// 注册与查询须使用相同的类型参数。若以具体类型注册（如 <c>Register&lt;Sword&gt;</c>），
    /// 再以接口类型查询（如 <c>Get&lt;IWeapon&gt;</c>），将返回 <c>null</c>。
    /// </para>
    /// </remarks>
    /// <seealso cref="GenericLocator{T}" />
    public interface IGenericLocator<T> where T : class
    {
        /// <summary>
        /// 注册实例，以 <c>typeof(TItem)</c> 作为键。重复注册将覆盖已有实例。
        /// </summary>
        /// <typeparam name="TItem">要注册的实例类型，必须为 <typeparamref name="T" /> 的子类型。</typeparam>
        /// <param name="instance">要注册的实例。</param>
        /// <remarks>
        /// 注意：注册与查询必须使用相同的类型参数。若以具体类型注册（如 <c>Register&lt;Sword&gt;</c>），
        /// 再以接口类型查询（如 <c>Get&lt;IWeapon&gt;</c>），将返回 <c>null</c>。
        /// </remarks>
        void Register<TItem>(TItem instance) where TItem : class, T;

        /// <summary>
        /// 注册实例，以 <see cref="Type" /> 作为键。重复注册将覆盖已有实例。
        /// </summary>
        /// <param name="type">注册时使用的键类型，实例必须可赋值给该类型。</param>
        /// <param name="instance">要注册的实例。</param>
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
        /// 判断指定类型是否已注册。
        /// </summary>
        /// <typeparam name="TItem">要检查的实例类型，必须为 <typeparamref name="T" /> 的子类型。</typeparam>
        /// <returns>已注册则返回 <c>true</c>；否则返回 <c>false</c>。</returns>
        bool IsRegistered<TItem>() where TItem : class, T;

        /// <summary>
        /// 注销指定类型的注册。
        /// </summary>
        /// <typeparam name="TItem">要注销的实例类型，必须为 <typeparamref name="T" /> 的子类型。</typeparam>
        void Unregister<TItem>() where TItem : class, T;

        /// <summary>
        /// 清空所有已注册的实例。
        /// </summary>
        void Clear();

        /// <summary>
        /// 按 <see cref="Type" /> 获取已注册的实例，不存在则返回 null。
        /// <para>用于依赖项校验等需要运行时 Type 查询的场景。</para>
        /// </summary>
        /// <param name="type">要查询的 <see cref="Type" />，作为注册键。</param>
        /// <returns>已注册的实例；若未注册则返回 <c>null</c>。</returns>
        T GetByType(Type type);

        /// <summary>
        /// 按注册顺序获取所有已注册的实例。
        /// </summary>
        /// <returns>所有已注册实例的 <see cref="IEnumerable{T}" /> 集合，不含类型键。</returns>
        IEnumerable<T> GetAll();
    }
}
