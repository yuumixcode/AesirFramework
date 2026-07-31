# Sample 开发计划(最终版)

> **说明**:为 V1-V8 每个版本定义验证用 Sample。每版一个 Sample 场景(部分版本 2 个),复杂度由 ★1 逐步递增到 ★4,验证目标严格对应版本的"核心能力"。

---

## 1. 总体 Sample 路线图

| 版本 | Sample 场景 | 核心验证点 | 复杂度 |
|---|---|---|---|
| V1 | `S1_KeyPress.unity` | 基本发布 + 静态订阅 | ★ |
| V2 | `S2_Priority.unity` | 5 档 Priority + StopPropagation + 动态订阅 + PublishDelayed | ★★ |
| V3 | `S3_Filter.unity` | 6 种过滤器 + DefaultChannel | ★★ |
| V4 | `S4_SystemEvent.unity` | 4 个 SystemEvent + 性能告警 + 单例 | ★★★ |
| V5 | `S5_SoAndBridge.unity` | GameEventSO + SubclassSelector + UnityEvent 桥接 | ★★★ |
| V6 | `S6_Animator.unity` | PublishOnPlayback + 反射注入 Animator | ★★★ |
| V7 | `S7_Tools.unity` | 5 个工具窗口(实时监控 + 过滤 + Ping) | ★★★★ |
| V8 | `S8_DocsAndMenu.unity` | Welcome 引导 + 文档窗口 + Hierarchy 菜单 | ★★ |

---

## 2. 各版本 Sample 详细设计

### V1 Sample — `S1_KeyPress`(MVP 验证)

**位置**:`Assets/Samples/01_KeyPress/`

**最终目标**:验证 V1 核心能力(基本发布 + 静态订阅)。

#### 2.1 涉及场景
- `S1_KeyPress.unity` — 单一场景,3 个 GameObject:
  - `GameEventHub`(挂 `GameEventHub` 组件)
  - `KeyPressEmitter`(挂 `EventEmitter.cs`)
  - `KeyPressSubscriber`(挂 `KeyPressSubscriber.cs`)

#### 2.2 涉及脚本

**OnKeyPressed.cs** — 业务事件
```csharp
namespace CriminalMakers.GameEventHub.Samples.S1
{
    public class OnKeyPressed : GameEvent
    {
    }
}
```

**EventEmitter.cs** — 按键发布
```csharp
namespace CriminalMakers.GameEventHub.Samples.S1
{
    [AddComponentMenu("")]
    public class EventEmitter : MonoBehaviour
    {
        public KeyCode triggerKey = KeyCode.Space;
        void Update()
        {
            if (Input.GetKeyDown(triggerKey))
                new OnKeyPressed().Publish(this);
        }
    }
}
```

**KeyPressSubscriber.cs** — 静态订阅
```csharp
namespace CriminalMakers.GameEventHub.Samples.S1
{
    [AddComponentMenu("")]
    public class KeyPressSubscriber : MonoBehaviour
    {
        void OnEnable() => GameEventHub.Bind(this);
        void OnDisable() => GameEventHub.Unbind(this);

        [OnGameEvent]
        private void OnKeyPressed(OnKeyPressed e)
        {
            Debug.Log($"[{name}] Received OnKeyPressed");
        }
    }
}
```

#### 2.3 验证步骤

| # | 操作 | 预期 |
|---|---|---|
| 1 | 进入 Play | Console 无错误 |
| 2 | 按 Space | Console 输出 `[KeyPressSubscriber] Received OnKeyPressed` |
| 3 | 复制 Subscriber,再按 Space | 2 条日志 |
| 4 | 禁用 Subscriber,按 Space | 1 条日志 |
| 5 | 启用,按 Space | 2 条日志 |
| 6 | 运行时 Destroy Subscriber,按 Space | 无 NullReferenceException |

---

### V2 Sample — `S2_Priority`(优先级 + 取消 + 动态 + 延迟)

**位置**:`Assets/Samples/02_Priority/`

**最终目标**:验证 V2 核心能力(5 档 Priority + 链式 API + StopPropagation + 动态订阅 + PublishDelayed)。

#### 2.1 涉及场景
- `S2_Priority.unity` — 单场景,8 个 GameObject:
  - `GameEventHub`
  - `PriorityEmitter`(挂 `PriorityEmitter.cs`)
  - 5 个订阅者:`EssentialLogger` / `HighCanceller` / `MediumLogger` / `LowLogger` / `CleanupLogger`
  - `DynamicSubscriber`(挂 `DynamicSubscriber.cs`)
  - `DelayedEmitter`(挂 `DelayedEmitter.cs`)

#### 2.2 涉及脚本

**PriorityEmitter.cs** — 普通发布
```csharp
[AddComponentMenu("")]
public class PriorityEmitter : MonoBehaviour
{
    public KeyCode triggerKey = KeyCode.Space;
    void Update()
    {
        if (Input.GetKeyDown(triggerKey))
            new OnKeyPressed().Publish(this);
    }
}
```

**5 个 Logger**:
```csharp
// EssentialLogger.cs
[OnGameEvent(SubscriberPriority.Essential)]
private void OnKeyPressed(OnKeyPressed e) => Debug.Log("Essential");

// HighCanceller.cs
[OnGameEvent(SubscriberPriority.High)]
private void OnKeyPressed(OnKeyPressed e)
{
    Debug.Log("High (will cancel)");
    e.StopPropagation(this);  // 取消后续 Medium/Low
}

// MediumLogger.cs
[OnGameEvent(SubscriberPriority.Medium)]
private void OnKeyPressed(OnKeyPressed e) => Debug.Log("Medium");

// LowLogger.cs
[OnGameEvent(SubscriberPriority.Low)]
private void OnKeyPressed(OnKeyPressed e) => Debug.Log("Low");

// CleanupLogger.cs
[OnGameEvent(SubscriberPriority.Cleanup)]
private void OnKeyPressed(OnKeyPressed e) => Debug.Log("Cleanup");
```

**DynamicSubscriber.cs** — 协程 Listen/unsub 切换
```csharp
[AddComponentMenu("")]
public class DynamicSubscriber : MonoBehaviour
{
    private IEnumerator Start()
    {
        while (true)
        {
            var unsub = GameEventHub.Listen(this, (OnKeyPressed e) => Debug.Log("Dynamic"));
            yield return new WaitForSeconds(3);
            unsub();
            yield return new WaitForSeconds(3);
        }
    }
}
```

**DelayedEmitter.cs** — 延迟发布
```csharp
[AddComponentMenu("")]
public class DelayedEmitter : MonoBehaviour
{
    public KeyCode triggerKey = KeyCode.D;
    public float delay = 2f;
    void Update()
    {
        if (Input.GetKeyDown(triggerKey))
            new OnKeyPressed().PublishDelayed(this, delay);
    }
}
```

#### 2.3 验证步骤

| # | 操作 | 预期 |
|---|---|---|
| 1 | 禁用 HighCanceller,按 Space | 顺序:Essential → High → Medium → Low → Cleanup |
| 2 | 启用 HighCanceller,按 Space | 顺序:Essential → High(后续被取消) |
| 3 | 启用 HighCanceller + CleanupLogger,按 Space | 顺序:Essential → High → Cleanup(Medium/Low 跳过) |
| 4 | 启动后等几秒,看 DynamicSubscriber 状态 | 偶数秒 Dynamic 出现,奇数秒不出现 |
| 5 | 按 D 键 | 2 秒后日志出现 |
| 6 | 按 D 键多次 | 多次延迟事件按时间顺序触发 |
| 7 | 用 Shared 测试 | 改 PriorityEmitter 用 `new OnKeyPressed().Shared().Publish(this)`,2 个改 counter 的订阅者 | 共享 |

---

### V3 Sample — `S3_Filter`(过滤器)

**位置**:`Assets/Samples/03_Filter/`

**最终目标**:验证 V3 核心能力(6 种过滤器 + DefaultChannel + Essential 绕过)。

#### 2.1 涉及场景
- `S3_Filter.unity`:
  - `GameEventHub`
  - `Bus`(挂 `Bus.cs`,带 `Collider2D zone` BoxCollider2D trigger)
  - 区域内:`PassengerIn1`, `PassengerIn2`(挂 `Passenger.cs`)
  - 区域外:`PassengerOut`(挂 `Passenger.cs`)
  - `BusHonkPlayer`(挂在 Bus 上,挂 `BusHonkPlayer.cs`)
  - 2 个 `TaggedSubscriber`:`TaggedPlayer`(Tag=Player)/ `TaggedEnemy`(Tag=Enemy)

#### 2.2 涉及脚本

**OnBusHonk.cs**:
```csharp
[DefaultChannel("Bus")]
public class OnBusHonk : GameEvent {}
```

**Bus.cs**:
```csharp
[AddComponentMenu("")]
public class Bus : MonoBehaviour
{
    public float speed = 5f;
    public Collider2D zoneToEmitEvent2D;
    public KeyCode honkKey = KeyCode.Space;

    void Update()
    {
        // 移动控制
        if (Input.GetKey(KeyCode.A)) transform.position += Vector3.left * (speed * Time.deltaTime);
        if (Input.GetKey(KeyCode.D)) transform.position += Vector3.right * (speed * Time.deltaTime);

        // 鸣笛,带过滤器
        if (Input.GetKeyDown(honkKey))
        {
            new OnBusHonk()
                .WithFilter(new InsideCollider2D(zoneToEmitEvent2D))
                .Publish(this);
        }
    }
}
```

**Passenger.cs**:
```csharp
[AddComponentMenu("")]
public class Passenger : MonoBehaviour
{
    public TextMeshProUGUI actionBubbleText;
    void OnEnable() => GameEventHub.Bind(this);
    void OnDisable() => GameEventHub.Unbind(this);

    [OnGameEvent]
    private void OnBusHonk(OnBusHonk e)
    {
        actionBubbleText.text = "Boarding the bus";
        Destroy(gameObject, 3f);
    }
}
```

**BusHonkPlayer.cs** — Essential 绕过过滤:
```csharp
[AddComponentMenu("")]
public class BusHonkPlayer : MonoBehaviour
{
    public AudioSource audioSource;
    void OnEnable() => GameEventHub.Bind(this);
    void OnDisable() => GameEventHub.Unbind(this);

    [OnGameEvent(SubscriberPriority.Essential)]
    private void OnBusHonk(OnBusHonk e) => audioSource.Play();
}
```

**TaggedSubscriber.cs**:
```csharp
[AddComponentMenu("")]
public class TaggedSubscriber : MonoBehaviour
{
    public string targetTag = "Player";
    void OnEnable() => GameEventHub.Bind(this);
    void OnDisable() => GameEventHub.Unbind(this);

    [OnGameEvent]
    private void OnBusHonk(OnBusHonk e)
    {
        if (gameObject.CompareTag(targetTag))
            Debug.Log($"[Tagged {targetTag}] Got OnBusHonk");
    }
}
```

#### 2.3 验证步骤

| # | 操作 | 预期 |
|---|---|---|
| 1 | 启动,Bus 在 PassengerIn1 旁,按 Space | 区域内 2 个 Passenger "Boarding",BusHonkPlayer 播放 |
| 2 | Bus 远离 PassengerOut,按 Space | PassengerOut 不响应 |
| 3 | BusHonkPlayer 始终播放 | 即使 Bus 不在 Passenger 旁(因为 Essential 绕过) |
| 4 | 改 Bus 用 `WithFilter(new WithTag("Player"))` | 只有 TaggedPlayer 响应 |
| 5 | 改 Bus 用 `WithFilter(new OnlySelf(includeChildren: true))` | Bus + 子节点都响应 |
| 6 | Bus 用 `WithFilter(new WithPriority(SubscriberPriority.Medium))` | 只有标 Medium 的响应 |
| 7 | 跨场景测试 | 切到另一场景的 Bus,只有同场景订阅者响应(SameSceneAsEmitter) |
| 8 | 测试 DefaultChannel | 不显式 SetChannel,事件 _channel="Bus" |
| 9 | 故意抛异常 | 自定义 filter throw,业务订阅者仍正常 |

---

### V4 Sample — `S4_SystemEvent`(系统事件 + 可靠性)

**位置**:`Assets/Samples/04_SystemEvent/`

**最终目标**:验证 V4 核心能力(4 个 SystemEvent + 单例合并 + 性能告警 + 死引用清理)。

#### 2.1 涉及场景
- `S4_SystemEvent.unity`:
  - `GameEventHub`(TriggerEventOnStart = true)
  - `SystemLogger`(挂 `SystemLogger.cs`)
  - `Emitter`(挂 `EventEmitter.cs`)
  - `SlowSubscriber`(挂 `SlowSubscriber.cs`)
  - `SubscriberToKill`(挂 `SubscriberToKill.cs`,运行时销毁)
  - `DuplicateHubEmitter`(挂 `DuplicateHubEmitter.cs`)

#### 2.2 涉及脚本

**SystemLogger.cs** — 监听 4 个系统事件:
```csharp
[AddComponentMenu("")]
public class SystemLogger : MonoBehaviour
{
    void OnEnable() => GameEventHub.Bind(this);
    void OnDisable() => GameEventHub.Unbind(this);

    [OnGameEvent(SubscriberPriority.Essential)]
    private void OnStarted(OnEventSystemStarted e) => Debug.Log("[System] Started!");

    [OnGameEvent(SubscriberPriority.Essential)]
    private void OnBound(OnObjectBoundToEventSystem e)
        => Debug.Log($"[System] Bound: {e.BoundObject.GetType().Name} (static={e.isStatic})");

    [OnGameEvent(SubscriberPriority.Essential)]
    private void OnUnbound(OnObjectUnboundFromEventSystem e)
        => Debug.Log($"[System] Unbound: {e.unboundObject.GetType().Name}");

    [OnGameEvent(SubscriberPriority.Essential)]
    private void OnRaised(OnEventRaised e)
        => Debug.Log($"[System] Raised: {e.EventRaised.GetType().Name} ({e.SubscribersCalledString})");
}
```

**SlowSubscriber.cs** — 触发性能告警:
```csharp
[AddComponentMenu("")]
public class SlowSubscriber : MonoBehaviour
{
    void OnEnable() => GameEventHub.Bind(this);
    void OnDisable() => GameEventHub.Unbind(this);

    [OnGameEvent]
    private void OnKeyPressed(OnKeyPressed e)
    {
        System.Threading.Thread.Sleep(60);  // 60ms > 50ms 阈值
    }
}
```

**SubscriberToKill.cs**:
```csharp
[AddComponentMenu("")]
public class SubscriberToKill : MonoBehaviour
{
    public KeyCode killKey = KeyCode.K;
    void OnEnable() => GameEventHub.Bind(this);
    void OnDisable() => GameEventHub.Unbind(this);

    [OnGameEvent]
    private void OnKeyPressed(OnKeyPressed e) => Debug.Log("ToKill received");

    void Update()
    {
        if (Input.GetKeyDown(killKey)) Destroy(gameObject);
    }
}
```

**DuplicateHubEmitter.cs** — 运行时新建 Hub:
```csharp
[AddComponentMenu("")]
public class DuplicateHubEmitter : MonoBehaviour
{
    public KeyCode dupKey = KeyCode.H;
    void Update()
    {
        if (Input.GetKeyDown(dupKey))
        {
            var go = new GameObject("Duplicate Hub");
            go.AddComponent<GameEventHub>();
        }
    }
}
```

#### 2.3 验证步骤

| # | 操作 | 预期 |
|---|---|---|
| 1 | 进入 Play | Console 立即输出 "[System] Started!" |
| 2 | 看 SystemLogger | 多个 "[System] Bound: XXX (static=True)" |
| 3 | 按 Space | "[System] Raised: OnKeyPressed (X/Y subscriber(s) called)" |
| 4 | 按 K 键销毁 SubscriberToKill | "[System] Unbound: SubscriberToKill" |
| 5 | 再按 Space | Console Warning "A null BindingInfo... has been removed" |
| 6 | 重复 Destroy + Publish | 反复 Warning(预期行为) |
| 7 | 按 Space(有 SlowSubscriber) | Console Warning "took 60.x ms" |
| 8 | 按 H 键 | Console Warning "Game Event Hub already exists. Refusing to create another one." |
| 9 | 取消 EnsureSingleInstance | 按 H 不再警告(创建共存) |

---

### V5 Sample — `S5_SoAndBridge`(SO 资产化)

**位置**:`Assets/Samples/05_SoAndBridge/`

**最终目标**:验证 V5 核心能力(GameEventSO + SubclassSelector + UnityEvent 桥接 + Project 右键)。

#### 2.1 涉及场景
- `S5_SoAndBridge.unity`:
  - `GameEventHub`
  - `SoEventEmitter`(挂 `SoEventEmitter.cs`,字段 `[SerializeField] GameEventSO eventSo`)
  - `TraditionalSubscriber`(挂 `TraditionalSubscriber.cs`)
  - `BridgeSubscriber`(挂 `UnityEventOnGameEvent`)

#### 2.2 涉及脚本

**OnKeyPressed.cs / OnCounter.cs / OnHealthChange.cs**:
```csharp
public class OnKeyPressed : GameEvent {}
public class OnCounter : GameEvent { public int counter; }
public class OnHealthChange : GameEvent {}
```

**SoEventEmitter.cs**:
```csharp
[AddComponentMenu("")]
public class SoEventEmitter : MonoBehaviour
{
    [SerializeField] private GameEventSO eventSo;
    public KeyCode triggerKey = KeyCode.Space;
    void Update()
    {
        if (Input.GetKeyDown(triggerKey)) eventSo.Publish(this);
    }
}
```

**TraditionalSubscriber.cs**:
```csharp
[AddComponentMenu("")]
public class TraditionalSubscriber : MonoBehaviour
{
    void OnEnable() => GameEventHub.Bind(this);
    void OnDisable() => GameEventHub.Unbind(this);
    [OnGameEvent] private void OnKeyPressed(OnKeyPressed e) => Debug.Log("Traditional");
    [OnGameEvent] private void OnCounter(OnCounter e) => Debug.Log($"Counter: {e.counter++}");
}
```

#### 2.3 涉及资产
- `Assets/Samples/05_SoAndBridge/Events/OnKeyPressed.asset`(GameEvent: OnKeyPressed, shared=false, channel=)
- `Assets/Samples/05_SoAndBridge/Events/OnCounter.asset`(GameEvent: OnCounter, shared=true, channel=)
- `Assets/Samples/05_SoAndBridge/Events/OnHealthChange.asset`(GameEvent: OnHealthChange, nonCancellable=true, channel=)

#### 2.4 验证步骤

| # | 操作 | 预期 |
|---|---|---|
| 1 | Project 右键 → Create > Game Event Hub > Game Event ScriptableObject | 生成 .asset |
| 2 | 选中 .asset,Inspector 选 OnKeyPressed | 字段下拉切换,内联属性显示 |
| 3 | 验证 ExcludeSubclassSelector | 系统事件不在候选 |
| 4 | 选中 .asset,点 "Publish" 按钮 | 业务订阅者收到 |
| 5 | SoEventEmitter 拖入 OnKeyPressed.asset,按 Space | Traditional 收到 |
| 6 | 拖入 OnCounter.asset(shared=true),2 个改 counter | 都看到最终值 |
| 7 | 拖入 OnHealthChange.asset(nonCancellable=true),订阅者调 StopPropagation | Console 警告,继续传播 |
| 8 | 在 SO Inspector 配 WithTag("Player") 过滤 | 只有 Tag=Player 收到 |
| 9 | simulateEmitter 拖入 Transform | 事件 _emitter 是 Transform |
| 10 | BridgeSubscriber 配 UnityEvent 到 Debug.Log,按 Space | 同时输出 "Traditional" 和 UnityEvent 触发的 Debug.Log |
| 11 | 禁用 BridgeSubscriber,按 Space | 只 "Traditional" |

---

### V6 Sample — `S6_Animator`(Animator 集成)

**位置**:`Assets/Samples/06_Animator/`

**最终目标**:验证 V6 核心能力(PublishOnPlayback + 反射注入 Animator + Editor 动画预览)。

#### 2.1 涉及场景
- `S6_Animator.unity`:
  - `GameEventHub`
  - `Player` GameObject(挂 `WalkController` + `Animator` + Animator Controller 资产)
  - 2 个 `StepDisplayer`:`LeftFoot` (leftStep=true) / `RightFoot` (leftStep=false)
  - `StepCounter`(TextMeshPro)
  - `StepAnnouncer`(TextMeshPro)
  - `SpeedDisplayer`(TextMeshPro)

#### 2.2 涉及脚本

**OnStep.cs**:
```csharp
public class OnStep : GameEvent
{
    public Animator animator;       // 反射注入
    public bool isLeftFoot;
    public OnStep() {}
    public OnStep(bool isLeft) { isLeftFoot = isLeft; }
}
```

**WalkController.cs**:
```csharp
[AddComponentMenu("")]
public class WalkController : MonoBehaviour
{
    public Animator animator;
    public KeyCode walkKey = KeyCode.Space;
    void Update()
    {
        if (Input.GetKeyDown(walkKey)) animator.CrossFade("Walking", 0.2f);
        if (Input.GetKeyUp(walkKey)) animator.CrossFade("Idle", 0.2f);
        animator.SetFloat("speed", Random.Range(0.5f, 1.5f));
    }
}
```

**StepDisplayer.cs**:
```csharp
public class StepDisplayer : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public bool leftStep = false;
    void OnEnable() { Hide(); GameEventHub.Bind(this); }
    void OnDisable() => GameEventHub.Unbind(this);

    [OnGameEvent]
    public void OnStep(OnStep e)
    {
        if (e.isLeftFoot != leftStep) return;
        meshRenderer.enabled = true;
        Invoke(nameof(Hide), 0.1f);
    }
    private void Hide() => meshRenderer.enabled = false;
}
```

**StepCounter.cs / StepAnnouncer.cs / SpeedDisplayer.cs**:
```csharp
// 都在 [OnGameEvent(Essential)] 监听,只接受 Player 的事件
[OnGameEvent(SubscriberPriority.Essential)]
private void OnPlayerStep(OnStep e)
{
    if (e._emitter is not Component comp) return;
    if (!comp.gameObject.CompareTag("Player")) return;
    // ... 业务逻辑
}
```

#### 2.3 涉及资产
- `Assets/Animations/PlayerWalk.anim` — 0.5s 走路动画
- `Assets/Animator/WalkController.controller`:
  - 2 个 state:Idle / Walking
  - Walking 挂 `PublishOnPlayback` + 2 个 PlaybackEvent:
    - `time=0.0` → OnStep_Left.asset
    - `time=0.5` → OnStep_Right.asset
- `Assets/Samples/06_Animator/Events/OnStep_Left.asset` 和 `OnStep_Right.asset`

#### 2.4 验证步骤

| # | 操作 | 预期 |
|---|---|---|
| 1 | 进入 Play | Idle 状态 |
| 2 | 按 Space 切到 Walking | 走 1 个循环 |
| 3 | 看 StepCounter | 显示 "Step count: 2" |
| 4 | 看 StepAnnouncer | 闪烁显示 "Step left performed!" / "Step right performed!" |
| 5 | 看 StepDisplayer(Left) | 0.1s 闪现 |
| 6 | 看 StepDisplayer(Right) | 0.1s 闪现(交替) |
| 7 | 看 SpeedDisplayer | 显示 "Speed parameter: <随机数>" |
| 8 | Editor 模式选中 Walking state | PublishOnPlaybackEditor 渲染 |
| 9 | 拖动 PlaybackEvent 的 time Slider | Scene 视图 GameObject 姿态实时变化 |
| 10 | Editor 中按 "Create & Add" 按钮 | 弹 SaveFilePanel,生成 SO 并加入列表 |

---

### V7 Sample — `S7_Tools`(编辑器工具)

**位置**:`Assets/Samples/07_Tools/`

**最终目标**:验证 V7 核心能力(5 个工具窗口 + EditorPrefs 持久化 + GameEventHubEditor)。

#### 2.1 涉及场景
- `S7_Tools.unity`:
  - `GameEventHub`
  - 多种 Emitter:
    - `HighEmitter`(`new OnKeyPressed().Publish(this)`)
    - `MediumEmitter`
    - `LowEmitter`
    - `EssentialEmitter`(用 `Listen`)
    - `CleanupEmitter`
  - 多种 Subscriber(各种 priority,有些带过滤器)
  - `SpawnMany`(挂 `SpawnMany.cs`)
  - `CancelEmitter`(用 `WithFilter` + `NonCancellable`)

#### 2.2 涉及脚本

**OnShowcaseEvent.cs**:
```csharp
[DefaultChannel("Showcase")]
public class OnShowcaseEvent : GameEvent {}
```

**各种 Emitter/Logger** — 简化版,只为触发各种组合。

**SpawnMany.cs**:
```csharp
[AddComponentMenu("")]
public class SpawnMany : MonoBehaviour
{
    public KeyCode spawnKey = KeyCode.M;
    public GameObject subscriberPrefab;
    public int count = 50;
    void Update()
    {
        if (Input.GetKeyDown(spawnKey))
        {
            for (int i = 0; i < count; i++) Instantiate(subscriberPrefab, transform);
        }
    }
}
```

**CancelEmitter.cs**:
```csharp
[AddComponentMenu("")]
public class CancelEmitter : MonoBehaviour
{
    public KeyCode triggerKey = KeyCode.C;
    void Update()
    {
        if (Input.GetKeyDown(triggerKey))
        {
            new OnShowcaseEvent()
                .WithFilter(new WithTag("Player"))
                .NonCancellable()
                .Publish(this);
        }
    }
}
```

#### 2.3 验证步骤

| # | 工具 | 验证 |
|---|---|---|
| 1 | Subscription Monitor | 启动 Play,激活各种订阅者,列表实时更新订阅者数 |
| 2 | Log Tool | 启动后触发各种事件,列表填充,带 channel/优先级/过滤器标签 |
| 3 | Log 过滤 - EventRaised 开关 | 关闭,OnEventRaised 全部消失 |
| 4 | Log 过滤 - Channel | 加 "Showcase" 过滤,只保留 showcase channel 事件 |
| 5 | Log 过滤 - 持久化 | 关闭 Unity 重启,过滤器配置保留 |
| 6 | Log - Clear Log | 列表清空 |
| 7 | Log - Quick repeat | 右键 → Quick repeat,新行出现 |
| 8 | Log - Open in tester | 右键 → Open in tester,TesterTool 打开 |
| 9 | Log - Save as SO | 右键 → Save as SO,生成 .asset |
| 10 | Log - Edit emitter script | 打开 IDE 编辑 emitter 脚本 |
| 11 | Event Detail | Log 中点击 data 按钮,显示完整元数据 |
| 12 | Event Actors | Log 中点击 actors 按钮,显示 emitter + subscribers |
| 13 | Event Actors - Ping | 点击 GameObject 类型 Ping,Hierarchy 高亮 |
| 14 | Tester Tool | 打开,选 OnShowcaseEvent SO,Publish |
| 15 | GameEventHubEditor | 选中 Hub,看 Inspector |
| 16 | KPI | Active Events 数字 = 业务事件数 |
| 17 | 工具按钮 | Monitor / Tester / Log 按钮可点 |
| 18 | 按 M 键 | 生成 50 个订阅者,Monitor 数字更新 |
| 19 | 性能 | 50 订阅者下 Log 工具不卡 |

---

### V8 Sample — `S8_DocsAndMenu`(文档与菜单)

**位置**:`Assets/Samples/08_DocsAndMenu/`

**最终目标**:验证 V8 核心能力(Welcome + 文档 + Hierarchy 菜单)。

#### 2.1 涉及场景
- `S8_DocsAndMenu.unity`:
  - 空场景(只用于演示 Hierarchy 菜单)
  - 运行时通过 Hierarchy 菜单创建 Hub

#### 2.2 涉及脚本
**无新脚本**,纯验证 V8 功能。

#### 2.3 验证步骤

| # | 操作 | 预期 |
|---|---|---|
| 1 | 删 `EditorPrefs["GameEventHub_WelcomeShown"]` | 重启 Unity |
| 2 | Welcome 自动弹 | 验证 |
| 3 | 关闭 Unity 重启 | 不再弹(已设 key) |
| 4 | Tools > Game Event Hub > Welcome | 手动打开 |
| 5 | 点击 "Add Game Event Hub to scene" | 场景出现 Hub |
| 6 | 点击 "Do demo" | 打开 Demo 场景 + 文档跳到 "Demo Scene" |
| 7 | 点击 "Show documentation" | 打开文档窗口 |
| 8 | Tools > Game Event Hub > Documentation | 打开文档 |
| 9 | 左侧选 Quick Start | 5 篇可翻 |
| 10 | 切到 Examples | 8 篇可翻 |
| 11 | 切到 Tools > Game Event Hub > Changelog | 跳到 Changelog 章节,5 版本可看 |
| 12 | 渲染代码块 | 看到带颜色的代码块 |
| 13 | 渲染图片 | 文档中图片显示 |
| 14 | 渲染引用 | `> text` 显示 |
| 15 | 选 Troubleshooting 章节 | 7+ 故障条目 |
| 16 | 切到空场景 | 测试 Hierarchy 菜单 |
| 17 | GameObject > Game Event Hub | 场景出现 Hub GameObject |
| 18 | 选中 Hub,看 Inspector | 有 "Documentation" 按钮 |
| 19 | 点击 Documentation 按钮 | 打开文档窗口 |
| 20 | 再点 GameObject > Game Event Hub | Console Warning "already exists" |

---

## 3. 整体 Sample 学习路径

按以下顺序阅读 Sample,逐步掌握整个事件系统:

| 顺序 | Sample | 学习目标 | 5 分钟可达成? |
|---|---|---|---|
| 1 | S1_KeyPress | 理解"发布-订阅-取消"基础流 | ✓ |
| 2 | S2_Priority | 理解"5 档优先级"+"取消传播"+"动态订阅" | ✓ |
| 3 | S3_Filter | 理解"过滤器"+"Essential 绕过" | ✓ |
| 4 | S4_SystemEvent | 理解"元事件"+"性能监控"+"单例" | 10 分钟 |
| 5 | S5_SoAndBridge | 理解"事件模板化"+"UnityEvent 桥接" | ✓ |
| 6 | S6_Animator | 理解"动画帧驱动"+"反射注入" | 10 分钟 |
| 7 | S7_Tools | 学会用工具调试 | 15 分钟 |
| 8 | S8_DocsAndMenu | 学会用文档和菜单 | ✓ |

**每步配合** `Tools > Game Event Hub > Documentation > Quick Start` / `Examples` 章节阅读。

---

## 4. 复杂度递进总表

| 版本 | Sample 脚本数 | 资产(场景+SO+动画) | 引入的新概念 | 脚本行数(估) |
|---|---|---|---|---|
| V1 | 3 | 1 scene | GameEvent / OnGameEvent / 反射 | 30 |
| V2 | 8 | 1 scene | Priority / 动态订阅 / 元事件钩子 | 80 |
| V3 | 5 | 1 scene + 2 prefab | 过滤器 6 种 / DefaultChannel | 60 |
| V4 | 5 | 1 scene | 4 个 SystemEvent / 性能监控 | 50 |
| V5 | 4 | 1 scene + 3 SO | ScriptableObject / SubclassSelector / UnityEvent | 40 |
| V6 | 5 | 1 scene + 1 anim + 1 controller + 2 SO | Animator 集成 / 反射注入 | 60 |
| V7 | 10+ | 1 scene + 1 prefab | 5 个工具窗口 | 200+ |
| V8 | (无) | (无) | Welcome / 文档系统 / 菜单 | 0 |

**原则**:
- V1 Sample < 50 行总代码,V7 Sample > 200 行
- V1 一个场景 3 个 GameObject,V7 一个场景 > 10 个
- V4 开始引入 .asset,V6 引入 .anim + .controller
- V8 不新增 Sample 脚本(纯验证 V8 功能)

---

## 5. 验收总表

完成 V1-V8 Sample 后,需确保:

- [x] 每个 Sample 有 `README.md` 说明验证步骤
- [x] 每个 Sample 场景可独立打开、独立运行
- [x] Sample 数量与版本对应(8 个 Sample,部分版本 2 个场景)
- [x] Sample 命名一致(S<Vx>_<Feature>.unity)
- [x] Sample 脚本统一放在 `Assets/Samples/<Vx>_<Name>/Scripts/`
- [x] Sample 资产(GameEventSO、Animator Controller)统一放在 `Assets/Samples/<Vx>_<Name>/<子目录>/`
- [x] 所有 Sample 在 V8 完成后能全部跑通
- [x] Sample 与 Documentation 的 `Examples/` 章节一一对应
- [x] Sample 不破坏 V1-V7 任何已有功能

---

## 6. Sample 与 Examples 文档章节对应

文档系统(`Documentation/Data/Examples/`)的 8 个 Markdown 与 Sample 一一对应:

| Documentation 章节 | Sample | 用途 |
|---|---|---|
| `1. Emit event on key.md` | S1_KeyPress | 按键发布 |
| `2. Emit delayed.md` | S2_Priority(Delayed) | 延迟发布 |
| `3. Cancel event.md` | S2_Priority(HighCanceller) | 取消传播 |
| `4. Dynamic subscriber.md` | S2_Priority(DynamicSubscriber) | 动态订阅 |
| `5. Shared vs Unique.md` | (嵌入 S2 验证) | Shared/Unique 模式 |
| `6. Emit with filters.md` | S3_Filter | 过滤器 |
| `7. Emit from Animator.md` | S6_Animator | Animator 集成 |
| `8. Channels.md` | (嵌入 S5/S7 验证) | Channel 系统 |

每个 Documentation 章节内容应:
- 简短描述(2-3 段)
- 关键代码片段(可复制)
- 截图引用
- 链接到对应 Sample 场景
