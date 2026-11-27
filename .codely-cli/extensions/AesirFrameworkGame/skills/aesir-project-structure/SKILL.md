---
name: aesir-project-structure
description: Unity/团结引擎 PC 独立游戏项目的目录结构规范与文档组织指南。搭工程骨架、规划 Assets 按用途分层、设计 Global/Shared/Unique 复用范围、划定哪些目录进 Git、搭 Zensical 文档站与同仓部署 GitHub Pages、写  CODELY.md 时使用。触发词：项目目录怎么组织、Assets 怎么分层、按用途还是按类型、哪些目录进 Git、docs 文档站怎么搭、CODELY.md 怎么写、PC 独立游戏工程骨架。
---

# PC 独立游戏项目目录结构（发布到 Steam）

用 Aesir 从零做 PC 独立游戏时的**工程骨架**：先按本文把目录与文档结构立好，再用 `../aesir-boot-scene/SKILL.md` 生成启动场景，最后用 `../aesir-framework/SKILL.md` 写业务代码。本文给决策与骨架；完整目录模板、三层分层原理与建站部署细节在 `references/`（按需读）。单机/联网、资源以本地打包为主、DLC 用多 depot 分发的独立游戏都适用。

图例：`✅` 纳入 Git　`❌` 忽略（可重建/产物/个人设置）。

## 决策速查（先记这五条）

| 问题 | 答案 |
|---|---|
| 目录主轴按什么分？ | **按用途**（Characters/Environment/UI/Effects/Audio/Animation），不按类型（Textures/Materials）或职能（美术/程序） |
| 每个用途域内部怎么分？ | **统一按复用范围三层**：`Global`（全局常驻）/ `Shared`（部分复用）/ `Unique`（单对象独有） |
| 资产怎么归类？ | 一个按钮贴图属于 UI 就进 UI 域，不因它是纹理就进 Textures；命名带范围线索（如 `gold_glow_bossarena_unique`） |
| 复用范围变了怎么办？ | **必须同步移动 + 改名**：Unique 被第二个对象引用 → 升 Shared；Shared 被绝大多数对象引用 → 升 Global；升降只在同一用途域内进行 |
| 脚本子目录按什么分？ | 看角色**是否注册进 Context**：注册进去的框架角色（Context / Model / Service）直接在 `Scripts/` 下建 `Contexts/` `Model/` `Service/`；没注册的 View / Controller 偏业务逻辑层，按 `Gameplay/` `UI/` 分层（对齐 Input System 的 `Gameplay` / `UI` Action Map）。所有角色统一全局命名空间；`Bootstrap.cs` 放根，`Common/` 留跨项目沉淀 |
| 调整目录结构前必做什么？ | **先逐个分析 `Assets/` 根目录现有文件夹**：被系统/包按固定路径或 GUID 引用的必须/推荐留根，纯自有资源进 `_Game/`；完成后**列出留根清单+理由**，此步不可省略（Aesir 模板六系统目录裁剪即此例，展开见 §1 链接的 reference） |
| 场景/预制体要加类型前缀吗？ | **不加**。文件名自带语义（`MainMenu` 一看就是场景、`Player` 一看就是预制体）；代码里引用时本来就会带扩展名（`.unity` / `.prefab` / `.png`），类型信息已由后缀承载，再加 `scn_` / `pref_` 是冗余 |
| 文档和构建产物怎么管？ | `docs/` + `CODELY.md` 进 Git 作单一真源；`site/`、`Builds/`、`Library/` 等产物忽略 |

## 1. 目录骨架

```
<ProjectRoot>/
├── Assets/
│   ├── _Game/                   # 自有资源；六系统目录留根、不进此层（见上表）
│   │   ├── Art/<用途>/{Global,Shared,Unique}/   # Characters / Environment / UI / Effects
│   │   ├── Audio/{BGM,SFX,Voice}/   Animation/   Fonts/   Atlas/
│   │   ├── Prefabs/   Scenes/   # 文件名不加类型前缀
│   │   ├── Scripts/             # 业务代码进 Assembly-CSharp，不建 asmdef；切法见 §2.2
│   │   ├── Config/{Source,Defs,Gen}/   Sandbox/<username>/
│   ├── ThirdParty/（或 Plugins/）   Editor/   Editor Default Resources/
│   └── Gizmos/   StreamingAssets/   AddressableResources/   Tests/
├── Packages/   ProjectSettings/          # ✅ 与 Assets 同为版本控制三件套
├── zensical.toml   docs/✅   site/❌      # 文档源 / 构建产物
├── CODELY.md   AGENTS.md→   CLAUDE.md→   README.md
└── Builds/❌   Library/ Temp/ Logs/ obj/ UserSettings/❌
```

完整目录树（每个子目录的用途注释）、按用途组织的原理、Global/Shared/Unique 三层的加载语义与升降规则见 [references/directory-layout.md](references/directory-layout.md)。

**平台要点**：PC 独立游戏带宽与存储非瓶颈，资源可多本地打包。Addressables 分组按"同时加载/卸载"划分；DLC 用独立 Build/Load Path + Steam 多 depot，运行时 `GetDLCCount()` / `IsDLCInstalled()` 判定并动态设 LoadPath。

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

## 3. 目录即资源包蓝图

**分组边界与目录边界对齐**（官方"1:1 匹配"）：文件夹是人力可维护的真理之源，用来校验 Addressables 分组是否正确。它不要求"一个组只能有一个文件夹"——生命周期相同的多个 `Global/` 合进同一常驻组，边界依然清晰：**目录按用途分，打包按生命周期合**。展开与 YooAsset 对应见 [references/directory-layout.md](references/directory-layout.md)。

## 4. 版本控制边界与工程纪律

Unity 官方要求纳入版本控制的只有 **`Assets` + `Packages` + `ProjectSettings`**，`Library`、`UserSettings` 必须完全忽略（只是本地缓存和个人设置）。

配套纪律（都是踩坑点）：

- **`.meta` 必须提交**：导入设置写在 `.meta` 而非资产文件；开启 Visible Meta Files；移动资产要在 Editor 内操作让 `.meta` 同行，否则 VCS 视为"删+增"丢历史。
- **空目录放 `.keep`** 才能被 Git 提交；只提交空目录的 `.meta` 会导致别人机器上"有 .meta 无目录"而被 Unity 删除。
- **文件名/文件夹名不用空格**（命令行工具处理空格路径出错），用 CamelCase；自有资源用下划线置顶，第三方独立隔离并附版本说明。
- 大场景拆小场景 + Prefab/Nested Prefab；用 Editor 脚本一键生成统一目录 + 脚本模板（`Assets/ScriptTemplates`）+ Presets 强制默认设置。

## 5. docs 文档结构：单一真源 + AI 友好

**最关键纪律：单一真源 + 结论回流。** 云端（飞书/Notion）负责讨论过程，Git 负责当前有效结论；讨论定了就把结论写回 Git 对应 md（GDD / style-guide / ADR），附云端链接备查，避免"飞书写 A、代码按 B"。大图/视频/PSD 本体不进 Git，md 只放链接 + 文字描述——AI 读文字就够。

进 Git 的判据：需与版本对齐、**AI 要读**、纯文本能表达、长期契约与决策。过程性内容（日报/排期/逐条 bug）与需零门槛编辑的留云端。

**让文档适合 AI**：

1. 根 `CODELY.md` 是 AI 总入口（200–300 行）：构建命令、测试命令、项目结构、代码风格、Git 工作流、**边界（能改什么、不能碰什么）**，细节外链 `docs/`；其它工具用软链指向它（`AGENTS.md → CODELY.md`、`CLAUDE.md → CODELY.md`），勿复制内容。
2. `docs/index.md` 是文档地图兼站点首页，人和 AI 进项目先读它。
3. 规范/决策/API 一律纯文本 Markdown；每篇开头写清"管什么、何时算过时"，用 ADR 固化"为什么"。

**小团队落地顺序**：`CODELY.md + README.md + docs/index.md`（半天，收益最大）→ 一页 GDD → style-guide 三条 → ARCHITECTURE + conventions → 用到再建 adr/build/workflow。50 页 GDD 的宿命是写完没人看。

**Zensical 环境门禁（先记结论）**：涉及 Zensical 前 MUST 先问用户是否真的需要；若机器已有全局 `zensical`，MUST 再问用户是否复用（同意才可直接用）；否则让用户在**项目内 venv** 与**项目内 uv** 之间二选一。**禁止为用全局而新装**（`pip install zensical` / `pipx` / `uv tool` 等全局安装）。

`docs/` 完整分类、小写目录的理由、Zensical 建站与同仓 GitHub Pages 部署（含 `zensical.toml` 与 `deploy.yml` 可抄示例、两套隔离命令与禁止清单）见 [references/docs-and-site.md](references/docs-and-site.md)。

## 与 Aesir 工作流的衔接

- **本文 → boot-scene → framework**：先用本文立好目录骨架与 `docs/` 雏形，再走 `../aesir-boot-scene/SKILL.md` 生成 `Bootstrap.unity`（`Assets/_Game/Scenes/`）+ `Bootstrap.cs`（`Assets/_Game/Scripts/` 根），最后 `../aesir-framework/SKILL.md` 写业务（业务脚本按业务方面放 `Assets/_Game/Scripts/Gameplay/` 与 `UI/` 下）。
- **脚本目录约定呼应**：按 §2.2——注册进 Context 的 Context / Model / Service 在 `Scripts/` 下按角色建 `Contexts/` `Model/` `Service/`，没注册的 View / Controller 按 `Gameplay/` `UI/` 业务分层，全部全局命名空间；启动场景 skill 默认让业务代码放 `Assembly-CSharp`（少一层编译上下文）也兼容本模板。
- **docs 与 CODELY.md`：本文第 5 节的 `CODELY.md` 单一真源 + 软链约定，与本扩展维护的框架工程 `CODELY.md` 是同一套纪律，落地时直接沿用。
