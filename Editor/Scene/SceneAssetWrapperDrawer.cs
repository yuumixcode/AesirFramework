#if !ODIN_INSPECTOR
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirModules.Editor
{
    /// <summary>
    /// <see cref="SceneAssetWrapper" /> 的原生 PropertyDrawer —— 无 Odin Inspector 环境的 Inspector 兜底。
    /// <para>
    /// Odin 在场时本类型经 <c>#if !ODIN_INSPECTOR</c> 整体剔除，面板效果仍由
    /// SceneAssetWrapperAttributeProcessor（三态着色 / 一键修复按钮 / 悬空 Error 框）提供；
    /// 未安装 Odin 时由本 Drawer 提供等价信息量的操作面：场景资产拖拽框、状态行（与 Odin 版同源的
    /// <see cref="SceneAssetWrapper.GetSceneAssetColor" /> 着色）、BuildSettings 一键修复按钮。
    /// </para>
    /// <para>仅支持单值字段（wrapper 不作集合使用）；数组 / 列表元素场景显示为不支持。</para>
    /// </summary>
    [CustomPropertyDrawer(typeof(SceneAssetWrapper))]
    public class SceneAssetWrapperDrawer : PropertyDrawer
    {
        const float StatusLineHeight = 16f;
        const float ButtonHeight = 20f;
        const float Spacing = 2f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var wrapper = GetWrapper(property);
            if (wrapper == null)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            var height = EditorGUIUtility.singleLineHeight; // 场景资产拖拽框
            height += Spacing + StatusLineHeight;           // 状态行
            if (wrapper.CanAddToBuild || wrapper.CanEnableInBuild)
            {
                height += Spacing + ButtonHeight;           // 修复按钮行
            }

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var wrapper = GetWrapper(property);
            if (wrapper == null)
            {
                EditorGUI.HelpBox(position, "SceneAssetWrapper 字段为 null 或位于集合中（不支持集合元素绘制）。",
                    MessageType.None);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            // 场景资产拖拽框：赋值经 SceneAsset setter 自动同步路径 / GUID / Addressables 地址
            var rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            var asset = (SceneAsset)EditorGUI.ObjectField(rect, label, wrapper.SceneAsset, typeof(SceneAsset), false);
            if (!ReferenceEquals(asset, wrapper.SceneAsset))
            {
                RecordAndApply(property, "Set SceneAssetWrapper", w => w.SceneAsset = asset);
            }

            // 状态行（与 Odin 版同源着色：悬空/未入 Build 红、禁用黄、Addressable 青、正常白）
            rect.y += rect.height + Spacing;
            rect.height = StatusLineHeight;
            var previousColor = GUI.color;
            GUI.color = wrapper.GetSceneAssetColor();
            EditorGUI.LabelField(rect, BuildStateText(wrapper), EditorStyles.miniLabel);
            GUI.color = previousColor;

            // 修复按钮（与 Odin 版同款一键动作）
            if (wrapper.CanAddToBuild || wrapper.CanEnableInBuild)
            {
                rect.y += rect.height + Spacing;
                rect.height = ButtonHeight;
                if (GUI.Button(rect, wrapper.CanAddToBuild ? "添加到 BuildSettings" : "在 BuildSettings 中启用"))
                {
                    RecordAndApply(property, "Fix Scene In BuildSettings",
                        wrapper.CanAddToBuild
                            ? (System.Action<SceneAssetWrapper>)(w => w.AddCurrentSceneToBuildSettings())
                            : w => w.EnableCurrentSceneInBuildSettings());
                }
            }

            EditorGUI.EndProperty();
        }

        /// <summary>取宿主对象上的 wrapper 实例（仅支持单值字段；集合字段返回 null）。</summary>
        SceneAssetWrapper GetWrapper(SerializedProperty property) =>
            fieldInfo != null
                ? fieldInfo.GetValue(property.serializedObject.targetObject) as SceneAssetWrapper
                : null;

        void RecordAndApply(SerializedProperty property, string undoName,
            System.Action<SceneAssetWrapper> apply)
        {
            var target = property.serializedObject.targetObject;
            var wrapper = fieldInfo != null ? fieldInfo.GetValue(target) as SceneAssetWrapper : null;
            if (wrapper == null)
            {
                return;
            }

            Undo.RecordObject(target, undoName);
            apply(wrapper);
            EditorUtility.SetDirty(target);
        }

        static string BuildStateText(SceneAssetWrapper wrapper)
        {
            if (wrapper.IsDangling)
            {
                return "引用悬空：场景资产已不存在，请重新拖拽修复";
            }

            var state = wrapper.State;
            switch (state)
            {
                case SceneAssetWrapperState.Regular:
                    return wrapper.DisabledInBuildSettings
                        ? $"Build Settings 条目被禁用：{wrapper.ScenePath}"
                        : $"正常（Build Settings 已登记）：{wrapper.ScenePath}";
                case SceneAssetWrapperState.Addressable:
                    try
                    {
                        return $"Addressable：{wrapper.Address}";
                    }
                    catch (SceneAssetWrapperException)
                    {
                        // Addressables 包已卸载（地址残留）时 Address 抛 SupportDisabled——面板容错显示而非中断绘制
                        return "Addressable：地址不可用（Addressables 支持已禁用）";
                    }
                default:
                    var reason = wrapper.UnsafeReason;
                    return reason != SceneAssetWrapperUnsafeReason.Empty
                        ? $"不可加载：{reason}"
                        : "未分配场景";
            }
        }
    }
}
#endif
