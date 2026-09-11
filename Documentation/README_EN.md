# Aesir Modules

Functional module package for Aesir Architecture (RAA). Currently provides a UI framework (Manager of Managers pattern), an experimental event module, audio management, scene management tooling, and a script documentation generator.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](../LICENSE.md)
[![Version](https://img.shields.io/badge/version-0.20.0-blue.svg)](../CHANGELOG.md)
[![Unity](https://img.shields.io/badge/Unity-2022.3%2B-black.svg)](https://unity.com/)
[![Install via Git URL](https://img.shields.io/badge/UPM-Git%20URL-blueviolet.svg)](#installation)
[![中文](https://img.shields.io/badge/README-中文-red.svg)](../README.md)

> 📦 **This package is part of the [AesirFramework](https://github.com/yuumixcode/AesirFramework) monorepo.** This package **depends on**:
> - **[Aesir Architecture](https://github.com/yuumixcode/AesirFramework)** (`>= 0.20.0`)

## Modules

| Module | Status | Description |
|------|------|------|
| UI | Implemented | `UIModule` singleton (Manager of Managers) + `UIRoot` 4-layer Canvas + panel lifecycle + pluggable asset loading |
| Event | ⚠️ Experimental | `EventModule` dual-track subscription (Attribute + Script) + 4 priority levels + expression-tree optimization + subscriber filters (precise delivery) + dead-reference cleanup + SO assetization. Not yet validated in a production project |
| Audio | Implemented | `AudioModule` singleton (minimal 2D audio facade) + SFX round-robin exclusive sources + BGM crossfade + 3-channel volume/mute persistence |
| Scene | Implemented | `SceneModule` scene load/additive/unload/activate + scene lifecycle events + `SceneAssetWrapper` serializable reference + editor tools (BootstrapSceneHelper / Scene Editor Settings) |
| ScriptDocGenerator | Implemented (requires Odin) | Reflection-based C# type analysis generating structured API docs (incremental, preserves hand-written content) + Summary tool (syncs XML `<summary>` and the `[Summary]` attribute, attribute-first) |

> Two additional optional capabilities: **Binder component binding** (`Runtime/UI/OdinInspector/Binder/`, requires Odin Inspector) and **Input System input module adaptation** (`Runtime/UI/InputSystem/`, separate assembly, active automatically when the Input System is enabled).

## Dependencies

- **Aesir Architecture (RAA)** `cn.runestone.aesir.architecture` >= 0.20.0 (required)
- **Odin Inspector** (optional): participates only via `#if ODIN_INSPECTOR` conditional compilation; auto-excluded when not installed. Note that **the `SceneAssetWrapper` Inspector experience of the Scene module (drag-assign, coloring, one-click fix buttons) requires Odin**; without Odin, only the API surface is guaranteed (construct via `FromScenePath`, assign `SceneAsset` in code, use the TryGet family) — the panel is not supported.

## Directory Layout

The package uses the standard Unity custom package root layout: `Runtime/` and `Editor/` are the two root-level folders, and functional modules live as subfolders within the corresponding layer (e.g. `Runtime/UI/` and `Editor/UI/`); the shared foundation lives in `Runtime/Common/`.

Assembly organization:

- **Core assemblies** are anchored at the layer roots (`Runtime/Runestone.AesirModules.asmdef`, `Editor/Runestone.AesirModules.Editor.asmdef`) — module main code under a layer joins the corresponding core assembly automatically, no asmref needed
- **Fine-grained assemblies** are anchored under `Common/` (Odin runtime `Runtime/Common/OdinInspector/`, Odin editor `Editor/Common/OdinInspector/`, Addressables glue `Editor/Common/Addressables/`); a module's matching dedicated code sits in its own `OdinInspector/` / `Addressables/` subfolder and is joined via an **Assembly Definition Reference (asmref)**
- **Deleting a module** = deleting `Runtime/<module>/` and `Editor/<module>/` (if present), without affecting the compilation of any other module

## Installation

### UPM (Git URL, recommended)

In the Unity Package Manager window, click `+` → `Add package from git URL...`:

```
https://github.com/yuumixcode/AesirFramework.git#AesirModules-v0.20.0
```

Or edit `Packages/manifest.json`:

```json
{
  "dependencies": {
    "cn.runestone.aesir.modules": "https://github.com/yuumixcode/AesirFramework.git#AesirModules-v0.20.0"
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
| `AesirBasePanelViewController<T>` | Component | MVC-mode panel controller base: inherits `AesirBasePanel` and binds to a Context type (`IController`), accessing Models / Services via the Context and executing Commands / Queries |
| `IUIAssetLoader` / `ResourcesUILoader` | Engine | Pluggable asset loading contract and default implementation (Resources folder). The contract is **synchronous**: suitable for Resources, synchronous caches and similar pipelines; async pipelines such as Addressables must be preloaded and returned synchronously |
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

To use custom asset loading, replace the default loader:

```csharp
UIModule.Instance.RegisterAssetLoader(new MyAddressablesLoader());
```

> **Loading contract**: `IUIAssetLoader.Load` is synchronous — suitable for Resources, synchronous caches and similar pipelines. Async pipelines such as Addressables cannot express waiting through this interface — they must be preloaded and returned synchronously (`Handle.Result` carries WebGL deadlock and main-thread blocking risks; evaluate carefully).

4. Panel lifecycle (all driven by `UIModule`):

```csharp
public class MainMenuPanel : AesirBasePanel
{
    protected override void OnInit() { }               // Called once after first instantiation
    protected override void OnShow(object payload) { } // Called on each show (including the first)
    protected override void OnHide() { }               // Called on hide (defaults to SetActive(false))
    protected override void OnClose() { }              // Called before controlled destroy (HidePanel with DestroyOnHide=true)
}
```

> **Lifecycle details**: panels are instantiated in an inactive state (Awake / OnEnable are deferred until activation inside Show, so OnEnable can safely access references that only get values after OnInit), driven in the order attach-to-layer → `Initialize` → `Show`; panel registration is keyed by the instance's **actual type** — when the prefab's root script is a *derived* class of the registered type, calling via the base type hits the key-semantics diagnostics (a repeated Show is rejected with an error; Hide / Get warn instead of failing silently). **Register, show, hide and get panels through the same type consistently** (`HideSelf()` inside a panel always uses the actual type and is always safe). `OnClose` is only invoked on the controlled destroy path (`HidePanel` with `DestroyOnHide=true`); **uncontrolled destroys (scene unload, external `Destroy`) only trigger `OnDestroy`** — put event unsubscription and cleanup in `OnDestroy` (or both), writing them only in `OnClose` leaks on scene transitions.

### Directory Structure

```
Runtime/UI/                        # joins the core runtime assembly (layer-root anchor)
├── UIModule.cs                    # UI manager singleton
├── UIRoot.cs                      # UI root node (4-layer Canvas construction)
├── IUIPanel.cs                    # Panel contract
├── AesirBasePanel.cs              # Panel base class
├── AesirBasePanelView.cs          # MVP panel view base (Context-bound)
├── AesirBasePanelViewController.cs # MVC panel controller base (Context + Command/Query capabilities)
├── UILayer.cs                     # Layer enum
├── UICanvasConfigSO.cs            # Canvas config asset
├── UIAssetLoader/                 # IUIAssetLoader + ResourcesUILoader
├── InputSystem/                   # Input System input module adaptation (separate optional assembly)
└── OdinInspector/Binder/          # Binder family (joined into the Odin assembly via asmref)
Editor/UI/                         # joins the core editor assembly (layer-root anchor)
├── UIModuleMenuItems.cs           # Create UIRoot / Default UICanvasConfig menu items
└── OdinInspector/                 # Odin AttributeProcessors (joined into the Odin editor assembly via asmref)
```

> **Design boundaries**:
> - **The layer system is a closed set** — `UILayer` is fixed at four layers (Background / Normal / Popup / Top), with the base sorting orders hardcoded to 100 / 200 / 300 / 400 and force-overwritten on every Awake. Adding a layer or changing orders requires modifying the framework source; no configuration surface is provided. Four layers are sufficient for teaching and small-to-mid projects.
> - **Panel roots attach directly under the layer Canvas (no per-panel Canvas)** — panels of the same layer share the layer Canvas: batching-friendly for same-atlas UI, at the cost of batch interruption on interleaved panels and no per-panel Canvas / independent sortingOrder surface. For an independent Canvas (animation isolation, render effects), add a child Canvas inside the panel prefab yourself (not managed by `UICanvasConfigSO`). In-layer rendering order is decided solely by the Show order (`SetAsLastSibling`).
> - **The loading contract is synchronous** — see the "Loading contract" note above; no async interface.
> - **`OnClose` only runs on the controlled destroy path** — see the lifecycle details above; uncontrolled destroys (scene unload / external Destroy) only trigger `OnDestroy`.
> - **Exclude the UI layer from your main camera** — the UICamera cullingMask only contains UI(5) and TransparentFX(1), but a main game camera that also includes the UI layer will render the UI twice; exclude it in your own camera setup.

See [Documentation/ui-module.md](./ui-module.md) for the detailed module documentation.

## Event Module

> ⚠️ **Experimental module**: not yet validated in a production project; APIs may change.

An event system based on dual-track subscription. Attribute subscription marks methods with the `[AesirListener]` attribute; Script subscription registers lambda delegates dynamically via `AddListener<T>`. Both kinds coexist in the same dispatch flow, sorted by 4 priority levels.

Dispatch is built in with subscriber filters (precise delivery: tag / priority tier / same scene / Collider2D bounds / same hierarchy family), automatic dead-reference cleanup, and an optional execution-time warning. SO assetization (`AesirEventArgsSO`) plus a UnityEvent bridge component let non-programmers configure events in the Inspector.

### Core Types

| Type | Description |
|------|------|
| `AesirEventArgs` | Abstract base class for event args. All custom event args inherit from it and flow through EventModule as data carriers; supports chained `WithFilter` delivery filters |
| `AesirListenerAttribute` | Method attribute marking that the method listens to a given event-args type (AllowMultiple: one method may listen to several event types) |
| `EventModule` | MonoBehaviour singleton managing the dual registries and event dispatch; auto-cleans destroyed subscribers during dispatch, optional `executionMsLimit` warning |
| `BindingInfo` | Binding info base class; `StaticBindingInfo` holds a MethodInfo + expression-tree-compiled delegate; `DynamicBindingInfo<T>` holds an `Action<T>` delegate directly |
| `ISubscriberFilter` | Subscriber-filter strategy interface; built-ins: `WithTag` / `WithPriority` / `SameSceneAsEmitter` / `OnlySelf` / `InsideCollider2D` |
| `SubscriberPriority` | Subscription priority enum (4 levels: First/High/Medium/Last) |
| `AesirEventArgsSO` | ScriptableObject wrapper for event args — save events as .asset resources, configure and raise them in the Inspector |
| `UnityEventOnAesirEvent` | UnityEvent bridge component letting non-programmers chain event callbacks in the Inspector |
| `SubclassSelector` | UI Toolkit PropertyDrawer providing a subclass dropdown for `[SerializeReference]` fields |
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

5. Subscriber filters (precise delivery, chained at publish time):

```csharp
// Only subscribers tagged "Enemy" receive it
new OnExplosion().WithFilter(new WithTag("Enemy")).Invoke(this);

// Combined filters: enemies inside the explosion radius
new OnExplosion()
    .WithFilter(new InsideCollider2D())
    .WithFilter(new WithTag("Enemy"))
    .Invoke(this);
```

Built-in filters: `WithTag` / `WithPriority` / `SameSceneAsEmitter` (multi-scene additive workflows) / `OnlySelf` (self, subtree or parent chain) / `InsideCollider2D` (spatial broadcast). Implement `ISubscriberFilter` for custom filters.

6. SO assetization (non-programmers configure events in the Inspector):

- Right-click in Project `Create → Aesir → Event Module → AesirEventArgsSO` to create an event asset; pick the args subclass via the `SubclassSelector` dropdown and configure its payload; click "触发事件（Raise）" in the Inspector at runtime or call `asset.Raise()` from code
- Add the `UnityEventOnAesirEvent` component, pick the event type, and bind UnityEvent callbacks in On Raised

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

// Subscriber filters (chained, all must pass for delivery)
new MyEventArgs().WithFilter(new WithTag("Enemy")).Invoke(this);

// Raise an SO asset (sender = the SO asset itself)
myEventArgsSO.Raise();

// Parameterless method subscription (must specify the event-args type explicitly)
[AesirListener(typeof(OnKeyPressed))]
private void OnKeyPressed() { ... }
```

> **Design boundaries**: dispatch is synchronous and non-reentrant (do not publish events synchronously inside subscriber callbacks); destroyed subscribers left unsubscribed are auto-cleaned during dispatch (dead-reference cleanup); system events (meta events) and channel labels depend on the editor toolchain and will be designed together with it. See [event-module.md](./event-module.md) for details.

### Directory Structure

```
Runtime/Events/                    # joins the core runtime assembly (layer-root anchor)
├── AesirEventArgs.cs              # Event args base class (Sender + WithFilter chain API)
├── AesirEventArgsSO.cs            # ScriptableObject wrapper (CreateAssetMenu + Raise)
├── AesirListenerAttribute.cs      # Subscriber attribute (AllowMultiple)
├── AesirEventUtility.cs           # Static utilities
├── BindingInfo.cs                 # Binding info base + StaticBindingInfo + DynamicBindingInfo<T>
├── ISubscriberFilter.cs           # Filter strategy interface
├── SubscriberFilters.cs          # Built-in filters (WithTag/WithPriority/SameSceneAsEmitter/OnlySelf/InsideCollider2D)
├── SubclassSelectorAttribute.cs   # Subclass-dropdown attribute for [SerializeReference] fields
├── Component/
│   ├── EventModule.cs             # Event module singleton (dead-ref cleanup + filters + perf monitoring)
│   └── UnityEventOnAesirEvent.cs  # UnityEvent bridge component
└── SubscriberPriority.cs          # Priority enum (4 levels)

Editor/Events/                     # joins the core editor assembly
├── SubclassSelectorDrawer.cs      # Subclass dropdown PropertyDrawer (UI Toolkit)
└── AesirEventArgsSOEditor.cs      # SO custom inspector (runtime-only Raise button)
```

Detailed docs: [event-module.md](./event-module.md).

## Audio Module

A minimal 2D audio facade (`AudioModule` singleton, all public APIs are static members — call and go):

- **SFX** — fixed-size round-robin exclusive audio sources (default 8, configurable): zero per-play instantiation overhead, per-play local volume and pitch applied independently, oldest source pre-empted when all are busy; `pitchJitter` randomizes pitch to avoid a mechanical feel
- **BGM** — dedicated looping source; idempotent when the same clip is already playing (re-entering a scene does not interrupt the music); switching supports coroutine crossfade (`unscaledDeltaTime`, slow motion does not detune)
- **Volume & mute** — Master / BGM / SFX three-channel multiplicative chain + three mute switches (Master as the master gate); settings take effect immediately and persist via PlayerPrefs (auto-restored on restart, key prefix configurable)
- **Pause** — `PauseAll` / `ResumeAll` pair, fits pause menus and app-backgrounding
- **Configuration** — zero-config usable; optional `AudioConfigSO` manages default volumes, persistence toggle and the PlayerPrefs key prefix

```csharp
using Runestone.AesirModules;

// SFX: fire-and-forget, ±0.1 pitch jitter
AudioModule.PlaySfx(clickClip, pitchJitter: 0.1f);

// BGM: immediate play; idempotent when the same clip is playing
AudioModule.PlayBgm(bgmClip);

// Switch track: fade out the old one over 1.5s → fade in the new one over 1.5s
AudioModule.PlayBgm(sceneB, fadeSeconds: 1.5f);

// Volume: effective and persisted on set
AudioModule.SfxVolume = 0.5f;
AudioModule.MasterMute = true;
```

> **Design boundaries**: 2D audio only — use the native `AudioSource.PlayClipAtPoint` for 3D spatial sounds; no AudioMixer integration; `PlaySfx` is fire-and-forget with no per-sound stop or completion callbacks (use MiniEvent for callbacks). See [audio-module.md](./audio-module.md) for details.

### Directory Structure

```
Runtime/Audio/                    # joins the core runtime assembly (layer-root anchor)
├── AudioModule.cs                # Audio manager singleton (SFX round-robin / BGM crossfade / volume persistence)
└── AudioConfigSO.cs              # Config asset (default volumes / persistence toggle / PlayerPrefs key prefix)
Editor/Audio/                     # joins the core editor assembly (layer-root anchor)
├── AudioModuleMenuItems.cs       # GameObject pre-placement menu
└── OdinInspector/                # AudioModule Processor (joined into the Odin editor assembly via asmref)
```

## Scene Module

`SceneModule` (MonoBehaviour singleton) manages scene loading, additive tracking and unload recycling, aligned with the native Unity `LoadSceneMode` semantics:

### Core Types

| Type | Description |
|------|------|
| `SceneModule` | Scene management singleton: Single/Additive loading (completion/failure/progress callbacks), unloading, reload, active-scene switching, `SceneLoadedEvent` / `SceneUnloadedEvent` lifecycle broadcast, DDOL serialized setting |
| `SceneAssetWrapper` | Serializable scene reference: GUID-anchor self-healing (rename/move immune, broken-ref recovery), state-machine validation (`State` / `UnsafeReason`), `TryGet` safe-read family; address-query capability extends automatically when Addressables is installed. The Inspector panel experience (drag-assign, coloring, one-click fixes) requires Odin Inspector; without Odin, only the API surface is guaranteed. Functional design references [Eflatun.SceneReference](https://github.com/starikcetin/Eflatun.SceneReference) |
| `SceneAssetWrapperState` / `SceneAssetWrapperUnsafeReason` | Reference state (Regular/Addressable/Unsafe) and unsafe-reason enums |
| `SceneAssetWrapperAddressablesBridge` | Addressables editor capability static bridge (zero Addressables dependency in the core assembly; hidden automatically when the package is absent) |
| `SceneAssetWrapperException` family | Four dedicated exceptions (empty reference / creation failure / package absent / not addressable), each message carries "fix / avoid" guidance |

Main `SceneModule` API:

```csharp
// Loading (path & SceneAssetWrapper overloads; onProgress reports per-frame 0-1
// progress, normalized against Unity's 0.9 activation cap so bars can reach 100%)
SceneModule.Instance.LoadSceneSingle(scenePath,
    onCompleted: () => { },
    onFailed:    () => { },
    onProgress:  p => { });
SceneModule.Instance.LoadSceneAdditive(scenePath);

// Unloading (module-additive scenes leave tracking automatically; batch unload
// skips a failed scene with a warning instead of failing everything)
SceneModule.Instance.UnloadScene(scenePath);
SceneModule.Instance.UnloadAllAddedScenes();

// Active-scene switching (multi-scene workflow: decides lighting source and
// the default Instantiate landing scene)
SceneModule.Instance.SetActiveScene(scenePath);

// Reload the active scene (async Single semantics)
SceneModule.Instance.ReloadScene();

// Scene lifecycle broadcast (MiniEvent, path payload; AddListener returns an auto-remove handle)
SceneModule.Instance.SceneLoadedEvent.AddListener(path => Debug.Log($"Loaded {path}"));
SceneModule.Instance.SceneUnloadedEvent.AddListener(path => Debug.Log($"Unloaded {path}"));

// Queries: AddedScenePaths (additive tracking) / LastLoadedScene / BootstrapSceneAssetWrapper
```

### Quick Start

1. Pre-place (recommended): attach `SceneModule` to an object in the bootstrap scene (or just touch `SceneModule.Instance` to auto-create under the `[Aesir Modules]` host). For a root-object pre-placement, DDOL follows the `dontDestroyOnLoad` field (default on) — **keep it on**: a Single load unloads every old scene, and an instance without DDOL is destroyed along with its scene, aborting in-flight load callbacks.

2. Declare scene references with `SceneAssetWrapper` and drag-assign in the Inspector (requires Odin):

```csharp
using Runestone.AesirModules;
using UnityEngine;

public class LevelFlow : MonoBehaviour
{
    [SerializeField] SceneAssetWrapper gameplayScene; // drag-assign in Inspector (requires Odin)

    void Start()
    {
        // Invalid refs (empty / not in Build Settings) and Addressable scenes route to onFailed, never throw
        SceneModule.Instance.LoadSceneSingle(gameplayScene,
            onCompleted: () => Debug.Log("Level entered"),
            onProgress: p => Debug.Log($"Loading {p:P0}"));

        // Without Odin: constructing a reference in code works exactly the same
        // var sceneRef = SceneAssetWrapper.FromScenePath("Assets/Scenes/Gameplay.unity");
    }
}
```

3. Editor validation: wrapper fields ship with tri-state coloring (Addressable cyan / dangling & missing-from-build red / disabled yellow / normal white) and one-click fix buttons (add to Build Settings / enable / make Addressable).

### Design Boundaries

- **Odin Inspector boundary** — the `SceneAssetWrapper` Inspector panel effects depend on Odin (injected via AttributeProcessor); without Odin only the API surface is guaranteed: construct via `SceneAssetWrapper.FromScenePath(...)`, assign the `SceneAsset` property in code (editor only), read via the TryGet family. The panel is not supported.
- **Addressable scenes are not loaded by SceneModule** — with Addressables installed the wrapper provides the address (`Address` / `TryGetAddress`); load and unload directly through the Addressables API (`Addressables.LoadSceneAsync(wrapper.Address)`).
- **Do not additive-load the same path twice** — Unity loads two scene instances while tracking records one path; `UnloadScene` unloads only one of them and the leftover instance escapes tracking.
- **Bootstrap split of duties** — at runtime `SceneModule` only holds the `bootstrapScene` reference for user code to read (`BootstrapSceneAssetWrapper`) and performs no automatic flow; Build Settings index 0 and force-opening the Bootstrap scene on Play are handled by the editor `BootstrapSceneHelper` (enabled in `Tools → Aesir → Scene Editor Settings`, off by default).
- **No cross-scene payload / no async** — pass data across scenes via framework MiniEvents or a shared Model; async support awaits a framework-wide decision.

### Directory Structure

```
Runtime/Scene/                     # joins the core runtime assembly (layer-root anchor)
├── SceneModule.cs                 # Scene management singleton (load / unload / reload / activate / events / DDOL)
├── SceneAssetWrapper.cs           # Serializable scene reference (GUID anchor + state machine)
├── SceneAssetWrapperState.cs      # Reference state machine
├── SceneAssetWrapperUnsafeReason.cs
├── SceneAssetWrapperAddressablesBridge.cs  # Addressables capability static bridge
└── Exceptions/                    # Dedicated exception family
Editor/Scene/                      # joins the core editor assembly (layer-root anchor)
├── SceneManagerWindow.cs          # Scene Editor Settings window (Tools/Aesir/Scene Editor Settings)
├── BootstrapSceneHelper.cs        # Bootstrap scene registration tool (off by default)
├── SceneEditorSettings.cs         # Editor persisted settings
├── Tests/                         # EditMode tests (SceneAssetWrapper 27 cases + SceneModule 20 cases)
├── OdinInspector/                 # SceneAssetWrapper Processor (joined via asmref)
└── Addressables/                  # Addressables glue implementation (joined via asmref)
```

## Binder Component Binding (Odin optional)

Located at `Runtime/UI/OdinInspector/Binder/` (joined into the Odin assembly via asmref, requires Odin Inspector). The whole Binder feature lives in the Odin assembly (including `[BinderBaseType]`): its type selectors (component/base-class ValueDropdowns) strongly depend on Odin Inspector.

- `BinderTag` marks child objects that need bound references (1 component bound by default); its "bound component count" declares how many components to bind on that object. The hierarchy context menu `GameObject/Aesir/` attaches `BinderAssistant` / `BinderTag` to selected objects in one click.
- `BinderAssistant` sits on the panel root. "Build Binding Units" incrementally maintains the binding list from the tags (each entry records component type, field name, and binding path). Two generation modes are supported (default "Same-Script Incremental"): "Same-Script Incremental" only replaces the `#region 绑定字段（自动生成）` block (fields + `BindComponents` method, fully-qualified and self-contained) inside the target `*.cs`, leaving everything outside the region to the developer — a scaffold is created automatically when the file does not exist; "Partial Class" produces the hand-written partial `*.cs` (generated once) and an auto-maintained file (suffix selectable, default `.designer.cs` — collapsed by default in Rider, recommended for Rider users). Generated bound fields are grouped under a `TitleGroup` ("绑定字段（自动生成）") marking them as Binder-maintained. Both modes auto-attach the generated component and bind once after compilation.
- The generated script's base class is selectable from a dropdown: built-in `MonoBehaviour`, the pre-selected Aesir panel family (`AesirBasePanel`, `AesirBasePanelView<T>`, `AesirBasePanelViewController<T>` — the core assembly cannot reference the Odin assembly back to carry the attribute, so the Binder pre-selects them via typeof), and user classes marked with `[BinderBaseType]` (requires referencing `Runestone.AesirModules.OdinInspector`); for the Aesir generic panel bases, pick a concrete Context type from the "Context 类型" dropdown (project-wide AbstractContext derivatives; the placeholder is never emitted into generated code).
- The default namespace and the partial suffix candidate list are persisted in-editor via ScriptableSingleton.
- Code generation is pure text assembly, covered by the EditMode test assembly `Runestone.AesirModules.Tests` (package-root `Tests/`); `IComponentBinder` remains the extension point for custom binders.

## Script Doc Generator Module (requires Odin)

Located at `Runtime/ScriptDocGenerator/OdinInspector/` and `Editor/ScriptDocGenerator/OdinInspector/` (joined into the Odin assemblies via asmref; **hard dependency on Odin Inspector**, auto-excluded when Odin is not installed). Namespace `Runestone.AesirModules.ScriptDocGenerator` (.Editor).

- **Script Doc Generator** — analyzes C# type information via reflection to generate structured API documentation: fully offline, millisecond-fast for single types, incremental generation (preserves hand-written content after `## Additional Notes` and any Front Matter), Markdown output ready for AI knowledge bases; parameter/returns/remarks/typeparam description columns end to end (Zensical generator); customizable output path (defaults to `<project root>/ScriptDocGenerator/`, outside Assets so no .meta files) / namespace subfolders / file extension / four type-source granularities (single type, multiple types, single assembly, multiple assemblies — the assembly dropdown lists script assemblies only), extensible via `DocGeneratorSettingsSO`, `IAnalysisDataFactory`, and `IAttributeFilter`. Entry point: `Tools → Aesir → Script Doc Generator`.
- **Summary Tool** — Project window context menu (`Assets → Script Doc Generator → Process Summary`) syncs XML `<summary>` comments and the `[Summary]` attribute with the attribute as the authoritative source (attribute-first, XML fallback): Sync (aligns both, keeps both) / Replace (collapses to a single attribute) / Remove (removes attributes, with confirmation) modes, batch processing with a single asset refresh, quote escaping, line-ending preservation, preprocessor-directive aware, auto-adds the `using` directive.
- **Custom attributes** — `[Summary]` (readable at runtime via `GetSummary()`), `[ReferenceLinkURL]` (attaches documentation links to types).

See [script-doc-generator.md](./script-doc-generator.md) (Chinese) for full documentation.

## Samples

- **Browsing / downloading this repository directly**: samples live in the package's `Samples/` folder, ready to view and run.
- **Git URL install**: Package Manager → select this package → `Samples` tab → import on demand (sources are kept in the package's hidden `Samples~/` folder, excluded from builds).
- **unitypackage import**: samples ship inside the package and run right after import.

Currently provided:

| Sample | Description |
|------|------|
| `Events/01_KeyPress` | Basic event-module publish-subscribe sample: a key press publishes an event, `[AesirListener]` static subscription |
| `Events/02_Filters` | Subscriber-filter sample: Space publishes an alarm with chained `WithTag` + `InsideCollider2D` filters, R publishes an `OnlySelf` family order; in-scene contrast groups (inside/outside/untagged, family/unrelated) visualize precise delivery |
| `Events/03_SOAsset` | SO assetization sample: `ScoreEventAsset.asset` configures the event payload (SubclassSelector dropdown); the `UnityEventOnAesirEvent` bridge component chains UnityEvent callbacks in the Inspector with zero code |
| `Audio/01_BasicUsage` | Audio module basic-usage sample: SFX playback (with pitch jitter), BGM crossfade switching, 3-channel volume and mute persistence |

## License

MIT

