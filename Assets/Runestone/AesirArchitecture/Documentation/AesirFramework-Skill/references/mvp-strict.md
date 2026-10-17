# MVP 严格档（Strict）代码模板

> 接口注册 + Command 写入 + Query 读取 + 双接口 Presenter。~8-10 个文件。
> CQRS 严格分离：写入全部经 Command，读取全部经 Query。Presenter 对 Model 的写/读均不直接接触。

## 文件清单

| 文件 | 说明 |
|------|------|
| `IXxxModel.cs` | Model 接口（继承 `IModel`） |
| `XxxModel.cs` | Model 实现 |
| `XxxContext.cs` | Context，按接口注册 Model |
| `IXxxView.cs` | View 契约接口 |
| `XxxPanel.cs` | 被动视图，实现 `IXxxView`，按接口持有 Presenter |
| `IXxxPresenter.cs` | Presenter 业务接口（View 持有类型，**不继承** `IPresenter`，继承 `IDisposable`） |
| `XxxPresenter.cs` | Presenter 实现（业务接口 + `IPresenter<T>`） |
| `XxxCommand.cs` | Command（每个操作一个） |
| `GetXxxQuery.cs` | Query（读取用） |

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
        RegisterModel<ICounterModel>(new CounterModel());
    }
}
```

### 4. View 接口

```csharp
using System;

public interface ICounterView
{
    event Action IncreaseClicked;
    event Action DecreaseClicked;
    event Action ResetClicked;
    void UpdateCount(int count);
}
```

### 5. Presenter 业务接口

```csharp
using System;

// 不继承 IPresenter/IPresenter<T>——使 View 类型层面拿不到 ExecuteCommand
// 继承 IDisposable——View 在 OnDestroy 时释放事件订阅
public interface ICounterPresenter : IDisposable
{
    /// <summary>同步初始值到 View，由 View 在 Start 中调用。</summary>
    void SyncInitialValue();
}
```

### 6. Presenter 实现

```csharp
using Runestone.AesirArchitecture;

public sealed class CounterPresenter : ICounterPresenter, IPresenter<CounterContext>
{
    readonly ICounterView _view;

    public CounterPresenter(ICounterView view)
    {
        _view = view;
        _view.IncreaseClicked += OnIncreaseClicked;
        _view.DecreaseClicked += OnDecreaseClicked;
        _view.ResetClicked += OnResetClicked;
    }

    public void SyncInitialValue()
    {
        _view.UpdateCount(this.ExecuteQuery<GetCounterValueQuery, int>());
    }

    void OnIncreaseClicked()
    {
        this.ExecuteCommand<IncreaseCommand>();
        _view.UpdateCount(this.ExecuteQuery<GetCounterValueQuery, int>());
    }

    void OnDecreaseClicked()
    {
        this.ExecuteCommand<DecreaseCommand>();
        _view.UpdateCount(this.ExecuteQuery<GetCounterValueQuery, int>());
    }

    void OnResetClicked()
    {
        this.ExecuteCommand<ResetCommand>();
        _view.UpdateCount(this.ExecuteQuery<GetCounterValueQuery, int>());
    }

    public void Dispose()
    {
        _view.IncreaseClicked -= OnIncreaseClicked;
        _view.DecreaseClicked -= OnDecreaseClicked;
        _view.ResetClicked -= OnResetClicked;
    }
}
```

### 7. Command（每个操作一个）

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

### 8. Query

```csharp
using Runestone.AesirArchitecture;

public class GetCounterValueQuery : AbstractQuery<int>
{
    protected override int OnExecute()
    {
        return this.GetModel<ICounterModel>().Count.Value;
    }
}
```

### 9. View

```csharp
using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class CounterPanel : MonoBehaviour, ICounterView
{
    [SerializeField] Text countText;
    [SerializeField] Button increaseButton;
    [SerializeField] Button decreaseButton;
    [SerializeField] Button resetButton;

    // 按接口类型存储 Presenter（非具体类）
    ICounterPresenter _presenter;

    void Awake()
    {
        _presenter = new CounterPresenter(this);
    }

    void Start()
    {
        _presenter.SyncInitialValue();
    }

    void OnEnable()
    {
        increaseButton.onClick.AddListener(RaiseIncreaseClicked);
        decreaseButton.onClick.AddListener(RaiseDecreaseClicked);
        resetButton.onClick.AddListener(RaiseResetClicked);
    }

    void OnDisable()
    {
        increaseButton.onClick.RemoveListener(RaiseIncreaseClicked);
        decreaseButton.onClick.RemoveListener(RaiseDecreaseClicked);
        resetButton.onClick.RemoveListener(RaiseResetClicked);
    }

    void OnDestroy()
    {
        _presenter.Dispose();
    }

    public event Action IncreaseClicked;
    public event Action DecreaseClicked;
    public event Action ResetClicked;

    void RaiseIncreaseClicked() => IncreaseClicked?.Invoke();
    void RaiseDecreaseClicked() => DecreaseClicked?.Invoke();
    void RaiseResetClicked() => ResetClicked?.Invoke();

    public void UpdateCount(int count)
    {
        countText.text = count.ToString();
    }
}
```

## 数据流

```
按钮点击 → View event → Presenter → ExecuteCommand → Model 写方法
                → ExecuteQuery 拉取 → Presenter 推送 View 刷新
```

## 关键设计要点

- **双接口设计**：`ICounterPresenter`（业务接口，View 持有，仅生命周期方法）不含 `IPresenter<T>`，View 类型层面拿不到 `ExecuteCommand`
- **Presenter 不直接接触 Model**：写入经 Command，读取经 Query
- **与 MVC-Strict 对称**：MVC 的 View 按接口持有 Controller，MVP 的 View 按接口持有 Presenter；差异是 MVP 的业务经 View 事件流向 Presenter，View 无需主动调用业务方法
