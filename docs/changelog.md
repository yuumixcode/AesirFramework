# 更新日志

本页为站点摘要视图。完整历史(含 0.14.0 之前版本与各子包独立变更)见主仓库:

- [根 CHANGELOG.md](https://github.com/yuumixcode/AesirFramework/blob/main/CHANGELOG.md)(monorepo 聚合视图)
- [GitHub Releases](https://github.com/yuumixcode/AesirFramework/releases)(unitypackage 下载)

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/),版本号遵循[语义化版本](https://semver.org/lang/zh-CN/)。

## 当前版本

| 子包 | 包名 | 版本 |
|------|------|------|
| Aesir Architecture | `cn.runestone.aesir.architecture` | **0.35.0** |
| Aesir Modules | `cn.runestone.aesir.modules` | **0.35.0** |

!!! tip "版本策略"
    两包同号发版(CI 校验一致),推荐同版本安装。Aesir Modules 依赖 Aesir Architecture;Aesir Architecture 不依赖任何 Aesir 子包。

## [0.35.0] - 2026-11-23

**事件模块大版本(含破坏性变更)**:补齐"能拦、能看、能测"三块可观测性能力,分发热路径做到零排序、零间接层、零冗余解析;移除 `[AesirListener]` 特性反射订阅,只保留 Script 订阅。

**Aesir Modules**

- **取消传播**:`AesirEventArgs.Cancel(canceller)` 中止本次分发,`First`/`Last` 两档始终执行(前置校验/收尾清理),`NonCancellable()` 免疫取消;用于输入吞噬、伤害减免链、互斥 UI 抢先响应
- **公开诊断 API `AesirEventDiagnostics`**:订阅树查询 + 环形缓冲链路快照,运行时零 Odin 依赖、关闭时分发热路径零分配;未装 Odin 也能据此自建监视面板
- **三个调试窗口**:`Tools/Aesir/Modules/Event Monitor | Event Log | Event Tester`(UI Toolkit 版 + Odin 版信息等价,菜单自动路由)
- **分发性能与结构精简(功能面不变)**:注册表改 Type 键 + 注册期有序插入(分发零排序)、去 `object[]` 间接层、非泛型订阅去反射、重入缓冲按深度复用、空间过滤器每趟缓存
- **【破坏性】移除 `[AesirListener]` 特性反射订阅**:`AddListener<T>(this, callback)` + 句柄 `Dispose()` 退订;新增 `RemoveListener<T>(object)` 精确无句柄退订;迁移表见包内 `Documentation~/event-module.md`
- **【破坏性】同一参数实例重入发布改 fail-fast 拒绝**:跨实例事件链不受影响;事件链请用新参数实例或经 `AesirTimer.NextFrame` 延后发布
- **【破坏性】低频公开 API 收窄**:删除 `Clone()`/`ICloneable`、字符串绑定键;`BindingInfo` 转 internal
- **SDG 修复**:开放泛型成员默认值读取不再崩溃、不再阻断整程序集文档生成
- **新增示例**:`04_Cancellation`(取消传播四档执行链)/ `05_Diagnostics`(纯公开 API 消费诊断数据)

**Aesir Architecture**

- 代码格式化与 XML 注释排版统一(无运行时行为变更),版本号同步至 0.35.0

## [0.34.1] - 2026-11-20

**测试基础设施修复(仓库级)**:补回测试程序集的 Odin 预编译引用,修正 SDG 测试夹具路径。无运行时代码变更,门禁数字与 0.34.0 一致。

**Aesir Architecture**

- **恢复 PlayMode 测试程序集的 Odin 预编译引用**:`fdc5724b` 误删后,测试一碰 `AesirMonoBehaviour` 基类链即 `CS0012`;补回 `Sirenix.*.dll` 引用

**Aesir Modules**

- **恢复三个测试程序集的 Odin 预编译引用**:`Tests.Editor` / `Tests` / `Tests.UniTask` 同上修复
- **`ScriptDocGeneratorApiTests` 临时夹具移出包内只读目录**:改在可写 `Assets/` 下创建,规避 UPM immutable folder 拿不到 `.meta` 的问题

## [0.34.0] - 2026-11-19

**编辑器工具去 Odin 化 + AesirTimer 分层重构 + 配置资产显式创建(仓库级)**:RAA/RAM 运行时代码一律不依赖 Odin Inspector(有/无 Odin 编译均 0 error),编辑器窗口与 Drawer 改用 UI Toolkit,Odin 降级为纯样式开关。

**仓库级变更**

- **编辑器工具去 Odin 化 + UI Toolkit 复刻**:Getting Started / Timer Debugger / Update / Scene Settings 四窗口与三个 Drawer 改 UI Toolkit;新增 `Preferences → Aesir` 偏好页 + Editor 专属样式 SO(不进构建),Odin 降级为纯样式开关;SceneAssetWrapper 绘制改 UI Toolkit 优先 + IMGUI 兜底、零 Odin 依赖
- **AI 扩展 `AesirFrameworkGame` 1.2.0 → 1.3.0**:新增 `aesir-project-structure` 技能(12 技能),统一资产命名去类型前缀与工程目录约定;按 PlaneWar 三篇复盘优化 boot-scene / framework / ui / router 技能
- **移除 Binder / SDG 的 UI Toolkit 窗口**:两个工具只保留原 Odin 窗口,数据层零 Odin 依赖不变

**Aesir Architecture**

- **`AesirTimer` 分层重构(无破坏性)**:拆为 `AbstractTimer` 抽象基类 + 六种具体计时器(`Countdown` / `NextFrame` / `Repeating` / `Interval` / `Frequency` / `Stopwatch`),既有公开 API 零改动;示例补三种新形态,新增 9 例 EditMode 用例

**Aesir Modules**

- **模块配置资产改为显式创建(行为变更)**:移除导入即自动创建,改菜单 `Tools/Aesir/Modules/Create Module Config Assets` + 运行时内存兜底,不再写用户工程
- **UI 实例化简化**:移除停用模板机制改 `Instantiate → SetActive(false)`,实例名去重复 `(Clone)`;补 `Resources.Load` 资产语义实证与决策复盘文档

## [0.33.0] - 2026-11-16

**分发形态与文档结构收敛(仓库级) + AesirTimer 计时器原语 + 世界空间 UI Canvas**:PlaneWar 迁出包外成为宿主工程完整游戏案例;两包 `Documentation/` 取消、英文说明升到包根;AI 技能统一为仓库根整包扩展 `AesirFrameworkGame`;CI 主位迁至 CNB。

**仓库级变更**

- **PlaneWar 迁出包外**:从 `Samples/PlaneWar` 迁至仓库根 `Assets/PlaneWar/`,按实际游戏项目分层,场景改名 `PlaneWar.unity`;类名去 `Sample` 前缀、命名空间改 `PlaneWar`、程序集改 `PlaneWar.Mono`、移除整文件 `#if UNITY_EDITOR` 包裹——正常编译进玩家构建。**破坏性**:`package.json` 的 samples 清单少一项,可导入示例回到 11 个;已装旧版残留由更新器差集在下次更新时清理
- **包内 `Documentation/` 取消**:英文说明升到包根 `README_EN.md`,使用文档只留 `Documentation~/`;`Documentation~/README_EN.md` 删除。代价:unitypackage 用户不再随包拿到使用文档(读本站或克隆仓库)
- **AI 技能统一到整包扩展 `AesirFrameworkGame`**:两个包各自的技能全部并入仓库根 `.codely-cli/extensions/AesirFrameworkGame/`(11 技能 + 2 模板),包内 `TuanjieCowork~/` 只留一份下载与安装说明;模块技能顶部补「依赖门禁」三条硬规则(探测 → 读记忆 → 询问安装,禁止用原生 API 顶替)
- **CI 主位迁 CNB**:`.cnb.yml` 接管包分支 split、`update-info.json` 回写、滚动 tag 与 git-sync 推 GitHub;GitHub 侧只保留 tag → Release
- **`.codely-cli` 入库策略重订**:自研扩展全量入库,其余扩展按 `RESTORE.md` 一条命令重装(新克隆须先装 TJGenerators 扩展再打开工程)

### Aesir Architecture

**Added**

- **`AesirTimer` 计时器原语**:框架驱动的倒计时器,调用即开始、到期自动出队,让纯 C# 的 Model / Service / Command 不借 MonoBehaviour、不借协程即可延时与做冷却进度。每个计时器是独立实例,注册表只保存存活实例用于每帧推进——既可持引用读 `Progress` 喂 UI,也可创建后丢弃(到期自动出队,不泄漏)。注入复用既有 `AesirPlayerLoop`(`BeforeUpdate` + `order: int.MinValue`),继承注入自愈;结算期与执行期两阶段分离,回调内新建 / 取消无需挂起队列;单趟扫描 + 就地压实,O(n) 零分配,同帧到期按创建顺序回调。另提供 `NextFrame(...)` 与 `unscaled` 选项。**有意不做**:`Start`、自带 `Tick` 的不自驱形态、正计时秒表、对象池
- **`AesirTimer.Every` / `Pause` / `Resume` / `RemainingExecutions`**:`Every(interval, onTick, repeatCount = -1, unscaled = false)` 首次回调在 `interval` 之后,有限次数用尽自动出队;一帧卡顿跨越多个间隔只回调一次(重新装填用 `= duration` 而非 `+=`);`Pause` / `Resume` 只挂起单实例,`Resume` 对 `NextFrame` 重新对齐结算周期号;`Restart` 复位剩余时间、执行计数与暂停态
- **计时器调试面板 `Tools → Aesir → Architecture → Timer Debugger`**:Odin 版 + IMGUI 兜底版,priority 996。拉取与重绘分离(按时间 0.05–10 秒 / 按帧 1–600,`EditorPrefs` 持久化),显示状态 / 形态 / 时间源 / 剩余 / 进度 / 已执行 / 还剩次数 / 存活时长 / 回调归属与累计统计;编辑模式无数据源,统计与表格恒为零
- **计时器字段检视**:原生 `[CustomPropertyDrawer]`(需 `[SerializeReference]`)与 Odin `OdinValueDrawer`,共用文案与进度条样式;绘制器取值走 `fieldInfo` 反射而非 `SerializedProperty.managedReferenceValue`
- **`AesirTimer` 示例**:延时 / 冷却 / 有限重复进入 Play 自动开始,其余动作经自定义 Inspector 按钮触发,面板只读 `Remaining` / `Progress` / `RemainingExecutions`(查询式计时示范);另含不继承 MonoBehaviour 的纯 C# 冷却模型 `SkillCooldown`
- **计时器测试**:EditMode 15 例(到期、保序、回调内新建、取消互斥、边界、`Restart`、`NextFrame`、`unscaled`、重复与暂停)+ PlayMode 3 例(真实 PlayerLoop 跨帧、`NextFrame`、`timeScale = 0`)

**Changed**

- **计时器面板数据源与样式解耦**:拆为 `AesirTimerSnapshot` / `AesirTimerStatistics`(只搬数据)→ `AesirTimerDebugSource`(抓取节奏与汇总)→ `AesirTimerText`(文案与颜色)→ `AesirTimerSummaryView`(进度条与摘要);IMGUI 窗口、Odin 窗口与两种绘制器统一消费快照
- **AI 技能迁出包外**:六篇架构技能并入仓库根扩展 `AesirFrameworkGame`,包内只留导流文档;`README` / `README_EN` 结构树与模式决策链接改指扩展内文件

**Removed**

- **PlaneWar 示例移出包外(破坏性)**见上方仓库级变更
- **`references/jam-playbook.md`(jam 实战手册)**:核心缺口(计时器)已由 `AesirTimer` 在框架内解决,继续保留"手抄一份补件"与「同一需求只留一条路径」冲突;工程目录约定与自检要点已并入 `aesir-framework` 技能

**Fixed**

- **Getting Started「打开场景」强制另存临时场景**:`OpenSampleScene` 原用 `SaveOpenScenes()` 一次保存全部打开场景,对无资产路径的临时场景只能弹出另存为面板;改为 `SaveModifiedScenes()`——有路径的静默落盘,无路径的跳过并走标准询问(不保存 / 取消 / 保存),取消才中止切换

### Aesir Modules

**Added**

- **世界空间 UI Canvas 预设(`WorldSpaceUICanvas`)**:`RenderMode.WorldSpace` 预设,自动接线 Canvas / RectTransform(square) / CanvasScaler(ConstantPixelSize) / GraphicRaycaster,子树归入 UI 层(5),默认 sortingOrder 450;支持跟随父级或显式 `FollowTarget` + `PositionOffset`,以及 `FaceCamera` / `Billboard` / `FlipFacing` 朝向控制。世界空间 UI 是 Panel / Window 之外的第三种形态,**不进 UIModule 注册表与蒙版调度**,生命周期随宿主场景物体;配置走独立链(组件 → `UIRoot.WorldSpaceCanvasConfig` → `UICanvasConfigSO.CreateWorldSpaceDefault()`)
- **世界空间 Canvas 守护用例**:`Tests/Editor/UI/WorldSpaceUICanvasTests.cs` 7 例(默认配置、接线、跳过屏幕 Scaler、sortingOrder / UI 层、跟随目标与偏移、运行时创建、空目标防呆)

**Changed**

- **AI 技能统一到扩展 + 依赖门禁**:四个模块技能并入 `AesirFrameworkGame`,包 `README` / `README_EN` 的「AI 技能包」小节改写;缺 RAM / RAA 时 AI 先探测、再读记忆、未明确禁止才询问安装
- **`Documentation/` 取消**:五篇模块使用文档只保留在 `Documentation~/`

---

## [0.32.0] - 2026-10-17

**Aesir Architecture(文档) + Aesir Modules(功能修复)**:RAA 包内 AI 文档整合为 AesirFramework-Skill 并新增 AesirBootScene-Skill;RAM 落地 Scene / UI / Wrapper 全量核查修复批次。

### Aesir Architecture

**Added**

- **AI Skill 文档集整合(AesirFramework-Skill)**:新建全框架 AI 编码指南(`Documentation/AesirFramework-Skill/`),整合收编项目全部 AI 相关文档——原 AesirArchitecture-Skill 六档模板(MVC/MVP × Quick/Standard/Strict)与 core-api 速查、常见陷阱清单(10 条)、事件机制决策表、Jam 实战手册(原私有规范文档首次随包分发)与新增 RAM 四模块 API 速查(UI/Scene/Event/Audio)。**档位口径按最新裁决修订:默认按最低档(Quick)执行**——具体类注册、可写 `ObservableValue` 直接暴露、不建 Model/Service 接口、不做只读 property 包装、不用 Command/Query;升档为加法
- **AI 启动场景 Skill(AesirBootScene-Skill)**:一键创建启动场景(主菜单场景)+ 启动脚本——空场景 + 唯一 `GameBootstrap` 物体(挂启动脚本 + AudioListener,无 Main Camera),登记 Build Settings index 0

**Removed**

- **AesirArchitecture-Skill / 常见陷阱清单.md / 事件机制决策表.md(独立文档收编)**:内容已全部收编进 AesirFramework-Skill 的 SKILL.md 与 references/;独立副本删除,README 文档地图同步改指新 skill
- **产品文档精简**:`Documentation/` 收敛为纯产品文档三类(英文 README / 使用文档 / AI Skill);内部审查报告与演进记录迁至开发者私有文档仓,不再随 unitypackage 分发

### Aesir Modules

**Added**

- **无 Odin 环境的 Scene/配置 Inspector 兜底**:新增 `SceneAssetWrapperDrawer`(原生 PropertyDrawer)与 `SceneModuleConfigSOEditor`(原生 CustomEditor),均 `#if !ODIN_INSPECTOR` 包裹(Odin 在场时整体剔除、面板效果仍由 Odin Processor 提供)——未装 Odin 时 `SceneAssetWrapper` 可拖拽赋值、看状态行并使用"添加到 BuildSettings / 启用"一键修复;`SceneModuleConfigSO` 的启动场景与进度上限可在 Project 窗口直接编辑
- **`Show<T>(string path)` / `Open<T>(string path)` 重载**:位置参数直接传资源路径,消除字符串被 `object payload` 形参吞掉、随后报"未注册预制体且未提供 path"且不揭示真实原因的陷阱
- **外部场景卸载自动对账**:SceneModule 订阅 `SceneManager.sceneUnloaded`,经原生 API 卸载掉叠加追踪中的场景时自动移出记账并告警;宿主失活/销毁时配平批量卸载的重入深度

**Changed**

- **查询类静态 API 不再创建模块实例(行为变更)**:`UIModule.Get<T>()` / `ContainPrefab<T>()` / `GetWindow<T>()` / `ContainWindowPrefab<T>()` 改经 `TryGetExisting` 非创建式取值——无实例时返回 `null` / `false`,不再凭空创建 `[Aesir Modules]` 宿主树
- **Single 加载并发互斥(行为变更)**:进行中的 Single 期间收到新 Single 请求,同路径幂等忽略(告警、不触发回调)、异路径拒绝并触发 `onFailed`
- **裸场景名与完整路径双支持**:`LoadSceneSingle` / `LoadSceneAdditive` 记账按路径查找失败后按名回退;推荐完整资产路径(BuildSettings 粒度即路径、无重名歧义)
- **面板克隆子树递归归入 UI 层**:对齐窗口侧 `SetLayerRecursively`——GameObject.layer 不随父级继承,预制体内非 UGUI 渲染器(如 SpriteRenderer)不再因 layer 0 被 UICamera 的 cullingMask 挡掉
- **引用注册后带 path 调用给出诊断**:经 `RegisterPrefab` / `RegisterWindowPrefab` 注册的条目再带 `path` 调用 Show/Open 时输出"path 被忽略"警告;加载器初始化收敛为惰性;加载失败文案按契约分派(不再一律写"面板")

**Fixed**

- **失活预放置的 SceneModule 实例不再被采纳**:预放置模块所在 GameObject 失活时跳过(失活物体无法启动协程,采纳会让首次加载直接抛 UnityException),改为运行时新建并告警;UI 示例场景(SampleForUIBasic)主相机 cullingMask 排除 UI 层与 TransparentFX(此前 Everything 与自家"主相机须排除 UI 层"规则相悖,照抄示例即 UI 双渲染)

## [0.31.2] - 2026-10-06

**补丁版本**:RAA 侧编译告警清理,无功能变更。

- **`IGenericLocator<T>` 删除冗余的 `Dispose()` 声明**:接口本已继承 `IDisposable`,另行声明同名同签名的成员只是隐藏继承成员(编译告警 `CS0108`),并非新增契约。删除后接口契约完全不变,实现类与调用点零改动;原声明上承载的语义文档(清空仅解除注册关系、不销毁被注册的实例;`AbstractContext<T>.Dispose` 只持有接口抽象、需经接口而非具体实现清空)上移到接口级备注。本站该 API 页同步(方法摘要表 / 成员详情段 / 类级备注)
- Aesir Modules 源码与 0.31.1 一致,本次仅随 RAA 的告警修复同步版本号

## [0.31.1] - 2026-09-29

**文档补丁版本**:两包源码与 0.31.0 一致,无功能变更;本轮全部内容落在本站与仓库记忆:

- **Scripting API 页全量重生成**至 0.31.0 口径(RAA 131 页 / RAM 153 页,9 个程序集 284 类型):新增 `IView<T>`、`AesirPlayerLoop` 家族、`AudioChannel`、`SceneModuleUniTask`、`NiceTypeName`、`SceneModuleSettingsWindowOdin` 等页,移除 `Internal/ListExtensions`、`Editor/SceneManagerWindow`
- **修复侧栏 API 导航缺陷**:导航路径前缀重复(`architecture/scripting-api/architecture/scripting-api/…`)导致全部 API 链接 404;修复后 284 条导航目标全部有效且与页面一一对应
- **内容页对齐 0.31.0 共 27 处**:示例总数 9/10 → 11 与 `RuntimeInitializeLoadType` 登记状态、已删 `RegisterCustomLifecycle` 用法示例、`AesirPlayerLoop` 按帧节流自愈口径、双注册表收 internal、Binder 编辑器工具链不随 Player 打包、包不再声明测试框架依赖,以及 `IView<T>` / `GetAllEntries` / `ClearListeners` 接口面 / `EventModule` DDOL / Binder 设置持久化 / `Channels` 等新能力补录
- **补第三方素材出处**:PlaneWar 示例使用的 Vertical 2D Shooting BE4(Goldmetal)补上版权与授权口径,与包内 Third Party Notices 对齐

## [0.31.0] - 2026-09-29

**仓库级变更**:三轮审查修复全量落地(复核 / 终审 / 修复优化方案),覆盖初始化语义、程序集边界、静态重置、日志规范、持久化与分发裁剪等 100 余项,含多处破坏性 API 变更(见下)。分发侧 `auto-publish-branches.yml` 在 subtree split 后剔除开发侧 `Samples/` 与 `Documentation/`——UPM 产物只保留 `~` 镜像,消费工程不再无条件多出样例程序集,也不会与 Package Manager 导入的样例撞 GUID;开发仓库接入 UniTask(`com.cysharp.unitask`,UPM Git URL)用于双态验证。

### Aesir Architecture

- **Added**

    - `IView<T>` 泛型表现层接口(默认接口实现绑定 `AbstractContext<T>.Instance` 单例,对齐 `IController<T>` / `IPresenter<T>`):View 适配对(AesirView / MonoView)与 VC 适配对(AesirViewController / MonoViewController)改经 DIM 绑定并删除手写显式实现,VC 实例额外获得 `IController<T>` 可赋值性;`IGenericLocator<T>` 新增 `GetAllEntries()` 诊断成员并继承 `IDisposable`。

- **Changed**

    - **`AesirArchitecturePlayerLoop` → `AesirPlayerLoop`、`AesirArchitectureLifecyclePhase` → `AesirLifecyclePhase`(破坏性改名)** —— 该类型是跨包共用的框架级帧钩子工件(RAA 对外公共 API,RAM 侧零引用、无对照角色),按命名规范"跨包共用 → 直接 `Aesir` + 语义名、不带包名段"精简;宿主 `AesirArchitecture` 与门面 `AesirArchitectureDebug` 与 RAM 的 `AesirModules` / `AesirModulesDebug` 成对,保留全名。迁移:调用点改 `AesirPlayerLoop.Register(AesirLifecyclePhase.BeforeUpdate, …)`。

    - **初始化语义修正(破坏性修复)** —— `Initialize()` 曾在调用 `Configure()` 之前就置位内部标志,使 `Configure()` 里注册的每个模块**当场初始化一次、遍历时再初始化一次**(订阅重复注册、计数/加载类副作用翻倍),且"先全部 Model 后全部 Service"的两阶段顺序被打乱。现改为「集体初始化未完成时注册只登记、完成后注册立即初始化」,遍历期间注册的模块由"重复取快照直到没有未初始化模块"同轮补齐。

    - **`AbstractContext<T>.Dispose()` 由 `virtual` 收为非虚(破坏性)** —— 类上同时存在两个析构扩展点,子类覆写 `Dispose()` 漏调 `base` 会留下"单例缓存不清 + `Initialized` 残留"的僵尸上下文;定制点统一为 `OnDispose()`,收尾段(置 `Initialized = false` + 摘单例缓存)移入 `finally` 保证不变量。

    - AbstractContext 容器字段改声明为 `IGenericLocator<T>` 接口类型(DIP 落地);`GetAll()` 改为调用时刻快照——枚举期间注册/注销不再抛「集合已修改」异常。`ClearListeners()` 补进 `IObservableCollection<T>` / `IReadOnlyObservableValue<T>` 接口面(对象池回收时的退订入口);`RemoveListenerHandleCollection` 收 internal。

    - 更新器窗口刷新回调拆分——`AesirUpdateController` 构造器新增可省的"仅重绘"回调,进度 tick 与状态文本变更不再触发窗口重算列表(此前每次 tick 都走一遍列表重算)。

    - 更新器服务层/编排层职责分离(SRP):确认框文案构建迁入 `AesirUpdateController`,`UpdatePackagesAsync` 导入期收进度条改为回调;已知包登记统一到 `AesirGetStartedService.KnownPackages`(补 DirName 字段,更新器与概览卡片共用,新增公开包只改一处);更新日志拉取阶段套 30 秒整轮预算(此前双包串行最坏约 64 秒不可逃生)。

    - README 中英快速开始改快捷档主线(第一课 3 个脚本跑通数据闭环,严格档移入「进阶」);三档文件数统一按脚本计;结构树补 Documentation 三项与 Getting Started 文件;`[InternalContext]` 首次获得文档说明;AI 编码指南 Quick 档模板改只读属性暴露写法(不再教公开可变字段)。

    - `AesirScheduler` 自愈注释改实话(自愈仅发生在首次注册,被第三方覆盖后需手动 `EnsureInjected()`);ResetStatics 死防御分支删除(两处);三个 Odin AttributeProcessor 收窄 `internal sealed`;`MonoLifecycleProxy.GetListenerCount` 收 internal;`AesirMonoBehaviour` / `AesirScriptableObject` 补 `ODIN_INSPECTOR_EDITOR_ONLY` 数据丢失警告。

- **Fixed**

    - **域重载重置链路无异常隔离 + 先 Dispose 后置空** —— `ResetStaticsAll` 的遍历无 `try/catch`,任一回调抛异常即中断后续全部重置;`AbstractContext<T>` 的回调 `Dispose()` 抛异常时 `_instance` 永不清空。现逐回调 `try/catch`(记警告后继续)且先取 `_instance` 置空再 `Dispose`。

    - **更新器差集清理两处数据安全缺陷** —— ① 对"新清单为空"补守卫(远程 update-info.json 半截 JSON / 缺 files 字段时不再把整包判为残留删光,与上次清单守卫对称);② 差集前缀改按包目录名从清单反推(安装根被移动到其它文件夹时,清理此前静默失效、残留持续累积)。

    - **`ScriptingSymbolEditorUtilityTests` 改写真实宏定义符号导致 Test Runner 中途域重载** —— 每次 `PlayerSettings` 变更都会请求 "Define symbols changed" 全量脚本重编译,编译完成时本轮 EditMode 尚未结束,Test Runner 报 `Unexpected assembly reload happened while running tests` 并丢失上下文;增删语义抽为纯函数,测试改纯逻辑断言(真实写入属手动验证项)。

    - `RemoveListenerOnSceneUnloadedTrigger` 宿主销毁时逐桶执行句柄移除(此前只清桶不 Dispose,其他场景的监听永久残留);`AesirAssetPaths` 静态构造异常护栏(「类型中毒」降级为单次可恢复的路径降级);`ObservableDictionary` / `ObservableHashSet` 补 `[SerializeField]`(失实注释修复后 Inspector 编辑初始元素真实生效);`ObservableHashSet` 批量方法先物化源序列(传集合自身不再中断);`CloneCollection.Contains` 真实现 + 死构造器删除 + 注释中文化;两个集合的「零分配」宣称修正为仅 `Invoke` 路径成立。

- **Removed**

    - **`AesirScheduler` 帧粒度时间调度器整体移除(破坏性)** —— 该原语自 0.21.0 引入后全仓零使用(源码引用仅存在于自身定义与专属测试,示例与 Runtime 无一处调用);需要延时请用 `MonoLifecycleProxy` 帧代理 + 计时字段,或直接把驱动方挂在 GameObject 上用协程。

    - **`ObservableQueue<T>`**(破坏性)——四集合中使用频率最低且唯一无写侧接口;内置家族收敛为三种高频集合(List / Dictionary / HashSet),队列用上游 Cysharp.ObservableCollections。

    - **`MonoLifecycleProxyExtensions` 整类**(破坏性)——6 个公开扩展方法全仓零使用且均为一行转发,`UnregisterCustomLifecycle` 的 receiver 不参与逻辑、暗示错误心智模型;更新器死入口 `FetchLatestReleaseSnapshotAsync` 与 `Internal/ListExtensions.cs` 死文件。

### Aesir Modules

- **Added**

    - **UniTask 驱动链 PlayMode 测试程序集 `Runestone.AesirModules.Tests.UniTask`** —— 覆盖 `SceneModuleUniTask` 全部 8 个公开 API(含入口即取消、加载中取消、宿主销毁不悬挂、进度归一化),`defineConstraints` 双守卫,未装 UniTask 的工程整体不编译。

    - 日志门面补 `ScriptDocGeneratorTag`;测试守护网新增三处(`UIRoot.CreateInputModule` / `AesirEventUtility.KeyCache` 静态重置、重复实例只销毁自身组件——该行为在 EditMode 下不可断言,落在 PlayMode 程序集)。

- **Changed**

    - **测试程序集按「是否依赖 Odin 类型」重新划分** —— 原先编辑器测试程序集带 `ODIN_INSPECTOR` 门控,未装 Odin(或活动平台非 Editor)时整程序集静默不编译、`Tests/Editor/UI/` 的 44 个用例在 Test Runner 中消失且零报错;现分两层:真正依赖 Odin 类型的用例(Binder 4 文件 + ScriptDocGenerator 全模块)迁入门控程序集 `Runestone.AesirModules.Tests.Editor.OdinInspector`,其余留在基线程序集(保留 `Sirenix.*.dll` 预编译引用但移除门控——指向不存在程序集的预编译引用会被 Unity 静默忽略)。

    - **Binder 编辑器工具链移出 Player 构建 + ScriptDocGenerator 运行时代码解除对 Odin 运行时程序集的依赖** —— 3,400+ 行编辑器工具链(BinderAssistant 全程序集扫描、代码生成器、写脚本)不再随 Standalone 打包(纯文本逻辑 + 整文件 `#if UNITY_EDITOR` 两道防线);ScriptDocGenerator 新增 `NiceTypeName` 承接类型格式化(逐条移植 Sirenix 规则,27/27 输出一致)。

    - **日志输出统一收敛到包门面 `AesirModulesDebug`(日志文案变更)** —— 生产路径此前有 50 余处绕开门面的裸 `Debug.Log*`(含无前缀者),覆盖面含事件模块绑定失败告警、ScriptDocGenerator 全模块与 `BootstrapSceneHelper`;`[ScriptDocGeneratorAPI]` 前缀改用门面 source 次前缀保留。**代价是控制台文本前缀形态改变**(彩色加粗主前缀 + 中括号次前缀),文本子串不变。唯一有意例外是 `AesirDependencyInstaller`(其所在程序集必须零引用,补齐依赖期间门面不可用),已在类 remarks 写明。

    - **`EventModule` 双注册表 `AttributeBindings` / `DynamicBindings` 收窄为 internal(破坏性)** —— 二者是实现细节,订阅/退订必须经公开 API 走同一套绑定键与死引用清理;业务代码若直接读过会编译失败。

    - **`BinderEditorSettings` 改为真持久化(行为变更)** —— 此前类上没有 `[FilePath]`、自身 `Save()` 只做 `SetDirty` + `SaveAssets`(对非资产对象无效),值"进程内活、单例重建即回默认";现补 `[FilePath("ScriptableSingleton/AesirModules/BinderEditorSettings.asset", ProjectFolder)]` 并改调基类 `Save(true)`,重启编辑器后后缀列表 / 默认后缀 / 最近命名空间保留。

    - **`UIRoot` 重复实例改 `Destroy(this)`(行为变更)** —— 只销毁本组件,不连带销毁用户物体上的其它组件与已搭好的四层 Canvas 层级;`UIRoot.CreateInputModule` 与 `AesirEventUtility` 绑定键缓存一并纳入域加载期静态重置。

    - `AudioModule` 通道维度收敛为单一数据源 `AudioChannel`(新增通道改动点由约 20 处降到约 4 处,公开成员与 PlayerPrefs 键名逐字不变);样例程序集 `autoReferenced` 归位为 `false`(样例类型不再自动进入用户代码补全)。

- **Fixed**

    - **序列化数据层静默失效** —— `UIRoot` 四层 Canvas 引用表因 `readonly` + 缺 `[SerializeField]` 两个条件同时不满足,永远以空列表进入运行时(层子物体改名即新建重复 Canvas、同 `sortingOrder` 叠加渲染);`SceneEditorSettings` 与 `RuntimeInitializeLoadTypeSettings` 的私有字段同样漏标——类上有 `[FilePath]`,设置文件里却没有键,值只活在当前进程内,重建单例(编辑器重启 / 切项目)即回落默认:场景模块的 Bootstrapper 开关与示例的五个时机开关静默失效。

    - **面板 / 窗口的销毁反清理可被子类屏蔽** —— `OnDestroy` 由 private 改 `protected virtual` 并在 remarks 强制调 `base`,同时 `UIModule` 增加按 Unity 假 null 驱逐已销毁记录的自愈(此前子类写了 `OnDestroy` 就会让注册表残留已销毁实例,之后每次 Show/Open 抛 `MissingReferenceException` 且无自愈路径)。

    - **`EventModule` 预放置路线不受 `DontDestroyOnLoad` 保护**(补序列化字段 + Awake 守卫);**`FindAnyObjectByType` 跳过 inactive 预放置模块**(全模块统一为 `FindObjectsInactive.Include`,`uiCanvasConfigSO` / `bootstrapScene` / `sfxSourceCount` 等 Inspector 配置不再从第一帧起被静默忽略)。

    - **`SubclassSelector` 下拉滤掉全部事件参数子类** —— 过滤用 `IsDefined(SerializableAttribute, inherit: false)` 只看类型自身声明的特性,而 `AesirEventArgs` 已带 `[Serializable]`、派生类无需重复标注,导致 SO 资产化主路径下拉恒为空;改为继承判定。**`RaiseEvent` 重入时共享参数实例的 `Sender` 被内层覆写**——三个内建过滤器全部依赖 `Sender`,会静默错投或漏投;改为分发开始时取局部 sender 并逐绑定重新断言。

    - **`SceneAssetWrapper` 判等与哈希统一主键(行为变更)** —— 原 `Equals` 按 GUID、`GetHashCode` 按路径,同一场景在移动/改名后会 `Equals == true` 而哈希不等,`Dictionary` / `HashSet` 静默保留重复条目、查找漏命中;现路径优先、任一侧无路径时回退 GUID。另:GUID 自愈分支补回写 Addressables 地址(此前静默降级为 `Unsafe` 被拒绝加载)。

    - **`SceneModule` 场景事件改静态持有**(`dontDestroyOnLoad` 取消勾选 + Single 模式加载销毁宿主后,DDOL 常驻系统永久收不到广播);`AddedScenePaths` 改返回快照(此前外泄内部可变列表,遍历期变更抛异常且可强转回写);`UnloadScene` 补"最后一个已加载场景"保护(改按真实场景数统计,排除 `DontDestroyOnLoad` 伪场景)。

    - **更新器重载锁泄漏(整会话无法域重载,只能重启)** —— 收尾重构为「加锁移入 `try` + 局部配平标志 + 顺序改为先解锁再清 Busy」,任何异常路径恰好解锁一次;配套 `SessionState` 标记 + 域加载兜底恢复被强杀流程残留的进度条与重载锁。

    - **`ScriptDocGenerator` 的 PropertyTree 域重载泄漏**(每次域重载 GC 报错)与**绘制期资产库操作**(单例解析收敛到 `OnEnable`、实例按域缓存,消灭绘制回调里的 `CreateAsset` + 全项目重扫);`ScriptDocGeneratorUtility` 双 Front Matter 缺陷(重生成已带 FM 的文件会叠加两份);调试检查模式 `ShowIf` 组合改单一复合条件——Odin 对同一成员的多个 `ShowIf` 是 OR 语义,该缺陷曾让 TypeData 列表无视调试开关常驻窗口。

    - **UniTask 门控测试程序集从未参与编译(8 条用例静默消失)** —— 该程序集自身漏声明 `com.cysharp.unitask` 的 `versionDefines`,而 versionDefines 产生的宏只对声明它的程序集可见,故约束永不满足、程序集被静默排除出编译管线;补一份 versionDefines 并新增两条守卫。PlayMode 套件首次真跑另暴露两处用例缺陷(进度末值断言与生产语义相反、缺场景卫生被同学例遗留的双实例污染,现以"锚场景 + 按 Scene 句柄逐个卸载"根治)。

## [0.30.0] - 2026-09-27

**仓库级变更**:Git 分支策略重构——版本分支(`AesirXxx-v<版本>`,随发版轮换并删除)废弃,改为常驻滚动分支 `AesirArchitecture-latest` / `AesirModules-latest`(CI 每次推送 main 时 subtree split 滚动更新,分支名永久固定):Git URL 一次输入持续可用,升级 = Package Manager 移除后用同一 URL 重新添加;钉旧版本用 Release tag(`?path=Assets/Runestone/<包目录>#v<版本>`,tag 永久保留);CI 自动清理远端残留的版本分支。

### Aesir Architecture

- **Changed**

    - README 安装指引改锚常驻 latest 分支——UPM 安装 URL 由固定版本分支改为 `#AesirArchitecture-latest`,升级口径同步为「移除后用同一 URL 重新添加,无需随发版修改」。

### Aesir Modules

- **Changed**

    - `AesirDependencyInstaller` 补装 URL 常驻分支化——缺 Aesir Architecture 时一键补装的 Git URL 改为固定常量 `#AesirArchitecture-latest`(不再按本包 version 拼接版本分支),移除版本推导函数与 `FallbackSelfVersion` 兜底常量(发版零联动);安装确认框与收尾日志文案同步。

    - README 安装指引改锚常驻 latest 分支(含 manifest.json 示例)。

## [0.29.0] - 2026-09-27

### Aesir Architecture

- **Added**

    - Getting Started 窗口支持 UPM / 嵌入式安装的示例一键导入——未导入示例卡片:整卡点击 Toast 引导导入,右侧按钮由「去导入」(跳转 Package Manager)升级为「导入 Sample」:确认框含示例介绍与导入后位置(可取消),确认后经 Package Manager Sample API(`UnityEditor.PackageManager.UI.Sample`)直接导入到 `Assets/Samples/`,成功自动重扫刷新清单;已导入幂等跳过,清单匹配不到红色 Toast 兜底;IMGUI 兜底与 Odin 版同步。

    - `package.json` 新增 UPM 元数据链接字段(`documentationUrl` 指向文档站 Architecture 分区、`changelogUrl` 指向更新日志页):UPM 安装后 Package Manager 包详情页出现 View documentation / View changelog 链接;README 安装指引补充具体升级操作(Package Manager 不对 Git URL 包显示更新提示,升级 = 移除后重新添加新版本分支的 Git URL 或修改 manifest.json 分支名)。

- **Removed**

    - PlaneWar 场景引用修复菜单(`Tools → Aesir → Architecture → Samples → PlaneWar → Fix Scene References`)——开发期一次性修复工具,示例场景/预制体引用已随资产固化,随 `Runestone.AesirArchitecture.Samples.PlaneWarMono.Editor` 程序集一并移除;`RuntimeInitializeLoadType` 菜单显式 priority 995 接管 Architecture 组排序锚点,`Tools/Aesir` 菜单布局不变。

### Aesir Modules

- **Added**

    - `package.json` 新增 UPM 元数据链接字段(`documentationUrl` 指向文档站 Modules 分区、`changelogUrl` 指向更新日志页):UPM 安装后 Package Manager 包详情页出现 View documentation / View changelog 链接。

## [0.28.0] - 2026-09-27

### Aesir Modules

- **Removed**

    - `package.json` 依赖声明整段移除(破坏性)——UPM 不支持包内 Git URL 依赖(Unity 官方硬规定,保留会使单独安装直接失败),移除后单装可成功,缺 Aesir Architecture 时经 `Install Dependencies` 菜单一键补装;`com.unity.test-framework` 硬依赖一并移除(测试程序集已有 `UNITY_INCLUDE_TESTS` 守卫,非必需)。
    - 测试程序集收敛(破坏性)——每包只保留 `xxx.Tests.Editor`(EditMode)与 `xxx.Tests`(PlayMode)两个测试程序集:`Runestone.AesirModules.Scene.Tests` 并入包级 EditMode 程序集;包级 EditMode 改名 `Runestone.AesirModules.Tests.Editor`;PlayMode 程序集改名 `Runestone.AesirModules.Tests`。

- **Changed**

    - Scene 模块设置双窗口与无 Odin 编译修复——`SceneEditorSettings` 数据层 `#if ODIN_INSPECTOR` 包裹展示特性;Odin 版窗口迁入守卫程序集;新增原生 IMGUI 兜底窗口(OdinWindowOpener 路由,更新器双窗口同款);菜单更名 `Scene Module Settings`;无 Odin 环境 UPM E2E 0 编译错误(修复前 108 个 CS0246)。
    - `Install Dependencies` 补装菜单适用范围扩展——UPM 单独安装本包(缺 Aesir Architecture、核心程序集编译失败)同样触发一键补装;安装教程改为两包分别添加。

### Aesir Architecture

- **Changed**

    - `Tools → Aesir → Check for Updates` 菜单按安装形态显隐——扫描不到 Assets 形态的 Aesir 包安装时(纯 UPM 形态)菜单整体隐藏,Assets 形态安装(含与 UPM 混合并存)时照常显示。

## [0.27.1] - 2026-09-27

---

### Aesir Modules

**Fixed**

- **UniTask 集成的程序集名错误(asmdef 引用名与宏维护器检测名)** — UniTask 的命名空间名 `Cysharp.Threading.Tasks` 被误当作程序集名使用(com.cysharp.unitask 包内 asmdef 实际名为 `UniTask`):①核心与适配程序集的 asmdef `references` 解析不到任何程序集,含 UniTask 的消费工程刷新即报 `CS0246`(`'Cysharp'` / `'UniTaskVoid'` 找不到);②`AesirUniTaskDefineKeeper` 按该名检测域内程序集恒为 false,unitypackage / DLL 安装形态下已装 UniTask 的工程全局宏 `AESIR_MODULES_UNITASK` 反被误删、UniTask 分支与适配程序集静默失效零报错(UPM 安装形态宏由 versionDefines 管理,未受影响——多数工程未察觉的原因)。现 references 改按程序集名 `UniTask` 引用;检测改为白名单(`UniTask`——asmdef 源码 / unitypackage 形态;`Cysharp.Threading.Tasks`——NuGet 预编译 DLL);`AesirUniTaskDefineKeeperTests` 新增命名守卫 2 用例(白名单含真实程序集名 + 两处 asmdef 引用锁定),共 7 用例

Aesir Architecture 本版本无功能变更,随版本配套发布。

## [0.27.0] - 2026-09-27

---

### Aesir Architecture

**Added**

- **更新器单包更新入口** — 包列表每行新增「更新」按钮(IMGUI 与 Odin 窗口同步),另一已知包在场且落后时确认框前置「配套版本警告」,提示但不阻止
- **「全部更新」升级为"补全 + 更新"语义** — 目标扩展为"过期包 + 缺失的已知包补装",确认框逐包标注「更新 / 新安装」并前置缺包说明

**Fixed**

- **「全部更新」在 GitHub 直连不可用时卡死在下载进度条** — unitypackage 下载按「直连 → 镜像站代理(ghproxy.net / gh-proxy.com)」逐线路兜底,全部失败时异常附手动下载指引
- **更新进度条不可取消** — 下载阶段进度条可随时点「取消」中止,取消后温和收尾(区分已完成导入与未更新的包)
- **更新流程收尾异常导致程序集重载锁泄漏** — 收尾重构为配平守卫结构,任何异常路径恰好解锁一次;顺序改为「先解锁、再清忙碌标记」,不再存在死锁窗口
- **`AESIR_ARCHITECTURE` 宏确保器写入时机重入风险(预防性加固)** — 写宏推迟到 `EditorApplication.delayCall`,仅在符号缺失时写入
- **Tools/Aesir 组菜单排序随域重载抖动** — Getting Started 菜单优先级调整,稳定居 Tools/Odin 组之后并自动插独立分割线

**Changed**

- **移除 DDOL 关闭时的运行时 Warning 提醒日志** — 非 DDOL 提示完全由 Inspector 信息框承担
- **DDOL 开关字段前移至类声明首位** — 确立各单例类「DDOL 开关在最上」的统一排布
- **更新确认框文案重构** — 全部更新与单包更新分别构建确认文案;`UpdatePackagesAsync` 返回 `UpdateResult`(已完成包 / 是否取消 / 未更新包)

**Removed**

- **移除更新器更新前自动备份机制(`.aesir-backup/`)** — 全量复制数千文件对消费者 git 仓库构成无谓噪音;回滚走 GitHub Releases 旧版 unitypackage 重新导入(确认框已明示),`BackupRunestone` / `UpdateResult.BackupPath` 等 API 一并移除

---

### Aesir Modules

**Added**

- **Scene 模块 UniTask 适配(新程序集 `Runestone.AesirModules.UniTask`)** — 安装 UniTask 时内部流程自动改为 UniTask 驱动,新增可 await 的 `SceneModuleUniTask` API;宏 `AESIR_MODULES_UNITASK` 由 versionDefines + 编辑器宏维护器自动维护
- **UI 模块全局配置资产 `UIModuleConfigSO`(单例)** — 窗口蒙版模式等模块级配置迁出 `UIModule` 序列化字段,不再要求预放置;加载器注册 → Resources 兜底 → 内存默认实例三级解析,编辑器自动创建兜底资产
- **场景模块全局配置资产 `SceneModuleConfigSO`(单例)** — 承载全局启动场景兜底与加载进度归一化上限,设计对齐 `UIModuleConfigSO`
- **Script Doc Generator 静态 API(`ScriptDocGeneratorAPI`)** — 面板的无 UI 等价入口,供自动化脚本与 AI 直接调用(按类型/程序集/文件夹生成,返回 `ScriptDocGenerationResult`)

**Changed(含破坏性)**

- **`SceneModule` 公开 API 静态门面化(破坏性)** — 全部公开成员改为静态(直接 `SceneModule.LoadSceneSingle(...)`),实例侧全私有(守护用例锁定)
- **窗口蒙版模式配置迁移至 `UIModuleConfigSO`(破坏性)** — 移除 `UIModule` 的 `maskMode` 序列化字段,升级后以配置资产为准
- **加载进度上限配置迁移至 `SceneModuleConfigSO`** — 常量 0.9 改为配置资产字段,消费端钳制 (0, 1]
- **移除各模块 DDOL 关闭时的运行时 Warning 提醒日志;DDOL 开关字段统一前移至类声明首位**
- **Script Doc Generator「调试检查模式」重排至窗口最底部** — TypeData 中间结果列表位于开关下方,提示三态化

**Fixed**

- **Script Doc Generator 调试检查模式关闭时 TypeData 列表仍显示** — 根因是 Odin 对同一成员的多个 ShowIf 为 OR 语义,现合并为复合条件属性(AND)
- **Script Doc Generator 调试模式警告信息框表达式解析错误** — `$value` 误用,改为 `@成员名` 根实例上下文解析
- **Script Doc Generator 窗口 PropertyTree 未释放** — `OnDisable` 释放并置空,消除 GC 警告
- **Script Doc Generator 绘制回调内解析资产库单例导致卡顿与 "GUIStateObj is deleted" 报错** — 单例解析收敛到 `OnEnable` + 按域缓存

---

## [0.26.0] - 2026-09-25

---

### Aesir Architecture

**Added**

- **更新器检测线路可见化** — 检测结果新增线路模型(直连 GitHub / 镜像站 / CDN 中转):窗口显示「GitHub 直连是否可用(可用即版本信息 100% 实时)」与「最终获取线路」,新增「检测详情(各层尝试)」折叠区(含每层耗时与失败原因),结果来自 CDN 中转时额外给出延迟提示

**Changed**

- **更新器版本检测改为「直连 GitHub → 镜像站 → CDN 中转」三层兜底(修复新版本检测延迟)** — 此前 jsDelivr CDN 排在首位,其分支缓存最长约 12 小时,刚发布的版本在窗口里仍会显示为旧版本。现直连层依次尝试 Releases API、`releases/latest` 的 302 探测、仓库内 `update-info.json` 的直连 raw(单源超时 5 秒即落下一层),直连不可用才落镜像站(ghproxy.net / gh-proxy.com,代理 raw 内容、版本实时),最后才是 CDN 中转;只有 tag 的结果会按同 tag 校验补齐文件清单,拒绝陈旧清单避免错删文件

**Fixed**

- **更新/检测缺少超时,进度条可能长时间卡住** — 连接检测与 unitypackage 下载补齐硬性墙钟上限:单源检测 5 秒、整轮检测 30 秒(超时后不再发起新请求并留痕)、下载「总时长 120 秒 + 连续 30 秒无进展」双判据;任一超时都会中止请求并抛明确异常,上层必定收起进度条
- **两个包连续更新时流程可能中途断裂(进度条停留 + 按钮提前可点)** — 导入 unitypackage 带来的脚本变更会触发域重载,异步流程随旧域消失导致第二个包等不到、进度条停在上一包的导入文案上,而忙碌标志被重置又让按钮可点。现整段更新流程锁住程序集重载、只在全部收尾后解锁一次;导入期间先收起本工具进度条避免与 Unity 自带导入条互相覆盖;另加域重载兜底收尾
- **Odin 版更新器窗口正文被渲染成「禁用灰」** — Odin 对不可编辑属性会推入禁用绘制作用域,导致说明框、列表标签、行文本与状态行全部呈禁用态;现按 Odin 官方做法标注 `[EnableGUI]` 强制按可用状态绘制(不可编辑语义不变),实测文字亮度 128 → 196

---

### Aesir Modules

- 与 Aesir Architecture 同步发布 0.26.0(版本号对齐,本包无功能变更)

---

## [0.25.1] - 2026-09-25

---

### Aesir Modules

**Fixed**

- **场景测试套件把测试场景常驻 EditorBuildSettings(并被打进玩家构建)** — 原先经 `[InitializeOnLoadMethod]` 在编辑模式域加载期把两个测试场景登记为 enabled 条目且从不摘除,条目随 `ProjectSettings/EditorBuildSettings.asset` 落盘、进玩家构建;现改为 `IPrebuildSetup` 在进入 Play 前的编辑模式阶段登记、`IPostBuildCleanup` 退出 Play 后按名摘除,条目仅存在于本次运行期间,另有域加载兜底清扫回收被强杀遗留的条目。新增 EditMode 守护用例锁定「测试场景不得常驻 BuildSettings」
- **SceneModule 测试无法随包进入实际工程** — 测试原先硬依赖宿主工程存在 `Assets/Scenes/SampleScene.unity`(消费工程可能已删除它),缺失时 `FromScenePath` 直接抛异常;现由 SetUp 在缺失时从包内最小场景夹具临时复制、TearDown 按「谁创建谁删除」还原(含空目录)。测试资产路径不再写死 Assets 相对路径,改为按文件名经 AssetDatabase 定位(Assets 安装 / 嵌入式包 / UPM Git 安装自适应)

---

### Aesir Architecture

- 与 Aesir Modules 同步发布 0.25.1(版本号对齐,本包无功能变更)

---

## [0.25.0] - 2026-09-25

---

### Aesir Modules

**Fixed**

- **Script Doc Generator 增量重生成产出双 Front Matter** — Zensical 生成器自产 YAML 头后,增量合并逻辑会把旧文件的 Front Matter 再拼一份到新内容前,对已存在文档重生成必然产出双重头部;合并逻辑收敛为 `MergeFrontMatterWhenMissing`:新内容自带 Front Matter 时以新生成的为准,不自带(如中文 API 生成器)仍保留旧文件头部。新增 EditMode 回归测试 4 用例

---

### Aesir Architecture

- 与 Aesir Modules 同步发布 0.25.0(版本号对齐,本包无功能变更)

---

## [0.24.0] - 2026-09-25

---

### Aesir Architecture

**Added**

- **Aesir Getting Started 窗口** — 菜单 `Tools → Aesir → Getting Started`(priority -1000 居顶 + 独立分割线):概览页展示包卡片(含未安装包占位引导),包页按教学分组列出示例;整卡点击定位示例文件夹,带场景的示例经「打开场景」按钮直达(先保存当前场景一次再切换),结果经右下角 Toast 提示。数据层以各包 package.json samples 清单为唯一真源;IMGUI 兜底 + Odin 动效窗口经 `OdinWindowOpener` 委托路由
- **示例构建剔除钩子** — 构建时自动把 Build Settings 场景列表中的 Aesir 示例场景从本次构建剔除并输出 `[Aesir Build]` 日志(持久数据不动);「示例不进玩家构建」自此覆盖脚本与场景两条路径
- **安装位置锚点机制** — 包根 `AesirPathLookup.asset` 锚点(参照 Odin Inspector 同款资产),`AesirAssetPaths` 三级解析实际安装根——Runestone 可整体移动到项目任意文件夹,构建剔除 / 包更新器 / Getting Started 三个消费端全部跟随
- **示例脚本构建剔除守护测试** — 运行时示例程序集内每个 .cs 必须整文件 `#if UNITY_EDITOR` 包裹,漏包裹在 EditMode 测试失败

**Changed**

- 更新器 Odin 窗口标题区改手绘(移除灰暗 `[Title]`);Tools/Aesir 菜单按包分组(包专属项归入 `Architecture/`、`Modules/` 子菜单);Check for Updates 移至菜单最底部(1100 + 分割线);`QuickCreateSOMenuItem` 移入 `Editor/MenuItems/`;`AesirArchitecturePlayerLoop` / `AesirScheduler` 文件归位 `Runtime/Common/`

**Renamed(破坏性变更)**

- `ScriptingSymbolUtility` → `ScriptingSymbolEditorUtility`(Utilities 目录命名规范,外部脚本直接引用需同步更名)

---

### Aesir Modules

**Added**

- **UI 模块 Canvas 根窗口形态** — 与 Panel 并列的第二种 UI 形态:窗口预制体根节点自带 Canvas,挂 UIRoot 下(不经四层 Canvas),`sortingOrder` 默认 500 恒在面板四层之上;静态 API `UIModule.Open<T>()` / `Close<T>()` / `GetWindow<T>()` 等;预制体结构约定 `Mask`(蒙版) + `Content`(内容容器)
- **窗口蒙版遮罩机制** — `maskMode` 单遮(仅最高层可见窗口蒙版生效)/ 叠遮(各窗口独立),运行时可切换;蒙版点击经 `OnMaskClicked()` 虚方法默认按 `closeOnMaskClick` 关闭
- **Binder 窗口感知** — 基类下拉含 `AesirBaseWindow` 家族,根节点带 Canvas 时默认脚本名后缀 `Window`
- **示例 UI Basic Usage**(`Samples/UI/01_BasicUsage`,已登记 samples);窗口与蒙版 EditMode 测试 22 用例
- **缺依赖一键补装(`AesirDependencyInstaller`)** — unitypackage 形态下 Aesir Architecture 缺失时,菜单 `Tools → Aesir → Modules → Install Dependencies` 确认后经 UPM 自动补装;配套 package.json 依赖由不可解析的 semver 改为 Git URL 版本分支(UPM 安装时自动递归拉取);新增零引用 `Runestone.AesirModules.Editor.Bootstrap` 程序集

**Changed**

- Script Doc Generator / Scene Editor Settings 菜单归入 `Tools/Aesir/Modules/`;Odin / Addressables 细分程序集锚点由 `Common/` 迁至 `Integration/`(程序集名与引用零变化);示例 `KeyPressedEvent` 的 using 指令移入 `#if UNITY_EDITOR` 内

**Planned(下期候选)**

- SmartShowHide 伪隐藏(全屏窗口弹出时自动伪隐藏被遮挡面板、关闭后恢复)

---

## [0.23.0] - 2026-09-23

### Aesir Architecture

本批为全仓锐评修复与"保持极简"定位收敛,含破坏性变更(详见包内 CHANGELOG 的 [0.23.0] 段)。

- **Added**: `ObservableQueue<T>` 专属测试(8 用例,补齐四集合专属测试的最后缺口); AesirScheduler NaN 延时回归用例
- **Changed**: AesirScheduler NaN 延时语义与文档对齐(NaN 传播 = 永不触发); `ObservableList<T>.Move` 同索引零变化不通知; `MiniEvent` / `MiniEvent<T>` 的 AddListener 补 null 守卫; `ObservableQueue<T>` 结构补齐对齐其余三集合(sealed / `[Serializable]` / 结构体枚举器 / 构造 null 容忍); 根 README 失实宣称修正、测试数与示例计数更新、快速开始改版为最小五概念快车道
- **Removed(破坏性变更)**: `ObservableHashSet<T>` 集合代数全套 10 方法(`IObservableHashSet<T>` 不再继承 `ISet<T>`,需要时用内部 `HashSet<T>` 或上游 Cysharp.ObservableCollections); `ObservableList<T>` 区间 Sort / Reverse 重载; ReadOnlySpan 批量重载(保留 `T[]` 与 `IEnumerable<T>` 双轨); Dictionary / HashSet 构造器收敛至「默认 / 初始元素 / 比较器」三个; `ObservableValue<T>.SetValue` 别名; 调试 / 测试专用面收窄 internal(`IContext` 与 `IGenericLocator<T>` 各删 4 个接口成员、`MiniEvent.GetListeners`、`AesirArchitecturePlayerLoop.Reset` 等)
- **Renamed(破坏性变更)**: `ObservableValue<T>.Clear()` → `ClearListeners()`——命名对齐集合家族语义

### Aesir Modules

- **Added**: Binder 代码生成器同类型多组件测试 ×2(按出现序号取 `GetComponents<T>()[n]`); SceneModule 批量卸载重入 PlayMode 回归
- **Fixed**: Binder 三处 P1(Missing 脚本组件空引用 / 增量模式命名空间错位致自动挂载静默失效 / 同类型多组件错绑)+ partial 模式重生成幂等; SceneModule 批量卸载重入洞(嵌套 UnloadAllAddedScenes 截断外层快照); UIModule / AudioModule / EventModule 重复实例销毁粒度统一 `Destroy(this)`; `PrewarmPanel` 补键语义守卫; 三个测试 asmdef 补 `ODIN_INSPECTOR` 装配守卫; SDG 缩进正则修正与 SourceScanner 显式接口实现文档键错配修复(fail-closed 告警)
- **Changed**: 更新器双窗口编排上提共享控制器 `AesirUpdateController`(消除 ~200 行双真源); `ui-module.md` 补 Binder 两条边界声明(仅编辑期构建 / 生成产物使用 Odin 特性)

## [0.22.0] - 2026-09-22

### Aesir Architecture

- **可观察集合升级为 ObservableCollections 轻量内置子集** —— 新增 `ObservableQueue<T>`,四种集合统一单轨变更通知 `AddListener` / `RemoveListener`(`MiniEvent<T>` 承载,返回 `AutoRemoveListenerHandle`,可绑定 Unity 生命周期自动移除):无变更的写操作不通知、批量操作逐项通知、字典值更新以 Replace 表达(旧值在 OldItem)、Move 单事件、Sort / Reverse / Clear 统一 Reset;Odin Inspector 内联调试面板(可选);与上游 Cysharp 库可在同一项目共存(程序集/包名/命名空间三层隔离)
- **Removed(破坏性变更)** —— 轻量事件 API 全套(`AddXxxListener` 与四个事件参数类型)、原生 `CollectionChanged` 事件与 `SortOperation<T>` 移除,语义并入单轨事件(迁移:改订阅 `AddListener`,按 `e.Action` 分流);移除内部加锁与 `SyncRoot`(集合边界统一为仅主线程使用)
- 测试改写为单轨断言(含句柄绑定 GameObject OnDisable 集成用例),RAA Editor EditMode 186 用例全绿

### Aesir Modules

- 与 Aesir Architecture 0.22.0 版本同步发布,本包无功能变更

## [0.21.0] - 2026-09-14

### Aesir Architecture

- **包内更新器全面升级**——新增 Odin Inspector 界面(与 IMGUI 兜底共用逻辑)、更新日志面板(按远程 tag 拉取包内 CHANGELOG 展示本地→远程区间段落)与更新前确认框;修复预发布版本比较(rc 与正式版判等)、移除单包更新入口(防版本撕裂)、残留清理移到导入成功之后
- **新增 `AesirScheduler` 帧粒度时间调度原语**——`Delay(seconds, callback)` / `NextFrame(callback)` 纯 C# 静态 API,经 PlayerLoop BeforeUpdate 钩子结算,为无协程能力的 Model / Service / Command 提供合法延时手段;有意收窄(帧粒度/游戏时间/一次性任务/不池化)
- package.json samples 登记 `RuntimeInitializeLoadType` 示例(共 11 个可导入示例)
- 修复 DDOL 根物体保护(预放置为子物体时跟随宿主)、脏排序只重排脏列表、QuickCreateSO 空资源名
- 测试扩充:View/触发器族、工具类、更新器 19→32、MiniEvent RemoveListener;PlayMode 测试卫生(断言拆分/卸载等待/全量 Warning 捕获/真实时间窗)

### Aesir Modules

- **事件模块三缺陷修复(全仓最严重单点)**——重入分发(回调内再发布事件)不再覆写共享参数数组;分发改注册表快照迭代(回调内退订/注册不干扰本趟);优先级稳定排序(同档按注册顺序);摘除"实验性"标注,文档改写快照与重入安全语义、零分配口径收敛
- **音频模块修复**——补 `ResetStatics` 静态重置(RAM 唯一漏掉的单例铁律)、`PlayBgm` 淡出中重播同曲取消淡出续接(幂等误伤)、`PlaySfx` 音调钳制 [0.01, 3] 不再反播;示例音量滑条改"拖动结束落键"
- **UI 模块**——`ShowPanel`/`PrewarmPanel` 注册时序前移(OnShow 抛异常不泄漏、递归 Show 不重复实例化);re-show 强转改 `as` 判空、`InstantiateInactive` try/finally 恢复源预制体、`RegisterPrefab` 换路径诊断警告
- **场景模块**——`UnloadAllAddedScenes` 快照迭代(广播期间嵌套加载/卸载不干扰本趟)、Single 成功路径 `SetActiveScene` 前 `IsValid` 校验;**新增 RAM 首个 PlayMode 测试套件**(真实加载成功路径 4 用例,`Tests/Runtime/`)
- **ScriptDocGenerator**——XML 实体解码(泛型实体不再双重转义)、多成员代码块归属分析(fail-closed 跳过告警)、Default 生成器与 Zensical 收敛共享 `MemberGrouper` 引擎(582→343 行)、杂项六项修复
- 测试扩充:EventModuleTests 22→32、AudioModuleTests 30→38、UIModuleTests 13→17、XmlSummaryToolTests 25→34、新增 Zensical 输出 7 用例

## [0.20.0] - 2026-09-11

### Aesir Architecture

- 新增《设计变更记录》文档——废弃机制(事件总线/ModelReplaced/120 帧轮询/初始化失败回滚/异常吞噬等十组)与设计来源统一收录,源码注释此后只描述当前行为
- 注释精简(12 文件):移除源码注释中的历史演进叙述(更名史/fake-null 演进史/复刻来源等),重复 remarks 去重
- 修复 `AesirArchitecture.DontDestroyOnLoad` 遗留 CS0108 编译警告(补 `new` 修饰符,行为不变)

### Aesir Modules

- **事件模块分发增强与 SO 资产化**——订阅者过滤器(`ISubscriberFilter` + `WithTag`/`WithPriority`/`SameSceneAsEmitter`/`OnlySelf`/`InsideCollider2D`,fail-closed);死引用清理(分发期自动移除已销毁订阅者);性能监控(`executionMsLimit`,默认关闭);`AesirEventArgsSO` 资产发布者 + `UnityEventOnAesirEvent` 桥接组件 + `SubclassSelector` 子类下拉;热路径绑定键缓存(分发热路径稳态零分配,编译委托加速 ~78 倍);新增 22 用例与 `02_Filters`/`03_SOAsset` 示例
- **新增音频模块(2D)**——`AudioModule` 全静态门面:SFX 独占音源轮询(每播音调/音量独立,无每播实例化开销)、BGM 淡入淡出与同曲幂等、Master/BGM/SFX 三通道音量与静音 PlayerPrefs 持久化;30 用例与 `01_BasicUsage` 示例
- **场景模块行为层补齐**——`SceneModule` 补 DDOL 字段(修复预放置实例被自己的 `LoadSceneSingle` 销毁)、新增 `SetActiveScene` 与场景事件广播、`onProgress` 进度回调、`SceneAssetWrapper` 悬空判定;20 用例与专属文档
- **UI 模块注册表重构(含破坏性变更)**——三字典合并单注册表(键=实际类型,基类类型调用 ShowPanel 报错拒绝/HidePanel/GetPanel 警告)、层 Canvas 缺失 fail-fast 中止、`HidePanel<T>` 约束收紧为 `MonoBehaviour, IUIPanel`;移除 `UIModule.RegisterUIRoot` 与 `IUIAssetLoader.Unload`(破坏性);13 用例与专属文档
- **ScriptDocGenerator 修复与增强**——生成器三处输出 bug 修复(单成员类丢章节/常量表过滤写反/空继承章节)、Zensical 生成器参数与备注全链路输出、面板配置域重载不再丢失、默认输出目录移出 Assets、UI Toolkit 窗口移除(Odin 窗口为唯一入口)、Summary 工具改为 `[Summary]` 特性优先语义
- 修复 `AesirListenerAttribute` 缺 `AllowMultiple`、`SubscriberPriority` 文档口径修正(实为 4 档 First/High/Medium/Last)

## [0.19.0] - 2026-09-11

### Aesir Architecture

- 新增 `IContext` / `AbstractContext<T>` 的 `UnregisterModel<TModel>` / `UnregisterService<TService>`——按类型键摘除注册并释放实例(幂等,注销后再注册追加到顺序末尾)
- 新增 `AesirArchitecture.DontDestroyOnLoad` 只读属性——暴露 DDOL 决策取值,供运行时查询与编辑器条件提示复用
- 修复 Odin AttributeProcessor 两处信息框宣称与实现不符(类级条件 Warning 补齐/DDOL 警告改条件显示);`AbstractSubmodule.Dispose` 重置 `Initialized`;快捷档示例 Model 改只读属性暴露、严格档示例缓存 Query 实例复用

### Aesir Modules

- 版本号与 Aesir Architecture 同步更新至 `0.19.0`,本包本版本无功能性变更

## [0.18.0] - 2026-09-10

### Aesir Architecture

- `AesirArchitecturePlayerLoop.Register` 现返回 `AutoRemoveListenerHandle`——Dispose 时自动注销本次注册,对齐全框架句柄风格;忽略返回值的既有调用不受影响
- 新增 `CapabilityExtensionsTests`(10 用例)——CQRS 执行链首次获得测试覆盖:带参/无参命令与查询、命令链、查询组合、`IController<T>` 默认接口实现绑定、两阶段初始化异常语义、Dispose 后单例重建
- 修复 **Model 初始化阶段获取 Service 的误导性报错**——明确「先全部 Model、后全部 Service」两阶段初始化下 Model 阶段无法获取 Service(与注册顺序无关),须延迟到运行期方法调用
- 修复 `AbstractContext<T>.Dispose` 后的**僵尸单例**——释放后解除 `Instance` 单例缓存,再次访问重建并重新初始化全新上下文,而非返回容器已清空的空壳
- 修正三处 XML 文档失实:`AesirArchitecture` 组件职责(实为 DDOL 宿主,不初始化架构数据)、`AesirArchitecturePlayerLoop` 已废弃的"周期性检测"宣称、`MiniEvent`"零分配"措辞(仅 Invoke 路径零分配)
- 解除 Editor 工具对测试框架的结构绑定:Editor 主程序集移除 `UNITY_INCLUDE_TESTS` defineConstraint、package.json 移除 test-framework 硬依赖;PlayMode 测试程序集以 `ODIN_INSPECTOR` 守卫(无 Odin 环境自动排除)
- `RegisterCustomLifecycle(mono / GameObject, evt, callback)` 现将监听绑定到所在物体销毁事件自动移除(行为变更,原先不自动移除)
- 重复实例去重改用 `Destroy(this)` 不再连带销毁业务物体;私有 `Reset` 更名 `ClearState` 避免撞名 Unity 魔法方法;`AbstractQuery` 补 `[Serializable]`

### Aesir Modules

- **ScriptDocGenerator 模块(需 Odin)**——原 `Assets/ScriptDocGenerator` 独立工具整合为包内功能模块:反射分析 C# 类型生成结构化 API 文档(增量保留手写内容),附 Summary 工具(XML `<summary>` ↔ `[Summary]` 双向同步);153 个单元测试汇入测试程序集;入口 `Tools → Aesir → Script Doc Generator`

## [0.17.0] - 2026-09-06

### Aesir Architecture

- 新增 `InternalContextAttribute` —— 标记框架内部 Context(示例 / 测试用途);Aesir Modules Binder 的「Context 类型」选择器自动跳过被标记的类型
- 修复**示例场景无法运行**(0.14.0 起回归):示例程序集改为运行时程序集 + 示例脚本整文件 `#if UNITY_EDITOR` 包裹 —— 编辑器内可编译、可挂载、可 Play,玩家构建整体剔除(示例类型 0 入包)

### Aesir Modules

- **Binder 组件绑定全面完善** —— 双生成模式(「同一脚本增量」默认 /「Partial 分部类」可选,默认 `.designer.cs` 后缀);基类下拉新增 `AesirBasePanelView<T>` / `AesirBasePanelViewController<T>` 预选;层级右键菜单快捷挂载 `BinderAssistant` / `BinderTag`;命名空间默认值与后缀候选持久化;新增 69 个 EditMode 测试
- 修复 Binder 生成脚本接口不匹配、Partial 模式覆盖手写文件、绑定校验空引用等问题
- 同步修复示例场景回归(同 Architecture 机制)

## [0.16.x] - 2026-09-06

- `LICENSE.md` 与 `Third Party Notices.md` 移至包根,对齐 UPM 包根约定
- Third Party Notices 收录设计参考条目:Cysharp/ObservableCollections(Architecture)、Eflatun.SceneReference(Modules)

## [0.16.0] - 2026-09-06(Modules 主要变更)

- `SceneAssetWrapper` 功能增强(吸收 Eflatun.SceneReference):GUID 锚点自愈、`State` / `UnsafeReason` 状态机、`TryGet` 安全读取家族、工厂方法
- Addressables 条件架构:未安装时相关代码整体不编译、零报错
- **破坏性变更**:删除 `AddScene` / `UnloadAddedScene` 统一为 `LoadSceneAdditive` 纯叠加追踪;`ReloadScene` 同步改异步;`SceneAssetWrapper` 空引用语义收紧(抛 `EmptySceneAssetWrapperException`);加载失败全系新增 `onFailed` 回调
- 目录调整为标准 Unity 自定义包根结构(`Runtime/` / `Editor/` 两级,模块以子目录存在,模块间零依赖)

## [0.15.0] - 2026-09-05

- 新增可观察集合 `ObservableHashSet<T>`;新增 RuntimeInitializeLoadType 示例
- 包内更新器大陆优化:jsDelivr 多域名 → GitHub API → 302 重定向探测三级兜底
- Modules 无功能性变更(版本同步)

## [0.14.0] - 2026-09-05

- **AesirFramework 转型**:AesirInspector 迁出为独立仓库;本仓库由 Aesir Architecture 与 Aesir Modules 组成
- Samples / Documentation 双目录结构(`Samples/` 编写主位 + `Samples~/` 发布镜像)
- 新增 PlaneWar 实战示例(Mono 版)

## 更早版本

0.13.0 及更早的完整变更(含 ObservableList / ObservableDictionary、DDOL 重设计、MVP 示例家族、Query 系统等里程碑)请查阅[根 CHANGELOG](https://github.com/yuumixcode/AesirFramework/blob/main/CHANGELOG.md)与各子包内 `CHANGELOG.md`。
