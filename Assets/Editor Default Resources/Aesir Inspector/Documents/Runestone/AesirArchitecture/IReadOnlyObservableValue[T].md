# `IReadOnlyObservableValue<T>`

## 介绍

- 种类: `interface`
- 所在程序集: `Runestone.AesirArchitecture`
- 所在命名空间: `Runestone.AesirArchitecture`

``` csharp
public interface IReadOnlyObservableValue<T> 
```

### 注释

- 只读可观察属性接口。 View 层通过此接口添加监听，不能修改值。

## 方法

### 所有方法签名总览

| 方法完整签名 |
| :--- | 
| `public abstract AutoRemoveListenerHandle AddListener(Action<T> callback)` |
| `public abstract AutoRemoveListenerHandle AddListenerAndInvoke(Action<T> callback)` |
| `public abstract void InvokeEvent()` |
| `public abstract void RemoveListener(Action<T> callback)` |

### 声明的普通方法

| 普通方法名称 | 注释 |
| :--- | :--- | 
| `public abstract AutoRemoveListenerHandle AddListener` | 添加监听者。回调参数为新值。 |
| `public abstract AutoRemoveListenerHandle AddListenerAndInvoke` | 添加监听并立即触发一次当前值，用于初始化时同步监听方状态。 |
| `public abstract void InvokeEvent` | 触发值变更通知，用于强制刷新监听方状态。 |
| `public abstract void RemoveListener` | 移除监听者。 |

## 属性

### 声明的属性

| 属性签名 | 注释 |
| :--- | :--- |
| `public T Value { get; }` |  |

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Aesir Inspector`](https://github.com/yuumixcode/Unity-Aesir-Packages) 辅助生成。
