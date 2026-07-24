using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// Service 基类。继承 <see cref="AbstractSubmodule" /> 获得生命周期管理，实现 <see cref="IService" /> 标记服务层角色。
    /// </summary>
    [Serializable]
    public abstract class AbstractService : AbstractSubmodule, IService { }
}
