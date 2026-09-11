# Changelog / 变更日志

> **本文件为 monorepo 聚合视图**——同时记录两个子包（Aesir Architecture / Aesir Modules）的版本变更。每个条目标注 `[architecture]` / `[modules]` 归属。
>
> 各自子包的**详细历史**（含 0.4.0 之前的版本）见：
> - Aesir Architecture: [`Assets/Runestone/AesirArchitecture/CHANGELOG.md`](./Assets/Runestone/AesirArchitecture/CHANGELOG.md)
> - Aesir Modules: [`Assets/Runestone/AesirModules/CHANGELOG.md`](./Assets/Runestone/AesirModules/CHANGELOG.md)
>
> Aesir Inspector 已迁移至独立公开仓库（面向 Odin Inspector 开发者的学习工具包），其变更日志不再随本仓库维护。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/)，
版本号遵循 [Semantic Versioning](https://semver.org/lang/zh-CN/)。

Format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
versions follow [Semantic Versioning](https://semver.org/).

---

## 当前版本 / Current Version

| 子包 / Sub-Package | 包名 / Package ID | 版本 / Version |
|---|---|---|
| Aesir Architecture | `cn.runestone.aesir.architecture` | **0.20.0** |
| Aesir Modules | `cn.runestone.aesir.modules` | **0.20.0** |

> **安装方式 / Installation**：本仓库作为单一 monorepo 发布，两个子包均通过 [UPM Git URL](https://github.com/yuumixcode/AesirFramework.git) 拉取（推荐固定版本分支 `#AesirArchitecture-v0.20.0` / `#AesirModules-v0.20.0`），按需选用。
> *The repository is published as a single monorepo. Both sub-packages are pulled via [UPM Git URL](https://github.com/yuumixcode/AesirFramework.git) (pinned version branches recommended) and used on demand.*
>
> **依赖关系 / Dependency**:
> - **Aesir Architecture** — 不依赖任何 Aesir 子包 / depends on no Aesir sub-package
> - **Aesir Modules** — 仅依赖 Aesir Architecture / depends on Aesir Architecture only

---

## [0.20.0] - 2026-09-11

---

### [architecture] Aesir Architecture


### Added

- **《设计变更记录》文档（`Documentation/设计变更记录.md`，`Documentation~` 镜像同步）** — 按设计点收录已废弃机制（Context 事件总线体系、ModelReplaced/ServiceReplaced、120 帧自愈轮询、初始化失败回滚、监听异常吞噬、BeforeFixedUpdate、GenericLocator.Global/GetRegistry、Interface→Instance 更名、DDOL 标志组合、fake-null 隐式重置）与设计来源（QFramework / Odin Inspector / Aesir Inspector / Cysharp.ObservableCollections），附命名演进速查表；源码注释此后只描述当前行为，历史演进统一收录于此

### Changed

- **注释精简（12 文件，净 −44 行）** — 移除源码注释中的废弃设计历史叙述（`AbstractContext.Instance` 的更名史、`AesirArchitecture.ResetStatics` 的 fake-null 演进史、`QuickCreateSOMenuItem` 的 Aesir Inspector 复刻细节、`ScriptingSymbolUtility` 的 Odin 来源、包内更新器的 QFramework PackageKit 对比段）；重复注释去重（4 个能力标记接口 remarks、4 个 Execute 扩展方法 remarks、`RemoveListenerOnSceneUnloadedTrigger` 单例说明、`AesirArchitecturePlayerLoop` 注入点标识、`AbstractContext.Dispose` 双重注释、`ObservableValue` 类级重复段）

### Fixed

- **`AesirArchitecture.DontDestroyOnLoad` 补 `new` 修饰符** — 显式隐藏 `UnityEngine.Object.DontDestroyOnLoad(Object)` 静态方法，消除 0.19.0 引入该属性时遗留的 CS0108 编译警告（行为不变）

---

### [modules] Aesir Modules


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

---

## [0.19.0] - 2026-09-11

### [architecture] Aesir Architecture

#### Added

- **`IContext` / `AbstractContext<T>` 新增 `UnregisterModel<TModel>` / `UnregisterService<TService>`** —— 按类型键摘除注册并释放被摘除实例（订阅不迁移），与动态替换语义配套；未注册时静默无操作（幂等），注销后再次注册按新插入语义追加到注册顺序末尾
- **`AesirArchitecture` 新增只读属性 `DontDestroyOnLoad`** —— 对外暴露 DDOL 决策的当前取值，供运行时查询与编辑器条件提示（Odin 可见性表达式）复用

#### Fixed

- **补齐 `AesirArchitectureAttributeProcessor` 宣称的类级条件 Warning 信息框** —— XML 文档宣称"DDOL 开关关闭时显示的类级 Warning"，此前实现只有两个 Info 框（宣称与实现不符）；现以 `@!$value.DontDestroyOnLoad` 表达式条件注入（仅关闭时显示，与运行时提醒日志口径一致）
- **`RemoveListenerOnSceneUnloadedTriggerAttributeProcessor` 的宿主 DDOL 警告改为条件显示** —— 此前无条件常显，默认配置（DDOL 开启）下构成高严重度噪音；现仅在宿主存在且关闭时显示（经 `GetComponent` 读宿主只读属性；组件误放至非宿主物体时不显示）
- **`AbstractSubmodule.Dispose` 现重置 `Initialized` 为 false** —— 已释放的模块不再自称已初始化，消除动态替换场景下旧实例初始化状态的误导
- **`RemoveListenerOnSceneUnloadedTrigger.Awake` 现写入静态单例缓存** —— 与 `AesirArchitecture` / `MonoLifecycleProxy` 单例范式同形；预放置双实例在各自 Awake 期完成判重，修复"两个从未经 `Instance` 访问的预放置实例双双订阅 `sceneUnloaded`"的窗口（`Instance` getter 的 `FindAnyObjectByType` 保留为销毁后重发现与执行顺序竞态的兜底）

#### Changed

- **示例：Counter-Mvc-Quick / Counter-Mvp-Quick 的 Model 改为只读属性暴露可写 ObservableValue** —— 公开可变字段 `count` 收敛为私有 `[SerializeField]` 字段 + 只读属性 `Count`（写入路径 `Count.Value++` 不变，快捷档零封装语义不受影响；序列化名不变，既有场景/预制体数据兼容），防外部整体替换 ObservableValue 实例导致订阅悬空
- **示例：Counter-Mvc-Strict / Counter-Mvp-Strict 查询改缓存实例复用** —— Controller / Presenter 缓存 Query 实例经带实例重载执行（零分配），替代此前每次调用无参重载（每次分配一个查询实例）；严格档 View 注释同步改写为"复用查询实例"教学口径
- **README（中英）：MiniEvent"零分配事件"项目结构行补"（Invoke 路径）"限定** —— 0.18.0 修正类文档同款失实时漏网的两处
- **README（中英）项目结构补列 `ObservableCollections` 与 `RuntimeInitializeLoadType` 示例目录** —— 此前树中缺失

---

### [modules] Aesir Modules

#### Changed

- 版本号与 Aesir Architecture 同步更新至 `0.19.0`，本包本版本无功能性变更

---

## [0.18.0] - 2026-09-10

---

### [architecture] Aesir Architecture

#### Added

- **`AesirArchitecturePlayerLoop.Register` 返回 `AutoRemoveListenerHandle`** — Dispose 时自动注销本次注册，对齐全框架监听句柄风格；忽略返回值的既有调用不受影响
- **`CapabilityExtensionsTests`（10 用例）** — CQRS 执行链首次获得测试覆盖：带参/无参命令与查询、命令链、查询组合、`IController<T>` 默认接口实现绑定、Model 初始化阶段获取 Service 的两阶段约束异常语义、Dispose 后单例重建

#### Fixed

- **修复 Model 初始化阶段获取 Service 的误导性报错** — `CapabilityExtensions.GetService` 原报错指示"调整注册顺序"，但框架按「先全部 Model、后全部 Service」两阶段初始化，Model 的 `OnInitialize` 阶段无论注册顺序如何都无法获取 Service；现明确提示须延迟到运行期方法调用中获取
- **修复 `AbstractContext<T>.Dispose` 后的"僵尸单例"** — 释放后解除 `Instance` 单例缓存，再次访问将按懒加载语义重建并重新初始化全新上下文，而非返回容器已清空的空壳（原先此时 `GetModel`/`GetService` 会抛出指向性错误的"未注册"异常）
- **修正三处 XML 文档失实** — `AesirArchitecture` 组件宣称"使架构基础设施先于业务逻辑完成初始化"（实际不初始化任何架构数据，纯 C# Context 懒加载，组件仅为 DDOL 宿主）；`AesirArchitecturePlayerLoop` 残留已废弃的"MonoLifecycleProxy 运行期间周期性检测"宣称（120 帧自愈轮询 0.9.0 已移除）；`MiniEvent`"零分配的监听管理"（仅 Invoke 路径零分配，订阅路径有多播合并与句柄闭包分配）——`ObservableValue` / `ObservableList` / `ObservableDictionary` / `ObservableHashSet` 类文档同步修正
- `IView` 文档口径修正——"一切写操作经 Command"是严格档编写约定而非接口强制，接口层仅封命令执行入口
- `ICustomLifecycle.cs` 文件级文档错挂到 `ICustomFixedUpdate`（双 `<summary>` 的 XML 产物错乱），已并入正确的接口文档
- `RemoveListenerOnSceneUnloadedTrigger` 补重复实例守卫（原先重复实例会重复订阅 `sceneUnloaded` 造成分桶分裂）
- `ObservableList.AddRange` 传入 null 现抛 `ArgumentNullException`（对齐 BCL 行为，原为 NullReferenceException）

#### Changed

- **解除 Editor 工具对测试框架的结构绑定** — Editor 主程序集移除 `UNITY_INCLUDE_TESTS` defineConstraint、package.json 移除 `com.unity.test-framework` 硬依赖：消费者移除测试框架时宏确保器 / 包内更新器等编辑器工具不再整程序集静默消失，生产项目也不再被强塞测试框架
- **PlayMode 测试程序集以 `ODIN_INSPECTOR` 守卫** — `Tests/Runtime` 引用 Sirenix 预编译 DLL（Odin 序列化基类链所需），无 Odin 的消费环境自动排除该程序集，修复与"Odin 可选"宣称矛盾导致的导入编译错误
- `MonoLifecycleProxyExtensions.RegisterCustomLifecycle(mono / GameObject, evt, callback)` 现将监听绑定到所在 GameObject 的销毁事件自动移除——与无参重载语义对齐，消除"同名重载生命周期语义分裂"陷阱（行为变更：原先监听不会随物体销毁自动移除）
- `AesirArchitecture` / `MonoLifecycleProxy` 重复实例改用 `Destroy(this)`——不再连带销毁用户预放置组件的业务物体
- 私有 `Reset` 更名 `ClearState`——避免撞名 Unity 魔法方法导致编辑器 Inspector Reset 回调误触（`MonoLifecycleProxy` / `RemoveListenerOnSceneUnloadedTrigger`）
- `AbstractQuery` 补 `[Serializable]`（与 `AbstractCommand` 对齐）
- 严格档示例 `UpdateCountText` 收敛为 private 并补"勿在通知回调热路径反复 Query"注释

---

### [modules] Aesir Modules

#### Added

- **ScriptDocGenerator 模块（需 Odin）** — 原 `Assets/ScriptDocGenerator` 独立工具整合为 Aesir Modules 功能模块：反射分析 C# 类型生成结构化 API 文档（增量保留手写内容），附 Summary 工具（XML `<summary>` ↔ `[Summary]` 双向同步）。命名空间 `Runestone.AesirModules.ScriptDocGenerator`(.Editor)，代码经 asmref 汇入 Odin 程序集；入口 `Tools → Aesir → Script Doc Generator`

## [0.17.0] - 2026-09-06

---

### [architecture] Aesir Architecture

#### Added

- **`InternalContextAttribute`** — 标记框架内部 Context（示例 / 测试等非用户工作流用途）的元数据特性；AesirModules Binder 的「Context 类型」选择器会跳过被标记的类型
- 包内全部示例与测试 Context（含嵌套）标注 `[InternalContext]`

#### Fixed

- **示例场景无法运行（0.14.0 起回归）** — 示例程序集从 Editor-only（`includePlatforms`）改为运行时程序集 + 示例脚本整文件 `#if UNITY_EDITOR` 包裹：Editor-only asmdef 的 MonoBehaviour 被 Unity 判定为"编辑器脚本"、禁止挂载场景物体，导致全部示例场景组件 Missing Script；重构后编辑器内正常编译、挂载与 Play Mode 运行，玩家构建仍整体剔除（示例类型 0 入包）

---

### [modules] Aesir Modules

#### Added

- **Binder 组件绑定全面完善** — 双生成模式（「同一脚本增量」默认 / 「Partial 分部类」后缀可选、默认 `.designer.cs`）；「Context 类型」下拉扫描 AbstractContext 派生类并自动排除 `[InternalContext]`；基类下拉新增 `AesirBasePanelView<T>` / `AesirBasePanelViewController<T>` 预选（新增 `AesirBasePanelViewController<T>`）；层级右键菜单快捷挂载 `BinderAssistant` / `BinderTag`；生成代码 TitleGroup 分组、4 空格缩进、全限定自包含；命名空间默认值与后缀候选经 ScriptableSingleton 持久化；新增 EditMode 测试程序集 `Runestone.AesirModules.Tests`（69 个用例）

#### Fixed

- Binder 生成脚本 `IComponentBinder` 接口不匹配导致的编译错误；Partial 模式重新生成覆盖手写 controller 文件的问题；绑定校验空引用 / 层级路径越界；EditorPrefs 键规范与自动挂载的跨 asmdef 类型解析
- **示例场景无法运行（0.14.0 起回归）** — 同 Architecture 机制：`Events/01_KeyPress` 示例程序集改为运行时程序集 + 整文件 `#if UNITY_EDITOR`，修复 Editor-only asmdef 禁止挂载导致的 Missing Script；玩家构建整体剔除

---

## [0.16.2] - 2026-09-06

---

### [architecture] Aesir Architecture

#### Changed

- **`LICENSE.md` 与 `Third Party Notices.md` 移至包根** — 对齐 UPM 包根约定（Package Manager 识别包根的 Third Party Notices），包 README 的 LICENSE 徽章链接随之生效；版权行统一为 Runestone Yuumix

---

### [modules] Aesir Modules

#### Changed

- **`LICENSE.md` 与 `Third Party Notices.md` 移至包根** — 同 Architecture；`Documentation~/` 镜像不再存放两文件的副本

---

## [0.16.1] - 2026-09-06

---

### [architecture] Aesir Architecture

#### Changed

- **Third Party Notices 收录 ObservableCollections（Cysharp，MIT）** — 设计参考条目：可观察集合高级能力（Move / Sort / 同步视图 / R3 集成）的推荐替代库；注明仅设计参考、未包含源码
- 根 README（中英）推荐链接新增 ObservableCollections

---

### [modules] Aesir Modules

#### Changed

- **新增 `Documentation/Third Party Notices.md`** — 收录 Eflatun.SceneReference（MIT）设计参考条目（Scene 模块 `SceneAssetWrapper` 吸收其功能设计）；注明仅设计参考、未包含源码
- **README（中英）Scene 模块 `SceneAssetWrapper` 条目补注功能设计参考来源**
- 根 README（中英）推荐链接新增 Eflatun.SceneReference

---

## [0.16.0] - 2026-09-06

---

### [architecture] Aesir Architecture

#### Changed

- 版本号与 Aesir Modules 同步更新至 `0.16.0`，本包本版本无功能性变更

---

### [modules] Aesir Modules

#### Added

- **`SceneAssetWrapper` 功能增强（吸收 Eflatun.SceneReference）** — GUID 锚点自愈（`sceneGuid` 序列化字段 + `EditorSyncFromAsset`）、`State` / `UnsafeReason` 状态机校验、`TryGet` 安全读取家族、`FromScenePath` / `FromAsset`（编辑器）工厂方法、`Address` 序列化缓存与 `AddressablesSupportEnabled`
- **Addressables 条件架构** — 核心 asmdef `versionDefines` 定义 `AESIR_MODULES_ADDRESSABLES`；独立胶水程序集 `Runestone.AesirModules.Editor.Addressables`（未安装 Addressables 时整体不编译，零报错），经 `SceneAssetWrapperAddressablesBridge` 静态委托桥注册地址查询与加入默认组能力
- **Scene 模块测试程序集 `Runestone.AesirModules.Scene.Tests`** — 27 个 EditMode 用例，自适应装 / 未装 Addressables 环境

#### Changed（破坏性变更）

- **删除 `AddScene` / `UnloadAddedScene`**（含 `*WithSceneAssetWrapper` 变体）— 统一为 `LoadSceneAdditive` 纯叠加追踪：不再自动卸载上个场景、不再抢占激活场景
- **`ReloadScene` 同步改异步** — 带 `onCompleted` / `onFailed` 回调
- **加载失败新增 `onFailed` 回调** — `LoadSceneSingle` / `LoadSceneAdditive` / `UnloadScene` / `ReloadScene` 全系支持
- **`SceneAssetWrapper` 空引用语义收紧** — `ScenePath` / `Guid` / `SceneName` / `BuildIndex` / `LoadedScene` 空引用由返回空值改为抛 `EmptySceneAssetWrapperException`
- **`*WithSceneAssetWrapper` 6 个方法改为同名重载**

#### Changed（包结构重组）

- **目录调整为标准 Unity 自定义包根结构** — 包根两级目录 `Runtime/` 与 `Editor/`，功能模块（UI / Scene / Events）以子目录存在于对应层级，模块间零依赖，删除模块 = 删除对应层的模块子目录；核心程序集锚点在层根（模块主代码自动汇入），Odin / Addressables 细分程序集锚点收拢至 `Common/` 下，模块专属代码经 4 个 asmref 汇入；程序集名称与公共 API 不变

---

## [0.15.0] - 2026-09-05

---

### [architecture] Aesir Architecture

#### Added

- **可观察集合 `ObservableHashSet<T>`** — 与 List / Dictionary 同构：组合 `HashSet<T>` 存储 + `MiniEvent` 零分配事件；事件负载单值直传 `Action<T>`（对齐 Dictionary 的 KeyValuePair 直传先例）；只读接口 `IReadOnlyObservableHashSet<T>` 为不变型；新增 ObservableHashSetTests 14 个
- **RuntimeInitializeLoadType 示例** — 编辑器窗口演示 `RuntimeInitializeLoadType` 五个初始化时机的执行顺序与静态重置最佳实践；菜单归位 `Tools/Aesir/Architecture/Samples/RuntimeInitializeLoadType`；ScriptableSingleton 落盘统一 `ScriptableSingleton/` 前缀

#### Changed

- **包内更新器大陆优化（jsDelivr 方案）** — 版本检测三级兜底：jsDelivr 四域名拉取仓库内 `.github/update-info.json`（CI 发版后以 `[skip ci]` 提交回 main）→ GitHub Releases API → `releases/latest` 302 重定向探测；unitypackage 下载仍走 GitHub Release 直链
- **CI** — `build_unitypackage.py` 的 files-manifest 升级为 update-info.json（顶层 version/tag）；`auto-release.yml` 发布后回写 main
- **ObservableDictionary.Remove(TKey) 优化** — 改用 `Dictionary.Remove(key, out TValue)` 单次哈希查找取回旧值
- **Odin 编辑器处理器命名空间对齐** — `Editor.OdinIntegration` → `Editor.OdinInspector`（与 0.9.0 程序集/目录改名对齐）
- **文档核查整理** — 两包中英 README 修正过时描述并补更新器/unitypackage 安装说明；AesirModules README 从 UIManager 旧架构重写为 UIModule 架构；Skill core-api.md `Interface` → `Instance`；CONTRIBUTING 单例规范更正

#### Fixed

- **PlaneWar 场景引用修复工具** — 原硬编码 `Assets/Samples/.../0.12.0` 路径失效，改为 `FindAssets` 动态定位（兼容包内 `Samples/` 与 UPM 导入副本两种布局）

---

### [modules] Aesir Modules

#### Changed

- 版本号与 Aesir Architecture 同步更新至 `0.15.0`，本包无功能性变更；中英 README 随全项目文档核查刷新至 UIModule 架构

---

### [repo] 仓库级

#### Changed

- **根 README 重构（中英）** — 新增 Aesir Modules 对等章节、文档地图、质量与 CI；安装方式扩为 4 种（新增 unitypackage 导入 + 包内更新器）
- **CONTRIBUTING 修正** — OdinIntegration 旧命名更正；MonoBehaviour 单例规范更新为预放置优先 + `dontDestroyOnLoad` 序列化字段

#### Removed

- **仓库本地 AesirPackageInstaller**（`Assets/Scripts/Editor/`）— 硬编码版本常量需发版手动同步，本地复制安装路线已被 unitypackage Release 管线取代；版本/安装检查统一为包内更新器

---

## [0.14.0] - 2026-09-05

---

### [architecture] Aesir Architecture

#### Changed

- **AesirFramework 转型** — AesirInspector 已从本仓库移出，作为面向 Odin Inspector 开发者的学习工具包独立发布；本仓库更名定位为 AesirFramework，由 Aesir Architecture 与 Aesir Modules 组成
- **Samples 双目录结构** — 包内同时提供 `Samples/`（仓库内直接可见可运行）与 `Samples~/`（Git URL 安装后经 Package Manager 按需导入的源镜像）；以 `Samples/` 为编写主位，内容同步至 `Samples~`，两份内容保持一致
- **示例程序集构建剔除** — 全部示例程序集（asmdef）`includePlatforms` 收窄为 `Editor`，示例脚本与资产（场景/预制体/贴图均无 Resources 目录、无构建场景引用）不会进入玩家构建包
- **示例命名空间统一** — 计数器六档与 PlaneWar 示例统一为 `Runestone.AesirArchitecture.Samples.<示例名>`（MvcQuick / MvcStandard / MvcStrict / MvpQuick / MvpStandard / MvpStrict / PlaneWarMono）；`MiniEvent` 与 `ObservableValue` 两示例因命名空间段与所演示的框架类型同名冲突（CS0118），保留 `Runestone.AesirArchitecture.Samples` 前缀命名空间

#### Added

- **PlaneWar 实战示例（Mono 版）** — 纵版射击飞机大战：自包含素材、得分 HUD、三型敌机与重开流程；已注册进 package.json samples，并纳入 Samples 双目录同步

---

### [modules] Aesir Modules

#### Changed

- **AesirFramework 转型同步** — 依赖声明 `cn.runestone.aesir.architecture` 同步至 `0.14.0`
- **Samples 双目录结构** — 包内同时提供 `Samples/` 与 `Samples~/`；示例目录统一为 `Events/01_KeyPress`（与 package.json samples 路径一致），示例代码随最新版本更新（EventEmitter → EventSender、OnKeyPressed → KeyPressedEvent）并以 `Samples/` 为编写主位同步至 `Samples~`
- **示例程序集构建剔除 + 命名空间统一** — 示例程序集 `includePlatforms` 收窄为 `Editor`（不进入构建包）；命名空间统一为 `Runestone.AesirModules.Samples.Events.KeyPress`

---

## [0.13.0] - 2026-09-03

---

### [architecture] Aesir Architecture

#### Added

- **可观察集合 `ObservableList<T>` / `ObservableDictionary<TKey, TValue>`** — 组合 `List<T>` / `Dictionary<TKey, TValue>` 存储 + `MiniEvent` 零分配事件（与 ObservableValue 同一套读写分离与事件模式）；List 事件 Added / Removed / Replaced / Cleared，Dictionary 事件 Added / Removed / Updated / Cleared；readonly struct 事件参数（`CollectionAddEventArgs` 等）；只读接口 `IReadOnlyObservableList<T>` / `IReadOnlyObservableDictionary<TKey, TValue>` 为不变型（结构体事件参数与协变冲突）；监听 API 返回 `AutoRemoveListenerHandle` 自动清理；新增测试 25 个

#### Changed

- **Context 动态替换模块输出 Warning** — `RegisterModel` / `RegisterService` 键命中已有实例时输出一条 Warning（提醒旧实例 Dispose 后事件订阅不迁移）；首次注册不输出，替换仍为合法操作
- **README 中英文同步** — 新增"可观察集合"特性条目与"集合可观察全家桶不做"设计边界（高级能力推荐 Cysharp.ObservableCollections）

---

### [modules] Aesir Modules

#### Changed

- **UIRoot 序列化引用重构** — 层 Canvas / UICamera / EventSystem 序列化引用持久化（`List<LayerCanvasEntry>` 替代字典），存在性判定只依赖引用非空，子物体重命名不再破坏引用；旧版层级按约定名一次性回收；`PresetLayers` 静态缓存避免 `Enum.GetValues` 装箱
- **Input System 集成目录迁移** — `Runtime/InputSystem/` → `Runtime/UI/InputSystem/`

---

### [inspector] Aesir Inspector

#### Changed

- 版本号与 Aesir Architecture / Aesir Modules 同步更新至 `0.13.0`，本包本版本无功能性变更

---

## [0.12.0] - 2026-08-22

---

### [architecture] Aesir Architecture

#### Added

- **AI Skill 文档集** — `Documentation~/AesirArchitecture-Skill/` 新增 AI 编码指南，供 AI 助手按 RAA 架构规范快速生成代码：SKILL.md 主指南（MVC/MVP 模式决策树、Quick/Standard/Strict 三档分级表、核心 API 速查、事件机制决策表、10 条关键约定、命名规范与文件组织指南）+ 6 份三档完整代码模板（MVC/MVP 各三档）+ core-api.md 辅助模块 API 速查

---

### [modules] Aesir Modules

#### Changed

- 版本号与 Aesir Architecture / Aesir Inspector 同步更新至 `0.12.0`，本包本版本无功能性变更

---

### [inspector] Aesir Inspector

#### Changed

- 版本号与 Aesir Architecture / Aesir Modules 同步更新至 `0.12.0`，本包本版本无功能性变更

---

## [0.11.0] - 2026-08-22

---

### [architecture] Aesir Architecture

#### Breaking Changes

- **DDOL 机制重设计：预放置/运行时创建统一由 `dontDestroyOnLoad` 序列化字段控制** — 默认勾选时（含场景预放置实例）一律加入 DontDestroyOnLoad 场景（此前预放置实例保留在场景中）；取消勾选时随所在场景卸载销毁，须自行处理多场景叠加（Additive）加载。移除 `_pendingDontDestroyOnLoad` 静态标志与 `_isPrePlaced`
- **MVP 快捷档更名：`Counter-Mvp-Simple` → `Counter-Mvp-Quick`** — 档位命名与 MVC 对齐（简单档 → 快捷档）；类名 / asmdef / 场景 / 预制体同步更名（GUID 不变）
- **MVP View 统一为纯 `MonoBehaviour`** — 三档 View 不再继承 `MonoView<T>`（被动视图不携带 Context 能力）；快捷档零接口抽象（移除 `ISampleMvpSimpleCounterView`）

#### Added

- **示例家族从 4 个扩为 6 个渐进档位**（MVC / MVP 各三档）— 新设 `Counter-Mvc-Standard`（只读暴露 + 写方法，View/Controller 分离共享 Model）与 `Counter-Mvp-Standard`（Presenter 直调写方法 + Model 直读推送）；MVC-1 移除 Model 接口对齐零接口抽象
- **MVC-3 严格档重设计** — Controller 抽为纯 C# 类 + 业务窄接口 `ISampleMvcStrictCounterController`（独立文件，不继承 `IController`）；View 按接口持有 Model 订阅刷新（撤销"View 零持有"旧口径）；Query 收窄为加工值场景（`GetRoundedCountQuery` 替代 `GetCounterValueQuery`）
- **严格档双接口设计** — MVC-3 / MVP-3 View 按业务窄接口存储 Controller / Presenter，类型层面拿不到 `ExecuteCommand` / `GetModel` 等框架能力，读写分离由类型系统闭环
- **DDOL 开关字段级 InfoBox** — `[Tooltip]` 迁移为 AttributeProcessor 注入的 Info 级信息框（样式与逻辑分离，运行时程序集零 Inspector 样式特性）

#### Fixed

- **DDOL 警告 InfoBox 可见性反转** — Odin `visibleIfMemberName` 对 bool 成员为"true 时显示"语义，旧写法导致警告在开关开启时显示、关闭时静默；改用 `"@!" + 字段名` 表达式反转（探针验证 8/8 PASS）

#### Changed

- **README 新增「示例（Samples）」总览节**（8 示例双三档对照表）；移除「与 QFramework 的差异」对比章节；英文 README 同步并修正安装 URL `?path=` 参数
- **package.json samples 六档条目**；《事件机制决策表》MVC-3 口径修正（原始值订阅刷新，Query 仅加工值）
- **全包 XML 注释与代码格式统一**

---

### [modules] Aesir Modules

#### Breaking Changes

- **DDOL 机制重设计：预放置/运行时创建统一由 `dontDestroyOnLoad` 序列化字段控制** — `AesirModules` / `UIRoot` / `UIModule` 新增 `[SerializeField] bool dontDestroyOnLoad = true`（默认加入 DontDestroyOnLoad 场景，此前预放置实例保留在场景中）；取消勾选时随所在场景卸载销毁，须自行处理多场景叠加（Additive）加载。`UIModule` 字段仅预放置为根物体时生效（运行时创建挂载于 [Aesir Modules] 宿主下跟随宿主决策）；移除 `AesirModules` / `UIRoot` 的 `_pendingDontDestroyOnLoad` 静态标志

#### Added

- **DDOL 开关字段级 InfoBox + 警告可见性修复** — 新增 `AesirModulesAttributeProcessor` / `UIModuleAttributeProcessor`、扩展 `UIRootAttributeProcessor`：字段级 Info 信息框（替代运行时 `[Tooltip]`，样式与逻辑分离）+ 类级警告框改用 `"@!" + 字段名` 反转表达式（修复 Odin VisibleIf 语义导致的可见性反转）

#### Changed

- **依赖版本同步** — Architecture 依赖版本号更新至 `0.11.0`；全包 XML 注释与 asmdef 缩进格式统一

---

### [inspector] Aesir Inspector

#### Changed

- 版本号与 Aesir Architecture / Aesir Modules 同步更新至 `0.11.0`，本包本版本无功能性变更

---

## [0.10.0] - 2026-08-19

---

### [architecture] Aesir Architecture

#### Breaking Changes

- **`AbstractContext<T>.Interface` 更名 `Instance`，返回类型 `IContext` → `T`** — 消除与 C# 关键字混淆；Context 子类自定义成员免强转。迁移：全局替换 `.Interface` → `.Instance`

#### Fixed（框架运行时一致性）

- **`AesirArchitecture` 根单例补类内静态重置**（此前依赖 fake-null 隐式救援）
- **`GenericLocator<T>` 保序 + `AbstractContext.Dispose` 真逆序**（此前 Dispose 正序遍历，与注释"逆序"矛盾）
- **未注册异常近失识别**（实现类注册/接口查询时提示类型参数一致）
- **`IModel` 注释纠错**（去除误述的 GetService 能力）

#### Fixed（示例与框架承诺对齐）

- **MVP 被动视图接口去 `IView`**（"View 不访问 Model"从接口层面落实）
- **MVP 事件化**（`Action` 属性 → `event`，外部不可替换/置空/触发委托链）
- **按钮监听精确配对**（`RemoveAllListeners` → 对称 `RemoveListener`）
- **MVP 标准档写入改走 Command**（与 MVC 共享"表现层写入必经 Command"铁律）
- **场景初始值同步**（`AddListenerAndInvoke` / `SyncInitialValue`）

#### Added（渐进式示例家族）

- **示例从 2 个扩为 6 个渐进档位**：`Counter-Mvc-Quick`（快捷）/ `Counter-MVC`（标准）/ `Counter-Mvc-Strict`（严格）/ `Counter-Mvp-Simple`（简单）/ `Counter-MVP`（标准）/ `Counter-Mvp-Strict`（严格）
- **Model 暴露面分档**（通常档可写 ObservableValue / 严格档只读接口 + 写方法；全家族统一 `[SerializeField]` 字段 + 属性转发形式）
- **7 个新增 EditMode 测试**（测试总数 34 → 41）
- **《事件机制决策表》《常见陷阱清单》**（Documentation~/）

#### Changed（文档）

- **README 快速开始对齐真实示例**（MonoView + 无参 Controller；Model 可写 ObservableValue）
- **写入约定三档口径**（设计边界表新增写入纪律档位）
- **三档渐进路径**（快捷 → 标准 → 严格三课）
- **设计原则第 7/8 条**（Inspector 精简原则 AI 优先 / Odin 三条铁律）
- **英文 README 同步**

## [0.9.0] - 2026-08-15

---

### [architecture] Aesir Architecture

#### Changed

- **MVC 优先定位** — package.json/README 从「MVP/MVC」改为「MVC」，IController 为推荐入口，IPresenter 为可选严格模式
- **目录重构** — Runtime/ 从 Component/ + Engine/ 两级扁平改为三层：Core/（Context + MVC/MVP 核心）、Modules/（Event/CustomLifecycle/Locator/Observable/Utilities 辅助模块）、Common/（框架基础设施）
- **极简化** — MiniEvent 恢复零分配直调，撤销异常吞噬（统一原生 C# fail-fast 语义）；Interface 改为初始化成功后才赋值，失败不缓存、不做回滚；移除 120 帧自愈轮询（保留 EnsureInjected + Register 期检测）；移除 ModelReplaced/ServiceReplaced 替换通知；移除 GenericLocator.Global 与 GetRegistry()
- **Odin 程序集重命名** — OdinIntegration → OdinInspector（三包统一）
- **场景卸载分桶改 Scene.handle** — 消除同名场景误清；新增 RemoveListenerExtensions 显式归桶重载
- **GetModel/GetService 未注册时抛 InvalidOperationException** — 不再返回 null
- **静态变量重置职责拆分** — 非泛型单例类内 RIOLM，泛型经 ResetStaticsAssistant

---

### [modules] Aesir Modules

#### Changed

- **Odin 程序集重命名** — OdinIntegration → OdinInspector（三包统一）
- **依赖版本同步** — Architecture 依赖版本号更新至 0.9.0

#### Fixed

- **UI 模块缺陷修复** — InstantiateInactive 停用态实例化、字典键归一化、AesirBasePanel.OnDestroy 静态反清理、EventSystem 全场景检查、Build 统一走 EnsureCanvasConfig、GetLayerRoot 缺层 LogError、内部状态异常补 Error 日志、ShowPanel/Show 泛型重载

---

### [inspector] Aesir Inspector

#### Changed

- **Odin 程序集重命名** — OdinIntegration → OdinInspector（三包统一）：Runtime/Editor/Tests 三处目录 + 6 个引用方 asmdef 同步更新
- **文档同步** — aesir-inspector.md 程序集表、development.md 依赖图更新

---

## [0.8.0] - 2026-08-06

---

### [architecture] Aesir Architecture

#### Fixed

- **MonoLifecycleProxy 排序 Bug 修复** — `InvokeEvent` 原先排序 `_sortedListeners` 后仍调用 `MiniEvent.Invoke()`（按注册顺序），导致 `order` 参数无效。改为直接遍历 `_sortedListeners` 按排序结果调用回调。移除 `MiniEvent` 死代码（`_events` 字典、`GetOrCreateEvent` 方法）

#### Removed

- **移除 `BeforeFixedUpdate` 事件** — 该事件通过 PlayerLoop 每帧触发，但 `FixedUpdate` 并非每帧执行，语义误导且无实际使用。涉及删除 `MonoLifecycleEvent.BeforeFixedUpdate` 枚举值、`AesirArchitectureLifecyclePhase.BeforeFixedUpdate` 枚举值、PlayerLoop 注入逻辑、`ICustomBeforeFixedUpdate` 接口、`MonoLifecycleProxy` 中对应的注册/注销分支

#### Changed

- **`AesirArchitectureLifeCyclePhase` → `AesirArchitectureLifecyclePhase`** — 统一拼写为 Lifecycle（一个单词）
- **`FindFirstObjectByType` → `FindAnyObjectByType`** — 后者不依赖 InstanceID 排序，在 Unity 6 中向前兼容
- **`ClearAllListeners` 不再注销 PlayerLoop** — PlayerLoop 注销移至 `OnDestroy`，避免测试间 PlayerLoop 注册丢失
- **新增 `MonoLifecycleProxyTests`** — PlayMode 测试覆盖订阅、排序、稳定排序、句柄取消订阅、监听者数量、全帧级事件顺序

---

### [modules] Aesir Modules

#### Changed

- **`FindFirstObjectByType` → `FindAnyObjectByType`** — 所有 MonoBehaviour 单例（`AesirModules`、`UIRoot`、`UIModule`、`EventModule`、`SceneModule`）的 `Instance` getter 改用 `FindAnyObjectByType`，后者不依赖 InstanceID 排序，在 Unity 6 中向前兼容

---

### [inspector] Aesir Inspector

#### Changed

- 版本号与 Aesir Architecture / Aesir Modules 同步更新至 `0.8.0`，本包本版本无功能性变更

---

## [0.7.0] - 2026-08-05

---

### [architecture] Aesir Architecture

#### Changed

- **单例模式重构：预放置优先** — 所有 MonoBehaviour 单例（`AesirArchitecture`、`MonoLifecycleProxy`、`RemoveListenerOnSceneUnloadedTrigger`）的 `Instance` getter 优先通过 `FindAnyObjectByType` 搜索已加载场景中预放置的实例，未找到时才运行时创建
- **条件式 DontDestroyOnLoad** — `AesirArchitecture` 新增 `static bool _createdByRuntime` 标志，仅运行时创建的实例调用 `DontDestroyOnLoad`，场景中预放置的实例保留在场景中随场景生命周期销毁
- **移除 Bootstrap 自动初始化** — 移除 `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)] Bootstrap()` 方法，避免在场景加载前创建 DDOL 实例抢占预放置实例

#### Removed

- `AesirArchitecture.Bootstrap()` 方法及其 `[RuntimeInitializeOnLoadMethod]` 特性

---

### [modules] Aesir Modules

#### Changed

- **单例模式重构：预放置优先** — 所有 MonoBehaviour 单例（`AesirModules`、`UIRoot`、`UIModule`、`EventModule`、`SceneModule`）的 `Instance` getter 优先通过 `FindAnyObjectByType` 搜索已加载场景中预放置的实例，未找到时才运行时创建
- **条件式 DontDestroyOnLoad** — `AesirModules` 和 `UIRoot` 新增 `static bool _createdByRuntime` 标志，仅运行时创建的实例调用 `DontDestroyOnLoad`，场景中预放置的实例保留在场景中随场景生命周期销毁
- **移除 Bootstrap 自动初始化** — 移除 `AesirModules` 的 `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)] Bootstrap()` 方法

#### Removed

- `AesirModules.Bootstrap()` 方法及其 `[RuntimeInitializeOnLoadMethod]` 特性

---

### [inspector] Aesir Inspector

#### Changed

- 版本号与 Aesir Architecture / Aesir Modules 同步更新至 `0.7.0`，本包本版本无功能性变更

---

## [0.6.0] - 2026-08-05

---

### [architecture] Aesir Architecture

#### Added

- **MonoLifecycleProxy 生命周期代理** — 全局单例组件，将 Unity 原生生命周期回调和自定义 PlayerLoop 阶段统一为可订阅的 MiniEvent
- **ICustomXXX 生命周期接口** — 8 个接口，实现后通过 `RegisterAuto()` 自动注册到匹配事件
- **MonoLifecycleProxyExtensions 扩展方法** — 支持 MonoBehaviour / GameObject / object 的 `AddListener` / `RemoveListener` / `RegisterLifecycle` 扩展方法
- **全包 XML 文档注释增强** — 69 个 .cs 文件补充完整详细的 XML 文档注释

#### Changed

- **MonoLifecycleEvent 移除低实用性事件** — 移除 Awake / OnEnable / OnDisable / OnDestroy，新增 OnApplicationFocus / OnApplicationPause
- **Samples 目录版本对齐** — `Assets/Samples/Aesir Architecture/0.5.0/` → `0.6.0/`

---

### [modules] Aesir Modules

#### Changed

- 版本号与 Aesir Architecture / Aesir Inspector 同步更新至 `0.6.0`，本包本版本无功能性变更

---

### [inspector] Aesir Inspector

#### Added

- **源代码文件查找与内容缓存**：新增 `SourceFileEntry` 数据容器，支持缓存避免重复读取
- **块注释内的假 XML 注释过滤**：逐行跟踪 `/* */` 块注释状态，块注释内的 `///` 不被误判
- **跨程序集同名类型区分**：summary 缓存键加入程序集名前缀
- **重载方法 summary 区分**：方法 summary 键附加参数类型列表，支持多行声明参数跨行
- **嵌套类型和泛型类型 summary 解析**：不再错误返回外层类的 summary
- **文件名与类型名不匹配时的源文件查找**：通过全局内容扫描找到源文件
- **多程序集批量分析模式**：`TypeSource` 枚举新增 `MultipleAssemblies` 模式
- **反射解析器迁移至 Runtime/Unity**：使 `[Summary]` 和 `[ReferenceLinkURL]` 特性不再依赖 `ODIN_INSPECTOR` 程序集约束
- **源码解析单元测试**：新增 38 个测试（SourceParsingTests 34 + OverloadPrefixTests 4），总计 107 个测试全部通过

#### Changed

- **移除 OdinBridge 桥接层**：改为 `#if ODIN_INSPECTOR` 条件编译直接使用 `Sirenix.Utilities` API
- **模块整合**：将 `ReflectionAnalyzer`、`SummaryTool`、`OdinSourceFileHelper` 整合到 `ScriptDocGenerator` 模块下
- **回归单面板设计**：从 4 个独立 Panel SO 回归为单个 `ScriptDocGeneratorSO` + `TypeSource` 枚举切换模式
- **OdinSourceFileHelper 精简**：移除花括号跟踪、类型体定位等复杂逻辑
- **Summary 解析优先级**：优先检查 `[Summary]` 特性，无则回退到源代码 XML 注释解析

#### Removed

- **OdinAutoTooltip 自动 Tooltip 功能**
- **OdinBridge 桥接模式**（4 个文件）
- **多 Panel 设计**（5 个文件）

#### Fixed

- **块注释内的 XML 注释被误解析**：修复后块注释内的 `///` 行被正确忽略
- **泛型类型的 summary 无法解析**：修复后泛型类型的 summary 可正常解析
- **Type 自身的 summary 无法解析**：修复后类型自身的 summary 可正常解析
- **嵌套类型的 summary 返回外层类的注释**：修复后嵌套类型返回各自的 summary
- **多行属性声明的成员名提取失败**：修复后可正确提取成员名
- **泛型方法和表达式体泛型方法的成员名提取错误**：修复后可正确提取方法名
- **重载方法的 summary 互相覆盖**：修复后每个重载方法通过参数类型列表区分
- **重载方法的 `[Overload]` 前缀重复追加**：修复后每个重载方法只追加一次
- **`ReferenceLinkURL` 特性在文档中显示不全**：修复后完整显示特性及其参数
- **文件名与类型名不匹配时源文件无法找到**：修复后通过全局内容扫描找到源文件
- **`null` 关键字被误提取为成员名**：修复后不再被提取
- **多行方法声明参数跨行时参数类型提取失败**：修复后通过跨行收集声明文本直到括号匹配

---

## [0.5.0] - 2026-08-01

---

### [architecture] Aesir Architecture

#### Fixed

- **单例竞争修复**：`AesirArchitecture` 重复实例 `Destroy` 后提前 `return`，避免继续执行赋值和 `DontDestroyOnLoad`；`OnDestroy` 仅在 `_instance == this` 时清空，避免销毁非自身实例时误清
- **RemoveListenerTrigger**：移除 `[DisallowMultipleComponent]` 限制

---

### [modules] Aesir Modules

#### Changed

- 目录重命名：`Odin Integration` → `OdinIntegration`（与 Inspector 保持一致）
- 版本号与 Aesir Architecture / Aesir Inspector 同步更新至 `0.5.0`

---

### [inspector] Aesir Inspector

#### Added

- **Odin 自动 Tooltip (OdinAutoTooltip)** ⚡：从源代码 XML `/// <summary>` 注释自动生成 Inspector Tooltip 的 Odin 属性处理器。提取自 [JakePineOdinTools](https://github.com/JakePineGames/JakePineOdinTools)（MIT, © 2026 Jake Pine）。已有 Tooltip 时读取现有值并追加新内容后动态替换原特性
- **ScriptDocGenerator 源码 Summary 解析**：`MemberData` 添加 `SummaryResolver` 委托，Editor 程序集加载时注入源码解析实现，从 `.cs` 文件的 XML `/// <summary>` 注释中读取成员摘要
- **ScriptDocGenerator OdinMenuEditorWindow 重构**：窗口从 `OdinEditorWindow` 重写为 `OdinMenuEditorWindow`，左侧菜单 4 种工作模式（单脚本、多脚本、单程序集、多程序集），每种模式独立面板 SO
- **共享源码解析工具**：`OdinSourceFileHelper`（源文件定位与成员声明提取）和 `SourceSummaryParser`（XML summary 解析），消除 `SourceSummaryInitializer` 与 `OdinAutoTooltipAttributeProcessor` 之间的重复代码

#### Changed

- 目录重命名：`Odin Integration` → `OdinIntegration`
- **Third Party Notices 更新**：替换占位内容，添加 JakePineOdinTools 第三方组件记录
- **Summary 工具标注为推荐替代**：README 中标注推荐新代码使用 OdinAutoTooltip

#### Removed

- **移除 `[Summary]` 特性装饰**：252 个文件中 897 处 `[Summary("...")]` 装饰已全部移除。`SummaryAttribute` 类保留作为 ScriptDocGenerator 的回退兼容
- **移除 MIT LICENSE 头部**：所有 `.cs` 文件的 LICENSE 头部已移除，仅在 `CodeStyle/AesirInspectorCodeStyle.cs` 保留一份

#### Fixed

- 修复 `ScriptDocGeneratorController.GenerateMultipleTypeDocs` 中 `generatorSettings` 被当作 bool 的 bug

---
- Editor 工具链（SO Inspector / MVP 脚手架 / 模块可视化）
- 运行时集合（RuntimeSet）

**Aesir Modules**:
- Scene 模块（SceneLoader、SceneReference）
- 对象池扩展（当前用隐藏复用，必要时增加 UIForm 对象池）

---

## [0.4.2] - 2026-07-24

---

### [architecture] Aesir Architecture

#### Fixed

- **GetModel / GetService 初始化状态校验** — 在 `GetModel<T>()` 和 `GetService<T>()` 扩展方法中，null 检查之后新增 `Initialized` 状态检查。若目标已注册但尚未初始化，抛出 `InvalidOperationException` 并报告调用者类型和目标类型，提示注册顺序错误或循环依赖。此前获取已注册但未初始化的模块会静默返回半成品实例，可能导致难以排查的运行时错误

---

### [modules] Aesir Modules

#### Changed

- 版本号与 Aesir Architecture / Aesir Inspector 同步更新至 `0.4.2`，本包本版本无功能性变更

---

### [inspector] Aesir Inspector

#### Changed

- 版本号与 Aesir Architecture / Aesir Modules 同步更新至 `0.4.2`，本包本版本无功能性变更

---

## [0.4.1] - 2026-07-24

---

### [architecture] Aesir Architecture

#### Changed

- **Samples 代码优化**：Counter-MVC 的 View（`SampleMvcCounterMainPanel`）和 Counter-MVP 的 Presenter（`SampleMvpCounterPresenter`）从缓存 Model 字段引用改为属性每次获取（`=> this.GetModel<T>()`），并添加注释说明此模式支持运行时动态替换 Model/Service，旧实例可被 GC 正常回收
- **Samples 版本文件夹**：`Assets/Samples/Aesir Architecture/0.3.2/` → `0.4.0/`，与 `package.json` 版本对齐

---

### [inspector] Aesir Inspector

#### Changed

- **Samples 版本文件夹**：`Assets/Samples/Aesir Inspector/0.4.0-pre.1/` → `0.4.0/`，与 `package.json` 版本对齐

---

## [0.4.0] - 2026-07-24

> **总览 / Overview**：本版本三大里程碑 — ① Aesir Inspector 完成品牌命名空间统一（RunLab → Runestone）；② Aesir Modules 重构为 Manager of Managers 模式；③ Aesir Architecture 引入 Query 查询系统（CQRS 读操作）。三个子包**版本号统一对齐到 0.4.0**。
>
> *Three milestones: ① Aesir Inspector brand namespace unification (RunLab → Runestone); ② Aesir Modules refactor to Manager of Managers pattern; ③ Aesir Architecture introduces the Query system (CQRS read). All three sub-packages unified to version 0.4.0.*

---

### [architecture] Aesir Architecture

#### Added

- **Query 查询系统** — 新增 `IQuery<TResult>` / `AbstractQuery<TResult>` / `ICanExecuteQuery` 能力接口及 `ExecuteQuery` 扩展方法，实现 CQRS 读写分离。Controller 和 Presenter 同时具备 ExecuteCommand + ExecuteQuery 能力，Service 保持不参与 Command/Query 执行

---

### [modules] Aesir Modules

#### Changed（Manager of Managers 模式，合并为 UIManager）

- **架构重构**：UI 模块从 RAA Service 简化为 Manager of Managers 模式。原 `UISystem`（纯 C# 单例）与 `UIRoot`（MonoBehaviour 根节点）合并为 `UIManager`——单一 MonoBehaviour 单例（继承 `AesirMonoBehaviour`），同时承担面板管理与 UI 层级构建职责。
- **两层结构**：移除 `Runtime/Odin Integration/` 空壳程序集，架构精简为 Engine/UI/（接口、配置、枚举）+ Component/UI/（UIManager、AesirUIPanel、UICanvasConfigSO）。Odin 条件编译通过 `#if ODIN_INSPECTOR` 实现，无需独立程序集。
- **API 精简**：移除 `Back()`、`CloseAll()`、`Get<T>()`、`pauseUnderneath`、`OnPause`/`OnResume`、`SetUIRoot()` 等不适用的功能；生命周期简化为 `OnInit` → `OnShow` → `OnHide`/`OnClose`。
- **命名调整**：管理器统一为 `UIManager`；面板基类 `Panel` → `AesirUIPanel`；加载器接口 `IUILoader` → `IUIAssetLoader`。
- **预制体管理**：恢复 `RegisterPrefab<T>` 静态快捷方法，支持注册模式和路径模式并存。
- **XML 注释**：全部公共成员补充 XML 文档注释，统一使用多行格式。
- **文档重写**：README、使用手册、机制文档、调研分析全部同步为当前架构。
- **依赖升级**：`cn.runestone.aesir.architecture` 依赖由 `0.3.2` → `0.4.0`

---

### [inspector] Aesir Inspector

#### ⚠ BREAKING CHANGES（破坏性变更 · 升级前必读 / Read before upgrading）

> **品牌命名空间统一 / Brand namespace unification**：将所有 `RunLab` 引用统一为 `Runestone`（符文石），与 Aesir Architecture / Aesir Modules 保持一致。
> 所有 `RunLab.*` 命名空间、`cn.runlab.aesir-inspector` 包名、9 个 asmdef 全部改名为 `Runestone.*` / `cn.runestone.aesir.inspector`。
> 升级后**所有使用本包的代码需要批量替换 `using RunLab.*` → `using Runestone.*`**。

##### 迁移指南 / Migration Guide

| 范围 / Scope | 旧 / Before | 新 / After |
|---|---|---|
| 包名 / Package ID | `cn.runlab.aesir-inspector` | `cn.runestone.aesir.inspector` |
| 命名空间 / Namespace | `RunLab.AesirInspector` | `Runestone.AesirInspector` |
| 命名空间 / Namespace | `RunLab.AesirInspector.Editor` | `Runestone.AesirInspector.Editor` |
| 命名空间 / Namespace | `RunLab.AesirInspector.Tests` | `Runestone.AesirInspector.Tests` |
| 命名空间 / Namespace | `RunLab.AesirInspector.Editor.Tests` | `Runestone.AesirInspector.Editor.Tests` |
| 命名空间 / Namespace | `RunLab.AesirInspector.OdinIntegration` | `Runestone.AesirInspector.OdinIntegration` |
| 命名空间 / Namespace | `RunLab.AesirInspector.OdinIntegration.Editor` | `Runestone.AesirInspector.OdinIntegration.Editor` |
| 命名空间 / Namespace | `RunLab.AesirInspector.Samples.*` | `Runestone.AesirInspector.Samples.*` |
| asmdef 名称 / Assembly name | `RunLab.AesirInspector`（及所有变体） | `Runestone.AesirInspector`（及所有变体） |
| 版权字符串 / Copyright | `Copyright (c) 2026 RunLab - Yuumix` | `Copyright (c) 2026 Runestone - Yuumix` |

##### 代码侧替换示例 / Code-side replace examples

```csharp
// 旧 / Before
using RunLab.AesirInspector;
using RunLab.AesirInspector.Editor;
using RunLab.AesirInspector.OdinIntegration;

// 新 / After
using Runestone.AesirInspector;
using Runestone.AesirInspector.Editor;
using Runestone.AesirInspector.OdinIntegration;
```

```jsonc
// asmdef references 旧 / Before
"references": [
  "RunLab.AesirInspector",
  "RunLab.AesirInspector.Editor"
]

// 新 / After
"references": [
  "Runestone.AesirInspector",
  "Runestone.AesirInspector.Editor"
]
```

##### 范围 / Scope
- 422 个 .cs 文件 / 12 个 asmdef + 12 个 asmdef.meta / 1 个 package.json / 1 个 LICENSE.md / 多份 README/CHANGELOG/CONTRIBUTING

#### Changed

- 将 `OdinWrapper` 重命名为 `Odin Integration`（目录）/ `OdinIntegration`（命名空间与程序集），以更准确表达集成层的语义
- 将 `Runtime/Unity/Bilingualism/` 重命名为 `Runtime/Unity/Localization/`，对齐 Unity 官方 Localization 包命名
- 将 `Runtime/Unity/InspectorControls/` 重命名为 `Runtime/Unity/Inspector/`，采用 Unity 单数名词惯例
- 将 `Runtime/Unity/Logger/` 重命名为 `Runtime/Unity/Logging/`，对齐 Unity 源码 `Runtime/Export/Logging/` 命名

---

## 版本策略 / Versioning Policy

> **monorepo 版本号与子包版本号独立维护，但本次（0.4.0）三个子包统一对齐。**
> - **monorepo 视图（本文件）**：聚合三个子包的变更，便于仓库级用户一眼看完。
> - **子包视图（各子包 `CHANGELOG.md`）**：只记录该子包自己的变更历史，含 0.4.0 之前的版本。
> - **版本号规则**：本次特殊，三个子包均升到 0.4.0。未来允许各子包独立 bump。
>
> *Monorepo view (this file) aggregates all three sub-packages; per-package view tracks individual history. Version 0.4.0 is unified across all three sub-packages; future versions may bump independently.*

---

## 仓库级变更 / Repository-Level Changes

> 仓库结构、文档、跨子包变更等"monorepo 层面"的事情。子包自身的功能变更记录在上面 `[0.4.0]` 聚合区块。

### 0.4.0 - 2026-07-24

#### Added
- **根目录文档**：monorepo 级别 `README.md`（中英双语段对照，三个子包总入口）、`CONTRIBUTING.md`（统一贡献指南）、`CODE_OF_CONDUCT.md`（Contributor Covenant v2.1）
- **CHANGELOG 聚合模式**：根 CHANGELOG 改为聚合视图，同时记录三个子包变更
- **品牌命名空间统一**：Aesir Inspector 所有 `RunLab` 引用统一为 `Runestone`

#### Changed
- 三个子包版本号统一对齐到 `0.4.0`
- 三个子包均通过单一 monorepo Git URL (`https://github.com/yuumixcode/Unity-Aesir-Packages.git`) 发布，不再有独立子包仓库
- 根 `README.md` 从占位符（2 行）重写为完整总入口
- 三个子包 README 顶部增加 monorepo 引用块

---
