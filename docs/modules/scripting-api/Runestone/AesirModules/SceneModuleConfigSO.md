---
title: SceneModuleConfigSO
description: "Runestone.AesirModules.SceneModuleConfigSO 的 API 文档"
---

# `SceneModuleConfigSO`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `UnityEngine.Object` → `UnityEngine.ScriptableObject` → `Sirenix.OdinInspector.SerializedScriptableObject` → `Runestone.AesirArchitecture.AesirScriptableObject` → `SceneModuleConfigSO`

**实现接口:** `UnityEngine.ISerializationCallbackReceiver`

## 声明

``` csharp
public class SceneModuleConfigSO : Runestone.AesirArchitecture.AesirScriptableObject, 
UnityEngine.ISerializationCallbackReceiver
```

场景模块全局配置（单例资产）。承载无需预放置 [SceneModule] 即可调整的模块级配置： 在 Project 窗口直接编辑本资产即可生效，不再要求预放置 SceneModule 物体。

**备注**

单例 Instance 的解析顺序： RegisterConfigLoader 注册的加载器——注册后 Resources 兜底不再执行， 用于彻底放弃 Resources 的项目（如 Addressables 或代码内构造配置）； Resources 兜底（ResourcePath）；可在 Project 窗口自建资产，或经菜单 Tools/Aesir/Modules/Create Module Config Assets 显式创建（包导入时不写用户工程）； 内存默认实例——前两级均未命中时以代码默认值创建，仅存在于内存、不持久化。 加载进度上限（progressCap）在每次加载时读取，运行时修改本资产对后续加载立即生效。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`SceneModuleConfigSO()`](#constructor-scenemoduleconfigso) | — |

</div>

### SceneModuleConfigSO() {#constructor-scenemoduleconfigso}

``` csharp
public SceneModuleConfigSO()
```

## 字段

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ResourcePath`](#field-resourcepath) | Resources 兜底路径（相对任意 Resources 根，不含扩展名）。资产固定位于 Assets/Resources/SceneModuleConfig/SceneModuleConfig.asset。 |

</div>

### ResourcePath {#field-resourcepath}

Resources 兜底路径（相对任意 Resources 根，不含扩展名）。资产固定位于 Assets/Resources/SceneModuleConfig/SceneModuleConfig.asset。

``` csharp
public const string ResourcePath = "SceneModuleConfig/SceneModuleConfig";
```

## 属性

**声明的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Instance`](#property-instance) | 全局单例。按类 remarks 声明的顺序解析并缓存；缓存失效（资产被销毁的 Unity 假 null）时自动重新解析。 |

</div>

**继承的属性**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `hideFlags` | — | `Object` |
| `name` | — | `Object` |

</div>

### Instance {#property-instance}

全局单例。按类 remarks 声明的顺序解析并缓存；缓存失效（资产被销毁的 Unity 假 null）时自动重新解析。

``` csharp
public static SceneModuleConfigSO Instance { get; }
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`CreateDefault()`](#method-createdefault) | — |
| [`RegisterConfigLoader(Func<SceneModuleConfigSO>)`](#method-registerconfigloader-func-scenemoduleconfigso) | 注册配置加载器，替代 Resources 兜底（注册后 Instance 只经加载器解析）。 须在首次消费配置（场景加载或读取 BootstrapSceneAssetWrapper）之前调用， 例如 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] 或首个场景的引导脚本中。 |
| [`UnregisterConfigLoader()`](#method-unregisterconfigloader) | 注销配置加载器（幂等）。注销后 Instance 重新按 Resources 兜底解析， 已按加载器路径缓存的实例一并失效。 |

</div>

**继承的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `GetInstanceID()` | — | `Object` |
| `Equals(object)` | — | `Object` |
| `GetHashCode()` | — | `Object` |
| `ToString()` | — | `Object` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |
| `OnAfterDeserialize()` | — | `SerializedScriptableObject` |
| `OnBeforeSerialize()` | — | `SerializedScriptableObject` |
| `SetDirty()` | — | `ScriptableObject` |

</div>

### CreateDefault() {#method-createdefault}

``` csharp
public static SceneModuleConfigSO CreateDefault()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `SceneModuleConfigSO` | — |

</div>

### RegisterConfigLoader(Func<SceneModuleConfigSO>) {#method-registerconfigloader-func-scenemoduleconfigso}

注册配置加载器，替代 Resources 兜底（注册后 Instance 只经加载器解析）。 须在首次消费配置（场景加载或读取 BootstrapSceneAssetWrapper）之前调用， 例如 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] 或首个场景的引导脚本中。

``` csharp
public static void RegisterConfigLoader(Func<SceneModuleConfigSO> loader)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `loader` | `Func<SceneModuleConfigSO>` | 同步加载器；返回 null 时按内存默认配置兜底。 |

</div>

### UnregisterConfigLoader() {#method-unregisterconfigloader}

注销配置加载器（幂等）。注销后 Instance 重新按 Resources 兜底解析， 已按加载器路径缓存的实例一并失效。

``` csharp
public static void UnregisterConfigLoader()
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
