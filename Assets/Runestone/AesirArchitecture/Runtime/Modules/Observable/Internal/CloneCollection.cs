#nullable enable
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Runestone.AesirArchitecture.Internal
{
    /// <summary>
    /// 只读克隆集合：把源序列物化为租借数组的临时快照，
    /// 供批量操作在写入自身前先完成拷贝（如源序列传入集合自身时避免"枚举中修改"异常）。
    /// </summary>
    /// <typeparam name="T">元素类型。</typeparam>
    /// <remarks>
    /// 数组经 <see cref="ArrayPool{T}.Shared" /> 租借、<see cref="Dispose" /> 时归还；
    /// 上游 Cysharp.ObservableCollections 经 CollectionsMarshal 零拷贝取 <see cref="List{T}" /> 内部数组，
    /// Unity netstandard2.1 参考程序集无该 API，本项目以逐项物化作语义等价的降级实现。
    /// </remarks>
    internal struct CloneCollection<T> : IDisposable
    {
        T[]? array;
        readonly int length;

        /// <summary>
        /// 已物化元素的只读跨度（长度为实际元素数，非租借容量）。
        /// </summary>
        public ReadOnlySpan<T> Span => array.AsSpan(0, length);

        /// <summary>
        /// 以 <see cref="IEnumerable{T}" /> 形态暴露已物化元素（供按接口消费的批量方法使用）。
        /// </summary>
        public IEnumerable<T> AsEnumerable() => new EnumerableCollection(array, length);

        /// <summary>
        /// 从可枚举源物化克隆。源可提供非枚举计数（<c>TryGetNonEnumeratedCount</c>）时按计数一次性租借，
        /// 否则从 16 起步倍增扩容。
        /// </summary>
        /// <param name="source">要克隆的源序列。</param>
        public CloneCollection(IEnumerable<T> source)
        {
            if (source.TryGetNonEnumeratedCount(out var count))
            {
                var array = ArrayPool<T>.Shared.Rent(count);

                if (source is ICollection<T> c)
                {
                    c.CopyTo(array, 0);
                }
                else
                {
                    var i = 0;
                    foreach (var item in source)
                    {
                        array[i++] = item;
                    }
                }

                this.array = array;
                length = count;
            }
            else
            {
                var array = ArrayPool<T>.Shared.Rent(16);

                var i = 0;
                foreach (var item in source)
                {
                    TryEnsureCapacity(ref array, i);
                    array[i++] = item;
                }

                this.array = array;
                length = i;
            }
        }

        /// <summary>
        /// 从 <see cref="List{T}" /> 区间物化克隆（Unity 无 CollectionsMarshal，逐项拷贝替代零拷贝）。
        /// </summary>
        /// <param name="source">源列表。</param>
        /// <param name="index">区间起始索引。</param>
        /// <param name="count">区间元素数。</param>
        public CloneCollection(List<T> source, int index, int count)
        {
            var array = ArrayPool<T>.Shared.Rent(count);
            for (var i = 0; i < count; i++)
            {
                array[i] = source[index + i];
            }

            this.array = array;
            length = count;
        }

        /// <summary>
        /// 租借数组不足以容纳下一元素时倍增换租（旧数组归还池）。
        /// </summary>
        static void TryEnsureCapacity(ref T[] array, int index)
        {
            if (array.Length == index)
            {
                var newArray = ArrayPool<T>.Shared.Rent(index * 2);
                Array.Copy(array, newArray, index);
                ArrayPool<T>.Shared.Return(array, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
                array = newArray;
            }
        }

        /// <summary>
        /// 归还租借数组；重复调用幂等。
        /// </summary>
        public void Dispose()
        {
            if (array != null)
            {
                ArrayPool<T>.Shared.Return(array, RuntimeHelpers.IsReferenceOrContainsReferences<T>());
                array = null;
            }
        }

        /// <summary>
        /// 只读包装视图：以 <see cref="ICollection{T}" /> 契约暴露已物化元素，
        /// 写方法一律不支持（克隆仅作为批量操作的输入快照）。
        /// </summary>
        class EnumerableCollection : ICollection<T>
        {
            readonly T[] array;
            readonly int count;

            /// <summary>
            /// 以可能为 null 的租借数组构造（null 视为空集合）。
            /// </summary>
            public EnumerableCollection(T[]? array, int count)
            {
                if (array == null)
                {
                    this.array = Array.Empty<T>();
                    this.count = 0;
                }
                else
                {
                    this.array = array;
                    this.count = count;
                }
            }

            /// <summary>已物化的元素数。</summary>
            public int Count => count;

            /// <summary>固定为只读。</summary>
            public bool IsReadOnly => true;

            /// <summary>不支持——克隆为只读快照。</summary>
            public void Add(T item) => throw new NotSupportedException();

            /// <summary>不支持——克隆为只读快照。</summary>
            public void Clear() => throw new NotSupportedException();

            /// <summary>
            /// 按值扫描已物化区间。实现 <see cref="ICollection{T}" /> 契约而非抛异常——
            /// 调用方按接口约定使用 Contains 不应触雷。
            /// </summary>
            public bool Contains(T item) => Array.IndexOf(array, item, 0, count) >= 0;

            /// <summary>拷贝已物化区间到目标数组。</summary>
            public void CopyTo(T[] dest, int destIndex) => Array.Copy(array, 0, dest, destIndex, count);

            /// <summary>按序枚举已物化区间。</summary>
            public IEnumerator<T> GetEnumerator()
            {
                for (var i = 0; i < count; i++)
                {
                    yield return array[i];
                }
            }

            /// <summary>不支持——克隆为只读快照。</summary>
            public bool Remove(T item) => throw new NotSupportedException();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
