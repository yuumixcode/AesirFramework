# Unity-Aesir-Packages · Aesir Series Unity Packages

> A progressive architecture framework, UI framework, and editor toolkit collection for Tuanjie Engine / Unity. Each sub-package can be installed separately via Git URL and used on demand.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE)
[![Unity](https://img.shields.io/badge/Unity-2022.3%2B-black.svg)](https://unity.com/)
[![Tuanjie](https://img.shields.io/badge/Tuanjie-2022.3%2B-blueviolet.svg)](#)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](./CONTRIBUTING.md)
[![Code of Conduct](https://img.shields.io/badge/Contributor%20Covenant-2.1-4baaaa.svg)](./CODE_OF_CONDUCT.md)
[![中文](https://img.shields.io/badge/README-中文-red.svg)](./README.md)

---

## ✨ Three Sub-Packages, Independently Installable

> **Key point: all sub-packages are published from the same Git repository.** Aesir Architecture and Aesir Inspector are **independent of each other**; Aesir Modules **depends on** Architecture. Aesir Inspector **requires [Odin Inspector](https://odininspector.com/)**.

| Sub-Package | Purpose | Package ID | Version |
|---|---|---|---|
| **Aesir Architecture** | Progressive MVC framework (capability composition, command pattern, PlayerLoop) | `cn.runestone.aesir.architecture` | `0.9.0` |
| **Aesir Modules** | UI framework (Manager of Managers, 4-layer Canvas, panel lifecycle) + ⚠️ Experimental Event Module | `cn.runestone.aesir.modules` | `0.9.0` |
| **Aesir Inspector** | Editor extension library (bilingual Inspector, safe editor utilities, script doc generator, Summary sync tool) | `cn.runestone.aesir.inspector` | `0.9.0` |

> 📝 **Namespaces**: All sub-packages use `Runestone.*` namespaces (brand: "Runestone" / 符文石).

---

## 🧩 Dependency Graph

```
┌──────────────────────┐         ┌──────────────────────────┐
│  Aesir Architecture  │         │  Aesir Inspector          │
│  MVP/MVC foundation  │ independent │  editor extensions (requires Odin) │
│  capabilities / etc. │         └──────────────────────────┘
└──────────────────────┘
            ▲
            │ depends on
            │
┌──────────────────────┐
│   Aesir Modules      │ ─── depends on Architecture
│   UI framework       │
└──────────────────────┘
```

**Key constraints**:

- **Aesir Architecture** — does NOT depend on any Aesir sub-package; can be installed standalone
- **Aesir Inspector** — does NOT depend on any Aesir sub-package; can be installed standalone; **requires Odin Inspector**
- **Aesir Modules** — depends on `cn.runestone.aesir.architecture`

---

## 📦 Installation

### Option 1: Via UPM Git URL (Recommended)

In the Unity Package Manager window, click `+` in the top-left → `Add package from git URL...` and paste the corresponding sub-package URL:

| Sub-Package | Git URL |
|---|---|
| Aesir Architecture | `https://github.com/yuumixcode/Unity-Aesir-Packages.git?path=Assets/Runestone/AesirArchitecture` |
| Aesir Modules | `https://github.com/yuumixcode/Unity-Aesir-Packages.git?path=Assets/Runestone/AesirModules` |
| Aesir Inspector | `https://github.com/yuumixcode/Unity-Aesir-Packages.git?path=Assets/Runestone/AesirInspector` |

Install only what you need.

**When installing only Aesir Modules**, UPM will automatically resolve the dependencies and pull Aesir Architecture (declared in `package.json`'s `dependencies` field).

### Option 2: Via `manifest.json`

Add the following to your project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "cn.runestone.aesir.architecture": "https://github.com/yuumixcode/Unity-Aesir-Packages.git?path=Assets/Runestone/AesirArchitecture",
    "cn.runestone.aesir.modules": "https://github.com/yuumixcode/Unity-Aesir-Packages.git?path=Assets/Runestone/AesirModules",
    "cn.runestone.aesir.inspector": "https://github.com/yuumixcode/Unity-Aesir-Packages.git?path=Assets/Runestone/AesirInspector"
  }
}
```

Add only the sub-packages you need — they are independent of each other (except Aesir Modules auto-depends on Architecture). Aesir Inspector requires Odin Inspector to be installed separately.

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

Full guide: [`Assets/Runestone/AesirArchitecture/Documentation~/README_EN.md`](./Assets/Runestone/AesirArchitecture/Documentation~/README_EN.md) (English) / [`README.md`](./Assets/Runestone/AesirArchitecture/README.md) (中文).

### Aesir Modules — Open a Panel with `Open<T>()`

```csharp
UIManager.RegisterPrefab<MainMenuPanel>(prefab);
UIManager.Open<MainMenuPanel>();
UIManager.Open<ConfirmDialogPanel>(new ConfirmData { message = "Confirm?" });
```

Full guide: [`Assets/Runestone/AesirModules/Documentation~/README_EN.md`](./Assets/Runestone/AesirModules/Documentation~/README_EN.md) (English) / [`README.md`](./Assets/Runestone/AesirModules/README.md) (中文).

### Aesir Inspector — Right-Click to Sync XML Summary

Select a script in the Project window → right-click → `Aesir → Summary Tool` → choose Sync / Replace / Remove.

Full guide: [`Assets/Runestone/AesirInspector/Documentation~/README_EN.md`](./Assets/Runestone/AesirInspector/Documentation~/README_EN.md) (English) / [`Assets/Runestone/AesirInspector/README.md`](./Assets/Runestone/AesirInspector/README.md) (中文).

---

## 🗂️ Repository Layout

> This is a **multi-package monorepo** — all three sub-packages live here, each installable independently via Git URL.

```
Unity-Aesir-Packages/                       # this repo
├── README.md                              # 中文
├── README_EN.md                           # this file
├── LICENSE                                # MIT
├── CHANGELOG.md                           # repo-level changelog (aggregates all three)
├── CONTRIBUTING.md                        # contributing guide
├── CODE_OF_CONDUCT.md                     # community guidelines
├── AGENTS.md                              # agent collaboration notes
├── CODELY.md                              # detailed architecture docs
└── Assets/Runestone/
    ├── AesirArchitecture/                 # does NOT depend on other Aesir packages
    │   ├── Runtime/  Editor/  Tests/  Samples~/  Documentation~/
    │   ├── README.md  CHANGELOG.md  LICENSE.md  package.json
    ├── AesirModules/                      # depends on Architecture
    │   ├── Runtime/  Editor/  Documentation~/
    │   ├── README.md  CHANGELOG.md  LICENSE.md  package.json
    └── AesirInspector/                    # does NOT depend on other Aesir packages; requires Odin Inspector
        ├── Runtime/  Editor/  Tests/  Samples~/  Documentation~/
        ├── README.md  CHANGELOG.md  CONTRIBUTING.md  LICENSE.md  package.json
```

---

## 🛠️ Development Setup

> - **Unity / Tuanjie**: 2022.3.62f3c1 (or equivalent LTS)
> - **Render Pipeline**: URP 14.0.12
> - **Dependency**: [Odin Inspector](https://odininspector.com/) 3.3.x+ — Aesir Inspector **requires** Odin Inspector; Aesir Architecture / Modules use `#if ODIN_INSPECTOR` conditional compilation for optional integration

On first open, Unity resolves dependencies from `Packages/manifest.json` automatically.

### Pre-warm Package Cache Without GUI

```bash
Unity -batchmode -quit -projectPath . -nographics -logFile /dev/null
```

### CLI Tests

```bash
# Edit mode
Unity -batchmode -quit -projectPath . \
       -testPlatform editmode -runTests \
       -testResults TestResults.xml -logFile test.log

# Play mode — replace editmode with playmode
```

Detailed build & test workflows: [`AGENTS.md`](./AGENTS.md).

---

## 🤝 Contributing

Contributions of all kinds are welcome — bug reports, feature requests, doc improvements, and code.

- Read [`CONTRIBUTING.md`](./CONTRIBUTING.md) for the workflow
- Follow [`CODE_OF_CONDUCT.md`](./CODE_OF_CONDUCT.md)
- Branch from `main`; commit messages follow [Conventional Commits](https://www.conventionalcommits.org/)

The Aesir Inspector sub-package has its own detailed `CONTRIBUTING.md` (with full code-style rules):
[`Assets/Runestone/AesirInspector/CONTRIBUTING.md`](./Assets/Runestone/AesirInspector/CONTRIBUTING.md).

---

## 📄 License

This repository and all three sub-packages are released under the **MIT License**.

```
MIT License

Copyright (c) 2026 Yuumix
```

See root [`LICENSE`](./LICENSE) and per-package `LICENSE.md` for details.

---

## 🔗 Links

> - **Author homepage**: [yuumixcode](https://github.com/yuumixcode)
