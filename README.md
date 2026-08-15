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
> - **[Aesir Modules](https://github.com/yuumixcode/Unity-Aesir-Packages)**（依赖 Architecture + Inspector）

## 概述

AesirArchitecture（RAA）是一个以 **Unity 原生优先** 为核心理念的架构框架。它不构建与引擎平行的自建体系，而是深度绑定 Unity 的 PlayerLoop、ScriptableObject、Editor API 等原生能力，在保持轻量的同时为中小型到中大型项目提供清晰的 MVC / MVP 分层。框架以 **MVC 为主要模式**，`IController` 是推荐的快速开发入口；`IPresenter`（MVP）作为可选的严格分层模式。

### 核心特性

- **MVC 优先架构** — `IController` + `ICommand` 命令模式 + `IQuery<TResult>` 查询模式（CQRS），Controller 作为 MVC 的核心入口直接修改 Model；`IPresenter`（MVP）作为可选的严格 Model-View 隔离模式
- **PlayerLoop 原生生命周期** — 通过 `AesirArchitecturePlayerLoop` 将自定义子系统注入 Unity PlayerLoop，提供 `BeforeUpdate` / `AfterUpdate` 帧回调，无需 MonoBehaviour
- **能力接口组合** — 通过 `ICanGetModel`、`ICanExecuteCommand` 等能力标记接口组合出 `IModel` / `IService` / `IView` / `IController` / `IPresenter`，按需暴露能力
- **命令模式** — `ICommand` 负责写操作，同步执行
- **查询模式** — `IQuery<TResult>` 负责读操作，返回结果，无副作用
- **ObservableValue 响应式属性** — Model 持有可写实例，View 通过 `IReadOnlyObservableValue<out T>` 协变只读访问，保障层级安全
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
| 可观察属性 | `BindableProperty<T>` | `ObservableValue<T>` + `IReadOnlyObservableValue<out T>` 协变只读 |
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
    IReadOnlyObservableValue<int> Count { get; }
    void Increase();
    void Decrease();
    void Reset();
}

public sealed class CounterModel : AbstractModel, ICounterModel
{
    readonly ObservableValue<int> _count = new ObservableValue<int>(0);

    public IReadOnlyObservableValue<int> Count => _count;

    public void Increase() => _count.Value++;
    public void Decrease() => _count.Value--;
    public void Reset() => _count.Value = 0;

    protected override void OnInitialize() { }
}
```

### 3. 定义 View（MVC 模式）

```csharp
public class UICounterMvcPanel : AesirView<CounterContext>
{
    [SerializeField] Text countText;
    [SerializeField] Button increaseButton;

    ICounterModel _model;
    ICounterController _ctrl;

    void Awake()
    {
        _model = this.GetModel<ICounterModel>();
        _model.Count.AddListener(UpdateCountText)
                   .RemoveListenerWhenGameObjectOnDestroyed(gameObject);
        _ctrl = new CounterController(_model);
    }

    void OnEnable() => increaseButton.onClick.AddListener(_ctrl.Increase);
    void OnDisable() => increaseButton.onClick.RemoveListener(_ctrl.Increase);

    public void UpdateCountText(int count) => countText.text = count.ToString();
}
```

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
│  AesirView<T> / MonoView<T>          │
│  AesirViewController<T>              │
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
├── Documentation~/README_EN.md     # English version
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
│   ├── Counter-MVC/               # MVC 模式计数器 Demo（推荐）
│   ├── UI Counter-MVP/            # MVP 模式计数器 Demo
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
| `Configure()` 及各模块初始化中禁止访问 `Interface` | 会因单例尚未发布而递归创建第二个上下文实例 |
| `Register` 与 `Get` 必须使用相同类型参数 | 按键精确匹配，用实现类查询接口键注册的实例返回 null / 抛未注册异常 |
| 运行时替换 Model/Service 仅用于测试调试 | 旧实例被 Dispose，其上的订阅不会迁移，已订阅的 View 需自行重新订阅 |
| 第三方 SDK 修改 PlayerLoop 后手动调用一次 `AesirArchitecturePlayerLoop.EnsureInjected()` | BeforeUpdate / AfterUpdate 钩子静默失效（`Register` 注册回调时会自动检测补插） |

## 设计原则

1. **Unity 原生优先** — 优先使用 Unity 引擎能力（PlayerLoop、ScriptableObject、Editor API），而非自建平行体系
2. **Domain Reload 兼容（铁律）** — 静态变量必须显式重置，反复进出 Play Mode 无残留
3. **低 MonoBehaviour 依赖** — 核心框架由纯 C# 对象组成，MonoBehaviour 仅作适配层
4. **渐进式** — 小项目轻量使用，大项目逐步扩展，不强制全量引入
5. **SO 与纯代码双通道**（规划中） — 每个 SO 能力都有纯 C# 替代方案
6. **团结引擎优先** — 以团结引擎为一等公民

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
