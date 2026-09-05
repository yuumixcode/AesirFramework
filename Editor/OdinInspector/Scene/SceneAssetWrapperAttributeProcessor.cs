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
            attributes.Add(new InfoBoxAttribute("此场景没有在 BuildSettings 中，请添加后再使用！", InfoMessageType.Error,
                $"@$value.{nameof(SceneAssetWrapper.NotInBuildSettings)}"));
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
                    attributes.Add(new ShowIfAttribute(nameof(SceneAssetWrapper.NotInBuildSettings)));
                    var button = new ButtonAttribute("添加当前场景到 BuildSettings")
                    {
                        Icon = SdfIconType.PlusSquareFill
                    };
                    attributes.Add(button);
                    break;
            }
        }
    }
}
