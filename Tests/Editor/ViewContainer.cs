// 上游行为对齐测试：移植自 Cysharp/ObservableCollections 的测试套件
// （原测试使用 xUnit + FluentAssertions，此处转换为 NUnit 断言并保持用例语义一致）。

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using NUnit.Framework;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    public struct ViewContainer<T> : IEquatable<ViewContainer<T>>, IComparable<ViewContainer<T>>
    {
        public ViewContainer(T value)
        {
            Value = value;
        }

        public T Value { get; }

        public static implicit operator ViewContainer<T>(T value) => new(value);

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public int CompareTo(ViewContainer<T> other)
        {
            return Comparer<T>.Default.Compare(Value, other.Value);
        }

        public bool Equals(ViewContainer<T> other)
        {
            return EqualityComparer<T>.Default.Equals(Value, other.Value);
        }
    }

    //public class TestFilter<T> : ISynchronizedViewFilter<T>
    //{
    //    readonly Func<T, bool> filter;
    //    public List<SynchronizedViewChangedEventArgs<T, ViewContainer<T>>> CalledOnCollectionChanged = new();

    //    public TestFilter(Func<T, bool> filter)
    //    {
    //        this.filter = filter;
    //    }


    //    public bool IsMatch(T value)
    //    {
    //        return this.filter.Invoke(value);
    //    }

    //    public void OnCollectionChanged(in SynchronizedViewChangedEventArgs<T, ViewContainer<T>> args)
    //    {
    //        CalledOnCollectionChanged.Add(args);
    //    }

    //    public void WhenTrue(T value, ViewContainer<T> view)
    //    {
    //        CalledWhenTrue.Add((value, view));
    //    }



    //    public TestFilter2(Func<KeyValuePair<T, T>, ViewContainer<T>, bool> filter)
    //    {
    //        this.filter = filter;
    //    }


    //    public bool IsMatch(KeyValuePair<T, T> value, ViewContainer<T> view)
    //    {
    //        return this.filter.Invoke(value, view);
    //    }

    //    public void OnCollectionChanged(in SynchronizedViewChangedEventArgs<KeyValuePair<T, T>, ViewContainer<T>> args)
    //    {
    //        CalledOnCollectionChanged.Add(args);
    //    }

    //    public void WhenTrue(KeyValuePair<T, T> value, ViewContainer<T> view)
    //    {
    //        CalledWhenTrue.Add((value, view));
    //    }

}
