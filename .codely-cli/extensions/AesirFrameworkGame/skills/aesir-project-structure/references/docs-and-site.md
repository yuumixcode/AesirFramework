# docs 文档结构与 Zensical 站点（展开参考）

> `../SKILL.md` 的展开参考：要建 docs/ 文档体系、把文档做成 Zensical 站点、或同仓部署 GitHub Pages 时读本文。两份配置示例可直接抄。

## 5. docs 文档结构：单一真源 + 结论回流 + AI 友好

模板里的 `docs/` 不只给 Unity 工程用，它同时是人和 AI 的文档入口。

**"什么放云端、什么进 Git"第一层切分**，判据是是否与代码同版本、是否需要 AI 读取：

| 判据 | 进 Git（默认） | 进云端（飞书/Notion） |
|---|---|---|
| 版本绑定 | 需与某 commit/版本对齐 | 与版本无关 |
| 消费者 | **AI 要读**、程序要引用 | 主要为人讨论、评论、@ |
| 形态 | 纯文本 Markdown 能表达 | 大图/视频/PSD/思维导图 |
| 时效 | 长期契约、规范、决策、API | 过程性（日报/排期/逐条 bug） |
| 编辑门槛 | 团队能接受 Markdown + Git | 策划/美术需零门槛编辑 |

**最关键纪律：单一真源 + 结论回流。** 云端负责讨论过程，Git 负责当前有效结论；讨论在飞书进行，一旦定了就把结论写回 Git 对应 md（尤其 GDD / style-guide / ADR），附云端链接备查，避免"飞书写 A、代码按 B"的分裂。大文件本体不进 Git，md 只放链接 + 文字描述——AI 读文字就够。

**让文档真正适合 AI**：

1. 根目录 `CODELY.md` 是 **AI 总入口**（TuanjieCowork 优先读取）：写清构建命令、测试命令、项目结构、代码风格、Git 工作流、**边界（AI 能改什么、不能碰什么）**，控制 200–300 行，细节外链到 `docs/`；其它 AI 工具用软链指向它，保证单一真源、勿复制内容：`AGENTS.md → CODELY.md`（Cursor / Factory 等）、`CLAUDE.md → CODELY.md`（Claude Code）。
2. `docs/index.md` 文档地图（兼 Zensical 站点首页），人和 AI 进项目先读它，按需取用。
3. 规范/决策/API 一律纯文本 Markdown——AI 读不到飞书评论和 PSD。`llms.txt` 是对外文档站约定，内部仓库用不上。
4. 每篇开头写清"管什么、何时算过时"，用 ADR（背景/选项/决定/理由）固化"为什么"。

**小团队落地顺序**：先建 `CODELY.md + README.md + docs/index.md`（半天搞定收益最大）→ 一页 GDD → style-guide 三条（色彩/UI/字体）→ ARCHITECTURE + conventions → 用到再建 adr/build/workflow。50 页 GDD 的宿命是"写完没人看"，一页起步边做边补。

### 5.1 为什么文档目录用小写 `docs/`，而 Unity 目录是大写

刻意区分，不是疏漏：

1. **大写是 Unity 的"魔法名"**：`Assets`、`Packages`、`ProjectSettings`、`StreamingAssets`、`Resources` 是引擎硬编码识别的保留目录，名字必须精确匹配（含大小写），写成小写引擎就不认——`resources/` 下 `Resources.Load` 会失效。
2. **`docs/` 不是 Unity 目录**：它在 `Assets/` 之外（同级），Unity 根本不扫描它，命名不受引擎约束。
3. **小写 `docs/` 是 Zensical 默认源目录约定**（`docs_dir = "docs"`），在项目隔离环境里 `zensical serve` / `zensical build` 零配置即可工作（安装方式见 §5.2）。改成 `Docs/` 也能用，但需在 `zensical.toml` 显式设 `docs_dir`，徒增配置。
4. **视觉信号**：大写 = "Unity 托管的世界"，小写 = "人 / AI / 文档工具链的世界"。

> 这也解释了 Unity 包内 `Samples~/`、`Documentation~/` 这种尾随波浪线目录——`~` 让 AssetDatabase 忽略它们，与 `docs/` 放在 Assets 外是同一种"让 Unity 别碰文档"的思路。

### 5.2 把 docs/ 做成 Zensical 站点：让每个人都能在本地看文档

文档全进 Git 解决"能拉取"，但直接读 `.md` 对策划、美术不友好（没导航、样式、图文排版）。做法是把 `docs/` 同时作为 **Zensical 静态站点源目录**——一份 Markdown，两种消费方式：AI 读原文，人看站点。

**为什么推荐 Zensical**：Material for MkDocs 团队（squidfunk）用 Rust 写的官方继任者（MkDocs 已停止维护），MIT 免费开源，配置用 TOML（`zensical.toml`），内置搜索、即时导航、暗色主题，构建快、零成本、零运维，纯静态无后端。

**前置门禁（AI 必读）**：Zensical 只解决"人也要看站点"的需求，按三步走：

1. **先问是否需要**：动手前 MUST 用 `ask_user` 问用户是否确实需要 Zensical。不需要就停在纯 Markdown 文档（不安装、不写 `zensical.toml`、不写部署流水线）。
2. **查全局并询问是否复用**：确认需要后，探测本机是否已有可用的全局 `zensical`（如 `command -v zensical`）。**已有则再问用户是否复用该全局工具**——用户同意即可直接用，不必建项目环境；用户不同意则走第 3 步。禁止未经询问就调用全局 `zensical`。
3. **全局未装或用户不复用时建项目环境**：MUST 让用户**二选一**——项目内 venv 或项目内 uv（uv 是前置条件，未装时用户可选 venv）。AI MUST NOT 代选、MUST NOT 默认其一、MUST NOT 在用户未作答时安装。

**关键点**：可以复用用户**已有**的全局工具，但**禁止为了用全局而新装**（`pip install zensical` / `pipx install zensical` / `uv tool install zensical`）。项目内安装一律锁 `0.1.1`。

**目录**：

```
zensical.toml          # 站点配置：nav 导航、[project.theme]、插件
docs/                  # 源目录（默认）
site/                  # 构建输出 → 必须进 .gitignore
```

**命令（第 3 步的项目内环境，二选一）**：

方案 A：项目内 venv

```bash
python3 -m venv .venv
.venv/bin/pip install "zensical==0.1.1"    # 锁版本，0.x 期间禁止裸装
.venv/bin/zensical serve                   # 本地预览，默认 http://localhost:8000，改文件自动 reload
.venv/bin/zensical build --strict          # 构建到 site/，warning 当 error，可进 CI 校验链接与格式
```

方案 B：项目内 uv（需已安装 uv；已有 `pyproject.toml` 的工程跳过 `uv init`）

```bash
uv init                    # 仅当项目尚无 pyproject.toml
uv add "zensical==0.1.1"   # 锁进项目的 pyproject.toml / uv.lock
uv run zensical serve
uv run zensical build --strict
```

**禁止清单（禁止的是"为了用全局而新装"，不是禁止复用已有工具）**：

- `pip install zensical`（全局或 `--user`）；
- `pipx install zensical`；
- `uv tool install zensical`（会把可执行文件写进用户级 `~/.local/bin`）；
- 未问过用户就调用全局 `zensical`，或未激活项目隔离环境就调 `zensical`。

本机已装全局 `zensical` 时，按第 2 步问过用户并获同意即可直接用；用户不同意或本机没装时，一律走上面的项目内 venv / uv 两套命令。项目内安装锁 `0.1.1`，与本仓 `docs.yml` 及 `tools/docs-site/README.md` 一致；0.x 期间不写裸安装命令，避免上游破坏性变更（0.1.1 的 `edit_uri` panic 坑见 `zensical.toml` 注释）。复用全局时版本由用户现有安装决定，不代其升级。

**团队收益**：任何人拉下仓库、按上文确定环境后跑一条 `zensical serve`，就能看到带导航/搜索/高亮/图文的文档站；单一真源不分裂（站点内容就是 Git 里的 Markdown 本体，`docs/index.md` 既是站点首页也是 AI 文档地图）；质量门禁（`zensical build --strict` 把失效链接/格式问题当错误纳入 CI）；配置优先、不写后端（评论/统计走 Giscus、Plausible 等免费 SaaS）。

**`zensical.toml` 最小示例**（TOML 父表 `[project]` 的键必须在子表之前写完）：

```toml
[project]
site_name = "Project Docs"
docs_dir = "docs"
site_dir = "site"
nav = [
  { "首页" = "index.md" },
  { "策划" = "product/GDD.md" },
  { "美术" = "art/style-guide.md" },
  { "技术" = "tech/ARCHITECTURE.md" },
  { "术语表" = "glossary.md" },
]

[project.theme]
variant = "modern"
language = "zh"
```

### 5.3 同仓部署 GitHub Pages：代码与官网同一个仓库

开源项目需要官网/文档站时**不必拆两个仓库**——代码和官网可以在同一仓库，关键是用对发布方式。

**为什么不能"直接"部署**：① `docs/` 是 Zensical 源目录（Markdown），不是能直接当网页的 HTML，GitHub Pages 自带构建只支持 Jekyll；② Pages 的"从分支发布"只允许选分支 root `/` 或 `/docs`——选 root 会把整个仓库（含 `Assets/`、源码）当网站发布，选 `/docs` 只发布 Markdown 源文件；③ **安全警告**：即使仓库私有，Pages 站点也是公开的，选 root 发布等于把整个项目源码公开。

**正确做法**：GitHub Actions 构建 `site/`，用 `actions/upload-pages-artifact` **只上传 `site/` 这一个目录**——Unity 代码不进入发布产物。Settings → Pages → Source 选 **GitHub Actions**。

```yaml
# .github/workflows/deploy.yml
name: Deploy Docs
on:
  push:
    branches: [main]
permissions:
  contents: read
  pages: write
  id-token: write
jobs:
  deploy:
    runs-on: ubuntu-latest
    environment: { name: github-pages }
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-python@v5
        with: { python-version: "3.12" }
      - run: pip install "zensical==0.1.1"   # 仅 CI 一次性 runner 使用，非本机全局安装
      - run: zensical build --strict
      - uses: actions/configure-pages@v5
      - uses: actions/upload-pages-artifact@v3
        with: { path: site }   # ← 只发这里，Unity 内容不会被发布
      - uses: actions/deploy-pages@v4
```

**CI 例外**：上面 workflow 里的 `pip install "zensical==0.1.1"` 只在 GitHub Actions 的一次性 runner 上执行，不写用户机器，不属于"禁止为用全局而新装"的禁令范围；本地开发按 §5.2 的三步门禁选择复用已有全局工具或项目内 venv / uv。

**三个实操注意点**：① 项目站点 URL 带子路径 `用户名.github.io/仓库名/`，`zensical.toml` 的 `site_url` 必须写成 `https://<用户名>.github.io/<仓库名>/`，否则 CSS/JS/即时导航 404；② `.nojekyll`——走 Actions + artifact 通常自动处理，旧方案推 `gh-pages` 分支需放空 `.nojekyll`，含符号链接的仓库必须用 Actions；③ `site/` 进 `.gitignore`。

> 同仓部署收益：代码与文档同版本、同 PR、同 tag，不会出现"代码在 A 仓库、官网在 B 仓库、内容对不上"的漂移。独立仓库（如本指南作者的两个站点）是另一种选择——用同一套 `zensical build --strict`，只是仓库边界不同。

