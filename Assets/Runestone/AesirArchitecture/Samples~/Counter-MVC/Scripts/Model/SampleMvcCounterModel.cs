using System;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVC 示例 —— 计数器模型实现。
    /// </summary>
    /// <remarks>
    /// 使用 <see cref="ObservableValue{T}"/> 持有计数值，
    /// 所有修改操作（Increase / Decrease / Reset）只需更新 <c>Count.Value</c>，
    /// 变更通知由 ObservableValue 自动完成，Model 无需手动管理事件发布。
    /// <para><c>[Serializable]</c> 标记使其可在 Unity Inspector 中序列化显示。</para>
    /// </remarks>
    /// <seealso cref="ISampleMvcCounterModel"/>
    /// <seealso cref="Runestone.AesirArchitecture.AbstractModel"/>
    /// <seealso cref="Runestone.AesirArchitecture.ObservableValue{T}"/>
    [Serializable]
    public sealed class SampleMvcCounterModel : AbstractModel, ISampleMvcCounterModel
    {
        /// <summary>
        /// 当前计数值，初始化为 0。
        /// </summary>
        /// <remarks>
        /// 每次赋值 <c>Count.Value</c> 时，ObservableValue 会比较新旧值，
        /// 仅在值确实变化时才触发监听回调，避免无效刷新。
        /// </remarks>
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
        /// <remarks>
        /// 本示例无需额外初始化逻辑，保持空实现。
        /// 生产项目中可在此处做资源预加载、初始数据填充等操作。
        /// </remarks>
        protected override void OnInitialize() { }
    }
}
