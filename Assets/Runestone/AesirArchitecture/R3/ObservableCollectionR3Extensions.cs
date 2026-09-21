// R3 响应式扩展：移植自 Cysharp/ObservableCollections 的 ObservableCollections.R3 包。
// 上游使用 C# 12 语法（file-scoped namespace / record struct / 主构造器），
// 此处降级为 Unity 2022.3（C# 9）可编译的等价写法；R3 侧 API 与语义保持一致。

#nullable enable
using Runestone.AesirArchitecture;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Threading;
using R3;

namespace Runestone.AesirArchitecture.R3
{
    public readonly struct CollectionAddEvent<T>
    {
        public readonly int Index;
        public readonly T Value;

        public CollectionAddEvent(int Index, T Value)
        {
            this.Index = Index;
            this.Value = Value;
        }
    }
    public readonly struct CollectionRemoveEvent<T>
    {
        public readonly int Index;
        public readonly T Value;

        public CollectionRemoveEvent(int Index, T Value)
        {
            this.Index = Index;
            this.Value = Value;
        }
    }
    public readonly struct CollectionReplaceEvent<T>
    {
        public readonly int Index;
        public readonly T OldValue;
        public readonly T NewValue;

        public CollectionReplaceEvent(int Index, T OldValue, T NewValue)
        {
            this.Index = Index;
            this.OldValue = OldValue;
            this.NewValue = NewValue;
        }
    }
    public readonly struct CollectionMoveEvent<T>
    {
        public readonly int OldIndex;
        public readonly int NewIndex;
        public readonly T Value;

        public CollectionMoveEvent(int OldIndex, int NewIndex, T Value)
        {
            this.OldIndex = OldIndex;
            this.NewIndex = NewIndex;
            this.Value = Value;
        }
    }
    public readonly struct CollectionResetEvent<T>
    {
        readonly SortOperation<T> sortOperation;

        public bool IsClear => sortOperation.IsClear;
        public bool IsSort => sortOperation.IsSort;
        public bool IsReverse => sortOperation.IsReverse;
        public int Index => sortOperation.Index;
        public int Count => sortOperation.Count;
        public IComparer<T>? Comparer => sortOperation.Comparer;

        public CollectionResetEvent(SortOperation<T> sortOperation)
        {
            this.sortOperation = sortOperation;
        }
    }

    [StructLayout(LayoutKind.Auto)]
    public readonly struct CollectionChangedEvent<T>
    {
        public readonly NotifyCollectionChangedAction Action;
        public readonly T NewItem;
        public readonly T OldItem;
        public readonly int NewStartingIndex;
        public readonly int OldStartingIndex;
        public readonly SortOperation<T> SortOperation;

        public CollectionChangedEvent(NotifyCollectionChangedAction action, T newItem, T oldItem, int newStartingIndex, int oldStartingIndex, SortOperation<T> sortOperation)
        {
            Action = action;
            NewItem = newItem;
            OldItem = oldItem;
            NewStartingIndex = newStartingIndex;
            OldStartingIndex = oldStartingIndex;
            SortOperation = sortOperation;
        }
    }

    public readonly struct DictionaryAddEvent<TKey, TValue>
    {
        public readonly TKey Key;
        public readonly TValue Value;

        public DictionaryAddEvent(TKey Key, TValue Value)
        {
            this.Key = Key;
            this.Value = Value;
        }
    }

    public readonly struct DictionaryRemoveEvent<TKey, TValue>
    {
        public readonly TKey Key;
        public readonly TValue Value;

        public DictionaryRemoveEvent(TKey Key, TValue Value)
        {
            this.Key = Key;
            this.Value = Value;
        }
    }

    public readonly struct DictionaryReplaceEvent<TKey, TValue>
    {
        public readonly TKey Key;
        public readonly TValue OldValue;
        public readonly TValue NewValue;

        public DictionaryReplaceEvent(TKey Key, TValue OldValue, TValue NewValue)
        {
            this.Key = Key;
            this.OldValue = OldValue;
            this.NewValue = NewValue;
        }
    }

    public static partial class ObservableCollectionR3Extensions
    {
        public static Observable<CollectionChangedEvent<T>> ObserveChanged<T>(this IObservableCollection<T> source, CancellationToken cancellationToken = default)
        {
            return new ObservableCollectionChanged<T>(source, cancellationToken);
        }

        public static Observable<CollectionAddEvent<T>> ObserveAdd<T>(this IObservableCollection<T> source, CancellationToken cancellationToken = default)
        {
            return new ObservableCollectionAdd<T>(source, cancellationToken);
        }

        public static Observable<CollectionRemoveEvent<T>> ObserveRemove<T>(this IObservableCollection<T> source, CancellationToken cancellationToken = default)
        {
            return new ObservableCollectionRemove<T>(source, cancellationToken);
        }

        public static Observable<CollectionReplaceEvent<T>> ObserveReplace<T>(this IObservableCollection<T> source, CancellationToken cancellationToken = default)
        {
            return new ObservableCollectionReplace<T>(source, cancellationToken);
        }

        public static Observable<CollectionMoveEvent<T>> ObserveMove<T>(this IObservableCollection<T> source, CancellationToken cancellationToken = default)
        {
            return new ObservableCollectionMove<T>(source, cancellationToken);
        }

        public static Observable<CollectionResetEvent<T>> ObserveReset<T>(this IObservableCollection<T> source, CancellationToken cancellationToken = default)
        {
            return new ObservableCollectionReset<T>(source, cancellationToken);
        }

        public static Observable<Unit> ObserveClear<T>(this IObservableCollection<T> source, CancellationToken cancellationToken = default)
        {
            return new ObservableCollectionClear<T>(source, cancellationToken);
        }

        public static Observable<(int Index, int Count)> ObserveReverse<T>(this IObservableCollection<T> source, CancellationToken cancellationToken = default)
        {
            return new ObservableCollectionReverse<T>(source, cancellationToken);
        }

        public static Observable<(int Index, int Count, IComparer<T>? Comparer)> ObserveSort<T>(this IObservableCollection<T> source, CancellationToken cancellationToken = default)
        {
            return new ObservableCollectionSort<T>(source, cancellationToken);
        }

        public static Observable<int> ObserveCountChanged<T>(this IObservableCollection<T> source, bool notifyCurrentCount = false, CancellationToken cancellationToken = default)
        {
            return new ObservableCollectionCountChanged<T>(source, notifyCurrentCount, cancellationToken);
        }
    }

    public static class ObservableDictionaryR3Extensions
    {
        public static Observable<DictionaryAddEvent<TKey, TValue>> ObserveDictionaryAdd<TKey, TValue>(this IReadOnlyObservableDictionary<TKey, TValue> source,
            CancellationToken cancellationToken = default)
        {
            return new ObservableDictionaryAdd<TKey, TValue>(source, cancellationToken);
        }

        public static Observable<DictionaryRemoveEvent<TKey, TValue>> ObserveDictionaryRemove<TKey, TValue>(this IReadOnlyObservableDictionary<TKey, TValue> source,
            CancellationToken cancellationToken = default)
        {
            return new ObservableDictionaryRemove<TKey, TValue>(source, cancellationToken);
        }
        public static Observable<DictionaryReplaceEvent<TKey, TValue>> ObserveDictionaryReplace<TKey, TValue>(this IReadOnlyObservableDictionary<TKey, TValue> source,
            CancellationToken cancellationToken = default)
        {
            return new ObservableDictionaryReplace<TKey, TValue>(source, cancellationToken);
        }
    }

    sealed class ObservableCollectionChanged<T> : Observable<CollectionChangedEvent<T>>
    {
        readonly IObservableCollection<T> collection;
        readonly CancellationToken cancellationToken;

        public ObservableCollectionChanged(IObservableCollection<T> collection, CancellationToken cancellationToken)
        {
            this.collection = collection;
            this.cancellationToken = cancellationToken;
        }

        protected override IDisposable SubscribeCore(Observer<CollectionChangedEvent<T>> observer)
        {
            return new _ObservableCollectionAdd(collection, observer, cancellationToken);
        }

        sealed class _ObservableCollectionAdd : ObservableCollectionObserverBase<T, CollectionChangedEvent<T>>
        {
            public _ObservableCollectionAdd(IObservableCollection<T> collection, Observer<CollectionChangedEvent<T>> observer, CancellationToken cancellationToken)
                : base(collection, observer, cancellationToken)
            {
            }

            protected override void Handler(in NotifyCollectionChangedEventArgs<T> eventArgs)
            {
                if (eventArgs.IsSingleItem)
                {
                    var newArgs = new CollectionChangedEvent<T>(
                        eventArgs.Action,
                        eventArgs.NewItem,
                        eventArgs.OldItem,
                        eventArgs.NewStartingIndex,
                        eventArgs.OldStartingIndex,
                        eventArgs.SortOperation);

                    observer.OnNext(newArgs);
                }
                else
                {
                    if (eventArgs.Action == NotifyCollectionChangedAction.Add)
                    {
                        var i = eventArgs.NewStartingIndex;
                        foreach (var item in eventArgs.NewItems)
                        {
                            var newArgs = new CollectionChangedEvent<T>(
                                eventArgs.Action,
                                item,
                                eventArgs.OldItem,
                                newStartingIndex: i,
                                eventArgs.OldStartingIndex,
                                eventArgs.SortOperation);

                            if (eventArgs.NewStartingIndex != -1) i++;

                            observer.OnNext(newArgs);
                        }
                    }
                    else if (eventArgs.Action == NotifyCollectionChangedAction.Remove)
                    {
                        foreach (var item in eventArgs.OldItems)
                        {
                            var newArgs = new CollectionChangedEvent<T>(
                                eventArgs.Action,
                                eventArgs.NewItem,
                                item,
                                eventArgs.NewStartingIndex,
                                eventArgs.OldStartingIndex, // removed, uses same index
                                eventArgs.SortOperation);

                            observer.OnNext(newArgs);
                        }
                    }
                }
            }
        }
    }

    sealed class ObservableCollectionAdd<T> : Observable<CollectionAddEvent<T>>
    {
        readonly IObservableCollection<T> collection;
        readonly CancellationToken cancellationToken;

        public ObservableCollectionAdd(IObservableCollection<T> collection, CancellationToken cancellationToken)
        {
            this.collection = collection;
            this.cancellationToken = cancellationToken;
        }

        protected override IDisposable SubscribeCore(Observer<CollectionAddEvent<T>> observer)
        {
            return new _ObservableCollectionAdd(collection, observer, cancellationToken);
        }

        sealed class _ObservableCollectionAdd : ObservableCollectionObserverBase<T, CollectionAddEvent<T>>
        {
            public _ObservableCollectionAdd(IObservableCollection<T> collection, Observer<CollectionAddEvent<T>> observer, CancellationToken cancellationToken)
                : base(collection, observer, cancellationToken)
            {
            }

            protected override void Handler(in NotifyCollectionChangedEventArgs<T> eventArgs)
            {
                if (eventArgs.Action == NotifyCollectionChangedAction.Add)
                {
                    if (eventArgs.IsSingleItem)
                    {
                        observer.OnNext(new CollectionAddEvent<T>(eventArgs.NewStartingIndex, eventArgs.NewItem));
                    }
                    else
                    {
                        var i = eventArgs.NewStartingIndex;
                        foreach (var item in eventArgs.NewItems)
                        {
                            observer.OnNext(new CollectionAddEvent<T>(i++, item));
                        }
                    }
                }
            }
        }
    }

    sealed class ObservableCollectionRemove<T> : Observable<CollectionRemoveEvent<T>>
    {
        readonly IObservableCollection<T> collection;
        readonly CancellationToken cancellationToken;

        public ObservableCollectionRemove(IObservableCollection<T> collection, CancellationToken cancellationToken)
        {
            this.collection = collection;
            this.cancellationToken = cancellationToken;
        }

        protected override IDisposable SubscribeCore(Observer<CollectionRemoveEvent<T>> observer)
        {
            return new _ObservableCollectionRemove(collection, observer, cancellationToken);
        }

        sealed class _ObservableCollectionRemove : ObservableCollectionObserverBase<T, CollectionRemoveEvent<T>>
        {
            public _ObservableCollectionRemove(IObservableCollection<T> collection, Observer<CollectionRemoveEvent<T>> observer, CancellationToken cancellationToken)
                : base(collection, observer, cancellationToken)
            {
            }

            protected override void Handler(in NotifyCollectionChangedEventArgs<T> eventArgs)
            {
                if (eventArgs.Action == NotifyCollectionChangedAction.Remove)
                {
                    if (eventArgs.IsSingleItem)
                    {
                        observer.OnNext(new CollectionRemoveEvent<T>(eventArgs.OldStartingIndex, eventArgs.OldItem));
                    }
                    else
                    {
                        foreach (var item in eventArgs.OldItems)
                        {
                            observer.OnNext(new CollectionRemoveEvent<T>(eventArgs.OldStartingIndex, item)); // remove uses same index
                        }
                    }
                }
            }
        }
    }

    sealed class ObservableCollectionReplace<T> : Observable<CollectionReplaceEvent<T>>
    {
        readonly IObservableCollection<T> collection;
        readonly CancellationToken cancellationToken;

        public ObservableCollectionReplace(IObservableCollection<T> collection, CancellationToken cancellationToken)
        {
            this.collection = collection;
            this.cancellationToken = cancellationToken;
        }

        protected override IDisposable SubscribeCore(Observer<CollectionReplaceEvent<T>> observer)
        {
            return new _ObservableCollectionReplace(collection, observer, cancellationToken);
        }

        sealed class _ObservableCollectionReplace : ObservableCollectionObserverBase<T, CollectionReplaceEvent<T>>
        {
            public _ObservableCollectionReplace(IObservableCollection<T> collection, Observer<CollectionReplaceEvent<T>> observer, CancellationToken cancellationToken)
                : base(collection, observer, cancellationToken)
            {
            }

            protected override void Handler(in NotifyCollectionChangedEventArgs<T> eventArgs)
            {
                if (eventArgs.Action == NotifyCollectionChangedAction.Replace)
                {
                    observer.OnNext(new CollectionReplaceEvent<T>(eventArgs.NewStartingIndex, eventArgs.OldItem, eventArgs.NewItem));
                }
            }
        }
    }

    sealed class ObservableCollectionMove<T> : Observable<CollectionMoveEvent<T>>
    {
        readonly IObservableCollection<T> collection;
        readonly CancellationToken cancellationToken;

        public ObservableCollectionMove(IObservableCollection<T> collection, CancellationToken cancellationToken)
        {
            this.collection = collection;
            this.cancellationToken = cancellationToken;
        }

        protected override IDisposable SubscribeCore(Observer<CollectionMoveEvent<T>> observer)
        {
            return new _ObservableCollectionMove(collection, observer, cancellationToken);
        }

        sealed class _ObservableCollectionMove : ObservableCollectionObserverBase<T, CollectionMoveEvent<T>>
        {
            public _ObservableCollectionMove(IObservableCollection<T> collection, Observer<CollectionMoveEvent<T>> observer, CancellationToken cancellationToken)
                : base(collection, observer, cancellationToken)
            {
            }

            protected override void Handler(in NotifyCollectionChangedEventArgs<T> eventArgs)
            {
                if (eventArgs.Action == NotifyCollectionChangedAction.Move)
                {
                    observer.OnNext(new CollectionMoveEvent<T>(eventArgs.OldStartingIndex, eventArgs.NewStartingIndex, eventArgs.NewItem));
                }
            }
        }
    }
    sealed class ObservableCollectionReset<T> : Observable<CollectionResetEvent<T>>
    {
        readonly IObservableCollection<T> collection;
        readonly CancellationToken cancellationToken;

        public ObservableCollectionReset(IObservableCollection<T> collection, CancellationToken cancellationToken)
        {
            this.collection = collection;
            this.cancellationToken = cancellationToken;
        }

        protected override IDisposable SubscribeCore(Observer<CollectionResetEvent<T>> observer)
        {
            return new _ObservableCollectionReset(collection, observer, cancellationToken);
        }

        sealed class _ObservableCollectionReset : ObservableCollectionObserverBase<T, CollectionResetEvent<T>>
        {
            public _ObservableCollectionReset(IObservableCollection<T> collection, Observer<CollectionResetEvent<T>> observer, CancellationToken cancellationToken)
                : base(collection, observer, cancellationToken)
            {
            }

            protected override void Handler(in NotifyCollectionChangedEventArgs<T> eventArgs)
            {
                if (eventArgs.Action == NotifyCollectionChangedAction.Reset)
                {
                    observer.OnNext(new CollectionResetEvent<T>(eventArgs.SortOperation));
                }
            }
        }
    }

    sealed class ObservableCollectionClear<T> : Observable<Unit>
    {
        readonly IObservableCollection<T> collection;
        readonly CancellationToken cancellationToken;

        public ObservableCollectionClear(IObservableCollection<T> collection, CancellationToken cancellationToken)
        {
            this.collection = collection;
            this.cancellationToken = cancellationToken;
        }

        protected override IDisposable SubscribeCore(Observer<Unit> observer)
        {
            return new _ObservableCollectionClear(collection, observer, cancellationToken);
        }

        sealed class _ObservableCollectionClear : ObservableCollectionObserverBase<T, Unit>
        {
            public _ObservableCollectionClear(IObservableCollection<T> collection, Observer<Unit> observer, CancellationToken cancellationToken)
                : base(collection, observer, cancellationToken)
            {
            }

            protected override void Handler(in NotifyCollectionChangedEventArgs<T> eventArgs)
            {
                if (eventArgs.Action == NotifyCollectionChangedAction.Reset && eventArgs.SortOperation.IsClear)
                {
                    observer.OnNext(Unit.Default);
                }
            }
        }
    }

    sealed class ObservableCollectionReverse<T> : Observable<(int Index, int Count)>
    {
        readonly IObservableCollection<T> collection;
        readonly CancellationToken cancellationToken;

        public ObservableCollectionReverse(IObservableCollection<T> collection, CancellationToken cancellationToken)
        {
            this.collection = collection;
            this.cancellationToken = cancellationToken;
        }

        protected override IDisposable SubscribeCore(Observer<(int Index, int Count)> observer)
        {
            return new _ObservableCollectionReverse(collection, observer, cancellationToken);
        }

        sealed class _ObservableCollectionReverse : ObservableCollectionObserverBase<T, (int Index, int Count)>
        {
            public _ObservableCollectionReverse(IObservableCollection<T> collection, Observer<(int Index, int Count)> observer, CancellationToken cancellationToken)
                : base(collection, observer, cancellationToken)
            {
            }

            protected override void Handler(in NotifyCollectionChangedEventArgs<T> eventArgs)
            {
                if (eventArgs.Action == NotifyCollectionChangedAction.Reset && eventArgs.SortOperation.IsReverse)
                {
                    observer.OnNext((eventArgs.SortOperation.Index, eventArgs.SortOperation.Count));
                }
            }
        }
    }

    sealed class ObservableCollectionSort<T> : Observable<(int Index, int Count, IComparer<T>? Comparer)>
    {
        readonly IObservableCollection<T> collection;
        readonly CancellationToken cancellationToken;

        public ObservableCollectionSort(IObservableCollection<T> collection, CancellationToken cancellationToken)
        {
            this.collection = collection;
            this.cancellationToken = cancellationToken;
        }

        protected override IDisposable SubscribeCore(Observer<(int Index, int Count, IComparer<T>? Comparer)> observer)
        {
            return new _ObservableCollectionSort(collection, observer, cancellationToken);
        }

        sealed class _ObservableCollectionSort : ObservableCollectionObserverBase<T, (int Index, int Count, IComparer<T>? Comparer)>
        {
            public _ObservableCollectionSort(IObservableCollection<T> collection, Observer<(int Index, int Count, IComparer<T>? Comparer)> observer, CancellationToken cancellationToken)
                : base(collection, observer, cancellationToken)
            {
            }

            protected override void Handler(in NotifyCollectionChangedEventArgs<T> eventArgs)
            {
                if (eventArgs.Action == NotifyCollectionChangedAction.Reset && eventArgs.SortOperation.IsSort)
                {
                    observer.OnNext(eventArgs.SortOperation.AsTuple());
                }
            }
        }
    }

    sealed class ObservableCollectionCountChanged<T> : Observable<int>
    {
        readonly IObservableCollection<T> collection;
        readonly bool notifyCurrentCount;
        readonly CancellationToken cancellationToken;

        public ObservableCollectionCountChanged(IObservableCollection<T> collection, bool notifyCurrentCount, CancellationToken cancellationToken)
        {
            this.collection = collection;
            this.notifyCurrentCount = notifyCurrentCount;
            this.cancellationToken = cancellationToken;
        }

        protected override IDisposable SubscribeCore(Observer<int> observer)
        {
            return new _ObservableCollectionCountChanged(collection, notifyCurrentCount, observer, cancellationToken);
        }

        sealed class _ObservableCollectionCountChanged : ObservableCollectionObserverBase<T, int>
        {
            int countPrev;

            public _ObservableCollectionCountChanged(
                IObservableCollection<T> collection,
                bool notifyCurrentCount,
                Observer<int> observer,
                CancellationToken cancellationToken) : base(collection, observer, cancellationToken)
            {
                this.countPrev = collection.Count;
                if (notifyCurrentCount)
                {
                    observer.OnNext(collection.Count);
                }
            }

            protected override void Handler(in NotifyCollectionChangedEventArgs<T> eventArgs)
            {
                switch (eventArgs.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                    case NotifyCollectionChangedAction.Remove:
                    case NotifyCollectionChangedAction.Reset when countPrev != collection.Count:
                        observer.OnNext(collection.Count);
                        break;
                }
                countPrev = collection.Count;
            }
        }
    }

    sealed class ObservableDictionaryAdd<TKey, TValue> : Observable<DictionaryAddEvent<TKey, TValue>>
    {
        readonly IReadOnlyObservableDictionary<TKey, TValue> dictionary;
        readonly CancellationToken cancellationToken;

        public ObservableDictionaryAdd(IReadOnlyObservableDictionary<TKey, TValue> dictionary, CancellationToken cancellationToken)
        {
            this.dictionary = dictionary;
            this.cancellationToken = cancellationToken;
        }

        protected override IDisposable SubscribeCore(Observer<DictionaryAddEvent<TKey, TValue>> observer)
        {
            return new _DictionaryCollectionAdd(dictionary, observer, cancellationToken);
        }

        sealed class _DictionaryCollectionAdd :
            ObservableCollectionObserverBase<KeyValuePair<TKey, TValue>, DictionaryAddEvent<TKey, TValue>>
        {
            public _DictionaryCollectionAdd(IObservableCollection<KeyValuePair<TKey, TValue>> collection, Observer<DictionaryAddEvent<TKey, TValue>> observer, CancellationToken cancellationToken)
                : base(collection,
                observer, cancellationToken)
            {
            }

            protected override void Handler(in NotifyCollectionChangedEventArgs<KeyValuePair<TKey, TValue>> eventArgs)
            {
                if (eventArgs.Action == NotifyCollectionChangedAction.Add)
                {
                    if (eventArgs.IsSingleItem)
                    {
                        observer.OnNext(
                            new DictionaryAddEvent<TKey, TValue>(eventArgs.NewItem.Key, eventArgs.NewItem.Value));
                    }
                    else
                    {
                        var i = eventArgs.NewStartingIndex;
                        foreach (var item in eventArgs.NewItems)
                        {
                            observer.OnNext(new DictionaryAddEvent<TKey, TValue>(item.Key, item.Value));
                        }
                    }
                }
            }
        }
    }

    sealed class ObservableDictionaryRemove<TKey, TValue> : Observable<DictionaryRemoveEvent<TKey, TValue>>
    {
        readonly IReadOnlyObservableDictionary<TKey, TValue> dictionary;
        readonly CancellationToken cancellationToken;

        public ObservableDictionaryRemove(IReadOnlyObservableDictionary<TKey, TValue> dictionary, CancellationToken cancellationToken)
        {
            this.dictionary = dictionary;
            this.cancellationToken = cancellationToken;
        }

        protected override IDisposable SubscribeCore(Observer<DictionaryRemoveEvent<TKey, TValue>> observer)
        {
            return new _DictionaryCollectionRemove(dictionary, observer, cancellationToken);
        }

        sealed class _DictionaryCollectionRemove :
            ObservableCollectionObserverBase<KeyValuePair<TKey, TValue>, DictionaryRemoveEvent<TKey, TValue>>
        {
            public _DictionaryCollectionRemove(IObservableCollection<KeyValuePair<TKey, TValue>> collection, Observer<DictionaryRemoveEvent<TKey, TValue>> observer, CancellationToken cancellationToken)
                : base(collection,
                observer, cancellationToken)
            {
            }

            protected override void Handler(in NotifyCollectionChangedEventArgs<KeyValuePair<TKey, TValue>> eventArgs)
            {
                if (eventArgs.Action == NotifyCollectionChangedAction.Remove)
                {
                    if (eventArgs.IsSingleItem)
                    {
                        observer.OnNext(
                            new DictionaryRemoveEvent<TKey, TValue>(eventArgs.OldItem.Key, eventArgs.OldItem.Value));
                    }
                    else
                    {
                        var i = eventArgs.NewStartingIndex;
                        foreach (var item in eventArgs.NewItems)
                        {
                            observer.OnNext(new DictionaryRemoveEvent<TKey, TValue>(item.Key, item.Value));
                        }
                    }
                }
            }
        }
    }

    sealed class ObservableDictionaryReplace<TKey, TValue> : Observable<DictionaryReplaceEvent<TKey, TValue>>
    {
        readonly IReadOnlyObservableDictionary<TKey, TValue> dictionary;
        readonly CancellationToken cancellationToken;

        public ObservableDictionaryReplace(IReadOnlyObservableDictionary<TKey, TValue> dictionary, CancellationToken cancellationToken)
        {
            this.dictionary = dictionary;
            this.cancellationToken = cancellationToken;
        }

        protected override IDisposable SubscribeCore(Observer<DictionaryReplaceEvent<TKey, TValue>> observer)
        {
            return new _DictionaryCollectionReplace(dictionary, observer, cancellationToken);
        }

        sealed class _DictionaryCollectionReplace :
            ObservableCollectionObserverBase<KeyValuePair<TKey, TValue>, DictionaryReplaceEvent<TKey, TValue>>
        {
            public _DictionaryCollectionReplace(IObservableCollection<KeyValuePair<TKey, TValue>> collection, Observer<DictionaryReplaceEvent<TKey, TValue>> observer, CancellationToken cancellationToken)
                : base(collection,
                observer, cancellationToken)
            {
            }

            protected override void Handler(in NotifyCollectionChangedEventArgs<KeyValuePair<TKey, TValue>> eventArgs)
            {
                if (eventArgs.Action == NotifyCollectionChangedAction.Replace)
                {
                    observer.OnNext(new DictionaryReplaceEvent<TKey, TValue>(
                        eventArgs.NewItem.Key,
                        eventArgs.OldItem.Value,
                        eventArgs.NewItem.Value));
                }
            }
        }
    }

    abstract class ObservableCollectionObserverBase<T, TEvent> : IDisposable
    {
        protected readonly IObservableCollection<T> collection;
        protected readonly Observer<TEvent> observer;
        readonly CancellationTokenRegistration cancellationTokenRegistration;
        readonly NotifyCollectionChangedEventHandler<T> handlerDelegate;

        public ObservableCollectionObserverBase(IObservableCollection<T> collection, Observer<TEvent> observer, CancellationToken cancellationToken)
        {
            this.collection = collection;
            this.observer = observer;
            this.handlerDelegate = Handler;

            collection.CollectionChanged += handlerDelegate;

            if (cancellationToken.CanBeCanceled)
            {
                cancellationTokenRegistration = cancellationToken.UnsafeRegister(static state =>
                {
                    var s = (ObservableCollectionObserverBase<T, TEvent>)state!;
                    s.observer.OnCompleted();
                    s.Dispose();
                }, this);
            }
        }

        public void Dispose()
        {
            collection.CollectionChanged -= handlerDelegate;
            cancellationTokenRegistration.Dispose();
        }

        protected abstract void Handler(in NotifyCollectionChangedEventArgs<T> eventArgs);
    }
}
