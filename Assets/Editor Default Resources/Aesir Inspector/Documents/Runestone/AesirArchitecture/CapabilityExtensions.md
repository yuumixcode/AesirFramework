# `CapabilityExtensions`

## 介绍

- 种类: `static class`
- 所在程序集: `Runestone.AesirArchitecture`
- 所在命名空间: `Runestone.AesirArchitecture`

``` csharp
[Extension]
public static class CapabilityExtensions
```

### 注释

- 能力扩展方法集合

## 方法

### 所有方法签名总览

| 方法完整签名 |
| :--- | 
| `public Type GetType()` |
| `public virtual bool Equals(object obj)` |
| `public virtual int GetHashCode()` |
| `public virtual string ToString()` |
| `[Ext] public static T GetModel<T>(this ICanGetModel self)` |
| `[Ext] public static T GetService<T>(this ICanGetService self)` |
| `[Ext] public static TResult ExecuteQuery<TQuery, TResult>(this ICanExecuteQuery self)` |
| `[Ext] public static TResult ExecuteQuery<TResult>(this ICanExecuteQuery self, IQuery<TResult> query)` |
| `[Overload] [Overload] [Ext] public static void ExecuteCommand<T>(this ICanExecuteCommand self)` |
| `[Overload] [Overload] [Ext] public static void ExecuteCommand<T>(this ICanExecuteCommand self, T command)` |
| `protected object MemberwiseClone()` |
| `protected virtual void Finalize()` |

### 声明的普通方法

| 普通方法名称 | 注释 |
| :--- | :--- | 
| `[Ext] public static T GetModel<T>` | 获取已注册的 Model。若未注册则抛出包含调用者和目标类型信息的异常。 若已注册但尚未初始化，则抛出注册顺序错误或循环依赖异常。 |
| `[Ext] public static T GetService<T>` | 获取已注册的 Service。若未注册则抛出包含调用者和目标类型信息的异常。 若已注册但尚未初始化，则抛出注册顺序错误或循环依赖异常。 |
| `[Ext] public static TResult ExecuteQuery<TQuery, TResult>` | 执行无参查询 |
| `[Ext] public static TResult ExecuteQuery<TResult>` | 执行带参查询 |
| `[Ext] public static void ExecuteCommand<T>` | 执行无参命令 |
| `[Ext] public static void ExecuteCommand<T>` | 执行带参命令 |

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
