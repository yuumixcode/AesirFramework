namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 数据层接口。持有状态（通常使用 <see cref="ObservableValue{T}" />）。
    /// <para>
    /// 能力：GetModel, Invoke
    /// </para>
    /// </summary>
    public interface IModel : IContextHolder, ICanSetContext, ICanGetModel, ICanInvokeEvent,
        ICanInitialize { }
}
