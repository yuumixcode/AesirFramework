# AesirFramework · Aesir Series Unity Packages

> A progressive MVC architecture and UI framework collection for Tuanjie Engine / Unity. Each sub-package can be installed separately via Git URL and used on demand.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE)
[![Unity](https://img.shields.io/badge/Unity-2022.3%2B-black.svg)](https://unity.com/)
[![Tuanjie](https://img.shields.io/badge/Tuanjie-2022.3%2B-blueviolet.svg)](#)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](./CONTRIBUTING.md)
[![Code of Conduct](https://img.shields.io/badge/Contributor%20Covenant-2.1-4baaaa.svg)](./CODE_OF_CONDUCT.md)
[![中文](https://img.shields.io/badge/README-中文-red.svg)](./README.md)

---

## ✨ Two Sub-Packages, Independently Installable

> **Key point: all sub-packages are published from the same Git repository.** Aesir Modules **depends on** Aesir Architecture; both can be installed standalone.

| Sub-Package | Purpose | Package ID | Version |
|---|---|---|---|
| **Aesir Architecture** | Progressive MVC architecture (capability composition, Command/Query, PlayerLoop lifecycle, reactive properties) | `cn.runestone.aesir.architecture` | `0.15.0` |
| **Aesir Modules** | UI framework (Manager of Managers, 4-layer Canvas, panel lifecycle) + ⚠️ Experimental Event Module | `cn.runestone.aesir.modules` | `0.15.0` |

> 📝 **Namespaces**: All sub-packages use `Runestone.*` namespaces (brand: "Runestone" / 符文石).

---

## 🏛️ Aesir Architecture (RAA) — Architecture Framework

**RAA is a progressive MVC architecture built on a "Unity-native-first" philosophy.** It does not build a parallel system to the engine; instead it deeply integrates with Unity's PlayerLoop, ScriptableObject, and Editor API — staying lightweight while providing clear layering for small to medium-large projects. The framework is **MVC-first** (`IController` is the recommended entry point for rapid development); `IPresenter` (MVP) is an optional pattern for stricter layering.

### Architecture Roles & Capability Interfaces

The framework uses **capability interface composition** — each role exposes only what it needs by combining fine-grained capability interfaces, instead of inheriting a monolithic base interface:

| Role | Interface | Capabilities | Responsibility |
|------|-----------|--------------|----------------|
| **Model** | `IModel` → `AbstractModel` | GetModel, GetService | Data layer; holds `ObservableValue<T>`, modifications only via write methods |
| **Service** | `IService` → `AbstractService` | GetModel, GetService | Cross-module coordination; may write Models directly, cannot execute Command/Query |
| **View** | `IView` | GetModel, GetService (read-only) | Presentation; subscribes to Model notifications for refresh |
| **Controller** | `IController` | GetModel, GetService, **ExecuteCommand**, **ExecuteQuery** | MVC entry point (recommended) |
| **Presenter** | `IPresenter` | All Controller + IDisposable | MVP pattern (optional); mediates Model ↔ View, View is passive |
| **Command** | `ICommand` → `AbstractCommand` | Execute() | Write operations (synchronous, no return value) |
| **Query** | `IQuery<TResult>` | Execute() → TResult | Read operations (no side effects), CQRS-style |

`AbstractContext<T>` (CRTP generic static singleton) is the architecture root: subclasses register Models/Services in `Configure()`; the first access to `Instance` triggers initialization (Models then Services in registration order); unregistered types throw descriptive exceptions instead of returning null.

### The Three-Tier Progressive Path (Core Design)

RAA's most distinctive feature is **tiered progression** — from the minimal concept set to a fully decoupled strict layering, one core increment per tier. MVC and MVP each have three tiers that mirror each other lesson by lesson:

| Tier | Model exposure | MVC (View self-subscribes) | MVP (View passive, Presenter pushes) |
|------|---------------|----------------------------|--------------------------------------|
| **Lesson 1 · Quick** | Concrete-class registration, writable `ObservableValue` | `MonoViewController<T>` writes directly | Presenter writes directly and pushes |
| **Lesson 2 · Standard** | Read-only exposure + write methods | Controller calls write methods | Presenter calls write methods |
| **Lesson 3 · Strict** | Interface registration + read-only + write methods | Command writes + Query processed reads | Command writes + Query reads |

Direct writes are legal at the Quick tier (great for prototypes); the Standard tier encapsulates modification entry points (recommended starting point); the Strict tier fully decouples reads/writes with the best extensibility. At the Strict tier, Views hold Controllers/Presenters via **narrow business interfaces** (framework capabilities like `ExecuteCommand` are unreachable at the type level) — read/write separation is enforced by the type system. The package ships 6 counter samples + ObservableValue / MiniEvent / PlaneWar hands-on samples, importable lesson by lesson.

### Core Mechanics at a Glance

- **`ObservableValue<T>` reactive property** — Models hold writable instances; Views subscribe read-only via `IReadOnlyObservableValue<T>`; `AddListenerAndInvoke` synchronizes the initial value on subscription
- **Observable collection family** — `ObservableList<T>` / `ObservableDictionary<TKey,TValue>` / `ObservableHashSet<T>` provide Added / Removed / Replaced / Updated / Cleared change notifications, following the same read-write separation and event pattern as ObservableValue
- **`MiniEvent` / `MiniEvent<T>`** — Zero-allocation lightweight events (direct multicast invocation, native C# fail-fast semantics); returns `AutoRemoveListenerHandle` for automatic cleanup, with auto-unsubscribe on GameObject destroy / scene unload
- **Native PlayerLoop lifecycle** — `AesirArchitecturePlayerLoop` injects `BeforeUpdate` / `AfterUpdate` frame callbacks without MonoBehaviour; `EnsureInjected()` self-heals after third-party SDKs rewrite the PlayerLoop
- **Explicit DDOL decision** — Root singletons expose a serialized `dontDestroyOnLoad` field governing both pre-placed and runtime-created instances (persistent by default; when disabled the instance dies with its scene — Inspector warning + runtime reminder, additive multi-scene loading is up to you)
- **Domain Reload safety (iron rule)** — All statics explicitly reset (RIOLM inside non-generic classes / `ResetStaticsAssistant` for generic ones); no residue across Play Mode re-entry
- **Pure C# core + MonoBehaviour adapters** — The Engine layer has zero MonoBehaviour dependencies; `MonoView<T>` / `MonoViewController<T>` etc. serve as adapters; Odin is an optional enhancement, never a runtime prerequisite

### Design Philosophy

1. **Unity-native first** — Use engine capabilities (PlayerLoop / SO / Editor API); no parallel homegrown systems
2. **Minimal boundaries** — No event bus, no multi-Context instances, no Command pooling / async / Undo; low-probability issues are prevented up front via documented conventions and edit-time hints (InfoBox), not runtime defensive code
3. **Presentation/logic separation** — All Inspector presentation is injected dynamically via Odin AttributeProcessors; runtime assemblies carry zero styling attributes

---

## 🧱 Aesir Modules (RAM) — Functional Module Package

**RAM is the functional module collection built on top of Architecture**, currently providing a UI framework and an experimental event module, plus scene management tooling.

### UI Framework

- **`UIModule` (Manager-of-Managers singleton)** — Static shortcut API: `UIModule.Show<T>()` / `Hide<T>()` / `Get<T>()` / `Prewarm<T>()` / `RegisterPrefab<T>()`; panel state machine: active → deactivated cache → destroyed
- **`UIRoot` 4-layer Canvas hierarchy** — One-click construction of Background / Normal / Popup / Top layered Canvases + UICamera + EventSystem (`GameObject → Aesir Modules → Create UIRoot`); layer Canvas references are serialized and persisted
- **Panel lifecycle** — `IUIPanel` contract `Initialize → Show(payload) → Hide → DestroyPanel`; `AesirBasePanel` offers virtual `OnInit` / `OnShow` / `OnHide` / `OnClose`; `DestroyOnHide` decides whether hiding destroys the panel or caches it for reuse
- **Pluggable asset loading** — `ResourcesUILoader` by default (Resources folder); implement `IUIAssetLoader` to swap in Addressables or anything else
- **Prewarming** — `Prewarm<T>()` pre-instantiates frame by frame; `PrewarmAll()` spreads out first-open instantiation cost
- **Binder component binding (Odin optional)** — `BinderAssistant` / `BinderTag` auto-bind UI elements to panel scripts
- **Input System adaptation** — A separate assembly automatically swaps in `InputSystemUIInputModule` when the Input System is enabled

### Event Module (⚠️ Experimental)

A dual-track subscription event system: `[AesirListener]` attribute-based static subscription + `AddListener<T>` dynamic lambda subscription, coexisting in one dispatch flow sorted by 5 priority levels; static bindings use expression-tree-compiled delegates to optimize reflection overhead. **Not yet validated in a production project; APIs may change.**

### Scene Module & Editor Tools

- **`SceneModule`** — Automatic discovery and prioritized loading of the bootstrap scene, additive scene load tracking
- **Editor tools** — `SceneManagerWindow` scene management window, `BootstrapSceneHelper` one-click bootstrap scene registration, `SceneAssetWrapper` serializable scene reference

---

## 🕸️ Dependency Graph

```
┌──────────────────────┐
│  Aesir Architecture  │
│  MVC architecture    │
│  (core package)      │
└──────────────────────┘
            ▲
            │ depends on
            │
┌──────────────────────┐
│   Aesir Modules      │
│   UI framework       │
└──────────────────────┘
```

**Key constraints**:

- **Aesir Architecture** — does NOT depend on any Aesir sub-package; can be installed standalone
- **Aesir Modules** — depends on `cn.runestone.aesir.architecture`

---

## 📦 Installation

### Option 1: Pinned Version Branch (Recommended)

In the Unity Package Manager window, click `+` in the top-left → `Add package from git URL...` and paste the corresponding sub-package URL:

| Sub-Package | Git URL (pinned to 0.15.0) |
|---|---|
| Aesir Architecture | `https://github.com/yuumixcode/AesirFramework.git#AesirArchitecture-v0.15.0` |
| Aesir Modules | `https://github.com/yuumixcode/AesirFramework.git#AesirModules-v0.15.0` |

> Version branches are generated automatically by CI on every push to `main` via a per-package subtree split (the package content is the branch root). The repository only keeps the latest version branches.

### Option 2: unitypackage Import + In-Package Updater (mainland / offline friendly)

Download the matching unitypackage from [GitHub Releases](https://github.com/yuumixcode/AesirFramework/releases) and drop it into your project:

| Asset | Content |
|---|---|
| `AesirArchitecture-v<version>.unitypackage` | Aesir Architecture only |
| `AesirModules-v<version>.unitypackage` | Aesir Modules only (no dependencies; import Architecture yourself) |
| `AesirFramework-v<version>.unitypackage` | Both packages combined |

Packages installed this way live under `Assets/Runestone/` (code editable), and **updating requires no manual re-download**: open the in-package updater via `Tools → Aesir → Check for Updates` for one-click "detect new version → auto backup → diff-based stale cleanup → silent import". Version detection uses multi-source fallback for mainland connectivity (jsDelivr CDN → GitHub API → redirect probe); via CDN, a new release may take up to ~12 hours to be detected.

> Copies installed via Git URL (UPM) are outside the updater's scope — update them with the Package Manager directly.

### Option 3: Track main (Development Preview)

```
https://github.com/yuumixcode/AesirFramework.git?path=Assets/Runestone/AesirArchitecture
https://github.com/yuumixcode/AesirFramework.git?path=Assets/Runestone/AesirModules
```

### Option 4: Via `manifest.json`

Add the following to your project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "cn.runestone.aesir.architecture": "https://github.com/yuumixcode/AesirFramework.git#AesirArchitecture-v0.15.0",
    "cn.runestone.aesir.modules": "https://github.com/yuumixcode/AesirFramework.git#AesirModules-v0.15.0"
  }
}
```

Add only the sub-packages you need — **when installing only Aesir Modules**, UPM will automatically resolve the dependencies and pull Aesir Architecture (declared in `package.json`'s `dependencies` field).

### Installing Samples

- **Browsing / downloading this repository directly**: samples live in each package's `Samples/` folder, ready to view and run.
- **Git URL install**: Package Manager → select the package → `Samples` tab → import on demand; the sample sources are also kept in each package's hidden `Samples~/` folder.
- **unitypackage import**: samples ship inside the package and run right after import.

---

## 🚀 Quick Start

> Pick the sub-package you're most interested in for the full guide. Below is a "smell test" only.

### Aesir Architecture — A Context in 3 Lines

```csharp
using Runestone.AesirArchitecture;

public class CounterContext : AbstractContext<CounterContext>
{
    protected override void Configure() => RegisterModel<ICounterModel>(new CounterModel());
}
```

For the full three-lesson path — View subscription refresh, Controller command dispatch — see the package README and the 6 counter samples (Package Manager → Samples).

Full guide: [`Assets/Runestone/AesirArchitecture/Documentation/README_EN.md`](./Assets/Runestone/AesirArchitecture/Documentation/README_EN.md) (English) / [`README.md`](./Assets/Runestone/AesirArchitecture/README.md) (中文).

### Aesir Modules — Show a Panel with `Show<T>()`

```csharp
UIModule.RegisterPrefab<MainMenuPanel>(prefab);
UIModule.Show<MainMenuPanel>();
UIModule.Show<ConfirmDialogPanel, ConfirmData>(new ConfirmData { message = "Confirm?" });
```

The UI framework provides Manager-of-Managers singletons, a 4-layer Canvas hierarchy (`UILayer`), panel lifecycle (active → deactivated cache → destroyed), and a pluggable asset loader (Resources by default, Addressables-ready).

Full guide: [`Assets/Runestone/AesirModules/Documentation/README_EN.md`](./Assets/Runestone/AesirModules/Documentation/README_EN.md) (English) / [`README.md`](./Assets/Runestone/AesirModules/README.md) (中文).

---

## 🗺️ Documentation Map

| Document | Location |
|---|---|
| Architecture tutorial (中文 / English) | Each package's `README.md` / `Documentation/README_EN.md` |
| Event-mechanism decision table (choosing between ObservableValue / MiniEvent / collections / EventModule) | [`AesirArchitecture/Documentation/事件机制决策表.md`](./Assets/Runestone/AesirArchitecture/Documentation/事件机制决策表.md) |
| Common pitfalls (10 frequent traps and fixes) | [`AesirArchitecture/Documentation/常见陷阱清单.md`](./Assets/Runestone/AesirArchitecture/Documentation/常见陷阱清单.md) |
| AI coding guide (for AI assistants generating RAA-style code) | [`AesirArchitecture/Documentation/AesirArchitecture-Skill/SKILL.md`](./Assets/Runestone/AesirArchitecture/Documentation/AesirArchitecture-Skill/SKILL.md) |
| Event module detailed manual | [`AesirModules/Documentation/event-module.md`](./Assets/Runestone/AesirModules/Documentation/event-module.md) |
| Changelogs (repo-level aggregate / per-package) | Root [`CHANGELOG.md`](./CHANGELOG.md) and each package's `CHANGELOG.md` |

> The in-package `Documentation/` folder ships with the unitypackage (excluded from player builds); Git URL users can read the `Documentation~/` mirror contents via the Package Manager.

---

## 🗂️ Repository Layout

> This is a **multi-package monorepo** — both sub-packages live here, each installable independently via Git URL.

```
AesirFramework/                            # this repo
├── README.md                              # 中文
├── README_EN.md                           # this file
├── LICENSE                                # MIT
├── CHANGELOG.md                           # repo-level changelog (aggregates both packages)
├── CONTRIBUTING.md                        # contributing guide
├── CODE_OF_CONDUCT.md                     # community guidelines
├── CODELY.md                              # detailed architecture docs
├── Assets/Scripts/Editor/                 # repo-local editor tooling (package export etc., not shipped with packages)
└── Assets/Runestone/
    ├── AesirArchitecture/                 # does NOT depend on other Aesir packages
    │   ├── Runtime/  Editor/  Tests/
    │   ├── Samples/                       # samples (directly visible & runnable in this repo)
    │   ├── Samples~/                      # sample sources (import on demand after Git URL install)
    │   ├── Documentation/                 # docs master (visible in Assets, shipped in unitypackage)
    │   ├── Documentation~/                # docs mirror (hidden copy for Git URL installs)
    │   └── README.md  CHANGELOG.md  package.json
    └── AesirModules/                      # depends on Architecture
        ├── Runtime/  Editor/
        ├── Samples/                       # samples (directly visible & runnable in this repo)
        ├── Samples~/                      # sample sources (import on demand after Git URL install)
        ├── Documentation/                 # docs master (visible in Assets, shipped in unitypackage)
        ├── Documentation~/                # docs mirror (hidden copy for Git URL installs)
        └── README.md  CHANGELOG.md  package.json
```

---

## ✅ Quality & CI

- **Tests** — 100 EditMode tests (in-package updater, Context, the Observable family, etc.); PlayMode tests cover MonoLifecycleProxy snapshot semantics, lifecycle event ordering, and more; CLI usage below in [Development Setup](#️-development-setup)
- **CI (GitHub Actions)** —
  - `auto-release.yml`: every push to `main` publishes a GitHub Release (three unitypackages plus the update-info.json / files-manifest used by the in-package updater)
  - `auto-publish-branches.yml`: per-package subtree split generating `AesirArchitecture-v<version>` / `AesirModules-v<version>` pinned branches

---

## 🛠️ Development Setup

> - **Unity / Tuanjie**: 2022.3.62f3c1 (or equivalent LTS)
> - **Render Pipeline**: URP 14.0.12
> - **Dependency**: [Odin Inspector](https://odininspector.com/) 3.3.x+ — optional enhancement; Aesir Architecture / Modules use `#if ODIN_INSPECTOR` conditional compilation and are excluded automatically when Odin is absent

On first open, Unity resolves dependencies from `Packages/manifest.json` automatically.

### Pre-warm Package Cache Without GUI

```bash
Unity -batchmode -quit -projectPath . -nographics -logFile /dev/null
```

### CLI Tests

```bash
# Edit mode
Unity -batchmode -projectPath . \
       -testPlatform editmode -runTests \
       -testResults TestResults.xml -logFile test.log

# Play mode — replace editmode with playmode
```

---

## 🤝 Contributing

Contributions of all kinds are welcome — bug reports, feature requests, doc improvements, and code.

- Read [`CONTRIBUTING.md`](./CONTRIBUTING.md) for the workflow
- Follow [`CODE_OF_CONDUCT.md`](./CODE_OF_CONDUCT.md)
- Branch from `main`; commit messages follow [Conventional Commits](https://www.conventionalcommits.org/)

---

## 📄 License

This repository and both sub-packages are released under the **MIT License**.

```
MIT License

Copyright (c) 2026 Yuumix
```

See root [`LICENSE`](./LICENSE) for details.

---

## 🔗 Recommended Links

- **Aesir Inspector** — separate public repository, a learning toolkit for [Odin Inspector](https://odininspector.com/) developers: [yuumixcode/AesirInspector](https://github.com/yuumixcode/AesirInspector)
- **Author homepage**: [yuumixcode](https://github.com/yuumixcode)


