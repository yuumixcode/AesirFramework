using System;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// 计数器模型实现
    /// </summary>
    [Serializable]
    public sealed class SampleMvcCounterModel : AbstractModel, ISampleMvcCounterModel
    {
        public ObservableValue<int> Count { get; set; } = new ObservableValue<int>(0);

        /// <summary>
        /// 计数+1并发布变更事件
        /// </summary>
        public void Increase()
        {
            Count.Value++;
        }

        /// <summary>
        /// 计数-1并发布变更事件
        /// </summary>
        public void Decrease()
        {
            Count.Value--;
        }

        /// <summary>
        /// 重置为0并发布变更事件
        /// </summary>
        public void Reset()
        {
            Count.Value = 0;
        }

        /// <summary>
        /// 初始化逻辑
        /// </summary>
        protected override void OnInitialize() { }
    }
}
