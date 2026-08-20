# Aesir Architecture

> A progressive MVP/MVC architecture framework for **Tuanjie Engine** / **Unity**, treating Unity native features as first-class citizens.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](../LICENSE.md)
[![Version](https://img.shields.io/badge/version-0.10.0-blue.svg)](../CHANGELOG.md)
[![Unity](https://img.shields.io/badge/Unity-2022.3%2B-black.svg)](https://unity.com/)
[![Install via Git URL](https://img.shields.io/badge/UPM-Git%20URL-blueviolet.svg)](#installation)
[![中文](https://img.shields.io/badge/README-中文-red.svg)](../README.md)

> 📦 **This package is part of the [Unity-Aesir-Packages](https://github.com/yuumixcode/Unity-Aesir-Packages) monorepo.** This package does **NOT depend on** any other Aesir sub-package (standalone installable).
>
> Related:
> - **[Aesir Inspector](https://github.com/yuumixcode/Unity-Aesir-Packages)** (standalone)
> - **[Aesir Modules](https://github.com/yuumixcode/Unity-Aesir-Packages)** (depends on Architecture)

> **Odin Inspector is an optional enhancement** (Inspector presentation and debugging experience), not a runtime prerequisite — the core architecture loop (Context registration → initialization → Command/Query → ObservableValue notification) runs fully without Odin.

## Overview

AesirArchitecture (RAA) is an architecture framework built on a **Unity-native-first** philosophy. It does not build a parallel system to the engine; instead, it deeply integrates with Unity native capabilities such as PlayerLoop, ScriptableObject, and the Editor API — keeping things lightweight while providing a clear MVC / MVP layering for small to medium-large projects. The framework is **MVC-first**: `IController` is the recommended entry point for rapid development; `IPresenter` (MVP) is an optional pattern for stricter Model-View separation.

### Core Features

- **MVC-first architecture** — `IController` + `ICommand` command pattern + `IQuery<TResult>` query pattern (CQRS); Controller is the primary MVC entry point that directly modifies Model. `IPresenter` (MVP) is an optional pattern for stricter Model-View separation
- **Native PlayerLoop lifecycle** — Inject custom subsystems into Unity's PlayerLoop via `AesirArchitecturePlayerLoop`, providing `BeforeUpdate` / `AfterUpdate` frame callbacks without MonoBehaviour
- **Capability interface composition** — Compose `IModel` / `IService` / `IView` / `IController` / `IPresenter` from fine-grained capability marker interfaces (`ICanGetModel`, `ICanExecuteCommand`, etc.) — expose only what you need
- **Command pattern** — `ICommand` handles write operations, executed synchronously
- **Query pattern** — `IQuery<TResult>` handles read operations, returns data without side effects
- **`ObservableValue<T>` reactive property** — Normal tier: Model exposes writable `ObservableValue<T>` directly (quick tier: presentation writes directly; standard tier: Command writes internally); strict tier: narrowed to covariant `IReadOnlyObservableValue<out T>` + write methods
- **Runtime error logging** — `GetModel<T>()` / `GetService<T>()` throws exceptions with caller-type and target-type info when unregistered, replacing pre-flight validation; supports runtime model replacement
- **`AbstractSubmodule` unified submodule lifecycle** — Shared lifecycle logic for Model and Service is extracted into `AbstractSubmodule` base class, eliminating code duplication
- **`GenericLocator<T>` generic locator** — Type-keyed registration/query locator replacing the legacy Container, with global singleton support
- **Domain Reload safety** — Static variables explicitly reset via `[RuntimeInitializeOnLoadMethod]`; no residue across Play Mode entry/exit
- **Pure C# core + MonoBehaviour adapter** — Framework core is pure C#; the Engine layer does NOT depend on any Component layer type. `AesirView<T>` / `MonoView<T>` / `AesirViewController<T>` serve as MonoBehaviour adapters
- **MVC + MVP dual modes** — `IController` (MVC, recommended) for fast development, `IPresenter` (MVP, optional) for stricter Model-View separation

### Comparison with QFramework

| Dimension | QFramework | AesirArchitecture |
|------|-----------|-------------------|
| Lifecycle | MonoBehaviour event callbacks | Native PlayerLoop injection (BeforeUpdate / AfterUpdate) |
| Architecture root | Generic singleton `Architecture<T>` | Generic static singleton `AbstractContext<T>` + `GenericLocator` |
| Observable property | `BindableProperty<T>` | `ObservableValue<T>` (normal tier writable / strict tier `IReadOnlyObservableValue<out T>` read-only) |
| Logging | `Debug.Log` | `AesirArchitectureDebug` (conditional compilation, unified) |
| Static state | No Domain Reset guarantee | `[RuntimeInitializeOnLoadMethod]` explicit reset |
| Presentation layer | No clear abstraction | `IView` interface + `IController` / `IPresenter` dual modes |

## Installation

### Via UPM (Git URL)

Install via UPM by adding a Git URL in Unity Package Manager:

```
https://github.com/yuumixcode/Unity-Aesir-Packages.git
```

UPM identifies this package via the `name` field in `package.json` (`cn.runestone.aesir.architecture`).

### Manual Installation

Copy this package directory into your project's `Packages/` folder.

## Quick Start

### 1. Define a Context

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

### 2. Define a Model

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
    // Normal tier: expose writable ObservableValue directly (standard tier: written by Command internally)
    public ObservableValue<int> Count { get; set; } = new ObservableValue<int>(0);

    public void Increase() => Count.Value++;
    public void Decrease() => Count.Value--;
    public void Reset() => Count.Value = 0;

    protected override void OnInitialize() { }
}
```

### 3. Define a View (MVC standard tier)

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

> **Three-tier progressive path**:
> - **Lesson 1 (Quick tier, ~5 files)**: Context + Model + `MonoViewController<T>` panel (View doubles as Controller, direct read/write), see `Counter-Mvc-Quick` sample;
> - **Lesson 2 (Standard tier, ~8 files)**: Extract `MonoView` + standalone Controller, writes go through Command;
> - **Lesson 3 (Strict tier, ~7 files)**: Read-only Model interface + write methods, reads go through Query, see `Counter-Mvc-Strict` sample.
> MVP tiers (`Counter-Mvp-Simple` / `Counter-Mvp-Strict`) mirror the same structure.

### 4. Use a Command

```csharp
// Define a command
public class AddScoreCommand : AbstractCommand
{
    protected override void OnExecute()
    {
        var model = this.GetModel<IScoreModel>();
        model.AddScore(10);
    }
}

// Execute a command
this.ExecuteCommand<AddScoreCommand>();
```

## Architecture Overview

```
┌─────────────────────────────────────────────────┐
│               AbstractContext<T>                 │
│     (Generic static singleton + Domain Reset)    │
│                                                  │
│  ┌──────────┐  ┌──────────┐                    │
│  │  Models  │  │ Services │                    │
│  │          │  │          │                    │
│  └──────────┘  └──────────┘                    │
│  ┌──────────────────────────────────────────────┐│
│  │       GenericLocator<T> (type locator)       ││
│  └──────────────────────────────────────────────┘│
└──────────────────┬──────────────────────────────┘
                   │ Capability interface composition
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
│  (Odin-enhanced; use Mono* without Odin)│
│        (MonoBehaviour adapter layer)   │
└──────────────────────────────────────┘
                   │
                   ▼
┌──────────────────────────────────────┐
│     AesirArchitecturePlayerLoop       │
│  (Native PlayerLoop injection)        │
└──────────────────────────────────────┘
```

### Capability Matrix

| Module | GetModel | GetService | ExecuteCommand | Initialize | Dispose |
|------|:--------:|:---------:|:--------------:|:----------:|:-------:|
| **IModel** | ✓ | | | ✓ | ✓ |
| **IService** | ✓ | ✓ | | ✓ | ✓ |
| **IView** | ✓ | ✓ | | | |
| **IController** | ✓ | ✓ | ✓ | | |
| **IPresenter** | ✓ | ✓ | ✓ | | ✓ |

## Project Structure

```
cn.runestone.aesir.architecture/
├── package.json
├── README.md                       # Chinese
├── Documentation~/
│   ├── README_EN.md               # This file
│   ├── 事件机制决策表.md            # Event mechanism decision table (Chinese)
│   └── 常见陷阱清单.md              # Common pitfalls checklist (Chinese)
├── CHANGELOG.md
├── LICENSE.md
├── Third Party Notices.md
├── Runtime/
│   ├── Runestone.AesirArchitecture.asmdef
│   ├── Core/                      # Core: Context + MVC/MVP architecture
│   │   ├── Component/             # MonoBehaviour adapter layer
│   │   │   ├── View/              # AesirView, MonoView
│   │   │   └── ViewController/    # AesirViewController, MonoViewController
│   │   └── Engine/                # Pure C# core (no MonoBehaviour dependency)
│   │       ├── Capabilities/      # ICan* interfaces + CapabilityExtensions
│   │       ├── Context/           # IContext, AbstractContext<T>
│   │       └── Modules/          # IModel, IService, IView, IController, IPresenter + Abstract bases
│   │           ├── Interfaces/
│   │           └── Abstracts/
│   ├── Modules/                   # Helper modules
│   │   ├── Event/                 # MiniEvent zero-alloc events + auto-remove triggers
│   │   ├── CustomLifecycle/       # MonoLifecycleProxy lifecycle proxy
│   │   ├── Locator/               # GenericLocator type-keyed locator
│   │   ├── Observable/            # ObservableValue reactive property
│   │   └── Utilities/             # PlayerLoopUtility + AesirArchitecturePlayerLoop
│   ├── Common/                    # Framework infrastructure
│   │   ├── AesirArchitecture.cs   # Framework MonoBehaviour singleton entry
│   │   ├── AesirMonoBehaviour.cs  # Odin-adapted base class
│   │   ├── AesirScriptableObject.cs
│   │   ├── AesirArchitectureDebug.cs
│   │   ├── AssemblyInfo.cs
│   │   └── ResetStaticsAssistant.cs
│   └── OdinInspector/            # Independent assembly (depends on Odin Inspector)
│       ├── Runestone.AesirArchitecture.OdinInspector.asmdef
│       └── DescriptionSO.cs
├── Editor/
│   ├── Runestone.AesirArchitecture.Editor.asmdef
│   ├── Common/
│   │   └── EnsureAesirArchitectureDefine.cs  # Compile symbol management
│   ├── Utilities/
│   │   └── ScriptingSymbolUtility.cs
│   └── OdinInspector/            # Odin Inspector integration (optional)
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
│   ├── Counter-Mvc-Quick/         # MVC-1 quick tier (MonoViewController direct read/write, lesson 1)
│   ├── Counter-Mvc-Strict/        # MVC-3 strict tier (read-only Model + Command writes + Query reads)
│   ├── Counter-Mvp-Simple/        # MVP-1 simple tier (Presenter writes Model directly)
│   ├── Counter-Mvp-Strict/        # MVP-3 strict tier (Command writes + Query reads)
│   ├── ObservableValue/           # ObservableValue Inspector demo (Odin Inspector)
│   └── MiniEvent/                 # MiniEvent usage examples
└── Third Party Notices.md          # Third-party license notices
```

## Design Boundaries

> The framework stays minimal: low-probability issues, or issues caused by discouraged coding patterns, are prevented up front by the conventions in this section — not by defensive runtime code.

### Not Provided

- **Event bus / EventChannel** — Cross-module communication uses GetModel + ObservableValue subscriptions, or direct MiniEvent references
- **Multiple Context instances** — `AbstractContext<T>` is a CRTP generic singleton, one instance per concrete context type; model multi-save / multi-room scenarios at the business layer
- **Command/Query pooling, async, queues, Undo/Redo** — `ExecuteCommand` / `ExecuteQuery` stay synchronous and uncached; wrap at the business layer for allocation-sensitive hot paths
- **View lifecycle scaffolding** — The View layer stays thin; panel lifecycle is handled by UIModule in Aesir Modules
- **Thread safety** — All framework types are main-thread only; dispatch back to the main thread before touching the framework from async code (e.g. `Task.Run`)

### Coding Conventions (violations fail fast; the framework does not compensate)

| Convention | Consequence of violation |
|------|---------|
| Listener callbacks must not throw | Exceptions propagate and interrupt subsequent callbacks of the same event (native C# event semantics); Unity logs the error |
| Never access `Instance` inside `Configure()` or module initialization | A second context instance is created recursively (the singleton is not published yet) |
| `Register` and `Get` must use the same type argument | Exact-key matching: querying an interface-keyed registration by implementation type returns null / throws not-registered (with near-miss hint) |
| Runtime Model/Service replacement is for testing/debugging only | The old instance is disposed, its subscriptions are not migrated; subscribed views must re-subscribe |
| Call `AesirArchitecturePlayerLoop.EnsureInjected()` once after a third-party SDK rewrites the PlayerLoop | BeforeUpdate / AfterUpdate hooks silently stop firing (`Register` self-heals on callback registration) |
| **Write-discipline tiers** | Quick/Simple tier: direct Model writes are legal; Standard tier onward: presentation-layer writes must go through Command; Strict tier: read-only Model + write methods; Services may write directly. Recommended: start from the Standard tier |

## Design Principles

1. **Unity-native first** — Use Unity engine capabilities (PlayerLoop, ScriptableObject, Editor API) rather than building a parallel system
2. **Domain Reload compatible (iron rule)** — Static variables must be explicitly reset; no residue across Play Mode entry/exit
3. **Low MonoBehaviour dependency** — Core framework is pure C#; MonoBehaviour is only the adapter layer
4. **Progressive** — Use lightly in small projects, scale up gradually for large projects; no forced full adoption
5. **SO and pure code dual channels (planned)** — Each SO capability has a pure C# alternative
6. **Tuanjie Engine first** — Tuanjie is a first-class citizen
7. **Inspector austerity (AI-first)** — Odin enhances debuggability; core configuration lives in code and asset files (AI-readable, versionable); the Inspector is a debugging aid, not a configuration surface — non-essential content stays off the panel
8. **Odin three iron rules** — The core architecture loop never depends on Odin (Context registration → initialization → Command/Query → ObservableValue notification runs fully without Odin); experience-optimizing tools (e.g. the Context Debugger) may use Odin; presentation and logic stay separate (ObservableValue is the canonical example: logic runs without Odin, Inspector presentation is enhanced by an Odin AttributeProcessor)

## Roadmap

- [x] Core MVC / MVP layering (MVC-first)
- [x] Native PlayerLoop lifecycle injection
- [x] Command pattern (sync)
- [x] Query pattern (CQRS read)
- [x] `ObservableValue` reactive property
- [x] `GenericLocator` generic locator
- [x] `AbstractSubmodule` unified submodule lifecycle
- [x] Runtime error logging (replacing pre-flight validation)
- [x] Engine layer decoupled from Component layer (pure C#)
- [x] Domain Reload safety
- [ ] ScriptableObject visualization config layer
- [ ] SO EventChannel
- [ ] Editor toolchain (SO Inspector / MVP scaffold / module visualization)
- [ ] RuntimeSet

## License

[MIT](./LICENSE.md)
