---
name: aesir-architecture
description: AesirArchitecture (RAA) 渐进式 MVC/MVP 架构框架的 AI 编码指南。当需要为 Unity/团结引擎项目编写 RAA 架构代码时使用——包括创建 Context、Model、View、Controller、Presenter、Command、Query，以及选择 MVC/MVP 模式和 Quick/Standard/Strict 三档分级。触发场景：用户提到 AesirArchitecture、RAA、AbstractContext、MonoView、MonoViewController、ObservableValue、MiniEvent、IPresenter、IController、AbstractCommand、AbstractQuery，或要求"用 RAA 架构写"、"按 Aesir 架构组织代码"、"用 Aesir 架构"。
---

# AesirArchitecture AI 编码指南

## 框架概览

RAA（Runestone Aesir Architecture）是 Unity/团结引擎的渐进式 MVC/MVP 架构框架。核心设计：

- **MVC 优先**：`IController` 是推荐入口；`IPresenter`（MVP）为可选的严格分层模式
- **能力接口组合**：角色通过细粒度能力接口（`ICanGetModel`、`ICanExecuteCommand` 等）定义，只暴露所需能力
- **纯 C# 核心 + MonoBehaviour 适配层**：框架核心不依赖 MonoBehaviour
- **Domain Reload 安全**：静态变量显式重置，Play Mode 进出无残留

命名空间：`Runestone.AesirArchitecture`

## 第一步：选择模式与档位

### MVC vs MVP

| 维度 | MVC（推荐） | MVP（可选） |
|------|------------|------------|
| View 访问 Model | View 自行订阅 `ObservableValue` | View 不接触 Model，由 Presenter 推送刷新 |
| View 基类 | `MonoView<T>` 或 `MonoViewController<T>` | 纯 `MonoBehaviour`（被动视图） |
| 刷新方式 | View 主动订阅 | Presenter 推送 |
| View 输入暴露 | 直接在回调中调 Controller | C# `event` 抛给 Presenter |
| 适用场景 | 大多数情况 | 需要 View 与 Model 完全解耦时 |

### 三档分级

| 档位 | Model 注册 | Model 暴露面 | 写入路径 | 读取路径 | 文件数 |
|------|-----------|-------------|---------|---------|--------|
| **Quick** | 具体类 | 可写 `ObservableValue` 直接公开 | 直改 `ObservableValue` | 直读 `.Value` | ~3-4 |
| **Standard** | 具体类 | 只读接口 `IReadOnlyObservableValue<T>` + 写方法 | 调 Model 写方法 | 直读 `.Value` | ~4-5 |
| **Strict** | 接口 | 只读接口 + 写方法 | `ExecuteCommand` | `ExecuteQuery`（加工值）/ 直读（原始值） | ~7-10 |

**选择建议**：从 Standard 起步；原型阶段可用 Quick；需要 CQRS 严格分离时用 Strict。

### 选定后加载对应模板

根据用户选择的模式和档位，加载对应的完整代码模板：

| 模式 + 档位 | 引用文件 |
|-------------|---------|
| MVC Quick | [references/mvc-quick.md](references/mvc-quick.md) |
| MVC Standard | [references/mvc-standard.md](references/mvc-standard.md) |
| MVC Strict | [references/mvc-strict.md](references/mvc-strict.md) |
| MVP Quick | [references/mvp-quick.md](references/mvp-quick.md) |
| MVP Standard | [references/mvp-standard.md](references/mvp-standard.md) |
| MVP Strict | [references/mvp-strict.md](references/mvp-strict.md) |

需要了解 `ObservableValue`、`MiniEvent`、`PlayerLoop` 等辅助模块时，加载 [references/core-api.md](references/core-api.md)。

## 核心 API 速查

### Context — 模块注册中心

```csharp
// 纯 C# 单例（CRTP 泛型），不依赖 MonoBehaviour
public class GameContext : AbstractContext<GameContext>
{
    protected override void Configure()
    {
        // Quick/Standard: 按具体类注册
        RegisterModel(new CounterModel());
        // Strict: 按接口注册（Register 与 Get 类型参数必须一致）
        RegisterModel<ICounterModel>(new CounterModel());
        // Service 同理
        RegisterService<IAudioService>(new AudioService());
    }
}
```

### Model — 数据层

```csharp
// Quick: 可写 ObservableValue 直接公开，不定义接口
[Serializable]
public sealed class CounterModel : AbstractModel
{
    [SerializeField] public ObservableValue<int> count = new(0);
}

// Standard/Strict: 只读暴露 + 写方法
public sealed class CounterModel : AbstractModel, ICounterModel
{
    [SerializeField] ObservableValue<int> count = new(0);
    public IReadOnlyObservableValue<int> Count => count;
    public void Increase() => count.Value++;
    protected override void OnInitialize() { }
}
```

### View — MVC

```csharp
// Quick: MonoViewController<T>（View 兼 Controller，有写能力）
public class CounterPanel : MonoViewController<GameContext> { ... }

// Standard/Strict: MonoView<T>（仅只读能力，不可执行 Command）
public class CounterPanel : MonoView<GameContext> { ... }
```

### View — MVP（纯 MonoBehaviour）

```csharp
// Quick: 无接口，Presenter 持具体类
public sealed class CounterPanel : MonoBehaviour { ... }

// Standard/Strict: 实现 IXxxView 接口
public sealed class CounterPanel : MonoBehaviour, ICounterView { ... }
```

### Controller — MVC Standard/Strict

```csharp
// Standard: 纯 C# 类，View 注入共享 Model
public sealed class CounterController
{
    readonly CounterModel _model;
    public CounterController(CounterModel model) => _model = model;
    public void Increase() => _model.Increase();
}

// Strict: 双接口——业务接口（View 持有）+ IController<T>（框架能力）
public interface ICounterController
{
    void Increase();
    int GetRoundedCount();
}

public sealed class CounterController : ICounterController, IController<GameContext>
{
    public void Increase() => this.ExecuteCommand<IncreaseCommand>();
    public int GetRoundedCount() => this.ExecuteQuery<GetRoundedCountQuery, int>();
}
```

### Presenter — MVP

```csharp
// Quick: IPresenter<T>，持有具体 View 类
public sealed class CounterPresenter : IPresenter<GameContext> { ... }

// Strict: 双接口——业务接口（View 持有）+ IPresenter<T>
public interface ICounterPresenter : IDisposable
{
    void SyncInitialValue();
}
public sealed class CounterPresenter : ICounterPresenter, IPresenter<GameContext> { ... }
```

### Command / Query

```csharp
// Command（只写无返回值）
public class IncreaseCommand : AbstractCommand
{
    protected override void OnExecute()
    {
        this.GetModel<ICounterModel>().Increase();
    }
}

// Query（只读返回值，仅用于加工值；返回原始值直接用只读订阅）
public class GetRoundedCountQuery : AbstractQuery<int>
{
    protected override int OnExecute()
    {
        int count = this.GetModel<ICounterModel>().Count.Value;
        return (int)Math.Round(count / 10.0, MidpointRounding.AwayFromZero) * 10;
    }
}

// 执行
this.ExecuteCommand<IncreaseCommand>();
int result = this.ExecuteQuery<GetRoundedCountQuery, int>();
```

## 事件机制决策

| 场景 | 使用 | 理由 |
|------|------|------|
| Model 状态变化通知外界 | `ObservableValue<T>` | 状态即数据，自动通知；新订阅者用 `AddListenerAndInvoke` 拿当前值 |
| 一次性瞬时通知（"门开了"） | `MiniEvent` / `MiniEvent<T>` | 零分配直调，返回 `AutoRemoveListenerHandle` |
| View→Presenter 输入（MVP） | C# `event` | 编译期限制外部只能 `+=`/`-=` |
| Inspector 拖拽 UI 交互 | `UnityEvent` | 可视化配置，美术可调 |

**反模式**：不要用 `Action` 属性代替 `event`（外部可整体替换/置空）；不要用 `MiniEvent` 承载持续变化的状态（用 `ObservableValue`）。

## 关键约定（违反即 fail-fast）

1. **Register 与 Get 类型参数必须一致**——按接口注册就按接口获取，按具体类注册就按具体类获取
2. **Configure() 中禁止访问 `Instance`**——会递归创建第二个上下文实例
3. **GetModel 在 Start/Awake 缓存为字段**——避免 Update 等每帧路径重复字典查找
4. **监听回调不应抛异常**——MiniEvent 是零分配直调，异常会中断后续监听者
5. **ObservableValue 订阅用 `AddListenerAndInvoke`**——确保新订阅者立即拿到当前值
6. **OnEnable/OnDisable 逐一对称 Add/Remove**——不要用 `RemoveAllListeners` 一刀切
7. **MVP View 一律纯 `MonoBehaviour`**——不继承 `MonoView<T>`（那是 MVC 的）
8. **Quick 档零接口抽象**——不建 Model/View/Presenter/Controller 任何接口
9. **Strict 档双接口设计**——业务接口（View 持有类型，不含框架能力）+ 框架角色接口（`IController<T>` / `IPresenter<T>`，提供 Command/Query 能力）
10. **Query 仅用于加工值**——返回原始值直接用只读订阅，无需 Query

## 命名规范

- **命名空间**：`Runestone.AesirArchitecture`（框架）、业务代码自定义
- **Context**：`XxxContext : AbstractContext<XxxContext>`
- **Model**：`IXxxModel : IModel`（接口）、`XxxModel : AbstractModel, IXxxModel`（实现）
- **View**：`XxxPanel : MonoView<XxxContext>` / `MonoBehaviour`
- **Controller**：`IXxxController`（业务接口）、`XxxController : IXxxController, IController<XxxContext>`
- **Presenter**：`IXxxPresenter : IDisposable`（业务接口）、`XxxPresenter : IXxxPresenter, IPresenter<XxxContext>`
- **Command**：`XxxCommand : AbstractCommand`
- **Query**：`GetXxxQuery : AbstractQuery<TResult>`
- **ObservableValue**：私有字段 `count`（`[SerializeField]`），对外属性 `Count`（`IReadOnlyObservableValue<T>`）
- **事件**：MVP View 事件无 `On` 前缀（`IncreaseClicked`），订阅 `OnIncreaseClicked`，触发 `RaiseIncreaseClicked`

## 文件组织

每档典型文件结构（以 Counter 为例）：

```
Scripts/
├── Context/          # XxxContext.cs
├── Model/            # IXxxModel.cs（Strict）+ XxxModel.cs
├── View/             # IXxxView.cs（MVP Standard+）+ XxxPanel.cs
├── Controller/       # IXxxController.cs（MVC Strict）+ XxxController.cs
├── Presenter/        # IXxxPresenter.cs（MVP Strict）+ XxxPresenter.cs
├── Command/          # XxxCommand.cs（Strict）
└── Query/            # GetXxxQuery.cs（Strict）
```

## 常见陷阱

详见同目录 [../常见陷阱清单.md](../常见陷阱清单.md)（10 条，每条配错误现象 + 原因 + 修法）。
事件机制选择详见 [../事件机制决策表.md](../事件机制决策表.md)。
