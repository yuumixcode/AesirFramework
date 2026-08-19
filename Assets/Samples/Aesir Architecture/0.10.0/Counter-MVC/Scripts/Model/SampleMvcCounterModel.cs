using System;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVC 示例 —— 计数器模型实现。
    /// </summary>
    /// <remarks>
    /// 使用 <see cref="ObservableValue{T}"/> 持有计数值，
    /// 所有修改操作（Increase / Decrease / Reset）只需更新 <c>Count.Value</c>，
    /// 变更通知由 ObservableValue 自动完成，Model 无需手动管理事件发布。
    /// <para><b>通常档暴露面</b>：Model 直接暴露可写 <see cref="ObservableValue{T}"/>，
    /// 外部（快捷档由表现层直写，标准档由 Command 内部直写）可直接改值；
    /// 严格档收窄为只读接口 + 写方法，见 Counter-Mvc-Strict 示例。</para>
    /// <para><b>序列化口径</b>：<c>[Serializable]</c> + auto-property 形式
    /// 在 Unity 原生 Inspector 中不可见（Unity 不序列化 auto-property）；
    /// 安装 Odin Inspector 后可正常序列化显示，属 Inspector 展示加成，不影响运行。</para>
    /// </remarks>
    /// <seealso cref="ISampleMvcCounterModel"/>
    /// <seealso cref="Runestone.AesirArchitecture.AbstractModel"/>
    /// <seealso cref="Runestone.AesirArchitecture.ObservableValue{T}"/>
    [Serializable]
    public sealed class SampleMvcCounterModel : AbstractModel, ISampleMvcCounterModel
    {
        /// <summary>
        /// 当前计数值，初始化为 0。
        /// </summary>
        /// <remarks>
        /// 每次赋值 <c>Count.Value</c> 时，ObservableValue 会比较新旧值，
        /// 仅在值确实变化时才触发监听回调，避免无效刷新。
        /// <para><c>[SerializeField]</c> 字段形式可被 Unity 原生与 Odin 序列化显示
        ///（区别于旧版 auto-property——后者不被序列化，Context Debugger 无法观察）。</para>
        /// </remarks>
        [UnityEngine.SerializeField]
        ObservableValue<int> _count = new ObservableValue<int>(0);

        /// <summary>
        /// 当前计数值（通常档可写暴露）。
        /// </summary>
        public ObservableValue<int> Count => _count;

        /// <summary>
        /// 计数 +1，通过 <see cref="Count"/> 的 setter 自动发布变更事件。
        /// </summary>
        public void Increase()
        {
            _count.Value++;
        }

        /// <summary>
        /// 计数 -1，通过 <see cref="Count"/> 的 setter 自动发布变更事件。
        /// </summary>
        public void Decrease()
        {
            _count.Value--;
        }

        /// <summary>
        /// 将计数重置为 0，通过 <see cref="Count"/> 的 setter 自动发布变更事件。
        /// </summary>
        public void Reset()
        {
            _count.Value = 0;
        }

        /// <summary>
        /// Model 初始化回调，在注册到 Context 时由框架调用。
        /// </summary>
        /// <remarks>
        /// 本示例无需额外初始化逻辑，保持空实现。
        /// 生产项目中可在此处做资源预加载、初始数据填充等操作。
        /// </remarks>
        protected override void OnInitialize() { }
    }
}
