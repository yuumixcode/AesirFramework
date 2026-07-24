# Changelog / 变更日志

> **本文件为 monorepo 聚合视图**——同时记录三个子包（Aesir Architecture / Aesir Modules / Aesir Inspector）的版本变更。每个条目标注 `[architecture]` / `[modules]` / `[inspector]` 归属。
>
> 各自子包的**详细历史**（含 0.4.0 之前的版本）见：
> - Aesir Architecture: [`Assets/Runestone/AesirArchitecture/CHANGELOG.md`](./Assets/Runestone/AesirArchitecture/CHANGELOG.md)
> - Aesir Modules: [`Assets/Runestone/AesirModules/CHANGELOG.md`](./Assets/Runestone/AesirModules/CHANGELOG.md)
> - Aesir Inspector: [`Assets/Runestone/AesirInspector/CHANGELOG.md`](./Assets/Runestone/AesirInspector/CHANGELOG.md)（中文） / [`Documentation~/en/CHANGELOG.md`](./Assets/Runestone/AesirInspector/Documentation~/en/CHANGELOG.md)（English）

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/)，
版本号遵循 [Semantic Versioning](https://semver.org/lang/zh-CN/)。

Format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
versions follow [Semantic Versioning](https://semver.org/).

---

## 当前版本 / Current Version

| 子包 / Sub-Package | 包名 / Package ID | 版本 / Version |
|---|---|---|
| Aesir Architecture | `cn.runestone.aesir.architecture` | **0.4.0** |
| Aesir Modules | `cn.runestone.aesir.modules` | **0.4.0** |
| Aesir Inspector | `cn.runestone.aesir.inspector` | **0.4.0** |

> **安装方式 / Installation**：本仓库作为单一 monorepo 发布，三个子包均通过 [UPM Git URL](https://github.com/yuumixcode/Unity-Aesir-Packages.git) 拉取，按需选用。
> *The repository is published as a single monorepo. All three sub-packages are pulled via [UPM Git URL](https://github.com/yuumixcode/Unity-Aesir-Packages.git) and used on demand.*
>
> **依赖关系 / Dependency**:
> - **Aesir Architecture** — 不依赖任何 Aesir 子包 / depends on no Aesir sub-package
> - **Aesir Inspector** — 不依赖任何 Aesir 子包 / depends on no Aesir sub-package
> - **Aesir Modules** — 同时依赖 Aesir Architecture + Aesir Inspector / depends on BOTH Aesir Architecture AND Aesir Inspector

---

## [Unreleased] / 未发布

### Changed

- **依赖关系明确 / Dependency clarified**：
  - **Aesir Architecture** — 明确不依赖任何 Aesir 子包（独立可装）
  - **Aesir Inspector** — 明确不依赖任何 Aesir 子包（独立可装）
  - **Aesir Modules** — 明确**同时依赖** Aesir Architecture + Aesir Inspector（`package.json` 的 `dependencies` 字段同步更新）
- **README 拆中英文 / README split by language**：
  - 根目录新增 `README.en.md`（英文版）
  - Aesir Architecture 新增 `README.en.md`
  - Aesir Modules 新增 `README.en.md`
  - Aesir Inspector 英文版保留在 `Documentation~/en/README.md`（已存在，未变动）
  - 拆完后中英文不再交叉在同一文档中
- **Aesir Modules 依赖升级**：`cn.runestone.aesir.architecture` 由 `0.3.2` → `0.4.0`

### Added

- 三个子包 README 顶部 monorepo 引用块重写，明确各包的依赖关系

### 规划中 / Planned

**Aesir Architecture**:
- ScriptableObject 可视化配置层
- SO EventChannel 事件通道
- Editor 工具链（SO Inspector / MVP 脚手架 / 模块可视化）
- 运行时集合（RuntimeSet）

**Aesir Modules**:
- Scene 模块（SceneLoader、SceneReference）
- 对象池扩展（当前用隐藏复用，必要时增加 UIForm 对象池）

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
