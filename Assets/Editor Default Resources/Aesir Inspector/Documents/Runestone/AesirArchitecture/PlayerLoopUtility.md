# `PlayerLoopUtility`

## 介绍

- 种类: `static class`
- 所在程序集: `Runestone.AesirArchitecture`
- 所在命名空间: `Runestone.AesirArchitecture`

``` csharp
public static class PlayerLoopUtility
```

### 注释

- PlayerLoop 操作的静态工具类，提供子系统的插入、查询与描述功能。 供框架内部和外部用户扩展 PlayerLoop，不局限于 AesirArchitectureLifeCyclePhase 预定义阶段。

## 方法

### 所有方法签名总览

| 方法完整签名 |
| :--- | 
| `public Type GetType()` |
| `public virtual bool Equals(object obj)` |
| `public virtual int GetHashCode()` |
| `public virtual string ToString()` |
| `public static bool ContainsSystem<TTarget>()` |
| `public static bool InsertSystemAfter<TTarget>(PlayerLoopSystem system)` |
| `public static bool InsertSystemBefore<TTarget>(PlayerLoopSystem system)` |
| `public static string GetCurrentPlayerLoopDescription()` |
| `protected object MemberwiseClone()` |
| `protected virtual void Finalize()` |

### 声明的普通方法

| 普通方法名称 | 注释 |
| :--- | :--- | 
| `public static bool ContainsSystem<TTarget>` | 检测 PlayerLoop 中是否包含指定类型的子系统 |
| `public static bool InsertSystemAfter<TTarget>` | 在 PlayerLoop 中指定子系统后插入自定义系统 |
| `public static bool InsertSystemBefore<TTarget>` | 在 PlayerLoop 中指定子系统前插入自定义系统 |
| `public static string GetCurrentPlayerLoopDescription` | 将当前 PlayerLoop 所有子系统按执行顺序输出为字符串。 Aesir Architecture 注入的子系统会以 [Aesir Architecture] 前缀标注。 |

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
