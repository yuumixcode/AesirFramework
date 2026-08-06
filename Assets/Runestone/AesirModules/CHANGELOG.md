# Changelog

本项目的所有重要变更均会记录在此文件中。

格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/)，
版本号遵循 [Semantic Versioning](https://semver.org/lang/zh-CN/)。

## [0.7.0] - 2026-08-05

### Changed

- **单例模式重构：预放置优先** — 所有 MonoBehaviour 单例（`AesirModules`、`UIRoot`、`UIModule`、`EventModule`、`SceneModule`）的 `Instance` getter 优先通过 `FindAnyObjectByType` 搜索已加载场景中预放置的实例，未找到时才运行时创建
- **条件式 DontDestroyOnLoad** — `AesirModules` 和 `UIRoot` 新增 `static bool _createdByRuntime` 标志，仅运行时创建的实例调用 `DontDestroyOnLoad`，场景中预放置的实例保留在场景中随场景生命周期销毁
- **移除 Bootstrap 自动初始化** — 移除 `AesirModules` 的 `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)] Bootstrap()` 方法，避免在场景加载前创建 DDOL 实例抢占预放置实例

### Removed

- `AesirModules.Bootstrap()` 方法及其 `[RuntimeInitializeOnLoadMethod]` 特性

## [0.6.0] - 2026-08-05

### Changed

- 版本号与 Aesir Architecture / Aesir Inspector 同步更新至 `0.6.0`，本包本版本无功能性变更

## [0.5.0] - 2026-08-01

### Changed

- 目录重命名：`Odin Integration` → `OdinIntegration`（与 Inspector 保持一致）
- 版本号与 Aesir Architecture / Aesir Inspector 同步更新至 `0.5.0`，本包本版本无功能性变更

## [Unreleased]

### Added

- **事件模块（Event Module）**：⚠️ 实验性模块，尚未在实际项目中验证
  - 双轨订阅：Attribute 订阅（`[AesirListener]` 特性）+ Script 订阅（`AddListener<T>` 动态 Lambda）
  - 5 档优先级排序分发（First/High/Medium/Low/Last）
  - `AutoRemoveListenerHandle` 退订句柄（与 `MiniEvent` 模式一致）
  - 表达式树编译优化：`StaticBindingInfo` 在注册时将 `MethodInfo` 编译为委托，热路径零反射
  - 双注册表分离（`AttributeBindings` + `DynamicBindings`）
  - `AesirEventArgs` 基类 + `AesirListenerAttribute` 特性 + `AesirEventUtility` 工具
  - 示例 `Samples~/Events/01_KeyPress/` — 按键发布-订阅演示
  - 文档 `Documentation~/event-module.md`

### Changed

- **package.json**：更新描述反映事件模块，新增 `samples` 数组声明
- **README**：新增事件模块章节（核心类型表、快速开始、API 速查、目录结构）

### 规划中

- Scene 模块（SceneLoader、SceneReference）
- 对象池扩展（当前用隐藏复用，必要时增加 UIForm 对象池）

## [0.4.2] - 2026-07-24

### Changed

- 版本号与 Aesir Architecture / Aesir Inspector 同步更新至 `0.4.2`，本包本版本无功能性变更

## [0.4.0] - 2026-07-24

### Changed（Manager of Managers 模式，合并为 UIManager）

- **架构重构**：UI 模块从 RAA Service 简化为 Manager of Managers 模式。原 `UISystem`（纯 C# 单例）与 `UIRoot`（MonoBehaviour 根节点）合并为 `UIManager`——单一 MonoBehaviour 单例（继承 `AesirMonoBehaviour`），同时承担面板管理与 UI 层级构建职责。
- **两层结构**：移除 `Runtime/Odin Integration/` 空壳程序集，架构精简为 Engine/UI/（接口、配置、枚举）+ Component/UI/（UIManager、AesirUIPanel、UICanvasConfigSO）。Odin 条件编译通过 `#if ODIN_INSPECTOR` 实现，无需独立程序集。
- **API 精简**：移除 `Back()`、`CloseAll()`、`Get<T>()`、`pauseUnderneath`、`OnPause`/`OnResume`、`SetUIRoot()` 等不适用的功能；生命周期简化为 `OnInit` → `OnShow` → `OnHide`/`OnClose`。
- **命名调整**：管理器统一为 `UIManager`；面板基类 `Panel` → `AesirUIPanel`；加载器接口 `IUILoader` → `IUIAssetLoader`。
- **预制体管理**：恢复 `RegisterPrefab<T>` 静态快捷方法，支持注册模式和路径模式并存。
- **XML 注释**：全部公共成员补充 XML 文档注释，统一使用多行格式。
- **文档重写**：README、使用手册、机制文档、调研分析全部同步为当前架构。

## [0.3.0] - 2026-07-12

### Changed（重构 · 作为 RAA 模块接入，去前缀，三层结构）

- **接入方式**：UI 模块整体作为 **Aesir Architecture (RAA) 的一个 Service** 接入。`UIManager : AbstractService`，注册于 `UIContext : AbstractContext<UIContext>`；提供 `UIManager.Service` 便捷单例入口（首次访问自动构建 UI 根节点）。
- **去前缀**：移除全部 `Aesir` 前缀——`AesirUIManager→UIManager`、`AesirPanel→Panel`、`AesirUILayer→UILayer`、`AesirPanelConfig→PanelConfig`、`AesirUILog→UILog`、`AesirResourcesUILoader→ResourcesUILoader`、`IAesirPanel→IUIPanel`、`IAesirUILoader→IUILoader`；Canvas 配置 `AesirCanvasConfigSO→UICanvasConfigSO`。
- **三层结构（镜像 RAA）**：`Runtime/UI/` 拆分为
  - `Runtime/Engine/`（纯 C# 架构核心：`UIManager`/`UIContext`/`UILayer`/`PanelConfig`/`IUIPanel`/`IUILoader`/`IUICanvasConfig`/`ResourcesUILoader`/`UILog`）
  - `Runtime/Component/`（`Panel` 基类，继承 RAA `AesirView<UIContext>`）
  - `Runtime/Odin Integration/`（`UICanvasConfigSO` + asmdef）+ `Editor/Odin Integration/`（AttributeProcessor + asmdef）
- **依赖回归 RAA**：新增依赖 `cn.runestone.aesir.architecture` 0.3.2（在 0.2.0 中曾被移除）；`Odin Inspector` 重新作为**可选**集成层（asmdef `defineConstraints: ODIN_INSPECTOR`，未导入时自动排除，SO 退化为普通 `ScriptableObject`）。
- **Canvas 结构**：由单 Canvas + 兄弟节点改为 1 个 Canvas + 4 层独立子 Canvas（`overrideSorting`），`SortOrder = 层级序号 × 100 + 配置偏移`。
- **API 变更**：移除 `RegisterPrefab`（统一走 `IUILoader`）；`Instance` 单例改为 `UIManager.Service`；新增 `SetCanvasConfig(IUICanvasConfig)`。

### Added

- `UIContext`：`AbstractContext<UIContext>`，在 `Configure` 中注册 `UIManager`。
- `IUICanvasConfig` 契约接口 + `UICanvasConfigSO`（Odin Integration 层，`CreateAssetMenu` 路径 `Aesir Modules/UI Canvas Config`）。
- `Panel.CloseSelf()`：面板内便捷关闭自身。
- PlayMode 单元测试（`Tests/Runtime/UIManagerTests.cs`）：覆盖 Open/Close/Back/pauseUnderneath。

### Removed

- `RegisterPrefab` / `AesirUIManager.Instance` 旧 API。
- 旧的 `Runtime/UI/` 平面目录。

## [0.2.0] - 2026-07-12

### Changed（重构 · 极简化为零依赖框架）

- **AesirUIManager** — 重写为场景单例：自动创建 Canvas + 四层全屏根节点（Background/Base/Popup/Top）；新增**导航栈**（`Open` 入栈 / `Back` 出栈）；加载通过 `IAesirUILoader` 解耦（默认 Resources 加载器）
- **AesirPanel** — 完善生命周期虚方法：`OnInit → OnShow → OnHide → OnClose`，实现 `IAesirPanel` 接口
- **AesirPanelConfig** — `layer` 默认 `Base`；`destroyOnHide` 重命名为 `recycleOnClose`
- **AesirUILayer** — 由三层（Background/Normal/Top）扩展为四层（Background/Base/Popup/Top），数值连续 0/1/2/3
- **API 变更**：`RegisterPanelPrefab`→`RegisterPrefab`、`ShowPanel`→`Open`、`HidePanel`→`Close`、`GetPanel`→`Get`，新增 `Back()` / `CloseAll()`
- **依赖精简**：移除对 `cn.runestone.aesir.architecture`（RAA）的依赖；移除 `Odin Inspector` 依赖，Inspector 表现改用原生 `[Header]/[Tooltip]` 特性
- **新增**：`Core/IAesirPanel.cs`、`Core/IAesirUILoader.cs`、`Loaders/AesirResourcesUILoader.cs`、`AesirUILog.cs`、`AesirUICanvasConfigSO.cs`（替代原 `AesirCanvasConfigSO` + Odin AttributeProcessor）
- **移除**：`AesirCanvasConfigSO`（被 `AesirUICanvasConfigSO` 取代）、`Editor/OdinIntegration`（Odin 集成，不再需要）

### Added

- 参考 QFramework UIKit / GameFramework UI / SUIFW 的设计，提供层级管理 + 导航栈 + 标准生命周期的极简组合
- 调研分析文档 `Documentation~/ui-framework-analysis.md`

## [0.1.0] - 2026-07-12

### Added

- **AesirUIManager** — UI 管理器单例，三层 Canvas（Background/Normal/Top）管理，Panel 注册/显示/隐藏/获取 API，Domain Reload 安全
- **AesirPanel** — Panel 抽象基类，继承 `AesirMonoBehaviour`，提供 Show/Hide 虚方法
- **AesirPanelConfig** — Panel 配置类（层级 + destroyOnHide）
- **AesirUILayer** — 面板层级枚举（Background=0/Normal=1/Top=2）
- **AesirCanvasConfigSO** — Canvas 统一配置 SO，继承 `AesirScriptableObject`，提供 ApplyToCanvas() 方法
- **AesirCanvasConfigSOAttributeProcessor** — Odin Inspector AttributeProcessor，为 CanvasConfigSO 注入分组和条件显示特性
- **EnsureAesirModulesDefine** — 自动注册 `AESIR_MODULES` 宏定义符号
- **单元测试** — AesirPanelConfig、AesirUILayer、AesirUIManager 面板流程测试
