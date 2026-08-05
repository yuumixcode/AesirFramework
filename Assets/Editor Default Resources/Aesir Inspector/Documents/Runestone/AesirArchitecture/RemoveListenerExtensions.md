# `RemoveListenerExtensions`

## 介绍

- 种类: `static class`
- 所在程序集: `Runestone.AesirArchitecture`
- 所在命名空间: `Runestone.AesirArchitecture`

``` csharp
[Extension]
public static class RemoveListenerExtensions
```

### 注释

- 事件监听器自动移除扩展方法类，用于绑定移除操作到 Unity 生命周期

## 方法

### 所有方法签名总览

| 方法完整签名 |
| :--- | 
| `public Type GetType()` |
| `public virtual bool Equals(object obj)` |
| `public virtual int GetHashCode()` |
| `public virtual string ToString()` |
| `[Overload] [Overload] [Ext] public static void RemoveListenerWhenGameObjectOnDestroyed(this AutoRemoveListenerHandle removeListener, GameObject gameObject)` |
| `[Overload] [Overload] [Ext] public static void RemoveListenerWhenGameObjectOnDestroyed(this AutoRemoveListenerHandle removeListener, MonoBehaviour mono)` |
| `[Overload] [Overload] [Ext] public static void RemoveListenerWhenGameObjectOnDisable(this AutoRemoveListenerHandle removeListener, GameObject gameObject)` |
| `[Overload] [Overload] [Ext] public static void RemoveListenerWhenGameObjectOnDisable(this AutoRemoveListenerHandle removeListener, MonoBehaviour mono)` |
| `[Ext] public static void RemoveListenerWhenOnSceneUnloaded(this AutoRemoveListenerHandle removeListener)` |
| `protected object MemberwiseClone()` |
| `protected virtual void Finalize()` |

### 声明的普通方法

| 普通方法名称 | 注释 |
| :--- | :--- | 
| `[Ext] public static void RemoveListenerWhenGameObjectOnDestroyed` | 当指定的 GameObject 被销毁时自动移除监听 |
| `[Ext] public static void RemoveListenerWhenGameObjectOnDestroyed` | 当指定的 GameObject 被销毁时自动移除监听 |
| `[Ext] public static void RemoveListenerWhenGameObjectOnDisable` | 当指定的 GameObject 被禁用（OnDisable）时自动移除监听 |
| `[Ext] public static void RemoveListenerWhenGameObjectOnDisable` | 当指定的 GameObject 被禁用（OnDisable）时自动移除监听 |
| `[Ext] public static void RemoveListenerWhenOnSceneUnloaded` | 当场景卸载时自动移除监听 |

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
