using System;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// 计数器模型实现（MVP 版）
    /// </summary>
    [Serializable]
    public sealed class SampleMvpCounterModel : AbstractModel, ISampleMvpCounterModel
    {
        public ObservableValue<int> Count { get; set; } = new ObservableValue<int>(0);

        public void Increase()
        {
            Count.Value++;
        }

        public void Decrease()
        {
            Count.Value--;
        }

        public void Reset()
        {
            Count.Value = 0;
        }

        protected override void OnInitialize() { }
    }
}
