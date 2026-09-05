using UnityEngine;

namespace Runestone.AesirModules.Samples.Events.KeyPress
{
    /// <summary>
    /// 按键事件订阅者。通过 [AesirListener] 静态订阅 <see cref="OnKeyPressed"/> 事件。
    /// </summary>
    [AddComponentMenu("")]
    public class KeyPressSubscriber : MonoBehaviour
    {
        void OnEnable()
        {
            EventModule.AddListener(this);
        }

        void OnDisable()
        {
            EventModule.RemoveListener(this);
        }

        [AesirListener]
        private void OnKeyPressed(KeyPressedEvent e)
        {
            Debug.Log($"[{name}] 收到 OnKeyPressed 事件，按键：{e.Key}，发布者：{e.Sender}");
        }
    }
}
