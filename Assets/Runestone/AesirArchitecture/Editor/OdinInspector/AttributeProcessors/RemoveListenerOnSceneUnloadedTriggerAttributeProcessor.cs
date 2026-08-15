using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace Runestone.AesirArchitecture.Editor.OdinIntegration
{
    /// <summary>
    /// 为 RemoveListenerOnSceneUnloadedTrigger 类提供 Odin Inspector 属性处理器
    /// </summary>
    /// <remarks>
    /// 利用 Odin Attribute Processor 机制，在不修改 <see cref="RemoveListenerOnSceneUnloadedTrigger"/> 代码的前提下，
    /// 为其在 Inspector 中自动添加 <see cref="InfoBoxAttribute"/> 警告信息框，
    /// 提示场景预放置宿主的生命周期约束：
    /// 本组件挂载于 <c>[Aesir Architecture]</c> 宿主，若宿主为场景预放置实例（不调用 <c>DontDestroyOnLoad</c>），
    /// 宿主所在场景卸载时本组件会随之销毁，此后其他场景卸载将不再自动清理监听 —
    /// 使用预放置宿主时需手动控制生命周期（预放置实例仅推荐用于多场景叠加加载方案）。
    /// </remarks>
    public class RemoveListenerOnSceneUnloadedTriggerAttributeProcessor : OdinAttributeProcessor<RemoveListenerOnSceneUnloadedTrigger>
    {
        const string PrePlacedUsageWarning = "当前实例是场景预放置实例，仅推荐在多场景叠加加载（Additive）方案中使用；需要手动控制其生命周期";

        /// <summary>
        /// 处理类自身的特性，添加预放置宿主风险警告信息框
        /// </summary>
        /// <param name="property">Odin Inspector 正在构建的属性节点，代表被处理的目标对象</param>
        /// <param name="attributes">该属性节点当前已附加的特性列表，处理器可向其中添加新特性</param>
        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        {
            attributes.Add(new InfoBoxAttribute(PrePlacedUsageWarning, InfoMessageType.Warning));
        }
    }
}
