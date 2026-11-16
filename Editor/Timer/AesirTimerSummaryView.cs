using UnityEditor;
using UnityEngine;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// 单行摘要的 IMGUI 样式：进度条 + 一行状态描述，供两个字段绘制器（原生 / Odin）共用。
    /// </summary>
    /// <remarks>
    /// 绘制与文案分开放：本类只决定"怎么摆"（进度条宽度占比、行内间距），措辞与颜色全部来自
    /// <see cref="AesirTimerText" />。要换一种字段外观（例如只画进度条、或改成两行）时替换本类，
    /// 数据（<see cref="AesirTimerSnapshot" />）与文案都不用动。
    /// </remarks>
    internal static class AesirTimerSummaryView
    {
        /// <summary>
        /// 在给定区域内绘制"进度条 + 单行摘要"。
        /// </summary>
        /// <param name="rect">整块可用区域</param>
        /// <param name="timer">抓取结果，未持有实例时只给出创建指引</param>
        internal static void Draw(Rect rect, in AesirTimerSnapshot timer)
        {
            if (!timer.HasTimer)
            {
                EditorGUI.LabelField(rect,
                    "字段未持有实例：AesirTimer 由工厂方法创建（Countdown / Every / NextFrame）",
                    EditorStyles.miniLabel);
                return;
            }

            var barWidth = Mathf.Min(110f, rect.width * 0.3f);
            var barRect = new Rect(rect.x, rect.y + 2f, barWidth, rect.height - 4f);
            EditorGUI.ProgressBar(barRect, AesirTimerText.Progress01(timer), AesirTimerText.DescribeProgress(timer));

            var textRect = new Rect(barRect.xMax + 6f, rect.y, rect.width - barWidth - 6f, rect.height);
            EditorGUI.LabelField(textRect, AesirTimerText.DescribeOneLine(timer), EditorStyles.miniLabel);
        }
    }
}
