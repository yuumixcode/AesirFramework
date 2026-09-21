using System;
using System.Collections.Generic;

namespace Runestone.AesirArchitecture.Internal
{
    /// <summary>
    /// <see cref="List{T}" /> 的只读跨度批量操作降级实现。
    /// </summary>
    /// <remarks>
    /// 上游 Cysharp.ObservableCollections 在 <c>Shims/Collections.cs</c> 中通过 <c>Unsafe.As</c> 直接改写
    /// <see cref="List{T}" /> 内部数组实现零拷贝批量插入（仅 .NET 8 有原生 <c>AddRange(ReadOnlySpan&lt;T&gt;)</c>）。
    /// 本项目不引入 <c>System.Runtime.CompilerServices.Unsafe</c>（Unity netstandard2.1 参考程序集不含该程序集），
    /// 因此改为语义等价的降级实现：插入走"先物化再插入"，添加走逐项追加。
    /// </remarks>
    internal static class ListExtensions
    {
        /// <summary>
        /// 批量添加只读跨度中的元素（降级实现：逐项追加，容量按需增长）。
        /// </summary>
        /// <typeparam name="T">元素类型。</typeparam>
        /// <param name="list">目标列表。</param>
        /// <param name="source">要添加的元素只读跨度。</param>
        internal static void AddRange<T>(this List<T> list, ReadOnlySpan<T> source)
        {
            foreach (var item in source)
            {
                list.Add(item);
            }
        }

        /// <summary>
        /// 在指定索引插入只读跨度中的元素（降级实现：先物化再调用 BCL 批量插入，避免 O(n·m) 逐项搬移）。
        /// </summary>
        /// <typeparam name="T">元素类型。</typeparam>
        /// <param name="list">目标列表。</param>
        /// <param name="index">插入位置索引。</param>
        /// <param name="source">要插入的元素只读跨度。</param>
        internal static void InsertRange<T>(this List<T> list, int index, ReadOnlySpan<T> source)
        {
            if (source.IsEmpty)
            {
                return;
            }

            list.InsertRange(index, source.ToArray());
        }
    }
}
