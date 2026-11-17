using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// <see cref="AesirTimer" /> 字段的只读检视：状态词（着色）+ 进度条 + 单行摘要。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     <b>生效前提</b>：字段必须是 Unity 的序列化属性。AesirTimer 是普通引用类型，
    ///     原生 Inspector 只为 <c>[SerializeReference]</c> 字段建立托管引用属性；写成普通
    ///     <c>public AesirTimer</c> 时它根本不在属性树里（Unity 不绘制不可序列化的类型），
    ///     本绘制器也就不会被调用。零侵入的查看路径是
    ///     <c>Tools/Aesir/Architecture/Timer Debugger</c> 窗口；想在自己组件的面板里看计时器，
    ///     写自定义 Inspector（见 <c>Samples/AesirTimer</c> 的示例）。
    ///     </para>
    ///     <para>
    ///     <b>取值方式</b>：走 <see cref="PropertyDrawer.fieldInfo" /> 反射而不是
    ///     <c>SerializedProperty.managedReferenceValue</c>——后者的 getter 会为 null 引用
    ///     即时实例化对象（看一眼面板就凭空多出一个计时器实例）。
    ///     因此只在字段深度为 0（直属宿主对象）时读取；集合元素等位置退化为创建指引文本。
    ///     </para>
    ///     <para>
    ///     绘制外观来自 <see cref="AesirTimerSummaryView" />，措辞来自 <see cref="AesirTimerText" />，
    ///     本类只负责把字段值抓成 <see cref="AesirTimerSnapshot" />。
    ///     </para>
    /// </remarks>
    [CustomPropertyDrawer(typeof(AesirTimer))]
    internal sealed class AesirTimerDrawer : PropertyDrawer
    {
        const float Spacing = 2f;

        /// <summary>
        /// UI Toolkit 绘制入口（复刻 AesirTimerOdinDrawer 的两行摘要）：状态词着色 + 进度条 + 单行摘要。
        /// </summary>
        /// <remarks>
        /// 与 <see cref="OnGUI" /> 并存：UI Toolkit Inspector 走本方法，纯 IMGUI 上下文走 OnGUI。
        /// 数据取 <see cref="AesirTimerSnapshot" />，文案 / 颜色取 <see cref="AesirTimerText" />，
        /// 外观用 VisualElement 重排（IMGUI 的 AesirTimerSummaryView 不复用）。
        /// </remarks>
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();
            root.AddToClassList("aesir-timer-field");

            var timer = AesirTimerSnapshot.Capture(ResolveTimer(property));

            var stateLabel = new Label($"{property.displayName}：{AesirTimerText.DescribeState(timer)}");
            stateLabel.style.color = AesirTimerText.StateColor(timer);
            stateLabel.AddToClassList("aesir-timer-field__state");
            root.Add(stateLabel);

            if (!timer.HasTimer)
            {
                root.Add(new HelpBox(
                    "字段未持有实例：AesirTimer 由工厂方法创建（Countdown / Every / NextFrame）",
                    HelpBoxMessageType.Info));
                return root;
            }

            var bar = new ProgressBar
            {
                value = AesirTimerText.Progress01(timer) * 100f,
                title = AesirTimerText.DescribeProgress(timer)
            };
            bar.AddToClassList("aesir-timer-field__progress");
            root.Add(bar);

            var summaryLabel = new Label(AesirTimerText.DescribeOneLine(timer));
            summaryLabel.AddToClassList("aesir-timer-field__summary");
            root.Add(summaryLabel);

            return root;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
            EditorGUIUtility.singleLineHeight * 2f + Spacing;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var timer = AesirTimerSnapshot.Capture(ResolveTimer(property));
            var lineHeight = EditorGUIUtility.singleLineHeight;

            var stateRect = new Rect(position.x, position.y, position.width, lineHeight);
            var previousColor = GUI.contentColor;
            GUI.contentColor = AesirTimerText.StateColor(timer);
            EditorGUI.LabelField(stateRect, label, new GUIContent(AesirTimerText.DescribeState(timer)));
            GUI.contentColor = previousColor;

            var summaryRect = new Rect(position.x, stateRect.yMax + Spacing, position.width, lineHeight);
            AesirTimerSummaryView.Draw(summaryRect, timer);
        }

        AesirTimer ResolveTimer(SerializedProperty property)
        {
            if (fieldInfo == null || property.depth != 0 || property.serializedObject.targetObject == null)
            {
                return null;
            }

            return fieldInfo.GetValue(property.serializedObject.targetObject) as AesirTimer;
        }
    }
}
