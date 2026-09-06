#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
namespace Runestone.AesirArchitecture.Samples.MvpStrict
{
    /// <summary>
    /// MVP-3 严格档示例 —— 增加计数命令。
    /// </summary>
    /// <seealso cref="Runestone.AesirArchitecture.AbstractCommand" />
    public class SampleMvpStrictIncreaseCommand : AbstractCommand
    {
        /// <summary>
        /// 执行增加计数逻辑：从 Context 获取 Model 并调用其写方法。
        /// </summary>
        protected override void OnExecute()
        {
            this.GetModel<ISampleMvpStrictCounterModel>().Increase();
            AesirArchitectureDebug.Log("Strict Mvp Increase Counter");
        }
    }
}
#endif
