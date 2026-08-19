using System;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVC-1 快捷档示例 —— 计数器模型实现。
    /// </summary>
    /// <seealso cref="ISampleMvcQuickCounterModel"/>
    [Serializable]
    public sealed class SampleMvcQuickCounterModel : AbstractModel, ISampleMvcQuickCounterModel
    {
        /// <summary>
        /// 当前计数值，初始化为 0。
        /// </summary>
        public ObservableValue<int> Count { get; set; } = new ObservableValue<int>(0);

        /// <summary>
        /// Model 初始化回调，在注册到 Context 时由框架调用。
        /// </summary>
        protected override void OnInitialize() { }
    }
}
