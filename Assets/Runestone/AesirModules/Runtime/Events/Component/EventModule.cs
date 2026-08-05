using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Runestone.AesirArchitecture;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 事件模块（MonoBehaviour 单例）。
    /// 通过 <c>[AesirListener]</c> 特性实现 Attribute 订阅，通过
    /// <see cref="AddListener{TEventArgs}(object, Action{TEventArgs})" /> 实现 Script 订阅，
    /// 通过 <see cref="InvokeEvent{TEventArgs}" /> 分发事件。
    /// <para>
    /// 支持 5 档优先级排序分发与双轨订阅共存。两种订阅分别存储于独立注册表，
    /// 分发时合并并按优先级排序。
    /// </para>
    /// <para>
    /// 作为 <see cref="AesirModules" /> 的子物体存在，由 <see cref="AesirModules.GetOrAddChild{T}" /> 懒加载创建。
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Aesir Modules/Event Module")]
    public class EventModule : AesirMonoBehaviour
    {
        #region 公开 API — 事件触发

        /// <summary>
        /// 触发事件。合并两个注册表的订阅者，按优先级排序后依次调用。
        /// </summary>
        public static void InvokeEvent<TEventArgs>(object sender, TEventArgs eventArgs)
            where TEventArgs : AesirEventArgs =>
            Instance.RaiseEvent(sender, eventArgs);

        #endregion

        #region 事件分发

        void RaiseEvent<TEventArgs>(object sender, TEventArgs eventArgs) where TEventArgs : AesirEventArgs
        {
            if (eventArgs == null)
            {
                AesirModulesDebug.LogError(AesirModulesDebug.EventModuleTag, "事件参数实例为 null。");
                return;
            }

            if (sender == null)
            {
                AesirModulesDebug.LogError(AesirModulesDebug.EventModuleTag,
                    $"发布者为 null，无法触发事件 {AesirEventUtility.GetEventName<TEventArgs>()}。");
                return;
            }

            eventArgs.SetSender(sender);

            var key = AesirEventUtility.GetEventBindingKey(eventArgs);

            // 取两个注册表的列表引用（不拷贝）
            AttributeBindings.TryGetValue(key, out var attrList);
            DynamicBindings.TryGetValue(key, out var dynList);

            var attrCount = attrList?.Count ?? 0;
            var dynCount = dynList?.Count ?? 0;
            var totalCount = attrCount + dynCount;
            if (totalCount == 0)
            {
                return;
            }

            // 只在有多个来源时才合并 + 排序；单来源直接用原列表
            List<BindingInfo> sorted;
            if (attrCount == 0)
            {
                sorted = dynList;
            }
            else if (dynCount == 0)
            {
                sorted = attrList;
            }
            else
            {
                sorted = new List<BindingInfo>(totalCount);
                sorted.AddRange(attrList);
                sorted.AddRange(dynList);
            }

            // count <= 1 时跳过排序
            if (sorted.Count > 1)
            {
                sorted.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            }

            // 复用 _invokeArgs 避免每次循环都 new object[]
            _invokeArgs[0] = eventArgs;

            var count = sorted.Count;
            for (var i = 0; i < count; i++)
            {
                var binding = sorted[i];
                if (AesirEventUtility.IsObjectUnityNull(binding.Subscriber))
                {
                    continue;
                }

                try
                {
                    binding.Invoke(_invokeArgs);
                }
                catch (TargetInvocationException ex)
                {
                    AesirModulesDebug.LogError(AesirModulesDebug.EventModuleTag,
                        $"订阅者 {binding.Subscriber} 处理事件 " +
                        $"{AesirEventUtility.GetEventName<TEventArgs>()} 时出错：" +
                        $"{ex.InnerException?.Message}");
                }
                catch (Exception ex)
                {
                    AesirModulesDebug.LogError(AesirModulesDebug.EventModuleTag, $"事件分发异常：{ex.Message}");
                }
            }
        }

        #endregion

        #region 单例

        static EventModule _instance;

        /// <summary>
        /// 全局单例入口。
        /// 优先在已加载场景中查找预放置的实例；未找到时在 <see cref="AesirModules" />（DDOL）下创建子物体。
        /// </summary>
        public static EventModule Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                // 尝试在已加载的场景中查找预放置的实例
                _instance = FindFirstObjectByType<EventModule>();
                if (_instance != null)
                {
                    return _instance;
                }

                // 未找到预放置实例 → 在 AesirModules 下创建（跟随父级 DDOL）
                _instance = AesirModules.GetOrAddChild<EventModule>();
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
        }

        void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        #endregion

        #region 注册表

        /// <summary>
        /// Attribute 订阅注册表。以事件类型 AssemblyQualifiedName 为键。
        /// </summary>
        public Dictionary<string, List<BindingInfo>> AttributeBindings =
            new Dictionary<string, List<BindingInfo>>();

        /// <summary>
        /// Script 订阅注册表。以事件类型 AssemblyQualifiedName 为键。
        /// </summary>
        public Dictionary<string, List<BindingInfo>> DynamicBindings =
            new Dictionary<string, List<BindingInfo>>();

        /// <summary>
        /// 复用的参数数组，避免每次分发都分配 object[]。
        /// EventModule 是单例，分发是同步的（非重入），所以单个实例字段即可。
        /// </summary>
        readonly object[] _invokeArgs = new object[1];

        #endregion

        #region 公开 API — Attribute 订阅

        /// <summary>
        /// 添加 Attribute 订阅者。反射扫描对象上标有 <c>[AesirListener]</c> 的方法并注册。
        /// 通常在 <c>OnEnable</c> 中调用。
        /// </summary>
        public static void AddListener(object subscriber) => Instance.Bind(subscriber);

        /// <summary>
        /// 移除订阅者。从两个注册表中移除该对象的所有绑定（含 Attribute 和 Script）。
        /// 通常在 <c>OnDisable</c> 中调用。
        /// </summary>
        public static void RemoveListener(object subscriber) => Instance.Unbind(subscriber);

        #endregion

        #region 公开 API — Script 订阅

        /// <summary>
        /// 添加 Script 订阅。通过 Lambda 委托监听指定事件类型，无需 <c>[AesirListener]</c> 特性。
        /// 返回自动移除句柄，<see cref="AutoRemoveListenerHandle.Dispose" /> 或 using 块结束时自动注销。
        /// 默认优先级 <see cref="SubscriberPriority.Medium" />。
        /// </summary>
        public static AutoRemoveListenerHandle AddListener<TEventArgs>(object subscriber,
            Action<TEventArgs> callback) where TEventArgs : AesirEventArgs =>
            Instance.AddDynamicBinding(subscriber, callback, SubscriberPriority.Medium);

        /// <summary>
        /// 添加 Script 订阅，指定优先级。返回自动移除句柄。
        /// </summary>
        public static AutoRemoveListenerHandle AddListener<TEventArgs>(object subscriber,
            Action<TEventArgs> callback,
            SubscriberPriority priority) where TEventArgs : AesirEventArgs =>
            Instance.AddDynamicBinding(subscriber, callback, priority);

        /// <summary>
        /// 添加 Script 订阅（非泛型版）。通过事件参数实例推断类型，反射适配。
        /// 默认优先级 <see cref="SubscriberPriority.Medium" />。
        /// </summary>
        public static AutoRemoveListenerHandle AddListener(object subscriber,
            AesirEventArgs eventArgs,
            Action<AesirEventArgs> callback) =>
            Instance.AddDynamicBindingGeneric(subscriber, eventArgs, callback, SubscriberPriority.Medium);

        /// <summary>
        /// 添加 Script 订阅（非泛型版），指定优先级。
        /// </summary>
        public static AutoRemoveListenerHandle AddListener(object subscriber,
            AesirEventArgs eventArgs,
            SubscriberPriority priority,
            Action<AesirEventArgs> callback) =>
            Instance.AddDynamicBindingGeneric(subscriber, eventArgs, callback, priority);

        #endregion

        #region Attribute 订阅注册

        void Bind(object subscriber)
        {
            if (subscriber == null)
            {
                return;
            }

            // 使用 GetMethods 而非 GetMembers，避免扫描属性、字段等无关成员
            var methods = subscriber.GetType().GetMethods(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var method in methods)
            {
                try
                {
                    var attributes = method.GetCustomAttributes(typeof(AesirListenerAttribute), true);
                    if (attributes.Length == 0)
                    {
                        continue;
                    }

                    foreach (var attr in attributes.Cast<AesirListenerAttribute>())
                    {
                        var bindingKey = ResolveBindingKey(attr, method);
                        if (string.IsNullOrEmpty(bindingKey))
                        {
                            Debug.LogWarning($"方法 {method.Name}（{subscriber.GetType().Name}）无法确定监听的事件类型。" +
                                             "请通过 [AesirListener(typeof(MyEventArgs))] 显式指定，" +
                                             "或为方法添加一个 AesirEventArgs 子类参数。");
                            continue;
                        }

                        var info = new StaticBindingInfo(bindingKey, method, subscriber, attr.Priority);
                        if (!IsAlreadyBound(info))
                        {
                            AddToRegistry(AttributeBindings, info);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"绑定 {method.Name}（{subscriber.GetType().Name}）时出错：{e.Message}");
                }
            }
        }

        bool Unbind(object subscriber)
        {
            if (subscriber == null)
            {
                return false;
            }

            var wasUnbound = RemoveSubscriberFromRegistry(AttributeBindings, subscriber);
            wasUnbound |= RemoveSubscriberFromRegistry(DynamicBindings, subscriber);
            return wasUnbound;
        }

        static string ResolveBindingKey(AesirListenerAttribute attribute, MethodInfo method)
        {
            if (attribute.EventType != null)
            {
                return attribute.EventType.AssemblyQualifiedName;
            }

            var parameters = method.GetParameters();
            if (parameters.Length > 0 && typeof(AesirEventArgs).IsAssignableFrom(parameters[0].ParameterType))
            {
                return parameters[0].ParameterType.AssemblyQualifiedName;
            }

            return null;
        }

        bool IsAlreadyBound(BindingInfo info)
        {
            if (!AttributeBindings.TryGetValue(info.BindingKey, out var list))
            {
                return false;
            }

            // 用 foreach 替代 LINQ Any，避免闭包和迭代器分配
            var staticInfo = (StaticBindingInfo)info;
            foreach (var b in list)
            {
                if (b is StaticBindingInfo sb && ReferenceEquals(b.Subscriber, info.Subscriber) &&
                    sb.Method == staticInfo.Method)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Script 订阅注册

        AutoRemoveListenerHandle AddDynamicBinding<TEventArgs>(object subscriber,
            Action<TEventArgs> callback,
            SubscriberPriority priority) where TEventArgs : AesirEventArgs
        {
            var info = new DynamicBindingInfo<TEventArgs>(callback, priority, subscriber);
            AddToRegistry(DynamicBindings, info);
            return new AutoRemoveListenerHandle(() => RemoveFromRegistry(DynamicBindings, info));
        }

        /// <summary>
        /// AddDynamicBinding 方法的 MethodInfo 缓存，避免每次非泛型注册都反射查找。
        /// </summary>
        static readonly MethodInfo AddDynamicBindingMethod = typeof(EventModule).GetMethod(
            nameof(AddDynamicBinding), BindingFlags.NonPublic | BindingFlags.Instance);

        AutoRemoveListenerHandle AddDynamicBindingGeneric(object subscriber,
            AesirEventArgs eventArgs,
            Action<AesirEventArgs> action,
            SubscriberPriority priority)
        {
            if (eventArgs == null)
            {
                throw new ArgumentNullException(nameof(eventArgs));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var eventType = eventArgs.GetType();
            if (!typeof(AesirEventArgs).IsAssignableFrom(eventType))
            {
                throw new ArgumentException($"类型 {eventType.Name} 不是有效的 AesirEventArgs。");
            }

            // Delegate.CreateDelegate 把 Action<AesirEventArgs> 重新包成 Action<TEventArgs>
            var wrapperType = typeof(Action<>).MakeGenericType(eventType);
            var dynamicAction = Delegate.CreateDelegate(wrapperType, action.Target, action.Method);

            // 用缓存的 MethodInfo 反射调 AddDynamicBinding<TEventArgs>
            var genericMethod = AddDynamicBindingMethod.MakeGenericMethod(eventType);
            return (AutoRemoveListenerHandle)genericMethod.Invoke(this,
                new[] { subscriber, dynamicAction, priority });
        }

        #endregion

        #region 注册表通用操作

        static void AddToRegistry(Dictionary<string, List<BindingInfo>> registry, BindingInfo info)
        {
            if (string.IsNullOrEmpty(info.BindingKey))
            {
                return;
            }

            if (!registry.TryGetValue(info.BindingKey, out var list))
            {
                list = new List<BindingInfo>();
                registry[info.BindingKey] = list;
            }

            list.Add(info);
        }

        static void RemoveFromRegistry(Dictionary<string, List<BindingInfo>> registry, BindingInfo info)
        {
            if (info == null || string.IsNullOrEmpty(info.BindingKey))
            {
                return;
            }

            if (registry.TryGetValue(info.BindingKey, out var list))
            {
                list.Remove(info);
                if (list.Count == 0)
                {
                    registry.Remove(info.BindingKey);
                }
            }
        }

        static bool RemoveSubscriberFromRegistry(Dictionary<string, List<BindingInfo>> registry,
            object subscriber)
        {
            var wasRemoved = false;
            foreach (var key in registry.Keys.ToList())
            {
                var list = registry[key];
                var count = list.Count;
                list.RemoveAll(b => ReferenceEquals(b.Subscriber, subscriber));
                if (list.Count < count)
                {
                    wasRemoved = true;
                }

                if (list.Count == 0)
                {
                    registry.Remove(key);
                }
            }

            return wasRemoved;
        }

        #endregion
    }
}
