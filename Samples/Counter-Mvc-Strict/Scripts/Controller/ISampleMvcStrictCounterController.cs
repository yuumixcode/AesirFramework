namespace Runestone.AesirArchitecture.Samples.MvcStrict
{
    /// <summary>
    /// MVC-3 严格档示例 —— 计数器控制器接口。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     <b>严格档暴露面</b>：View 按此接口存储 Controller 实例（与 Model 的接口存储对称），
    ///     经接口只可获得业务操作入口（增 / 减 / 重置 / 加工值查询），不感知具体实现。
    ///     </para>
    ///     <para>
    ///     <b>
    ///     不继承 <see cref="Runestone.AesirArchitecture.IController" /> /
    ///     <see cref="Runestone.AesirArchitecture.IController{T}" />
    ///     </b>
    ///     ：
    ///     使 View 在类型层面拿不到 ExecuteCommand、GetModel 等框架能力，
    ///     "View 不可执行 Command"的读写分离由类型系统闭环保证。
    ///     </para>
    ///     <para>
    ///     对照：Model 接口（<see cref="ISampleMvcStrictCounterModel" />）继承
    ///     <see cref="Runestone.AesirArchitecture.IModel" /> 是注册需要（Context 按接口类型注册 Model）；
    ///     Controller 不注册进 Context，故无需继承框架角色接口。
    ///     </para>
    /// </remarks>
    /// <seealso cref="SampleMvcStrictCounterController" />
    /// <seealso cref="SampleMvcStrictCounterMainPanel" />
    public interface ISampleMvcStrictCounterController
    {
        /// <summary>
        /// 计数 +1。
        /// </summary>
        void Increase();

        /// <summary>
        /// 计数 -1。
        /// </summary>
        void Decrease();

        /// <summary>
        /// 将计数重置为 0。
        /// </summary>
        void ResetCounter();

        /// <summary>
        /// 查询十位四舍五入后的近似值（原始值不变）。
        /// </summary>
        int GetRoundedCount();
    }
}
