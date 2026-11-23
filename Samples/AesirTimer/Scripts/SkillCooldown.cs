#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// 纯 C# 冷却模型：不继承 MonoBehaviour、不用协程，靠 <see cref="AesirTimer" /> 表达"还差几秒可用"。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     这正是 <see cref="AesirTimer" /> 的目标场景——Model / Service / Command 这类拿不到每帧回调的纯 C# 类，
    ///     也能拥有延时与冷却，而 UI 只需读 <see cref="Progress" /> 画进度条，不需要任何人驱动。
    ///     </para>
    ///     <para>
    ///     冷却中的实例可以被 <see cref="Pause" /> 挂起（例如过场动画期间冻结输入冷却），恢复后从冻结处继续。
    ///     </para>
    /// </remarks>
    public sealed class SkillCooldown
    {
        readonly float _duration;
        AesirTimer _timer;

        /// <summary>创建一个冷却位。</summary>
        /// <param name="duration">冷却时长（秒）</param>
        public SkillCooldown(float duration) => _duration = duration;

        /// <summary>此刻是否可用（从未施放 / 已到期 / 已取消）。</summary>
        public bool IsReady => _timer == null || _timer.IsFinished || _timer.IsCancelled;

        /// <summary>冷却进度 0 → 1，可直接喂进度条（就绪时为 1）。</summary>
        public float Progress => IsReady ? 1f : _timer.Progress;

        /// <summary>还需等待的秒数（就绪时为 0）。</summary>
        public float Remaining => IsReady ? 0f : _timer.Remaining;

        /// <summary>是否处于暂停（过场冻结）。</summary>
        public bool IsPaused => _timer != null && _timer.IsPaused;

        /// <summary>尝试施放：可用则起冷却并返回 true，冷却中返回 false。</summary>
        public bool TryCast()
        {
            if (!IsReady)
            {
                return false;
            }

            _timer = AesirTimer.Countdown(_duration);
            return true;
        }

        /// <summary>冻结冷却（只影响这一个冷却位，与 <c>Time.timeScale</c> 无关）。</summary>
        public void Pause() => _timer?.Pause();

        /// <summary>解除冻结，从冻结时刻继续。</summary>
        public void Resume() => _timer?.Resume();

        /// <summary>清空冷却位，回到就绪状态。</summary>
        public void Reset()
        {
            _timer?.Cancel();
            _timer = null;
        }
    }
}
#endif
