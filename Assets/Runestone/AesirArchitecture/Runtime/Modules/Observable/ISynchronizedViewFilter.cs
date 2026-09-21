#nullable enable
using Runestone.AesirArchitecture.Internal;
using System;

namespace Runestone.AesirArchitecture
{
    // Obsolete...
    [Obsolete("this interface is obsoleted. Use ISynchronizedViewFilter<T, TView> instead.")]
    public interface ISynchronizedViewFilter<T>
    {
        bool IsMatch(T value);
    }

    public interface ISynchronizedViewFilter<T, TView>
    {
        bool IsMatch(T value, TView view);
    }

    internal class SynchronizedViewValueOnlyFilter<T, TView> : ISynchronizedViewFilter<T, TView>
    {
        readonly Func<T, bool> isMatch;

        public SynchronizedViewValueOnlyFilter(Func<T, bool> isMatch) => this.isMatch = isMatch;

        public bool IsMatch(T value, TView view) => isMatch(value);

        class NullViewFilter : ISynchronizedViewFilter<T, TView>
        {
            public bool IsMatch(T value, TView view) => true;
        }
    }

    public class SynchronizedViewFilter<T, TView> : ISynchronizedViewFilter<T, TView>
    {
        public static readonly ISynchronizedViewFilter<T, TView> Null = new NullViewFilter();

        readonly Func<T, TView, bool> isMatch;

        public SynchronizedViewFilter(Func<T, TView, bool> isMatch) => this.isMatch = isMatch;

        public bool IsMatch(T value, TView view) => isMatch(value, view);

        class NullViewFilter : ISynchronizedViewFilter<T, TView>
        {
            public bool IsMatch(T value, TView view) => true;
        }
    }
}
