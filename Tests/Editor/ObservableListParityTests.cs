// 上游行为对齐测试：移植自 Cysharp/ObservableCollections 的测试套件
// （原测试使用 xUnit + FluentAssertions，此处转换为 NUnit 断言并保持用例语义一致）。

using System.Collections.ObjectModel;
using NUnit.Framework;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    /// <summary>
    /// <see cref="ObservableList{T}" /> 的写操作结果与 BCL <see cref="ObservableCollection{T}" /> 一致。
    /// </summary>
    public class ObservableListParityTests
    {
        [Test]
        public void Mutation_MatchesObservableCollection()
        {
            var reference = new ObservableCollection<int>();
            var list = new ObservableList<int>();

            void Equal(params int[] expected)
            {
                Assert.AreEqual(expected, reference);
                Assert.AreEqual(expected, list);
            }

            list.Add(10);
            reference.Add(10); // 0
            list.Add(50);
            reference.Add(50); // 1
            list.Add(30);
            reference.Add(30); // 2
            list.Add(20);
            reference.Add(20); // 3
            list.Add(40);
            reference.Add(40); // 4
            Equal(10, 50, 30, 20, 40);

            list.Move(3, 1);
            reference.Move(3, 1);
            Equal(10, 20, 50, 30, 40);

            list.Insert(2, 99);
            reference.Insert(2, 99);
            Equal(10, 20, 99, 50, 30, 40);

            list.RemoveAt(2);
            reference.RemoveAt(2);
            Equal(10, 20, 50, 30, 40);

            list[3] = 88;
            reference[3] = 88;
            Equal(10, 20, 50, 88, 40);

            list.Clear();
            reference.Clear();
            Equal();

            // BCL 无 Range 重载，逐项施加同一变更后比对
            list.AddRange(new[] { 100, 200, 300 });
            reference.Add(100);
            reference.Add(200);
            reference.Add(300);
            Equal(100, 200, 300);

            list.InsertRange(1, new[] { 400, 500, 600 });
            reference.Insert(1, 400);
            reference.Insert(2, 500);
            reference.Insert(3, 600);
            Equal(100, 400, 500, 600, 200, 300);

            list.RemoveRange(2, 2);
            reference.RemoveAt(2);
            reference.RemoveAt(2);
            Equal(100, 400, 200, 300);
        }
    }
}
