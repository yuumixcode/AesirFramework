using System;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVP 示例 —— 计数器模型实现。
    /// </summary>
    /// <remarks>
    /// 与 MVC 版本（<see cref="SampleMvcCounterModel"/>）的实现逻辑完全一致，
    /// 说明 Model 层在 MVC 和 MVP 模式下是通用的，架构模式的差异不影响数据层设计。
    /// <para><c>[Serializable]</c> 标记使其可在 Unity Inspector 中序列化显示。</para>
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
        public ObservableValue<int> Count { get; set; } = new ObservableValue<int>(0);

        /// <summary>
        /// 计数 +1，通过 <see cref="Count"/> 的 setter 自动发布变更事件。
        /// </summary>
        public void Increase()
        {
            Count.Value++;
        }

        /// <summary>
        /// 计数 -1，通过 <see cref="Count"/> 的 setter 自动发布变更事件。
        /// </summary>
        public void Decrease()
        {
            Count.Value--;
        }

        /// <summary>
        /// 将计数重置为 0，通过 <see cref="Count"/> 的 setter 自动发布变更事件。
        /// </summary>
        public void Reset()
        {
            Count.Value = 0;
        }

        /// <summary>
        /// Model 初始化回调，在注册到 Context 时由框架调用。
        /// </summary>
        protected override void OnInitialize() { }
    }
}
