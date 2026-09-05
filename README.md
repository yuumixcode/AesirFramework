# AesirFramework · Aesir 系列 Unity 框架集合

> 面向团结引擎 / Unity 的渐进式 MVC 架构 + UI 框架。两个子包通过 Git URL 直接安装，按需选用。

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE)
[![Unity](https://img.shields.io/badge/Unity-2022.3%2B-black.svg)](https://unity.com/)
[![Tuanjie](https://img.shields.io/badge/Tuanjie-2022.3%2B-blueviolet.svg)](#)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](./CONTRIBUTING.md)
[![Code of Conduct](https://img.shields.io/badge/Contributor%20Covenant-2.1-4baaaa.svg)](./CODE_OF_CONDUCT.md)
[![English](https://img.shields.io/badge/README-English-blue.svg)](./README_EN.md)

---

## ✨ 两个子包，独立可装

> **关键点：所有子包都通过同一 Git 仓库发布。** Aesir Modules **依赖** Aesir Architecture；两者均可独立安装 Architecture。

| 子包 | 用途 | 包名 | 版本 |
|---|---|---|---|
| **Aesir Architecture** | 渐进式 MVC 架构（能力接口组合、Command/Query、PlayerLoop 生命周期、响应式属性） | `cn.runestone.aesir.architecture` | `0.16.0` |
| **Aesir Modules** | UI 框架（Manager of Managers、四层 Canvas、面板生命周期）+ ⚠️ 实验性事件模块 | `cn.runestone.aesir.modules` | `0.16.0` |

> 📝 **命名空间**：所有子包统一使用 `Runestone.*` 命名空间（品牌名"符文石"）。

---

## 🏛️ Aesir Architecture（RAA）——架构框架

**RAA 是一个以"Unity 原生优先"为核心理念的渐进式 MVC 架构**。它不构建与引擎平行的自建体系，而是深度绑定 Unity 的 PlayerLoop、ScriptableObject、Editor API 等原生能力，在保持轻量的同时为中小型到中大型项目提供清晰的分层规范。框架以 **MVC 为主要模式**（`IController` 是推荐的快速开发入口），`IPresenter`（MVP）作为可选的严格分层模式。

### 架构角色与能力接口

框架采用**能力接口组合**模式——每个角色通过组合细粒度能力接口按需暴露能力，而非继承大而全的基类接口：

| 角色 | 接口 | 能力 | 职责 |
|------|------|------|------|
| **Model** | `IModel` → `AbstractModel` | GetModel, GetService | 数据层；持有 `ObservableValue<T>`，修改必经写方法 |
| **Service** | `IService` → `AbstractService` | GetModel, GetService | 跨模块协调；可直写 Model，不能执行 Command/Query |
| **View** | `IView` | GetModel, GetService（只读） | 表现层；自订阅 Model 通知刷新 |
| **Controller** | `IController` | GetModel, GetService, **ExecuteCommand**, **ExecuteQuery** | MVC 模式入口（推荐） |
| **Presenter** | `IPresenter` | 全部 Controller + IDisposable | MVP 模式（可选）；中介 Model ↔ View，View 被动 |
| **Command** | `ICommand` → `AbstractCommand` | Execute() | 写操作（同步、无返回值） |
| **Query** | `IQuery<TResult>` | Execute() → TResult | 读操作（无副作用），CQRS 风格 |

`AbstractContext<T>`（CRTP 泛型静态单例）是架构根：子类在 `Configure()` 中注册 Model / Service，`Instance` 首次访问触发初始化（注册顺序初始化 Model → Service），未注册类型抛含修复提示的异常而非返回 null。

### 三档渐进路径（核心设计）

RAA 最鲜明的特征是**按档位渐进**——从最少概念跑通闭环，到读写全解耦的严格分层，每档只有一个核心增量。MVC 与 MVP 各三档逐课同构对照：

| 档位 | Model 暴露面 | MVC（View 自订阅） | MVP（View 被动、Presenter 推送） |
|------|-------------|--------------------|----------------------------------|
| **第一课 · 快捷档** | 具体类注册，可写 `ObservableValue` 直改 | `MonoViewController<T>` 直改值 | Presenter 直改值并推送 |
| **第二课 · 标准档** | 只读暴露 + 写方法 | Controller 直调写方法 | Presenter 直调写方法 |
| **第三课 · 严格档** | 接口注册 + 只读暴露 + 写方法 | Command 写 + Query 加工读 | Command 写 + Query 读 |

快捷档直改合法、适合原型；标准档封装修改入口（推荐起步）；严格档读写全解耦、扩展性最好。View / Controller / Presenter 在严格档按**业务窄接口**存储（类型层面拿不到 `ExecuteCommand` 等框架能力），读写分离由类型系统闭环。包内提供 6 个计数器示例 + ObservableValue / MiniEvent / PlaneWar 三个实战示例，逐课可导入。

### 核心机制速览

- **`ObservableValue<T>` 响应式属性** — Model 持有可写实例，View 经 `IReadOnlyObservableValue<T>` 只读订阅；`AddListenerAndInvoke` 订阅即同步初始值
- **可观察集合家族** — `ObservableList<T>` / `ObservableDictionary<TKey,TValue>` / `ObservableHashSet<T>` 提供 Added / Removed / Replaced / Updated / Cleared 变更通知，与 ObservableValue 同一套读写分离与事件模式
- **`MiniEvent` / `MiniEvent<T>`** — 零分配轻量事件（直接多播调用，原生 C# fail-fast 语义）；返回 `AutoRemoveListenerHandle` 自动清理，支持随 GameObject 销毁 / 场景卸载自动注销
- **PlayerLoop 原生生命周期** — `AesirArchitecturePlayerLoop` 注入 `BeforeUpdate` / `AfterUpdate` 帧回调，无需 MonoBehaviour；第三方 SDK 覆盖 PlayerLoop 后 `EnsureInjected()` 自愈
- **DDOL 显式决策** — 根单例的 `dontDestroyOnLoad` 序列化字段统一控制预放置 / 运行时两种来源（默认跨场景持久；关闭时随场景卸载销毁，Inspector 警告 + 运行时提醒，多场景叠加加载自行处理）
- **Domain Reload 安全（铁律）** — 静态变量全部显式重置（非泛型类内 RIOLM / 泛型类经 `ResetStaticsAssistant`），反复进出 Play Mode 无残留
- **纯 C# 核心 + MonoBehaviour 适配** — Engine 层零 MonoBehaviour 依赖；`MonoView<T>` / `MonoViewController<T>` 等作为适配层，Odin 可选增强而非运行前置

### 设计哲学

1. **Unity 原生优先** — 用引擎能力（PlayerLoop / SO / Editor API），不自建平行体系
2. **极简边界** — 不做事件总线、Context 多实例、Command 池化 / async / Undo；低概率问题用文档约定与编辑期提示（InfoBox）杜绝，不加运行时防御兜底
3. **样式与逻辑分离** — Inspector 呈现全部经 Odin AttributeProcessor 动态注入，运行时程序集零样式特性

---

## 🧱 Aesir Modules（RAM）——功能模块包

**RAM 是 Architecture 之上的功能模块集合**，当前提供 UI 框架与实验性事件模块，并附带场景管理工具。

### UI 框架

- **`UIModule`（Manager of Managers 单例）** — 静态快捷 API：`UIModule.Show<T>()` / `Hide<T>()` / `Get<T>()` / `Prewarm<T>()` / `RegisterPrefab<T>()`；面板状态机为 激活 → 停用缓存 → 销毁
- **`UIRoot` 四层 Canvas 层级** — 一键构建 Background / Normal / Popup / Top 分层 Canvas + UICamera + EventSystem（`GameObject → Aesir Modules → Create UIRoot`），层级 Canvas 序列化引用持久化
- **面板生命周期** — `IUIPanel` 契约 `Initialize → Show(payload) → Hide → DestroyPanel`；`AesirBasePanel` 提供虚方法 `OnInit` / `OnShow` / `OnHide` / `OnClose`；`DestroyOnHide` 决定隐藏时销毁还是缓存复用
- **可插拔资源加载** — 默认 `ResourcesUILoader`（Resources 目录），实现 `IUIAssetLoader` 即可替换为 Addressables 等
- **预热** — `Prewarm<T>()` 逐帧预实例化，`PrewarmAll()` 分摊首次打开的实例化卡顿
- **Binder 组件绑定（Odin 可选）** — `BinderAssistant` / `BinderTag` 将 UI 元素自动绑定到面板脚本
- **Input System 适配** — 独立程序集在启用 Input System 时自动以 `InputSystemUIInputModule` 替换默认输入模块

### 事件模块（⚠️ 实验性）

双轨订阅事件系统：`[AesirListener]` 特性静态订阅 + `AddListener<T>` 动态 Lambda 订阅，共存于同一分发流程，按 5 档优先级排序执行；静态绑定经表达式树编译委托优化反射开销。**尚未在实际项目中验证，API 可能调整。**

### 场景模块与编辑器工具

- **`SceneModule`** — 启动场景（Bootstrap）自动发现与优先加载、叠加场景动态加载追踪
- **编辑器工具** — `SceneManagerWindow` 场景管理窗口、`BootstrapSceneHelper` 启动场景一键注册、`SceneAssetWrapper` 可序列化场景引用

---

## 🕸️ 依赖关系

```
┌──────────────────────┐
│  Aesir Architecture  │
│  MVC 架构（核心包）    │
│  能力接口 / 命令 / 事件 │
└──────────────────────┘
            ▲
            │ 依赖
            │
┌──────────────────────┐
│   Aesir Modules      │
│   UI 框架             │
└──────────────────────┘
```

**关键约束**：

- **Aesir Architecture** — 不依赖任何 Aesir 子包，可独立安装
- **Aesir Modules** — 依赖 `cn.runestone.aesir.architecture`

---

## 📦 安装方式

### 方式 1：固定版本分支安装（推荐）

在 Unity Package Manager 窗口点击左上角 `+` → `Add package from git URL...`，填入对应子包的 Git URL：

| 子包 | Git URL（固定 0.16.0） |
|---|---|
| Aesir Architecture | `https://github.com/yuumixcode/AesirFramework.git#AesirArchitecture-v0.16.0` |
| Aesir Modules | `https://github.com/yuumixcode/AesirFramework.git#AesirModules-v0.16.0` |

> 版本分支由 CI 在每次推送 `main` 时自动按包目录 subtree split 生成（包内容即分支根目录），仓库只保留最新版本分支。

### 方式 2：unitypackage 导入 + 包内更新器（大陆 / 离线友好）

从 [GitHub Releases](https://github.com/yuumixcode/AesirFramework/releases) 下载对应 unitypackage 拖入项目：

| 资产 | 内容 |
|---|---|
| `AesirArchitecture-v<版本>.unitypackage` | 仅 Aesir Architecture |
| `AesirModules-v<版本>.unitypackage` | 仅 Aesir Modules（不含依赖，需自行导入 Architecture） |
| `AesirFramework-v<版本>.unitypackage` | 两包合并 |

以此方式安装的包装在 `Assets/Runestone/` 下（代码可改），**更新无需手动重新下载**：Unity 菜单 `Tools → Aesir → Check for Updates` 打开包内更新器，一键完成"检测新版本 → 自动备份 → 差集清理残留 → 静默导入"。版本检测面向大陆做了多源兜底（jsDelivr CDN → GitHub API → 重定向探测）；经 CDN 检测，最新发布最长约 12 小时后才会被检测到。

> 经 Git URL（UPM）安装的副本不在更新器管辖内，请直接用 Package Manager 更新。

### 方式 3：跟踪 main 最新（开发预览）

```
https://github.com/yuumixcode/AesirFramework.git?path=Assets/Runestone/AesirArchitecture
https://github.com/yuumixcode/AesirFramework.git?path=Assets/Runestone/AesirModules
```

### 方式 4：通过 `manifest.json` 编辑安装

在项目的 `Packages/manifest.json` 文件中添加：

```json
{
  "dependencies": {
    "cn.runestone.aesir.architecture": "https://github.com/yuumixcode/AesirFramework.git#AesirArchitecture-v0.16.0",
    "cn.runestone.aesir.modules": "https://github.com/yuumixcode/AesirFramework.git#AesirModules-v0.16.0"
  }
}
```

只添加你需要的子包——**只安装 Aesir Modules 时**，UPM 会自动解析依赖并拉取 Aesir Architecture（`package.json` 的 `dependencies` 字段已声明）。

### 安装示例（Samples）

- **本仓库直接浏览 / 下载源码**：示例就在各包的 `Samples/` 文件夹内，可直接查看运行。
- **Git URL 安装**：Package Manager → 选中包 → `Samples` 标签页 → 按需 Import；示例源同时保留在各包的 `Samples~/` 隐藏目录中。
- **unitypackage 导入**：示例随包内含，导入后即可运行。

---

## 🚀 快速开始

> 选一个你感兴趣的子包深入看，下面只是"嗅觉测试"。

### Aesir Architecture — 3 行起一个 Context

```csharp
using Runestone.AesirArchitecture;

public class CounterContext : AbstractContext<CounterContext>
{
    protected override void Configure() => RegisterModel<ICounterModel>(new CounterModel());
}
```

View 订阅刷新、Controller 发布 Command 的完整三课路径，见包内 README 与 6 个计数器示例（Package Manager → Samples）。

完整指南见 [`Assets/Runestone/AesirArchitecture/README.md`](./Assets/Runestone/AesirArchitecture/README.md)（中文）/ [`Documentation/README_EN.md`](./Assets/Runestone/AesirArchitecture/Documentation/README_EN.md)（English）。

### Aesir Modules — 一个 `Show<T>()` 打开面板

```csharp
UIModule.RegisterPrefab<MainMenuPanel>(prefab);
UIModule.Show<MainMenuPanel>();
UIModule.Show<ConfirmDialogPanel, ConfirmData>(new ConfirmData { message = "确定？" });
```

UI 框架提供 Manager-of-Managers 单例、四层 Canvas 层级（`UILayer`）、面板生命周期（激活 → 停用缓存 → 销毁）与可插拔资源加载器（默认 Resources，可换 Addressables）。

完整指南见 [`Assets/Runestone/AesirModules/README.md`](./Assets/Runestone/AesirModules/README.md)（中文）/ [`Documentation/README_EN.md`](./Assets/Runestone/AesirModules/Documentation/README_EN.md)（English）。

---

## 🗺️ 文档地图

| 文档 | 位置 |
|---|---|
| 架构教学（中文/英文） | 各包 `README.md` / `Documentation/README_EN.md` |
| 事件机制决策表（ObservableValue / MiniEvent / 集合 / EventModule 怎么选） | [`AesirArchitecture/Documentation/事件机制决策表.md`](./Assets/Runestone/AesirArchitecture/Documentation/事件机制决策表.md) |
| 常见陷阱清单（10 条高频坑与修法） | [`AesirArchitecture/Documentation/常见陷阱清单.md`](./Assets/Runestone/AesirArchitecture/Documentation/常见陷阱清单.md) |
| AI 编码指南（供 AI 助手按 RAA 规范生成代码） | [`AesirArchitecture/Documentation/AesirArchitecture-Skill/SKILL.md`](./Assets/Runestone/AesirArchitecture/Documentation/AesirArchitecture-Skill/SKILL.md) |
| 事件模块详细手册 | [`AesirModules/Documentation/event-module.md`](./Assets/Runestone/AesirModules/Documentation/event-module.md) |
| 变更日志（仓库级聚合 / 各包子包） | 根 [`CHANGELOG.md`](./CHANGELOG.md) 及各包 `CHANGELOG.md` |

> 包内 `Documentation/` 随 unitypackage 一并导出（不进玩家构建）；Git URL 安装的用户可经 Package Manager 读取 `Documentation~/` 镜像内容。

---

## 🗂️ 仓库目录结构

> 这是一个**多包 monorepo**——两个子包的源都在这里，但每个子包都能独立通过 Git URL 安装。

```
AesirFramework/                            # 你现在看到的仓库
├── README.md                              # 本文件（中文）
├── README_EN.md                           # English version
├── LICENSE                                # MIT
├── CHANGELOG.md                           # 仓库级别变更日志（聚合两个子包）
├── CONTRIBUTING.md                        # 贡献指南
├── CODE_OF_CONDUCT.md                     # 社区准则
├── CODELY.md                              # 架构详细文档
├── Assets/Scripts/Editor/                 # 仓库本地编辑器工具（包导出等，不随包分发）
└── Assets/Runestone/
    ├── AesirArchitecture/                 # 不依赖其他 Aesir 子包
    │   ├── Runtime/  Editor/  Tests/
    │   ├── Samples/                       # 示例（仓库内直接可见可运行）
    │   ├── Samples~/                      # 示例源（Git URL 安装后按需导入）
    │   ├── Documentation/                 # 文档主位（Assets 可见、随 unitypackage 导出）
    │   ├── Documentation~/                # 文档镜像（Git URL 安装隐藏副本）
    │   └── README.md  CHANGELOG.md  package.json
    └── AesirModules/                      # 依赖 Architecture
        ├── Runtime/  Editor/
        ├── Samples/                       # 示例（仓库内直接可见可运行）
        ├── Samples~/                      # 示例源（Git URL 安装后按需导入）
        ├── Documentation/                 # 文档主位（Assets 可见、随 unitypackage 导出）
        ├── Documentation~/                # 文档镜像（Git URL 安装隐藏副本）
        └── README.md  CHANGELOG.md  package.json
```

---

## ✅ 质量与 CI

- **测试** — EditMode 测试 100 个（含包内更新器、Context、Observable 家族等），PlayMode 测试覆盖 MonoLifecycleProxy 快照语义、生命周期事件顺序等；命令行跑法见[开发环境](#️-开发环境)
- **CI（GitHub Actions）** —
  - `auto-release.yml`：每次推送 `main` 自动发布 GitHub Release（三个 unitypackage + 更新器所需的 update-info.json / files-manifest）
  - `auto-publish-branches.yml`：按包目录 subtree split 生成 `AesirArchitecture-v<版本>` / `AesirModules-v<版本>` 固定版本分支

---

## 🛠️ 开发环境

> - **Unity / Tuanjie**: 2022.3.62f3c1（或等价 LTS 版本）
> - **渲染管线**: URP 14.0.12
> - **依赖**: [Odin Inspector](https://odininspector.com/) 3.3.x+ — 可选增强，Aesir Architecture / Modules 通过 `#if ODIN_INSPECTOR` 条件编译集成，未安装时自动排除

首次打开项目时，Unity 会自动从 `Packages/manifest.json` 解析依赖。

### 不想开 GUI 预热包缓存

```bash
Unity -batchmode -quit -projectPath . -nographics -logFile /dev/null
```

### CLI 测试

```bash
# Edit 模式
Unity -batchmode -projectPath . \
       -testPlatform editmode -runTests \
       -testResults TestResults.xml -logFile test.log

# Play 模式 — 把 editmode 换成 playmode
```

---

## 🤝 贡献

欢迎任何形式的贡献——Bug 报告、功能建议、文档改进、代码提交。

- 阅读 [`CONTRIBUTING.md`](./CONTRIBUTING.md) 了解提交流程
- 遵循 [`CODE_OF_CONDUCT.md`](./CODE_OF_CONDUCT.md)
- 推荐从 `main` 拉分支；提交信息遵循 [Conventional Commits](https://www.conventionalcommits.org/)

---

## 📄 许可证

本仓库及两个子包均采用 **MIT License** 开源。

```
MIT License

Copyright (c) 2026 Yuumix
```

详见根目录 [`LICENSE`](./LICENSE)。

---

## 🔗 推荐链接

- **Aesir Inspector** — 独立公开仓库，专门面向 [Odin Inspector](https://odininspector.com/) 开发者的学习工具包：[yuumixcode/AesirInspector](https://github.com/yuumixcode/AesirInspector)
- **作者主页**: [yuumixcode](https://github.com/yuumixcode)
