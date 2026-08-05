# `RemoveListenerHandleCollection`

## 介绍

- 种类: `class`
- 所在程序集: `Runestone.AesirArchitecture`
- 所在命名空间: `Runestone.AesirArchitecture`

``` csharp
public sealed class RemoveListenerHandleCollection
```

### 注释

- 监听句柄集合。管理 AutoRemoveListenerHandle 句柄的添加与批量移除， 供 RemoveListenerTrigger 和 RemoveListenerOnSceneUnloadedTrigger 复用。

## 构造方法

| 构造方法签名 [仅包含公共实例方法] | 注释 |
| :--- | :--- |
| `public RemoveListenerHandleCollection()` |  |

## 方法

### 所有方法签名总览

| 方法完整签名 |
| :--- | 
| `public Type GetType()` |
| `public virtual bool Equals(object obj)` |
| `public virtual int GetHashCode()` |
| `public virtual string ToString()` |
| `public void Add(AutoRemoveListenerHandle handle)` |
| `public void RemoveAllListeners()` |
| `protected object MemberwiseClone()` |
| `protected virtual void Finalize()` |

### 声明的普通方法

| 普通方法名称 | 注释 |
| :--- | :--- | 
| `public void Add` | 添加监听句柄，使其在调用条件满足时自动移除 |
| `public void RemoveAllListeners` | 移除所有已注册的监听并清空列表 |

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
