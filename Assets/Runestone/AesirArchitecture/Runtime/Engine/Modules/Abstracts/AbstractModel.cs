using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// Model 基类。继承 <see cref="AbstractSubmodule" /> 获得生命周期管理，实现 <see cref="IModel" /> 标记数据层角色。
    /// </summary>
    /// <remarks>
    /// Model 是数据层，持有状态（通常使用 <see cref="ObservableValue{T}" />）。
    /// 状态仅通过 Command 写入，View 不直接调用 Model 的写入方法；
    /// Model 通过 <c>IReadOnlyObservableValue&lt;T&gt;</c> 向 View 暴露只读订阅，
    /// 确保数据流向单向可控——View 只能观察变化，不能回写。
    /// </remarks>
    /// <seealso cref="AbstractSubmodule"/>
    /// <seealso cref="IModel"/>
    /// <seealso cref="ObservableValue{T}"/>
    [Serializable]
    public abstract class AbstractModel : AbstractSubmodule, IModel { }
}
