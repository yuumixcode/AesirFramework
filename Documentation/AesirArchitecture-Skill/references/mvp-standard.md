# MVP 标准档（Standard）代码模板

> Model 收窄为只读暴露 + 写方法；View 建接口契约；Presenter 调写方法 + 直读推送。~5 个文件。
> 与 MVC-Standard 的 Model 完全一致；差异在 View 是被动视图，刷新由 Presenter 推送。

## 文件清单

| 文件 | 说明 |
|------|------|
| `XxxContext.cs` | Context，按具体类注册 Model |
| `XxxModel.cs` | Model，只读暴露 + 写方法 |
| `IXxxView.cs` | View 契约接口（用户输入事件 + 刷新入口） |
| `XxxPanel.cs` | 被动视图，实现 `IXxxView` |
| `XxxPresenter.cs` | Presenter，持有 `IXxxView`，调写方法 + 直读推送 |

## 完整代码模板

### 1. Context

```csharp
using Runestone.AesirArchitecture;

public sealed class CounterContext : AbstractContext<CounterContext>
{
    protected override void Configure()
    {
        RegisterModel(new CounterModel());
    }
}
```

### 2. Model

```csharp
using System;
using UnityEngine;
using Runestone.AesirArchitecture;

[Serializable]
public sealed class CounterModel : AbstractModel
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

### 3. View 接口

```csharp
using System;

// 不继承 IView——被动视图契约不携带任何 Context 能力
public interface ICounterView
{
    event Action IncreaseClicked;
    event Action DecreaseClicked;
    event Action ResetClicked;

    /// <summary>由 Presenter 调用，推送最新值到 View 刷新。</summary>
    void UpdateCount(int count);
}
```

### 4. View 实现

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

    CounterPresenter _presenter;

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

### 5. Presenter

```csharp
using Runestone.AesirArchitecture;

public sealed class CounterPresenter : IPresenter<CounterContext>
{
    readonly ICounterView _view;
    readonly CounterModel _model;

    public CounterPresenter(ICounterView view)
    {
        _view = view;
        _model = this.GetModel<CounterModel>();
        _view.IncreaseClicked += OnIncreaseClicked;
        _view.DecreaseClicked += OnDecreaseClicked;
        _view.ResetClicked += OnResetClicked;
    }

    public void SyncInitialValue()
    {
        _view.UpdateCount(_model.Count.Value);
    }

    void OnIncreaseClicked()
    {
        _model.Increase();
        _view.UpdateCount(_model.Count.Value);
    }

    void OnDecreaseClicked()
    {
        _model.Decrease();
        _view.UpdateCount(_model.Count.Value);
    }

    void OnResetClicked()
    {
        _model.Reset();
        _view.UpdateCount(_model.Count.Value);
    }

    public void Dispose()
    {
        _view.IncreaseClicked -= OnIncreaseClicked;
        _view.DecreaseClicked -= OnDecreaseClicked;
        _view.ResetClicked -= OnResetClicked;
    }
}
```

## 数据流

```
按钮点击 → View event → Presenter 调 Model 写方法 → Presenter 直读 Count.Value → 推送 View 刷新
```

## 升级路径

- → **Strict**：Model 按接口注册，写入走 Command、读取走 Query，Presenter 按双接口设计
