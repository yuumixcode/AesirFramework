using System;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVP-3 严格档示例 —— 计数器 Presenter 接口。
    /// </summary>
    /// <remarks>
    /// <para><b>严格档暴露面</b>：View 按此接口存储 Presenter 实例，
    /// 经接口只可获得生命周期入口（同步初始值 / 释放订阅），不感知具体实现。</para>
    /// <para><b>不继承 <see cref="Runestone.AesirArchitecture.IPresenter"/> / <see cref="Runestone.AesirArchitecture.IPresenter{T}"/></b>：
    /// 使 View 在类型层面拿不到 ExecuteCommand、GetModel 等框架能力，
    /// "View 不可执行 Command"的 MVP 边界由类型系统闭环保证——
    /// 与 MVC-3（Counter-Mvc-Strict）的 Controller 业务接口同构，差异是 MVP 的业务经 View 事件流向 Presenter、
    /// View 无需主动调用业务方法，故接口只含生命周期方法。</para>
    /// <para><b>继承 <see cref="IDisposable"/></b>：View 在 OnDestroy 时释放 Presenter 的事件订阅
    ///（MVP 特有：Presenter 订阅了 View 事件，需显式注销）。</para>
    /// </remarks>
    /// <seealso cref="SampleMvpStrictCounterPresenter"/>
    /// <seealso cref="SampleMvpStrictCounterMainPanel"/>
    public interface ISampleMvpStrictCounterPresenter : IDisposable
    {
        /// <summary>
        /// 同步初始值到 View，由 View 在 Start 中调用。
        /// </summary>
        void SyncInitialValue();
    }
}
