using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// Mono 生命周期事件代理。作为全局单例挂载在 [Aesir Architecture] GameObject 上，
    /// 将 Unity 原生生命周期回调和自定义 PlayerLoop 阶段统一为可订阅的有序事件。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     通过 <see cref="Instance" /> 访问实例方法 <see cref="AddListener" />、<see cref="RemoveListener" /> 等，
    ///     或通过 <see cref="MonoLifecycleProxyExtensions" /> 扩展方法快捷调用。
    ///     </para>
    ///     <para>
    ///     <b>可排序监听列表</b>：每个事件维护一个 <see cref="List{T}" /> 存储 <see cref="ListenerEntry" />，
    ///     使用 Order + InsertionIndex 稳定排序，按排序结果依次调用回调。
    ///     与 <see cref="AesirArchitecturePlayerLoop" /> 的排序机制一致。
    ///     </para>
    ///     <para>
    ///     <b>自动取消订阅</b>：通过 <see cref="Register(MonoBehaviour)" /> 注册的 MonoBehaviour，
    ///     其所有监听句柄会绑定到目标 GameObject 的 OnDestroy 事件，物体销毁时自动从代理中取消订阅。
    ///     非 MonoBehaviour 对象通过 <see cref="Register(object)" /> 注册，返回组合句柄由调用方管理生命周期。
    ///     </para>
    ///     <para>
    ///     <b>PlayerLoop 集成</b>：<see cref="MonoLifecycleEvent.BeforeUpdate" /> 和
    ///     <see cref="MonoLifecycleEvent.AfterUpdate" />
    ///     通过注册到 <see cref="AesirArchitecturePlayerLoop" /> 实现，Awake 时注册、OnDestroy 时注销。
    ///     </para>
    ///     <para>
    ///     <b>ICustomXXX 自动注册</b>：调用 <see cref="RegisterAuto(object)" /> 传入实现了任意
    ///     ICustomXXX 接口的对象（MonoBehaviour 或纯 C# 类均可），代理会自动扫描并注册所有对应方法到匹配的生命周期事件。
    ///     </para>
    /// </remarks>
    /// <seealso cref="MonoLifecycleEvent" />
    /// <seealso cref="ICustomFixedUpdate" />
    /// <seealso cref="AesirArchitecturePlayerLoop" />
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-998)]
    public sealed class MonoLifecycleProxy : AesirMonoBehaviour
    {
        static MonoLifecycleProxy _instance;

        readonly Dictionary<MonoLifecycleEvent, List<ListenerEntry>> _sortedListeners =
            new Dictionary<MonoLifecycleEvent, List<ListenerEntry>>();

        bool _sortDirty;
        long _nextInsertionIndex;
        bool _playerLoopRegistered;

        /// <summary>
        /// 重置所有实例状态：清空监听、注销 PlayerLoop
        /// </summary>
        /// <remarks>
        /// 由 <see cref="ResetStatics" /> 和 <see cref="OnDestroy" /> 内部调用，
        /// 确保无论域重载还是组件销毁，都走同一条完整重置路径，不会遗漏 PlayerLoop 注销等清理步骤。
        /// </remarks>
        void Reset()
        {
            ClearAllListeners();
            UnregisterFromPlayerLoop();
        }

        /// <summary>
        /// 域加载时重置静态单例并清空监听，兼容关闭 Domain Reload 的 Play 模式设置
        /// </summary>
        /// <remarks>
        /// 由 <c>[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]</c> 自动触发，无需手动调用。
        /// 关闭 Domain Reload 时静态字段不会自动清零，此方法确保下次进入 Play 模式时单例被重新查找/创建，
        /// 不残留上一次 Play 会话的引用与监听。
        /// </remarks>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            if (_instance != null)
            {
                _instance.Reset();
            }

            _instance = null;
        }

        void Update()
        {
            InvokeEvent(MonoLifecycleEvent.Update);
        }

        void FixedUpdate()
        {
            InvokeEvent(MonoLifecycleEvent.FixedUpdate);
        }

        void LateUpdate()
        {
            InvokeEvent(MonoLifecycleEvent.LateUpdate);
        }

        void OnDestroy()
        {
            Reset();
        }

        void OnApplicationFocus(bool focused)
        {
            InvokeEvent(MonoLifecycleEvent.OnApplicationFocus);
        }

        void OnApplicationPause(bool pauseStatus)
        {
            InvokeEvent(MonoLifecycleEvent.OnApplicationPause);
        }

        void OnApplicationQuit()
        {
            InvokeEvent(MonoLifecycleEvent.OnApplicationQuit);
        }

        /// <summary>
        /// 快捷注册（MonoBehaviour 专用）。扫描实现的所有 ICustomXXX 接口，
        /// 将对应方法自动注册到匹配的生命周期事件中，并绑定到目标 GameObject 的 OnDestroy 自动取消订阅。
        /// </summary>
        /// <param name="mono">实现了任意 ICustomXXX 接口的 MonoBehaviour</param>
        public static void Register(MonoBehaviour mono)
        {
            Instance.RegisterAuto(mono).RemoveListenerWhenGameObjectOnDestroyed(mono);
        }

        /// <summary>
        /// 快捷注册（任意对象）。扫描实现的所有 ICustomXXX 接口，
        /// 将对应方法自动注册到匹配的生命周期事件中。
        /// </summary>
        /// <param name="obj">实现了任意 ICustomXXX 接口的对象</param>
        /// <returns>组合句柄，Dispose 时一次性移除本次注册的所有监听</returns>
        /// <remarks>
        /// 适用于非 MonoBehaviour 的纯 C# 类。调用方负责在适当时机 Dispose 返回的句柄以取消订阅，
        /// 或配合 <see cref="RemoveListenerExtensions" /> 绑定到其他 Unity 生命周期事件。
        /// </remarks>
        public static AutoRemoveListenerHandle Register(object obj) => Instance.RegisterAuto(obj);

        /// <summary>
        /// 添加生命周期事件监听，返回可自动移除的监听句柄。
        /// </summary>
        /// <param name="evt">要订阅的生命周期事件类型</param>
        /// <param name="callback">事件触发时执行的回调委托</param>
        /// <param name="order">执行优先级，值越小越先执行；同 order 时按注册顺序执行</param>
        /// <returns>用于后续自动移除该监听的句柄</returns>
        public AutoRemoveListenerHandle AddListener(MonoLifecycleEvent evt, Action callback, int order = 0)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            var entry = new ListenerEntry
            {
                Callback = callback,
                Order = order,
                InsertionIndex = _nextInsertionIndex++
            };

            GetOrCreateList(evt).Add(entry);
            _sortDirty = true;

            return new AutoRemoveListenerHandle(() => RemoveListener(evt, callback));
        }

        /// <summary>
        /// 移除指定事件的监听者
        /// </summary>
        /// <param name="evt">目标生命周期事件类型</param>
        /// <param name="callback">要移除的回调委托，必须与注册时传入的实例相同</param>
        public void RemoveListener(MonoLifecycleEvent evt, Action callback)
        {
            if (_sortedListeners.TryGetValue(evt, out var list))
            {
                for (var i = list.Count - 1; i >= 0; i--)
                {
                    if (list[i].Callback == callback)
                    {
                        list.RemoveAt(i);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// 快捷注册。扫描对象实现的所有 ICustomXXX 接口，
        /// 将对应方法自动注册到匹配的生命周期事件中，返回组合句柄。
        /// </summary>
        /// <param name="obj">实现了任意 ICustomXXX 接口的对象（MonoBehaviour 或纯 C# 类均可）</param>
        /// <returns>组合句柄，Dispose 时一次性移除本次注册的所有监听；若对象未实现任何接口则返回默认句柄</returns>
        /// <remarks>
        /// 对于 MonoBehaviour，可使用静态 <see cref="Register(MonoBehaviour)" /> 替代，
        /// 后者会额外将句柄绑定到 GameObject 的 OnDestroy 自动取消订阅。
        /// 对于非 MonoBehaviour 对象，调用方需自行管理返回句柄的生命周期。
        /// </remarks>
        public AutoRemoveListenerHandle RegisterAuto(object obj)
        {
            if (obj == null)
            {
                return default;
            }

            var handles = new List<AutoRemoveListenerHandle>();

            if (obj is ICustomFixedUpdate fu)
            {
                handles.Add(AddListener(MonoLifecycleEvent.FixedUpdate, fu.OnCustomFixedUpdate));
            }

            if (obj is ICustomBeforeUpdate bu)
            {
                handles.Add(AddListener(MonoLifecycleEvent.BeforeUpdate, bu.OnCustomBeforeUpdate));
            }

            if (obj is ICustomUpdate u)
            {
                handles.Add(AddListener(MonoLifecycleEvent.Update, u.OnCustomUpdate));
            }

            if (obj is ICustomLateUpdate lu)
            {
                handles.Add(AddListener(MonoLifecycleEvent.LateUpdate, lu.OnCustomLateUpdate));
            }

            if (obj is ICustomAfterUpdate au)
            {
                handles.Add(AddListener(MonoLifecycleEvent.AfterUpdate, au.OnCustomAfterUpdate));
            }

            if (obj is ICustomOnApplicationFocus af)
            {
                handles.Add(AddListener(MonoLifecycleEvent.OnApplicationFocus, af.OnCustomApplicationFocus));
            }

            if (obj is ICustomOnApplicationPause ap)
            {
                handles.Add(AddListener(MonoLifecycleEvent.OnApplicationPause, ap.OnCustomApplicationPause));
            }

            if (obj is ICustomOnApplicationQuit aq)
            {
                handles.Add(AddListener(MonoLifecycleEvent.OnApplicationQuit, aq.OnCustomApplicationQuit));
            }

            return new AutoRemoveListenerHandle(() =>
            {
                foreach (var h in handles)
                {
                    h.Dispose();
                }
            });
        }

        /// <summary>
        /// 获取指定事件当前的监听者数量
        /// </summary>
        /// <param name="evt">目标生命周期事件类型</param>
        /// <returns>已注册的监听者数量；若该事件无监听者则返回 0</returns>
        public int GetListenerCount(MonoLifecycleEvent evt) =>
            _sortedListeners.TryGetValue(evt, out var list) ? list.Count : 0;

        /// <summary>
        /// 清空所有事件的监听者
        /// </summary>
        public void ClearAllListeners()
        {
            _sortedListeners.Clear();
            _sortDirty = false;
        }

        void OnBeforeUpdate()
        {
            InvokeEvent(MonoLifecycleEvent.BeforeUpdate);
        }

        void OnAfterUpdate()
        {
            InvokeEvent(MonoLifecycleEvent.AfterUpdate);
        }

        void InvokeEvent(MonoLifecycleEvent evt)
        {
            if (!_sortedListeners.TryGetValue(evt, out var list) || list.Count == 0)
            {
                return;
            }

            EnsureSorted();
            for (var i = 0; i < list.Count; i++)
            {
                list[i].Callback();
            }
        }

        void EnsureSorted()
        {
            if (!_sortDirty)
            {
                return;
            }

            foreach (var kvp in _sortedListeners)
            {
                kvp.Value.Sort((a, b) =>
                {
                    var res = a.Order.CompareTo(b.Order);
                    return res != 0 ? res : a.InsertionIndex.CompareTo(b.InsertionIndex);
                });
            }

            _sortDirty = false;
        }

        void RegisterToPlayerLoop()
        {
            if (_playerLoopRegistered)
            {
                return;
            }

            AesirArchitecturePlayerLoop.Register(AesirArchitectureLifecyclePhase.BeforeUpdate,
                OnBeforeUpdate);
            AesirArchitecturePlayerLoop.Register(AesirArchitectureLifecyclePhase.AfterUpdate, OnAfterUpdate);
            _playerLoopRegistered = true;
        }

        void UnregisterFromPlayerLoop()
        {
            if (!_playerLoopRegistered)
            {
                return;
            }

            AesirArchitecturePlayerLoop.Unregister(AesirArchitectureLifecyclePhase.BeforeUpdate,
                OnBeforeUpdate);
            AesirArchitecturePlayerLoop.Unregister(AesirArchitectureLifecyclePhase.AfterUpdate,
                OnAfterUpdate);
            _playerLoopRegistered = false;
        }

        List<ListenerEntry> GetOrCreateList(MonoLifecycleEvent evt)
        {
            if (!_sortedListeners.TryGetValue(evt, out var list))
            {
                list = new List<ListenerEntry>();
                _sortedListeners[evt] = list;
            }

            return list;
        }

        /// <summary>
        /// 监听条目，记录单个回调及其排序信息
        /// </summary>
        /// <remarks>
        /// <see cref="InsertionIndex" /> 是自增序号，当多个条目的 <see cref="Order" /> 相同时，
        /// 使用 InsertionIndex 作为次级排序键，确保相同优先级的回调按注册顺序执行，实现稳定排序。
        /// 与 <see cref="AesirArchitecturePlayerLoop" /> 的 HookEntry 结构一致。
        /// </remarks>
        struct ListenerEntry
        {
            public Action Callback;
            public int Order;
            public long InsertionIndex;
        }

        #region 单例

        /// <summary>
        /// 获取全局唯一的 MonoLifecycleProxy 实例。
        /// </summary>
        /// <remarks>
        /// 优先在已加载场景中查找预放置的实例；未找到时通过 <see cref="AesirArchitecture.GetOrAddComponent{T}" />
        /// 挂载到 [Aesir Architecture] GameObject 上，复用架构宿主对象。
        /// </remarks>
        public static MonoLifecycleProxy Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                // 尝试在已加载的场景中查找预放置的实例
                _instance = FindAnyObjectByType<MonoLifecycleProxy>();
                if (_instance != null)
                {
                    return _instance;
                }

                _instance = AesirArchitecture.GetOrAddComponent<MonoLifecycleProxy>();
                return _instance;
            }
        }

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            RegisterToPlayerLoop();
        }

        #endregion
    }
}
