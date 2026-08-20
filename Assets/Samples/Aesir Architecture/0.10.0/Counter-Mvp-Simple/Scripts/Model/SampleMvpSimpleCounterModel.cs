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
        /// <remarks>
        /// <c>[SerializeField]</c> 字段形式可被 Unity 原生与 Odin 序列化显示
        ///（区别于旧版 auto-property——后者不被序列化，Context Debugger 无法观察）。
        /// </remarks>
        [UnityEngine.SerializeField]
        ObservableValue<int> _count = new ObservableValue<int>(0);

        /// <summary>
        /// 当前计数值（简单档可写暴露）。
        /// </summary>
        public ObservableValue<int> Count => _count;

        /// <summary>
        /// 计数 +1。
        /// </summary>
        public void Increase() => _count.Value++;

        /// <summary>
        /// 计数 -1。
        /// </summary>
        public void Decrease() => _count.Value--;

        /// <summary>
        /// 将计数重置为 0。
        /// </summary>
        public void Reset() => _count.Value = 0;
    }
}
