# MVC 快捷档（Quick）代码模板

> 最少概念跑通闭环。Context + Model + 面板（View 兼 Controller），~3 个文件。
> 适合原型/小功能；不建接口、不建 Command、不建独立 Controller。

## 文件清单

| 文件 | 说明 |
|------|------|
| `XxxContext.cs` | Context，按具体类注册 Model |
| `XxxModel.cs` | Model，可写 ObservableValue 直接公开 |
| `XxxPanel.cs` | 面板，继承 `MonoViewController<T>`（View 兼 Controller） |

## 完整代码模板

### 1. Context

```csharp
using Runestone.AesirArchitecture;

public sealed class CounterContext : AbstractContext<CounterContext>
{
    protected override void Configure()
    {
        // 按具体类注册（无接口抽象）
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

### 3. View 兼 Controller

```csharp
using UnityEngine;
using UnityEngine.UI;
using Runestone.AesirArchitecture;

public class CounterPanel : MonoViewController<CounterContext>
{
    [SerializeField] Text countText;
    [SerializeField] Button increaseButton;
    [SerializeField] Button decreaseButton;
    [SerializeField] Button resetButton;

    CounterModel _model;

    void Start()
    {
        // GetModel 缓存为字段，避免每帧字典查找
        _model = this.GetModel<CounterModel>();
        // AddListenerAndInvoke: 订阅并立即触发一次（拿到当前值）
        _model.count.AddListenerAndInvoke(UpdateCountText)
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
        // 与 OnEnable 逐一对称 Remove
        increaseButton.onClick.RemoveListener(Increase);
        decreaseButton.onClick.RemoveListener(Decrease);
        resetButton.onClick.RemoveListener(ResetCounter);
    }

    // 直改 ObservableValue（快捷档特有写法）
    void Increase() => _model.count.Value++;
    void Decrease() => _model.count.Value--;
    void ResetCounter() => _model.count.Value = 0;

    public void UpdateCountText(int count)
    {
        if (countText != null)
            countText.text = count.ToString();
    }
}
```

## 数据流

```
按钮点击 → 面板直改 count.Value → ObservableValue 通知 → 面板刷新
```

## 升级路径

- → **Standard**：Model 收窄为只读暴露 + 写方法，View 与 Controller 拆为两个实例
- → **Strict**：Model 按接口注册，写入改走 Command，加工读取走 Query
