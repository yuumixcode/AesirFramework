using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// <see cref="AesirTimer" /> 字段的 Odin 检视摘要：在默认绘制之前插入一行
    /// "状态 · 形态 · 剩余 · 次数 · 回调"，并用进度条呈现当前间隔的进度。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     Odin 按反射绘制成员，因此只要该字段处于 Odin 的属性树中就会被绘制——
    ///     例如宿主是 <c>SerializedMonoBehaviour</c> / <c>SerializedScriptableObject</c>，
    ///     或字段标了 <c>[ShowInInspector]</c>。普通 <c>MonoBehaviour</c> 的非序列化字段
    ///     两个 Inspector 都不会显示（Unity 侧的限制），请用
    ///     <c>Tools/Aesir/Architecture/Timer Debugger</c> 窗口查看。
    ///     </para>
    ///     <para>
    ///     绘制后经 <c>CallNextDrawer</c> 让默认绘制继续（空引用仍保持 Odin 的拖拽/引用语义）。
    ///     抓取走 <see cref="AesirTimerSnapshot" />，外观走 <see cref="AesirTimerSummaryView" />，
    ///     措辞由 <see cref="AesirTimerText" /> 统一提供。
    ///     </para>
    /// </remarks>
    [DrawerPriority]
    internal sealed class AesirTimerOdinDrawer : OdinValueDrawer<AesirTimer>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var timer = AesirTimerSnapshot.Capture(ValueEntry.SmartValue);

            var stateRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
            var previousColor = GUI.contentColor;
            GUI.contentColor = AesirTimerText.StateColor(timer);
            EditorGUI.LabelField(stateRect, label, new GUIContent(AesirTimerText.DescribeState(timer)));
            GUI.contentColor = previousColor;

            var summaryRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
            AesirTimerSummaryView.Draw(summaryRect, timer);

            CallNextDrawer(label);
        }
    }
}
