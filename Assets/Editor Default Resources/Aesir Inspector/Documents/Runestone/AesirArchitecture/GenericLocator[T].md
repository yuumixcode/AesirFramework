# `GenericLocator<T>`

## 介绍

- 种类: `class`
- 所在程序集: `Runestone.AesirArchitecture`
- 所在命名空间: `Runestone.AesirArchitecture`

``` csharp
[Serializable]
public sealed class GenericLocator<T> : Runestone.AesirArchitecture.IGenericLocator<T>, 
System.IDisposable where T : class
```

### 注释

- 泛型对象定位器。按类型注册、查询与获取以 为基类的对象实例。 纯 C# 实现，可作为独立实例使用，也可通过 Global 访问全局单例。

## 构造方法

| 构造方法签名 [仅包含公共实例方法] | 注释 |
| :--- | :--- |
| `public GenericLocator<T>()` |  |

## 方法

### 所有方法签名总览

| 方法完整签名 |
| :--- | 
| `public Dictionary<Type, T> GetRegistry()` |
| `public IEnumerable<T> GetAll()` |
| `public T GetByType(Type type)` |
| `public TItem Get<TItem>()` |
| `public Type GetType()` |
| `public bool IsRegistered<TItem>()` |
| `public bool TryGet<TItem>(out ref TItem instance)` |
| `public virtual bool Equals(object obj)` |
| `public virtual int GetHashCode()` |
| `public virtual string ToString()` |
| `public void Clear()` |
| `public void Dispose()` |
| `public void Register(Type type, T instance)` |
| `public void Register<TItem>(TItem instance)` |
| `public void Unregister<TItem>()` |
| `protected object MemberwiseClone()` |
| `protected virtual void Finalize()` |

### 声明的普通方法

| 普通方法名称 | 注释 |
| :--- | :--- | 
| `public Dictionary<Type, T> GetRegistry` | 获取底层注册表字典 |
| `public IEnumerable<T> GetAll` | 获取所有已注册的实例集合 |
| `public T GetByType` | 按 Type 获取实例（非泛型版本） |
| `public TItem Get<TItem>` | 获取指定类型的实例。如果不存在，返回 null。 |
| `public bool IsRegistered<TItem>` | 检查是否已注册指定类型的实例 |
| `public bool TryGet<TItem>` |  |
| `public void Clear` | 清空所有已注册的实例 |
| `public void Dispose` | 释放资源，清空所有注册。若当前实例为全局实例，则同时清除全局引用。 |
| `public void Register` | 按显式指定的类型注册一个实例 |
| `public void Register<TItem>` | 注册一个实例。如果类型已存在，则覆盖原有注册。 |
| `public void Unregister<TItem>` | 注销指定类型的实例 |

### 继承的普通方法

| 普通方法名称 | 注释 | 声明方法的类 |
| :--- | :--- | :--- |
| `public Type GetType` |  | `System.Object` |
| `public virtual bool Equals` |  | `System.Object` |
| `public virtual int GetHashCode` |  | `System.Object` |
| `public virtual string ToString` |  | `System.Object` |
| `protected object MemberwiseClone` |  | `System.Object` |
| `protected virtual void Finalize` |  | `System.Object` |

## 属性

### 声明的属性

| 属性签名 | 注释 |
| :--- | :--- |
| `public static GenericLocator<T> Global { get; }` | 获取全局定位器实例。首次访问时懒初始化。 |

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Aesir Inspector`](https://github.com/yuumixcode/Unity-Aesir-Packages) 辅助生成。
