// 上游行为对齐测试：移植自 Cysharp/ObservableCollections 的测试套件
// （原测试使用 xUnit + FluentAssertions，此处转换为 NUnit 断言并保持用例语义一致）。

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    public class ObservableQueueTests
    {
        [Test]
        public void View()
        {
            var queue = new ObservableQueue<int>();
            var view = queue.CreateView(x => new ViewContainer<int>(x));

            queue.Enqueue(10);
            queue.Enqueue(50);
            queue.Enqueue(30);
            queue.Enqueue(20);
            queue.Enqueue(40);

            void Equal(params int[] expected)
            {
                Assert.AreEqual(expected, queue);
                Assert.AreEqual(expected, view.Select(x => x.Value));
            }

            Equal(10, 50, 30, 20, 40);

            queue.EnqueueRange(new[] { 1, 2, 3, 4, 5 });
            Equal(10, 50, 30, 20, 40, 1, 2, 3, 4, 5);

            Assert.AreEqual(10, queue.Dequeue());
            Equal(50, 30, 20, 40, 1, 2, 3, 4, 5);

            Assert.IsTrue(queue.TryDequeue(out var q));
            Assert.AreEqual(50, q);
            Equal(30, 20, 40, 1, 2, 3, 4, 5);

            queue.DequeueRange(4);
            Equal(2, 3, 4, 5);

            queue.Clear();

            Equal();
        }



        //    view.AttachFilter(filter);
        //    filter.CalledWhenTrue.Select(x => x.Item1).Should().Equal(30);
        //    filter.CalledWhenFalse.Select(x => x.Item1).Should().Equal(10, 50, 20, 40);

        //    view.Select(x => x.Value).Should().Equal(30);

        //    filter.Clear();

        //    queue.Enqueue(33);
        //    queue.EnqueueRange(new[] { 98 });

        //    filter.CalledOnCollectionChanged.Select(x => (x.Action, x.NewValue, x.NewViewIndex)).Should().Equal((NotifyCollectionChangedAction.Add, 33, 5), (NotifyCollectionChangedAction.Add, 98, 6));
        //    filter.Clear();

        //    queue.Dequeue();
        //    queue.DequeueRange(2);
        //    filter.CalledOnCollectionChanged.Select(x => (x.Action, x.OldValue, x.OldViewIndex)).Should().Equal((NotifyCollectionChangedAction.Remove, 10, 0), (NotifyCollectionChangedAction.Remove, 50, 0), (NotifyCollectionChangedAction.Remove, 30, 0));
        //}
        


        //    view.AttachFilter(filter, true);
            

        //    filter.CalledWhenTrue.Count.Should().Be(1);
        //    filter.CalledWhenFalse.Count.Should().Be(4);
        //}   
    }
}
