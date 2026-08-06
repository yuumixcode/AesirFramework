# Aesir Modules

Functional module package for Aesir Architecture (RAA). Currently provides a minimal UI framework using Unity's "Manager of Managers" pattern, organized into Engine / Component two layers.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](../LICENSE.md)
[![Version](https://img.shields.io/badge/version-0.8.0-blue.svg)](../CHANGELOG.md)
[![Unity](https://img.shields.io/badge/Unity-2022.3%2B-black.svg)](https://unity.com/)
[![Install via Git URL](https://img.shields.io/badge/UPM-Git%20URL-blueviolet.svg)](#installation)
[![中文](https://img.shields.io/badge/README-中文-red.svg)](../README.md)

> 📦 **This package is part of the [Unity-Aesir-Packages](https://github.com/yuumixcode/Unity-Aesir-Packages) monorepo.** This package **depends on BOTH**:
> - **[Aesir Architecture](https://github.com/yuumixcode/Unity-Aesir-Packages)** (`>= 0.8.0`)
> - **[Aesir Inspector](https://github.com/yuumixcode/Unity-Aesir-Packages)** (`>= 0.8.0`)

## Modules

| Module | Status | Description |
|------|------|------|
| UI | Implemented | UIManager singleton; 4-layer Canvas + panel lifecycle |
| Scene | Planned | Scene loading, SceneReference |

## Dependencies

- **Aesir Architecture (RAA)** `cn.runestone.aesir.architecture` >= 0.8.0 (required)
- **Aesir Inspector** `cn.runestone.aesir.inspector` >= 0.8.0 (required)
- **Odin Inspector** (optional): participates only via `#if ODIN_INSPECTOR` conditional compilation; auto-excluded when not installed.

## Installation

Add the package via Unity Package Manager:

```
Packages/manifest.json -> dependencies:
"cn.runestone.aesir.modules": "https://github.com/yuumixcode/Unity-Aesir-Packages.git"
```

UPM automatically resolves the `dependencies` field in `package.json` and pulls Aesir Architecture + Aesir Inspector.

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

## License

MIT
