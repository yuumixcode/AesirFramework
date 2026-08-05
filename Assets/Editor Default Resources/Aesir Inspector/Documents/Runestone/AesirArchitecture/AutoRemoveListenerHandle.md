# `AutoRemoveListenerHandle`

## 介绍

- 种类: `struct`
- 所在程序集: `Runestone.AesirArchitecture`
- 所在命名空间: `Runestone.AesirArchitecture`

``` csharp
public struct AutoRemoveListenerHandle : System.ValueType, 
System.IDisposable
```

### 注释

- 自动移除监听句柄。包装注销回调。

## 构造方法

| 构造方法签名 [仅包含公共实例方法] | 注释 |
| :--- | :--- |
| `public AutoRemoveListenerHandle(Action removeListenerCallback)` |  |

## 方法

### 所有方法签名总览

| 方法完整签名 |
| :--- | 
| `public Type GetType()` |
| `public override bool Equals(object obj)` |
| `public override int GetHashCode()` |
| `public override string ToString()` |
| `public void Dispose()` |
| `protected object MemberwiseClone()` |
| `protected virtual void Finalize()` |

### 声明的普通方法

| 普通方法名称 | 注释 |
| :--- | :--- | 
| `public void Dispose` | 执行移除监听，重复调用安全 |

### 继承的普通方法

| 普通方法名称 | 注释 | 声明方法的类 |
| :--- | :--- | :--- |
| `public Type GetType` |  | `System.Object` |
| `public override bool Equals` |  | `System.ValueType` |
| `public override int GetHashCode` |  | `System.ValueType` |
| `public override string ToString` |  | `System.ValueType` |
| `protected object MemberwiseClone` |  | `System.Object` |
| `protected virtual void Finalize` |  | `System.Object` |

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Aesir Inspector`](https://github.com/yuumixcode/Unity-Aesir-Packages) 辅助生成。
