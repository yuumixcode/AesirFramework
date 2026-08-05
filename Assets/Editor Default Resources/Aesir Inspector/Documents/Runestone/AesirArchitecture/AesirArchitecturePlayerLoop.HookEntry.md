# `AesirArchitecturePlayerLoop.HookEntry`

## 介绍

- 种类: `struct`
- 所在程序集: `Runestone.AesirArchitecture`
- 所在命名空间: `Runestone.AesirArchitecture`

``` csharp
private struct AesirArchitecturePlayerLoop.HookEntry : System.ValueType
```

### 注释

- 基于 PlayerLoop 的生命周期钩子系统，无需 MonoBehaviour 即可接入游戏级帧回调。 通过 Register 注册回调，order 越小越先执行；系统自动在域加载时注入 PlayerLoop。

## 方法

### 所有方法签名总览

| 方法完整签名 |
| :--- | 
| `public Type GetType()` |
| `public override bool Equals(object obj)` |
| `public override int GetHashCode()` |
| `public override string ToString()` |
| `protected object MemberwiseClone()` |
| `protected virtual void Finalize()` |

### 继承的普通方法

| 普通方法名称 | 注释 | 声明方法的类 |
| :--- | :--- | :--- |
| `public Type GetType` |  | `System.Object` |
| `public override bool Equals` |  | `System.ValueType` |
| `public override int GetHashCode` |  | `System.ValueType` |
| `public override string ToString` |  | `System.ValueType` |
| `protected object MemberwiseClone` |  | `System.Object` |
| `protected virtual void Finalize` |  | `System.Object` |

## 字段

### 声明的普通字段

| 字段名称 | 注释 | 
| :--- | :--- | 
| `public Action Callback;` |  |
| `public int Order;` |  |
| `public long InsertionIndex;` |  |

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Aesir Inspector`](https://github.com/yuumixcode/Unity-Aesir-Packages) 辅助生成。
