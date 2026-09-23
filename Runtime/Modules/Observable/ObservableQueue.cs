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
    /// <para>
    /// <c>[Serializable]</c> 标记与类型上的 <c>[SerializeField]</c> 供 Odin 序列化等第三方集成使用——
    /// Unity 原生不序列化 <see cref="Queue{T}" />，初始元素请经构造函数或 <see cref="EnqueueRange" /> 填充。
    /// </para>
    /// </remarks>
    [Serializable]
    public sealed class ObservableQueue<T> : IReadOnlyCollection<T>, IObservableCollection<T>
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
            // 对齐其余三集合：初始元素为 null 时视为空集合（BCL Queue<T> 构造对 null 抛 ArgumentNullException）
            this.queue = collection != null ? new Queue<T>(collection) : new Queue<T>();
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

        /// <summary>
        /// 返回遍历元素的结构体枚举器，foreach 具体类型时零分配。
        /// </summary>
        /// <returns>元素枚举器。</returns>
        public Enumerator GetEnumerator() => new Enumerator(queue.GetEnumerator());

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => queue.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)queue).GetEnumerator();

        /// <summary>
        /// 元素枚举器。
        /// </summary>
        /// <remarks>
        /// 结构体枚举器，foreach 具体类型时零分配（与其他三集合一致）。
        /// 遍历期间修改队列会抛 <see cref="InvalidOperationException" />（继承自内部 <see cref="Queue{T}" /> 枚举器的版本检查，与 BCL 语义一致）。
        /// </remarks>
        public struct Enumerator : IEnumerator<T>
        {
            Queue<T>.Enumerator _inner;

            internal Enumerator(Queue<T>.Enumerator inner) => _inner = inner;

            /// <summary>
            /// 获取当前位置的元素。
            /// </summary>
            public T Current => _inner.Current;

            /// <summary>
            /// 前进到下一个元素。
            /// </summary>
            /// <returns>存在下一个元素返回 <c>true</c>，遍历结束返回 <c>false</c>。</returns>
            public bool MoveNext() => _inner.MoveNext();

            object IEnumerator.Current => _inner.Current;

            void IEnumerator.Reset() => throw new NotSupportedException();

            void IDisposable.Dispose() { }
        }
    }
}
