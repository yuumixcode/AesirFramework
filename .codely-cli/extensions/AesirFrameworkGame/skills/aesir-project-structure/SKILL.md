---
name: aesir-project-structure
description: Unity/团结引擎 PC 独立游戏项目的目录结构规范与文档组织指南。从零搭建工程骨架、规划 Assets 按用途分层、设计 Global/Shared/Unique 复用范围、划定哪些目录进 Git、搭 Zensical 文档站与同仓部署 GitHub Pages、写 AI 友好的 CODELY.md 时使用。触发场景："项目目录怎么组织"、"Assets 怎么分层"、"按用途还是按类型"、"哪些目录进 Git"、"docs 文档站怎么搭"、"CODELY.md 怎么写"、"PC 独立游戏工程骨架"。
---

# PC 独立游戏项目目录结构（发布到 Steam）

用 Aesir 从零做 PC 独立游戏时的**工程骨架**：先按本文把目录与文档结构立好，再用 `../aesir-boot-scene/SKILL.md` 生成启动场景，最后用 `../aesir-framework/SKILL.md` 写业务代码。本文自成一体——目录模板 + 用它的全部设计原理都在这里，单机/联网、资源以本地打包为主、DLC 用多 depot 分发的独立游戏都适用。

图例：`✅` 纳入 Git　`❌` 忽略（可重建/产物/个人设置）。

## 决策速查（先记这五条）

| 问题 | 答案 |
|---|---|
| 目录主轴按什么分？ | **按用途**（Characters/Environment/UI/Effects/Audio/Animation），不按类型（Textures/Materials）或职能（美术/程序） |
| 每个用途域内部怎么分？ | **统一按复用范围三层**：`Global`（全局常驻）/ `Shared`（部分复用）/ `Unique`（单对象独有） |
| 资产怎么归类？ | 一个按钮贴图属于 UI 就进 UI 域，不因它是纹理就进 Textures；命名带范围线索（如 `gold_glow_bossarena_unique`） |
| 复用范围变了怎么办？ | **必须同步移动 + 改名**：Unique 被第二个对象引用 → 升 Shared；Shared 被绝大多数对象引用 → 升 Global；升降只在同一用途域内进行 |
| 脚本子目录按什么分？ | 看角色**是否注册进 Context**：注册进去的框架角色（Context / Model / Service）直接在 `Scripts/` 下建 `Contexts/` `Model/` `Service/`；没注册的 View / Controller 偏业务逻辑层，按 `Gameplay/` `UI/` 分层（对齐 Input System 的 `Gameplay` / `UI` Action Map）。所有角色统一全局命名空间；`Bootstrap.cs` 放根，`Common/` 留跨项目沉淀 |
| 场景/预制体要加类型前缀吗？ | **不加**。文件名自带语义（`MainMenu` 一看就是场景、`Player` 一看就是预制体）；代码里引用时本来就会带扩展名（`.unity` / `.prefab` / `.png`），类型信息已由后缀承载，再加 `scn_` / `pref_` 是冗余 |
| 文档和构建产物怎么管？ | `docs/` + `CODELY.md` 进 Git 作单一真源；`site/`、`Builds/`、`Library/` 等产物忽略 |

## 1. 目录模板

```
<ProjectRoot>/
├── Assets/                              # ✅ 全部资产（含每个文件/文件夹的 .meta）
│   ├── _Game/                          # 【自有资源】下划线置顶，与第三方隔离
│   │   ├── Art/                         # 用途为主轴；每个用途域内部统一按复用范围分三层（详见决策速查）
│   │   │   ├── Characters/{Global,Shared,Unique}/   # 每角色一文件夹，其网格/动画/材质/音效按复用范围归层
│   │   │   ├── Environment/{Global,Shared,Unique}/  # 地形/天空/植被/建筑/光照/关卡专属道具
│   │   │   ├── UI/{Global,Shared,Unique}/           # 通用控件/图标(Global)、跨界面复用件(Shared)、单界面专属(Unique)
│   │   │   ├── Effects/{Global,Shared,Unique}/      # 粒子用的模型/材质/纹理/预制体
│   │   │   └── Atlas/                   # 自动生成图集输出（按来源用途归档，不再单建全局类型域）
│   │   ├── Audio/{BGM,SFX,Voice}/
│   │   ├── Animation/
│   │   ├── Prefabs/                     # 不加类型前缀，名字自带语义
│   │   ├── Scenes/                      # 不加类型前缀，Bootstrap.unity 等直接放这里；大场景拆小场景，Additive 加载
│   │   ├── Fonts/
│   │   ├── Scripts/                     # 业务代码放 Assembly-CSharp，不建 asmdef；子目录切法见 §2.2
│   │   │   ├── Bootstrap.cs             # 启动引导（由 aesir-boot-scene 产出）：全局命名空间，放根目录
│   │   │   ├── Contexts/                # 框架角色：注册进 Context 的 Context（全局命名空间）
│   │   │   ├── Model/                   # 框架角色：注册进 Context 的 Model（运行时数据源）
│   │   │   ├── Service/                 # 框架角色：注册进 Context 的 Service（跨模块协调）
│   │   │   ├── Gameplay/                # 业务分层：未注册的 View / Controller 中偏玩法的一面（玩家 / 敌人 / 生成器）
│   │   │   ├── UI/                      # 业务分层：未注册的 View / Controller 中偏界面的一面（面板 / 窗口 / HUD）
│   │   │   └── Common/                  # 通用代码层：沉淀在本项目、可复用到其他项目的代码（随源码迭代，暂不走 UPM 包）
│   │   ├── Config/{Source,Defs,Gen}/    # 若用配表（用到再说）
│   │   ├── Resources/                   # ⚠️ 少用：其中资产无论是否使用都会打进包
│   │   └── Sandbox/<username>/          # 测试/沙盒区，按成员划分
│   ├── ThirdParty/  (或 Plugins/)       # 【第三方】保持原始结构，附来源与版本说明文件
│   ├── Editor/                          # 全局编辑器脚本（目录模板、打包、导表）
│   ├── Editor Default Resources/        # Editor 专用资源
│   ├── Gizmos/                          # 场景视图图标
│   ├── StreamingAssets/                 # 原样拷入包内的资源
│   ├── AddressableResources/            # 按资源包/Group 组织（含 DLC 分组）
│   └── Tests/                           # Editor/PlayMode 测试，发布时可整体剔除
├── Packages/                            # ✅ manifest.json / packages-lock.json（团队依赖一致）
├── ProjectSettings/                     # ✅ Physics/Tags/Player/Input/Graphics 等
├── zensical.toml                        # ✅ Zensical 站点配置（nav 导航、主题、插件；docs_dir 默认 docs）
├── docs/                                # ✅ Zensical 源目录（小写 = 工具默认约定；Assets 之外，Unity 不导入）
│   ├── index.md                         # ★站点首页 = 文档地图：每篇一句话 + 链接
│   ├── product/
│   │   ├── GDD.md                       # 一页游戏设计（核心循环/系统/数值/范围）
│   │   ├── systems/ levels/ narrative/ decisions/   # 设计决策记录 ADR
│   ├── art/
│   │   ├── ART_BIBLE.md                 # 美术风格圣经：定调 + 参考
│   │   ├── style-guide.md               # 色彩/字体/UI/线条/材质规范
│   │   ├── characters/ references/      # references 放图片本体（美术/开发者本地建站即可图文查看）
│   ├── tech/
│   │   ├── ARCHITECTURE.md              # 架构总览、模块边界、依赖方向
│   │   ├── conventions.md               # 命名 / 目录 / 代码风格
│   │   ├── adr/ build.md api/           # 技术决策记录 / 构建发布 / API
│   ├── workflow/
│   │   ├── onboarding.md git-workflow.md asset-pipeline.md
│   ├── glossary.md                      # ★中英术语表：统一产品/美术/技术术语与英文对应，消除中英混用歧义
│   ├── CHANGELOG.md                     # 版本变更（设计 + 技术合流）
│   ├── assets/                          # 站点图片/截图（美术参考图、UI 示意、流程图）
│   ├── stylesheets/extra.css            # 站点外观覆盖（配置优先，尽量不动主题源）
│   └── javascripts/extra.js             # 站点交互脚本（可选）
├── site/                                # ❌ zensical build 静态输出（进 .gitignore）
├── CODELY.md                            # ✅ AI 总入口（≤300 行，六块内容；TuanjieCowork 优先）
├── AGENTS.md → CODELY.md                # 软链兼容 Cursor / Factory 等（单一真源，勿复制内容）
├── CLAUDE.md → CODELY.md                # 软链兼容 Claude Code（单一真源，勿复制内容）
├── README.md                            # ✅ 人类入口：简介 + 快速开始 + 指向 docs/index.md
├── .gitignore / LICENSE
├── Builds/{Windows,Linux,macOS}/        # ❌ 构建产物；按平台/渠道分子目录
└── Library/ Temp/ Logs/ obj/ UserSettings/   # ❌ 缓存/中间产物/个人设置
```

> 上面的 `docs/` 是完整视图；最小可用形态是 `docs/index.md` + 用到的分类目录（见第 5 节）。

**平台要点**：PC 独立游戏带宽与存储非瓶颈，资源可多本地打包。Addressables 分组按"同时加载/卸载"划分——线性游戏按章节切大组，非线性用小组件。DLC 用 Addressables 独立 Build/Load Path（如 `StreamingAssets/DLCs/`），主构建排除该目录；Steam 多 depot 分发，运行时 `GetDLCCount()` / `IsDLCInstalled()` 判定并动态设 LoadPath。

## 2. 按"用途"组织，而不是按类型或职能

Unity 官方博客把目录结构分为四类：随机、按资产类型、按功能（团队职能）、**按用途（by purpose/use）**。官方结论是"按用途迄今为止最好"，因为它**传达资产意图**——团队和打包系统据此判断何时加载/卸载。*"An appropriate file directory should not obscure information, but reveal it."*

派生两条正交、缺一不可的规则：

1. **主轴 = 用途**：一级目录按用途域划分，不用材质/纹理这类"类型"当顶层。类型信息由文件后缀和域内子目录承载，用途信息才值得占据顶层。
2. **每个用途域内部统一按复用范围分三层**（详见第 2.1 节）。复用范围决定加载/卸载时机，是最值得显式表达的第二种意图。

**命名即意图**：`gold_glow_trex_endgame` 一看便知是 Boss 终局专用，避免被误用于教程导致整包被下载。小项目早期按类型组织够用，但规模上升后"知道类型就好找、其余信息被掩盖"的缺点放大——所以模板采用按用途为主轴、各域内部再按复用范围分层的两级结构。

### 2.1 每个用途域内部的三层拆分（Global / Shared / Unique）

三层是**所有用途域统一的内部结构**，不是环境域特权：一个被多处复用的 UI 按钮组件属于 `UI/Shared/`，只在一个界面用的图标属于 `UI/Unique/<界面>/`，全局通用的字体/控件属于 `UI/Global/`——和环境域一样清清楚楚。混放的直接后果是某个界面的专属贴图被误当成全局，玩家一进游戏就把所有界面的资源一起下载。

```
Art/
├── UI/                            # 界面域同样分三层，不是环境专属
│   ├── Global/                    # 全局通用：字体、通用控件、公共图标、通用背景
│   ├── Shared/                    # 被部分界面复用：如通用按钮组件、通用弹窗模板
│   └── Unique/                    # 单界面独有：与界面一一对应
│       └── HUD_Main/             #   该界面的布局预制体、专属贴图/音效
├── Environment/
│   ├── Global/                    # 跨所有关卡：天空盒、全局光照/后处理、通用地形材质、全局 shader
│   │   └── {Skybox,Lighting,Materials,Shaders}/
│   ├── Shared/                    # 被部分关卡复用：按用途单列，不与 Global/Unique 混放
│   │   ├── Forest/                #   森林气候共用的树木、灌木、落叶粒子
│   │   └── RuinsKit/              #   废墟关卡共用的建筑模块套件
│   └── Unique/                    # 单关卡独有：与关卡一一对应
│       ├── Level_01/              #   该关卡的地形网格、专属道具、独有贴图/音效
│       └── BossArena/
└── Characters/                    # 角色域同理，材质/纹理随角色归层，不再单建全局 Textures/
    ├── Global/                    # 所有角色共用：通用骨骼、共享 shader
    ├── Shared/                    # 部分角色共用：飞行怪的翅膀/粒子套件
    └── Unique/
        └── T_Rex/                #   该角色专属：网格/动画/材质/音效
```

三层各自的加载语义（与"目录即资源包蓝图"直接对应）：

| 层 | 复用范围 | Addressables / 资源包策略 | 加载时机 |
|---|---|---|---|
| **Global** | 该用途域内贯穿全局 | 常驻组，启动即加载、永不卸载 | 游戏启动 |
| **Shared** | 被部分对象复用 | 按用途各建独立组 | 被任意引用者加载时载入，最后一个引用者卸载后释放 |
| **Unique** | 单一对象独有 | 与对象 1:1 建组（或并入关卡/界面场景包） | 随对象 Additive 加载 / 卸载 |

> **一个包里可以打多个文件夹**：Addressables 组按"同时加载/卸载"划分，一个组 Add 多个文件夹和只 Add 一个是一回事——把 `UI/Global/`、`Environment/Global/`、`Characters/Global/` 一起加进同一个常驻组即可。所以"用途为主轴"和"Global 统一常驻"不冲突：**目录按用途分，打包按生命周期合**。

**判定与维护规则**（资产复用范围变化时**必须**同步移动 + 改名，否则目录语义失真）：

1. Unique 资产被第二个对象引用 → **升为 Shared**，移入该用途域用途明确的 `Shared/<用途>/` 并更名。
2. Shared 资产被所有/绝大多数对象引用 → **升为 Global**；引用变少则降级。升降只在同一用途域内进行（UI 的 Shared 升为 UI 的 Global，不跨域搬家）。
3. 命名带上范围线索（如 `gold_glow_bossarena_unique`），让意图在打包系统里也可读。

> 小项目可先只用 `Unique/` 一层，等复用真的出现再向上提升——但一旦三层并存，必须严格按上述规则维护，否则 `Shared/` 会退化成"说不清归谁的杂物抽屉"。

### 2.2 脚本目录：注册进 Context 的按角色分，没注册的按业务方面分

上面的"用途 / 复用范围"针对**资产**。脚本目录另有一套切法，判据是**角色是否注册进 Context**：

| 脚本 | 是否注册进 Context | 落到哪里 |
|---|---|---|
| Context / Model / Service | **是**（框架角色 / 运行时数据源 / 跨模块协调） | `Scripts/Contexts/`、`Scripts/Model/`、`Scripts/Service/` |
| View / Controller | **否**（虽是框架角色，但偏业务逻辑层） | `Scripts/Gameplay/` 或 `Scripts/UI/` |
| Bootstrap / 通用沉淀 | 不适用 | `Scripts/` 根 / `Scripts/Common/` |

**注册进 Context 的直接在 `Scripts/` 下按角色分子目录**：`GameContext` 是 Context、`GameModel` 是 Model、`InventoryService` 是 Service——它们经 `RegisterModel` / `RegisterService` 进 Context，是框架角色，按角色名建 `Contexts/` `Model/` `Service/`。

**没注册进 Context 的 View / Controller 按业务方面分层**：它们是框架角色，但承载业务逻辑，按 `Gameplay/`（玩家 / 敌人 / 生成器）与 `UI/`（面板 / 窗口 / HUD）划分。这两块与 Input System 的两套 Action Map 同源——`Gameplay` 对应玩家 / 世界的 Gameplay Action Map，`UI` 对应 UI Action Map（菜单、HUD 的点选导航）。

**所有脚本统一全局命名空间**：目录只表达"框架角色归属"或"业务方面"，不产生 `namespace Gameplay` / `namespace Model`——一个脚本可能是 Context 也可能是 View，那是角色分类，不该成为命名空间。`Bootstrap.cs` 放根、`Common/` 留跨项目沉淀，两者都不参与上述划分。

## 3. 目录即资源包蓝图（1:1 匹配）

官方硬结论：*"A well-structured folder system and file naming convention should enable a 1:1 match between asset bundles and Addressables groups."*

文件夹是**人力可维护的真理之源**，用来校验资源分组设置是否正确；官方明确"文件夹和 Addressables 组两者都要"，编码式资源清单方案长期成本效益差。这就是模板里 `AddressableResources/` 与 Assets 内按用途分组一一对应的原因。YooAsset 的 `PackDirectory / PackTopDirectory` + `AddressByFilePath` 正是这条思想的落实。

> "1:1 匹配"指**分组边界与目录边界对齐**——某个文件夹整体归哪个组一目了然。它不要求"一个组只能有一个文件夹"：生命周期相同的多个文件夹（如各用途域的 `Global/`）合进同一个组，边界依然清晰，仍是 1:1 思想的应用。

## 4. 版本控制边界与工程纪律

Unity 官方要求纳入版本控制的只有 **`Assets` + `Packages` + `ProjectSettings`**，`Library`、`UserSettings` 必须完全忽略（只是本地缓存和个人设置）。

配套纪律（都是踩坑点）：

- **`.meta` 必须提交**：导入设置写在 `.meta` 而非资产文件；开启 Visible Meta Files；移动资产要在 Editor 内操作让 `.meta` 同行，否则 VCS 视为"删+增"丢历史。
- **空目录放 `.keep`** 才能被 Git 提交；只提交空目录的 `.meta` 会导致别人机器上"有 .meta 无目录"而被 Unity 删除。
- **文件名/文件夹名不用空格**（命令行工具处理空格路径出错），用 CamelCase；自有资源用下划线置顶，第三方独立隔离并附版本说明。
- 大场景拆小场景 + Prefab/Nested Prefab；用 Editor 脚本一键生成统一目录 + 脚本模板（`Assets/ScriptTemplates`）+ Presets 强制默认设置。

## 5. docs 文档结构：单一真源 + 结论回流 + AI 友好

模板里的 `docs/` 不只给 Unity 工程用，它同时是人和 AI 的文档入口。

**"什么放云端、什么进 Git"第一层切分**，判据是是否与代码同版本、是否需要 AI 读取：

| 判据 | 进 Git（默认） | 进云端（飞书/Notion） |
|---|---|---|
| 版本绑定 | 需与某 commit/版本对齐 | 与版本无关 |
| 消费者 | **AI 要读**、程序要引用 | 主要为人讨论、评论、@ |
| 形态 | 纯文本 Markdown 能表达 | 大图/视频/PSD/思维导图 |
| 时效 | 长期契约、规范、决策、API | 过程性（日报/排期/逐条 bug） |
| 编辑门槛 | 团队能接受 Markdown + Git | 策划/美术需零门槛编辑 |

**最关键纪律：单一真源 + 结论回流。** 云端负责讨论过程，Git 负责当前有效结论；讨论在飞书进行，一旦定了就把结论写回 Git 对应 md（尤其 GDD / style-guide / ADR），附云端链接备查，避免"飞书写 A、代码按 B"的分裂。大文件本体不进 Git，md 只放链接 + 文字描述——AI 读文字就够。

**让文档真正适合 AI**：

1. 根目录 `CODELY.md` 是 **AI 总入口**（TuanjieCowork 优先读取）：写清构建命令、测试命令、项目结构、代码风格、Git 工作流、**边界（AI 能改什么、不能碰什么）**，控制 200–300 行，细节外链到 `docs/`；其它 AI 工具用软链指向它，保证单一真源、勿复制内容：`AGENTS.md → CODELY.md`（Cursor / Factory 等）、`CLAUDE.md → CODELY.md`（Claude Code）。
2. `docs/index.md` 文档地图（兼 Zensical 站点首页），人和 AI 进项目先读它，按需取用。
3. 规范/决策/API 一律纯文本 Markdown——AI 读不到飞书评论和 PSD。`llms.txt` 是对外文档站约定，内部仓库用不上。
4. 每篇开头写清"管什么、何时算过时"，用 ADR（背景/选项/决定/理由）固化"为什么"。

**小团队落地顺序**：先建 `CODELY.md + README.md + docs/index.md`（半天搞定收益最大）→ 一页 GDD → style-guide 三条（色彩/UI/字体）→ ARCHITECTURE + conventions → 用到再建 adr/build/workflow。50 页 GDD 的宿命是"写完没人看"，一页起步边做边补。

### 5.1 为什么文档目录用小写 `docs/`，而 Unity 目录是大写

刻意区分，不是疏漏：

1. **大写是 Unity 的"魔法名"**：`Assets`、`Packages`、`ProjectSettings`、`StreamingAssets`、`Resources` 是引擎硬编码识别的保留目录，名字必须精确匹配（含大小写），写成小写引擎就不认——`resources/` 下 `Resources.Load` 会失效。
2. **`docs/` 不是 Unity 目录**：它在 `Assets/` 之外（同级），Unity 根本不扫描它，命名不受引擎约束。
3. **小写 `docs/` 是 Zensical 默认源目录约定**（`docs_dir = "docs"`），`zensical serve` / `zensical build` 零配置即可工作。改成 `Docs/` 也能用，但需在 `zensical.toml` 显式设 `docs_dir`，徒增配置。
4. **视觉信号**：大写 = "Unity 托管的世界"，小写 = "人 / AI / 文档工具链的世界"。

> 这也解释了 Unity 包内 `Samples~/`、`Documentation~/` 这种尾随波浪线目录——`~` 让 AssetDatabase 忽略它们，与 `docs/` 放在 Assets 外是同一种"让 Unity 别碰文档"的思路。

### 5.2 把 docs/ 做成 Zensical 站点：让每个人都能在本地看文档

文档全进 Git 解决"能拉取"，但直接读 `.md` 对策划、美术不友好（没导航、样式、图文排版）。做法是把 `docs/` 同时作为 **Zensical 静态站点源目录**——一份 Markdown，两种消费方式：AI 读原文，人看站点。

**为什么推荐 Zensical**：Material for MkDocs 团队（squidfunk）用 Rust 写的官方继任者（MkDocs 已停止维护），MIT 免费开源，配置用 TOML（`zensical.toml`），内置搜索、即时导航、暗色主题，构建快、零成本、零运维，纯静态无后端。

**目录与命令**：

```
zensical.toml          # 站点配置：nav 导航、[project.theme]、插件
docs/                  # 源目录（默认）
site/                  # 构建输出 → 必须进 .gitignore
```

```bash
zensical serve         # 本地预览，默认 http://localhost:8000，改文件自动 reload
zensical build --strict  # 构建到 site/，warning 当 error，可进 CI 校验链接与格式
```

**团队收益**：任何人零门槛查看（拉下仓库跑一条 `zensical serve` 就是带导航/搜索/高亮/图文的文档站）；单一真源不分裂（站点内容就是 Git 里的 Markdown 本体，`docs/index.md` 既是站点首页也是 AI 文档地图）；质量门禁（`zensical build --strict` 把失效链接/格式问题当错误纳入 CI）；配置优先、不写后端（评论/统计走 Giscus、Plausible 等免费 SaaS）。

**`zensical.toml` 最小示例**（TOML 父表 `[project]` 的键必须在子表之前写完）：

```toml
[project]
site_name = "Project Docs"
docs_dir = "docs"
site_dir = "site"
nav = [
  { "首页" = "index.md" },
  { "策划" = "product/GDD.md" },
  { "美术" = "art/style-guide.md" },
  { "技术" = "tech/ARCHITECTURE.md" },
  { "术语表" = "glossary.md" },
]

[project.theme]
variant = "modern"
language = "zh"
```

### 5.3 同仓部署 GitHub Pages：代码与官网同一个仓库

开源项目需要官网/文档站时**不必拆两个仓库**——代码和官网可以在同一仓库，关键是用对发布方式。

**为什么不能"直接"部署**：① `docs/` 是 Zensical 源目录（Markdown），不是能直接当网页的 HTML，GitHub Pages 自带构建只支持 Jekyll；② Pages 的"从分支发布"只允许选分支 root `/` 或 `/docs`——选 root 会把整个仓库（含 `Assets/`、源码）当网站发布，选 `/docs` 只发布 Markdown 源文件；③ **安全警告**：即使仓库私有，Pages 站点也是公开的，选 root 发布等于把整个项目源码公开。

**正确做法**：GitHub Actions 构建 `site/`，用 `actions/upload-pages-artifact` **只上传 `site/` 这一个目录**——Unity 代码不进入发布产物。Settings → Pages → Source 选 **GitHub Actions**。

```yaml
# .github/workflows/deploy.yml
name: Deploy Docs
on:
  push:
    branches: [main]
permissions:
  contents: read
  pages: write
  id-token: write
jobs:
  deploy:
    runs-on: ubuntu-latest
    environment: { name: github-pages }
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-python@v5
        with: { python-version: "3.12" }
      - run: pip install zensical
      - run: zensical build --strict
      - uses: actions/configure-pages@v5
      - uses: actions/upload-pages-artifact@v3
        with: { path: site }   # ← 只发这里，Unity 内容不会被发布
      - uses: actions/deploy-pages@v4
```

**三个实操注意点**：① 项目站点 URL 带子路径 `用户名.github.io/仓库名/`，`zensical.toml` 的 `site_url` 必须写成 `https://<用户名>.github.io/<仓库名>/`，否则 CSS/JS/即时导航 404；② `.nojekyll`——走 Actions + artifact 通常自动处理，旧方案推 `gh-pages` 分支需放空 `.nojekyll`，含符号链接的仓库必须用 Actions；③ `site/` 进 `.gitignore`。

> 同仓部署收益：代码与文档同版本、同 PR、同 tag，不会出现"代码在 A 仓库、官网在 B 仓库、内容对不上"的漂移。独立仓库（如本指南作者的两个站点）是另一种选择——用同一套 `zensical build --strict`，只是仓库边界不同。

## 与 Aesir 工作流的衔接

- **本文 → boot-scene → framework**：先用本文立好目录骨架与 `docs/` 雏形，再走 `../aesir-boot-scene/SKILL.md` 生成 `Bootstrap.unity`（`Assets/_Game/Scenes/`）+ `Bootstrap.cs`（`Assets/_Game/Scripts/` 根），最后 `../aesir-framework/SKILL.md` 写业务（业务脚本按业务方面放 `Assets/_Game/Scripts/Gameplay/` 与 `UI/` 下）。
- **脚本目录约定呼应**：按 §2.2——注册进 Context 的 Context / Model / Service 在 `Scripts/` 下按角色建 `Contexts/` `Model/` `Service/`，没注册的 View / Controller 按 `Gameplay/` `UI/` 业务分层，全部全局命名空间；启动场景 skill 默认让业务代码放 `Assembly-CSharp`（少一层编译上下文）也兼容本模板。
- **docs 与 CODELY.md`：本文第 5 节的 `CODELY.md` 单一真源 + 软链约定，与本扩展维护的框架工程 `CODELY.md` 是同一套纪律，落地时直接沿用。
