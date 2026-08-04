# `AbstractAttributeData`

## 介绍

- 种类: `abstract class`
- 所在程序集: `Runestone.AesirInspector.OdinIntegration.Editor`
- 所在命名空间: `Runestone.AesirInspector.OdinIntegration.Editor`

``` csharp
public abstract class AbstractAttributeData
```

## 方法

### 所有方法签名总览

| 方法完整签名 |
| :--- | 
| `public ScriptableObject GetInitialExample()` |
| `public Type GetType()` |
| `public virtual bool Equals(object obj)` |
| `public virtual int GetHashCode()` |
| `public virtual string ToString()` |
| `protected object MemberwiseClone()` |
| `protected virtual void Finalize()` |

### 声明的普通方法

| 普通方法名称 | 注释 |
| :--- | :--- | 
| `public ScriptableObject GetInitialExample` | 获取初始显示的案例 ScriptableObject。 |

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
| `public AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; }` | 使用案例预览项数组。 |
| `public BilingualData[] UsageTips { get; set; }` | 使用提示数组。 |
| `public BilingualHeaderControl BilingualHeaderControl { get; set; }` | 顶部说明控件。 |
| `public ParameterValue[] AttributeParameters { get; set; }` | 特性参数数组。 |
| `public ResolvedStringParameterValue[] ResolvedStringParameters { get; set; }` | 被解析的字符串参数数组。 |

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Aesir Inspector`](https://github.com/yuumixcode/Unity-Aesir-Packages) 辅助生成。
