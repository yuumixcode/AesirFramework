using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace Runestone.AesirModules.Editor.OdinInspector
{
    /// <summary>
    /// 为 <see cref="UIModule" /> 提供 Odin Inspector 属性处理器，
    /// 动态注入 DDOL 开关的说明与警告信息框，使 Runtime 程序集零 Odin 依赖。
    /// </summary>
    public class UIModuleAttributeProcessor : OdinAttributeProcessor<UIModule>
    {
        /// <summary>
        /// DDOL 开关的取值含义说明文案（字段级 Info 信息框内容）。
        /// </summary>
        /// <remarks>
        /// 与其余三个 DDOL 开关不同：UIModule 的开关仅在预放置为根物体时生效，
        /// 运行时自动创建的实例挂载在 [Aesir Modules] 宿主下、跟随宿主决策。
        /// </remarks>
        const string DontDestroyOnLoadFieldInfo = "勾选：本物体（预放置为根物体时）加入 DontDestroyOnLoad 场景，跨场景持久存在（默认）。\n" +
                                                  "取消勾选：本物体保留在所在场景、随场景卸载销毁——必须自行处理多场景叠加（Additive）加载。\n" +
                                                  "运行时自动创建的实例挂载在 [Aesir Modules] 下，DDOL 跟随宿主决策，本字段不生效。";

        /// <summary>
        /// 处理子成员的特性，为 DDOL 开关字段动态注入取值含义说明（Info 级信息框，恒显示）
        /// </summary>
        /// <param name="parentProperty">父属性节点（目标对象本身）</param>
        /// <param name="member">正在处理的成员信息</param>
        /// <param name="attributes">该成员当前已附加的特性列表，处理器可向其中添加新特性</param>
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty,
            MemberInfo member,
            List<Attribute> attributes)
        {
            if (member.Name == UIModule.DontDestroyOnLoadFieldName)
            {
                attributes.Add(new InfoBoxAttribute(DontDestroyOnLoadFieldInfo));
            }
        }
    }
}
