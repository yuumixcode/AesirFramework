using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace Runestone.AesirArchitecture.Editor.OdinIntegration
{
    /// <summary>
    /// 为 AesirArchitecture 类动态添加特性。
    /// </summary>
    public class AesirArchitectureAttributeProcessor : OdinAttributeProcessor<AesirArchitecture>
    {
        /// <summary>
        /// 场景预放置实例使用须知文案。
        /// </summary>
        /// <remarks>
        /// 预放置实例不调用 <c>DontDestroyOnLoad</c>、随所在场景销毁，仅推荐在多场景叠加加载方案中使用；
        /// 重复实例自毁时会销毁整个宿主 GameObject；宿主销毁会连带销毁其上的全局服务组件并清空生命周期订阅。
        /// </remarks>
        const string PrePlacedUsageWarning = "当前实例是场景预放置实例，仅推荐在多场景叠加加载（Additive）方案中使用；需要手动控制其生命周期";

        /// <summary>
        /// 处理类自身的特性，添加描述信息框
        /// </summary>
        /// <param name="property">Odin Inspector 正在构建的属性节点，代表被处理的目标对象</param>
        /// <param name="attributes">该属性节点当前已附加的特性列表，处理器可向其中添加新特性</param>
        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        {
            attributes.Add(new InfoBoxAttribute("Aesir Architecture 接入 MonoBehaviour 生命周期的全局持久化物体对象"));
            attributes.Add(new InfoBoxAttribute(PrePlacedUsageWarning, InfoMessageType.Warning,
                AesirArchitecture.IsPrePlacedFieldName));
        }
    }
}
