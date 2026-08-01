# OdinAutoTooltip

**Applies Unity `TooltipAttribute` automatically to Odin inspector members from XML `<summary>` doc comments in source (or member name as fallback).** Write the summary once and IDE documentation and inspector tooltips stay in sync — no need to duplicate text in `[Tooltip("...")]`. When inspector text should differ, add an explicit `[Tooltip("...")]`.

_Requires **Odin Inspector** (Sirenix). Unity 2021+ / C# 9+._

---

## How it works

For each inspector member, if there is no existing `TooltipAttribute` or Odin `PropertyTooltipAttribute`, the processor applies a tooltip from the member's `/// <summary>` in source (when `USE_SUMMARIES` is on), or from the **member name** as a fallback (when `USE_SUMMARIES` is on or off and `USE_MEMBER_NAME` is on). Any member that already has `TooltipAttribute` or `PropertyTooltipAttribute` is left alone — including `[Tooltip(null)]`, which opts out of auto-generation and shows no hover text.

Source parsing looks backward from the member declaration for a `/// <summary>` block. Attributes, preprocessor directives, and `//` comments may appear between the summary and the member (`summary → attributes → member` and `attributes → summary → member` both work).

### From XML summary

```csharp
/// <summary>How fast the unit moves in world units per second.</summary>
public float moveSpeed = 5f;
```
<img width="492" height="66" alt="image" src="https://github.com/user-attachments/assets/140ab36a-3d41-47a9-bd2f-710efcda77ce" />


### From member name (no summary)

When there is no summary, the tooltip falls back to the field name. `[LabelText]` changes the inspector label only — the tooltip still shows the real member name, which is useful when the label is friendlier than the identifier:

```csharp
[LabelText("Move Speed")]
public float playerMoveSpeed = 5f;
```
<img width="493" height="46" alt="image" src="https://github.com/user-attachments/assets/cf6b3666-d33d-4390-b53b-5dad426f5653" />

---

## Installation

Part of [JakePineOdinTools](../README.md). Requires the sibling **OdinSource** folder (shared source parsing).

```
../OdinSource/Editor/OdinSourceFileHelper.cs      ← required shared utilities
Editor/OdinAutoTooltipAttributeProcessor.cs       ← core attribute processor
Runtime/OdinAutoTooltipSample.cs                  ← demo component (optional)
```

No other setup needed. The processor registers itself via Odin's `OdinAttributeProcessor<object>`.

---

## Basic usage

Write XML summaries above fields, properties, and methods. They appear as inspector tooltips automatically.

```csharp
/// <summary>How fast the unit moves in world units per second.</summary>
public float moveSpeed = 5f;

/// <summary>When enabled, the object loops its animation.</summary>
public bool loop;

/// <summary>Resets all runtime state to defaults.</summary>
[Button]
private void ResetState()
{
}
```
<img width="489" height="90" alt="image" src="https://github.com/user-attachments/assets/22f36e72-7845-4ff9-a3ab-8c3daf6e8461" />

<img width="487" height="53" alt="image" src="https://github.com/user-attachments/assets/f3a1519b-ee16-43a9-a001-8a4ffa4d6b13" />

---

## Examples

### Multi-line summary

```csharp
/// <summary>
/// Maximum health for this unit. Clamped at runtime by
/// <see cref="minHealth"/> and upgrade modifiers.
/// </summary>
public int maxHealth = 100;
```
<img width="492" height="64" alt="image" src="https://github.com/user-attachments/assets/eac1cb84-5333-4602-be69-c4b3facdea36" />

`<see cref="TypeName"/>` tags are stripped to the short type name (`minHealth` in the example above). Other XML tags are removed; text is collapsed to a single line.

### Overriding auto-tooltips

Members that already have `TooltipAttribute` or `PropertyTooltipAttribute` are skipped — the processor does not replace them.

**Different inspector text** — keep the summary for IDE docs, override what the inspector shows:

```csharp
/// <summary>This summary is ignored for the tooltip.</summary>
[Tooltip("Shown in the inspector instead.")]
public float damage = 10f;
```
<img width="486" height="43" alt="image" src="https://github.com/user-attachments/assets/82ca0b0d-2c17-4ee8-b511-6b6441986fed" /><br>

**No inspector tooltip** — keep the summary for IDE docs, suppress auto tooltip generation in the inspector:

```csharp
/// <summary>Internal tuning value; documented in code only.</summary>
[Tooltip(null)]
public float hiddenTuning = 1f;
```
<img width="443" height="48" alt="image" src="https://github.com/user-attachments/assets/a2562201-ca8f-4a21-bf3f-4401d12c80b9" />


### Summary and attributes (either order)

The summary must appear somewhere in the preamble before the member — above attributes, below attributes, or between attribute lines. Preprocessor directives and `//` comments in the preamble are also skipped. If multiple summaries appear in the preamble, the one closest to the member wins.

```csharp
/// <summary>Spawn offset from the anchor transform.</summary>
[FoldoutGroup("Spawn")]
[Range(0f, 10f)]
public Vector3 spawnOffset;

[FoldoutGroup("Stats")]
/// <summary>Current health points.</summary>
public int health;
```
<img width="488" height="70" alt="image" src="https://github.com/user-attachments/assets/a099502b-51bd-4d13-9fbd-df56608dc8ec" />

<img width="486" height="73" alt="image" src="https://github.com/user-attachments/assets/1599f180-b550-4a5c-9db5-d7adc468b869" />


### Nested types

Summaries are parsed within the body of the declaring nested type, so inner classes do not pick up summaries meant for outer members. Each nested type's fields get tooltips from their own summaries (or member-name fallback).

```csharp
public class Outer
{
    /// <summary>Tooltip for outerField inside Outer.</summary>
    public int outerField;

    public Inner inner = new Inner();

    public class Inner
    {
        /// <summary>Inner field tooltip.</summary>
        public int innerField;
    }
}
```
<img width="481" height="132" alt="image" src="https://github.com/user-attachments/assets/0b6e9933-6732-4a08-b6a9-c5249d8065f2" />


---

## Limitations

### Class summaries do not propagate to fields of that type

A `/// <summary>` on a **class, struct, or enum** documents that type in the IDE. It does **not** become the tooltip when another class declares a **field or property of that type**.

```csharp
public class OdinAutoTooltipSample : MonoBehaviour
{
    public Outer nestedOuter = new Outer();
}

/// <summary>
/// The summary on a class does not auto generate any tooltips.
/// </summary>
[System.Serializable]
public class Outer
{
    public int outerField;
}
```

Hovering `nestedOuter` uses member-name fallback (`nestedOuter`), not the `Outer` class summary. Expanding the foldout still applies auto-tooltips to `outerField` and `innerField` from their field summaries; `inner` falls back to member name `inner`.

### Enum fields vs enum values

Does not work with enum members.  Must explcitly put `[Tooltip]` on each enum member, even if same text as summary.

Summaries on a **field** of enum type work as usual — the tooltip applies to the field in the inspector, not to each entry in the dropdown.

```csharp
public enum Mode
{
    /// <summary>Play forward from start to end.</summary>
    [Tooltip("Play forward from start to end.")]
    Forward,

    /// <summary>Play backward from end to start.</summary>
    [Tooltip("Play backward from end to start.")]
    Reverse,
}

public class PlaybackSettings
{
    /// <summary>Playback direction for this clip.</summary>
    public Mode mode;
}
```
<img width="489" height="98" alt="image" src="https://github.com/user-attachments/assets/eed194c1-28f1-413d-aecd-fa41aeca739c" />

Auto-tooltip from XML on enum members would require a separate feature (for example, a custom enum drawer that reads summaries from source at edit time). That is not part of this plugin today.

---

## Configuration

Edit the static fields at the top of `OdinAutoTooltipAttributeProcessor.cs`. `ENABLED` is a compile-time `readonly` switch; the others can be assigned at runtime:

| Option | Default | Meaning |
|---|---|---|
| `ENABLED` | `true` | Master switch (compile-time `readonly`). When false, the processor does nothing — zero per-repaint overhead. |
| `USE_SUMMARIES` | `true` | Apply tooltips from `/// <summary>` doc comments in source. |
| `USE_MEMBER_NAME` | `true` | When no summary is available, use the member name as the tooltip. When false, members without a summary get no auto-tooltip. |

Examples: summaries only (no name fallback) — `USE_SUMMARIES = true`, `USE_MEMBER_NAME = false`. Member names only (no source parsing) — `USE_SUMMARIES = false`, `USE_MEMBER_NAME = true`.

```csharp
// Change the defaults at the top of OdinAutoTooltipAttributeProcessor.cs:
OdinAutoTooltipAttributeProcessor.ENABLED = false;           // off entirely (compile-time readonly)
OdinAutoTooltipAttributeProcessor.USE_SUMMARIES = true;      // XML summaries
OdinAutoTooltipAttributeProcessor.USE_MEMBER_NAME = false;   // no name fallback
```

### Project-specific skip rules

The processor exposes an optional hook for game-project rules that should not live in the plugin:

```csharp
OdinAutoTooltipAttributeProcessor.ShouldSkipMember = (property, member, attributes) =>
{
    // return true to skip auto-tooltip for this member
    return false;
};
```

Register the delegate from `[InitializeOnLoad]` or a static constructor in your own editor assembly. Return `true` when a tooltip would conflict with custom drawer layout (for example, fields drawn inline in a foldout header with fixed pixel widths).

---

## Performance

All work is **editor-only** and **on-demand**. Nothing runs for types your project never inspects.

### When work happens

Odin calls `ProcessChildMemberAttributes` while building the inspector for a **selected or drawn object**. The processor runs per inspector member (field, property, method) that Odin resolves — not for every type in the assembly at startup.

A source file is read only when **all** of the following are true:

1. `ENABLED` is true and `USE_SUMMARIES` is true
2. The member has no existing `TooltipAttribute` or `PropertyTooltipAttribute`
3. `ShouldSkipMember` (if set) does not skip the member
4. That member's **declaring type** has not been parsed yet this session

The first member of a type that needs a summary triggers one disk read and one parse for that type. Every other member on the same type reuses the cached result.

### Caching (two layers)

| Cache | Key | Cleared when |
|---|---|---|
| Source lines (`OdinSourceFileHelper` in OdinSource) | `Type` | Assembly reload |
| Parsed summaries (`OdinAutoTooltipAttributeProcessor`) | Declaring `Type` → member name → summary text | Assembly reload |

After a type is warm, later inspector repaints only do dictionary lookups and attribute list inserts — no repeated `File.ReadAllLines` or XML scanning for that type.

### Cheap modes

| Setting | Cost |
|---|---|
| `ENABLED = false` | Immediate return. No parsing, no cache access. |
| `USE_SUMMARIES = false`, `USE_MEMBER_NAME = true` | No source files read. Member name string only. |
| Member already has `[Tooltip]` | Skipped before any lookup. |
| Type never shown in inspector | Never read or parsed. |

### After code changes

Caches clear on **assembly reload** (script recompile). The next time you inspect an object of that type, the `.cs` file is read and parsed once again. Editing a source file without recompiling may show stale tooltips until Unity reloads scripts.

---

## Notes

- The processor reads the **source `.cs` file** at editor time. The file must be on disk (standard Unity workflow).
- Only `/// <summary>` blocks in the member preamble are used (attributes may appear above or below the summary). `<param>`, `<returns>`, and other doc tags in the same doc comment are ignored for the tooltip text. A summary placed after the member declaration is not associated with that member.
- If source cannot be found (generated types, etc.), the member name is used when `USE_MEMBER_NAME` is true and no explicit tooltip attribute exists.
- Prefer XML `<summary>` on fields over `[Tooltip]` so Inspector tooltips stay in sync with IDE documentation. Use `[Tooltip]` only when the Inspector text must differ from the summary (enum values are the common case — see **Limitations**).
