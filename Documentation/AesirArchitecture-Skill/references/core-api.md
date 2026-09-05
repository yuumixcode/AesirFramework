# 核心 API 参考

> RAA 辅助模块的 API 速查。主架构 API（Context、Model、View、Controller、Presenter、Command、Query）见 SKILL.md。

## ObservableValue\<T\> — 响应式属性

Model 持有可写实例，View 通过 `IReadOnlyObservableValue<T>` 订阅。

### 基本用法

```csharp
// 创建（初始值 0）
ObservableValue<int> count = new ObservableValue<int>(0);

// 写入（Model 内部）
count.Value = 10;
count.Value++;

// 读取当前值
int current = count.Value;

// 对外只读暴露
public IReadOnlyObservableValue<int> Count => count;
```

### 订阅

```csharp
// 订阅 + 立即触发一次（推荐——确保新订阅者拿到当前值）
model.Count.AddListenerAndInvoke(OnCountChanged)
    .RemoveListenerWhenGameObjectOnDestroyed(gameObject);

// 仅订阅不立即触发
model.Count.AddListener(OnCountChanged)
    .RemoveListenerWhenGameObjectOnDestroyed(gameObject);

// 静默设置值（不触发通知）
count.SetValueSilently(5);
```

### 回调签名

```csharp
// ObservableValue<T> 的监听签名：void(T newValue) — 单参数，新值
void OnCountChanged(int newValue)
{
    // newValue 是变更后的当前值
}
```

## MiniEvent / MiniEvent\<T\> — 零分配事件

### 基本用法

```csharp
// 无参事件
MiniEvent doorOpened = new MiniEvent();
doorOpened.AddListener(OnDoorOpened)
    .RemoveListenerWhenGameObjectOnDestroyed(gameObject);
doorOpened.Invoke();

// 单参事件
MiniEvent<int> scoreChanged = new MiniEvent<int>();
scoreChanged.AddListener(OnScoreChanged)
    .RemoveListenerWhenGameObjectOnDestroyed(gameObject);
scoreChanged.Invoke(100);
```

### 自动注销句柄

```csharp
// AddListener 返回 AutoRemoveListenerHandle
var handle = myEvent.AddListener(callback);

// 绑定生命周期
handle.RemoveListenerWhenGameObjectOnDestroyed(gameObject);  // GameObject 销毁时
handle.RemoveListenerWhenGameObjectOnDisable(gameObject);     // GameObject 禁用时（UI 面板等）

// 手动注销
handle.RemoveListener();
```

### 多参数事件

```csharp
// 多参数载荷用 struct 包裹，作为单参事件
public struct DamageInfo
{
    public int Amount;
    public GameObject Source;
}

MiniEvent<DamageInfo> damageDealt = new MiniEvent<DamageInfo>();
damageDealt.Invoke(new DamageInfo { Amount = 50, Source = attacker });
```

> **异常语义**：MiniEvent 是零分配直调，异常语义 = 原生 C# 事件（fail-fast）。一个监听者抛异常会中断后续监听者。监听回调不应抛异常。

## AbstractContext\<T\> — 上下文

### 生命周期

```csharp
public class GameContext : AbstractContext<GameContext>
{
    protected override void Configure()
    {
        // 注册 Model 和 Service（被依赖的先注册）
        RegisterModel<IGameModel>(new GameModel());
        RegisterService<IAudioService>(new AudioService());
    }
}

// 访问单例（懒加载，初始化成功后才赋值）
GameContext.Instance.GetModel<IGameModel>();

// 模块 OnInitialize（全部注册完毕后调用）
public class GameModel : AbstractModel, IGameModel
{
    protected override void OnInitialize()
    {
        // 此时可安全访问其他已注册的 Model/Service
    }
}
```

### 禁忌

- **Configure() 中禁止访问 `Instance`**——会递归创建第二个上下文
- **Register 与 Get 类型参数必须一致**——按接口注册就按接口获取
- **被依赖的模块先注册**——框架按注册顺序初始化 Model → Service

## GenericLocator\<T\> — 类型键控定位器

```csharp
// 注册
GenericLocator<IAudioService>.Register(new AudioService());

// 获取
var audio = GenericLocator<IAudioService>.Get();

// 注销
GenericLocator<IAudioService>.Unregister();
```

## PlayerLoop 生命周期注入

```csharp
// 注册帧回调（不需要 MonoBehaviour）
AesirArchitecturePlayerLoop.Register(AesirArchitectureLifecyclePhase.BeforeUpdate, MyFrameCallback);
AesirArchitecturePlayerLoop.Register(AesirArchitectureLifecyclePhase.AfterUpdate, MyAfterUpdateCallback);

// 注销
AesirArchitecturePlayerLoop.Unregister(AesirArchitectureLifecyclePhase.BeforeUpdate, MyFrameCallback);

// 确保已注入（第三方 SDK 修改 PlayerLoop 后调用一次）
AesirArchitecturePlayerLoop.EnsureInjected();
```

> `Register` 注册回调时会自动检测 PlayerLoop 注入状态并补插。
> 可用阶段：`BeforeUpdate`（Update 前）、`AfterUpdate`（PostLateUpdate 后）。

## AesirArchitecture — MonoBehaviour 单例入口

```csharp
// 场景预放置（推荐）：直接在场景中放 AesirArchitecture 组件
// 运行时创建：Instance getter 自动 FindAnyObjectByType，未找到时创建 + DDOL
```

> 预放置实例不 DDOL；运行时创建的实例 DDOL。

## MonoView\<T\> / MonoViewController\<T\> — MonoBehaviour 适配层

| 基类 | 能力 | 用途 |
|------|------|------|
| `MonoView<T>` | GetModel, GetService（只读） | MVC Standard/Strict 的 View |
| `MonoViewController<T>` | GetModel, GetService, ExecuteCommand, ExecuteQuery | MVC Quick 的 View 兼 Controller |
| `AesirView<T>` | 同 `MonoView<T>` + Odin 增强 | 需要 Odin Inspector 增强时 |
| `AesirViewController<T>` | 同 `MonoViewController<T>` + Odin 增强 | 需要 Odin Inspector 增强时 |

> 无 Odin 时用 `Mono*` 版本；有 Odin 时用 `Aesir*` 版本（功能相同，Inspector 体验更好）。

## 能力矩阵

| 角色 | GetModel | GetService | ExecuteCommand | ExecuteQuery | Initialize | Dispose |
|------|:--------:|:---------:|:--------------:|:------------:|:----------:|:-------:|
| IModel | ✓ | | | | ✓ | ✓ |
| IService | ✓ | ✓ | | | ✓ | ✓ |
| IView | ✓ | ✓ | | | | |
| IController | ✓ | ✓ | ✓ | ✓ | | |
| IPresenter | ✓ | ✓ | ✓ | ✓ | | ✓ |

## AbstractSubmodule — 统一子模块生命周期

Model 和 Service 的共享生命周期逻辑：

```csharp
public abstract class AbstractModel : AbstractSubmodule, IModel { ... }
public abstract class AbstractService : AbstractSubmodule, IService { ... }

// 子模块生命周期：
// Register → OnInitialize() → ... → OnDispose()
```

## ResetStaticsAssistant — 静态变量重置

```csharp
// 泛型类的静态变量在 Domain Reload 后会被 Unity 静默跳过重置
// 通过 Register 注册重置回调
public class MyGenericClass<T>
{
    static MyGenericClass<T> _instance;

    static MyGenericClass()
    {
        ResetStaticsAssistant.Register(() => _instance = null);
    }
}
```

> 非泛型单例类用 `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]` 自重置即可。
