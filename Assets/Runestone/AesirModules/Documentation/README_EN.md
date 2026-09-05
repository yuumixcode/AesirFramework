# Aesir Modules

Functional module package for Aesir Architecture (RAA). Currently provides a UI framework (Manager of Managers pattern), an experimental event module, and scene management tooling.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](../LICENSE.md)
[![Version](https://img.shields.io/badge/version-0.15.0-blue.svg)](../CHANGELOG.md)
[![Unity](https://img.shields.io/badge/Unity-2022.3%2B-black.svg)](https://unity.com/)
[![Install via Git URL](https://img.shields.io/badge/UPM-Git%20URL-blueviolet.svg)](#installation)
[![中文](https://img.shields.io/badge/README-中文-red.svg)](../README.md)

> 📦 **This package is part of the [AesirFramework](https://github.com/yuumixcode/AesirFramework) monorepo.** This package **depends on**:
> - **[Aesir Architecture](https://github.com/yuumixcode/AesirFramework)** (`>= 0.15.0`)

## Modules

| Module | Status | Description |
|------|------|------|
| UI | Implemented | `UIModule` singleton (Manager of Managers) + `UIRoot` 4-layer Canvas + panel lifecycle + pluggable asset loading |
| Event | ⚠️ Experimental | `EventModule` dual-track subscription (Attribute + Script) + 5 priority levels + expression-tree optimization. Not yet validated in a production project |
| Scene | Implemented | `SceneModule` bootstrap/additive scene management + editor tools (SceneManagerWindow / BootstrapSceneHelper) |

> Two additional optional capabilities: **Binder component binding** (`UI/Runtime/OdinInspector/Binder/`, requires Odin Inspector) and **Input System input module adaptation** (`UI/Runtime/InputSystem/`, separate assembly, active automatically when the Input System is enabled).

## Dependencies

- **Aesir Architecture (RAA)** `cn.runestone.aesir.architecture` >= 0.15.0 (required)
- **Odin Inspector** (optional): participates only via `#if ODIN_INSPECTOR` conditional compilation; auto-excluded when not installed.

## Directory Layout

Functional modules (UI / Scene / Events) are independent of one another, and **all content of each module is consolidated in a single folder** containing only two secondary directories, `Runtime/` and `Editor/`; the shared foundation (host singleton, debug utilities) lives in `Common/`.

The asmdef anchors of the standing assemblies are centralized in `Common/`; module code is joined into the corresponding assembly via **Assembly Definition References (asmref)**:

- Module main code (`Runtime/`, `Editor/`) → joined into the core assemblies `Runestone.AesirModules` / `Runestone.AesirModules.Editor`
- Module-specific Odin code (`*/OdinInspector/` subfolders) → joined into `Runestone.AesirModules(.Editor).OdinInspector`
- The Scene Addressables editor glue (`Scene/Editor/Addressables/`) → joined into `Runestone.AesirModules.Editor.Addressables`

Consequently, **deleting a module folder removes that module completely** (including its Odin/Addressables code and tests) without affecting the compilation of any other module.

## Installation

### UPM (Git URL, recommended)

In the Unity Package Manager window, click `+` → `Add package from git URL...`:

```
https://github.com/yuumixcode/AesirFramework.git#AesirModules-v0.15.0
```

Or edit `Packages/manifest.json`:

```json
{
  "dependencies": {
    "cn.runestone.aesir.modules": "https://github.com/yuumixcode/AesirFramework.git#AesirModules-v0.15.0"
  }
}
```

To track the latest development version on `main`, replace the URL with `https://github.com/yuumixcode/AesirFramework.git?path=Assets/Runestone/AesirModules`.

UPM automatically resolves the `dependencies` field in `package.json` and pulls Aesir Architecture.

### unitypackage Import

Download `AesirModules-v<version>.unitypackage` (or the combined `AesirFramework-v<version>.unitypackage`) from [GitHub Releases](https://github.com/yuumixcode/AesirFramework/releases) and import it. Packages installed this way live under `Assets/Runestone/` and can be checked and updated in one click via the Unity menu `Tools → Aesir → Check for Updates` (shipped with Aesir Architecture).

## UI Module

### Core Types

| Type | Layer | Description |
|------|----|------|
| `UIModule` | Component | UI manager singleton: panel registration, showing, hiding, prewarming and registry maintenance; static shortcut API for global access |
| `UIRoot` | Component | UI root node: builds the 4-layer Canvas (Background / Normal / Popup / Top) + UICamera + EventSystem, applies the unified Canvas config |
| `IUIPanel` | Engine | Panel contract: lifecycle `Initialize → Show(payload) → Hide → DestroyPanel`; properties `Layer` / `DestroyOnHide` / `IsOpen` |
| `AesirBasePanel` | Component | Abstract panel base: virtual `OnInit` / `OnShow` / `OnHide` / `OnClose`, serialized fields `layer` / `destroyOnHide`, convenience `HideSelf()` |
| `AesirBasePanelView<T>` | Component | MVP-mode panel view base: inherits `AesirBasePanel` and binds to a Context type (`IView`), accessing Models / Services via the Context |
| `IUIAssetLoader` / `ResourcesUILoader` | Engine | Pluggable asset loading contract and default implementation (Resources folder) |
| `UICanvasConfigSO` | Component | Unified Canvas config asset (a default asset can be created from the Create menu) |
| `UILayer` | Engine | Layer enum: Background / Normal / Popup / Top |

### Quick Start

1. Create the UI root node with the full hierarchy via `GameObject → Aesir Modules → Create UIRoot` (or pre-place an object with `UIRoot` in the scene).
2. Create a panel prefab whose root node has a script inheriting from `AesirBasePanel` (inherit `AesirBasePanelView<TContext>` for the MVP pattern).
3. Register the prefab and show the panel:

```csharp
// Register the panel prefab
UIModule.RegisterPrefab<MainMenuPanel>(prefab);

// Show the panel
UIModule.Show<MainMenuPanel>();

// Show with a strongly-typed payload
UIModule.Show<ConfirmDialogPanel, ConfirmData>(new ConfirmData { message = "Confirm?" });

// Hide the panel (DestroyOnHide decides destroy vs. cache for reuse)
UIModule.Hide<ConfirmDialogPanel>();

// Prewarm: pre-instantiate and hide; the first Show reuses it directly
UIModule.Prewarm<MainMenuPanel>();
```

To use custom asset loading (e.g. Addressables), replace the default loader:

```csharp
UIModule.Instance.RegisterAssetLoader(new MyAddressablesLoader());
```

4. Panel lifecycle (all driven by `UIModule`):

```csharp
public class MainMenuPanel : AesirBasePanel
{
    protected override void OnInit() { }               // Called once after first instantiation
    protected override void OnShow(object payload) { } // Called on each show (including the first)
    protected override void OnHide() { }               // Called on hide (defaults to SetActive(false))
    protected override void OnClose() { }              // Called before destroy
}
```

> **Lifecycle details**: panels are instantiated in an inactive state (Awake / OnEnable are deferred until activation inside Show, so OnEnable can safely access references that only get values after OnInit), driven in the order attach-to-layer → `Initialize` → `Show`; panel registration is keyed by the instance's **actual type** — after showing via a base type, close it via the actual type (or `HideSelf()` inside the panel).

### Directory Structure

```
UI/Runtime/                        # joined into the core runtime assembly via asmref
├── UIModule.cs                    # UI manager singleton
├── UIRoot.cs                      # UI root node (4-layer Canvas construction)
├── IUIPanel.cs                    # Panel contract
├── AesirBasePanel.cs              # Panel base class
├── AesirBasePanelView.cs          # MVP panel view base (Context-bound)
├── UILayer.cs                     # Layer enum
├── UICanvasConfigSO.cs            # Canvas config asset
├── UIAssetLoader/                 # IUIAssetLoader + ResourcesUILoader
├── InputSystem/                   # Input System input module adaptation (separate optional assembly)
└── OdinInspector/Binder/          # Binder family (joined into the Odin assembly via asmref)
UI/Editor/                         # joined into the core editor assembly via asmref
├── UIModuleMenuItems.cs           # Create UIRoot / Default UICanvasConfig menu items
└── OdinInspector/                 # Odin AttributeProcessors (joined into the Odin editor assembly via asmref)
```

## Event Module

> ⚠️ **Experimental module**: not yet validated in a production project; APIs may change.

An event system based on dual-track subscription. Attribute subscription marks methods with the `[AesirListener]` attribute; Script subscription registers lambda delegates dynamically via `AddListener<T>`. Both kinds coexist in the same dispatch flow, sorted by 5 priority levels.

### Core Types

| Type | Description |
|------|------|
| `AesirEventArgs` | Abstract base class for event args. All custom event args inherit from it and flow through EventModule as data carriers |
| `AesirListenerAttribute` | Method attribute marking that the method listens to a given event-args type |
| `EventModule` | MonoBehaviour singleton managing the dual registries and event dispatch |
| `BindingInfo` | Binding info base class; `StaticBindingInfo` holds a MethodInfo + expression-tree-compiled delegate; `DynamicBindingInfo<T>` holds an `Action<T>` delegate directly |
| `SubscriberPriority` | Subscription priority enum (5 levels: First/High/Medium/Low/Last) |
| `AesirEventUtility` | Static utility methods for the event module |

### Quick Start

1. Define event args:

```csharp
using Runestone.AesirModules;

public class OnPlayerScored : AesirEventArgs
{
    public int points;
    public string playerName;
}
```

2. Attribute subscription (static binding):

```csharp
using UnityEngine;
using Runestone.AesirModules;

public class ScoreUI : MonoBehaviour
{
    void OnEnable()  => EventModule.AddListener(this);
    void OnDisable() => EventModule.RemoveListener(this);

    [AesirListener]
    private void OnPlayerScored(OnPlayerScored e)
    {
        Debug.Log($"[ScoreUI] {e.playerName} scored {e.points}");
    }
}
```

3. Script subscription (dynamic binding):

```csharp
using UnityEngine;
using Runestone.AesirModules;

public class ScoreController : MonoBehaviour
{
    AutoRemoveListenerHandle _handle;

    void OnEnable() =>
        _handle = EventModule.AddListener<OnPlayerScored>(this, e =>
            Debug.Log($"Score: {e.points}"));

    void OnDisable() => _handle.Dispose();
}
```

4. Publish an event:

```csharp
new OnPlayerScored { points = 10, playerName = "Player1" }.Invoke(this);
```

### API Cheat Sheet

```csharp
// Attribute subscribe / unsubscribe
EventModule.AddListener(this);    // call in OnEnable
EventModule.RemoveListener(this); // call in OnDisable

// Script subscription (returns AutoRemoveListenerHandle)
var handle = EventModule.AddListener<MyEventArgs>(this, e => { ... });
handle.Dispose();  // unsubscribe

// With priority
EventModule.AddListener<MyEventArgs>(this, e => { ... }, SubscriberPriority.First);

// Publish
new MyEventArgs().Invoke(this);              // chained call
EventModule.InvokeEvent(sender, eventArgs);   // direct call

// Parameterless method subscription (must specify the event-args type explicitly)
[AesirListener(typeof(OnKeyPressed))]
private void OnKeyPressed() { ... }
```

### Directory Structure

```
Events/Runtime/                    # joined into the core runtime assembly via asmref
├── AesirEventArgs.cs              # Event args base class
├── AesirListenerAttribute.cs      # Subscriber attribute
├── AesirEventUtility.cs           # Static utilities
├── BindingInfo.cs                 # Binding info base + StaticBindingInfo + DynamicBindingInfo<T>
├── Component/
│   └── EventModule.cs             # Event module singleton
└── SubscriberPriority.cs          # Priority enum (5 levels)
```

Detailed docs: [event-module.md](./event-module.md).

## Scene Module

`SceneModule` (MonoBehaviour singleton) manages the bootstrap and additive scenes:

- **Bootstrap scene** — auto-discovers the bootstrap scene by preset names (`Bootstrap` / `Bootstrapper` etc.) or a custom `SceneAssetWrapper` reference, ensures it sits at index 0 of Build Settings and loads first
- **Scene loading** — `LoadSceneSingle` for single-mode loading, `LoadSceneAdditive` for additive loading (pure additive tracking, `AddedScenePaths` / `LastLoadedScene` / `GetTotalLoadingProgress`); both accept a path or a `SceneAssetWrapper` overload with completion/failure callbacks
- **Scene unloading** — `UnloadScene` by path or reference, `UnloadAllAddedScenes` for batch recycling, `ReloadScene` for async reload of the current scene
- **`SceneAssetWrapper`** — serializable scene reference: GUID-anchor self-healing, state-machine validation (`State` / `UnsafeReason`), a `TryGet` safe-read family; when Addressables is installed, address-query capability is extended automatically (via a conditionally-compiled glue assembly; related features hide themselves when it is absent)
- **Editor companions** — `SceneManagerWindow` scene management window; `BootstrapSceneHelper` auto/manual bootstrap scene collection and Build Settings registration

### Directory Structure

```
Scene/Runtime/                     # joined into the core runtime assembly via asmref
├── SceneModule.cs                 # Scene management singleton (bootstrap / load / unload / reload)
├── SceneAssetWrapper.cs           # Serializable scene reference (GUID anchor + state machine)
├── SceneAssetWrapperState.cs      # Reference state machine
├── SceneAssetWrapperUnsafeReason.cs
├── SceneAssetWrapperAddressablesBridge.cs  # Addressables capability static bridge
└── Exceptions/                    # Dedicated exception family
Scene/Editor/                      # joined into the core editor assembly via asmref
├── SceneManagerWindow.cs          # Scene management window
├── BootstrapSceneHelper.cs        # Bootstrap scene registration tool
├── SceneEditorSettings.cs         # Editor persisted settings
├── Tests/                         # EditMode tests (separate test assembly)
├── OdinInspector/                 # SceneAssetWrapper Processor (joined via asmref)
└── Addressables/                  # Addressables glue implementation (joined via asmref)
```

## Binder Component Binding (Odin optional)

Located in `UI/Runtime/OdinInspector/Binder/` (joined into the Odin assembly via asmref, requires Odin Inspector): `BinderAssistant` / `BinderTag` auto-bind UI elements under a panel (Text, Button, etc.) to script fields by hierarchy, reducing manual reference dragging; extend via `IComponentBinder` for custom binders.

## Samples

- **Browsing / downloading this repository directly**: samples live in the package's `Samples/` folder, ready to view and run.
- **Git URL install**: Package Manager → select this package → `Samples` tab → import on demand (sources are kept in the package's hidden `Samples~/` folder, excluded from builds).
- **unitypackage import**: samples ship inside the package and run right after import.

Currently provided:

| Sample | Description |
|------|------|
| `Events/01_KeyPress` | Basic event-module publish-subscribe sample: a key press publishes an event, `[AesirListener]` static subscription |

## License

MIT

