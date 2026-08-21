using UnityEngine;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVP-3 严格档示例 —— 计数器模型实现。
    /// </summary>
    /// <remarks>
    /// 严格档：私有可序列化字段 + 只读属性转发，写方法内部修改值；
    /// Context 按<b>接口</b>注册本实例（与标准档的具体类注册形成对照）。
    /// 与 MVC-3（Counter-Mvc-Strict）的 Model 完全一致。
    /// <para>对照：快捷档（Counter-Mvp-Quick）可写 ObservableValue 直接对外开放；
    /// 标准档（Counter-Mvp-Standard）只读暴露 + 写方法、具体类注册。</para>
    /// </remarks>
    /// <seealso cref="ISampleMvpStrictCounterModel"/>
    [System.Serializable]
    public sealed class SampleMvpStrictCounterModel : AbstractModel, ISampleMvpStrictCounterModel
    {
        [SerializeField]
        ObservableValue<int> count = new ObservableValue<int>(0);

        /// <summary>
        /// 当前计数值（只读），外部只可订阅与读取。
        /// </summary>
        public IReadOnlyObservableValue<int> Count => count;

        /// <summary>
        /// 计数 +1（写方法，Command 调用）。
        /// </summary>
        public void Increase() => count.Value++;

        /// <summary>
        /// 计数 -1（写方法，Command 调用）。
        /// </summary>
        public void Decrease() => count.Value--;

        /// <summary>
        /// 将计数重置为 0（写方法，Command 调用）。
        /// </summary>
        public void Reset() => count.Value = 0;
    }
}
