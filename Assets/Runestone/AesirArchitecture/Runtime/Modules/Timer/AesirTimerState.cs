namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 计时器的可观察状态，供编辑器调试窗口与检视面板分类展示。
    /// </summary>
    /// <remarks>
    /// 顺序即"关注优先级"：调试面板按此排序，异常态（<see cref="Uninitialized" />）排在最后。
    /// 运行时逻辑不依赖本枚举，结算只读写 <see cref="AesirTimer" /> 内部的布尔标志。
    /// </remarks>
    internal enum AesirTimerState
    {
        /// <summary>运行中：已入队且未被暂停。</summary>
        Running,

        /// <summary>已暂停：仍在注册表中，但结算跳过时间累减与执行。</summary>
        Paused,

        /// <summary>已正常到期（有限次执行已用尽）。</summary>
        Finished,

        /// <summary>已被取消（与 <see cref="Finished" /> 互斥）。</summary>
        Cancelled,

        /// <summary>
        /// 未初始化：允许执行次数为 0 的实例，或 <c>[SerializeReference]</c> 序列化后
        /// 被反序列化重建、从未真正创建过的残影（回调与时长均为默认值）。
        /// </summary>
        Uninitialized
    }
}
