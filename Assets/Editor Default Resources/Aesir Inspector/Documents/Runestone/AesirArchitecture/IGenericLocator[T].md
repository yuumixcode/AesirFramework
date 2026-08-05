# `IGenericLocator<T>`

## 介绍

- 种类: `interface`
- 所在程序集: `Runestone.AesirArchitecture`
- 所在命名空间: `Runestone.AesirArchitecture`

``` csharp
public interface IGenericLocator<T> where T : class
```

### 注释

- 泛型定位器接口。提供按类型注册、查询与获取对象实例的契约。

## 方法

### 所有方法签名总览

| 方法完整签名 |
| :--- | 
| `public abstract Dictionary<Type, T> GetRegistry()` |
| `public abstract IEnumerable<T> GetAll()` |
| `public abstract T GetByType(Type type)` |
| `public abstract TItem Get<TItem>()` |
| `public abstract bool IsRegistered<TItem>()` |
| `public abstract bool TryGet<TItem>(out ref TItem instance)` |
| `public abstract void Clear()` |
| `public abstract void Register(Type type, T instance)` |
| `public abstract void Register<TItem>(TItem instance)` |
| `public abstract void Unregister<TItem>()` |

### 声明的普通方法

| 普通方法名称 | 注释 |
| :--- | :--- | 
| `public abstract Dictionary<Type, T> GetRegistry` | 获取注册表字典的只读视图。 |
| `public abstract IEnumerable<T> GetAll` | 按注册顺序获取所有已注册的实例。 |
| `public abstract T GetByType` | 按 Type 获取已注册的实例，不存在则返回 null。 用于依赖项校验等需要运行时 Type 查询的场景。 |
| `public abstract TItem Get<TItem>` | 获取已注册的实例，不存在则返回 null。 |
| `public abstract bool IsRegistered<TItem>` | 判断指定类型是否已注册。 |
| `public abstract bool TryGet<TItem>` |  |
| `public abstract void Clear` | 清空所有已注册的实例。 |
| `public abstract void Register` | 注册实例，以 Type 作为键。重复注册将覆盖已有实例。 |
| `public abstract void Register<TItem>` | 注册实例，以 typeof(TItem) 作为键。重复注册将覆盖已有实例。 注意：注册与查询必须使用相同的类型参数。若以具体类型注册（如 Register&lt;Sword&gt;）， 再以接口类型查询（如 Get&lt;IWeapon&gt;），将返回 null。 |
| `public abstract void Unregister<TItem>` | 注销指定类型的注册。 |

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Aesir Inspector`](https://github.com/yuumixcode/Unity-Aesir-Packages) 辅助生成。
