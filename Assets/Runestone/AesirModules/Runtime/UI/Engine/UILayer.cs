namespace Runestone.AesirModules
{
    /// <summary>
    /// UI 层级。Background &lt; Normal &lt; Popup &lt; Top。
    /// </summary>
    public enum UILayer
    {
        /// <summary>
        /// 背景层，SortingOrder 基准 100。
        /// </summary>
        Background = 0,

        /// <summary>
        /// 常规层，SortingOrder 基准 200。
        /// </summary>
        Normal = 1,

        /// <summary>
        /// 弹窗层，SortingOrder 基准 300。
        /// </summary>
        Popup = 2,

        /// <summary>
        /// 顶层（Toast/系统提示），SortingOrder 基准 400。
        /// </summary>
        Top = 3
    }
}
