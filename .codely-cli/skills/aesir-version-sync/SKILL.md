---
name: aesir-version-sync
description: >-
  在 Aesir monorepo 中同步更新所有子包（Architecture / Modules / Inspector）的版本号。
  当用户说"发新版本"、"更新版本"、"bump version"、"version sync"、"同步版本号"、
  或想发布新版本时触发。处理根目录 CHANGELOG、README（中英文）、各包 CHANGELOG、
  package.json 版本号、依赖版本更新和 Samples 文件夹版本对齐。
---

# Aesir 包版本同步

在 monorepo 中同步更新三个 Aesir 子包的版本号。

## 仓库结构

三个子包，各自有独立的 `package.json` 和 `CHANGELOG.md`：

| 包 | package.json | CHANGELOG | 包 ID |
|----|-------------|-----------|-------|
| Architecture | `Assets/Runestone/AesirArchitecture/package.json` | `Assets/Runestone/AesirArchitecture/CHANGELOG.md` | `cn.runestone.aesir.architecture` |
| Modules | `Assets/Runestone/AesirModules/package.json` | `Assets/Runestone/AesirModules/CHANGELOG.md` | `cn.runestone.aesir.modules` |
| Inspector | `Assets/Runestone/AesirInspector/package.json` | `Assets/Runestone/AesirInspector/CHANGELOG.md` | `cn.runestone.aesir-inspector` |

根目录聚合 CHANGELOG：`CHANGELOG.md`（项目根目录）。

根目录 README：`README.md`（中文）和 `README.en.md`（英文），两文件中均有版本号表格需要同步。

### 依赖链

- **Architecture** — 不依赖任何 Aesir 子包
- **Inspector** — 不依赖任何 Aesir 子包
- **Modules** — 同时依赖 Architecture + Inspector（见其 `package.json` 的 `dependencies`）

当 Architecture 或 Inspector 升版本时，**必须同步更新** Modules 的 `package.json` 中对应的依赖版本号。

## 工作流程

### 1. 确定新版本号

向用户确认目标版本号（如 `0.4.1`）。三个包统一使用相同版本号。

### 2. 确认哪些包有实际变更

向用户确认本次哪些包有实际功能变更。没有变更的包也需要写一条版本记录，防止版本历史断档。

### 3. 更新 package.json

**三个包**都要更新 `"version"` 字段：

```json
"version": "0.4.1",
```

如果 Architecture 或 Inspector 有变更，还要更新 Modules 的 `dependencies`：

```json
"dependencies": {
  "cn.runestone.aesir.architecture": "0.4.1",
  "cn.runestone.aesir.inspector": "0.4.1"
}
```

### 4. 更新各包 CHANGELOG

在每个包的 `CHANGELOG.md` 中，在 `[Unreleased]` 下方、上一个版本上方，插入新的 `## [版本号] - YYYY-MM-DD` 区块。

#### 有实际变更的包

按 Added / Changed / Fixed / Removed 等分类编写实际变更内容。

#### 无实际变更的包

写一条同步记录防止版本断档：

```markdown
## [0.4.1] - 2026-07-24

### Changed

- 版本号与 Aesir Architecture / Aesir Inspector 同步更新至 `0.4.1`，本包本版本无功能性变更
```

（根据实际存在的其他包名调整句子中的包名。）

### 5. 更新根目录 CHANGELOG

插入新的 `## [版本号] - YYYY-MM-DD` 区块，用 `###` 大标题区分不同包：

```markdown
## [0.4.1] - 2026-07-24

---

### [architecture] Aesir Architecture

#### Changed

- （实际变更内容）

---

### [modules] Aesir Modules

#### Changed

- 版本号与 Aesir Architecture / Aesir Inspector 同步更新至 `0.4.1`，本包本版本无功能性变更

---

### [inspector] Aesir Inspector

#### Changed

- （实际变更内容）
```

各包之间用 `---` 分隔线隔开。只有有变更记录的包才出现 `###` 区块。

### 6. 更新根目录 README

根目录 `README.md`（中文）和 `README.en.md`（英文）中各有一个版本号表格，列出了三个包的当前版本。需要将旧版本号替换为新版本号。

`README.md` 中的表格格式：

```markdown
| 子包 | 用途 | 包名 | 版本 |
|---|---|---|---|
| **Aesir Architecture** | ... | `cn.runestone.aesir.architecture` | `0.4.1` |
| **Aesir Modules** | ... | `cn.runestone.aesir.modules` | `0.4.1` |
| **Aesir Inspector** | ... | `cn.runestone.aesir.inspector` | `0.4.1` |
```

`README.en.md` 中的表格格式：

```markdown
| Sub-Package | Purpose | Package ID | Version |
|---|---|---|---|
| **Aesir Architecture** | ... | `cn.runestone.aesir.architecture` | `0.4.1` |
| **Aesir Modules** | ... | `cn.runestone.aesir.modules` | `0.4.1` |
| **Aesir Inspector** | ... | `cn.runestone.aesir.inspector` | `0.4.1` |
```

用 `replace` 工具或 `run_shell_command`（sed）将两个文件中的旧版本号批量替换为新版本号。

### 7. 更新各子包 README 版本徽章

每个子包的 `README.md`（中文）中有版本号徽章，需要同步更新：

| 包 | README.md（中文） | README_EN.md（英文，位于 Documentation~） |
|----|-----------|-------------|
| Architecture | `Assets/Runestone/AesirArchitecture/README.md` | `Assets/Runestone/AesirArchitecture/Documentation~/README_EN.md` |
| Modules | `Assets/Runestone/AesirModules/README.md` | `Assets/Runestone/AesirModules/Documentation~/README_EN.md` |
| Inspector | `Assets/Runestone/AesirInspector/README.md` | `Assets/Runestone/AesirInspector/Documentation~/README_EN.md` |

徽章格式：

```markdown
[![Version](https://img.shields.io/badge/version-0.4.1-blue.svg)](./CHANGELOG.md)
```

注意：英文 README 位于 `Documentation~/` 子目录中，徽章中的相对路径需用 `../CHANGELOG.md`。

此外，Modules 的 README 中列出了 Architecture 和 Inspector 的依赖版本号（如 `>= 0.4.2`），也需要同步更新。

### 8. 对齐 Samples 文件夹

检查 `Assets/Samples/<包显示名>/<旧版本>/`，如果版本号与 `package.json` 不一致，用 `unity_asset` 的 `rename` 操作重命名：

- `Assets/Samples/Aesir Architecture/<旧版本>/` → `Assets/Samples/Aesir Architecture/<新版本>/`
- `Assets/Samples/Aesir Inspector/<旧版本>/` → `Assets/Samples/Aesir Inspector/<新版本>/`

重命名后，搜索 `CODELY.md` 中的旧版本路径并更新。

### 9. 同步 Samples（可选）

如果用户需要将源码同步到开发副本：

```bash
python3 sync-samples.py to-dev
```

### 10. 编译验证

执行 `unity_workflow`（action: `compile_and_validate`）确保无编译错误。

## 关键约定

- 根目录 CHANGELOG 使用 `### [architecture]` / `### [modules]` / `### [inspector]` 大标题区分各包，包之间用 `---` 分隔
- 根目录 README.md 和 README.en.md 中的版本号表格必须同步更新
- 各子包 README.md（中文）和 `Documentation~/README_EN.md`（英文）中的版本徽章和依赖版本号也必须同步更新
- 文档命名规范：根目录英文文档使用 `.en.md` 后缀（如 `README.en.md`）；各子包英文 README 统一放在 `Documentation~/README_EN.md`
- 各包自己的 CHANGELOG 只记录该包的变更历史
- 日期格式统一使用 `YYYY-MM-DD`
- 即使某包没有功能变更，也必须写一条 CHANGELOG 记录——防止版本历史断档
- 保留各 CHANGELOG 中的 `[Unreleased]` 区块（新版本插在它下方）
