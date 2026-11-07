# CODELY.md — AesirFramework

## 项目概览

**AesirFramework**（原 Unity-Aesir-Packages）是 **Runestone Yuumix** 开发的 Unity/团结引擎框架集合，提供渐进式 MVC 架构框架与功能模块。

- **Unity 版本：** 2022.3.62f3c1
- **渲染管线：** URP（Universal Render Pipeline 14.0.12）
- **许可证：** MIT
- **作者：** [yuumixcode](https://github.com/yuumixcode)
- **语言：** C#（代码注释和 XML 文档使用中文，代码标识符使用英文）
- **代码规模：** 约 312 个 .cs 文件（含 Samples 与 Samples~ 双份示例源），分布在 2 个包中（Architecture 约 188、Modules 约 124）
- **仓库定位：** 纯 Unity 工程仓库 — Codely 本地工具链（`.codely-cli/`、`.codely.packages/`、`.codely/`、`.codelyignore`）已全部 gitignore，仅 `CODELY.md` 概述文档保留入库

### 包列表

| 包名 | 包 ID | 版本 | 命名空间 | 说明 |
|------|------|------|---------|------|
| Aesir Architecture | `cn.runestone.aesir.architecture` | 0.32.0 | `Runestone.AesirArchitecture` | 渐进式 MVC 架构框架 — 能力接口组合、命令/查询模式、轻量事件（MiniEvent）与响应式属性（ObservableValue）、PlayerLoop 生命周期（AesirPlayerLoop）、纯 C# 架构根 + MonoBehaviour 适配层 |
| Aesir Modules | `cn.runestone.aesir.modules` | 0.32.0 | `Runestone.AesirModules` | 功能模块 — 轻量级 UI 框架（Manager-of-Managers 单例、四层 Canvas 层级、面板生命周期、可替换资源加载器）+ 事件模块 + 音频模块 + 场景模块 + 脚本文档生成模块（需 Odin） |

> **Aesir Inspector 已独立**：迁出为独立公开仓库，定位为专门面向 Odin Inspector 开发者的学习工具包，不再随本仓库分发。

### 依赖关系

- **Aesir Architecture** — 不依赖任何 Aesir 子包，可独立安装
- **Aesir Modules** — 依赖 `cn.runestone.aesir.architecture`（0.32.0）
- **Aesir Inspector** — 独立公开仓库，与本仓库无依赖关系

---

## Aesir Architecture（0.32.0）

> 框架以 **MVC 为主要模式**，`IController` 是推荐的快速开发入口；`IPresenter`（MVP）作为可选的严格分层模式。

### 核心设计

框架采用**能力接口组合**模式。每个角色（View、Controller、Presenter、Command、Query、Service、Model）通过组合细粒度能力接口来定义：

- `ICanGetModel` / `ICanGetService` — 读取已注册模块
- `ICanExecuteCommand` / `ICanExecuteQuery` — 写/读分发
- `ICanSetContext` / `IContextHolder` — 上下文绑定

### 模块角色

| 角色 | 接口 | 能力 | 说明 |
|------|------|------|------|
| **Model** | `IModel` → `AbstractModel` | GetModel | 数据层；仅通过 Command 写入 |
| **Service** | `IService` → `AbstractService` | GetModel, GetService | 跨模块协调；不能执行 Command/Query |
| **View** | `IView` | GetModel, GetService | 只读访问；不能执行 Command |
| **Controller** | `IController` | GetModel, GetService, ExecuteCommand, ExecuteQuery | MVC 模式入口（推荐） |
| **Presenter** | `IPresenter` | 全部 Controller + IDisposable | MVP 模式（可选）；中介 Model ↔ View |
| **Command** | `ICommand` → `AbstractCommand` | Execute()，只写无返回值 | 修改 Model 状态 |
| **Query** | `IQuery<TResult>` → `AbstractQuery` | Execute() → TResult，只读 | 返回数据，无副作用 |

### 上下文系统

- `IContext` — 模块注册与获取；`GetModel`/`GetService` 未注册时抛 `InvalidOperationException`（不返回 null）
- `AbstractContext<T>` — 纯 C# 单例实现（不依赖 MonoBehaviour）
  - `Configure()` 抽象方法 — 在此注册 Model 和 Service
  - `Instance` 静态属性 — 懒加载单例访问器；初始化成功后才赋值 `_instance`，失败不缓存、根因异常每次抛出
  - `Initialize()` — 调用 `Configure()`，然后按注册顺序初始化 Model → Service；失败不做回滚
  - `Dispose()` — 逆序销毁 Service → Model（按注册逆序，`GenericLocator<T>` 保序）
- **泛型角色 DIM 家族** — `IView<T>` / `IController<T>` / `IPresenter<T>` 以默认接口实现把 `IContextHolder.Context` 绑定到 `AbstractContext<T>.Instance`：View 适配对（AesirView/MonoView）经 `IView<T>`、VC 适配对（AesirViewController/MonoViewController）经 `IController<T>` 绑定（手写显式实现已删除）

### 关键运行时类

- **`AesirArchitecture`** — MonoBehaviour 单例（`[DefaultExecutionOrder(-999)]`），预放置/运行时创建统一由 `[SerializeField] bool dontDestroyOnLoad = true` 序列化字段控制
- **`AesirMonoBehaviour`** — 架构感知 MonoBehaviour 基类
- **`AesirScriptableObject`** — 架构感知 ScriptableObject 基类
- **`ObservableValue<T>`** — 响应式属性；Model 持有可写实例，View 通过 `IReadOnlyObservableValue<T>` 订阅。支持 `SetValueSilently`、`AddListenerAndInvoke`
- **`ObservableList<T>` / `ObservableDictionary<TKey,TValue>` / `ObservableHashSet<T>`** — 可观察集合（ObservableCollections 高频内置子集；三种高频集合，队列等其余形态用上游）；**单轨变更通知** `AddListener` / `RemoveListener`（`IObservableCollection<T>`，`MiniEvent<T>` 承载，载荷为普通只读结构体 `CollectionChangedEventArgs<T>`，返回 `AutoRemoveListenerHandle` 可绑 Unity 生命周期）；语义：无变更不通知、批量逐项、字典值更新为 Replace（旧值在 OldItem）、Move 单事件、Sort/Reverse/Clear 统一 Reset；只读接口为不变型（结构体事件参数与协变冲突 CS1961）
- **`MiniEvent` / `MiniEvent<T>`** — 轻量级零分配事件系统（直接多播调用）；返回 `AutoRemoveListenerHandle` 自动清理。异常语义 = 原生 C# 事件（fail-fast）
- **`MonoLifecycleProxy`** — 生命周期代理，将 Unity 原生回调统一为可订阅的 MiniEvent；调用期增删监听为**快照语义**（`_invoking` 标志 + `_pendingChanges` 挂起队列，趟末按发生顺序应用，对齐原生多播委托）
- **`GenericLocator<T>` / `IGenericLocator<T>`** — 类型键控的服务定位器（保序注册/查询）；AbstractContext 容器字段声明为 `IGenericLocator<T>`（接口含 GetAllEntries 诊断成员、继承 IDisposable）；`GetAll()` 返回调用时刻快照——枚举期间注册/注销不抛"集合已修改"；注册是否立即初始化只看「集体初始化是否已完成」（未完成只登记，完成后立即初始化），初始化遍历期注册的模块由 Initialize() 内的待初始化循环同轮补齐
- **`AesirPlayerLoop`** — PlayerLoop 注入；`EnsureInjected()` 公开 API + `Register` 期自动检测
- **`ResetStaticsAssistant`** — 仅服务泛型类的静态重置（泛型类 RIOLM 被 Unity 静默跳过）

### 渐进式示例家族（六档）

| 档位 | 示例 | Model 暴露面 | 读写路径 | View 边界 |
|------|------|-------------|---------|-----------|
| MVC-1 快捷 | Counter-Mvc-Quick | 具体类，可写 ObservableValue | View 兼 Controller 直写直读 | `MonoViewController<T>` |
| MVC-2 标准 | Counter-Mvc-Standard | 具体类，只读暴露 + 写方法 | Controller 直调写方法 | View 与 Controller 分离 |
| MVC-3 严格 | Counter-Mvc-Strict | 接口注册，只读暴露 + 写方法 | Command 写 + Query 读 | View 按窄接口持有 Controller |
| MVP-1 快捷 | Counter-Mvp-Quick | 具体类，可写 ObservableValue | Presenter 直写 + 推送 | 纯 MonoBehaviour，零接口 |
| MVP-2 标准 | Counter-Mvp-Standard | 具体类，只读暴露 + 写方法 | Presenter 直调写方法 | View 契约 `IXxxView` |
| MVP-3 严格 | Counter-Mvp-Strict | 接口注册，只读暴露 + 写方法 | Command 写 + Query 读 | View 按窄接口持有 Presenter |

### Samples 双目录结构（0.14.0 起）

- **`Samples/`**（编写主位）— 仓库内直接可见、可运行；每个示例目录含独立 asmdef、Scripts/Scene/Prefab
- **`Samples~/`**（发布镜像）— Git URL 安装后经 Package Manager → Samples 标签页按需导入；内容由 `Samples/` 同步而来，两份保持一致
- 同步方向：**先编写 `Samples/`，再同步到 `Samples~/`**（含 .meta，GUID 与 UPM 导入链路一致）
- **构建剔除（2026-09-06 修订）** — 示例程序集为**运行时程序集**（不设 includePlatforms）+ 每个示例脚本**整文件 `#if UNITY_EDITOR` 包裹**：编辑器内正常编译、场景可挂载、Play Mode 可运行，玩家构建时整体编译剔除（示例类型 0 入包）。纯编辑器工具（PlaneWarMono.Editor、RuntimeInitializeLoadType）仍为 Editor-only asmdef。动因：Editor-only asmdef 的 MonoBehaviour 会被 Unity 判定为"编辑器脚本"而**禁止挂载到场景物体**（Missing Script），示例场景无法运行。示例内无 Resources 目录、无构建场景引用，资产亦不入包
- **安装位置锚点机制（2026-09-24）** — 参照 Odin Inspector 的 OdinPathLookup.asset：每包包根放空壳锚点资产 `AesirPathLookup.asset`（`AesirPathLookup : ScriptableObject`，RAA Editor 程序集；锚点 .meta GUID 固定写入 `AesirAssetPaths` 常量——RAA d2965568e68354d3ca31d6085ac86865、RAM 465d45b44c6cf426882248dceeea94a0），`Editor/Common/AesirAssetPaths.cs` 静态构造按「默认根 Directory.Exists → 锚点 GUID 查询 → FindAssets("t:AesirPathLookup") 兜底」三级解析安装根列表（仅覆盖 Assets 安装形态；UPM 路径固定无需锚点），域重载后自动重解析；锚点资产 Inspector 由 `AesirPathLookupAssetEditor` 防误删说明 + 期望/实际 GUID 比对。消费端三处：构建剔除（IsSampleScene 注入根重载）、更新器（扫描+确认框文案动态根）、Getting Started（包扫描多根 + `PackageRootPath` 动态拼示例根）。更新器 unitypackage 导入仍装回默认位置（Unity 固有），确认框与 README 已声明。测试 `AesirAssetPathsTests.cs`（6 用例）
- **示例场景构建钩子（2026-09-24）** — `Editor/Common/AesirSamplesBuildFilter.cs`：[InitializeOnLoadMethod] 经 `BuildPlayerWindow.RegisterBuildPlayerHandler` 拦截 Build Settings 窗口构建入口，把场景列表中的 Aesir 示例场景（定位安装根下的 `<包>/Samples/`——经 AesirAssetPaths 锚点定位、Runestone 可移动到项目任意文件夹，默认 `Assets/Runestone`；与 `Assets/Samples/Aesir Architecture|Modules/<版本>/`）从**本次构建选项**剔除并输出 `[Aesir Build]` 日志（Build Settings 持久数据不动）；改 `BuildPlayerOptions.scenes` 后转交 `DefaultBuildMethods.BuildPlayer`。不用 IPreprocessBuildWithReport（场景列表已快照、改 EditorBuildSettings 污染持久数据）；CI/自定义脚本直接调 `BuildPipeline.BuildPlayer` 不经过钩子。测试 `Tests/Editor/AesirSamplesBuildFilterTests.cs`（10 用例，含移动形态注入根）。另：Check for Updates 菜单 priority 990→**1100** 置 Tools/Aesir 最底部（与上方工具组最大 1002 差 >10 自动插独立分割线），关联注释（GetStarted/PlaneWarSceneSetup×2/ScriptDocGeneratorMenuPaths）已同步
- **命名空间规范** — `Runestone.AesirArchitecture.Samples.<示例名>`（MvcQuick / MvcStandard / MvcStrict / MvpQuick / MvpStandard / MvpStrict / PlaneWarMono / ObservableCollections）；`MiniEvent` 与 `ObservableValue` 两示例因命名空间段与所演示的框架类型同名冲突（CS0118），保留前缀 `Runestone.AesirArchitecture.Samples`

### 设计边界（极简原则）

- **事件总线 / EventChannel** — 不做；跨模块通信使用互相 GetModel + ObservableValue 订阅，或直接引用 MiniEvent
- **Context 多实例** — 不做；CRTP 泛型单例，多存档/多房间在业务层建模
- **Command/Query 池化、async、Undo/Redo** — 不做；保持同步、无缓存
- **时间调度的完整形态** — 不做（0.30.0 后连帧粒度原语 `AesirScheduler` 也整体移除，全仓零使用）；需要延时用 `MonoLifecycleProxy` 的帧代理配计时字段，或直接把驱动方挂在 GameObject 上用协程
- **View 生命周期脚手架** — 不做；面板生命周期由 Aesir Modules 的 UIModule 负责
- **线程安全** — 不做；仅保证主线程使用；集合内部不加锁（仅主线程约定）
- **集合高级能力** — 不做；队列等其他集合形态 / 同步视图与过滤器 / R3 集成 / 环形缓冲 / 栈 / XAML 绑定用上游 Cysharp.ObservableCollections
- **监听回调不应抛异常** — 框架约定（fail-fast），异常直接向上传播由 Unity 记日志
- **`Configure()` 中禁止访问 `Instance`** — 会递归创建第二个上下文实例
- **`Register` 与 `Get` 必须使用相同类型参数** — 按键精确匹配
- **运行时替换 Model/Service 仅用于测试调试** — 旧实例被 Dispose，其上的订阅不会迁移（会输出 Warning 日志）
- **第三方 SDK 修改 PlayerLoop 后手动调用 `AesirPlayerLoop.EnsureInjected()`** — `Register` 注册回调时会自动检测补插
- **写入纪律档位** — 快捷/标准档表现层可直写 Model；标准档起表现层写入必经 Command；严格档只读 + 写方法；Service 可直写
- **示例不进构建包** — 示例程序集为运行时程序集 + 整文件 `#if UNITY_EDITOR`，构建整体剔除（见 Samples 双目录结构节；勿改回 Editor-only asmdef——其脚本禁止挂载场景物体）

### Documentation 双目录结构（0.14.0 起）

- **`Documentation/`（编写主位）** — Assets 中可见、随 unitypackage 导出；两包各一份，存放 README_EN、教学文档与 AI Skill 文档集（AesirFramework-Skill 全框架 AI 编码指南：六档模板 + 模块速查 + 陷阱/事件决策表收编 + jam 实战手册；AesirBootScene-Skill 启动场景创建；2026-10-15 整合，旧 AesirArchitecture-Skill 与两份独立文档已删除）；LICENSE.md 与 Third Party Notices.md 位于包根（UPM 约定位置，随 unitypackage 导出）
- **`Documentation~/`（UPM 镜像）** — Git URL 安装时包内的隐藏副本；内容无 .meta（UPM 不导入它），由 `Documentation/` 同步而来（同步时排除 .meta 与 .DS_Store）
- **同步方向** — 先编写 `Documentation/`，再同步到 `Documentation~/`
- **构建剔除** — .md 为 Unity 官方 TextAsset 格式（2022.3 手册明确列出 .md；本项目 Unity 2022.3.62f3c1 实测经 TextScriptImporter 导入），但无任何场景/资产引用，玩家构建自动排除；约定该目录只放文档，不放会被引用的资产
- **双目录动因** — 与 Samples 同理：`~` 目录对 AssetDatabase 不可见、不进 unitypackage，而 Release 下载与包内更新器的用户同样需要随包文档

### 包内更新器（Editor）

- **`AesirUpdateService` / `AesirUpdateWindow`（IMGUI 兜底）/ `AesirUpdateWindowOdin`（Odin 版）** — 服务与兜底窗口位于 `Editor/UpdateChecker/`（`Runestone.AesirArchitecture.Editor` 程序集），Odin 窗口位于 `Editor/OdinInspector/UpdateChecker/`（Odin 编辑器程序集，ODIN_INSPECTOR 条件编译）；菜单 `Tools/Aesir/Check for Updates` 经 `AesirUpdateWindow.OdinWindowOpener` 静态委托路由：Odin 程序集经 [InitializeOnLoadMethod] 注册打开方式，未装 Odin 时落回 IMGUI 兜底窗口，两版共用全部服务逻辑；**菜单经 validate 按安装形态显隐**（纯本地目录 IO 扫描）：扫描不到 Assets 形态的 Aesir 包安装时整体隐藏——纯 UPM 安装的副本由 Package Manager 管辖，更新器无管辖对象；Assets 形态安装（含与 UPM 混合并存）时照常显示
- 面向"代码导入 Assets/Runestone（非 UPM）"的用户：经 `AesirAssetPaths` 锚点定位安装根（Runestone 可移动到项目任意文件夹）后扫描 `*/package.json` 识别本地安装（`ScanInstalledPackagesFromAllRoots` 多根合并、跨根同包去重）；版本检测三层兜底（`CheckLatestReleaseAsync`，首个成功即返回，单源超时 5 秒即落下一层）：① **直连 GitHub（最实时）**——Releases API（未认证 60 次/时/IP）→ releases/latest 的 302 Location 探测（绕开 API 限流）→ 仓库内 `.github/update-info.json` 的直连 raw（无 API 限流且带文件清单，缓存仅数分钟）；② **镜像站**——`ghproxy.net` / `gh-proxy.com` 代理 raw 内容（版本实时）；③ **CDN 中转**——jsDelivr 四域名（cdn/testingcf/gcore/fastly，分支缓存最长约 12 小时，故排最后）。直连可用时绝不用后两层（保证「能连 GitHub 就是最新」）；只有 tag 的结果按同 tag 校验补齐文件清单（拒绝陈旧清单，避免错删文件）。检测结果带线路模型（`ReleaseRouteKind` / `DetectionAttempt` / `ReleaseCheckResult`），窗口显示「GitHub 直连是否可用 + 最终获取线路」并在落到 CDN 中转时提示延迟，另有「检测详情（各层尝试）」折叠区。**超时与进度条**：单源检测 5 秒、整轮检测 30 秒（`DetectionTotalTimeoutSeconds`，超时后不再发起新请求并留痕「跳过：整轮检测已超时」）、下载「总时长 120 秒 + 连续 30 秒无进展」双判据（`TimeoutBudget` 墙钟判据——`UnityWebRequest.timeout` 只覆盖"完全无数据"，慢速滴水不触发），任一超时中止请求并抛明确异常、上层 `finally` 必定 `ClearProgressBar`；**两包连续更新的流程连续性**：导入 unitypackage 会带来脚本变更，中途域重载会让异步流程随旧域消失（第二个包等不到、进度条停在导入文案、`Busy` 被 `[NonSerialized]` 重置后按钮又可点）——现整段更新用 `EditorApplication.LockReloadAssemblies()` 锁住重载（**实测可推迟编译后的重载**），全部收尾后才解锁一次；导入期间先收起本工具进度条避免与 Unity 自带导入条重叠；域重载兜底收尾（`SessionState` 标记 + `[InitializeOnLoadMethod]`）清理被强杀流程残留的进度条与重载锁。unitypackage 下载始终走 GitHub Release 直链（jsDelivr 不代理 Release 资产）
- 更新入口（拆分）：包列表每行「更新」按钮 = `RequestUpdateSingle`（单包，供只装一个包的项目按需更新）；工具栏「全部更新」= `RequestUpdateAll`，语义为"补全 + 更新"——目标 `ComputeUpdateTargets` = 过期包 + 缺失的已知包补装（已知包唯一登记 = `AesirGetStartedService.KnownPackages`，含 DirName/Id/显示名/描述全字段，更新器与 Getting Started 概览卡片共用；补装目标 Version 为空、落位默认安装根），按钮可见性 = 有待更新包或缺失包
- 更新流程：确认框二次确认（文案构建位于编排控制器 `AesirUpdateController`（SRP：服务层不混 UI 文案）；`BuildUpdateAllConfirmation` 全部更新 / `BuildSingleUpdateConfirmation` 单包——逐包列示「本地 → 远程」并标注「更新 / 新安装」、公共尾注含覆盖回滚指引（可从 GitHub Releases 下载旧版 unitypackage 重新导入）/装回默认位置/保持前台提示，检测到 `.git` 追加开发仓库警告；单包更新时另一已知包在场且落后则前置「配套版本警告」，提示但不阻止）→ 下载 `<包目录名>-v<版本>.unitypackage`（Release 资产命名约定；`DownloadUnityPackageAsync` 按「GitHub 直连 → 镜像站代理（ghproxy.net / gh-proxy.com 前缀 + 完整 Release 地址）」逐线路兜底，全部失败异常附手动下载指引；`DownloadBytesAsync` 带取消探测委托，进度条经 `DisplayCancelableProgressBar` 可随时取消，取消后温和收尾——区分已完成导入与未更新的包）→ 按"上次安装清单 − 新版清单"差集删除残留条目（导入成功后执行，仅限本包目录内；两侧清单任一为空即跳过删除——对称守卫防半截 JSON 误删整包；不误伤用户新增文件）→ `AssetDatabase.ImportPackage` 静默导入 → 逐包合并登记 `.aesir/installed-manifest.json`（更新中途域重载时已导入包的状态保证正确落盘）；执行逻辑下沉 `AesirUpdateService.UpdatePackagesAsync`（返回 `UpdateResult`：已完成包/是否取消/未更新包，双窗口共用；导入期收进度条经 `onImportStarting` 回调交控制器执行）
- 更新日志：检测成功后按远程 tag 拉取包内 `CHANGELOG.md`（jsDelivr 多域名 → GitHub Raw 兜底，`FetchPackageChangelogAsync`），提取「本地 → 远程」区间的版本段落显示在窗口面板（`BuildChangelogDigestAsync`，整轮 30 秒墙钟预算——此前双包串行最坏约 64 秒且不可逃生；耗尽后剩余包不再远程拉取）；远程拉取失败回退本地包内 CHANGELOG 最新段落并标注来源
- 结构要点：远程版本 / 检测结果 / 更新日志为窗口序列化字段（更新导入触发域重载后内容不丢）；Odin 窗口行视图模型仅在状态变化时重建并预计算显示文本与颜色，IMGUI 窗口缓存过期包列表与 `.git` 检测 —— OnGUI 期间零 LINQ、零磁盘 IO
- CI（auto-release.yml）发版后把 `update-info.json`（version/tag + 各包文件清单，`build_unitypackage.py --update-info` 生成，数组结构兼容 JsonUtility）以 `[skip ci]` 提交回 main 供 jsDelivr 拉取；Release 资产仅 3 个 unitypackage。仅给 tag 的检测路径（API / 302 探测）会尽力补齐清单，补齐不到则更新时自动跳过残留清理
- 设计边界：UPM（Git URL）安装的副本不在管辖内（不在 Assets 下，扫描不到），请用 Package Manager 更新；开发仓库（存在 `.git`）窗口显示警告且确认框追加警示勿更新——Release 内容会覆盖本地源码；实现参考 QFramework PackageKit（版本记录随包走 + 先删后导），增强点为精确差集清理
- EditMode 单测：`Tests/Editor/AesirUpdateServiceTests.cs`（版本比较 / package.json 解析 / 清单差集 / 残留删除与空目录回收 / 清单 JSON / Release 资产定位 / CHANGELOG 解析·区间筛选·渲染 / 差集双侧空守卫）；`AesirUpdateControllerTests.cs`（Busy 收尾配平 / 中断恢复解锁 / 确认框文本——随文案职责迁移）

### Getting Started 窗口（0.24.0）

- `Tools/Aesir/Getting Started`（priority -980 居 Tools/Aesir 顶部 + 独立分割线；大于 Tools/Odin 组的 -1000，消除两组优先级打平导致的组间排序抖动），窗口标题「Aesir Getting Started Window」
- `AesirGetStartedService`（数据层）+ `AesirGetStartedWindow`（IMGUI 兜底，核心 Editor 程序集）+ `AesirGetStartedWindowOdin`（Odin 版，页面栈 + 包卡片 + 概览垂直收起/页间水平滑动动效，参照 Odin Inspector Getting Started，经 `OdinWindowOpener` 静态委托路由，与更新器同模式）
- 以各包 package.json samples 清单为唯一真源（新增示例零代码跟进）；包发现覆盖 Assets 安装（经 AesirAssetPaths 锚点多根）/ 嵌入式包 / UPM Git 安装（未导入示例保留条目，窗口内一键导入，见交互行）
- 交互：整卡点击 = PingSample 定位示例文件夹 + Toast；有场景卡「打开场景」按钮 = OpenSampleScene（先保存当前场景一次再切换）+ Toast；未导入卡（UPM / 嵌入式）整卡点击 = 导入引导 Toast（「请点击右侧「导入 Sample」按钮导入 xxx Sample 案例」）、「导入 Sample」按钮 = ConfirmAndImportUpmSample（EditorUtility.DisplayDialog 确认框含示例介绍与导入后位置，可取消 → 确认后经 UnityEditor.PackageManager.UI.Sample API 导入到 Assets/Samples/<包显示名>/<版本>/<示例显示名>/；本引擎 Sample.Import() 返回 bool、ImportOptions 为 internal 只能无参调用、已导入幂等跳过、清单匹配不到红色 Toast 兜底）；导入成功 RefreshAfterImport 重扫并重绑当前包页（PackagePage.Rebind）。测试 AesirGetStartedServiceTests（finder 参数注入伪造，不触碰真实 Package Manager）

### Odin Inspector 集成

- 通过 `ODIN_INSPECTOR` 定义符号条件编译
- Odin 编辑器处理器使用独立 asmdef：`Runestone.AesirArchitecture.Editor.OdinInspector`（Editor/OdinInspector/，ODIN_INSPECTOR 条件编译）；运行时无独立 Odin 程序集，核心运行时程序集经 `#if ODIN_INSPECTOR` 直接使用 Sirenix API（Sirenix DLL 以自动引用方式接入，未安装 Odin 时相关代码整体编译剔除）
- 预放置实例风险通过 Odin AttributeProcessor 注入 Warning InfoBox
- DDOL 开关字段级 InfoBox — `[Tooltip]` 迁移为 AttributeProcessor 注入的 Info 级信息框（样式与逻辑分离，运行时程序集零 Inspector 样式特性）

### 自定义定义符号

- `AESIR_ARCHITECTURE` — 由 `EnsureAesirArchitectureDefine` 自动设置（注意：`SetScriptingDefineSymbols` 值未变化时不落盘，宏变更后需 grep `ProjectSettings.asset` 验证）
- `ODIN_INSPECTOR`、`ODIN_INSPECTOR_3`、`ODIN_INSPECTOR_3_1`、`ODIN_INSPECTOR_3_2`、`ODIN_INSPECTOR_3_3` — 由 Sirenix 管理
- `AESIR_INSPECTOR` — 已随 AesirInspector 迁出而移除（2026-09-05 已从 ProjectSettings.asset 全平台清除）

---

## Aesir Modules（0.32.0）

### UI 框架

- **`UIModule`** — 通过 `AesirModules.GetOrAddChild<T>()` 创建的 MonoBehaviour 单例，管理面板生命周期
  - 静态 API：`UIModule.Show<T>()`、`UIModule.Hide<T>()`、`UIModule.Get<T>()`、`UIModule.Prewarm<T>()`、`UIModule.RegisterPrefab<T>()`
  - 面板状态：激活 → 停用（缓存）→ 销毁
  - `PrewarmAll()` — 通过协程逐帧预实例化
- **`IUIPanel`** — 生命周期契约：`Initialize → Show(payload) → Hide → DestroyPanel`
  - `Layer`（UILayer）、`DestroyOnHide`（bool）、`IsOpen`（bool）
- **`AesirBasePanel`** — MonoBehaviour 面板基类；`OnDestroy` 静态反清理 `UIModule.RemovePanelRecord`
- **`UIRoot`** — 构建四层 Canvas 层级；`UILayer` 枚举定义层级；层 Canvas / UICamera / EventSystem 为 `[SerializeField]` 序列化引用持久化（`List<LayerCanvasEntry>`）
- **`IUIAssetLoader` / `ResourcesUILoader`** — 可插拔资源加载（默认 Resources；可替换为 Addressables）
- **`BinderAssistant` / `BinderTag`** — UI 元素组件绑定系统（OdinInspector 程序集）；窗口感知（0.24.0）——基类下拉含 `AesirBaseWindow` 家族，根节点带 Canvas 时默认脚本名后缀 `Window`、默认基类直指 `AesirBaseWindow`
- **Canvas 根窗口形态（0.24.0）** — 与 Panel 并列的第二种 UI 形态：`IUIWindow` 契约（`OnInit → OnShow → OnHide → OnClose`）+ `AesirBaseWindow` / `AesirBaseWindowView<T>` / `AesirBaseWindowViewController<T>` 基类家族；窗口预制体根节点自带 Canvas，实例化后直接挂 UIRoot 下（不经四层 Canvas），`sortingOrder` 序列化字段为单一真源（默认 500 恒在面板四层之上）；预制体结构约定 `Mask`（蒙版）+ `Content`（内容容器）子物体
  - 静态快捷 API：`UIModule.Open<T>(payload, path)` / `Close<T>()` / `GetWindow<T>()` / `PrewarmWindow<T>()` / `RegisterWindowPrefab<T>()` / `ContainWindowPrefab<T>()`（实例侧为 `OpenWindow` / `CloseWindow` 等长名）；与 Panel 共用预制体注册表与 loader、实例注册表独立、跨契约误用报错
  - 蒙版遮罩：初始值配置于 `UIModuleConfigSO` 单例资产（编辑器自动创建于 `Assets/Resources/UIModuleConfig/`；可经 `RegisterConfigLoader` 注册加载器替代 Resources 兜底，否则走 Resources → 内存默认实例三级解析），运行时 `MaskMode` 属性切换重算；蒙版点击经 `AesirBaseWindow.OnMaskClicked()` 虚方法，默认 `closeOnMaskClick`（默认 false）关闭
  - 命名规范：窗口类/脚本/预制体名以 `Window` 结尾（`LoadingWindow` 式，不取 `XxxCanvas`——会被误读为 Canvas 派生类）
  - 示例：`Samples/UI/01_BasicUsage`（面板与窗口协作 + 蒙版对照 + 全屏加载窗口，场景 SampleForUIBasic）
- **`SceneModule`** — 场景管理模块：公开 API 全静态门面（直接 `SceneModule.xxx`，实例侧全私有、守护用例锁定）+ 协程/UniTask 双驱动（宏 `AESIR_MODULES_UNITASK` 自动维护——UPM 包经 versionDefines、其他形态经编辑器宏维护器 `AesirUniTaskDefineKeeper`；含 UniTask 时内部流程改 UniTask 驱动）+ 适配程序集 `Runestone.AesirModules.UniTask`（`SceneModuleUniTask` 可 await API，宏关闭整体不编译）+ `SceneModuleConfigSO` 单例配置（启动场景兜底 + 进度归一化上限，Resources 兜底 + 可注册加载器）

### 事件模块

- EventModule V2：EventModule 直接继承 AesirMonoBehaviour；双注册表分离（AttributeBindings + DynamicBindings）
- `BindingInfo` 基类仅含 BindingKey/Subscriber/Priority + 抽象 `Invoke()`；`StaticBindingInfo` 持有 MethodInfo + 表达式树编译委托；`DynamicBindingInfo<T>` 持有 `Action<T>` 直接委托；Script 订阅返回 `AutoRemoveListenerHandle`
- `SubscriberPriority` 枚举：First/High/Medium/Last（4 档；High=Attribute 订阅默认、Medium=Script 订阅默认）
- 事件参数载体继承 `AesirEventArgs`，命名为 XxxEventArgs（仅作数据载体流经 EventModule 调度；支持 WithFilter/WithFilters 链式声明订阅者过滤器）
- 分发增强（2026-09-11）：订阅者过滤器（`ISubscriberFilter` 策略接口 + 内建 WithTag/WithPriority/SameSceneAsEmitter/OnlySelf/InsideCollider2D，fail-closed）；死引用清理（分发期自动移除已销毁订阅者 + 编辑器告警）；性能监控（`executionMsLimit` 毫秒阈值，默认 0 关闭）；`AesirListenerAttribute` 补 AllowMultiple=true
- 热路径绑定键缓存（2026-09-11）：`AesirEventUtility.GetEventBindingKey` 按事件类型缓存 `AssemblyQualifiedName`（原生拼接 ~1µs/次且每次新分配字符串，缓存后 ~20ns 字典查询零分配）——分发热路径稳态零分配；性能特征回归测试锁定（键缓存引用同一性 + 编译委托 vs 反射 20 万次对比计时，实测加速 ~78 倍），22 用例 EventModuleTests
- SO 资产化（2026-09-11）：`AesirEventArgsSO`（CreateAssetMenu → Aesir/Event Module/，以资产为发布者 Raise）+ `UnityEventOnAesirEvent` 桥接组件 + `SubclassSelector`/`ExcludeSubclassSelector`（[SerializeReference] 子类下拉，UI Toolkit Drawer，运行时程序集放特性/编辑器程序集放 Drawer）+ SO 自定义 Inspector（运行模式限定 Raise 按钮）
- 系统事件（元事件）与 DefaultChannel 暂缓（依赖编辑器工具链）；单实例合并不实施（Awake 已销毁重复实例，订阅均经 Instance 单一通道）
- 设计文档位于 `Aesir-Notebook/docs/modules/event/`（私有笔记仓 Aesir-Notebook：`feature-roadmap.md` / `sample-plan.md`）
- EditMode 测试：Tests/Editor/EventModuleTests.cs（19 用例，含过滤器/死引用清理/性能监控/SO/多特性订阅）

### 音频模块

- `AudioModule` — 全静态门面（经 `AesirModules.GetOrAddChild<T>()` 创建 MonoBehaviour 单例）：SFX 用 N 个独占 AudioSource 轮询（默认 8，等效池化、每播音调/音量独立）；BGM 专用 loop 源 + 独立淡变系数协程淡入淡出（unscaledDeltaTime）；三通道音量乘法链 + PlayerPrefs 持久化（键前缀 `AudioConfigSO` 可配）
- 设计边界：不做 3D（原生 PlayClipAtPoint 替代）/ AudioMixer / 每音效 Stop / 播完回调
- 示例：`Samples/Audio/01_BasicUsage`（OnGUI 面板 + 自包含 wav + 场景 SampleForAudioBasic）

### DDOL 机制

- 6 个单例类（`AesirArchitecture`、`AesirModules`、`AudioModule`、`SceneModule`、`UIModule`、`UIRoot`）均有 `[SerializeField] bool dontDestroyOnLoad = true`，预放置/运行时创建统一由该字段控制；该字段一律声明为类体第一个字段
- 关闭 DDOL 仅在 Inspector 给出信息框提示（Odin 条件 Warning / DetailedInfoBox），不输出运行时 Warning 日志
- `UIModule` 的字段仅在预放置为根物体时生效，运行时自动创建时挂载于 [Aesir Modules] 宿主下跟随宿主决策

### 依赖补全（0.24.0）

- package.json **不再声明任何依赖**（2026-09-27 实测裁决）：Unity Package Manager 不支持包内 Git URL 依赖（仅项目 manifest.json 可声明，官方硬规定——保留该声明会使 UPM 单装本包直接失败，报 `Version 'https://...' is invalid. Expected a 'SemVer' compatible value.`）；`com.unity.test-framework` 硬依赖亦移除（测试程序集已有 UNITY_INCLUDE_TESTS 守卫，非必需）。**RAA 缺席由 Install Dependencies 菜单补装兜底**（补装 URL 为常驻 `latest` 分支固定常量，发版零联动项）
- `AesirDependencyInstaller`（`Editor/Bootstrap/`，菜单 `Tools/Aesir/Modules/Install Dependencies`，priority 998）— RAA 缺失时一键补装（触发场景：unitypackage 只导入本包、或 UPM 单装本包——因包内依赖声明不可用）：确认窗口 → `Client.Add`（URL 为固定常量 `#AesirArchitecture-latest` 常驻分支，分支名永久固定，一次输入持续更新）；RAA 缺失检测 = UPM `PackageInfo.GetAllRegisteredPackages()` + Assets 形态 `AesirPathLookup` 锚点定位包根按 package.json `name` 字段判定；成功收尾经 `SessionState` + `[InitializeOnLoadMethod]` 跨域重载
- `Runestone.AesirModules.Editor.Bootstrap` 为**零引用程序集**（references 空数组）——铁律：不得向其加对 RAM 核心 / RAA / Odin 的引用，否则缺依赖时菜单自身不编译
- 测试：Tests/Editor/AesirDependencyInstallerTests.cs（12 用例）

### 场景编辑器

- `SceneModuleSettingsWindow` / `SceneModuleSettingsWindowOdin` — Scene 模块设置双窗口（菜单 `Tools → Aesir/Modules/Scene Module Settings`，OdinWindowOpener 路由，无 Odin 环境落 IMGUI 兜底；Odin 版经 asmref 汇入守卫程序集，修复 RAM 主 Editor 程序集无 Odin 编译失败）
- `BootstrapSceneHelper` — 场景引导工具
- `SceneAssetWrapper` — 可序列化场景资源引用
- 场景测试卫生 — `Tests/Editor/Scene/` 与 `Tests/Runtime/` 的场景测试不依赖宿主工程内容：场景路径按文件名经 AssetDatabase 定位；`SceneAssetWrapperTests` 在宿主缺失 `Assets/Scenes/SampleScene.unity` 时从包内最小场景夹具复制、TearDown 按「谁创建谁删除」还原；`SceneModulePlayModeTests` 的 BuildSettings 登记经 `IPrebuildSetup` / `IPostBuildCleanup` 仅存在于运行期间（另有域加载兜底清扫），并由 `SceneModuleTestSceneHygieneTests` 守护

### 脚本文档生成模块（需 Odin）

- 位于 `Runtime/ScriptDocGenerator/OdinInspector/` 与 `Editor/ScriptDocGenerator/OdinInspector/`，整体经 asmref 汇入 `Runestone.AesirModules.OdinInspector` / `Runestone.AesirModules.Editor.OdinInspector`（强依赖 Odin Inspector，未安装自动排除）；`Editor.OdinInspector` asmdef 因本模块新增对运行时 Odin 程序集的引用
- 命名空间 `Runestone.AesirModules.ScriptDocGenerator`(.Editor)；全部 public API 跨程序集访问（与原独立 asmdef 一致，无 InternalsVisibleTo）
- 功能：反射生成结构化 API 文档（增量保留 `## Additional Notes` 后手写内容）+ Summary 工具（XML `<summary>` ↔ `[Summary]` 双向同步，自动补 `using Runestone.AesirModules.ScriptDocGenerator;`）
- 静态 API（`ScriptDocGeneratorAPI`，Editor/OdinInspector/）：面板的无 UI 等价入口，供自动化脚本与 AI 直接调用——`GenerateDocsForType/ForTypes/ForAssembly(短名或 FullName)/ForFolder(文件夹含子文件夹全部脚本，源码扫描映射类型、普通类不依赖 MonoScript.GetClass)`；`settings=null` 用 Default 预设、`outputFolder=null` 用项目根 ScriptDocGenerator/；返回 `ScriptDocGenerationResult`（写入清单/设置名/未解析类型名）；全程无弹窗不打开文件，覆盖语义=面板多程序集模式（增量合并）；写入核心 `WriteTypeDocSilently`（internal）面板与 API 共用；配套 `FindAllSettings()` 枚举全部生成器设置资产
- 入口 `Tools → Aesir → Modules → Script Doc Generator`；Assets 右键 `Assets/Script Doc Generator`
- 测试位于包根 `Tests/Editor/ScriptDocGenerator/`（命名空间 `Runestone.AesirModules.Tests.Editor.ScriptDocGenerator`，255 用例）；模块文档 `Documentation/script-doc-generator.md`（镜像 `Documentation~/`）

### Samples 双目录结构（0.14.0 起，与 Architecture 同规则）

- `Samples/`（编写主位）与 `Samples~/`（发布镜像）并存；事件模块示例目录 `Events/01_KeyPress`、`Events/02_Filters`、`Events/03_SOAsset`（与 package.json samples 路径一致；02/03 含场景与 SO 资产）
- 示例程序集 `Runestone.AesirModules.Samples.Events.KeyPress` 为运行时程序集 + 整文件 `#if UNITY_EDITOR`（构建剔除）；命名空间 `Runestone.AesirModules.Samples.Events.KeyPress`

---

## 程序集定义

### Aesir Architecture（16 个 asmdef）

| 程序集 | 路径 | 说明 |
|--------|------|------|
| `Runestone.AesirArchitecture` | Runtime/ | 核心运行时 |
| `Runestone.AesirArchitecture.Editor` | Editor/ | 编辑器（含 QuickCreateSOMenuItem、EnsureAesirArchitectureDefine） |
| `Runestone.AesirArchitecture.Editor.OdinInspector` | Editor/OdinInspector/ | ODIN_INSPECTOR |
| `Runestone.AesirArchitecture.Tests` | Tests/Runtime/ | PlayMode 测试（MonoLifecycleProxy 快照语义等） |
| `Runestone.AesirArchitecture.Tests.Editor` | Tests/Editor/ | EditMode 测试（110 个） |
| `Runestone.AesirArchitecture.Samples.MvcQuick` | Samples/Counter-Mvc-Quick/Scripts/ | 运行时 + #if UNITY_EDITOR |
| `Runestone.AesirArchitecture.Samples.MvcStandard` | Samples/Counter-Mvc-Standard/Scripts/ | 运行时 + #if UNITY_EDITOR |
| `Runestone.AesirArchitecture.Samples.MvcStrict` | Samples/Counter-Mvc-Strict/Scripts/ | 运行时 + #if UNITY_EDITOR |
| `Runestone.AesirArchitecture.Samples.MvpQuick` | Samples/Counter-Mvp-Quick/Scripts/ | 运行时 + #if UNITY_EDITOR |
| `Runestone.AesirArchitecture.Samples.MvpStandard` | Samples/Counter-Mvp-Standard/Scripts/ | 运行时 + #if UNITY_EDITOR |
| `Runestone.AesirArchitecture.Samples.MvpStrict` | Samples/Counter-Mvp-Strict/Scripts/ | 运行时 + #if UNITY_EDITOR |
| `Runestone.AesirArchitecture.Samples.MiniEvent` | Samples/MiniEvent/Scripts/ | 运行时 + #if UNITY_EDITOR |
| `Runestone.AesirArchitecture.Samples.ObservableValue` | Samples/ObservableValue/Scripts/ | 运行时 + #if UNITY_EDITOR |
| `Runestone.AesirArchitecture.Samples.ObservableCollections` | Samples/ObservableCollections/Scripts/ | 运行时 + #if UNITY_EDITOR |
| `Runestone.AesirArchitecture.Samples.PlaneWarMono` | Samples/PlaneWar/Scripts/Mono/ | 运行时 + #if UNITY_EDITOR |
| `Runestone.AesirArchitecture.Samples.RuntimeInitializeLoadType` | Samples/RuntimeInitializeLoadType/ | Editor-only asmdef（初始化时机演示） |

### Aesir Modules（15 个 asmdef + 7 个 asmref）

> 标准 Unity 自定义包根结构：包根两级目录 `Runtime/` 与 `Editor/`，功能模块以子目录形式存在于对应层级（如 `Runtime/UI/`、`Editor/UI/`），模块间零依赖；删除模块 = 删 `Runtime/<模块>/` 与 `Editor/<模块>/`。核心程序集锚点在层根，层内模块主代码自动汇入；细分程序集为第三方适配/集成，锚点在 `Integration/` 下，模块专属代码（`OdinInspector/`、`Addressables/` 子目录）经 Assembly Definition Reference（asmref）汇入。

| 程序集 | 锚点路径 | 说明 |
|--------|------|------|
| `Runestone.AesirModules` | Runtime/（层根） | 核心运行时锚点（引用 AesirArchitecture）；Common/UI/Scene/Events 主代码自动汇入 |
| `Runestone.AesirModules.OdinInspector` | Runtime/Integration/OdinInspector/ | Binder 全家桶（ODIN_INSPECTOR）；Runtime/UI/OdinInspector/ 与 Runtime/ScriptDocGenerator/OdinInspector/ 经 asmref 汇入 |
| `Runestone.AesirModules.UniTask` | Runtime/Integration/UniTask/ | UniTask 适配（defineConstraints AESIR_MODULES_UNITASK + versionDefines com.cysharp.unitask）；SceneModuleUniTask 可 await API |
| `Runestone.AesirModules.Editor` | Editor/（层根） | 核心编辑器锚点；Common/UI/Scene 编辑器代码自动汇入 |
| `Runestone.AesirModules.Editor.OdinInspector` | Editor/Integration/OdinInspector/ | Odin 处理器（ODIN_INSPECTOR）；Editor/UI、Editor/Scene、Editor/ScriptDocGenerator 的 OdinInspector/ 经 asmref 汇入 |
| `Runestone.AesirModules.Editor.Addressables` | Editor/Integration/Addressables/ | Addressables 胶水（AESIR_MODULES_ADDRESSABLES）；Editor/Scene/Addressables/ 经 asmref 汇入 |
| `Runestone.AesirModules.Editor.Bootstrap` | Editor/Bootstrap/ | 依赖补全菜单 AesirDependencyInstaller（零引用程序集，references 空数组——不得引入任何 Aesir/Odin 引用） |
| `Runestone.AesirModules.InputSystem` | Runtime/UI/InputSystem/ | UIRoot 输入模块替换（ENABLE_INPUT_SYSTEM，独立可选） |
| `Runestone.AesirModules.Tests.Editor` | Tests/Editor/ | 包级 EditMode 测试（Binder/EventModule/Audio/UIModule/SceneModule/ScriptDocGenerator/依赖补全；引用 core+Odin+Bootstrap，Editor-only + UNITY_INCLUDE_TESTS） |
| `Runestone.AesirModules.Tests` | Tests/Runtime/ | 包级 PlayMode 测试（SceneModule 真实加载成功路径 5 用例 + TestScenes 最小场景资产；BuildSettings 登记经 IPrebuildSetup/IPostBuildCleanup 仅存在于运行期间 + 域加载兜底清扫，仅编辑器编译） |
| `Runestone.AesirModules.Samples.Events.KeyPress` | Samples/Events/01_KeyPress/ | 运行时 + #if UNITY_EDITOR |
| `Runestone.AesirModules.Samples.Events.Filters` | Samples/Events/02_Filters/ | 运行时 + #if UNITY_EDITOR |
| `Runestone.AesirModules.Samples.Events.SOAsset` | Samples/Events/03_SOAsset/ | 运行时 + #if UNITY_EDITOR |
| `Runestone.AesirModules.Samples.Audio.BasicUsage` | Samples/Audio/01_BasicUsage/ | 运行时 + #if UNITY_EDITOR |
| `Runestone.AesirModules.Samples.UI.BasicUsage` | Samples/UI/01_BasicUsage/ | 运行时 + #if UNITY_EDITOR |

---

## 场景

| 场景 | 路径 | 用途 |
|------|------|------|
| SampleScene | `Assets/Scenes/SampleScene.unity` | 默认 Unity 示例场景 |
| SampleForCounterMvcQuick | `Assets/Runestone/AesirArchitecture/Samples/Counter-Mvc-Quick/Scene/` | MVC 快捷档计数器示例 |
| SampleForCounterMvcStandard | `Assets/Runestone/AesirArchitecture/Samples/Counter-Mvc-Standard/Scene/` | MVC 标准档计数器示例 |
| SampleForCounterMvcStrict | `Assets/Runestone/AesirArchitecture/Samples/Counter-Mvc-Strict/Scene/` | MVC 严格档计数器示例 |
| SampleForCounterMvpQuick | `Assets/Runestone/AesirArchitecture/Samples/Counter-Mvp-Quick/Scene/` | MVP 快捷档计数器示例 |
| SampleForCounterMvpStandard | `Assets/Runestone/AesirArchitecture/Samples/Counter-Mvp-Standard/Scene/` | MVP 标准档计数器示例 |
| SampleForCounterMvpStrict | `Assets/Runestone/AesirArchitecture/Samples/Counter-Mvp-Strict/Scene/` | MVP 严格档计数器示例 |
| MiniEventSample | `Assets/Runestone/AesirArchitecture/Samples/MiniEvent/Scene/` | MiniEvent 使用示例 |
| ObservableValueInspector | `Assets/Runestone/AesirArchitecture/Samples/ObservableValue/Scene/` | ObservableValue Inspector 演示 |
| ObservableCollectionsSample | `Assets/Runestone/AesirArchitecture/Samples/ObservableCollections/Scene/` | 可观察集合（List/Dictionary/HashSet）使用示例 |
| SampleForPlaneWarMono | `Assets/Runestone/AesirArchitecture/Samples/PlaneWar/Scene/` | PlaneWar 纵版射击（Mono 版）示例 |
| SampleForEventFilters | `Assets/Runestone/AesirModules/Samples/Events/02_Filters/Scene/` | 事件模块订阅者过滤器对照示例 |
| SampleForEventSOAsset | `Assets/Runestone/AesirModules/Samples/Events/03_SOAsset/Scene/` | 事件模块 SO 资产化 + UnityEvent 桥接示例 |
| SampleForAudioBasic | `Assets/Runestone/AesirModules/Samples/Audio/01_BasicUsage/Scene/` | 音频模块基础用法（OnGUI 面板）示例 |
| SampleForUIBasic | `Assets/Runestone/AesirModules/Samples/UI/01_BasicUsage/Scene/` | UI 模块面板与 Canvas 根窗口协作（蒙版对照）示例 |

> 示例场景均在包内 `Samples/` 目录（`Samples~/` 为镜像）；Git URL 安装的项目经 Package Manager 导入后位于 `Assets/Samples/<包名>/<版本>/`。

---

## 示例

### Aesir Architecture（包内 `Samples/`，镜像 `Samples~/`）

1. **Counter-Mvc-Quick（快捷档）** — `MonoViewController<T>` 直写直读，最少概念跑通数据驱动 UI 闭环
2. **Counter-Mvc-Standard（标准档）** — Model 只读暴露 + 写方法；View 与 Controller 分离共享 Model
3. **Counter-Mvc-Strict（严格档）** — Model 接口注册 + 只读暴露 + 写方法；Command 写 + Query 读；View 按窄接口持有 Controller
4. **Counter-Mvp-Quick（快捷档）** — Presenter 直改可写 ObservableValue（零接口抽象）并推送被动 View
5. **Counter-Mvp-Standard（标准档）** — Model 只读暴露 + 写方法；Presenter 直调写方法 + Model 直读推送
6. **Counter-Mvp-Strict（严格档）** — Command 写 + Query 读；View 按窄接口持有 Presenter，与 MVC 严格档同构
7. **ObservableValue (Odin Inspector)** — `ObservableValue<T>` 自定义 Drawer 演示；需要 Odin Inspector
8. **ObservableCollections（可观察集合）** — `ObservableList<T>` / `ObservableDictionary<TKey,TValue>` / `ObservableHashSet<T>` 使用示例：单轨订阅 `AddListener`（按 Action 分流 Add / Remove / Replace / Move / Reset），ContextMenu 触发增删改查与集合运算；命名空间 `Runestone.AesirArchitecture.Samples.ObservableCollections`
9. **MiniEvent** — `MiniEvent` 和 `MiniEvent<T>` 使用示例（无参/单参事件）
10. **PlaneWar（Mono 版）** — 纵版射击飞机大战实战示例：得分 HUD、三型敌机、重开流程；命名空间 `Runestone.AesirArchitecture.Samples.PlaneWarMono`；RAA 版（Scripts/Raa）待编写
11. **RuntimeInitializeLoadType（初始化时机）** — 五个 `RuntimeInitializeLoadType` 阶段触发顺序演示（SubsystemRegistration → AfterAssembliesLoaded → BeforeSplashScreen → BeforeSceneLoad → AfterSceneLoad），开关经设置窗口控制（`Tools → Aesir → Architecture → Samples/RuntimeInitializeLoadType`）；纯编辑器程序集（Editor-only asmdef），已登记 package.json samples

### Aesir Modules（包内 `Samples/`）

1. **Event Module - Key Press** — 事件模块基本发布-订阅示例：按键发布事件、`[AesirListener]` 静态订阅
2. **Event Module - Filters（过滤器）** — `02_Filters`：Space 发布 `WithTag("Player")`+`InsideCollider2D` 双重过滤警报、R 发布 `OnlySelf` 家族命令；场景 `SampleForEventFilters.unity` 内置圈内/圈外/无标签、家族/无关多组对照组 + uGUI HUD 接收日志（跨平台中文字体）；命名空间 `Runestone.AesirModules.Samples.Events.Filters`
3. **Event Module - SO Asset（SO 资产化）** — `03_SOAsset`：`ScoreEventAsset.asset`（AesirEventArgsSO + managedReference 指向 ScoreChangedEvent，NewScore=100）+ 场景 `SampleForEventSOAsset.unity` 演示 `UnityEventOnAesirEvent` 桥接组件零代码串联 UnityEvent 回调（On Raised 持久绑定 ScoreBulb.Flash）+ ScorePublisher 代码订阅同事件展示双消费方式；命名空间 `Runestone.AesirModules.Samples.Events.SOAsset`
4. **Audio Module - Basic Usage（基础用法）** — `01_BasicUsage`：OnGUI 面板演示 SFX 轮询播放、BGM 淡入淡出切换与三通道音量/静音持久化；场景 `SampleForAudioBasic.unity`；命名空间 `Runestone.AesirModules.Samples.Audio.BasicUsage`
5. **UI Module - Basic Usage（基础用法）** — `01_BasicUsage`：面板与 Canvas 根窗口两种形态协作（Normal 层控制面板 + Top 层日志 HUD 跨平台中文动态字体 + 设置/叠加窗口蒙版单遮/叠遮对照 + 全屏加载窗口 payload 自动关闭）；场景 `SampleForUIBasic.unity`；命名空间 `Runestone.AesirModules.Samples.UI.BasicUsage`

---

## 依赖

### Unity Registry 包

- `com.unity.render-pipelines.universal` 14.0.12 — URP
- `com.unity.feature.2d` 2.0.1 — 2D 工具包
- `com.unity.textmeshpro` 3.0.9 — 文本渲染
- `com.unity.timeline` 1.7.7 — Timeline
- `com.unity.ugui` 1.0.0 — uGUI
- `com.unity.test-framework` 1.1.33 — Unity 测试框架
- `com.unity.ide.rider` 3.0.40 — Rider IDE 支持
- `com.unity.ide.visualstudio` 2.0.27 — VS IDE 支持

> manifest.json 不含任何本地 `file:` 引用（Codely 本地工具链已于 2026-09-05 清理出库）。

### 第三方插件

- **Sirenix Odin Inspector** — 位于 `Assets/Plugins/Sirenix/`（已 gitignore；独立授权）。Architecture 与 Modules 通过 `#if ODIN_INSPECTOR` 条件编译可选集成，未安装时自动排除。

---

## 目录结构

```
AesirFramework/
├── README.md / README_EN.md / LICENSE / CHANGELOG.md / CONTRIBUTING.md / CODELY.md
├── .github/workflows/auto-publish-branches.yml   # CI：main 推送时按包目录 subtree split 滚动更新常驻 latest 分支，并清理废弃版本分支
├── Assets/
│   ├── Runestone/                     # 包源代码
│   │   ├── AesirArchitecture/         # 核心架构框架
│   │   │   ├── Runtime/
│   │   │   │   ├── Core/              # Context 上下文 + MVC/MVP 核心（Engine 纯 C# / Component 适配层）
│   │   │   │   ├── Modules/           # Event(MiniEvent) / CustomLifecycle(MonoLifecycleProxy) / Locator / Observable / Utilities
│   │   │   │   └── Common/            # AesirArchitecture 单例、Debug、ResetStaticsAssistant、PlayerLoop 注入
│   │   │   ├── Editor/                # 定义符号管理、MenuItems/（QuickCreateSO）、Utilities/（ScriptingSymbolEditorUtility）、Common/（锚点 AesirAssetPaths + 构建剔除钩子）、GetStarted/（窗口与数据层）、UpdateChecker/、Odin AttributeProcessors
│   │   │   ├── Tests/                 # Editor 110 个 + Runtime PlayMode 测试
│   │   │   ├── Samples/               # 示例（编写主位，11 个，构建剔除）
│   │   │   ├── Samples~/              # 示例发布镜像（Package Manager 按需导入）
│   │   │   ├── Documentation/         # 文档主位（Assets 可见、随 unitypackage 导出、不进构建）
│   │   │   └── Documentation~/        # 文档镜像（Git URL 安装隐藏副本，无 .meta）
│   │   └── AesirModules/              # 功能模块（标准包根结构，模块间零依赖）
│   │       ├── Runtime/               # 核心运行时锚点（Runestone.AesirModules.asmdef）
│   │       │   ├── Common/            # 共享基础与调试工具（AesirModulesDebug 等）
│   │       │   ├── Integration/       # Odin 运行时锚点（OdinInspector/，第三方适配/集成程序集）
│   │       │   ├── UI/                # UIModule、UIRoot、AesirBaseWindow 窗口家族（InputSystem/ 独立程序集、OdinInspector/ Binder 经 asmref）
│   │       │   ├── Scene/             # SceneModule、SceneAssetWrapper（含 Exceptions/）
│   │       │   ├── Events/            # 事件模块（含 Component/）
│   │       │   └── ScriptDocGenerator/ # 脚本文档生成模块（OdinInspector/ 经 asmref 汇入 Odin 程序集）
│   │       ├── Editor/                # 核心编辑器锚点（Runestone.AesirModules.Editor.asmdef）
│   │       │   ├── Bootstrap/         # 依赖补全菜单 AesirDependencyInstaller（零引用程序集）
│   │       │   ├── Integration/       # Odin 编辑器锚点（OdinInspector/）+ Addressables 胶水锚点（Addressables/）
│   │       │   ├── UI/                # UI 菜单项（OdinInspector/ Processor 经 asmref）
│   │       │   ├── Scene/             # 场景编辑器窗口（Tests/ 测试程序集、OdinInspector/、Addressables/ 经 asmref/asmdef）
│   │       │   └── ScriptDocGenerator/ # 脚本文档生成模块编辑器（OdinInspector/ 经 asmref 汇入 Odin 编辑器程序集）
│   │       ├── Samples/               # 示例（编写主位，Events/、Audio/、UI/）
│   │       ├── Samples~/              # 示例发布镜像
│   │       ├── Documentation/         # 文档主位（Assets 可见、随 unitypackage 导出）
│   │       └── Documentation~/        # 文档镜像（无 .meta）
│   ├── Scenes/                        # SampleScene
│   ├── Settings/                      # URP 资源（UniversalRP, Renderer2D）
│   └── Plugins/Sirenix/               # Odin Inspector（已 gitignore）
└── Packages/manifest.json             # 无本地 file: 引用
```

---

## 开发规范

### 命名

- **命名空间：** `Runestone.AesirArchitecture`（含 `Runestone.AesirArchitecture.Samples.<示例名>` / `.Editor` / `.OdinInspector`）、`Runestone.AesirModules`（含 `Runestone.AesirModules.Samples.<示例名>`）
- **程序集名：** `Runestone.AesirArchitecture.*`、`Runestone.AesirModules.*`；示例程序集名与命名空间一致
- **类名：** PascalCase（如 `AbstractContext`、`ObservableValue`、`MiniEvent`）
- **接口：** `I` 前缀（如 `ICommand`、`IContext`、`IUIPanel`）
- **抽象类：** `Abstract` 前缀（如 `AbstractModel`、`AbstractCommand`）
- **MonoBehaviour 单例：** 静态 `Instance` 属性、`[DefaultExecutionOrder(-999)]`、`dontDestroyOnLoad` 序列化字段统一控制 DDOL
- **私有字段：** `_camelCase`（非序列化）、`camelCase`（`[SerializeField]`）
- **常量/静态只读：** PascalCase
- **Lifecycle 拼写：** 一个单词 Lifecycle（非 LifeCycle）
- **事件参数载体：** 仅作数据载体流经外部调度器 → `XxxEventArgs`；自身持有监听者并有 Invoke 能力 → `XxxEvent`

### 代码风格

> 统一代码风格指南位于根目录 `CodeStyle/AesirCodeStyle.cs`（不参与编译）。

- **XML 文档注释使用中文**（摘要、参数说明、备注）；代码标识符使用英文
- 数据类标记 `[Serializable]`
- 显式接口实现上下文注入（`IContextHolder.Context`、`ICanSetContext.SetContext`）
- 单例 `Instance` getter 优先 `FindAnyObjectByType` 搜索预放置实例（不用已废弃排序语义的 `FindFirstObjectByType`）
- 非泛型单例类内 `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]` 重置静态字段；泛型类经 `ResetStaticsAssistant.Register()` 注册重置回调
- 允许自由使用 `#region` 分段，无最低代码量要求
- MonoBehaviour 运行状态字段一律用显式非序列化字段（自动属性 backing field 会被场景序列化残留）
- Odin 依赖代码隔离在 `OdinInspector/` 子目录（RAA 仅 Editor 侧独立 asmdef；RAM 经 asmref 汇入独立 Odin 程序集）；核心程序集经 `#if ODIN_INSPECTOR` 直接使用 Sirenix API
- Processor：`internal sealed`，与目标类同文件定义（Odin AttributeProcessor 通过 `nameof` 成员匹配，勿改其匹配的成员形式）

---

## 单元测试规范

> 完整规范（白名单/黑名单逐条对照 + 反模式实例 + 密度预算 + 自检 8 问 + 跑测试姿势）：`Aesir-Notebook/docs/standards/unit-testing-standards.md`（私有笔记仓）。**新增与修改的用例必须遵守；存量用例全部保留、不回炉重写**（触碰某测试文件时顺手收敛被触碰部分）。

- **三问门槛**：说不出"什么改动会让它红"、指不出受害者、存在更便宜的等价保护（类型约束 / asmdef 静态守卫 / `[TestCase]` 合表）→ 不写。**不以覆盖率或用例数作为验收指标**。
- **只测六类**：公开 API 行为契约、不显然的不变量、真实缺陷回归（一缺陷一例）、Unity/第三方怪癖（踩过才写）、纯函数等价类边界（每类 1 例）、外部约定格式往返。**优先用静态守护用例替代行为穷举**（如 `AesirSamplesScriptGuardTests`、`SceneModuleTestSceneHygieneTests`）。
- **禁止**：测语言/BCL 已保证的事、平凡 getter 与构造器赋值、私有字段与引用同一性（除单例/复用本身是契约）、生成文本/日志/UI 文案的逐字断言、正则与解析器穷举矩阵、同一行为的多种写法各一例、计时窗口断言、改工程状态的副作用测试、参数组合爆炸。
- **密度**：单个被测类型 3–10 例；>15 例须在文件头写明"互不相同的受害者契约"；单文件 ≤350 行 / ≤20 例；纯重构与内部改名 **0 新增用例**。
- **写法**：`被测对象_场景_期望` 可当一句话读；一例一因（断言必须同因失败）；AAA 且 Act 只一次；断言值不断言实现字面量；新文件用 `Assert.That`、既有文件沿用其风格；日志断言用 `new Regex(...)`（富文本前缀，纯字符串匹配不上）；`TearDown` 异常路径也必须回收。
- **放置**：`Destroy`/`Awake` 真执行、协程/帧/PlayerLoop、场景加载卸载、渲染 → **PlayMode**（EditMode 下 `Destroy` 为 no-op、`AddComponent` 不触发 `Awake`）；资产库/菜单/Inspector/文档生成/纯计算 → EditMode。基线测试程序集**不得**加 `ODIN_INSPECTOR` 门控，Odin 依赖用例放 `*.Tests.Editor.OdinInspector`。
- **可移植**：不依赖宿主场景/BuildSettings/自定义宏/当前打开场景与选中物；需要场景用包内夹具 `AssetDatabase.CopyAsset` + TearDown 删除（禁 `NewScene(Additive)`）；需要 BuildSettings 用 `IPrebuildSetup`/`IPostBuildCleanup`（跑后零残留）；临时文件写 `Temp/`。
- **禁止副作用（事故级）**：测试内写脚本宏（触发域重载 → `Unexpected assembly reload happened while running tests`，整轮丢失）、常驻写 BuildSettings、在 `Assets/` 留资产或改真实 SO、依赖网络、无 deadline 的等待、同步循环用"下一帧才生效"的字段作退出条件、把引擎错误当"预期"用 `LogAssert.Expect` 吞掉。
- **测试红了**：契约有意变 → 改测试 + CHANGELOG 记破坏性；契约没变只是实现变 → 判为测试过拟合，删/简化测试，**绝不为保绿而扭曲实现**。
- **现状（2026-11-08）**：6 个测试程序集 / 68 个测试文件 / 约 780 条用例（最近门禁实跑 831 条含参数化展开）。最密且触碰时优先收敛：`BinderCodeGeneratorTests`(65)、`AesirUpdateServiceTests`(55)、`SourceParsingTests`(50)、`AudioModuleTests`(39)、`EventModuleTests`(36)、`XmlSummaryToolTests`(34)。

---

## 构建与运行

### 编辑器

1. 在 Unity 2022.3.62f3c1（或团结引擎等效版本）中打开项目
2. 打开 `Assets/Runestone/AesirArchitecture/Samples/Counter-Mvp-Quick/Scene/SampleForCounterMvpQuick.unity`
3. 按 **Play** 运行 MVP 快捷档计数器示例（其余示例场景见"场景"节）

### 命令行

```bash
# 编译预热
Unity -batchmode -quit -projectPath . -logFile build.log

# EditMode 测试
Unity -batchmode -projectPath . -testPlatform editmode -runTests \
       -testResults TestResults.xml -logFile test.log
```

尚无自定义构建脚本；示例脚本以整文件 `#if UNITY_EDITOR` 剔除，玩家构建自动排除示例。

---

## 版本控制

### 忽略（按 `.gitignore`）

- `Library/`、`Temp/`、`obj/`、`Build/`、`Builds/`、`Logs/`、`UserSettings/`
- `.codely-cli/`、`.codely.packages/`、`.codely/`、`.codelyignore`、`.com-unity-codely.json` — Codely 本地工具链（2026-09-05 起全部不入库）
- `Assets/Plugins/Sirenix/` — Odin Inspector（独立授权，不分发）
- `Aesir-Notebook/` — 开发笔记（独立私有仓库，Zensical 项目：`zensical serve` 本地预览）
- `AesirFramework-Site/` — 公开产品站（独立公开仓库，Zensical + GitHub Pages）
- `Assets/Vertical 2D Shooting BE4/` — 第三方付费素材原件（PlaneWar 示例内已有自包含拷贝）
- 生成的 IDE 文件（`*.csproj`、`*.sln`、`.vs/`、`.idea/`）、`*.unitypackage`、测试生成场景等

### 纳入版本控制

- `Assets/Runestone/` — 两个包源代码（含 `Samples/` 与 `Samples~/` 双份示例）
- `Assets/Scenes/`、`Assets/Settings/`
- `Packages/manifest.json`（无本地 file: 引用）、`Packages/packages-lock.json`
- `ProjectSettings/`、`CODELY.md`、`README.md`、`LICENSE`

### 分支策略

- `main` — 开发主线
- 常驻分支 `AesirArchitecture-latest` / `AesirModules-latest` — CI 在 main 推送时自动 subtree split 滚动更新（包内容为分支根），**分支名永久固定**：Git URL 一次输入持续更新，升级 = Package Manager 移除后用同一 URL 重新 Add；钉旧版本用 Release tag（`?path=<包路径>#v<版本>`，tag 永久保留）；CI 自动清理废弃的 `Aesir*-v*` 版本分支（旧策略「版本分支随发版轮换并删除」已废弃——钉住它的 URL 会失效报 Could not clone）

---

## 待办 / 待确认

- PlaneWar RAA 版示例（`Samples/PlaneWar/Scripts/Raa/`）待编写
- Event Module V2 后续功能见 `feature-roadmap.md`（Aesir-Notebook 私有笔记仓）
- 无 Addressables 配置（UI 默认 `ResourcesUILoader`；`IUIAssetLoader` 已预留接口）

## Codely Structured Memories

> 2026-11-08 整理：211 条 → 本版。整理前全文归档于 `.codely-cli/memory/archive/CODELY-memories-2026-11-08.md`（含各次发版/审查的逐条过程日志与 commit 号）。本版只保留**跨会话仍成立的规则、裁决、实测坑位与当前基线**；过程叙事、已被后续覆盖的状态、历史发版细节不再入记忆。

### User

- [2026-10-17 11:11:13] [user] 用户是 yuumixcode（Runestone），Aesir 框架作者。仓库 AesirFramework 现维护**两个包**：Aesir Architecture（RAA，`cn.runestone.aesir.architecture`）与 Aesir Modules（RAM，`cn.runestone.aesir.modules`）；Aesir Inspector 已于 0.14 迁出为独立公开仓库，本仓不再包含。偏好中文交流：注释/文档中文、标识符英文、git commit message 中文（subject + body）。

### Feedback

- [2026-08-17 00:07:18] [feedback] **铁律：所有 git 命令必须用 Homebrew git** —— `export PATH=/opt/homebrew/bin:$PATH`（git-lfs 在 `/opt/homebrew/bin/git-lfs`）。**Why:** 工具 bash 子 shell 的 PATH 不含该目录，git 触发 LFS filter 时报 `git-lfs: command not found / the remote end hung up`（exit 128）；禁用 LFS filter 绕过会把指针文件当明文 diff 出大量假修改。**How to apply:** status/diff/add/commit/log/push 一律带该前缀。

- [2026-11-08 02:40:00] [feedback] **框架定位与档位（累计用户裁决，优先级最高）**：①面向**独立游戏**，保持**精简 + 渐进式架构**，受众为框架初学者与中级程序员，内置能力只保留高频项、低频项写进设计边界并引导上游。②**两包差异化**：RAA 极简是第一排序标准（低频公开 API 一律删/收窄）；RAM **功能性比极简更重要**（功能模块的面是价值不是负担）。③核心架构默认走**最低档（Quick）**：不建 Model/Service 接口、不做只读 property 包装、持有引用直接用具体类、`RegisterModel/RegisterService` 注册实现类型；只有确需多实现/替换时才引入接口。④**Model = 运行时数据的底层数据源**（UI 绑定的 ObservableValue + 普通字段 + 选择性存档数据），不是 UI 附属。⑤不做：Context 事件总线 / Context 多实例 / Command·Query 池化·async·Undo / 时间调度原语 / 线程安全 / 框架级对象池。⑥可观察集合只留 **List / Dictionary / HashSet** 三种（ObservableQueue 已于 2026-09-28 删除），单轨 `AddListener` + `CollectionChangedEventArgs<T>` 普通只读结构体载荷，**集合内部不加锁**（只约定主线程）。

- [2026-09-29 13:39:03] [feedback] **类型命名规范**：①**跨包共用**（一个实现两包都用，或框架级公共工件）→ `Aesir` + 语义名，不带包名段（`AesirMonoBehaviour` / `MiniEvent` / `AesirPlayerLoop`）；②**两包各自都有一份同类角色** → 用完整包名 `AesirArchitecture` / `AesirModules` + 语义名（宿主单例、日志门面、AttributeProcessor）；③单包自有且无对照角色 → 现状。**其他命名约定**：生命周期单词写 `Lifecycle`（非 LifeCycle）；事件参数只作数据载体 → `XxxEventArgs`，自身持有监听者并有 Invoke 能力 → `XxxEvent`；MonoBehaviour 运行状态字段一律显式非序列化字段（`bool _isGameOver;` + 只读属性，禁止自动属性——backing field 会被场景序列化残留）；单例类的 `[SerializeField] bool dontDestroyOnLoad` **必须是类体第一个字段**；私有字段 `_camelCase`、序列化字段 `camelCase`；抽象类 `Abstract` 前缀、接口 `I` 前缀；单例 `Instance` getter 用 `FindAnyObjectByType` + `FindObjectsInactive.Include`（不用已废弃的 `FindFirstObjectByType`）；Unity 6 兼容且未废弃的 API 优先。

- [2026-08-15 22:20:13] [feedback] **极简原则（已全部实施，勿回退）**：低概率问题、或因不推荐写法造成的问题，一律用文档约定杜绝，**不加防御性代码兜底**；异常不吞噬（MiniEvent 零分配直调、fail-fast）；不做初始化回滚、不做自愈轮询。加任何"保护措施"前先问：防的问题概率多高？是否用户写错代码？隐性代价是什么？允许自由使用 `#region`（无行数门槛）。

- [2026-09-29 13:20:45] [feedback] **日志输出规范（两包）**：①前缀需加粗并带颜色；②**严禁连续出现两个带颜色的前缀**（两个矩形/中括号框选前缀允许，带色加粗主前缀不得连续两个）；③生产路径（Runtime/Editor）一律走包门面 —— RAA `AesirArchitectureDebug`（`source` 次前缀）、RAM `AesirModulesDebug`（首参 `prefixTag` 模块标签 + 可选 `source`）。**三处有意例外勿"顺手修正"**：`Editor/Bootstrap/AesirDependencyInstaller`（零引用程序集，门面不可用，已写 remarks）、`Samples/`（教学演示前缀）、`Tests/`（被测签名输出）。

- [2026-09-11 10:18:36] [feedback] **注释口径**：源码注释只描述当前版本行为与"为什么"，**不写历史演进**（"此前/原名/参考自"一律进包内 `Documentation/设计变更记录.md`，`Documentation~/` 有镜像）；XML 注释中文；不解释显然代码；非直觉的性能优化（如表达式树编译委托）用 remarks 说明原理、量级对比与缺点。

- [2026-11-08 02:40:00] [feedback] **测试规范（完整版见 `Docs/AesirFramework/单元测试编写规范.md`，私有仓）**：三问门槛（说不出"什么改动让它红"/指不出受害者/有更便宜等价保护 → 不写）；只测六类（公开 API 契约、不显然的不变量、真实缺陷回归、Unity/第三方怪癖、纯函数边界、外部格式往返）；禁止测语言/BCL 已保证的事、平凡 getter、私有字段与引用同一性、生成文案逐字断言、参数组合爆炸；单类型 3–10 例、单文件 ≤350 行；`被测对象_场景_期望`；新文件 `Assert.That`；`TearDown` 异常路径也要回收；**不以覆盖率或用例数作验收**。契约有意变 → 改测试 + CHANGELOG；契约没变只是实现变 → 删/简化测试，**绝不为保绿扭曲实现**。

- [2026-11-08 02:40:00] [feedback] **测试执行与放置（实测硬约束）**：①`Destroy`/`Awake` 真执行、协程/帧/PlayerLoop、场景加载卸载、渲染 → **PlayMode**（EditMode 下 `Destroy` 被引擎拒绝为 no-op、`AddComponent` 不触发 `Awake`）。②**EditMode 测试内禁止写脚本宏**（`PlayerSettings` 写宏会触发全量重编译 + 域重载 → `Unexpected assembly reload happened while running tests` 整轮丢失）；只测 `ContainsSymbol/AddSymbol/RemoveSymbol` 纯函数。③本引擎 nunit 被裁剪：**无 `Assert.ThrowsAsync`**；`async Task` 用例可能被静默忽略 → 断言同步写，用 `GetAwaiter().GetResult()` 驱动已完成 Task。④测试需要场景资产用 `AssetDatabase.CopyAsset(包内夹具)`，**不要** `NewScene(Additive)`（未命名场景会抛异常）；BuildSettings 登记用 `IPrebuildSetup` + `IPostBuildCleanup`（跑后零残留），禁止常驻。⑤PlayMode 门禁必须**退出编辑器走 batchmode**（编辑器内 UTF 1.1.33 会误入 PlayerLauncher 报 "prebuild setup" 错误）：全绿 exit 0、失败 exit 2，需解析 XML；跑前删旧 XML 与 `Temp/UnityLockfile`。⑥日志断言用 `new Regex(...)`（门面富文本前缀 + 多行 Destroy 错误纯字符串匹配不上）。

- [2026-09-25 22:27:54] [feedback] **编辑器内跑 TestRunnerApi 的四步法**：①`Filter.testMode` 必须显式设（`new Filter { testMode = TestMode.EditMode, assemblyNames = new[]{...} }`，不设则静默不跑）；②改完测试文件先 `unity_editor.refresh` 再跑（否则跑旧 DLL，"改了结果永远一样"）；③**别用 `await RunFinished`**（会让 exec_editor_script 超时）：启动脚本立即返回 → 回调把结果写 `SessionState` → 另一脚本轮询；④`TestRunnerApi` 每次新建后 `UnregisterCallbacks`（回调跨 submission 累积），同一会话多次 `Execute` 会互相取消，需 refresh 后单次 Execute 带全部程序集名。结果取值：`ITestResultAdaptor` 的 `ResultState` 是字符串/枚举、无 `TestCaseCount`/`TestFullName`。

- [2026-11-08 02:40:00] [feedback] **编译冻结 / 域重载卡死的诊断链（多次耗费数小时）**：任一程序集编译失败（含 Assembly-CSharp 里用户生成文件）→ **域不重载，全部程序集停留在旧版本**：Bridge 报 `isCompiling=started` 长期不落、磁盘 DLL 已新但反射找不到新成员、测试永远跑旧代码。解法链：先 `grep "error CS" Editor.log` 找出阻塞源并修掉 → `osascript` 按 **PID** 前置编辑器（`tell process "Unity" ... set frontmost`，进程名在本机可能是 `Tuanjie`）→ `CompilationPipeline.RequestScriptCompilation()` 或 Assets/Refresh → 等域重载 → 反射复验类型存在性。**判定"磁盘 DLL 已含新代码"用 python/strings 字节级搜 DLL 里 UTF-16 断言文本**，可区分"没编译"与"没重载"。**SIGILL（exit 132，编辑器无声消失）先读 `~/Library/Logs/DiagnosticReports/Unity-*.ips` 的 `exception.message`**：写 "excessive recursion" 即托管层无限递归爆栈（曾因属性 expression-body 自引用 `=> X;` 笔误），用 `public\s+\w[\w<>\[\],\s]*?\s(\w+)\s*=>\s*\1\s*;` 全仓扫。

- [2026-09-29 13:45:38] [feedback] **`ScriptableSingleton<T>` 落盘两件套**：`[FilePath]` + 每个持久字段 `[SerializeField]`；`instance` 是小写（Unity 原生 API，`.Instance` 报 CS0117）。**判别法铁律**："改脚本触发域重载看值是否保留"**无判别力**（进程内单例对象被 Unity 保留，实例 ID 不变）——正解是看设置文件是否出现字段键 + 清私有静态 `s_Instance`（等价重启）后读回；反复清会产生原生孤实例（下次域重载报 `ScriptableSingleton already exists`），用 `Resources.FindObjectsOfTypeAll<T>()` 销毁多余实例恢复。

- [2026-09-29 13:47:44] [feedback] **程序集划分铁律**：程序集**只按需求面划分** —— 运行时 / 编辑器 / 某插件集成，**不按模块划分**；模块专属代码（Odin 面、Addressables 面）经 **asmref** 汇入对应层的集成程序集，绝不新建"模块名"程序集。Odin 依赖库结构：`Runtime/<模块>/OdinInspector/` + `Editor/<模块>/OdinInspector/` 经 asmref 汇入 `Runestone.<包>.OdinInspector` / `.Editor.OdinInspector`。`Editor/Bootstrap`（依赖补全）是**零引用程序集**（`references: []`），不得加任何 Aesir/Odin 引用，否则缺依赖时菜单自身不编译。

- [2026-09-29 15:59:48] [feedback] **条件编译实测（Unity 2022.3.62f3c1）**：①**asmdef 的 `versionDefines` 产生的宏只对声明它的程序集可见，不是全局宏** —— 凡以某宏作 `defineConstraints` 的程序集必须**各自再声明一份** `versionDefines`，否则被 Unity **静默排除**（零报错、用例凭空消失）。②`defineConstraints` 不满足时先排除程序集再解析 references，故胶水程序集可用 **name 引用**缺失包的程序集。③名称引用指向缺失程序集被 Unity **静默忽略**（不是硬依赖）。④**凡第三方程序集引用/检测一律用 asmdef 名而非命名空间名**（UniTask 的 asmdef 名是 `UniTask`，`Cysharp.Threading.Tasks` 只是命名空间；`AesirUniTaskDefineKeeper` 用白名单检测，有守护测试）。

- [2026-09-29 13:42:18] [feedback] **测试程序集命名与引用面**：命名 = `Runestone.<包名>.Tests` +（EditMode 时）`.Editor` +（模块专属时）`.<模块名>`，即 `Editor` 段插在 `Tests` 与模块段之间（例：`Runestone.AesirModules.Tests.Editor.UniTask`）。基线程序集列 4 条 Sirenix 预编译 DLL（指向不存在程序集会被静默忽略，故有 Odin / 无 Odin 两种环境都能编译）；**真正依赖 Odin 类型（含 Binder、SDG）的用例集中在门控程序集 `*.Tests.Editor.OdinInspector`**，基线程序集不得加 `ODIN_INSPECTOR` 门控（测试经 `AesirMonoBehaviour → SerializedMonoBehaviour` 基类链需要 Sirenix 引用）。`overrideReferences: true` 时需显式列 `nunit.framework.dll`（`isExplicitlyReferenced:1`）。

- [2026-09-06 11:50:20] [feedback] **示例程序集规范**：示例程序集是**运行时程序集**（`includePlatforms: []`）+ 每个示例脚本**整文件 `#if UNITY_EDITOR` 包裹**。**Why:** Editor-only asmdef 的 MonoBehaviour 被判定为编辑器脚本，禁止挂场景（Missing Script），0.14.0 曾因此让全部示例无法运行；方案既能 Play Mode 运行又让构建 0 示例类型入包。**联动**：`AesirSamplesBuildFilter`（`Editor/Common/`）经 `BuildPlayerWindow.RegisterBuildPlayerHandler` 在构建时把示例场景从**本次构建选项**剔除（不改 Build Settings 持久数据），覆盖三类安装形态（Assets 锚点根 / PM 导入 `Assets/Samples/<包>/<版本>/` / Runestone 移动形态）；`AesirSamplesScriptGuardTests` 守护包裹完整性。示例命名空间 `Runestone.<包>.Samples.<示例名>`（`MiniEvent` / `ObservableValue` 两示例保留前缀只因命名空间段与类型同名冲突 CS0118，勿"修正"）。

- [2026-09-22 10:23:48] [feedback] **Samples 双目录同步**：`Samples/` 是编写主位，`Samples~/` 是 UPM 镜像（含 .meta、GUID 一致）；`Documentation/` → `Documentation~/` 同理（镜像排除 .meta 与 `.DS_Store`，注意用 `--exclude='*.meta'` 会误配 `.md.meta`，应写 `--exclude='*.md.meta'`）。同步用 `.codely-cli/skills/sync-samples/scripts/sync_samples.py --check`（纯 Python，逐文件判方向、删镜像多余项、补顶层目录 .meta、校验 package.json samples 清单）。**逐文件 diff 判断方向，不要凭"主位为准"硬覆盖**（历史上主位反而是错的一侧）；PlaneWar 场景若出现 BinderTag / BinderCodeGenerator / MenuProbe 即污染（Architecture 无 Modules 依赖）；本仓示例场景 `GraphicRaycaster` 的 `m_Bits` 惯例值是 55。

- [2026-10-17 11:06:40] [feedback] **产品包 Documentation 三类标准**：只保留 ①英文 `README_EN` ②使用文档 ③AI Skill；迭代过程/内部文档一律写 `Docs/`（私有仓 Aesir-Docs）不随包分发。英文命名：根目录用 `_EN.md` 后缀（`README_EN.md`），子包英文 README 在 `Documentation/README_EN.md`。废弃机制的历史演进统一写入包内 `Documentation/设计变更记录.md`。

- [2026-11-08 02:40:00] [feedback] **文档站（AesirFramework-Docs，独立仓库）约定**：Zensical + GitHub Pages。①**页面正文用半角标点**（与主仓 README 全角不同），从 README 拷贝需转标点。②**中文标题必须用显式英文锚点** `## 标题 {#module-xxx}`（Zensical slugify 会删掉中文字符，含中文锚点的页内链接必 build 失败）；表格加 class 必须用 `<div class="x" markdown="1">` 包裹（`{: .cls }` 对表格无效）。③模板覆写须显式声明 `[project.theme] custom_dir`（当前未使用 overrides）。④**版本落点标注（"<版本> 起/已于 <版本>"）随发版替换为当前版本**，但站点 changelog 历史段永不改写；主仓 CODELY.md 的特性小节括号是**引入版本标注、不随发版替换**。⑤`git status` 对文档站可用 shell 访问（该目录被主仓 .gitignore 排除，glob/search 不可见）。⑥提交身份用 zeriying@gmail.com，`docs:` 中文单行主题。

- [2026-11-08 02:40:00] [feedback] **发版流程（aesir-version-sync）**：两包同号（CI 校验一致）。步骤 = ①`git grep <旧版本号>` 全仓摸底（**grep 输出勿截断**；版本徽章在 README L6 且链接指向 CHANGELOG，`grep -vi changelog` 会误滤，需单独摸 `shields.io/badge/version`；`str.count` 按出现数而非行数）→ ②两包 package.json / 根 CHANGELOG（**当前版本表格行 + 固定分支名 + 新段落**）/ 两包中英 README / 根 README / CODELY.md 正文（**python 限定在 `## Codely Structured Memories` 标记之前替换**，记忆条目里的历史版本号保留）/ 文档站 6 件套 → ③分批提交（fix → feat → fix → docs → chore 版本批）+ 推送 → ④CI：`auto-release.yml` 发 `v<版本>` Release（三 unitypackage：RAA / RAF / RAM）并回写 `update-info.json`（`[skip ci]`，推完需 pull）；`auto-publish-branches.yml` 滚动更新常驻 `AesirArchitecture-latest` / `AesirModules-latest` 分支（**branch 名永久固定**，发版零联动；分发时 `git rm -r Samples Documentation`，latest 根只有 `Samples~` / `Documentation~`），并自动清理废弃 `Aesir*-v*` 分支（**不再手动删分支**）→ ⑤文档站版本批推送 + Deploy 检查。**坑**：根 CHANGELOG 插入用 `%` 格式化会被正文 `%` 撞格式符（用占位符 `.replace()`）；`git add -A` 会带上测试产物（`.gitignore` 已含 `TestResults*.xml` / `playmode-test.log`）；改名文件要连 `.meta` 一起 add。

- [2026-11-08 02:40:00] [feedback] **Odin 面板/绘制坑位（本仓反复踩）**：①**"禁用灰"**：`InspectorProperty.Draw` 对只读属性/只读集合子项推 `PushGUIEnabled(false)`，同链上的 InfoBox/文本全部变灰 → 只读展示成员一律加 **`[EnableGUI]`**（子项要各自标注）；文案用**属性**而非静态字段（静态字段初始化器里调 AssetDatabase 会抛 `GUIDToAssetPath_Internal is not allowed...`）。②**多个 `ShowIf` 是 OR 语义**，需要 AND 时写**一个复合条件属性**。③`[Title]` 的 Subtitle 样式自带 alpha×0.7，深色主题下像 Disabled → 手绘标题（`BoldLabel`/`Label` 派生 + 1px 分隔线）。④`SirenixGUIStyles.MiniButton` 背景不随 rect 高度拉伸（恒 ~18px 锚在顶部）→ 要"变大按钮"换 `SirenixGUIStyles.Button` / `GUI.skin.button`。⑤类级 `DetailedInfoBox` 只能放一个（两个抛 `state already exists`）。⑥InfoBox 的 `$value` 在 bool 字段上指该 bool 值（`@$value.X` 会报找不到标识符），静态文案用 `"$" + nameof(常量)` 编译期拼接（Odin resolved-string 参数必须编译期常量，`static readonly` 报 CS0182）。⑦`TitleGroup` 把成员收进组节点，成员不在 `RootProperty.Children` 顶层（结构探针须递归）；`InspectorProperty.State.Visible` 在 `PropertyTree.Create` 后即有值，可做可见性矩阵探针。⑧窗口 `PropertyTree` 必须 `OnDisable` 里 `Dispose()`；**绘制回调禁止解析资产库单例**（会触发 `CreateAsset + AssetDatabase.Refresh` 全项目重扫并报 `GUIStateObj is deleted`），单例收敛到 `OnEnable` 且按域缓存（Unity fake-null 语义自动失效重建）。⑨AttributeProcessor 用 `internal sealed`，与目标类同文件；成员匹配靠 `nameof`，**勿改被匹配成员的字段/属性形式**。⑩窗口感知的交互语义按用户裁决：概览卡整卡不可点、只有底部按钮可点；示例卡整卡 = PingSample 定位 + Toast，右侧按钮 = 打开场景。

- [2026-09-29 16:00:07] [feedback] **编辑器窗口/脚本验证方法**：①**同步收敛循环必须同步改变循环条件** —— 只置"下一帧才生效"的字段当退出条件会构成主线程死循环（曾连点 4 张卡卡死编辑器；`while (toasts.Count >= Max)` 内必须自己 `RemoveAt`）。②`EditorWindow.SendEvent` 合成鼠标事件**不可靠**（新窗口上连 `GUI.Button` 都不触发，重叠区小按钮会被后声明的整卡抢走）→ 按钮命中只能真鼠标。③`[InitializeOnLoad]`/`[InitializeOnLoadMethod]` 里**禁止直接 `AssetDatabase.CreateAsset` / 写脚本宏**（报 "Unable to import newly created asset" / 重入），一律推迟到 `EditorApplication.delayCall`；**后台失焦时 delayCall 完全不派发**（`activate` + `QueuePlayerLoopUpdate` 都无效）→ 用反射取 `delayCall` 委托表 `DynamicInvoke` 验证，或前置编辑器。④`exec_editor_script` 内 `new GameObject()` / `DestroyImmediate()` **不标脏场景**（验证保存要显式 `MarkSceneDirty`）；`Scene` 结构快照跨 `OpenScene` 失效（先把 path 存成 string）；33 类同框坑：`ReferenceEquals` 必须写 `object.ReferenceEquals`、裸 `Object` 报 CS0104（写 `UnityEngine.Object.*`）、`Transform.Cast<>()` 需 `using System.Linq`、`Instance.GenericMethod<T>` 遇同名静态泛型报 CS0176、`MenuItem` 特性类名是 `UnityEditor.MenuItem`（无 `Attribute` 后缀，字段 `menuItem/priority` 是公共字段）。⑤**窗口像素级验证**：多 Unity 实例时用 Swift `CGWindowListCopyWindowInfo` 按 `kCGWindowOwnerPID` 取 `kCGWindowNumber` → `screencapture -x -o -l <id>`（可截被遮挡窗口）；文字亮度用 PIL 行扫描"最亮 1% 均值"（≈128 禁用 / ≈196 正常）；锁屏时截屏全黑，需用户解锁。⑥临时探针类放 `Assets/Editor/`（编译进真实程序集、测量循环零反射开销），用完连 `.meta` 一起删。

### Project

- [2026-11-08 02:40:00] [project] **当前基线（0.32.0，2026-10-17 发布）**：Unity **2022.3.62f3c1（Unity 中国版，不是团结引擎）**、URP 14.0.12、C# 9。程序集规模：RAA 16 个 asmdef；RAM 15 个 asmdef + 7 个 asmref。门禁：EditMode `batchmode -testFilter "Runestone"` **831 total / 828 pass / 0 fail / 3 skip**（skip = Addressables 自适应 ×2 + batchmode 未保存场景下的预期 Ignore）、PlayMode batchmode **31/31**、`refresh` 0 错 0 警、`sync-samples --check` 零漂移、BuildSettings 跑后零残留。**改动测试/asmdef 必须重跑；纯文档补丁版可豁免 PlayMode（程序集二进制未变）**。RAA 侧 6 个测试文件最密且触碰时优先收敛：`BinderCodeGeneratorTests`、`AesirUpdateServiceTests`、`SourceParsingTests`、`AudioModuleTests`、`EventModuleTests`、`XmlSummaryToolTests`。

- [2026-09-29 11:16:13] [project] **Context / 架构核心语义（定稿，勿回退）**：①`AbstractContext.Initialize()` 先 `Configure()`，注册是否立即初始化**只看 `Initialized`**（集体初始化未完成 → 只登记；已完成 → 立即初始化），`Configure()` 期注册的模块由 `InitializePendingModules`（重复取快照直到无未初始化）在同一轮补齐 —— **不做两阶段外的二次初始化**（曾有 `_initializing` 标志导致重复初始化，已移除）。②`Dispose()` 收尾移入 `finally`，并清 `_instance` 缓存（否则返回僵尸上下文）。③`GenericLocator.GetAll()` 返回调用时刻**快照**；`IContext` 只暴露 `RegisterModel/RegisterService/GetModel/GetService`（`Unregister*`/`GetAll*` 已删）。④`MonoLifecycleProxy` 调用期增删监听是**快照语义**（`_invoking` + `_pendingChanges` 挂起队列，趟末按发生顺序应用）。⑤`MiniEvent` 只有 Invoke 路径零分配，AddListener 有分配（文档已限定）。⑥泛型类静态重置走 `ResetStaticsAssistant`（引擎对泛型类 RIOLM 静默跳过），非泛型单例类内 `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]`。⑦能力墙：`GetModel/GetService` 是 advisory（可经 `self.Context` 绕过），`ExecuteCommand/Query` 是硬墙；`IContextHolder` 显式实现注入；泛型 `IView<T>/IController<T>/IPresenter<T>` 以 DIM 绑定上下文；**VC 适配对声明 `IView, IController<T>`**（View 角色保持非泛型，避免双 DIM 冲突）。⑧六档示例：MVC/MVP × Quick/Standard/Strict（Quick = 具体类直写直读，Standard = 只读暴露 + 写方法，Strict = 接口注册 + Command 写 + Query 读），MVP View 一律纯 MonoBehaviour。

- [2026-09-25 12:37:07] [project] **RAM UI 模块定稿**：`Panel = 默认形态 / Window = 进阶形态`（Window 根自带 Canvas，`AesirBaseWindow.sortingOrder` 字段是排序单一真源，默认 500 恒在四层面板之上）。面板四层 Canvas 由 `UIRoot` 构建，层 Canvas / UICamera / EventSystem 序列化持久化；`UIModule` 全静态门面：`Show<T>(payload, path:)` / `Open<T>(payload, path:)` / `Close/Hide` / `Get<T>`（**非创建式查询**）/ `Prewarm` / `RegisterPrefab`；另有 `Show<T>(string path)` / `Open<T>(string path)` 重载消除"字符串被 payload 吞"的陷阱（`path:` 命名参数仍是推荐写法）。蒙版模式在 `UIModuleConfigSO`（`Assets/Resources/UIModuleConfig/`，编辑器自动创建；解析链 = 注册加载器 → Resources → 内存默认），`MaskMode` 运行时切换重算，点击走 `OnMaskClicked()` 虚方法。键语义三件套：同基类类型重复 Show 报错拒绝、Hide/Get 未命中给 Warning、精确 miss 幂等静默；缺层 fail-fast。**`OnClose` 只在受控销毁路径（Hide + DestroyOnHide）调用，外部销毁只有 `OnDestroy`** —— 解绑必须放 `OnDestroy`。`IUIAssetLoader` 是**同步契约**（Addressables 需预加载后同步返回）。面板克隆递归设 UI 层（与窗口对齐）。Binder 全家桶在 Odin 程序集，窗口感知（基类下拉 / `Window` 后缀 / Canvas 根默认 `AesirBaseWindow`）。设计边界：层级封闭集、不做 per-panel Canvas、不做窗口导航栈。

- [2026-10-15 16:18:01] [project] **RAM Scene 模块定稿**：公开 API 全静态门面（`SceneModule.LoadSceneSingle/LoadSceneAdditive/Unload/ReloadScene/SetActiveScene`，实例侧全私有并有守护用例）；场景事件 `SceneLoadedEvent`/`SceneUnloadedEvent`（MiniEvent<string>）、`LastLoadedScene`/`AddedScenePaths` 均非创建式读取。**协程 / UniTask 双驱动**：核心三处 `#if AESIR_MODULES_UNITASK` 切换（适配程序集 `Runestone.AesirModules.UniTask` 提供 `SceneModuleUniTask` 可 await API），宏由 `AesirUniTaskDefineKeeper` 维护（UPM 包在场时交给 versionDefines）。行为：裸名/完整路径双支持（`ResolveLoadedScene` 按名回退）、Single 并发互斥（同路径幂等忽略 / 异路径拒绝 + `onFailed`）、`sceneUnloaded` 订阅对账外部原生卸载、失活预放置跳过、`SceneModuleConfigSO`（`Assets/Resources/SceneModuleConfig/`）承载 `bootstrapScene` + `progressCap`（默认 0.9，协程/UniTask 共用），**无 Odin 时有原生 `PropertyDrawer` + `CustomEditor` 兜底**。`SceneAssetWrapper`：GUID 锚点自愈、`State/UnsafeReason`、异常族（空引用抛 `EmptySceneAssetWrapperException`）、`IsDangling` 以"GUID 自愈后资产可定位"为准、判等与 `GetHashCode` 都是**路径优先**。

- [2026-09-11 12:18:42] [project] **RAM Events 模块定稿**：`SubscriberPriority` 只有 **4 档**（First/High/Medium/Last；High=Attribute 订阅默认、Medium=Script 默认）。双注册表（AttributeBindings 静态 MethodInfo→表达式树编译委托；DynamicBindings 直接 `Action<T>`）均 internal；`ISubscriberFilter` + `AesirEventArgs.WithFilter/WithFilters` 链式（懒分配、未过滤零开销、**fail-closed**），内建 `WithTag/WithPriority/SameSceneAsEmitter/OnlySelf/InsideCollider2D`；分发期死引用自动清理（编辑器告警，玩家构建静默）；`executionMsLimit` 性能监控默认 0 关闭；绑定键按 Type 缓存（热路径稳态零分配，实测编译委托 ~4ns vs `MethodInfo.Invoke` ~300ns）；重入保护 + `_iterationBuffer` 快照 + InsertionIndex 稳定排序。SO 资产化：`AesirEventArgsSO`（发布者是资产本身）+ `UnityEventOnAesirEvent` 桥接组件 + `SubclassSelector`；`AesirListener` 允许 `AllowMultiple`。事件参数载体继承 `AesirEventArgs`，命名 `XxxEventArgs`。

- [2026-09-11 10:33:51] [project] **RAM Audio 模块定稿**：全静态门面；SFX 用 **N 个独占 AudioSource 轮询**（默认 8，等效池化，每播音调/音量独立；不用 `PlayOneShot`、不每次 Instantiate）；BGM 专用 loop 源 + `_bgmFadeFactor` 独立系数淡变（`unscaledDeltaTime`）；三通道音量乘法链 + PlayerPrefs 持久化（键前缀在 `AudioConfigSO`）；`AudioChannel` 概念已引入。`IsBgmPlaying/CurrentBgm` 非创建式；**音量/静音六个 getter 有意保持创建式**（配置在实例序列化字段上，无实例无法还原）。pitch 钳制 `[0.01, 3]`。设计边界：不做 3D（用 `PlayClipAtPoint`）/ AudioMixer / 每音效 Stop / 播完回调。

- [2026-09-27 10:45:00] [project] **RAM ScriptDocGenerator（SDG，强依赖 Odin）**：`Runtime/ScriptDocGenerator/OdinInspector/` + `Editor/ScriptDocGenerator/OdinInspector/`，经 asmref 汇入 Odin 程序集。无 UI 静态门面 `ScriptDocGeneratorAPI`（`GenerateDocsForType/ForTypes/ForAssembly/ForFolder`，`settings=null` 用 Default、`outputFolder=null` 用项目根 `ScriptDocGenerator/`，全程无弹窗不打开文件，覆盖语义 = 面板多程序集模式）；写入核心 `WriteTypeDocSilently` 面板与 API 共用。默认输出目录是**项目根 `ScriptDocGenerator/`**（.gitignore 已登记），不在 `Assets/` 内。单例（`ScriptDocGeneratorPanelSO` / `DefaultSettings` / `ZensicalSettings` 的 `Instance`）**按域缓存**（`if (_instance)` 利用 fake-null），绘制回调只做空值保护。`ShowIf` 用复合条件属性（单类型/列表各自一个）。`MergeFrontMatterWhenMissing` 守卫重生成时双 Front Matter。Summary 工具语义：**`[Summary]` 特性是权威源**（存在时回写 XML 对齐，无特性则回退 XML 生成）。

- [2026-09-27 11:16:56] [project] **Aesir 更新器（RAA Editor，Assets 安装形态专用）**：`AesirUpdateService`（服务）+ `AesirUpdateController`（**编排唯一真源**，双窗口只做瘦壳：IMGUI 兜底 + Odin 版经 `OdinWindowOpener` 路由）。菜单 `Tools/Aesir/Check for Updates`，**validate 按安装形态显隐**（扫不到 Assets 形态安装就整体隐藏；纯 UPM 由 Package Manager 管辖）。版本检测**三层兜底，首个成功即返回**：①直连 GitHub（Releases API → `releases/latest` 302 探测 → 仓库 raw `update-info.json`，只有 tag 的结果按同 tag 补齐清单，拒绝陈旧清单）②镜像站（ghproxy.net / gh-proxy.com）③CDN 中转（jsDelivr 四域名）。**直连可用时绝不用后两层**。超时：单源 5s（`ProbeLatestTagFromRedirect` 12s）、整轮 30s、下载"总 120s + 连续 30s 无进展"双判据（`TimeoutBudget` 墙钟，`UnityWebRequest.timeout` 只覆盖完全无数据）。**全程 `LockReloadAssemblies`**（实测可推迟编译后重载），收尾顺序铁律 = **先 `UnlockReloadAssemblies` 再 `EndBusy`**（异常路径可用 `reloadLocked` 局部标志配平），配 `SessionState` + `[InitializeOnLoadMethod]` 兜底清理。两入口：单包更新 / 全部更新（= 补全缺失已知包 + 更新过期包）。**已移除自动备份**（回滚指引改为从 Releases 下旧版 unitypackage）；残留清理差集在**导入成功后**执行，两侧清单任一为空即跳过；前缀按包目录名从清单反推。unitypackage 下载走 Release 直链，失败逐线路兜底（jsDelivr 不代理 Release 资产）。

- [2026-10-17 11:11:13] [project] **Getting Started 窗口与 Tools/Aesir 菜单图景**：窗口 `AesirGettingStartedWindow`（IMGUI 兜底）+ `AesirGettingStartedWindowOdin`（Odin 版：页面栈 + 包卡片 + 概览收起/页间滑动），菜单 `Tools/Aesir/Getting Started`（**priority -980**；-1000 会与 Odin 的 `Tools/Odin/Getting Started` 打平导致组序抖动，-980 与 AesirInspector 同值锚定，改一侧需同步另一仓常量）。数据层以各包 `package.json` 的 samples 清单为**唯一真源**；包发现覆盖 Assets 锚点根 / 嵌入式包 / UPM（未导入示例保留条目，窗口内一键导入：`Sample.FindByPackage` + 确认框 + `Sample.Import()` 后重扫重绑，`Import()` 返回 bool）。**priority 图景**：Getting Started(-980) → Architecture 组(995) → Modules 组(999) → Export Packages(1000/1001/1002) → Check for Updates(1100 置底)；父菜单组级 priority 取子项最小值，差 >10 自动插分割线；新增 RAA 包工具进 `Tools/Aesir/Architecture/`、示例进 `Architecture/Samples/`、RAM 工具进 `Tools/Aesir/Modules/`；`GameObject/Aesir/` 与 `Assets/Create` 的前缀差异是既有现状，不动。

- [2026-09-24 21:40:19] [project] **安装位置锚点机制（照 Odin `OdinPathLookup.asset` 复刻）**：每包包根放空壳 `AesirPathLookup.asset`（类型在 RAA Editor 程序集；锚点 GUID 写死进 `AesirAssetPaths` 常量），静态构造三级定位（默认根 `Directory.Exists` → 锚点 GUID 查询 O(1) → `FindAssets("t:AesirPathLookup")` 兜底），产出 `InstallRoots` / `PrimaryInstallRoot`，域重载自动重解析。**仅覆盖 Assets 安装形态**（UPM 路径固定无需锚点；unitypackage 导入必装回 `Assets/Runestone`，文档已声明）。消费端三处：构建剔除注入根、更新器多根扫描（跨根同包去重）、Getting Started 包根拼接。新增包接入 = 包根放资产 + 常量加 GUID，消费端零改动。团结引擎内新建的 .meta 是 Base64 GUID（与 Unity 32 hex 不同），但 Unity 工程导入团结属**兼容方向**（保留 32 hex），锚点第二级仍命中，第三级类型搜索兜底。

- [2026-09-27 20:49:04] [project] **UPM 事实定案（实测）**：①**UPM 不支持包内 `package.json` 的 Git URL 依赖**（只有项目 manifest 可声明），保留该声明会让单装本包直接失败（`Version 'https://...' is invalid. Expected a 'SemVer' compatible value`）→ 两包 package.json **均无 dependencies**（TF 硬依赖也已移除）；RAA 缺席由 `Tools/Aesir/Modules/Install Dependencies`（零引用 Bootstrap 程序集，`Client.Add` 常驻 `latest` 分支 URL）补装。②**双直装可行**：manifest 同时声明两包时，同名直接依赖遮蔽包内间接依赖条目。③**UPM 包内测试程序集在消费者项目默认不编译、不出现在 Test Runner**（gating 变量 = 程序集位于 UPM 包内，与官方包同款）；opt-in 入口是 Project Settings → Package Manager → Testables（GUI 未实测）。④C# asmdef 全用**名字引用**，故 Assets RAM + UPM RAA 混合形态可编译。⑤两包 package.json 有 `documentationUrl` / `changelogUrl`（指向文档站，无版本号故不受发版影响）。⑥UPM 升级无提示（PM 不对 Git URL 包显示更新），升级 = 移除后重新添加 / 改 manifest 分支名。

- [2026-09-06 16:14:34] [project] **Unity/引擎实测坑位合集（Unity 2022.3.62f3c1 中国版）**：①**两个 Trigger Collider2D 之间必须至少一个 Kinematic Rigidbody2D** 才能触发 `OnTriggerEnter2D`。②MonoBehaviour 用自动属性会残留序列化值（见命名规范条目）。③`TextureImporter` 的切片 API 在本引擎是 `spritesheet`（`SpriteMetaData[]`）与 `spritePixelsPerUnit`，不是标准 Unity 文档的 `sprites`/`spritePixelsToUnit`。④`PlayerSettings.SetScriptingDefineSymbols` 在"值未变化"时**不落盘**，且变更值的磁盘写入也是**延迟的**（退出编辑器才落）；写宏后必须 grep `ProjectSettings.asset` 验证。⑤`BuildPlayerWindow.DefaultBuildMethods.BuildPlayer` 返回 **void**；`RegisterBuildPlayerHandler` 是覆盖式注册（域重载清空，每次 `[InitializeOnLoadMethod]` 重注册安全）。⑥`ScriptableSingleton.instance` 小写（Unity 原生）。⑦`PlayerLoop` 注入：`EnsureInjected()` + `Register` 期自动检测；第三方 SDK 改 PlayerLoop 后需手动再调。⑧`SetScriptingDefineSymbols` 对废弃 BuildTargetGroup（WebPlayer）抛异常，枚举清理需 try/catch。⑨编辑器进程名可能是 `Tuanjie`（Unity 中国版应用名），`osascript activate "Unity"` 静默无效。

- [2026-09-06 12:21:57] [project] **Codely Bridge / 工具链坑位**：①`unity_editor.refresh` 在编译域重载前会 `SaveScene` 当前场景（会把未清理的测试对象写盘）→ refresh 前确保临时对象已删；无编译发生时 refresh 不保存，需显式 `SaveScene`。②`exec_editor_script` 脚本上下文**不含 `UNITY_EDITOR` 宏**（调用 `[Conditional("UNITY_EDITOR")]` 方法会被整体剔除 → 断言全失败且像是被测代码 bug），只能用反射或内联等价逻辑。③脚本上下文的 UnityEngine API 面比真实引擎窄（`Material.GetPropertyCount/GetPropertyType`、`TMP_TextInfo.glyphCount` 等都不可用）→ 读材质用 `GetFloat/GetColor/GetTexture/HasProperty`，改属性走 `SerializedObject` 或直接改 YAML 后 refresh。④脚本里引用**本会话新编译的 asmdef 类型**会 CS0246 → `AppDomain.CurrentDomain.GetAssemblies()` 解析 Type + 反射/`AddComponent(Type)`。⑤`Bridge` 的 refresh 偶发打印 `currentFileSystemTime.ticks` Assert（来自 Bridge 自身 RefreshJob），与项目代码无关。⑥Codely 应用轮换 Bridge 版本文件夹后若域仍持旧程序集，refresh 会报 TJGenerators 大量 CS0246 → 反射调 `UnityEditor.PackageManager.Client.Resolve()`（脚本响应 timeout 正是域重载生效的证据），再 `unity_refresh` 重连。⑦长驻进程（预览服务器等）必须 `run_in_background`（`nohup ... &` 会随 shell 退出被清理）。

- [2026-10-17 13:42:06] [project] **Batchmode 最小临时项目（E2E）结构与坑**：`-projectPath` 必须用**真实路径**（macOS `/tmp` 是符号链接，会 `Couldn't set project path to: (空)`）；必须有**空 `Assets/` 目录**与完整两行 `ProjectSettings/ProjectVersion.txt`（`m_EditorVersionWithRevision`）。RAM 相关项目还要补 `com.unity.ugui` / `com.unity.modules.audio` / `com.unity.modules.physics2d`，否则出现环境性 CS 假错误。batchmode 有编译错误时 `-executeMethod` 被 "Aborting batchmode due to failure" 阻断；`-runTests` 与 `-quit` 互斥；`-logFile` 勿放项目 `Temp/`（干净退出会整目录删除）；同项目不可双开。无编辑器会话时可用 `~/.codely-cli/tmp/bin/dotnet` build 仓库根的 csproj 做离线编译验证（需 python 剔除 csproj 里已删源文件的 `Compile Include` 行；引用程序集缺 `MemoryExtensions` 会导致 dotnet 假绿，**Unity batchmode 才是真值**）。

- [2026-08-24 20:44:35] [project] **跨平台中文动态字体方案（用户要求）**：不使用 Mac 专有字体、不引入全局 TMP 字体资产；运行时 `Font.GetOSInstalledFontNames()` 探测候选 + `Font.CreateDynamicFontFromOSFont()`。候选优先级：Microsoft YaHei（Windows）→ PingFang SC（macOS）→ Noto Sans CJK SC（Linux）→ WenQuanYi Zen Hei；全不可用回退 `Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")`。示例里需要中文 uGUI Text 一律走 `SampleUIFonts` 之类组件；需要 TMP 中文才走 `TMPChineseFont` skill（需用户授权全局回退）。

- [2026-09-21 19:41:29] [project] **可观察集合与上游关系**：只留 List/Dictionary/HashSet（Queue 已删）；就地升级（同名同命名空间）、类型名与上游 1:1、命名空间 `Runestone.AesirArchitecture` 隔离、不加 `where T : notnull`；单轨 `AddListener/RemoveListener`（`MiniEvent<CollectionChangedEventArgs<T>>` 承载，返回 `AutoRemoveListenerHandle` 可绑 Unity 生命周期）；语义：无变更不通知、批量逐项通知、字典值更新为 Replace（旧值在 `OldItem`）、Move 单事件、Sort/Reverse/Clear 统一 Reset（List `<2` 元素重排不通知，但 `Clear` 判空后必须直发 Reset，不可复用该守卫）、写后通知 fail-fast；Odin 内联调试面板 + `GetChangedListenerCount`。**与上游 Cysharp.ObservableCollections 可共存**（7 个同名公开类型被命名空间隔离；唯一风险是同文件双 using 裸引用同名类型的 CS0104，用别名解决）。**移植坑（C# 12 → C# 9）**：file-scoped namespace、`record struct`、主构造器、集合表达式 `[a,b]` 全部要降级；Unity netstandard2.1 无 `Unsafe`/`CollectionsMarshal`（零拷贝需降级为逐项/物化）；`Shims/Collections.cs` 的 BCL 扩展要收进本项目命名空间；用脚本去重 using 时**只匹配 using 指令**（曾误删 3 行 `using var`）。上游仓库 `https://github.com/Cysharp/ObservableCollections`（MIT）。

### Reference

- [2026-11-08 02:40:00] [reference] **私有仓 `Docs/`（Aesir-Docs，gitignored，未随主仓分发）**：`Docs/AesirFramework/单元测试编写规范.md`（测试规范全文）、`Docs/AesirFramework/全仓锐评/`（历史审查报告 01–10，均已处置完毕，仅作追溯）、`Docs/AesirArchitecture/设计变更记录.md`（包内同名文件的来源）、`Docs/AesirModules/{EventModule,UIModule,SceneModule}/`（模块锐评与路线图）、`Docs/RAA-Course/`（视频课程规划）、`Docs/GameJam/AI-Native-GameJam-架构规范.md`（24h Jam 规范：Jam 档定义、UI 用编辑器脚本生成预制体、4 个缺口补件）。提交风格 `docs:` 中文单行主题。

- [2026-11-08 02:40:00] [reference] **公开文档站 `AesirFramework-Docs/`**（与 `Assets` 同级，被主仓 .gitignore 排除）：线上 `https://yuumixcode.github.io/AesirFramework-Docs/`；本地构建 `~/.local/bin/zensical build --strict`（pipx 安装；CI `deploy.yml` 锁定版本，以 `--version` 实际值为准）。Scripting API 页由 SDG 的 Zensical 生成器产出（RAA `docs/architecture/scripting-api/`、RAM `docs/modules/scripting-api/`），索引/导航重建脚本归档于 `.codely-cli/memory/docs-site-rebuild-api-nav.py`；**重生成流程** = 清本地 `ScriptDocGenerator/Runestone` → `exec_editor_script` 纯反射调 `ScriptDocGeneratorUtility.AnalyzeMultipleAssemblies(List<string>)` + `GenerateMultipleTypeDocs(...)` → `rsync -a --delete` 到站点 → 重跑索引/nav 脚本 → build --strict。

- [2026-11-08 02:40:00] [reference] **本机技能与脚本**：`.codely-cli/skills/`（gitignored，仅本机）：`sync-samples`（`scripts/sync_samples.py --check/--dry-run`）、`aesir-framework` / `aesir-boot-scene`（**权威源在包内 `Documentation/` 的 AesirFramework-Skill / AesirBootScene-Skill**，改包内后需重新拷贝本机壳）、`code-review-unity`、`odin-inspector-decompile` / `odin-inspector-plugin-dev` / `odin-validator`、`unity-editor-scripting` 等。用户级技能权威目录 `~/.codely-cli/skills/`（`~/.agents/skills/` 优先级更高会遮蔽同名副本，新增/移动后必须删旧副本）。归档：`.codely-cli/memory/archive/`（本项目记忆整理前全文、Cowork 会话清理手册 `cowork-session-cleanup.md` 等在 `.codely-cli/memory/`）。

- [2026-09-05 16:20:30] [reference] **发布产物与 CI 脚本**：`.github/workflows/auto-release.yml`（推送 main → 发布 `v<版本>` Release，三 unitypackage）与 `auto-publish-branches.yml`（subtree split 滚动 `*-latest` 分支 + 清理废弃版本分支）；导出脚本 `.github/scripts/build_unitypackage.py`（纯 Python 生成 unitypackage，**禁止**改用第三方 CLI —— 实测会产出重复 GUID / 全零 meta / 丢文件夹条目）。unitypackage 语义要点：跳过 `~` 后缀与 `.` 开头目录（`Samples~` / `Documentation~` 不入包，且 .meta 与主位同 GUID）、排除 IncludeDependencies 拖入的内置包源、**无 Odin 铁律**（`ODIN_INSPECTOR` defineConstraints 原样保留，包内零 Sirenix 引用）。CI 发版后回写 `update-info.json` 供更新器直连 raw 检测。
