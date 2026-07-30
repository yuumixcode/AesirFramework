# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

[中文](../../CHANGELOG.md)

---

## [Unreleased]

## [0.4.0] - 2026-07-24

### ⚠ BREAKING CHANGES (Read before upgrading)

> **Brand namespace unification**: All `RunLab` references renamed to `Runestone` to align with Aesir Architecture / Aesir Modules.
> All `RunLab.*` namespaces, `cn.runlab.aesir-inspector` package ID, and 9 asmdef files renamed to `Runestone.*` / `cn.runestone.aesir.inspector`.
> After upgrading, **all code using this package must replace `using RunLab.*` → `using Runestone.*`** in bulk.

#### Migration Guide

| Scope | Before | After |
|---|---|---|
| Package ID | `cn.runlab.aesir-inspector` | `cn.runestone.aesir.inspector` |
| Namespace | `RunLab.AesirInspector` | `Runestone.AesirInspector` |
| Namespace | `RunLab.AesirInspector.Editor` | `Runestone.AesirInspector.Editor` |
| Namespace | `RunLab.AesirInspector.Tests` | `Runestone.AesirInspector.Tests` |
| Namespace | `RunLab.AesirInspector.Editor.Tests` | `Runestone.AesirInspector.Editor.Tests` |
| Namespace | `RunLab.AesirInspector.OdinIntegration` | `Runestone.AesirInspector.OdinIntegration` |
| Namespace | `RunLab.AesirInspector.OdinIntegration.Editor` | `Runestone.AesirInspector.OdinIntegration.Editor` |
| Namespace | `RunLab.AesirInspector.Samples.*` | `Runestone.AesirInspector.Samples.*` |
| Assembly name | `RunLab.AesirInspector` (and all variants) | `Runestone.AesirInspector` (and all variants) |
| Copyright | `Copyright (c) 2026 RunLab - Yuumix` | `Copyright (c) 2026 Runestone - Yuumix` |

#### Code-side replace examples

```csharp
// Before
using RunLab.AesirInspector;
using RunLab.AesirInspector.Editor;
using RunLab.AesirInspector.OdinIntegration;

// After
using Runestone.AesirInspector;
using Runestone.AesirInspector.Editor;
using Runestone.AesirInspector.OdinIntegration;
```

```jsonc
// asmdef references Before
"references": [
  "RunLab.AesirInspector",
  "RunLab.AesirInspector.Editor"
]

// After
"references": [
  "Runestone.AesirInspector",
  "Runestone.AesirInspector.Editor"
]
```

#### Scope
- 422 .cs files / 12 asmdef + 12 asmdef.meta / 1 package.json / 1 LICENSE.md / multiple README/CHANGELOG/CONTRIBUTING

### Changed
- Renamed `OdinWrapper` to `Odin Integration` (directories) / `OdinIntegration` (namespaces and assemblies) for more accurate semantic representation of the integration layer
- Renamed `Runtime/Unity/Bilingualism/` to `Runtime/Unity/Localization/`, aligning with Unity's official Localization package naming
- Renamed `Runtime/Unity/InspectorControls/` to `Runtime/Unity/Inspector/`, adopting Unity's singular noun convention
- Renamed `Runtime/Unity/Logger/` to `Runtime/Unity/Logging/`, aligning with Unity source `Runtime/Export/Logging/` naming

---

## [0.4.0-pre.1] - 2026-04-29

### Architecture

#### Added
- Added `OdinWrapper` independent assembly with Runtime (`Runestone.AesirInspector.OdinWrapper`) and Editor (`Runestone.AesirInspector.OdinWrapper.Editor`) asmdef files, both with `defineConstraints: ODIN_INSPECTOR`, fully isolating Odin Inspector dependency from the core assembly `473640f`

#### Changed
- Removed `ODIN_INSPECTOR` define constraint from core Runtime assembly `Runestone.AesirInspector`, eliminating hard dependency on Odin Inspector `473640f`
- Adjusted Editor assembly `Runestone.AesirInspector.Editor` references to remove direct Odin dependency `473640f`

### OdinBridge

#### Added
- Added `IOdinBridge` interface defining `IsOdinPresent` and other Odin availability queries `473640f`
- Added `DefaultOdinBridge` as fallback implementation when Odin is not present `473640f`
- Added `OdinBridgeLocator` for automatic Odin bridge discovery with default fallback `473640f`
- Added `OdinInspectorBridge` (OdinWrapper/Editor/Bridge/) as editor-side bridge implementation when Odin is available `473640f`

### OdinWrapper

#### Added
- Added `OdinWrapper/Editor/AttributeProcessors/` directory with 5 OdinAttributeProcessors: `AesirInspectorLanguageSettingsProcessor`, `AesirInspectorResetProcessor`, `BilingualDisplayAsStringProcessor`, `BilingualHeaderProcessor`, `HorizontalSeparateProcessor` `473640f`

#### Changed
- Moved `Editor/AttributeOverviewPro/` to `OdinWrapper/Editor/AttributeOverviewPro/` `473640f`
- Moved `Editor/Drawers/Bilingual/` to `OdinWrapper/Editor/Drawers/` `473640f`
- Moved `Editor/ExtensionManager/` to `OdinWrapper/Editor/ExtensionManager/` `473640f`
- Moved `Editor/MiniTools/` to `OdinWrapper/Editor/MiniTools/` `473640f`
- Moved `Editor/ScriptDocGenerator/` to `OdinWrapper/Editor/ScriptDocGenerator/` `473640f`
- Moved `Editor/Core/Windows/` to `OdinWrapper/Editor/Windows/` `473640f`
- Moved 6 Bilingual attributes from `Runtime/Bilingual/Attributes/` to `OdinWrapper/Runtime/Attributes/` `473640f`
- Moved `Editor/Core/AesirCodeHighlighter.cs` to `OdinWrapper/Runtime/OdinCodeHighlighter.cs` `473640f`
- Renamed `OdinSyntaxHighlighterSO` to `OdinSyntaxHighlighterPanelSO` `473640f`

### Bilingualism

#### Changed
- Renamed `Runtime/Bilingual/` to `Runtime/Bilingualism/` `473640f`
- Simplified `AesirInspectorLanguageSettingsSO`, removed Odin dependency logic, now handled by `AesirInspectorLanguageSettingsProcessor` `473640f`

#### Removed
- Removed `DisplayAsStringBilingualConfigAttribute`, replaced by `BilingualDisplayAsStringControl` + Processor `473640f`
- Removed `ShowIfChineseAttribute` and `ShowIfEnglishAttribute`, replaced by Processor `473640f`
- Removed `DisplayAsStringBilingualWidget` and `HeaderBilingualWidget`, replaced by corresponding Controls `473640f`

### InspectorControls

#### Added
- Added `BilingualDisplayAsStringControl`, replacing `DisplayAsStringBilingualWidget` `473640f`
- Added `BilingualHeaderControl`, replacing `HeaderBilingualWidget` `473640f`

#### Changed
- Renamed `Runtime/InspectorWidgets/` to `Runtime/InspectorControls/`, unified Widget naming to Control `473640f`
- Renamed `HorizontalSeparateWidget` to `HorizontalSeparateControl` `473640f`

### Core

#### Changed
- Simplified `IAesirInspectorReset` interface, reset logic moved to `AesirInspectorResetProcessor` `473640f`
- Moved `AesirInspectorLogger` from `Runtime/Core/` to `Runtime/Logger/` `473640f`
- Moved `AesirInspectorLoggerSettings` from `Runtime/Core/` to `Runtime/Logger/` `473640f`
- Moved `SummaryAttribute` from `Runtime/Attributes/Docs/` to `Runtime/Attributes/`, flattened directory `473640f`

#### Removed
- Removed `ShowEnablePropertyAttribute` deprecated attribute `473640f`

### Utilities

#### Changed
- Significantly enhanced `ReflectionUtility` with additional reflection utility methods `473640f`

#### Removed
- Removed `OdinInspectorSafeEditorUtility`, replaced by OdinBridge pattern `473640f`

### ScriptDocGenerator

#### Changed
- Removed Odin attribute dependencies from all AnalysisData classes (ConstructorData, EventData, FieldData, MemberData, MethodData, ParameterData, ParameterDirection, PropertyData, TypeData) `473640f`

### Samples

#### Changed
- Moved `Samples~/` to `Samples/` (Plugin Config Solutions, RuntimeInitializeLoadType), making samples visible to users `473640f`

#### Removed
- Removed Codely Skills Library sample (custom-package-creator) `473640f`

### Tests

#### Changed
- Removed `ODIN_INSPECTOR` define constraint from `Runestone.AesirInspector.Tests` asmdef `473640f`
- Adjusted `Runestone.AesirInspector.Editor.Tests` asmdef assembly references `473640f`
- Reformatted multiple test files with region reordering and removed unused using directives `473640f`

### Code Style

#### Changed
- Updated `AESIR_INSPECTOR_CODE_STYLE.cs` code style guide to align with new assembly architecture and naming conventions `473640f`

---

## [0.3.1] - 2026-04-27

### Core

#### Added
- Added `AesirInspectorLoggerSettings` ScriptableObject for log level configuration with `enableInfoLog` (default false) and `enableWarningLog` (default true) `45a4837`

#### Changed
- Moved `AesirInspectorLogger` from Utilities to Core directory, integrated LoggerSettings switch check in Info/Warning methods, removed `MethodImpl` attribute `45a4837`
- Renamed `AesirInspectorWebLinks.GitWebsite` to `GitUrl`, changed `OdinInspectorDocsUrl` link from documentation to tutorials `45a4837`
- Changed `IAesirInspectorReset` context menu label from "Aesir Toolkit Reset" to "Aesir Inspector Reset" `45a4837`
- Restructured `AesirInspectorMenuItems` menu paths: split `ToolsMenuRoot` into `ToolsAesirRoot` (Tools/Aesir) and `ToolsAesirInspectorRoot` (Tools/Aesir/Inspector), added priority constants for all menu items `cf6126c`
- Removed `#if UNITY_EDITOR && ODIN_INSPECTOR_3_3` guard from `AesirCodeHighlighter`, moved using statements outside namespace `cf6126c`

#### Removed
- Removed `#if ODIN_INSPECTOR_3_3` preprocessor guards across the entire project, making Odin Inspector a hard dependency `cf6126c`

### Bilingual

#### Changed
- Renamed `AesirInspectorLanguageSettings` to `AesirInspectorLanguageSettingsSO` following ScriptableObject naming convention `cf6126c`
- Renamed `DisplayAsStringBilingualWidgetConfigAttribute` to `DisplayAsStringBilingualConfigAttribute`, removed Widget infix `cf6126c`
- Moved `BilingualData` from `Runtime/Bilingual/Attributes/` to `Runtime/Bilingual/` `cf6126c`
- Marked `_chineseIntroduction` and `_englishIntroduction` fields as readonly in `HeaderBilingualWidget`, changed conditional compilation from `#if ODIN_INSPECTOR_3_3` to `#if UNITY_EDITOR` `45a4837` `cf6126c`
- Removed `#region Internal` from `BilingualBoxGroupAttribute` and `BilingualButtonAttribute` `cf6126c`
- Moved `TitleAlignment` property out of `#if ODIN_INSPECTOR_3_3` guard in `BilingualTitleGroupAttribute` `cf6126c`

#### Removed
- Removed `#if ODIN_INSPECTOR_3_3` guards from all Bilingual attributes and drawers `cf6126c`

### AttributeOverviewPro

#### Changed
- Renamed entire `Editor/AttributeOverview/` directory to `Editor/AttributeOverviewPro/` `cf6126c`
- Moved `AttributeExamplePreviewItem`, `ParameterValue`, `ResolvedStringParameterValue` from `Data/` to `Core/` subdirectory `cf6126c`
- Renamed `AssetListExampleForCustomFilterMethodSO` to `AssetListExampleWithCustomFilterMethodSO` `cf6126c`

### Utilities

#### Changed
- Replaced `new T[0]` with `Array.Empty<T>()` and `new Type[1]` with `new[]` in `OdinInspectorSafeEditorUtility` `45a4837`
- Added `[Conditional("UNITY_EDITOR")]` to `PathSafeEditorUtility.EnsureDirectoryExists` `45a4837`

#### Removed
- Removed `#region Public Methods` and `#region` patterns from all utility classes `45a4837`

### MiniTools

#### Changed
- Renamed `AssemblyFilterExample` to `FilterOutAesirInspectorAssembly` `cf6126c`

#### Removed
- Removed `#if ODIN_INSPECTOR_3_3` guards from MiniTools module `cf6126c`

### ScriptDocGenerator

#### Changed
- Moved Odin attributes before XML comments in all AnalysisData classes `cf6126c`

#### Removed
- Removed `#if ODIN_INSPECTOR_3_3` guards from ScriptDocGenerator module `cf6126c`

### Code Style

#### Changed
- Marked `_darkLineHeight`, `_lightLineHeight`, `_spaceAfter`, `_spaceBefore` as readonly and `DarkLineColor`, `LightLineColor` as static in `HorizontalSeparateWidget` `cf6126c`

#### Removed
- Removed `#region Internal` pattern, updated code style guide and example code `45a4837` `cf6126c`

### Samples

#### Changed
- Renamed PluginConfig sample directory `58fdbce`

### Docs

#### Added
- Added `ATTRIBUTE_OVERVIEW_PRO_GUIDE.md` coding guide for AttributeOverviewPro module covering Data-Panel-Example trio, singleton SO pattern, OdinAttributeProcessor injection, GUITable caching, bilingual system, naming conventions `cf6126c`
- Added `SCRIPT_DOC_GENERATOR_GUIDE.md` coding standards for ScriptDocGenerator module covering architecture layering, singleton, reset, event communication, file output `cf6126c`
- Added `UTILITIES_GUIDE.md` coding guide for Utilities module `45a4837`

#### Changed
- Simplified `AESIR_INSPECTOR_CODE_STYLE_GUIDE.md` by removing #region Internal rules and simplifying Odin Inspector integration guidelines `45a4837` `cf6126c`

---

## [0.3.0] - 2026-04-25

### Core

#### Added
- Added `AesirInspectorMenuItems` unified menu path and priority management class for Tools menu and Assets context menu `77f3b1b`
- Added Getting Started window with version display, feature list, and documentation links `77f3b1b`
- Added Preferences window with integrated language settings `77f3b1b`
- Added `AesirInspectorVersion` version info static class `77f3b1b`
- Added `IAesirInspectorReset` interface and `AesirInspectorResetAttributeProcessor` for auto-adding context menu reset entry `77f3b1b`
- Added `AesirCodeHighlighter` code syntax highlighter `77f3b1b`

#### Changed
- Silenced installation detection log output (commented out `Debug.Log`) `77f3b1b`
- Extended `AesirInspectorPaths` with AttributeOverview and MiniTools path constants `77f3b1b`
- Extended `AesirInspectorWebLinks` with GitHub repository, license, changelog, and Odin Inspector docs links `77f3b1b`

### Bilingual

#### Added
- Added `ShowEnablePropertyAttribute` composite attribute `2ac8573`
- Added `HorizontalSeparateWidget` horizontal separator Inspector component `2ac8573`

#### Changed
- Refactored `HeaderBilingualWidget` `2ac8573`

### Utilities

#### Added
- Added `AesirInspectorLogger` logging utility class `2ac8573`
- Added `PathUtility` and `PathSafeEditorUtility` path utility classes `2ac8573`
- Added `ReflectionUtility` reflection utility class `2ac8573`
- Added `RegexUtility` regular expression utility class `2ac8573`
- Added `HierarchyUtility` and `HierarchySafeEditorUtility` Hierarchy utility classes `2ac8573`
- Added `MonoScriptSafeEditorUtility` MonoScript utility class `2ac8573`
- Added `PlayerLoopUtility` PlayerLoop utility class `2ac8573`
- Added `PredefinedAssemblyUtility` predefined assembly utility class `2ac8573`
- Added `ProjectSafeEditorUtility` project-safe editor utility class `2ac8573`

#### Changed
- Extended `ScriptableObjectSafeEditorUtility` with additional ScriptableObject editor operation methods `2ac8573`
- Extended `OdinInspectorSafeEditorUtility` and `UrlUtility` `2ac8573`

### MiniTools

#### Added
- Added `AesirInspectorMiniToolsWindow` main window `b7068eb`
- Added MenuItemViewer with `IAssemblyFilter` assembly filtering and `ISearchFilterable` search support `b7068eb`
- Added OdinSyntaxHighlighter panel delegating to `AesirCodeHighlighter` `b7068eb`
- Added QuickCreateSO context menu tool for quick ScriptableObject generation with single and multi-selection support `b7068eb`

### ScriptDocGenerator

#### Added
- Added document generator window and visual panel ScriptableObject singleton `c2f2e75`
- Added `ScriptDocGeneratorController` logic controller for type analysis and document generation `c2f2e75`
- Added Assets context menu items for adding scripts to TargetType or TemporaryTypes `c2f2e75`
- Added Chinese Scripting API configuration and document generator settings `c2f2e75`
- Added complete type analysis data model layer: `MemberData`, `FieldData`, `PropertyData`, `MethodData`, `ConstructorData`, `EventData`, `ParameterData`, `TypeData` and corresponding interfaces `c2f2e75`
- Added `TypeAnalyzerStaticExtensions` and `TypeAnalyzerUtility` type analyzer utilities `c2f2e75`
- Added `AccessModifierType`, `TypeCategory`, `ParameterDirection` enumerations `c2f2e75`
- Added `DefaultAnalysisDataFactory`, `DefaultAttributeFilter`, `DerivedMemberDataComparer` core utilities `c2f2e75`
- Added `ReferenceLinkURLAttribute` reference link attribute `c2f2e75`

### AttributeOverview

#### Added
- Added `AttributeOverviewWindow` and `AttributeOverviewDatabaseSO` for attribute overview management `0e53a40`
- Added panel abstract framework: generic base `AttributeOverviewPanelSO<T>`, `AbstractAttributePanelSO` with Odin AttributeProcessor auto-configuration `0e53a40`
- Added AssetList, AssetsOnly, CustomValueDrawer built-in attribute panels `0e53a40`
- Added `AesirExampleAttribute` and `AttributeCategoryAttribute` attribute markers `0e53a40`
- Added attribute data models: `AbstractAttributeData`, `ParameterValue`, `ResolvedStringParameterValue`, `AttributeExamplePreviewItem` `0e53a40`
- Added `AesirAttributeCategory` category enumeration and `OdinInspectorDocumentationLinks` documentation link constants `0e53a40`
- Added attribute overview editor utility and usage examples `0e53a40`

### SummaryTool

#### Added
- Added `XmlSummaryTool` for XML Summary comment processing with Sync/Replace/Remove operations `2ac8573`
- Added `XmlCodePart` XML code part parser `2ac8573`
- Added SummaryTool Assets context menu items `2ac8573`

### ExtensionManager

#### Added
- Added `ExtensionPackageManagerWindow` with Git URL installation support `2ac8573`
- Added `ExtensionPackageCard` extension package card data class `2ac8573`
- Added `PackageManagerEditorUtility` Package Manager editor utility class `2ac8573`

### Samples

#### Added
- Added PluginConfigSolutions sample demonstrating ScriptableSingleton usage in Preferences and Project `2ac8573`
- Added RuntimeInitializeLoadType sample demonstrating five initialization timings and best practices `2ac8573`

### Tests

#### Added
- Added ScriptDocGenerator comprehensive unit tests covering constructors, events, fields, methods, properties, type data, and member inheritance `1cf6d6d`
- Added SummaryTool XML comment processing tests `1cf6d6d`
- Added UnityEngine.Object operator overload Runtime tests `1cf6d6d`

#### Changed
- Added `ODIN_INSPECTOR` define constraint to both test asmdef files `1cf6d6d`

---

## [0.2.1] - 2026-04-23

### Added

- Added Aesir Inspector installation detection feature `b7de538`

---

## [0.2.0] - 2026-04-23

### Added

- Implemented bilingual Inspector system and core infrastructure `a2c750b`
- Added Codely Skills Library sample, including custom-package-creator skill `9422695`

---

## [0.1.0] - 2026-04-22

### Added

- Initial release.
