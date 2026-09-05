using System.Collections.Generic;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 完整可观察集合接口。
    /// <para>Model 层通过此接口读写集合；View 层使用 <see cref="IReadOnlyObservableHashSet{T}" /> 只读订阅。</para>
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <remarks>
    /// 集合代数操作（UnionWith / ExceptWith / IntersectWith / SymmetricExceptWith）逐项触发 Added / Removed 事件。
    /// 所有写操作完成后才触发对应事件，监听者回调中读取到的集合已是变更后的状态。
    /// 无变更的操作不触发事件：Add 重复元素、Remove 不存在的元素、Clear 空集合。
    /// </remarks>
    /// <seealso cref="IReadOnlyObservableHashSet{T}" />
    /// <seealso cref="ObservableHashSet{T}" />
    public interface IObservableHashSet<T> : IReadOnlyObservableHashSet<T>, ISet<T>
    {
        // 以下成员用 new 重新声明，统一 ISet<T>（ICollection<T> 链）与 IReadOnlyObservableHashSet<T>（IReadOnlyCollection<T> 链）两条平行继承链上的同名成员。
        // 缺少重声明时，通过本接口访问这些成员会因双链各有一份声明而产生 CS0229 多义性。

        /// <summary>
        /// 元素数量。重新声明以消除 <see cref="ICollection{T}" /> 与 <see cref="IReadOnlyCollection{T}" /> 的双链多义性。
        /// </summary>
        new int Count { get; }

        /// <summary>
        /// 判断是否包含指定元素。重新声明以消除 <see cref="ICollection{T}" /> 与 <see cref="IReadOnlyObservableHashSet{T}" /> 的双链多义性。
        /// </summary>
        /// <param name="item">要查找的元素。</param>
        /// <returns>包含返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        new bool Contains(T item);
    }
}
