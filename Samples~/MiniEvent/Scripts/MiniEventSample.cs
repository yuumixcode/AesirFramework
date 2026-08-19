using System;
using UnityEngine;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MiniEvent 与 MiniEvent&lt;T&gt; 演示组件。
    /// <para>展示无参事件、带参事件的注册、触发与自动注销。</para>
    /// <para>通过 ContextMenu 可在 Inspector 右键菜单中触发事件，验证监听回调。</para>
    /// </summary>
    public sealed class MiniEventSample : MonoBehaviour
    {
        struct CustomEvent
        {
            public int Score;
            public string Message;
        }

        readonly MiniEvent _onGameStart = new MiniEvent();
        readonly MiniEvent<string> _onMessageReceived = new MiniEvent<string>();
        readonly MiniEvent<int> _onScoreChanged = new MiniEvent<int>();
        readonly MiniEvent<CustomEvent> _onCustomEvent = new MiniEvent<CustomEvent>();

        void Start()
        {
            _onCustomEvent
                .AddListener(evt => Debug.Log($"[MiniEvent] OnCustomEvent → {evt.Score}, {evt.Message}"))
                .RemoveListenerWhenGameObjectOnDestroyed(gameObject);
        }

        void OnEnable()
        {
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
