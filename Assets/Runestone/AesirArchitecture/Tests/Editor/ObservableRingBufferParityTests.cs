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
    public class ObservableRingBufferParityTests
    {
        [Test]
        public void Standard()
        {
            var buf = new ObservableRingBuffer<int>();
            var view = buf.CreateView(x => new ViewContainer<int>(x));

            buf.AddLast(10);
            buf.AddLast(50);
            buf.AddLast(30);
            buf.AddLast(20);
            buf.AddLast(40);

            void Equal(params int[] expected)
            {
                Assert.AreEqual(expected, buf);
                Assert.AreEqual(expected, view.Select(x => x.Value));
            }

            Equal(10, 50, 30, 20, 40);

            buf[2] = 99;
            Equal(10, 50, 99, 20, 40);

            buf.AddFirst(1000);
            Equal(1000, 10, 50, 99, 20, 40);

            Assert.AreEqual(1000, buf.RemoveFirst());
            Equal(10, 50, 99, 20, 40);

            Assert.AreEqual(40, buf.RemoveLast());
            Equal(10, 50, 99, 20);

            buf.AddLastRange(new[] { 1, 2, 3 });
            buf.AddLastRange(new[] { 4, 5, 6 }.AsSpan());
            buf.AddLastRange(new[] { 7, 8, 9 }.AsEnumerable());

            Equal(10, 50, 99, 20, 1, 2, 3, 4, 5, 6, 7, 8, 9);

            buf.Clear();
            Equal();

            buf.AddFirst(9999);
            Equal(9999);
        }


        [Test]
        public void FixedSize()
        {
            var buf = new ObservableFixedSizeRingBuffer<int>(5);
            var view = buf.CreateView(x => new ViewContainer<int>(x));

            void Equal(params int[] expected)
            {
                Assert.AreEqual(expected, buf);
                Assert.AreEqual(expected, view.Select(x => x.Value));
            }

            buf.AddLast(10);
            buf.AddLast(50);
            buf.AddLast(30);
            buf.AddLast(20);
            buf.AddLast(40);

            Equal(10, 50, 30, 20, 40);

            buf.AddLast(100);
            Equal(50, 30, 20, 40, 100);

            buf.AddFirst(99);
            Equal(99, 50, 30, 20, 40);

            buf[0] = 10;
            buf[2] = 99;
            Equal(10, 50, 99, 20, 40);

            buf.AddFirst(1000);
            Equal(1000, 10, 50, 99, 20);

            Assert.AreEqual(1000, buf.RemoveFirst());
            Equal(10, 50, 99, 20);

            Assert.AreEqual(20, buf.RemoveLast());
            Equal(10, 50, 99);

            buf.AddLastRange(new[] { 1, 2, 3 });
            Equal(50, 99, 1, 2, 3);
            buf.AddLastRange(new[] { 4, 5, 6 }.AsSpan());
            Equal(2, 3, 4, 5, 6);
            buf.AddLastRange(new[] { 7, 8, 9 }.AsEnumerable());
            Equal(5, 6, 7, 8, 9);

            buf.Clear();
            Equal();

            buf.AddLastRange(new int[] { });
            Equal();

            buf.AddLastRange(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
            Equal(8, 9, 10, 11, 12);

            buf.AddLastRange(new[] { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120 }.AsSpan());
            Equal(80, 90, 100, 110, 120);
        }
    }
}
