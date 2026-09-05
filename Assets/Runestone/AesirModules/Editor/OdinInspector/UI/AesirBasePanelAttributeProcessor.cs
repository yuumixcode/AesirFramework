using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace Runestone.AesirModules.Editor.OdinInspector
{
    /// <summary>
    /// 为 <see cref="AesirBasePanel" /> 提供 Odin Inspector 属性处理器，
    /// 动态注入 Inspector 显示特性，使 Runtime 程序集零 Odin 依赖。
    /// </summary>
    public class AesirBasePanelAttributeProcessor : OdinAttributeProcessor<AesirBasePanel>
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty,
            MemberInfo member,
            List<Attribute> attributes)
        {
            switch (member.Name)
            {
                case AesirBasePanel.LayerFieldName:
                    attributes.Add(new PropertyOrderAttribute(-999));
                    attributes.Add(new BoxGroupAttribute("Aesir Base Panel 基础配置"));
                    attributes.Add(new LabelTextAttribute("UI 层级"));
                    break;

                case AesirBasePanel.DestroyOnHideFieldName:
                    attributes.Add(new PropertyOrderAttribute(-999));
                    attributes.Add(new BoxGroupAttribute("Aesir Base Panel 基础配置"));
                    attributes.Add(new LabelTextAttribute("隐藏时销毁物体对象"));
                    break;
            }
        }
    }
}
