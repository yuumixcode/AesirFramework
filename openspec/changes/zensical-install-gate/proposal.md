## Why

框架扩展 `AesirFrameworkGame` 的 `aesir-project-structure` 技能在 Zensical 建站参考里直接给出 `pip install zensical` 与裸 `zensical serve` / `zensical build`，AI 会照抄执行**全局安装**，污染用户机器的 Python 环境；同时文档把"用不用 Zensical"当成默认前提，用户不想建站时也会被带偏。用户已明确要求：涉及 zensical 一律不得全局安装，先问是否需要，再由用户选项目内 venv 或项目内 uv。

## What Changes

- 新增**前置询问门禁**：AI 在扩展技能里凡涉及 Zensical（建站 / 预览 / 严格构建 / CI 示例）之前，必须先询问用户是否确实需要 Zensical；用户不需要则停在纯 Markdown 文档，不安装、不建站、不写部署流水线。
- 用户确认需要后，必须让用户**二选一**，且两种方案都只写项目内、不写全局：
  - 项目内 venv：`python3 -m venv .venv` + `.venv/bin/pip install "zensical==0.1.1"`；
  - 项目内 uv：`uv init` + `uv add "zensical==0.1.1"` + `uv run zensical ...`，依赖锁在项目的 `pyproject.toml` / `uv.lock`。
- 明确**禁止**的实现：`pip install zensical`（全局 / `--user`）、`pipx install zensical`、`uv tool install zensical`、以及任何未进入项目隔离环境的调用。
- 落地位置：`aesir-project-structure/SKILL.md` 正文保留一句结论与触发，`references/docs-and-site.md` 展开两套命令与禁止清单（替换现有 `pip install zensical` 示例）；扩展 `CHANGELOG.md` 记一条。
- 不做：不写自动安装脚本、不替用户选择方案、不改动本仓 `.github/workflows/docs.yml`、不实际安装任何软件。

## Capabilities

### New Capabilities

- `ai-extension-zensical-gate`: 框架扩展中涉及 Zensical 时的前置询问与隔离安装契约——先问是否需要、再由用户选项目内 venv 或项目内 uv、禁止全局安装的边界与例外。

### Modified Capabilities

无（本仓主规格库当前为空；`optimize-aesir-extension` 的两个能力尚在变更中，本次不改它们）。

## Impact

- 只改 AI 扩展文档：`.codely-cli/extensions/AesirFrameworkGame/skills/aesir-project-structure/SKILL.md` 与 `references/docs-and-site.md`，另加扩展 `CHANGELOG.md` 一条。
- 不改 Unity 代码 / 包 / 场景 / 测试（扩展目录对 AssetDatabase 不可见）；不触发编译或 EditMode / PlayMode 门禁。
- 与在途变更 `optimize-aesir-extension` 共用上述两个技能文件（其任务 2.2 / 3.1 正在改写它们）：**实施前必须确认该变更的提交状态与文件内容**，避免覆盖他人已完成的改动；本变更只做规划，不触碰这些文件。
