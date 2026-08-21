using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace Runestone.AesirArchitecture.Editor.OdinIntegration
{
    /// <summary>
    /// 为 AesirArchitecture 类动态添加特性。
    /// </summary>
    /// <remarks>
    /// Inspector 呈现全部由本处理器动态注入（样式与逻辑分离），运行时程序集不持有任何 Inspector 样式特性：
    /// <list type="bullet">
    ///     <item>类级 Info 信息框：类职责说明（恒显示）。</item>
    ///     <item>类级 Warning 信息框：DDOL 开关关闭时的多场景叠加风险提醒（仅在关闭时显示）。</item>
    ///     <item>字段级 Info 信息框：<c>dontDestroyOnLoad</c> 开关的取值含义说明（恒显示，替代运行时 Tooltip）。</item>
    /// </list>
    /// </remarks>
    public class AesirArchitectureAttributeProcessor : OdinAttributeProcessor<AesirArchitecture>
    {
        /// <summary>
        /// DDOL 开关的取值含义说明文案（字段级 Info 信息框内容）。
        /// </summary>
        const string DontDestroyOnLoadFieldInfo = "勾选：本物体加入 DontDestroyOnLoad 场景，跨场景持久存在（默认）。\n" +
                                                  "取消勾选：本物体保留在所在场景、随场景卸载销毁——必须自行处理多场景叠加（Additive）加载。\n" +
                                                  "运行时自动创建的实例恒为勾选状态。";

        /// <summary>
        /// 处理类自身的特性，添加描述信息框与 DDOL 关闭警告信息框
        /// </summary>
        /// <param name="property">Odin Inspector 正在构建的属性节点，代表被处理的目标对象</param>
        /// <param name="attributes">该属性节点当前已附加的特性列表，处理器可向其中添加新特性</param>
        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        {
            attributes.Add(new InfoBoxAttribute("Aesir Architecture 接入 MonoBehaviour 生命周期的全局持久化物体对象"));
        }

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
            if (member.Name == AesirArchitecture.DontDestroyOnLoadFieldName)
            {
                attributes.Add(new InfoBoxAttribute(DontDestroyOnLoadFieldInfo));
            }
        }
    }
}
