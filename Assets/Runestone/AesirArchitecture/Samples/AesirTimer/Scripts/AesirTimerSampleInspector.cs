#if UNITY_EDITOR // 示例仅编辑器内参与编译（与本目录其他示例文件同构：运行时程序集 + 整文件 #if）
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// 示例组件的自定义 Inspector —— 把"计时器在检视面板上的样子"直接摆出来。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     框架自带的 <c>[CustomPropertyDrawer(typeof(AesirTimer))]</c> 只对 <c>[SerializeReference]</c>
    ///     字段生效（原生 Inspector 不为普通引用类型建立属性），而计时器由工厂方法在运行时创建、
    ///     没有可序列化的初始状态，示例因此走"自定义 Inspector"这条路：字段仍是普通私有字段，
    ///     面板只做只读展示 + 动作按钮。
    ///     </para>
    ///     <para>
    ///     行数据来自 <see cref="AesirTimerSample.CaptureRows" />，与运行时 <c>OnGUI</c> 面板同源；
    ///     本类只提供另一套样式（EditorGUILayout 表格 + 按钮），换样式不影响数据。
    ///     </para>
    ///     <para>
    ///     动作按钮与组件的 <c>ContextMenu</c> 调用同一批方法——右键菜单是脱离本文件也能用的兜底路径，
    ///     按钮是可发现的主路径。
    ///     </para>
    /// </remarks>
    [CustomEditor(typeof(AesirTimerSample))]
    internal sealed class AesirTimerSampleInspector : Editor
    {
        /// <summary>播放中的重绘间隔：约 10 Hz，足够看清进度条又不至于让面板和 Game 视图抢资源。</summary>
        const double RepaintIntervalSeconds = 0.1;

        readonly List<AesirTimerSample.TimerRow> _rows = new List<AesirTimerSample.TimerRow>();

        double _nextRepaintTime;

        void OnEnable() => EditorApplication.update += OnEditorUpdate;

        void OnDisable() => EditorApplication.update -= OnEditorUpdate;

        /// <summary>
        /// 播放中按固定间隔重绘（编辑模式完全不产生重绘：那里没有会变化的数据）。
        /// </summary>
        void OnEditorUpdate()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            var now = EditorApplication.timeSinceStartup;

            if (now < _nextRepaintTime)
            {
                return;
            }

            _nextRepaintTime = now + RepaintIntervalSeconds;
            Repaint();
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var sample = (AesirTimerSample)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("计时器状态（只读查询，无驱动代码）", EditorStyles.boldLabel);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "计时器只在 Play 模式下由 PlayerLoop 推进：编辑模式没有数据源，这里显示为空闲状态。",
                    MessageType.Info);
            }

            sample.CaptureRows(_rows);

            for (var i = 0; i < _rows.Count; i++)
            {
                DrawRow(_rows[i]);
            }

            EditorGUILayout.Space();
            DrawActions(sample);
        }

        /// <summary>Inspector 样式：标题 + 着色状态词 + 进度条 + 明细。</summary>
        static void DrawRow(in AesirTimerSample.TimerRow row)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(row.Label, EditorStyles.miniLabel, GUILayout.Width(130f));

            var previousColor = GUI.contentColor;
            GUI.contentColor = row.StateColor;
            GUILayout.Label(row.State, EditorStyles.miniBoldLabel, GUILayout.Width(110f));
            GUI.contentColor = previousColor;

            var barRect = GUILayoutUtility.GetRect(72f, EditorGUIUtility.singleLineHeight, GUILayout.Width(72f));
            EditorGUI.ProgressBar(barRect, row.Progress, $"{row.Progress * 100f:0}%");

            GUILayout.Label(row.Detail, EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
        }

        static void DrawActions(AesirTimerSample sample)
        {
            // 编辑模式下创建计时器只会往注册表里塞永远不会推进的实例，因此整组按钮只在 Play 模式可用
            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("开始延时")) sample.StartDelay();
                    if (GUILayout.Button("开始冷却")) sample.StartCooldown();
                    if (GUILayout.Button("每 N 秒 ×M 次")) sample.StartRepeats();
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("切换心跳 ∞")) sample.ToggleHeartbeat();
                    if (GUILayout.Button("下一帧一次")) sample.RunNextFrame();
                    if (GUILayout.Button("冲刺一次")) sample.TryDash();
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("秒表开始/读取")) sample.ToggleStopwatch();
                    if (GUILayout.Button("区间 3s×0.5s")) sample.StartInterval();
                    if (GUILayout.Button("频率 4/s")) sample.ToggleFrequency();
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("暂停全部")) sample.PauseAll();
                    if (GUILayout.Button("恢复全部")) sample.ResumeAll();
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("重开冷却")) sample.RestartCooldown();
                    if (GUILayout.Button("取消全部")) sample.CancelAll();
                }
            }
        }
    }
}
#endif
