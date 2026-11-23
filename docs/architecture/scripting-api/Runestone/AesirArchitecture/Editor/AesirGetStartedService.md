---
title: AesirGetStartedService
description: "Runestone.AesirArchitecture.Editor.AesirGetStartedService 的 API 文档"
---

# `AesirGetStartedService`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `AesirGetStartedService`

## 声明

``` csharp
public static class AesirGetStartedService
```

Aesir Getting Started 服务 — 两版 Getting Started 窗口（IMGUI 兜底 / Odin 版）共用的无状态数据层： 扫描本机安装的 Aesir 包、解析示例清单与场景资产、按教学分组归置示例，并提供跳转动作。
包发现覆盖三种安装形态（同名包按此优先级取一）：① 代码导入 InstallRootRelativePath（复制 / unitypackage，示例位于包内 Samples/）；② 嵌入式包（Packages/ 目录下的包源码）；③ UPM Git URL 安装 （包源在 PackageCacheRootPath）。后两种 UPM 形态的示例须导入到 Assets/Samples/<包显示名>/<版本>/<示例显示名>/，未导入的条目仍保留在清单中 （IsImported 为 false），由窗口的「导入 Sample」按钮经确认框 直接调用 Unity Package Manager 的 Sample API 导入（ConfirmAndImportUpmSample）。

示例清单（出现与否、路径、显示名、描述）以各包 package.json 的 samples 列表为唯一真源—— 新增示例只需更新 package.json，窗口自动跟进。而分组 / 顺序 / 档位徽章是代码侧展示启发式 （登记点即 GetSampleGroup / GetGroupOrder / GetSampleBadge 三个方法， 未命中启发式的示例落入默认分组）——需自定义分组归属时改这三个登记点，而非 package.json。

## 字段

**常量字段**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ImportConfirmCancelButton`](#field-importconfirmcancelbutton) | 导入确认框的取消按钮文本。 |
| [`ImportConfirmOkButton`](#field-importconfirmokbutton) | 导入确认框的确认按钮文本。 |
| [`ImportConfirmTitle`](#field-importconfirmtitle) | 导入确认框标题。 |
| [`PackageCacheRootPath`](#field-packagecacherootpath) | UPM Git URL 安装的包缓存根目录（项目相对路径，Unity 不导入）。 |
| [`UpmSamplesRootPath`](#field-upmsamplesrootpath) | UPM 示例导入的落地根目录（Package Manager → Samples → Import 后的位置）。 |

</div>

**声明的字段**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`KnownPackages`](#field-knownpackages) | 已公开发布的 Aesir 包唯一登记（顺序即概览卡片顺序）。更新器（补装目标 / 配套版本警告 / Release 资产定位）与 Getting Started 概览卡片共用本登记——新增公开包只改此处。 |

</div>

### ImportConfirmCancelButton {#field-importconfirmcancelbutton}

导入确认框的取消按钮文本。

``` csharp
public const string ImportConfirmCancelButton = "取消";
```

### ImportConfirmOkButton {#field-importconfirmokbutton}

导入确认框的确认按钮文本。

``` csharp
public const string ImportConfirmOkButton = "导入";
```

### ImportConfirmTitle {#field-importconfirmtitle}

导入确认框标题。

``` csharp
public const string ImportConfirmTitle = "导入 Sample";
```

### PackageCacheRootPath {#field-packagecacherootpath}

UPM Git URL 安装的包缓存根目录（项目相对路径，Unity 不导入）。

``` csharp
public const string PackageCacheRootPath = "Library/PackageCache";
```

### UpmSamplesRootPath {#field-upmsamplesrootpath}

UPM 示例导入的落地根目录（Package Manager → Samples → Import 后的位置）。

``` csharp
public const string UpmSamplesRootPath = "Assets/Samples";
```

### KnownPackages {#field-knownpackages}

已公开发布的 Aesir 包唯一登记（顺序即概览卡片顺序）。更新器（补装目标 / 配套版本警告 / Release 资产定位）与 Getting Started 概览卡片共用本登记——新增公开包只改此处。

``` csharp
public static readonly AesirGetStartedService.AesirKnownPackage[] KnownPackages;
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`ConfirmAndImportUpmSample(AesirGetStartedService.AesirSampleInfo, ref string)`](#method-confirmandimportupmsample-aesirgetstartedservice-aesirsampleinfo-ref-string) | 弹出 Unity 确认框（含示例介绍与导入后位置，可取消），确认后经 Unity Package Manager 的 Sample API 把示例导入到 Assets/Samples/<包显示名>/<版本>/<示例显示名>/； 取消返回 Cancelled，不动文件系统。 |
| [`ScanPackages()`](#method-scanpackages) | 扫描本机安装的 Aesir 包并解析示例。包按 Id 字母序返回 （cn.runestone.aesir.architecture → cn.runestone.aesir.modules，恰为教学顺序）； 同名包按「Assets 副本 → 嵌入式 → PackageCache」优先级取一。 |
| [`GroupSamples(AesirGetStartedService.AesirPackageInfo)`](#method-groupsamples-aesirgetstartedservice-aesirpackageinfo) | 按教学分组归置包内示例（组内保持 package.json 声明顺序；组间按 GetGroupOrder 排序，同序号保持首现顺序——插入排序保证稳定性）。 |
| [`OpenSampleScene(AesirGetStartedService.AesirSampleInfo)`](#method-opensamplescene-aesirgetstartedservice-aesirsampleinfo) | 打开示例场景：先保存当前打开的场景一次（无未保存修改时为空操作），再切换到示例场景。 有资产路径的场景静默落盘——示例导航是高频浏览动作，不为正常场景弹确认框；无路径的临时场景 （新建未保存 / 崩溃恢复出的 Temp/__Backupscenes 场景）无法静默保存 （SaveOpenScenes 会让 Unity 弹出另存为面板强制命名）， 故跳过它们并改用 Unity 打开场景的标准询问（不保存 / 取消 / 保存，取消则中止切换）。 无场景 / 未导入示例退化为 PingSample 定位。 |
| [`PingSample(AesirGetStartedService.AesirSampleInfo)`](#method-pingsample-aesirgetstartedservice-aesirsampleinfo) | 在 Project 窗口选中并 Ping 示例根目录文件夹（未导入时打开 Package Manager）。 |
| [`TryGetExtraSampleAction(AesirGetStartedService.AesirSampleInfo, ref AesirGetStartedService.AesirSampleAction)`](#method-trygetextrasampleaction-aesirgetstartedservice-aesirsampleinfo-ref-aesirgetstartedservice-aesirsampleaction) | 取示例注册的自定义动作（两版 Getting Started 的右侧按钮共用）。 |
| [`BuildImportConfirmMessage(AesirGetStartedService.AesirSampleInfo)`](#method-buildimportconfirmmessage-aesirgetstartedservice-aesirsampleinfo) | 导入确认框正文：包名 + 示例名 + 示例介绍 + 导入后位置。 |
| [`BuildNotImportedToastMessage(AesirGetStartedService.AesirSampleInfo)`](#method-buildnotimportedtoastmessage-aesirgetstartedservice-aesirsampleinfo) | 未导入示例整卡点击的 Toast 文本（引导经右侧按钮导入——导入动作在窗口内完成，不必去 Package Manager）。 |
| [`BuildOpenSceneToastMessage(AesirGetStartedService.AesirSampleInfo)`](#method-buildopenscenetoastmessage-aesirgetstartedservice-aesirsampleinfo) | 打开场景动作的 Toast 提示文本（告知示例场景已打开）。 |
| [`BuildPingToastMessage(AesirGetStartedService.AesirSampleInfo)`](#method-buildpingtoastmessage-aesirgetstartedservice-aesirsampleinfo) | 定位动作的 Toast 提示文本（告知用户已在 Project 窗口选中示例文件夹）。 |
| [`GetSampleBadge(AesirGetStartedService.AesirSampleInfo)`](#method-getsamplebadge-aesirgetstartedservice-aesirsampleinfo) | 档位徽章文本（无档位返回 null）：名称中的 快捷 / 标准 / 严格，或目录序号（01 基础、其余进阶）。 |
| [`GetSampleGroup(AesirGetStartedService.AesirSampleInfo)`](#method-getsamplegroup-aesirgetstartedservice-aesirsampleinfo) | 示例分组标题（渐进式教学顺序：MVC → MVP → 功能演示 → 各模块）。 |
| [`GetUpmSampleImportPath(AesirGetStartedService.AesirSampleInfo)`](#method-getupmsampleimportpath-aesirgetstartedservice-aesirsampleinfo) | UPM / 嵌入式安装的示例导入后位置（项目相对路径，与 Package Manager 实际落地位置一致）。 |
| [`OpenPackageManager()`](#method-openpackagemanager) | 打开 Package Manager（UPM / 嵌入式安装的示例经其 Samples 标签页导入）。 |
| [`RegisterExtraSampleAction(string, string, Action)`](#method-registerextrasampleaction-string-string-action) | 注册示例的自定义动作。key 取 package.json samples 清单里的相对目录（如 RuntimeInitializeLoadType）。 由示例自己的编辑器程序集在 [InitializeOnLoadMethod] 中注册——核心编辑器程序集不引用示例程序集。 |

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

### ConfirmAndImportUpmSample(AesirGetStartedService.AesirSampleInfo, ref string) {#method-confirmandimportupmsample-aesirgetstartedservice-aesirsampleinfo-ref-string}

弹出 Unity 确认框（含示例介绍与导入后位置，可取消），确认后经 Unity Package Manager 的 Sample API 把示例导入到 Assets/Samples/<包显示名>/<版本>/<示例显示名>/； 取消返回 Cancelled，不动文件系统。

``` csharp
public static AesirGetStartedService.AesirSampleImportResult ConfirmAndImportUpmSample(AesirGetStartedService.AesirSampleInfo sample, out ref string message)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `sample` | `AesirGetStartedService.AesirSampleInfo` | — |
| `message` | `ref string` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `AesirGetStartedService.AesirSampleImportResult` | — |

</div>

### ScanPackages() {#method-scanpackages}

扫描本机安装的 Aesir 包并解析示例。包按 Id 字母序返回 （cn.runestone.aesir.architecture → cn.runestone.aesir.modules，恰为教学顺序）； 同名包按「Assets 副本 → 嵌入式 → PackageCache」优先级取一。

``` csharp
public static List<AesirGetStartedService.AesirPackageInfo> ScanPackages()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `List<AesirGetStartedService.AesirPackageInfo>` | — |

</div>

### GroupSamples(AesirGetStartedService.AesirPackageInfo) {#method-groupsamples-aesirgetstartedservice-aesirpackageinfo}

按教学分组归置包内示例（组内保持 package.json 声明顺序；组间按 GetGroupOrder 排序，同序号保持首现顺序——插入排序保证稳定性）。

``` csharp
public static List<AesirGetStartedService.SampleGroup> GroupSamples(AesirGetStartedService.AesirPackageInfo pkg)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `pkg` | `AesirGetStartedService.AesirPackageInfo` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `List<AesirGetStartedService.SampleGroup>` | — |

</div>

### OpenSampleScene(AesirGetStartedService.AesirSampleInfo) {#method-opensamplescene-aesirgetstartedservice-aesirsampleinfo}

打开示例场景：先保存当前打开的场景一次（无未保存修改时为空操作），再切换到示例场景。
有资产路径的场景静默落盘——示例导航是高频浏览动作，不为正常场景弹确认框；无路径的临时场景 （新建未保存 / 崩溃恢复出的 Temp/__Backupscenes 场景）无法静默保存 （SaveOpenScenes 会让 Unity 弹出另存为面板强制命名）， 故跳过它们并改用 Unity 打开场景的标准询问（不保存 / 取消 / 保存，取消则中止切换）。 无场景 / 未导入示例退化为 PingSample 定位。

``` csharp
public static bool OpenSampleScene(AesirGetStartedService.AesirSampleInfo sample)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `sample` | `AesirGetStartedService.AesirSampleInfo` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | 是否已实际打开示例场景（保存被取消或示例不可打开时为 false，不产生切换）。 |

</div>

### PingSample(AesirGetStartedService.AesirSampleInfo) {#method-pingsample-aesirgetstartedservice-aesirsampleinfo}

在 Project 窗口选中并 Ping 示例根目录文件夹（未导入时打开 Package Manager）。

``` csharp
public static bool PingSample(AesirGetStartedService.AesirSampleInfo sample)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `sample` | `AesirGetStartedService.AesirSampleInfo` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | 是否定位成功（资产缺失或未导入时为 false）。 |

</div>

### TryGetExtraSampleAction(AesirGetStartedService.AesirSampleInfo, ref AesirGetStartedService.AesirSampleAction) {#method-trygetextrasampleaction-aesirgetstartedservice-aesirsampleinfo-ref-aesirgetstartedservice-aesirsampleaction}

取示例注册的自定义动作（两版 Getting Started 的右侧按钮共用）。

``` csharp
public static bool TryGetExtraSampleAction(AesirGetStartedService.AesirSampleInfo sample, out ref AesirGetStartedService.AesirSampleAction action)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `sample` | `AesirGetStartedService.AesirSampleInfo` | — |
| `action` | `ref AesirGetStartedService.AesirSampleAction` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### BuildImportConfirmMessage(AesirGetStartedService.AesirSampleInfo) {#method-buildimportconfirmmessage-aesirgetstartedservice-aesirsampleinfo}

导入确认框正文：包名 + 示例名 + 示例介绍 + 导入后位置。

``` csharp
public static string BuildImportConfirmMessage(AesirGetStartedService.AesirSampleInfo sample)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `sample` | `AesirGetStartedService.AesirSampleInfo` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

### BuildNotImportedToastMessage(AesirGetStartedService.AesirSampleInfo) {#method-buildnotimportedtoastmessage-aesirgetstartedservice-aesirsampleinfo}

未导入示例整卡点击的 Toast 文本（引导经右侧按钮导入——导入动作在窗口内完成，不必去 Package Manager）。

``` csharp
public static string BuildNotImportedToastMessage(AesirGetStartedService.AesirSampleInfo sample)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `sample` | `AesirGetStartedService.AesirSampleInfo` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

### BuildOpenSceneToastMessage(AesirGetStartedService.AesirSampleInfo) {#method-buildopenscenetoastmessage-aesirgetstartedservice-aesirsampleinfo}

打开场景动作的 Toast 提示文本（告知示例场景已打开）。

``` csharp
public static string BuildOpenSceneToastMessage(AesirGetStartedService.AesirSampleInfo sample)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `sample` | `AesirGetStartedService.AesirSampleInfo` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

### BuildPingToastMessage(AesirGetStartedService.AesirSampleInfo) {#method-buildpingtoastmessage-aesirgetstartedservice-aesirsampleinfo}

定位动作的 Toast 提示文本（告知用户已在 Project 窗口选中示例文件夹）。

``` csharp
public static string BuildPingToastMessage(AesirGetStartedService.AesirSampleInfo sample)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `sample` | `AesirGetStartedService.AesirSampleInfo` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

### GetSampleBadge(AesirGetStartedService.AesirSampleInfo) {#method-getsamplebadge-aesirgetstartedservice-aesirsampleinfo}

档位徽章文本（无档位返回 null）：名称中的 快捷 / 标准 / 严格，或目录序号（01 基础、其余进阶）。

``` csharp
public static string GetSampleBadge(AesirGetStartedService.AesirSampleInfo sample)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `sample` | `AesirGetStartedService.AesirSampleInfo` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

### GetSampleGroup(AesirGetStartedService.AesirSampleInfo) {#method-getsamplegroup-aesirgetstartedservice-aesirsampleinfo}

示例分组标题（渐进式教学顺序：MVC → MVP → 功能演示 → 各模块）。

**备注**

展示启发式的登记点之一：按显示名前缀归类，未命中任何前缀的示例落入「功能演示」默认组。 新增教学分组时在此追加前缀规则，并同步 GetGroupOrder 的排序键。

``` csharp
public static string GetSampleGroup(AesirGetStartedService.AesirSampleInfo sample)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `sample` | `AesirGetStartedService.AesirSampleInfo` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

### GetUpmSampleImportPath(AesirGetStartedService.AesirSampleInfo) {#method-getupmsampleimportpath-aesirgetstartedservice-aesirsampleinfo}

UPM / 嵌入式安装的示例导入后位置（项目相对路径，与 Package Manager 实际落地位置一致）。

``` csharp
public static string GetUpmSampleImportPath(AesirGetStartedService.AesirSampleInfo sample)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `sample` | `AesirGetStartedService.AesirSampleInfo` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

### OpenPackageManager() {#method-openpackagemanager}

打开 Package Manager（UPM / 嵌入式安装的示例经其 Samples 标签页导入）。

``` csharp
public static void OpenPackageManager()
```

### RegisterExtraSampleAction(string, string, Action) {#method-registerextrasampleaction-string-string-action}

注册示例的自定义动作。key 取 package.json samples 清单里的相对目录（如 RuntimeInitializeLoadType）。 由示例自己的编辑器程序集在 [InitializeOnLoadMethod] 中注册——核心编辑器程序集不引用示例程序集。

``` csharp
public static void RegisterExtraSampleAction(string relativeDir, string label, Action invoke)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `relativeDir` | `string` | — |
| `label` | `string` | — |
| `invoke` | `Action` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
