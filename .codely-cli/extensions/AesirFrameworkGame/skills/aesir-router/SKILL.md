---
name: aesir-router
description: AesirFrameworkGame 的入口与分派技能。用 Aesir 从零做小游戏、同时用到架构与多个模块（UI/场景/事件/音频/保存）、不确定加载哪个 aesir-* 技能时使用——意图→技能分派表、组合路径、依赖门禁、误路由纠正。触发词：Aesir 全家桶、完整游戏骨架、项目起步、这个需求该用哪个技能、原生 MonoBehaviour 迁移、按 Aesir 做完整流程。
---

# AesirFrameworkGame 技能路由

本扩展打包 AesirFramework 的编码技能：5 个架构技能（入口 / 六档 / 核心 API / 决策 / 陷阱）+ 2 个工程初始化技能（项目结构 / 启动场景）+ 5 个功能模块技能。**先过下面的依赖门禁，再按本文定位意图、加载对应技能**，不要一次把全部技能读进来。

## 依赖门禁（先于分派，硬规则）

**RAA** = `cn.runestone.aesir.architecture`，**RAM** = `cn.runestone.aesir.modules`（RAM 依赖 RAA）。加载任何模块技能之前先走完这三步，顺序不能颠倒。

### ① 触发条件：先探测工程实际装了什么

**hook 通常已注入结论**——直接采信；未注入或中途装过包时按 [references/dependency-gate.md](references/dependency-gate.md) 的四条判据手工探测，**不要只凭用户口述**。

| 工程实际有 | 可用 | 失效（不许写，也不许用原生 API 顶替） |
|---|---|---|
| RAA + RAM | 全部 13 个技能 | — |
| 只有 RAA | router / framework / tiers / core-api / decisions / pitfalls / boot-scene / project-structure | `aesir-ui` / `aesir-events` / `aesir-audio` / `aesir-scene` / `aesir-save` 全部失效——五个模块一个都编不过 |
| 只有 RAM | 仅 project-structure（纯目录/文档知识，不依赖包） | 其余全部——先补 RAA |
| 都没有 | 仅 project-structure（立目录骨架时第一个用它）+ 解释概念与写法 | 其余全部——先装包 |

> `aesir-project-structure` 是唯一不依赖任何包的技能（纯目录结构与文档规范），四种状态都可用——新工程往往最先用它立骨架，再装包。

缺包时的禁止项：改用原生 `SceneManager`、自建 Canvas 栈、`AudioSource.PlayOneShot`、`static event` 总线、`File.WriteAllText` 直写存档顶替，或凭印象编造框架 API。正确做法是走 ② ③。

### ② 记忆读取：先读，结论优先于询问

调用 `read_memory`（scope=all），并检查工程内的显式约定文件（`AGENTS.md` / `CLAUDE.md` / `CODELY.md` / 项目记忆）。

- 记忆里**明确禁止**（如"不要安装 RAM 包""只允许 RAA""禁止引入新包"）→ **不安装**，只做已装包的活；需求依赖缺失模块时说明缺口与代价，让开发者决定。
- 记忆里**没有明确禁止** → 进入 ③，**必须询问**，不许静默安装。

### ③ 询问与安装：没有明确禁止就必须问

用选择题（`ask_user`）问，问句里必须说清三件事：**装哪个包**、**从哪里装**、**装完会改什么**（写 `Packages/manifest.json` 并从网络下载）。**线路顺序是 CNB 优先、GitHub 兜底**，不要把 GitHub 当首选。

| 包 | 国内直连（首选） | 国际镜像（兜底） |
|---|---|---|
| RAA | `https://cnb.cool/runestone-game/AesirFramework.git#AesirArchitecture-latest` | `https://github.com/yuumixcode/AesirFramework.git#AesirArchitecture-latest` |
| RAM | `https://cnb.cool/runestone-game/AesirFramework.git#AesirModules-latest` | `https://github.com/yuumixcode/AesirFramework.git#AesirModules-latest` |

- 两个都缺且开发者同意 → **先 RAA 后 RAM**（RAM 编译依赖 RAA）。
- 只缺 RAA 但已装 RAM → 不要手写 manifest 依赖（UPM 不支持包内 Git 依赖），走菜单 `Tools/Aesir/Modules/Install Dependencies`。
- 装完等域重载，按 ① 复验，把结论写进工程记忆，避免重复询问。
- 开发者拒绝安装 → 停留在他已有包的能力范围内，明确告知哪些需求做不了、为什么。

## 分派表

| 用户意图 | 加载 | 触发词示例 |
|---|---|---|
| 从零开始用 Aesir 写代码、Quick 档三件套、工程约定与命名 | `../aesir-framework/SKILL.md` | "用 Aesir 写"、"按架构组织"、"搭游戏骨架"、"从哪开始" |
| 选档位、升档、写 Command/Query、要完整模板（MVC/MVP） | `../aesir-tiers/SKILL.md` | "选哪种模式"、"Quick 还是 Strict"、"怎么升档"、"用 MVP 写" |
| 状态通知、可观察集合、MiniEvent、计时器、帧回调 | `../aesir-core-api/SKILL.md` | "状态变了通知 UI"、"技能冷却"、"不用协程怎么延时"、"用事件还是 ObservableValue" |
| 选 MVC 还是 MVP、为什么没有 MVVM、通知机制怎么选 | `../aesir-decisions/SKILL.md` | "MVC 还是 MVP"、"能不能用 MVVM"、"为什么不用 MVVM" |
| 报错、行为诡异、订阅收不到、提交前自查 | `../aesir-pitfalls/SKILL.md` | "Aesir 报错"、"取不到 Model"、"订阅没反应"、"内存泄漏" |
| 规划项目目录、Assets 分层、按用途还是按类型、哪些目录进 Git、搭 docs 文档站、写 CODELY.md | `../aesir-project-structure/SKILL.md` | "项目目录怎么组织"、"Assets 怎么分层"、"哪些目录进 Git"、"docs 怎么搭"、"工程骨架" |
| 初始化工程、建启动场景、生成 Bootstrap、登记 Build Settings | `../aesir-boot-scene/SKILL.md` | "创建启动场景"、"jam 开工"、"搭工程骨架"、"建主菜单场景" |
| 做界面：面板/弹窗/蒙版/敌方血条 | `../aesir-ui/SKILL.md` | "加开始界面"、"暂停面板"、"设置窗口"、"血条" |
| 模块间解耦、发布-订阅、事件过滤 | `../aesir-events/SKILL.md` | "发个消息"、"UI 响应事件"、"只让范围内敌人受伤" |
| 音效、背景音乐、音量设置 | `../aesir-audio/SKILL.md` | "加点击音效"、"背景音乐"、"音量滑条"、"切场景换歌" |
| 场景切换、加载进度、叠加场景、重开 | `../aesir-scene/SKILL.md` | "从主菜单进游戏"、"加载进度条"、"打完一关回大厅" |
| 存档/读档、云存档 | `../aesir-save/SKILL.md` | "加存档"、"存档损坏"、"版本迁移"、"自动保存"、"云存档" |

## 实战模板（本扩展独有）

| 文件 | 内容 | 何时加载 |
|---|---|---|
| `../aesir-framework/references/templates-game-skeleton.md` | 完整可玩小游戏骨架（得分/失败、HUD 订阅、生成器、开火冷却、重开） | 从零做能玩的小游戏 |
| `../aesir-framework/references/templates-migration-from-mono.md` | 原生 MonoBehaviour → Aesir 逐项迁移对照 | 已有原生代码要迁进框架 |

## 三种典型组合路径

### A. 从零做一个小游戏（最常见）

1. `aesir-project-structure` → 立目录骨架与 `docs/` 雏形（纯知识，不依赖包）。
2. 装 RAA + RAM（RAM 缺 RAA 走菜单 `Tools/Aesir/Modules/Install Dependencies`）。
3. `aesir-boot-scene` → 产出 `Bootstrap.unity` + `Bootstrap.cs`，Build Settings index 0。
4. `aesir-framework` → 写 `GameContext` + 第一个 Model + 第一个面板，跑通闭环。**默认 Quick 档，不要一上来建接口和 Command。**
5. `templates-game-skeleton.md` → 按玩法补齐得分/失败/生成器/重开。
6. 按需叠加模块技能：界面细节 → `aesir-ui`；关卡切换 → `aesir-scene`；音效 → `aesir-audio`；跨模块广播 → `aesir-events`；存档 → `aesir-save`。

### B. 已有工程，只补某一面

只加载对应模块技能即可，**不要连带重写架构**。模块技能内部都写清了"默认路径"（一行 `Show` / `LoadSceneSingle` / `PlayBgm` / `Save`），先照抄默认路径，跑不通再读该技能的排错段。

### C. 从原生写法迁移

先 `templates-migration-from-mono.md` 定迁移顺序（数据先走 Model，表现后走订阅），再 `aesir-framework` 落代码。**不要一次全迁**：先迁"状态 + HUD 订阅"，再迁"计时/音效/场景/存档"，碰撞与输入保持原生。

## 依赖与加载约束

- **RAM 五个模块技能都依赖 RAA**：用 `aesir-ui` / `aesir-events` / `aesir-audio` / `aesir-scene` / `aesir-save` 时，架构底座以 `aesir-framework`（入口）+ `aesir-tiers`（档位）为准（Context 注册、Quick 档写法、句柄）。
- **架构面已按问题拆成五篇，按需只加载一篇**：入口 `aesir-framework`、档位与模板 `aesir-tiers`、核心 API `aesir-core-api`、决策 `aesir-decisions`、陷阱 `aesir-pitfalls`。写业务时通常"入口 + 需要的那一篇"，不要五篇一起读。
- RAM 模块细节（蒙版叠遮、过滤器、淡变曲线、UniTask awaitable、事务写入）以五个模块技能为准，入口只留调用示例。
- 通知机制只有一个默认答案：**同组件内状态 → `ObservableValue<T>`；跨系统广播 → `EventModule`**。选之前看 `../aesir-decisions/SKILL.md`。

## 常见误路由（纠正后交给正确技能）

| 用户说法 | 不要做 | 应走 |
|---|---|---|
| "用 MVVM 写" | 自建 ViewModel 层 / 绑定引擎 | 说明框架暂不提供 MVVM（架构决策），引导 MVC（首推）或 MVP；理由见 `../aesir-decisions/SKILL.md` |
| "加个 UI 系统" | 自己写 Canvas 栈 / 面板管理器 | `aesir-ui`（UIRoot 四层 Canvas + UIModule 已内建） |
| "做事件总线" | 自建 `static event` 总线 | `aesir-events`（EventModule 已内建，含优先级与过滤器） |
| "切场景" | 直接 `SceneManager.LoadScene` | `aesir-scene`（混用会让模块记账失真） |
| "写个存档系统" | `File.WriteAllText` 直写 / 存档塞 PlayerPrefs | `aesir-save`（事务写入 + 备份回退已内建） |
| "面板打开是黑的/点不中" | 改 UIModule 源码 | `aesir-ui` 的三个必知陷阱（`path:` 命名参数、cullingMask 排除 UI 层、`Get<T>()` 有懒创建副作用） |
| "重开游戏数据还在" | 手动逐个字段清零 | `GameContext.Instance.Dispose()` 后再 `SceneModule.ReloadScene()`——Context 是纯 C# 单例，不随场景销毁 |

## 硬约定（全扩展通用）

1. **禁止修改框架包**（`Assets/Runestone/**`、`Library/PackageCache/cn.runestone.*`）：框架问题在业务侧绕过并记录。
2. **禁止手写 `.prefab` / `.unity` / `.asset` YAML**：一律经编辑器脚本生成。
3. **监听句柄必须绑生命周期**（`RemoveListenerWhenGameObjectOnDestroyed` / `OnDisable` 对称移除）；解绑逻辑放 `OnDestroy`（`OnClose` 只在受控销毁路径调用）。
4. 代码注释中文、标识符英文；数据类标 `[Serializable]`。