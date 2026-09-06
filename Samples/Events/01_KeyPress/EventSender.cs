#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
using UnityEngine;

namespace Runestone.AesirModules.Samples.Events.KeyPress
{
    /// <summary>
    /// 按键事件发布者。按指定键发布 <see cref="KeyPressedEvent"/> 事件。
    /// </summary>
    [AddComponentMenu("")]
    public class EventSender : MonoBehaviour
    {
        [SerializeField] KeyCode triggerKey = KeyCode.Space;

        void Update()
        {
            if (Input.GetKeyDown(triggerKey))
            {
                new KeyPressedEvent { Key = triggerKey }.Invoke(this);
            }
        }
    }
}
#endif
