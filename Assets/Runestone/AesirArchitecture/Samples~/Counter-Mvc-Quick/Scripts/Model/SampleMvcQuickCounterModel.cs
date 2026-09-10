#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
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
        /// <c>[SerializeField]</c> 私有字段形式可被 Unity 原生与 Odin 序列化显示
        /// （序列化名 <c>count</c> 不变，既有场景 / 预制体数据兼容）。
        /// </remarks>
        [SerializeField]
        ObservableValue<int> count = new ObservableValue<int>(0);

        /// <summary>
        /// 当前计数值的对外暴露（快捷档可写——可直接改 <c>Value</c>，不可替换整个实例）。
        /// </summary>
        /// <remarks>
        /// 以只读属性暴露：表现层仍可直改 <c>Count.Value++</c>（快捷档零封装语义不变），
        /// 但无法整体替换 ObservableValue 实例导致既有订阅悬空——修复公开可变字段的封装倒退。
        /// </remarks>
        public ObservableValue<int> Count => count;
    }
}
#endif
