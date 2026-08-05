# `ObservableValue<T>`

## 介绍

- 种类: `class`
- 所在程序集: `Runestone.AesirArchitecture`
- 所在命名空间: `Runestone.AesirArchitecture`

``` csharp
[Serializable]
public sealed class ObservableValue<T> : Runestone.AesirArchitecture.IObservableValue<T>, 
Runestone.AesirArchitecture.IReadOnlyObservableValue<T> 
```

### 注释

- 可观察属性实现。 Model 层持有可写实例，View 层通过 IReadOnlyObservableValue{T} 只读订阅。

## 构造方法

| 构造方法签名 [仅包含公共实例方法] | 注释 |
| :--- | :--- |
| `public ObservableValue<T>()` |  |
| `public ObservableValue<T>(T initialValue)` |  |

## 方法

### 所有方法签名总览

| 方法完整签名 |
| :--- | 
| `public AutoRemoveListenerHandle AddListener(Action<T> callback)` |
| `public AutoRemoveListenerHandle AddListenerAndInvoke(Action<T> callback)` |
| `public Type GetType()` |
| `public virtual bool Equals(object obj)` |
| `public virtual int GetHashCode()` |
| `public virtual string ToString()` |
| `public void Clear()` |
| `public void InvokeEvent()` |
| `public void RemoveListener(Action<T> callback)` |
| `public void SetValue(T v)` |
| `public void SetValueSilently(T v)` |
| `protected object MemberwiseClone()` |
| `protected virtual void Finalize()` |

### 声明的普通方法

| 普通方法名称 | 注释 |
| :--- | :--- | 
| `public AutoRemoveListenerHandle AddListener` | 添加监听者。回调参数为新值。 |
| `public AutoRemoveListenerHandle AddListenerAndInvoke` | 添加监听并立即触发一次当前值，用于初始化时同步监听方状态。 |
| `public void Clear` | 清除所有监听。 |
| `public void InvokeEvent` | 触发值变更通知，用于强制刷新订阅方状态。 |
| `public void RemoveListener` | 移除监听者。 |
| `public void SetValue` | 设置值。语义等价于 Value 的 setter。 |
| `public void SetValueSilently` | 静默设置值，不触发通知。用于反序列化或批量更新后统一触发。 |

### 继承的普通方法

| 普通方法名称 | 注释 | 声明方法的类 |
| :--- | :--- | :--- |
| `public Type GetType` |  | `System.Object` |
| `public virtual bool Equals` |  | `System.Object` |
| `public virtual int GetHashCode` |  | `System.Object` |
| `public virtual string ToString` |  | `System.Object` |
| `protected object MemberwiseClone` |  | `System.Object` |
| `protected virtual void Finalize` |  | `System.Object` |

## 属性

### 声明的属性

| 属性签名 | 注释 |
| :--- | :--- |
| `public T Value { get; set; }` | 读写属性值。设置新值时若与旧值不同，则触发变更通知。 |

## 字段

### 常量字段

| 字段完整签名 | 注释 |
| :--- | :--- |
| `public const string InvokeMethodName = "InvokeEvent";` |  |
| `public const string PrivateValueFieldName = "value";` |  |

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Aesir Inspector`](https://github.com/yuumixcode/Unity-Aesir-Packages) 辅助生成。
