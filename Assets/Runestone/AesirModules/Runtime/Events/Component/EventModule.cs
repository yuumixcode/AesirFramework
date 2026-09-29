using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Runestone.AesirArchitecture;
using UnityEngine;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 事件模块（MonoBehaviour 单例）。
    /// 通过 <c>[AesirListener]</c> 特性实现 Attribute 订阅，通过
    /// <see cref="AddListener{TEventArgs}(object, Action{TEventArgs})" /> 实现 Script 订阅，
    /// 通过 <see cref="InvokeEvent{TEventArgs}" /> 分发事件。
    /// <para>
    /// 支持 4 档优先级排序分发与双轨订阅共存。两种订阅分别存储于独立注册表，
    /// 分发时合并并按优先级排序。
    /// </para>
    /// <para>
    /// 分发期可靠性：自动检测并清理已销毁的 Unity 对象订阅者（死引用）；
    /// 支持 <see cref="AesirEventArgs.WithFilter" /> 声明的订阅者过滤器实现精确投递；
    /// 可通过 <c>executionMsLimit</c> 开启分发耗时告警。
    /// </para>
    /// <para>
    /// <b>快照语义与重入安全</b>：每趟分发基于注册表快照迭代——回调内退订/注册只影响后续分发，
    /// 不干扰本趟；回调内同步发布事件（重入）使用独立的迭代缓冲区与参数数组，
    /// 外层分发不受覆写影响。发布者同理按快照校正：内层分发覆写共享参数实例的
    /// <see cref="AesirEventArgs.Sender" /> 后，外层遍历会把 Sender 重新校正回本趟发布者，
    /// 过滤器判定与订阅者读取均不受内层污染。性能计时（executionMsLimit）仅对顶层分发生效。
    /// 排序为 Priority 主键 + 注册序号次键的稳定排序，同优先级按注册顺序执行。
    /// </para>
    /// <para>
    /// 作为 <see cref="AesirModules" /> 的子物体存在，由 <see cref="AesirModules.GetOrAddChild{T}" /> 懒加载创建。
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(-999)]
    [AddComponentMenu("Aesir Modules/Event Module")]
    public class EventModule : AesirMonoBehaviour
    {
        /// <summary>
        /// 是否将本物体加入 DontDestroyOnLoad 场景。仅在本物体为根物体（场景预放置）时生效。
        /// </summary>
        /// <remarks>
        /// 默认 true（跨场景持久）。与 <see cref="AudioModule" /> 的 <c>dontDestroyOnLoad</c>
        /// 同名同义，保持包内单例的生命周期决策一致。
        /// <para>
        /// <b>为什么预放置路径同样需要它：</b>本模块的双注册表（AttributeBindings / DynamicBindings）
        /// 是实例字段，随模块本体一同存亡。预放置实例若不加入 DDOL，场景卸载时
        /// <see cref="OnDestroy" /> 会把单例置空、注册表随之丢失，跨场景常驻的订阅者
        /// 再也收不到任何事件，其 <see cref="AutoRemoveListenerHandle" /> 闭包也会捕获已死的实例。
        /// 运行时自动创建的实例挂载在 [Aesir Modules] 宿主下（非根物体），DDOL 跟随宿主决策，
        /// 本字段不参与判断。
        /// </para>
        /// <para>
        /// <b>行为变更提示</b>：本字段为后续版本新增，默认 <c>true</c>——场景里已放置的根物体
        /// <see cref="EventModule" /> 从下一个会话起会被迁入 DDOL 场景（此前随场景卸载销毁）。
        /// 需要"随场景卸载"的旧行为请显式取消勾选。
        /// </para>
        /// </remarks>
        [SerializeField]
        bool dontDestroyOnLoad = true;

        #region 公开 API — 事件触发

        /// <summary>
        /// 触发事件。合并两个注册表的订阅者，按优先级排序后依次调用。
        /// <para>
        /// 走非创建式获取：订阅表是<b>实例字段</b>，因此"不存在实例"等价于"不存在任何订阅者"，
        /// 分发无事可做——若走 <see cref="Instance" /> 会在场景卸载等时机重建 DDOL 宿主
        /// （详见 <see cref="TryGetExisting" /> 的说明）。
        /// </para>
        /// </summary>
        public static void InvokeEvent<TEventArgs>(object sender, TEventArgs eventArgs)
            where TEventArgs : AesirEventArgs
        {
            if (!TryGetExisting(out var module))
            {
                return;
            }

            module.RaiseEvent(sender, eventArgs);
        }

        #endregion

        #region 可靠性与性能监控

        /// <summary>
        /// 分发耗时告警阈值（毫秒）。0 = 关闭性能监控。
        /// <para>
        /// 分发耗时超过该值时输出 Warning 日志（含事件名、耗时与订阅者数量）。
        /// 预放置实例可在 Inspector 中调整；运行时创建的实例使用默认值 0。
        /// </para>
        /// </summary>
        [SerializeField]
        float executionMsLimit;

        /// <summary>
        /// 本轮分发收集到的死绑定（订阅者已销毁）。循环外统一移除，避免遍历时修改注册表列表；
        /// 复用实例列表保持稳态零分配。
        /// </summary>
        readonly List<BindingInfo> _deadBindings = new List<BindingInfo>();

        /// <summary>
        /// 分发耗时测量。静态复用实例，避免每次分发分配 <see cref="Stopwatch" />。
        /// </summary>
        static readonly Stopwatch _dispatchStopwatch = new Stopwatch();

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

            // 本趟分发的发布者快照。共享参数实例（缓存复用的 _evt、AesirEventArgsSO 资产内实例）
            // 的 Sender 会被内层（重入）分发覆写，外层遍历期间一律以此快照为准
            var dispatchSender = sender;
            eventArgs.SetSender(dispatchSender);

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

            // 过滤器在循环前取一次引用；无过滤器时为 null，跳过检查保持零开销
            var filters = eventArgs.FilterList;

            // 重入（订阅者回调内同步发布事件）时：性能计时只对顶层分发生效，
            // 避免内层 Restart/Stop 破坏外层计时
            var reentrant = _dispatchDepth > 0;
            var measureExecution = !reentrant && executionMsLimit > 0f;
            if (measureExecution)
            {
                _dispatchStopwatch.Restart();
            }

            // 快照语义：合并两个注册表到迭代缓冲区后再遍历。
            // 回调内退订/注册只修改注册表本身，本趟迭代基于快照执行完毕，
            // 不会出现"退订后续订阅者导致跳过一个"的索引位移。
            // 顶层分发复用 _iterationBuffer（Clear 保留容量，稳态零分配）；
            // 重入分发使用独立局部列表，不覆写顶层正在迭代的缓冲区。
            var sorted = reentrant ? new List<BindingInfo>(totalCount) : _iterationBuffer;
            if (attrList != null)
            {
                sorted.AddRange(attrList);
            }

            if (dynList != null)
            {
                sorted.AddRange(dynList);
            }

            // count <= 1 时跳过排序；Priority 为主键、InsertionIndex 为次键的稳定排序，
            // 同优先级按注册顺序执行（对齐 RAA HookEntry 范式）
            if (sorted.Count > 1)
            {
                sorted.Sort((a, b) =>
                {
                    var res = a.Priority.CompareTo(b.Priority);
                    return res != 0 ? res : a.InsertionIndex.CompareTo(b.InsertionIndex);
                });
            }

            // 重入分发使用独立的参数数组与死绑定收集，避免覆写顶层共享状态
            var invokeArgs = reentrant ? new object[1] : _invokeArgs;
            var deadBindings = reentrant ? new List<BindingInfo>() : _deadBindings;
            invokeArgs[0] = eventArgs;

            _dispatchDepth++;
            var count = sorted.Count;
            try
            {
                for (var i = 0; i < count; i++)
                {
                    var binding = sorted[i];
                    if (AesirEventUtility.IsObjectUnityNull(binding.Subscriber))
                    {
                        // 已销毁订阅者：收集到循环外统一移除，避免遍历时修改注册表列表
                        deadBindings.Add(binding);
                        continue;
                    }

                    // 重入安全：前一个订阅者回调内的内层分发可能已把共享参数实例的 Sender
                    // 覆写为内层发布者。以本趟快照重新校正（引用相等即无操作），
                    // 使过滤器与订阅者读到的始终是外层发布者
                    if (!ReferenceEquals(eventArgs.Sender, dispatchSender))
                    {
                        eventArgs.SetSender(dispatchSender);
                    }

                    try
                    {
                        if (filters != null && !PassFilters(filters, eventArgs, binding))
                        {
                            continue;
                        }

                        binding.Invoke(invokeArgs);
                    }
                    catch (Exception ex)
                    {
                        // 分发路径已无 MethodInfo.Invoke（Attribute 订阅走表达式树委托、
                        // Script 订阅走委托直调），不存在 TargetInvocationException 包装，
                        // 其诊断信息在此统一补齐：订阅者身份 + 事件名 + 内层异常（若有）
                        var inner = ex.InnerException != null
                            ? $"，内层异常：{ex.InnerException.Message}"
                            : string.Empty;
                        AesirModulesDebug.LogError(AesirModulesDebug.EventModuleTag,
                            $"事件分发异常：{ex.Message}" +
                            $"（订阅者 {binding.Subscriber}，事件 " +
                            $"{AesirEventUtility.GetEventName<TEventArgs>()}{inner}）");
                    }
                }
            }
            finally
            {
                _dispatchDepth--;
                if (!reentrant)
                {
                    _iterationBuffer.Clear();
                }
            }

            if (deadBindings.Count > 0)
            {
                RemoveDeadBindings(deadBindings, eventArgs.GetType().Name);
            }

            if (measureExecution)
            {
                _dispatchStopwatch.Stop();
                var elapsedMs = _dispatchStopwatch.Elapsed.TotalMilliseconds;
                if (elapsedMs > executionMsLimit)
                {
                    AesirModulesDebug.LogWarning(AesirModulesDebug.EventModuleTag,
                        $"事件 {eventArgs.GetType().Name} 分发耗时 {elapsedMs:F2}ms（{count} 个订阅者），" +
                        $"超过阈值 {executionMsLimit}ms。");
                }
            }
        }

        /// <summary>
        /// 逐个执行过滤器检查，任一过滤器不通过即拦截该订阅者。
        /// 接收具体 <see cref="List{T}" /> 避免接口枚举装箱分配。
        /// </summary>
        /// <remarks>
        /// 调用方保证 <paramref name="eventArgs" /> 的 <see cref="AesirEventArgs.Sender" />
        /// 已是本趟分发的发布者（遍历中按快照校正），过滤器可直接据此判定。
        /// </remarks>
        static bool PassFilters(List<ISubscriberFilter> filters,
            AesirEventArgs eventArgs,
            BindingInfo binding)
        {
            for (var i = 0; i < filters.Count; i++)
            {
                if (!filters[i].ShouldReceive(eventArgs, binding.Subscriber, binding.Priority))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 从双注册表移除本轮分发收集到的已销毁订阅者绑定，并输出告警提示检查退订遗漏。
        /// 日志为 UNITY_EDITOR 条件调用，玩家构建中静默清理。
        /// </summary>
        /// <param name="deadBindings">本轮分发收集到的死绑定列表，移除后清空（复用列表保留容量）。</param>
        /// <param name="eventName">事件名，用于日志定位。</param>
        void RemoveDeadBindings(List<BindingInfo> deadBindings, string eventName)
        {
            for (var i = 0; i < deadBindings.Count; i++)
            {
                var dead = deadBindings[i];
                RemoveFromRegistry(AttributeBindings, dead);
                RemoveFromRegistry(DynamicBindings, dead);
            }

            AesirModulesDebug.LogWarning(AesirModulesDebug.EventModuleTag,
                $"事件 {eventName}：已清理 {deadBindings.Count} 个已销毁订阅者的绑定，" +
                "请检查是否遗漏退订（建议 OnDisable 中 RemoveListener 或 Dispose 句柄）。");
            deadBindings.Clear();
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

                // 尝试在已加载的场景中查找预放置的实例（含未激活对象：
                // Unity 不对未激活物体调用 Awake，"随用随开"式预放置的模块若被 Exclude 漏掉，
                // 会在这里被判为不存在而重复创建，Inspector 配置值从一开始就被忽略）
                // 使用 FindAnyObjectByType 而非 FindFirstObjectByType，后者因依赖 InstanceID 排序在 Unity 6 中已废弃
                _instance = FindAnyObjectByType<EventModule>(FindObjectsInactive.Include);
                if (_instance != null)
                {
                    return _instance;
                }

                // 未找到预放置实例 → 在 AesirModules 下创建（跟随父级 DDOL）
                _instance = AesirModules.GetOrAddChild<EventModule>();
                return _instance;
            }
        }

        /// <summary>
        /// 非创建式单例获取：实例不存在时返回 <c>false</c>，<b>不触发懒创建</b>。
        /// </summary>
        /// <remarks>
        /// 供 <see cref="InvokeEvent{TEventArgs}" /> 使用：订阅表是实例字段，没有实例就没有订阅者，
        /// 分发无事可做。若走 <see cref="Instance" />，在场景卸载/退出等时机触发"发布事件"会在正在卸载的
        /// 场景里重建 <c>[Aesir Modules]</c> 宿主并加入 DDOL，留下一个没有注册表的泄漏宿主。
        /// 订阅侧（<see cref="AddListener(object)" /> 等）仍需创建语义，不受本方法影响。
        /// </remarks>
        /// <param name="module">找到时输出现有实例；否则输出 <c>null</c></param>
        /// <returns>存在可用实例则返回 <c>true</c></returns>
        internal static bool TryGetExisting(out EventModule module)
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<EventModule>(FindObjectsInactive.Include);
            }

            module = _instance;
            return _instance != null;
        }

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                // 重复实例只销毁自身组件，避免连带销毁宿主整树（对齐 RAA 先例）
                Destroy(this);
                return;
            }

            _instance = this;

            // 非根物体（运行时自动创建于 [Aesir Modules] 宿主下）时 DDOL 跟随宿主，本字段不参与判断
            if (dontDestroyOnLoad && transform.root == transform)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        /// <summary>
        /// 域加载时重置静态单例，兼容关闭 Domain Reload 的 Play 模式设置。
        /// </summary>
        /// <remarks>
        /// 非泛型类按框架铁律在类内声明 <c>[RuntimeInitializeOnLoadMethod]</c> 自重置，
        /// 而非经 <see cref="ResetStaticsAssistant" />（该助手仅服务泛型类——泛型类中的 RIOLM 会被 Unity 静默跳过）。
        /// 仅清空静态引用：既有物体仍留在场景中，下次 <see cref="Instance" /> 访问经
        /// <c>FindAnyObjectByType</c> 兜底重发现。
        /// <para>
        /// 不依赖 Unity fake-null 隐式救援：<c>Instance</c> 入口的 <c>_instance != null</c>
        /// 恰好能识别已销毁对象，但那是运算符重载的副作用而非显式重置，
        /// 与包内铁律（fake-null 隐式重置已废弃）不符。
        /// </para>
        /// </remarks>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            _instance = null;
        }

        #endregion

        #region 注册表

        /// <summary>
        /// Attribute 订阅注册表。以事件类型 AssemblyQualifiedName 为键。
        /// </summary>
        /// <remarks>
        /// internal：注册表是实现细节（订阅/退订必须经公开 API 走同一套绑定键与死引用清理），
        /// 包外不可见；包内测试经 <c>InternalsVisibleTo</c> 访问。
        /// </remarks>
        internal Dictionary<string, List<BindingInfo>> AttributeBindings =
            new Dictionary<string, List<BindingInfo>>();

        /// <summary>
        /// Script 订阅注册表。以事件类型 AssemblyQualifiedName 为键。
        /// </summary>
        /// <remarks>
        /// internal 的理由同 <see cref="AttributeBindings" />。
        /// </remarks>
        internal Dictionary<string, List<BindingInfo>> DynamicBindings =
            new Dictionary<string, List<BindingInfo>>();

        /// <summary>
        /// 复用的参数数组，避免每次分发都分配 object[]。
        /// 仅顶层分发（<see cref="_dispatchDepth" /> 为 0）使用；
        /// 重入分发使用独立局部数组，避免覆写顶层正在使用的参数。
        /// </summary>
        readonly object[] _invokeArgs = new object[1];

        /// <summary>
        /// 分发深度计数。0 = 顶层分发；&gt; 0 = 订阅者回调内同步发布事件（重入）。
        /// 重入分发改用独立的局部迭代缓冲区/参数数组/死绑定收集，不触碰顶层共享状态。
        /// </summary>
        int _dispatchDepth;

        /// <summary>
        /// 顶层分发复用的迭代缓冲区。分发基于注册表快照迭代：
        /// 订阅者回调内退订/注册只改注册表，不影响本趟迭代（本趟仍按快照执行完毕）。
        /// <see cref="List{T}.Clear" /> 保留容量，稳态零分配。
        /// </summary>
        readonly List<BindingInfo> _iterationBuffer = new List<BindingInfo>();

        /// <summary>
        /// 注册顺序自增序号源。注册时分配给 <see cref="BindingInfo.InsertionIndex" />，
        /// 跨双注册表全局递增，使合并排序的次键在全序上有定义。
        /// </summary>
        long _nextInsertionIndex;

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
                            AesirModulesDebug.LogWarning(AesirModulesDebug.EventModuleTag,
                                $"方法 {method.Name}（{subscriber.GetType().Name}）无法确定监听的事件类型。" +
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
                    AesirModulesDebug.LogError(AesirModulesDebug.EventModuleTag,
                        $"绑定 {method.Name}（{subscriber.GetType().Name}）时出错：{e.Message}");
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

        void AddToRegistry(Dictionary<string, List<BindingInfo>> registry, BindingInfo info)
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

            // 注册顺序次键：跨双注册表全局递增，同优先级排序按注册顺序稳定执行
            info.InsertionIndex = _nextInsertionIndex++;
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
