---
title: AesirUpdateService
description: "Runestone.AesirArchitecture.Editor.AesirUpdateService 的 API 文档"
---

# `AesirUpdateService`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirArchitecture.Editor`
    - **程序集:** `Runestone.AesirArchitecture.Editor`

**继承链:** `System.Object` → `AesirUpdateService`

## 声明

``` csharp
public static class AesirUpdateService
```

Aesir 包自动更新服务 — 供 AesirUpdateWindow 调用的无状态工具集。
适用场景：用户通过复制 / unitypackage 导入方式将包装在 InstallRootRelativePath （Assets/Runestone）下、代码可被修改的非 UPM 安装。经 Package Manager（Git URL）安装的副本 不位于 Assets 下，本工具扫描不到，应改用 Package Manager 更新。

版本检测按「直连 GitHub → GitHub 镜像站 → 免费 CDN 中转」三层顺序兜底（unitypackage 下载始终走 GitHub Release 直链），首个成功即返回，并记录最终线路与各层尝试结果供界面展示：直连层依次尝试 Releases API、releases/latest 的 302 探测、仓库内 update-info.json 的直连 raw；直连不可用时才落镜像站 （内容实时）；最后才是 jsDelivr 等 CDN 中转（分支缓存最长约 12 小时，界面会给出延迟提示）。 见 CheckLatestReleaseAsync。

更新流程：检测远程版本 → 对比本地版本（直接读取包内 package.json）→ 拉取并展示「本地 → 远程」 更新日志（BuildChangelogDigestAsync）→ 确认框二次确认（弹框决策与确认文案由 AesirUpdateController 编排层承担）→ 下载 .unitypackage（直连 → 镜像站逐线路兜底，可随时取消）→ 静默导入 → 按"上次安装清单 − 新版清单"精确差集清理残留（导入成功后才清理，不误伤用户新增文件）→ 逐包登记安装清单。

## 字段

**常量字段**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`DetectionAttemptTimeoutSeconds`](#field-detectionattempttimeoutseconds) | 单次检测尝试的超时（秒）——单源不可达时快速失败并落到下一层， 避免层层长等待把整体检测拖到几十秒。 |
| [`DetectionTotalTimeoutSeconds`](#field-detectiontotaltimeoutseconds) | 整轮版本检测的总超时（秒）——超时后不再发起新的源请求（已完成的尝试记录保留、界面照常展示）， 避免"直连 3 源 + 镜像 2 源 + CDN 4 源"全部卡住时把窗口长时间停住。 最坏时长 ≈ 本轮已发起请求的剩余超时之和（预算到点后只跳过后续源，不再等待新请求）： 逐源 5 秒基线 + 302 探测的 12 秒豁免，实测上限约 32 秒。 |
| [`DownloadStallTimeoutSeconds`](#field-downloadstalltimeoutseconds) | 下载无进展超时（秒）——下载过程中连续该时长没有任何字节进展即中止 （总时长上限见 DownloadTimeoutSeconds）。服务端慢速滴水或连接挂起时， UnityWebRequest.timeout 不会触发，只有墙钟判据能兜住。 |
| [`DownloadTimeoutSeconds`](#field-downloadtimeoutseconds) | unitypackage 下载总时长上限（秒）——大文件慢速连接给足余量；另有"无进展"上限见 DownloadStallTimeoutSeconds。 |
| [`GitHubCheckTimeoutSeconds`](#field-githubchecktimeoutseconds) | GitHub 直连的 302 重定向探测与 GitHub Raw 日志兜底超时（秒）——有意高于单次检测尝试 （DetectionAttemptTimeoutSeconds）：这两条链路直连 github.com， 大陆网络环境下重定向握手与 raw 下载普遍偏慢，5 秒基线会误伤。 |
| [`JsDelivrCheckTimeoutSeconds`](#field-jsdelivrchecktimeoutseconds) | jsDelivr 检测超时（秒）——不可达时通常立刻失败，超时不宜过长。 |
| [`ChangelogFileName`](#field-changelogfilename) | 包内 CHANGELOG 文件名（Keep a Changelog 格式）。 |
| [`InstallRootRelativePath`](#field-installrootrelativepath) | 默认安装根（项目相对路径）——快路径首查位置。实际安装根经 AesirAssetPaths 锚点定位（Runestone 可移动到项目任意文件夹），扫描入口用 ScanInstalledPackagesFromAllRoots；本常量供单根扫描的默认参数与测试 fixture 使用。 |
| [`ManifestFileName`](#field-manifestfilename) | 本地安装清单文件名（记录每次更新成功后各包的完整文件列表）。 |
| [`RepoPath`](#field-repopath) | GitHub 仓库路径（owner/repo）。 |
| [`StateDirName`](#field-statedirname) | 项目根目录下的更新状态目录名（点前缀，Unity 不导入）。 |
| [`UpdateInfoRelativePath`](#field-updateinforelativepath) | update-info.json 在仓库内的路径（CI 发版后以 [skip ci] 提交回 main）。 |

</div>

**声明的字段**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`GitHubDownloadUrlBase`](#field-githubdownloadurlbase) | GitHub Release 资产下载地址前缀（资产命名约定见 ReleaseSnapshot.GetUnityPackageUrl）。 |
| [`GitHubRawUpdateInfoUrl`](#field-githubrawupdateinfourl) | 直连 GitHub 的 update-info.json 地址（GitHub 自有域名，无 API 限流、带文件清单； 缓存仅数分钟，远优于 CDN 中转）。 |
| [`GitHubRawUrlBase`](#field-githubrawurlbase) | AesirFramework 全部已知包的统一登记（唯一真源）：见 KnownPackages ——目录名、包 id、显示名与描述齐全；本类与 Getting Started 概览卡片共用同一份登记。 目录名同时是 Release 资产命名（<目录名>-v<版本>.unitypackage）与默认安装目录名。 |
| [`LatestReleaseApiUrl`](#field-latestreleaseapiurl) | GitHub Releases 最新版 API（降级源之二，未认证限流 60 次/时/IP）。 |
| [`LatestReleasePageUrl`](#field-latestreleasepageurl) | GitHub releases/latest 页面地址（302 到最新 tag，可完全绕开 API 限流）。 |
| [`ReleasesPageUrl`](#field-releasespageurl) | Releases 网页地址（供用户手动下载 / 查看更新日志）。 |
| [`GitHubMirrorPrefixes`](#field-githubmirrorprefixes) | GitHub 镜像站前缀（第三方代理 raw.githubusercontent.com；「前缀 + 完整 GitHub 地址」形态）。 顺序即尝试顺序——直连不通时镜像站仍返回实时内容。 |
| [`JsDelivrDomains`](#field-jsdelivrdomains) | jsDelivr CDN 域名（按大陆可达性经验排序；fastly 会 301 跳转到主域名，自动跟随）。 |

</div>

### DetectionAttemptTimeoutSeconds {#field-detectionattempttimeoutseconds}

单次检测尝试的超时（秒）——单源不可达时快速失败并落到下一层， 避免层层长等待把整体检测拖到几十秒。

``` csharp
public const int DetectionAttemptTimeoutSeconds = 5;
```

### DetectionTotalTimeoutSeconds {#field-detectiontotaltimeoutseconds}

整轮版本检测的总超时（秒）——超时后不再发起新的源请求（已完成的尝试记录保留、界面照常展示）， 避免"直连 3 源 + 镜像 2 源 + CDN 4 源"全部卡住时把窗口长时间停住。
最坏时长 ≈ 本轮已发起请求的剩余超时之和（预算到点后只跳过后续源，不再等待新请求）： 逐源 5 秒基线 + 302 探测的 12 秒豁免，实测上限约 32 秒。

``` csharp
public const int DetectionTotalTimeoutSeconds = 30;
```

### DownloadStallTimeoutSeconds {#field-downloadstalltimeoutseconds}

下载无进展超时（秒）——下载过程中连续该时长没有任何字节进展即中止 （总时长上限见 DownloadTimeoutSeconds）。服务端慢速滴水或连接挂起时， UnityWebRequest.timeout 不会触发，只有墙钟判据能兜住。

``` csharp
public const int DownloadStallTimeoutSeconds = 30;
```

### DownloadTimeoutSeconds {#field-downloadtimeoutseconds}

unitypackage 下载总时长上限（秒）——大文件慢速连接给足余量；另有"无进展"上限见 DownloadStallTimeoutSeconds。

``` csharp
public const int DownloadTimeoutSeconds = 120;
```

### GitHubCheckTimeoutSeconds {#field-githubchecktimeoutseconds}

GitHub 直连的 302 重定向探测与 GitHub Raw 日志兜底超时（秒）——有意高于单次检测尝试 （DetectionAttemptTimeoutSeconds）：这两条链路直连 github.com， 大陆网络环境下重定向握手与 raw 下载普遍偏慢，5 秒基线会误伤。

``` csharp
public const int GitHubCheckTimeoutSeconds = 12;
```

### JsDelivrCheckTimeoutSeconds {#field-jsdelivrchecktimeoutseconds}

jsDelivr 检测超时（秒）——不可达时通常立刻失败，超时不宜过长。

``` csharp
public const int JsDelivrCheckTimeoutSeconds = 5;
```

### ChangelogFileName {#field-changelogfilename}

包内 CHANGELOG 文件名（Keep a Changelog 格式）。

``` csharp
public const string ChangelogFileName = "CHANGELOG.md";
```

### InstallRootRelativePath {#field-installrootrelativepath}

默认安装根（项目相对路径）——快路径首查位置。实际安装根经 AesirAssetPaths 锚点定位（Runestone 可移动到项目任意文件夹），扫描入口用 ScanInstalledPackagesFromAllRoots；本常量供单根扫描的默认参数与测试 fixture 使用。

``` csharp
public const string InstallRootRelativePath = "Assets/Runestone";
```

### ManifestFileName {#field-manifestfilename}

本地安装清单文件名（记录每次更新成功后各包的完整文件列表）。

``` csharp
public const string ManifestFileName = "installed-manifest.json";
```

### RepoPath {#field-repopath}

GitHub 仓库路径（owner/repo）。

``` csharp
public const string RepoPath = "yuumixcode/AesirFramework";
```

### StateDirName {#field-statedirname}

项目根目录下的更新状态目录名（点前缀，Unity 不导入）。

``` csharp
public const string StateDirName = ".aesir";
```

### UpdateInfoRelativePath {#field-updateinforelativepath}

update-info.json 在仓库内的路径（CI 发版后以 [skip ci] 提交回 main）。

``` csharp
public const string UpdateInfoRelativePath = ".github/update-info.json";
```

### GitHubDownloadUrlBase {#field-githubdownloadurlbase}

GitHub Release 资产下载地址前缀（资产命名约定见 ReleaseSnapshot.GetUnityPackageUrl）。

``` csharp
public static readonly string GitHubDownloadUrlBase = "https://github.com/yuumixcode/AesirFramework/releases/download";
```

### GitHubRawUpdateInfoUrl {#field-githubrawupdateinfourl}

直连 GitHub 的 update-info.json 地址（GitHub 自有域名，无 API 限流、带文件清单； 缓存仅数分钟，远优于 CDN 中转）。

``` csharp
public static readonly string GitHubRawUpdateInfoUrl = "https://raw.githubusercontent.com/yuumixcode/AesirFramework/main/.github/update-info.json";
```

### GitHubRawUrlBase {#field-githubrawurlbase}

AesirFramework 全部已知包的统一登记（唯一真源）：见 KnownPackages ——目录名、包 id、显示名与描述齐全；本类与 Getting Started 概览卡片共用同一份登记。 目录名同时是 Release 资产命名（<目录名>-v<版本>.unitypackage）与默认安装目录名。

``` csharp
public static readonly string GitHubRawUrlBase = "https://raw.githubusercontent.com/yuumixcode/AesirFramework";
```

### LatestReleaseApiUrl {#field-latestreleaseapiurl}

GitHub Releases 最新版 API（降级源之二，未认证限流 60 次/时/IP）。

``` csharp
public static readonly string LatestReleaseApiUrl = "https://api.github.com/repos/yuumixcode/AesirFramework/releases/latest";
```

### LatestReleasePageUrl {#field-latestreleasepageurl}

GitHub releases/latest 页面地址（302 到最新 tag，可完全绕开 API 限流）。

``` csharp
public static readonly string LatestReleasePageUrl = "https://github.com/yuumixcode/AesirFramework/releases/latest";
```

### ReleasesPageUrl {#field-releasespageurl}

Releases 网页地址（供用户手动下载 / 查看更新日志）。

``` csharp
public static readonly string ReleasesPageUrl = "https://github.com/yuumixcode/AesirFramework/releases";
```

### GitHubMirrorPrefixes {#field-githubmirrorprefixes}

GitHub 镜像站前缀（第三方代理 raw.githubusercontent.com；「前缀 + 完整 GitHub 地址」形态）。 顺序即尝试顺序——直连不通时镜像站仍返回实时内容。

``` csharp
public static readonly string[] GitHubMirrorPrefixes;
```

### JsDelivrDomains {#field-jsdelivrdomains}

jsDelivr CDN 域名（按大陆可达性经验排序；fastly 会 301 跳转到主域名，自动跟随）。

``` csharp
public static readonly string[] JsDelivrDomains;
```

## 属性

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`PrimaryInstallRoot`](#property-primaryinstallroot) | 主安装根（项目相对路径）——提示文案用；委托 PrimaryInstallRoot 动态解析（默认根优先，无任何本地安装时回退 InstallRootRelativePath）。 |
| [`ProjectRootPath`](#property-projectrootpath) | Unity 项目根目录（Application.dataPath 的上一级）。 |
| [`StateFilePath`](#property-statefilepath) | 本地安装清单的项目相对路径。 |

</div>

### PrimaryInstallRoot {#property-primaryinstallroot}

主安装根（项目相对路径）——提示文案用；委托 PrimaryInstallRoot 动态解析（默认根优先，无任何本地安装时回退 InstallRootRelativePath）。

``` csharp
public static string PrimaryInstallRoot { get; } = "Assets/Runestone";
```

### ProjectRootPath {#property-projectrootpath}

Unity 项目根目录（Application.dataPath 的上一级）。

``` csharp
public static string ProjectRootPath { get; } = "/Users/yuumix/Projects/Unity/AesirFramework";
```

### StateFilePath {#property-statefilepath}

本地安装清单的项目相对路径。

``` csharp
public static string StateFilePath { get; } = ".aesir/installed-manifest.json";
```

## 方法

**声明的方法**

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`LoadLocalManifest()`](#method-loadlocalmanifest) | 读取本地安装清单；文件不存在或损坏返回 null。 |
| [`MergePackageEntry(AesirUpdateService.FilesManifest, AesirUpdateService.FilesManifest.PackageEntry)`](#method-mergepackageentry-aesirupdateservice-filesmanifest-aesirupdateservice-filesmanifest-packageentry) | 将远程清单中的一个包条目合并进本地清单（按 name 替换或追加）。 返回合并后的新清单实例（输入参数不被修改）。 |
| [`ParseFilesManifest(string)`](#method-parsefilesmanifest-string) | 解析本地清单 JSON；内容为空或格式异常时返回 null（调用方按"无记录"处理）。 |
| [`ParseUpdateInfo(string)`](#method-parseupdateinfo-string) | 解析 update-info.json；内容为空或格式异常时返回 null（调用方按"源不可用"处理）。 |
| [`CollectNewerSections(IReadOnlyList<AesirUpdateService.ChangelogSection>, string, string)`](#method-collectnewersections-ireadonlylist-aesirupdateservice-changelogsection-string-string) | 从段落列表中筛出位于 (localVersion, remoteVersion] 区间的版本段落（保持原顺序：新 → 旧）。 remoteVersion 为空时不设上限。 |
| [`ParseChangelogSections(string)`](#method-parsechangelogsections-string) | 解析 Keep a Changelog 格式的 markdown，按 ## [x.y.z] 切分版本段落（保持文件原顺序：新 → 旧）。 非版本标题（## [Unreleased]、## 当前版本 等）不产生段落；遇到任意其他二级标题即结束当前段落。 输入为空或异常时返回空列表。 |
| [`ComputeOutdatedPackages(List<AesirUpdateService.InstalledPackage>, string)`](#method-computeoutdatedpackages-list-aesirupdateservice-installedpackage-string) | 取全部待更新包（本地版本低于远程版本），按包 id 排序保证依赖顺序（Architecture 先于 Modules）。 远程版本为空时返回空列表。 |
| [`ComputeUpdateTargets(List<AesirUpdateService.InstalledPackage>, string)`](#method-computeupdatetargets-list-aesirupdateservice-installedpackage-string) | 计算「全部更新」的执行目标：全部待更新包 ComputeOutdatedPackages， 加上 KnownPackages 中本地未安装的包（补装目标， Version 为空）；合并后按包 id 排序保证依赖顺序。 语义：「全部更新」= 让整个框架（全部已知包）到达远程版本——旧的更新、缺的补装。 补装目标经确认框明示（「未安装 → vX（新安装）」），用户可选择取消后单独更新已安装的包。 补装包的 unitypackage 导入始终落位默认安装根（Unity 机制固有）。 |
| [`ScanInstalledPackages(string)`](#method-scaninstalledpackages-string) | 扫描 installRootRelativePath 下的 Aesir 包安装。 识别依据：子目录中存在 package.json 且包 id 以 cn.runestone.aesir. 开头。 |
| [`ScanInstalledPackagesFromAllRoots()`](#method-scaninstalledpackagesfromallroots) | 扫描全部本地安装根（InstallRoots——经锚点资产定位， Runestone 可移动到项目任意文件夹；默认根优先）下的 Aesir 包安装。 跨根同包 id 去重，先扫描的根胜出（默认根在最前）。 |
| [`ComputeStaleFiles(string[], string[], string, string)`](#method-computestalefiles-string-string-string-string) | 计算需要删除的残留条目：上次安装清单中存在、新版清单中不存在、且位于指定包目录内。 上次清单为空（首次安装 / 无历史记录）时返回空列表 — 没有历史就无法界定"该删什么"， 宁可残留也不误删。用户在包内新增的文件不在任何清单中，天然不会被删除。  新清单为空（null / 无 files 字段）同样返回空列表 — 与上次清单守卫对称： 远程 update-info.json 半截 JSON / 格式漂移会被 JsonUtility 反序列化成空清单， 若无此守卫，刚导入的整个包会被差集逻辑连文件带空目录删光（唯一"数据损坏级"路径）。 清单缺失时更新流程本就不执行差集清理（见 UpdatePackagesAsync 调用点）， 此守卫为双保险。  返回的是本地路径：清单内的路径恒为"仓库相对路径"（如 Assets/Runestone/AesirArchitecture/…， 与 unitypackage 内 pathname 同源），而安装根可被用户移动到任意文件夹。因此先按 packageDirName 从清单条目中辨认出清单侧的包前缀，再把前缀替换为 packageAssetsPath（本地实际路径）后返回——否则移动安装形态下 前缀永远不匹配，差集清理会静默失效（既删不掉残留，也不会有任何提示）。 |
| [`ParsePackageJson(string)`](#method-parsepackagejson-string) | 解析 package.json 的 name 与 version 字段。 轻量字段提取（要求自包含，不引入 JSON 库）。 |
| [`CheckLatestReleaseAsync(Func<string, int, Task<string>>, Func<Task<string>>, Func<double>)`](#method-checklatestreleaseasync-func-string-int-task-string-func-task-string-func-double) | 检测远程最新 Release（tag + 可能的清单），按「直连 GitHub → 镜像站 → CDN 中转」三层顺序兜底， 首个成功即返回；单次尝试超时 DetectionAttemptTimeoutSeconds 秒。 |
| [`UpdatePackagesAsync(AesirUpdateService.ReleaseSnapshot, IReadOnlyList<AesirUpdateService.InstalledPackage>, Action<string, float>, Action, Func<bool>)`](#method-updatepackagesasync-aesirupdateservice-releasesnapshot-ireadonlylist-aesirupdateservice-installedpackage-action-string-float-action-func-bool) | 执行更新：逐包（下载 → 静默导入 → 按清单差集清理残留 → 登记安装清单）。 下载经 DownloadUnityPackageAsync 逐线路兜底（直连 → 镜像站代理）； 用户取消时以 UpdateResult（Cancelled = true）正常返回， 已导入的包保持有效、剩余目标如实列入 SkippedDirNames； 其他失败仍向上抛出（由窗口层弹错误框，异常消息含手动下载指引）。 |
| [`DownloadBytesAsync(string, Action<float>, int, int, Func<double>, Func<bool>)`](#method-downloadbytesasync-string-action-float-int-int-func-double-func-bool) | GET 二进制内容（用于下载 unitypackage），通过回调上报 0~1 下载进度。 双重墙钟上限：总时长 timeoutSeconds 与"无进展" stallTimeoutSeconds——任一超限即 Abort 并抛超时异常， 保证上层进度条必定收尾（不会出现长时间卡住的进度条）。 |
| [`DownloadUnityPackageAsync(AesirUpdateService.ReleaseSnapshot, string, Action<string, float>, Func<bool>)`](#method-downloadunitypackageasync-aesirupdateservice-releasesnapshot-string-action-string-float-func-bool) | 下载指定包的 unitypackage：按「GitHub Release 直链 → GitHub 镜像站代理」顺序逐线路尝试， 单条线路失败（超时 / 连接失败）自动落到下一线路，每条线路独立适用双重墙钟超时。 用户取消（isCanceled 返回 true）立即抛 OperationCanceledException， 不再尝试后续线路。全部线路失败时抛出含各线路错误明细与手动下载指引的异常—— 用户网络全部线路不可达时仍有明确的自助路径。 |
| [`BuildChangelogDigestAsync(string, IReadOnlyList<AesirUpdateService.InstalledPackage>, Func<double>)`](#method-buildchangelogdigestasync-string-ireadonlylist-aesirupdateservice-installedpackage-func-double) | 生成「本地版本 → 远程版本」的更新日志摘要文本（每包一节，按传入顺序）。 远程优先：按 tag 拉取包内 CHANGELOG.md 并提取比本地新的段落；远程拉取失败时回退本地包内 CHANGELOG 的最新段落并标注来源。日志属辅助信息，任何单包失败不影响其余包与其摘要输出。  整轮墙钟预算 DetectionTotalTimeoutSeconds 秒：拉取阶段逃逸在检测预算之外 （每包最多 4 个 jsDelivr 域 × 5s + GitHub Raw 12s，双包串行最坏约 64 秒）会让坏网络用户 长时间停在不可取消的进度条上（本页与检测阶段均用 DisplayProgressBar，无取消按钮）—— 预算耗尽后剩余包不再发起远程拉取，直接回退本地日志，把最坏时长压到"预算 + 单次请求超时"。 预算只拦截"下一个包"的拉取，在途请求仍按自身超时收尾，故总时长上限为预算 + 单次请求超时。 |
| [`FetchPackageChangelogAsync(string, string)`](#method-fetchpackagechangelogasync-string-string) | 拉取指定 Release 版本的包内 CHANGELOG.md（jsDelivr 多域名 → GitHub Raw 兜底，首个成功即返回）。 全部失败时抛出含各源错误明细的异常。 |
| [`GetTextAsync(string, int, Func<double>)`](#method-gettextasync-string-int-func-double) | GET 文本内容（UnityWebRequest，编辑器主线程异步等待）。超时秒数由调用方显式给出 （各检测源有各自的超时基线，不给默认值以防新调用方不慎超出 5 秒检测基线）。 除 request.timeout（无数据超时）外再加墙钟硬上限：超时即 Abort 并抛超时异常。 |
| [`ProbeLatestTagFromRedirect(Func<double>)`](#method-probelatesttagfromredirect-func-double) | 向 GitHub releases/latest 发起禁止重定向的 HEAD 请求，从 302 Location 中提取最新 tag。 完全绕开 API 限流（该路径不走 api.github.com）。 |
| [`IsFreshInstall(AesirUpdateService.InstalledPackage)`](#method-isfreshinstall-aesirupdateservice-installedpackage) | 目标是否为补装（本地未安装、由「全部更新」新安装的包）——以 Version 为空判定。 |
| [`IsGitRepository()`](#method-isgitrepository) | 当前项目根目录是否存在 .git（若是 AesirFramework 开发仓库，执行更新会覆盖本地源码）。 |
| [`DefaultClock()`](#method-defaultclock) | 默认时钟：编辑器启动以来的秒数（测试可注入自定义时钟以验证超时分支）。 |
| [`CompareVersion(string, string)`](#method-compareversion-string-string) | 比较两个语义化版本号（允许 v/V 前缀，缺省段按 0 处理）。 数字段相同时正式版高于预发布版（1.0.0-rc1 < 1.0.0），两侧均为预发布按标识文本排序。 返回值：a < b 为负，相等为 0，a > b 为正。 |
| [`DeleteStaleEntries(IEnumerable<string>)`](#method-deletestaleentries-ienumerable-string) | 删除残留条目：文件直接删除（连带同名 .meta）；目录仅在其为空时删除。 返回实际删除的条目数。传入的路径必须是项目相对路径。 |
| [`PruneEmptyDirectories(string)`](#method-pruneemptydirectories-string) | 自底向上删除指定包目录下的空目录（连带 .meta，不删除包根目录本身）。 返回删除的目录数。用于残留文件删除后收尾清理空目录。 |
| [`BuildCdnDelayHintText()`](#method-buildcdndelayhinttext) | CDN 中转延迟提示（仅当结果来自 CDN 中转时展示）。 |
| [`BuildConnectionStatusText(bool)`](#method-buildconnectionstatustext-bool) | 连接状态文案：直连可用 = 本次结果 100% 实时；否则说明结果来自兜底线路。 |
| [`BuildDetectionSummary(string, AesirUpdateService.ReleaseRouteKind, bool)`](#method-builddetectionsummary-string-aesirupdateservice-releaseroutekind-bool) | 检测结果摘要（连接状态 + 获取线路两行，界面合并展示）。 |
| [`BuildJsDelivrUpdateInfoUrl(string)`](#method-buildjsdelivrupdateinfourl-string) | jsDelivr CDN 上的 update-info.json 地址（分支引用，缓存最长约 12 小时）。 |
| [`BuildRouteText(string, AesirUpdateService.ReleaseRouteKind)`](#method-buildroutetext-string-aesirupdateservice-releaseroutekind) | 获取线路文案：最终结果是经哪条线路拿到的（直连 / 镜像站 / CDN 中转）。 |
| [`ExtractJsonField(string, string)`](#method-extractjsonfield-string-string) | 从 JSON 文本中按字段名提取第一个双引号字符串值（不依赖第三方 JSON 库）。 |
| [`ExtractTagFromLocation(string)`](#method-extracttagfromlocation-string) | 从 releases/latest 重定向地址中提取 tag（如 .../releases/tag/v0.15.0 → v0.15.0）； 不匹配返回 null。 |
| [`GetRouteKindLabel(AesirUpdateService.ReleaseRouteKind)`](#method-getroutekindlabel-aesirupdateservice-releaseroutekind) | 线路类别的展示标签（界面「获取线路」用）。 |
| [`LoadLocalChangelog(string)`](#method-loadlocalchangelog-string) | 读取本地包内 CHANGELOG.md 全文；文件不存在返回 null。 |
| [`RenderChangelogText(IReadOnlyList<AesirUpdateService.ChangelogSection>)`](#method-renderchangelogtext-ireadonlylist-aesirupdateservice-changelogsection) | 将版本段落渲染回可读的 markdown 文本（标题行 + 正文，段落间空行分隔）。 |
| [`ToAbsolutePath(string)`](#method-toabsolutepath-string) | 将项目相对路径转为绝对路径。 |
| [`SaveLocalManifest(AesirUpdateService.FilesManifest)`](#method-savelocalmanifest-aesirupdateservice-filesmanifest) | 写入本地安装清单（自动创建 .aesir 状态目录）。 |

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

### LoadLocalManifest() {#method-loadlocalmanifest}

读取本地安装清单；文件不存在或损坏返回 null。

``` csharp
public static AesirUpdateService.FilesManifest LoadLocalManifest()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `AesirUpdateService.FilesManifest` | — |

</div>

### MergePackageEntry(AesirUpdateService.FilesManifest, AesirUpdateService.FilesManifest.PackageEntry) {#method-mergepackageentry-aesirupdateservice-filesmanifest-aesirupdateservice-filesmanifest-packageentry}

将远程清单中的一个包条目合并进本地清单（按 name 替换或追加）。 返回合并后的新清单实例（输入参数不被修改）。

``` csharp
public static AesirUpdateService.FilesManifest MergePackageEntry(AesirUpdateService.FilesManifest localManifest, AesirUpdateService.FilesManifest.PackageEntry entry)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `localManifest` | `AesirUpdateService.FilesManifest` | — |
| `entry` | `AesirUpdateService.FilesManifest.PackageEntry` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `AesirUpdateService.FilesManifest` | — |

</div>

### ParseFilesManifest(string) {#method-parsefilesmanifest-string}

解析本地清单 JSON；内容为空或格式异常时返回 null（调用方按"无记录"处理）。

``` csharp
public static AesirUpdateService.FilesManifest ParseFilesManifest(string json)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `json` | `string` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `AesirUpdateService.FilesManifest` | — |

</div>

### ParseUpdateInfo(string) {#method-parseupdateinfo-string}

解析 update-info.json；内容为空或格式异常时返回 null（调用方按"源不可用"处理）。

``` csharp
public static AesirUpdateService.UpdateInfo ParseUpdateInfo(string json)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `json` | `string` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `AesirUpdateService.UpdateInfo` | — |

</div>

### CollectNewerSections(IReadOnlyList<AesirUpdateService.ChangelogSection>, string, string) {#method-collectnewersections-ireadonlylist-aesirupdateservice-changelogsection-string-string}

从段落列表中筛出位于 (localVersion, remoteVersion] 区间的版本段落（保持原顺序：新 → 旧）。 remoteVersion 为空时不设上限。

``` csharp
public static List<AesirUpdateService.ChangelogSection> CollectNewerSections(IReadOnlyList<AesirUpdateService.ChangelogSection> sections, string localVersion, string remoteVersion)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `sections` | `IReadOnlyList<AesirUpdateService.ChangelogSection>` | — |
| `localVersion` | `string` | — |
| `remoteVersion` | `string` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `List<AesirUpdateService.ChangelogSection>` | — |

</div>

### ParseChangelogSections(string) {#method-parsechangelogsections-string}

解析 Keep a Changelog 格式的 markdown，按 ## [x.y.z] 切分版本段落（保持文件原顺序：新 → 旧）。
非版本标题（## [Unreleased]、## 当前版本 等）不产生段落；遇到任意其他二级标题即结束当前段落。 输入为空或异常时返回空列表。

``` csharp
public static List<AesirUpdateService.ChangelogSection> ParseChangelogSections(string markdown)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `markdown` | `string` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `List<AesirUpdateService.ChangelogSection>` | — |

</div>

### ComputeOutdatedPackages(List<AesirUpdateService.InstalledPackage>, string) {#method-computeoutdatedpackages-list-aesirupdateservice-installedpackage-string}

取全部待更新包（本地版本低于远程版本），按包 id 排序保证依赖顺序（Architecture 先于 Modules）。 远程版本为空时返回空列表。

``` csharp
public static List<AesirUpdateService.InstalledPackage> ComputeOutdatedPackages(List<AesirUpdateService.InstalledPackage> packages, string remoteVersion)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `packages` | `List<AesirUpdateService.InstalledPackage>` | — |
| `remoteVersion` | `string` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `List<AesirUpdateService.InstalledPackage>` | — |

</div>

### ComputeUpdateTargets(List<AesirUpdateService.InstalledPackage>, string) {#method-computeupdatetargets-list-aesirupdateservice-installedpackage-string}

计算「全部更新」的执行目标：全部待更新包 ComputeOutdatedPackages， 加上 KnownPackages 中本地未安装的包（补装目标， Version 为空）；合并后按包 id 排序保证依赖顺序。
语义：「全部更新」= 让整个框架（全部已知包）到达远程版本——旧的更新、缺的补装。 补装目标经确认框明示（「未安装 → vX（新安装）」），用户可选择取消后单独更新已安装的包。 补装包的 unitypackage 导入始终落位默认安装根（Unity 机制固有）。

``` csharp
public static List<AesirUpdateService.InstalledPackage> ComputeUpdateTargets(List<AesirUpdateService.InstalledPackage> installedPackages, string remoteVersion)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `installedPackages` | `List<AesirUpdateService.InstalledPackage>` | — |
| `remoteVersion` | `string` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `List<AesirUpdateService.InstalledPackage>` | — |

</div>

### ScanInstalledPackages(string) {#method-scaninstalledpackages-string}

扫描 installRootRelativePath 下的 Aesir 包安装。 识别依据：子目录中存在 package.json 且包 id 以 cn.runestone.aesir. 开头。

``` csharp
public static List<AesirUpdateService.InstalledPackage> ScanInstalledPackages(string installRootRelativePath = "Assets/Runestone")
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `installRootRelativePath` | `string` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `List<AesirUpdateService.InstalledPackage>` | — |

</div>

### ScanInstalledPackagesFromAllRoots() {#method-scaninstalledpackagesfromallroots}

扫描全部本地安装根（InstallRoots——经锚点资产定位， Runestone 可移动到项目任意文件夹；默认根优先）下的 Aesir 包安装。 跨根同包 id 去重，先扫描的根胜出（默认根在最前）。

``` csharp
public static List<AesirUpdateService.InstalledPackage> ScanInstalledPackagesFromAllRoots()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `List<AesirUpdateService.InstalledPackage>` | — |

</div>

### ComputeStaleFiles(string[], string[], string, string) {#method-computestalefiles-string-string-string-string}

计算需要删除的残留条目：上次安装清单中存在、新版清单中不存在、且位于指定包目录内。
上次清单为空（首次安装 / 无历史记录）时返回空列表 — 没有历史就无法界定"该删什么"， 宁可残留也不误删。用户在包内新增的文件不在任何清单中，天然不会被删除。

新清单为空（null / 无 files 字段）同样返回空列表 — 与上次清单守卫对称： 远程 update-info.json 半截 JSON / 格式漂移会被 JsonUtility 反序列化成空清单， 若无此守卫，刚导入的整个包会被差集逻辑连文件带空目录删光（唯一"数据损坏级"路径）。 清单缺失时更新流程本就不执行差集清理（见 UpdatePackagesAsync 调用点）， 此守卫为双保险。

返回的是本地路径：清单内的路径恒为"仓库相对路径"（如 Assets/Runestone/AesirArchitecture/…， 与 unitypackage 内 pathname 同源），而安装根可被用户移动到任意文件夹。因此先按 packageDirName 从清单条目中辨认出清单侧的包前缀，再把前缀替换为 packageAssetsPath（本地实际路径）后返回——否则移动安装形态下 前缀永远不匹配，差集清理会静默失效（既删不掉残留，也不会有任何提示）。

``` csharp
public static List<string> ComputeStaleFiles(string[] previousFiles, string[] newFiles, string packageAssetsPath, string packageDirName)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `previousFiles` | `string[]` | 上次安装清单的文件列表（仓库相对路径） |
| `newFiles` | `string[]` | 新版清单的文件列表（仓库相对路径） |
| `packageAssetsPath` | `string` | 该包在本地的项目相对路径（如 Assets/Runestone/AesirArchitecture） |
| `packageDirName` | `string` | 该包的目录名（如 AesirArchitecture），用于在清单中辨认包前缀 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `List<string>` | — |

</div>

### ParsePackageJson(string) {#method-parsepackagejson-string}

解析 package.json 的 name 与 version 字段。
轻量字段提取（要求自包含，不引入 JSON 库）。

``` csharp
public static ValueTuple<string, string> ParsePackageJson(string path)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `path` | `string` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `ValueTuple<string, string>` | — |

</div>

### CheckLatestReleaseAsync(Func<string, int, Task<string>>, Func<Task<string>>, Func<double>) {#method-checklatestreleaseasync-func-string-int-task-string-func-task-string-func-double}

检测远程最新 Release（tag + 可能的清单），按「直连 GitHub → 镜像站 → CDN 中转」三层顺序兜底， 首个成功即返回；单次尝试超时 DetectionAttemptTimeoutSeconds 秒。

**备注**

第一层直连 GitHub（最实时，依次尝试）：① Releases API（结构化、发布即刻可见，未认证 60 次/时/IP）； ② releases/latest 的 302 重定向探测（完全绕开 API 限流）；③ 仓库内 update-info.json 的直连 raw （无 API 限流且带文件清单，缓存仅数分钟）。

第二层镜像站（GitHubMirrorPrefixes 代理 raw 内容，内容实时）； 第三层免费 CDN 中转（JsDelivrDomains，分支缓存最长约 12 小时）。 直连可用时绝不用后两层——这保证「能连 GitHub 就是最新」。

只有 tag 的结果（API / 302）会再按同层顺序补齐文件清单（清单缺失会跳过"差集清理残留"）； 补齐只接受 tag 与本次检测一致者——CDN 可能仍是旧版本，错配清单会误删文件。

全部失败时抛出含各源错误明细的异常。

``` csharp
[AsyncStateMachine]
public static async Task<AesirUpdateService.ReleaseCheckResult> CheckLatestReleaseAsync(Func<string, int, Task<string>> fetchTextAsync = null, Func<Task<string>> probeLatestTagAsync = null, Func<double> clock = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `fetchTextAsync` | `Func<string, int, Task<string>>` | 文本拉取委托（默认走真实网络 GetTextAsync）；测试注入 fake 以覆盖兜底顺序。 |
| `probeLatestTagAsync` | `Func<Task<string>>` | 302 探测委托（默认走真实网络 ProbeLatestTagFromRedirect）。 |
| `clock` | `Func<double>` | 时钟委托（默认 DefaultClock，编辑器启动秒数）；测试注入以覆盖整轮超时分支。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `Task<AesirUpdateService.ReleaseCheckResult>` | — |

</div>

### UpdatePackagesAsync(AesirUpdateService.ReleaseSnapshot, IReadOnlyList<AesirUpdateService.InstalledPackage>, Action<string, float>, Action, Func<bool>) {#method-updatepackagesasync-aesirupdateservice-releasesnapshot-ireadonlylist-aesirupdateservice-installedpackage-action-string-float-action-func-bool}

执行更新：逐包（下载 → 静默导入 → 按清单差集清理残留 → 登记安装清单）。 下载经 DownloadUnityPackageAsync 逐线路兜底（直连 → 镜像站代理）； 用户取消时以 UpdateResult（Cancelled = true）正常返回， 已导入的包保持有效、剩余目标如实列入 SkippedDirNames； 其他失败仍向上抛出（由窗口层弹错误框，异常消息含手动下载指引）。

``` csharp
[AsyncStateMachine]
public static async Task<AesirUpdateService.UpdateResult> UpdatePackagesAsync(AesirUpdateService.ReleaseSnapshot snapshot, IReadOnlyList<AesirUpdateService.InstalledPackage> targets, Action<string, float> onProgress, Action onImportStarting, Func<bool> isCanceled = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `snapshot` | `AesirUpdateService.ReleaseSnapshot` | 检测结果快照；Info 同时承载新清单， 302 重定向降级路径无清单，残留清理自动跳过。 |
| `targets` | `IReadOnlyList<AesirUpdateService.InstalledPackage>` | 执行目标列表，须已按依赖顺序排列（Architecture 先于 Modules）； 可含补装目标（Version 为空，见 ComputeUpdateTargets）。 |
| `onProgress` | `Action<string, float>` | 进度回调（阶段描述 + 0~1 进度）。 |
| `onImportStarting` | `Action` | 导入开始回调（编排层在此收起自家进度条——导入期间 Unity 会显示自己的导入进度条，两条并存会互相覆盖； 导入结束后下一次 onProgress 回调重新显示）。进度条收放权属编排层，服务不直接触碰 UI。 |
| `isCanceled` | `Func<bool>` | 取消探测委托（下载阶段每帧评估；用户点进度条「取消」后返回 true）。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `Task<AesirUpdateService.UpdateResult>` | — |

</div>

### DownloadBytesAsync(string, Action<float>, int, int, Func<double>, Func<bool>) {#method-downloadbytesasync-string-action-float-int-int-func-double-func-bool}

GET 二进制内容（用于下载 unitypackage），通过回调上报 0~1 下载进度。 双重墙钟上限：总时长 timeoutSeconds 与"无进展" stallTimeoutSeconds——任一超限即 Abort 并抛超时异常， 保证上层进度条必定收尾（不会出现长时间卡住的进度条）。

``` csharp
[AsyncStateMachine]
public static async Task<byte[]> DownloadBytesAsync(string url, Action<float> onProgress = null, int timeoutSeconds = 120, int stallTimeoutSeconds = 30, Func<double> clock = null, Func<bool> isCanceled = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `url` | `string` | — |
| `onProgress` | `Action<float>` | — |
| `timeoutSeconds` | `int` | — |
| `stallTimeoutSeconds` | `int` | — |
| `clock` | `Func<double>` | — |
| `isCanceled` | `Func<bool>` | 取消探测委托（每帧评估一次）：返回 true 时 Abort 请求并抛 OperationCanceledException——给卡在慢速线路上的用户提供逃生门， 不必等待超时判据触发。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `Task<byte[]>` | — |

</div>

### DownloadUnityPackageAsync(AesirUpdateService.ReleaseSnapshot, string, Action<string, float>, Func<bool>) {#method-downloadunitypackageasync-aesirupdateservice-releasesnapshot-string-action-string-float-func-bool}

下载指定包的 unitypackage：按「GitHub Release 直链 → GitHub 镜像站代理」顺序逐线路尝试， 单条线路失败（超时 / 连接失败）自动落到下一线路，每条线路独立适用双重墙钟超时。 用户取消（isCanceled 返回 true）立即抛 OperationCanceledException， 不再尝试后续线路。全部线路失败时抛出含各线路错误明细与手动下载指引的异常—— 用户网络全部线路不可达时仍有明确的自助路径。

**备注**

镜像站（GitHubMirrorPrefixes）以「前缀 + 完整 GitHub 地址」形态代理 Release 资产， 与版本检测的镜像站兜底共用域名清单；属第三方公益服务，可用性与限速不受本框架控制， 故仅作为直连失败后的兜底、绝不排在直连之前。jsDelivr 不代理 Release 资产，不在此列。

``` csharp
[AsyncStateMachine]
public static async Task<byte[]> DownloadUnityPackageAsync(AesirUpdateService.ReleaseSnapshot snapshot, string packageDirName, Action<string, float> onProgress = null, Func<bool> isCanceled = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `snapshot` | `AesirUpdateService.ReleaseSnapshot` | — |
| `packageDirName` | `string` | — |
| `onProgress` | `Action<string, float>` | 进度回调（当前线路展示名, 0~1 进度）；换线路时进度从零重报。 |
| `isCanceled` | `Func<bool>` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `Task<byte[]>` | — |

</div>

### BuildChangelogDigestAsync(string, IReadOnlyList<AesirUpdateService.InstalledPackage>, Func<double>) {#method-buildchangelogdigestasync-string-ireadonlylist-aesirupdateservice-installedpackage-func-double}

生成「本地版本 → 远程版本」的更新日志摘要文本（每包一节，按传入顺序）。
远程优先：按 tag 拉取包内 CHANGELOG.md 并提取比本地新的段落；远程拉取失败时回退本地包内 CHANGELOG 的最新段落并标注来源。日志属辅助信息，任何单包失败不影响其余包与其摘要输出。

整轮墙钟预算 DetectionTotalTimeoutSeconds 秒：拉取阶段逃逸在检测预算之外 （每包最多 4 个 jsDelivr 域 × 5s + GitHub Raw 12s，双包串行最坏约 64 秒）会让坏网络用户 长时间停在不可取消的进度条上（本页与检测阶段均用 DisplayProgressBar，无取消按钮）—— 预算耗尽后剩余包不再发起远程拉取，直接回退本地日志，把最坏时长压到"预算 + 单次请求超时"。 预算只拦截"下一个包"的拉取，在途请求仍按自身超时收尾，故总时长上限为预算 + 单次请求超时。

``` csharp
[AsyncStateMachine]
public static async Task<string> BuildChangelogDigestAsync(string remoteTag, IReadOnlyList<AesirUpdateService.InstalledPackage> outdatedPackages, Func<double> clock = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `remoteTag` | `string` | — |
| `outdatedPackages` | `IReadOnlyList<AesirUpdateService.InstalledPackage>` | — |
| `clock` | `Func<double>` | 时钟委托（默认 DefaultClock）；测试注入以覆盖预算耗尽分支。 |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `Task<string>` | — |

</div>

### FetchPackageChangelogAsync(string, string) {#method-fetchpackagechangelogasync-string-string}

拉取指定 Release 版本的包内 CHANGELOG.md（jsDelivr 多域名 → GitHub Raw 兜底，首个成功即返回）。 全部失败时抛出含各源错误明细的异常。

``` csharp
[AsyncStateMachine]
public static async Task<string> FetchPackageChangelogAsync(string tag, string packageDirName)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `tag` | `string` | — |
| `packageDirName` | `string` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `Task<string>` | — |

</div>

### GetTextAsync(string, int, Func<double>) {#method-gettextasync-string-int-func-double}

GET 文本内容（UnityWebRequest，编辑器主线程异步等待）。超时秒数由调用方显式给出 （各检测源有各自的超时基线，不给默认值以防新调用方不慎超出 5 秒检测基线）。 除 request.timeout（无数据超时）外再加墙钟硬上限：超时即 Abort 并抛超时异常。

``` csharp
[AsyncStateMachine]
public static async Task<string> GetTextAsync(string url, int timeoutSeconds, Func<double> clock = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `url` | `string` | — |
| `timeoutSeconds` | `int` | — |
| `clock` | `Func<double>` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `Task<string>` | — |

</div>

### ProbeLatestTagFromRedirect(Func<double>) {#method-probelatesttagfromredirect-func-double}

向 GitHub releases/latest 发起禁止重定向的 HEAD 请求，从 302 Location 中提取最新 tag。 完全绕开 API 限流（该路径不走 api.github.com）。

``` csharp
[AsyncStateMachine]
public static async Task<string> ProbeLatestTagFromRedirect(Func<double> clock = null)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `clock` | `Func<double>` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `Task<string>` | — |

</div>

### IsFreshInstall(AesirUpdateService.InstalledPackage) {#method-isfreshinstall-aesirupdateservice-installedpackage}

目标是否为补装（本地未安装、由「全部更新」新安装的包）——以 Version 为空判定。

``` csharp
public static bool IsFreshInstall(AesirUpdateService.InstalledPackage package)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `package` | `AesirUpdateService.InstalledPackage` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### IsGitRepository() {#method-isgitrepository}

当前项目根目录是否存在 .git（若是 AesirFramework 开发仓库，执行更新会覆盖本地源码）。

``` csharp
public static bool IsGitRepository()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `bool` | — |

</div>

### DefaultClock() {#method-defaultclock}

默认时钟：编辑器启动以来的秒数（测试可注入自定义时钟以验证超时分支）。

``` csharp
public static double DefaultClock()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `double` | — |

</div>

### CompareVersion(string, string) {#method-compareversion-string-string}

比较两个语义化版本号（允许 v/V 前缀，缺省段按 0 处理）。 数字段相同时正式版高于预发布版（1.0.0-rc1 < 1.0.0），两侧均为预发布按标识文本排序。 返回值：a < b 为负，相等为 0，a > b 为正。

``` csharp
public static int CompareVersion(string a, string b)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `a` | `string` | — |
| `b` | `string` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `int` | — |

</div>

### DeleteStaleEntries(IEnumerable<string>) {#method-deletestaleentries-ienumerable-string}

删除残留条目：文件直接删除（连带同名 .meta）；目录仅在其为空时删除。 返回实际删除的条目数。传入的路径必须是项目相对路径。

``` csharp
public static int DeleteStaleEntries(IEnumerable<string> stalePaths)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `stalePaths` | `IEnumerable<string>` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `int` | — |

</div>

### PruneEmptyDirectories(string) {#method-pruneemptydirectories-string}

自底向上删除指定包目录下的空目录（连带 .meta，不删除包根目录本身）。 返回删除的目录数。用于残留文件删除后收尾清理空目录。

``` csharp
public static int PruneEmptyDirectories(string packageAssetsPath)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `packageAssetsPath` | `string` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `int` | — |

</div>

### BuildCdnDelayHintText() {#method-buildcdndelayhinttext}

CDN 中转延迟提示（仅当结果来自 CDN 中转时展示）。

``` csharp
public static string BuildCdnDelayHintText()
```

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

### BuildConnectionStatusText(bool) {#method-buildconnectionstatustext-bool}

连接状态文案：直连可用 = 本次结果 100% 实时；否则说明结果来自兜底线路。

``` csharp
public static string BuildConnectionStatusText(bool gitHubDirectAvailable)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `gitHubDirectAvailable` | `bool` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

### BuildDetectionSummary(string, AesirUpdateService.ReleaseRouteKind, bool) {#method-builddetectionsummary-string-aesirupdateservice-releaseroutekind-bool}

检测结果摘要（连接状态 + 获取线路两行，界面合并展示）。

``` csharp
public static string BuildDetectionSummary(string routeName, AesirUpdateService.ReleaseRouteKind kind, bool gitHubDirectAvailable)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `routeName` | `string` | — |
| `kind` | `AesirUpdateService.ReleaseRouteKind` | — |
| `gitHubDirectAvailable` | `bool` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

### BuildJsDelivrUpdateInfoUrl(string) {#method-buildjsdelivrupdateinfourl-string}

jsDelivr CDN 上的 update-info.json 地址（分支引用，缓存最长约 12 小时）。

``` csharp
public static string BuildJsDelivrUpdateInfoUrl(string domain)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `domain` | `string` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

### BuildRouteText(string, AesirUpdateService.ReleaseRouteKind) {#method-buildroutetext-string-aesirupdateservice-releaseroutekind}

获取线路文案：最终结果是经哪条线路拿到的（直连 / 镜像站 / CDN 中转）。

``` csharp
public static string BuildRouteText(string routeName, AesirUpdateService.ReleaseRouteKind kind)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `routeName` | `string` | — |
| `kind` | `AesirUpdateService.ReleaseRouteKind` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

### ExtractJsonField(string, string) {#method-extractjsonfield-string-string}

从 JSON 文本中按字段名提取第一个双引号字符串值（不依赖第三方 JSON 库）。

``` csharp
public static string ExtractJsonField(string json, string fieldName)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `json` | `string` | — |
| `fieldName` | `string` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

### ExtractTagFromLocation(string) {#method-extracttagfromlocation-string}

从 releases/latest 重定向地址中提取 tag（如 .../releases/tag/v0.15.0 → v0.15.0）； 不匹配返回 null。

``` csharp
public static string ExtractTagFromLocation(string location)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `location` | `string` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

### GetRouteKindLabel(AesirUpdateService.ReleaseRouteKind) {#method-getroutekindlabel-aesirupdateservice-releaseroutekind}

线路类别的展示标签（界面「获取线路」用）。

``` csharp
public static string GetRouteKindLabel(AesirUpdateService.ReleaseRouteKind kind)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `kind` | `AesirUpdateService.ReleaseRouteKind` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

### LoadLocalChangelog(string) {#method-loadlocalchangelog-string}

读取本地包内 CHANGELOG.md 全文；文件不存在返回 null。

``` csharp
public static string LoadLocalChangelog(string packageAssetsPath)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `packageAssetsPath` | `string` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

### RenderChangelogText(IReadOnlyList<AesirUpdateService.ChangelogSection>) {#method-renderchangelogtext-ireadonlylist-aesirupdateservice-changelogsection}

将版本段落渲染回可读的 markdown 文本（标题行 + 正文，段落间空行分隔）。

``` csharp
public static string RenderChangelogText(IReadOnlyList<AesirUpdateService.ChangelogSection> sections)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `sections` | `IReadOnlyList<AesirUpdateService.ChangelogSection>` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

### ToAbsolutePath(string) {#method-toabsolutepath-string}

将项目相对路径转为绝对路径。

``` csharp
public static string ToAbsolutePath(string projectRelativePath)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `projectRelativePath` | `string` | — |

</div>

**返回值**

<div class="api-returns-table" markdown="1">

| 类型 | 说明 |
| :--- | :--- |
| `string` | — |

</div>

### SaveLocalManifest(AesirUpdateService.FilesManifest) {#method-savelocalmanifest-aesirupdateservice-filesmanifest}

写入本地安装清单（自动创建 .aesir 状态目录）。

``` csharp
public static void SaveLocalManifest(AesirUpdateService.FilesManifest manifest)
```

**参数**

<div class="api-params-table" markdown="1">

| 名称 | 类型 | 说明 |
| :--- | :--- | :--- |
| `manifest` | `AesirUpdateService.FilesManifest` | — |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
