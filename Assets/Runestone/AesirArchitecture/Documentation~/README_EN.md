# Aesir Architecture

> A progressive MVP/MVC architecture framework for **Tuanjie Engine** / **Unity**, treating Unity native features as first-class citizens.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](../LICENSE.md)
[![Version](https://img.shields.io/badge/version-0.8.0-blue.svg)](../CHANGELOG.md)
[![Unity](https://img.shields.io/badge/Unity-2022.3%2B-black.svg)](https://unity.com/)
[![Install via Git URL](https://img.shields.io/badge/UPM-Git%20URL-blueviolet.svg)](#installation)
[![中文](https://img.shields.io/badge/README-中文-red.svg)](../README.md)

> 📦 **This package is part of the [Unity-Aesir-Packages](https://github.com/yuumixcode/Unity-Aesir-Packages) monorepo.** This package does **NOT depend on** any other Aesir sub-package (standalone installable).
>
> Related:
> - **[Aesir Inspector](https://github.com/yuumixcode/Unity-Aesir-Packages)** (standalone)
> - **[Aesir Modules](https://github.com/yuumixcode/Unity-Aesir-Packages)** (depends on Architecture + Inspector)

## Overview

AesirArchitecture (RAA) is an architecture framework built on a **Unity-native-first** philosophy. It does not build a parallel system to the engine; instead, it deeply integrates with Unity native capabilities such as PlayerLoop, ScriptableObject, and the Editor API — keeping things lightweight while providing a clear MVP / MVC layering for small to medium-large projects.

### Core Features

- **Native PlayerLoop lifecycle** — Inject custom subsystems into Unity's PlayerLoop via `AesirArchitecturePlayerLoop`, providing `BeforeUpdate` / `AfterUpdate` frame callbacks without MonoBehaviour
- **Capability interface composition** — Compose `IModel` / `IService` / `IView` / `IController` / `IPresenter` from fine-grained capability marker interfaces (`ICanGetModel`, `ICanExecuteCommand`, etc.) — expose only what you need
- **Command pattern** — `ICommand` handles write operations, executed synchronously
- **Query pattern** — `IQuery<TResult>` handles read operations, returns data without side effects
- **`ObservableValue<T>` reactive property** — Model holds a writable instance; View subscribes via covariant `IReadOnlyObservableValue<out T>` for layer safety
- **Runtime error logging** — `GetModel<T>()` / `GetService<T>()` throws exceptions with caller-type and target-type info when unregistered, replacing pre-flight validation; supports runtime model replacement
- **`AbstractSubmodule` unified submodule lifecycle** — Shared lifecycle logic for Model and Service is extracted into `AbstractSubmodule` base class, eliminating code duplication
- **`GenericLocator<T>` generic locator** — Type-keyed registration/query locator replacing the legacy Container, with global singleton support
- **Domain Reload safety** — Static variables explicitly reset via `[RuntimeInitializeOnLoadMethod]`; no residue across Play Mode entry/exit
- **Pure C# core + MonoBehaviour adapter** — Framework core is pure C#; the Engine layer does NOT depend on any Component layer type. `AesirView<T>` / `MonoView<T>` / `AesirViewController<T>` serve as MonoBehaviour adapters
- **MVC + MVP dual modes** — `IController` for fast development, `IPresenter` for stricter Model-View separation

### Comparison with QFramework

| Dimension | QFramework | AesirArchitecture |
|------|-----------|-------------------|
| Lifecycle | MonoBehaviour event callbacks | Native PlayerLoop injection (BeforeUpdate / AfterUpdate) |
| Architecture root | Generic singleton `Architecture<T>` | Generic static singleton `AbstractContext<T>` + `GenericLocator` |
| Observable property | `BindableProperty<T>` | `ObservableValue<T>` + covariant `IReadOnlyObservableValue<out T>` |
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

### 3. Define a View (MVC pattern)

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
│  AesirView<T> / MonoView<T>          │
│  AesirViewController<T>              │
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
├── Documentation~/README_EN.md     # This file
├── CHANGELOG.md
├── LICENSE.md
├── Third Party Notices.md
├── Runtime/
│   ├── Runestone.AesirArchitecture.asmdef
│   ├── Engine/                    # Pure C# + UnityEngine API (no MonoBehaviour dependency)
│   │   ├── Common/
│   │   │   ├── AesirArchitectureDebug.cs         # Unified logging
│   │   │   ├── AesirArchitecturePlayerLoop.cs    # PlayerLoop injection
│   │   │   ├── AssemblyInfo.cs                   # InternalsVisibleTo declarations
│   │   │   └── ResetStaticsAssistant.cs          # Static variable reset helper
│   │   ├── Context/               # IContext, AbstractContext<T>
│   │   ├── Modules/               # IModel, IService, IView, IController, IPresenter + Abstract bases
│   │   │   ├── Interfaces/        # Module interfaces
│   │   │   └── Abstracts/         # AbstractSubmodule, AbstractModel, AbstractService, AbstractCommand, AbstractQuery
│   │   ├── Capabilities/          # Capabilities.cs (ICan* interfaces) + CapabilityExtensions.cs
│   │   ├── Event/                 # MiniEvent<T>, AutoRemoveListenerHandle, RemoveListenerExtensions
│   │   ├── Observable/            # ObservableValue<T>, IObservableValue<T>, IReadOnlyObservableValue<T>
│   │   ├── Locator/               # GenericLocator<T>, IGenericLocator<T>
│   │   └── Utilities/             # PlayerLoopUtility
│   ├── Component/                 # MonoBehaviour components (depend on MonoBehaviour)
│   │   ├── Common/
│   │   │   ├── AesirArchitecture.cs           # Framework MonoBehaviour singleton entry
│   │   │   ├── AesirMonoBehaviour.cs          # Odin-adapted base class
│   │   │   └── AesirScriptableObject.cs      # Odin-adapted SO base
│   │   ├── View/
│   │   │   ├── AesirView.cs                  # Odin-adapted View base
│   │   │   └── MonoView.cs                   # Pure MonoBehaviour View base
│   │   ├── ViewController/
│   │   │   ├── AesirViewController.cs         # View + Controller dual-role base (Odin-adapted)
│   │   │   └── MonoViewController.cs          # View + Controller dual-role base (pure MonoBehaviour)
│   │   └── Event/
│   │       ├── RemoveListenerTrigger.cs              # Auto-remove listener trigger base
│   │       ├── RemoveListenerOnDestroyTrigger.cs
│   │       ├── RemoveListenerOnDisableTrigger.cs
│   │       └── RemoveListenerOnSceneUnloadedTrigger.cs
│   └── OdinIntergration/          # Independent assembly (depends on Odin Inspector)
│       ├── Runestone.AesirArchitecture.OdinIntegration.asmdef
│       └── DescriptionSO.cs
├── Editor/
│   ├── Runestone.AesirArchitecture.Editor.asmdef
│   ├── Common/
│   │   └── EnsureAesirArchitectureDefine.cs  # Compile symbol management
│   ├── Utilities/
│   │   └── ScriptingSymbolUtility.cs
│   └── OdinIntegration/          # Odin Inspector integration (optional)
│       ├── Runestone.AesirArchitecture.Editor.OdinIntegration.asmdef
│       └── AttributeProcessors/
│           ├── AesirArchitectureAttributeProcessor.cs
│           └── ObservableValueAttributeProcessor.cs
├── Tests/
│   ├── Runtime/
│   │   └── Runestone.AesirArchitecture.Tests.asmdef
│   └── Editor/
│       └── Runestone.AesirArchitecture.Tests.Editor.asmdef
├── Samples~/
│   ├── Counter-MVC/               # MVC pattern counter demo
│   ├── UI Counter-MVP/            # MVP pattern counter demo
│   ├── ObservableValue/           # ObservableValue Inspector demo (Odin Inspector)
│   └── MiniEvent/                 # MiniEvent usage examples
└── Third Party Notices.md          # Third-party license notices
```

## Design Principles

1. **Unity-native first** — Use Unity engine capabilities (PlayerLoop, ScriptableObject, Editor API) rather than building a parallel system
2. **Domain Reload compatible (iron rule)** — Static variables must be explicitly reset; no residue across Play Mode entry/exit
3. **Low MonoBehaviour dependency** — Core framework is pure C#; MonoBehaviour is only the adapter layer
4. **Progressive** — Use lightly in small projects, scale up gradually for large projects; no forced full adoption
5. **SO and pure code dual channels (planned)** — Each SO capability has a pure C# alternative
6. **Tuanjie Engine first** — Tuanjie is a first-class citizen

## Roadmap

- [x] Core MVP / MVC layering
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
