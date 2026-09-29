# Changelog

本项目的所有重要变更均会记录在此文件中。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/)，
版本号遵循 [Semantic Versioning](https://semver.org/lang/zh-CN/)。

## [Unreleased]

### Added

- **UniTask 驱动链 PlayMode 测试程序集** — 新增 `Runestone.AesirModules.Tests.UniTask`（`Tests/Runtime/UniTask/`），覆盖 `SceneModuleUniTask` 的 8 个公开 API：单参 / wrapper 重载加载、叠加加载不改激活场景、卸载与批量卸载（空追踪 no-op）、无效路径抛 `InvalidOperationException` 且不触发事件、入口即取消（同步抛、不发起加载）、加载中取消（仅中止等待、底层流程继续完成）、宿主销毁（`destroyCancellationToken` 链接生效、等待方以取消收场不悬挂）、`onProgress` 单调不减且不超过归一化上限。程序集带 `defineConstraints`（`UNITY_INCLUDE_TESTS` + `AESIR_MODULES_UNITASK`）——**未装 UniTask 的工程整体不编译、回归计数不变**；`namespace` 停在 `Runestone.AesirModules.Tests` 以避免 `UniTask` 段遮蔽 `Cysharp.Threading.Tasks.UniTask`（CS0118）。场景卫生沿用既有四件套（`IPrebuildSetup` / `IPostBuildCleanup` + 域加载兜底清扫 + 场景按文件名定位），与 `SceneModulePlayModeTests` 共享 `TestScenes/` 夹具
- **日志门面补 `ScriptDocGeneratorTag`** — ScriptDocGenerator 模块的日志此前散落在裸 `Debug.Log*` 与多种前缀（含无前缀）上，收敛时需要一个模块标识
- **测试守护网新增三处** — `UIRootTests`（`CreateInputModule` 静态重置）、`AesirEventUtilityTests`（绑定键缓存静态重置）、`UIRootPlayModeTests`（重复实例只销毁自身组件——该行为在 EditMode 下不可断言：`Destroy` 被引擎拒绝且两种销毁粒度都是 no-op，故落在 PlayMode 程序集）
### Fixed

- **UniTask 门控测试程序集从未参与编译（8 条用例静默消失）** — `Runestone.AesirModules.Tests.UniTask` 的 `defineConstraints` 含 `AESIR_MODULES_UNITASK`，但该程序集自身没有声明 `com.cysharp.unitask` 的 `versionDefines`——versionDefines 产生的宏**只对声明它的程序集可见**（不会成为全局宏），故 UPM 安装形态下约束永不满足，程序集被 Unity 静默排除出编译管线：用例在 Test Runner 中整体缺失、**总数不变且零报错**（此前只在"手动置全局宏 + 桩程序集"的验证路径下通过，真包安装即暴露）。测试程序集补一份 `versionDefines`（与核心 / 适配程序集同款 belt-and-braces）。`AesirUniTaskDefineKeeperTests` 同步修正装配不变量（UPM 形态下全局宏缺席属预期——宏由 versionDefines 提供）并新增两条守卫：① 文本侧——凡以 `AESIR_MODULES_UNITASK` 作约束的程序集都必须各自声明 versionDefines；② 行为侧——适配程序集与门控测试程序集的加载状态必须一致
- **UniTask PlayMode 套件首次真跑暴露的两处用例缺陷（用例此前从未执行）** — ①**进度断言与生产语义相反**：`onProgress` 仅在加载期间按 `progressCap` 归一化，末值由 `CompleteLoad` 收尾为 `1f`（协程与 UniTask 两条路径同一实现），用例却断言"末值不超过 progressCap"——改为与协程套件一致的口径（单调不减 + 归一化值不超过 1 + 末值 = 1.0 + 事件广播前已达 1.0）。②**缺场景卫生，断言被同学例遗留的双实例污染**：Single 用例会留下"唯一已加载场景"，该遗留实例一旦与后续用例的叠加目标同路径，叠加加载即产出**第二个实例**——模块追踪按路径粒度只记一条、按路径卸载只移除其一（见 `LoadSceneAdditive` 约定），"卸载后场景应从场景管理器消失"必然失败。补 `[UnitySetUp]` / `[UnityTearDown]` 清扫：运行时创建空"锚场景"使 `sceneCount > 1` 恒成立，再按 **Scene 句柄**逐个卸载测试场景实例（同路径多实例时按路径卸载行为不可控）——套件因此与用例次序、与前序套件遗留彻底解耦（协程套件以 `[Order]` 次序规避同一问题，本套件用锚场景根治）。定性依据为逐帧枚举场景管理器的临时探针：卸载在两条驱动路径上完成时机一致（同帧即反映实例减少，`scenes=[B, A, B]` → 卸载后 `[A, B]`），失败与驱动路径无关
- **`SceneEditorSettings` 四个私有字段漏 `[SerializeField]`** — 类上声明了 `[FilePath]`，但四个字段都没带标记 → 设置文件里没有对应键，值只活在当前进程内，重建单例（编辑器重启 / 切项目）即回落默认：Bootstrapper 搜集与强制加载开关静默失效。补标记（`using UnityEngine;` 同步补上）

- **UIRoot 四层 Canvas 引用表漏序列化**（数据层）— `_layerCanvases` 声明为 `readonly` 且缺 `[SerializeField]`，Unity 两个条件都不满足，四层 Canvas 引用表**永远以空列表进入运行时**（其上方注释却宣称"随场景序列化持久"）。层子物体一旦改名，`EnsurePresetLayers` 既找不到引用也找不到同名物体 → 新建重复 Canvas，同 `sortingOrder` 双 Canvas 叠加渲染；`EnsureUICamera` / `EnsureEventSystem` 的"引用非空即跳过"快路径也永久失效，退化为每帧全场景扫描。改为 `[SerializeField] [HideInInspector] private List<LayerCanvasEntry> layerCanvases`，与同类 `uiCamera` / `eventSystem` / `uiCanvasConfigSO` 对齐
- **面板 / 窗口的销毁反清理可被子类屏蔽**（内存 + 稳定崩溃）— `AesirBasePanel` / `AesirBaseWindow` 的 `OnDestroy` 是 private 非 virtual，而两者的 XML 注释正建议子类自己写 `OnDestroy` 解除事件——写了就屏蔽基类版本，`UIModule.RemovePanelRecord` / `RemoveWindowRecord` 永不执行，注册表残留已销毁实例，之后每次 `ShowPanel` / `OpenWindow` 抛 `MissingReferenceException` 且无自愈路径。双重修复：① 改 `protected virtual` 并在 remarks 强制子类调 `base.OnDestroy()`；② `UIModule` 新增按 Unity 假 null 驱逐已销毁记录的自愈（面板 Show/Hide/Prewarm、窗口 Open/Close/Prewarm 与 `RefreshWindowMasks` 双循环）
- **`EventModule` 预放置路线不受 DontDestroyOnLoad 保护**（静默失效）— `Instance` 文档承诺"优先查找预放置实例"，但该类既无 `dontDestroyOnLoad` 字段也不调 DDOL（`AudioModule` / `AesirModules` 都有）。预放置在场景根的 `EventModule` 随场景卸载销毁 → 整个注册表消失，DDOL 常驻订阅者此后永久收不到任何事件，`AutoRemoveListenerHandle` 捕获旧实例字典使注销静默无效。补同名序列化字段 + Awake 的 DDOL 守卫（含 `transform.root == transform`）
- **`SubclassSelector` 下拉滤掉全部事件参数子类**（SO 资产化主路径不可用）— `SubclassSelectorDrawer` 用 `IsDefined(SerializableAttribute, inherit: false)` 过滤，只看类型自身声明的特性；而 `AesirEventArgs` 自身已带 `[Serializable]`，派生类无需重复标注（Unity 接受继承来的特性，已提交的 `ScoreEventAsset.asset` 中同类型即被正常序列化）。4 个样例事件类全部未重复标注，点开下拉只有"（无可选子类型）"。改为继承判定，与 Unity 真实要求对齐
- **`FindAnyObjectByType` 跳过 inactive 预放置模块**（Inspector 配置被静默忽略）— 该 API 默认 `FindObjectsInactive.Exclude`，而 Unity 不对未激活物体调用 `Awake`，"随用随开"式预放置的模块既不被赋值也搜不到 → 在宿主下另建一个，Inspector 里配的 `executionMsLimit` / `sfxSourceCount` / `AudioConfigSO` 从一开始就无效。`EventModule` / `AudioModule` 的 `Instance` 查找均改为含未激活对象
- **`UIModule.UICamera` 对 Unity 对象用 `?.`** — `?.` 走普通引用判空，绕过 Unity 重载的 `==`；宿主销毁后返回**已销毁对象**的 `Camera` 而非 `null`，调用方拿到 `MissingReferenceException`。改为 `_uiRoot == null ? null : _uiRoot.UICamera`（本项目铁律：`UnityEngine.Object` 一律用 `== null`）
- **`InstantiateInactive` 直接切换调用方预制体资产的激活状态** — `ResolvePrefab` 返回的可能是 `Resources.Load` 取回的预制体**资产本身**，在编辑器 Play 下被 `SetActive` 进出会把导入资产标记为 dirty。改为每个源预制体克隆一次 inactive 模板并缓存，之后 `Instantiate(template)` 天然不触发 `Awake` / `OnEnable`
- **`RaiseEvent` 重入时共享参数实例的 `Sender` 被内层覆写** — sender 写进参数实例本身，而重入保护只覆盖迭代缓冲区。当被复用的参数实例（如 `AesirEventArgsSO` 资产内的实例）以另一个发布者再次 `Invoke` 时，外层剩余订阅者被按内层发布者过滤，三个内建过滤器（`SameSceneAsEmitter` / `OnlySelf` / `InsideCollider2D`）全部依赖 `eventArgs.Sender` → 静默错投或漏投。改为分发开始时取局部 sender 并逐绑定重新断言
- **`SceneModule` 场景事件是实例字段，模块实例重建后订阅永久失联** — `_sceneLoadedEvent` / `_sceneUnloadedEvent` 为实例字段 `MiniEvent`，而静态门面每次从 `Instance` 现取。`dontDestroyOnLoad` 取消勾选 + Single 模式加载销毁宿主后，DDOL 常驻系统此前拿到的句柄指向已废弃对象，永久收不到广播且旧事件被句柄持有造成泄漏。改 `static readonly`，`ResetStatics` 中 `Dispose` 保证 EBM 会话也干净
- **`SceneAssetWrapper` GUID 自愈不回写 Addressables 地址** — 主分支（`sceneAsset != null`）同时回写 `scenePath` / `sceneGuid` / `sceneAddress`，自愈分支只恢复 `scenePath`。悬空且曾被标记为 Addressable 的场景因此静默降级为 `Unsafe`，加载被以"不在 BuildSettings"为由拒绝，GUID 自愈在该场景下形同虚设。自愈分支补一次桥查询并回写（桥不可用时保持原值不动）
- **`SceneAssetWrapper` 相等性在玩家构建中自相矛盾** — `FromScenePath` 非编辑器分支 `sceneGuid` 恒空 → `IdentityKey` 退化为路径，而反序列化实例携带 GUID → 身份键为 GUID，同一场景被判为两个不同引用，字典 / 集合静默保留重复项并漏命中。`Equals` 改为对称（有 GUID 用 GUID，否则按路径不区分大小写），`GetHashCode` 统一按路径
- **`AddedScenePaths` 把内部可变 `List<string>` 以 `IReadOnlyList` 形式外泄** — 消费方遍历期间监听者触发 `CompleteLoad` / `CompleteUnload` 增删即抛 `InvalidOperationException`（堆栈指向消费方，排查方向被误导），且强转回 `List<string>` 即可直接改写模块状态。改为返回 `.ToArray()` 快照
- **`UnloadScene` 无"最后一个已加载场景"保护** — Single 模式加载成功后叠加追踪被清空、场景数只剩 1，此时调用会把引擎级错误日志直接打进用户 Console 并触发 `onFailed`。`BeginUnload` 先判 `sceneCount <= 1`，走模块自身的诊断分支
- **`AesirModules` / `UIModule` / `UIRoot` 缺非泛型单例的 `SubsystemRegistration` 静态重置** — 包内其余非泛型单例（`AudioModule` / `SceneModule` / `UIModuleConfigSO` / `SceneModuleConfigSO` / `EventModule`）均已按铁律类内自重置，这四个漏网。当前靠 `Instance` getter 的 Unity 假 null 恰好兜住（行为上不崩），但那是 `Object ==` 重载的隐式救援而非显式重置——项目已在 Architecture 侧把该做法判定为"碰巧正确而非按原则正确"并废弃。四处补 `ResetStatics`（`UIRoot` 一并重置 `_defaultCanvasConfig`）
- **`AesirModules.Awake` 的 DDOL 调用缺根物体判定** — `UIModule` / `UIRoot` 同款调用都带 `transform.root == transform` 守卫并在注释写明"仅根物体生效"，此处没有。宿主若被预放置为子物体，Unity 打印警告、勾选静默失效，而所有运行时模块都挂在它下面 → 整棵模块树随场景卸载销毁。补守卫与对应说明
- **`RegisterAssetLoader` 不校验 null** — 同类的 `RegisterPanelPrefab` / `RegisterWindowPrefab` 都判空 + `LogError` 后返回，此处没有。误配后错误延后到 `ResolvePrefab` 深处抛裸 `NullReferenceException`，堆栈指向框架内部而非配置处。补齐同款守卫
- **`EnsurePresetLayers` 认领到同名但无 Canvas 的子物体后直接跳过** — 既不补建也不记错，该层永久缺失，而 `Build()` / `Initialize()` 号称幂等自愈却修不好这一层。改为就地补齐 `Canvas` + `CanvasScaler` + `GraphicRaycaster` 并登记
- **`EventModule` 的 `catch (TargetInvocationException)` 分支不可达** — 两条调用路径都不经 `MethodInfo.Invoke`（表达式树委托 / 直接调 `_callback`），该分支从不执行，订阅者抛异常时只走通用分支，丢失"哪个订阅者 + 内层异常"这条唯一有用的定位信息。删除死分支并把上下文折入存活的通用分支（保留原 `事件分发异常：` 前缀，既有测试断言的子串仍匹配）
- **AudioModule 惰性启用下的淡变协程静默失效** — 上一条修复让 `Instance` 能找到未激活的预放置模块后，未激活物体上 `StartCoroutine` 会被 Unity 丢弃，BGM 卡在旧系数上。`StartBgmFade` 增加瞬时降级：无法启动协程时直接落到淡变终态（切歌 = 立即换片段并淡入系数置 1，停止 = 立即停），仅丢失过渡过程，终态与淡变终点一致

- **`UIModule.CloseWindow` 注册表自愈分支未重算蒙版** — 命中"实例已销毁"的残留条目时此前只摘除条目并返回；该路径意味着 `RemoveWindowRecord` 从未执行（子类遮蔽 `OnDestroy` 的场景），蒙版因此停留在旧状态（原顶层窗口的蒙版可能仍显示、新顶层窗口的蒙版未启用）。现补 `RefreshWindowMasks()`
- **`Runestone.AesirModules.Tests.Editor.OdinInspector` 程序集引用面补全（NUnit + Sirenix）** — 新程序集初版误用 `overrideReferences: false` + 空 `precompiledReferences`（`nunit.framework.dll` 是"显式引用"，`isExplicitlyReferenced: 1`，不参与自动引用）→ 整程序集 CS0246；且迁入的 Binder / ScriptDocGenerator 用例还需要 `Sirenix.Serialization`（`SerializedMonoBehaviour` 基类链，缺失即 CS0012 / CS0311）与 `Sirenix.OdinInspector.Attributes`（`ValueDropdownList<>`，CS0012）。现改为 `overrideReferences: true` + `nunit.framework.dll` + 4 条 `Sirenix.*.dll`，并补 `Runestone.AesirModules.OdinInspector` / `Runestone.AesirModules.Editor.OdinInspector` / `Runestone.AesirModules.Editor` 引用
- **`BinderAssistant.GetPartialSuffixOptions` 不可达返回语句**（CS0162）— 候选后缀改由 `BinderEditorSettings` 驱动时遗留了旧的硬编码 `return new ValueDropdownList<string> { { ".designer.cs", ".designer.cs" } }` 死代码。删除，行为不变
- **`SceneAssetWrapper` 空引用判等回归**（行为）— 判等改「路径优先」后，路径与 GUID 俱空的两个包装器被判为不相等，而两者哈希相同、既有用例 `EmptyWrapper_EqualitySemantics` 断言其相等。补齐"两侧都没有任何身份信息即彼此相等"分支（与哈希键一致），空包装器仍可作字典 / 集合键
- **`EventModuleTests.Instance_FindsInactivePreplacedModule` 断言命中不确定** — 用例清空静态单例后直接在场景内查找，而 SetUp 创建的模块此刻仍活在场景里（`FindAnyObjectByType` 命中顺序不保证）→ 该断言时通时不同。改为先销毁 SetUp 模块，使场景内只剩待查的未激活预放置实例，断言才真正指向"含未激活对象查找"契约

- **日志输出统一收敛到包门面 `AesirModulesDebug`**（日志文案变更）— 生产路径此前有 50 余处绕开门面的裸 `Debug.Log*`（含无前缀者）：Runtime 侧 `BindingInfo` / `EventModule` 的绑定失败告警与 SDG 的 `DefaultAnalysisDataFactory` / `TypeData` / `TypeAnalyzerStaticExtensions`；编辑器侧 ScriptDocGenerator 全模块（`[ScriptDocGeneratorAPI]` 前缀改用门面 source 次前缀保留）、`BootstrapSceneHelper`（`SceneModuleTag` + 类名 source）。**代价是控制台文本前缀形态改变**（彩色加粗主前缀 + 中括号次前缀），文本子串不变。**唯一有意例外**：`AesirDependencyInstaller`（`Editor/Bootstrap/`）保留裸 `Debug.Log` —— 其所在程序集必须零引用（补齐依赖期间 RAM 核心程序集可能根本不编译，门面不可用），已在类 remarks 写明
- **`EventModule` 双注册表收窄为 internal（破坏性）** — `AttributeBindings` / `DynamicBindings` 是实现细节（订阅 / 退订必须经公开 API 走同一套绑定键与死引用清理），包外不再可见；包内测试经 `InternalsVisibleTo` 访问。业务代码若直接读过这两个注册表会编译失败
- **`BinderEditorSettings` 改为真持久化（行为变更）** — 该类此前**没有** `[FilePath]`，三个 `[SerializeField]` 对磁盘是死标记（基类 `Save(bool)` 找不到路径只打警告、不写文件），且自己的 `Save()` 只做 `SetDirty` + `SaveAssets`（对非资产对象无效）→ 值"进程内活、单例重建即回默认"。现补 `[FilePath("ScriptableSingleton/AesirModules/BinderEditorSettings.asset", ProjectFolder)]`（该目录已被 .gitignore 覆盖）、`Save()` 改调基类 `Save(true)`、`SetLastNamespace` 由 `SetDirty` 改 `Save(true)`：**重启编辑器后后缀列表 / 默认后缀 / 最近命名空间予以保留**（此前重启即丢）
- **`SceneEditorSettings` 各 setter 值相等早退** — 拖拽 / 连点开关时每个 tick 都触发一次 `Save(true)`（磁盘写）；现仅在实际变化时写盘
- **`UIRoot` 重复实例改 `Destroy(this)`（行为变更）** — 对齐 RAA 与 RAM 其余模块的销毁粒度：只销毁本组件，不连带销毁用户物体上的其它组件，也不连带销毁已按约定搭好的四层 Canvas 层级（此前 `Destroy(gameObject)` 会连物体一起销毁）
- **`UIRoot.CreateInputModule` 与 `AesirEventUtility.KeyCache` 纳入静态重置** — 两者都是关闭 Domain Reload 时会跨会话残留的静态状态：前者是 InputSystem 程序集注册的输入模块工厂（清空不丢注册——本重置跑在 `SubsystemRegistration`，早于注册方的 `BeforeSceneLoad`），后者是纯派生数据的绑定键缓存（清空后按需重建）；`AesirEventUtility` 补 `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]`
- **模块配置资产初始化器抽公共实现** — UI / Scene 两份等价的「Resources 兜底资产自动创建」收敛到 `Editor/Common/AesirSingletonAssetInitializer`（保留两个模块各自的 `XxxModuleConfigAssetInitializer` 作为 3 行调用方与资产路径常量持有者）
- **`EventModule` 补 `[DefaultExecutionOrder(-999)]` 并删除死守卫** — 与 `UIModule` / `AudioModule` / `UIRoot` / `SceneModule` / `AesirModules` 同族一致；`AddDynamicBindingGeneric` 里 `IsAssignableFrom(AesirEventArgs)` 的抛异常分支恒不成立（入参已声明该类型）→ 删除，避免读者误以为存在类型防线
- **Binder 编辑器代码的程序集归属收尾** — `BinderMenuItems` 落 `Editor/UI/OdinInspector/`（经既有 asmref 汇入 `Runestone.AesirModules.Editor.OdinInspector`），删除在该 Editor-only 程序集内冗余的整文件 `#if UNITY_EDITOR` 守卫；`BinderAssistant` / `BinderInfo` 两支指向已撤销程序集的注释同步修正
- **注释与文档口径修正** — Addressables 胶水注释改「核心运行时程序集与胶水程序集**各自**声明一份 `versionDefines`（belt-and-braces）」；`UIRoot.ResetStatics` 关于 `_defaultCanvasConfig` 的措辞改为「解除引用、交由 GC 回收（无强引用）」；README 中英与 `scene-module.md` 的「无 Odin 边界」补可执行替代路径（代码构造引用 + Build Settings 面板手动加场景）
### Changed

- **核心程序集解除对 UniTask 的硬引用**（渐进式可选项归位）— `Runestone.AesirModules.asmdef` 此前把 `UniTask` 写进 `references`（硬依赖），同时又用 `versionDefines` 把它声明为可选项并定义 `AESIR_MODULES_UNITASK` 门控宏，而 `versionDefines` 只能增删宏、不能移除程序集引用。经核实本仓 `manifest.json` 无该包、`PackageCache` 无、全仓无 UniTask dll——核心程序集当时即带悬空引用。移除该引用；`SceneModule` 中全部 UniTask 用法经复核均已 `#if AESIR_MODULES_UNITASK` 包裹，协程回退路径（README 承诺"未包含 UniTask 时回退协程驱动"）随之在程序集图上成立
- **Binder 编辑器工具链移出 Player 构建**（包体 + 解析失败）— `Runestone.AesirModules.OdinInspector` 此前 `includePlatforms: []`（全平台），使 3,400+ 行的 `BinderAssistant`（969 行，含 `AppDomain` 全程序集 `GetTypes()` 扫描）、代码生成器、`System.IO` 写脚本等编辑器工具链随 Standalone 打包（`ODIN_INSPECTOR` 对 Standalone 已定义）。现按"纯文本逻辑 + 整文件 `#if UNITY_EDITOR`"两道防线收敛：`BinderCodeGenerator`（纯字符串拼装，无 Unity/Odin 依赖，必须留在运行时可见程序集供 `BinderAssistant` 调用）与 `BinderEditorSettings`（整文件 `#if UNITY_EDITOR`，`ScriptableSingleton` 本就是编辑器对象）位于 `Runtime/UI/OdinInspector/Binder/`，其编辑器实现整体由 `#if UNITY_EDITOR` 剔除；真正的编辑器入口 `BinderMenuItems`（`[MenuItem]`）落在 Odin 集成的编辑器程序集 `Runestone.AesirModules.Editor.OdinInspector`（`Editor/UI/OdinInspector/`，经既有 asmref 汇入）。因 `IComponentBinder` 被用户运行时程序集里的生成代码按全限定名引用、且 `BinderAssistant` 是场景组件，`IComponentBinder` / `BinderAssistant` / `BinderInfo` / `BinderTag` / `BinderHierarchyUtility` 必须留在运行时可见程序集——其 Odin 面（`using`、Odin 特性、`ValueDropdown` 成员、Inspector 绘制与脚本写入块）全部移入 `#if UNITY_EDITOR` 保护，经预处理器模拟核实**玩家视图零 Odin 引用、编辑器视图与改前等价**
- **测试程序集按「是否依赖 Odin 类型」重新划分（测试保护网归位）** — 原先编辑器测试程序集带 `ODIN_INSPECTOR`（用户手动添加的符号）门控 + 4 条 `Sirenix.*.dll` 预编译引用：未装 Odin（或活动平台非 Editor）时整程序集静默不编译，`Tests/Editor/UI/` 的 44 个用例在 Test Runner 中消失且无任何报错。现按依赖性质分两层：① 真正依赖 Odin 类型的用例（Binder 4 个文件 + ScriptDocGenerator 全模块）迁入新建的门控程序集 `Runestone.AesirModules.Tests.Editor.OdinInspector`（`Tests/Editor/OdinInspector/`，`.meta` / GUID 随 `git mv` 保留）；② 其余用例留在基线程序集 `Runestone.AesirModules.Tests.Editor`，**保留 4 条 `Sirenix.*.dll` 引用但移除门控**。保留引用而移除门控的依据：Odin 环境下 `AesirMonoBehaviour` 等框架基类的基类链指向 `SerializedMonoBehaviour`，任何 `AddComponent<T>` / `CreateInstance<T>` / 派生 / `==` 都要求编译器解析该基类（缺失即 CS0012 / CS0311）——**不引用 Sirenix 的测试程序集在装了 Odin 的工程里必然编译失败**；而**指向不存在程序集的预编译引用会被 Unity 静默忽略**（本仓实测：写入一个不存在的 dll 名既不报错也不影响编译），故保留引用在有 Odin 与无 Odin 两种环境下同时成立。同一口径应用到两个 PlayMode 测试程序集（`Runestone.AesirModules.Tests` / `Runestone.AesirArchitecture.Tests`，原先同样带 `ODIN_INSPECTOR` 门控）；`Runestone.AesirArchitecture.Tests.Editor` 不引用 Sirenix——其用例经类型系统断言（`typeof(...).IsAssignableFrom`）而非实例化 Odin 基类链类型
- **`AudioModule` 通道维度收敛为单一数据源**（OCP）— 通道（BGM / SFX / UI）此前摊平在字段、6 个键名属性、6 个公开静态音量/静音属性、配置默认值、6 段配置读取、`ApplyVolumes` / `ApplyMutes` 各两分支共 8 处，新增一条通道需改约 20 句。引入 `readonly struct AudioChannel` 与 `AudioModule.Channels`（`IReadOnlyList`），音量 / 静音数组与配置、持久化读取均按通道集合遍历。**所有既有公开成员与 PlayerPrefs 键名逐字不变**（纯重构，无行为变化、无破坏性 API 变更），新增通道的改动点由约 20 处降到约 4 处。总音量作为跨通道闸门、源拓扑差异（`ApplyVolumes` / `ApplyMutes` 保留 BGM-vs-SFX 分支）已就地注释说明
- **样例程序集 `autoReferenced` 归位** — 5 个 Modules 样例程序集（`Samples/` 与 `Samples~/` 各一份，共 10 个文件）此前为 `true`，样例类型会被用户的 `Assembly-CSharp` 自动引用、无需显式引用即出现在代码补全里，且是命名冲突温床；Architecture 侧 11 个样例均为 `false`。统一改为 `false`（两棵镜像树同步，无漂移）
- **分发裁剪：UPM 产物只保留 Package Manager 认的那一份**（打包）— monorepo 为同时服务开发可见与两种安装方式，每个包带 `Samples/` + `Samples~/` 与 `Documentation/` + `Documentation~/` 两套镜像，且**带完全相同的 `.meta` GUID**。原样推到 UPM 分支会：① 非 `~` 的 `Samples/` 被 Unity 正常索引，每个消费工程无条件多出 11~16 个样例程序集、约 75 个脚本与 12 个场景/预制体；② 用户经 Package Manager 导入样例时 GUID 被保留，与已索引副本撞车，同一项目内两个资产共用一个 GUID。`auto-publish-branches.yml` 在 `git subtree split` 之后 `git rm -r Samples Documentation` 再推送，并做双向校验（开发侧副本必须为 0、`Samples~` / `Documentation~` 必须存在，任一不满足即 `exit 1`）。unitypackage 侧无需改动——`build_unitypackage.py` 的 `_is_ignored_name()` 已过滤 `~` 结尾目录（实测两包 `~` 目录条目为 0），两条分发线互为补集

- **Binder 编辑器工具链的程序集归属修正（回收拆分引入的悬空引用与多余程序集）** — 拆分过程中一度新建 Editor-only 程序集 `Runestone.AesirModules.Editor.Binder` 承载 `BinderCodeGenerator` / `BinderEditorSettings`，但**全平台**程序集 `Runestone.AesirModules.OdinInspector` 里的 `BinderAssistant` 仍在 20+ 处调用这两个类型（全在 `#if UNITY_EDITOR` 内）——全平台程序集不能引用 Editor-only 程序集，补引用则与"Editor.Binder 已引用 OdinInspector"构成程序集环。现按上一条的两道防线收敛，**该程序集整体撤销**：它的 `references` 与 `defineConstraints` 与既有 `Runestone.AesirModules.Editor.OdinInspector` 逐字段相同，属按模块（而非按"运行时 / 编辑器 / 某个插件的集成"这一需求面）拆出的多余程序集；`BinderMenuItems` 迁入后者消费既有 asmref，`BinderCodeGenerator` / `BinderEditorSettings` 回到运行时可见程序集。玩家视图的 Odin 引用仍为零（Binder 侧全部 Odin 面继续由 `#if UNITY_EDITOR` 守着）
- **恢复核心程序集的 `UniTask` 引用（撤销上轮的删除）** — 上轮以"核心程序集把可选项写成硬引用"为由删除了 `references` 里的 `"UniTask"`，但 `SceneModule` 的 `#if AESIR_MODULES_UNITASK` 驱动实现仍在核心程序集内，而该宏由核心程序集自身的 `versionDefines` 提供 —— 装了 UniTask 的工程会因找不到 `Cysharp.Threading.Tasks` 直接 CS0246（开发仓未装该包，故不会暴露）。且按程序集名引用一个**不存在**的程序集会被 Unity 静默忽略（本仓已实测），原引用并不构成硬依赖：可选项由 `versionDefines` + `#if` + 独立适配程序集共同承担。恢复 `"UniTask"` 引用，`AesirUniTaskDefineKeeperTests` 的命名守卫用例随即可通过
- **ScriptDocGenerator 运行时代码解除对 Odin 运行时程序集的依赖** — `TypeAnalyzerStaticExtensions` / `TypeData` 直接使用 `Sirenix.Utilities` 的 `GetNiceName` / `GetNiceFullName`（类型格式化）与 `using Sirenix.Utilities;`，而它们经 asmref 汇入**全平台**程序集 `Runestone.AesirModules.OdinInspector` —— 与 Binder 同款问题：Player 构建会依赖 Odin 运行时程序集（消费方若把 Odin 装成编辑器限定则构建失败）。现新增 `NiceTypeName`（逐条移植 Sirenix `TypeExtensions` 的别名表与数组 / 可空 / 引用 / 泛型 / 嵌套拼接规则，不涉及 Odin 专属能力）承接格式化；`TypeData` 的 `ReflectionUtility.IsStatic` 本就解析到本包同名类，Sirenix 的 `using` 属冗余，一并移除。**程序集内对 Odin 的"直接"代码引用现为 0**（剥离 `#if UNITY_EDITOR` 分支后扫描：仅剩生成器输出字符串里的 `[Sirenix.OdinInspector.TitleGroup]` 文本与文档注释）。注意仍有**经 `AesirMonoBehaviour` 基类链的间接依赖**——那是框架既定设计（未定义 `ODIN_INSPECTOR_EDITOR_ONLY` 时玩家侧也用 Odin 序列化后端），与本条无关
- **`SceneAssetWrapper` 判等与哈希统一主键（行为变更）** — `Equals` 此前"两侧都有 GUID 就按 GUID 比"，而 `GetHashCode` 一律按路径，两者主键不一致会违反哈希契约：同一场景在移动/改名后（旧引用残留陈旧 `scenePath` + 有效 `sceneGuid`，新引用是新路径同 GUID）会出现 `Equals == true` 而哈希不等 → `Dictionary`/`HashSet` 静默保留重复条目、查找漏命中。现统一为**路径优先、任一侧无路径时回退 GUID**（与哈希主键一致）。取舍：跨改名识别不再由判等承担，需依赖 `sceneGuid` 锚点自愈把路径写回后再比较；补"同 GUID 不同路径"与"一侧无 GUID"两条回归用例
- **`EventModule` / `AudioModule` 之外的模块统一"查找含未激活对象"口径** — `FindAnyObjectByType<T>()` 默认排除未激活对象，而 Unity 不对未激活物体调用 `Awake`，"先禁用、用到时再启用"式预放置的单例既不进 `_instance`、也搜不到 → 框架会在 `[Aesir Modules]` 下另建一个，Inspector 里的配置（`uiCanvasConfigSO` / `bootstrapScene` / `sfxSourceCount` / `dontDestroyOnLoad` 等）从第一帧起被静默忽略。上一轮只修了 `EventModule` / `AudioModule`，本轮把 `UIModule` / `UIRoot`（含 `EventSystem` 探测）/ `AesirModules` / `SceneModule` / `AesirArchitecture` / `MonoLifecycleProxy` / `RemoveListenerOnSceneUnloadedTrigger` 一并统一为 `FindObjectsInactive.Include`
- **`AesirModules.GetOrAddChild<T>` 幂等化** — 已存在同名子物体但缺少目标组件时就地补齐组件，而不是继续走"新建 GameObject"分支：`Transform.Find` 始终命中**最先**的同名子物体，空壳不被认领会让每次调用都再堆一个（与 `UIRoot.EnsurePresetLayers` 对"同名但缺 Canvas"的就地补齐口径一致）
- **`AesirBasePanel.HideSelf` / `AesirBaseWindow.CloseSelf` 改走非创建式获取** — 面板 / 窗口销毁与场景卸载阶段 `UIModule` 往往已随之消失，原先经 `UIModule.Instance` 会在正在卸载的场景里**重建 `[Aesir Modules]` 宿主并加入 DDOL**，留下"单例存在但注册表全丢"的泄漏宿主；新增 `UIModule.TryGetExisting(out UIModule)`，与 `RemovePanelRecord` / `RemoveWindowRecord` 的静态早退口径一致
- **`EventModule.InvokeEvent` 改走非创建式获取** — 订阅表是实例字段，"不存在实例"等价于"不存在任何订阅者"，分发无事可做；原先经 `Instance` 会在场景卸载/退出等时机重建宿主（同上）。订阅侧（`AddListener` 等）仍需创建语义，不受影响
- **`SceneModule.UnloadScene` 的"最后一个场景"守卫改用真实场景数** — 原先直接用 `SceneManager.sceneCount <= 1`，而该值把 `DontDestroyOnLoad` 伪场景计入；本框架默认 DDOL 常驻（宿主 / `EventModule` / `AudioModule` 都在 DDOL），真实场景只剩一个时 `sceneCount` 通常为 2 → 守卫不触发，回落到引擎原始报错直接打进用户 Console。新增 `CountLoadedRealScenes()` 按场景逐个统计（排除 DDOL）
- **`UIModule` 停用模板实例化改 `instantiateInWorldSpace: false`** — 模板克隆体取预制体自身的局部变换，不因容器位置产生偏移；同时在 XML 中披露该方案的常驻开销（每个实例化过的预制体保留一份完整层级克隆体，且因持有预制体引用而不被 `Resources.UnloadUnusedAssets` 回收；生命周期随 `UIModule` 所属 GameObject）


## [0.30.0] - 2026-09-27

### Changed

- **`AesirDependencyInstaller` 补装 URL 常驻分支化** — 缺 Aesir Architecture 时一键补装的 Git URL 由「本包 version 动态拼接版本分支」改为固定常量 `https://github.com/yuumixcode/AesirFramework.git#AesirArchitecture-latest`（锚定 CI 常驻滚动分支——旧策略的版本分支随发版轮换并删除，拼接出的 URL 会失效报 "Could not clone"）；移除版本推导函数（`ReadSelfVersion` / `BuildDependencyBranchName` / `BuildDependencyGitUrl`）与 `FallbackSelfVersion` 兜底常量（发版不再需要同步 bump），安装确认框与收尾日志文案同步；`AesirDependencyInstallerTests` 以常量 URL 守卫用例替代版本拼接用例（断言 URL 不含版本号）
- **README 安装指引改锚常驻 `latest` 分支** — 仓库分支策略重构（详见根 CHANGELOG [Unreleased]）：UPM（Git URL）安装 URL（含 manifest.json 示例）改为 `latest` 分支，升级口径同步为「移除后用同一 URL 重新添加，无需随发版修改」

## [0.29.0] - 2026-09-27

### Added

- **`package.json` 新增 UPM 元数据链接字段** — `documentationUrl` 指向文档站 Modules 分区、`changelogUrl` 指向文档站更新日志页：UPM（Git URL）安装后在 Package Manager 包详情页出现「View documentation」「View changelog」链接

## [0.28.0] - 2026-09-27

### Removed

- **`package.json` 依赖声明整段移除（破坏性）** — 两项均实测定案：① Aesir Architecture 的 Git URL 依赖条目——Unity Package Manager 不支持在包内声明 Git URL 依赖（仅项目 manifest.json 可声明，Unity 官方硬规定），保留它会使 UPM 单独安装本包直接失败（`Version 'https://...' is invalid. Expected a 'SemVer' compatible value.`）；移除后单装可成功，缺 Aesir Architecture 时经菜单 `Tools → Aesir → Modules → Install Dependencies` 一键补装，安装教程改为两包分别添加；② `com.unity.test-framework` 硬依赖——测试程序集已有 `UNITY_INCLUDE_TESTS` 守卫（未安装 Test Framework 时整体排除），硬依赖非必需，只会把未安装 Test Framework 的项目强行拉入该包

### Changed

- **测试程序集收敛（破坏性）** — 每包只保留 `xxx.Tests.Editor`（EditMode）与 `xxx.Tests`（PlayMode）两个测试程序集，Test Runner 不再出现第三个程序集折叠节点：`Runestone.AesirModules.Scene.Tests`（`Editor/Scene/Tests/`）并入包级 EditMode 程序集并迁移至 `Tests/Editor/Scene/`（文件 GUID 不变）；包级 EditMode 程序集 `Runestone.AesirModules.Tests` 改名 `Runestone.AesirModules.Tests.Editor`（对齐 RAA 命名模式，asmdef 移至 `Tests/Editor/`）；PlayMode 程序集 `Runestone.AesirModules.Tests.Runtime` 改名 `Runestone.AesirModules.Tests`（命名空间同步去掉 `.Runtime` 后缀）；5 处 `InternalsVisibleTo` 目标名同步更新
- **Scene 模块设置双窗口与无 Odin 编译修复** — `Editor/Scene/` 的设置类曾直接 `using Sirenix` 且无守卫（汇入主编辑器程序集 `Runestone.AesirModules.Editor`），无 Odin 消费者环境（unitypackage / UPM 均然）下该程序集编译失败，「Odin 可选」宣称不成立——此前开发仓装 Odin 从未暴露，UPM E2E 实测 108 个 CS0246 实锤。修复形态：①`SceneEditorSettings` 留主程序集作纯数据层（Odin 展示特性整体 `#if ODIN_INSPECTOR` 包裹，未装 Odin 编译剔除，数据读写零变化）；②Odin 版窗口 `SceneModuleSettingsWindowOdin`（原 `SceneManagerWindow`）迁至 `Editor/Scene/OdinInspector/` 经 asmref 汇入 ODIN 守卫程序集（Editor.OdinInspector asmdef 补 `Runestone.AesirModules.Editor` 引用）；③新增原生 IMGUI 兜底 `SceneModuleSettingsWindow`（持菜单入口，`OdinWindowOpener` 静态委托路由，与包内更新器双窗口同款模式），未装 Odin 时展示等价信息量（两个开关 + 两个只读路径 + 手动搜集按钮）。菜单 `Tools → Aesir → Modules → Scene Editor Settings` 更名 **`Scene Module Settings`**（窗口标题同步）。验证：无 Odin 环境 file: 双包 E2E 0 编译错误（修复前 108 个）、主项目 Odin 窗口真实绘制通过
- **`Install Dependencies` 补装菜单适用范围扩展** — 除 unitypackage 只导入本包外，UPM 单独安装本包（缺 Aesir Architecture、核心程序集编译失败）同样触发该菜单一键补装；包内注释与 README 口径同步（此前「UPM 自动拉取依赖」的宣称失实——Unity 不支持包间 Git URL 依赖）

## [0.27.1] - 2026-09-27

### Fixed

- **UniTask 集成的程序集名错误（asmdef 引用名与宏维护器检测名，0.27.0 引入）** — UniTask 的命名空间名 `Cysharp.Threading.Tasks` 被误当作程序集名使用（com.cysharp.unitask 包内 asmdef 的实际程序集名为 `UniTask`），两处受害：①核心与适配程序集的 asmdef `references` 以该名引用 UniTask——解析不到任何程序集，含 UniTask 的消费工程刷新即报 `CS0246: 'Cysharp' / 'UniTaskVoid' could not be found`（versionDefines 正常给宏，`#if AESIR_MODULES_UNITASK` 分支参与编译才暴露引用失败）；②`AesirUniTaskDefineKeeper` 按该名检测域内程序集恒为 false——unitypackage / DLL 安装形态下已装 UniTask 的工程全局宏反被误删，UniTask 分支与适配程序集静默失效零报错（UPM 安装形态宏由 versionDefines 管理、维护器不干预，该 bug 休眠，故多数工程未察觉）。现 references 改按程序集名 `UniTask` 引用；检测改为程序集名白名单（`UniTask`——asmdef 源码 / unitypackage 形态；`Cysharp.Threading.Tasks`——NuGet 预编译 DLL）。`AesirUniTaskDefineKeeperTests` 新增命名守卫 2 用例（白名单含真实程序集名 + 两处 asmdef 引用锁定），共 7 用例

## [0.27.0] - 2026-09-27

### Added

- **Scene 模块 UniTask 适配（新程序集 `Runestone.AesirModules.UniTask`）** — 游戏工程包含 UniTask 时自动生效：①内部加载/卸载流程由协程驱动替换为 UniTask 驱动（`UniTask.NextFrame` / `ToUniTask`，宿主销毁经 `destroyCancellationToken` 静默中止，公开 API 与回调语义与协程路径完全一致，共享校验与完成记账）；②适配程序集（`Runtime/Integration/UniTask/`，宏关闭时整体不编译）提供可 await 的 `SceneModuleUniTask` 静态 API：`LoadSceneSingleAsync` / `LoadSceneAdditiveAsync` / `UnloadSceneAsync`（含 `SceneAssetWrapper` 重载）/ `ReloadSceneAsync` / `UnloadAllAddedScenesAsync`——失败抛 `InvalidOperationException`（原因见 Console），`CancellationToken` 取消仅中止等待、底层流程继续完成，宿主销毁时等待方以取消收场不悬挂。宏 `AESIR_MODULES_UNITASK` 自动维护：UPM 包（`com.cysharp.unitask`）安装经核心与适配程序集的 versionDefines 装卸自动生效；unitypackage / DLL 安装经编辑器宏维护器 `AesirUniTaskDefineKeeper`（`Editor/UniTask/`，`[InitializeOnLoad]` + `delayCall` 推迟写宏防重入）按域内 `Cysharp.Threading.Tasks` 程序集在场与否增删全局宏。新增 `AesirUniTaskDefineKeeperTests`（5 用例：决策矩阵 + 适配程序集装配与宏存在性等价守卫）

- **UI 模块全局配置资产 `UIModuleConfigSO`（单例）** — 将模块级配置（当前承载窗口蒙版调度模式 `maskMode`）从 `UIModule` 序列化字段迁出：在 Project 窗口直接编辑资产即可生效，不再要求预放置 `[UIModule]`。单例解析顺序：①`RegisterConfigLoader` 注册的加载器（注册后 Resources 兜底不再执行，供彻底放弃 Resources 的项目；重复注册 fail-fast，`UnregisterConfigLoader` 幂等注销并使已缓存实例失效）；②Resources 兜底（`Resources/UIModuleConfig/UIModuleConfig`）；③内存默认实例。编辑器在编辑模式域加载后自动创建兜底资产（`UIModuleConfigAssetInitializer`，Resources 路径已命中或项目内已有同类型资产时不重复创建）。新增 EditMode 测试 `UIModuleConfigSOTests`（10 用例）

- **场景模块全局配置资产 `SceneModuleConfigSO`（单例）** — 承载无需预放置 `[SceneModule]` 即可调整的模块级配置，设计对齐 `UIModuleConfigSO`。单例解析顺序：①`RegisterConfigLoader` 注册的加载器（注册后 Resources 兜底不再执行，供彻底放弃 Resources 的项目；重复注册 fail-fast，`UnregisterConfigLoader` 幂等注销并使已缓存实例失效）；②Resources 兜底（`Resources/SceneModuleConfig/SceneModuleConfig`，编辑器在编辑模式域加载后自动创建兜底资产 `SceneModuleConfigAssetInitializer`，Resources 路径已命中或项目内已有同类型资产时不重复创建）；③内存默认实例。当前承载：全局启动场景兜底 `bootstrapScene`（预放置实例的序列化字段未赋值时 `BootstrapSceneAssetWrapper` 回退读取本值）与加载进度归一化上限 `progressCap`（默认 0.9）。新增 EditMode 测试 `SceneModuleConfigSOTests`（11 用例）

- **Script Doc Generator 静态 API（`ScriptDocGeneratorAPI`）** — 面板操作的无 UI 等价入口，面向自动化脚本与 AI 助手直接调用：`GenerateDocsForType` / `GenerateDocsForTypes` / `GenerateDocsForAssembly`（支持程序集短名或 FullName）/ `GenerateDocsForFolder`（文件夹含子文件夹内全部脚本，源码扫描映射类型，普通 C# 类不依赖 `MonoScript.GetClass()`）；配套 `DefaultSettings` / `ZensicalSettings` / `DefaultOutputFolder` / `FindAllSettings`（枚举项目内全部生成器设置资产，含自定义派生）。全程无确认弹窗、不自动打开生成结果，覆盖语义与面板"多程序集模式"一致（增量合并保留手写内容）；返回 `ScriptDocGenerationResult`（写入文件清单 / 设置名 / 输出根目录 / 未解析类型名，`ToString` 出单行摘要）。写入核心自面板流程下沉为 `WriteTypeDocSilently`（面板交互行为不变）。新增 EditMode 测试 `ScriptDocGeneratorApiTests`（16 用例：程序集解析 / 路径归一化 / 文件夹类型解析 / 三种来源端到端 / 预设枚举）

### Changed

- **`SceneModule` 公开 API 静态门面化（破坏性）** — 全部公开成员改为静态，直接 `SceneModule.LoadSceneSingle(...)` 调用（首次调用自动创建/查找单例），不再需要 `SceneModule.Instance.xxx`（`Instance` 属性保留供组件级访问）；实例侧成员全部私有化，类型不再暴露任何公开实例 API（EditMode 守护用例锁定）。顺带修正两处违反「MonoBehaviour 运行状态用显式非序列化字段」约定的自动属性：`LastLoadedScene` 与 `SceneLoadedEvent` / `SceneUnloadedEvent` 转为显式字段（此前自动属性 backing field 会被场景序列化残留，跨 Play 污染状态）；`SetActiveScene` / `ReloadScene` 为纯静态操作，不会创建模块实例

- **窗口蒙版模式的配置来源迁移至 `UIModuleConfigSO`** — 移除 `UIModule` 的 `maskMode` 序列化字段（破坏性：预放置实例上已序列化的 `maskMode` 值不再读取，升级后以配置资产为准，未创建配置资产时为代码默认值单遮）；`UIModule.MaskMode` 属性保留，初值在首次访问时取自 `UIModuleConfigSO`，运行时切换语义不变（只覆盖内存值并立即重算蒙版）

- **`SceneModule` 加载进度归一化上限的来源迁移至 `SceneModuleConfigSO`** — 内部常量 `SceneLoadProgressCap`（0.9）改为配置资产字段 `progressCap`（默认 0.9，消费端钳制到 (0, 1] 防止误配置造成除零或反向进度），每次加载时读取、协程与 UniTask 两条驱动路径共用；`bootstrapScene` 序列化字段保留且预放置实例优先，未赋值时新增配置资产全局兜底（均为非破坏性增强）

- **移除 `AesirModules` / `UIModule` / `AudioModule` / `SceneModule` / `UIRoot` DDOL 关闭时的运行时 Warning 提醒日志** — 非 DDOL 提示完全由 Inspector 承担（Odin 信息框），运行时不再输出日志，避免「不支持多场景叠加」的观感误导；XML 文档同步

- **DDOL 开关字段前移至类声明首位** — `AesirModules` / `AudioModule` / `SceneModule` / `UIModule` / `UIRoot` 的 `dontDestroyOnLoad` 序列化字段统一移至类体第一个字段（常量与静态字段之前）；`SceneModule`（自定义启动场景之前）与 `UIRoot`（Canvas 统一配置之前）的 Inspector 展示顺序随之变化，DDOL 决策均为第一项

- **Script Doc Generator「调试检查模式」重排至窗口最底部并完善提示** — 开关由窗口顶部（路径设置之后）移至面板最底部，TypeData 中间结果列表位于开关下方（开启后渲染）；按状态三态提示：未开启时显示 Info 指引（仅当需要检查中间过程的 TypeData——即单个成员的解析结果——才开启，日常生成无需开启）；开启时显示 Info 说明（窗口显示类型分析中间产物 TypeData，可展开检查每个成员的解析结果）；开启且程序集模式时追加 Warning 性能警示（分析的类足够多时整图渲染明显卡顿）

### Fixed

- **Script Doc Generator 调试检查模式关闭时 TypeData 中间结果列表仍显示** — `_typeData` / `_typeDataList` 此前并挂两个 ShowIf（调试开关 + 类型来源模式），而 Odin 对同一成员的多个 ShowIf 是 **OR 语义**（任一条件满足即显示）——类型来源模式条件在对应模式下恒真，导致列表无视调试检查开关直接渲染（多程序集模式下几百个类型的整图常驻窗口）；现合并为单个复合条件属性 `ShowSingleTypeAnalysisData` / `ShowListTypeAnalysisData`（AND 语义），仅在开启调试检查模式且对应类型来源模式时渲染
- **Script Doc Generator 面板"调试检查模式"的警告信息框表达式解析错误** — 可见性表达式误写 `@$value.ShowAssemblyDebugWarning`：InfoBox 挂在 bool 字段上，Odin 的 `$value` 指该字段自身的布尔值，绘制时报 `Unable to locate identifier 'ShowAssemblyDebugWarning' in context of type 'System.bool'`（面板截图中红色报错）；改为 `@ShowAssemblyDebugWarning` 在根实例上下文解析成员
- **Script Doc Generator 面板在绘制回调内解析资产库单例导致的卡顿与 "GUIStateObj is deleted" 报错** — `ScriptDocGeneratorWindow.DrawEditor` 此前每次绘制都解析 `ScriptDocGeneratorPanelSO.Instance`（未命中时触发 `CreateAsset` + `AssetDatabase.Refresh` 全项目重扫，卡死编辑器并产出 "the GUIStateObj is deleted, but is accessed"）；现 `DrawEditor` 只做空值保护、单例解析收敛到 `OnEnable`，`ScriptDocGeneratorPanelSO` / `DefaultScriptingAPISettingsSO` / `ZensicalScriptingAPISettingsSO` 三个单例改为 static 字段按域缓存（Unity fake-null 语义，资产被删后缓存自动失效重建），配套测试程序集补 `UnityEditor.TestRunner` 引用
- **Script Doc Generator 窗口的 PropertyTree 未释放** — `ScriptDocGeneratorWindow._soTree` 在域重载 / 窗口销毁时未 Dispose，Odin 在下次 GC 时报 "An Odin PropertyTree instance is being garbage collected without first having been disposed"；现于 `OnDisable` 释放并置空

## [0.26.0] - 2026-09-25

- **与 Aesir Architecture 0.26.0 版本同步发布** — 本包无功能变更；Architecture 侧升级包内更新器（版本检测改为「直连 GitHub → 镜像站 → CDN 中转」三层兜底并显示获取线路、检测与下载补齐超时、修复两个包连续更新时进度条停留与按钮提前可点的交互问题）

## [0.25.1] - 2026-09-25

### Fixed

- **PlayMode 场景套件把测试场景常驻 EditorBuildSettings（并被打进玩家构建）** — 此前经 `[InitializeOnLoadMethod]` 在编辑模式域加载期把两个测试场景登记为 enabled 条目且从不摘除，条目随 `ProjectSettings/EditorBuildSettings.asset` 落盘、进入玩家构建（本仓库该文件已入库这两条，本次一并清除）。现改为 `IPrebuildSetup` 在进入 Play 前的编辑模式阶段登记、`IPostBuildCleanup` 退出 Play 后按名摘除——条目仅存在于本次运行期间，不进构建也不污染宿主工程配置；另有域加载兜底清扫回收被强杀运行（编辑器崩溃、进程被 kill）遗留的条目。新增 EditMode 守护用例 `SceneModuleTestSceneHygieneTests` 锁定「测试场景不得常驻 BuildSettings」
- **SceneModule 测试无法随包进入实际工程** — ①`SceneAssetWrapperTests` 硬依赖宿主工程存在 `Assets/Scenes/SampleScene.unity`（Unity 默认模板路径，消费工程可能已删除它），缺失时 `FromScenePath` 直接抛 `SceneAssetWrapperCreationException`：现由 SetUp 在缺失时从包内最小场景夹具（随测试分发）临时复制一份、TearDown 按“谁创建谁删除”还原（含本次创建的空目录），宿主工程原有场景一律不动（不就地新建场景：编辑器在“当前打开场景未命名未保存”时拒绝追加式新建，batchmode 下必现）；②PlayMode 套件把测试场景路径写死为 `Assets/Runestone/AesirModules/Tests/Runtime/TestScenes/…`，包以 UPM 形态安装（`Packages/…`）时该路径不存在：现按文件名经 AssetDatabase 定位，Assets 安装 / 嵌入式包 / UPM Git 安装三种形态自适应；③`SceneAssetWrapperTests` 的 `WrapperCsPath` 同类硬编码一并改为按文件名定位（原先在 UPM 形态下静默失效，非场景 GUID 用例变成空 GUID 假通过）

## [0.25.0] - 2026-09-25

### Fixed

- **Script Doc Generator 增量重生成产出双 Front Matter** — Zensical 生成器自产 YAML 头后，`ScriptDocGeneratorUtility` 的增量合并逻辑（`TryGetFrontMatter` 拼接）会把旧文件的 Front Matter 再拼一份到新内容前，对已存在文档重生成必然产出双重头部（Zensical 解析失败）；合并逻辑收敛为 `MergeFrontMatterWhenMissing`：新内容自带 Front Matter 时以新生成的为准，不自带（如中文 API 生成器）仍保留旧文件头部。新增 EditMode 回归测试 `FrontMatterMergeTests`（4 用例：自带头部不拼接 / 不自带保留旧头部 / 旧文件无头部原样 / 无闭合不拼回）
## [0.24.0] - 2026-09-25

### Added

- **UI 模块新增 Canvas 根窗口形态（`IUIWindow` / `AesirBaseWindow` 家族）** — 与 Panel（共享层 Canvas 的面板）并列的第二种 UI 形态：窗口预制体根节点自带 Canvas，由 `UIModule` 实例化后直接挂载到 UIRoot 下（不经四层 Canvas），默认 `sortingOrder` 500 恒在面板四层（≤400）之上，多窗口按声明值自治排序。命名规范：类名 = 脚本名 = 预制体名以 `Window` 结尾（`LoadingWindow` 式；按 .NET 命名惯例 `XxxCanvas` 后缀会误导为 `UnityEngine.Canvas` 派生类，故取运行时零占用的 Window）；生命周期与面板同构（`OnInit` / `OnShow` / `OnHide` / `OnClose`，Awake/OnEnable 推迟到首次激活）。静态快捷 API：`UIModule.Open<T>()` / `Close<T>()` / `GetWindow<T>()` / `PrewarmWindow<T>()` / `RegisterWindowPrefab<T>()`（实例侧为 `OpenWindow` / `CloseWindow` 等长名）；挂载时统一接线 UICamera / 渲染模式 / Canvas 缩放配置并递归设置 UI 层，根缺 Canvas 时报错中止、不保留半挂载实例。预制体内部结构约定：`Mask` 子物体为蒙版（Image 全屏拉伸 + 可选 Button），`Content` 子物体为实际 UI 元素容器
- **窗口蒙版遮罩机制（单遮 / 叠遮）** — `UIModule` 新增序列化配置 `maskMode`（运行时可经 `MaskMode` 属性切换）：单遮 = 全局仅最高层可见窗口的蒙版生效（多窗口叠加透明度不叠加），叠遮 = 各窗口蒙版独立跟随自身打开状态；每次 Open / Close / 销毁后重算，同 sortingOrder 时以后开者居上。蒙版点击经 Button 接线回调 `AesirBaseWindow.OnMaskClicked()`，默认按 `closeOnMaskClick` 决定是否关闭本窗口（子类可覆写自定义行为）；无 `Mask` 子物体的窗口天然不参与遮挡
- **Binder 支持 Canvas 根窗口感知** — 基类预选下拉新增 `AesirBaseWindow` / `AesirBaseWindowView<T>` / `AesirBaseWindowViewController<T>`；根节点带 Canvas 时默认脚本名后缀为 `Window`（面板根保持 `Panel`）、默认基类直指 `AesirBaseWindow`；物体名已带对应后缀时不再重复拼接（顺带修复 `ScorePanel` 物体生成 `ScorePanelPanel` 的存量双后缀瑕疵）
- **新增示例 UI Basic Usage（`Samples/UI/01_BasicUsage`）** — 面板与窗口两种形态协作：Normal 层控制面板 + Top 层日志 HUD（跨平台中文动态字体）+ 设置/叠加窗口（蒙版 + 点击蒙版关闭）+ 全屏加载窗口（无蒙版 + payload 自动关闭），支持运行时切换单遮/叠遮对照；已登记 package.json samples（Package Manager Samples 页可导入）
- **UI 模块窗口与蒙版 EditMode 测试** — `UIModuleWindowTests` 17 用例（挂载接线 / sortingOrder / UI 层递归 / 生命周期契约 / Close 双分叉 / 键语义 / Panel↔Window 跨契约互斥 / 根缺 Canvas 中止 / 反清理 + 蒙版单遮重算 / 同序 tie / 叠遮独立 / 点击蒙版 / 无 Mask 子物体无操作）+ `BinderAssistantWindowTests` 5 用例（默认脚本名后缀 / 默认基类 / 后缀去重 / 基类下拉窗口家族）
- **缺依赖一键补装（`AesirDependencyInstaller`）** — unitypackage 导入形态下本包存在但 Aesir Architecture 缺失时，新菜单 `Tools/Aesir/Modules/Install Dependencies`（priority 998，Modules 组内第一）确认后经 UPM `Client.Add` 按 Git URL 版本分支自动补装。安装 URL 由本包 package.json 的 `version` 动态拼接（包根经 `[CallerFilePath]` 定位，两包同号发版保证对齐，异常回退内置常量）；RAA 缺失检测覆盖 UPM 注册表（`PackageInfo.GetAllRegisteredPackages`）与 Assets 形态（`AesirPathLookup` 锚点定位包根后按 package.json `name` 字段精确判定，避免依赖键名子串误判）；安装成功收尾经 `SessionState` 标记 + `[InitializeOnLoadMethod]` 跨域重载提示（`Client.Add` 触发的编译域重载会吞静态字段与后续执行，失败则即时弹窗）。配套：本包 package.json 对 RAA 的依赖声明由 semver 版本号（`"0.23.0"`——该包 id 不在 Unity Registry，UPM 无法解析、README「自动拉取依赖」宣称失实）改为 Git URL 版本分支（`#AesirArchitecture-v<版本>`），UPM 安装本包时自动递归拉取 RAA；新增零引用编辑器程序集 `Runestone.AesirModules.Editor.Bootstrap`（references 为空数组：若引用 RAM 核心 / RAA / Odin，缺依赖时菜单自身将不编译）。EditMode 测试 14 用例（URL 拼接 / 锚点路径推导 / 包根判定 / UPM 注册名矩阵 / RAA 在场不显示）

### Changed

- **Script Doc Generator 菜单让出 Tools/Aesir 顶部位并归入 Modules 组** — priority 由 -895 改为 999（顶部位让给 Aesir Architecture 的 Aesir Getting Started，其下有独立分割线）；菜单路径由 `Tools/Aesir/Script Doc Generator` 归位至 `Tools/Aesir/Modules/Script Doc Generator`——两包专属菜单项以 `Tools/Aesir/Architecture/`、`Tools/Aesir/Modules/` 两组子菜单区分；999 决定 Modules 组的组级排序（父菜单 priority 由子项最小值决定），与相邻组差值 ≤ 10 不产生分割线
- **Scene Editor Settings 菜单归入 Modules 组** — 菜单路径由 `Tools/Aesir/Scene Editor Settings` 归位至 `Tools/Aesir/Modules/Scene Editor Settings`（默认 priority 1000，组内位于 Script Doc Generator 之后）
- **Odin / Addressables 细分程序集锚点迁至 `Integration/`** — 两者性质为第三方适配/集成而非共享基础设施，锚点由 `Runtime/Common/OdinInspector/`、`Editor/Common/OdinInspector/`、`Editor/Common/Addressables/` 迁至 `Runtime/Integration/OdinInspector/`、`Editor/Integration/OdinInspector/`、`Editor/Integration/Addressables/`（挪空的 `Editor/Common/` 删除；`Runtime/Common/` 保留宿主与调试工具）；各模块内 `OdinInspector/`、`Addressables/` 子目录与 asmref 汇入模式不变，程序集名与相互引用零变化，对 UPM / unitypackage 消费者透明
- **示例 `KeyPressedEvent` 脚本 `using` 指令移入 `#if UNITY_EDITOR` 内** — 对齐全部示例脚本的统一包裹形态（运行时示例程序集整文件包裹，`#if` 之外不残留 using），由 Aesir Architecture 本批新增的示例脚本守护测试驱动修正；纯规范偏差，该类型本就不参与玩家构建

### Planned（下期候选）

- **SmartShowHide 伪隐藏** — 全屏窗口弹出时自动伪隐藏被其遮挡的全部面板（CanvasGroup 置零、逻辑上仍为显示中、窗口关闭后自动恢复），把不可见面板从渲染管线剔除以省满帧重绘（思路来自 ZMUIFrameWork）；本批未实现，待窗口蒙版机制经实际项目验证后再评估

## [0.23.0] - 2026-09-23

> 本批为全仓锐评修复批次（与 Aesir Architecture 0.23.0 同步发布；Architecture 侧含可观察集合破坏性变更，升级时请一并阅读其 CHANGELOG 的 Removed / Renamed 节）。

### Added

- **Binder 代码生成器同类型多组件测试** — `BinderCodeGeneratorTests` 新增 2 用例：同一物体多个同类型组件时首个单元保持 `GetComponent<T>()` 零开销、后续单元按出现序号生成 `GetComponents<T>()[n]`（含绑定自身路径），锁定错绑修复
- **SceneModule 批量卸载重入回归（PlayMode）** — `SceneModulePlayModeTests` 新增用例：卸载广播的监听者回调内再调 `UnloadAllAddedScenes`，内外两层各自完整完成、全部场景卸载、追踪清空（修复前内层 `Clear` 会截断外层迭代导致漏卸 + 误报完成）

### Fixed

- **BinderTag：Missing 脚本组件导致空引用** — `GetComponents<Component>()` 对 Missing MonoBehaviour 返回含 null 的数组，`Types` 属性的 `is not BinderTag` 放行 null 后在 `GetType()` 抛 NRE（绑定入口对缺脚本物体必炸）；现过滤 `null or BinderTag`
- **BinderAssistant：增量模式自动挂载静默失效** — 类型全名此前直接拼 UI 配置的 `TargetNamespace`，而增量模式下目标文件已存在时该字段不校验也不写回，开发者事后改过 namespace 即错位 → 编译后类型解析失败、自动挂载静默丢失；现从目标文件实际声明的命名空间解析（文件不存在 / 未声明时回退 UI 配置）
- **BinderCodeGenerator：同类型多组件错绑** — 生成代码统一 `GetComponent<T>()` 取首个命中，同物体两个同类型单元会解析到同一实例（宣称支持多组件绑定却静默错绑）；现按单元出现序号生成 `GetComponents<T>()[n]`；顺带删除 `BuildBindStatement` 中两支完全相同的 `isSelf` 死分支
- **BinderCodeGenerator：partial 模式重生成非幂等** — 生成文件头部的 `DateTime.Now` 时间戳在每次重新生成时产生 diff（partial 模式整体覆盖）；移除该时间戳行恢复幂等（增量模式 region 本就无时间戳）
- **SceneModule：批量卸载重入洞** — `_unloadSnapshotBuffer` 为实例级复用缓冲且无重入保护：卸载广播的监听者回调内再调 `UnloadAllAddedScenes` 时，两层协程迭代同一 List，内层 `finally Clear` 清空外层正在迭代的列表 → 外层提前退出、剩余场景漏卸而 `onAllUnloaded` 误报完成；现照 EventModule 范式加重入深度计数，重入层改用局部快照
- **UIModule / AudioModule / EventModule：重复实例销毁粒度** — Awake 重复实例处理由 `Destroy(gameObject)` 改为 `Destroy(this)`（对齐 SceneModule 与 RAA 先例）：组件级模块运行时创建于 `[Aesir Modules]` 宿主下，`Destroy(gameObject)` 会连带销毁宿主整树与其他模块；宿主 `AesirModules` 自身（根物体）保持 `Destroy(gameObject)`
- **UIModule：`PrewarmPanel` 缺键语义守卫** — 以基类类型 Prewarm 时可重复实例化并覆盖 `_panelDict` 键、泄漏旧实例；现补 `FindRegisteredRelatedPanelKey` 守卫（与 `ShowPanel` 三处同款诊断）
- **SDG：`XmlCodePart.GetSummaryAttributeText` 缩进正则** — `^\s*` 含换行，xml 块以空行开头时得到跨行「缩进」导致注入行错位；改为 `^[ \t]*`

### Changed

- **装配守卫补齐** — `Runestone.AesirModules.Tests` / `Runestone.AesirModules.Tests.Runtime` / `Runestone.AesirModules.Scene.Tests` 三个测试 asmdef 的 `defineConstraints` 追加 `ODIN_INSPECTOR`（此前硬列 4 个 Sirenix DLL 并引用 Odin 程序集却无守卫——无 Odin 消费者环境启用 Test Framework 即编译失败，与「装了自动出现、卸了整体消失」承诺矛盾；对齐 RAA Tests/Runtime 范本）
- **Odin 守卫口径统一** — `Runestone.AesirModules.Editor.OdinInspector` 的 `defineConstraints` 移除 `AESIR_ARCHITECTURE`（与 Runtime 侧 Odin 程序集对齐，同守 `ODIN_INSPECTOR`；宏确保器维护的宏不参与装配语义，消除「半套 Odin」的口径缝隙）
- **`ui-module.md` 补两条边界声明** — 「Binder 仅服务编辑期构建」（运行时 AddComponent 创建的面板不做自动绑定，无运行时重绑兜底）与「Binder 生成产物使用 Odin 特性」（使用 Binder 的前提为已安装 Odin Inspector）

### 规划中

- 对象池扩展（当前用隐藏复用，必要时增加 UIForm 对象池）

## [0.22.0] - 2026-09-22

### Changed

- **与 Aesir Architecture 0.22.0 版本同步发布** — 本包无功能变更；Architecture 侧为可观察集合大版本升级（收敛单轨变更通知并移除内部加锁），升级时请一并阅读其 CHANGELOG 的 Removed（破坏性变更）节

### 规划中

- 对象池扩展（当前用隐藏复用，必要时增加 UIForm 对象池）

## [0.21.0] - 2026-09-14

### Fixed

- **事件模块：重入分发覆写共享参数数组** — 分发是同步的，订阅者回调内再发布任何事件（同型或异型）时，内层 `RaiseEvent` 覆写共享参数数组，外层循环继续时剩余订阅者收到内层事件参数（编译委托抛 InvalidCastException 被吞成错误日志、动态绑定打"参数类型不匹配"）；现重入层使用独立局部参数数组、局部迭代列表与局部死绑定收集，外层分发不受干扰；性能计时仅对顶层分发生效（重入层不计时，避免嵌套 Restart/Stop 互相破坏计时）
- **事件模块：分发中退订跳过订阅者** — 分发改为注册表快照迭代（顶层复用迭代缓冲区，Clear 保留容量稳态零分配）：回调内退订/注册只修改注册表本身，本趟按快照执行完毕，不再出现"退订后续订阅者导致跳过一个"的索引位移（退订者本趟仍收到、新订阅者从下趟生效）
- **事件模块：`WithTag` 构造拒绝 null 与空串** — `CompareTag("")` 语义无意义，按 fail-fast 在构造期抛 `ArgumentNullException` / `ArgumentException`
- **音频模块：补 `ResetStatics` 静态重置** — 非泛型单例按框架铁律在类内声明 `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]` 自重置（`AudioModule` 是 RAM 中唯一漏掉的单例），Disable Domain Reload 下异常中断不再残留 `_instance` 引用
- **音频模块：`PlayBgm` 幂等条件修正（淡出误伤）** — 此前 `StopBgm` 淡出进行中再 `PlayBgm(同曲)` 被幂等检查吞掉、淡出协程继续走到 Stop；现幂等早退追加"无进行中淡变协程"条件，淡出中重播同曲取消淡出并从当前系数续接（与连续切歌续接语义一致）
- **音频模块：`PlaySfx` 音调钳制到 [0.01, 3]** — `pitch` 与 `pitchJitter` 任意组合不再产生负音调（Unity 负 pitch 为反向播放）；下限取 0.01 而非 0（pitch = 0 时音源无声）
- **UI 模块：`ShowPanel` / `PrewarmPanel` 注册时序前移** — 注册表登记提前到 `Initialize()` 之前：`OnShow` 抛异常不再产生"激活但永不注册"的泄漏面板，`OnInit` / `OnShow` 内 `GetPanel<自身>()` 与递归 `ShowPanel` 同类型不再重复实例化
- **UI 模块：非泛型路径与预制体处理边界** — re-show 路径 `(MonoBehaviour)panel` 硬强转改 `as` + 判空报错（非 MonoBehaviour 的 IUIPanel 实现二次 Show 不再抛 InvalidCastException）；`InstantiateInactive` 恢复源预制体 `activeSelf` 增加 try/finally（Instantiate 抛异常不再留下被翻转的源预制体）；`RegisterPrefab` 同类型换路径调用时输出诊断警告（此前静默沿用旧路径）
- **场景模块：`UnloadAllAddedScenes` 快照迭代与 Single 成功路径校验** — 卸载广播期间监听者嵌套加载/卸载不再干扰本趟迭代（快照缓冲区复用，Clear 保留容量）；Single 加载成功后 `SetActiveScene` 前校验 `Scene.IsValid()`，无效场景记录错误不再抛异常
- **脚本文档生成：XML 实体双重转义** — `<summary>` 含 `List&lt;T&gt;` 等实体时，Summary 工具提取不解码、回写再转义，产生 `List&amp;lt;T&amp;gt;` 错误输出；现提取后解码实体、回写前再转义（两步互逆），含泛型实体的文档不再高频踩中
- **脚本文档生成：多成员代码块的 [Summary] 错删/错注** — 一个 `code` 块含多个成员时，工具按"整块 = 第一个成员"处理会删错/注错特性；现按"XML 注释结束后紧邻的首个成员声明"做归属分析，检测到非首成员持有 `[Summary]` 时 Sync/Replace/Remove 三模式均跳过该块并告警（fail-closed，不乱改用户代码）
- **脚本文档生成：杂项修复** — Remove 模式补 header 区（首个 XML 注释之前）的 `[Summary]` 清理；Sync 幂等比较前两侧同等空白压缩（仅空白差异不再触发无意义回写）；`EventData` 的 `GetAddMethod` 补空守卫（仅 remove 访问器的非常规事件不再 NPE）；合成方法过滤从名称前缀启发式改为 `IsSpecialName` + 声明类型存在同名事件/属性的精确判定（用户合法命名的 `get_Thing()` 普通方法不再被误杀）；`RemovedSummaryXml` 清除摘要删除后残留的孤儿空 `///` 行（只清剩余注释区首尾，标签间段落排版保留）；字段合成过滤收敛到 `GetUserDefinedFields` 单一真源（`TypeData` 不再重复过滤）；record 判定（`<Clone>$` 合成方法）与 record class/struct 统一映射 `TypeCategory.Record` 的设计边界以注释声明
- **UnityEventOnAesirEvent：未配置事件参数的早退路径显式归零句柄** — 不再依赖 `Dispose` 内部空安全，"句柄仅在成功订阅后非默认"的不变量对组件自身成立

### Changed

- **事件模块：优先级稳定排序** — 排序比较器以 `Priority` 为主键、注册顺序号 `BindingInfo.InsertionIndex` 为次键（对齐 RAA PlayerLoop 钩子排序范式）；同优先级订阅者的相对顺序从"无契约"收紧为"按注册顺序执行"
- **事件模块：摘除"实验性"标注** — 0.20.0 已具备订阅者过滤器、SO 资产化、性能模型与测试覆盖，分发正确性三缺陷（重入/快照/稳定排序）修复后，README 与模块文档删除"实验性"帽子
- **事件模块：文档口径重写** — 删除"约定不在回调内同步发布事件"的免责声明，改写为"快照迭代与重入安全"语义说明；"热路径稳态零分配"收敛为精确口径（零字符串分配/零装箱/零闭包；顶层分发零列表分配，排序比较器包装与重入层局部分配除外）
- **脚本文档生成：Default 生成器引擎合并** — `DefaultScriptingAPISettingsSO` 四个成员节各自重复的"三旗标探测 + 表头/行发射"收敛到与 Zensical 生成器共享的 `MemberGrouper` 分组引擎（常量 → 声明 → 继承 → 运算符），582 → 343 行，分组语义两生成器单源化；Default 输出格式经护栏测试锁定保持不变

### Added

- **测试扩充（全仓锐评盲区补齐）** — `EventModuleTests` 22→32（重入分发三层嵌套、快照退订/注册语义、同优先级注册序、Attribute+Dynamic 跨轨全序、Attribute 轨死引用清理、`WithTag` 空串/null、千订阅者信息性软门槛）；`AudioModuleTests` 30→38（`ResetStatics` 重置、淡出中 `PlayBgm` 续接与幂等、`SwitchBgm`/`StopBgm` 协程手动驱动、pitch 钳制边界）；`UIModuleTests` 13→17（OnShow 内递归 Show 不重复实例化、OnShow 抛异常不泄漏、Awake/OnEnable 推迟到 Show 激活的生命周期契约）；`XmlSummaryToolTests` 25→34（实体解码 roundtrip、多成员块矩阵）；`TypeDataTests` 补合成方法过滤用例；新增 `ZensicalScriptingAPIOutputTests`（7 用例，锁定 Front Matter/分组/详情链接——Default 引擎合并的前置护栏）
- **场景模块 PlayMode 测试套件（RAM 首个 PlayMode 程序集）** — 新增 `Tests/Runtime/`（`Runestone.AesirModules.Tests.Runtime`）覆盖 SceneModule 真实加载成功路径：Single 回调顺序（进度 1.0 归一化 → `SceneLoadedEvent` → onCompleted）、激活场景切换与追踪清空、模块 DDOL 存活、Additive 追踪与激活场景不变、`UnloadAllAddedScenes` 全量卸载、广播期间嵌套叠加的快照迭代语义（P2-S1 修复锁定）；测试场景为 `TestScenes/` 两个最小 .unity，经 `[InitializeOnLoadMethod]` 在编辑模式域加载期登记为 BuildSettings enabled 条目（PlayMode 内写登记表不被运行中的场景管理器采纳，disabled 条目运行时不可加载——均实测；BuildSettings 不随包分发，消费者不受影响）；Single 用例以 `[Order]` 固定末位执行（其会留下唯一已加载场景，先跑会污染后续用例）
- **音频模块：音量滑条"拖动结束落键"示范** — 音量 setter 每次赋值即写 PlayerPrefs，连续拖动逐帧落键属误用；模块文档补"拖动结束写入"指引，包内示例滑条改为鼠标松开一次性写入（`Samples~` 镜像同步）

## [0.20.0] - 2026-09-11

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
