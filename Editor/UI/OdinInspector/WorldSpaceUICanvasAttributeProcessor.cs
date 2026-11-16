using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace Runestone.AesirModules.Editor.OdinInspector
{
    /// <summary>
    /// 为 <see cref="WorldSpaceUICanvas" /> 提供 Odin Inspector 属性处理器：
    /// 动态注入分组与条件显示特性，使 Runtime 程序集零 Odin 依赖。
    /// </summary>
    public class WorldSpaceUICanvasAttributeProcessor : OdinAttributeProcessor<WorldSpaceUICanvas>
    {
        /// <summary>
        /// 为跟随 / 朝向 / 配置 / 排序字段注入分组与条件显示。
        /// </summary>
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty,
            MemberInfo member, List<Attribute> attributes)
        {
            switch (member.Name)
            {
                case WorldSpaceUICanvas.FollowTargetFieldName:
                case WorldSpaceUICanvas.PositionOffsetFieldName:
                    attributes.Add(new BoxGroupAttribute("Follow"));
                    break;

                case WorldSpaceUICanvas.FaceCameraFieldName:
                case WorldSpaceUICanvas.BillboardModeFieldName:
                    attributes.Add(new BoxGroupAttribute("Facing"));
                    break;

                case WorldSpaceUICanvas.FlipFacingFieldName:
                    attributes.Add(new BoxGroupAttribute("Facing"));
                    attributes.Add(new ShowIfAttribute(WorldSpaceUICanvas.FaceCameraFieldName));
                    break;

                case WorldSpaceUICanvas.CanvasConfigFieldName:
                    attributes.Add(new BoxGroupAttribute("Rendering"));
                    break;

                case WorldSpaceUICanvas.SortingOrderFieldName:
                    attributes.Add(new BoxGroupAttribute("Rendering"));
                    break;
            }
        }
    }
}
