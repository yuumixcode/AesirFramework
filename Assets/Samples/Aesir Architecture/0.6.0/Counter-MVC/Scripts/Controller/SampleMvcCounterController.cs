namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// 计数器 Demo 游戏控制器。MonoBehaviour 适配层，实现 IController 接口。
    /// <para>负责绑定 UI 按钮、订阅事件、驱动 Command。</para>
    /// </summary>
    public class SampleMvcCounterController : ISampleMvcCounterController
    {
        public void Increase()
        {
            this.ExecuteCommand<SampleMvcIncreaseCommand>();
        }

        public void Decrease()
        {
            this.ExecuteCommand<SampleMvcDecreaseCommand>();
        }

        public void ResetCounter()
        {
            this.ExecuteCommand<SampleMvcResetCommand>();
        }
    }
}
