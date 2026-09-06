# Aesir Architecture

> A progressive MVC architecture framework for **Tuanjie Engine** / **Unity**, treating Unity native features as first-class citizens.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](../LICENSE.md)
[![Version](https://img.shields.io/badge/version-0.17.0-blue.svg)](../CHANGELOG.md)
[![Unity](https://img.shields.io/badge/Unity-2022.3%2B-black.svg)](https://unity.com/)
[![Install via Git URL](https://img.shields.io/badge/UPM-Git%20URL-blueviolet.svg)](#installation)
[![中文](https://img.shields.io/badge/README-中文-red.svg)](../README.md)

> 📦 **This package is part of the [AesirFramework](https://github.com/yuumixcode/AesirFramework) monorepo.** This package does **NOT depend on** any other Aesir sub-package (standalone installable).
>
> Related:
> - **[Aesir Modules](https://github.com/yuumixcode/AesirFramework)** (depends on Architecture)

> **Odin Inspector is an optional enhancement** (Inspector presentation and debugging experience), not a runtime prerequisite — the core architecture loop (Context registration → initialization → Command/Query → ObservableValue notification) runs fully without Odin.

## Overview

AesirArchitecture (RAA) is an architecture framework built on a **Unity-native-first** philosophy. It does not build a parallel system to the engine; instead, it deeply integrates with Unity native capabilities such as PlayerLoop, ScriptableObject, and the Editor API — keeping things lightweight while providing a clear MVC / MVP layering for small to medium-large projects. The framework is **MVC-first**: `IController` is the recommended entry point for rapid development; `IPresenter` (MVP) is an optional pattern for stricter Model-View separation.

### Core Features

- **MVC-first architecture** — `IController` + `ICommand` command pattern + `IQuery<TResult>` query pattern (CQRS); Controller is the primary MVC entry point that directly modifies Model. `IPresenter` (MVP) is an optional pattern for stricter Model-View separation
- **Native PlayerLoop lifecycle** — Inject custom subsystems into Unity's PlayerLoop via `AesirArchitecturePlayerLoop`, providing `BeforeUpdate` / `AfterUpdate` frame callbacks without MonoBehaviour
- **Capability interface composition** — Compose `IModel` / `IService` / `IView` / `IController` / `IPresenter` from fine-grained capability marker interfaces (`ICanGetModel`, `ICanExecuteCommand`, etc.) — expose only what you need
- **Command pattern** — `ICommand` handles write operations, executed synchronously
- **Query pattern** — `IQuery<TResult>` handles read operations, returns data without side effects
- **`ObservableValue<T>` reactive property** — Quick tier: Model exposes writable `ObservableValue<T>` directly (presentation writes directly); Standard tier onward: narrowed to covariant `IReadOnlyObservableValue<out T>` + write methods; Strict tier: interface registration + Command writes on top
- **Observable collections** — `ObservableList<T>` / `ObservableDictionary<TKey, TValue>` / `ObservableHashSet<T>` provide the most common change notifications (Added / Removed / Replaced / Updated / Cleared), following the same read-write separation and MiniEvent event pattern as ObservableValue
- **Runtime error logging** — `GetModel<T>()` / `GetService<T>()` throws exceptions with caller-type and target-type info when unregistered, replacing pre-flight validation; supports runtime model replacement
- **`AbstractSubmodule` unified submodule lifecycle** — Shared lifecycle logic for Model and Service is extracted into `AbstractSubmodule` base class, eliminating code duplication
- **`GenericLocator<T>` generic locator** — Type-keyed registration/query locator replacing the legacy Container, preserving registration order
- **Domain Reload safety** — Static variables explicitly reset via `[RuntimeInitializeOnLoadMethod]`; no residue across Play Mode entry/exit
- **Pure C# core + MonoBehaviour adapter** — Framework core is pure C#; the Engine layer does NOT depend on any Component layer type. `AesirView<T>` / `MonoView<T>` / `AesirViewController<T>` serve as MonoBehaviour adapters
- **MVC + MVP dual modes** — `IController` (MVC, recommended) for fast development, `IPresenter` (MVP, optional) for stricter Model-View separation

## Installation

### Via UPM (Git URL)

Install via UPM with a Git URL pinned to the 0.16.2 version branch (the branch root is the package content):

```
https://github.com/yuumixcode/AesirFramework.git#AesirArchitecture-v0.17.0
```

Track the latest development version on `main`:

```
https://github.com/yuumixcode/AesirFramework.git?path=Assets/Runestone/AesirArchitecture
```

UPM identifies this package via the `name` field in `package.json` (`cn.runestone.aesir.architecture`).

### Manual Installation

Copy this package directory into your project's `Packages/` folder.

### unitypackage Import

Download `AesirArchitecture-v<version>.unitypackage` (or the combined `AesirFramework-v<version>.unitypackage`) from [GitHub Releases](https://github.com/yuumixcode/AesirFramework/releases) and import it. Packages installed this way live under `Assets/Runestone/` and can be checked and updated in one click via the Unity menu `Tools → Aesir → Check for Updates` — the **in-package updater**: version detection uses multi-source fallback for mainland connectivity (jsDelivr CDN → GitHub API → redirect probe); it backs up `Assets/Runestone` before updating, then removes stale entries by the exact diff of "previous install manifest − new manifest" without touching user-added files.

> Copies installed via Git URL (UPM) are outside the updater's scope — update them with the Package Manager directly.

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
    IReadOnlyObservableValue<int> Count { get; }
    void Increase();
    void Decrease();
    void Reset();
}

public sealed class CounterModel : AbstractModel, ICounterModel
{
    // Quick tier may expose a writable ObservableValue directly; standard tier onward: read-only interface + write methods
    [SerializeField] ObservableValue<int> count = new ObservableValue<int>(0);

    public IReadOnlyObservableValue<int> Count => count;
    public void Increase() => count.Value++;
    public void Decrease() => count.Value--;
    public void Reset() => count.Value = 0;

    protected override void OnInitialize() { }
}
```

### 3. Define a View (MVC strict tier)

```csharp
public class UICounterMvcPanel : MonoView<CounterContext>
{
    [SerializeField] Text countText;
    [SerializeField] Button increaseButton;

    ICounterModel _model;
    CounterController _ctrl;

    void Start()
    {
        _model = this.GetModel<ICounterModel>();
        _model.Count.AddListenerAndInvoke(UpdateCountText)
            .RemoveListenerWhenGameObjectOnDestroyed(gameObject);
        _ctrl = new CounterController();
    }

    void OnEnable() => increaseButton.onClick.AddListener(_ctrl.Increase);
    void OnDisable() => increaseButton.onClick.RemoveListener(_ctrl.Increase);

    public void UpdateCountText(int count) => countText.text = count.ToString();
}
```

> **Three-tier progressive path**:
> - **Lesson 1 (Quick tier, ~5 files)**: Context + Model (writable ObservableValue exposed directly) + `MonoViewController<T>` panel (View doubles as Controller, writes directly), see `Counter-Mvc-Quick` sample;
> - **Lesson 2 (Standard tier, ~6 files)**: Model narrowed to read-only interface exposure + write methods; View becomes `MonoView`, separated from the Controller instance while sharing the same Model (write methods called directly, no Command), see `Counter-Mvc-Standard` sample;
> - **Lesson 3 (Strict tier, ~10 files)**: Model registered by interface; Controller issues Commands via Context, processed reads go through Query, View holds the Model as the interface type with subscription refresh and holds the Controller via a narrow business interface, see `Counter-Mvc-Strict` sample.
> The three MVP tiers (`Counter-Mvp-Quick` / `Counter-Mvp-Standard` / `Counter-Mvp-Strict`) mirror the MVC structure — the Model exposure is identical at each tier; the only difference is the refresh path: MVC Views subscribe to the Model themselves, MVP Views are passive and refreshed by the Presenter.

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

## Samples

The package provides 9 importable samples (Package Manager → Aesir Architecture → Samples). The counter family follows a **three-tier progressive** layout, with MVC and MVP mirroring each other tier by tier — the Model exposure is identical at each tier; the only difference is the refresh path (MVC: Views subscribe to the Model; MVP: Views are passive, the Presenter pushes).

### MVC family (View subscribes itself)

| Sample | Tier | Model exposure | Write / read path |
|------|------|-------------|----------------|
| `Counter-Mvc-Quick` | Lesson 1 · Quick | Concrete-class registration, writable `ObservableValue` modified directly | View-as-Controller writes / reads directly |
| `Counter-Mvc-Standard` | Lesson 2 · Standard | Concrete-class registration, read-only exposure + write methods | Controller calls write methods / View read-only subscription |
| `Counter-Mvc-Strict` | Lesson 3 · Strict | Interface registration, read-only exposure + write methods | Controller issues Commands / processed reads via Query |

### MVP family (View passive, Presenter pushes)

| Sample | Tier | Model exposure | Write / read path |
|------|------|-------------|----------------|
| `Counter-Mvp-Quick` | Lesson 1 · Quick | Concrete-class registration, writable `ObservableValue` modified directly | Presenter writes / reads directly and pushes |
| `Counter-Mvp-Standard` | Lesson 2 · Standard | Concrete-class registration, read-only exposure + write methods | Presenter calls write methods / reads Model and pushes |
| `Counter-Mvp-Strict` | Lesson 3 · Strict | Interface registration, read-only exposure + write methods | Command writes / Query pulls and pushes |

> **MVP View boundary**: all three MVP Views are plain `MonoBehaviour` (passive views carry no Context capability); the Quick tier has no interface abstractions at all (the Presenter holds the concrete panel class), from the Standard tier onward the View contract exists as an `IXxxView` interface, and the Strict tier View additionally holds the Presenter via a narrow business interface (`IXxxPresenter`, lifecycle methods only).

### Utility samples

| Sample | Description | Dependency |
|------|------|------|
| `ObservableValue` | Custom Drawer demo: how simple and compound serializable types render in the Inspector | Odin Inspector |
| `MiniEvent` | Parameterless / single-parameter event usage; multi-parameter payloads are best wrapped in a struct as a single-parameter event | None |

### Hands-on sample

| Sample | Description | Dependency |
|------|------|------|
| `PlaneWar` | Vertical shooter "Plane War" (Mono version): a complete self-contained mini-game demonstrating how MiniEvent, ObservableValue, and MonoLifecycleProxy combine in real gameplay | None |

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
├── Documentation/
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
│   │   ├── Observable/            # ObservableValue reactive property + ObservableList/ObservableDictionary/ObservableHashSet observable collections
│   │   └── Utilities/             # PlayerLoopUtility + AesirArchitecturePlayerLoop
│   ├── Common/                    # Framework infrastructure
│   │   ├── AesirArchitecture.cs   # Framework MonoBehaviour singleton entry
│   │   ├── AesirMonoBehaviour.cs  # Odin-adapted base class
│   │   ├── AesirScriptableObject.cs
│   │   ├── AesirArchitectureDebug.cs
│   │   ├── AssemblyInfo.cs
│   │   └── ResetStaticsAssistant.cs
├── Editor/
│   ├── Runestone.AesirArchitecture.Editor.asmdef
│   ├── Common/
│   │   └── EnsureAesirArchitectureDefine.cs  # Compile symbol management
│   ├── Utilities/
│   │   ├── ScriptingSymbolUtility.cs
│   │   └── QuickCreateSOMenuItem.cs          # Context-menu quick SO creation (yields to Aesir Inspector when present)
│   ├── UpdateChecker/                        # In-package updater (Tools → Aesir → Check for Updates)
│   │   ├── AesirUpdateService.cs             # Stateless toolkit: install scanning, multi-source version check, manifest diff, backup
│   │   └── AesirUpdateWindow.cs              # Updater window
│   └── OdinInspector/            # Odin Inspector integration (optional)
│       ├── Runestone.AesirArchitecture.Editor.OdinInspector.asmdef
│       └── AttributeProcessors/
│           ├── AesirArchitectureAttributeProcessor.cs
│           ├── RemoveListenerOnSceneUnloadedTriggerAttributeProcessor.cs
│           └── ObservableValueAttributeProcessor.cs
├── Tests/
│   ├── Runtime/
│   │   └── Runestone.AesirArchitecture.Tests.asmdef
│   └── Editor/
│       └── Runestone.AesirArchitecture.Tests.Editor.asmdef
├── Samples/                       # Samples (directly visible & runnable in this repo, kept in sync with Samples~)
│   ├── Counter-Mvc-Quick/         # MVC-1 quick tier (MonoViewController writes writable ObservableValue directly, lesson 1)
│   ├── Counter-Mvc-Standard/      # MVC-2 standard tier (read-only exposure + write methods, View/Controller separated sharing Model, lesson 2)
│   ├── Counter-Mvc-Strict/        # MVC-3 strict tier (interface registration + Command writes + Query processed reads, lesson 3)
│   ├── Counter-Mvp-Quick/         # MVP-1 quick tier (Presenter writes writable ObservableValue directly, lesson 1)
│   ├── Counter-Mvp-Standard/      # MVP-2 standard tier (read-only exposure + write methods, Presenter calls write methods directly, lesson 2)
│   ├── Counter-Mvp-Strict/        # MVP-3 strict tier (Command writes + Query reads, View holds Presenter via narrow interface, lesson 3)
│   ├── ObservableValue/           # ObservableValue Inspector demo (Odin Inspector)
│   ├── MiniEvent/                 # MiniEvent usage examples
│   └── PlaneWar/                  # Vertical shooter "Plane War" (Mono hands-on sample)
├── Samples~/                      # Sample source mirror (import on demand via Package Manager after Git URL install; excluded from builds)
└── Third Party Notices.md          # Third-party license notices
```

## Design Boundaries

> The framework stays minimal: low-probability issues, or issues caused by discouraged coding patterns, are prevented up front by the conventions in this section — not by defensive runtime code.

### Not Provided

- **Event bus / EventChannel** — Cross-module communication uses GetModel + ObservableValue subscriptions, or direct MiniEvent references
- **Multiple Context instances** — `AbstractContext<T>` is a CRTP generic singleton, one instance per concrete context type; model multi-save / multi-room scenarios at the business layer
- **Command/Query pooling, async, queues, Undo/Redo** — `ExecuteCommand` / `ExecuteQuery` stay synchronous and uncached; wrap at the business layer for allocation-sensitive hot paths
- **View lifecycle scaffolding** — The View layer stays thin; panel lifecycle is handled by UIModule in Aesir Modules
- **Full observable-collection suite** — Only `ObservableList<T>` / `ObservableDictionary<TKey, TValue>` / `ObservableHashSet<T>` with the most common Added / Removed / Replaced / Updated / Cleared notifications are provided; Move, Sort, synchronized views, R3 integration, and other advanced capabilities are out of scope — use [Cysharp.ObservableCollections](https://github.com/Cysharp/ObservableCollections) when you need them
- **Thread safety** — All framework types are main-thread only; dispatch back to the main thread before touching the framework from async code (e.g. `Task.Run`)

### Coding Conventions (violations fail fast; the framework does not compensate)

| Convention | Consequence of violation |
|------|---------|
| Listener callbacks must not throw | Exceptions propagate and interrupt subsequent callbacks of the same event (native C# event semantics); Unity logs the error |
| Never access `Instance` inside `Configure()` or module initialization | A second context instance is created recursively (the singleton is not published yet) |
| `Register` and `Get` must use the same type argument | Exact-key matching: querying an interface-keyed registration by implementation type returns null / throws not-registered (with near-miss hint) |
| Runtime Model/Service replacement is for testing/debugging only | The old instance is disposed, its subscriptions are not migrated; subscribed views must re-subscribe |
| Call `AesirArchitecturePlayerLoop.EnsureInjected()` once after a third-party SDK rewrites the PlayerLoop | BeforeUpdate / AfterUpdate hooks silently stop firing (`Register` self-heals on callback registration) |
| **Write-discipline tiers** | Quick tier: direct writes to the writable ObservableValue are legal; Standard tier: Model narrowed to read-only interface + write methods (Controller calls write methods directly); Strict tier: writes must go through Command + interface registration; Services may write directly. Recommended: start from the Standard tier |

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
- [x] Observable collection family (List / Dictionary / HashSet)
- [x] In-package updater (Aesir Updater)
- [ ] ScriptableObject visualization config layer
- [ ] Editor toolchain (SO Inspector / MVP scaffold / module visualization)
- [ ] RuntimeSet

## License

[MIT](./LICENSE.md)
