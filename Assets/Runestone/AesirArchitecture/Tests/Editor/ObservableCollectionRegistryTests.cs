// 上游行为对齐测试之外的项目自有测试：可观察集合调试注册表。
// 上游无调试注册表（本项目新增），此处锁定其登记 / 注销 / 弱引用清理语义。

using System.Collections.Generic;
using NUnit.Framework;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    /// <summary>
    /// <see cref="ObservableCollectionRegistry" /> 的行为测试。
    /// </summary>
    public class ObservableCollectionRegistryTests
    {
        [SetUp]
        public void SetUp() => ObservableCollectionRegistry.Clear();

        [TearDown]
        public void TearDown() => ObservableCollectionRegistry.Clear();

        [Test]
        public void 构造集合_自动登记()
        {
            var list = new ObservableList<int>();
            var set = new ObservableHashSet<int>();
            var queue = new ObservableQueue<int>();

            var collections = ObservableCollectionRegistry.GetLiveCollections();

            Assert.AreEqual(3, collections.Count);
            Assert.Contains(list, collections);
            Assert.Contains(set, collections);
            Assert.Contains(queue, collections);
        }

        [Test]
        public void CreateView_自动登记视图并关联源集合()
        {
            var list = new ObservableList<int>();
            using var view = list.CreateView(x => x * 2);

            var views = ObservableCollectionRegistry.GetLiveViews();

            Assert.AreEqual(1, views.Count);
            Assert.AreSame(list, views[0].Source);
            Assert.AreSame(view, views[0].View);
        }

        [Test]
        public void Unregister_仅移除目标实例()
        {
            var first = new ObservableList<int>();
            var second = new ObservableList<int>();

            ObservableCollectionRegistry.Unregister(first);

            var collections = ObservableCollectionRegistry.GetLiveCollections();
            Assert.AreEqual(1, collections.Count);
            Assert.AreSame(second, collections[0]);
        }

        [Test]
        public void Clear_清空集合与视图()
        {
            var list = new ObservableList<int>();
            using var view = list.CreateView(x => x);

            ObservableCollectionRegistry.Clear();

            Assert.AreEqual(0, ObservableCollectionRegistry.GetLiveCollections().Count);
            Assert.AreEqual(0, ObservableCollectionRegistry.GetLiveViews().Count);
        }

        [Test]
        public void 已回收实例_在下一次读取时清理()
        {
            // 用独立方法创建，确保引用离开作用域后可被回收
            CreateTemporaryCollection();

            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();

            var collections = ObservableCollectionRegistry.GetLiveCollections();

            // 弱引用已失效的条目不再出现在结果中（未被 GC 时最多 1 条，但绝不含已回收实例）
            foreach (var collection in collections)
            {
                Assert.IsNotNull(collection);
            }
        }

        [Test]
        public void Register_空引用被忽略()
        {
            ObservableCollectionRegistry.Register(null);

            Assert.AreEqual(0, ObservableCollectionRegistry.GetLiveCollections().Count);
        }

        static void CreateTemporaryCollection()
        {
            var temporary = new ObservableList<int>();
            temporary.AddRange(new[] { 1, 2, 3 });
        }
    }
}
