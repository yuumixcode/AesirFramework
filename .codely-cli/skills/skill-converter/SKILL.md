---
name: skill-converter
description: Convert GitHub repositories or local folders into Codely CLI skills or extensions. Use when the user provides a GitHub link to a skill repository and wants to convert it, or when converting a local folder. Automatically determines whether to create a skill (single MD file) or extension (complex content with scripts/assets).
---

# Skill Converter

Convert GitHub repositories or local folders into Codely CLI skills or extensions and install them to the project.

## When to Use

Use this skill when:
- User provides a GitHub URL and asks to convert it
- User says "convert this skill from [GitHub URL]"
- User wants to download a skill/extension from a remote repository
- User has a local folder they want to convert

## Automatic Type Detection

The converter automatically determines the output type based on source content:

| Content | Output Type | Location |
|---------|-------------|----------|
| Has `SKILL.md` + NO `skills/` subdirectory | **Skill** | `.codely-cli/skills/` |
| Has `SKILL.md` + Has `skills/` subdirectory | **Extension** | `.codely-cli/extensions/` |

**Note:** Other content (scripts/, references/, etc.) does NOT affect the type detection. Only the presence of `skills/` subdirectory matters.

### Detection Logic

```powershell
# Check if source is a simple skill
$skillMdPath = Join-Path $source 'SKILL.md'
$skillsDirPath = Join-Path $source 'skills'

if (Test-Path $skillMdPath -and -not (Test-Path $skillsDirPath)) {
    # → Skill (regardless of other content like scripts/, references/)
} else {
    # → Extension
}
```

### Examples

| Source Structure | Output Type |
|-----------------|-------------|
| `SKILL.md` (only) | Skill |
| `SKILL.md`, `scripts/`, `references/` | Skill |
| `SKILL.md`, `skills/` | Extension |
| `SKILL.md`, `scripts/`, `skills/` | Extension |
| No `SKILL.md` | Extension (if convertible) |

## Workflow

### Step 1: Parse the Source

Identify the source type:
- **GitHub URL**: Extract repository info (owner, repo, branch, path)
- **Local path**: Validate the path exists

For GitHub URLs, support these formats:
- `https://github.com/{owner}/{repo}/tree/{branch}/{path}`
- `https://github.com/{owner}/{repo}` (root of repo)

### Step 2: Download (if GitHub)

For GitHub sources, use **sparse-checkout** to only download the target subfolder:
1. If `git` is available and a subfolder path is specified, use sparse-checkout (fast, minimal download)
2. If sparse-checkout fails or git unavailable, fall back to full archive download
3. Navigate to the specified subfolder if provided

### Step 3: Detect Type & Convertibility Check

Run `check_convertibility.ps1` and content analysis:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/check_convertibility.ps1 -Path <source-path>
```

**What CAN be converted:**

| Type | Characteristics |
|---|---|
| Pure docs / knowledge | SKILL.md + references/ or assets/ |
| Generic scripts | SKILL.md + scripts/ that don't import platform SDKs |
| Generic assets | SKILL.md + templates/ with universal files |

**What CANNOT be converted (blockers):**

| Check | FAIL condition |
|---|---|
| **Skill Format** | No SKILL.md but has `.claude-plugin/`, `POWER.md`, or `agents/openai.yaml` |
| **Platform Structure** | Has `commands/` (Claude slash-cmds), `agents/*.md` (Claude sub-agents) |
| **SDK Binding** | Scripts import `anthropic`, `claude_agent_sdk`, `@anthropic-ai/sdk` |
| **Description Syntax** | Description contains `TRIGGER when`, `DO NOT TRIGGER`, `spawn_agent`, etc. |

### Step 4: Ask Install Scope

Before installing, ask the user where to install:

```
question: "安装在全局还是本项目内？"
type: choice
options:
  - label: "本项目"
    description: "安装到 <project>/.codely-cli/ （默认）"
  - label: "全局"
    description: "安装到 ~/.codely-cli/ （所有项目共享）"
```

### Step 5: Convert to Skill (Single MD Only)

If source has only a single `.md` file:

1. **Generate/validate SKILL.md**:
   - Ensure YAML frontmatter with `name` and `description`
   - Convert description to single-line format

2. **Install to skills directory**:
   - Project: `<project>/.codely-cli/skills/{name}/`
   - Global: `~/.codely-cli/skills/{name}/`

### Step 6: Convert to Extension (Complex Content)

If source has multiple files or non-MD content:

1. **Generate `gemini-extension.json`**:
   ```json
   {
     "name": "{extension-name}",
     "version": "1.0.0",
     "description": "{extracted from SKILL.md or README.md}"
   }
   ```
   
   **Format reference**: See `example/UnityGameDevWorkflow/gemini-extension.json`
   - No BOM (UTF-8 without BOM)
   - Simple structure: name, version, description only

2. **Generate `CODELY.md`**:
   - Convert SKILL.md frontmatter → remove `license` field
   - Keep body content as instruction
   - Add standard footer

3. **Organize directory structure**:
   ```
   {extension-name}/
   ├── gemini-extension.json
   ├── CODELY.md
   ├── README.md (optional)
   ├── scripts/         (if present)
   ├── references/      (if present)
   ├── templates/       (if present)
   ├── assets/          (if present)
   └── skills/          (if present - for extensions with sub-skills)
   ```

4. **Install to extensions directory**:
   - Project: `<project>/.codely-cli/extensions/{name}/`
   - Global: `~/.codely-cli/extensions/{name}/`

### Step 7: Validate Installation

Run validation script after installation:

```powershell
powershell -ExecutionPolicy Bypass -File scripts/validate_skill.ps1 -Path <installed-path>
powershell -ExecutionPolicy Bypass -File scripts/validate_extension.ps1 -Path <installed-path>
```

## Extension Format Reference

### gemini-extension.json Schema

**Reference**: `example/UnityGameDevWorkflow/gemini-extension.json`

```json
{
  "name": "extension-name",
  "version": "1.0.0",
  "description": "Single-line description"
}
```

**Important**:
- File must be UTF-8 encoded WITHOUT BOM
- Only `name`, `version`, `description` are required
- Additional fields (settings, mcpServers) can be added if needed

### Optional Fields

```json
{
  "name": "extension-name",
  "version": "1.0.0",
  "description": "Single-line description",
  "settings": [
    {
      "name": "SETTING_NAME",
      "description": "What this setting does",
      "default": "default_value",
      "required": false,
      "secret": false
    }
  ],
  "mcpServers": {
    "server-name": {
      "command": "node",
      "args": ["${extensionPath}/server.js"],
      "env": {}
    }
  }
}
```

### CODELY.md Format

```markdown
# {extension-name} - Codely CLI Extension

{Description}

## Features

- Feature 1
- Feature 2

## Configuration

**Required:**
- `SETTING_NAME`: Description

**Optional:**
- `OTHER_SETTING`: Description (default: value)

## Usage

```
"Example usage prompt"
"Another example"
```

---

*This extension was converted using [skill-converter]*
```

## Script Reference

**Windows (PowerShell):**
```powershell
# Auto-detect and convert
powershell -ExecutionPolicy Bypass -File scripts/convert_from_github.ps1 -Url <url> [-Scope project|global] [-Overwrite]

# Convert local folder
powershell -ExecutionPolicy Bypass -File scripts/convert_from_local.ps1 -Path <path> [-Scope project|global] [-Overwrite]

# Force as extension
powershell -ExecutionPolicy Bypass -File scripts/convert_from_github.ps1 -Url <url> -AsExtension

# Force as skill
powershell -ExecutionPolicy Bypass -File scripts/convert_from_github.ps1 -Url <url> -AsSkill
```

**Parameters:**
- `Url`: GitHub URL to convert (required)
- `Scope`: `project` (default) or `global`
- `AsExtension`: Force conversion as extension
- `AsSkill`: Force conversion as skill
- `Overwrite`: Allow replacing existing directory

## Output Directories

| Type | Project Scope | Global Scope |
|------|---------------|--------------|
| Skill | `<project>/.codely-cli/skills/<name>/` | `~/.codely-cli/skills/<name>/` |
| Extension | `<project>/.codely-cli/extensions/<name>/` | `~/.codely-cli/extensions/<name>/` |

## Notes

- Always validate after conversion
- Clean up temp directories after installation
- Preserve existing SKILL.md when present
- Convert names to lowercase kebab-case
- Extract description from README.md if SKILL.md missing