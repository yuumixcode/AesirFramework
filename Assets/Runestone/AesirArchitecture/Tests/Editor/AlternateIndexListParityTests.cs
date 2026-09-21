// 上游行为对齐测试：移植自 Cysharp/ObservableCollections 的测试套件
// （原测试使用 xUnit + FluentAssertions，此处转换为 NUnit 断言并保持用例语义一致）。

using Runestone.AesirArchitecture.Internal;
using NUnit.Framework;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    public class AlternateIndexListParityTests
    {
        [Test]
        public void Insert()
        {
            var list = new AlternateIndexList<string>();

            list.Insert(0, "foo");
            list.Insert(1, "bar");
            list.Insert(2, "baz");
            Assert.AreEqual(new[] { (0, "foo"), (1, "bar"), (2, "baz") }, list.GetIndexedValues());

            list.Insert(1, "new-bar");
            Assert.AreEqual(new[] { (0, "foo"), (1, "new-bar"), (2, "bar"), (3, "baz") }, list.GetIndexedValues());


            list.Insert(6, "zoo");
            Assert.AreEqual(new[] { (0, "foo"), (1, "new-bar"), (2, "bar"), (3, "baz"), (6, "zoo") }, list.GetIndexedValues());
        }

        [Test]
        public void InsertRange()
        {
            var list = new AlternateIndexList<string>();

            list.Insert(0, "foo");
            list.Insert(1, "bar");
            list.Insert(2, "baz");

            list.InsertRange(1, new[] { "new-foo", "new-bar", "new-baz" });
            Assert.AreEqual(new[] { (0, "foo"), (1, "new-foo"), (2, "new-bar"), (3, "new-baz"), (4, "bar"), (5, "baz") }, list.GetIndexedValues());
        }

        [Test]
        public void InsertSparsed()
        {
            var list = new AlternateIndexList<string>();

            list.Insert(2, "foo");
            list.Insert(8, "baz"); // baz
            list.Insert(4, "bar");
            Assert.AreEqual(new[] { (2, "foo"), (4, "bar"), (9, "baz") }, list.GetIndexedValues());

            list.InsertRange(3, new[] { "new-foo", "new-bar", "new-baz" });
            Assert.AreEqual(new[] { (2, "foo"), (3, "new-foo"), (4, "new-bar"), (5, "new-baz"), (7, "bar"), (12, "baz") }, list.GetIndexedValues());

            list.InsertRange(1, new[] { "zoo" });
            Assert.AreEqual(new[] { (1, "zoo"), (3, "foo"), (4, "new-foo"), (5, "new-bar"), (6, "new-baz"), (8, "bar"), (13, "baz") }, list.GetIndexedValues());
        }

        [Test]
        public void Remove()
        {
            var list = new AlternateIndexList<string>();

            list.Insert(0, "foo");
            list.Insert(1, "bar");
            list.Insert(2, "baz");

            list.Remove("bar");
            Assert.AreEqual(new[] { (0, "foo"), (1, "baz") }, list.GetIndexedValues());

            list.RemoveAt(0);
            Assert.AreEqual(new[] { (0, "baz") }, list.GetIndexedValues());
        }

        [Test]
        public void RemoveRange()
        {
            var list = new AlternateIndexList<string>();

            list.Insert(0, "foo");
            list.Insert(1, "bar");
            list.Insert(2, "baz");

            list.RemoveRange(1, 2);
            Assert.AreEqual(new[] { (0, "foo") }, list.GetIndexedValues());
        }

        [Test]
        public void TryGetSet()
        {
            var list = new AlternateIndexList<string>();

            list.Insert(0, "foo");
            list.Insert(2, "bar");
            list.Insert(4, "baz");

            Assert.IsTrue(list.TryGetAtAlternateIndex(2, out var bar));
            Assert.AreEqual("bar", bar);

            Assert.IsTrue(list.TrySetAtAlternateIndex(4, "new-baz", out var i));
            Assert.IsTrue(list.TryGetAtAlternateIndex(4, out var baz));
            Assert.AreEqual("new-baz", baz);
        }

        [Test]
        public void TryReplaceByValue()
        {
            var list = new AlternateIndexList<string>();

            list.Insert(0, "foo");
            list.Insert(2, "bar");
            list.Insert(4, "baz");

            list.TryReplaceByValue("bar", "new-bar", out var i);
            Assert.AreEqual(new[] { (0, "foo"), (2, "new-bar"), (4, "baz") }, list.GetIndexedValues());
        }
}

}