// 上游行为对齐测试：移植自 Cysharp/ObservableCollections 的测试套件
// （原测试使用 xUnit + FluentAssertions，此处转换为 NUnit 断言并保持用例语义一致）。

using System.Collections.Specialized;
using System.Diagnostics;
using NUnit.Framework;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    public class NotifyCollectionChangedBindingParityTests
    {
        [Test]
        public void ToNotifyCollectionChanged()
        {
            var list = new ObservableList<int>();

            list.Add(10);
            list.Add(20);
            list.Add(30);

            var notify = list.CreateView(x => $"${x}").ToNotifyCollectionChanged();

            list.Add(40);
            list.Add(50);

            using var e = notify.GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("$10", e.Current);
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("$20", e.Current);
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("$30", e.Current);
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("$40", e.Current);
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("$50", e.Current);
            Assert.IsFalse(e.MoveNext());
        }

        [Test]
        public void ToNotifyCollectionChanged_Filter()
        {
            var list = new ObservableList<int>();

            list.Add(1);
            list.Add(2);
            list.Add(5);
            list.Add(3);

            var view = list.CreateView(x => $"${x}");
            var notify = view.ToNotifyCollectionChanged();

            view.AttachFilter((value) => value % 2 == 0);

            list.Add(4);

            using var e = notify.GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("$2", e.Current);
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("$4", e.Current);
            Assert.IsFalse(e.MoveNext());
        }

        [Test]
        public void ToNotifyCollectionChanged_Move()
        {
            var list = new ObservableList<int> { 0, 1, 2, 3 };

            var view = list.CreateView(i => i)
                .ToNotifyCollectionChanged();

            int moveEventCount = 0;

            view.CollectionChanged += (_, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Move)
                    moveEventCount++;
            };

            list.Move(0, 1);

            Assert.AreEqual(1, moveEventCount);
        }
}

}