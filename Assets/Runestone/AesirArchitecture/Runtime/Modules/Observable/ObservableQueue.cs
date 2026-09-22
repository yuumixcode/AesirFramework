#nullable enable
using Runestone.AesirArchitecture.Internal;
using System.Buffers;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 可观察队列实现。
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <remarks>
    /// 内部组合 <see cref="Queue{T}" /> 存储元素，变更通知经 <see cref="MiniEvent{T}" /> 分发——Invoke 路径零分配。
    /// 变更通知为单一事件（<see cref="AddListener" />）：入队 → Add（索引为队尾位置）、出队 → Remove（索引固定 0）、
    /// Clear → Reset（非空才通知）；批量入队 / 出队逐项通知；无变更的操作（TryDequeue 空队列）不通知。
    /// </remarks>
    public class ObservableQueue<T> : IReadOnlyCollection<T>, IObservableCollection<T>
    {
        readonly Queue<T> queue;

        readonly MiniEvent<CollectionChangedEventArgs<T>> _changedEvent =
            new MiniEvent<CollectionChangedEventArgs<T>>();

        public ObservableQueue()
        {
            this.queue = new Queue<T>();
        }

        public ObservableQueue(int capacity)
        {
            this.queue = new Queue<T>(capacity);
        }

        public ObservableQueue(IEnumerable<T> collection)
        {
            this.queue = new Queue<T>(collection);
        }

        public int Count
        {
            get
            {
                return queue.Count;
            }
        }

        /// <summary>
        /// 元素入队，触发 Add 通知（索引为队尾位置）。
        /// </summary>
        /// <param name="item">要入队的元素。</param>
        public void Enqueue(T item)
        {
            var index = queue.Count;
            queue.Enqueue(item);
            _changedEvent.Invoke(CollectionChangedEventArgs<T>.Add(item, index));
        }

        /// <summary>
        /// 批量入队元素序列，逐项触发 Add 通知。
        /// </summary>
        /// <param name="items">要入队的元素序列。</param>
        /// <remarks>先整体拷贝再入队，传入队列自身时也能正常终止。</remarks>
        public void EnqueueRange(IEnumerable<T> items)
        {
            using (var xs = new CloneCollection<T>(items))
            {
                foreach (var item in xs.Span)
                {
                    Enqueue(item);
                }
            }
        }

        /// <summary>
        /// 批量入队元素数组，逐项触发 Add 通知。
        /// </summary>
        /// <param name="items">要入队的元素数组。</param>
        public void EnqueueRange(T[] items)
        {
            foreach (var item in items)
            {
                Enqueue(item);
            }
        }

        /// <summary>
        /// 批量入队元素（只读跨度重载），逐项触发 Add 通知。
        /// </summary>
        /// <param name="items">要入队的元素只读跨度。</param>
        public void EnqueueRange(ReadOnlySpan<T> items)
        {
            foreach (var item in items)
            {
                Enqueue(item);
            }
        }

        /// <summary>
        /// 队首元素出队并返回，触发 Remove 通知（索引固定 0）。
        /// </summary>
        /// <returns>出队的元素。</returns>
        public T Dequeue()
        {
            var v = queue.Dequeue();
            _changedEvent.Invoke(CollectionChangedEventArgs<T>.Remove(v, 0));
            return v;
        }

        /// <summary>
        /// 尝试让队首元素出队，成功时触发 Remove 通知（索引固定 0）；空队列不通知。
        /// </summary>
        /// <param name="result">出队的元素；队列为空时为类型默认值。</param>
        /// <returns>出队成功返回 <c>true</c>，队列为空返回 <c>false</c>。</returns>
        public bool TryDequeue([MaybeNullWhen(false)] out T result)
        {
            if (queue.Count != 0)
            {
                result = queue.Dequeue();
                _changedEvent.Invoke(CollectionChangedEventArgs<T>.Remove(result, 0));
                return true;
            }
            result = default;
            return false;
        }

        /// <summary>
        /// 让队首的 <paramref name="count" /> 个元素依次出队，逐项触发 Remove 通知（索引固定 0）。
        /// </summary>
        /// <param name="count">要出队的元素数量。</param>
        public void DequeueRange(int count)
        {
            for (var i = 0; i < count; i++)
            {
                Dequeue();
            }
        }

        /// <summary>
        /// 让队首元素依次出队并写入 <paramref name="dest" />，逐项触发 Remove 通知（索引固定 0）。
        /// </summary>
        /// <param name="dest">接收出队元素的跨度，长度即出队数量。</param>
        public void DequeueRange(Span<T> dest)
        {
            for (var i = 0; i < dest.Length; i++)
            {
                dest[i] = Dequeue();
            }
        }

        /// <summary>
        /// 清空队列。队列非空时以 Reset 通知；已为空时不通知。
        /// </summary>
        public void Clear()
        {
            if (queue.Count == 0)
            {
                return;
            }

            queue.Clear();
            _changedEvent.Invoke(CollectionChangedEventArgs<T>.Reset());
        }

        public T Peek()
        {
            return queue.Peek();
        }

        public bool TryPeek([MaybeNullWhen(false)] out T result)
        {
            if (queue.Count != 0)
            {
                result = queue.Peek();
                return true;
            }
            result = default;
            return false;
        }

        public T[] ToArray()
        {
            return queue.ToArray();
        }

        public void TrimExcess()
        {
            queue.TrimExcess();
        }

        /// <inheritdoc cref="IObservableCollection{T}.AddListener" />
        public AutoRemoveListenerHandle AddListener(Action<CollectionChangedEventArgs<T>> callback) =>
            _changedEvent.AddListener(callback);

        /// <inheritdoc cref="IObservableCollection{T}.RemoveListener" />
        public void RemoveListener(Action<CollectionChangedEventArgs<T>> callback) =>
            _changedEvent.RemoveListener(callback);

        /// <summary>
        /// 清空所有变更监听。
        /// </summary>
        /// <remarks>
        /// 清除全部监听引用，防止因监听者未释放导致的内存泄漏。
        /// 与 <see cref="Clear" /> 不同——后者清空的是队列元素。
        /// </remarks>
        public void ClearListeners()
        {
            _changedEvent.Dispose();
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in queue)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
