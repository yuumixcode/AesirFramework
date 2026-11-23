using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// 可观察集合的 UI Toolkit 内联摘要（无 Odin 依赖）：复刻 Odin 版 ObservableCollectionDrawer 的
    /// "元素数 | 变更监听 + 元素预览" 调试信息，其下保留默认子字段绘制。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     调试信息经反射读取（<see cref="ObservableCollectionInspectorUtility" />，已迁到核心程序集，零 Odin 依赖）。
    ///     覆盖 <see cref="ObservableList{T}" />、<see cref="ObservableDictionary{TKey,TValue}" />、
    ///     <see cref="ObservableHashSet{T}" /> 三种集合，各用一个开放泛型 CustomPropertyDrawer。
    ///     </para>
    ///     <para>
    ///     <b>生效前提</b>：字段须是 Unity 可序列化属性（<c>[SerializeReference]</c> 或 Odin 序列化后端）。
    ///     普通泛型字段 Unity 原生不绘制，两个 Inspector 都不会显示。
    ///     </para>
    /// </remarks>
    internal static class ObservableCollectionView
    {
        internal static VisualElement Create(SerializedProperty property, FieldInfo fieldInfo)
        {
            var root = new VisualElement();
            root.AddToClassList("aesir-observable-collection");

            var collection = ResolveCollection(property, fieldInfo);
            var summary = new VisualElement();
            summary.AddToClassList("aesir-observable-collection__summary");
            summary.style.whiteSpace = WhiteSpace.Normal;
            summary.style.paddingLeft = 6f;
            summary.style.paddingTop = 3f;
            summary.style.paddingBottom = 3f;
            summary.style.marginBottom = 3f;
            summary.style.borderLeftWidth = 2f;

            if (collection != null)
            {
                var label = new Label($"元素 {ObservableCollectionInspectorUtility.GetCount(collection)}" +
                                      $"　|　变更监听 {ObservableCollectionInspectorUtility.GetChangedListenerCount(collection)}\n" +
                                      ObservableCollectionInspectorUtility.GetItemPreview(collection));
                label.style.whiteSpace = WhiteSpace.Normal;
                summary.Add(label);
            }
            else
            {
                summary.Add(new Label("（未持有集合实例）"));
            }

            root.Add(summary);
            AppendDefaultChildren(property, root);
            return root;
        }

        /// <summary>追加默认子字段绘制（复刻 Odin 版 CallNextDrawer）。</summary>
        static void AppendDefaultChildren(SerializedProperty property, VisualElement root)
        {
            var iterator = property.Copy();
            var end = iterator.GetEndProperty();
            var first = true;
            while (iterator.NextVisible(first) && !SerializedProperty.EqualContents(iterator, end))
            {
                first = false;
                root.Add(new PropertyField(iterator.Copy()));
            }
        }

        static object ResolveCollection(SerializedProperty property, FieldInfo fieldInfo)
        {
            if (fieldInfo == null || property.depth != 0 || property.serializedObject.targetObject == null)
            {
                return null;
            }

            return fieldInfo.GetValue(property.serializedObject.targetObject);
        }
    }

    /// <summary>ObservableList&lt;T&gt; 的 UI Toolkit 摘要绘制器。</summary>
    [CustomPropertyDrawer(typeof(ObservableList<>))]
    internal sealed class ObservableListUIToolkitDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) =>
            ObservableCollectionView.Create(property, fieldInfo);
    }

    /// <summary>ObservableDictionary&lt;TKey,TValue&gt; 的 UI Toolkit 摘要绘制器。</summary>
    [CustomPropertyDrawer(typeof(ObservableDictionary<,>))]
    internal sealed class ObservableDictionaryUIToolkitDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) =>
            ObservableCollectionView.Create(property, fieldInfo);
    }

    /// <summary>ObservableHashSet&lt;T&gt; 的 UI Toolkit 摘要绘制器。</summary>
    [CustomPropertyDrawer(typeof(ObservableHashSet<>))]
    internal sealed class ObservableHashSetUIToolkitDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property) =>
            ObservableCollectionView.Create(property, fieldInfo);
    }
}
