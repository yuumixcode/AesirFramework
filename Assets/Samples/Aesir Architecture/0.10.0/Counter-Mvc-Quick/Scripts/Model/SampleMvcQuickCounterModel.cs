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
        /// <remarks>
        /// <c>[SerializeField]</c> 字段形式可被 Unity 原生与 Odin 序列化显示
        ///（区别于旧版 auto-property——后者不被序列化，Context Debugger 无法观察）。
        /// </remarks>
        [UnityEngine.SerializeField]
        ObservableValue<int> _count = new ObservableValue<int>(0);

        /// <summary>
        /// 当前计数值（快捷档可写暴露，表现层可直接改值）。
        /// </summary>
        public ObservableValue<int> Count => _count;

        /// <summary>
        /// Model 初始化回调，在注册到 Context 时由框架调用。
        /// </summary>
        protected override void OnInitialize() { }
    }
}
