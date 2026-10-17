#if !ODIN_INSPECTOR
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirModules.Editor
{
    /// <summary>
    /// <see cref="SceneModuleConfigSO" /> 的原生 Inspector 兜底 —— 无 Odin Inspector 环境下
    /// 让模块级配置（自定义启动场景 / 加载进度归一化上限）在 Project 窗口可直接编辑。
    /// <para>
    /// Odin 在场时本类型经 <c>#if !ODIN_INSPECTOR</c> 整体剔除（Odin 直接绘制序列化字段并叠加其样式增强）；
    /// 未安装 Odin 时若缺少本兜底，两个 internal 字段的子内容全部不可见，Inspector 只剩一个空资源面板
    /// ——「无需预放置 SceneModule 即可配置启动场景」的承诺将无法兑现。
    /// 启动场景字段经 <see cref="SceneAssetWrapperDrawer" /> 渲染（拖拽 + 状态行 + 一键修复按钮）。
    /// </para>
    /// </summary>
    [CustomEditor(typeof(SceneModuleConfigSO))]
    public class SceneModuleConfigSOEditor : UnityEditor.Editor
    {
        SerializedProperty _bootstrapScene;
        SerializedProperty _progressCap;

        void OnEnable()
        {
            _bootstrapScene = serializedObject.FindProperty("bootstrapScene");
            _progressCap = serializedObject.FindProperty("progressCap");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // 启动场景：走 SceneAssetWrapperDrawer（含拖拽框 / 状态行 / BuildSettings 一键修复）；
            // 不传自定义 label——由 Unity 自动读取字段的 [Tooltip] 特性
            EditorGUILayout.PropertyField(_bootstrapScene, true);
            EditorGUILayout.Slider(_progressCap, 0.01f, 1f, new GUIContent("加载进度归一化上限"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
