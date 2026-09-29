using System;
using System.Collections.Generic;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 泛型对象定位器。按类型注册、查询与获取以 <typeparamref name="T" /> 为基类的对象实例。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     内部以 <see cref="Type" /> 为键、<typeparamref name="T" /> 为值的 <see cref="Dictionary{TKey, TValue}" /> 作为容器，
    ///     支持按类型注册和获取实例。注册时以 <c>typeof(TItem)</c> 作为键，查询时须使用相同的类型参数。
    ///     </para>
    ///     <para>
    ///     <see cref="AbstractContext{T}" /> 内部使用两个 <see cref="GenericLocator{T}" /> 实例分别管理
    ///     <c>IModel</c> 和 <c>IService</c>，实现 Model / Service 的注册与查询。
    ///     </para>
    /// </remarks>
    [Serializable]
    public sealed class GenericLocator<T> : IGenericLocator<T>, IDisposable where T : class
    {
        /// <summary>
        /// 注册键的插入顺序列表。<see cref="GetAll" /> 按此顺序枚举，
        /// 使"按注册顺序"成为有结构保证的契约，而非依赖 <see cref="Dictionary{TKey, TValue}" />
        /// 枚举顺序这一无 .NET 契约保证的实现细节。
        /// </summary>
        /// <remarks>
        /// <see cref="Register{TItem}" /> 仅在键不存在时追加（覆盖注册不改变原位置）；
        /// <see cref="Unregister{TItem}" /> 同步移除（再注册时按新插入语义追加到末尾）。
        /// </remarks>
        readonly List<Type> _insertionOrder = new List<Type>();

        readonly Dictionary<Type, T> _registry = new Dictionary<Type, T>();

        /// <summary>
        /// 释放资源，清空所有注册。
        /// </summary>
        public void Dispose()
        {
            Clear();
        }

        /// <summary>
        /// 注册一个实例。如果类型已存在，则覆盖原有注册（不改变其插入顺序位置）。
        /// </summary>
        /// <typeparam name="TItem">要注册的实例类型，必须为 <typeparamref name="T" /> 的子类型。</typeparam>
        /// <param name="instance">要注册的实例。</param>
        public void Register<TItem>(TItem instance) where TItem : class, T
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance), "注册实例不可为 null（与 Register(Type, T) 的校验对称）");
            }

            var key = typeof(TItem);
            if (!_registry.ContainsKey(key))
            {
                _insertionOrder.Add(key);
            }

            _registry[key] = instance;
        }

        /// <summary>
        /// 按显式指定的类型注册一个实例。如果类型已存在，则覆盖原有注册（不改变其插入顺序位置）。
        /// </summary>
        /// <param name="type">注册时使用的键类型，实例必须可赋值给该类型。</param>
        /// <param name="instance">要注册的实例。</param>
        /// <exception cref="ArgumentException">
        /// 当 <paramref name="instance" /> 不能赋值给 <paramref name="type" /> 时抛出。
        /// </exception>
        public void Register(Type type, T instance)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (!type.IsInstanceOfType(instance))
            {
                throw new ArgumentException($"实例类型与 {type.Name} 不匹配", nameof(instance));
            }

            if (!_registry.ContainsKey(type))
            {
                _insertionOrder.Add(type);
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
        /// 注销指定类型的实例
        /// </summary>
        /// <typeparam name="TItem">要注销的实例类型，必须为 <typeparamref name="T" /> 的子类型。</typeparam>
        public void Unregister<TItem>() where TItem : class, T
        {
            var key = typeof(TItem);
            _registry.Remove(key);
            _insertionOrder.Remove(key);
        }

        /// <summary>
        /// 按注册顺序获取所有已注册的实例集合
        /// </summary>
        /// <returns>所有已注册实例的 <see cref="IEnumerable{T}" /> 集合，不含类型键，按注册顺序排列。</returns>
        /// <remarks>
        /// 返回调用时刻的完整快照（同步物化）：消费端可在枚举期间修改定位器而不抛"集合已修改"异常，
        /// 期间发生的注册/注销不影响已返回的枚举。物化分配仅发生在调用时（初始化/关停等冷路径）。
        /// 与诊断成员 <see cref="GetAllEntries" /> 取同一份快照语义，两者修改安全性契约一致。
        /// </remarks>
        public IEnumerable<T> GetAll()
        {
            var items = new List<T>(_insertionOrder.Count);
            foreach (var key in _insertionOrder)
            {
                items.Add(_registry[key]);
            }

            return items;
        }

        /// <summary>
        /// 检查是否已注册指定类型的实例（内部调试与测试用）。
        /// </summary>
        /// <typeparam name="TItem">要检查的实例类型，必须为 <typeparamref name="T" /> 的子类型。</typeparam>
        /// <returns>已注册则返回 <c>true</c>；否则返回 <c>false</c>。</returns>
        internal bool IsRegistered<TItem>() where TItem : class, T =>
            _registry.ContainsKey(typeof(TItem));

        /// <summary>
        /// 清空所有已注册的实例（<see cref="Dispose" /> 的底层实现）。
        /// </summary>
        internal void Clear()
        {
            _registry.Clear();
            _insertionOrder.Clear();
        }

        /// <summary>
        /// 按 Type 获取实例（非泛型版本，测试与调试用途）。
        /// </summary>
        /// <param name="type">要查询的 <see cref="Type" />，作为注册键。</param>
        /// <returns>已注册的实例；若未注册则返回 <c>null</c>。</returns>
        internal T GetByType(Type type) =>
            _registry.GetValueOrDefault(type);

        /// <summary>
        /// 获取所有已注册键值对（诊断用途，近失识别专用）。
        /// </summary>
        /// <remarks>
        /// 正常查询请使用 <see cref="Get{TItem}" /> / <see cref="TryGet{TItem}" />。
        /// 此成员仅供诊断路径遍历已注册条目（如 <see cref="AbstractContext{T}" /> 在"未注册"异常中
        /// 识别"已注册实例可赋值给查询类型"的近失情况）；正常路径不产生开销。
        /// <para>
        /// 与 <see cref="GetAll" /> 一致地返回调用时刻的物化快照：枚举期间修改定位器不会抛"集合已修改"异常，
        /// 期间发生的注册/注销不影响已返回的枚举。物化分配仅发生在调用时（异常诊断等冷路径）。
        /// </para>
        /// </remarks>
        public IEnumerable<KeyValuePair<Type, T>> GetAllEntries() =>
            new List<KeyValuePair<Type, T>>(_registry);
    }
}
