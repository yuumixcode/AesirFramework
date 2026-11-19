using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Runestone.AesirModules.Editor
{
    /// <summary>
    /// <see cref="SceneAssetWrapper" /> 的属性绘制器 —— UI Toolkit 优先，IMGUI 兜底，零 Odin 依赖。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 装了 Odin 时由 <c>SceneAssetWrapperAttributeProcessor</c> 依据样式开关决定让路（注入
    /// <c>DrawWithUnityAttribute</c>）还是接管；未装 Odin 时本绘制器是唯一实现。两条绘制路径共用
    /// <see cref="BuildStateText" /> 与 <see cref="SceneAssetWrapper.GetSceneAssetColor" />，信息量一致。
    /// </para>
    /// <para>仅支持单值字段（wrapper 不作集合使用）；数组 / 列表元素显示为不支持。</para>
    /// </remarks>
    [CustomPropertyDrawer(typeof(SceneAssetWrapper))]
    public class SceneAssetWrapperDrawer : PropertyDrawer
    {
        const float StatusLineHeight = 16f;
        const float ButtonHeight = 20f;
        const float Spacing = 2f;

        /// <summary>UI Toolkit 路径：Inspector / UIToolkit 编辑器走这里。</summary>
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();
            root.AddToClassList("aesir-scene-wrapper");

            var wrapper = GetWrapper(property);
            if (wrapper == null)
            {
                root.Add(new HelpBox("SceneAssetWrapper 字段为 null 或位于集合中（不支持集合元素绘制）。",
                    HelpBoxMessageType.Warning));
                return root;
            }

            var status = new Label();
            status.AddToClassList("aesir-scene-wrapper__status");

            var actions = new VisualElement();
            actions.AddToClassList("aesir-scene-wrapper__actions");

            // 场景资产拖拽框：赋值经 SceneAsset setter 自动同步路径 / GUID / Addressables 地址
            var objectField = new ObjectField(property.displayName)
            {
                objectType = typeof(SceneAsset),
                allowSceneObjects = false,
                value = wrapper.SceneAsset
            };
            objectField.RegisterValueChangedCallback(evt =>
            {
                var asset = evt.newValue as SceneAsset;
                if (ReferenceEquals(asset, wrapper.SceneAsset))
                {
                    return;
                }

                Apply(property, "Set SceneAssetWrapper", w => w.SceneAsset = asset);
                Refresh();
            });
            root.Add(objectField);
            root.Add(status);
            root.Add(actions);

            void Refresh()
            {
                var current = GetWrapper(property);
                if (current == null)
                {
                    return;
                }

                status.text = BuildStateText(current);
                status.style.color = current.GetSceneAssetColor();
                status.style.whiteSpace = WhiteSpace.Normal;

                actions.Clear();
                if (current.CanAddToBuild)
                {
                    AddFixButton(actions, "添加到 BuildSettings",
                        () => Apply(property, "Fix Scene In BuildSettings", w => w.AddCurrentSceneToBuildSettings()));
                }

                if (current.CanEnableInBuild)
                {
                    AddFixButton(actions, "在 BuildSettings 中启用",
                        () => Apply(property, "Fix Scene In BuildSettings", w => w.EnableCurrentSceneInBuildSettings()));
                }

                if (current.CanMakeAddressable)
                {
                    AddFixButton(actions, "加入 Addressables 默认组",
                        () => Apply(property, "Make Scene Addressable", w => w.AddSceneToAddressables()));
                }
            }

            Refresh();
            return root;
        }

        /// <summary>IMGUI 路径：Odin 编辑器窗口内 <c>DrawWithUnity</c> 让路后的绘制。</summary>
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var wrapper = GetWrapper(property);
            if (wrapper == null)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            var height = EditorGUIUtility.singleLineHeight; // 场景资产拖拽框
            height += Spacing + StatusLineHeight;           // 状态行
            var fixCount = FixCount(wrapper);
            if (fixCount > 0)
            {
                height += Spacing + (ButtonHeight + Spacing) * fixCount;
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

            var rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            var asset = (SceneAsset)EditorGUI.ObjectField(rect, label, wrapper.SceneAsset, typeof(SceneAsset), false);
            if (!ReferenceEquals(asset, wrapper.SceneAsset))
            {
                Apply(property, "Set SceneAssetWrapper", w => w.SceneAsset = asset);
            }

            rect.y += rect.height + Spacing;
            rect.height = StatusLineHeight;
            var previousColor = GUI.color;
            GUI.color = wrapper.GetSceneAssetColor();
            EditorGUI.LabelField(rect, BuildStateText(wrapper), EditorStyles.miniLabel);
            GUI.color = previousColor;

            var cursor = new Rect(rect.x, rect.y + rect.height + Spacing, rect.width, ButtonHeight);
            DrawFixButton(cursor, wrapper.CanAddToBuild, "添加到 BuildSettings",
                () => Apply(property, "Fix Scene In BuildSettings", w => w.AddCurrentSceneToBuildSettings()), ref cursor);
            DrawFixButton(cursor, wrapper.CanEnableInBuild, "在 BuildSettings 中启用",
                () => Apply(property, "Fix Scene In BuildSettings", w => w.EnableCurrentSceneInBuildSettings()), ref cursor);
            DrawFixButton(cursor, wrapper.CanMakeAddressable, "加入 Addressables 默认组",
                () => Apply(property, "Make Scene Addressable", w => w.AddSceneToAddressables()), ref cursor);

            EditorGUI.EndProperty();
        }

        static void AddFixButton(VisualElement parent, string label, Action onClick)
        {
            var button = new Button(onClick) { text = label };
            button.AddToClassList("aesir-button");
            parent.Add(button);
        }

        static void DrawFixButton(Rect rect, bool visible, string label, Action onClick, ref Rect cursor)
        {
            if (!visible)
            {
                return;
            }

            if (GUI.Button(rect, label))
            {
                onClick();
            }

            cursor.y += ButtonHeight + Spacing;
        }

        static int FixCount(SceneAssetWrapper wrapper) =>
            (wrapper.CanAddToBuild ? 1 : 0) +
            (wrapper.CanEnableInBuild ? 1 : 0) +
            (wrapper.CanMakeAddressable ? 1 : 0);

        /// <summary>取宿主对象上的 wrapper 实例（仅支持单值字段；集合字段返回 null）。</summary>
        SceneAssetWrapper GetWrapper(SerializedProperty property) =>
            fieldInfo != null
                ? fieldInfo.GetValue(property.serializedObject.targetObject) as SceneAssetWrapper
                : null;

        void Apply(SerializedProperty property, string undoName, Action<SceneAssetWrapper> apply)
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
            property.serializedObject.Update();
        }

        static string BuildStateText(SceneAssetWrapper wrapper)
        {
            if (wrapper.IsDangling)
            {
                return "引用悬空：场景资产已不存在，请重新拖拽修复";
            }

            switch (wrapper.State)
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
