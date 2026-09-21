using System;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// 可观察集合的 Odin 内联调试面板 —— 在 Inspector 中直接显示集合运行状态与元素预览，
    /// 其下仍保留默认绘制（元素可正常编辑）。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 调试信息全部经反射读取（<see cref="ObservableCollectionInspectorUtility" />），
    /// 集合的公开 API 面与上游 Cysharp.ObservableCollections 保持一致。
    /// 未安装 Odin Inspector 时本文件整体不参与编译，纯代码 API 不受影响。
    /// </para>
    /// <para>
    /// 覆盖 <see cref="ObservableList{T}" />、<see cref="ObservableDictionary{TKey, TValue}" />、
    /// <see cref="ObservableHashSet{T}" />、<see cref="ObservableQueue{T}" />、
    /// <see cref="ObservableStack{T}" />、<see cref="ObservableRingBuffer{T}" />、
    /// <see cref="ObservableFixedSizeRingBuffer{T}" /> 七种集合。
    /// </para>
    /// </remarks>
    internal static class ObservableCollectionDrawerHelper
    {
        /// <summary>
        /// 绘制集合摘要（元素数 / 订阅者 / 视图数）与元素预览。
        /// </summary>
        /// <param name="collection">集合实例；为 null 时不绘制。</param>
        internal static void DrawSummary(object collection)
        {
            if (collection == null)
            {
                return;
            }

            var viewCount = ObservableCollectionInspectorUtility.CountViewsOf(
                collection,
                ObservableCollectionRegistry.GetLiveViews());

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(
                $"元素 {ObservableCollectionInspectorUtility.GetCount(collection)}"
                + $"　|　CollectionChanged 订阅 {ObservableCollectionInspectorUtility.GetCollectionChangedSubscriberCount(collection)}"
                + $"　|　轻量事件监听 {ObservableCollectionInspectorUtility.GetLightEventListenerCount(collection)}"
                + $"　|　同步视图 {viewCount}",
                EditorStyles.miniLabel);
            EditorGUILayout.LabelField(
                ObservableCollectionInspectorUtility.GetItemPreview(collection),
                EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.EndVertical();
        }
    }

    /// <summary>可观察列表的内联调试面板。</summary>
    [DrawerPriority(0, 0, 0)]
    internal sealed class ObservableListDrawer<T> : OdinValueDrawer<ObservableList<T>>
    {
        /// <inheritdoc />
        protected override void DrawPropertyLayout(GUIContent label)
        {
            ObservableCollectionDrawerHelper.DrawSummary(ValueEntry.SmartValue);
            CallNextDrawer(label);
        }
    }

    /// <summary>可观察字典的内联调试面板。</summary>
    [DrawerPriority(0, 0, 0)]
    internal sealed class ObservableDictionaryDrawer<TKey, TValue> : OdinValueDrawer<ObservableDictionary<TKey, TValue>>
    {
        /// <inheritdoc />
        protected override void DrawPropertyLayout(GUIContent label)
        {
            ObservableCollectionDrawerHelper.DrawSummary(ValueEntry.SmartValue);
            CallNextDrawer(label);
        }
    }

    /// <summary>可观察集合（HashSet）的内联调试面板。</summary>
    [DrawerPriority(0, 0, 0)]
    internal sealed class ObservableHashSetDrawer<T> : OdinValueDrawer<ObservableHashSet<T>>
    {
        /// <inheritdoc />
        protected override void DrawPropertyLayout(GUIContent label)
        {
            ObservableCollectionDrawerHelper.DrawSummary(ValueEntry.SmartValue);
            CallNextDrawer(label);
        }
    }

    /// <summary>可观察队列的内联调试面板。</summary>
    [DrawerPriority(0, 0, 0)]
    internal sealed class ObservableQueueDrawer<T> : OdinValueDrawer<ObservableQueue<T>>
    {
        /// <inheritdoc />
        protected override void DrawPropertyLayout(GUIContent label)
        {
            ObservableCollectionDrawerHelper.DrawSummary(ValueEntry.SmartValue);
            CallNextDrawer(label);
        }
    }

    /// <summary>可观察栈的内联调试面板。</summary>
    [DrawerPriority(0, 0, 0)]
    internal sealed class ObservableStackDrawer<T> : OdinValueDrawer<ObservableStack<T>>
    {
        /// <inheritdoc />
        protected override void DrawPropertyLayout(GUIContent label)
        {
            ObservableCollectionDrawerHelper.DrawSummary(ValueEntry.SmartValue);
            CallNextDrawer(label);
        }
    }

    /// <summary>可观察环形缓冲区的内联调试面板。</summary>
    [DrawerPriority(0, 0, 0)]
    internal sealed class ObservableRingBufferDrawer<T> : OdinValueDrawer<ObservableRingBuffer<T>>
    {
        /// <inheritdoc />
        protected override void DrawPropertyLayout(GUIContent label)
        {
            ObservableCollectionDrawerHelper.DrawSummary(ValueEntry.SmartValue);
            CallNextDrawer(label);
        }
    }

    /// <summary>可观察定长环形缓冲区的内联调试面板。</summary>
    [DrawerPriority(0, 0, 0)]
    internal sealed class ObservableFixedSizeRingBufferDrawer<T> : OdinValueDrawer<ObservableFixedSizeRingBuffer<T>>
    {
        /// <inheritdoc />
        protected override void DrawPropertyLayout(GUIContent label)
        {
            ObservableCollectionDrawerHelper.DrawSummary(ValueEntry.SmartValue);
            CallNextDrawer(label);
        }
    }
}
