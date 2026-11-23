---
title: SceneAssetWrapperAddressablesBridge
description: "Runestone.AesirModules.SceneAssetWrapperAddressablesBridge 的 API 文档"
---

# `SceneAssetWrapperAddressablesBridge`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `SceneAssetWrapperAddressablesBridge`

## 声明

``` csharp
public static class SceneAssetWrapperAddressablesBridge
```

Addressables 编辑器能力的静态桥。
核心程序集不引用任何 Addressables 程序集；由可选程序集 Runestone.AesirModules.Editor.Addressables（仅当项目安装了 Addressables 包时才参与编译） 在编辑器加载时把能力注册进来。未注册时所有 Addressables 编辑器功能自动隐藏， 不产生任何编译错误——这是"项目中没有 Addressables 时相关代码整体不编译"约束的运行时侧落点。

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`GetAddressHandler`](#property-getaddresshandler) | 地址查询委托：入参为场景资产路径，返回其在 Addressables 中的地址；不可寻址或失败时返回 null。 |
| [`MakeAddressableHandler`](#property-makeaddressablehandler) | 加入默认组委托：入参为场景资产路径，成功时返回新地址（Addressables 默认寻址下通常等于资产路径），失败返回 null。 |
| [`IsAvailable`](#property-isavailable) | 桥是否已注册（即项目安装了 Addressables 包且胶水程序集已参与编译）。 |

</div>

### GetAddressHandler {#property-getaddresshandler}

地址查询委托：入参为场景资产路径，返回其在 Addressables 中的地址；不可寻址或失败时返回 null。

``` csharp
public static Func<string, string> GetAddressHandler { get; private set; }
```

### MakeAddressableHandler {#property-makeaddressablehandler}

加入默认组委托：入参为场景资产路径，成功时返回新地址（Addressables 默认寻址下通常等于资产路径），失败返回 null。

``` csharp
public static Func<string, string> MakeAddressableHandler { get; private set; }
```

### IsAvailable {#property-isavailable}

桥是否已注册（即项目安装了 Addressables 包且胶水程序集已参与编译）。

``` csharp
public static bool IsAvailable { get; }
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Register(Func<string, string>, Func<string, string>)`](#method-register-func-string-string-func-string-string) | 注册桥接能力。由 Runestone.AesirModules.Editor.Addressables 程序集的 [InitializeOnLoad] 调用。 |
| [`Unregister()`](#method-unregister) | 注销桥接能力。域重载后静态委托会被清空，胶水程序集会随 [InitializeOnLoad] 重新注册； 单测用它来隔离用例间的桥状态。 |

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

### Register(Func<string, string>, Func<string, string>) {#method-register-func-string-string-func-string-string}

注册桥接能力。由 Runestone.AesirModules.Editor.Addressables 程序集的 [InitializeOnLoad] 调用。

``` csharp
public static void Register(Func<string, string> getAddress, Func<string, string> makeAddressable)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `getAddress` | `Func<string, string>` | — |
| `makeAddressable` | `Func<string, string>` | — |

</div>

### Unregister() {#method-unregister}

注销桥接能力。域重载后静态委托会被清空，胶水程序集会随 [InitializeOnLoad] 重新注册； 单测用它来隔离用例间的桥状态。

``` csharp
public static void Unregister()
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
