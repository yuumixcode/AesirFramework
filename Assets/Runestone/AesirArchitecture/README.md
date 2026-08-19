# Aesir Architecture

> 面向团结引擎 / Unity 的渐进式 MVC 架构框架，以 Unity 原生特性为一等公民。

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE.md)
[![Version](https://img.shields.io/badge/version-0.9.0-blue.svg)](./CHANGELOG.md)
[![Unity](https://img.shields.io/badge/Unity-2022.3%2B-black.svg)](https://unity.com/)
[![Install via Git URL](https://img.shields.io/badge/UPM-Git%20URL-blueviolet.svg)](#安装)
[![English](https://img.shields.io/badge/README-English-blue.svg)](./Documentation~/README_EN.md)

> 📦 **本包是 [Unity-Aesir-Packages](https://github.com/yuumixcode/Unity-Aesir-Packages) monorepo 的一部分**。本包**不依赖**其他 Aesir 子包（独立可装）。
>
> 关联包：
> - **[Aesir Inspector](https://github.com/yuumixcode/Unity-Aesir-Packages)**（独立）
> - **[Aesir Modules](https://github.com/yuumixcode/Unity-Aesir-Packages)**（依赖 Architecture）

> **Odin Inspector 为可选增强**（Inspector 样式与调试体验），非运行前置——核心架构流程闭环（Context 注册 → 初始化 → Command/Query → ObservableValue 通知）无 Odin 可完整运行。

## 概述

AesirArchitecture（RAA）是一个以 **Unity 原生优先** 为核心理念的架构框架。它不构建与引擎平行的自建体系，而是深度绑定 Unity 的 PlayerLoop、ScriptableObject、Editor API 等原生能力，在保持轻量的同时为中小型到中大型项目提供清晰的 MVC / MVP 分层。框架以 **MVC 为主要模式**，`IController` 是推荐的快速开发入口；`IPresenter`（MVP）作为可选的严格分层模式。

### 核心特性

- **MVC 优先架构** — `IController` + `ICommand` 命令模式 + `IQuery<TResult>` 查询模式（CQRS），Controller 作为 MVC 的核心入口直接修改 Model；`IPresenter`（MVP）作为可选的严格 Model-View 隔离模式
- **PlayerLoop 原生生命周期** — 通过 `AesirArchitecturePlayerLoop` 将自定义子系统注入 Unity PlayerLoop，提供 `BeforeUpdate` / `AfterUpdate` 帧回调，无需 MonoBehaviour
- **能力接口组合** — 通过 `ICanGetModel`、`ICanExecuteCommand` 等能力标记接口组合出 `IModel` / `IService` / `IView` / `IController` / `IPresenter`，按需暴露能力
- **命令模式** — `ICommand` 负责写操作，同步执行
- **查询模式** — `IQuery<TResult>` 负责读操作，返回结果，无副作用
- **ObservableValue 响应式属性** — 通常档 Model 直接暴露可写 `ObservableValue<T>`（快捷档表现层直写，标准档 Command 内部直写）；严格档收窄为 `IReadOnlyObservableValue<out T>` 只读接口 + 写方法
- **运行时错误日志** — `GetModel<T>()` / `GetService<T>()` 在目标未注册时抛出含调用者类型和目标类型信息的异常，替代前置依赖校验，兼容运行时替换 Model 的调试模式
- **AbstractSubmodule 统一子模块生命周期** — Model 和 Service 的公共生命周期逻辑提取到 `AbstractSubmodule` 基类，消除代码重复
- **GenericLocator 泛型定位器** — 按类型注册/查询的通用定位器，替代旧版 Container，支持全局单例
- **Domain Reload 安全** — 静态变量通过 `[RuntimeInitializeOnLoadMethod]` 显式重置，反复进出 Play Mode 无残留
- **纯 C# 核心 + MonoBehaviour 适配** — 框架核心为纯 C# 对象，Engine 层不依赖任何 Component 层类型，`AesirView<T>` / `MonoView<T>` / `AesirViewController<T>` 作为 MonoBehaviour 适配层
- **MVC + MVP 双模式** — `IController`（MVC，推荐）适合快速开发，`IPresenter`（MVP，可选）提供更严格的 Model-View 隔离

### 与 QFramework 的差异

| 维度 | QFramework | AesirArchitecture |
|------|-----------|-------------------|
| 生命周期 | MonoBehaviour 事件回调 | PlayerLoop 原生注入（BeforeUpdate / AfterUpdate） |
| 架构根 | 泛型单例 `Architecture<T>` | 泛型静态单例 `AbstractContext<T>` + `GenericLocator` 全局定位 |
| 可观察属性 | `BindableProperty<T>` | `ObservableValue<T>`（通常档可写 / 严格档 `IReadOnlyObservableValue<out T>` 只读）|
| 日志 | `Debug.Log` | `AesirArchitectureDebug` 条件编译统一日志 |
| 静态状态 | 无 Domain Reset 保障 | `[RuntimeInitializeOnLoadMethod]` 显式重置 |
| 表现层 | 无明确抽象 | `IView` 表现层接口 + `IController` / `IPresenter` 双模式 |

## 安装

### 通过 UPM（Git URL）

在 Unity Package Manager 中通过 Git URL 安装：

```
https://github.com/yuumixcode/Unity-Aesir-Packages.git?path=Assets/Runestone/AesirArchitecture
```

UPM 会自动通过 `package.json` 的 `name` 字段识别本包（`cn.runestone.aesir.architecture`）。

### 手动安装

将本包目录复制到项目的 `Packages/` 目录下即可。

## 快速开始

### 1. 定义 Context

```csharp
using Runestone.AesirArchitecture;

public class CounterContext : AbstractContext<CounterContext>
{
    protected override void Configure()
    {
        RegisterModel<ICounterModel>(new CounterModel());
    }
}
```

### 2. 定义 Model

```csharp
public interface ICounterModel : IModel
{
    ObservableValue<int> Count { get; }
    void Increase();
    void Decrease();
    void Reset();
}

public sealed class CounterModel : AbstractModel, ICounterModel
{
    // 通常档：直接暴露可写 ObservableValue（标准档由 Command 内部直写）
    public ObservableValue<int> Count { get; set; } = new ObservableValue<int>(0);

    public void Increase() => Count.Value++;
    public void Decrease() => Count.Value--;
    public void Reset() => Count.Value = 0;

    protected override void OnInitialize() { }
}
```

### 3. 定义 View（MVC 标准档）

```csharp
public class UICounterMvcPanel : MonoView<CounterContext>
{
    [SerializeField] Text countText;
    [SerializeField] Button increaseButton;

    ICounterController _ctrl;

    void Awake()
    {
        this.GetModel<ICounterModel>().Count
            .AddListenerAndInvoke(UpdateCountText)
            .RemoveListenerWhenGameObjectOnDestroyed(gameObject);
        _ctrl = new CounterController();
    }

    void OnEnable() => increaseButton.onClick.AddListener(_ctrl.Increase);
    void OnDisable() => increaseButton.onClick.RemoveListener(_ctrl.Increase);

    public void UpdateCountText(int count) => countText.text = count.ToString();
}
```

> **三档渐进路径**：
> - **第一课（快捷档，~5 文件）**：Context + Model + `MonoViewController<T>` 面板（View 兼 Controller 直写直读），见 `Counter-Mvc-Quick` 示例；
> - **第二课（标准档，~8 文件）**：拆出 `MonoView` + 独立 Controller，写入改走 Command，见 `Counter-MVC` 示例；
> - **第三课（严格档，~7 文件）**：Model 只读接口 + 写方法，读取改走 Query，见 `Counter-Mvc-Strict` 示例。
> MVP 三档（`Counter-Mvp-Simple` / `Counter-MVP` / `Counter-Mvp-Strict`）同构对照。

### 4. 使用 Command

```csharp
// 定义命令
public class AddScoreCommand : AbstractCommand
{
    protected override void OnExecute()
    {
        var model = this.GetModel<IScoreModel>();
        model.AddScore(10);
    }
}

// 执行命令
this.ExecuteCommand<AddScoreCommand>();
```

## 架构总览

```
┌─────────────────────────────────────────────────┐
│               AbstractContext<T>                 │
│     (泛型静态单例 + Domain Reset)                │
│                                                  │
│  ┌──────────┐  ┌──────────┐                    │
│  │  Models  │  │ Services │                    │
│  │          │  │          │                    │
│  └──────────┘  └──────────┘                    │
│  ┌──────────────────────────────────────────────┐│
│  │       GenericLocator<T> (类型定位器)         ││
│  └──────────────────────────────────────────────┘│
└──────────────────┬──────────────────────────────┘
                   │ 能力接口组合
     ┌─────────────┼─────────────┐
     ▼             ▼             ▼
┌─────────┐ ┌───────────┐ ┌────────────┐
│  IView  │ │IController│ │ IPresenter │
│         │ │  (MVC)    │ │   (MVP)    │
└─────────┘ └───────────┘ └────────────┘
     │             │             │
     ▼             ▼             ▼
┌──────────────────────────────────────┐
│  MonoView<T> / MonoViewController<T>  │
│  AesirView<T> / AesirViewController<T>│
│  （Odin 增强版，无 Odin 时用 Mono 系列）│
│        (MonoBehaviour 适配层)          │
└──────────────────────────────────────┘
                   │
                   ▼
┌──────────────────────────────────────┐
│     AesirArchitecturePlayerLoop       │
│  (PlayerLoop 原生注入: Before/After)   │
└──────────────────────────────────────┘
```

### 能力矩阵

| 模块 | GetModel | GetService | ExecuteCommand | Initialize | Dispose |
|------|:--------:|:---------:|:--------------:|:----------:|:-------:|
| **IModel** | ✓ | | | ✓ | ✓ |
| **IService** | ✓ | ✓ | | ✓ | ✓ |
| **IView** | ✓ | ✓ | | | |
| **IController** | ✓ | ✓ | ✓ | | |
| **IPresenter** | ✓ | ✓ | ✓ | | ✓ |

## 项目结构

```
cn.runestone.aesir.architecture/
├── package.json
├── README.md                       # 本文件（中文）
├── Documentation~/
│   ├── README_EN.md               # English version
│   ├── 事件机制决策表.md            # 四种通知机制的场景决策表
│   └── 常见陷阱清单.md              # 10 条初学者高频陷阱与修法
├── CHANGELOG.md
├── LICENSE.md
├── Third Party Notices.md
├── Runtime/
│   ├── Runestone.AesirArchitecture.asmdef
│   ├── Core/                      # 核心：Context 上下文 + MVC/MVP 架构
│   │   ├── Component/             # MonoBehaviour 适配层
│   │   │   ├── View/              # AesirView, MonoView
│   │   │   └── ViewController/    # AesirViewController, MonoViewController
│   │   └── Engine/                # 纯 C# 核心（不依赖 MonoBehaviour）
│   │       ├── Capabilities/      # ICan* 能力接口 + CapabilityExtensions 扩展方法
│   │       ├── Context/           # IContext, AbstractContext<T>
│   │       └── Modules/           # IModel, IService, IView, IController, IPresenter + Abstract 基类
│   │           ├── Interfaces/
│   │           └── Abstracts/
│   ├── Modules/                   # 辅助模块
│   │   ├── Event/                 # MiniEvent 零分配事件 + 自动移除监听触发器
│   │   ├── CustomLifecycle/       # MonoLifecycleProxy 生命周期代理
│   │   ├── Locator/               # GenericLocator 泛型定位器
│   │   ├── Observable/            # ObservableValue 响应式属性
│   │   └── Utilities/             # PlayerLoopUtility + AesirArchitecturePlayerLoop
│   ├── Common/                    # 框架基础设施
│   │   ├── AesirArchitecture.cs   # 框架 MonoBehaviour 单例入口
│   │   ├── AesirMonoBehaviour.cs  # Odin 自动适配基类
│   │   ├── AesirScriptableObject.cs
│   │   ├── AesirArchitectureDebug.cs
│   │   ├── AssemblyInfo.cs
│   │   └── ResetStaticsAssistant.cs
│   └── OdinInspector/            # 独立程序集（依赖 Odin Inspector）
│       ├── Runestone.AesirArchitecture.OdinInspector.asmdef
│       └── DescriptionSO.cs
├── Editor/
│   ├── Runestone.AesirArchitecture.Editor.asmdef
│   ├── Common/
│   │   └── EnsureAesirArchitectureDefine.cs  # 编译符号管理
│   ├── Utilities/
│   │   └── ScriptingSymbolUtility.cs
│   └── OdinInspector/            # Odin Inspector 集成（可选）
│       ├── Runestone.AesirArchitecture.Editor.OdinInspector.asmdef
│       └── AttributeProcessors/
│           ├── AesirArchitectureAttributeProcessor.cs
│           ├── MonoLifecycleProxyAttributeProcessor.cs
│           ├── RemoveListenerOnSceneUnloadedTriggerAttributeProcessor.cs
│           └── ObservableValueAttributeProcessor.cs
├── Tests/
│   ├── Runtime/
│   │   └── Runestone.AesirArchitecture.Tests.asmdef
│   └── Editor/
│       └── Runestone.AesirArchitecture.Tests.Editor.asmdef
├── Samples~/
│   ├── Counter-Mvc-Quick/         # MVC-1 快捷档（MonoViewController 直写直读，第一课）
│   ├── Counter-MVC/               # MVC-2 标准档（Command 写入 + 独立 Controller，第二课）
│   ├── Counter-Mvc-Strict/        # MVC-3 严格档（只读 Model + Command 写 + Query 读，进阶）
│   ├── Counter-Mvp-Simple/        # MVP-1 简单档（Presenter 直写 Model）
│   ├── Counter-MVP/               # MVP-2 标准档（Presenter 走 Command）
│   ├── Counter-Mvp-Strict/        # MVP-3 严格档（Command 写 + Query 读）
│   ├── ObservableValue/           # ObservableValue Inspector 演示（Odin Inspector）
│   └── MiniEvent/                 # MiniEvent 使用案例
└── Third Party Notices.md          # 第三方许可声明
```

## 设计边界

> 框架保持极简：低概率问题、或因不推荐编写方式造成的问题，用本节约定在项目前期直接杜绝，而非依赖运行时防御性代码兜底。

### 不做的事

- **事件总线 / EventChannel** — 跨模块通信使用互相 GetModel + ObservableValue 订阅，或直接引用 MiniEvent
- **Context 多实例** — `AbstractContext<T>` 为 CRTP 泛型单例，每个具体上下文类型全局仅一份；多存档、多房间等场景请在业务层建模
- **Command/Query 池化、async、队列、Undo/Redo** — `ExecuteCommand` / `ExecuteQuery` 保持同步、无缓存；高频路径有分配敏感需求时在业务层包装
- **View 生命周期脚手架** — View 层保持极薄，面板生命周期由 Aesir Modules 的 UIModule 负责
- **线程安全** — 所有框架类型仅保证主线程使用；Service 中 `Task.Run` 等异步回调请先调度回主线程再访问框架

### 编写约定（违反时 fail-fast 报错，框架不做兜底）

| 约定 | 违反后果 |
|------|---------|
| 监听回调不应抛异常 | 异常直接向上传播并中断同事件后续回调（原生 C# 事件语义），由 Unity 记日志 |
| `Configure()` 及各模块初始化中禁止访问 `Instance` | 会因单例尚未发布而递归创建第二个上下文实例 |
| `Register` 与 `Get` 必须使用相同类型参数 | 按键精确匹配，用实现类查询接口键注册的实例返回 null / 抛未注册异常（含近失识别提示） |
| 运行时替换 Model/Service 仅用于测试调试 | 旧实例被 Dispose，其上的订阅不会迁移，已订阅的 View 需自行重新订阅 |
| 第三方 SDK 修改 PlayerLoop 后手动调用一次 `AesirArchitecturePlayerLoop.EnsureInjected()` | BeforeUpdate / AfterUpdate 钩子静默失效（`Register` 注册回调时会自动检测补插） |
| **写入纪律档位** | 快捷/简单档直写 Model 合法；标准档起表现层写入必须经 Command；严格档 Model 只读 + 写方法；Service 可直写。推荐项目从标准档起步 |

## 设计原则

1. **Unity 原生优先** — 优先使用 Unity 引擎能力（PlayerLoop、ScriptableObject、Editor API），而非自建平行体系
2. **Domain Reload 兼容（铁律）** — 静态变量必须显式重置，反复进出 Play Mode 无残留
3. **低 MonoBehaviour 依赖** — 核心框架由纯 C# 对象组成，MonoBehaviour 仅作适配层
4. **渐进式** — 小项目轻量使用，大项目逐步扩展，不强制全量引入
5. **SO 与纯代码双通道**（规划中） — 每个 SO 能力都有纯 C# 替代方案
6. **团结引擎优先** — 以团结引擎为一等公民
7. **Inspector 精简原则（AI 优先）** — Odin 优化的目标是调试可见性；核心配置首选代码与资产文件（AI 可读写、可版本管理），Inspector 仅作调试加成；非必要、非重要内容不进面板
8. **Odin 三条铁律** — 核心架构流程闭环不依赖 Odin（Context 注册 → Model/Service 初始化 → Command/Query 执行 → ObservableValue 通知全链路无 Odin 可运行）；架构调试器等体验优化品可用 Odin；样式优化与代码逻辑分离（ObservableValue 为范例：逻辑无 Odin 照常运行，样式由 Odin AttributeProcessor 增强）

## 路线图

- [x] 核心 MVC / MVP 分层（MVC 优先）
- [x] PlayerLoop 原生生命周期注入
- [x] 命令模式（同步）
- [x] 查询模式（CQRS 读操作）
- [x] ObservableValue 响应式属性
- [x] GenericLocator 泛型定位器
- [x] AbstractSubmodule 统一子模块生命周期
- [x] 运行时错误日志（替代前置依赖校验）
- [x] Engine 层脱离 Component 层（纯 C#）
- [x] Domain Reload 安全
- [ ] ScriptableObject 可视化配置层
- [ ] SO EventChannel 事件通道
- [ ] Editor 工具链（SO Inspector / MVP 脚手架 / 模块可视化）
- [ ] 运行时集合（RuntimeSet）

## 许可证

[MIT](./LICENSE.md)
