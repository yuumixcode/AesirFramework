# AesirFrameworkGame

## 概览

AesirFramework（Runestone Yuumix 出品的 Unity／团结引擎框架）的 AI 编码技能包，也是 Aesir 全部 AI 技能的**唯一主位**。

面向用 Unity／团结引擎 + AesirFramework 做**独立游戏**的开发者：从零规划目录与搭工程骨架、组织 Context/Model/View/Controller（默认最低档 Quick）、做面板与弹窗、发事件解耦、加音效与背景音乐、切场景与加载进度、存档读档与备份回退、把原生 MonoBehaviour 代码迁进框架。

- **13 个技能 + 20 篇 references**：技能正文只放决策、默认调用路径与坑位结论；完整代码模板、教程与展开原理在 `references/`，按需加载。
- **依赖门禁自动化**：会话启动时经 hook 探测工程装没装 RAA/RAM，把结论与可用技能范围注入上下文——AI 拿到的是事实，不是待办。
- **篇幅护栏**：技能正文 ≤6000 字符、单条触发描述 ≤260 字符、描述总和 ≤2800，由 `scripts/validate.py` 机械把关，防止长期迭代中膨胀。

框架本身分两个 UPM 包：**RAA**（`cn.runestone.aesir.architecture`，渐进式 MVC/MVP 架构）与 **RAM**（`cn.runestone.aesir.modules`，UI／事件／音频／场景／保存五个功能模块）。RAM 依赖 RAA。

## 技能一览

| 技能 | 覆盖 | references |
|---|---|---|
| `aesir-router` | 入口与分派：依赖门禁、意图→技能、组合路径、误路由纠正 | 1 |
| `aesir-framework` | 架构底座：核心裁决、Quick 档三件套、约定与命名 | 2（小游戏骨架、Mono 迁移表） |
| `aesir-tiers` | 六档分级与升档触发条件 | 6（MVC／MVP 各三档模板） |
| `aesir-core-api` | ObservableValue、可观察集合、MiniEvent、AesirTimer、PlayerLoop | 1 |
| `aesir-decisions` | MVC 还是 MVP、为什么没有 MVVM、四种通知机制怎么选 | 2 |
| `aesir-pitfalls` | 十条高频陷阱（现象 + 原因 + 修法） | 0 |
| `aesir-project-structure` | 工程目录模板、按用途分层、版本控制边界、docs 文档站 | 2（目录模板、建站与部署） |
| `aesir-boot-scene` | 启动场景 Bootstrap.unity + Bootstrap.cs + Build Settings index 0 | 1（两段完整脚本） |
| `aesir-ui` | UIModule／UIRoot：Panel 与 Window、蒙版、世界空间血条 | 4（12 条坑位、世界空间、UI 场景 DDOL、URP 相机堆叠） |
| `aesir-events` | EventModule：订阅与句柄、优先级、过滤器、取消传播、诊断 | 1（分发语义） |
| `aesir-audio` | AudioModule：PlaySfx、PlayBgm 淡变、三通道音量与持久化 | 0 |
| `aesir-scene` | SceneModule：Single／Additive、进度归一化、UniTask awaitable | 0 |
| `aesir-save` | SaveModule：Save/Load 存档槽、事务写入与备份回退、三轨序列化、四扩展点 | 0 |

单点需求直接加载对应技能；不确定用哪个、或要同时用到架构与多个模块时先加载 `aesir-router`。

## 安装

**给开发者看的完整下载与安装说明在包内 `TuanjieCowork~/README.md`**（随框架仓库分发时位于 [AesirArchitecture](../../../Assets/Runestone/AesirArchitecture/TuanjieCowork~/README.md) 与 [AesirModules](../../../Assets/Runestone/AesirModules/TuanjieCowork~/README.md)，路径以框架仓库根为基准）：团结 Cowork 用户走稀疏克隆 + `codely extensions install`，非 Cowork 用户下载后把 `skills/` 拆进自己的 AI 工具。

本机登记为工作区扩展（Cowork 打开该工程即自动加载，hook 随之生效）：

```bash
codely extensions install .codely-cli/extensions/AesirFrameworkGame --scope workspace --consent
```

只把技能装进技能目录（不登记扩展、不启用 hook）：

```bash
python3 scripts/install.py                 # 装到项目级 .codely-cli/skills/
python3 scripts/install.py --scope global  # 装到 ~/.codely-cli/skills/
python3 scripts/install.py --only aesir-router --check   # 干跑，只看会装/覆盖什么
```

框架包本身缺席时，技能不会硬写模块代码：先读工程记忆，再询问是否经 Git URL 安装（CNB 优先、GitHub 兜底）。只缺 RAA 但已装 RAM 时走菜单 `Tools/Aesir/Modules/Install Dependencies`。

## 用法

直接用自然语言提需求，技能按触发词自动加载：

| 你说 | 加载 |
|---|---|
| "用 Aesir 从零做个小游戏" | router → project-structure → boot-scene → framework |
| "加个暂停弹窗" / "做敌方血条" | aesir-ui |
| "状态变了怎么通知 UI" / "用事件还是 ObservableValue" | aesir-decisions → aesir-core-api |
| "技能冷却不用协程怎么写" | aesir-core-api（AesirTimer） |
| "切场景换背景音乐" | aesir-scene + aesir-audio |
| "让范围内敌人只收到这次爆炸" | aesir-events（过滤器） |
| "存档崩溃不丢" / "云存档扩展" | aesir-save |
| "Aesir 报错取不到 Model" | aesir-pitfalls |

会话启动时会看到一行 `[Aesir 依赖门禁]` 上下文，标明两包安装状态与可用技能范围。它是**启动时快照**——会话中途装过包后按 `aesir-router` 的探测方法复验。

不想启用该 hook 时，在工程 `.codely-cli/settings.json` 的 `hooks.disabled` 里按名禁用 `aesir-package-gate`，技能功能不受影响。

## 维护

**技能正文的唯一主位是本扩展的 `skills/`**，包内不再有副本（两个包的 `TuanjieCowork~/` 只保留导流 README）。改完必须跑校验：

```bash
python3 scripts/validate.py          # 期望「0 个错误，0 个警告」
python3 scripts/validate.py --quiet  # 只在出错时输出
```

校验覆盖：清单字段与归属信息、门面文档、技能 frontmatter 与目录名一致、触发描述与正文篇幅上限、相对链接可达、hook 配置结构、目录洁净度（无 `.meta`／`.DS_Store`）、包内不得再出现技能副本。

改内容时遵守：

1. **一个技能只回答一类问题**——入口不写模板、模板不进核心 API、决策与陷阱各自独立。
2. **结论留正文，展开进 references**：坑位与禁止项的结论（哪怕一句话）必须留在 SKILL.md，下沉的只是复现步骤、原理展开与完整代码。
3. 篇幅超限时先想"这段能不能下沉"，不要靠删结论达标；阈值常量在 `scripts/validate.py` 头部。
4. 动技能名或目录结构时同步 `aesir-router` 的分派表、`CODELY.md` 与 `gemini-extension.json`。
5. `~` 结尾目录与 `.codely-cli/` 都不被 AssetDatabase 导入、不进 unitypackage，技能不会混进 Unity 工程或包产物。

许可证 MIT，与框架仓库一致。