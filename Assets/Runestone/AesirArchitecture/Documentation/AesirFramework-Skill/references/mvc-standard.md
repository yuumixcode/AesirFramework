# MVC 标准档（Standard）代码模板

> Model 收窄为只读暴露 + 写方法；View 与 Controller 拆为两个实例共享同一 Model。~4 个文件。
> 写入不经 Command，由 Controller 直接调 Model 写方法。

## 文件清单

| 文件 | 说明 |
|------|------|
| `XxxContext.cs` | Context，按具体类注册 Model |
| `XxxModel.cs` | Model，只读暴露 + 写方法 |
| `XxxController.cs` | 纯 C# Controller，View 注入共享 Model |
| `XxxPanel.cs` | 面板，继承 `MonoView<T>`（仅只读能力） |

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

    /// <summary>当前计数值（只读），外部只可订阅与读取。</summary>
    public IReadOnlyObservableValue<int> Count => count;

    public void Increase() => count.Value++;
    public void Decrease() => count.Value--;
    public void Reset() => count.Value = 0;

    protected override void OnInitialize() { }
}
```

### 3. Controller

```csharp
public sealed class CounterController
{
    readonly CounterModel _model;

    /// <summary>View 注入共享的 Model 实例（同一引用）。</summary>
    public CounterController(CounterModel model)
    {
        _model = model;
    }

    public void Increase()
    {
        _model.Increase();
        AesirArchitectureDebug.Log("Increase");
    }

    public void Decrease()
    {
        _model.Decrease();
        AesirArchitectureDebug.Log("Decrease");
    }

    public void ResetCounter()
    {
        _model.Reset();
        AesirArchitectureDebug.Log("Reset");
    }
}
```

### 4. View

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

    CounterModel _model;
    CounterController _controller;

    void Start()
    {
        // View 与 Controller 共享同一个 Model 实例
        _model = this.GetModel<CounterModel>();
        _controller = new CounterController(_model);
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
            countText.text = count.ToString();
    }
}
```

## 数据流

```
按钮点击 → Controller 调 Model 写方法 → ObservableValue 通知 → 面板刷新
```

## 升级路径

- → **Strict**：Model 按接口注册，Controller 改走 `ExecuteCommand`，加工读取走 `ExecuteQuery`
