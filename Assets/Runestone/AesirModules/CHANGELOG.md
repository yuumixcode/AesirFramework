# Changelog

本项目的所有重要变更均会记录在此文件中。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/)，
版本号遵循 [Semantic Versioning](https://semver.org/lang/zh-CN/)。

## [Unreleased]

### Added

- **事件模块分发增强与 SO 资产化**：
  - **订阅者过滤器（精确投递）** — `ISubscriberFilter` 策略接口 + `AesirEventArgs.WithFilter/WithFilters` 链式 API；内建五种过滤器：`WithTag` / `WithPriority` / `SameSceneAsEmitter`（多场景叠加加载）/ `OnlySelf`（自身/子树/父级链）/ `InsideCollider2D`（空间局域广播）；过滤器无法解析对象时按不通过处理（fail-closed），异常与其他订阅者隔离
  - **死引用清理** — 分发时自动检测已销毁的 Unity 订阅者并从双注册表移除（编辑器内输出告警）；此前仅跳过不清理，注册表会永久累积
  - **性能监控** — `EventModule` 新增 `executionMsLimit` 字段（毫秒阈值，默认 0 关闭），单次分发超阈值输出 Warning（含事件名、耗时与订阅者数量）；静态 Stopwatch 复用零稳态分配
  - **SO 资产化** — `AesirEventArgsSO`（Project 右键 `Create → Aesir → Event Module → AesirEventArgsSO` 创建，以资产为发布者触发）+ `UnityEventOnAesirEvent` 桥接组件（Inspector 串联 UnityEvent 回调）+ `SubclassSelector` / `ExcludeSubclassSelector`（`[SerializeReference]` 子类下拉，UI Toolkit PropertyDrawer）+ SO 自定义 Inspector（运行模式限定 Raise 按钮）
  - **热路径绑定键缓存** — `GetEventBindingKey` 按事件类型缓存 `AssemblyQualifiedName`：原生拼接每次发布 ~1µs 且新分配一个字符串（分发热路径上唯一剩余分配点），缓存后 ~20ns 字典查询、稳态零分配；实测编译委托调用 ~4ns/次（vs `MethodInfo.Invoke` ~300ns，加速 ~78 倍），1000 订阅者单次发布 ~571µs（含死引用检查、过滤器检查与排序）
  - 新增 EditMode 测试 `EventModuleTests`（22 用例：过滤器全五类、链式 API、死引用清理、性能监控、SO 资产化、多特性订阅、类型推断注册，性能特征回归锁——键缓存引用同一性断言 + 编译委托 vs 反射 20 万次迭代对比计时）
  - **包内示例** — `Samples/Events/02_Filters`（Space 发布 `WithTag`+`InsideCollider2D` 双重过滤警报、R 发布 `OnlySelf` 家族命令，场景内置多组对照组 + uGUI HUD 日志）、`Samples/Events/03_SOAsset`（`ScoreEventAsset.asset` 配置事件载荷、`UnityEventOnAesirEvent` 桥接组件零代码串联 UnityEvent 回调）；均已登记 package.json samples 并同步 `Samples~` 镜像
- **音频模块（Audio Module）** — 2D 音频极简门面 `AudioModule`（MonoBehaviour 单例，公开 API 全为静态成员）：
  - **SFX** — 固定数量独占音源轮询（默认 8，可配）：无每播实例化开销，每次播放的局部音量/音调独立生效，源全忙时抢占最旧；`PlaySfx(clip, volume, pitch, pitchJitter)` 支持音调随机抖动
  - **BGM** — 专用循环音源；同曲在播幂等返回（跨场景重复触发不打断音乐）；`PlayBgm(clip, fadeSeconds)` / `StopBgm(fadeSeconds)` 协程淡入淡出（`unscaledDeltaTime`，slow motion 不变调）
  - **音量与静音** — Master / BGM / SFX 三通道乘法链 + 三通道静音（Master 总闸），设置即时生效并经 PlayerPrefs 持久化（键前缀可配）
  - **暂停** — `PauseAll` / `ResumeAll`
  - **配置** — 零配置可用；`AudioConfigSO`（Create 菜单 `Aesir Modules → Audio → AudioConfig`）管默认音量、持久化开关与键前缀；预放置实例 Inspector 说明由 Odin AttributeProcessor 注入
  - 新增 EditMode 测试 `AudioModuleTests`（30 用例：单例生命周期、SFX 轮询、BGM 语义、音量/静音乘法链、持久化、配置载入）与示例 `Samples/Audio/01_BasicUsage`（OnGUI 面板驱动全部 API，自包含程序化 wav 音频）
- **场景模块（Scene Module）行为层补齐与缺陷修复**：
  - **生命周期对齐** — `SceneModule` 补 `dontDestroyOnLoad` 序列化字段（默认开，对齐 UIModule 三态 DDOL 范式：预放置根物体 DDOL / 子物体跟随宿主 / 关闭警告）；此前预放置实例会被自己的 `LoadSceneSingle` 随场景卸载销毁、加载回调静默丢失
  - **`SetActiveScene`** — 激活场景切换 API（`path` / `SceneAssetWrapper` 双重重载，返回 bool），补齐多场景叠加工作流刚需（决定光照设置来源与 Instantiate 默认落点）
  - **场景事件广播** — `SceneLoadedEvent` / `SceneUnloadedEvent`（`MiniEvent<string>`，参数为场景路径），多系统可订阅场景生命周期
  - **加载进度** — `LoadSceneSingle` / `LoadSceneAdditive` 新增 `onProgress` 逐帧进度回调（按 Unity 场景激活上限 0.9 归一化到 1）；移除全仓零消费者的 `GetTotalLoadingProgress`
  - **API 收敛** — 移除零消费者且语义有陷阱的 `BootstrapScene` 属性（stale Scene 快照）及运行时 bootstrap 半链（`AutoSetupBootstrapScene`）；`PresetBootstrapSceneNames` 改只读 `IReadOnlyList<string>`；`UnloadAllAddedScenes` 单个场景卸载失败改为跳过并告警（原为空等一帧静默跳过）
  - **`SceneAssetWrapper`** — 新增 `IsDangling` 悬空判定与 Inspector 悬空 Error 信息框（此前只有红色着色无说明）；`TryGetAddress` XML 修正"与 Address 一致"的失实表述
  - **Inspector 文案** — `SceneModule` 的 DetailedInfoBox 由失实的"自动搜索 BuildSettings"改为 DDOL 使用须知
  - 新增 EditMode 测试 `SceneModuleTests`（20 用例：拒绝矩阵、协程失败分支、Single 失败保留追踪、SetActiveScene、事件、重复实例与 DDOL 语义）与专属文档 `Documentation/scene-module.md`
- **系统事件（元事件）与 DefaultChannel 频道标签暂缓** — 依赖编辑器工具链（Subscription Monitor / Event Log 等调试窗口），待工具链立项后一并设计；单实例合并不实施（Awake 已销毁重复实例，且订阅均经 `Instance` 单一通道注册，重复实例不可能积累绑定）
  - **UI 模块文档补齐** — 新增专属文档 `Documentation/ui-module.md`（架构总览、生命周期与 OnClose/OnDestroy 职责分界、注册键语义、加载契约、Canvas 配置、设计边界）；中英 README 的 UI 章节补 `AesirBasePanelViewController<T>`（核心类型表与目录结构此前漏列，仅 Binder 节提及）、键语义警示与设计边界节
- **脚本文档生成模块（ScriptDocGenerator）修复与增强**：
  - **生成器正确性修复** — 中文 API 生成器三处输出 bug：单事件类/单方法接口曾整体丢失"事件/方法"章节（入口阈值 `<= 1` 应为 `<= 0`）；public 非 const 字段曾混入"常量字段"表并被双重展示（过滤布尔写反）；仅私有继承属性的类型曾输出空"继承的属性"章节（flag 预计算漏 API 守卫）。回归测试锁定（`DefaultScriptingAPIOutputTests` 4 用例）
  - **参数/备注全链路输出（Zensical 生成器）** — 参数表与返回值表新增"说明"列（XML `<param>` / `<returns>` 注释落地）；类型头部与成员详情渲染 `<remarks>` 备注、`<value>` 属性值说明与 `<typeparam>` 泛型参数说明；参数类型改从分析期结构化数据直取（`IParameterData[]`），移除约 80 行格式化字符串反解析
  - **分析数据层注释链路打通** — `ITypeData.TypeParamSummaries` / `IPropertyData.ValueSummary` / `IMemberData.RemarksSummary` / `IConstructorData.Parameters` 与 `ParamSummaries`：六种 XML 文档标签全部从 SourceScanner 流到数据类与生成器；源码文档缓存在每次分析前失效（此前仅靠域重载）
  - **调试检查模式** — 分析中间结果（完整成员树）默认不再绘制进 Inspector 且不写入面板资产（此前程序集模式下数百类型整图序列化进 .asset，窗口卡顿 + 资产膨胀）；新增"调试检查模式"开关按需开启渲染，程序集模式下开启时有性能警告信息框
  - **面板状态与交互** — 域重载不再清空用户配置（此前每次编译丢输出路径/生成器/类型来源，仅保留跨域必然失效的分析态）；程序集下拉只列项目脚本程序集并排序（此前全域数百项含引擎模块）；分析完成提示改用 Odin 原生 Toast 与正确文案
  - **默认输出目录移出 Assets** — 项目根 `ScriptDocGenerator/`（.gitignore 已登记），生成的 .md 不再产生 .meta；需随包分发时可在面板改回 Assets 内路径或任意绝对路径
  - **窗口收敛** — 移除 UI Toolkit 版窗口与其菜单项（功能子集且写死默认生成器、状态不落盘、工作流逻辑双份）；Odin 窗口为唯一主入口
  - **Summary 工具语义升级（行为变更）** — 内容源改为 **`[Summary]` 特性优先**：已存在可解析特性时以特性文本为权威（Sync 模式 XML 与特性不一致时回写 XML 对齐，Replace 模式保持特性内容），无特性时回退 XML 生成特性；拼接字符串实参等无法安全解析的特性按"无特性"处理回退 XML
  - **Summary 工具源码安全修复** — 特性文本双引号/反斜杠转义（此前含 `"` 的 summary 会生成非法 C#）；保留原文件行尾风格（此前 CRLF 整文件转 LF）；`////` 四斜杠普通注释不再误判为 XML 文档注释；Remove 模式批量删除前确认对话框、无特性文件原样跳过不重写；批量处理仅触发一次 `AssetDatabase.Refresh`
  - **Front Matter 修复** — 旧文档无闭合 `---` 分隔符时不再把整个文件误当 Front Matter 拼回（此前产物为"旧全文 + 新文档"叠加）
  - **其他修复** — 事件访问器方法过滤 `Contains` → `StartsWith`（名字含 `add_` 的正常方法曾被误杀）；移除 `TypeData.TypeInfo` 死属性、`GetReadableTypeName` 不可达的 `obj` 尾部截断、`ReflectionUtility` 两处异常吞噬/包装（恢复 fail-fast）；字段式事件 `IsStatic` 改以 AddMethod 为准（原 `GetRaiseMethod` 必空引用）
  - 新增 EditMode 测试：`DefaultScriptingAPIOutputTests`（4）与 `XmlSummaryToolTests` 新语义 7 用例（双向对齐/转义/CRLF/`////`/无变化跳过/拼接回退）

### Changed

- **UI 模块注册表重构（含破坏性变更）**：
  - **单注册表** — `_activatedPanelDict` / `_deactivatedPanelDict` / `_uiPanelDict` 三字典合并为单实例注册表 `_panelDict`（键 = 实际类型）；激活/停用状态由面板自身 `IUIPanel.IsOpen` 承担，消灭三表双写不一致空间与理论不可达的"内部状态异常"防御分支；`ShowPanel` / `HidePanel` / `PrewarmPanel` 的重复实例化/挂层管线收敛为 `InstantiateAndAttach` 单实现（原先 4 处复制粘贴）
  - **键语义诊断（行为变更）** — 注册表以实例实际类型为键；以基类类型调用时：重复 `ShowPanel` **报错拒绝并返回 null**（此前会静默重复实例化）、`HidePanel` / `GetPanel` 记录键语义警告（此前 `HidePanel` 静默无效、`GetPanel` 警告文案指向错误方向）；精确未命中且无关联实例时三者均按幂等语义静默（`GetPanel` 原警告移除，与 `HidePanel` 口径统一）
  - **层 Canvas 缺失中止（行为变更）** — `ShowPanel` / `PrewarmPanel` 在面板所属层 Canvas 缺失（UIRoot 层级结构性损坏）时记录错误并中止显示、清理半挂载实例（此前仅跳过挂层、生命周期照跑，产出不可见僵尸面板）
  - **`HidePanel<T>` 泛型约束统一** — `where T : IUIPanel` 收紧为 `where T : MonoBehaviour, IUIPanel`（与 Show/Get/Prewarm 家族一致）
  - **Assets/Create 默认 UICanvasConfig 菜单幂等化** — 已存在时复用并提示（原为拒绝创建 + 警告），与 UIRoot Inspector 按钮共用同一实现，消灭两份等价代码的行为漂移
- **面板 `OnClose` 语义修正（文档）** — XML 注释与 README 明确：仅受控销毁路径（`HidePanel` 且 `DestroyOnHide=true`）调用；场景卸载/外部 `Destroy` 只触发 `OnDestroy`，事件解绑与订阅释放须放 `OnDestroy`（或两处都写），仅写 `OnClose` 会在场景切换时泄漏
- **加载器契约收敛（文档）** — `IUIAssetLoader` 宣称从"可替换为 Addressables 等"收敛为同步语义（Resources / 同步缓存）；异步管线需自行预加载后同步返回，XML 与 README 均已注明 WebGL `Handle.Result` 死锁风险

### Removed

- **`UIModule.RegisterUIRoot(UIRoot)`（破坏性）** — UIRoot 不再在 Awake 中反向注册 UIModule，改为纯拉方向（`UIModule.EnsureReady` 经 `UIRoot.Instance` 惰性发现，行为不变）；根因：编辑器菜单 `GameObject → Aesir Modules → UI/Create UIRoot` 此前会经 `RegisterUIRoot` 连带触发 `UIModule.Instance → [Aesir Modules]` 宿主的运行时懒创建链，使运行时路径物体被固化进场景（且 Undo 只注册 UIRoot、撤销会残留连带物体）
- **`IUIAssetLoader.Unload(GameObject)`（破坏性）** — 全仓零调用的死接口方法；预制体引用由 UIModule 注册表持有、生命周期与模块一致，契约不设释放方法

### Fixed

- **UI 模块缺陷修复**：
  - `AesirBasePanel.OnClose` XML 注释失实修正（"面板即将销毁前调用"→ 仅受控销毁路径调用 + OnDestroy 职责分界说明）
  - UIRoot Editor 段（`CreateAndLoadCanvasConfigAsset`）与 `UIModuleMenuItems.CreateUICanvasConfigAsset` 双份等价实现统一为 `UIRoot.EnsureDefaultCanvasConfigAsset` 共享入口
  - `UIModule` 顶部残留的空 `#if ODIN_INSPECTOR` 条件编译块清除
  - 新增 EditMode 测试 `UIModuleTests`（13 用例，`Tests/Editor/UI/`）：三路 Show 状态迁移与生命周期顺序（OnInit → OnShow，Awake 推迟由复用断言锁定）、Hide 的 DestroyOnHide 双分叉、Prewarm 幂等与复用、以基类类型操作的键语义诊断（Error/Warning 分级）、`RemovePanelRecord` 外部销毁反清理、层 Canvas 缺失中止、未知类型幂等静默——补齐 0.1.0 时代 UIManager 面板流程测试（拆分时丢失）的回归保护
- **Scene 模块缺陷修复**：
  - Single 加载失败不再误清叠加追踪——`_addedScenePaths` 清空从"加载前"移至"加载成功后"，失败路径保留旧追踪
  - 重复实例改为 `Destroy(this)`（不连带销毁宿主物体上的其他组件，对齐 RAA 先例），并补 `[RuntimeInitializeOnLoadMethod]` 静态重置
  - `BootstrapSceneHelper` 预设名搜索修复"子串命中即断 + 大小写双标"：`FindAssets` 子串命中但精确过滤落空时继续尝试下一个预设名、文件名比较统一 `OrdinalIgnoreCase`——此前 `Foo_Bootstrapper` 之类会吞掉真实启动场景（如小写 `bootstrap.unity`）的注册
  - `SceneEditorSettings` 设置资产路径迁至 `ScriptableSingleton/AesirModules/`（遵循项目 ScriptableSingleton 前缀约定，自动被 .gitignore 覆盖；原 `ProjectEditorSettings/` 为未忽略的游离目录）
  - 编辑器窗口菜单归位 `Tools/Aesir/Scene Editor Settings`（原 `Tools/场景管理方案设置窗口`，违反包内菜单约定）
  - 文档对齐实现：README"启动场景"段拆写运行时（仅持有引用）与编辑器（`BootstrapSceneHelper`，默认关闭）双系统职责；README 依赖节与 `SceneAssetWrapper` XML 明示 Odin Inspector 边界——wrapper 的 Inspector 面板效果需 Odin，未安装仅保证 API 可用（`FromScenePath` 构造 / `SceneAsset` 代码赋值 / TryGet 家族），面板不支持
- `AesirListenerAttribute` 补 `AllowMultiple = true`（文档早已宣称、`Bind` 亦按多特性编写，此前单个方法无法标注多个 `[AesirListener]` 监听多种事件）
- 修正文档与代码不一致：`SubscriberPriority` 实际为 4 档（First/High/Medium/Last），此前 XML 注释、中英 README 与 event-module.md 均描述为 5 档（含 Essential/Low/Cleanup 等不存在的枚举值）

### 规划中

- 对象池扩展（当前用隐藏复用，必要时增加 UIForm 对象池）

## [0.19.0] - 2026-09-11

### Changed

- 版本号与 Aesir Architecture 同步更新至 `0.19.0`，本包本版本无功能性变更

## [0.18.0] - 2026-09-10

### Added

- **ScriptDocGenerator 模块（需 Odin）** — 原 `Assets/ScriptDocGenerator` 独立工具整合为本包功能模块：反射分析 C# 类型生成结构化 API 文档（增量保留手写内容），附 Summary 工具（XML `<summary>` ↔ `[Summary]` 双向同步）。命名空间 `Runestone.AesirModules.ScriptDocGenerator`(.Editor)，代码经 asmref 汇入 Odin 程序集；入口 `Tools → Aesir → Script Doc Generator`；153 个单元测试汇入 `Runestone.AesirModules.Tests`

## [0.17.0] - 2026-09-06

### Added

- **Binder 组件绑定全面完善**：
  - 双生成模式：「同一脚本增量」（默认）与「Partial 分部类」（后缀可选，默认 `.designer.cs`）
  - 「Context 类型」下拉（AbstractContext 派生类扫描，`[InternalContext]` 内部 Context 自动排除）
  - 基类下拉新增 AesirBasePanelView<T> / AesirBasePanelViewController<T> 预选（新增 `AesirBasePanelViewController<T>`）
  - 层级右键菜单快捷挂载 BinderAssistant / BinderTag
  - 生成代码：TitleGroup 分组、4 空格缩进、全限定自包含、region 含 BindComponents
  - 命名空间默认值与后缀候选列表 ScriptableSingleton 持久化
  - 默认字段名规则优化；BinderTag 默认绑定 1 个组件；类级 DetailedInfoBox 使用说明
  - 新增 EditMode 测试程序集 `Runestone.AesirModules.Tests`（69 个用例）

### Fixed

- 生成脚本实现 IComponentBinder 接口不匹配导致的编译错误
- Partial 模式重新生成覆盖手写 controller 文件的问题
- 绑定校验空引用 / 层级路径越界；EditorPrefs 键规范与自动挂载的跨 asmdef 类型解析
- **示例场景无法运行（0.14.0 起回归）** — `Events/01_KeyPress` 示例程序集从 Editor-only 改为运行时程序集 + 整文件 `#if UNITY_EDITOR` 包裹：修复 Editor-only asmdef 的 MonoBehaviour 禁止挂载场景物体导致的 Missing Script；玩家构建整体剔除

## [0.16.2] - 2026-09-06

### Changed

- **`LICENSE.md` 与 `Third Party Notices.md` 移至包根** — 同 Architecture；`Documentation~/` 镜像不再存放两文件的副本

## [0.16.1] - 2026-09-06

### Changed

- **新增 `Documentation/Third Party Notices.md`** — 收录 Eflatun.SceneReference（MIT）设计参考条目（Scene 模块 `SceneAssetWrapper` 吸收其功能设计）；注明仅设计参考、未包含源码
- **README（中英）Scene 模块 `SceneAssetWrapper` 条目补注功能设计参考来源**
- 根 README（中英）推荐链接新增 Eflatun.SceneReference

## [0.16.0] - 2026-09-06

### Added

- **`SceneAssetWrapper` 功能增强（吸收 Eflatun.SceneReference）** — GUID 锚点自愈（`sceneGuid` 序列化字段 + `EditorSyncFromAsset`）、`State` / `UnsafeReason` 状态机校验、`TryGet` 安全读取家族、`FromScenePath` / `FromAsset`（编辑器）工厂方法、`Address` 序列化缓存与 `AddressablesSupportEnabled`
- **Addressables 条件架构** — 核心 asmdef `versionDefines` 定义 `AESIR_MODULES_ADDRESSABLES`；独立胶水程序集 `Runestone.AesirModules.Editor.Addressables`（未安装 Addressables 时整体不编译，零报错），经 `SceneAssetWrapperAddressablesBridge` 静态委托桥注册地址查询与加入默认组能力
- **Scene 模块测试程序集 `Runestone.AesirModules.Scene.Tests`** — 27 个 EditMode 用例，自适应装 / 未装 Addressables 环境

### Changed（破坏性变更）

- **删除 `AddScene` / `UnloadAddedScene`**（含 `*WithSceneAssetWrapper` 变体）— 统一为 `LoadSceneAdditive` 纯叠加追踪：不再自动卸载上个场景、不再抢占激活场景
- **`ReloadScene` 同步改异步** — 带 `onCompleted` / `onFailed` 回调
- **加载失败新增 `onFailed` 回调** — `LoadSceneSingle` / `LoadSceneAdditive` / `UnloadScene` / `ReloadScene` 全系支持
- **`SceneAssetWrapper` 空引用语义收紧** — `ScenePath` / `Guid` / `SceneName` / `BuildIndex` / `LoadedScene` 空引用由返回空值改为抛 `EmptySceneAssetWrapperException`
- **`*WithSceneAssetWrapper` 6 个方法改为同名重载**

### Changed（包结构重组）

- **目录调整为标准 Unity 自定义包根结构** — 包根两级目录 `Runtime/` 与 `Editor/`，功能模块（UI / Scene / Events）以子目录存在于对应层级，模块间零依赖，删除模块 = 删除对应层的模块子目录；核心程序集锚点在层根（模块主代码自动汇入），Odin / Addressables 细分程序集锚点收拢至 `Common/` 下，模块专属代码经 4 个 asmref 汇入；程序集名称与公共 API 不变

## [0.15.0] - 2026-09-05

### Changed

- 版本号与 Aesir Architecture 同步更新至 `0.15.0`，本包无功能性变更；中英 README 随全项目文档核查刷新至 UIModule 架构（UI 章节重写、Scene 模块状态修正、移除失效的 ui-module-manual 链接）

> 注：本文件曾漏记 0.5.0 – 0.14.0 的逐版本条目，该区间历史见仓库根 CHANGELOG（聚合视图）。

## [0.14.0] - 2026-09-05

### Changed（仓库结构调整）

- **AesirFramework 转型同步** — AesirInspector 已独立为公开仓库（面向 Odin Inspector 开发者的学习工具包），本仓库仅含 Architecture 与 Modules 两个子包；依赖声明 `cn.runestone.aesir.architecture` 同步至 `0.14.0`
- **Samples 双目录结构** — 包内同时提供 `Samples/`（仓库内直接可见可运行）与 `Samples~/`（Git URL 安装后按需导入的源镜像）；示例目录统一为 `Events/01_KeyPress`（与 package.json samples 路径一致），示例代码随最新版本更新（EventEmitter → EventSender、OnKeyPressed → KeyPressedEvent）并以 `Samples/` 为编写主位同步至 `Samples~`
- **示例程序集构建剔除 + 命名空间统一** — 示例程序集 `includePlatforms` 收窄为 `Editor`（不进入构建包）；命名空间统一为 `Runestone.AesirModules.Samples.Events.KeyPress`

## [0.13.0] - 2026-09-03

### Changed

- **UIRoot 序列化引用重构** — 层 Canvas、UICamera、EventSystem 改为 `[SerializeField]` 引用持久化（`_layerCanvases` 以 `List<LayerCanvasEntry>` 存储替代字典，`[HideInInspector]` 隐藏，Inspector 呈现仍由 `UIRootAttributeProcessor` 注入）：存在性判定只依赖引用非空（Unity 假 null 即子物体已销毁、需重建），子物体重命名不再破坏引用、不再按物体名全量查找；旧版已搭建层级在引用缺失时按约定名一次性回收（保存场景后引用持久化，此后不再按名查找）；`PresetLayers` 静态缓存 `UILayer` 枚举值，避免每次构建调用 `Enum.GetValues` 产生装箱分配
- **Input System 集成目录迁移** — `Runtime/InputSystem/` → `Runtime/UI/InputSystem/`（程序集 `Runestone.AesirModules.InputSystem` 归位 UI 域，`InputSystemModuleHook` 逻辑不变）

## [0.12.0] - 2026-08-22

### Changed

- 版本号与 Aesir Architecture / Aesir Inspector 同步更新至 `0.12.0`，本包本版本无功能性变更

## [0.11.0] - 2026-08-22

### Breaking Changes

- **DDOL 机制重设计：预放置/运行时创建统一由 `dontDestroyOnLoad` 序列化字段控制** — `AesirModules`、`UIRoot`、`UIModule`（UI 模块）新增 `[SerializeField] bool dontDestroyOnLoad = true`，默认勾选时（含场景预放置实例）一律在 Awake 加入 DontDestroyOnLoad 场景（此前预放置实例保留在场景中）；取消勾选时实例保留在所在场景、随场景卸载销毁，必须自行处理多场景叠加（Additive）加载——Inspector 显示警告 InfoBox（Odin，新增 `AesirModulesAttributeProcessor` / `UIModuleAttributeProcessor`，扩展 `UIRootAttributeProcessor`），运行时输出提醒日志（仅编辑器期）。`UIModule` 的字段仅在预放置为根物体时生效，运行时自动创建时挂载于 [Aesir Modules] 宿主下跟随宿主决策。移除 `AesirModules` / `UIRoot` 的 `static bool _pendingDontDestroyOnLoad`（字段默认值天然覆盖运行时创建路径）

### Added

- **DDOL 开关字段级 InfoBox** — `dontDestroyOnLoad` 字段的 `[Tooltip]` 迁移为 AttributeProcessor 经 `ProcessChildMemberAttributes` 注入的 Info 级信息框（取值含义说明，恒显示），运行时程序集零 Inspector 样式特性
- **DDOL 警告 InfoBox 可见性修复** — 警告框改用 `"@!" + 字段名` 反转表达式，关闭开关时正确显示警告（此前 Odin `visibleIfMemberName` 的"true 时显示"语义导致警告在安全态显示、风险态静默；PropertyTree 探针验证 8/8 PASS）

### Changed

- **依赖版本同步** — Architecture 依赖版本号更新至 `0.11.0`
- **全包 XML 注释与 asmdef 缩进格式统一**（`<see />` 闭合空格、`<para>` / `<item>` 缩进、断行规范）

## [0.9.0] - 2026-08-15

### Changed

- **Odin 程序集重命名** — `OdinIntegration` → `OdinInspector`（三包统一）；Editor 程序集 `Runestone.AesirModules.Editor.OdinInspector`，目录 `Editor/OdinIntegration/` → `Editor/OdinInspector/`；`AssemblyInfo.cs` 的 `InternalsVisibleTo` 同步更新
- **依赖版本同步** — Architecture 依赖版本号更新至 `0.9.0`

### Fixed

- **UI 模块缺陷修复**（基于 Docs/AesirModules-UI模块-优化分析.md）：
  - #5 InstantiateInactive 停用态实例化，时序：挂层→Initialize→Show 内激活
  - #6 字典键归一化为 `uiPanel.GetType()`
  - #7 AesirBasePanel.OnDestroy → UIModule.RemovePanelRecord 静态反清理
  - #8 EventSystem 全场景 FindAnyObjectByType 检查
  - #9 Build 统一走 EnsureCanvasConfig + ApplyCanvasConfig，默认 SO 静态缓存
  - #10 GetLayerRoot 缺层 LogError + null
  - #11 内部状态异常补 Error 日志
  - #17 ShowPanel<TPanel,TPayload> / Show<TPanel,TPayload> 泛型重载

## [0.8.0] - 2026-08-06

### Changed

- **`FindFirstObjectByType` → `FindAnyObjectByType`** — 所有 MonoBehaviour 单例（`AesirModules`、`UIRoot`、`UIModule`、`EventModule`、`SceneModule`）的 `Instance` getter 改用 `FindAnyObjectByType`，后者不依赖 InstanceID 排序，在 Unity 6 中向前兼容

## [0.7.0] - 2026-08-05

### Changed

- **单例模式重构：预放置优先** — 所有 MonoBehaviour 单例（`AesirModules`、`UIRoot`、`UIModule`、`EventModule`、`SceneModule`）的 `Instance` getter 优先通过 `FindAnyObjectByType` 搜索已加载场景中预放置的实例，未找到时才运行时创建
- **条件式 DontDestroyOnLoad** — `AesirModules` 和 `UIRoot` 新增 `static bool _createdByRuntime` 标志，仅运行时创建的实例调用 `DontDestroyOnLoad`，场景中预放置的实例保留在场景中随场景生命周期销毁
- **移除 Bootstrap 自动初始化** — 移除 `AesirModules` 的 `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)] Bootstrap()` 方法，避免在场景加载前创建 DDOL 实例抢占预放置实例

### Removed

- `AesirModules.Bootstrap()` 方法及其 `[RuntimeInitializeOnLoadMethod]` 特性

## [0.6.0] - 2026-08-05

### Changed

- 版本号与 Aesir Architecture / Aesir Inspector 同步更新至 `0.6.0`，本包本版本无功能性变更

## [0.5.0] - 2026-08-01

### Changed

- 目录重命名：`Odin Integration` → `OdinIntegration`（与 Inspector 保持一致）
- 版本号与 Aesir Architecture / Aesir Inspector 同步更新至 `0.5.0`，本包本版本无功能性变更

## [0.4.2] - 2026-07-24

### Changed

- 版本号与 Aesir Architecture / Aesir Inspector 同步更新至 `0.4.2`，本包本版本无功能性变更

## [0.4.0] - 2026-07-24

### Changed（Manager of Managers 模式，合并为 UIManager）

- **架构重构**：UI 模块从 RAA Service 简化为 Manager of Managers 模式。原 `UISystem`（纯 C# 单例）与 `UIRoot`（MonoBehaviour 根节点）合并为 `UIManager`——单一 MonoBehaviour 单例（继承 `AesirMonoBehaviour`），同时承担面板管理与 UI 层级构建职责。
- **两层结构**：移除 `Runtime/Odin Integration/` 空壳程序集，架构精简为 Engine/UI/（接口、配置、枚举）+ Component/UI/（UIManager、AesirUIPanel、UICanvasConfigSO）。Odin 条件编译通过 `#if ODIN_INSPECTOR` 实现，无需独立程序集。
- **API 精简**：移除 `Back()`、`CloseAll()`、`Get<T>()`、`pauseUnderneath`、`OnPause`/`OnResume`、`SetUIRoot()` 等不适用的功能；生命周期简化为 `OnInit` → `OnShow` → `OnHide`/`OnClose`。
- **命名调整**：管理器统一为 `UIManager`；面板基类 `Panel` → `AesirUIPanel`；加载器接口 `IUILoader` → `IUIAssetLoader`。
- **预制体管理**：恢复 `RegisterPrefab<T>` 静态快捷方法，支持注册模式和路径模式并存。
- **XML 注释**：全部公共成员补充 XML 文档注释，统一使用多行格式。
- **文档重写**：README、使用手册、机制文档、调研分析全部同步为当前架构。

## [0.3.0] - 2026-07-12

### Changed（重构 · 作为 RAA 模块接入，去前缀，三层结构）

- **接入方式**：UI 模块整体作为 **Aesir Architecture (RAA) 的一个 Service** 接入。`UIManager : AbstractService`，注册于 `UIContext : AbstractContext<UIContext>`；提供 `UIManager.Service` 便捷单例入口（首次访问自动构建 UI 根节点）。
- **去前缀**：移除全部 `Aesir` 前缀——`AesirUIManager→UIManager`、`AesirPanel→Panel`、`AesirUILayer→UILayer`、`AesirPanelConfig→PanelConfig`、`AesirUILog→UILog`、`AesirResourcesUILoader→ResourcesUILoader`、`IAesirPanel→IUIPanel`、`IAesirUILoader→IUILoader`；Canvas 配置 `AesirCanvasConfigSO→UICanvasConfigSO`。
- **三层结构（镜像 RAA）**：`Runtime/UI/` 拆分为
  - `Runtime/Engine/`（纯 C# 架构核心：`UIManager`/`UIContext`/`UILayer`/`PanelConfig`/`IUIPanel`/`IUILoader`/`IUICanvasConfig`/`ResourcesUILoader`/`UILog`）
  - `Runtime/Component/`（`Panel` 基类，继承 RAA `AesirView<UIContext>`）
  - `Runtime/Odin Integration/`（`UICanvasConfigSO` + asmdef）+ `Editor/Odin Integration/`（AttributeProcessor + asmdef）
- **依赖回归 RAA**：新增依赖 `cn.runestone.aesir.architecture` 0.3.2（在 0.2.0 中曾被移除）；`Odin Inspector` 重新作为**可选**集成层（asmdef `defineConstraints: ODIN_INSPECTOR`，未导入时自动排除，SO 退化为普通 `ScriptableObject`）。
- **Canvas 结构**：由单 Canvas + 兄弟节点改为 1 个 Canvas + 4 层独立子 Canvas（`overrideSorting`），`SortOrder = 层级序号 × 100 + 配置偏移`。
- **API 变更**：移除 `RegisterPrefab`（统一走 `IUILoader`）；`Instance` 单例改为 `UIManager.Service`；新增 `SetCanvasConfig(IUICanvasConfig)`。

### Added

- `UIContext`：`AbstractContext<UIContext>`，在 `Configure` 中注册 `UIManager`。
- `IUICanvasConfig` 契约接口 + `UICanvasConfigSO`（Odin Integration 层，`CreateAssetMenu` 路径 `Aesir Modules/UI Canvas Config`）。
- `Panel.CloseSelf()`：面板内便捷关闭自身。
- PlayMode 单元测试（`Tests/Runtime/UIManagerTests.cs`）：覆盖 Open/Close/Back/pauseUnderneath。

### Removed

- `RegisterPrefab` / `AesirUIManager.Instance` 旧 API。
- 旧的 `Runtime/UI/` 平面目录。

## [0.2.0] - 2026-07-12

### Changed（重构 · 极简化为零依赖框架）

- **AesirUIManager** — 重写为场景单例：自动创建 Canvas + 四层全屏根节点（Background/Base/Popup/Top）；新增**导航栈**（`Open` 入栈 / `Back` 出栈）；加载通过 `IAesirUILoader` 解耦（默认 Resources 加载器）
- **AesirPanel** — 完善生命周期虚方法：`OnInit → OnShow → OnHide → OnClose`，实现 `IAesirPanel` 接口
- **AesirPanelConfig** — `layer` 默认 `Base`；`destroyOnHide` 重命名为 `recycleOnClose`
- **AesirUILayer** — 由三层（Background/Normal/Top）扩展为四层（Background/Base/Popup/Top），数值连续 0/1/2/3
- **API 变更**：`RegisterPanelPrefab`→`RegisterPrefab`、`ShowPanel`→`Open`、`HidePanel`→`Close`、`GetPanel`→`Get`，新增 `Back()` / `CloseAll()`
- **依赖精简**：移除对 `cn.runestone.aesir.architecture`（RAA）的依赖；移除 `Odin Inspector` 依赖，Inspector 表现改用原生 `[Header]/[Tooltip]` 特性
- **新增**：`Core/IAesirPanel.cs`、`Core/IAesirUILoader.cs`、`Loaders/AesirResourcesUILoader.cs`、`AesirUILog.cs`、`AesirUICanvasConfigSO.cs`（替代原 `AesirCanvasConfigSO` + Odin AttributeProcessor）
- **移除**：`AesirCanvasConfigSO`（被 `AesirUICanvasConfigSO` 取代）、`Editor/OdinIntegration`（Odin 集成，不再需要）

### Added

- 参考 QFramework UIKit / GameFramework UI / SUIFW 的设计，提供层级管理 + 导航栈 + 标准生命周期的极简组合
- 调研分析文档 `Documentation~/ui-framework-analysis.md`

## [0.1.0] - 2026-07-12

### Added

- **AesirUIManager** — UI 管理器单例，三层 Canvas（Background/Normal/Top）管理，Panel 注册/显示/隐藏/获取 API，Domain Reload 安全
- **AesirPanel** — Panel 抽象基类，继承 `AesirMonoBehaviour`，提供 Show/Hide 虚方法
- **AesirPanelConfig** — Panel 配置类（层级 + destroyOnHide）
- **AesirUILayer** — 面板层级枚举（Background=0/Normal=1/Top=2）
- **AesirCanvasConfigSO** — Canvas 统一配置 SO，继承 `AesirScriptableObject`，提供 ApplyToCanvas() 方法
- **AesirCanvasConfigSOAttributeProcessor** — Odin Inspector AttributeProcessor，为 CanvasConfigSO 注入分组和条件显示特性
- **EnsureAesirModulesDefine** — 自动注册 `AESIR_MODULES` 宏定义符号
- **单元测试** — AesirPanelConfig、AesirUILayer、AesirUIManager 面板流程测试
