# Aesir Modules

Functional module package for Aesir Architecture (RAA). Currently provides a UI framework (Manager of Managers pattern) and an event module (reflection binding + attribute-marked static subscription + dynamic subscription + expression-tree optimization).

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](../LICENSE.md)
[![Version](https://img.shields.io/badge/version-0.14.0-blue.svg)](../CHANGELOG.md)
[![Unity](https://img.shields.io/badge/Unity-2022.3%2B-black.svg)](https://unity.com/)
[![Install via Git URL](https://img.shields.io/badge/UPM-Git%20URL-blueviolet.svg)](#installation)
[![中文](https://img.shields.io/badge/README-中文-red.svg)](../README.md)

> 📦 **This package is part of the [AesirFramework](https://github.com/yuumixcode/AesirFramework) monorepo.** This package **depends on**:
> - **[Aesir Architecture](https://github.com/yuumixcode/AesirFramework)** (`>= 0.14.0`)

## Modules

| Module | Status | Description |
|------|------|------|
| UI | Implemented | UIManager singleton; 4-layer Canvas + panel lifecycle |
| Event | ⚠️ Experimental | EventModule singleton; dual-track subscription (Attribute + Script) + 5 priority levels + expression-tree optimization. Not yet validated in a production project |
| Scene | Planned | Scene loading, SceneReference |

## Dependencies

- **Aesir Architecture (RAA)** `cn.runestone.aesir.architecture` >= 0.14.0 (required)
- **Odin Inspector** (optional): participates only via `#if ODIN_INSPECTOR` conditional compilation; auto-excluded when not installed.

## Installation

Add the package via Unity Package Manager (pinned to the 0.14.0 version branch; the branch root is the package content):

```
Packages/manifest.json -> dependencies:
"cn.runestone.aesir.modules": "https://github.com/yuumixcode/AesirFramework.git#AesirModules-v0.14.0"
```

Track the latest development version on `main`:

```
"cn.runestone.aesir.modules": "https://github.com/yuumixcode/AesirFramework.git?path=Assets/Runestone/AesirModules"
```

UPM automatically resolves the `dependencies` field in `package.json` and pulls Aesir Architecture.

## UI Module

### Core Types

| Type | Layer | Description |
|------|------|------|
| `UIManager` | Component | MonoBehaviour singleton; manages panel loading, lifecycle, and layering; includes 4-layer Canvas, UICamera, EventSystem |
| `IUIPanel` | Engine | Panel contract interface |
| `IUIAssetLoader` | Engine | Asset loader contract interface |
| `IUICanvasConfig` | Engine | Canvas config contract interface |
| `PanelConfig` | Engine | Panel config (layer / destroyOnClose) |
| `UILayer` | Engine | Layer enum: Background=0, Normal=1, Popup=2, Top=3 |
| `ResourcesUILoader` | Engine | Default loader (Resources folder) |
| `AesirUIPanel` | Component | Panel abstract base, inherits from AesirMonoBehaviour |
| `UICanvasConfigSO` | Component | ScriptableObject config asset (CreateAssetMenu) |

### Quick Start

1. Create a UI prefab whose root node has a script inheriting from `AesirUIPanel`.
2. Register the prefab and open the panel:

```csharp
// Setup at startup (must be called before first panel operation)
UIManager.Instance.SetLoader(new ResourcesUILoader());
UIManager.Instance.SetCanvasConfig(configSO);   // optional

// Register a prefab
UIManager.RegisterPrefab<MainMenuPanel>(prefab);

// Open a panel
UIManager.Open<MainMenuPanel>();

// Open with payload
UIManager.Open<ConfirmDialogPanel>(new ConfirmData { message = "Confirm?" });

// Close a panel
UIManager.Close<ConfirmDialogPanel>();
```

3. Panel lifecycle:

```csharp
public class MainMenuPanel : AesirUIPanel
{
    protected override void OnInit() { }           // Called on first creation
    protected override void OnShow(object payload) { } // Called on each show
    protected override void OnHide() { }            // Called on hide
    protected override void OnClose() { }           // Called on destroy
}
```

### Directory Structure

```
Runtime/
├── Engine/UI/
│   ├── Interfaces/
│   │   ├── IUIPanel.cs
│   │   ├── IUIAssetLoader.cs
│   │   └── IUICanvasConfig.cs
│   ├── PanelConfig.cs
│   ├── UILayer.cs
│   ├── ResourcesUILoader.cs
│   └── UILog.cs
├── Component/UI/
│   ├── UIManager.cs
│   ├── AesirUIPanel.cs
│   └── UICanvasConfigSO.cs
Editor/
└── Odin Integration/UI/
    └── UICanvasConfigSOAttributeProcessor.cs
```

Detailed docs: [Documentation~/ui-module-manual.md](Documentation~/ui-module-manual.md).

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
Runtime/Events/
├── AesirEventArgs.cs              # Event args base class
├── AesirListenerAttribute.cs      # Subscriber attribute
├── AesirEventUtility.cs           # Static utilities
├── BindingInfo.cs                 # Binding info base + StaticBindingInfo + DynamicBindingInfo<T>
├── Component/
│   └── EventModule.cs             # Event module singleton
└── SubscriberPriority.cs          # Priority enum (5 levels)
```

Detailed docs: [Documentation~/event-module.md](Documentation~/event-module.md).

## Samples

- **Browsing / downloading this repository directly**: samples live in the package's `Samples/` folder, ready to view and run.
- **Git URL install**: Package Manager → select this package → `Samples` tab → import on demand (sources are kept in the package's hidden `Samples~/` folder, excluded from builds).

Currently provided:

| Sample | Description |
|------|------|
| `Events/01_KeyPress` | Basic event-module publish-subscribe sample: a key press publishes an event, `[AesirListener]` static subscription |

## License

MIT
