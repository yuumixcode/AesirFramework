using System;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVP-1 快捷档示例 —— 计数器模型实现。
    /// </summary>
    /// <remarks>
    /// 快捷档：不定义 Model 接口、不封装修改方法——可写 ObservableValue 直接对外开放，
    /// Presenter 直接改值（<c>count.Value++</c>），与 MVC-1（Counter-Mvc-Quick）的 Model 完全一致。
    /// <para>对照：标准档（Counter-Mvp-Standard）收窄为只读暴露 + 写方法；
    /// 严格档（Counter-Mvp-Strict）再加接口注册 + Command 写入。</para>
    /// </remarks>
    /// <seealso cref="SampleMvpQuickCounterPresenter"/>
    [Serializable]
    public sealed class SampleMvpQuickCounterModel : AbstractModel
    {
        /// <summary>
        /// 当前计数值（快捷档可写暴露），初始化为 0。
        /// </summary>
        /// <remarks>
        /// <c>[SerializeField]</c> 字段形式可被 Unity 原生与 Odin 序列化显示。
        /// </remarks>
        [UnityEngine.SerializeField]
        public ObservableValue<int> count = new ObservableValue<int>(0);
    }
}
