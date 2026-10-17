# MVC 严格档（Strict）代码模板

> 接口注册 + Command 写入 + Query 加工读取。View 按接口持有 Model 和 Controller。~7-10 个文件。
> CQRS 严格分离：写入全部经 Command，加工值读取经 Query（原始值直接用只读订阅）。

## 文件清单

| 文件 | 说明 |
|------|------|
| `IXxxModel.cs` | Model 接口（继承 `IModel`） |
| `XxxModel.cs` | Model 实现 |
| `XxxContext.cs` | Context，按接口注册 Model |
| `IXxxController.cs` | Controller 业务接口（View 持有类型，**不继承** `IController`） |
| `XxxController.cs` | Controller 实现（实现业务接口 + `IController<T>`） |
| `XxxCommand.cs` | Command（每个操作一个） |
| `GetXxxQuery.cs` | Query（仅加工值需要） |
| `XxxPanel.cs` | 面板，继承 `MonoView<T>`，按接口持有 Model 和 Controller |

## 完整代码模板

### 1. Model 接口

```csharp
using Runestone.AesirArchitecture;

public interface ICounterModel : IModel
{
    IReadOnlyObservableValue<int> Count { get; }
    void Increase();
    void Decrease();
    void Reset();
}
```

### 2. Model 实现

```csharp
using System;
using UnityEngine;
using Runestone.AesirArchitecture;

[Serializable]
public sealed class CounterModel : AbstractModel, ICounterModel
{
    [SerializeField]
    ObservableValue<int> count = new ObservableValue<int>(0);

    public IReadOnlyObservableValue<int> Count => count;

    public void Increase() => count.Value++;
    public void Decrease() => count.Value--;
    public void Reset() => count.Value = 0;

    protected override void OnInitialize() { }
}
```

### 3. Context

```csharp
using Runestone.AesirArchitecture;

public sealed class CounterContext : AbstractContext<CounterContext>
{
    protected override void Configure()
    {
        // 按接口注册（与 GetModel<ICounterModel> 类型参数一致）
        RegisterModel<ICounterModel>(new CounterModel());
    }
}
```

### 4. Controller 业务接口

```csharp
// 不继承 IController/IController<T>——使 View 类型层面拿不到 ExecuteCommand
public interface ICounterController
{
    void Increase();
    void Decrease();
    void ResetCounter();

    /// <summary>查询加工值（十位四舍五入）。原始值直接用只读订阅，无需此方法。</summary>
    int GetRoundedCount();
}
```

### 5. Controller 实现

```csharp
using Runestone.AesirArchitecture;

public sealed class CounterController : ICounterController, IController<CounterContext>
{
    public void Increase() => this.ExecuteCommand<IncreaseCommand>();
    public void Decrease() => this.ExecuteCommand<DecreaseCommand>();
    public void ResetCounter() => this.ExecuteCommand<ResetCommand>();

    public int GetRoundedCount() => this.ExecuteQuery<GetRoundedCountQuery, int>();
}
```

### 6. Command（每个操作一个）

```csharp
using Runestone.AesirArchitecture;

public class IncreaseCommand : AbstractCommand
{
    protected override void OnExecute()
    {
        this.GetModel<ICounterModel>().Increase();
    }
}

public class DecreaseCommand : AbstractCommand
{
    protected override void OnExecute()
    {
        this.GetModel<ICounterModel>().Decrease();
    }
}

public class ResetCommand : AbstractCommand
{
    protected override void OnExecute()
    {
        this.GetModel<ICounterModel>().Reset();
    }
}
```

### 7. Query（仅加工值需要）

```csharp
using System;
using Runestone.AesirArchitecture;

public class GetRoundedCountQuery : AbstractQuery<int>
{
    protected override int OnExecute()
    {
        int count = this.GetModel<ICounterModel>().Count.Value;
        return (int)Math.Round(count / 10.0, MidpointRounding.AwayFromZero) * 10;
    }
}
```

### 8. View

```csharp
using UnityEngine;
using UnityEngine.UI;
using Runestone.AesirArchitecture;

public class CounterPanel : MonoView<CounterContext>
{
    [SerializeField] Text countText;
    [SerializeField] Button increaseButton;
    [SerializeField] Button decreaseButton;
    [SerializeField] Button resetButton;

    // 按接口类型存储（非具体类）
    ICounterModel _model;
    ICounterController _controller;

    void Start()
    {
        _controller = new CounterController();
        _model = this.GetModel<ICounterModel>();
        _model.Count.AddListenerAndInvoke(UpdateCountText)
            .RemoveListenerWhenGameObjectOnDestroyed(gameObject);
    }

    void OnEnable()
    {
        increaseButton.onClick.AddListener(Increase);
        decreaseButton.onClick.AddListener(Decrease);
        resetButton.onClick.AddListener(ResetCounter);
    }

    void OnDisable()
    {
        increaseButton.onClick.RemoveListener(Increase);
        decreaseButton.onClick.RemoveListener(Decrease);
        resetButton.onClick.RemoveListener(ResetCounter);
    }

    void Increase() => _controller.Increase();
    void Decrease() => _controller.Decrease();
    void ResetCounter() => _controller.ResetCounter();

    public void UpdateCountText(int count)
    {
        if (countText != null)
        {
            // 原始值来自只读订阅；加工值经 Controller 查询 Query
            countText.text = $"{count}（≈{_controller.GetRoundedCount()}）";
        }
    }
}
```

## 数据流

```
写入：按钮点击 → Controller → ExecuteCommand → Model 写方法 → ObservableValue 通知 → 面板刷新
读取：面板需要加工值 → Controller → ExecuteQuery → 返回
```

## 关键设计要点

- **双接口设计**：`ICounterController`（业务接口，View 持有）不含 `IController<T>`，View 类型层面拿不到 `ExecuteCommand`
- **Controller 不持有 Model**：经 Context 的 `GetModel` 获取，与 View 完全解耦
- **Query 仅用于加工值**：返回原始值直接用只读订阅 `Count`，无需 Query
