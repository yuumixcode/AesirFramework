---
name: sync-samples
description: >-
  Sync a package's authoring samples (Samples/) to its UPM mirror (Samples~/).
  Triggers when the user says "同步 Samples", "统一 Samples", "sync samples",
  "同步示例", "update package samples", "sync Samples~", or after editing anything
  under Assets/Runestone/*/Samples/ that must be mirrored to Samples~/.
---

# Sync Samples（`Samples/` → `Samples~/`）

每个包有两份示例目录，两份必须**逐字节一致**（含 `.meta`，GUID 保持一致）：

| 目录 | 角色 | AssetDatabase | 随 unitypackage 导出 |
|------|------|---------------|---------------------|
| `Assets/Runestone/<包>/Samples/` | **编写主位**（可见、可直接运行） | 可见 | 是 |
| `Assets/Runestone/<包>/Samples~/` | **发布镜像**（UPM 经 Package Manager 按需导入） | 不可见 | 否 |

默认同步方向：**`Samples/` → `Samples~/`**。

> ⚠️ **本技能已废弃的旧工作流**：早期方向是 `Assets/Samples/<包>/<版本>/` → `Samples~/`
> （把 Package Manager 导入副本的改动写回包内）。`Assets/Samples/` 导入副本已从仓库删除，
> 该路径不再存在。若你看到脚本或文档里出现 `Assets/Samples/`，说明内容过时。

## 工作流

### 1. 先判断方向（关键步骤，别跳过）

```bash
python3 scripts/sync_samples.py --check
```

逐文件看差异**性质**：

- **常见情况**：主位更新（Rider 格式化、成员顺序、`using` 清理、新增内容、场景补对象）→ 主位覆盖镜像。
- **反例（务必警惕）**：镜像可能才是正确的一侧。历史事故两例：
  - `9188839` 把 AesirModules 的 `BinderTag` ×3、`MenuProbe` 探针对象、HUD 上的 Binder 代码生成器
    （`TargetNamespace=Game` / `ScriptName=HUDPanel`）污染写进了 PlaneWar 场景——Architecture 无 Modules
    依赖，独立安装时这些组件是 Missing Script；
  - `4bbe260` 把 PlaneWar `GameManager` 的显式非序列化字段（`_score` / `_isGameOver` + 只读属性）
    改回了自动属性，违反「MonoBehaviour 运行状态字段一律用显式非序列化字段」铁律。

  遇到"主位像是回退/污染"的文件，先查历史确认再决定：

  ```bash
  git log --oneline -3 -- <文件路径>
  git log --oneline -S"<关键符号>" -- <文件路径>   # 定位引入/移除某成员的提交
  ```

  **按正确的一侧修正后再统一**，不要凭方向约定硬覆盖；例外情况在 commit message 里写明。

### 2. 执行同步

```bash
python3 scripts/sync_samples.py --dry-run   # 预览动作
python3 scripts/sync_samples.py             # 覆盖镜像 + 删除镜像多余项 + 补齐顶层目录 .meta
```

### 3. 校验

```bash
diff -rq Assets/Runestone/<包>/Samples Assets/Runestone/<包>/Samples~   # 应无输出
python3 scripts/sync_samples.py --check                                  # 应全部 OK
```

### 4. 编译与回归

- 主位 `Samples/` 下的脚本/场景改动需要 `unity_editor.refresh` 重新导入编译（0 错 0 警）。
- 跑 EditMode 测试；**改了场景**再补一次 Play Mode 冒烟（用 `exec_runtime_script` 返回结构化事实，
  不必录像）。

### 5. 提交

中文 commit message，注明同步范围；若存在"按镜像修正主位"的例外，逐条列出原因。

## 脚本行为

```bash
python3 scripts/sync_samples.py [--project-root .] [--package NAME] [--check | --dry-run]
```

- 发现 `Assets/Runestone/*/` 下**同时**含 `Samples/` 与 `Samples~/` 的包（`--package` 可限定单个包目录名）。
- 逐示例目录同步：覆盖同名文件、删除镜像中多余的文件与空目录（跳过 `.DS_Store` 等 junk）。
- 补齐 `Samples/*.meta` → `Samples~/*.meta`（**顶层目录级 `.meta` 最容易漏**——0.14.0 起曾缺失 12 个）。
- 顺带校验 `package.json` 的 `samples` 清单：每条 `path` 在两处都存在，且镜像下没有未登记的示例程序集目录。
- `--check` 只读报告（含每个差异文件的 `+新增/-删除` 行数，便于判断方向）；`--dry-run` 预览动作；两者都不写文件。

## 注意

- 镜像对 AssetDatabase 不可见：**同步镜像不需要 Unity refresh**，只有主位 `Samples/` 改动才需要。
- `.meta` 必须一起同步（GUID 是 UPM 导入链路稳定的前提）；新增/删除示例目录时，两处的**目录级 `.meta`** 要一起增删。
- 示例脚本是**运行时程序集 + 整文件 `#if UNITY_EDITOR` 包裹**（构建整体剔除）；不要改回 Editor-only asmdef——
  那样 MonoBehaviour 会被判定为"编辑器脚本"而无法挂载到场景物体。
- `package.json` 的 `path` 是相对**包根**的路径（形如 `Samples~/Counter-Mvc-Quick`）。
- 删除某个功能模块时，记得连带删除其示例目录与两处 `.meta`，并清理 `package.json` 的 `samples` 条目。
