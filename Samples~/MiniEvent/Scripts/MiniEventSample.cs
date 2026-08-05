using System;
using UnityEngine;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MiniEvent 与 MiniEvent&lt;T&gt; 使用示例。
    /// </summary>
    /// <remarks>
    /// 本示例展示了框架内置轻量事件系统的两种形态：
    /// <list type="bullet">
    /// <item><see cref="MiniEvent"/> —— 无参数事件，适合简单的"发生了某事"通知。</item>
    /// <item><see cref="MiniEvent{T}"/> —— 带参数事件，可将数据载荷随事件一起传递。</item>
    /// </list>
    /// 同时演示了两种自动注销策略：
    /// <list type="bullet">
    /// <item><c>RemoveListenerWhenGameObjectOnDestroyed</c> —— 随 GameObject 销毁时自动注销，适合生命周期与 GameObject 一致的监听。</item>
    /// <item><c>RemoveListenerWhenGameObjectOnDisable</c> —— 随 GameObject 禁用时自动注销，适合仅在面板激活时才需要监听的场景。</item>
    /// </list>
    /// <para>通过 [ContextMenu] 可在 Inspector 右键菜单中手动触发各事件，验证监听回调是否正常执行。</para>
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.MiniEvent"/>
    /// <seealso cref="Runestone.AesirArchitecture.MiniEvent{T}"/>
    public sealed class MiniEventSample : MonoBehaviour
    {
        /// <summary>
        /// 自定义事件载荷结构体，演示 MiniEvent 对复合数据类型的支持。
        /// </summary>
        struct CustomEvent
        {
            /// <summary>事件携带的分数数据。</summary>
            public int Score;

            /// <summary>事件携带的文本消息。</summary>
            public string Message;
        }

        readonly MiniEvent _onGameStart = new MiniEvent();
        readonly MiniEvent<string> _onMessageReceived = new MiniEvent<string>();
        readonly MiniEvent<int> _onScoreChanged = new MiniEvent<int>();
        readonly MiniEvent<CustomEvent> _onCustomEvent = new MiniEvent<CustomEvent>();

        void Start()
        {
            // 使用 RemoveListenerWhenGameObjectOnDestroyed 确保监听随 GameObject 销毁自动清理，
            // 避免因事件源比监听者存活更久而导致的内存泄漏。
            _onCustomEvent
                .AddListener(evt => Debug.Log($"[MiniEvent] OnCustomEvent → {evt.Score}, {evt.Message}"))
                .RemoveListenerWhenGameObjectOnDestroyed(gameObject);
        }

        void OnEnable()
        {
            // 使用 RemoveListenerWhenGameObjectOnDisable 确保面板隐藏时停止监听，
            // 重新激活时不会重复注册（因为上次已注销），天然避免重复回调问题。
            _onGameStart.AddListener(() => Debug.Log("[MiniEvent] OnGameStart 被触发"))
                .RemoveListenerWhenGameObjectOnDisable(gameObject);

            _onScoreChanged.AddListener(score => Debug.Log($"[MiniEvent] OnScoreChanged → {score}"))
                .RemoveListenerWhenGameObjectOnDisable(gameObject);

            _onMessageReceived.AddListener(msg => Debug.Log($"[MiniEvent] OnMessageReceived → {msg}"))
                .RemoveListenerWhenGameObjectOnDisable(gameObject);
        }

        [ContextMenu("触发 OnGameStart")]
        void InvokeGameStart() => _onGameStart.Invoke();

        [ContextMenu("触发 OnScoreChanged (+10)")]
        void InvokeScoreChanged() => _onScoreChanged.Invoke(10);

        [ContextMenu("触发 OnMessageReceived")]
        void InvokeMessageReceived() => _onMessageReceived.Invoke("Hello MiniEvent!");

        [ContextMenu("触发 OnCustomEvent")]
        void InvokeCustomEvent() =>
            _onCustomEvent.Invoke(new CustomEvent { Score = 100, Message = "Hello MiniEvent!" });
    }
}
