using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// Model 基类。继承 <see cref="AbstractSubmodule" /> 获得生命周期管理，实现 <see cref="IModel" /> 标记数据层角色。
    /// </summary>
    [Serializable]
    public abstract class AbstractModel : AbstractSubmodule, IModel { }
}
