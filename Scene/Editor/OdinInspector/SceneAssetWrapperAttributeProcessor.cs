using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace Runestone.AesirModules.Editor.OdinInspector
{
    /// <summary>
    /// 为 <see cref="SceneAssetWrapper" /> 提供 Odin Inspector 属性处理器，
    /// 动态注入 Inspector 显示特性，使 Runtime 程序集零 Odin 依赖。
    /// <para>
    /// 工具箱对位 Eflatun.SceneReference 的内联工具（三态着色 + 修复按钮）：
    /// 未加入 BuildSettings（红 + 添加按钮）、已加入但被禁用（黄 + 启用按钮）、
    /// Addressable 场景（青色 + 说明框）、引用悬空（红色）。
    /// </para>
    /// </summary>
    public class SceneAssetWrapperAttributeProcessor : OdinAttributeProcessor<SceneAssetWrapper>
    {
        /// <summary>
        /// 为 SceneAssetWrapper 字段本身注入特性（内联显示、校验提示框）。
        /// </summary>
        public override void ProcessSelfAttributes(InspectorProperty parentProperty,
            List<Attribute> attributes)
        {
            attributes.Add(new InlinePropertyAttribute());

            // 未加入 BuildSettings 且不可 Addressable → Error
            attributes.Add(new InfoBoxAttribute("此场景未加入 BuildSettings，运行时无法加载！",
                InfoMessageType.Error, $"@$value.{nameof(SceneAssetWrapper.CanAddToBuild)}"));

            // 已加入但被禁用 → Warning
            attributes.Add(new InfoBoxAttribute("此场景在 BuildSettings 中被禁用，运行时无法加载！",
                InfoMessageType.Warning, $"@$value.{nameof(SceneAssetWrapper.CanEnableInBuild)}"));

            // Addressable 场景 → 说明（SceneModule 无法加载，需走 Addressables API）
            attributes.Add(new InfoBoxAttribute(
                "此场景为 Addressable 场景：运行时请通过 Addressables API（如 Addressables.LoadSceneAsync(wrapper.Address)）加载，SceneModule 无法加载它。",
                InfoMessageType.Info, $"@$value.{nameof(SceneAssetWrapper.IsAddressable)}"));
        }

        /// <summary>
        /// 为各成员注入 Odin 显示特性。
        /// </summary>
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty,
            MemberInfo member,
            List<Attribute> attributes)
        {
            switch (member.Name)
            {
                case SceneAssetWrapper.SceneAssetPropertyName:
                    attributes.Add(new ShowInInspectorAttribute());
                    attributes.Add(new HideLabelAttribute());
                    attributes.Add(new GUIColorAttribute(SceneAssetWrapper.GetSceneAssetColorMethodName));
                    attributes.Add(new CustomContextMenuAttribute("Reset Scene",
                        SceneAssetWrapper.ResetSceneMethodName));
                    break;

                case SceneAssetWrapper.AddCurrentSceneToBuildSettingsMethodName:
                    attributes.Add(new ShowIfAttribute(nameof(SceneAssetWrapper.CanAddToBuild)));
                    var addButton = new ButtonAttribute("添加到 BuildSettings")
                    {
                        Icon = SdfIconType.PlusSquareFill
                    };
                    attributes.Add(addButton);
                    break;

                case SceneAssetWrapper.EnableCurrentSceneInBuildSettingsMethodName:
                    attributes.Add(new ShowIfAttribute(nameof(SceneAssetWrapper.CanEnableInBuild)));
                    attributes.Add(new ButtonAttribute("在 BuildSettings 中启用"));
                    break;

                case SceneAssetWrapper.AddSceneToAddressablesMethodName:
                    // 仅当安装了 Addressables 包（桥已注册）且场景当前不可寻址时显示
                    attributes.Add(new ShowIfAttribute(nameof(SceneAssetWrapper.CanMakeAddressable)));
                    attributes.Add(new ButtonAttribute("加入 Addressables 默认组"));
                    break;
            }
        }
    }
}
