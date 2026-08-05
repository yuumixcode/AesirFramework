using System;
using System.Collections.Generic;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 泛型对象定位器。按类型注册、查询与获取以 <typeparamref name="T" /> 为基类的对象实例。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 内部以 <see cref="Type" /> 为键、<typeparamref name="T" /> 为值的 <see cref="Dictionary{TKey, TValue}" /> 作为容器，
    /// 支持按类型注册和获取实例。注册时以 <c>typeof(TItem)</c> 作为键，查询时须使用相同的类型参数。
    /// </para>
    /// <para>
    /// 支持全局单例模式：通过 <see cref="Global" /> 属性访问的全局实例在首次访问时懒初始化。
    /// 静态构造函数将重置回调注册到 <see cref="ResetStaticsAssistant" />，在 Unity 关闭 Domain Reload 时
    /// 自动清除全局引用，避免跨域加载残留旧数据。
    /// </para>
    /// <para>
    /// 调用 <see cref="Dispose" /> 时，若当前实例恰为全局实例，则同时清除全局引用，防止后续
    /// <see cref="Global" /> 返回已释放的实例。
    /// </para>
    /// <para>
    /// <see cref="AbstractContext{T}" /> 内部使用两个 <see cref="GenericLocator{T}" /> 实例分别管理
    /// <c>IModel</c> 和 <c>IService</c>，实现 Model / Service 的注册与查询。
    /// </para>
    /// </remarks>
    [Serializable]
    public sealed class GenericLocator<T> : IGenericLocator<T>, IDisposable where T : class
    {
        static GenericLocator<T> _global;
        readonly Dictionary<Type, T> _registry = new Dictionary<Type, T>();

        static GenericLocator()
        {
            ResetStaticsAssistant.Register(() => _global = null);
        }

        /// <summary>
        /// 获取全局定位器实例。首次访问时懒初始化。
        /// </summary>
        /// <remarks>
        /// <para>
        /// 采用懒初始化策略，仅在首次访问时创建实例，避免不必要的分配。
        /// </para>
        /// <para>
        /// 静态构造函数已将重置回调注册到 <see cref="ResetStaticsAssistant" />，在 Unity 关闭
        /// Domain Reload 的场景下，每次进入 PlayMode 时会自动将 <c>_global</c> 置为 <c>null</c>，
        /// 确保不会残留上一轮域加载的旧实例，实现域加载安全。
        /// </para>
        /// </remarks>
        public static GenericLocator<T> Global
        {
            get
            {
                _global ??= new GenericLocator<T>();
                return _global;
            }
        }

        /// <summary>
        /// 释放资源，清空所有注册。若当前实例为全局实例，则同时清除全局引用。
        /// </summary>
        /// <remarks>
        /// <para>
        /// 当当前实例与 <see cref="Global" /> 返回的全局实例为同一引用时，Dispose 会额外将
        /// <c>_global</c> 置为 <c>null</c>。这样后续再访问 <see cref="Global" /> 时会重新创建
        /// 一个全新的空实例，而非返回已清空但可能残留引用的旧实例。
        /// </para>
        /// <para>
        /// 若当前实例为独立实例（非全局实例），则仅清空自身注册表，不影响全局实例。
        /// </para>
        /// </remarks>
        public void Dispose()
        {
            Clear();

            if (ReferenceEquals(_global, this))
            {
                _global = null;
            }
        }

        /// <summary>
        /// 注册一个实例。如果类型已存在，则覆盖原有注册。
        /// </summary>
        /// <typeparam name="TItem">要注册的实例类型，必须为 <typeparamref name="T" /> 的子类型。</typeparam>
        /// <param name="instance">要注册的实例。</param>
        public void Register<TItem>(TItem instance) where TItem : class, T
        {
            var key = typeof(TItem);
            _registry[key] = instance;
        }

        /// <summary>
        /// 按显式指定的类型注册一个实例
        /// </summary>
        /// <param name="type">注册时使用的键类型，实例必须可赋值给该类型。</param>
        /// <param name="instance">要注册的实例。</param>
        /// <exception cref="ArgumentException">
        /// 当 <paramref name="instance" /> 不能赋值给 <paramref name="type" /> 时抛出。
        /// </exception>
        public void Register(Type type, T instance)
        {
            if (!type.IsInstanceOfType(instance))
            {
                throw new ArgumentException($"实例类型与 {type.Name} 不匹配", nameof(instance));
            }

            _registry[type] = instance;
        }

        /// <summary>
        /// 获取指定类型的实例。如果不存在，返回 null。
        /// </summary>
        /// <typeparam name="TItem">要获取的实例类型，必须为 <typeparamref name="T" /> 的子类型。</typeparam>
        /// <returns>已注册的实例；若未注册则返回 <c>null</c>。</returns>
        public TItem Get<TItem>() where TItem : class, T
        {
            if (_registry.TryGetValue(typeof(TItem), out var value))
            {
                return value as TItem;
            }

            return null;
        }

        /// <summary>
        /// 尝试获取指定类型的实例
        /// </summary>
        /// <typeparam name="TItem">要获取的实例类型，必须为 <typeparamref name="T" /> 的子类型。</typeparam>
        /// <param name="instance">找到时输出已注册的实例；未找到时输出 <c>null</c>。</param>
        /// <returns>成功找到则返回 <c>true</c>；未注册则返回 <c>false</c>。</returns>
        public bool TryGet<TItem>(out TItem instance) where TItem : class, T
        {
            if (_registry.TryGetValue(typeof(TItem), out var value))
            {
                instance = value as TItem;
                return true;
            }

            instance = null;
            return false;
        }

        /// <summary>
        /// 检查是否已注册指定类型的实例
        /// </summary>
        /// <typeparam name="TItem">要检查的实例类型，必须为 <typeparamref name="T" /> 的子类型。</typeparam>
        /// <returns>已注册则返回 <c>true</c>；否则返回 <c>false</c>。</returns>
        public bool IsRegistered<TItem>() where TItem : class, T =>
            _registry.ContainsKey(typeof(TItem));

        /// <summary>
        /// 注销指定类型的实例
        /// </summary>
        /// <typeparam name="TItem">要注销的实例类型，必须为 <typeparamref name="T" /> 的子类型。</typeparam>
        public void Unregister<TItem>() where TItem : class, T
        {
            var key = typeof(TItem);
            _registry.Remove(key);
        }

        /// <summary>
        /// 清空所有已注册的实例
        /// </summary>
        public void Clear()
        {
            _registry.Clear();
        }

        /// <summary>
        /// 按 Type 获取实例（非泛型版本）
        /// </summary>
        /// <param name="type">要查询的 <see cref="Type" />，作为注册键。</param>
        /// <returns>已注册的实例；若未注册则返回 <c>null</c>。</returns>
        public T GetByType(Type type) =>
            _registry.GetValueOrDefault(type);

        /// <summary>
        /// 获取所有已注册的实例集合
        /// </summary>
        /// <returns>所有已注册实例的 <see cref="IEnumerable{T}" /> 集合，不含类型键。</returns>
        public IEnumerable<T> GetAll() => _registry.Values;

        /// <summary>
        /// 获取底层注册表字典
        /// </summary>
        /// <returns>底层 <see cref="Dictionary{TKey, TValue}" /> 注册表。</returns>
        /// <remarks>
        /// 返回的是底层字典的直接引用，而非只读视图。调用方可直接对其进行增删改操作，
        /// 修改会立即生效。如需安全遍历请先创建副本。
        /// </remarks>
        public Dictionary<Type, T> GetRegistry() => _registry;
    }
}
