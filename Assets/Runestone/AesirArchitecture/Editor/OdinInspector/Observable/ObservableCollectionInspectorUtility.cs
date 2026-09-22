using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// 可观察集合调试面板的反射工具 —— 在编辑器侧读取集合的运行时状态。
    /// </summary>
    /// <remarks>
    /// 面板是调试工具而非热路径，这里统一用反射读取（无需运行时程序集暴露调试接口）。
    /// </remarks>
    internal static class ObservableCollectionInspectorUtility
    {
        #region 常量

        /// <summary>元素预览的最大条目数。</summary>
        internal const int PreviewItemLimit = 24;

        #endregion

        #region 状态读取

        /// <summary>
        /// 读取元素数量（<c>Count</c> 属性或字段）。
        /// </summary>
        /// <param name="target">目标实例。</param>
        /// <returns>元素数量；无法读取时返回 -1。</returns>
        internal static int GetCount(object target) => TryReadInt(target, "Count", out var value) ? value : -1;

        /// <summary>
        /// 读取变更事件的监听者数量（读取内部 <see cref="MiniEvent{T}" /> 字段的监听列表）。
        /// </summary>
        /// <param name="collection">可观察集合实例。</param>
        /// <returns>监听者数量；无监听或非可观察集合返回 0。</returns>
        /// <remarks>单轨通知后四种集合的变更事件统一由名为 <c>_changedEvent</c> 的 MiniEvent 字段持有。</remarks>
        internal static int GetChangedListenerCount(object collection)
        {
            if (collection == null)
            {
                return 0;
            }

            return GetMiniEventListenerCount(collection.GetType(), collection, "_changedEvent");
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
            return getListeners?.Invoke(miniEvent, null) is Delegate[] listeners ? listeners.Length : 0;
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
