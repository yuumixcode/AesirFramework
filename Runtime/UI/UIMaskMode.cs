using UnityEngine;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 窗口蒙版调度模式，配置于 <see cref="UIModule" />，运行时可经 <see cref="UIModule.MaskMode" /> 切换。
    /// </summary>
    public enum UIMaskMode
    {
        /// <summary>
        /// 单遮模式：全局仅最高层可见窗口的蒙版生效，多窗口叠加时透明度不叠加。
        /// </summary>
        [InspectorName("单遮模式")]
        Single = 0,

        /// <summary>
        /// 叠遮模式：每个窗口的蒙版独立生效，多窗口叠加时透明度逐层叠加。
        /// </summary>
        [InspectorName("叠遮模式")]
        Stacked = 1,
    }
}
