using System;
using System.Collections.Generic;
using System.Diagnostics;
using Runestone.AesirArchitecture;
using UnityEngine;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 事件模块（MonoBehaviour 单例）。
    /// 通过 <see cref="AddListener{TEventArgs}(object, Action{TEventArgs})" /> 建立 Script 订阅，
    /// 通过 <see cref="InvokeEvent{TEventArgs}" /> 分发事件。
    /// <para>
    /// 订阅方持有 <see cref="AutoRemoveListenerHandle" /> 精确退订；分发按 4 档优先级稳定执行
    /// （注册表在注册期维护有序：Priority 升序、同档按注册顺序，分发侧零排序）。
    /// 分发期可靠性：自动检测并清理已销毁的 Unity 对象订阅者（死引用）；
    /// 支持 <see cref="AesirEventArgs.WithFilter" /> 声明的订阅者过滤器实现精确投递；
    /// 可通过 <c>executionMsLimit</c> 开启分发耗时告警；支持 <see cref="AesirEventArgs.Cancel" /> 取消传播。
    /// </para>
    /// <para>
    /// <b>快照语义</b>：每趟分发基于注册表快照迭代——回调内退订/注册只影响后续分发，不干扰本趟。
    /// <b>重入约定</b>：订阅者回调内同步发布<b>其他参数实例</b>（同型或异型）是安全的，
    /// 重入层使用按深度复用的独立迭代缓冲区；<b>同一参数实例</b>在分发期间不得被再次发布
    /// （fail-fast 拒绝并输出错误日志），事件链请使用新实例或经 <c>AesirTimer.NextFrame</c> 延后发布。
    /// 性能计时与诊断采集仅对顶层分发生效。
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
        /// <b>为什么预放置路径同样需要它：</b>本模块的订阅注册表是实例字段，随模块本体一同存亡。
        /// 预放置实例若不加入 DDOL，场景卸载时 <see cref="OnDestroy" /> 会把单例置空、注册表随之丢失，
        /// 跨场景常驻的订阅者再也收不到任何事件，其 <see cref="AutoRemoveListenerHandle" /> 闭包也会捕获已死的实例。
        /// 运行时自动创建的实例挂载在 [Aesir Modules] 宿主下（非根物体），DDOL 跟随宿主决策，
        /// 本字段不参与判断。
        /// </para>
        /// </remarks>
        [SerializeField]
        bool dontDestroyOnLoad = true;

        #region 公开 API — 事件触发

        /// <summary>
        /// 触发事件。取注册表订阅者，按注册期维护的优先级顺序依次调用。
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
                    $"发布者为 null，无法触发事件 {typeof(TEventArgs).Name}。");
                return;
            }

            // 重入约定：同一参数实例在分发期间不得被再次发布。共享实例（AesirEventArgsSO 资产、
            // 用户复用实例）重入会互相覆写 Sender 与分发状态——按低概率场景约定直接拒绝（fail-fast），
            // 不做逐订阅者校正兜底；事件链请使用新实例或延后一帧发布
            if (eventArgs.Dispatching)
            {
                AesirModulesDebug.LogError(AesirModulesDebug.EventModuleTag,
                    $"事件 {eventArgs.GetType().Name}：同一参数实例正在分发中，禁止重入发布同一实例。" +
                    "事件链请使用新的参数实例，或经 AesirTimer.NextFrame 延后发布。");
                return;
            }

            eventArgs.SetSender(sender);

            var eventType = eventArgs.GetType();
            if (!Bindings.TryGetValue(eventType, out var list) || list.Count == 0)
            {
                return;
            }

            // 复用实例不得把上一趟的取消状态与过滤器解析缓存带入本趟。
            // 重入层使用不同参数实例，复位只作用于本实例，不影响外层
            eventArgs.ResetCancellationState();
            eventArgs.ResetDispatchCaches();

            // 重入（订阅者回调内同步发布其他实例）时：性能计时与诊断采集只对顶层生效，
            // 避免内层 Restart/Stop 破坏外层计时
            var reentrant = _dispatchDepth > 0;
            var measureExecution = !reentrant && (executionMsLimit > 0f || AesirEventDiagnostics.Enabled);
            if (measureExecution)
            {
                _dispatchStopwatch.Restart();
            }

            // 快照语义：把订阅列表拷贝到迭代缓冲区后再遍历，回调内退订/注册只修改注册表本身，
            // 不会出现"退订后续订阅者导致跳过一个"的索引位移。
            // 顶层复用 _iterationBuffer；重入层使用按深度复用的独立缓冲区，不覆写外层正在迭代的快照
            // （Clear 保留容量，稳态零分配）。注册表已在注册期有序，此处无需排序
            var buffer = reentrant
                ? GetReentrantBuffer(_dispatchDepth - 1, _reentrantBuffers)
                : _iterationBuffer;
            buffer.AddRange(list);
            var deadBindings = reentrant
                ? GetReentrantBuffer(_dispatchDepth - 1, _reentrantDeadBuffers)
                : _deadBindings;

            // 过滤器在循环前取一次引用；无过滤器时为 null，跳过检查保持零开销
            var filters = eventArgs.FilterList;

            _dispatchDepth++;
            eventArgs.Dispatching = true;
            var count = buffer.Count;
            var invokedCount = 0;
            try
            {
                for (var i = 0; i < count; i++)
                {
                    var binding = buffer[i];
                    if (AesirEventUtility.IsObjectUnityNull(binding.Subscriber))
                    {
                        // 已销毁订阅者：收集到循环外统一移除，避免遍历时修改注册表列表
                        deadBindings.Add(binding);
                        continue;
                    }

                    // 取消传播：First / Last 两档不受取消影响（前置校验与收尾清理始终执行）；
                    // 一旦被取消，High / Medium 档剩余订阅者跳过。有序注册表保证本判断只跳过、不回头
                    if (eventArgs.IsCancelled && binding.Priority != SubscriberPriority.First &&
                        binding.Priority != SubscriberPriority.Last)
                    {
                        continue;
                    }

                    try
                    {
                        if (filters != null && !PassFilters(filters, eventArgs, binding))
                        {
                            continue;
                        }

                        binding.Invoke(eventArgs);
                        invokedCount++;
                    }
                    catch (Exception ex)
                    {
                        // 分发路径只有 Script 订阅的委托直调，不存在 TargetInvocationException 包装，
                        // 其诊断信息在此统一补齐：订阅者身份 + 事件名 + 内层异常（若有）
                        var inner = ex.InnerException != null
                            ? $"，内层异常：{ex.InnerException.Message}"
                            : string.Empty;
                        AesirModulesDebug.LogError(AesirModulesDebug.EventModuleTag,
                            $"事件分发异常：{ex.Message}" + $"（订阅者 {binding.Subscriber}，事件 " +
                            $"{eventType.Name}{inner}）");
                    }
                }
            }
            finally
            {
                _dispatchDepth--;
                eventArgs.Dispatching = false;
                buffer.Clear();
            }

            if (deadBindings.Count > 0)
            {
                RemoveDeadBindings(deadBindings, eventType.Name);
            }

            var elapsedMs = 0f;
            if (measureExecution)
            {
                _dispatchStopwatch.Stop();
                elapsedMs = (float)_dispatchStopwatch.Elapsed.TotalMilliseconds;
                if (executionMsLimit > 0f && elapsedMs > executionMsLimit)
                {
                    AesirModulesDebug.LogWarning(AesirModulesDebug.EventModuleTag,
                        $"事件 {eventType.Name} 分发耗时 {elapsedMs:F2}ms（{count} 个订阅者），" +
                        $"超过阈值 {executionMsLimit}ms。");
                }
            }

            // 诊断采集只对顶层分发生效；门面内部有 Enabled 快速判断，关闭时零构造。
            // 事件类型取运行期实际类型而非 typeof(TEventArgs)：非泛型 Invoke()/Invoke(sender)
            // 会把编译期参数推断为基类 AesirEventArgs，用泛型参数会误报为基类
            if (!reentrant && AesirEventDiagnostics.Enabled)
            {
                AesirEventDiagnostics.RecordRaised(eventArgs, eventType, invokedCount, count, elapsedMs);
            }
        }

        /// <summary>
        /// 逐个执行过滤器检查，任一过滤器不通过即拦截该订阅者。
        /// 接收具体 <see cref="List{T}" /> 避免接口枚举装箱分配。
        /// </summary>
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
        /// 从注册表移除本轮分发收集到的已销毁订阅者绑定，并输出告警提示检查退订遗漏。
        /// 日志为 UNITY_EDITOR 条件调用，玩家构建中静默清理。
        /// </summary>
        /// <param name="deadBindings">本轮分发收集到的死绑定列表，移除后清空（复用列表保留容量）。</param>
        /// <param name="eventName">事件名，用于日志定位。</param>
        void RemoveDeadBindings(List<BindingInfo> deadBindings, string eventName)
        {
            for (var i = 0; i < deadBindings.Count; i++)
            {
                RemoveFromRegistry(deadBindings[i]);
            }

            AesirModulesDebug.LogWarning(AesirModulesDebug.EventModuleTag,
                $"事件 {eventName}：已清理 {deadBindings.Count} 个已销毁订阅者的绑定，" +
                "请检查是否遗漏退订（建议 OnDisable 中 Dispose 句柄）。");
            deadBindings.Clear();
        }

        /// <summary>
        /// 取指定深度的重入缓冲区（首次到达该深度时创建，之后复用；调用方用毕 Clear）。
        /// 深度以 0 为第一层重入，池随最大嵌套深度增长一次后稳态零分配。
        /// </summary>
        static List<BindingInfo> GetReentrantBuffer(int depthIndex, List<List<BindingInfo>> pool)
        {
            while (pool.Count <= depthIndex)
            {
                pool.Add(new List<BindingInfo>());
            }

            return pool[depthIndex];
        }

        #endregion

        #region 单例

        static EventModule _instance;

        /// <summary>
        /// 实例缺失期是否已做过一次全场景查找。<see cref="TryGetExisting" /> 的负缓存：
        /// 没有实例就没有订阅者，查找至多一次，之后直接走"无事可做"快速路径。
        /// </summary>
        static bool _instanceSearched;

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
        /// <para>
        /// 查找带负缓存：每个"实例缺失期"至多一次 <c>FindAnyObjectByType</c> 全场景扫描——
        /// 不使用事件模块的工程在退出时机发布事件时，不应每次发布都付一次全对象扫描。
        /// 新实例经 <see cref="Awake" /> 主动写入单例，不依赖查找，负缓存不会漏掉后出现的实例。
        /// </para>
        /// 订阅侧（<see cref="AddListener{TEventArgs}(object, Action{TEventArgs})" /> 等）仍需创建语义，不受本方法影响。
        /// </remarks>
        /// <param name="module">找到时输出现有实例；否则输出 <c>null</c></param>
        /// <returns>存在可用实例则返回 <c>true</c></returns>
        internal static bool TryGetExisting(out EventModule module)
        {
            if (_instance == null && !_instanceSearched)
            {
                _instanceSearched = true;
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
            _instanceSearched = true;

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
                // 允许下一个"实例缺失期"重新查找一次（如后续场景另有预放置实例）
                _instanceSearched = false;
            }
        }

        /// <summary>
        /// 域加载时重置静态单例，兼容关闭 Domain Reload 的 Play 模式设置。
        /// </summary>
        /// <remarks>
        /// 非泛型类按框架铁律在类内声明 <c>[RuntimeInitializeOnLoadMethod]</c> 自重置，
        /// 而非经 <c>ResetStaticsAssistant</c>（该助手仅服务泛型类——泛型类中的 RIOLM 会被 Unity 静默跳过）。
        /// 仅清空静态引用：既有物体仍留在场景中，下次 <see cref="Instance" /> 访问经
        /// <c>FindAnyObjectByType</c> 兜底重发现。
        /// </remarks>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            _instance = null;
            _instanceSearched = false;
        }

        #endregion

        #region 注册表

        /// <summary>
        /// Script 订阅注册表。以事件参数类型为键，列表在注册期维护有序
        /// （Priority 升序、同档按注册顺序），分发侧直接快照迭代、零排序。
        /// <para>
        /// internal：注册表是实现细节（订阅/退订必须经公开 API 走同一套注册与死引用清理），
        /// 包外不可见；包内测试经 <c>InternalsVisibleTo</c> 访问。
        /// </para>
        /// </summary>
        internal readonly Dictionary<Type, List<BindingInfo>> Bindings =
            new Dictionary<Type, List<BindingInfo>>();

        /// <summary>
        /// 顶层分发复用的迭代缓冲区。分发基于注册表快照迭代：
        /// 订阅者回调内退订/注册只改注册表，不影响本趟迭代（本趟仍按快照执行完毕）。
        /// <see cref="List{T}.Clear" /> 保留容量，稳态零分配。
        /// </summary>
        readonly List<BindingInfo> _iterationBuffer = new List<BindingInfo>();

        /// <summary>
        /// 顶层分发收集到的死绑定（订阅者已销毁）。循环外统一移除，避免遍历时修改注册表列表；
        /// 复用实例列表保持稳态零分配。
        /// </summary>
        readonly List<BindingInfo> _deadBindings = new List<BindingInfo>();

        /// <summary>
        /// 重入分发的迭代缓冲区池，按深度索引复用（第一层重入用 [0]）。
        /// 重入层不覆写外层正在迭代的快照；池随最大嵌套深度增长一次后稳态零分配。
        /// </summary>
        readonly List<List<BindingInfo>> _reentrantBuffers = new List<List<BindingInfo>>();

        /// <summary>重入分发的死绑定收集池，按深度索引复用，语义同 <see cref="_reentrantBuffers" />。</summary>
        readonly List<List<BindingInfo>> _reentrantDeadBuffers = new List<List<BindingInfo>>();

        /// <summary>
        /// 分发深度计数。0 = 顶层分发；&gt; 0 = 订阅者回调内同步发布其他参数实例（重入）。
        /// 重入分发改用按深度复用的独立缓冲区，不触碰顶层共享状态。
        /// </summary>
        int _dispatchDepth;

        #endregion

        #region 公开 API — Script 订阅

        /// <summary>
        /// 添加 Script 订阅。通过 Lambda 委托监听指定事件类型，返回自动移除句柄。
        /// <see cref="AutoRemoveListenerHandle.Dispose" /> 或 using 块结束时自动注销。
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
        /// 添加 Script 订阅（非泛型版）。通过事件参数实例推断类型。
        /// 默认优先级 <see cref="SubscriberPriority.Medium" />。
        /// </summary>
        public static AutoRemoveListenerHandle AddListener(object subscriber,
            AesirEventArgs eventArgs,
            Action<AesirEventArgs> callback) =>
            Instance.AddDynamicBinding(subscriber, eventArgs, callback, SubscriberPriority.Medium);

        /// <summary>
        /// 添加 Script 订阅（非泛型版），指定优先级。
        /// </summary>
        public static AutoRemoveListenerHandle AddListener(object subscriber,
            AesirEventArgs eventArgs,
            SubscriberPriority priority,
            Action<AesirEventArgs> callback) =>
            Instance.AddDynamicBinding(subscriber, eventArgs, callback, priority);

        /// <summary>
        /// 移除指定对象对指定事件类型的全部 Script 订阅（O(1) 查表 + O(k) 移除）。
        /// <para>
        /// 用于退订方无句柄的场景：诊断工具强制解绑、外部临时屏蔽某对象的某事件。
        /// 首选退订路径仍是 <see cref="AutoRemoveListenerHandle.Dispose" />（持有句柄时精确移除）。
        /// </para>
        /// </summary>
        public static void RemoveListener<TEventArgs>(object subscriber) where TEventArgs : AesirEventArgs
        {
            if (subscriber == null)
            {
                return;
            }

            Instance.RemoveSubscriber(subscriber, typeof(TEventArgs));
        }

        /// <summary>
        /// 移除指定对象的全部 Script 订阅（O(n) 冷路径，批量逃生舱）。
        /// <para>
        /// 用于退订方无句柄、且需要一次性清空某对象全部订阅的场景。
        /// 持有句柄时请逐个 <see cref="AutoRemoveListenerHandle.Dispose" />；只需退订单个事件时用
        /// <see cref="RemoveListener{TEventArgs}(object)" />。
        /// </para>
        /// </summary>
        public static void RemoveListener(object subscriber)
        {
            if (subscriber == null)
            {
                return;
            }

            Instance.RemoveSubscriber(subscriber, null);
        }

        #endregion

        #region Script 订阅注册

        AutoRemoveListenerHandle AddDynamicBinding<TEventArgs>(object subscriber,
            Action<TEventArgs> callback,
            SubscriberPriority priority) where TEventArgs : AesirEventArgs
        {
            var info = new ScriptBindingInfo<TEventArgs>(callback, priority, subscriber, typeof(TEventArgs));
            AddToRegistry(info);
            return new AutoRemoveListenerHandle(() => RemoveFromRegistry(info));
        }

        AutoRemoveListenerHandle AddDynamicBinding(object subscriber,
            AesirEventArgs eventArgs,
            Action<AesirEventArgs> callback,
            SubscriberPriority priority)
        {
            if (eventArgs == null)
            {
                throw new ArgumentNullException(nameof(eventArgs));
            }

            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            // 注册表键 = 参数实例的运行期类型；分发按同一键精确匹配，
            // 基类回调可直接接收，无需反射适配
            var info = new ScriptBindingInfo<AesirEventArgs>(callback, priority, subscriber,
                eventArgs.GetType());
            AddToRegistry(info);
            return new AutoRemoveListenerHandle(() => RemoveFromRegistry(info));
        }

        #endregion

        #region 注册表通用操作

        void AddToRegistry(BindingInfo info)
        {
            if (!Bindings.TryGetValue(info.EventType, out var list))
            {
                list = new List<BindingInfo>();
                Bindings[info.EventType] = list;
            }

            // 有序插入：Priority 升序、同档按注册顺序（新绑定插到本档末尾）。
            // 注册是冷路径、分发是热路径——顺序在这里维护，分发侧零排序零比较
            var index = list.Count;
            while (index > 0 && list[index - 1].Priority > info.Priority)
            {
                index--;
            }

            list.Insert(index, info);

            if (AesirEventDiagnostics.Enabled)
            {
                AesirEventDiagnostics.RecordBound(info);
            }
        }

        void RemoveFromRegistry(BindingInfo info)
        {
            if (Bindings.TryGetValue(info.EventType, out var list))
            {
                list.Remove(info);
                if (list.Count == 0)
                {
                    Bindings.Remove(info.EventType);
                }
            }

            if (AesirEventDiagnostics.Enabled)
            {
                AesirEventDiagnostics.RecordUnbound(info);
            }
        }

        /// <summary>
        /// 移除指定订阅者的绑定。<paramref name="eventType" /> 为 null 时移除该对象的全部订阅。
        /// </summary>
        void RemoveSubscriber(object subscriber, Type eventType)
        {
            if (eventType != null)
            {
                RemoveSubscriberFromList(eventType, subscriber);
                return;
            }

            // 全键：快照键集（遍历中可能删键，不能直接迭代 Keys），单次 CopyTo 不做双重拷贝
            var keys = new Type[Bindings.Count];
            Bindings.Keys.CopyTo(keys, 0);
            foreach (var key in keys)
            {
                RemoveSubscriberFromList(key, subscriber);
            }
        }

        void RemoveSubscriberFromList(Type eventType, object subscriber)
        {
            if (!Bindings.TryGetValue(eventType, out var list))
            {
                return;
            }

            // 诊断开启时先记录将被移除的绑定（RemoveAll 之后原列表元素不再可取）
            if (AesirEventDiagnostics.Enabled)
            {
                for (var i = 0; i < list.Count; i++)
                {
                    if (ReferenceEquals(list[i].Subscriber, subscriber))
                    {
                        AesirEventDiagnostics.RecordUnbound(list[i]);
                    }
                }
            }

            list.RemoveAll(b => ReferenceEquals(b.Subscriber, subscriber));
            if (list.Count == 0)
            {
                Bindings.Remove(eventType);
            }
        }

        #endregion
    }
}
