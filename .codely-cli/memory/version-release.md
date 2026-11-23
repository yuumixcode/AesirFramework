# 版本发布 / Tag / GitHub-CNB 双仓（索引条目：`version-release`）

> 从主 `memory.md` 拆出的主题块。触发词：发版、版本、tag、Release、CNB、GitHub 主仓、sync-to-cnb、反推按钮、build_unitypackage、git 历史瘦身、门禁基线。
> 主索引只保留一句话指针；本文件是全文真源。

## 当前架构：GitHub 主仓 + CNB 国内下载仓（2026-11-23 反转，用户裁决）

- **反转原因**：CNB 的 AI 提交 Issue / PR 虽有免费额度但效果不佳，开发主流程回归 GitHub。此前 2026-11-16～11-23 为 CNB 主仓时代（下方历史条目均出自该时期）。
- **本地 remote**：`origin` = `git@github.com:yuumixcode/AesirFramework.git`（SSH，推 workflow 文件不受 OAuth scope 限制）；`cnb` = `https://cnb.cool/runestone-game/AesirFramework`（仅 fetch / 应急）。
- **GitHub `sync-to-cnb.yml`（唯一跨仓同步入口）**：main push → 包分支 split + 裁剪推 GitHub → `update-info.json` 回写（`[skip ci]` 提交推回 GitHub main，带 rebase 重试）→ 版本 tag（`git ls-remote` 已存在即跳过，沿承 PR #7 不可变语义）→ 新 tag 时 `gh workflow run auto-release.yml --ref main -f tag=...` 派发 GitHub Release → **main / 两包分支 / tags 全部 force 推 CNB** + 对齐 CNB 多余 v* tag + 清理两边废弃 `Aesir*-v*` 分支。需仓库 Secrets **`CNB_USERNAME` / `CNB_TOKEN`**（CNB 个人访问令牌 repo 读写；CNB 流水线内置 `CNB_TOKEN` 出不了 CNB）。
- **GitHub Release 触发关键**：`GITHUB_TOKEN` 推的 tag 不触发 push 触发的 workflow（平台限制，与旧 git-sync 不触发是两回事）；`workflow_dispatch` / `repository_dispatch` 是豁免事件，故用显式派发（需 `actions: write`）。
- **CNB `.cnb.yml` 只剩两条**：`v*` tag_push → 三份 unitypackage + CNB Release（外部 git push 推 tag 同样触发）；`web_trigger_push_github`（按钮在 `.cnb/web_trigger.yml`，分支详情页手动触发）把 CNB AI 修复分支 force 推 GitHub 同名分支，脚本禁止 main / `Aesir*-latest`，浅克隆先 `--unshallow`。
- **CNB AI Issue 闭环**：CNB 提 Issue → AI 在临时分支修复 → 点「反推此分支到 GitHub」→ GitHub 开 PR 正常 Merge → sync 流水线 force 写回 CNB。旧纪律「GitHub 零提交 / 不点 Merge / main 永不 force」全部作废；新纪律是 **CNB 侧零提交**。
- **PR #7（2026-11-23 合并，CNB AI 产物）**：rolling tag 改「远端已存在即跳过」——CNB 侧删 tag 会被后台自动 tag 规则立即重建导致后续 push 被拒。该语义已迁入 sync-to-cnb.yml，与 2026-11-20「禁止连续推同名 tag」铁律一致。
- 反推按钮的 `GIT_ACCESS_TOKEN` 是 OAuth 令牌，无 workflow scope 时改 workflow 文件的分支会被 GitHub 拒推——这类修复走本地 SSH 直推。
- **首跑已验证（2026-11-23，run 45140383615，提交 50cb4456）**：全步骤绿；update-info 无变化跳过回写、v0.34.1 tag 已存在跳过创建、Dispatch 按预期 skipped；main / 两包分支 / tag 四项引用 GitHub 与 CNB 完全一致；CNB Release v0.34.1/v0.34.0/v0.33.0 各 3 资产、GitHub Release v0.34.1 三资产齐全。**全链路已验证**：0.35.0 发版覆盖新 tag 创建 + Release 派发 + CNB tag_push 触发（见下「0.35.0 发布」节）；仅剩反推按钮待下次 CNB AI Issue 时实测。

## 铁律：禁止连续推送相同（同名）版本 tag（2026-11-20 用户明确）

> 规范原文（主索引同句）：**禁止连续推送相同的版本 tag**——同一版本 tag 推送后被 CI 消费，再推同名 tag 会破坏幂等与可追溯；哪怕只是很小的更新，也**必须 bump 补丁版本**（patch，如 0.34.1→0.34.2）再走完整发版流程。

- **同一个版本 tag（如 `v0.34.1`）推送成功后，禁止为任何后续改动再次推送同名的 tag**——哪怕只是补一条 CHANGELOG、一句文档、一个 memory 提交。
- 任何再小的更新，也**必须先把版本号 bump 到新的补丁版本**（patch，如 `0.34.1 → 0.34.2`），改完两包 `package.json` / 根 CHANGELOG / 文档站版本落点，再由流水线生成**新的** tag。
- **Why:** 滚动 tag 是给"当前在研版本、尚未定稿"用的；一旦 tag 已随发版推送并被 CI 消费（CNB Release / GitHub Release 都 checkout tag 指向的 commit 生成正文），重推同名 tag = 同一 tag 指向不同 commit，Release 正文与用户下载内容不确定，镜像同步也失去幂等基准。
- **How to apply:** 发版后若要补内容，走"补丁版本"完整流程，不要 `git push -f <cnb-url> refs/heads/main:refs/tags/v<同名版本>`；那条 force 滚 tag 的老办法仅适用于"tag 尚未推送 / 同一发版窗口内 CHANGELOG 未定稿"的场景，tag 一旦推出即作废该路径。
- 历史背景：2026-11-19 曾用"把 tag 滚到含完整 CHANGELOG 的 main HEAD"修 CNB Release 正文（见下「发版后补 CHANGELOG」条），那是 tag 首次发布前的补救；本规则确立后，同类需求一律 bump patch 版本。

## 0.35.0 发布（2026-11-23，GitHub 主仓时代首次发版，全链路验证通过）

- 事件模块大版本（RAM）：取消传播 + `AesirEventDiagnostics` 公开诊断 API + Event Monitor/Log/Tester 三窗口（UI Toolkit + Odin 双版）+ 分发精简（Type 键注册期有序、去 object[]/反射、重入缓冲复用、负缓存）；**三项破坏性**：移除 `[AesirListener]` 特性订阅（只留 Script 订阅）、同实例重入 fail-fast、低频 API 收窄（Clone/字符串键删除、BindingInfo internal）。SDG 开放泛型崩溃修复。RAA 为格式化同步版。新示例 04_Cancellation / 05_Diagnostics。
- **发版前审查拦下 8 处 WIP 语义损坏**：外部格式化批引入 6 处属性自引用无限递归（AbstractTimer 5 + SceneModule 1）+ 2 处可观察集合 backing 字段误删 `[SerializeField]`（Odin 持久化依赖）。手法：剥离注释的 token 级 HEAD 比对逐文件审查 + Python 反向引用正则扫描（grep -E 不支持反向引用，扫描无效）。
- **门禁（0.35.0 基线）**：编译 0 error、EditMode **881/878 pass/0 fail/3 skip**、PlayMode **36/36**、镜像零漂移、扩展 validate 0/0、两站 zensical --strict 无 issue。含用例文件 76 / Tests .cs 78 / 属性 730 [Test]+67 [TestCase]+36 [UnityTest]。
- **新流水线完整链路首验**：push main（6 批提交）→ sync-to-cnb run 45144649833 全绿（split → update-info 回写 6c87309f → **v0.35.0 新 tag 创建** → gh workflow run 派发 auto-release run 45144708880 成功 → force 推 CNB）→ **CNB 收到外部 tag push 正常触发 tag_push 流水线**，CNB Release v0.35.0 三资产 + body 含 [modules]；GitHub Release 三资产；两边 main/包分支/tag 全一致。tag 不可变语义同跑验证（0.34.1 首跑跳过创建）。
- 三仓同批：主仓 GitHub main→6c87309f（含回写）、Notebook 279763a（补 design-changelog 18-21 + 决策索引 + 基线）、Site 1736f56（changelog/releases/事件页/API 页重生成）。
- 本机产物排除口径：Exports/、ProjectSettings.asset 改动、TimelineSettings.asset、.codely-cli/settings.json 不提交；manifest bridge 1.0.93 随 chore(deps) 提交。第三方参考资产 Criminal Makers/Game Event Hub 已移废纸篓（用户裁决）。
- **并发格式化防护实录**：发版中途用户另开的格式化进程两次改动工作区（17:02 批 + 示例字段重排），按「停下→diff 审查→ask_user 裁决→并入」处理，最终树重跑门禁后才推送。

## 门禁基线（当前 0.35.0，2026-11-23）

- [2026-11-23] **发布基线**：当前已发布 **0.35.0**（2026-11-23 事件模块大版本）；上一稳定版 0.34.1（2026-11-20）、0.34.0（2026-11-19）。Unity **2022.3.62f3c1（Unity 中国版，不是团结引擎）**、URP 14.0.12、C# 9；RAA 16 asmdef / RAM 17 asmdef + 7 asmref（0.34.0 后）。门禁：EditMode batchmode **872 total / 869 pass / 0 fail / 3 skip**（skip = Addressables 自适应 ×2 + batchmode 未保存场景预期 Ignore）、PlayMode **36/36**（RAA 20 / RAM 8 / UniTask 8）、`sync-samples --check` 零漂移、扩展 `validate.py` 0 错 0 警、两个文档站 `zensical build --strict` 无 issue。**改测试/asmdef 必须重跑；纯文档补丁可豁免 PlayMode**。最密测试文件（触碰时优先收敛）：`BinderCodeGeneratorTests`、`AesirUpdateServiceTests`、`SourceParsingTests`、`AudioModuleTests`、`EventModuleTests`、`XmlSummaryToolTests`。

## 发版流程（skill `aesir-version-sync`）

- [2026-11-08 02:40:00] ①`git grep <旧版本>` 全仓摸底（**输出勿截断**；版本徽章在 README L6 且链接指向 CHANGELOG，`grep -vi changelog` 会误滤，需单独摸 `shields.io/badge/version`）→ ②两包 package.json / 根 CHANGELOG（当前版本行 + 固定分支名 + 新段落）/ 两包中英 README / 根 README / CODELY.md 正文（**python 只在 `## Codely Structured Memories` 之前替换**）/ 文档站 6 件套 → ③分批提交（fix → feat → fix → docs → chore 版本批）→ ④CI 发 Release + 回写 `update-info.json`（`[skip ci]`，推完需 pull）+ 滚动 `*-latest` 分支 → ⑤文档站推送 + Deploy 检查。**坑**：根 CHANGELOG 插入用 `%` 格式化会被正文 `%` 撞格式符（用占位符 `.replace()`）；`git add -A` 会带上测试产物；改名文件要连 `.meta` 一起 add。

## 0.33.0 发布（2026-11-16）

- [2026-11-16 19:35:55] PlaneWar 迁出包外到 `Assets/PlaneWar`（示例回到 11 个，破坏性）、包内 `Documentation/` 取消（只留 `Documentation~/`）、AI 技能统一到 `.codely-cli/extensions/AesirFrameworkGame`、CI 主位迁 CNB、RAM 世界空间 UI Canvas、RAA AesirTimer。三仓同批推送：主仓 CNB（main 7a75a93 之后 / tag v0.33.0 / 两包分支 0.33.0 / CNB Release 三资产）、`Aesir-Notebook`（重构成 standards/decisions/lessons/engineering 四分区 + talks/ 演讲材料）、`AesirFramework-Site`（changelog 加 0.33.0、版本表、生态/FAQ/兼容性页对齐，Pages 部署成功、线上首页/changelog/releases 均 200 且显示 0.33.0）。GitHub Release v0.33.0 三份 unitypackage 已用 `gh release create --repo yuumixcode/AesirFramework` 本地构建发布。

## 0.34.0 发布 + GitHub Release tag 根治（2026-11-19）

- [2026-11-19 13:01:12] 三仓同批推送（主仓 CNB main→fce5b436，流水线回写 747653ff/滚动 tag v0.34.0/两包 latest 分支 cb443569·ae466357/git-sync 同步 GitHub 镜像全部成功；Aesir-Notebook bb4e36c、AesirFramework-Site b773759 直接推 GitHub）。本地分批提交 7 批（AI 扩展 1.3.0 / 编辑器去 Odin+UI Toolkit / Timer 分层重构 / 配置资产显式创建 / UI 实例化简化+Drawer 去 Odin / 移除 Binder-SDG 窗口 / 版本同步），排除 Exports/*.exit/ProjectSettings 本机产物。门禁：EditMode 872/869 pass/0 fail/3 skip、PlayMode 36/36、编译 0 error。**GitHub 侧 auto-release.yml tag 不触发 Actions 的坑位再次复现**（v0.34.0 tag 已送达但无 run，最新 run 仍是 v0.33.0），已按兜底用 `.github/scripts/build_unitypackage.py` 本地构建 RAA/RAM/RAF 三包 + gh release create 发布成功（三资产齐全）。CNB 侧 tag_push Release 无法从网页确认资产（页面返回 CHANGELOG 内容）。
- **tag 触发根治**：CNB v* tag_push 新增 publish-github-release stage 经 REST API 直发，auto-release.yml 降为幂等兜底+workflow_dispatch。端到端验证：push main 触发 tag 强推→CNB 重建 Release，资产 createdAt 04:33（晚于手动 04:21）。**改 workflow 文件会卡 git-sync main 同步**：CNB OAuth token 无 workflow scope，GitHub 拒推 `.github/workflows/*`；包分支/tag 照常、main 卡旧提交。兜底用 SSH 直推（不受 OAuth scope 限）：`git push git@github.com:yuumixcode/AesirFramework.git HEAD:main`。已记入 CODELY 坑位。发版验收必须核对 GitHub main HEAD == CNB main HEAD，不能只看 tag/包分支。

## 0.34.1 补丁版发布（2026-11-20）

- [2026-11-20 14:01:28] 纯测试基础设施修复，无运行时代码变更：修复 fdc5724b 误删 4 个测试 asmdef 的 Sirenix.*.dll 导致的 CS0012 + SDG 夹具移出包内 immutable folder。门禁与 0.34.0 一致（编译 0 error、EditMode 872/869/0/3、PlayMode 36/36，均 batchmode 实测）。三仓同批推送：主仓 CNB main→bd32e42a（流水线回写 1c619dcc/tag v0.34.1/两包 latest 滚动），Notebook 9a0f179、Site 3876fb2 直接推 GitHub。GitHub Release v0.34.1 三资产齐全（CNB publish-github-release 直发主路径，本次不依赖 tag 触发 Actions）。**踩坑**：发版后补的一条 memory 提交（423ed6de）推送后，CNB main 前进但 GitHub 镜像 main 停在回写提交——git-sync 未跟上（非 workflow scope 问题，此次无 workflow 改动），按兜底用 SSH `git push git@github.com:yuumixcode/AesirFramework.git HEAD:main` 快进对齐成功。tag 仍指向 1c619dcc（含完整 CHANGELOG 的版本同步提交）正确，memory 提交不含 CHANGELOG 变更无需滚 tag。

## CNB / GitHub 镜像实操坑位

- [2026-11-16 19:35:59] ①CNB 导入仓库会重写提交哈希，GitHub main 与 CNB main **无共同祖先**（各有独立 `Initial commit`），CODELY 里「fetch → merge 回 CNB」的规程失效（`refusing to merge unrelated histories`，且文件全是 added-by-both 冲突），`git-sync` 的 main 阶段失败后包分支与 tag 同步阶段一并被跳过；处置是确认差异只剩 `.cnb.yml` 且已被 CNB 侧新版本取代后，一次性 `--force-with-lease` 对齐 GitHub 的 main 与两包分支。②GitHub 侧 tag 推送不触发 `auto-release.yml`（`actions/runs` 无记录、repo events 无 PushEvent，重推 tag 亦然），兜底为本地跑同一个 `build_unitypackage.py` 再用 `gh release create <tag> --repo <owner>/<repo> --notes-file …` 发布（本仓无 GitHub remote，`gh` 必须显式 `--repo`）。
- [2026-11-16 21:03:34] **CNB（cnb.cool）刻意不支持 SSH 访问 Git，换成 `git@cnb.cool:` 远端必然失败，不要再尝试**。依据三条：①官方一手 `https://docs.cnb.cool/zh/guide/git-access.html` 专节「不支持 SSH」：认证只有 HTTPS + 访问令牌，用户名固定 `cnb`。②2026-11-16 实测排除网络/DNS/客户端因素：`cnb.cool` / `ssh.cnb.cool` / `git.cnb.cool` 解析到 28.0.0.44/.121/.122，四解析器一致；三域名 22 端口 TCP 可连但 SSH 在 `kex_exchange_identification` 即被服务端 `Connection closed`（Homebrew OpenSSH 10.5 与系统一致）；443 是正常 HTTPS（DigiCert `CN=*.cnb.cool`、`O=Tencent Technology (Shenzhen)`）；SSH over 443 与 `git ls-remote ssh://git@cnb.cool/...` 同样失败 → 服务端根本没跑 SSH git 服务。③平台「添加 SSH 公钥」（类型下拉 `Signing Key`）只服务于提交签名验证与云原生开发 Remote-SSH，与 git 传输无关。网上 CSDN《腾讯云 CNB 上手实践》称 `ssh -T git@cnb.cool` 可免密推送，属套用 GitHub/Gitee 模板的错误内容（疑 AI 生成），勿照做。**免密的合法替代**：访问令牌 + 凭证存储（本机已用钥匙串 ACL 全放行）、CI 用只读《部署令牌》（用户名仍 `cnb`）。
- [2026-11-19 13:14:23] CNB OpenAPI 实操：本地 `git credential fill` 取到的 cnb.cool token 只有 code 权限——`PATCH /{repo}/-/releases/{id}` 因缺 `repo-release:rw` 返回 403、`GET /{repo}/-/build/logs` 因 scope 不足返回 403（无法从本地查流水线状态）；读 Release 用 `GET /{repo}/-/releases` 必须带 `Accept: application/json`（否则 406），token 经 `printf "protocol=https\nhost=cnb.cool\n\n" | git credential fill` 取（账号 cnb，勿回显）。**How to apply:** 查 CNB Release 正文走带 header 的 GET；要改正文别用本地 token PATCH，走滚动 tag 触发 v* 流水线（见上方铁律：tag 已推出后一律 bump patch 版本）。
- [2026-11-20 13:59:10] CNB Release REST API 正确端点是 `https://api.cnb.cool/<owner>/<repo>/-/releases`（独立 api 子域），带 `Accept: application/json` + `Authorization: Bearer <token>` 返回 Release JSON 数组；此前误用的 `https://cnb.cool/api/<owner>/<repo>/-/releases` 返回 `{"errcode":3,"errmsg":"Invalid argument"}` HTTP 400。token 仍走 `printf "protocol=https\nhost=cnb.cool\n\n" | git credential fill` 取（只有 code 权限，够读 Release）。**Why:** 端点写错会误判成"Release 不存在/无权限"，白走排查弯路。**How to apply:** 验收 CNB Release 正文与资产用 api.cnb.cool 端点，检查 body 含 `[architecture]`/`[modules]`、assets 有 RAA/RAM/RAF 三份。
- [2026-11-16 20:49:01] 本机 cnb.cool 钥匙串条目（互联网密码，账号 `cnb`）的 ACL 已用 `security add-internet-password -a cnb -s cnb.cool -r htps -w "$TOKEN" -A -U` 改为允许所有应用，不再弹「git-credential-osxkeychain 想要访问密钥串中的密钥」；此前反复弹窗的根因是两份助手二进制并存（Homebrew `Cellar/git/<版本>/libexec/...` 与 `/Library/Developer/CommandLineTools/usr/libexec/...`）+ Homebrew 升级换路径使 ACL 失效。若日后又弹，按同一命令重跑一次即可。

## git 历史瘦身 / 入库边界

- [2026-11-16 18:25:20] **git 历史瘦身可复用姿势**：①工作区脏时不要在正本仓跑 `filter-repo`（它拒绝脏树，`--force` 只跳过"非全新克隆"检查）——用 `/tmp` 的 `git clone --mirror .` 重写并推远端，再在正本仓 `git fetch --force` + `git reset --mixed origin/main`，工作区文件一个不动。②重写后体积不降，多半是本地临时引用（如 `refs/tmp/*` 对比快照）和 reflog 仍指向旧对象：删 ref → `git reflog expire --expire=now --all` → `git gc --prune=now`（本例 146MB → 8.8MB）。③默认 fetch refspec 只覆盖 heads，tag 必须显式 `git fetch --force origin '+refs/tags/*:refs/tags/*'` 才跟随重写。④验证改写无副作用：拿备份 `.git` 的 `ls-tree -r <旧提交>` 减去被剔路径，与新 HEAD 逐文件 diff，差异应为 0 行。
- [2026-11-16 18:25:16] **仓库历史重写与 Codely 资产入库裁决**：①历史已剔除 `.codely-cli/extensions/TJGenerators`（115MB）与旧 `.codely.packages`：main `08492be → c7eb41b`、476 提交、25 个 tag 全部重算并强推，本地 `.git` 196MB → 8.8MB；其他机器必须**重新 clone**（pull 会因历史不连续失败），远端 `refs/notes/npc-push` 仍指向旧哈希（悬空无害）。②入库口径：只入库自研扩展 `AesirFrameworkGame` + `.codely-cli/{settings.json, memory.md, memory/, skills/, RESTORE.md}`；TJGenerators / TMPChineseFont / codely-unity-lsp-server / vtsls 以及 UnityInsight 索引、auto-saves、tool-outputs、clipboard 一律不入库，按 `.codely-cli/RESTORE.md` 的 zip URL 重装；`.gitignore` 另有凭证兜底。**How to apply:** 新克隆**先装 TJGenerators 再打开 Unity**——`Packages/manifest.json` 用 `file:../.codely-cli/extensions/TJGenerators/Packages/cn.tuanjie.ai.generators` 引用该路径，缺了相关脚本不编译。

## 打包脚本 / unitypackage

- [2026-09-05 16:20:30] 发布脚本 `.github/scripts/build_unitypackage.py`（纯 Python，**禁止换第三方 CLI**——实测产出重复 GUID / 全零 meta / 丢文件夹条目）：跳过 `~` 后缀与 `.` 开头目录（`Samples~` / `Documentation~` 不入包，.meta 与主位同 GUID）、排除 IncludeDependencies 拖入的内置包源、**无 Odin 铁律**（`ODIN_INSPECTOR` defineConstraints 原样保留）。
- [2026-11-08 13:07:15] **unitypackage 导出必须整目录剔除 `Tests/`**（含目录 `.meta`）：只删 asmdef 会让 `.cs` 并入 `Assembly-CSharp` 报 CS0246，比不删更糟。理由：测试断言框架内部契约，会污染消费者 Test Runner，且引用 Odin DLL 可能让缺 Odin 的用户编译失败。**边界**：仅 unitypackage；UPM 形态维持现状（消费者 testables opt-in 可编译，见 framework-core 块 UPM 条）。
