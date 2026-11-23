## 1. 前置核对（防并发覆盖）

- [x] 1.1 核对 `optimize-aesir-extension` 的提交状态：跑 `git status --short` 并读取 `skills/aesir-project-structure/SKILL.md` 与 `references/docs-and-site.md` 当前内容（验证：确认两文件的最新内容已纳入本任务依据；若与 design.md 的 Context 描述不符或存在另一实施者的未提交改动，停下 `ask_user` 裁决后再动，不自行合并）

## 2. 展开参考补门禁与两套隔离命令

- [x] 2.1 在 `references/docs-and-site.md` §5.2 的 Zensical 命令块前插入两步门禁：先问用户是否确实需要 Zensical（不需要就停在纯 Markdown，不装不建站），确认需要再让用户二选一（验证：两段文字齐全且顺序为"先问是否需要 → 再选方案"）
- [x] 2.2 把裸命令块替换为项目内 venv 与项目内 uv 两套：venv 用 `python3 -m venv .venv` + `.venv/bin/pip install "zensical==0.1.1"` + `.venv/bin/zensical ...`；uv 用 `uv add "zensical==0.1.1"` + `uv run zensical ...`（已有工程不必重复 `uv init`）（验证：两套命令均可复制执行、都锁 0.1.1，文中不再有裸 `pip install zensical`）
- [x] 2.3 紧跟命令块补禁止清单：全局 / `--user` 的 `pip install zensical`、`pipx install zensical`、`uv tool install zensical`、未激活项目隔离环境就调 `zensical`（验证：四类禁止项逐条可查）
- [x] 2.4 §5.3 的 CI 示例把 `pip install zensical` 改为 `pip install "zensical==0.1.1"`，并注明该命令只用于 GitHub Actions 一次性 runner、不属用户本机全局安装禁令（验证：版本锁与例外说明同时出现）

## 3. 技能正文可发现性

- [x] 3.1 在 `skills/aesir-project-structure/SKILL.md` 的 §5 references 索引处补一句门禁结论（先问是否需要 Zensical、再选项目内 venv 或 uv、禁止全局安装）并指向展开参考（验证：正文 ≤ 6000 字符，`references/docs-and-site.md` 相对链接可达）

## 4. 变更记录与门禁

- [x] 4.1 在扩展 `CHANGELOG.md` 按现有格式记一条本规范（验证：条目存在且描述与 specs 一致）
- [x] 4.2 跑 `python3 .codely-cli/extensions/AesirFrameworkGame/scripts/validate.py`（验证：退出码 0，输出「0 个错误，0 个警告」）
- [x] 4.3 diff 复核：`git diff` 检查两份技能文档，确认无裸 `pip install zensical`、两套隔离命令齐全、禁止清单在位（验证：三项均符合）
- [x] 4.4 确认 Unity 侧零影响：`git status --short` 无 `Assets/`、`Packages/`、`ProjectSettings/` 改动（验证：输出符合预期；工作树里其他在途变更不属本任务、不动）

## 5. 提交

- [x] 5.1 精确路径 `git add` 本次改动的扩展文档并提交（Conventional Commits，中文主题，主题/正文不含跳过 CI 的标记字面量）（验证：`git show --stat` 只含本变更列出的扩展文档）

## 归档时自动执行

- 本变更的规格 delta 合并进主规格（由 `openspec archive` 执行）
- 变更目录移入 `openspec/changes/archive/`（由 `openspec archive` 执行）
