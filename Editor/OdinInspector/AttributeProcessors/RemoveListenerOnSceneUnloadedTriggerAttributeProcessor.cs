using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace Runestone.AesirArchitecture.Editor.OdinInspector
{
    /// <summary>
    /// 为 RemoveListenerOnSceneUnloadedTrigger 类提供 Odin Inspector 属性处理器
    /// </summary>
    /// <remarks>
    /// 利用 Odin Attribute Processor 机制，在不修改 <see cref="RemoveListenerOnSceneUnloadedTrigger" /> 代码的前提下，
    /// 为其在 Inspector 中自动添加 <see cref="InfoBoxAttribute" /> 警告信息框，
    /// 提示宿主 DDOL 决策与本组件生命周期的联动约束：
    /// 本组件挂载于 <c>[Aesir Architecture]</c> 宿主，若宿主的 <c>dontDestroyOnLoad</c> 被关闭，
    /// 宿主（含本组件）会随所在场景卸载销毁，此后其他场景卸载将不再自动清理监听 —
    /// 使用该配置时需自行处理多场景叠加（Additive）加载下的生命周期。
    /// </remarks>
    public class RemoveListenerOnSceneUnloadedTriggerAttributeProcessor :
        OdinAttributeProcessor<RemoveListenerOnSceneUnloadedTrigger>
    {
        const string HostDontDestroyOnLoadDisabledWarning =
            "若 [Aesir Architecture] 宿主的 dontDestroyOnLoad 被关闭：宿主（含本组件）将随所在场景卸载销毁，" +
            "此后其他场景卸载不再自动清理监听——必须自行处理多场景叠加（Additive）加载下的生命周期";

        /// <summary>
        /// 处理类自身的特性，添加宿主 DDOL 关闭风险警告信息框
        /// </summary>
        /// <param name="property">Odin Inspector 正在构建的属性节点，代表被处理的目标对象</param>
        /// <param name="attributes">该属性节点当前已附加的特性列表，处理器可向其中添加新特性</param>
        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        {
            attributes.Add(
                new InfoBoxAttribute(HostDontDestroyOnLoadDisabledWarning, InfoMessageType.Warning));
        }
    }
}
