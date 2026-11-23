# 记忆分块索引（AesirFramework）

主 `memory.md` 只保留**高频裁决 + 指向本索引的一行指针**。遇到下面主题时，按需 `read_file` 读对应块全文，不要在主索引里找细节。

| 块文件 | 触发主题 / 何时读 | 内容 |
|---|---|---|
| `version-release.md` | 发版、版本号、tag、Release、GitHub 主仓/CNB 下载仓、sync-to-cnb、反推按钮、打包、门禁基线数字、git 历史瘦身 | 含 2026-11-23 双仓反转架构、铁律「禁止连续推同名 tag，微小更新也 bump patch」、0.33.0/0.34.0/0.34.1 全过程（CNB 主仓时代）、CNB API/SSH/钥匙串 |
| `testing.md` | 测试怎么写/在哪跑、EditMode/PlayMode、batchmode、TestRunnerApi、消费者工程验证 | 测试执行硬约束、TestRunnerApi 两条路径、batchmode 坑位、L3 消费者验证 |
| `unity-tooling.md` | 编译冻结、域重载、条件编译、asmdef、Inspector、场景保存、tuanjie CLI、Bridge、引擎坑位、删除走废纸篓、Unity LTS 铁律 | 编辑器/脚本行为坑位（与 testing 分工见块首） |
| `odin-migration.md` | Odin、Sirenix、UI Toolkit、USS、样式开关、无 Odin 验证、YokiFrame、Drawer | 已完结 goal 的全过程实录 + Odin 面板/绘制坑位 |
| `ui-instantiation.md` | UIModule 停用模板、Instantiate、Resources.Load 资产语义、Addressables、(Clone)、DDOL | 机制已移除；记录质疑—推翻过程与资产语义实测定论 |
| `framework-core.md` | 框架定位展开、各模块正文外定稿细节、UPM、菜单 priority、AesirTimer、世界空间 UI、Timer 重构、目录/命名裁决、扩展与技能安装 | 高频裁决的展开 + 模块补充 |
| `external-workflow.md` | Aesir-Notebook、AesirFramework-Site、deck、huashu-design、定时任务、goal/ask_user、三篇文章 | 外部站点/工具行为/历史留档 |
| `web-source-reliability.md` | 联网检索、AI 日报取源、哪些站点可抓 | 本机实测可用/不可用来源表 |
| `cowork-session-cleanup.md` | Cowork 会话批量清理、sessions.sqlite、IPC 删除 | 数据落点与清理顺序 |
| `docs-site-rebuild-api-nav.py` | 站点 API 页重生成的索引/nav 修复脚本 | 可执行脚本（非 md） |

## 维护约定

- 新增长条目时：**写进对应块文件**（或直接新建块），主 `memory.md` 只加一行指针 / 更新本索引。
- 主索引里的内容应是"一个月后跨功能仍需要、且一句话能说清"的裁决；过程性记录、踩坑细节、历史留档一律进块。
- 本目录在 `.gitignore` 白名单内（`memory/` 入库），块文件随仓库版本化。
