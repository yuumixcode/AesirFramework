# JakePine Odin Tools

Editor plugins for [Odin Inspector](https://odininspector.com/) (Sirenix) that read C# source at edit time. Unity 2021+ / C# 9+.

---

## Plugins

- **[OdinAutoTooltip](OdinAutoTooltip/README.md)** — apply `TooltipAttribute` from XML `/// <summary>` doc comments.

See each plugin's README for installation details, examples, and options.

---

### What to keep

| Folder | Required? |
|---|---|
| **OdinSource** | **Yes** — if you use any plugin below |
| **OdinAutoTooltip** | No — delete if you do not need auto-tooltips |

If you remove a plugin folder, leave **OdinSource** in place. The plugin shares one source-line cache through `OdinSourceFileHelper`.

---

## Folder structure

Copy the entire `JakePineOdinTools` folder into your project (for example `Assets/Plugins/JakePineOdinTools`):

```
JakePineOdinTools/
├── LICENSE.txt
├── README.md                    ← you are here
├── OdinSource/                  ← required — shared source parsing + cache
│   ├── README.md
│   └── Editor/
│       └── OdinSourceFileHelper.cs
└── OdinAutoTooltip/             ← optional — XML summary → tooltips
    ├── README.md
    └── Editor/
```
---

## Requirements

- Unity Editor
- Odin Inspector (for **OdinAutoTooltip**; **OdinSource** has no Odin dependency)
