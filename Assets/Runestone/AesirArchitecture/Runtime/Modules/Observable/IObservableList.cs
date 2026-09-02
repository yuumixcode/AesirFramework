using System.Collections.Generic;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 完整可观察列表接口。
    /// <para>Model 层通过此接口读写集合；View 层使用 <see cref="IReadOnlyObservableList{T}" /> 只读订阅。</para>
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <remarks>
    /// 在 <see cref="IList{T}" /> 基础上追加 <see cref="AddRange" /> 批量添加。
    /// 所有写操作完成后才触发对应事件，监听者回调中读取到的集合已是变更后的状态。
    /// </remarks>
    /// <seealso cref="IReadOnlyObservableList{T}" />
    /// <seealso cref="ObservableList{T}" />
    public interface IObservableList<T> : IReadOnlyObservableList<T>, IList<T>
    {
        // 以下成员用 new 重新声明，统一 IList<T> 与 IReadOnlyList<T> 两条平行继承链上的同名成员。
        // 缺少重声明时，通过本接口访问这些成员会因双链各有一份声明而产生 CS0229 多义性。

        /// <summary>
        /// 元素数量。重新声明以消除 <see cref="ICollection{T}" /> 与 <see cref="IReadOnlyCollection{T}" /> 的双链多义性。
        /// </summary>
        new int Count { get; }

        /// <summary>
        /// 读写指定索引的元素。重新声明以统一 <see cref="IList{T}" /> 与 <see cref="IReadOnlyList{T}" /> 的索引器。
        /// </summary>
        /// <param name="index">元素索引。</param>
        new T this[int index] { get; set; }

        /// <summary>
        /// 批量添加元素。逐项添加并逐项触发 Added 事件。
        /// </summary>
        /// <param name="items">要添加的元素序列。</param>
        /// <remarks>每添加一项触发一次 Added；如需"整体刷新一次通知"的语义，可先 <see cref="ICollection{T}.Clear" /> 再逐项 <see cref="ICollection{T}.Add" />。</remarks>
        void AddRange(IEnumerable<T> items);
    }
}
