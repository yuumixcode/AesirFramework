using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using NUnit.Framework;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    /// <summary>
    /// 验证 <see cref="ObservableQueue{T}" /> 的单轨变更通知：入队 Add（队尾索引）/ 出队 Remove（索引 0）、
    /// Clear 走 Reset、批量逐项通知，以及无变更跳过行为；补齐四集合专属测试的最后缺口。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     ObservableQueue 是 Model 层向 View 层暴露只读订阅的可观察集合载体，
    ///     写操作完成后才触发通知（回调中集合已是变更后状态）是其核心契约。
    ///     </para>
    ///     <para>纯 C# 逻辑，EditMode 即可运行。</para>
    /// </remarks>
    /// <seealso cref="ObservableQueue{T}" />
    public class ObservableQueueTests
    {
        /// <summary>
        /// 验证 Enqueue 触发 Add 通知（索引为队尾位置），批量入队逐项通知。
        /// </summary>
        [Test]
        public void Enqueue_FiresAddWithTailIndex()
        {
            var queue = new ObservableQueue<string>();
            var received = new List<CollectionChangedEventArgs<string>>();

            queue.AddListener(received.Add);
            queue.Enqueue("a");
            queue.Enqueue("b");

            Assert.AreEqual(2, received.Count, "两次入队应各触发一次 Add");
            Assert.AreEqual(NotifyCollectionChangedAction.Add, received[0].Action);
            Assert.AreEqual("a", received[0].NewItem);
            Assert.AreEqual(0, received[0].NewStartingIndex, "首个元素入队时索引为 0（队尾位置）");
            Assert.AreEqual("b", received[1].NewItem);
            Assert.AreEqual(1, received[1].NewStartingIndex, "第二个元素入队时索引为 1（队尾位置）");
            AesirArchitectureDebug.LogTestInfo("Enqueue: Add 通知携带队尾索引");
        }

        /// <summary>
        /// 验证 EnqueueRange 逐项触发 Add 通知（仅新增入队的元素，无去重语义）。
        /// </summary>
        [Test]
        public void EnqueueRange_FiresAddPerItem()
        {
            var queue = new ObservableQueue<int>();
            var received = new List<CollectionChangedEventArgs<int>>();

            queue.AddListener(received.Add);
            queue.EnqueueRange(new[] { 1, 2, 3 });

            Assert.AreEqual(3, received.Count, "批量入队应逐项触发 Add");
            Assert.AreEqual(1, received[0].NewItem);
            Assert.AreEqual(3, received[2].NewItem);
            Assert.AreEqual(2, received[2].NewStartingIndex, "第三个元素的入队索引应为 2");
            AesirArchitectureDebug.LogTestInfo("EnqueueRange: 逐项触发 Add");
        }

        /// <summary>
        /// 验证 Dequeue 触发 Remove 通知（索引固定 0）；TryDequeue 空队列不通知。
        /// </summary>
        [Test]
        public void Dequeue_FiresRemoveWithZeroIndex_TryDequeueEmptySilent()
        {
            var queue = new ObservableQueue<string>();
            var received = new List<CollectionChangedEventArgs<string>>();
            queue.Enqueue("a");
            queue.Enqueue("b");

            queue.AddListener(received.Add);
            Assert.AreEqual("a", queue.Dequeue(), "FIFO 语义应先出队首个元素");
            Assert.AreEqual(1, received.Count, "出队应触发一次 Remove");
            Assert.AreEqual(NotifyCollectionChangedAction.Remove, received[0].Action);
            Assert.AreEqual("a", received[0].OldItem);
            Assert.AreEqual(0, received[0].OldStartingIndex, "出队索引固定 0");

            queue.Clear();
            received.Clear();
            Assert.IsFalse(queue.TryDequeue(out _), "空队列 TryDequeue 应返回 false");
            Assert.AreEqual(0, received.Count, "空队列 TryDequeue 不应触发通知");
            AesirArchitectureDebug.LogTestInfo("Dequeue: Remove 索引固定 0；空队列 TryDequeue 静默");
        }

        /// <summary>
        /// 验证 Clear 非空以 Reset 通知、空队列 Clear 不通知。
        /// </summary>
        [Test]
        public void Clear_FiresResetOnlyWhenNonEmpty()
        {
            var queue = new ObservableQueue<int>();
            var received = new List<CollectionChangedEventArgs<int>>();

            queue.AddListener(received.Add);
            queue.Clear();
            Assert.AreEqual(0, received.Count, "空队列 Clear 不应触发通知");

            queue.Enqueue(1);
            queue.Enqueue(2);
            received.Clear();
            queue.Clear();
            Assert.AreEqual(1, received.Count, "非空 Clear 应触发一次 Reset（不含此前 Enqueue 的 Add 通知）");
            Assert.AreEqual(NotifyCollectionChangedAction.Reset, received[0].Action);
            AesirArchitectureDebug.LogTestInfo("Clear: 非空 Reset、空集合静默");
        }

        /// <summary>
        /// 验证 DequeueRange 数量超过队列长度时抛 <see cref="InvalidOperationException" />（fail-fast），
        /// 已出队元素与已发通知不回滚——半程状态是文档化行为，调用方应保证 dest 长度合法。
        /// </summary>
        [Test]
        public void DequeueRange_CountExceeds_ThrowsWithPartialState()
        {
            var queue = new ObservableQueue<int>();
            var received = new List<CollectionChangedEventArgs<int>>();
            queue.Enqueue(1);
            queue.Enqueue(2);
            queue.AddListener(received.Add);

            var dest = new int[5];
            Assert.Throws<InvalidOperationException>(() => queue.DequeueRange(dest), "dest 长度超过队列长度应抛出");

            Assert.AreEqual(0, queue.Count, "异常前已出队全部 2 个元素（半程状态不回滚）");
            Assert.AreEqual(2, received.Count, "已出队元素的通知已发出（不撤回）");
            AesirArchitectureDebug.LogTestInfo("DequeueRange: 超量抛出，半程状态不回滚");
        }

        /// <summary>
        /// 验证构造函数对 null 初始元素容忍为空队列（对齐其余三集合语义）。
        /// </summary>
        [Test]
        public void Constructor_NullCollection_TreatedAsEmpty()
        {
            Assert.DoesNotThrow(() => _ = new ObservableQueue<int>(null), "null 初始元素应视为空集合");
            Assert.AreEqual(0, new ObservableQueue<int>(null).Count);

            var fromItems = new ObservableQueue<int>(new[] { 1, 2 });
            Assert.AreEqual(2, fromItems.Count, "初始元素应正常填充且不触发通知");
            AesirArchitectureDebug.LogTestInfo("构造: null 容忍为空集合");
        }

        /// <summary>
        /// 验证结构体枚举器按入队顺序零分配遍历（foreach 具体类型），
        /// 遍历期间修改队列抛 <see cref="InvalidOperationException" />（BCL 版本检查语义）。
        /// </summary>
        [Test]
        public void Enumerator_IteratesInOrder_ThrowsWhenModified()
        {
            var queue = new ObservableQueue<int>();
            queue.Enqueue(1);
            queue.Enqueue(2);
            queue.Enqueue(3);

            var iterated = new List<int>();
            foreach (var item in queue)
            {
                iterated.Add(item);
            }

            CollectionAssert.AreEqual(new[] { 1, 2, 3 }, iterated, "应按入队顺序遍历");

            Assert.Throws<InvalidOperationException>(() =>
            {
                foreach (var _ in queue)
                {
                    queue.Enqueue(99);
                }
            }, "遍历期间修改队列应抛出（版本检查）");
            AesirArchitectureDebug.LogTestInfo("枚举器: 按序遍历，遍历中修改抛出");
        }

        /// <summary>
        /// 验证监听句柄 Dispose 后不再收到通知，ClearListeners 清空全部监听。
        /// </summary>
        [Test]
        public void HandleDispose_And_ClearListeners_StopNotifications()
        {
            var queue = new ObservableQueue<int>();
            var count = 0;

            var handle = queue.AddListener(_ => count++);
            queue.Enqueue(1);
            Assert.AreEqual(1, count, "句柄存续期应收到通知");

            handle.Dispose();
            queue.Enqueue(2);
            Assert.AreEqual(1, count, "句柄 Dispose 后不应再收到通知");

            queue.AddListener(_ => count++);
            queue.ClearListeners();
            queue.Enqueue(3);
            Assert.AreEqual(1, count, "ClearListeners 清空全部监听后不应再收到通知");
            AesirArchitectureDebug.LogTestInfo("HandleDispose/ClearListeners: 正确停止通知");
        }
    }
}
