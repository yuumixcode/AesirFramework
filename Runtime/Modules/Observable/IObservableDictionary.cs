using System.Collections.Generic;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 完整可观察字典接口。
    /// <para>Model 层通过此接口读写集合；View 层使用 <see cref="IReadOnlyObservableDictionary{TKey, TValue}" /> 只读订阅。</para>
    /// </summary>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <typeparam name="TValue">值类型</typeparam>
    /// <remarks>
    /// 所有写操作完成后才触发对应事件，监听者回调中读取到的集合已是变更后的状态。
    /// 无变更的操作不触发事件：Remove 不存在的键、Clear 空字典、索引器赋相同值。
    /// </remarks>
    /// <seealso cref="IReadOnlyObservableDictionary{TKey, TValue}" />
    /// <seealso cref="ObservableDictionary{TKey, TValue}" />
    public interface IObservableDictionary<TKey, TValue> :
        IReadOnlyObservableDictionary<TKey, TValue>,
        IDictionary<TKey, TValue>
    {
        // 以下成员用 new 重新声明，统一 IDictionary 与 IReadOnlyDictionary 两条平行继承链上的同名成员。
        // 缺少重声明时，通过本接口访问这些成员会因双链各有一份声明而产生 CS0229 多义性。

        /// <summary>
        /// 键值对数量。重新声明以消除 <see cref="ICollection{T}" /> 与 <see cref="IReadOnlyCollection{T}" /> 的双链多义性。
        /// </summary>
        new int Count { get; }

        /// <summary>
        /// 读写指定键的值。重新声明以统一 <see cref="IDictionary{TKey, TValue}" /> 与 <see cref="IReadOnlyDictionary{TKey, TValue}" /> 的索引器。
        /// </summary>
        /// <param name="key">键。</param>
        new TValue this[TKey key] { get; set; }

        /// <summary>
        /// 所有键的集合。重新声明以消除 <see cref="IDictionary{TKey, TValue}" />（ICollection）与
        /// <see cref="IReadOnlyDictionary{TKey, TValue}" />（IEnumerable）的返回类型多义性。
        /// </summary>
        new IEnumerable<TKey> Keys { get; }

        /// <summary>
        /// 所有值的集合。重新声明以消除 <see cref="IDictionary{TKey, TValue}" />（ICollection）与
        /// <see cref="IReadOnlyDictionary{TKey, TValue}" />（IEnumerable）的返回类型多义性。
        /// </summary>
        new IEnumerable<TValue> Values { get; }

        /// <summary>
        /// 判断是否包含指定键。重新声明以消除双链多义性。
        /// </summary>
        /// <param name="key">要查找的键。</param>
        /// <returns>包含返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        new bool ContainsKey(TKey key);

        /// <summary>
        /// 获取与指定键关联的值。重新声明以消除双链多义性。
        /// </summary>
        /// <param name="key">要查找的键。</param>
        /// <param name="value">键存在时为关联的值，否则为 <typeparamref name="TValue" /> 的默认值。</param>
        /// <returns>键存在返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        new bool TryGetValue(TKey key, out TValue value);
    }
}
