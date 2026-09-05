using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace Runestone.AesirModules.Editor.OdinInspector
{
    /// <summary>
    /// 为 <see cref="AesirModules" /> 提供 Odin Inspector 属性处理器，
    /// 动态注入 DDOL 开关的说明与警告信息框，使 Runtime 程序集零 Odin 依赖。
    /// </summary>
    public class AesirModulesAttributeProcessor : OdinAttributeProcessor<AesirModules>
    {
        /// <summary>
        /// DDOL 开关的取值含义说明文案（字段级 Info 信息框内容）。
        /// </summary>
        const string DontDestroyOnLoadFieldInfo = "勾选：本物体加入 DontDestroyOnLoad 场景，跨场景持久存在（默认）。\n" +
                                                  "取消勾选：本物体保留在所在场景、随场景卸载销毁——必须自行处理多场景叠加（Additive）加载。\n" +
                                                  "运行时自动创建的实例恒为勾选状态。";

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
            if (member.Name == AesirModules.DontDestroyOnLoadFieldName)
            {
                attributes.Add(new InfoBoxAttribute(DontDestroyOnLoadFieldInfo));
            }
        }
    }
}
