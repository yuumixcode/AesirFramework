using UnityEngine;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVC-3 严格档示例 —— 计数器模型实现。
    /// </summary>
    /// <remarks>
    /// 严格档：私有可写字段 + 只读属性转发，写方法内部修改值。
    /// <c>[SerializeField]</c> 字段形式可被 Unity 原生序列化显示（区别于通常档的 auto-property）。
    /// </remarks>
    /// <seealso cref="ISampleMvcStrictCounterModel"/>
    [System.Serializable]
    public sealed class SampleMvcStrictCounterModel : AbstractModel, ISampleMvcStrictCounterModel
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

        /// <summary>
        /// Model 初始化回调，在注册到 Context 时由框架调用。
        /// </summary>
        protected override void OnInitialize() { }
    }
}
