# `AesirArchitecturePlayerLoop`

## 介绍

- 种类: `static class`
- 所在程序集: `Runestone.AesirArchitecture`
- 所在命名空间: `Runestone.AesirArchitecture`

``` csharp
public static class AesirArchitecturePlayerLoop
```

### 注释

- 基于 PlayerLoop 的生命周期钩子系统，无需 MonoBehaviour 即可接入游戏级帧回调。 通过 Register 注册回调，order 越小越先执行；系统自动在域加载时注入 PlayerLoop。

## 方法

### 所有方法签名总览

| 方法完整签名 |
| :--- | 
| `public Type GetType()` |
| `public virtual bool Equals(object obj)` |
| `public virtual int GetHashCode()` |
| `public virtual string ToString()` |
| `public static int GetHookCount(AesirArchitectureLifeCyclePhase phase)` |
| `public static void Register(AesirArchitectureLifeCyclePhase phase, Action callback, int order = 0)` |
| `public static void Reset()` |
| `public static void Unregister(AesirArchitectureLifeCyclePhase phase, Action callback)` |
| `protected object MemberwiseClone()` |
| `protected virtual void Finalize()` |

### 声明的普通方法

| 普通方法名称 | 注释 |
| :--- | :--- | 
| `public static int GetHookCount` | 获取指定阶段的已注册回调数量 |
| `public static void Register` | 注册回调，order 越小越先执行，默认 0。 回调持有者销毁前必须调用 Unregister 注销；若未注销，回调将永久残留并阻止目标对象被回收。 |
| `public static void Reset` | 清空所有回调 |
| `public static void Unregister` | 注销回调。 必须传入注册时的同一委托实例，匿名函数无法通过此方法注销。 |

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
