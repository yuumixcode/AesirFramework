# Contributing Guide

Thank you for your interest in Aesir Inspector! We welcome contributions of all forms, including but not limited to bug reports, feature suggestions, documentation improvements, and code contributions.

[中文](./CONTRIBUTING.md)

---

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [How to Contribute](#how-to-contribute)
- [Development Environment](#development-environment)
- [Project Structure](#project-structure)
- [Coding Standards](#coding-standards)
- [Submitting a Pull Request](#submitting-a-pull-request)
- [Questions & Help](#questions--help)

## Code of Conduct

This project adheres to the [Contributor Covenant](https://www.contributor-covenant.org/version/2/1/code_of_conduct/) Code of Conduct. By participating, you agree to uphold its terms. Please treat every community member with respect and constructive engagement.

## How to Contribute

### Reporting Bugs

1. Search [Issues](https://github.com/yuumixcode/Unity-Aesir-Packages/issues) for existing reports.
2. If none exists, [create a new Issue](https://github.com/yuumixcode/Unity-Aesir-Packages/issues/new) with:
   - **Reproduction steps**: Detailed steps to trigger the bug.
   - **Expected behavior**: What you expected to happen.
   - **Actual behavior**: What actually happened.
   - **Environment**: Unity/Tuanjie version, Odin Inspector version (if applicable), OS.
   - **Screenshots/Logs**: Attach screenshots or error logs if possible.

### Suggesting Features

1. Search [Issues](https://github.com/yuumixcode/Unity-Aesir-Packages/issues) for similar suggestions.
2. If none exists, create a new Issue with the `Feature Request` label, including:
   - **Use case**: Describe the specific problem you want to solve.
   - **Proposed solution**: Describe your desired solution.
   - **Alternatives considered**: Other approaches you've considered.

### Contributing Code

1. Fork this repository.
2. Develop under `Assets/aesir-inspector/` in the repository root.
3. Follow the [Coding Standards](#coding-standards) below.
4. Submit a Pull Request to the `main` branch.

## Development Environment

### Prerequisites

- **Tuanjie Editor** 2022.3 or later (Unity 2022.3 fork)
- **Git**: For version control
- **Odin Inspector** 3.3.x or later (optional dependency, for developing OdinIntegration features)

### Cloning the Project

```bash
git clone https://github.com/yuumixcode/Unity-Aesir-Packages.git
```

Place the cloned repository in your Tuanjie project's `Assets/` directory, or reference it as a local package via Package Manager.

### Odin Inspector Integration

Odin Inspector is an optional dependency:

- **Without Odin**: Core functionality compiles and runs normally; OdinIntegration assemblies are automatically skipped.
- **With Odin**: Importing automatically adds the `ODIN_INSPECTOR` compile symbol, enabling OdinIntegration enhanced assemblies.

## Project Structure

```
Aesir Inspector/
├── Runtime/
│   ├── Unity/                     # Core runtime (Runestone.AesirInspector)
│   │   ├── Attributes/            # [Summary] and other custom attributes
│   │   ├── Core/                   # Version, Paths, WebLinks
│   │   ├── Inspector/              # Inspector display models
│   │   ├── Localization/           # Localization data and language settings
│   │   ├── Logging/               # Logging system
│   │   ├── OdinBridge/            # IOdinBridge bridge layer
│   │   ├── ScriptDocGenerator/    # Doc generator runtime models
│   │   └── Utilities/             # Safe editor utilities
│   └── Odin Integration/          # Odin runtime (ODIN_INSPECTOR)
│       ├── Attributes/            # Bilingual attributes
│       └── OdinCodeHighlighter.cs
├── Editor/
│   ├── Unity/                     # Core editor (Runestone.AesirInspector.Editor)
│   │   ├── Core/                  # Install detection, menu management
│   │   ├── MiniTools/             # QuickCreateSO
│   │   └── SummaryTool/           # XML Summary sync
│   └── Odin Integration/          # Odin editor (ODIN_INSPECTOR)
│       ├── AttributeOverviewPro/  # Attribute overview window
│       ├── AttributeProcessors/   # OdinAttributeProcessor
│       ├── Bridge/                # OdinInspectorBridge
│       ├── Drawers/               # Bilingual Drawers
│       ├── ExtensionManager/      # Extension package manager
│       ├── MiniTools/             # MenuItem Viewer, Syntax Highlighter
│       ├── ScriptDocGenerator/    # Doc generator editor logic
│       └── Windows/               # Getting Started, Preferences
├── Tests/
│   ├── Editor/                    # Edit-mode tests
│   └── Runtime/                   # Play-mode tests
├── Samples~/                      # Usage examples
└── Documentation~/                # User docs and developer guides
```

### Assembly Overview

| Assembly | Odin Dependency | Description |
|----------|----------------|-------------|
| `Runestone.AesirInspector` | None | Core runtime, must not reference Odin API |
| `Runestone.AesirInspector.Editor` | None | Core editor |
| `Runestone.AesirInspector.OdinIntegration` | `ODIN_INSPECTOR` | Odin runtime bridge |
| `Runestone.AesirInspector.OdinIntegration.Editor` | `ODIN_INSPECTOR` | Odin editor enhancements |

## Coding Standards

Please read and follow these standards before submitting code. Detailed guidelines are in `Runtime/Unity/CodeStyle/AesirInspectorCodeStyle.cs` and `Documentation~/development.md`.

### Comment Standards

This project adopts **self-documenting code** and a **no-comment paradigm**:

- **No XML comments**: Do not use `/// <summary>`, `/// <param>`, etc.
- **Naming is documentation**: Convey intent through clear naming, no extra comments needed.
- **`[Summary]` only for complex logic**: Use `[Summary("...")]` only when naming cannot fully express intent, explaining "why" not "what".

```csharp
// ✅ Self-documenting: clear naming, no comments needed
public int MaxRetryCount { get; }
public void ApplyDamage(float amount) { }

// ✅ [Summary] explains "why"
[Summary("后者覆盖前者，用于多配置源优先级合并")]
public void MergeConfigSources(IReadOnlyList<ConfigSource> sources) { }

// ❌ XML comments are prohibited
/// <summary>
/// Apply damage amount
/// </summary>
public void ApplyDamage(float amount) { }
```

### Naming Conventions

| Identifier | Rule | Example |
|------------|------|---------|
| Classes, interfaces | `PascalCase`, interfaces prefixed with `I` | `PlayerManager`, `IDamageable` |
| Private non-serialized fields | `_camelCase` | `_health` |
| Serialized fields `[SerializeField]` | `camelCase` | `moveSpeed` |
| Constants / static readonly | `PascalCase` | `MaxScore` |

### Unity/C# Key Rules

- **Never** use `?.` / `??` on `UnityEngine.Object` derived types.
- Private methods corresponding to public methods use the `Internal_` prefix.
- Wrap editor-only code with `#if UNITY_EDITOR`.
- Odin-dependent code **must** reside in the `Odin Integration/` subdirectory.
- Core assemblies **must not** directly reference Odin API — use `IOdinBridge` instead.

### Event Conventions

| Role | Naming | Example |
|------|--------|---------|
| Event | No `On` prefix | `DoorOpened` |
| Subscribe method | `On` + event name | `OnDoorOpened` |
| Raise method | `Raise` + event name | `RaiseDoorOpened` |

### Enum Conventions

- Standard: Include `None = 0`, explicit values.
- Flags: `[Flags]`, values as `1 << n`, compose with `|`.

### Utility Naming

| Category | Naming Rule | Directory |
|----------|-------------|-----------|
| Runtime | `XxxUtility` | `Runtime/Unity/Utilities/` |
| Editor safe wrapper | `XxxSafeEditorUtility` | `Runtime/Unity/Utilities/` |
| Editor-only | `XxxEditorUtility` | `Editor/Unity/` |

## Submitting a Pull Request

### Process

1. Ensure a corresponding Issue exists (bug fix or feature suggestion). If not, create one first.
2. Fork the repository and create a feature branch from `main`:
   ```bash
   git checkout -b feature/your-feature-name
   # or
   git checkout -b fix/your-bug-fix-name
   ```
3. Write code following the coding standards.
4. Add necessary unit tests (located in `Tests/Editor/` and `Tests/Runtime/`).
5. Ensure all existing tests pass.
6. Commit changes using present tense, with concise descriptions:
   ```
   Add bilingual attribute processor for Button
   Fix null reference in OdinBridgeLocator
   ```
7. Push to your fork and create a Pull Request.
8. Reference the related Issue in the PR description (e.g., `Closes #123`).

### Branch Naming

| Type | Format | Example |
|------|--------|---------|
| Feature | `feature/<name>` | `feature/bilingual-toggle` |
| Fix | `fix/<name>` | `fix/odin-bridge-null-ref` |
| Docs | `docs/<name>` | `docs/update-contributing-guide` |

### PR Checklist

Before submitting a PR, confirm:

- [ ] Code follows project coding standards
- [ ] No XML documentation comments (use `[Summary]` or self-documenting naming instead)
- [ ] No `?.` / `??` on `UnityEngine.Object` derived types
- [ ] Editor-only code wrapped with `#if UNITY_EDITOR`
- [ ] Odin-dependent code placed in `Odin Integration/` subdirectory
- [ ] Core assemblies do not directly reference Odin API
- [ ] Necessary unit tests added
- [ ] All tests pass
- [ ] Commit messages are concise and use present tense

## Questions & Help

- **Bug reports & feature suggestions**: [GitHub Issues](https://github.com/yuumixcode/Unity-Aesir-Packages/issues)
- **Discussions & questions**: [GitHub Discussions](https://github.com/yuumixcode/Unity-Aesir-Packages/discussions)
- **Email**: zeriying@gmail.com

---

Thank you for your contribution! Every submission makes Aesir Inspector better.
