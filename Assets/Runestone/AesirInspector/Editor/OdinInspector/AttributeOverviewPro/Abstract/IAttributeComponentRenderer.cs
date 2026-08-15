namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// 特性面板组件渲染器接口，遵循 SRP 原则。
    /// </summary>
    public interface IAttributeComponentRenderer
    {
        /// <summary>
        /// 是否可见（例如数据为空时不显示）。
        /// </summary>
        bool IsVisible { get; }

        /// <summary>
        /// 执行绘制逻辑。
        /// </summary>
        void Draw();

        /// <summary>
        /// 当语言发生变化时调用。
        /// </summary>
        void OnLanguageChanged();

        /// <summary>
        /// 重置组件状态。
        /// </summary>
        void Reset();
    }
}
