namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVC-3 严格档示例 —— 计数器控制器（纯 C# 类，经 Context 发布 Command / 执行 Query）。
    /// </summary>
    /// <remarks>
    /// 严格档：Controller 实现泛型 <see cref="IController{T}"/>（Context 经默认接口实现自动绑定），
    /// 由 View 在 Start 中 new 出；写入全部经 <c>ExecuteCommand</c> 发布——
    /// Controller 不持有 Model、不依赖 View 注入，与 View 完全解耦。
    /// <para>双接口设计：业务接口 <see cref="ISampleMvcStrictCounterController"/> 是 View 侧的持有类型
    ///（只暴露业务操作）；框架角色接口 <c>IController&lt;T&gt;</c> 提供 Command / Query 能力——
    /// 两者各司其职，读写分离在类型层面闭环。</para>
    /// <para>读取口径：原始值 View 直接用只读订阅；加工值（十位近似）经 <c>ExecuteQuery</c> 查询——
    /// Query 只在需要加工修饰（四舍五入等）时使用，返回原始值则无需 Query。</para>
    /// <para>对照：标准档（Counter-Mvc-Standard）Controller 由 View 注入共享 Model、直调写方法。</para>
    /// <para>数据流：View（用户输入）→ Controller → Command → Model 写方法 → ObservableValue 通知 → View 刷新；
    /// View 需加工值时 → Controller → Query（只读加工）→ 返回。</para>
    /// </remarks>
    /// <seealso cref="ISampleMvcStrictCounterController"/>
    /// <seealso cref="IController{T}"/>
    /// <seealso cref="SampleMvcStrictCounterMainPanel"/>
    public sealed class SampleMvcStrictCounterController : ISampleMvcStrictCounterController,
        IController<SampleMvcStrictCounterContext>
    {
        /// <summary>
        /// 计数 +1（发布 Command）。
        /// </summary>
        public void Increase() => this.ExecuteCommand<SampleMvcStrictIncreaseCommand>();

        /// <summary>
        /// 计数 -1（发布 Command）。
        /// </summary>
        public void Decrease() => this.ExecuteCommand<SampleMvcStrictDecreaseCommand>();

        /// <summary>
        /// 将计数重置为 0（发布 Command）。
        /// </summary>
        public void ResetCounter() => this.ExecuteCommand<SampleMvcStrictResetCommand>();

        /// <summary>
        /// 查询十位四舍五入后的近似值（原始值不变）。
        /// </summary>
        public int GetRoundedCount() => this.ExecuteQuery<GetRoundedCountQuery, int>();
    }
}
