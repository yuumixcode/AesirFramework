# `AesirArchitectureDebug`

## 介绍

- 种类: `static class`
- 所在程序集: `Runestone.AesirArchitecture`
- 所在命名空间: `Runestone.AesirArchitecture`

``` csharp
public static class AesirArchitectureDebug
```

### 注释

- AesirArchitecture 内部日志工具。 所有架构模块的日志输出应走此工具，以醒目的颜色和 [AesirArchitecture] 标识区分来源。 Log/Warning 通过 [Conditional] 在打包时自动剔除；Error 始终保留。

## 方法

### 所有方法签名总览

| 方法完整签名 |
| :--- | 
| `public Type GetType()` |
| `public virtual bool Equals(object obj)` |
| `public virtual int GetHashCode()` |
| `public virtual string ToString()` |
| `[Overload] [Overload] public static void Log(object source, string message)` |
| `[Overload] [Overload] public static void Log(string message)` |
| `[Overload] [Overload] public static void LogError(object source, string message)` |
| `[Overload] [Overload] public static void LogError(string message)` |
| `[Overload] [Overload] public static void LogTestInfo(object source, string message)` |
| `[Overload] [Overload] public static void LogTestInfo(string message)` |
| `[Overload] [Overload] public static void LogWarning(object source, string message)` |
| `[Overload] [Overload] public static void LogWarning(string message)` |
| `protected object MemberwiseClone()` |
| `protected virtual void Finalize()` |

### 声明的普通方法

| 普通方法名称 | 注释 |
| :--- | :--- | 
| `public static void Log` | 输出 Log 级别消息，附带来源标识 |
| `public static void Log` | 输出 Log 级别消息 |
| `public static void LogError` | 输出 Error 级别消息，附带来源标识 |
| `public static void LogError` | 输出 Error 级别消息 |
| `public static void LogTestInfo` | 输出单元测试日志消息，附带来源标识 |
| `public static void LogTestInfo` | 输出单元测试日志消息。 仅在定义了 UNITY_INCLUDE_TESTS 的程序集中生效，非测试构建自动剔除调用。 |
| `public static void LogWarning` | 输出 Warning 级别消息，附带来源标识 |
| `public static void LogWarning` | 输出 Warning 级别消息 |

### 继承的普通方法

| 普通方法名称 | 注释 | 声明方法的类 |
| :--- | :--- | :--- |
| `public Type GetType` |  | `System.Object` |
| `public virtual bool Equals` |  | `System.Object` |
| `public virtual int GetHashCode` |  | `System.Object` |
| `public virtual string ToString` |  | `System.Object` |
| `protected object MemberwiseClone` |  | `System.Object` |
| `protected virtual void Finalize` |  | `System.Object` |

## 字段

### 常量字段

| 字段完整签名 | 注释 |
| :--- | :--- |
| `public const string ErrorTag = "<color=#FF4444><b>[AesirArchitecture]</b></color>";` | Error 级别的富文本标签，供异常消息复用以保持控制台输出风格一致。 |
| `private const string Tag = "<color=#00FF88><b>[AesirArchitecture]</b></color>";` |  |
| `private const string TagError = "<color=#FF4444><b>[AesirArchitecture]</b></color>";` |  |
| `private const string TagTest = "<color=#00BFFF><b>[AesirArchitectureTest]</b></color>";` |  |
| `private const string TagWarning = "<color=#FFA500><b>[AesirArchitecture]</b></color>";` |  |

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Aesir Inspector`](https://github.com/yuumixcode/Unity-Aesir-Packages) 辅助生成。
