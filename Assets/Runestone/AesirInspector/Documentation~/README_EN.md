# Aesir Inspector

[中文](../../README.md) | [![license](https://img.shields.io/badge/license-MIT-green.svg)](../../LICENSE.md)

`Aesir Inspector` is a Unity editor extension library designed to provide bilingual Inspector UI, safe editor tooling, script documentation generation, and more. **Optional integration with Odin Inspector** for enhanced Inspector rendering and styling.

> **💡 About Odin Inspector Dependency**: Odin Inspector is an **optional dependency** of this project. Core features (Summary Tool, Safe Editor Utilities, Doc Generator runtime, etc.) work without Odin and compile normally in Odin-free environments. After importing Odin Inspector, the `ODIN_INSPECTOR` compilation symbol is automatically added, enabling the Odin Integration enhancement assemblies with bilingual attribute decorators, Attribute Drawers, Processors, and other enhanced features.

## Who Is This For

- **Editor Tool Developers**: Developers building custom Inspector tools who need bilingual (Chinese/English) UI display support.
- **Cross-Region / Cross-Locale Teams**: Teams with diverse language backgrounds who need to display both Chinese and English information in the Inspector panel to reduce communication costs.
- **Unity Editor Users**: Developers who want safe editor utilities, documentation generators, Summary sync tools, etc. — no Odin Inspector required.
- **Odin Inspector Users**: Developers who already use Odin Inspector and want richer attribute decorators and enhanced Inspector experience.
- **Code Standards Advocates**: Developers who want their team to follow consistent code style and documentation standards, improving project maintainability.

## Installation

### Via Git URL

1. Open the Unity Package Manager window.
2. Click the `+` button in the top-left corner and select `Add package from git URL...`.
3. Enter the following URL:
   ```
   https://github.com/yuumixcode/Unity-Aesir-Packages.git
   ```

### Via manifest.json

Add the following to your project's `Packages/manifest.json`:

```json
{
  "dependencies": {
    "cn.runestone.aesir-inspector": "https://github.com/yuumixcode/Unity-Aesir-Packages.git"
  }
}
```

### Installation Mode Detection

Aesir Inspector automatically detects the installation mode (UPM / Assets directory) at editor load time and exposes static properties through `AesirInspectorInstallationChecker`:

- `InstallMode`: Current installation mode (`Upm` / `AssetFolder` / `Unknown`).
- `IsUpm`: Whether installed via UPM.
- `IsAssetFolder`: Whether installed in the Assets directory (Asset Store import or Git submodule).

## Requirements

- **Unity**: 2022.3.2t3 (Tuanjie) or later.
- **Odin Inspector**: 3.3.x or later (optional dependency; importing it automatically adds the `ODIN_INSPECTOR` compilation symbol, enabling Odin Integration enhancement assemblies).

## Core Features

> **📌 Note**: Features marked with ⚡ require Odin Inspector to be installed.

### 1. Attribute Overview Pro ⚡

Displays all registered Odin Inspector and Aesir Inspector attribute panels in a searchable tree menu, with live preview and example code for each attribute.

- **Categorized Browsing**: Browse attributes by categories such as Essentials / Buttons / Collections / Groups / Conditionals.
- **Search & Locate**: Supports fuzzy search to quickly find target attributes.
- **Live Preview**: Select an attribute to view its effect and parameter configuration in the right panel.
- **Code Preview**: Select an attribute to view the corresponding example source code for quick reference.
- Open via `Tools → Aesir → Inspector → Attribute Overview Pro` menu.

### 2. Script Doc Generator ⚡

Generates structured API documentation by analyzing C# type information via reflection, with support for incremental generation and customization.

#### Use Cases

When your team, open-source project, or personal project needs API documentation — not just auto-generated type signatures, but also the ability to add personalized notes (usage examples, caveats, business context) — Script Doc Generator is designed for this. It generates the accurate API signature portion, and you add the content that only humans can write. The two never interfere with each other, and updates are incremental.

#### Key Advantages

- **🔒 Fully Offline**: Runs on C# reflection — no network, no external APIs, no third-party services. Offline environments, intranets, confidential projects — available anywhere.
- **⚡ Zero Wait**: Reflection analysis completes in milliseconds. Select a type, and the document appears. No progress bars, no waiting.
- **🎮 Integrated with Unity**: Runs as a native Editor extension, operated directly in the Inspector. No window switching, no external toolchain — documentation is right where you write code.
- **✏️ Incremental Generation**: When regenerating documentation, handwritten content after the `## Additional Notes` marker is automatically preserved. Existing Front Matter (YAML/TOML headers) is also preserved. Auto-generated signatures and manual notes never overwrite each other.
- **🤖 AI-Friendly**: Generates Markdown documents by default, with clear structure and semantics. Can be directly used to build AI API Q&A knowledge bases (RAG, Embedding, etc.), enabling AI assistants to accurately answer project API questions.
- **🔧 Configurable & Extensible**: Provides multiple configuration options and extension interfaces to adapt to different project documentation needs (see below).

#### Feature Details

- **Type Analysis**: Supports full signature and attribute parsing for classes, structs, interfaces, enums, delegates, records, etc., including generic constraints, base class inheritance, and interface implementations.
- **Field Analysis**: Covers all C# primitive types, collection types, delegate types, special types (abstract/dynamic/interface/nullable), const/static default values, access modifiers, compound keywords (const/static readonly/readonly), Unity built-in types, and attribute annotations.
- **Property Analysis**: Supports asymmetric getter/setter access modifiers (e.g., `public get / private set`), static properties, and default value initialization.
- **Method Analysis**: Supports generic methods, parameter default values, `params` variable arguments, `async` methods, operator overloading, and extension methods.
- **Inheritance Analysis**: Identifies virtual/abstract/override methods and interface implementations, tracing inheritance chain origins.
- **Auxiliary Features**: Member sorting (`DerivedMemberDataComparer`), method overload markers, constructor signature generation, event signature generation.

#### Configuration Options

| Option | Description |
|--------|-------------|
| Document Output Path | Custom target folder for document generation, supports drag-and-drop |
| Generate Folders by Namespace | When enabled, automatically creates subdirectories by type namespace, e.g., `Runestone.AesirInspector` → `Runestone/AesirInspector/` |
| Custom Document Extension | Defaults to `.md`; can be switched to `.mdx`, `.txt`, or any extension |
| Incremental Generation Marker | When enabled, automatically inserts an `## Additional Notes` section at the end of the document; content after this marker is preserved on regeneration |
| Type Source Mode | Single type / multiple types / entire assembly — three granularity levels to choose from |
| TypesCacheSO | Saves Type lists as reusable asset files, avoiding re-selection each time |

#### Extension Interfaces

| Interface | Description |
|-----------|-------------|
| `DocGeneratorSettingsSO` | Inherit this abstract class and implement `GetGeneratedDoc(ITypeData)` to customize document format and content. Includes `CnScriptingAPISettingsSO` (Chinese API Markdown document generator) as a reference implementation |
| `IAnalysisDataFactory` | Replace the entire type analysis factory to customize member data parsing logic |
| `IAttributeFilter` | Custom attribute filter controlling which attributes appear in generated documentation |

#### Unit Test Coverage

Script Doc Generator currently includes **153 unit tests** covering signature generation for various data types. Test cases are continuously expanded with feature iterations:

| Test Module | Tests | Description |
|-------------|-------|-------------|
| **FieldData** · Signatures | 41 | Signature generation for primitive types, collection types, delegate types, special types (abstract/dynamic/interface/nullable) |
| **FieldData** · Default Values | 32 | Default value generation for const and static fields, including decimal edge cases |
| **FieldData** · Modifiers | 10 | Compound keywords (const/static readonly/readonly) and all 6 access modifiers |
| **FieldData** · Unity | 7 | Unity built-in types (GameObject/Transform/Rigidbody, etc.) and attribute annotations (SerializeField/Range/ColorUsage/Obsolete) |
| **PropertyData** | 13 | Static property default values, asymmetric getter/setter access modifier combinations |
| **MethodData** · General | 11 | Generic methods, default parameters, params variable arguments, async methods, static methods |
| **MethodData** · Inheritance | 5 | virtual/abstract/override methods and interface implementation inheritance analysis |
| **MethodData** · Operators | 8 | Arithmetic operator overloading, implicit/explicit type conversion operators |
| **MethodData** · Extensions | 1 | Extension method signature generation and `[Ext]` marker |
| **ConstructorData** | 1 | Constructor signature generation, including base class constructor calls |
| **EventData** | 6 | Action/Func/Predicate/Comparison delegate type events and static events |
| **TypeData** | 14 | class/struct/interface/enum/delegate/record/static/sealed/generic type declarations, including attributes and generic constraints |
| **MemberData** · Inheritance | 4 | `IsFromInheritance` markers for fields/properties/events/methods inherited from base classes |

### 3. Summary Tool

Provides right-click menu shortcuts for bidirectional synchronization between XML `<summary>` comments and `[Summary]` attributes in C# scripts.

#### Use Cases

When your team requires public members to have both XML documentation comments and `[Summary]` attributes — manually maintaining two copies of identical content in different formats is tedious and error-prone. Summary Tool is designed for this. It extracts summaries from XML comments and automatically generates corresponding `[Summary]` attributes, keeping both always in sync.

#### Key Advantages

- **⚡ Right-Click to Use**: Select scripts in the Project window, right-click to execute — no extra windows needed.
- **🔄 Three Modes**: Sync, Replace, Remove — covering all daily maintenance needs.
- **📦 Batch Processing**: Supports multi-selecting scripts for simultaneous processing, bulk syncing or cleanup.
- **🧠 Smart Imports**: Automatically adds `using Runestone.AesirInspector;` after processing — no manual using statements.
- **🏗️ Macro-Aware**: Automatically recognizes `#if` and other preprocessor directives, ensuring `[Summary]` attributes are inserted inside conditional compilation blocks.

#### How It Works

The `XmlSummaryTool` processing pipeline has three stages: **Parse → Group → Output**.

**1. Parse Stage**: Scans source code line by line, locates the first `///` comment, marks all lines before it as **Header** (using, namespace, etc.), and sends the rest to the grouping stage.

**2. Group Stage**: Starting from the first `///`, alternately extracts **XML comment blocks** (consecutive `///` lines) and **code blocks** (non-`///` lines), producing an `XmlCodePart` list. Each `XmlCodePart` consists of `xml` (comment section) and `code` (code section).

**3. Output Stage**: Performs different operations on each `XmlCodePart` based on the selected mode:

| Mode | Output Composition | Description |
|------|--------------------|-------------|
| **Sync** | `xml` + `leading preprocessor` + `[Summary]` + `code after first [Summary] removed` | Preserves XML comments, adds/updates `[Summary]` after preprocessor directives; replaces existing `[Summary]` content with XML text |
| **Replace** | `xml with summary removed` + `leading preprocessor` + `[Summary]` + `code after first [Summary] removed` | Removes `<summary>` tags, replaces with `[Summary]` attribute; syncs existing `[Summary]` content with XML text |
| **Remove** | `xml` + `leading preprocessor` + `code with all [Summary] removed` | Only removes all `[Summary]` attributes, preserving XML comments |

**Macro Awareness**: When a code block starts with `#if`, `#elif`, `#else`, etc., the `[Summary]` attribute is inserted after these directives (i.e., inside the conditional compilation block), not before. For example:

```csharp
// Input
/// <summary>Editor method</summary>
#if UNITY_EDITOR
[Summary("Old content")]
public void Reset() { }
#endif

// Sync output — [Summary] is inside #if
/// <summary>Editor method</summary>
#if UNITY_EDITOR
[Summary("Editor method")]
public void Reset() { }
#endif
```

Finally, the output stage checks whether the Header already contains `using Runestone.AesirInspector;` and automatically adds it if missing.

### 4. Mini Tools ⚡

Integrates common editor utilities, accessible through the `Tools → Aesir → Inspector → Mini Tools` menu.

| Tool | Description |
|------|-------------|
| **MenuItem Viewer** | Collects and displays all `[MenuItem]` menu items in the project, supports assembly filtering and search for planning menu structure |
| **Syntax Highlighter** | Visual panel based on Odin's built-in syntax highlighter; input source code to test highlighting effects and output rich text markup |
| **Quick Create SO** | Right-click a MonoScript in the Project window to quickly generate a ScriptableObject asset file; supports multi-select batch creation |

### 5. Extension Package Manager ⚡

Quickly install recommended Aesir series and other popular open-source Unity Packages via Git URL.

- **One-Click Install/Remove**: Card-style UI displays installation status of recommended packages; click to install or remove.
- **Auto Detection**: Automatically checks installed package status when the window opens; real-time refresh after install/remove.
- Open via `Tools → Aesir → Inspector → Extension Package Manager` menu.

## Infrastructure

### 6. Bilingual Attributes ⚡

Provides a complete set of bilingual attribute decorators and Inspector Controls, supporting simultaneous display of Chinese and English information in the Inspector panel. Primarily designed for:

- **Editor Tool Development**: When developing other editor tools, you want the Inspector to support bilingual display so users of different language backgrounds can intuitively understand parameters and operations.
- **Team Collaboration**: For cross-region, cross-language teams sharing a project, bilingual display effectively reduces communication costs and prevents misoperations caused by language differences.

Available decorators and controls:

- `[BilingualTitle]`
- `[BilingualButton]`
- `[BilingualInfoBox]`
- `[BilingualText]`
- `BilingualDisplayAsStringControl` bilingual read-only text display control
- `BilingualHeaderControl` bilingual header control
- `HorizontalSeparateControl` horizontal separator control

### 7. OdinBridge

Provides an optional integration mechanism for Odin Inspector, allowing the core assembly to work without Odin while enabling Odin Integration assemblies to offer enhanced functionality when Odin is available:

| Class | Description |
|-------|-------------|
| `IOdinBridge` | Interface for querying Odin availability, defines `IsOdinPresent` and other capabilities |
| `DefaultOdinBridge` | Default bridge implementation when Odin is not present |
| `OdinBridgeLocator` | Automatically locates Odin bridge at runtime, falls back to `DefaultOdinBridge` when Odin is absent |
| `OdinInspectorBridge` | Editor-side enhanced bridge implementation when Odin is available |

### 8. Safe Editor Utilities

Provides safe wrappers for Unity Editor APIs, ensuring editor-only code is automatically stripped in builds.

| Utility | Description |
|---------|-------------|
| `ScriptableObjectSafeEditorUtility` | More reliable ScriptableObject asset creation and management |
| `MonoScriptSafeEditorUtility` | Find and select MonoScript assets by script name |
| `PathUtility` | Path string utilities: Unity path normalization, subpath extraction, path merging |
| `PathSafeEditorUtility` | Safe folder creation ensuring directories exist under Assets |
| `HierarchySafeEditorUtility` | Get a GameObject's absolute path in the Hierarchy |
| `HierarchyUtility` | Transform hierarchy path operations: full paths, relative paths, deep child lookups |
| `ProjectSafeEditorUtility` | Ping and select any project resource (supports folder paths) |
| `UrlUtility` | Convenient URL opening and external link handling |
| `ReflectionUtility` | Assembly and namespace reflection utilities |
| `PredefinedAssemblyUtility` | Predefined assembly type identification and interface implementation lookup |
| `PlayerLoopUtility` | Custom Unity PlayerLoop: insert, remove subsystems, print PlayerLoop structure |
| `RegexUtility` | Regex utilities: namespace/class name normalization, email/URL validation |
| `AesirInspectorLogger` | Unified logging with colored prefix, auto-stripped in builds, double-click to jump to caller; log levels configurable via `AesirInspectorLoggerSettings` |

### 9. Custom Attributes

| Attribute | Description |
|-----------|-------------|
| `[Summary]` | Comment attribute, equivalent to the `<summary>` portion of XML comments; summary text can be retrieved at runtime via `GetSummary()` |

### 10. Code Style & Standards

This project treats code style as equally important as functionality. Built-in strict coding standards and examples ensure consistency and maintainability in team collaboration:

- **Style Guide**: See `Runtime/CodeStyle/AESIR_INSPECTOR_CODE_STYLE.cs` for details.
- **Design Philosophy**: Good code style is not optional — it is the foundation of project quality. All contributors must follow these standards.
- **Design Philosophy**: Good code style is not optional — it is the foundation of project quality. All contributors must follow these standards.

## Usage Example

```csharp
using Runestone.AesirInspector;
using Sirenix.OdinInspector;
using UnityEngine;

public class ExampleMonoBehaviour : MonoBehaviour
{
    [BilingualTitle("玩家属性", "Player Stats")]
    [SerializeField]
    private int health;

    [BilingualButton("重置属性", "Reset Stats")]
    private void ResetStats()
    {
        health = 100;
    }
}
```

## License

This project is licensed under the MIT License. See [LICENSE.md](LICENSE.md) for details.
