#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
using System;

namespace Runestone.AesirModules.Samples.Events.Cancellation
{
    /// <summary>
    /// 伤害事件参数：携带基础伤害与最终结算伤害，供取消传播链上的各档订阅者读写。
    /// </summary>
    [Serializable]
    public sealed class DamageEventArgs : AesirEventArgs
    {
        /// <summary>基础伤害值。</summary>
        public int baseDamage;

        /// <summary>最终结算伤害（由 Medium 档写入；被取消时保持初始值）。</summary>
        public int finalDamage;
    }
}
#endif
