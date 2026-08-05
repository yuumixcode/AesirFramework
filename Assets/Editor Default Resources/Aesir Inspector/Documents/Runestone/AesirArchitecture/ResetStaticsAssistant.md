# `ResetStaticsAssistant`

## 介绍

- 种类: `static class`
- 所在程序集: `Runestone.AesirArchitecture`
- 所在命名空间: `Runestone.AesirArchitecture`

``` csharp
public static class ResetStaticsAssistant
```

### 注释

- 静态变量重置助手，用于运行时阶段自动重置静态变量，兼容 Disable Domain Reload。 关闭 Domain Reload 时静态回调列表不会重置，所以每次启动时均可调用重置方法。

## 方法

### 所有方法签名总览

| 方法完整签名 |
| :--- | 
| `public Type GetType()` |
| `public virtual bool Equals(object obj)` |
| `public virtual int GetHashCode()` |
| `public virtual string ToString()` |
| `public static void Register(Action callback)` |
| `protected object MemberwiseClone()` |
| `protected virtual void Finalize()` |

### 声明的普通方法

| 普通方法名称 | 注释 |
| :--- | :--- | 
| `public static void Register` | 注册静态变量重置回调，在 Domain Reload 时自动调用 |

### 继承的普通方法

| 普通方法名称 | 注释 | 声明方法的类 |
| :--- | :--- | :--- |
| `public Type GetType` |  | `System.Object` |
| `public virtual bool Equals` |  | `System.Object` |
| `public virtual int GetHashCode` |  | `System.Object` |
| `public virtual string ToString` |  | `System.Object` |
| `protected object MemberwiseClone` |  | `System.Object` |
| `protected virtual void Finalize` |  | `System.Object` |

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Aesir Inspector`](https://github.com/yuumixcode/Unity-Aesir-Packages) 辅助生成。
