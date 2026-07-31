using UnityEngine;

namespace Runestone.AesirModules.Samples.Events
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
