# `IObservableValue<T>`

## 介绍

- 种类: `interface`
- 所在程序集: `Runestone.AesirArchitecture`
- 所在命名空间: `Runestone.AesirArchitecture`

``` csharp
public interface IObservableValue<T> : Runestone.AesirArchitecture.IReadOnlyObservableValue<T> 
```

### 注释

- 完整可观察属性接口。 Presenter 层通过此接口读写数据。

## 方法

### 所有方法签名总览

| 方法完整签名 |
| :--- | 
| `public abstract void SetValue(T value)` |
| `public abstract void SetValueSilently(T value)` |

### 声明的普通方法

| 普通方法名称 | 注释 |
| :--- | :--- | 
| `public abstract void SetValue` | 设置值。语义等价于 Value 的 setter，便于以方法形式调用。 |
| `public abstract void SetValueSilently` | 静默设置值，不触发通知。用于反序列化或批量更新后统一触发。 |

## 属性

### 声明的属性

| 属性签名 | 注释 |
| :--- | :--- |
| `public T Value { get; set; }` | 读写属性值。设置新值时若与旧值不同，则触发变更通知。 |

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Aesir Inspector`](https://github.com/yuumixcode/Unity-Aesir-Packages) 辅助生成。
