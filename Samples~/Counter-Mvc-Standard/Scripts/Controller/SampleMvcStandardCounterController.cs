namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVC-2 标准档示例 —— 计数器控制器（纯 C# 类，不经 Context）。
    /// </summary>
    /// <remarks>
    /// 标准档：View 与 Controller 拆为两个实例，但<b>共享同一个 Model 实例</b>——
    /// Controller 持有的 Model 由 View 在 Start 中构造注入（即 View 缓存的那个具体类实例），
    /// 写入直接调用 Model 写方法，不经 Command、不接 Context。
    /// <para>对照：严格档（Counter-Mvc-Strict）Controller 经 Context 发布 Command，
    /// 不持有 Model、不依赖 View 注入。</para>
    /// <para>数据流：View（用户输入）→ Controller → Model 写方法 → ObservableValue 通知 → View 刷新。</para>
    /// </remarks>
    /// <seealso cref="SampleMvcStandardCounterModel"/>
    /// <seealso cref="SampleMvcStandardCounterMainPanel"/>
    public sealed class SampleMvcStandardCounterController
    {
        readonly SampleMvcStandardCounterModel _model;

        /// <summary>
        /// 创建控制器并接收 View 共享的 Model 实例（具体类注入）。
        /// </summary>
        /// <param name="model">View 持有的计数器 Model（同一实例）。</param>
        public SampleMvcStandardCounterController(SampleMvcStandardCounterModel model)
        {
            _model = model;
        }

        /// <summary>
        /// 计数 +1（直调 Model 写方法）。
        /// </summary>
        public void Increase()
        {
            _model.Increase();
            AesirArchitectureDebug.Log("Standard Increase Counter");
        }

        /// <summary>
        /// 计数 -1（直调 Model 写方法）。
        /// </summary>
        public void Decrease()
        {
            _model.Decrease();
            AesirArchitectureDebug.Log("Standard Decrease Counter");
        }

        /// <summary>
        /// 将计数重置为 0（直调 Model 写方法）。
        /// </summary>
        public void ResetCounter()
        {
            _model.Reset();
            AesirArchitectureDebug.Log("Standard Reset Counter");
        }
    }
}
