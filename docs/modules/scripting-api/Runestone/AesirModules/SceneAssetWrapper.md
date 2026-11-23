---
title: SceneAssetWrapper
description: "Runestone.AesirModules.SceneAssetWrapper 的 API 文档"
---

# `SceneAssetWrapper`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirModules`
    - **程序集:** `Runestone.AesirModules`

**继承链:** `System.Object` → `SceneAssetWrapper`

**实现接口:** `System.IEquatable<SceneAssetWrapper>`

## 声明

``` csharp
[Serializable]
public class SceneAssetWrapper : System.IEquatable<SceneAssetWrapper>
```

可序列化的场景引用，支持在编辑器中拖拽 SceneAsset 赋值，等价于 Eflatun.SceneReference（SceneReference 类型）+ Odin Inspector 的组合体。
设计要点： 编辑器侧以 SceneAsset 对象引用为数据源，每次访问自动同步路径、GUID、 Addressables 地址三个缓存字段；场景移动/重命名由 Unity 的对象引用机制自动重定向， 对象引用意外丢失时用序列化的 GUID 自愈路径。 运行时侧不依赖任何编辑器 API 与 Addressables 程序集，直接使用序列化的纯字符串数据。 遵循最小惊讶原则：Addressables 相关 API 在未安装 Addressables 包时依旧可见、可编译， 卸载包不会导致任何编译错误；运行期访问 Address 会抛出 AddressablesSupportDisabledException。 校验语义对位 Eflatun.SceneReference：先查 State / UnsafeReason，或直接用 TryGetScenePath 等 TryGet 家族； 未分配场景的访问器按 fail-fast 约定抛 EmptySceneAssetWrapperException。

Odin Inspector 边界：Inspector 拖拽赋值、三态着色与一键修复按钮等面板效果 依赖 Odin Inspector（经 AttributeProcessor 注入）。未安装 Odin 时仅保证 API 可用—— 用 FromScenePath 构造、SceneAsset 代码赋值（仅编辑器） 与 TryGet 家族完成全部操作，面板不支持。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`SceneAssetWrapper()`](#constructor-sceneassetwrapper) | 创建一个空引用（未分配任何场景）的包装器。永不抛异常。 |

</div>

### SceneAssetWrapper() {#constructor-sceneassetwrapper}

创建一个空引用（未分配任何场景）的包装器。永不抛异常。

``` csharp
public SceneAssetWrapper()
```

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`LoadedScene`](#property-loadedscene) | 场景的 Scene 结构。场景未加载时返回的结构无效， 用 IsValid 判断（与 Eflatun.SceneReference 同语义）。 |
| [`SceneAsset`](#property-sceneasset) | 编辑器中拖拽的 SceneAsset（数据源）。赋值时自动同步路径/GUID/Addressables 地址。 |
| [`State`](#property-state) | 引用的可用状态。安全（Regular/Addressable）只保证"有一条可行的加载途径"， 并不要求场景当前已加载——是否已加载请查 LoadedScene。 |
| [`UnsafeReason`](#property-unsafereason) | 引用不安全的具体原因。Empty 优先级最高；Addressable 场景即使不在 BuildSettings 也视为安全。 |
| [`CanAddToBuild`](#property-canaddtobuild) | 是否显示"添加到 BuildSettings"修复按钮。 |
| [`CanEnableInBuild`](#property-canenableinbuild) | 是否显示"在 BuildSettings 中启用"修复按钮。 |
| [`CanMakeAddressable`](#property-canmakeaddressable) | 是否显示"加入 Addressables"修复按钮（需要安装 Addressables 包且场景当前不可寻址）。 |
| [`DisabledInBuildSettings`](#property-disabledinbuildsettings) | 场景在 BuildSettings 中存在条目但被禁用（编辑器实时检测）。 |
| [`IsAddressable`](#property-isaddressable) | 此场景是否为 Addressable 场景。 编辑器下（桥已注册）实时核验并回写缓存；运行时按序列化数据判定。 |
| [`IsDangling`](#property-isdangling) | 场景引用已悬空：SceneAsset 对象引用丢失，且经 GUID 自愈回写的路径也定位不到场景资产 （资产被删除或移出工程）。Inspector 中以红色提示，可通过重新拖拽场景修复。 对象引用断链但资产仍可经路径定位（场景被移动后的 GUID 自愈成功）时不算悬空—— 加载完全可用，只是面板上建议重新拖拽一次以恢复对象引用。 |
| [`MissingFromBuild`](#property-missingfrombuild) | 场景完全不在 BuildSettings 中（既没有启用条目，也没有禁用条目）。 |
| [`NotInBuildSettings`](#property-notinbuildsettings) | 场景不在 BuildSettings 中时返回 true（含"已加入但被禁用"）；空引用返回 false。 |
| [`BuildIndex`](#property-buildindex) | 场景在 BuildSettings 中的序号。未加入（或被禁用）时返回 -1，不抛异常。 |
| [`Address`](#property-address) | 场景在 Addressables 中的地址，供 Addressables.LoadSceneAsync 使用。 |
| [`Guid`](#property-guid) | 场景资产 GUID。 |
| [`SceneName`](#property-scenename) | 场景名称（不包含扩展名）。 |
| [`ScenePath`](#property-scenepath) | 场景相对路径，包含后缀名。 |
| [`AddressablesSupportEnabled`](#property-addressablessupportenabled) | 项目是否安装了 Addressables 包。由 asmdef 的 versionDefines 宏 AESIR_MODULES_ADDRESSABLES 驱动，包缺失时相关代码不会编译，但 API 始终可见。 |

</div>

### LoadedScene {#property-loadedscene}

场景的 Scene 结构。场景未加载时返回的结构无效， 用 IsValid 判断（与 Eflatun.SceneReference 同语义）。

``` csharp
public Scene LoadedScene { get; }
```

### SceneAsset {#property-sceneasset}

编辑器中拖拽的 SceneAsset（数据源）。赋值时自动同步路径/GUID/Addressables 地址。

``` csharp
public SceneAsset SceneAsset { get; set; }
```

### State {#property-state}

引用的可用状态。安全（Regular/Addressable）只保证"有一条可行的加载途径"， 并不要求场景当前已加载——是否已加载请查 LoadedScene。

``` csharp
public SceneAssetWrapperState State { get; }
```

### UnsafeReason {#property-unsafereason}

引用不安全的具体原因。Empty 优先级最高；Addressable 场景即使不在 BuildSettings 也视为安全。

``` csharp
public SceneAssetWrapperUnsafeReason UnsafeReason { get; }
```

### CanAddToBuild {#property-canaddtobuild}

是否显示"添加到 BuildSettings"修复按钮。

``` csharp
public bool CanAddToBuild { get; }
```

### CanEnableInBuild {#property-canenableinbuild}

是否显示"在 BuildSettings 中启用"修复按钮。

``` csharp
public bool CanEnableInBuild { get; }
```

### CanMakeAddressable {#property-canmakeaddressable}

是否显示"加入 Addressables"修复按钮（需要安装 Addressables 包且场景当前不可寻址）。

``` csharp
public bool CanMakeAddressable { get; }
```

### DisabledInBuildSettings {#property-disabledinbuildsettings}

场景在 BuildSettings 中存在条目但被禁用（编辑器实时检测）。

``` csharp
public bool DisabledInBuildSettings { get; }
```

### IsAddressable {#property-isaddressable}

此场景是否为 Addressable 场景。 编辑器下（桥已注册）实时核验并回写缓存；运行时按序列化数据判定。

``` csharp
public bool IsAddressable { get; }
```

### IsDangling {#property-isdangling}

场景引用已悬空：SceneAsset 对象引用丢失，且经 GUID 自愈回写的路径也定位不到场景资产 （资产被删除或移出工程）。Inspector 中以红色提示，可通过重新拖拽场景修复。
对象引用断链但资产仍可经路径定位（场景被移动后的 GUID 自愈成功）时不算悬空—— 加载完全可用，只是面板上建议重新拖拽一次以恢复对象引用。

``` csharp
public bool IsDangling { get; }
```

### MissingFromBuild {#property-missingfrombuild}

场景完全不在 BuildSettings 中（既没有启用条目，也没有禁用条目）。

``` csharp
public bool MissingFromBuild { get; }
```

### NotInBuildSettings {#property-notinbuildsettings}

场景不在 BuildSettings 中时返回 true（含"已加入但被禁用"）；空引用返回 false。

``` csharp
public bool NotInBuildSettings { get; }
```

### BuildIndex {#property-buildindex}

场景在 BuildSettings 中的序号。未加入（或被禁用）时返回 -1，不抛异常。

``` csharp
public int BuildIndex { get; }
```

### Address {#property-address}

场景在 Addressables 中的地址，供 Addressables.LoadSceneAsync 使用。

``` csharp
public string Address { get; }
```

### Guid {#property-guid}

场景资产 GUID。

``` csharp
public string Guid { get; }
```

### SceneName {#property-scenename}

场景名称（不包含扩展名）。

``` csharp
public string SceneName { get; }
```

### ScenePath {#property-scenepath}

场景相对路径，包含后缀名。

``` csharp
public string ScenePath { get; }
```

### AddressablesSupportEnabled {#property-addressablessupportenabled}

项目是否安装了 Addressables 包。由 asmdef 的 versionDefines 宏 AESIR_MODULES_ADDRESSABLES 驱动，包缺失时相关代码不会编译，但 API 始终可见。

``` csharp
public static bool AddressablesSupportEnabled { get; }
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`Equals(SceneAssetWrapper)`](#method-equals-sceneassetwrapper) | 判断此引用与另一引用是否指向同一场景：优先按场景路径认定（均忽略大小写）， 任一侧路径缺失时回退到场景 GUID。 路径优先是为了与 GetHashCode 使用同一主键——哈希必然按路径取值 （Player 构建下 FromScenePath 构造的实例没有 GUID、反序列化实例带 GUID， 若哈希取 GUID 则同路径的两侧哈希不等）。判等与哈希主键不一致会违反哈希契约： 同一场景在 Dictionary{TKey,TValue} / HashSet{T} 中会出现两个条目且查找漏命中。  取舍说明：路径优先意味着"场景被移动后残留的陈旧路径"与"新路径"被视为两个不同引用 （一致的 false negative），而不是"GUID 相同即相等、哈希却不同"（不一致的 true-positive）。 需要跨改名稳定识别时，请依赖 sceneGuid 锚点自愈把路径写回后再比较。 |
| [`TryGetAddress(ref string)`](#method-trygetaddress-ref-string) | 尝试获取 Addressables 地址。空引用或场景不可寻址时返回 false、不抛异常； 与 Address 的差异：Address 对空引用抛 EmptySceneAssetWrapperException、对非 Addressable 场景抛 SceneNotAddressableException，本方法一律返回 false。 项目未安装 Addressables 包时两者一致，均抛 AddressablesSupportDisabledException。 |
| [`TryGetBuildIndex(ref int)`](#method-trygetbuildindex-ref-int) | 尝试获取 BuildSettings 序号。返回 true 时序号也可能为 -1（场景未加入 BuildSettings）。 |
| [`TryGetLoadedScene(ref Scene)`](#method-trygetloadedscene-ref-scene) | 尝试获取已加载场景的 Scene 结构。与 LoadedScene 不同， 仅当场景确实已加载且有效时返回 true。 |
| [`TryGetSceneName(ref string)`](#method-trygetscenename-ref-string) | 尝试获取场景名称（不含扩展名）。 |
| [`TryGetScenePath(ref string)`](#method-trygetscenepath-ref-string) | 尝试获取场景路径。空引用返回 false，不抛异常。 |
| [`FromAsset(SceneAsset)`](#method-fromasset-sceneasset) | 按 SceneAsset 构造（仅编辑器）。赋值时自动同步路径/GUID/Addressables 地址。 |
| [`FromScenePath(string)`](#method-fromscenepath-string) | 按场景路径构造。编辑器下校验路径上确实存在场景资产并解析 GUID/Addressables 地址； 运行时（构建后）不做存在性校验，加载是否可行由 State 表达。 |

</div>

**继承的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `Equals(object)` | 判断此引用与另一引用是否指向同一场景：优先按场景路径认定（均忽略大小写）， 任一侧路径缺失时回退到场景 GUID。 路径优先是为了与 GetHashCode 使用同一主键——哈希必然按路径取值 （Player 构建下 FromScenePath 构造的实例没有 GUID、反序列化实例带 GUID， 若哈希取 GUID 则同路径的两侧哈希不等）。判等与哈希主键不一致会违反哈希契约： 同一场景在 Dictionary{TKey,TValue} / HashSet{T} 中会出现两个条目且查找漏命中。  取舍说明：路径优先意味着"场景被移动后残留的陈旧路径"与"新路径"被视为两个不同引用 （一致的 false negative），而不是"GUID 相同即相等、哈希却不同"（不一致的 true-positive）。 需要跨改名稳定识别时，请依赖 sceneGuid 锚点自愈把路径写回后再比较。 | `SceneAssetWrapper` |
| `GetHashCode()` | 哈希一律取场景路径（与 Equals(SceneAssetWrapper) 的主键一致）； 路径缺失时退回 GUID。 | `SceneAssetWrapper` |
| `ToString()` | 输出场景名称；空引用输出空字符串（不抛异常）。 | `SceneAssetWrapper` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |

</div>

**运算符方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `public static bool operator !=(SceneAssetWrapper left, SceneAssetWrapper right)` | 空引用之间相等。 | `SceneAssetWrapper` |
| `public static bool operator ==(SceneAssetWrapper left, SceneAssetWrapper right)` | 空引用之间相等。 | `SceneAssetWrapper` |

</div>

### Equals(SceneAssetWrapper) {#method-equals-sceneassetwrapper}

判断此引用与另一引用是否指向同一场景：优先按场景路径认定（均忽略大小写）， 任一侧路径缺失时回退到场景 GUID。
路径优先是为了与 GetHashCode 使用同一主键——哈希必然按路径取值 （Player 构建下 FromScenePath 构造的实例没有 GUID、反序列化实例带 GUID， 若哈希取 GUID 则同路径的两侧哈希不等）。判等与哈希主键不一致会违反哈希契约： 同一场景在 Dictionary{TKey,TValue} / HashSet{T} 中会出现两个条目且查找漏命中。

取舍说明：路径优先意味着"场景被移动后残留的陈旧路径"与"新路径"被视为两个不同引用 （一致的 false negative），而不是"GUID 相同即相等、哈希却不同"（不一致的 true-positive）。 需要跨改名稳定识别时，请依赖 sceneGuid 锚点自愈把路径写回后再比较。

``` csharp
public bool Equals(SceneAssetWrapper other)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `other` | `SceneAssetWrapper` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### TryGetAddress(ref string) {#method-trygetaddress-ref-string}

尝试获取 Addressables 地址。空引用或场景不可寻址时返回 false、不抛异常； 与 Address 的差异：Address 对空引用抛 EmptySceneAssetWrapperException、对非 Addressable 场景抛 SceneNotAddressableException，本方法一律返回 false。 项目未安装 Addressables 包时两者一致，均抛 AddressablesSupportDisabledException。

``` csharp
public bool TryGetAddress(out ref string address)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `address` | `ref string` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### TryGetBuildIndex(ref int) {#method-trygetbuildindex-ref-int}

尝试获取 BuildSettings 序号。返回 true 时序号也可能为 -1（场景未加入 BuildSettings）。

``` csharp
public bool TryGetBuildIndex(out ref int buildIndex)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `buildIndex` | `ref int` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### TryGetLoadedScene(ref Scene) {#method-trygetloadedscene-ref-scene}

尝试获取已加载场景的 Scene 结构。与 LoadedScene 不同， 仅当场景确实已加载且有效时返回 true。

``` csharp
public bool TryGetLoadedScene(out ref Scene loadedScene)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `loadedScene` | `ref Scene` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### TryGetSceneName(ref string) {#method-trygetscenename-ref-string}

尝试获取场景名称（不含扩展名）。

``` csharp
public bool TryGetSceneName(out ref string sceneName)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `sceneName` | `ref string` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### TryGetScenePath(ref string) {#method-trygetscenepath-ref-string}

尝试获取场景路径。空引用返回 false，不抛异常。

``` csharp
public bool TryGetScenePath(out ref string path)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `path` | `ref string` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### FromAsset(SceneAsset) {#method-fromasset-sceneasset}

按 SceneAsset 构造（仅编辑器）。赋值时自动同步路径/GUID/Addressables 地址。

``` csharp
public static SceneAssetWrapper FromAsset(SceneAsset sceneAsset)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `sceneAsset` | `SceneAsset` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `SceneAssetWrapper` | — |

</div>

### FromScenePath(string) {#method-fromscenepath-string}

按场景路径构造。编辑器下校验路径上确实存在场景资产并解析 GUID/Addressables 地址； 运行时（构建后）不做存在性校验，加载是否可行由 State 表达。

``` csharp
public static SceneAssetWrapper FromScenePath(string scenePath)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `scenePath` | `string` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `SceneAssetWrapper` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
