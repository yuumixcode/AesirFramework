// R3 响应式扩展：移植自 Cysharp/ObservableCollections 的 ObservableCollections.R3 包。
// 上游使用 C# 12 语法（file-scoped namespace / record struct / 主构造器），
// 此处降级为 Unity 2022.3（C# 9）可编译的等价写法；R3 侧 API 与语义保持一致。

#nullable enable
using Runestone.AesirArchitecture;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Tracing;
using System.Runtime.InteropServices;
using System.Threading;
using R3;

namespace Runestone.AesirArchitecture.R3
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct ViewChangedEvent<T, TView>
    {
        public readonly NotifyCollectionChangedAction Action;
        public readonly (T Value, TView View) NewItem;
        public readonly (T Value, TView View) OldItem;
        public readonly int NewStartingIndex;
        public readonly int OldStartingIndex;
        public readonly SortOperation<T> SortOperation;

        public ViewChangedEvent(NotifyCollectionChangedAction action, (T, TView) newItem, (T, TView) oldItem, int newStartingIndex, int oldStartingIndex, SortOperation<T> sortOperation)
        {
            Action = action;
            NewItem = newItem;
            OldItem = oldItem;
            NewStartingIndex = newStartingIndex;
            OldStartingIndex = oldStartingIndex;
            SortOperation = sortOperation;
        }
    }

    [StructLayout(LayoutKind.Auto)]
    public readonly struct RejectedViewChangedEvent
    {
        public readonly RejectedViewChangedAction Action;
        public readonly int NewIndex;
        public readonly int OldIndex;

        public RejectedViewChangedEvent(RejectedViewChangedAction action, int newIndex, int oldIndex)
        {
            Action = action;
            NewIndex = newIndex;
            OldIndex = oldIndex;
        }
    }

    public static partial class ObservableCollectionR3Extensions
    {
        public static Observable<RejectedViewChangedEvent> ObserveRejected<T, TView>(this ISynchronizedView<T, TView> source, CancellationToken cancellationToken = default)
        {
            return new SynchronizedViewRejected<T, TView>(source, cancellationToken);
        }

        public static Observable<ViewChangedEvent<T, TView>> ObserveChanged<T, TView>(this ISynchronizedView<T, TView> source, CancellationToken cancellationToken = default)
        {
            return new SynchronizedViewChanged<T, TView>(source, cancellationToken);
        }

        public static Observable<CollectionAddEvent<(T Value, TView View)>> ObserveAdd<T, TView>(this ISynchronizedView<T, TView> source, CancellationToken cancellationToken = default)
        {
            return new SynchronizedViewAdd<T, TView>(source, cancellationToken);
        }

        public static Observable<CollectionRemoveEvent<(T Value, TView View)>> ObserveRemove<T, TView>(this ISynchronizedView<T, TView> source, CancellationToken cancellationToken = default)
        {
            return new SynchronizedViewRemove<T, TView>(source, cancellationToken);
        }

        public static Observable<CollectionReplaceEvent<(T Value, TView View)>> ObserveReplace<T, TView>(this ISynchronizedView<T, TView> source, CancellationToken cancellationToken = default)
        {
            return new SynchronizedViewReplace<T, TView>(source, cancellationToken);
        }

        public static Observable<CollectionMoveEvent<(T Value, TView View)>> ObserveMove<T, TView>(this ISynchronizedView<T, TView> source, CancellationToken cancellationToken = default)
        {
            return new SynchronizedViewMove<T, TView>(source, cancellationToken);
        }

        public static Observable<CollectionResetEvent<T>> ObserveReset<T, TView>(this ISynchronizedView<T, TView> source, CancellationToken cancellationToken = default)
        {
            return new SynchronizedViewReset<T, TView>(source, cancellationToken);
        }

        public static Observable<Unit> ObserveClear<T, TView>(this ISynchronizedView<T, TView> source, CancellationToken cancellationToken = default)
        {
            return new SynchronizedViewClear<T, TView>(source, cancellationToken);
        }

        public static Observable<(int Index, int Count)> ObserveReverse<T, TView>(this ISynchronizedView<T, TView> source, CancellationToken cancellationToken = default)
        {
            return new SynchronizedViewReverse<T, TView>(source, cancellationToken);
        }

        public static Observable<(int Index, int Count, IComparer<T>? Comparer)> ObserveSort<T, TView>(this ISynchronizedView<T, TView> source, CancellationToken cancellationToken = default)
        {
            return new SynchronizedViewSort<T, TView>(source, cancellationToken);
        }

        public static Observable<int> ObserveCountChanged<T, TView>(this ISynchronizedView<T, TView> source, bool notifyCurrentCount = false, CancellationToken cancellationToken = default)
        {
            return new SynchronizedViewCountChanged<T, TView>(source, notifyCurrentCount, cancellationToken);
        }
    }

    sealed class SynchronizedViewChanged<T, TView> : Observable<ViewChangedEvent<T, TView>>
    {
        readonly ISynchronizedView<T, TView> source;
        readonly CancellationToken cancellationToken;

        public SynchronizedViewChanged(ISynchronizedView<T, TView> source, CancellationToken cancellationToken)
        {
            this.source = source;
            this.cancellationToken = cancellationToken;
        }

        protected override IDisposable SubscribeCore(Observer<ViewChangedEvent<T, TView>> observer)
        {
            return new _SynchronizedViewChanged(source, observer, cancellationToken);
        }

        sealed class _SynchronizedViewChanged : SynchronizedViewObserverBase<T, TView, ViewChangedEvent<T, TView>>
        {
            public _SynchronizedViewChanged(ISynchronizedView<T, TView> source, Observer<ViewChangedEvent<T, TView>> observer, CancellationToken cancellationToken)
                : base(source, observer, cancellationToken)
            {
            }

            protected override void Handler(in SynchronizedViewChangedEventArgs<T, TView> eventArgs)
            {
                if (eventArgs.IsSingleItem)
                {
                    var newArgs = new ViewChangedEvent<T, TView>(
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
                        var index = eventArgs.NewStartingIndex;
                        for (int i = 0; i < eventArgs.NewValues.Length; i++)
                        {
                            var newItem = (eventArgs.NewValues[i], eventArgs.NewViews[i]);
                            var newArgs = new ViewChangedEvent<T, TView>(
                                eventArgs.Action,
                                newItem,
                                default,
                                index++,
                                eventArgs.OldStartingIndex,
                                eventArgs.SortOperation);

                            observer.OnNext(newArgs);
                        }
                    }
                    else if (eventArgs.Action == NotifyCollectionChangedAction.Remove)
                    {

                        for (int i = 0; i < eventArgs.OldValues.Length; i++)
                        {
                            var oldItem = (eventArgs.OldValues[i], eventArgs.OldViews[i]);
                            var newArgs = new ViewChangedEvent<T, TView>(
                                eventArgs.Action,
                                default,
                                oldItem,
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

    sealed class SynchronizedViewAdd<T, TView> : Observable<CollectionAddEvent<(T, TView)>>
    {
        readonly ISynchronizedView<T, TView> source;
        readonly CancellationToken cancellationToken;

        public SynchronizedViewAdd(ISynchronizedView<T, TView> source, CancellationToken cancellationToken)
        {
            this.source = source;
            this.cancellationToken = cancellationToken;
        }

        protected override IDisposable SubscribeCore(Observer<CollectionAddEvent<(T, TView)>> observer)
        {
            return new _SynchronizedViewAdd(source, observer, cancellationToken);
        }

        sealed class _SynchronizedViewAdd : SynchronizedViewObserverBase<T, TView, CollectionAddEvent<(T, TView)>>
        {
            public _SynchronizedViewAdd(ISynchronizedView<T, TView> source, Observer<CollectionAddEvent<(T, TView)>> observer, CancellationToken cancellationToken)
                : base(source, observer, cancellationToken)
            {
            }

            protected override void Handler(in SynchronizedViewChangedEventArgs<T, TView> eventArgs)
            {
                if (eventArgs.Action == NotifyCollectionChangedAction.Add)
                {
                    if (eventArgs.IsSingleItem)
                    {
                        observer.OnNext(new CollectionAddEvent<(T, TView)>(eventArgs.NewStartingIndex, eventArgs.NewItem));
                    }
                    else
                    {
                        var index = eventArgs.NewStartingIndex;
                        for (int i = 0; i < eventArgs.NewValues.Length; i++)
                        {
                            observer.OnNext(new CollectionAddEvent<(T, TView)>(index++, (eventArgs.NewValues[i], eventArgs.NewViews[i])));
                        }
                    }
                }
            }
        }
    }

    sealed class SynchronizedViewRemove<T, TView> : Observable<CollectionRemoveEvent<(T, TView)>>
    {
        readonly ISynchronizedView<T, TView> source;
        readonly CancellationToken cancellationToken;

        public SynchronizedViewRemove(ISynchronizedView<T, TView> source, CancellationToken cancellationToken)
        {
            this.source = source;
            this.cancellationToken = cancellationToken;
        }

        protected override IDisposable SubscribeCore(Observer<CollectionRemoveEvent<(T, TView)>> observer)
        {
            return new _SynchronizedViewRemove(source, observer, cancellationToken);
        }

        sealed class _SynchronizedViewRemove : SynchronizedViewObserverBase<T, TView, CollectionRemoveEvent<(T, TView)>>
        {
            public _SynchronizedViewRemove(ISynchronizedView<T, TView> source, Observer<CollectionRemoveEvent<(T, TView)>> observer, CancellationToken cancellationToken)
                : base(source, observer, cancellationToken)
            {
            }

            protected override void Handler(in SynchronizedViewChangedEventArgs<T, TView> eventArgs)
            {
                if (eventArgs.Action == NotifyCollectionChangedAction.Remove)
                {
                    if (eventArgs.IsSingleItem)
                    {
                        observer.OnNext(new CollectionRemoveEvent<(T, TView)>(eventArgs.OldStartingIndex, eventArgs.OldItem));
                    }
                    else
                    {
                        for (int i = 0; i < eventArgs.OldValues.Length; i++)
                        {
                            observer.OnNext(new CollectionRemoveEvent<(T, TView)>(eventArgs.OldStartingIndex, (eventArgs.OldValues[i], eventArgs.OldViews[i])));
                        }
                    }
                }
            }
        }
    }

    sealed class SynchronizedViewReplace<T, TView> : Observable<CollectionReplaceEvent<(T, TView)>>
    {
        readonly ISynchronizedView<T, TView> source;
        readonly CancellationToken cancellationToken;

        public SynchronizedViewReplace(ISynchronizedView<T, TView> source, CancellationToken cancellationToken)
        {
            this.source = source;
            this.cancellationToken = cancellationToken;
        }

        protected override IDisposable SubscribeCore(Observer<CollectionReplaceEvent<(T, TView)>> observer)
        {
            return new _SynchronizedViewReplace(source, observer, cancellationToken);
        }

        sealed class _SynchronizedViewReplace : SynchronizedViewObserverBase<T, TView, CollectionReplaceEvent<(T, TView)>>
        {
            public _SynchronizedViewReplace(ISynchronizedView<T, TView> source, Observer<CollectionReplaceEvent<(T, TView)>> observer, CancellationToken cancellationToken)
                : base(source, observer, cancellationToken)
            {
            }

            protected override void Handler(in SynchronizedViewChangedEventArgs<T, TView> eventArgs)
            {
                if (eventArgs.Action == NotifyCollectionChangedAction.Replace)
                {
                    observer.OnNext(new CollectionReplaceEvent<(T, TView)>(eventArgs.NewStartingIndex, eventArgs.OldItem, eventArgs.NewItem));
                }
            }
        }
    }

    sealed class SynchronizedViewMove<T, TView> : Observable<CollectionMoveEvent<(T, TView)>>
    {
        readonly ISynchronizedView<T, TView> source;
        readonly CancellationToken cancellationToken;

        public SynchronizedViewMove(ISynchronizedView<T, TView> source, CancellationToken cancellationToken)
        {
            this.source = source;
            this.cancellationToken = cancellationToken;
        }

        protected override IDisposable SubscribeCore(Observer<CollectionMoveEvent<(T, TView)>> observer)
        {
            return new _SynchronizedViewMove(source, observer, cancellationToken);
        }

        sealed class _SynchronizedViewMove : SynchronizedViewObserverBase<T, TView, CollectionMoveEvent<(T, TView)>>
        {
            public _SynchronizedViewMove(ISynchronizedView<T, TView> source, Observer<CollectionMoveEvent<(T, TView)>> observer, CancellationToken cancellationToken)
                : base(source, observer, cancellationToken)
            {
            }

            protected override void Handler(in SynchronizedViewChangedEventArgs<T, TView> eventArgs)
            {
                if (eventArgs.Action == NotifyCollectionChangedAction.Move)
                {
                    observer.OnNext(new CollectionMoveEvent<(T, TView)>(eventArgs.OldStartingIndex, eventArgs.NewStartingIndex, eventArgs.NewItem));
                }
            }
        }
    }

    sealed class SynchronizedViewReset<T, TView> : Observable<CollectionResetEvent<T>>
    {
        readonly ISynchronizedView<T, TView> source;
        readonly CancellationToken cancellationToken;

        public SynchronizedViewReset(ISynchronizedView<T, TView> source, CancellationToken cancellationToken)
        {
            this.source = source;
            this.cancellationToken = cancellationToken;
        }

        protected override IDisposable SubscribeCore(Observer<CollectionResetEvent<T>> observer)
        {
            return new _SynchronizedViewReset(source, observer, cancellationToken);
        }

        sealed class _SynchronizedViewReset : SynchronizedViewObserverBase<T, TView, CollectionResetEvent<T>>
        {
            public _SynchronizedViewReset(ISynchronizedView<T, TView> source, Observer<CollectionResetEvent<T>> observer, CancellationToken cancellationToken)
                : base(source, observer, cancellationToken)
            {
            }

            protected override void Handler(in SynchronizedViewChangedEventArgs<T, TView> eventArgs)
            {
                if (eventArgs.Action == NotifyCollectionChangedAction.Reset)
                {
                    observer.OnNext(new CollectionResetEvent<T>(eventArgs.SortOperation));
                }
            }
        }
    }

    sealed class SynchronizedViewClear<T, TView> : Observable<Unit>
    {
        readonly ISynchronizedView<T, TView> source;
        readonly CancellationToken cancellationToken;

        public SynchronizedViewClear(ISynchronizedView<T, TView> source, CancellationToken cancellationToken)
        {
            this.source = source;
            this.cancellationToken = cancellationToken;
        }

        protected override IDisposable SubscribeCore(Observer<Unit> observer)
        {
            return new _SynchronizedViewClear(source, observer, cancellationToken);
        }

        sealed class _SynchronizedViewClear : SynchronizedViewObserverBase<T, TView, Unit>
        {
            public _SynchronizedViewClear(ISynchronizedView<T, TView> source, Observer<Unit> observer, CancellationToken cancellationToken)
                : base(source, observer, cancellationToken)
            {
            }

            protected override void Handler(in SynchronizedViewChangedEventArgs<T, TView> eventArgs)
            {
                if (eventArgs.Action == NotifyCollectionChangedAction.Reset && eventArgs.SortOperation.IsClear)
                {
                    observer.OnNext(Unit.Default);
                }
            }
        }
    }

    sealed class SynchronizedViewReverse<T, TView> : Observable<(int Index, int Count)>
    {
        readonly ISynchronizedView<T, TView> source;
        readonly CancellationToken cancellationToken;

        public SynchronizedViewReverse(ISynchronizedView<T, TView> source, CancellationToken cancellationToken)
        {
            this.source = source;
            this.cancellationToken = cancellationToken;
        }

        protected override IDisposable SubscribeCore(Observer<(int Index, int Count)> observer)
        {
            return new _SynchronizedViewReverse(source, observer, cancellationToken);
        }

        sealed class _SynchronizedViewReverse : SynchronizedViewObserverBase<T, TView, (int Index, int Count)>
        {
            public _SynchronizedViewReverse(ISynchronizedView<T, TView> source, Observer<(int Index, int Count)> observer, CancellationToken cancellationToken)
                : base(source, observer, cancellationToken)
            {
            }

            protected override void Handler(in SynchronizedViewChangedEventArgs<T, TView> eventArgs)
            {
                if (eventArgs.Action == NotifyCollectionChangedAction.Reset && eventArgs.SortOperation.IsReverse)
                {
                    observer.OnNext((eventArgs.SortOperation.Index, eventArgs.SortOperation.Count));
                }
            }
        }
    }

    sealed class SynchronizedViewSort<T, TView> : Observable<(int Index, int Count, IComparer<T>? Comparer)>
    {
        readonly ISynchronizedView<T, TView> source;
        readonly CancellationToken cancellationToken;

        public SynchronizedViewSort(ISynchronizedView<T, TView> source, CancellationToken cancellationToken)
        {
            this.source = source;
            this.cancellationToken = cancellationToken;
        }

        protected override IDisposable SubscribeCore(Observer<(int Index, int Count, IComparer<T>? Comparer)> observer)
        {
            return new _SynchronizedViewSort(source, observer, cancellationToken);
        }

        sealed class _SynchronizedViewSort : SynchronizedViewObserverBase<T, TView, (int Index, int Count, IComparer<T>? Comparer)>
        {
            public _SynchronizedViewSort(ISynchronizedView<T, TView> source, Observer<(int Index, int Count, IComparer<T>? Comparer)> observer, CancellationToken cancellationToken)
                : base(source, observer, cancellationToken)
            {
            }

            protected override void Handler(in SynchronizedViewChangedEventArgs<T, TView> eventArgs)
            {
                if (eventArgs.Action == NotifyCollectionChangedAction.Reset && eventArgs.SortOperation.IsSort)
                {
                    observer.OnNext(eventArgs.SortOperation.AsTuple());
                }
            }
        }
    }

    sealed class SynchronizedViewCountChanged<T, TView> : Observable<int>
    {
        readonly ISynchronizedView<T, TView> source;
        readonly bool notifyCurrentCount;
        readonly CancellationToken cancellationToken;

        public SynchronizedViewCountChanged(ISynchronizedView<T, TView> source, bool notifyCurrentCount, CancellationToken cancellationToken)
        {
            this.source = source;
            this.notifyCurrentCount = notifyCurrentCount;
            this.cancellationToken = cancellationToken;
        }

        protected override IDisposable SubscribeCore(Observer<int> observer)
        {
            return new _SynchronizedViewCountChanged(source, notifyCurrentCount, observer, cancellationToken);
        }

        sealed class _SynchronizedViewCountChanged : SynchronizedViewObserverBase<T, TView, int>
        {
            int countPrev;

            public _SynchronizedViewCountChanged(
                ISynchronizedView<T, TView> source,
                bool notifyCurrentCount,
                Observer<int> observer,
                CancellationToken cancellationToken) : base(source, observer, cancellationToken)
            {
                this.countPrev = source.Count;
                if (notifyCurrentCount)
                {
                    observer.OnNext(source.Count);
                }
            }

            protected override void Handler(in SynchronizedViewChangedEventArgs<T, TView> eventArgs)
            {
                switch (eventArgs.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                    case NotifyCollectionChangedAction.Remove:
                    case NotifyCollectionChangedAction.Reset when countPrev != source.Count:
                        observer.OnNext(source.Count);
                        break;
                }
                countPrev = source.Count;
            }
        }
    }



    sealed class SynchronizedViewRejected<T, TView> : Observable<RejectedViewChangedEvent>
    {
        readonly ISynchronizedView<T, TView> source;
        readonly CancellationToken cancellationToken;

        public SynchronizedViewRejected(ISynchronizedView<T, TView> source, CancellationToken cancellationToken)
        {
            this.source = source;
            this.cancellationToken = cancellationToken;
        }

        protected override IDisposable SubscribeCore(Observer<RejectedViewChangedEvent> observer)
        {
            return new _SynchronizedViewRejected(source, observer, cancellationToken);
        }

        sealed class _SynchronizedViewRejected : IDisposable
        {
            readonly ISynchronizedView<T, TView> source;
            readonly Observer<RejectedViewChangedEvent> observer;
            readonly CancellationTokenRegistration cancellationTokenRegistration;
            readonly Action<RejectedViewChangedAction, int, int> handlerDelegate;

            public _SynchronizedViewRejected(ISynchronizedView<T, TView> source, Observer<RejectedViewChangedEvent> observer, CancellationToken cancellationToken)
            {
                this.source = source;
                this.observer = observer;
                this.handlerDelegate = Handler;

                source.RejectedViewChanged += handlerDelegate;

                if (cancellationToken.CanBeCanceled)
                {
                    cancellationTokenRegistration = cancellationToken.UnsafeRegister(static state =>
                    {
                        var s = (_SynchronizedViewRejected)state!;
                        s.observer.OnCompleted();
                        s.Dispose();
                    }, this);
                }
            }

            public void Dispose()
            {
                source.RejectedViewChanged -= handlerDelegate;
                cancellationTokenRegistration.Dispose();
            }

            void Handler(RejectedViewChangedAction rejectedViewChangedAction, int newIndex, int oldIndex)
            {
                observer.OnNext(new RejectedViewChangedEvent(rejectedViewChangedAction, newIndex, oldIndex));
            }
        }
    }

    abstract class SynchronizedViewObserverBase<T, TView, TEvent> : IDisposable
    {
        protected readonly ISynchronizedView<T, TView> source;
        protected readonly Observer<TEvent> observer;
        readonly CancellationTokenRegistration cancellationTokenRegistration;
        readonly NotifyViewChangedEventHandler<T, TView> handlerDelegate;

        public SynchronizedViewObserverBase(ISynchronizedView<T, TView> source, Observer<TEvent> observer, CancellationToken cancellationToken)
        {
            this.source = source;
            this.observer = observer;
            this.handlerDelegate = Handler;

            source.ViewChanged += handlerDelegate;

            if (cancellationToken.CanBeCanceled)
            {
                cancellationTokenRegistration = cancellationToken.UnsafeRegister(static state =>
                {
                    var s = (SynchronizedViewObserverBase<T, TView, TEvent>)state!;
                    s.observer.OnCompleted();
                    s.Dispose();
                }, this);
            }
        }

        public void Dispose()
        {
            source.ViewChanged -= handlerDelegate;
            cancellationTokenRegistration.Dispose();
        }

        protected abstract void Handler(in SynchronizedViewChangedEventArgs<T, TView> eventArgs);
    }
}
