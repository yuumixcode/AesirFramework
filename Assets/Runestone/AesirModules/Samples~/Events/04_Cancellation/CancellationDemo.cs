#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
using Runestone.AesirArchitecture;
using UnityEngine;

namespace Runestone.AesirModules.Samples.Events.Cancellation
{
    /// <summary>
    /// 取消传播示例：一个伤害事件被四档优先级订阅，Space 发布后从 HUD 观察实际执行链。
    /// <para>
    /// 四档订阅者按顺序：
    /// First（前置校验）→ High（伤害减免，调用 <c>Cancel</c> 取消传播）→
    /// Medium（伤害结算，被取消后跳过）→ Last（收尾日志，不受取消影响）。
    /// 再按 Space 且未取消时（按住 Shift 发布不可取消事件），可对照看到 Medium 也执行。
    /// </para>
    /// </summary>
    [AddComponentMenu("")]
    public class CancellationDemo : MonoBehaviour
    {
        readonly AutoRemoveListenerHandle[] _handles = new AutoRemoveListenerHandle[4];

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                var args = new DamageEventArgs { baseDamage = 100, finalDamage = -1 };
                // 按住 Shift 发布不可取消事件，用于对照：四档全部执行
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    args.NonCancellable();
                }

                args.Invoke(this);
            }
        }

        void OnEnable()
        {
            // First：前置校验，始终执行
            _handles[0] =
                EventModule.AddListener<DamageEventArgs>(this, OnValidateDamage, SubscriberPriority.First);
            // High：伤害减免，调用 Cancel 取消传播
            _handles[1] = EventModule.AddListener<DamageEventArgs>(this, OnMitigate, SubscriberPriority.High);
            // Medium：伤害结算，取消后将被跳过
            _handles[2] =
                EventModule.AddListener<DamageEventArgs>(this, OnApplyDamage, SubscriberPriority.Medium);
            // Last：收尾日志，始终执行
            _handles[3] = EventModule.AddListener<DamageEventArgs>(this, OnCleanup, SubscriberPriority.Last);
        }

        void OnDisable()
        {
            foreach (var handle in _handles)
            {
                handle.Dispose();
            }
        }

        void OnValidateDamage(DamageEventArgs e)
        {
            CancellationHud.Report("First", $"前置校验：基础伤害 {e.baseDamage}");
        }

        void OnMitigate(DamageEventArgs e)
        {
            if (e.IsNonCancellable)
            {
                CancellationHud.Report("High", "伤害减免：事件不可取消，跳过 Cancel");
                return;
            }

            CancellationHud.Report("High", "伤害减免：调用 Cancel 取消传播");
            e.Cancel(this);
        }

        void OnApplyDamage(DamageEventArgs e)
        {
            e.finalDamage = Mathf.Max(0, e.baseDamage - 30);
            CancellationHud.Report("Medium", $"伤害结算：最终伤害 {e.finalDamage}");
        }

        void OnCleanup(DamageEventArgs e)
        {
            var applied = e.finalDamage >= 0 ? $"{e.finalDamage}" : "未结算（被取消）";
            CancellationHud.Report("Last", $"收尾：最终伤害 = {applied}，已取消 = {e.IsCancelled}");
        }
    }
}
#endif
