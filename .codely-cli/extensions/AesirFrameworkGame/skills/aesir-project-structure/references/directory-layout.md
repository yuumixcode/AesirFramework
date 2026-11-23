# 目录模板与资源分层（展开参考）

> `../SKILL.md` 的展开参考：要落地完整目录树、理解 Global/Shared/Unique 三层的加载语义与升降规则、或对齐 Addressables 分组时读本文。决策速查与脚本目录切法在 SKILL.md。

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

## 3. 目录即资源包蓝图（1:1 匹配）

官方硬结论：*"A well-structured folder system and file naming convention should enable a 1:1 match between asset bundles and Addressables groups."*

文件夹是**人力可维护的真理之源**，用来校验资源分组设置是否正确；官方明确"文件夹和 Addressables 组两者都要"，编码式资源清单方案长期成本效益差。这就是模板里 `AddressableResources/` 与 Assets 内按用途分组一一对应的原因。YooAsset 的 `PackDirectory / PackTopDirectory` + `AddressByFilePath` 正是这条思想的落实。

> "1:1 匹配"指**分组边界与目录边界对齐**——某个文件夹整体归哪个组一目了然。它不要求"一个组只能有一个文件夹"：生命周期相同的多个文件夹（如各用途域的 `Global/`）合进同一个组，边界依然清晰，仍是 1:1 思想的应用。

