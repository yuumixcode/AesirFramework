using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 可观察集合调试注册表 —— 记录编辑器下创建的集合实例与其同步视图，供调试面板枚举。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 集合构造函数中调用 <see cref="Register" />、<c>CreateView</c> 中调用 <see cref="RegisterView" />，
    /// 两处均标注 <see cref="System.Diagnostics.ConditionalAttribute" />（<c>UNITY_EDITOR</c>）——
    /// 玩家构建时调用点连同实参求值被编译器整体移除，运行时零开销、零静态状态。
    /// </para>
    /// <para>
    /// 只持有弱引用：集合被 GC 回收后条目在下一次 <see cref="GetLiveCollections" /> /
    /// <see cref="GetLiveViews" /> 调用时顺带清理，无需显式注销。
    /// 本类不做线程安全保证（与集合本身的主线程约定一致）。
    /// </para>
    /// </remarks>
    public static class ObservableCollectionRegistry
    {
        /// <summary>
        /// 登记集合实例。集合构造函数中调用。
        /// </summary>
        /// <param name="collection">被登记的集合实例。</param>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Register(object collection)
        {
            if (collection == null)
            {
                return;
            }

            Compact();
            _collections.Add(new WeakReference(collection));
        }

        /// <summary>
        /// 登记同步视图。集合的 <c>CreateView</c> 中调用。
        /// </summary>
        /// <param name="source">视图所属的源集合。</param>
        /// <param name="view">同步视图实例。</param>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void RegisterView(object source, object view)
        {
            if (source == null || view == null)
            {
                return;
            }

            Compact();
            _views.Add(new ViewEntry(new WeakReference(source), new WeakReference(view)));
        }

        /// <summary>
        /// 显式注销集合（<see cref="Register" /> 的逆操作）。正常使用无需调用——弱引用会自动失效。
        /// </summary>
        /// <param name="collection">要注销的集合实例。</param>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Unregister(object collection)
        {
            if (collection == null)
            {
                return;
            }

            for (var i = _collections.Count - 1; i >= 0; i--)
            {
                var target = _collections[i].Target;
                if (target == null || ReferenceEquals(target, collection))
                {
                    _collections.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 取得当前存活的集合实例列表（顺带清理已回收的弱引用）。
        /// </summary>
        /// <returns>存活集合实例列表；玩家构建中始终为空。</returns>
        public static List<object> GetLiveCollections()
        {
            Compact();

            var result = new List<object>(_collections.Count);
            foreach (var weakReference in _collections)
            {
                var target = weakReference.Target;
                if (target != null)
                {
                    result.Add(target);
                }
            }

            return result;
        }

        /// <summary>
        /// 取得当前存活的同步视图列表（顺带清理已回收的弱引用）。
        /// </summary>
        /// <returns>「源集合 → 视图」对列表；玩家构建中始终为空。</returns>
        public static List<(object Source, object View)> GetLiveViews()
        {
            Compact();

            var result = new List<(object, object)>(_views.Count);
            foreach (var entry in _views)
            {
                var source = entry.Source.Target;
                var view = entry.View.Target;
                if (source != null && view != null)
                {
                    result.Add((source, view));
                }
            }

            return result;
        }

        /// <summary>
        /// 清空注册表。测试与域重载时使用。
        /// </summary>
        public static void Clear()
        {
            _collections.Clear();
            _views.Clear();
        }

        /// <summary>
        /// 移除已回收的弱引用条目。
        /// </summary>
        static void Compact()
        {
            for (var i = _collections.Count - 1; i >= 0; i--)
            {
                if (!_collections[i].IsAlive)
                {
                    _collections.RemoveAt(i);
                }
            }

            for (var i = _views.Count - 1; i >= 0; i--)
            {
                if (!_views[i].Source.IsAlive || !_views[i].View.IsAlive)
                {
                    _views.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 视图条目：分别持有源集合与视图的弱引用（避免视图经源集合形成强引用链）。
        /// </summary>
        struct ViewEntry
        {
            public readonly WeakReference Source;
            public readonly WeakReference View;

            public ViewEntry(WeakReference source, WeakReference view)
            {
                Source = source;
                View = view;
            }
        }

        static readonly List<WeakReference> _collections = new List<WeakReference>();
        static readonly List<ViewEntry> _views = new List<ViewEntry>();

        /// <summary>
        /// 进入播放模式时清空注册表（静态状态随域重载失效，此处兜底覆盖关闭域重载的场景）。
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => Clear();
    }
}
