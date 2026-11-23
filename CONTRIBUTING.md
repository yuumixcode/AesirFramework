# Contributing to AesirFramework / 贡献指南

> 感谢你考虑为本项目做出贡献！无论是 Bug 报告、功能建议、文档改进还是代码提交，我们都非常欢迎。
>
> Thanks for considering contributing! Bug reports, feature requests, documentation improvements, and code contributions are all welcome.
>
> 🏠 主仓库：[GitHub](https://github.com/yuumixcode/AesirFramework)（提交 / PR / tag / 流水线源头）· [CNB](https://cnb.cool/runestone-game/AesirFramework) 为国内下载仓（内容由 GitHub CI 自动同步）——**Issue / PR 都在 GitHub 提**。
> Primary repo: [GitHub](https://github.com/yuumixcode/AesirFramework) (source of commits / PRs / tags / pipelines) · [CNB](https://cnb.cool/runestone-game/AesirFramework) is the mainland-China download repository (auto-synced by GitHub CI) — **file Issues / PRs on GitHub**.

[English](#english) | [中文](#中文)

---

## 中文

### 目录

- [行为准则](#行为准则)
- [如何贡献](#如何贡献)
  - [报告 Bug](#报告-bug)
  - [建议功能](#建议功能)
  - [贡献代码](#贡献代码)
  - [改进文档](#改进文档)
- [开发环境搭建](#开发环境搭建)
- [仓库结构](#仓库结构)
- [编码规范](#编码规范)
- [测试要求](#测试要求)
- [提交规范](#提交规范)
- [Pull Request 流程](#pull-request-流程)
- [迭代检查清单](#迭代检查清单)
- [问题与帮助](#问题与帮助)

### 行为准则

本项目采用 [Contributor Covenant v2.1](./CODE_OF_CONDUCT.md) 行为准则。参与本项目即表示你同意遵守其条款——以尊重和建设性的方式对待每一位社区成员。

### 如何贡献

#### 报告 Bug

1. 在 [Issues](https://github.com/yuumixcode/AesirFramework/issues) 搜索是否已有相同问题。
2. 如果没有，**创建新 Issue**，并附上：
   - **复现步骤**：最小可复现路径
   - **预期行为** vs **实际行为**
   - **环境信息**：Unity / Tuanjie 版本、是否启用 Odin、操作系统
   - **截图/日志**（如适用）
3. 标注影响哪个子包：`Aesir Architecture` / `Aesir Modules` / `monorepo`

#### 建议功能

1. 搜索是否已有类似 Issue。
2. 创建新 Issue，使用 `Feature Request` 标签，附上：
   - **使用场景**：解决什么具体问题
   - **建议方案**：你期望的方案
   - **备选方案**：你考虑过的其他方案
3. 说明属于哪个子包。

#### 贡献代码

1. Fork 本仓库。
2. 从 `main` 拉特性分支（见下方 [Pull Request 流程](#pull-request-流程)）。
3. 在对应的 `Assets/Runestone/<子包>/` 下开发。
4. 遵循 [编码规范](#编码规范)。
5. 补充/更新测试。
6. 确保所有测试通过。
7. 提交 PR 到 `main`。

#### 改进文档

文档和代码同等重要。修改根目录文档、修正错别字、补充示例、补全中文/英文翻译——都欢迎。

### 开发环境搭建

- **Unity / Tuanjie**: 2022.3.62f3c1（或等价 LTS）
- **渲染管线**: URP 14.0.12
- **可选依赖**: [Odin Inspector](https://odininspector.com/) 3.3.x+（用于开发 OdinInspector 程序集的增强功能）
- **Git**: 用于版本控制

#### 克隆仓库

```bash
git clone https://github.com/yuumixcode/AesirFramework.git
# 国内加速 / mainland China: git clone https://cnb.cool/runestone-game/AesirFramework.git
```

#### 预热包缓存（不开 GUI）

```bash
Unity -batchmode -quit -projectPath . -nographics -logFile /dev/null
```

### 仓库结构

> 这是**多包 monorepo**——两个子包都放在 `Assets/Runestone/` 下，每个子包都有自己独立的 `package.json`、`README.md`、`CHANGELOG.md`。
>
> This is a **multi-package monorepo** — two sub-packages live under `Assets/Runestone/`, each with its own `package.json`, `README.md`, and `CHANGELOG.md`.

```
Assets/Runestone/
├── AesirArchitecture/      # Aesir Architecture 包源（不依赖其他 Aesir 子包）
└── AesirModules/           # Aesir Modules 包源（依赖 Architecture）
```

详细目录与程序集结构见 [`CODELY.md`](./CODELY.md)。

### 编码规范

#### 通用原则

- **C# 语言** / C#
- 类用 PascalCase，接口加 `I` 前缀，抽象类加 `Abstract` 前缀
- `MonoBehaviour` 单例：静态 `Instance` + `[DefaultExecutionOrder(-999)]`；是否 DDOL 由序列化字段 `dontDestroyOnLoad` 统一控制，`Instance` getter 优先 `FindAnyObjectByType` 搜索场景预放置实例（预放置优先）
- Domain Reload 安全：非泛型静态单例在类内用 `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]` 重置；泛型类（如 `AbstractContext<T>`）的 RIOLM 会被 Unity 静默跳过，必须经 `ResetStaticsAssistant.Register()` 注册重置回调
- Runtime / Editor 代码用 asmdef 隔离
- **XML 文档注释用中文；标识符用英文**

#### 子包级规范

| 子包 | 规范来源 |
|---|---|
| Aesir Architecture | [`CODELY.md`](./CODELY.md) |
| Aesir Modules | 同上 |

### 测试要求

- 框架: Unity Test Framework 1.1.33（NUnit 断言）
- Edit 模式测试放在 `Assets/Runestone/<子包>/Tests/Editor/`
- Play 模式测试放在 `Assets/Runestone/<子包>/Tests/Runtime/`
- **每个新行为都要补测试** / Every new behavior must include tests
- PR 前所有测试必须通过 / All tests must pass before opening a PR

跑测试：

```bash
Unity -batchmode -quit -projectPath . \
       -testPlatform editmode -runTests \
       -testResults TestResults.xml -logFile test.log
```

### 提交规范

使用 [Conventional Commits](https://www.conventionalcommits.org/)：

| 类型 | 用途 | 示例 |
|---|---|---|
| `feat:` | 新功能 | `feat: Add Query system with CQRS read/write separation` |
| `fix:` | Bug 修复 | `fix: Null reference in ObservableValue drawer` |
| `docs:` | 仅文档变更 | `docs: Update README for Aesir Modules` |
| `refactor:` | 重构（无功能变更） | `refactor: Extract AbstractSubmodule base class` |
| `test:` | 仅测试变更 | `test: Add coverage for AutoRemoveListenerHandle` |
| `chore:` | 构建/工具/依赖 | `chore: Bump Aesir Architecture to 0.14.0` |

> **Scope 建议**：`feat(architecture):` / `fix(modules):` —— 通过 scope 标明影响哪个子包。

### Pull Request 流程

1. **确认有对应 Issue**（Bug 修复或功能建议）；如没有请先创建。
2. 从 `main` 创建特性分支：
   ```bash
   git checkout -b feat/your-feature-name
   # 或
   git checkout -b fix/your-bug-fix
   # 或
   git checkout -b docs/your-doc-update
   ```
3. 编写代码，确保遵循编码规范。
4. 添加/更新测试。
5. 跑完整测试套件确认全绿。
6. 提交变更，提交信息遵循 Conventional Commits。
7. 推送到你的 Fork。
8. **使用 `gh pr create` 创建 PR**，描述引用相关 Issue（如 `Closes #123`）。

#### 分支命名

| 类型 | 格式 | 示例 |
|---|---|---|
| 功能 | `feat/<name>` | `feat/cqrs-query-system` |
| 修复 | `fix/<name>` | `fix/null-ref-in-observable-value` |
| 文档 | `docs/<name>` | `docs/add-english-readme` |
| 重构 | `refactor/<name>` | `refactor/extract-submodule-base` |

#### PR 检查清单

提交前确认：

- [ ] 代码遵循项目编码规范
- [ ] 已添加/更新单元测试
- [ ] 所有测试通过
- [ ] 公共 API 有中文 XML 文档注释
- [ ] 未引入未使用的依赖
- [ ] `Assets/Plugins/Sirenix/` 未被提交（已 gitignore）
- [ ] 提交信息遵循 Conventional Commits
- [ ] 涉及子包功能变更时，已更新对应子包 `CHANGELOG.md` 的 `[Unreleased]`

#### 主仓库与国内下载仓（PR 如何被合并）

主仓库在 [GitHub](https://github.com/yuumixcode/AesirFramework)，[CNB](https://cnb.cool/runestone-game/AesirFramework) 是国内下载仓（直连加速），内容由 GitHub CI 在每次 `main` 推送后自动同步。**贡献流程：Fork GitHub 仓库、向 GitHub `main` 提 PR，维护者直接在 GitHub 合并。**

- PR 在 GitHub 正常 Review 与 Merge，贡献记录完整保留。
- **维护者纪律**：CNB 侧零提交——main 与包分支由 GitHub 同步流水线 force 覆盖，CNB 上的直接提交会被冲掉；CNB AI 修复分支经分支详情页「反推此分支到 GitHub」按钮进入 GitHub 后同样走 PR。已发布的 GitHub `main` 历史永不改写（禁止 rebase / force-push），否则所有消费者的克隆一并失效。

### 迭代检查清单

一个功能从分支到提交，按顺序做完下面每一步再开 PR。**代码写完只是开始**——围绕代码的作业（示例、文档、测试）漏了，当下不影响"功能能跑"，却会让用户拿到跑不通的示例或过时的文档。

1. **定边界**：一句话写清这个功能做什么、明确不做什么；先确认能否用现有能力组合出来。
2. **建分支**：从 `main` 拉 `feat/<name>` / `fix/<name>` / `docs/<name>` / `refactor/<name>`。
3. **写代码**：运行时逻辑放 `Runtime/<模块>/`，编辑器工具放 `Editor/<模块>/`；公开 API 写中文 XML 注释。
4. **补示例与测试**：新增公开 API 配一个能 Play 的最小示例；每个新行为补测试，需要真实帧 / `Destroy` / 场景加载的放 PlayMode。
5. **更新文档**——按改动类型逐项勾：

   | 改了什么 | 必须同步更新 |
   |---|---|
   | 公开 API / 行为变更 | 包内 `Documentation~/` 使用文档 + 包根 `README.md` / `README_EN.md` 特性表 + 示例 + 该包 `CHANGELOG.md` 的 `[Unreleased]` |
   | 新增示例 | `package.json` 的 `samples[]` + `Samples~/` 镜像（含 `.meta`） |
   | Bug 修复 | 该包 `CHANGELOG.md` `[Unreleased]` 的 Fixed + 回归测试 |
   | 改 asmdef 或测试 | 无文档变更，但必须重跑全部测试 |

   > 包内只写"当前怎么用"；设计取舍和历史演进不要写进产品文档。示例主位在 `Samples/`，改完用 `python3 .codely-cli/skills/sync-samples/scripts/sync_samples.py --check` 确认镜像零漂移。

6. **跑门禁**：编译 0 error → EditMode 全绿 → 改了运行时行为再跑 PlayMode。
7. **提交**：一个功能一个 commit，message 说清"为什么"；PR 描述引用相关 Issue。

版本号 bump、根 CHANGELOG 新版本段落、tag 与 Release 由维护者在发版时统一处理，功能 PR **不要**自行改版本号。

### 问题与帮助

- **Bug 报告 & 功能建议**: [GitHub Issues](https://github.com/yuumixcode/AesirFramework/issues)
- **讨论 & 提问**: [GitHub Discussions](https://github.com/yuumixcode/AesirFramework/discussions)
- **Aesir Inspector 相关问题**: Aesir Inspector 已迁移至独立公开仓库，请前往 [yuumixcode/AesirInspector](https://github.com/yuumixcode/AesirInspector/issues) 提 Issue

---

## English

### Table of Contents

- [Code of Conduct](#code-of-conduct-1)
- [How to Contribute](#how-to-contribute)
  - [Reporting Bugs](#reporting-bugs)
  - [Suggesting Features](#suggesting-features)
  - [Contributing Code](#contributing-code)
  - [Improving Docs](#improving-docs)
- [Development Setup](#development-setup-1)
- [Repository Structure](#repository-structure-1)
- [Coding Standards](#coding-standards-1)
- [Testing Requirements](#testing-requirements-1)
- [Commit Conventions](#commit-conventions-1)
- [Pull Request Workflow](#pull-request-workflow-1)
- [Iteration Checklist](#iteration-checklist-1)
- [Getting Help](#getting-help-1)

### Code of Conduct

This project follows the [Contributor Covenant v2.1](./CODE_OF_CONDUCT.md). By participating, you agree to its terms — treat every community member with respect and constructive intent.

### How to Contribute

#### Reporting Bugs

1. Search [Issues](https://github.com/yuumixcode/AesirFramework/issues) for duplicates.
2. If none, open a new Issue with:
   - **Steps to reproduce** (minimal)
   - **Expected vs actual** behavior
   - **Environment**: Unity / Tuanjie version, Odin status, OS
   - **Screenshots / logs** if applicable
3. Tag the affected sub-package: `Aesir Architecture` / `Aesir Modules` / `monorepo`

#### Suggesting Features

1. Search for duplicates first.
2. Open a new Issue with the `Feature Request` label:
   - **Use case** you want to solve
   - **Proposed solution**
   - **Alternatives** you considered
3. Specify which sub-package it belongs to.

#### Contributing Code

1. Fork the repo.
2. Branch from `main` (see [Pull Request Workflow](#pull-request-workflow-1)).
3. Develop under `Assets/Runestone/<sub-package>/`.
4. Follow [Coding Standards](#coding-standards-1).
5. Add / update tests.
6. Ensure all tests pass.
7. Open a PR against `main`.

#### Improving Docs

Docs are first-class. Fix typos, expand examples, complete translations — all welcome.

### Development Setup

- **Unity / Tuanjie**: 2022.3.62f3c1 (or equivalent LTS)
- **Render Pipeline**: URP 14.0.12
- **Optional**: [Odin Inspector](https://odininspector.com/) 3.3.x+ (for developing OdinInspector assembly enhancements)
- **Git**

Clone:

```bash
git clone https://github.com/yuumixcode/AesirFramework.git
# 国内加速 / mainland China: git clone https://cnb.cool/runestone-game/AesirFramework.git
```

Pre-warm package cache without GUI:

```bash
Unity -batchmode -quit -projectPath . -nographics -logFile /dev/null
```

### Repository Structure

> This is a **multi-package monorepo** — two sub-packages live under `Assets/Runestone/`, each with its own `package.json`, `README.md`, and `CHANGELOG.md`.

```
Assets/Runestone/
├── AesirArchitecture/      # Aesir Architecture source
└── AesirModules/           # Aesir Modules source (depends on Architecture)
```

See [`CODELY.md`](./CODELY.md) for detailed layout.

### Coding Standards

#### General

- C#
- PascalCase for classes, `I` prefix for interfaces, `Abstract` prefix for abstracts
- `MonoBehaviour` singletons: static `Instance` + `[DefaultExecutionOrder(-999)]`; DDOL is governed by the serialized `dontDestroyOnLoad` field, and the `Instance` getter first searches for a pre-placed instance via `FindAnyObjectByType` (pre-placed-first)
- Domain Reload safety: in-class `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]` resets for non-generic static singletons; generic classes (e.g. `AbstractContext<T>`) whose RIOLM is silently ignored by Unity must register reset callbacks via `ResetStaticsAssistant.Register()`
- Runtime / Editor isolation via asmdef
- **XML doc comments in Chinese; identifiers in English**

#### Per-Sub-Package

| Sub-Package | Standards Reference |
|---|---|
| Aesir Architecture | [`CODELY.md`](./CODELY.md) |
| Aesir Modules | Same as above |

### Testing Requirements

- Framework: Unity Test Framework 1.1.33 (NUnit assertions)
- Edit-mode tests in `Assets/Runestone/<pkg>/Tests/Editor/`
- Play-mode tests in `Assets/Runestone/<pkg>/Tests/Runtime/`
- **Every new behavior needs tests**
- All tests must pass before PR

Run tests:

```bash
Unity -batchmode -quit -projectPath . \
       -testPlatform editmode -runTests \
       -testResults TestResults.xml -logFile test.log
```

### Commit Conventions

[Conventional Commits](https://www.conventionalcommits.org/):

| Type | Use | Example |
|---|---|---|
| `feat:` | New feature | `feat: Add Query system with CQRS read/write separation` |
| `fix:` | Bug fix | `fix: Null reference in ObservableValue drawer` |
| `docs:` | Docs only | `docs: Update README for Aesir Modules` |
| `refactor:` | Refactor (no feature change) | `refactor: Extract AbstractSubmodule base class` |
| `test:` | Tests only | `test: Add coverage for AutoRemoveListenerHandle` |
| `chore:` | Build / tooling / deps | `chore: Bump Aesir Architecture to 0.14.0` |

> **Recommended scope**: `feat(architecture):` / `fix(modules):` — make the affected sub-package explicit.

### Pull Request Workflow

1. Make sure there's a corresponding Issue; create one if not.
2. Branch from `main`:
   ```bash
   git checkout -b feat/your-feature-name
   # or
   git checkout -b fix/your-bug-fix
   # or
   git checkout -b docs/your-doc-update
   ```
3. Write code following the standards.
4. Add / update tests.
5. Run the full test suite; all green.
6. Commit with Conventional Commits.
7. Push to your fork.
8. Use `gh pr create` and reference the Issue (`Closes #123`).

#### Branch Naming

| Type | Format | Example |
|---|---|---|
| Feature | `feat/<name>` | `feat/cqrs-query-system` |
| Fix | `fix/<name>` | `fix/null-ref-in-observable-value` |
| Docs | `docs/<name>` | `docs/add-english-readme` |
| Refactor | `refactor/<name>` | `refactor/extract-submodule-base` |

#### PR Checklist

- [ ] Code follows project standards
- [ ] Tests added / updated
- [ ] All tests pass
- [ ] Public API has Chinese XML doc comments
- [ ] No new unused dependencies
- [ ] `Assets/Plugins/Sirenix/` not committed (gitignored)
- [ ] Commit messages follow Conventional Commits
- [ ] Sub-package feature changes have updated the corresponding `CHANGELOG.md` `[Unreleased]` section

#### Primary Repo & Download Mirror (how PRs get merged)

The primary repository lives on [GitHub](https://github.com/yuumixcode/AesirFramework); [CNB](https://cnb.cool/runestone-game/AesirFramework) is the mainland-China download repository (direct access), synced automatically by GitHub CI after every push to `main`. **Contribution workflow: fork the GitHub repo, open PRs against GitHub `main`, and the maintainer merges them directly on GitHub.**

- PRs are reviewed and merged on GitHub as usual; your contribution credit is fully preserved.
- **Maintainer discipline**: zero commits on the CNB side — `main` and the package branches are force-overwritten by the GitHub sync workflow, so direct CNB commits get wiped; CNB AI fix branches reach GitHub via the "push this branch to GitHub" button on the branch page and then go through the same PR flow. Never rewrite published GitHub `main` history (no rebase / force-push), or every consumer clone breaks.

### Iteration Checklist

For one feature from branch to PR, complete the steps below in order. **Finishing the code is only the beginning** — the work around it (samples, docs, tests) does not affect "does it run" right now, but skipping it ships broken samples or stale docs to users.

1. **Set the boundary**: one sentence on what the feature does and, explicitly, what it does *not* do; check first whether existing capabilities can be composed instead.
2. **Branch**: from `main`, create `feat/<name>` / `fix/<name>` / `docs/<name>` / `refactor/<name>`.
3. **Write code**: runtime logic in `Runtime/<module>/`, editor tooling in `Editor/<module>/`; add Chinese XML doc comments to public APIs.
4. **Add samples & tests**: ship a minimal playable sample for each new public API; add tests for every new behavior, placing anything needing real frames / `Destroy` / scene loading in PlayMode.
5. **Update docs** — check each item by change type:

   | What changed | Must also update |
   |---|---|
   | Public API / behavior change | package `Documentation~/` guide + package-root `README.md` / `README_EN.md` feature table + sample + that package's `CHANGELOG.md` `[Unreleased]` |
   | New sample | `package.json` `samples[]` + `Samples~/` mirror (including `.meta`) |
   | Bug fix | that package's `CHANGELOG.md` `[Unreleased]` Fixed + a regression test |
   | asmdef or test change | no doc change, but rerun the full test suite |

   > Package docs describe only "how to use it now"; design rationale and history do not belong in product docs. Samples are authored in `Samples/` — after editing, run `python3 .codely-cli/skills/sync-samples/scripts/sync_samples.py --check` to confirm the mirror has zero drift.

6. **Run the gates**: compile with 0 errors → EditMode all green → PlayMode only if runtime behavior changed.
7. **Commit**: one feature per commit, message explains "why"; reference the related Issue in the PR description.

Version bumps, the root CHANGELOG version section, tags and Releases are handled by maintainers at release time — feature PRs should **not** change version numbers.

### Getting Help

- **Bug reports & feature requests**: [GitHub Issues](https://github.com/yuumixcode/AesirFramework/issues)
- **Discussions & Q&A**: [GitHub Discussions](https://github.com/yuumixcode/AesirFramework/discussions)
- **Aesir Inspector-related issues**: Aesir Inspector now lives in its own public repository — open issues at [yuumixcode/AesirInspector](https://github.com/yuumixcode/AesirInspector/issues)

---

感谢你的贡献！每一次提交都让 AesirFramework 变得更好。
Thanks for contributing — every commit makes AesirFramework better.
