# AesirFrameworkGame

AesirFramework（Runestone Yuumix 出品的 Unity／团结引擎框架）的 AI 编码技能包，也是 Aesir 全部 AI 技能的**唯一主位**：11 个技能（1 路由 + 5 架构 + 1 工程初始化 + 4 功能模块）+ 2 篇实战模板。用 Unity／团结引擎 + AesirFramework 做独立游戏时使用——从零搭工程骨架、组织 Context/Model/View/Controller（默认 Quick 档）、做面板与弹窗、发事件解耦、加音效与背景音乐、切场景与加载进度、把原生 MonoBehaviour 代码迁进框架。

**面向独立游戏，主打精简 + 渐进式**：默认走最低档 Quick（不建接口、不做只读包装、直接持具体类），只在确需扩展时升档。

## 用哪个技能

先加载 `skills/aesir-router/SKILL.md`——它先过依赖门禁，再按意图分派到下面 11 个技能，并给出"从零做小游戏 / 只补某一面 / 从原生代码迁移"三条组合路径。单点需求可直接加载对应技能：

| 技能 | 覆盖 |
|---|---|
| `aesir-router` | 入口与分派：依赖门禁、意图 → 技能、组合路径、常见误路由纠正 |
| `aesir-framework` | 架构底座：Context／Model／View／Controller、五条核心裁决、Quick 档三件套、命名规范 |
| `aesir-tiers` | 六档分级与六份可运行模板（MVC／MVP 各三档）、升档触发条件 |
| `aesir-core-api` | `ObservableValue`、可观察集合、`MiniEvent`、`AesirTimer`、`AesirPlayerLoop` |
| `aesir-decisions` | MVC 还是 MVP、为什么没有 MVVM、四种通知机制怎么选 |
| `aesir-pitfalls` | 十条高频陷阱（现象 + 原因 + 修法） |
| `aesir-boot-scene` | 工程初始化：启动场景 `Boot.unity` + `GameBootstrap.cs` + Build Settings index 0 |
| `aesir-ui` | `UIModule`／`UIRoot`：Panel 与 Window 选型、生命周期、蒙版单遮／叠遮、世界空间血条 |
| `aesir-events` | `EventModule`：双轨订阅、四档优先级、订阅者过滤器、SO 事件资产化 |
| `aesir-audio` | `AudioModule`：PlaySfx 轮询、PlayBgm 淡变、三通道音量与持久化 |
| `aesir-scene` | `SceneModule`：Single／Additive、进度归一化、叠加追踪、UniTask awaitable |

实战模板（本扩展独有，源自 `Assets/PlaneWar`（宿主工程完整游戏案例）与 `Samples/Counter-*`）：

| 文件 | 内容 |
|---|---|
| `skills/aesir-framework/references/templates-game-skeleton.md` | 完整可玩小游戏骨架：得分／失败状态、HUD 订阅、节奏生成器、开火冷却、`Dispose()` + `ReloadScene()` 重开 |
| `skills/aesir-framework/references/templates-migration-from-mono.md` | 原生 MonoBehaviour → Aesir 逐项迁移表（静态单例、每帧轮询、Update 计时、SceneManager 直切、PlayOneShot） |

## 依赖门禁（硬规则，改技能时不要放宽）

技能只教**怎么写代码**，装包由 AI 在会话中按下面三步处理，顺序不可颠倒。任何模块技能都不得在缺包状态下硬写，也不得改用原生 API 顶替（`SceneManager` 直切、自建 Canvas 栈、`PlayOneShot`、`static event` 总线）。

| 阶段 | 动作 | 判定 |
|---|---|---|
| ① 触发条件 | 探测工程实际装了什么：`Packages/manifest.json` / `packages-lock.json` 有包 ID、`Packages/cn.runestone.aesir.*` 或 `Library/PackageCache/cn.runestone.aesir.*` 存在、内联安装时 `Assets/Runestone/AesirArchitecture|AesirModules` 存在、能否 `using Runestone.Aesir*` 编译通过 | 缺 RAA → 全部技能失效（RAM 连编译都过不了）；缺 RAM → `aesir-ui` / `aesir-events` / `aesir-audio` / `aesir-scene` 失效 |
| ② 记忆读取 | **必须先** `read_memory`（scope=all），并看工程内 `AGENTS.md` / `CLAUDE.md` / `CODELY.md` 的显式约定 | 记忆明确"不要安装 RAM 包""只允许 RAA""禁止引入新包" → **不安装**，只做已装包的活 |
| ③ 询问与安装 | 记忆未明确禁止 → **必须** `ask_user` 询问，问句说清：装哪个包、从哪装、会写 `Packages/manifest.json` 并从网络下载 | 同意 → 先 RAA 后 RAM；线路**CNB 优先、GitHub 兜底**，不要把 GitHub 当首选 |

| 包 | 国内直连（首选） | 国际镜像（兜底） |
|---|---|---|
| RAA | `https://cnb.cool/runestone-game/AesirFramework.git#AesirArchitecture-latest` | `https://github.com/yuumixcode/AesirFramework.git#AesirArchitecture-latest` |
| RAM | `https://cnb.cool/runestone-game/AesirFramework.git#AesirModules-latest` | `https://github.com/yuumixcode/AesirFramework.git#AesirModules-latest` |

只缺 RAA 但已装 RAM 时**不要手写 manifest 依赖**（RAM 的 `package.json` 不声明该依赖），走菜单 `Tools/Aesir/Modules/Install Dependencies`。装完等域重载结束再按 ① 复验，并把结论写进工程记忆，避免下次重复询问。

失效矩阵、拒绝安装后的应对、完整命令以 `skills/aesir-router/SKILL.md` 的「依赖门禁」为准；四个模块技能与 `aesir-boot-scene` 只在开头一句指回路由器，不重复整张表。

## 安装

**给开发者看的下载与安装说明在包内 `TuanjieCowork~/README.md`**（`Assets/Runestone/AesirArchitecture/TuanjieCowork~/README.md` 与 `Assets/Runestone/AesirModules/TuanjieCowork~/README.md`）：团结 Cowork 用户走稀疏克隆 + `codely extensions install`（或直接拷进 `.codely-cli/extensions/`）；非 Cowork 用户从 CNB／GitHub 下载后把 `skills/` 拆进自己的 AI 工具。

本机安装／登记本扩展：

```bash
# 登记为工作区扩展（团结 Cowork 打开该工程即自动加载；schema 见 gemini-extension.json）
codely extensions install .codely-cli/extensions/AesirFrameworkGame --scope workspace --consent

# 单独装技能到项目级／全局技能目录
python3 scripts/install.py
python3 scripts/install.py --scope global
python3 scripts/install.py --only aesir-router --check
```

## scripts

| 脚本 | 用途 |
|---|---|
| `scripts/install.py` | 批量安装技能到项目级／全局技能目录（`--only` 选技能、`--check` 干跑、`--force` 覆盖） |
| `scripts/validate.py` | 结构校验：manifest 合规、每个 SKILL.md 的 frontmatter 与目录名一致、相对链接可达、无 `.meta` 混入、**包内不得再出现 `TuanjieCowork~/Skills`** |

## 维护约定（改内容前必读）

1. **技能正文的唯一主位是本扩展的 `skills/`**：直接在 `.codely-cli/extensions/AesirFrameworkGame/skills/` 下编辑，**不再同步回包内**。两个包的 `TuanjieCowork~/` 只保留一份 `README.md` 导流文档（说明怎么下载／安装本扩展），出现 `TuanjieCowork~/Skills/` 会被 `validate.py` 判错。
2. `~` 结尾目录与 `.codely-cli/` 都不被 AssetDatabase 导入、不进 unitypackage，因此技能不会混进任何 Unity 工程或包产物。
3. 改完跑一遍 `python3 scripts/validate.py`（0 错误才算完成）；动到技能名或目录结构时同步 `skills/aesir-router/SKILL.md` 的分派表、本文件与 `gemini-extension.json` 的 description。
4. 技能边界按"一个技能只回答一类问题"拆分：入口不写模板、模板不进核心 API、决策与陷阱各自独立，AI 按意图只加载需要的那一篇。

## 上游

| 项 | 值 |
|---|---|
| 包 ID | `cn.runestone.aesir.architecture`（RAA）／`cn.runestone.aesir.modules`（RAM） |
| 依赖关系 | RAM 依赖 RAA；RAM 缺 RAA 时用菜单 `Tools/Aesir/Modules/Install Dependencies` 补装 |
| 仓库 | 主仓 [CNB](https://cnb.cool/runestone-game/AesirFramework)（国内直连）· 镜像 [GitHub](https://github.com/yuumixcode/AesirFramework) |
| Unity 版本 | 2022.3.x（团结引擎） |
| 许可证 | MIT |

---

*This extension was converted using skill-converter*
