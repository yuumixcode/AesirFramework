using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace Runestone.AesirArchitecture.Editor.OdinIntegration
{
    /// <summary>
    /// 为 AesirArchitecture 类提供 Odin Inspector 属性处理器
    /// </summary>
    /// <remarks>
    /// 利用 Odin AttributeProcessor 机制，在 Odin 构建 Inspector 属性树时自动为目标类添加特性，
    /// 无需修改目标类本身的代码即可增强其 Inspector 展示效果。
    /// 此处理器为 <see cref="AesirArchitecture"/> 添加 <see cref="InfoBoxAttribute"/>，
    /// 使开发者在 Inspector 中能直观了解该组件的用途。
    /// </remarks>
    public class AesirArchitectureAttributeProcessor : OdinAttributeProcessor<AesirArchitecture>
    {
        /// <summary>
        /// 处理类自身的特性，添加描述信息框
        /// </summary>
        /// <param name="property">Odin Inspector 正在构建的属性节点，代表被处理的目标对象</param>
        /// <param name="attributes">该属性节点当前已附加的特性列表，处理器可向其中添加新特性</param>
        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        {
            attributes.Add(new InfoBoxAttribute("Aesir Architecture 接入 MonoBehaviour 生命周期的全局持久化物体对象"));
        }
    }
}
