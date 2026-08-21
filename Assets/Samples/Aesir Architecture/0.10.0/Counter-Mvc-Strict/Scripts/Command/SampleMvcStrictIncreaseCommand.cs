namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVC-3 严格档示例 —— 增加计数命令。
    /// </summary>
    /// <remarks>
    /// 严格档：Command 经 Model 的写方法修改状态（不直接碰 ObservableValue 的值）——
    /// 与标准档（Counter-Mvc-Standard）Controller 直调写方法、
    /// 快捷档（Counter-Mvc-Quick）直改 ObservableValue 形成对照。
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.AbstractCommand" />
    public class SampleMvcStrictIncreaseCommand : AbstractCommand
    {
        /// <summary>
        /// 执行增加计数逻辑：从 Context 获取 Model 并调用其写方法。
        /// </summary>
        protected override void OnExecute()
        {
            this.GetModel<ISampleMvcStrictCounterModel>().Increase();
            AesirArchitectureDebug.Log("Strict Increase Counter");
        }
    }
}
