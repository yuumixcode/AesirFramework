using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace Runestone.AesirModules.Editor.OdinInspector
{
    /// <summary>
    /// 为 <see cref="UIRoot" /> 提供 Odin Inspector 属性处理器，
    /// 动态注入 Inspector 显示特性，使 Runtime 程序集零 Odin 依赖。
    /// </summary>
    public class UIRootAttributeProcessor : OdinAttributeProcessor<UIRoot>
    {
        /// <summary>
        /// DDOL 开关的取值含义说明文案（字段级 Info 信息框内容）。
        /// </summary>
        const string DontDestroyOnLoadFieldInfo = "勾选：本物体加入 DontDestroyOnLoad 场景，跨场景持久存在（默认）。\n" +
                                                  "取消勾选：本物体保留在所在场景、随场景卸载销毁——必须自行处理多场景叠加（Additive）加载。\n" +
                                                  "运行时自动创建的实例恒为勾选状态。";

        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty,
            MemberInfo member,
            List<Attribute> attributes)
        {
            if (member.Name == UIRoot.DontDestroyOnLoadFieldName)
            {
                attributes.Add(new InfoBoxAttribute(DontDestroyOnLoadFieldInfo));
                return;
            }

            switch (member.Name)
            {
                case UIRoot.LayerCanvasesFieldName:
                    attributes.Add(new ShowInInspectorAttribute());
                    attributes.Add(new HideInEditorModeAttribute());
                    break;

                case UIRoot.UICanvasConfigFieldName:
                    attributes.Add(new TitleAttribute("预设 Canvas 配置 [非 WorldSpace Canvas]",
                        "此配置文件将应用到所有预设 Canvas 层，如果未设置则使用默认配置"));
                    attributes.Add(new HideLabelAttribute());
                    attributes.Add(new InlineEditorAttribute(InlineEditorObjectFieldModes.Hidden));
                    break;

                case nameof(UIRoot.Build):
                    attributes.Add(new ButtonAttribute("构建 UIRoot 组件", ButtonSizes.Medium));
                    attributes.Add(new BoxGroupAttribute("UIRoot 辅助操作", centerLabel: true));
                    attributes.Add(new PropertyOrderAttribute(-99));
                    break;

                case UIRoot.CreateCanvasConfigAssetMethodName:
                    attributes.Add(new ButtonAttribute("创建默认 UICanvasConfig 资产并加载", ButtonSizes.Medium));
                    attributes.Add(new PropertyOrderAttribute(-99));
                    attributes.Add(new BoxGroupAttribute("UIRoot 辅助操作", centerLabel: true));
                    break;
            }
        }
    }
}
