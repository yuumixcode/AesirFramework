using System.Collections.Generic;

namespace Runestone.AesirArchitecture.Internal
{
    /// <summary>
    /// 可观察集合内部工具。
    /// </summary>
    /// <remarks>
    /// 上游 Cysharp.ObservableCollections 通过 <c>Shims/Collections.cs</c> 在 <see cref="System.Collections.Generic" />
    /// 命名空间内提供同名扩展；本项目改为在本命名空间内提供等价实现，避免污染 BCL 命名空间。
    /// </remarks>
    internal static class ObservableCollectionUtility
    {
        /// <summary>
        /// 尝试以 O(1) 取得序列元素数量，用于批量操作前预分配容量。
        /// </summary>
        /// <typeparam name="T">元素类型。</typeparam>
        /// <param name="source">源序列。</param>
        /// <param name="count">序列元素数量；无法直接取得时为 0。</param>
        /// <returns>能直接取得数量返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        internal static bool TryGetNonEnumeratedCount<T>(this IEnumerable<T> source, out int count)
        {
            if (source is ICollection<T> collection)
            {
                count = collection.Count;
                return true;
            }

            if (source is IReadOnlyCollection<T> readOnlyCollection)
            {
                count = readOnlyCollection.Count;
                return true;
            }

            count = 0;
            return false;
        }
    }
}
