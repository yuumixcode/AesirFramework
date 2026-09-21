using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// 可观察集合调试面板的反射工具 —— 在编辑器侧读取集合与同步视图的运行时状态。
    /// </summary>
    /// <remarks>
    /// 面板是调试工具而非热路径，这里统一用反射读取（无需运行时程序集暴露调试接口，
    /// 集合的公开 API 面保持与上游一致）。读取结果按需缓存 <see cref="Type" /> 级元数据。
    /// </remarks>
    internal static class ObservableCollectionInspectorUtility
    {
        #region 常量

        /// <summary>元素预览的最大条目数。</summary>
        internal const int PreviewItemLimit = 32;

        #endregion

        #region 集合类型识别

        /// <summary>
        /// 判断类型是否为受支持的可观察集合（含同步视图）。
        /// </summary>
        /// <param name="type">待判断类型。</param>
        /// <returns>是受支持类型返回 <c>true</c>。</returns>
        internal static bool IsSupported(object value)
        {
            if (value == null)
            {
                return false;
            }

            var type = value.GetType();
            if (type.IsGenericType)
            {
                var definition = type.GetGenericTypeDefinition();
                if (definition == typeof(ObservableList<>)
                    || definition == typeof(ObservableDictionary<,>)
                    || definition == typeof(ObservableHashSet<>)
                    || definition == typeof(ObservableQueue<>)
                    || definition == typeof(ObservableStack<>)
                    || definition == typeof(ObservableRingBuffer<>)
                    || definition == typeof(ObservableFixedSizeRingBuffer<>)
                    || definition == typeof(RingBuffer<>)
                    || definition == typeof(AlternateIndexList<>))
                {
                    return true;
                }
            }

            return value is IEnumerable;
        }

        /// <summary>
        /// 判断给定实例是否为可观察集合（而非同步视图或普通集合）。
        /// </summary>
        /// <param name="value">待判断实例。</param>
        /// <returns>是可观察集合返回 <c>true</c>。</returns>
        internal static bool IsObservableCollection(object value)
        {
            if (value == null)
            {
                return false;
            }

            var type = value.GetType();
            if (!type.IsGenericType)
            {
                return false;
            }

            var definition = type.GetGenericTypeDefinition();
            return definition == typeof(ObservableList<>)
                   || definition == typeof(ObservableDictionary<,>)
                   || definition == typeof(ObservableHashSet<>)
                   || definition == typeof(ObservableQueue<>)
                   || definition == typeof(ObservableStack<>)
                   || definition == typeof(ObservableRingBuffer<>)
                   || definition == typeof(ObservableFixedSizeRingBuffer<>);
        }

        /// <summary>
        /// 取得类型的简短显示名（泛型参数以 &lt;T&gt; 形式折叠）。
        /// </summary>
        /// <param name="type">目标类型。</param>
        /// <returns>简短类型名。</returns>
        internal static string GetDisplayTypeName(Type type)
        {
            if (type == null)
            {
                return "<null>";
            }

            if (!type.IsGenericType)
            {
                return type.Name;
            }

            var name = type.Name;
            var tick = name.IndexOf('`');
            if (tick >= 0)
            {
                name = name.Substring(0, tick);
            }

            var builder = new StringBuilder(name);
            builder.Append('<');
            var arguments = type.GetGenericArguments();
            for (var i = 0; i < arguments.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(arguments[i].Name);
            }

            builder.Append('>');
            return builder.ToString();
        }

        #endregion

        #region 状态读取

        /// <summary>
        /// 读取元素数量（<c>Count</c> 属性或字段）。
        /// </summary>
        /// <param name="target">目标实例。</param>
        /// <returns>元素数量；无法读取时返回 -1。</returns>
        internal static int GetCount(object target) => TryReadInt(target, "Count", out var value) ? value : -1;

        /// <summary>
        /// 读取未过滤的元素数量（同步视图）。
        /// </summary>
        /// <param name="target">同步视图实例。</param>
        /// <returns>未过滤数量；无法读取时返回 -1。</returns>
        internal static int GetUnfilteredCount(object target) =>
            TryReadInt(target, "UnfilteredCount", out var value) ? value : -1;

        /// <summary>
        /// 读取 <c>CollectionChanged</c> 的订阅者数量（读取事件背后字段的调用列表）。
        /// </summary>
        /// <param name="collection">可观察集合实例。</param>
        /// <returns>订阅者数量；无订阅或非可观察集合返回 0。</returns>
        internal static int GetCollectionChangedSubscriberCount(object collection)
        {
            if (collection == null)
            {
                return 0;
            }

            var field = collection.GetType().GetField(
                "CollectionChanged",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            return field?.GetValue(collection) is Delegate handler ? handler.GetInvocationList().Length : 0;
        }

        /// <summary>
        /// 读取轻量事件监听者总数（Added / Removed / Replaced(Updated) / Cleared 四轨之和）。
        /// </summary>
        /// <param name="collection">可观察集合实例。</param>
        /// <returns>监听者总数。</returns>
        /// <remarks>
        /// 轻量事件由 <c>MiniEvent</c> 字段持有，字段名为 <c>_addedEvent</c> / <c>_removedEvent</c> /
        /// <c>_replacedEvent</c> / <c>_updatedEvent</c> / <c>_clearedEvent</c>。
        /// </remarks>
        internal static int GetLightEventListenerCount(object collection)
        {
            if (collection == null)
            {
                return 0;
            }

            var total = 0;
            var type = collection.GetType();
            total += GetMiniEventListenerCount(type, collection, "_addedEvent");
            total += GetMiniEventListenerCount(type, collection, "_removedEvent");
            total += GetMiniEventListenerCount(type, collection, "_replacedEvent");
            total += GetMiniEventListenerCount(type, collection, "_updatedEvent");
            total += GetMiniEventListenerCount(type, collection, "_clearedEvent");
            return total;
        }

        /// <summary>
        /// 读取同步视图的过滤器描述。
        /// </summary>
        /// <param name="view">同步视图实例。</param>
        /// <returns>过滤器类型名；未附加过滤器时返回「无」。</returns>
        internal static string GetFilterDescription(object view)
        {
            if (view == null)
            {
                return "无";
            }

            var property = view.GetType().GetProperty("Filter", BindingFlags.Instance | BindingFlags.Public);
            var filter = property?.GetValue(view);
            return filter == null ? "无" : GetDisplayTypeName(filter.GetType());
        }

        /// <summary>
        /// 生成集合元素的预览文本（最多 <see cref="PreviewItemLimit" /> 项）。
        /// </summary>
        /// <param name="collection">目标集合（任意 <see cref="IEnumerable" />）。</param>
        /// <param name="limit">最大条目数。</param>
        /// <returns>形如 <c>[0] = 10, [1] = 20, …（共 120 项）</c> 的预览文本。</returns>
        internal static string GetItemPreview(object collection, int limit = PreviewItemLimit)
        {
            if (collection is not IEnumerable enumerable)
            {
                return "<不可枚举>";
            }

            var builder = new StringBuilder();
            var index = 0;
            var truncated = false;
            foreach (var item in enumerable)
            {
                if (index >= limit)
                {
                    truncated = true;
                    break;
                }

                if (index > 0)
                {
                    builder.Append(", ");
                }

                builder.Append('[').Append(index).Append("] = ").Append(FormatValue(item));
                index++;
            }

            if (index == 0)
            {
                return "<空>";
            }

            var total = GetCount(collection);
            if (truncated && total > index)
            {
                builder.Append(" …（共 ").Append(total).Append(" 项）");
            }

            return builder.ToString();
        }

        /// <summary>
        /// 统计某集合关联的同步视图数量。
        /// </summary>
        /// <param name="source">源集合实例。</param>
        /// <param name="views">「源集合 → 视图」对列表。</param>
        /// <returns>关联的视图数量。</returns>
        internal static int CountViewsOf(object source, IReadOnlyList<(object Source, object View)> views)
        {
            var count = 0;
            for (var i = 0; i < views.Count; i++)
            {
                if (ReferenceEquals(views[i].Source, source))
                {
                    count++;
                }
            }

            return count;
        }

        #endregion

        #region 私有实现

        /// <summary>
        /// 读取 <c>MiniEvent</c> 字段的监听者数量。
        /// </summary>
        /// <param name="type">集合类型。</param>
        /// <param name="instance">集合实例。</param>
        /// <param name="fieldName">字段名。</param>
        /// <returns>监听者数量。</returns>
        static int GetMiniEventListenerCount(Type type, object instance, string fieldName)
        {
            var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            var miniEvent = field?.GetValue(instance);
            if (miniEvent == null)
            {
                return 0;
            }

            var getListeners = miniEvent.GetType().GetMethod("GetListeners", BindingFlags.Instance | BindingFlags.Public);
            if (getListeners?.Invoke(miniEvent, null) is Delegate[] listeners)
            {
                return listeners.Length;
            }

            return 0;
        }

        /// <summary>
        /// 尝试按名称读取整数成员（属性优先，其次字段）。
        /// </summary>
        /// <param name="target">目标实例。</param>
        /// <param name="memberName">成员名。</param>
        /// <param name="value">读取到的值。</param>
        /// <returns>成功读取返回 <c>true</c>。</returns>
        static bool TryReadInt(object target, string memberName, out int value)
        {
            value = -1;
            if (target == null)
            {
                return false;
            }

            var type = target.GetType();
            var property = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public);
            if (property != null && property.PropertyType == typeof(int))
            {
                value = (int)property.GetValue(target);
                return true;
            }

            var field = type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null && field.FieldType == typeof(int))
            {
                value = (int)field.GetValue(target);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 把元素格式化为单行文本（截断过长内容）。
        /// </summary>
        /// <param name="value">元素值。</param>
        /// <returns>单行显示文本。</returns>
        static string FormatValue(object value)
        {
            if (value == null)
            {
                return "null";
            }

            var text = value.ToString();
            if (text == null)
            {
                return "<null>";
            }

            text = text.Replace('\n', ' ').Replace('\r', ' ');
            return text.Length > 48 ? text.Substring(0, 45) + "…" : text;
        }

        #endregion
    }
}
