// 上游行为对齐测试：移植自 Cysharp/ObservableCollections 的测试套件
// （原测试使用 xUnit + FluentAssertions，此处转换为 NUnit 断言并保持用例语义一致）。

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    public class RingBufferParityTests
    {
        [Test]
        public void All()
        {
            var list = new RingBuffer<int>();

            // befin from last...
            list.AddLast(1);Assert.AreEqual(new[] { 1 }, list);
            list.AddLast(2);Assert.AreEqual(new[] { 1, 2 }, list);
            list.AddLast(3);Assert.AreEqual(new[] { 1, 2, 3 }, list);
            list.AddLast(4);Assert.AreEqual(new[] { 1, 2, 3, 4 }, list);
            list.AddLast(5);Assert.AreEqual(new[] { 1, 2, 3, 4, 5 }, list);
            list.AddLast(6);Assert.AreEqual(new[] { 1, 2, 3, 4, 5, 6 }, list);

            Assert.AreEqual(6, list.RemoveLast());
            Assert.AreEqual(5, list.RemoveLast());
            Assert.AreEqual(new[] { 1, 2, 3, 4 }, list);
            Assert.AreEqual(new[] { 4, 3, 2, 1 }, list.Reverse());

            Assert.AreEqual(1, list.RemoveFirst());
            Assert.AreEqual(2, list.RemoveFirst());
            Assert.AreEqual(new[] { 3, 4 }, list);

            list.AddFirst(99);
            list.AddLast(88);
            Assert.AreEqual(new[] { 99, 3, 4, 88 }, list);

            // Adding Loop
            list.AddLast(5);
            list.AddLast(6);
            list.AddLast(7);
            list.AddLast(8);
            Assert.AreEqual(new[] { 99, 3, 4, 88, 5, 6, 7, 8 }, list);
            Assert.AreEqual(new[] { 8, 7, 6, 5, 88, 4, 3, 99 }, list.Reverse());


            // copy
            {
                var newArray = new int[10];
                list.CopyTo(newArray, 0);
                Assert.AreEqual(new[] { 99, 3, 4, 88, 5, 6, 7, 8, 0, 0 }, newArray);
            }
            {
                var newArray = new int[10];
                list.CopyTo(newArray, 1);
                Assert.AreEqual(new[] { 0, 99, 3, 4, 88, 5, 6, 7, 8, 0 }, newArray);
            }

            list.Clear();

            // befin from first...
            list.AddFirst(1);
            list.AddFirst(2);
            list.AddFirst(3);
            list.AddFirst(4);
            list.AddFirst(5);
            list.AddFirst(6);

            Assert.AreEqual(new[] { 6, 5, 4, 3, 2, 1 }, list);
            Assert.AreEqual(new[] { 1, 2, 3, 4, 5, 6 }, list.Reverse());

            Assert.AreEqual(1, list.RemoveLast());
            Assert.AreEqual(2, list.RemoveLast());
            Assert.AreEqual(new[] { 6, 5, 4, 3 }, list);

            Assert.AreEqual(6, list.RemoveFirst());
            Assert.AreEqual(5, list.RemoveFirst());
            Assert.AreEqual(new[] { 4, 3 }, list);

            list.AddFirst(99);
            list.AddLast(88);
            Assert.AreEqual(new[] { 99, 4, 3, 88 }, list);

            list.AddFirst(5);
            list.AddFirst(6);
            list.AddFirst(7);
            list.AddFirst(8);
            Assert.AreEqual(new[] { 8, 7, 6, 5, 99, 4, 3, 88 }, list);

            // set, get
            Assert.AreEqual(8, list[0]);
            Assert.AreEqual(7, list[1]);
            Assert.AreEqual(6, list[2]);
            Assert.AreEqual(5, list[3]);
            Assert.AreEqual(99, list[4]);
            Assert.AreEqual(4, list[5]);
            Assert.AreEqual(3, list[6]);
            Assert.AreEqual(88, list[7]);

            list[0] = 999;
            list[4] = 1099;
            list[7] = 888;

            // ensure capacity
            list.AddFirst(9);
            Assert.AreEqual(new[] { 9, 999, 7, 6, 5, 1099, 4, 3, 888 }, list);
            Assert.AreEqual(new[] { 888, 3, 4, 1099, 5, 6, 7, 999, 9 }, list.Reverse());

            list.AddFirst(199);
            list.AddLast(299);
            Assert.AreEqual(new[] { 199, 9, 999, 7, 6, 5, 1099, 4, 3, 888, 299 }, list);
            Assert.AreEqual(new[] { 299, 888, 3, 4, 1099, 5, 6, 7, 999, 9, 199 }, list.Reverse());

            // copy
            {
                var newArray = new int[15];
                list.CopyTo(newArray, 0);
                Assert.AreEqual(new[] { 199, 9, 999, 7, 6, 5, 1099, 4, 3, 888, 299, 0, 0, 0, 0 }, newArray);
            }
            {
                var newArray = new int[15];
                list.CopyTo(newArray, 2);
                Assert.AreEqual(new[] { 0, 0, 199, 9, 999, 7, 6, 5, 1099, 4, 3, 888, 299, 0, 0 }, newArray);
            }

        }

        [Test]
        public void Iteration()
        {
            var empty = new RingBuffer<int>();
            Assert.IsEmpty(empty.ToArray());


            for (int i = 0; i < 10; i++)
            {
                var buffer = new RingBuffer<int>();
                for (int j = 0; j < i; j++)
                {
                    buffer.AddLast(j);
                }
                Assert.AreEqual(Enumerable.Range(0, i).ToArray(), buffer.ToArray());
            }

            for (int i = 0; i < 10; i++)
            {
                var buffer = new RingBuffer<int>();
                for (int j = 0; j < i; j++)
                {
                    buffer.AddFirst(j);
                }
                Assert.AreEqual(Enumerable.Range(0, i).Reverse().ToArray(), buffer.ToArray());
            }
        }

        [Test]
        public void RandomIteration()
        {
            var buffer = new RingBuffer<int>();
            buffer.AddFirst(10);
            buffer.AddLast(20);
            buffer.AddLast(30);
            buffer.AddFirst(40);

            Assert.AreEqual(new[] { 40, 10, 20, 30 }, buffer.ToArray());
        }

        [Test]
        public void BinarySearchTest()
        {
            var empty = new RingBuffer<int>(new int[] { });
            var emptyL = new List<int>();
            var single = new RingBuffer<int>(new[] { 10 });
            var singleL = new List<int>(new[] { 10 });
            var buffer = new RingBuffer<int>(new[]
            {
                1, 4, 5, 6, 10, 14, 15,17, 20, 33
            });
            var multiL = new List<int>(new[]
            {
                1, 4, 5, 6, 10, 14, 15,17, 20, 33
            });

            Assert.Less(empty.BinarySearch(99), 0);
            Assert.AreEqual(emptyL.BinarySearch(99), empty.BinarySearch(99));
            {
                Assert.AreEqual(0, single.BinarySearch(10));
                var x1 = single.BinarySearch(4);
                Assert.Less(x1, 0);
                Assert.AreEqual(0, (~x1));
                Assert.AreEqual(single.BinarySearch(4), x1);

                var x2 = single.BinarySearch(40);
                Assert.Less(x2, 0);
                Assert.AreEqual(1, (~x2));
                Assert.AreEqual(single.BinarySearch(40), x2);
            }

            {
                for (int i = 0; i < 50; i++)
                {
                    Assert.AreEqual(multiL.BinarySearch(i), buffer.BinarySearch(i));
                }
            }





        }
    }
}
