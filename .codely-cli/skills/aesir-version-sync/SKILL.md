# Aesir 包版本同步

在 AesirFramework monorepo 中同步更新两个子包（Aesir Architecture / Aesir Modules）的版本号并走发布流程。

## 仓库结构

| 包 | package.json | CHANGELOG | 包 ID |
|----|-------------|-----------|------|
| Architecture | `Assets/Runestone/AesirArchitecture/package.json` | `Assets/Runestone/AesirArchitecture/CHANGELOG.md` | `cn.runestone.aesir.architecture` |
| Modules | `Assets/Runestone/AesirModules/package.json` | `Assets/Runestone/AesirModules/CHANGELOG.md` | `cn.runestone.aesir.modules` |

- 根目录聚合 CHANGELOG：`CHANGELOG.md`（顶部"当前版本 / Current Version"表格含两包版本号；安装说明为常驻 `latest` 分支口径，**不含版本号**）
- AesirInspector 已迁出独立公开仓库，不在本仓库版本同步范围内
- 依赖链：Architecture 无 Aesir 依赖；Modules 运行时依赖 Architecture——但 package.json **已不声明该依赖**（0.28.0 实测裁决：UPM 不支持包内 Git URL 依赖），缺依赖时经 `Install Dependencies` 菜单补装，**无 package.json 依赖版本需同步**

## 分支策略（2026-11-23 反转：GitHub 主仓后）

- 主仓在 GitHub（`github.com/yuumixcode/AesirFramework`），CNB（`cnb.cool/runestone-game/AesirFramework`）是国内下载仓；**唯一跨仓同步入口是 `.github/workflows/sync-to-cnb.yml`**（main push 触发，main / 两包分支 / tags 全部 force 写入 CNB）
- 本地 `origin` = GitHub（SSH），`cnb` remote 仅 fetch / 应急用；CNB 侧零提交，PR 在 GitHub 正常 Merge
- Git URL 安装锚定**常驻滚动分支** `AesirArchitecture-latest` / `AesirModules-latest`（main push 时 subtree split 滚动更新，分支名永久固定）——README 中的安装 URL **不含版本号，发版无需替换**；国内线路仍 CNB 优先
- 钉旧版本用 Release tag（`?path=Assets/Runestone/<包目录>#v<版本>`）；**版本 tag 一旦发布即不可变**——远端已存在即复用，版本号 bump 后的首次推送才创建新 tag，禁止重推同名 tag
- 旧策略的版本分支（`AesirXxx-v<版本>`）已废弃，由 sync 流水线顺带清理两边
- GitHub Actions 推 CNB 依赖仓库 Secrets `CNB_USERNAME` / `CNB_TOKEN`（CNB 个人访问令牌，repo 读写权限），缺失时同步流水线失败

## 工作流程

### 1. 确定新版本号

向用户确认目标版本号。两包始终使用相同版本号（CI 会校验一致，不一致直接发布失败）。语义参考：功能/破坏性变更 → 次版本 +1；纯文档/修复 → 修订号 +1。

### 2. 确认变更范围

用 `git log v<上一版本>..HEAD --oneline` 与 `git diff v<上一版本>..HEAD --stat` 梳理待发布内容，确认哪些包有实际变更。

### 3. 更新 package.json（两包）

只更新 `"version"` 字段（dependencies 字段已不存在，无依赖版本联动）。

### 4. 更新两包 CHANGELOG

在 `[Unreleased]` 区块之下、上一版本之上插入 `## [x.y.z] - YYYY-MM-DD`。有实际变更的包按 Added / Changed / Fixed / Removed 分类编写；无实际变更的包写一条同步记录防止版本历史断档。

### 5. 更新根 CHANGELOG

1. 顶部"当前版本 / Current Version"表格：两包版本号（安装说明为常驻 latest 分支口径，无需替换分支名）
2. 在"当前版本"表格之后、上一版本段落之前插入 `## [x.y.z] - YYYY-MM-DD`，用 `### [architecture]` / `### [modules]` 区分包，包之间用 `---` 分隔线

**推送前必须存在该段落**——auto-release.yml 从根 CHANGELOG 提取 Release 说明，缺失则 CI 失败（version not found）。

### 6. 更新根 README（README.md / README_EN.md）

版本号出现位置：包列表表格版本列。安装章节的 Git URL / manifest.json 示例为 latest 常驻口径（无版本号），README 中的 `#v<版本>` 是**钉版示例**——随 bump 一并替换为新 tag 属预期行为，勿回改。

### 7. 更新包 README（中英 + 镜像）

- 中文：`Assets/Runestone/<包>/README.md` —— 版本徽章、Modules 的依赖版本行（`>= x.y.z`）
- 英文：包根 `Assets/Runestone/<包>/README_EN.md` —— 同上（同一目录，徽章相对路径为 `./CHANGELOG.md`）。**`Documentation/` 目录已取消（0.33.0 起），没有镜像要同步**
- 安装 URL 为 latest 常驻口径（无版本号），发版无需改动；`#v<版本>` 是钉版示例，随 bump 一并替换属预期行为

### 8. 同步 CODELY.md

正文需同步：包列表版本表、依赖关系、`## 当前状态与待办`（已发布版本 + 本轮增量 + 测试基线数字）。用脚本（python/sed）精确替换正文，**只在 `## Codely Added Memories` 之前生效**，不要动记忆区里的历史版本号。

### 9. 全仓核对

`git grep <旧版本号>` —— 剩余引用应只剩：各 CHANGELOG 历史段落（不重写）、`.github/update-info.json`（CI 回写，勿手动改）、README 中的 `#v<版本>` 钉版示例与徽章/依赖版本行（若步骤 6/7 已全部替换则不应残留）。两份 package.json 用 `python3 -m json.tool` 校验合法性。纯版本/文档变更无需编译验证。

### 10. 提交与推送

- commit message：`chore: 版本号同步至 x.y.z` + 中文要点 body
- 推送被拒（fetch first）通常是上一次发版的 CI 回写提交（`chore: 同步 update-info.json 到 vX.Y.Z [skip ci]`）——`git fetch && git rebase origin/main` 后重推
- 推 `origin`（GitHub）`main` 自动触发 `sync-to-cnb.yml`：split 两包分支 → 回写 `update-info.json`（`[skip ci]` 提交推回 GitHub main）→ 版本 tag `v<版本>`（不存在才创建）→ 新 tag 时派发 GitHub Release → main / 两分支 / tags 全部 force 推 CNB。**tag 由流水线生成，不要本地手动打 tag**
- 两边各自发 Release：**GitHub 侧由 sync 流水线 `gh workflow run` 派发 `auto-release.yml`**（`GITHUB_TOKEN` 推的 tag 不触发 push workflow，必须显式派发）；**CNB 侧收到 v* tag push 触发 `.cnb.yml` 的 tag_push 流水线**（build + `git:release` + attachments）。资产都是 RAA / RAM / RAF 三个 unitypackage
- **tag 不可变**：同名 tag 已存在时流水线跳过创建、两边不重建 Release——发版后补内容一律 bump patch 版本再走完整流程；GitHub 侧同 tag 补发走 `auto-release.yml` 的 `workflow_dispatch`（正文仍取自 tag commit）
- 验收：`git ls-remote --tags origin 'v<版本>'` 存在、两包分支已滚动到新提交、`gh release view <tag> --json assets` 有 RAA/RAM/RAF 三份、CNB 侧 Release 同样齐全（`https://api.cnb.cool/runestone-game/AesirFramework/-/releases` 带 `Accept: application/json`）、CNB main HEAD == GitHub main HEAD

### 11. 文档落点同批

产品站已并入主仓（2026-11-23）：站点 `docs/changelog.md` 补该版本段落（摘要视图）+ `docs/product/releases.md` 版本表，**随主仓发版提交一起提交推送**（触发 `docs.yml` 发布），不再独立仓推送。`Aesir-Notebook`（开发笔记）仍独立 git 仓库，同批直接 push，同步"当前基线"类陈述（版本号、测试计数、分发形态），不走主仓的 sync-to-cnb 同步。
