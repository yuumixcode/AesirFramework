---
title: SceneModuleUniTask
description: "Runestone.AesirModules.SceneModuleUniTask 的 API 文档"
---

# `SceneModuleUniTask`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules.UniTask`

**继承链:** `System.Object` → `SceneModuleUniTask`

## 声明

``` csharp
public static class SceneModuleUniTask
```

SceneModule 的 UniTask 适配 API——游戏工程包含 UniTask 时（宏 AESIR_MODULES_UNITASK 自动维护）， 提供可 await 的场景加载/卸载流程。本类型位于独立适配程序集 Runestone.AesirModules.UniTask，未包含 UniTask 时整体不编译，公开 API 与 SceneModule 静态门面一一对应。
失败语义：场景路径无效、引用无效（不在 BuildSettings）、Addressable 场景等失败情形， await 侧抛出 InvalidOperationException——具体原因已由 SceneModule 输出到 Console。

取消语义：取消仅中止等待（await 侧抛 OperationCanceledException）， 底层加载/卸载流程继续完成（Unity 场景操作不支持中途取消）；SceneModule 宿主被销毁时 内部流程静默中止，等待方同样以取消收场（不会无限悬挂）。

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`LoadSceneAdditiveAsync(SceneAssetWrapper, Action<float>, CancellationToken)`](#method-loadsceneadditiveasync-sceneassetwrapper-action-float-cancellationtoken) | 加载场景并等待完成。Additive 模式。通过 SceneAssetWrapper 指定场景。 引用无效（空/不在 BuildSettings）或为 Addressable 场景时抛出 InvalidOperationException。 |
| [`LoadSceneAdditiveAsync(string, Action<float>, CancellationToken)`](#method-loadsceneadditiveasync-string-action-float-cancellationtoken) | 加载场景并等待完成。Additive 模式：纯叠加、不改变激活场景（对齐 Unity 原生语义），并记入叠加追踪。 约定：请勿对同一路径重复叠加加载——Unity 会加载两个场景实例，而追踪列表按路径粒度只记录一次。 |
| [`LoadSceneSingleAsync(SceneAssetWrapper, Action<float>, CancellationToken)`](#method-loadscenesingleasync-sceneassetwrapper-action-float-cancellationtoken) | 加载场景并等待完成。Single 模式。通过 SceneAssetWrapper 指定场景。 引用无效（空/不在 BuildSettings）或为 Addressable 场景时抛出 InvalidOperationException。 |
| [`LoadSceneSingleAsync(string, Action<float>, CancellationToken)`](#method-loadscenesingleasync-string-action-float-cancellationtoken) | 加载场景并等待完成。Single 模式：卸载全部场景、重设激活场景、加载成功后清空叠加追踪（失败时保留）。 |
| [`ReloadSceneAsync(CancellationToken)`](#method-reloadsceneasync-cancellationtoken) | 重新加载当前激活场景并等待完成。异步 Single 模式，加载成功后清空叠加场景追踪。 编辑器中激活场景尚未保存（无有效路径）时抛出 InvalidOperationException。 |
| [`UnloadAllAddedScenesAsync(CancellationToken)`](#method-unloadalladdedscenesasync-cancellationtoken) | 卸载所有经本模块叠加加载的场景并等待完成。单个场景卸载失败（场景已被外部卸载）时跳过并告警，不影响其余场景。 |
| [`UnloadSceneAsync(SceneAssetWrapper, CancellationToken)`](#method-unloadsceneasync-sceneassetwrapper-cancellationtoken) | 卸载场景并等待完成。通过 SceneAssetWrapper 指定场景。 |
| [`UnloadSceneAsync(string, CancellationToken)`](#method-unloadsceneasync-string-cancellationtoken) | 卸载场景并等待完成。若该场景在叠加追踪列表中则自动移出。 场景不存在等失败情形抛出 InvalidOperationException。 |

</div>

**继承的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `Equals(object)` | — | `object` |
| `GetHashCode()` | — | `object` |
| `ToString()` | — | `object` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |

</div>

### LoadSceneAdditiveAsync(SceneAssetWrapper, Action<float>, CancellationToken) {#method-loadsceneadditiveasync-sceneassetwrapper-action-float-cancellationtoken}

加载场景并等待完成。Additive 模式。通过 SceneAssetWrapper 指定场景。 引用无效（空/不在 BuildSettings）或为 Addressable 场景时抛出 InvalidOperationException。

``` csharp
public static UniTask LoadSceneAdditiveAsync(SceneAssetWrapper sceneRef, Action<float> onProgress = null, CancellationToken cancellationToken = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `sceneRef` | `SceneAssetWrapper` | — |
| `onProgress` | `Action<float>` | — |
| `cancellationToken` | `CancellationToken` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `UniTask` | — |

</div>

### LoadSceneAdditiveAsync(string, Action<float>, CancellationToken) {#method-loadsceneadditiveasync-string-action-float-cancellationtoken}

加载场景并等待完成。Additive 模式：纯叠加、不改变激活场景（对齐 Unity 原生语义），并记入叠加追踪。
约定：请勿对同一路径重复叠加加载——Unity 会加载两个场景实例，而追踪列表按路径粒度只记录一次。

``` csharp
public static UniTask LoadSceneAdditiveAsync(string scenePath, Action<float> onProgress = null, CancellationToken cancellationToken = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `scenePath` | `string` | — |
| `onProgress` | `Action<float>` | — |
| `cancellationToken` | `CancellationToken` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `UniTask` | — |

</div>

### LoadSceneSingleAsync(SceneAssetWrapper, Action<float>, CancellationToken) {#method-loadscenesingleasync-sceneassetwrapper-action-float-cancellationtoken}

加载场景并等待完成。Single 模式。通过 SceneAssetWrapper 指定场景。 引用无效（空/不在 BuildSettings）或为 Addressable 场景时抛出 InvalidOperationException。

``` csharp
public static UniTask LoadSceneSingleAsync(SceneAssetWrapper sceneRef, Action<float> onProgress = null, CancellationToken cancellationToken = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `sceneRef` | `SceneAssetWrapper` | — |
| `onProgress` | `Action<float>` | 逐帧进度回调（0-1，已按 SceneModuleConfigSO.progressCap 归一化，默认 0.9），随等待期间持续报告。 |
| `cancellationToken` | `CancellationToken` | 取消令牌（仅中止等待，流程本身继续完成）。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `UniTask` | — |

</div>

### LoadSceneSingleAsync(string, Action<float>, CancellationToken) {#method-loadscenesingleasync-string-action-float-cancellationtoken}

加载场景并等待完成。Single 模式：卸载全部场景、重设激活场景、加载成功后清空叠加追踪（失败时保留）。

``` csharp
public static UniTask LoadSceneSingleAsync(string scenePath, Action<float> onProgress = null, CancellationToken cancellationToken = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `scenePath` | `string` | 场景路径（须已登记 BuildSettings）。 |
| `onProgress` | `Action<float>` | 逐帧进度回调（0-1，已按 SceneModuleConfigSO.progressCap 归一化，默认 0.9），随等待期间持续报告。 |
| `cancellationToken` | `CancellationToken` | 取消令牌（仅中止等待，流程本身继续完成）。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `UniTask` | — |

</div>

### ReloadSceneAsync(CancellationToken) {#method-reloadsceneasync-cancellationtoken}

重新加载当前激活场景并等待完成。异步 Single 模式，加载成功后清空叠加场景追踪。 编辑器中激活场景尚未保存（无有效路径）时抛出 InvalidOperationException。

``` csharp
public static UniTask ReloadSceneAsync(CancellationToken cancellationToken = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `cancellationToken` | `CancellationToken` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `UniTask` | — |

</div>

### UnloadAllAddedScenesAsync(CancellationToken) {#method-unloadalladdedscenesasync-cancellationtoken}

卸载所有经本模块叠加加载的场景并等待完成。单个场景卸载失败（场景已被外部卸载）时跳过并告警，不影响其余场景。

``` csharp
public static UniTask UnloadAllAddedScenesAsync(CancellationToken cancellationToken = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `cancellationToken` | `CancellationToken` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `UniTask` | — |

</div>

### UnloadSceneAsync(SceneAssetWrapper, CancellationToken) {#method-unloadsceneasync-sceneassetwrapper-cancellationtoken}

卸载场景并等待完成。通过 SceneAssetWrapper 指定场景。

``` csharp
public static UniTask UnloadSceneAsync(SceneAssetWrapper sceneRef, CancellationToken cancellationToken = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `sceneRef` | `SceneAssetWrapper` | — |
| `cancellationToken` | `CancellationToken` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `UniTask` | — |

</div>

### UnloadSceneAsync(string, CancellationToken) {#method-unloadsceneasync-string-cancellationtoken}

卸载场景并等待完成。若该场景在叠加追踪列表中则自动移出。 场景不存在等失败情形抛出 InvalidOperationException。

``` csharp
public static UniTask UnloadSceneAsync(string scenePath, CancellationToken cancellationToken = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `scenePath` | `string` | — |
| `cancellationToken` | `CancellationToken` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `UniTask` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
