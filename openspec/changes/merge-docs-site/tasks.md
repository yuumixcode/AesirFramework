# 任务清单：文档站并入主仓

> 评审通过后执行。顺序原则：**先建新、验证通、再切旧、后清理**。
> 本机 git 命令一律先 `export PATH=/opt/homebrew/bin:$PATH`。

## 阶段 0：准备

- [x] 0.1 确认旧仓工作区干净、无未推送提交（`git status` + `git log origin/main..HEAD`）；确认旧站 `site/` 为最后一次成功构建产物（必要时 `zensical build --strict` 重建）
- [x] 0.2 主仓从 `main` 拉分支 `feat/merge-docs-site`；确认主仓工作区无他人未提交改动
- [x] 0.3 评审决策点落定：D1 历史保留（默认 A 文件级）、D2 URL 策略（默认 A 换路径+跳转 / B 自定义域名）、D3 旧仓终态（默认保留 active）

## 阶段 1：迁移进主仓（一个 commit）

- [x] 1.1 拷贝 `AesirFramework-Site/docs/` → 主仓 `docs/`（380 文件，含二进制图标/字体，逐文件比对数量与内容一致）
- [x] 1.2 拷贝 `zensical.toml` → 主仓根；改 `site_url`/`repo_url`/`repo_name` 三键；`edit_uri = ""` 与坑位注释原样保留
- [x] 1.3 站点 `tools/serve-preview.py`、`tools/shoot.mjs`、`assets-src/logo.png`、`README.md` → 主仓 `tools/docs-site/`；核对两个脚本的 cwd/相对路径假设并修正；README 内路径引用改同仓口径
- [x] 1.4 站点 `deploy.yml` → 主仓 `.github/workflows/docs.yml`：加 `paths: ['docs/**', 'zensical.toml', '.github/workflows/docs.yml']` + `workflow_dispatch`；保留 `zensical==0.1.1` 锁定与 `--strict`
- [x] 1.5 主仓 `.gitignore`：新增 `/site/`、`/.cache/`；暂留 `/AesirFramework-Site/` 排除行（阶段 5 清理时删）
- [x] 1.6 本地验证：主仓根 `~/.local/bin/zensical build --strict` → 0 issue；`zensical serve` 抽查首页 / architecture / modules / scripting-api（含 `{T}` 页）/ changelog / 搜索
- [x] 1.7 提交 `docs: 产品站并入主仓（docs/ + zensical.toml + docs.yml）`（精确路径 add，勿 `-A`）

## 阶段 2：主仓引用替换与门禁（一个 commit）

- [x] 2.1 两包 `package.json`：`documentationUrl`/`changelogUrl` 共 4 处换新前缀 `…/AesirFramework/…`
- [x] 2.2 `AesirGetStartedWindowOdin.cs` L146 `DocsUrl`：`AesirFramework-Docs`（404 死链）→ `https://yuumixcode.github.io/AesirFramework/`
- [x] 2.3 两包 CHANGELOG `[Unreleased]`：RAA 记 Fixed（Getting Started 文档链接 404）+ Changed（文档站同仓、URL 更新）；RAM 记 Changed（package.json 文档链接更新）；根 CHANGELOG 同步
- [x] 2.4 CODELY.md：目录结构图加 `docs/`、`tools/docs-site/`、`zensical.toml`、`site/`、`.cache/`；外部关联目录节删 Site 条目改同仓口径；迭代文档矩阵与门禁清单加 `zensical build --strict`
- [x] 2.5 门禁：编译预热 0 error → EditMode 全绿（解析 XML）→ `sync_samples.py --check` 零漂移；PlayMode 豁免（无运行时行为变更）
- [x] 2.6 提交 `fix: 文档站链接统一指向同仓新址（含 Getting Started 404 死链）`

## 阶段 3：启用主仓 Pages 并验证新站

- [x] 3.1 启用 Pages：`gh api -X POST repos/yuumixcode/AesirFramework/pages -f build_type=workflow`（失败则网页 Settings → Pages → Source: GitHub Actions）
- [x] 3.2 推送分支 → PR 合并到 main（正常 Merge，走 GitHub 主仓流程）；观察 `docs.yml` 首跑与 `sync-to-cnb.yml` 照常运行
- [x] 3.3 新站验证：`https://yuumixcode.github.io/AesirFramework/` 200；抽查深链（architecture/、modules/events/、scripting-api 含 `{T}` 页、changelog/、product/releases/）；instant navigation / 搜索 / 自托管字体 / 移动端渲染正常
- [x] 3.4 确认 CNB 镜像 main 已含 `docs/`（force push 正常），CNB tag 流水线未被误触发

## 阶段 4：旧仓切跳转版（新站验证通过后才执行）

- [x] 4.1 旧仓写 `tools/make-redirects.py`：遍历 `site/**/*.html` 生成 `redirect/` 全路径跳转页（meta refresh 0 + canonical 新址 + 手动链接；`404.html` → 新站首页）
- [x] 4.2 旧仓 `deploy.yml` 改纯静态上传（删 pip/zensical 步骤，`path: redirect`）；提交推送，观察部署成功
- [x] 4.3 跳转验证：旧首页、旧 architecture 深链、旧 scripting-api `{T}` 页、旧 changelog、旧 404 → 全部落到新站对应页；`curl -sI` 抽查 + 浏览器实点
- [x] 4.4 旧仓 README 顶部弃用说明（新地址 + 2026-11-23 迁移日期 + 本仓仅保留跳转与历史）；repo description 同步
- [x] 4.5 （可选）不 archive：D3 评审默认保留旧仓 active（零成本跳转壳），无 archive 动作

## 阶段 5：周边同步与清理

- [x] 5.1 `memory/docs-site-rebuild-api-nav.py`：`SITE` 常量改主仓根；跑一次干跑验证路径解析
- [x] 5.2 `aesir-version-sync` 技能：站点版本落点改「随主仓发版提交」，通读全篇改路径/流程引用；Notebook 仍独立 push 的口径保留
- [x] 5.3 memory 更新：`memory.md` 站点引用条目（update_memory）+ `memory/external-workflow.md` 块改同仓口径（站点写作约定原样保留）
- [x] 5.4 提醒用户：`Aesir-Notebook` 内 `iteration-workflow.md` 等站点路径口径需在其私有仓同步（不属本变更提交）
- [x] 5.5 本地 `AesirFramework-Site/` 目录确认干净且全部已推送后移废纸篓（可恢复）；主仓 `.gitignore` 删 `/AesirFramework-Site/` 行，提交
- [x] 5.6 收尾提交推送；确认主仓 `git status` 干净、两仓线上状态符合预期

## 验收标准

1. 新站 `…/AesirFramework/` 全量可达，`--strict` 0 issue，搜索/导航/字体正常
2. 旧站任意深链（含 `{T}` API 页）跳转到新站对应页；旧仓历史保留
3. 主仓内无残留 `AesirFramework-Site` / `AesirFramework-Docs` 引用（git grep 验证，CHANGELOG 历史段除外）
4. 编译预热 0 error、EditMode 全绿、sync_samples 零漂移
5. 发版流程口径统一：站点版本落点随主仓提交，不再双仓推送