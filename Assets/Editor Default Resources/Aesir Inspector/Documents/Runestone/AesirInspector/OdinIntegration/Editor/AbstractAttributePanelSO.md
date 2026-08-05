# `AbstractAttributePanelSO`

## 介绍

- 种类: `abstract class`
- 所在程序集: `Runestone.AesirInspector.OdinIntegration.Editor`
- 所在命名空间: `Runestone.AesirInspector.OdinIntegration.Editor`

``` csharp
public abstract class AbstractAttributePanelSO : Runestone.AesirInspector.OdinIntegration.Editor.AttributeOverviewPanelSO<AbstractAttributePanelSO>, 
UnityEngine.ISerializationCallbackReceiver, 
Runestone.AesirInspector.IAesirInspectorReset
```

### 注释

- 特性介绍面板抽象基类，负责渲染顶部控件、使用提示、参数表、案例预览与代码预览。

## 方法

### 所有方法签名总览

| 方法完整签名 |
| :--- | 
| `public Type GetType()` |
| `public abstract void Initialize()` |
| `public int GetInstanceID()` |
| `public override bool Equals(object other)` |
| `public override int GetHashCode()` |
| `public override string ToString()` |
| `public override void AesirInspectorReset()` |
| `protected object MemberwiseClone()` |
| `protected virtual void Finalize()` |
| `protected virtual void OnAfterDeserialize()` |
| `protected virtual void OnBeforeSerialize()` |
| `protected void SetData(AbstractAttributeData attributeData)` |
| `public void SetDirty()` |

### 声明的普通方法

| 普通方法名称 | 注释 |
| :--- | :--- | 
| `public abstract void Initialize` | 初始化面板，子类中调用 SetData 完成数据绑定。 |
| `protected void SetData` | 子类设置数据 |

### 继承的普通方法

| 普通方法名称 | 注释 | 声明方法的类 |
| :--- | :--- | :--- |
| `public Type GetType` |  | `System.Object` |
| `public int GetInstanceID` |  | `UnityEngine.Object` |
| `public override bool Equals` |  | `UnityEngine.Object` |
| `public override int GetHashCode` |  | `UnityEngine.Object` |
| `public override string ToString` |  | `UnityEngine.Object` |
| `public override void AesirInspectorReset` | 重置面板至初始状态。 | `Runestone.AesirInspector.OdinIntegration.Editor.AbstractAttributePanelSO` |
| `protected object MemberwiseClone` |  | `System.Object` |
| `protected virtual void Finalize` |  | `System.Object` |
| `protected virtual void OnAfterDeserialize` |  | `Sirenix.OdinInspector.SerializedScriptableObject` |
| `protected virtual void OnBeforeSerialize` |  | `Sirenix.OdinInspector.SerializedScriptableObject` |
| `public void SetDirty` |  | `UnityEngine.ScriptableObject` |

## 属性

### 声明的属性

| 属性签名 | 注释 |
| :--- | :--- |
| `public BilingualHeaderControl BilingualHeaderControl { get; }` | 顶部说明控件引用。 |
| `public ScriptableObject CurrentSelectedExample { get; set; }` | 当前选中的示例对象。 |

### 继承的属性

| 属性签名 | 注释 | 声明属性的类 | 
| :--- | :--- | :--- |
| `public HideFlags hideFlags { get; set; }` |  | `UnityEngine.Object` |
| `public string name { get; set; }` |  | `UnityEngine.Object` |

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Aesir Inspector`](https://github.com/yuumixcode/Unity-Aesir-Packages) 辅助生成。
