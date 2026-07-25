namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// 计数器模型接口（MVP 版）
    /// </summary>
    public interface ISampleMvpCounterModel : IModel
    {
        /// <summary>
        /// 当前计数值（可观察属性）
        /// </summary>
        ObservableValue<int> Count { get; }

        /// <summary>
        /// 计数+1
        /// </summary>
        void Increase();

        /// <summary>
        /// 计数-1
        /// </summary>
        void Decrease();

        /// <summary>
        /// 重置为0
        /// </summary>
        void Reset();
    }
}
