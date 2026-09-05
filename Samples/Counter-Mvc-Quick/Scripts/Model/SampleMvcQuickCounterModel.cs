using System;
using UnityEngine;

namespace Runestone.AesirArchitecture.Samples.MvcQuick
{
    /// <summary>
    /// MVC-1 快捷档示例 —— 计数器模型实现。
    /// </summary>
    /// <remarks>
    /// 快捷档：不定义 Model 接口、不封装修改方法——可写 ObservableValue 直接对外开放，
    /// 表现层（View 兼 Controller）直接改值（<c>value++</c>）。
    /// <para>
    /// 对照：标准档（Counter-Mvc-Standard）收窄为只读接口 + 写方法；
    /// 严格档（Counter-Mvc-Strict）再加接口注册 + Command 写入。
    /// </para>
    /// </remarks>
    /// <seealso cref="SampleMvcQuickCounterMainPanel" />
    [Serializable]
    public sealed class SampleMvcQuickCounterModel : AbstractModel
    {
        /// <summary>
        /// 当前计数值（快捷档可写暴露），初始化为 0。
        /// </summary>
        /// <remarks>
        /// <c>[SerializeField]</c> 字段形式可被 Unity 原生与 Odin 序列化显示。
        /// </remarks>
        [SerializeField]
        public ObservableValue<int> count = new ObservableValue<int>(0);
    }
}
