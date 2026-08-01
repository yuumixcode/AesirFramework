# Changelog / 变更日志

> **本文件为 monorepo 聚合视图**——同时记录三个子包（Aesir Architecture / Aesir Modules / Aesir Inspector）的版本变更。每个条目标注 `[architecture]` / `[modules]` / `[inspector]` 归属。
>
> 各自子包的**详细历史**（含 0.4.0 之前的版本）见：
> - Aesir Architecture: [`Assets/Runestone/AesirArchitecture/CHANGELOG.md`](./Assets/Runestone/AesirArchitecture/CHANGELOG.md)
> - Aesir Modules: [`Assets/Runestone/AesirModules/CHANGELOG.md`](./Assets/Runestone/AesirModules/CHANGELOG.md)
> - Aesir Inspector: [`Assets/Runestone/AesirInspector/CHANGELOG.md`](./Assets/Runestone/AesirInspector/CHANGELOG.md)（中文） / [`Documentation~/CHANGELOG_EN.md`](./Assets/Runestone/AesirInspector/Documentation~/CHANGELOG_EN.md)（English）

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/)，
版本号遵循 [Semantic Versioning](https://semver.org/lang/zh-CN/)。

Format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
versions follow [Semantic Versioning](https://semver.org/).

---

## 当前版本 / Current Version

| 子包 / Sub-Package | 包名 / Package ID | 版本 / Version |
|---|---|---|
| Aesir Architecture | `cn.runestone.aesir.architecture` | **0.5.0** |
| Aesir Modules | `cn.runestone.aesir.modules` | **0.5.0** |
| Aesir Inspector | `cn.runestone.aesir.inspector` | **0.5.0** |

> **安装方式 / Installation**：本仓库作为单一 monorepo 发布，三个子包均通过 [UPM Git URL](https://github.com/yuumixcode/Unity-Aesir-Packages.git) 拉取，按需选用。
> *The repository is published as a single monorepo. All three sub-packages are pulled via [UPM Git URL](https://github.com/yuumixcode/Unity-Aesir-Packages.git) and used on demand.*
>
> **依赖关系 / Dependency**:
> - **Aesir Architecture** — 不依赖任何 Aesir 子包 / depends on no Aesir sub-package
> - **Aesir Inspector** — 不依赖任何 Aesir 子包 / depends on no Aesir sub-package
> - **Aesir Modules** — 同时依赖 Aesir Architecture + Aesir Inspector / depends on BOTH Aesir Architecture AND Aesir Inspector

---

## [0.5.0] - 2026-08-01

---

### [architecture] Aesir Architecture

#### Fixed

- **单例竞争修复**：`AesirArchitecture` 重复实例 `Destroy` 后提前 `return`，避免继续执行赋值和 `DontDestroyOnLoad`；`OnDestroy` 仅在 `_instance == this` 时清空，避免销毁非自身实例时误清
- **RemoveListenerTrigger**：移除 `[DisallowMultipleComponent]` 限制

---

### [modules] Aesir Modules

#### Changed

- 目录重命名：`Odin Integration` → `OdinIntegration`（与 Inspector 保持一致）
- 版本号与 Aesir Architecture / Aesir Inspector 同步更新至 `0.5.0`

---

### [inspector] Aesir Inspector

#### Added

- **Odin 自动 Tooltip (OdinAutoTooltip)** ⚡：从源代码 XML `/// <summary>` 注释自动生成 Inspector Tooltip 的 Odin 属性处理器。提取自 [JakePineOdinTools](https://github.com/JakePineGames/JakePineOdinTools)（MIT, © 2026 Jake Pine）。已有 Tooltip 时读取现有值并追加新内容后动态替换原特性
- **ScriptDocGenerator 源码 Summary 解析**：`MemberData` 添加 `SummaryResolver` 委托，Editor 程序集加载时注入源码解析实现，从 `.cs` 文件的 XML `/// <summary>` 注释中读取成员摘要
- **ScriptDocGenerator OdinMenuEditorWindow 重构**：窗口从 `OdinEditorWindow` 重写为 `OdinMenuEditorWindow`，左侧菜单 4 种工作模式（单脚本、多脚本、单程序集、多程序集），每种模式独立面板 SO
- **共享源码解析工具**：`OdinSourceFileHelper`（源文件定位与成员声明提取）和 `SourceSummaryParser`（XML summary 解析），消除 `SourceSummaryInitializer` 与 `OdinAutoTooltipAttributeProcessor` 之间的重复代码

#### Changed

- 目录重命名：`Odin Integration` → `OdinIntegration`
- **Third Party Notices 更新**：替换占位内容，添加 JakePineOdinTools 第三方组件记录
- **Summary 工具标注为推荐替代**：README 中标注推荐新代码使用 OdinAutoTooltip

#### Removed

- **移除 `[Summary]` 特性装饰**：252 个文件中 897 处 `[Summary("...")]` 装饰已全部移除。`SummaryAttribute` 类保留作为 ScriptDocGenerator 的回退兼容
- **移除 MIT LICENSE 头部**：所有 `.cs` 文件的 LICENSE 头部已移除，仅在 `CodeStyle/AesirInspectorCodeStyle.cs` 保留一份

#### Fixed

- 修复 `ScriptDocGeneratorController.GenerateMultipleTypeDocs` 中 `generatorSettings` 被当作 bool 的 bug

---
- Editor 工具链（SO Inspector / MVP 脚手架 / 模块可视化）
- 运行时集合（RuntimeSet）

**Aesir Modules**:
- Scene 模块（SceneLoader、SceneReference）
- 对象池扩展（当前用隐藏复用，必要时增加 UIForm 对象池）

---

## [0.4.2] - 2026-07-24

---

### [architecture] Aesir Architecture

#### Fixed

- **GetModel / GetService 初始化状态校验** — 在 `GetModel<T>()` 和 `GetService<T>()` 扩展方法中，null 检查之后新增 `Initialized` 状态检查。若目标已注册但尚未初始化，抛出 `InvalidOperationException` 并报告调用者类型和目标类型，提示注册顺序错误或循环依赖。此前获取已注册但未初始化的模块会静默返回半成品实例，可能导致难以排查的运行时错误

---

### [modules] Aesir Modules

#### Changed

- 版本号与 Aesir Architecture / Aesir Inspector 同步更新至 `0.4.2`，本包本版本无功能性变更

---

### [inspector] Aesir Inspector

#### Changed

- 版本号与 Aesir Architecture / Aesir Modules 同步更新至 `0.4.2`，本包本版本无功能性变更

---

## [0.4.1] - 2026-07-24

---

### [architecture] Aesir Architecture

#### Changed

- **Samples 代码优化**：Counter-MVC 的 View（`SampleMvcCounterMainPanel`）和 Counter-MVP 的 Presenter（`SampleMvpCounterPresenter`）从缓存 Model 字段引用改为属性每次获取（`=> this.GetModel<T>()`），并添加注释说明此模式支持运行时动态替换 Model/Service，旧实例可被 GC 正常回收
- **Samples 版本文件夹**：`Assets/Samples/Aesir Architecture/0.3.2/` → `0.4.0/`，与 `package.json` 版本对齐

---

### [inspector] Aesir Inspector

#### Changed

- **Samples 版本文件夹**：`Assets/Samples/Aesir Inspector/0.4.0-pre.1/` → `0.4.0/`，与 `package.json` 版本对齐

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
