using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// <see cref="ObservableValue{T}" /> 的 UI Toolkit 属性绘制器（无 Odin 依赖）。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     复刻 <c>ObservableValueAttributeProcessor</c> 的四条 Odin 效果：
    ///     <list type="bullet">
    ///     <item><c>HideLabel</c>：根容器不画自身标签，改由内层 value 字段承载语义标签。</item>
    ///     <item><c>InlineProperty</c>：直接内联 value 字段的 <see cref="PropertyField" />，不产生嵌套 Foldout。</item>
    ///     <item>标签取父属性名（Odin 表达式 <c>@$property.Parent.Name</c> 的等价实现），用 <see cref="SerializedProperty.displayName" />。</item>
    ///     <item><c>OnValueChanged</c>：value 变更时调用 <see cref="ObservableValue{T}.InvokeEvent" /> 触发响应式通知。</item>
    ///     </list>
    ///     </para>
    ///     <para>
    ///     <b>开放泛型</b>：[CustomPropertyDrawer] 绑定开放类型 <c>ObservableValue&lt;&gt;</c>，Unity 2022.3
    ///     会为所有封闭构造（int / float / Vector3 / 自定义 enum …）匹配本绘制器，已实测确认。
    ///     </para>
    ///     <para>
    ///     <b>值与通知</b>：Unity 序列化直接写入 <c>value</c> 字段、绕过 <see cref="ObservableValue{T}.Value" />
    ///     setter，故不会自发通知。这里用 <c>TrackPropertyValue</c> 监听 value 字段变更，反射拿到实例后调用
    ///     <see cref="ObservableValue{T}.InvokeEvent" />（方法名取自 <see cref="ObservableValue{T}.InvokeMethodName" />）补齐通知。
    ///     </para>
    /// </remarks>
    [CustomPropertyDrawer(typeof(ObservableValue<>))]
    internal sealed class ObservableValueDrawer : PropertyDrawer
    {
        /// <summary>眼睛图标字符，对应 Odin 的 <c>SdfIconType.EyeFill</c>（UI Toolkit 无原生 SDF 图标，用 Unicode 近似）。</summary>
        const string EyeIcon = "\u25C9"; // ◉

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();
            root.AddToClassList("aesir-observable-value");

            var valueProperty = property.FindPropertyRelative(ObservableValue<int>.PrivateValueFieldName);
            if (valueProperty == null)
            {
                root.Add(new HelpBox($"找不到 {ObservableValue<int>.PrivateValueFieldName} 序列化字段。",
                    HelpBoxMessageType.Error));
                return root;
            }

            // HideLabel + InlineProperty：内联 value 字段，标签用父属性名 + 眼睛图标
            var valueField = new PropertyField(valueProperty.Copy())
            {
                label = $"{EyeIcon} {property.displayName}"
            };
            valueField.AddToClassList("aesir-observable-value__field");
            root.Add(valueField);

            // OnValueChanged：监听序列化 value 变更，反射调用 InvokeEvent 补齐通知
            valueField.TrackPropertyValue(valueProperty.Copy(), _ => InvokeNotification(property));

            return root;
        }

        /// <summary>
        /// 对正在绘制的 ObservableValue 实例调用 <see cref="ObservableValue{T}.InvokeEvent" />。
        /// </summary>
        /// <remarks>
        /// 仅处理直属宿主字段（depth == 0）：集合元素等深层属性无法通过 <see cref="PropertyDrawer.fieldInfo" />
        /// 定位实例，这类场景本就不是快捷示例的主要用法，降级为不通知（与 Odin 版在深层属性上的行为一致）。
        /// </remarks>
        void InvokeNotification(SerializedProperty property)
        {
            if (fieldInfo == null || property.depth != 0 || property.serializedObject.targetObject == null)
            {
                return;
            }

            var instance = fieldInfo.GetValue(property.serializedObject.targetObject);
            if (instance == null)
            {
                return;
            }

            var method = instance.GetType().GetMethod(ObservableValue<int>.InvokeMethodName,
                BindingFlags.Instance | BindingFlags.Public);
            method?.Invoke(instance, null);
        }
    }
}
