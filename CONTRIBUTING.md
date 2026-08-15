# Contributing to Unity-Aesir-Packages / 贡献指南

> 感谢你考虑为本项目做出贡献！无论是 Bug 报告、功能建议、文档改进还是代码提交，我们都非常欢迎。
>
> Thanks for considering contributing! Bug reports, feature requests, documentation improvements, and code contributions are all welcome.

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
- [问题与帮助](#问题与帮助)

### 行为准则

本项目采用 [Contributor Covenant v2.1](./CODE_OF_CONDUCT.md) 行为准则。参与本项目即表示你同意遵守其条款——以尊重和建设性的方式对待每一位社区成员。

### 如何贡献

#### 报告 Bug

1. 在 [Issues](https://github.com/yuumixcode/Unity-Aesir-Packages/issues) 搜索是否已有相同问题。
2. 如果没有，**创建新 Issue**，并附上：
   - **复现步骤**：最小可复现路径
   - **预期行为** vs **实际行为**
   - **环境信息**：Unity / Tuanjie 版本、是否启用 Odin、操作系统
   - **截图/日志**（如适用）
3. 标注影响哪个子包：`Aesir Architecture` / `Aesir Modules` / `Aesir Inspector` / `monorepo`

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
- **可选依赖**: [Odin Inspector](https://odininspector.com/) 3.3.x+（用于开发 OdinIntegration 增强功能）
- **Git**: 用于版本控制

#### 克隆仓库

```bash
git clone https://github.com/yuumixcode/Unity-Aesir-Packages.git
```

#### 预热包缓存（不开 GUI）

```bash
Unity -batchmode -quit -projectPath . -nographics -logFile /dev/null
```

### 仓库结构

> 这是**多包 monorepo**——三个子包都放在 `Assets/Runestone/` 下，每个子包都有自己独立的 `package.json`、`README.md`、`CHANGELOG.md`。
>
> This is a **multi-package monorepo** — three sub-packages live under `Assets/Runestone/`, each with its own `package.json`, `README.md`, and `CHANGELOG.md`.

```
Assets/Runestone/
├── AesirArchitecture/      # Aesir Architecture 包源（不依赖其他 Aesir 子包）
├── AesirModules/           # Aesir Modules 包源（依赖 Architecture）
└── AesirInspector/         # Aesir Inspector 包源（不依赖其他 Aesir 子包；强依赖 Odin Inspector）
```

详细目录与程序集结构见 [`AGENTS.md`](./AGENTS.md) / [`CODELY.md`](./CODELY.md)。

### 编码规范

#### 通用原则

- **C# 语言** / C#
- 类用 PascalCase，接口加 `I` 前缀，抽象类加 `Abstract` 前缀
- `MonoBehaviour` 单例：静态 `Instance` + `[DefaultExecutionOrder(-999)]` + `DontDestroyOnLoad`
- 通过 `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]` 自动启动
- Domain Reload 安全：非泛型静态单例在类内用 `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]` 重置；泛型类（如 `AbstractContext<T>`）的 RIOLM 会被 Unity 静默跳过，必须经 `ResetStaticsAssistant.Register()` 注册重置回调
- Runtime / Editor 代码用 asmdef 隔离
- **XML 文档注释用中文；标识符用英文**

#### 子包级规范

| 子包 | 规范来源 |
|---|---|
| Aesir Architecture | [`AGENTS.md`](./AGENTS.md) + [`CODELY.md`](./CODELY.md) |
| Aesir Modules | 同上 |
| Aesir Inspector | [`Assets/Runestone/AesirInspector/CONTRIBUTING.md`](./Assets/Runestone/AesirInspector/CONTRIBUTING.md)（最详细，含 CodeStyle 文件路径） |

> ⚠️ Aesir Inspector 采用**无 XML 注释范式**（用 `[Summary]` 特性替代），与其他两个子包风格不同。贡献前请先阅读对应子包的规范。

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
| `chore:` | 构建/工具/依赖 | `chore: Bump Aesir Architecture to 0.3.2` |

> **Scope 建议**：`feat(architecture):` / `fix(modules):` / `chore(inspector):` —— 通过 scope 标明影响哪个子包。

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
- [ ] 公共 API 有中文 XML 文档注释（Aesir Inspector 除外，用 `[Summary]`）
- [ ] 未引入未使用的依赖
- [ ] `Assets/Plugins/Sirenix/` 未被提交（已 gitignore）
- [ ] 提交信息遵循 Conventional Commits
- [ ] 涉及子包功能变更时，已更新对应子包 `CHANGELOG.md` 的 `[Unreleased]`

### 问题与帮助

- **Bug 报告 & 功能建议**: [GitHub Issues](https://github.com/yuumixcode/Unity-Aesir-Packages/issues)
- **讨论 & 提问**: [GitHub Discussions](https://github.com/yuumixcode/Unity-Aesir-Packages/discussions)
- **子包 Aesir Inspector 专属问题**: 前往 [yuumixcode/aesir-inspector](https://github.com/yuumixcode/Unity-Aesir-Packages/issues) 提 Issue

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
- [Getting Help](#getting-help-1)

### Code of Conduct

This project follows the [Contributor Covenant v2.1](./CODE_OF_CONDUCT.md). By participating, you agree to its terms — treat every community member with respect and constructive intent.

### How to Contribute

#### Reporting Bugs

1. Search [Issues](https://github.com/yuumixcode/Unity-Aesir-Packages/issues) for duplicates.
2. If none, open a new Issue with:
   - **Steps to reproduce** (minimal)
   - **Expected vs actual** behavior
   - **Environment**: Unity / Tuanjie version, Odin status, OS
   - **Screenshots / logs** if applicable
3. Tag the affected sub-package: `Aesir Architecture` / `Aesir Modules` / `Aesir Inspector` / `monorepo`

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
- **Optional**: [Odin Inspector](https://odininspector.com/) 3.3.x+ (for OdinIntegration development)
- **Git**

Clone:

```bash
git clone https://github.com/yuumixcode/Unity-Aesir-Packages.git
```

Pre-warm package cache without GUI:

```bash
Unity -batchmode -quit -projectPath . -nographics -logFile /dev/null
```

### Repository Structure

> This is a **multi-package monorepo** — three sub-packages live under `Assets/Runestone/`, each with its own `package.json`, `README.md`, and `CHANGELOG.md`.

```
Assets/Runestone/
├── AesirArchitecture/      # Aesir Architecture source
├── AesirModules/           # Aesir Modules source (depends on Architecture)
└── AesirInspector/         # Aesir Inspector source (independent; requires Odin Inspector)
```

See [`AGENTS.md`](./AGENTS.md) / [`CODELY.md`](./CODELY.md) for detailed layout.

### Coding Standards

#### General

- C#
- PascalCase for classes, `I` prefix for interfaces, `Abstract` prefix for abstracts
- `MonoBehaviour` singletons: static `Instance` + `[DefaultExecutionOrder(-999)]` + `DontDestroyOnLoad`
- `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]` for auto-bootstrap
- Domain Reload safety: in-class `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]` resets for non-generic static singletons; generic classes (e.g. `AbstractContext<T>`) whose RIOLM is silently ignored by Unity must register reset callbacks via `ResetStaticsAssistant.Register()`
- Runtime / Editor isolation via asmdef
- **XML doc comments in Chinese; identifiers in English**

#### Per-Sub-Package

| Sub-Package | Standards Reference |
|---|---|
| Aesir Architecture | [`AGENTS.md`](./AGENTS.md) + [`CODELY.md`](./CODELY.md) |
| Aesir Modules | Same as above |
| Aesir Inspector | [`Assets/Runestone/AesirInspector/CONTRIBUTING.md`](./Assets/Runestone/AesirInspector/CONTRIBUTING.md) (most detailed; includes CodeStyle file path) |

> ⚠️ Aesir Inspector uses a **no-XML-comment style** (`[Summary]` attributes instead) — different from the other two. Read the relevant sub-package's guide before contributing.

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
| `chore:` | Build / tooling / deps | `chore: Bump Aesir Architecture to 0.3.2` |

> **Recommended scope**: `feat(architecture):` / `fix(modules):` / `chore(inspector):` — make the affected sub-package explicit.

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
- [ ] Public API has Chinese XML doc comments (except Aesir Inspector, which uses `[Summary]`)
- [ ] No new unused dependencies
- [ ] `Assets/Plugins/Sirenix/` not committed (gitignored)
- [ ] Commit messages follow Conventional Commits
- [ ] Sub-package feature changes have updated the corresponding `CHANGELOG.md` `[Unreleased]` section

### Getting Help

- **Bug reports & feature requests**: [GitHub Issues](https://github.com/yuumixcode/Unity-Aesir-Packages/issues)
- **Discussions & Q&A**: [GitHub Discussions](https://github.com/yuumixcode/Unity-Aesir-Packages/discussions)
- **Aesir Inspector-specific issues**: open in [yuumixcode/aesir-inspector](https://github.com/yuumixcode/Unity-Aesir-Packages/issues)

---

感谢你的贡献！每一次提交都让 Aesir Packages 变得更好。
Thanks for contributing — every commit makes Aesir Packages better.
