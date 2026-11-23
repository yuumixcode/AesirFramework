# 设计：文档站并入主仓

> 前置事实见 proposal.md 可行性表。本文只写「怎么做、为什么这么做」。

## D1 目标布局（主仓根）

```
AesirFramework/
├── docs/                      # ← AesirFramework-Site/docs/ 原样迁入（380 文件，内部路径零变化）
│   ├── architecture/          #   含 scripting-api/（SDG 重生成落点，路径不变）
│   ├── modules/               #   含 scripting-api/
│   ├── product/ start/ assets/ javascripts/ stylesheets/ …
│   └── index.md / changelog.md / faq.md / support.md
├── zensical.toml              # ← 站点根配置迁入（改 3 个键，见 D2）
├── tools/
│   └── docs-site/             # ← 站点 tools/ + assets-src/ + README.md 合并迁入
│       ├── serve-preview.py   #   本地预览（迁入后核对 cwd 假设）
│       ├── shoot.mjs          #   截图工具（同上）
│       ├── assets-src/logo.png
│       └── README.md          #   原站点 README（工程说明 + 站点约定 + API 类型总数维护口径）
├── .github/workflows/
│   ├── docs.yml               # ← 站点 deploy.yml 改名迁入（见 D4）
│   ├── sync-to-cnb.yml        #   不动
│   └── auto-release.yml       #   不动
├── site/                      # 构建产物（gitignored，新增忽略）
├── .cache/                    # zensical 增量缓存（gitignored，新增忽略）
└── openspec/changes/merge-docs-site/   # 本提案（随变更入库）
```

裁决理由：

- **`docs/` 放根而非 `Assets/` 内**：Assets 内会被 Unity 导入生成 `.meta`、可能混进包产物；根目录对 AssetDatabase 不可见，与 `Aesir-Notebook` 同级同理。
- **站点 `tools/`、`assets-src/`、`README.md` 收进 `tools/docs-site/`**：主仓根 README 已存在（撞名）；`tools/` 语义留给主仓通用工具，按用途分子目录；这些文件**不能放 `docs/` 内**——zensical 会把非 md 文件原样拷进公开站点。
- **不迁**：站点 `LICENSE`（主仓已有 MIT）、`.gitignore`（合并进主仓）、`.codely-cli/`（本机工具链）、`site/`、`.cache/`（产物）。
- **内容与包内 `Documentation~/` 保持两套**：产品站（对外门面 + Scripting API）与包内使用文档（随包分发）定位不同，本次只动仓库边界，不动内容架构。

## D2 URL 治理

### zensical.toml 改动（仅 3 键 + 注释）

```toml
site_url  = "https://yuumixcode.github.io/AesirFramework/"
repo_url  = "https://github.com/yuumixcode/AesirFramework"
repo_name = "yuumixcode/AesirFramework"
edit_uri  = ""   # 保留！0.1.x 会拼「repo_url + edit_uri + 源路径」做 IRI 校验，
                 # Scripting API 页名含 {T} 泛型占位 → 花括号非法 panic。删键会被自动回填，必须显式置空。
```

`site_url` 是 instant navigation / instant previews / 错误页的前提，必须与新 Pages 地址一致。docs 内部路径零变化 → 除域名前缀外全站 URL 结构不变。

### 主仓内引用替换（已全仓摸清，共 6 类落点）

| 落点 | 现值 | 改为 |
|---|---|---|
| `Assets/Runestone/AesirArchitecture/package.json` L7-8 | `…/AesirFramework-Site/architecture/`、`…/changelog/` | `…/AesirFramework/architecture/`、`…/AesirFramework/changelog/` |
| `Assets/Runestone/AesirModules/package.json` L7-8 | `…/AesirFramework-Site/modules/`、`…/changelog/` | 同上新前缀 |
| `AesirGetStartedWindowOdin.cs` L146 `DocsUrl` | `…/AesirFramework-Docs/`（**已 404 的存量死链**） | `…/AesirFramework/`（顺带修复，CHANGELOG 记 Fixed） |
| `CODELY.md` L454 外部目录节 | 「AesirFramework-Site 独立仓库」 | 「docs/ 同仓站点」口径（连带目录结构图、门禁、文档矩阵） |
| `.codely-cli/memory.md` L35 + `memory/external-workflow.md` + `memory/docs-site-rebuild-api-nav.py` `SITE` 常量 | 独立仓路径/URL | 主仓根路径/新 URL |
| `.codely-cli/skills/aesir-version-sync/SKILL.md` L79 等 | 「两仓各自直接 push」 | 「站点随主仓提交，Notebook 仍独立 push」 |

**发版纪律衔接**：`package.json` 与编辑器代码的改动进两包 CHANGELOG `[Unreleased]`，**随下次正常发版到达用户，不为 URL 单独 bump 版本**——旧地址由跳转版兜底，期间无断链，不触碰「tag 不可变」铁律。

### 旧仓跳转版（保存量深链）

GitHub Pages 无服务端 301 能力，标准做法是 **meta refresh + canonical**：

1. 用旧站**最后一次成功构建产物** `site/`（66M，380 页）生成跳转 HTML：对每个 `site/**/*.html` 的相对路径 `p`，产出
   `<meta http-equiv="refresh" content="0; url=https://yuumixcode.github.io/AesirFramework/p">` + `<link rel="canonical" href="新地址/p">` + 标题与手动点击链接；`404.html` 跳新站首页。生成脚本放旧仓 `tools/make-redirects.py`（一次性，产物 `redirect/` 入库）。
2. 旧仓 `deploy.yml` 改纯静态上传：删 pip/zensical 步骤，`upload-pages-artifact` 的 `path: redirect`。**先部署验证跳转成功，才动旧站内容**；失败则旧站原样在线。
3. 旧仓 README 顶部加弃用说明（新地址 + 迁移日期 + 「本仓仅保留跳转与历史」），repo description 同步。
4. **不删仓**：54 commits 历史 + 跳转壳都留在旧仓。archive 为可选项（见 D3 风险）。

SEO：canonical 指向新站转移权重，meta refresh 非真 301 是 GitHub Pages 平台限制下的最优解；新站 sitemap 由 zensical 按新 `site_url` 自动生成。

### 备选 B：自定义域名（默认不采用）

若用户有自有域名，可给主仓 Pages 配 CNAME（如 `aesir.example.com`），旧仓 CNAME 同域 → URL 零变化、外链零治理。用户未提出域名，本提案按默认 A（换路径 + 跳转）设计；若评审时改选 B，D2 的引用替换与跳转版全部免做，只加 CNAME 文件与 DNS 步骤。

## D3 历史保留与旧仓终态

- **默认 A：文件级迁移**。主仓一个迁移 commit（`docs: 产品站并入主仓`），历史留在旧仓可追溯。理由：站点是多顶层目录布局（docs + zensical.toml + tools），`git subtree` 单前缀不适用；`git filter-repo` 重写 + unrelated-histories merge 会改写全部哈希、引入 54 个 `docs:` 提交进主仓主线，收益（blame 连续性）配不上复杂度。
- **旧仓终态默认保留 active**（跳转壳）。archive 前必须验证「archive 后 Pages 仍可达」——GitHub 文档未明确保证，实施时先 archive 再 curl 旧深链，不可达立即 unarchive。验证不过就永久保留 active，成本为零。

## D4 流水线与门禁

### 新增 `.github/workflows/docs.yml`

```yaml
name: Deploy Docs
on:
  push:
    branches: [main]
    paths: ['docs/**', 'zensical.toml', '.github/workflows/docs.yml']
  workflow_dispatch:
permissions: { contents: read, pages: write, id-token: write }
jobs:
  deploy:
    runs-on: ubuntu-latest
    environment: github-pages
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-python@v5
        with: { python-version: "3.12" }
      - run: pip install "zensical==0.1.1"   # 0.x 锁版本，沿用站点现行策略
      - run: zensical build --strict          # 沿用站点门禁：warning 即失败
      - uses: actions/configure-pages@v5
      - uses: actions/upload-pages-artifact@v3
        with: { path: site }
      - uses: actions/deploy-pages@v4
```

- 与站点现行 `deploy.yml` 同构，仅加 paths 过滤 + dispatch；**不升级 action 大版本、不升级 zensical**（迁移期减少变量）。
- 启用 Pages：`gh api -X POST repos/yuumixcode/AesirFramework/pages -f build_type=workflow`（或网页 Settings → Pages → Source: GitHub Actions）。首跑 `docs.yml` 即建站。
- **与现有 workflow 并存性**：各 workflow 权限独立；docs-only 提交会照常触发 `sync-to-cnb.yml`（包内容未变 → split 结果相同、版本 tag 已存在即跳过、CNB 镜像本就该含 docs）——无害，不加 paths-ignore，保持 CNB 全量镜像语义。`update-info.json` 回写提交带 `[skip ci]`，不会反触发 docs.yml。

### .gitignore 合并

- 删 `/AesirFramework-Site/` 排除行（本地目录清理后）。
- 新增 `/site/`、`/.cache/`（zensical 产物与缓存；旧仓靠本机全局忽略兜着 .cache，主仓显式声明不依赖本机配置）。
- `/Aesir-Notebook/` 保留不动。

### 门禁清单增补（CODELY.md 迭代流程第 6 步）

- docs/ 或 zensical.toml 变更 → 主仓根 `zensical build --strict`（0 issue）。
- 本变更自身触碰 RAA 编辑器代码 + package.json → 编译预热 0 error + EditMode 全绿；Samples 未动 → `sync_samples.py --check` 零漂移；PlayMode 可豁免（无运行时行为变更）。

## D5 周边流程同步

| 对象 | 改动 |
|---|---|
| SDG API 页重生成 | 流程不变（临时目录生成 → diff → 拷差异），落点从 `AesirFramework-Site/docs/…` 改主仓 `docs/…`；`docs-site-rebuild-api-nav.py` 的 `SITE` 常量改主仓根；`zensical.toml` nav 逐页登记、index 索引行、API 类型总数三件套照旧 |
| `aesir-version-sync` 技能 | 「双仓同批发版」改「站点版本落点（changelog.md / product/releases.md）随主仓发版提交」；Notebook 仍独立 push；通读全篇改路径引用 |
| CODELY.md | 目录结构图、外部关联目录节、构建与运行、迭代文档矩阵、门禁清单 |
| memory | `memory.md` 引用条目 + `external-workflow.md` 块更新（站点约定原样保留：半角标点、中文标题显式英文锚点、表格 class 包裹、edit_uri 置空） |
| Aesir-Notebook（私有仓，不属本变更） | 提醒用户同步 `iteration-workflow.md` 等处的站点路径口径 |

## D6 风险与回滚

| 风险 | 缓解 |
|---|---|
| 新站首跑失败（Pages 未启用 / 权限 / zensical 环境） | 旧站未动，线上零影响；修好重跑 `workflow_dispatch` |
| 跳转版部署后旧深链异常 | 跳转版部署前旧站完整在线；部署后逐类抽查（首页 / architecture 深链 / scripting-api 含 `{T}` 页 / changelog / 404），异常即回滚旧仓 workflow 重部署原站 |
| archive 后 Pages 不可达 | 先验证后 archive，不可达立即 unarchive，默认保留 active |
| `site_url` 变更影响 canonical/sitemap/搜索 | 预期行为；canonical + 跳转转移权重；`--strict` 保证站内链接零告警 |
| 主仓/CI 体量增长 | docs 3.2M 文本，checkout 秒级；不做 sparse-checkout（保持简单） |
| 用户工程内旧 documentationUrl 短期不更新 | 旧地址跳转兜底，无断链；下次发版自然更新 |
| 回滚主仓侧 | revert 迁移 commit + Pages 指回旧仓（旧仓 workflow 恢复原样即可重新部署原站） |

**实施顺序原则：先建新、验证通、再切旧、后清理。** 任一阶段失败，线上旧站始终可用。