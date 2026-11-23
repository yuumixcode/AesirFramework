# TuanjieCowork — Aesir 的 AI 技能获取入口

Aesir 的 AI 编码技能统一放在**整包扩展 AesirFrameworkGame** 里（同时覆盖 Aesir Architecture 与 Aesir
Modules），不再随单个包分发。本文件只讲两件事：**怎么下载这个扩展**、**怎么装进去**。

## 扩展里有什么

一个扩展 = 12 个技能 + 2 篇实战模板：

| 技能                      | 覆盖                                                                                           |
|---------------------------|------------------------------------------------------------------------------------------------|
| `aesir-router`            | 入口与分派：意图 → 技能、组合路径、依赖门禁、常见误路由纠正                                    |
| `aesir-framework`         | 架构底座：Context／Model／View／Controller、Quick 档三件套、核心裁决、命名                     |
| `aesir-tiers`             | 六档分级与模板（MVC／MVP 各三档）、升档触发条件                                                |
| `aesir-core-api`          | `ObservableValue`、可观察集合、`MiniEvent`、`AesirTimer`、`AesirPlayerLoop`                    |
| `aesir-decisions`         | MVC 还是 MVP、为什么没有 MVVM、四种通知机制怎么选                                              |
| `aesir-pitfalls`          | 十条高频陷阱（现象 + 原因 + 修法）                                                             |
| `aesir-project-structure` | PC 独立游戏目录模板、按用途 + Global/Shared/Unique 分层、版本控制边界、docs 文档站（不依赖包） |
| `aesir-boot-scene`        | 启动场景 `Bootstrap.unity` + `Bootstrap.cs` + Build Settings index 0                           |
| `aesir-ui`                | `UIModule`／`UIRoot`：Panel 与 Window、蒙版、世界空间血条                                      |
| `aesir-events`            | `EventModule`：Script 订阅 + 句柄退订、优先级、订阅者过滤器、取消传播、诊断 API、SO 事件资产   |
| `aesir-audio`             | `AudioModule`：`PlaySfx` 轮询、`PlayBgm` 淡变、三通道音量                                      |
| `aesir-scene`             | `SceneModule`：Single／Additive、进度归一化、UniTask awaitable                                 |

外加 `skills/aesir-framework/references/` 下的完整小游戏骨架与原生 MonoBehaviour 迁移表。

## 扩展在哪

| 获取方式                 | 路径                                                                      |
|--------------------------|---------------------------------------------------------------------------|
| 克隆/浏览本仓库          | `.codely-cli/extensions/AesirFrameworkGame/`（仓库根，与 `Assets/` 同级） |
| 团结 Cowork / Codely CLI | 装好后的 `<工程>/.codely-cli/extensions/AesirFrameworkGame/`              |

> 包内的 `TuanjieCowork~/` 只剩这份说明，不再含 `Skills/`；UPM 与 unitypackage 形态都拿不到技能正文——技能只在仓库根的扩展目录里维护。

---

## 一、装了团结 Cowork / Codely CLI 的开发者

### 第 1 步：只拉扩展目录（约 1 MB）

**优先走 CNB（国内直连）**：

```bash
git clone --depth 1 --filter=blob:none --sparse https://cnb.cool/runestone-game/AesirFramework.git aesir-ai-skills
cd aesir-ai-skills
git sparse-checkout set .codely-cli/extensions/AesirFrameworkGame
ls .codely-cli/extensions/AesirFrameworkGame/skills    # 应看到 12 个技能目录
```

CNB 拉不动时换 GitHub 镜像（路径与命令完全相同，只换 URL）：

```bash
git clone --depth 1 --filter=blob:none --sparse https://github.com/yuumixcode/AesirFramework.git aesir-ai-skills
```

> 稀疏克隆会在 `aesir-ai-skills/` 里留一个 `.git`。装完即可删除该临时目录（或至少删掉它的 `.git`
> ，避免和你自己的工程仓库相互干扰）。

### 第 2 步：装进工作区

有 Codely CLI 时用登记命令（`codely` 与 `cowork` 共用同一套扩展目录，`--scope workspace` 装进当前工程，`user`
装成全局）：

```bash
codely extensions install .codely-cli/extensions/AesirFrameworkGame --scope workspace --consent
```

没有 Codely CLI（只有团结 Cowork 客户端）时纯拷贝即可——扩展目录是即插即用的：

```bash
mkdir -p "<你的 Unity 工程>/.codely-cli/extensions"
cp -R .codely-cli/extensions/AesirFrameworkGame "<你的 Unity 工程>/.codely-cli/extensions/"
```

### 第 3 步：验证

```bash
codely extensions list          # 应出现 AesirFrameworkGame [enabled]
```

或用团结 Cowork 打开该工程，直接问「Aesir 的 Quick 档怎么写」——能命中 `aesir-framework` 就说明加载成功。

> 只想装其中几个技能、或用全局技能目录（`~/.codely-cli/skills/`）时，用扩展自带的批量安装脚本：
>
> ```bash
> python3 .codely-cli/extensions/AesirFrameworkGame/scripts/install.py --scope global
> python3 .codely-cli/extensions/AesirFrameworkGame/scripts/install.py --only aesir-router,aesir-ui
> ```

---

## 二、不用团结 Cowork 的开发者

技能正文就是一堆纯 Markdown（`skills/<技能名>/SKILL.md` + 可选 `references/*.md`），没有任何运行时依赖。按下面两步拆到你自己的
AI 工具里：

1. **拿到目录**：用上面第 1 步的稀疏克隆；不装 git
   就去 [GitHub](https://github.com/yuumixcode/AesirFramework/archive/refs/heads/main.zip) 下载仓库 zip，解压后只取
   `.codely-cli/extensions/AesirFrameworkGame/skills/`。
2. **按你的工具形态放置**：
    - 支持「技能目录」的工具（Claude Code、Cline 等）：把 `skills/` 下需要的技能目录整体拷进它的技能目录，
      **保留目录名**——目录名必须与 `SKILL.md` frontmatter 里的 `name` 一致。
    - 只支持单个规则文件的工具（Cursor `.cursor/rules`、`CLAUDE.md`、`AGENTS.md`、Copilot instructions 等）：把
      `skills/aesir-router/SKILL.md` 与你实际需要的技能正文合并成一份规则文件；**references
      的相对路径要一并改成实际位置**，否则 AI 打开 `../aesir-tiers/references/mvc-quick.md` 会落空。
    - 微信/文档式阅读也行：`aesir-router` 是索引，其余按需打开。

> 不要把所有技能一次性塞进上下文——`aesir-router` 的整套设计前提是「按意图只加载一篇」。

---

## 依赖提醒

- 技能只教**怎么写代码**，不负责装包。工程里没有 `cn.runestone.aesir.architecture` 时，架构技能与四个模块技能全部不可用。
- 四个模块技能（`aesir-ui` / `aesir-events` / `aesir-audio` / `aesir-scene`）都要求工程里有
  `cn.runestone.aesir.modules`，且它依赖 RAA。
- 安装包本身见包根 `README.md`；RAM 缺 RAA 时用菜单 `Tools/Aesir/Modules/Install Dependencies` 补装。

## 维护约定

- 技能正文的唯一主位是 `.codely-cli/extensions/AesirFrameworkGame/skills/`，直接在那里改；`TuanjieCowork~/`
  不再放技能副本（出现副本时 `scripts/validate.py` 会报错）。
- 改完跑 `python3 .codely-cli/extensions/AesirFrameworkGame/scripts/validate.py`。
- 技能随框架版本演进；升级后重新拉一次扩展目录覆盖即可。
