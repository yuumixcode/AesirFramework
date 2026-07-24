using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace Runestone.AesirArchitecture.Editor.OdinIntegration
{
    /// <summary>
    /// 为 AesirArchitecture 类提供 Odin Inspector 属性处理器
    /// </summary>
    public class AesirArchitectureAttributeProcessor : OdinAttributeProcessor<AesirArchitecture>
    {
        /// <summary>
        /// 处理类自身的特性，添加描述信息框
        /// </summary>
        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        {
            attributes.Add(new InfoBoxAttribute("Aesir Architecture 接入 MonoBehaviour 生命周期的全局持久化物体对象"));
        }
    }
}
