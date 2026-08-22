using System;
using UnityEngine;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVP-2 标准档示例 —— 计数器模型实现。
    /// </summary>
    /// <remarks>
    /// 标准档：内部私有可序列化字段 + 对外只读接口暴露 + 公开写方法——
    /// 外部只能订阅与读取（<see cref="Count" />），修改必经写方法；
    /// 但仍不做接口抽象（Context 按具体类注册），写入也不经 Command（由 Presenter 直接调写方法）。
    /// 与 MVC-2（Counter-Mvc-Standard）的 Model 完全一致。
    /// <para>
    /// 对照：快捷档（Counter-Mvp-Quick）可写 ObservableValue 直接对外开放；
    /// 严格档（Counter-Mvp-Strict）再加接口注册 + Command 写入。
    /// </para>
    /// </remarks>
    /// <seealso cref="SampleMvpStandardCounterPresenter" />
    [Serializable]
    public sealed class SampleMvpStandardCounterModel : AbstractModel
    {
        [SerializeField]
        ObservableValue<int> count = new ObservableValue<int>(0);

        /// <summary>
        /// 当前计数值（只读），外部只可订阅与读取。
        /// </summary>
        public IReadOnlyObservableValue<int> Count => count;

        /// <summary>
        /// 计数 +1（写方法）。
        /// </summary>
        public void Increase() => count.Value++;

        /// <summary>
        /// 计数 -1（写方法）。
        /// </summary>
        public void Decrease() => count.Value--;

        /// <summary>
        /// 将计数重置为 0（写方法）。
        /// </summary>
        public void Reset() => count.Value = 0;
    }
}
