#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
using Runestone.AesirArchitecture;
using UnityEngine;

namespace Runestone.AesirModules.Samples.Events.KeyPress
{
    /// <summary>
    /// 按键事件订阅者。用 Script 订阅（<see cref="EventModule.AddListener{TEventArgs}(object, System.Action{TEventArgs})" />）
    /// 监听 <see cref="KeyPressedEvent" /> 事件，持有句柄在 OnDisable 精确退订。
    /// </summary>
    [AddComponentMenu("")]
    public class KeyPressSubscriber : MonoBehaviour
    {
        AutoRemoveListenerHandle _handle;

        void OnEnable()
        {
            _handle = EventModule.AddListener<KeyPressedEvent>(this, OnKeyPressed);
        }

        void OnDisable()
        {
            _handle.Dispose();
        }

        void OnKeyPressed(KeyPressedEvent e)
        {
            Debug.Log($"[{name}] 收到 OnKeyPressed 事件，按键：{e.Key}，发布者：{e.Sender}");
        }
    }
}
#endif
