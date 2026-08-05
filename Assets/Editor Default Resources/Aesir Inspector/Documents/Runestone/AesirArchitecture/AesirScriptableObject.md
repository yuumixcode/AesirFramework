# `AesirScriptableObject`

## 介绍

- 种类: `abstract class`
- 所在程序集: `Runestone.AesirArchitecture`
- 所在命名空间: `Runestone.AesirArchitecture`

``` csharp
public abstract class AesirScriptableObject : Sirenix.OdinInspector.SerializedScriptableObject, 
UnityEngine.ISerializationCallbackReceiver
```

### 注释

- RAA 框架标准 ScriptableObject 基类，根据运行环境自动选择序列化方式。

## 方法

### 所有方法签名总览

| 方法完整签名 |
| :--- | 
| `public Type GetType()` |
| `public int GetInstanceID()` |
| `public override bool Equals(object other)` |
| `public override int GetHashCode()` |
| `public override string ToString()` |
| `protected object MemberwiseClone()` |
| `protected virtual void Finalize()` |
| `protected virtual void OnAfterDeserialize()` |
| `protected virtual void OnBeforeSerialize()` |
| `public void SetDirty()` |

### 继承的普通方法

| 普通方法名称 | 注释 | 声明方法的类 |
| :--- | :--- | :--- |
| `public Type GetType` |  | `System.Object` |
| `public int GetInstanceID` |  | `UnityEngine.Object` |
| `public override bool Equals` |  | `UnityEngine.Object` |
| `public override int GetHashCode` |  | `UnityEngine.Object` |
| `public override string ToString` |  | `UnityEngine.Object` |
| `protected object MemberwiseClone` |  | `System.Object` |
| `protected virtual void Finalize` |  | `System.Object` |
| `protected virtual void OnAfterDeserialize` |  | `Sirenix.OdinInspector.SerializedScriptableObject` |
| `protected virtual void OnBeforeSerialize` |  | `Sirenix.OdinInspector.SerializedScriptableObject` |
| `public void SetDirty` |  | `UnityEngine.ScriptableObject` |

## 属性

### 继承的属性

| 属性签名 | 注释 | 声明属性的类 | 
| :--- | :--- | :--- |
| `public HideFlags hideFlags { get; set; }` |  | `UnityEngine.Object` |
| `public string name { get; set; }` |  | `UnityEngine.Object` |

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Aesir Inspector`](https://github.com/yuumixcode/Unity-Aesir-Packages) 辅助生成。
