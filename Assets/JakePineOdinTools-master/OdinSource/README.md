# OdinSource

Shared editor utilities for Jake Pine Odin plugins. **Keep this folder** whenever you use OdinBatch, OdinAutoTooltip, or future plugins in this collection.

---

## What it provides

`OdinSourceFileHelper` (in `Editor/`) is the single place for:

- Finding a type’s `.cs` file on disk (`AssetDatabase` / `MonoScript`)
- **Caching** raw source lines per `Type` (cleared on assembly reload)
- Scoping to nested type bodies (`GetTypeKey`, `TryGetTypeBodyRange`)
- Parsing member declaration lines (`ExtractMemberName`, `IsFieldDeclarationLine`, `IsPropertyOrMethodDeclarationLine`, `FindMemberEndLine`, `GetNetBraceDepthChange`, etc.)

Plugins call into this helper instead of reading the same file twice.

---

## Installation

Included in [JakePineOdinTools](../README.md). Copy `OdinSource/` alongside any plugin folders you use.

Do **not** delete this folder while any Jake Pine Odin plugin remains in the project.

No setup beyond placing files under an `Editor/` folder (Unity compiles them into the editor assembly automatically).

---

## Dependencies

- Unity Editor only — **no Odin Inspector required**
