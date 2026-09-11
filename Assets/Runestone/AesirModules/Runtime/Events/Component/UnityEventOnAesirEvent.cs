using Runestone.AesirArchitecture;
using UnityEngine;
using UnityEngine.Events;

namespace Runestone.AesirModules
{
    /// <summary>
    /// UnityEvent 桥接组件。监听指定类型的 <see cref="AesirEventArgs" />，
    /// 事件触发时调用 Inspector 中配置的 <see cref="UnityEvent" /> 回调，
    /// 供非程序员在 Inspector 中串联事件响应，无需编写订阅代码。
    /// <para>
    /// 用法：挂载本组件 → 经 <see cref="SubclassSelectorAttribute" /> 下拉选择监听的事件类型 →
    /// 在 On Raised 中绑定任意 UnityEvent 回调（播放音效、激活物体等）。
    /// </para>
    /// <para>
    /// 运行时修改事件参数类型不会自动重订，需重新启用组件使订阅生效。
    /// </para>
    /// </summary>
    [AddComponentMenu("Aesir Modules/UnityEvent On AesirEvent")]
    [DisallowMultipleComponent]
    public class UnityEventOnAesirEvent : MonoBehaviour
    {
        /// <summary>
        /// 监听的事件参数实例，用于确定监听的事件类型。
        /// 经 [SerializeReference] + <see cref="SubclassSelectorAttribute" /> 在 Inspector 中选择。
        /// </summary>
        [SerializeReference]
        [SubclassSelector]
        AesirEventArgs eventArgs;

        /// <summary>
        /// 事件触发时调用的 UnityEvent 回调（在 Inspector 中配置）。
        /// </summary>
        [SerializeField]
        UnityEvent onRaised = new UnityEvent();

        /// <summary>订阅句柄，用于 OnDisable 时自动退订。</summary>
        AutoRemoveListenerHandle _handle;

        void OnEnable()
        {
            if (eventArgs == null)
            {
                AesirModulesDebug.LogWarning(this, AesirModulesDebug.EventModuleTag, "未配置事件参数类型，本组件不监听任何事件。");
                return;
            }

            _handle = EventModule.AddListener(this, eventArgs, OnEventRaised);
        }

        void OnDisable() => _handle.Dispose();

        void OnEventRaised(AesirEventArgs e) => onRaised?.Invoke();
    }
}
