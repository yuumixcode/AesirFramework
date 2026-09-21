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
    public class ObservableStackTests
    {
        [Test]
        public void View()
        {
            var stack = new ObservableStack<int>();
            var view = stack.CreateView(x => new ViewContainer<int>(x));

            stack.Push(10);
            stack.Push(50);
            stack.Push(30);
            stack.Push(20);
            stack.Push(40);

            void Equal(params int[] expected)
            {
                Assert.AreEqual(expected, stack);
                Assert.AreEqual(expected, view.Select(x => x.Value));
            }

            Equal(40, 20, 30, 50, 10);

            stack.PushRange(new[] { 1, 2, 3, 4, 5 });
            Equal(5, 4, 3, 2, 1, 40, 20, 30, 50, 10);

            Assert.AreEqual(5, stack.Pop());
            Equal(4, 3, 2, 1, 40, 20, 30, 50, 10);

            Assert.IsTrue(stack.TryPop(out var q));
            Assert.AreEqual(4, q);
            Equal(3, 2, 1, 40, 20, 30, 50, 10);

            stack.PopRange(4);
            Equal(20, 30, 50, 10);

            stack.Clear();

            Equal();
        }

        /// <summary>
        /// Ensures that a view created over a stack that already has elements enumerates in the same
        /// order as the source (from the top), and that it stays in sync with the source afterwards.
        /// </summary>
        [Test]
        public void ViewCreatedFromNonEmptySource()
        {
            var stack = new ObservableStack<int>(new[] { 10, 50, 30, 20, 40 });
            var view = stack.CreateView(x => new ViewContainer<int>(x));

            void Equal(params int[] expected)
            {
                Assert.AreEqual(expected, stack);
                Assert.AreEqual(expected, view.Select(x => x.Value));
            }

            Equal(40, 20, 30, 50, 10);

            stack.Push(1);
            Equal(1, 40, 20, 30, 50, 10);

            stack.PushRange(new[] { 2, 3 });
            Equal(3, 2, 1, 40, 20, 30, 50, 10);

            Assert.AreEqual(3, stack.Pop());
            Equal(2, 1, 40, 20, 30, 50, 10);

            stack.PopRange(2);
            Equal(40, 20, 30, 50, 10);

            stack.Clear();
            Equal();
        }

        [Test]
        public void PopFromViewCreatedFromNonEmptySource()
        {
            var stack = new ObservableStack<int>(new[] { 1, 2, 3 });
            var view = stack.CreateView(x => x);
            using var notify = view.ToNotifyCollectionChanged();

            var removedFromSource = -1;
            stack.CollectionChanged += (in NotifyCollectionChangedEventArgs<int> e) => removedFromSource = e.OldItem;

            var removedFromNotify = -1;
            notify.CollectionChanged += (sender, e) => removedFromNotify = (int)e.OldItems![0]!;

            Assert.AreEqual(3, stack.Pop());

            Assert.AreEqual(3, removedFromSource);
            Assert.AreEqual(3, removedFromNotify);
            Assert.AreEqual(new[] { 2, 1 }, stack);
            Assert.AreEqual(new[] { 2, 1 }, view);
            Assert.AreEqual(new[] { 2, 1 }, notify);
        }
    }
}
