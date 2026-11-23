# 架构裁决 / 模块定稿细节（索引条目：`framework-core`）

> 从主 `memory.md` 拆出的主题块。触发词：框架定位、极简、Quick、命名、日志、注释、Context、各模块补充、UPM、菜单 priority、AesirTimer、世界空间 UI、Timer 重构、AesirFrameworkGame 扩展、_Game 目录。
> 高频裁决（框架定位/极简/命名/日志/注释/目标铁律）仍在主索引；这里是展开与各模块"正文之外的定稿细节"。

## 框架定位与设计基调（正文「设计基调」章节的展开）

- [2026-11-08 02:40:00] **框架定位（最高优先级裁决）**：①面向**独立游戏**，精简 + 渐进式，受众是初学者与中级程序员；内置能力只留高频项，低频项写设计边界并引导上游。②**RAA 极简是第一排序标准**（低频公开 API 一律删/收窄），**RAM 功能性优先于极简**（功能模块的面是价值不是负担）。③核心架构默认**最低档 Quick**：不建 Model/Service 接口、不做只读 property 包装、直接持具体类、`Register*` 注册实现类型；只有确需多实现/替换才引入接口。④**Model = 运行时数据的底层数据源**，不是 UI 附属。
- [2026-08-15 22:20:13] **极简原则**：低概率问题、或由不推荐写法导致的问题，一律用文档约定杜绝，**不加防御性代码兜底**；异常不吞噬（fail-fast）；不做初始化回滚、不做自愈轮询。加任何"保护措施"前先问：防的问题概率多高？是否用户写错代码？隐性代价是什么？
- [2026-09-29 13:39:03] **跨包命名三分法**：跨包共用实现/框架级工件 → `Aesir` + 语义名不带包名段（`AesirMonoBehaviour` / `MiniEvent`）；两包各有一份同类角色 → 完整包名 `AesirArchitecture` / `AesirModules` + 语义名；单包自有且无对照 → 现状。
- [2026-09-29 13:20:45] **日志规范**：前缀加粗带色；**禁止连续两个带色前缀**（方括号框选前缀不在此列）；生产路径一律走门面（RAA `AesirArchitectureDebug` / RAM `AesirModulesDebug`）。三处有意例外勿"顺手修正"：`Editor/Bootstrap/AesirDependencyInstaller`（零引用程序集）、`Samples/`、`Tests/`。
- [2026-09-11 10:18:36] **注释口径**：只描述当前行为与"为什么"，**不写历史演进**；XML 注释中文；非直觉的性能优化用 remarks 写原理 + 量级对比。

## 各模块"正文之外的定稿细节"

- [2026-09-29 11:16:13] **Context 语义补充**：`Configure()` 期注册的模块由 `InitializePendingModules` 同轮补齐，**不做二次初始化**；`Dispose()` 收尾在 `finally` 且清 `_instance`（否则返回僵尸上下文）；`MiniEvent` 只有 Invoke 路径零分配，AddListener 有分配；能力墙上 `GetModel/GetService` 是 advisory（可经 `self.Context` 绕过），`ExecuteCommand/Query` 是硬墙；**VC 适配对声明 `IView, IController<T>`**（View 角色保持非泛型避免双 DIM 冲突）。
- [2026-09-25 12:37:07] **UI 模块补充**：**`OnClose` 只在受控销毁路径（Hide + DestroyOnHide）调用，外部销毁只有 `OnDestroy`** → 解绑必须放 `OnDestroy`。键语义三件套：同基类类型重复 Show 报错拒绝 / Hide·Get 未命中给 Warning / 精确 miss 幂等静默 / 缺层 fail-fast。面板克隆递归设 UI 层。`IUIAssetLoader` 是**同步契约**（Addressables 需预加载后同步返回）。
- [2026-10-15 16:18:01] **Scene 模块补充**：`SceneAssetWrapper` 的 GUID 锚点自愈、`IsDangling` 以"自愈后资产可定位"为准、判等与 `GetHashCode` **路径优先**；`SceneModuleConfigSO` 无 Odin 时有原生 PropertyDrawer + CustomEditor 兜底。
- [2026-09-11 12:18:42] **Events 模块补充**：`SubscriberPriority` 只有 4 档（First/High/Medium/Last）；过滤器 **fail-closed**、未过滤零开销；分发期死引用自动清理（编辑器告警、玩家构建静默）；`executionMsLimit` 默认 0 关闭；绑定键按 Type 缓存（编译委托 ~4ns vs `MethodInfo.Invoke` ~300ns）。
- [2026-09-11 10:33:51] **Audio 模块补充**：SFX 用 **N 个独占 AudioSource 轮询**（默认 8，不用 `PlayOneShot`、不每次 Instantiate）；`IsBgmPlaying/CurrentBgm` 非创建式，**音量/静音六个 getter 有意保持创建式**（配置在实例序列化字段上）；pitch 钳制 `[0.01, 3]`。
- [2026-09-27 10:45:00] **SDG 补充**：默认输出目录是**项目根 `ScriptDocGenerator/`**（不在 Assets 内）；单例按域缓存（if (_instance) 利用 fake-null），绘制回调只做空值保护；`MergeFrontMatterWhenMissing` 守卫重生成时双 Front Matter；Summary 工具语义 = **`[Summary]` 特性是权威源**。
- [2026-09-27 11:16:56] **更新器补充**：收尾顺序铁律 = **先 `UnlockReloadAssemblies` 再 `EndBusy`**（异常路径用 reloadLocked 局部标志配平）；`TimeoutBudget` 是墙钟判据（UnityWebRequest.timeout 只覆盖"完全无数据"，慢速滴水不触发）。
- [2026-09-21 19:41:29] **可观察集合补充**：写后通知 fail-fast；`Clear` 判空后必须直发 Reset（不可复用"元素 <2 不通知"守卫）。与上游 Cysharp.ObservableCollections **可共存**（同名公开类型被命名空间隔离，同文件双 using 裸引用会 CS0104，用别名）。**移植坑（C# 12 → 9）**：file-scoped namespace、record struct、主构造器、集合表达式全要降级；netstandard2.1 无 Unsafe/CollectionsMarshal；脚本去重 using 时只匹配 using 指令（曾误删 `using var`）。

## UPM / 打包 / 菜单

- [2026-09-27 20:49:04] **UPM 事实定案（实测）**：①**UPM 不支持包内 package.json 的 Git URL 依赖**（保留会让单装失败）→ 两包无 dependencies；RAA 缺席由 `Tools/Aesir/Modules/Install Dependencies` 补装。②双直装可行（同名直接依赖遮蔽间接依赖）。③UPM 包内测试程序集在消费者项目默认不编译（opt-in = Project Settings → Package Manager → Testables）。④asmdef 全用 name 引用，Assets RAM + UPM RAA 混合形态可编译。⑤UPM 升级无提示，升级 = 移除后重加。
- [2026-10-17 11:11:13] **菜单 priority 图景**：Getting Started(-980；-1000 会与 Tools/Odin/Getting Started 打平导致组序抖动) → Architecture 组(995) → Modules 组(999) → Export Packages(1000/1001/1002) → Check for Updates(1100 置底)；父组 priority 取子项最小值，差 >10 自动插分割线；新增 RAA 工具进 `Tools/Aesir/Architecture/`、RAM 工具进 `Tools/Aesir/Modules/`。

## AesirTimer / 世界空间 UI / 字体

- [2026-11-09 08:28:17] **AesirTimer 裁决**：RAA 只保留这一个计时器类型；**不复活** `AesirScheduler` 静态 Delay 形态——真实原因是 `Delay(s, cb)` 表达不出"可重复 + 可查询"两类真实用例（PlaneWar 全部计时都属于这两类）。四路径边界：MonoBehaviour 本地字段 / `MonoLifecycleProxy` / `AesirTimer` / 协程。参考 adammyhre/Unity-Improved-Timers **只借设计不借代码**（该包有未包裹 using UnityEditor、Start→Pause→Start 双倍 tick、Resume 假启动等缺陷）。
- [2026-11-19 02:09:22] **Timer 重构架构裁决（无破坏性）**：单一 AesirTimer 拆为 AbstractTimer（public 抽象基类，模板方法固定调度引擎 + 共有状态/查询/生命周期）→ AesirTimer（抽象门面，静态工厂 + 引擎委托）→ 六个具体类（Countdown/NextFrame/Repeating/Interval/Frequency/StopwatchTimer），具体类全部派生自 AesirTimer 以保住既有字段声明与调用零改动。Why: 要 SOLID 分层且不破坏 0.33.0 公开 API。How to apply: 新增计时形态继承 AbstractTimer 实现 Advance/Rearm 两钩子；调试面板 List 与 Snapshot.Capture 用 AbstractTimer 类型；分层思路致谢 adammyhre/Unity-Improved-Timers（仅学习来源，不进 Third Party Notices）。
- [2026-11-16 18:00:22] **世界空间 UI Canvas（0.32.0 远端合并 dd60c80）的 API 面与参数生效语义**：新增 `WorldSpaceUICanvas`（含 `WorldSpaceBillboardMode`、`WorldSpaceUICanvasAttributeProcessor`）、`UICanvasConfigSO.CreateWorldSpaceDefault()` / `ApplyToWorldSpaceCanvas(Canvas)`、`UIRoot.TryGetExisting()` / `WorldSpaceCanvasConfig` / `CreateWorldSpaceCanvas(Transform, string, UICanvasConfigSO)`；是世界空间第三种形态，不进 UIModule 注册表与蒙版调度。**生效语义**：`Awake → Build()` 一次性接线，`LateUpdate → ApplyTransform` 每帧读跟随目标/PositionOffset/FaceCamera/Billboard/FlipFacing（改即生效）；**配置与 sortingOrder 是接线期参数**——换配置需 `Build(overrideConfig)`，sortingOrder 为序列化字段（属性无 setter），Inspector 改动后同样要 `Build()` 才刷新。
- [2026-08-24 20:44:35] **跨平台中文动态字体（用户要求）**：不用 Mac 专有字体、不引入全局 TMP 字体资产；运行时 `Font.GetOSInstalledFontNames()` + `Font.CreateDynamicFontFromOSFont()`，候选 Microsoft YaHei → PingFang SC → Noto Sans CJK SC → WenQuanYi Zen Hei，全不可用回退 LegacyRuntime.ttf。示例中文 uGUI 一律走 `SampleUIFonts` 类组件；TMP 中文才走 TMPChineseFont skill（需用户授权）。
- [2026-11-14 02:42:53] 退出 Play 后 AesirTimer 静态累计计数仍读得到 → **不能指望退出 Play 时域重载清掉运行时静态字段**。任何"编辑器里展示运行时计数"的面板必须显式按 `EditorApplication.isPlaying` 归零，Play 切换那一刻立即重抓（已落到 AesirTimerDebugSource）。

## 项目目录 / 资源命名 / 文档标准（用户裁决）

- [2026-11-19 02:36:20] **资源命名不加类型前缀**：场景/预制体/图片等资产文件名**不加** `scn_`/`pref_` 前缀——文件名自带语义且代码引用带 `.unity`/`.prefab`/`.png` 扩展名，前缀冗余。Why: 用户纠正了目录结构技能里的前缀约定。How to apply: 生成/命名 Unity 资产、写或改 aesir-project-structure/boot-scene 技能时一律不加。
- [2026-11-19 02:44:09] 自有资源根目录是 `_Game`（**单下划线**），下按用途平铺：`Scenes/` 直接放场景（启动场景 `Bootstrap.unity`，无 Boot 子目录）、`Scripts/Contexts|Core|Model|View/` 按角色平铺（无 `Game/` 中间层），命名空间同步去掉 `Game.` 前缀，编辑器工具仍放 `Assets/Editor/`。Why: 用户 2026-11-19 纠正双下划线旧路径与中间层。
- [2026-11-19 12:10:26] **启动引导与脚本目录裁决**：启动脚本命名 `Bootstrap`（不加 Game 前缀、全局命名空间、放 `Assets/_Game/Scripts/` 根）；**脚本目录判据＝角色是否注册进 Context**——注册进去的 Context/Model/Service 按角色建 `Contexts/` `Model/` `Service/`，未注册的 View/Controller 偏业务逻辑层按 `Gameplay/` `UI/` 分层（对齐 Input System 两套 Action Map），`Common/` 留沉淀；**所有脚本统一全局命名空间**，目录只表达角色归属不产生 namespace。Why: 用户明确"能注册到 Context 的 Model/Service 是框架角色；View/Controller 未注册、偏业务逻辑层"。规范真源在 `skills/aesir-project-structure/SKILL.md` §2.2 与 `skills/aesir-boot-scene/SKILL.md`。
- [2026-11-08 12:21:45] **文档三类标准**：产品包 `Documentation/` 只留①英文 README_EN ②使用文档 ③AI Skill；迭代过程/内部文档写 `Aesir-Notebook/`，不随包分发。废弃机制历史演进写 notebook `docs/standards/design-changelog.md`（包内不保留设计变更记录）。
- [2026-11-18 20:21:29] **文档管理裁决（用户明确）**：策划案（GDD/美术风格/技术文档）尽量全部进 Git 随项目拉取；文件少时才用飞书等云端。"什么放云端、什么进 Git"视为项目结构一部分：需与代码同版本/需 AI 读的（GDD/style-guide/ADR/API/规范）进 Git 纯 Markdown，过程性讨论留云端。单一真源 + 结论回流：云端定论写回 Git md。文档站推荐 Zensical（zensical.toml + docs/ + site/），`zensical build --strict` 进 CI；docs 目录小写（位于 Assets 外不受 Unity 魔法名约束）。
- [2026-11-18 20:21:33] 本仓 .gitignore 只有 `ExportedObj/`，**没有 `Exports/` 规则**（CODELY.md 目录结构表把 Exports/ 列为 gitignored 是错的），`git check-ignore` 返回 NOT_IGNORED，写入 Exports/ 的文件显示为未跟踪 `??`。How to apply: 不想入库的交付物别默认 Exports/ 被忽略，确认 git status 或移到已忽略目录。

## AesirFrameworkGame 扩展 / 技能安装

- [2026-11-16 18:49:29] **AesirFrameworkGame 扩展**：位于 `.codely-cli/extensions/AesirFrameworkGame/`（唯一入库扩展），现含 12 技能 + 2 模板；`skills/` 是全部技能唯一主位，包内只留一份 `TuanjieCowork~/README.md` 导流文档；扩展含**依赖门禁**三条硬规则（探测缺包 → 先 read_memory、记忆明确禁装 RAM 就不装 → 未禁止必须 ask_user 询问，Git URL 安装 CNB 优先 GitHub 兜底）。项目级 `.codely-cli/skills/{aesir-framework,aesir-boot-scene}` 已删以消除副本。
- [2026-11-16 18:12:22] `codely skills install <本地路径|git URL> --scope user|workspace --consent` 是 Cowork/Codely Agent Skill 安装入口（`codely skills list [--in-skills-dir]` 校验、`codely skills uninstall <name> --scope …`）。验收方式 = install + list + uninstall，frontmatter 由真实解析器验证（本机 python 无 pyyaml）；路径中段的 `~` 不影响 install（只有行首 `~` 被 shell 展开）。工作区级同名技能会被覆盖，验证用 `--scope user` 避开。

## Samples / Documentation 双目录同步

- [2026-09-22 10:23:48] 同步脚本 `.codely-cli/skills/sync-samples/scripts/sync_samples.py --check`（逐文件判方向、删镜像多余项、补顶层 .meta、校验 package.json samples 清单）；**逐文件 diff 判断方向，不要凭"主位为准"硬覆盖**（历史上主位反而是错的一侧）。`Documentation~/` 排除时注意 `--exclude='*.meta'` 会误配 `.md.meta`，应写 `--exclude='*.md.meta'`。PlaneWar 场景出现 BinderTag / BinderCodeGenerator / MenuProbe 即污染（RAA 无 RAM 依赖）；本仓示例场景 `GraphicRaycaster` 的 `m_Bits` 惯例值 55。
