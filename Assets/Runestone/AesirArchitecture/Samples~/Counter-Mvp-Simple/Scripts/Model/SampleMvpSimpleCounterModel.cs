using System;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVP-1 简单档示例 —— 计数器模型实现。
    /// </summary>
    /// <seealso cref="ISampleMvpSimpleCounterModel"/>
    [Serializable]
    public sealed class SampleMvpSimpleCounterModel : AbstractModel, ISampleMvpSimpleCounterModel
    {
        /// <summary>
        /// 当前计数值，初始化为 0。
        /// </summary>
        public ObservableValue<int> Count { get; set; } = new ObservableValue<int>(0);

        /// <summary>
        /// 计数 +1。
        /// </summary>
        public void Increase() => Count.Value++;

        /// <summary>
        /// 计数 -1。
        /// </summary>
        public void Decrease() => Count.Value--;

        /// <summary>
        /// 将计数重置为 0。
        /// </summary>
        public void Reset() => Count.Value = 0;

        /// <summary>
        /// Model 初始化回调，在注册到 Context 时由框架调用。
        /// </summary>
        protected override void OnInitialize() { }
    }
}
