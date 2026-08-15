using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// Service 基类。继承 <see cref="AbstractSubmodule" /> 获得生命周期管理，实现 <see cref="IService" /> 标记服务层角色。
    /// </summary>
    /// <remarks>
    /// Service 是跨模块协调层，可读写 Model、调用其他 Service 以封装跨模块业务逻辑，
    /// 但不包含 Command / Query 执行能力——Command/Query 的执行入口应由
    /// Controller / Presenter 触发，避免 Service 成为逻辑黑洞。
    /// </remarks>
    /// <seealso cref="AbstractSubmodule"/>
    /// <seealso cref="IService"/>
    [Serializable]
    public abstract class AbstractService : AbstractSubmodule, IService { }
}
