---
name: sync-samples
description: >-
  Sync Unity package samples from Assets/Samples/ to each package's Samples~ folder.
  Triggers when the user says "sync samples", "同步 Samples", "同步示例",
  "sync Samples~", "update package samples", or when Samples in Assets/Samples/
  have been modified and need to be pushed back to the package source Samples~ folders.
  Also use proactively after editing imported samples or before committing package changes.
---

# Sync Samples

Unity packages store sample source in `Samples~` folders. When users import samples
via the Package Manager, Unity copies them to `Assets/Samples/<Package>/<Version>/`.
Edits made in `Assets/Samples/` must be synced back to `Samples~` to persist in the package.

## Workflow

1. Run `scripts/sync_samples.py` from the project root
2. Review the diff output
3. Commit the changes

## Script Usage

```bash
python3 scripts/sync_samples.py [--project-root .] [--dry-run]
```

- `--project-root`: Unity project root (default: current directory)
- `--dry-run`: Show what would be synced without modifying files

## How It Works

1. Scans `Assets/Runestone/*/package.json` for packages with a `samples` array
2. For each sample, maps `displayName` → `path` (Samples~ folder name)
3. Finds the corresponding folder in `Assets/Samples/<Package>/<Version>/`
4. Copies content from Assets/Samples to Samples~, replacing existing files
5. Reports a summary of synced files

## Key Behaviors

- **Direction**: Always Assets/Samples → Samples~ (user edits flow back to package source)
- **Folder name mapping**: displayName in Assets/Samples may differ from path in Samples~
  (e.g., "Plugin Config Solutions" → `Samples~/PluginConfigSolutions`)
- **Version detection**: Automatically finds the version subfolder under each package name
- **.meta handling**: Copies .meta files as-is; Unity regenerates if needed
- **Deletion**: Files removed in Assets/Samples are also removed from Samples~
