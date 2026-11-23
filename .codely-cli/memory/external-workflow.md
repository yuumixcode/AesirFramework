# 外部工作流 / 站点 / 工具行为（索引条目：`external-workflow`）

> 从主 `memory.md` 拆出的主题块。触发词：Aesir-Notebook、AesirFramework-Site、deck、huashu-design、定时任务、AI 日报、goal、ask_user、talks、脚本宏。
> 已有的 `web-source-reliability.md`（联网检索）与 `cowork-session-cleanup.md`（会话清理）各自独立，不在本块内。

## 外部目录与站点

- [2026-11-08 12:21:38] **私有笔记仓 `Aesir-Notebook/`**（gitignored，远端 `Aesir-Docs`）：独立 zensical.toml + docs/ 内容根，`~/.local/bin/zensical build --strict` 门禁。分区：`docs/standards/`（unit-testing-standards.md、design-changelog.md）、`docs/decisions/`、`docs/lessons/`、`docs/engineering/`。**无归档区，过时内容直接删（用 git 追溯）**；文件名 ASCII slug；提交 `docs:` 中文单行主题。
- [2026-11-08 03:00:15] **公开产品站 `AesirFramework-Site/`**（与 Assets 同级，被主仓 .gitignore 排除）：线上 `https://yuumixcode.github.io/AesirFramework-Site/`；本地 `~/.local/bin/zensical build --strict`（CI deploy.yml 锁 0.0.65）。`docs/architecture/`、`docs/modules/` **物理路径不可动**（SDG 重生成与 package.json documentationUrl 依赖）。**站点约定**：正文半角标点；中文标题必须显式英文锚点 `## 标题 {#module-xxx}`（slugify 删中文）；表格加 class 必须 `<div class="x" markdown="1">` 包裹；模板覆写须显式 `[project.theme] custom_dir`；版本落点随发版替换（changelog 历史段永不改写）；提交身份 zeriying@gmail.com。
- [2026-11-16 17:58:34] **AesirFramework-Site API 页增量重生成（2026-11-16 实证，替代"清目录全量 rsync"的粗暴路径）**：编辑器开着时无法 batchmode（同项目不可双开）→ 用 `tuanjie command eval --stdin` 调 `Runestone.AesirModules.ScriptDocGenerator.Editor.ScriptDocGeneratorAPI.GenerateDocsForAssembly(<程序集短名>, ScriptDocGeneratorAPI.ZensicalSettings, "/tmp/xxx")`，把 6 个 RAM 程序集（Runestone.AesirModules / .Editor / .Editor.Bootstrap / .Editor.OdinInspector / .OdinInspector / .UniTask）全量生成到临时目录，再 `diff -rq` 与站点 `docs/modules/scripting-api` 比对，只拷差异文件（本次 156 页仅 2 改 3 新，全站零格式漂移）。**settings 必须显式传 ZensicalSettings**——传 null 落到中文 Default 设置，输出的格式不是站点用的那套。生成器自带增量合并，会保留各页 `## Additional Notes` 之后的手写内容。同步必做三件事：①`docs/modules/scripting-api/index.md` 加索引行（类/结构体/接口/枚举分节，按字母序）并校准命名空间"共 N 个类型"计数；②`zensical.toml` 加**逐页** nav 条目（nav 是逐页枚举的，漏加＝页面存在但侧栏看不到）；③站点 README 的 API 类型总数（现 288 = RAA 132 + RAM 156）。收尾 `zensical build --strict`（0 issues 才算通）。Why：索引页与 nav 都不是生成器产物，只重生成页面必然留下"页在站外"的坑。辅助脚本 `.codely-cli/memory/docs-site-rebuild-api-nav.py`（SITE 常量、nav 顶层块名 `Scripting API`）。

## TJSpec 工作流与进度仪表盘（2026-11-23 增）

- **用户口径**：「走 TJspec 工作流」= openspec 式提案流程（proposal → design → tasks，评审后实施）+ **可视化面板 = TJSpec 进度仪表盘**。用户说「可视化检查 / 打开面板」时先想 dashboard，不是浏览器截站点图（本日曾误判一次）。
- **工具位置**：TJSpec 是 Codely 扩展，只装在 RTT 项目 `/Users/yuumix/Projects/Unity/RTT/.codely-cli/extensions/TJSpec/`（openspec 运行时 @fission-ai/openspec + schemas `gamespec-code`/`gamespec-game` + skills/hooks + `tools/dashboard.mjs`）；AesirFramework 未装扩展，面板服务可复用 RTT 的脚本零安装起。
- **dashboard 用法**：`node <TJSpec>/tools/dashboard.mjs [--port N] [--no-open] [--export <file.html>]`；默认端口 8321、占用自动 +1；**数据根 = 从 cwd 向上找含 `config.yaml`/`config.yml` 的最近 `openspec/`**（在哪个工程目录运行就展示哪个工程）；刷新页面即最新；tasks.md 勾选按宽松正则计数。手写 proposal/design/tasks 三件套与解析层兼容（标准工件），**缺 config.yaml 会落空态**。
- **本项目落点（2026-11-23）**：已建 `openspec/config.yaml`（schema: gamespec-code，承载仓库级工程变更，不承载游戏内容变更）；面板实例端口 **8322**（后台常驻），RTT 实例 8321 不动。首个变更 `merge-docs-site`（文档站并入主仓提案，三件套在 `openspec/changes/merge-docs-site/`，待评审，31 任务）。
- **Zensical 官方部署路线核实（2026-11-23，zensical.org/docs/publish-your-site）**：GitHub Pages 只走 Actions 构建产物发布（pip install → build → upload-pages-artifact(site) → deploy-pages），**无「docs/ 目录直接发布」模式**；发布地址跟随仓库名 `<user>.github.io/<repo>`；`site_url` 是 instant navigation/previews/错误页前提；`zensical serve` 对根路径 302 到 site_url 子路径（本地预览模拟线上路径结构）。merge-docs-site 干跑实测：新布局 + 新 site_url `--strict` 0 issues（3.5s），headless 三组页型截图与线上旧站逐像素一致（唯一差异 = repo_name 按钮文案）。
- **可视化替代路径（同日实测）**：web-access 的 CDP 代理在本机不可用——**9222 端口被 UGit 的 Electron 占用**（UA 含 UGit/Electron，不是用户 Chrome），proxy WebSocket 连接失败；替代 = Playwright 缓存 headless shell 直接 CLI 截图：`~/Library/Caches/ms-playwright/chromium_headless_shell-1234/chrome-headless-shell-mac-arm64/chrome-headless-shell --disable-gpu --hide-scrollbars --window-size=W,H --virtual-time-budget=15000 --screenshot=out.png URL`。另：`read_image` 不能并行（verifier busy），逐张串行重试；`run_shell_command` 里 `nohup … &` 起的常驻进程会随 shell 会话结束被杀，常驻服务要用 `run_in_background=true`。

## 演讲 deck / huashu-design

- [2026-11-09 12:24:13] **讲解 deck（已落地）**：`Aesir-Notebook/talks/dual-package/deck/` 多文件架构（slides/01..15.html + index.html 概览墙 + shared/tokens.css），PDF 在 `deck/out/一份源码两种包.pdf`（15 页矢量、3.1MB）；形态是**录制视频发 B 站**，方向 dir-1（Aicher：米白纸底 + 12 栏 160px 栅格 + 单信号橙 #C24A25）。讲稿开场按用户要求「点题 → 一句话总结 → 自我介绍 → 数据佐证 → 逐页」，全长 21:44；改讲稿后必须同步重算时间码。用法与重出 PDF 命令见 `deck/README.md`。
- [2026-11-09 12:24:21] 用 huashu-design 做 deck 三条本机实况：①`design_lint.py check` 依赖 Python 版 playwright（本机未装）会跳过体检 → 自写 node 脚本（playwright 在 skill 的 node_modules）做字号底线 + 矩形重叠检查；但内联 span/em 的 getBoundingClientRect 覆盖整条行盒，跨行"重叠"全是假阳性，只信截图。②`export_deck_pdf.mjs` 从 deck 目录运行时需 `ln -s <skill>/node_modules node_modules`（ESM 从脚本位置向上找依赖）；Chromium 在 `~/Library/Caches/ms-playwright`。③仍要自检 15 页逐页截图：字体（Noto Sans SC 900 + JetBrains Mono）走 Google Fonts，networkidle + ~1s 才不回落系统字体。Why: 这个流程每个环节都有静默失败点，不截图就是"以为成了"。
- 口径停在 0.32.0（待复核）：`Aesir-Notebook/talks/dual-package/` 逐字稿与 deck 的 831 条用例、包内 Documentation/ 三件套等表述，录制前需按 0.34.1 复核。

## 定时任务 / 工具行为

- [2026-11-19 09:04:05] 工作区定时任务「我的游戏 AI 日报」（id `13ee71ad-9d32-4034-9ad1-2a5a4d9130a5`，每天 09:00 Asia/Shanghai，cron `0 9 * * *`，autonomous，绑定创建时会话，模型 GLM-5.3-FLASH），2026-11-17 创建、每日自动运行把报告写入该会话；主题游戏 AI 制作方法/复盘/可玩案例/研究，输出中文 3–5 条。How to apply: 改设置走 schedule_list/schedule_update，不要另建同名任务；查历史报告按标题检索本工作区会话。检索可靠性见 `.codely-cli/memory/web-source-reliability.md`。
- [2026-11-19 02:18:50] **goal 自动续跑模式下 `ask_user` 返回空答案**（两次 text/choice 均 `{"answers":{}}`），用户在两次工具调用之间不会回应。Why: 该模式澄清拿不到输入。How to apply: 遇缺关键信息不要 ask_user 空等，在回复正文明确列出所需信息 + 可一键确认的默认路径，结束本轮让 goal 保持 active；不要因 ask_user 无返回判定 blocked（属可继续的证据收集）。

## 三篇文章 → 扩展优化（历史留档）

- [2026-11-19 02:31:12] 目标 ab45ecde「根据这三篇文章优化框架和 Extensions」的三篇文章已由用户给出，均在 Exports/：场景UI与战斗阶段问题总结.md / 道具与审查修复阶段问题总结.md / 项目初始化问题总结.md（PlaneWar 开发复盘）。已据此优化扩展：①aesir-boot-scene 模板补 `using Game.Core` + `AssetDatabase.SaveAssets()` + 删不存在的 UIPaths；②aesir-framework 主示例面板 MonoViewController<T> → AesirBasePanelViewController<T>；③aesir-ui SKILL 坑位扩到 12 条，新增 references/urp-ui-camera-stack.md。validate.py 0 错 0 警。Why: 文章反复裁决"框架包零改动、业务侧规避" + RAM 核心不硬依赖 URP，故 URP 未改框架 C#，以技能文档落地。How to apply: 三篇是提炼框架/扩展坑位的权威源；动扩展时注意可能有并发进程同改 CODELY.md/gemini-extension.json，只碰自己目标文件、勿整文件覆盖。
- [2026-11-19 02:31:20] 已作废：目标 ab45ecde 早期"三篇文章无法还原、目标 blocked"的记录，以 02:31:12 条为准。
- [2026-11-19 02:18:50] 工作区未提交的 Timer 重构（Runtime/Modules/Timer 六个子类等）属于另一个 active goal 07552a2a（session 160a5b58），不是"三篇文章优化"目标 ab45ecde 的工作。Why: 两个 goal 并行、同一工作区，易误判归属而覆盖他人改动。How to apply: 动手前先 git status 判归属，不碰不属于本目标的未提交改动。

## 交付物索引

- [2026-11-18 20:21:37] 商业级 Unity 项目目录结构调研报告在 `Exports/商业级Unity项目目录结构_完全指南.md`（451 行，未跟踪）：5 类场景完整目录树（PC Steam / 手游 / 小游戏 / 热更 HybridCLR+YooAsset / Luban 配表）→ 设计原理 → 来源附录。一手勘误：HybridCLR v9 数据目录 `Library/HybridCLR/`、官方域名 www.hybridclr.cn、Luban 文档站 www.datable.cn、微信总包 ≤30M、Zensical 官网 zensical.org、Material for MkDocs EOL 2027-05-05。
