using UnityEngine;

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
        [InspectorName("背景层 Background")]
        Background = 0,

        /// <summary>
        /// 常规层，SortingOrder 基准 200。
        /// </summary>
        [InspectorName("常规层 Normal")]
        Normal = 1,

        /// <summary>
        /// 弹窗层，SortingOrder 基准 300。
        /// </summary>
        [InspectorName("弹窗层 Popup")]
        Popup = 2,

        /// <summary>
        /// 顶层（Toast/系统提示），SortingOrder 基准 400。
        /// </summary>
        [InspectorName("顶层 Top")]
        Top = 3
    }
}
