using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Runestone.AesirModules.Editor
{
    /// <summary>
    /// <see cref="SubclassSelectorAttribute" /> 的 UI Toolkit 属性绘制器。
    /// 为 <c>[SerializeReference]</c> 字段提供子类下拉选择：
    /// 点击按钮弹出字段声明类型的全部可选子类（按命名空间分组），
    /// 选择后创建实例并渲染其序列化字段。
    /// </summary>
    [CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
    public class SubclassSelectorDrawer : PropertyDrawer
    {
        const string NullButtonText = "Null — 点击选择子类类型";

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();

            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                root.Add(new HelpBox("SubclassSelector 仅支持 [SerializeReference] 字段。",
                    HelpBoxMessageType.Error));
                return root;
            }

            var childContainer = new VisualElement();
            var button = new Button { text = GetCurrentTypeName(property) };
            button.clicked += () => ShowTypeMenu(property, button, childContainer);
            root.Add(button);
            root.Add(childContainer);
            BuildChildFields(property, childContainer);

            return root;
        }

        /// <summary>
        /// 弹出类型下拉菜单。
        /// </summary>
        void ShowTypeMenu(SerializedProperty property, Button button, VisualElement childContainer)
        {
            var menu = new GenericMenu();
            var types = GetFilteredTypes(fieldInfo.FieldType);
            if (types.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent("（无可选子类型）"), false);
            }

            foreach (var type in types)
            {
                var menuPath = string.IsNullOrEmpty(type.Namespace)
                    ? type.Name
                    : $"{type.Namespace}/{type.Name}";
                menu.AddItem(new GUIContent(menuPath), false,
                    () => SelectType(property, type, button, childContainer));
            }

            menu.DropDown(button.worldBound);
        }

        /// <summary>
        /// 选定类型：写入 managedReferenceValue 并重建子字段视图。
        /// </summary>
        void SelectType(SerializedProperty property, Type type, Button button, VisualElement childContainer)
        {
            property.serializedObject.Update();
            property.managedReferenceValue = Activator.CreateInstance(type);
            property.serializedObject.ApplyModifiedProperties();
            button.text = type.Name;
            BuildChildFields(property, childContainer);
        }

        static string GetCurrentTypeName(SerializedProperty property) =>
            property.managedReferenceValue != null
                ? property.managedReferenceValue.GetType().Name
                : NullButtonText;

        /// <summary>
        /// 渲染当前实例的序列化子字段。实例为 null 时清空子视图。
        /// </summary>
        static void BuildChildFields(SerializedProperty property, VisualElement container)
        {
            container.Clear();
            if (property.managedReferenceValue == null)
            {
                return;
            }

            // 遍历托管引用实例的直接子属性；PropertyField(SerializedProperty) 构造即自绑定
            var iterator = property.Copy();
            var end = iterator.GetEndProperty();
            var isFirst = true;
            while (iterator.NextVisible(isFirst) && !SerializedProperty.EqualContents(iterator, end))
            {
                isFirst = false;
                container.Add(new PropertyField(iterator.Copy()) { label = iterator.displayName });
            }
        }

        /// <summary>
        /// 收集字段声明类型的可选子类：排除抽象、泛型定义、不可序列化、
        /// UnityEngine.Object 派生与标有 <see cref="ExcludeSubclassSelectorAttribute" /> 的类型，
        /// 按命名空间 + 类名排序。
        /// </summary>
        static List<Type> GetFilteredTypes(Type baseType)
        {
            var result = new List<Type>();
            foreach (var type in TypeCache.GetTypesDerivedFrom(baseType))
            {
                if (type.IsAbstract || type.IsGenericTypeDefinition ||
                    !type.IsDefined(typeof(SerializableAttribute), false) ||
                    typeof(Object).IsAssignableFrom(type) ||
                    type.IsDefined(typeof(ExcludeSubclassSelectorAttribute), false))
                {
                    continue;
                }

                result.Add(type);
            }

            result.Sort(static (a, b) =>
            {
                var ns = string.CompareOrdinal(a.Namespace ?? string.Empty, b.Namespace ?? string.Empty);
                return ns != 0 ? ns : string.CompareOrdinal(a.Name, b.Name);
            });
            return result;
        }
    }
}
