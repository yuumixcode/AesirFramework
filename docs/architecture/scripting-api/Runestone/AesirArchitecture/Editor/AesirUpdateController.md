---
title: AesirUpdateController
description: "Runestone.AesirArchitecture.Editor.AesirUpdateController 的 API 文档"
---

# `AesirUpdateController`

!!! note ""

    - **种类:** `class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `AesirUpdateController`

## 声明

``` csharp
public sealed class AesirUpdateController
```

包更新窗口的共享编排控制器——检测 / 更新日志 / 更新执行 / 忙碌门禁 / 进度的唯一真源。
IMGUI 兜底窗口与 Odin 版窗口经构造注入同一 UpdateState 与视图回调复用本类， 消除两窗口各自维护一份编排逻辑的改一漏一风险（单包撕裂防呆、确认框、忙碌门禁只此一份）。 状态以 UpdateState 为载体挂在窗口的 [SerializeField] 字段上跨域重载保留； Busy 与 IsGitRepository 为 [NonSerialized] ——忙碌标志在更新导入触发的域重载后重置为 false（原协程已死，不重置会永久卡忙碌）， .git 检测每次 Initialize 重跑。

**备注**

本类与两个窗口同属更新器内部实现，非对外 API；窗口层只保留绘制与路由。

## 构造方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirUpdateController(AesirUpdateController.UpdateState, string, Action, Action<float>, Action)`](#constructor-aesirupdatecontroller-aesirupdatecontroller-updatestate-string-action-action-float-action) | 构造编排控制器。 |

</div>

### AesirUpdateController(AesirUpdateController.UpdateState, string, Action, Action<float>, Action) {#constructor-aesirupdatecontroller-aesirupdatecontroller-updatestate-string-action-action-float-action}

构造编排控制器。

``` csharp
public AesirUpdateController(AesirUpdateController.UpdateState state, string progressTitle, Action viewChanged, Action<float> progressChanged, Action repaintRequested = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `state` | `AesirUpdateController.UpdateState` | 状态载体（由窗口的 [SerializeField] 字段传入）。 |
| `progressTitle` | `string` | 进度对话框标题（两个窗口各自的标题文案）。 |
| `viewChanged` | `Action` | 结构变化后的视图刷新回调（重扫 / 忙碌切换时触发，窗口据此重算列表并重绘）。 |
| `progressChanged` | `Action<float>` | 进度变化回调（0-1；Odin 版据此更新窗口内进度条，IMGUI 版可忽略）。 |
| `repaintRequested` | `Action` | 「仅重绘」回调（进度每 tick 更新与状态文本变更时触发）。缺省回退到 viewChanged （保持既有行为）；两个窗口都传纯 Repaint——下载期间进度回调密集， 只重绘可避免每 tick 重算列表（Odin 版还会重建全部行视图模型）。 |

</div>

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`State`](#property-state) | 受控状态（窗口层只读）。 |

</div>

### State {#property-state}

受控状态（窗口层只读）。

``` csharp
public AesirUpdateController.UpdateState State { get; }
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`OutdatedPackages()`](#method-outdatedpackages) | 取全部待更新包（委托 ComputeOutdatedPackages，按包 id 排序保证依赖顺序）。 |
| [`UpdateTargets()`](#method-updatetargets) | 取「全部更新」的执行目标：全部待更新包 + 缺失的已知包（补装）， 委托 ComputeUpdateTargets——「全部更新」按钮的可见性也以此判断。 |
| [`CheckForUpdates()`](#method-checkforupdates) | 检查远程最新版本并拉取更新日志（忙碌中重复调用直接返回）。 |
| [`Initialize()`](#method-initialize) | 初始化：磁盘 .git 检测 + 首次重扫。窗口 OnEnable 调用（域重载后重跑两项 NonSerialized 检测）。 |
| [`RequestUpdateAll()`](#method-requestupdateall) | 「全部更新」入口（工具栏按钮）：目标 = 全部待更新包 + 缺失的已知包（补装， ComputeUpdateTargets）——「全部更新」的语义是让整个框架 到达远程版本（旧的更新、缺的安装）。确认框含补装说明（缺失的包会标为「新安装」并单独提示， 用户可选择取消后仅单独更新已安装的包）。取消或忙碌中直接返回。 |
| [`RequestUpdateSingle(AesirUpdateService.InstalledPackage)`](#method-requestupdatesingle-aesirupdateservice-installedpackage) | 「单包更新」入口（包列表行内按钮）：仅更新指定包，供只需要其中一个包的用户使用。 另一已知包在场且落后于远程版本时，确认框前置配套版本警告（两包按同版本配套发布， 仅更新其一可能造成版本撕裂——提示但不阻止，决定权在用户）。 |
| [`Rescan()`](#method-rescan) | 重新扫描本地安装（远程信息保留，更新导入触发域重载后继续展示）。 扫描根经锚点资产定位，Runestone 移动到项目任意文件夹后照常找到。 |

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

### OutdatedPackages() {#method-outdatedpackages}

取全部待更新包（委托 ComputeOutdatedPackages，按包 id 排序保证依赖顺序）。

``` csharp
public List<AesirUpdateService.InstalledPackage> OutdatedPackages()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `List<AesirUpdateService.InstalledPackage>` | — |

</div>

### UpdateTargets() {#method-updatetargets}

取「全部更新」的执行目标：全部待更新包 + 缺失的已知包（补装）， 委托 ComputeUpdateTargets——「全部更新」按钮的可见性也以此判断。

``` csharp
public List<AesirUpdateService.InstalledPackage> UpdateTargets()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `List<AesirUpdateService.InstalledPackage>` | — |

</div>

### CheckForUpdates() {#method-checkforupdates}

检查远程最新版本并拉取更新日志（忙碌中重复调用直接返回）。

``` csharp
[AsyncStateMachine]
public async void CheckForUpdates()
```

### Initialize() {#method-initialize}

初始化：磁盘 .git 检测 + 首次重扫。窗口 OnEnable 调用（域重载后重跑两项 NonSerialized 检测）。

``` csharp
public void Initialize()
```

### RequestUpdateAll() {#method-requestupdateall}

「全部更新」入口（工具栏按钮）：目标 = 全部待更新包 + 缺失的已知包（补装， ComputeUpdateTargets）——「全部更新」的语义是让整个框架 到达远程版本（旧的更新、缺的安装）。确认框含补装说明（缺失的包会标为「新安装」并单独提示， 用户可选择取消后仅单独更新已安装的包）。取消或忙碌中直接返回。

``` csharp
public void RequestUpdateAll()
```

### RequestUpdateSingle(AesirUpdateService.InstalledPackage) {#method-requestupdatesingle-aesirupdateservice-installedpackage}

「单包更新」入口（包列表行内按钮）：仅更新指定包，供只需要其中一个包的用户使用。 另一已知包在场且落后于远程版本时，确认框前置配套版本警告（两包按同版本配套发布， 仅更新其一可能造成版本撕裂——提示但不阻止，决定权在用户）。

``` csharp
public void RequestUpdateSingle(AesirUpdateService.InstalledPackage package)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `package` | `AesirUpdateService.InstalledPackage` | — |

</div>

### Rescan() {#method-rescan}

重新扫描本地安装（远程信息保留，更新导入触发域重载后继续展示）。 扫描根经锚点资产定位，Runestone 移动到项目任意文件夹后照常找到。

``` csharp
public void Rescan()
```

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
