# 提案：文档站并入主仓（merge-docs-site）

- 状态：**待评审**（本提案只设计，不实施）
- 日期：2026-11-23
- 工作流：openspec 式提案（proposal → design → tasks，评审通过后按 tasks 实施）

## Why（为什么做）

公开产品站 `AesirFramework-Site`（Zensical + GitHub Pages）与开发主仓 `AesirFramework` 是两个独立公开仓库。发版时要两仓分别提交推送、版本号两处同步、SDG 重生成要跨仓拷贝，长期存在漂移成本。官网本就是这一个开源项目的门面，同仓更省事、更不易漂移（`Exports/Unity项目目录结构指南` 已有此对照结论）。

目标：**站点内容放进主仓根目录 `docs/`，仍由 GitHub Actions 构建发布；不再维护第二个公开仓库**（旧仓降级为跳转壳，保留历史，不删除）。

## 可行性结论：可行，无硬性障碍

逐项核实过的事实（2026-11-23）：

| # | 检查项 | 结果 |
|---|---|---|
| 1 | 主仓可见性 | `public`（GitHub Free 即可用 Pages）✅ |
| 2 | 主仓 Pages 占用 | `has_pages=false`，每仓一站，无冲突 ✅ |
| 3 | Zensical 部署与仓库布局解耦 | 官方唯一路线 = Actions 构建产物发布（`pip install zensical → build → upload-pages-artifact(site) → deploy-pages`），只需 `zensical.toml` + `docs/`，放任何仓库均可 ✅ |
| 4 | 主仓根目录命名冲突 | `docs/`、`tools/`、`site/`、`.cache/`、`zensical.toml`、`openspec/` 均不存在 ✅ |
| 5 | Unity 导入 | 根 `docs/` 在 `Assets/`、`Packages/` 之外，不被 AssetDatabase 导入、不生成 `.meta` ✅ |
| 6 | unitypackage / 包分支 | `build_unitypackage.py` 按包目录打包、`sync-to-cnb.yml` 按包目录 subtree split，根 `docs/` 不进任何包产物 ✅ |
| 7 | 站点内容自包含 | 无 `custom_dir` 模板覆写；`extra_css`/`extra_javascript` 全部指向 `docs/` 内部；383 个跟踪文件、docs 3.2M/380 文件 ✅ |
| 8 | 体量 | 主仓 ~9.4M + docs 3.2M ≈ 13M，CNB 全量镜像同步同量级增长，可接受 ✅ |

**唯一必须认真处理的代价：站点 URL 变化。** GitHub Pages 地址跟随仓库名，合并后从 `https://yuumixcode.github.io/AesirFramework-Site/` 变为 `https://yuumixcode.github.io/AesirFramework/`。存量外链依赖面（已全仓摸清）：

- 两包 `package.json` 的 `documentationUrl` / `changelogUrl`（4 处，随包分发到用户工程）
- `AesirGetStartedWindowOdin.cs` L146 `DocsUrl` —— 指向**更早的 `AesirFramework-Docs`，现已 404**（存量死链，本次一并修复）
- `CODELY.md` 外部目录节、`.codely-cli/memory.md` + `memory/external-workflow.md`、`memory/docs-site-rebuild-api-nav.py` 的 `SITE` 常量、`aesir-version-sync` 技能的双仓发版流程
- 已发布 Release 正文、搜索引擎收录、外部文章引用（不可枚举，靠旧仓全路径跳转兜底）

## What Changes（改什么）

1. **迁移**：站点 `docs/` → 主仓 `docs/`（内部路径零变化）；`zensical.toml` → 主仓根（改 `site_url`/`repo_url`/`repo_name`，`edit_uri=""` 坑位保留）；站点 `tools/`、`assets-src/`、`README.md` → 主仓 `tools/docs-site/`；`deploy.yml` → 主仓 `.github/workflows/docs.yml`（加 paths 过滤，保留锁版本 + `--strict`）。
2. **URL 治理**：主仓内全部站点引用换新地址；旧仓部署**全路径 meta-refresh + canonical 跳转版**后标记弃用（可选 archive），旧深链永久可达。
3. **流水线**：主仓启用 Pages（Source: GitHub Actions）；`docs.yml` 与现有 `sync-to-cnb.yml`/`auto-release.yml` 并存互不干扰；本地门禁清单加 `zensical build --strict`（docs 变更时）。
4. **周边同步**：CODELY.md、memory、SDG 重建脚本、`aesir-version-sync` 技能改为同仓口径；两包 CHANGELOG `[Unreleased]` 记录。
5. **不做**：不合并站点内容与包内 `Documentation~/`（两者定位不同）；不升级 zensical 版本（保持 0.1.1）；不删除旧仓；不为 URL 变更单独发版（随下次发版生效，跳转兜底）。

## Impact（影响面）

- **受影响代码**：RAA `AesirGetStartedWindowOdin.cs`（1 个常量）、两包 `package.json`（4 个 URL）→ 需编译预热 + EditMode 门禁；不动 Samples、不动 asmdef。
- **受影响流程**：发版清单（站点版本落点并入主仓提交）、SDG API 页重生成（输出比对路径改主仓 `docs/`）、迭代文档矩阵（产品站路径）。
- **不受影响**：CNB 下载仓流水线、unitypackage 产物、包分支 split、UPM 安装链路、`Aesir-Notebook` 私有笔记仓（仍独立，仅提醒同步其中路径引用）。
- **风险与回滚**：先建新站、验证通过后才切旧仓跳转；任一步失败旧站原样可用。主仓侧 revert 迁移提交即可回滚。详见 design.md。

## 待评审决策点

| # | 决策 | 默认 | 备选 |
|---|---|---|---|
| D1 | 历史保留方式 | 文件级迁移，历史留在旧仓（54 commits 可追溯） | git filter-repo 重写路径后 merge 保历史（复杂，不推荐） |
| D2 | 旧 URL 处理 | 旧仓全路径跳转版 + 弃用标记 | 自有自定义域名平滑迁移（需用户有域名，URL 零变化） |
| D3 | 旧仓终态 | 保留 active、README 标弃用 | archive（需先验证 archive 后 Pages 仍可达） |