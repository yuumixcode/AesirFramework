namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVC 示例 —— 计数器控制器实现。
    /// </summary>
    /// <remarks>
    /// Controller 不直接修改 Model，而是将每个用户操作分发到对应的 Command。
    /// 这样将"触发意图"与"执行变更"解耦，Command 可被独立测试、记录日志或扩展为撤销/重做。
    /// <para>数据流：View → Controller（此处）→ Command → Model。</para>
    /// </remarks>
    /// <seealso cref="ISampleMvcCounterController"/>
    /// <seealso cref="Runestone.AesirArchitecture.IController{T}"/>
    /// <seealso cref="SampleMvcIncreaseCommand"/>
    /// <seealso cref="SampleMvcDecreaseCommand"/>
    /// <seealso cref="SampleMvcResetCommand"/>
    public class SampleMvcCounterController : ISampleMvcCounterController
    {
        /// <summary>
        /// 分发"增加计数"意图到 <see cref="SampleMvcIncreaseCommand"/>。
        /// </summary>
        public void Increase()
        {
            this.ExecuteCommand<SampleMvcIncreaseCommand>();
        }

        /// <summary>
        /// 分发"减少计数"意图到 <see cref="SampleMvcDecreaseCommand"/>。
        /// </summary>
        public void Decrease()
        {
            this.ExecuteCommand<SampleMvcDecreaseCommand>();
        }

        /// <summary>
        /// 分发"重置计数"意图到 <see cref="SampleMvcResetCommand"/>。
        /// </summary>
        public void ResetCounter()
        {
            this.ExecuteCommand<SampleMvcResetCommand>();
        }
    }
}
