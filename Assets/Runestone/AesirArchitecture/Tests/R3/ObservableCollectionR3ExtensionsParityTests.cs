// 上游行为对齐测试：移植自 Cysharp/ObservableCollections 的 ObservableCollections.R3.Tests。
// 仅在检测到 R3 时编译（程序集 defineConstraints: AESIR_R3）。

using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;
using R3;
using Runestone.AesirArchitecture;
using Runestone.AesirArchitecture.R3;

namespace Runestone.AesirArchitecture.Tests.R3
{
    public class ObservableCollectionExtensionsTest
    {
        [Test]
        public void ObserveAdd()
        {
            var events = new List<CollectionAddEvent<int>>();

            var collection = new ObservableList<int>();

            var subscription = collection.ObserveAdd().Subscribe(ev => events.Add(ev));
            collection.Add(10);
            collection.Add(50);
            collection.Add(30);

            Assert.AreEqual(3, events.Count);
            Assert.AreEqual(0, events[0].Index);
            Assert.AreEqual(10, events[0].Value);
            Assert.AreEqual(1, events[1].Index);
            Assert.AreEqual(50, events[1].Value);
            Assert.AreEqual(2, events[2].Index);
            Assert.AreEqual(30, events[2].Value);

            subscription.Dispose();

            collection.Add(100);
            Assert.AreEqual(3, events.Count);
        }

        [Test]
        public void ObserveAdd_CancellationToken()
        {
            var cts = new CancellationTokenSource();
            var events = new List<CollectionAddEvent<int>>();
            var result = default(Result?);

            var collection = new ObservableList<int>();

            var subscription = collection.ObserveAdd(cts.Token).Subscribe(ev => events.Add(ev), x => result = x);
            collection.Add(10);
            collection.Add(50);
            collection.Add(30);

            Assert.AreEqual(3, events.Count);

            cts.Cancel();

            Assert.IsTrue(result.HasValue);

            subscription.Dispose();

            collection.Add(100);
            Assert.AreEqual(3, events.Count);
        }

        [Test]
        public void ObserveDictionaryAdd()
        {
            var events = new List<DictionaryAddEvent<int, string>>();

            var dictionary = new ObservableDictionary<int, string>();

            var subscription = dictionary.ObserveDictionaryAdd().Subscribe(ev => events.Add(ev));
            dictionary.Add(0, "zero");
            dictionary.Add(1, "one");
            dictionary.Add(2, "two");

            Assert.AreEqual(3, events.Count);
            Assert.AreEqual(0, events[0].Key);
            Assert.AreEqual("zero", events[0].Value);
            Assert.AreEqual(1, events[1].Key);
            Assert.AreEqual("one", events[1].Value);
            Assert.AreEqual(2, events[2].Key);
            Assert.AreEqual("two", events[2].Value);

            subscription.Dispose();

            dictionary.Add(4, "four");
            Assert.AreEqual(3, events.Count);
        }

        [Test]
        public void ObserveRemove()
        {
            var events = new List<CollectionRemoveEvent<int>>();
            var collection = new ObservableList<int>(new[] { 111, 222, 333 });
            var cts = new CancellationTokenSource();
            var result = default(Result?);

            var subscription = collection.ObserveRemove(cts.Token).Subscribe(ev => events.Add(ev), x => result = x);
            collection.RemoveAt(1);

            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(1, events[0].Index);
            Assert.AreEqual(222, events[0].Value);

            cts.Cancel();
            Assert.IsTrue(result.HasValue);

            subscription.Dispose();

            collection.RemoveAt(0);
            Assert.AreEqual(1, events.Count);
        }

        [Test]
        public void ObserveDictionaryRemove()
        {
            var events = new List<DictionaryRemoveEvent<int, string>>();
            var dictionary = new ObservableDictionary<int,string>
            {
                { 0, "zero" },
                { 1, "one" },
                { 2, "two" }
            };
            var cts = new CancellationTokenSource();
            var result = default(Result?);

            var subscription = dictionary.ObserveDictionaryRemove((cts.Token)).Subscribe(ev => events.Add(ev), x => result = x);
            dictionary.Remove(0);

            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(0, events[0].Key);
            Assert.AreEqual("zero", events[0].Value);

            cts.Cancel();
            Assert.IsTrue(result.HasValue);

            subscription.Dispose();

            dictionary.Remove(1);
            Assert.AreEqual(1, events.Count);
        }

        [Test]
        public void ObserveReplace()
        {
            var events = new List<CollectionReplaceEvent<int>>();
            var collection = new ObservableList<int>(new[] { 111, 222, 333 });
            var cts = new CancellationTokenSource();
            var result = default(Result?);

            var subscription = collection.ObserveReplace(cts.Token).Subscribe(ev => events.Add(ev), x => result = x);
            collection[1] = 999;

            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(1, events[0].Index);
            Assert.AreEqual(222, events[0].OldValue);
            Assert.AreEqual(999, events[0].NewValue);

            cts.Cancel();
            Assert.IsTrue(result.HasValue);

            subscription.Dispose();

            collection[1] = 444;
            Assert.AreEqual(1, events.Count);
        }
    
        [Test]
        public void ObserveDictionaryReplace()
        {
            var events = new List<DictionaryReplaceEvent<int,string>>();
            var dictionary = new ObservableDictionary<int, string>()
            {
                { 0, "zero" },
                { 1, "one" },
                { 2, "two" }
            };
            var cts = new CancellationTokenSource();
            var result = default(Result?);

            var subscription = dictionary.ObserveDictionaryReplace(cts.Token).Subscribe(ev => events.Add(ev), x => result = x);
            dictionary[1] = "ten";

            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(1, events[0].Key);
            Assert.AreEqual("one", events[0].OldValue);
            Assert.AreEqual("ten", events[0].NewValue);

            cts.Cancel();
            Assert.IsTrue(result.HasValue);

            subscription.Dispose();

            dictionary[1] = "one hundred";
            Assert.AreEqual(1, events.Count);
        }

        [Test]
        public void ObserveMove()
        {
            var events = new List<CollectionMoveEvent<int>>();
            var collection = new ObservableList<int>(new[] { 111, 222, 333 });
            var cts = new CancellationTokenSource();
            var result = default(Result?);

            var subscription = collection.ObserveMove(cts.Token).Subscribe(ev => events.Add(ev), x => result = x);

            collection.Move(1, 2);

            Assert.AreEqual(1, events.Count);
            Assert.AreEqual(1, events[0].OldIndex);
            Assert.AreEqual(2, events[0].NewIndex);
            Assert.AreEqual(222, events[0].Value);

            cts.Cancel();
            Assert.IsTrue(result.HasValue);

            subscription.Dispose();

            collection.Move(1, 2);
            Assert.AreEqual(1, events.Count);
        }

        [Test]
        public void ObserveCountChanged()
        {
            var events = new List<int>();
            var collection = new ObservableList<int>(new[] { 111, 222, 333 });

            using var _ = collection.ObserveCountChanged().Subscribe(count => events.Add(count));

            Assert.IsEmpty(events);

            collection.Add(444);
            Assert.AreEqual(4, events[0]);

            collection.Remove(111);
            Assert.AreEqual(3, events[1]);

            collection.Move(0, 1);
            Assert.AreEqual(2, events.Count);

            collection[0] = 999;
            Assert.AreEqual(2, events.Count);

            collection.Clear();
            Assert.AreEqual(0, events[2]);

            collection.Clear();
            Assert.AreEqual(3, events.Count);
        }

        [Test]
        public void ObserveCountChanged_NotifyCurrent()
        {
            var events = new List<int>();
            var collection = new ObservableList<int>(new[] { 111, 222, 333 });

            var subscription = collection.ObserveCountChanged(notifyCurrentCount: true)
                .Subscribe(count => events.Add(count));
            Assert.AreEqual(3, events[0]);
        }
    }
}
