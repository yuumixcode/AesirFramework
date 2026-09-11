#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
using UnityEngine;
using Runestone.AesirModules;

namespace Runestone.AesirModules.Samples.Events.Filters
{
    /// <summary>
    /// 警报发布者。按 Space 发布 <see cref="AlarmEvent" />，链式声明两个过滤器：
    /// 只有"Tag 为 Player 且位于警报圈（本物体的 Collider2D 范围）内"的订阅者收到。
    /// 对照观察：圈外 Player 收不到（InsideCollider2D 拦截），圈内无 Tag 的平民收不到（WithTag 拦截）。
    /// </summary>
    [AddComponentMenu("")]
    public class AlarmPublisher : MonoBehaviour
    {
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                PublishAlarm();
            }
        }

        /// <summary>
        /// 发布带过滤器的警报事件：WithTag("Player") + InsideCollider2D 全部通过才投递。
        /// </summary>
        public void PublishAlarm()
        {
            new AlarmEvent { Message = "圈内玩家请注意" }
                .WithFilter(new WithTag("Player"))
                .WithFilter(new InsideCollider2D())
                .Invoke(this);
        }
    }
}
#endif
