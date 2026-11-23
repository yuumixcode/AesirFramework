## Context

- 框架扩展 `AesirFrameworkGame` 的 Zensical 内容集中在 `skills/aesir-project-structure/SKILL.md` 的引用行与 `references/docs-and-site.md` 的 §5.2 / §5.3。后者直接给出 `pip install zensical`、裸 `zensical serve`、`zensical build --strict` 与 CI 里 `pip install zensical` 示例，没有"是否真的需要站点"和"装到哪里"的前置判断。
- 本机现状印证了风险：`zensical 0.1.1` 同时以 `uv tool`（`~/.local/bin/zensical`）和 pipx 残留两份存在于用户级环境，而项目本身没有任何依赖清单文件——即"全局安装、版本口径分散"已经在发生。
- 在途变更 `optimize-aesir-extension` 的任务 2.2 / 3.1 正在改写上述两个技能文件（当前工作树处于未提交状态）；本变更必须在其最新内容上追加，不能整体覆盖。
- 本仓已有 `ai-extension-dependency-gate`（装包门禁）思路：hook 只陈述事实，读记忆与询问用户仍由 AI 在会话中完成。本变更沿用同一边界。

## Goals / Non-Goals

**Goals:**

- 把"先问是否需要 Zensical → 由用户选项目内 venv 或项目内 uv → 全程不写全局 Python 环境"做成 AI 载入技能时必然看得到的结论。
- 两种隔离方案都要给出可直接复制、版本一致（0.1.1）的命令，避免 AI 自己发明写法。

**Non-Goals:**

- 不改 `.github/workflows/docs.yml`，不在本次实际安装、升级或卸载本机工具（包括已有的 uv tool 与 pipx 残留）。
- 不新增 hook 或脚本自动探测 / 自动安装；不为语义规则加机械门禁。
- 不把禁令扩展到 CI 的一次性 runner（见 D3）。

## Decisions

### D1：门禁放技能正文 + 展开参考，不放 hook

这条规则的核心是**向用户提问并让其选择**，而 hook 的既定边界是"只提供事实，不替用户决策、不自动安装"（对齐 `ai-extension-dependency-gate` 的 hook 不替代门禁决策）。此外，AskUser 交互只能在会话里发生，hook 无法完成。因此正文保留一句可发现结论，展开参考放完整命令与禁止清单；`validate.py` 的篇幅 / 链接门禁照常覆盖，不为其增加语义检查。

### D2：两种方案都必须是项目内隔离，uv 取 `uv init` / `uv add` / `uv run`

- **venv**：`python3 -m venv .venv` → `.venv/bin/pip install "zensical==0.1.1"` → `.venv/bin/zensical ...`，依赖与版本记在项目内依赖文件。
- **uv**：`uv init`（若项目尚无 Python 工程）→ `uv add "zensical==0.1.1"` → `uv run zensical ...`，依赖锁进项目的 `pyproject.toml` / `uv.lock`。
- 明确禁止：`pip install zensical`（全局 / `--user`）、`pipx install zensical`、`uv tool install zensical`、未激活项目隔离环境就调 `zensical`。
- 备选否决：`uv tool` 虽能隔离依赖，但仍把可执行文件暴露到用户级 `~/.local/bin`，与"不要全局安装"冲突，故不列入选项。

### D3：CI 里的 `pip install zensical` 保留，禁令只针对用户本机与工程环境

GitHub Actions 的 runner 是一次性构建环境，不污染用户机器；本仓 `docs.yml` 已锁 `zensical==0.1.1` 并通过 `zensical build --strict` 做门禁。把 CI 也算成"全局安装"会迫使 workflow 改形却无实际收益。因此 §5.3 的部署示例保留 `pip install "zensical==0.1.1"`（顺手补上版本锁），并在展开参考注明这条是 CI 一次性环境例外。

### D4：命令示例统一锁 0.1.1

与本仓 `docs.yml`、`tools/docs-site/README.md` 的现行版本一致；0.x 期间不写裸 `zensical`，避免上游破坏性变更（0.1.1 的 `edit_uri` panic 坑位已记录在站点配置里）。

### D5：不新增脚本门禁

"是否先询问用户"是会话行为，脚本无法可靠判定；强行 grep 关键字只会制造误报。可验证落点改为：`SKILL.md` 正文含结论或链接、`docs-and-site.md` 不再出现裸 `pip install zensical`、两种方案命令齐全——这三条写进 tasks 作人工 / diff 核对。

## Risks / Trade-offs

- **撞在途改动** → 实施前 `git status` + 读取两个技能文件的当前内容，确认 `optimize-aesir-extension` 是否已提交；在最新内容上追加，不整文件覆盖。若发现内容已被另一实施者改过且与本设计不一致，停下 `ask_user` 裁决，不自行合并。
- **AI 仍跳过询问直接给命令** → 结论进正文首屏且 `references` 链接显式，命令块前置"先问是否需要 / 由用户选"两步；靠 PR 审阅兜底。
- **用户机器已有全局 `zensical`** → 命令块明确要求不调用全局可执行文件；正文提示"本机已有也不复用"。
- **用户机器未装 uv** → 询问方案中注明 uv 是前置条件，未装时用户可选 venv；AI 不代装 uv。

## Migration Plan

1. 核对 `optimize-aesir-extension` 的提交状态与目标文件当前内容（只读）。
2. 改 `references/docs-and-site.md`：§5.2 命令块换成"先询问 → 用户二选一 → venv / uv 两套命令 → 禁止清单"，§5.3 CI 示例补版本锁与一次性环境例外注。
3. 改 `SKILL.md`：在 §5 的 references 索引处补一句门禁结论，保持正文篇幅在 6000 字符上限内。
4. 扩展 `CHANGELOG.md` 记一条（按该文件现有格式）。
5. 跑 `python3 .codely-cli/extensions/AesirFrameworkGame/scripts/validate.py`，要求 0 错 0 警；不跑 Unity 编译 / 测试（扩展目录对 AssetDatabase 不可见）。
6. 提交：仅扩展目录内精确路径 `git add`，一个 commit；回退直接 revert 该 commit，无外部状态遗留。
