# MVP 快捷档（Quick）代码模板

> 被动视图 + Presenter 直改 Model。不建 View 接口、不建 Model 接口。~3-4 个文件。
> 与 MVC-Quick 的 Model 完全一致；差异在 View 是被动视图（不继承 `MonoView<T>`），刷新由 Presenter 推送。

## 文件清单

| 文件 | 说明 |
|------|------|
| `XxxContext.cs` | Context，按具体类注册 Model |
| `XxxModel.cs` | Model，可写 ObservableValue 直接公开 |
| `XxxPanel.cs` | 被动视图，纯 `MonoBehaviour`，无接口 |
| `XxxPresenter.cs` | Presenter，持有具体 View 类，直改 Model |

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
    public ObservableValue<int> count = new ObservableValue<int>(0);
}
```

### 3. View（被动视图）

```csharp
using System;
using UnityEngine;
using UnityEngine.UI;

// 纯 MonoBehaviour，不继承 MonoView<T>（那是 MVC 的）
// Quick 档不建 View 接口——Presenter 直接持有具体类
public sealed class CounterPanel : MonoBehaviour
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
        // 同步初始值，避免场景残留文本与 Model 不一致
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

    // 用户输入以 C# event 抛给 Presenter
    public event Action IncreaseClicked;
    public event Action DecreaseClicked;
    public event Action ResetClicked;

    void RaiseIncreaseClicked() => IncreaseClicked?.Invoke();
    void RaiseDecreaseClicked() => DecreaseClicked?.Invoke();
    void RaiseResetClicked() => ResetClicked?.Invoke();

    // 由 Presenter 调用刷新显示
    public void UpdateCount(int count)
    {
        countText.text = count.ToString();
    }
}
```

### 4. Presenter

```csharp
using Runestone.AesirArchitecture;

public sealed class CounterPresenter : IPresenter<CounterContext>
{
    readonly CounterPanel _view;
    readonly CounterModel _model;

    public CounterPresenter(CounterPanel view)
    {
        _view = view;
        // IPresenter<T> 提供 GetModel 能力
        _model = this.GetModel<CounterModel>();
        _view.IncreaseClicked += OnIncreaseClicked;
        _view.DecreaseClicked += OnDecreaseClicked;
        _view.ResetClicked += OnResetClicked;
    }

    public void SyncInitialValue()
    {
        _view.UpdateCount(_model.count.Value);
    }

    void OnIncreaseClicked()
    {
        _model.count.Value++;
        _view.UpdateCount(_model.count.Value);
    }

    void OnDecreaseClicked()
    {
        _model.count.Value--;
        _view.UpdateCount(_model.count.Value);
    }

    void OnResetClicked()
    {
        _model.count.Value = 0;
        _view.UpdateCount(_model.count.Value);
    }

    public void Dispose()
    {
        // 注销所有事件订阅
        _view.IncreaseClicked -= OnIncreaseClicked;
        _view.DecreaseClicked -= OnDecreaseClicked;
        _view.ResetClicked -= OnResetClicked;
    }
}
```

## 数据流

```
按钮点击 → View event → Presenter 直改 Model → Presenter 直读 → 推送 View 刷新
```

## 升级路径

- → **Standard**：Model 收窄为只读暴露 + 写方法，View 建接口契约
- → **Strict**：写入走 Command、读取走 Query，Presenter 按双接口设计
