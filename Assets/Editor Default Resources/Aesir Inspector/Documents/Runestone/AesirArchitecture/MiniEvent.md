# `MiniEvent`

## 介绍

- 种类: `class`
- 所在程序集: `Runestone.AesirArchitecture`
- 所在命名空间: `Runestone.AesirArchitecture`

``` csharp
public sealed class MiniEvent : System.IDisposable
```

### 注释

- 单参事件

## 构造方法

| 构造方法签名 [仅包含公共实例方法] | 注释 |
| :--- | :--- |
| `public MiniEvent()` |  |

## 方法

### 所有方法签名总览

| 方法完整签名 |
| :--- | 
| `public AutoRemoveListenerHandle AddListener(Action listener)` |
| `public Delegate[] GetListeners()` |
| `public Type GetType()` |
| `public virtual bool Equals(object obj)` |
| `public virtual int GetHashCode()` |
| `public virtual string ToString()` |
| `public void Dispose()` |
| `public void Invoke()` |
| `public void RemoveListener(Action listener)` |
| `protected object MemberwiseClone()` |
| `protected virtual void Finalize()` |

### 声明的普通方法

| 普通方法名称 | 注释 |
| :--- | :--- | 
| `public AutoRemoveListenerHandle AddListener` | 添加监听者，并返回可自动移除的监听句柄 |
| `public Delegate[] GetListeners` | 获取当前所有已注册的委托列表 |
| `public void Dispose` | 清空所有委托引用，释放内存 |
| `public void Invoke` | 调用事件，通知所有监听者 |
| `public void RemoveListener` | 移除监听者 |

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
