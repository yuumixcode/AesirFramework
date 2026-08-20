using UnityEngine;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVP-3 严格档示例 —— 计数器模型实现。
    /// </summary>
    /// <remarks>
    /// 严格档：私有可写字段 + 只读属性转发。<c>[SerializeField]</c> 字段形式可被 Unity 原生序列化显示。
    /// </remarks>
    /// <seealso cref="ISampleMvpStrictCounterModel"/>
    [System.Serializable]
    public sealed class SampleMvpStrictCounterModel : AbstractModel, ISampleMvpStrictCounterModel
    {
        [SerializeField]
        ObservableValue<int> _count = new ObservableValue<int>(0);

        /// <summary>
        /// 当前计数值（只读），外部只可订阅与读取。
        /// </summary>
        public IReadOnlyObservableValue<int> Count => _count;

        /// <summary>
        /// 计数 +1（写方法）。
        /// </summary>
        public void Increase() => _count.Value++;

        /// <summary>
        /// 计数 -1（写方法）。
        /// </summary>
        public void Decrease() => _count.Value--;

        /// <summary>
        /// 将计数重置为 0（写方法）。
        /// </summary>
        public void Reset() => _count.Value = 0;
    }
}
