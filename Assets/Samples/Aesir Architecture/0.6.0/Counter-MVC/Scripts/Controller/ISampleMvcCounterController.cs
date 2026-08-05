namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// 示例计数器控制器接口，定义了 UI 层可调用的操作方法
    /// </summary>
    public interface ISampleMvcCounterController : IController<SampleMvcCounterContext>
    {
        /// <summary>
        /// 增加计数值
        /// </summary>
        void Increase();

        /// <summary>
        /// 减少计数值
        /// </summary>
        void Decrease();

        /// <summary>
        /// 重置计数值
        /// </summary>
        void ResetCounter();
    }
}
