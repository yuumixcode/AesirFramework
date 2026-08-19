using System;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVP 示例 —— 计数器模型实现。
    /// </summary>
    /// <remarks>
    /// 与 MVC 版本（<see cref="SampleMvcCounterModel"/>）的实现逻辑一致，
    /// 说明 Model 层在 MVC 和 MVP 模式下是通用的，架构模式的差异不影响数据层设计。
    /// <para><b>通常档暴露面</b>：直接暴露可写 <see cref="ObservableValue{T}"/>；
    /// 严格档收窄为只读接口 + 写方法，见 Counter-Mvp-Strict 示例。</para>
    /// <para><b>序列化口径</b>：<c>[Serializable]</c> + auto-property 形式
    /// 在 Unity 原生 Inspector 中不可见；安装 Odin Inspector 后可正常显示，属展示加成，不影响运行。</para>
    /// </remarks>
    /// <seealso cref="ISampleMvpCounterModel"/>
    /// <seealso cref="Runestone.AesirArchitecture.AbstractModel"/>
    /// <seealso cref="Runestone.AesirArchitecture.ObservableValue{T}"/>
    /// <seealso cref="SampleMvcCounterModel"/>
    [Serializable]
    public sealed class SampleMvpCounterModel : AbstractModel, ISampleMvpCounterModel
    {
        /// <summary>
        /// 当前计数值，初始化为 0。
        /// </summary>
        /// <remarks>
        /// <c>[SerializeField]</c> 字段形式可被 Unity 原生与 Odin 序列化显示
        ///（区别于旧版 auto-property——后者不被序列化，Context Debugger 无法观察）。
        /// </remarks>
        [UnityEngine.SerializeField]
        ObservableValue<int> _count = new ObservableValue<int>(0);

        /// <summary>
        /// 当前计数值（通常档可写暴露）。
        /// </summary>
        public ObservableValue<int> Count => _count;

        /// <summary>
        /// 计数 +1，通过 <see cref="Count"/> 的 setter 自动发布变更事件。
        /// </summary>
        public void Increase()
        {
            _count.Value++;
        }

        /// <summary>
        /// 计数 -1，通过 <see cref="Count"/> 的 setter 自动发布变更事件。
        /// </summary>
        public void Decrease()
        {
            _count.Value--;
        }

        /// <summary>
        /// 将计数重置为 0，通过 <see cref="Count"/> 的 setter 自动发布变更事件。
        /// </summary>
        public void Reset()
        {
            _count.Value = 0;
        }

        /// <summary>
        /// Model 初始化回调，在注册到 Context 时由框架调用。
        /// </summary>
        protected override void OnInitialize() { }
    }
}
