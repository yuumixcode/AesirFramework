#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
namespace Runestone.AesirArchitecture.Samples.MvcStrict
{
    /// <summary>
    /// MVC-3 严格档示例 —— 重置计数命令。
    /// </summary>
    /// <seealso cref="SampleMvcStrictIncreaseCommand" />
    public class SampleMvcStrictResetCommand : AbstractCommand
    {
        /// <summary>
        /// 执行重置计数逻辑：从 Context 获取 Model 并调用其写方法。
        /// </summary>
        protected override void OnExecute()
        {
            this.GetModel<ISampleMvcStrictCounterModel>().Reset();
            AesirArchitectureDebug.Log("Strict Reset Counter");
        }
    }
}
#endif
