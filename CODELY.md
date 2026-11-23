# CODELY.md — AesirFramework

本文件是本仓库的项目指令上下文：说明项目用途、架构约定、目录归属、构建/测试命令与开发纪律。修改代码前先读对应章节。

---

## 项目概览

**AesirFramework** 是 **Runestone Yuumix** 开发的 Unity / 团结引擎框架集合，以单一 monorepo 发布两个可独立安装的 UPM 包，面向**独立游戏**的初学者与中级程序员，主打**精简 + 渐进式**。

| 项 | 值 |
|---|---|
| Unity 版本 | 2022.3.62f3c1（Unity 中国版） |
| 渲染管线 | URP 14.0.12（含 `com.unity.feature.2d`） |
| C# 版本 | C# 9 |
| 许可证 | MIT |
| 作者 | [yuumixcode](https://github.com/yuumixcode) |
| 命名空间 | `Runestone.AesirArchitecture` / `Runestone.AesirModules`（品牌名"符文石"） |
| 代码注释 | 中文 XML 文档注释，标识符英文 |
| 规模 | 两包内共 400+ 个 `.cs`（含 `Samples~/` 镜像）；另有宿主工程完整游戏案例 `Assets/PlaneWar/`（6 个脚本，不属两包） |

### 两个包

| 包 | 包 ID | 版本 | 说明 |
|---|---|---|---|
| **Aesir Architecture（RAA）** | `cn.runestone.aesir.architecture` | 0.35.0 | 渐进式 MVC 架构：能力接口组合、Command/Query（CQRS 风格）、MiniEvent、ObservableValue / 可观察集合、AesirPlayerLoop、AesirTimer、纯 C# Context + MonoBehaviour 适配层 |
| **Aesir Modules（RAM）** | `cn.runestone.aesir.modules` | 0.35.0 | 功能模块：UI 框架（Panel + Canvas 根 Window）、事件模块、音频模块、场景模块、脚本文档生成（需 Odin） |

依赖关系：**RAA 不依赖任何 Aesir 包**；**RAM 依赖 RAA**（`package.json` 中**不声明**该依赖，见下方"依赖补全"）。

Aesir Inspector 已迁出为独立公开仓库，本仓不再包含。根目录残留的 `Runestone.AesirInspector.*.csproj`、`Runestone.ScriptDocGenerator.*.csproj`、`Runestone.AesirArchitecture.R3.*.csproj`、`Runestone.AesirModules.*Binder*.csproj` 等均为 **gitignored 的历史 IDE 生成物**（`.csproj` / `.sln` 全在 `.gitignore` 内），不代表现存程序集，勿据此判断项目结构。

### 设计基调（最高优先级裁决）

1. **RAA 极简是第一排序标准**：低频公开 API 一律删或收窄；新增能力前先问"能否由既有类型组合出来"。
2. **RAM 功能性优先于极简**：功能模块的面（UI / 事件 / 音频 / 场景）是价值，不是负担。
3. **核心架构默认最低档 Quick**：不建 Model/Service 接口、不做只读 property 包装、直接持具体类、`Register*` 注册实现类型；只有确需多实现/替换时才引入接口。
4. **Model = 运行时数据的底层数据源**，不是 UI 附属。
5. **低概率问题用文档约定杜绝，不加防御性代码兜底**；异常不吞噬（fail-fast）；不做初始化回滚、不做自愈轮询。
6. **暂不提供 MVVM（2026-11-16 裁决）**：快速开发首推 MVC；MVP 是 Unity 官方最佳实践所教授的严格分层范式（可选）。MVVM 的核心增量是数据绑定引擎，与"Unity 原生优先、不构建平行自建体系"基调相悖，状态通知已由 `ObservableValue<T>` 覆盖；业务侧可自行按 MVVM 风格组织代码，框架包不提供一等公民支持（无 IViewModel / 绑定引擎 / 官方模板）。重评触发：UI Toolkit 运行时绑定成为主力 UI 方案 / 大型数据驱动 UI 消除订阅胶水成为普遍诉求 / 社区明确广泛需求。完整论述见 `Aesir-Notebook/docs/decisions/architecture-core.md` 第十一章与 `.codely-cli/extensions/AesirFrameworkGame/skills/aesir-decisions/references/mode-decisions.md`。

### 双主位：GitHub 主仓 + CNB 国内下载仓（2026-11-23 反转）

**主仓在 GitHub，CNB 是国内下载仓**——`origin` 指向 `git@github.com:yuumixcode/AesirFramework.git`（SSH，推 workflow 文件不受 OAuth scope 限制），CNB 保留为 `cnb` remote（`cnb.cool/runestone-game/AesirFramework`，仅 fetch / 应急用）。反转原因：CNB 的 AI Issue / PR 能力效果不佳，开发主流程回归 GitHub。

| 侧 | 职责 |
|---|---|
| GitHub（主仓） | `main` 推送触发 `.github/workflows/sync-to-cnb.yml`：包分支 split + 裁剪 → `update-info.json` 回写（`[skip ci]`）→ 版本 tag（不存在才创建）→ 新 tag 时派发 `auto-release.yml` 发 GitHub Release → **main / 两包分支 / tags 全部 force 写入 CNB** |
| CNB（下载仓） | 不做任何开发提交；收到 `v*` tag push 触发 `.cnb.yml` 的 tag_push 流水线发 CNB Release（国内下载加速）；另有 `web_trigger_push_github` 手动按钮把 AI 修复分支反推 GitHub |

硬纪律（写进两边流水线注释）：

- **CNB 侧零提交**：main 与包分支每次被 GitHub force 覆盖，CNB 上的直接提交会被冲掉。
- **PR 在 GitHub 正常 Merge**（旧"不点 Merge"纪律作废）；Issue / PR 社区入口统一 GitHub。
- **CNB AI Issue 流程**：CNB 上提 Issue 让 AI 在**临时分支**修复 → 分支详情页点「反推此分支到 GitHub」按钮（`.cnb/web_trigger.yml` + `.cnb.yml` 的 `web_trigger_push_github`，禁止反推 main / 常驻包分支）→ GitHub 开 PR 合并 → 同步流水线自动把结果写回 CNB，闭环完成。
- **版本 tag 一旦发布即不可变**（PR #7 裁决 + 2026-11-20 铁律）：远端已存在即复用，不删不重推；再小的更新也 bump patch 版本走完整发版流程。
- GitHub Actions 用 `GITHUB_TOKEN` 推的 tag / 分支**不会触发** push 触发的 workflow（平台限制），GitHub Release 靠 `gh workflow run` 显式派发（`workflow_dispatch` 是豁免事件）。
- GitHub Actions 推 CNB 需要仓库 Secrets **`CNB_USERNAME` / `CNB_TOKEN`**（CNB 个人访问令牌，repo 读写权限；CNB 流水线内置的 `CNB_TOKEN` 出不了 CNB）。
- 反推按钮用的 `GIT_ACCESS_TOKEN` 是 OAuth 类令牌，无 workflow scope 时 GitHub 拒推改动 `.github/workflows/*` 的分支——这类修复走本地 SSH 直推。

---

## 目录结构

```
AesirFramework/
├── README.md / README_EN.md / CHANGELOG.md / CONTRIBUTING.md / CODE_OF_CONDUCT*.md
├── CODELY.md                      # 本文件
├── .cnb.yml                       # CNB 下载仓流水线（v* tag → CNB Release；修复分支反推按钮）
├── .cnb/web_trigger.yml           # CNB 分支详情页自定义按钮（反推此分支到 GitHub）
├── CodeStyle/AesirCodeStyle.cs    # 统一代码风格指南（不参与编译）
├── docs/                          # 公开产品站内容（Zensical 同仓发布；内部 architecture/modules 等路径不可动）
├── zensical.toml                  # 站点配置（site_url / nav / theme；edit_uri 必须置空）
├── tools/docs-site/               # 站点本地工具（预览/截图）、品牌源图、工程约定 README
├── openspec/                      # openspec 式变更提案（config.yaml + changes/，TJSpec 仪表盘数据根）
├── .github/
│   ├── workflows/sync-to-cnb.yml      # 主仓流水线：split / update-info / 版本 tag / force 同步 CNB
│   ├── workflows/auto-release.yml     # 建三份 unitypackage → 滚动 GitHub Release（由 sync 派发）
│   ├── workflows/docs.yml             # 文档站流水线：docs/** 变更 → zensical build --strict → GitHub Pages
│   ├── scripts/build_unitypackage.py  # 打包脚本（纯 Python，禁换第三方 CLI）
│   └── scripts/extract_release_notes.py
├── .codely-cli/                   # AI 工具链（部分入库，见「AI 技能与 Codely 环境」）
│   ├── RESTORE.md                 # 换机 / 新克隆的环境还原手册
│   ├── settings.json / memory.md / memory/ / skills/
│   └── extensions/AesirFrameworkGame/   # 自研扩展，全量入库：12 技能 + 2 实战模板
├── Assets/
│   ├── Runestone/
│   │   ├── AesirArchitecture/     # RAA 包根
│   │   └── AesirModules/          # RAM 包根
│   ├── PlaneWar/                  # 完整游戏案例（纵版射击 Mono 版，不随包分发）
│   ├── Scripts/                   # 宿主工程自用脚本（不属两个包）
│   │   ├── HUDContext.cs / HUDPanel.cs      # Binder 生成的示范面板与 Context
│   │   └── Editor/AesirPackageExporter/     # Tools/Aesir/Export Packages 导出工具
│   ├── Scenes/SampleScene.unity   # Build Settings 唯一登记场景（index 0）
│   ├── Settings/                  # URP 资源（UniversalRP / Renderer2D / Lit2DSceneTemplate）
│   ├── Resources/                 # 模块配置资产（SceneModuleConfig / UIModuleConfig）
│   ├── Editor Default Resources/  # SDG 的资产标记与面板资源
│   ├── TextMesh Pro/              # TMP Essential Resources
│   ├── Vertical 2D Shooting BE4/  # Goldmetal 免费素材原件（gitignored，PlaneWar 内有自包含拷贝）
│   └── Plugins/Sirenix/           # Odin Inspector（gitignored，独立授权）
└── Packages/manifest.json
```

**gitignored 的本地产物（不要提交、也不要当作事实来源）**：`Library/`、`Temp/`、`obj/`、`Logs/`、`UserSettings/`、`Build/`、`Exports/`（历史 unitypackage）、`ScriptDocGenerator/`（默认文档输出根）、`site/`（文档站构建产物）、`.cache/`（zensical 增量缓存）、`TestResults*.xml`、`*.log`、`screenshots/`、`*.csproj`、`*.sln`、`Assets/Plugins/Sirenix/`、`Assets/Vertical 2D Shooting BE4/`、`.DS_Store`。

> 入库口径（2026-11-16 裁决）：`Assets/Resources/`（模块配置单例）与 `Assets/Editor Default Resources/Script Doc Generator/`（SDG 资产标记）**入库**——本仓库示例运行会读取它们；`ScriptableSingleton/`（`LoadTypeSettings` / `BinderEditorSettings` / `SceneEditorSettings`）属本机编辑器产物，**继续 gitignored**。注意：这两份 Resources 资产只是**本仓入库的示例基线**；包本身已取消"导入即自动创建"，用户工程需经菜单 `Tools/Aesir/Modules/Create Module Config Assets` 显式创建（或自行创建 / 注册加载器），缺资产时运行时用内存默认配置兜底。

### 两个包的标准结构

```
<包根>/
├── package.json / README.md / README_EN.md / CHANGELOG.md / LICENSE.md / Third Party Notices.md
├── AesirPathLookup.asset        # 安装位置锚点（GUID 固定，勿删/勿改）
├── Runtime/                     # 运行时锚点 asmdef 在层根，模块以子目录汇入
├── Editor/                      # 编辑器锚点 asmdef 在层根
├── Tests/                       # Editor（EditMode）/ Runtime（PlayMode）
├── Samples/                     # 编写主位（Assets 内可见、可运行、构建剔除）
├── Samples~/                    # 发布镜像（Package Manager 按需导入）
├── Documentation~/              # 使用文档（AssetDatabase 不可见、无 .meta、不进 unitypackage）
└── TuanjieCowork~/              # AI 技能获取入口（仅 README，技能正文不在此）
```

**`Documentation/`（无波浪线）已取消**：英文说明升到包根 `README_EN.md`，使用文档只在 `Documentation~/`。**`TuanjieCowork~/Skills/` 也已取消**：两个包各留一份 `README.md` 讲"扩展在哪、怎么装"，技能正文的唯一主位是仓库根的 `.codely-cli/extensions/AesirFrameworkGame/skills/`。

---

## Aesir Architecture（RAA）

### 能力接口组合

每个角色通过组合细粒度能力接口定义，而非继承大而全的接口：

- `ICanGetModel` / `ICanGetService` — 读已注册模块
- `ICanExecuteCommand` / `ICanExecuteQuery` — 写/读分发
- `ICanSetContext` / `IContextHolder` — 上下文绑定

| 角色 | 接口 | 能力 | 说明 |
|---|---|---|---|
| Model | `IModel` → `AbstractModel` | GetModel | 数据层；只经写方法改状态 |
| Service | `IService` → `AbstractService` | GetModel, GetService | 跨模块协调；可直写 Model，不能执行 Command/Query |
| View | `IView` | GetModel, GetService | 只读访问；自订阅刷新 |
| Controller | `IController` | + ExecuteCommand, ExecuteQuery | **MVC 入口（推荐）** |
| Presenter | `IPresenter` | Controller 全部 + `IDisposable` | MVP 中介（可选），View 被动 |
| Command | `ICommand` → `AbstractCommand` | `Execute()`，无返回值 | 写 |
| Query | `IQuery<TResult>` → `AbstractQuery` | `Execute() → TResult` | 读，无副作用 |

泛型角色 DIM 家族（`IView<T>` / `IController<T>` / `IPresenter<T>`）把 `IContextHolder.Context` 绑定到 `AbstractContext<T>.Instance`；适配对 `AesirView`/`MonoView`、`AesirViewController`/`MonoViewController` 经此绑定。VC 适配对声明为 `IView, IController<T>`（View 角色保持非泛型以避免双 DIM 冲突）。

### Context

`AbstractContext<T>`（CRTP 泛型单例，纯 C#，不依赖 MonoBehaviour）：

```csharp
public class HUDContext : AbstractContext<HUDContext>
{
    protected override void Configure()
    {
        RegisterModel(new MyModel());      // MODEL
        RegisterService(new MyService());  // SERVICE
    }
    // GetModel<T>() / GetService<T>()
}
```

- `Instance` 懒加载单例：初始化成功后才赋值；失败不缓存，根因异常每次照抛。
- `Initialize()` 调 `Configure()`，随后按注册顺序初始化 Model → Service；**不做回滚**。`Configure()` 期间注册的模块由同轮待初始化循环补齐（不做二次初始化）。
- `Dispose()` 逆序销毁 Service → Model，收尾在 `finally` 并清 `_instance`；替换实例时旧实例被 Dispose，其订阅不会迁移（输出 Warning）。
- `GetModel`/`GetService` 未注册时**抛带修复提示的 `InvalidOperationException`**，不返回 null。
- 容器字段类型为 `IGenericLocator<T>`（保序注册/查询、`GetAll()` 返回快照）；`Register` 与 `Get` 必须使用**相同类型参数**（按键精确匹配）。
- **`Configure()` 中禁止访问 `Instance`**（会递归创建第二个实例）。

### 关键运行时类型（`Runtime/Common`、`Runtime/Modules`）

| 类型 | 职责 |
|---|---|
| `AesirArchitecture` | MonoBehaviour 单例（`[DefaultExecutionOrder(-999)]`） |
| `AesirMonoBehaviour` / `AesirScriptableObject` | 架构感知基类 |
| `AesirPlayerLoop` | PlayerLoop 注入；`EnsureInjected()` 公开；`Register` 时自动检测补插（第三方 SDK 改 PlayerLoop 后需手动调用） |
| `AbstractTimer` / `AesirTimer` | 计时器功能模块（`Runtime/Modules/Timer`）：`AbstractTimer`（public 抽象基类，模板方法固定调度引擎 + 共有状态/查询/生命周期）→ `AesirTimer`（抽象门面，静态工厂 + 引擎委托）→ 六个具体类。工厂：`Countdown(seconds, cb, unscaled)`→`CountdownTimer`（一次性）、`NextFrame(cb)`→`NextFrameTimer`、`Every(interval, cb, repeatCount=-1, unscaled)`→`RepeatingTimer`（限次/无限）、`Interval(total, interval, cb, unscaled)`→`IntervalTimer`（总时长内每隔固定间隔）、`Frequency(ticksPerSecond, cb, unscaled)`→`FrequencyTimer`（每秒 N 次）、`Stopwatch(unscaled)`→`StopwatchTimer`（正计时读 `Elapsed`）。共有 API（在 `AbstractTimer`）：读 `Duration`/`Remaining`/`Progress`/`RemainingExecutions`/`IsFinished`/`IsCancelled`/`IsPaused`，控制 `Pause()`/`Resume()`/`Restart()`/`Cancel()`（`Dispose()` 等价取消）。复用 `AesirPlayerLoop`（BeforeUpdate + `order: int.MinValue`），结算期与执行期分离，回调内新建/取消延后一帧；每次结算最多回调一次（卡顿不补发）。分层思路参考 [adammyhre/Unity-Improved-Timers](https://github.com/adammyhre/Unity-Improved-Timers)，实现为自有。调试面板 `Tools/Aesir/Architecture/Timer Debugger`（Odin + IMGUI 双版本，数据源/样式分层，拉取节奏可调，编辑模式恒为零）与字段检视（原生需 `[SerializeReference]`，Odin 需 Odin 序列化宿主或 `[ShowInInspector]`；普通字段看示例的 `AesirTimerSampleInspector` 写法） |
| `ObservableValue<T>` | 响应式属性；Model 持可写实例，View 持 `IReadOnlyObservableValue<T>`；`SetValueSilently`、`AddListenerAndInvoke` |
| `ObservableList<T>` / `ObservableDictionary<TKey,TValue>` / `ObservableHashSet<T>` | 单轨变更通知（`AddListener`/`RemoveListener`），载荷为 `CollectionChangedEventArgs<T>`，返回 `AutoRemoveListenerHandle`；语义：无变更不通知、批量逐项、字典值更新为 Replace、Move 单事件、Sort/Reverse/Clear 统一 Reset |
| `MiniEvent` / `MiniEvent<T>` | 零分配多播事件（Invoke 路径零分配，AddListener 有分配）；异常语义 = 原生事件（fail-fast） |
| `MonoLifecycleProxy` | 把 Unity 原生回调统一为可订阅 MiniEvent；调用期增删监听为**快照语义** |
| `GenericLocator<T>` / `IGenericLocator<T>` | 类型键控、保序的服务定位器 |
| `ResetStaticsAssistant` | 泛型类静态字段重置（泛型类 RIOLM 会被 Unity 跳过） |

`RemoveListenerOnDestroyTrigger` / `OnDisable` / `OnSceneUnloaded` 把 `AutoRemoveListenerHandle` 绑到 Unity 生命周期。

### 六档渐进示例

| 档位 | 示例 | Model 暴露面 | 读写路径 | View 边界 |
|---|---|---|---|---|
| MVC-1 快捷 | `Counter-Mvc-Quick` | 具体类 + 可写 ObservableValue | View 兼 Controller 直写直读 | `MonoViewController<T>` |
| MVC-2 标准 | `Counter-Mvc-Standard` | 具体类 + 只读暴露 + 写方法 | Controller 直调写方法 | View 与 Controller 分离 |
| MVC-3 严格 | `Counter-Mvc-Strict` | 接口注册 + 只读 + 写方法 | Command 写 + Query 读 | 窄接口持 Controller |
| MVP-1 快捷 | `Counter-Mvp-Quick` | 具体类 + 可写 ObservableValue | Presenter 直写并推送 | 纯 MonoBehaviour，零接口 |
| MVP-2 标准 | `Counter-Mvp-Standard` | 具体类 + 只读 + 写方法 | Presenter 直调写方法 | View 契约 `IXxxView` |
| MVP-3 严格 | `Counter-Mvp-Strict` | 接口注册 + 只读 + 写方法 | Command 写 + Query 读 | 窄接口持 Presenter |

写作纪律：快捷/标准档表现层可直写 Model；标准档起写入应经 Command；严格档只读 + 写方法；Service 可直写。

其余示例：`ObservableValue`（Odin Drawer）、`ObservableCollections`、`MiniEvent`、`AesirTimer`（延时 / 冷却 / 重复 / 暂停 + 只读查询面板 + 自定义 Inspector 状态表与动作按钮，配 `Timer Debugger` 窗口）、`RuntimeInitializeLoadType`（五个阶段顺序，Editor-only asmdef；配置窗口不进菜单，从 `Tools → Aesir → Getting Started` 中该示例卡片右侧的「初始化时机设置」按钮打开）。

完整游戏案例不在包内：`Assets/PlaneWar/`（纵版射击 Mono 版，宿主工程实际游戏，见 `Assets/PlaneWar/README.md`）。

### RAA 编辑器工具（`Editor/`）

- **Getting Started 窗口** — `Tools/Aesir/Getting Started`，priority **-980**（居 Tools/Aesir 顶部）。以各包 `package.json` 的 `samples` 清单为唯一真源；覆盖 Assets 安装 / 嵌入式 / UPM 三种发现；未导入示例可窗口内一键导入（`PackageManager.UI.Sample` API）。分组 / 顺序 / 档位徽章是**代码侧展示启发式**，登记点是 `AesirGetStartedService` 的 `GetSampleGroup` / `GetGroupOrder` / `GetSampleBadge`，未命中落入默认分组——改分组改这三个方法，不改 `package.json`。Odin 版与 IMGUI 兜底版经 `OdinWindowOpener` 静态委托路由。「实战示例」分组随 PlaneWar 迁出已删除。
- **包内更新器** — `Tools/Aesir/Check for Updates`，priority **1100**（置底）。面向"代码导入 `Assets/Runestone`"的用户：锚点定位安装根 → 扫描 `*/package.json` → 三层版本检测（GitHub 直连 → 镜像站 `ghproxy.net`/`gh-proxy.com` → jsDelivr）→ 下载 `<包目录名>-v<版本>.unitypackage` → 差集删除残留条目 → 静默导入 → 登记 `.aesir/installed-manifest.json`。整段更新用 `EditorApplication.LockReloadAssemblies()` 防域重载打断；超时双判据（总时长 + 无进展）。纯 UPM 安装时菜单隐藏（无管辖对象）。
- **安装位置锚点** — 每个包根 `AesirPathLookup.asset`（`ScriptableObject` 空壳，`.meta` GUID 固定写在 `AesirAssetPaths` 常量里）。解析顺序：默认根 `Directory.Exists` → 锚点 GUID 查询 → `FindAssets("t:AesirPathLookup")` 兜底；域重载后重解析。**勿删、勿改 GUID。**
- **构建剔除** — `AesirSamplesBuildFilter` 经 `BuildPlayerWindow.RegisterBuildPlayerHandler` 把 Aesir 示例场景从本次构建选项剔除并打印 `[Aesir Build]` 日志（不改 Build Settings 持久数据）。仅拦截窗口构建入口，CI 直接调 `BuildPipeline.BuildPlayer` 不经过钩子。
- **导出工具** — 宿主工程 `Assets/Scripts/Editor/AesirPackageExporter/`：`Tools/Aesir/Export Packages/Export All Packages | Export Aesir Architecture | Export Aesir Modules`（priority 1000/1001/1002）。
- **计时器调试面板** — `Tools/Aesir/Architecture/Timer Debugger`，priority **996**。Odin 版（`AesirTimerDebugWindowOdin`，经 `OdinWindowOpener` 路由）+ IMGUI 兜底版。**数据源与样式分层**：`AesirTimerSnapshot` / `AesirTimerStatistics` 只搬数据（零文案零颜色）→ `AesirTimerDebugSource` 管抓取节奏与汇总 → `AesirTimerText` 文案/颜色样式 + `AesirTimerSummaryView` 单行摘要外观 → 两个窗口只做排布。换面板样式只换视图。**拉取与重绘分离**：工具栏配拉取模式（按时间 0.05–10s / 按帧 1–600，`EditorPrefs` 持久化，默认 0.5s），到点才枚举注册表并重建快照，其余重绘只读缓存行。**编辑模式无数据源**：非 Play 模式不枚举注册表、不读累计计数，统计与表格恒为零，Play 状态切换那一刻立即重抓。列：状态 / 形态 / 时间源 / 剩余 / 进度 / 已执行 / 还剩次数 / 存活 / 回调；排序为"运行中在前 + 剩余时间升序"，超 200 行截断。`Editor` 程序集经 `InternalsVisibleTo` 读运行时内部状态。
- 菜单分组惯例：父组 priority 取子项最小值，差 > 10 自动插分割线。新增 RAA 工具进 `Tools/Aesir/Architecture/`，RAM 工具进 `Tools/Aesir/Modules/`。

### 设计边界（RAA）

不做：事件总线 / EventChannel、Context 多实例、Command/Query 池化与 async 与 Undo/Redo、计时器家族之外的调度原语（`Start` 显式启动、对象池；秒表已作为 `StopwatchTimer` 纳入——它只累计时间、无终点，仍需持有引用手动取消，仅读时间戳的场合直接用 `Time.time`）、View 生命周期脚手架（交给 RAM 的 UIModule）、线程安全（仅主线程）、集合高级能力（队列/过滤视图/环形缓冲等用上游 Cysharp.ObservableCollections）。

时间调度的四条路径边界：MonoBehaviour 内本地 float 字段 / `MonoLifecycleProxy` 帧代理 / `AesirTimer` 工厂创建的计时器 / 协程。`AbstractTimer` 内部的两层暂停：全局靠 `Time.timeScale`（`unscaled` 不受影响），单实例靠 `Pause()`/`Resume()`。

---

## Aesir Modules（RAM）

模块间零依赖，删除模块 = 删 `Runtime/<模块>/` + `Editor/<模块>/`。

### UI 模块（`Runtime/UI`）

- **`UIModule`** — `AesirMonoBehaviour` 单例（`dontDestroyOnLoad` 序列化字段控制 DDOL），管理面板与窗口生命周期。
  - 静态门面：`Show<T>` / `Hide<T>` / `Get<T>` / `Prewarm<T>` / `RegisterPrefab<T>` / `ContainPrefab<T>`；窗口：`Open<T>` / `Close<T>` / `GetWindow<T>` / `PrewarmWindow<T>` / `RegisterWindowPrefab<T>` / `ContainWindowPrefab<T>`。实例侧为长名（`ShowPanel` / `OpenWindow` …）。
- **`IUIPanel`** — `Initialize → Show(payload) → Hide → DestroyPanel`；`Layer`（`UILayer`）、`DestroyOnHide`、`IsOpen`。
  - `AesirBasePanel` / `AesirBasePanelView<T>` / `AesirBasePanelViewController<T>`；面板实例化到 `UIRoot` 四层 Canvas 下。
- **Canvas 根窗口形态** — `IUIWindow`：`OnInit → OnShow → OnHide → OnClose`；`AesirBaseWindow` / `AesirBaseWindowView<T>` / `AesirBaseWindowViewController<T>`；预制体根节点自带 Canvas，直接挂 `UIRoot` 下，不经四层层级；`sortingOrder` 为单一真源（默认 500）；预制体约定 `Mask` + `Content` 子物体；`CloseSelf()` 为窗口既有的自关闭入口。
  - 蒙版：初值在 `UIModuleConfigSO` 单例资产，运行时经 `MaskMode` 切换；蒙版点击走 `AesirBaseWindow.OnMaskClicked()` 虚方法，`closeOnMaskClick`（默认 false）。层级枚举 `UIMaskMode`。
  - 命名规范：窗口类/脚本/预制体以 `Window` 结尾（不用 `XxxCanvas`，会被误读为 Canvas 派生类）。
- **`UIRoot`** — 构建四层 Canvas 层级；层 Canvas / UICamera / EventSystem 为序列化引用持久化（`UICanvasConfigSO`）。
  - **世界空间 UI Canvas 预设** — `WorldSpaceUICanvas`（Add Component → Aesir Modules → UI）：`RenderMode.WorldSpace` + RectTransform/CanvasScaler/GraphicRaycaster 自动接线，默认 sortingOrder 450（面板层 100–400 与窗口 ≥500 之间）；`FollowTarget`（空=跟随父级）+ `PositionOffset` + `FaceCamera`/`Billboard` 控制跟随与朝向，子树归入 UI 层(5)；`UIRoot.CreateWorldSpaceCanvas(target, name, config)` 运行时创建。世界空间配置走独立链（组件 → `UIRoot.WorldSpaceCanvasConfig` → `UICanvasConfigSO.CreateWorldSpaceDefault()`），**不进 UIModule 注册表 / 蒙版调度**，生命周期随场景宿主。Odin 侧配置经 `WorldSpaceUICanvasAttributeProcessor`。
- **`IUIAssetLoader` / `ResourcesUILoader`** — 可插拔资源加载，**同步契约**（Addressables 需预加载后同步返回）；`RegisterAssetLoader` 替换。
- **`BinderAssistant` / `BinderTag`**（Odin 程序集）— UI 元素绑定系统 + 代码生成；窗口感知，根节点带 Canvas 时默认 `Window` 后缀与 `AesirBaseWindow` 基类。生成脚本为 partial + 「同一脚本增量」模式，只替换「绑定字段」region。
- 已知契约：`OnClose` 只在受控销毁路径（Hide + DestroyOnHide）调用，**外部销毁只有 `OnDestroy`** → 解绑逻辑放 `OnDestroy`。同基类类型重复 Show 报错；Hide/Get 未命中给 Warning；缺层 fail-fast。

### 事件模块（`Runtime/Events`）

- `EventModule` 直接继承 `AesirMonoBehaviour`；**只保留脚本订阅一条路径**：`AddListener<T>(subscriber, callback)` 返回 `AutoRemoveListenerHandle` 精确退订，单一 `Bindings` 注册表（`Dictionary<Type, List<BindingInfo>>`，**注册期有序插入**：Priority 升序、同档注册序，分发零排序）；无句柄退订用 `RemoveListener<T>(obj)` / `RemoveListener(obj)`。`[AesirListener]` 特性反射订阅已移除（破坏性变更，理由见 `decisions/remove-attribute-subscription.md`）。
- `BindingInfo` / `ScriptBindingInfo<T>` 均为 **internal**（`Invoke(AesirEventArgs)` 委托直调，无 `object[]`；非泛型订阅以运行期类型为键注册基类委托，零反射）+ `SubscriberPriority`（First / High / Medium / Last；Script 默认 Medium；First/Last 不受取消传播影响）。
- 事件参数继承 `AesirEventArgs`，命名 `XxxEventArgs`；`WithFilter/WithFilters` 链式过滤器（`ISubscriberFilter`：`WithTag` / `WithPriority` / `SameSceneAsEmitter` / `OnlySelf` / `InsideCollider2D`，**fail-closed**，未过滤零开销；发布者 GO/Collider2D 按趟缓存共享）。
- **分发约定（低概率场景不做兜底）**：同一参数实例分发期间不得再次发布（`Dispatching` 标记 fail-fast 拒绝，跨实例事件链安全、重入缓冲按深度复用）；`SetSender` 为发布者侧 API；分发中途增删 Collider2D 不支持。`Clone()`/`ICloneable` 与字符串绑定键 API 已删除（见 `decisions/event-module-simplification.md`）。
- 分发期自动清理已销毁订阅者（编辑器告警、玩家构建静默）；`executionMsLimit` 毫秒阈值性能监控（默认 0 关闭）；`TryGetExisting` 负缓存（实例缺失期至多一次全场景查找）。
- SO 资产化：`AesirEventArgsSO`（资产作为发布者 Raise）+ `UnityEventOnAesirEvent` 桥接组件 + `SubclassSelector`/`ExcludeSubclassSelector`。
- 命名区分：仅作数据载体 → `XxxEventArgs`；自身持监听者并有 Invoke 能力 → `XxxEvent`。

### 音频模块（`Runtime/Audio`）

- `AudioModule` 全静态门面（`AesirModules.GetOrAddChild<T>()` 创建 MonoBehaviour 单例）。
- SFX 用 **N 个独占 `AudioSource` 轮询**（默认 8，等效池化；不用 `PlayOneShot`、不每次 Instantiate），每播音调/音量独立；pitch 钳制 `[0.01, 3]`。
- BGM 专用 loop 源 + 独立淡变系数协程（`unscaledDeltaTime`）；`PlayBgm(clip, fadeSeconds)` / `StopBgm(fadeSeconds)`。
- 三通道音量乘法链（Master / Bgm / Sfx，`AudioChannel` 结构体）+ PlayerPrefs 持久化（键前缀可配于 `AudioConfigSO`）。
- 边界：不做 3D（用原生 `PlayClipAtPoint`）、AudioMixer、每音效 Stop、播完回调。

### 场景模块（`Runtime/Scene`）

- `SceneModule` 全静态门面：`LoadSceneSingle` / `LoadSceneAdditive` / `UnloadScene` / `SetActiveScene` / `ReloadScene` / `UnloadAllAddedScenes`；`LastLoadedScene` / `AddedScenePaths` / `SceneLoadedEvent` / `SceneUnloadedEvent`。
- `SceneAssetWrapper` — 可序列化场景引用（GUID 锚点自愈；`IsDangling` 以"自愈后资产可定位"为准；判等与 `GetHashCode` 路径优先）。
- `SceneModuleConfigSO` 单例配置（启动场景兜底 + 进度归一化上限；无 Odin 时有原生 Drawer / CustomEditor 兜底）。
- 双驱动：宏 `AESIR_MODULES_UNITASK`（本仓库已装 UniTask → 当前生效）；含 UniTask 时内部流程改 UniTask 驱动，`SceneModuleUniTask` 提供可 await API。

### 脚本文档生成（需 Odin）

`Runtime/ScriptDocGenerator/OdinInspector` + `Editor/ScriptDocGenerator/OdinInspector`（经 asmref 汇入 Odin 程序集）。反射生成结构化 API 文档（增量保留 `## Additional Notes`）+ Summary 双向同步工具。静态入口 `ScriptDocGeneratorAPI`（`GenerateDocsForType/ForTypes/ForAssembly/ForFolder`，`settings == null` 用 Default，`outputFolder == null` 用项目根 `ScriptDocGenerator/`；全程无弹窗）。菜单：`Tools → Aesir → Modules → Script Doc Generator`。Summary 工具的权威源是 `[Summary]` 特性。资产标记与面板资源落在 `Assets/Editor Default Resources/Script Doc Generator/`。

### 依赖补全（`Editor/Bootstrap`）

`package.json` **不声明任何 Aesir 依赖**：UPM 不支持包内 Git URL 依赖（保留会让单装直接失败），`com.unity.test-framework` 也非硬依赖。RAA 缺席由 `Tools/Aesir/Modules/Install Dependencies`（priority **998**）补装：确认框 → `Client.Add`（固定常量 URL，指向常驻 `AesirArchitecture-latest` 分支）→ `SessionState` + `[InitializeOnLoadMethod]` 跨域重载收尾。

**铁律**：`Runestone.AesirModules.Editor.Bootstrap` 是**零引用程序集**（`references: []`），不得加 RAM 核心 / RAA / Odin 引用，否则缺依赖时菜单自身不编译。

### 设计边界（RAM）

不做：Addressables 默认集成（UI 默认 `ResourcesUILoader`，接口已预留）、AudioMixer、事件系统元事件 / DefaultChannel（编辑器工具链依赖，暂缓）、UI 虚拟列表与滚动复用。

---

## 程序集定义

### RAA（16 个 asmdef，`Samples~/` 按同名镜像一份）

| 程序集 | 锚点路径 |
|---|---|
| `Runestone.AesirArchitecture` | `Runtime/` |
| `Runestone.AesirArchitecture.Editor` | `Editor/` |
| `Runestone.AesirArchitecture.Editor.OdinInspector` | `Editor/OdinInspector/`（`ODIN_INSPECTOR`） |
| `Runestone.AesirArchitecture.Tests.Editor` | `Tests/Editor/` |
| `Runestone.AesirArchitecture.Tests` | `Tests/Runtime/`（PlayMode） |
| `Runestone.AesirArchitecture.Samples.MvcQuick` / `MvcStandard` / `MvcStrict` | `Samples/Counter-Mvc-*` |
| `Runestone.AesirArchitecture.Samples.MvpQuick` / `MvpStandard` / `MvpStrict` | `Samples/Counter-Mvp-*` |
| `Runestone.AesirArchitecture.Samples.MiniEvent` / `ObservableValue` / `ObservableCollections` | 同名示例 |
| `Runestone.AesirArchitecture.Samples.AesirTimer` | `Samples/AesirTimer/` |
| `Runestone.AesirArchitecture.Samples.RuntimeInitializeLoadType` | `Samples/RuntimeInitializeLoadType/Editor/`（Editor-only） |

示例程序集为**运行时程序集 + 整文件 `#if UNITY_EDITOR` 包裹**：编辑器内正常编译/挂载/Play，玩家构建整体剔除。**不要改回 Editor-only asmdef**——其 MonoBehaviour 会被判定为编辑器脚本而禁止挂场景（Missing Script）。

宿主工程的游戏案例程序集不在此表：`PlaneWar.Mono`（`Assets/PlaneWar/Scripts/Mono/`，命名空间 `PlaneWar`，`autoReferenced: false`），正常编译进玩家构建、不经示例的构建剔除。

### RAM（17 个 asmdef + 7 个 asmref）

| 程序集 | 锚点路径 | 说明 |
|---|---|---|
| `Runestone.AesirModules` | `Runtime/` | 核心运行时（引用 RAA）；Common/Audio/Events/Scene/UI 汇入 |
| `Runestone.AesirModules.OdinInspector` | `Runtime/Integration/OdinInspector/` | Binder 全家桶（`ODIN_INSPECTOR`）；`Runtime/UI/OdinInspector`、`Runtime/ScriptDocGenerator/OdinInspector` 经 asmref 汇入 |
| `Runestone.AesirModules.UniTask` | `Runtime/Integration/UniTask/` | `defineConstraints: AESIR_MODULES_UNITASK` + versionDefines `com.cysharp.unitask` |
| `Runestone.AesirModules.InputSystem` | `Runtime/UI/InputSystem/` | `ENABLE_INPUT_SYSTEM`，可选 |
| `Runestone.AesirModules.Editor` | `Editor/` | 核心编辑器 |
| `Runestone.AesirModules.Editor.OdinInspector` | `Editor/Integration/OdinInspector/` | UI/Scene/ScriptDocGenerator/Audio 的 `OdinInspector/` 经 asmref 汇入 |
| `Runestone.AesirModules.Editor.Addressables` | `Editor/Integration/Addressables/` | `AESIR_MODULES_ADDRESSABLES`；`Editor/Scene/Addressables` 经 asmref 汇入 |
| `Runestone.AesirModules.Editor.Bootstrap` | `Editor/Bootstrap/` | **零引用** |
| `Runestone.AesirModules.Tests.Editor` | `Tests/Editor/` | EditMode（基线，不得加 `ODIN_INSPECTOR` 门控） |
| `Runestone.AesirModules.Tests.Editor.OdinInspector` | `Tests/Editor/OdinInspector/` | 依赖 Odin 的 EditMode 用例 |
| `Runestone.AesirModules.Tests` | `Tests/Runtime/` | PlayMode |
| `Runestone.AesirModules.Tests.UniTask` | `Tests/Runtime/UniTask/` | PlayMode（UniTask） |
| `Runestone.AesirModules.Samples.Events.KeyPress` / `Filters` / `SOAsset` | `Samples/Events/0x_*` | 运行时 + `#if UNITY_EDITOR` |
| `Runestone.AesirModules.Samples.Audio.BasicUsage` | `Samples/Audio/01_BasicUsage/` | 同上 |
| `Runestone.AesirModules.Samples.UI.BasicUsage` | `Samples/UI/01_BasicUsage/` | 同上 |

asmref：`Editor/Audio/OdinInspector`、`Editor/Scene/Addressables`、`Editor/Scene/OdinInspector`、`Editor/ScriptDocGenerator/OdinInspector`、`Editor/UI/OdinInspector`、`Runtime/ScriptDocGenerator/OdinInspector`、`Runtime/UI/OdinInspector`。

**asmdef 纪律**：一律用 **name 引用**（不用 GUID）；`versionDefines` 产生的宏**只对声明它的程序集可见**，以该宏作 `defineConstraints` 的程序集必须各自再声明一份，否则被 Unity 静默排除（零报错、用例凭空消失）——UniTask 就是这种双声明形态。

---

## 测试规范

### 只测六类

公开 API 行为契约、不显然的不变量、真实缺陷回归（一缺陷一例）、Unity/第三方怪癖、纯函数等价类边界（每类 1 例）、外部约定格式往返。**优先用静态守护用例替代行为穷举**（如 `AesirSamplesScriptGuardTests`、`SceneModuleTestSceneHygieneTests`）。不以覆盖率或用例数作验收指标。

**禁止**：测语言/BCL 已保证的事、平凡 getter/构造器赋值、私有字段与引用同一性、生成文本与日志文案逐字断言、正则与解析器穷举矩阵、同一行为多种写法各一例、计时窗口断言、改工程状态的副作用测试、参数组合爆炸。

### 密度与写法

- 单类型 3–10 例；> 15 例须在文件头写明"互不相同的受害者契约"；单文件 ≤ 350 行 / ≤ 20 例；纯重构与内部改名 **0 新增用例**。
- 命名 `被测对象_场景_期望`，可当一句话读；一例一因；AAA 且 Act 只一次；断言值不断言实现字面量；日志断言用 `new Regex(...)`（富文本前缀，纯字符串匹配不上）；`TearDown` 异常路径也必须回收。
- 放置：`Destroy`/`Awake` 真执行、协程/帧/PlayerLoop、场景加载卸载、渲染 → **PlayMode**（EditMode 下 `Destroy` 是 no-op、`AddComponent` 不触发 `Awake`）；资产库/菜单/Inspector/文档生成/纯计算 → EditMode。
- 可移植：不依赖宿主场景 / BuildSettings / 自定义宏 / 当前打开场景；需要场景用包内夹具 `AssetDatabase.CopyAsset` + TearDown 删除（禁 `NewScene(Additive)`）；需要 BuildSettings 用 `IPrebuildSetup` / `IPostBuildCleanup`；临时文件写 `Temp/`。
- **禁止副作用（事故级）**：测试内写脚本宏（触发域重载 → `Unexpected assembly reload happened while running tests`，整轮丢失，只测 `ContainsSymbol/AddSymbol/RemoveSymbol`）、常驻写 BuildSettings、在 `Assets/` 留资产、依赖网络、无 deadline 的等待。
- 本引擎 nunit 被裁剪：**无 `Assert.ThrowsAsync`**，`async Task` 用例可能被静默忽略 → 同步断言 + `GetAwaiter().GetResult()`。
- 测试红了：契约有意变 → 改测试 + CHANGELOG 记破坏性；契约没变只是实现变 → 判为测试过拟合，删/简化测试，**绝不为保绿扭曲实现**。

### 现状（2026-11-23 实测）

- **6 个测试程序集 / 78 个 `.cs`（含 `Tests/Editor/OdinInspector/ScriptDocGenerator/Common/` 下的辅助类型）**；属性层面 `[Test]` 730、`[TestCase]` 67（含 `[TestCaseSource]`）、`[UnityTest]` 36；参数化展开后实跑 EditMode 881 + PlayMode 36。
- 最密、触碰时优先收敛：`BinderCodeGeneratorTests`、`AesirUpdateServiceTests`、`SourceParsingTests`、`AudioModuleTests`、`EventModuleTests`、`XmlSummaryToolTests`。
- 改测试或 asmdef **必须重跑**；纯文档补丁可豁免 PlayMode。

---

## 构建与运行

### 编辑器

1. **先装 TJGenerators 扩展再打开 Unity**（`Packages/manifest.json` 有本地路径依赖 `file:../.codely-cli/extensions/TJGenerators/Packages/cn.tuanjie.ai.generators`，该目录不入库；未装则包缺失、相关脚本不编译）。重装命令见 `.codely-cli/RESTORE.md`。
2. 用 Unity 2022.3.62f3c1 打开项目（首次导入会编译两包 + 示例）。
3. 主推示例：`Assets/Runestone/AesirArchitecture/Samples/Counter-Mvc-Quick/Scene/SampleForCounterMvcQuick.unity`。
4. 完整游戏案例：`Assets/PlaneWar/Scenes/PlaneWar.unity`（方向键/WASD 移动、Space 按住连发、失败后 Space 重开；**未登记在 Build Settings**，出包时自行加入）。
5. Odin Inspector 可选：装于 `Assets/Plugins/Sirenix/`（gitignored）。未安装时 `ODIN_INSPECTOR` 相关内容整体编译剔除，功能可用但 Binder / SDG / Odin 窗口退化。

### 命令行

```bash
# 编译预热（0 错误才算基线）
Unity -batchmode -quit -projectPath . -logFile build.log

# EditMode（按需加 -testFilter）
Unity -batchmode -projectPath . -testPlatform editmode -runTests \
       -testResults TestResults-editmode.xml -logFile editmode-test.log

# PlayMode（必须退出编辑器走 batchmode：编辑器内 Test Framework 1.1.33 会误入 PlayerLauncher）
Unity -batchmode -projectPath . -testPlatform playmode -runTests \
       -testResults TestResults-playmode.xml -logFile playmode-test.log
```

`-runTests` 与 `-quit` 互斥（不要同时给）。全绿 exit 0，失败 exit 2，需解析结果 XML。同项目不可双开。

Unity 侧无自定义构建脚本；示例脚本整文件 `#if UNITY_EDITOR`，玩家构建自动排除。unitypackage 由 `.github/scripts/build_unitypackage.py` 产出（纯 Python，**禁止换第三方 CLI**——实测会产出重复 GUID / 全零 meta / 丢文件夹条目；跳过 `~` 后缀与 `.` 开头目录；包根 `Tests/` 整目录剔除，含目录 `.meta`）。

### 实时核查工具

- **Unity Insight VFS（优先）**：`vfs_index_status` 确认索引就绪后用 `vfs_ls` / `vfs_glob` / `vfs_grep` / `vfs_read` / `vfs_refs` 查类、方法、调用关系、预制体/场景层级与引用；`read_file` 前先 `vfs_resolve_path`。索引未就绪时退回 `glob` / `search_file_content`。
- **Codely Bridge（脚本上下文）**：可直接跑 C# 片段，但有硬限制——裸 `Object` 报 CS0104（写 `UnityEngine.Object.*`）、无 `UNITY_EDITOR` 宏、`TestRunnerApi` 不推进测试、禁 top-level `await`、`Camera` 不是 MonoBehaviour。详见「本仓实测坑位」。

---

## 开发规范

### 命名

- 命名空间 `Runestone.AesirArchitecture`（含 `.Samples.<示例名>` / `.Editor` / `.Editor.OdinInspector`）、`Runestone.AesirModules`（同构）。示例程序集名与命名空间一致。
- 类 PascalCase；接口 `I` 前缀；抽象类 `Abstract` 前缀；常量/静态只读 PascalCase。
- 私有字段 `_camelCase`（非序列化），`camelCase`（`[SerializeField]`）。
- Lifecycle 一个单词（非 `LifeCycle`）。
- 跨包命名三分法：跨包共用实现 → `Aesir` + 语义名（`AesirMonoBehaviour`、`MiniEvent`）；两包各有一份同类角色 → 完整包名 + 语义名（`AesirArchitecture` / `AesirModules`）；单包自有且无对照 → 现状。

### 代码风格

- 统一风格指南在 `CodeStyle/AesirCodeStyle.cs`（不参与编译）。
- **XML 文档注释中文**，标识符英文；数据类标 `[Serializable]`。
- 注释只描述**当前行为与"为什么"**，不写历史演进；非直觉的性能优化用 remarks 写原理 + 量级对比。
- 显式接口实现上下文注入（`IContextHolder.Context`、`ICanSetContext.SetContext`）。
- 单例 `Instance` getter 用 `FindAnyObjectByType`（不用语义已废弃的 `FindFirstObjectByType`）；非泛型单例类内 `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]` 重置静态字段，泛型类经 `ResetStaticsAssistant.Register()`。
- MonoBehaviour 的**运行状态字段一律用显式非序列化字段**（自动属性 backing field 会被场景序列化残留）。
- 6 个 DDOL 单例（`AesirArchitecture`、`AesirModules`、`AudioModule`、`SceneModule`、`UIModule`、`UIRoot`）统一用 `[SerializeField] bool dontDestroyOnLoad = true` 且**声明为类体第一个字段**；关闭 DDOL 只在 Inspector 给信息框，不输出运行时 Warning。
- Odin 依赖代码隔离在 `OdinInspector/` 子目录（RAA 仅 Editor 侧独立 asmdef；RAM 经 asmref 汇入独立 Odin 程序集）；核心程序集经 `#if ODIN_INSPECTOR` 直接使用 Sirenix API。AttributeProcessor 用 `internal sealed` + 与目标类同文件定义（成员匹配靠 `nameof`，勿改被匹配成员的字段/属性形式）。
- 允许自由使用 `#region`。

### 日志

前缀加粗带色；**禁止连续两个带色前缀**（方括号框选前缀不在此列）；生产路径一律走门面（RAA `AesirArchitectureDebug` / RAM `AesirModulesDebug`）。三处有意例外，勿"顺手修正"：`Editor/Bootstrap/AesirDependencyInstaller`（零引用程序集）、`Samples/`、`Tests/`。

### 自定义宏

- `AESIR_ARCHITECTURE` — 由 `EnsureAesirArchitectureDefine` 自动写入（`SetScriptingDefineSymbols` 值未变时不落盘 → 写完需 grep `ProjectSettings/ProjectSettings.asset` 验证）。
- `AESIR_MODULES_UNITASK` — 由 asmdef `versionDefines`（`com.cysharp.unitask`）产生 + `AesirUniTaskDefineKeeper` 编辑器维护。
- `ODIN_INSPECTOR` / `ODIN_INSPECTOR_3*` — Sirenix 管理。

### Samples 双目录 / Documentation~ 与 TuanjieCowork~ 单目录

- `Samples/` 是**编写主位**；`Samples~/` 是 UPM 镜像（`~` 目录对 AssetDatabase 不可见、不进 unitypackage）。
- 同步方向：先改主位，再同步镜像（含 `.meta`，GUID 与 UPM 导入链路一致）。
- **`Documentation~/` 无主位**：使用文档只放 `Documentation~/`（尾随 `~`，AssetDatabase 不可见、不进 unitypackage、无 `.meta`），在该目录直接编辑、无镜像同步；英文说明放包根 `README_EN.md`（`.md` 会被 Unity 当资产导入，保持一份）。当前存量：RAA `observable-collections.md`；RAM `ui-module.md` / `event-module.md` / `audio-module.md` / `scene-module.md` / `script-doc-generator.md`。unitypackage 用户不随包拿到使用文档——权威副本在文档站。
- 本机同步脚本：`.codely-cli/skills/sync-samples/scripts/sync_samples.py --check`。**逐文件 diff 判断方向，不要凭"主位为准"硬覆盖**（历史上主位反而是错的一侧）；脚本会校验 `package.json` 的 `samples` 清单。`rsync --exclude='*.meta'` 会误配 `.md.meta`，应写 `--exclude='*.md.meta'`。
- 文档分层：包根 ①`README.md`（中文）②`README_EN.md`（英文）；`Documentation~/` 放使用文档。迭代过程与内部设计文档写私有笔记仓 `Aesir-Notebook/`，不随包分发；包内不保留设计变更记录（CHANGELOG 除外）。
- **`TuanjieCowork~/` 只放一份 `README.md`**（技能获取入口）。AI 内容不进 Unity 工程——`~` 目录不被 AssetDatabase 导入、不生成 `.meta`、不进 unitypackage（`build_unitypackage.py` 的 `_is_ignored_name` 跳过尾随 `~`）。**禁止创建无波浪线的 `TuanjieCowork/`**：会被当普通资产导入并写进 unitypackage 分发给所有使用者。

### AI 技能与 Codely 环境（`.codely-cli/`）

**技能正文的唯一主位**：`.codely-cli/extensions/AesirFrameworkGame/skills/`（整包扩展，`gemini-extension.json` v1.3.0），12 个技能 + 2 篇实战模板：

| 技能 | 覆盖 |
|---|---|
| `aesir-router` | 入口与分派：意图 → 技能、组合路径、依赖门禁、常见误路由纠正 |
| `aesir-framework` | 架构底座：Context／Model／View／Controller、Quick 三件套、核心裁决、命名、十条约定 |
| `aesir-tiers` | 六档分级与模板（MVC／MVP 各三档）、升档触发条件 |
| `aesir-core-api` | `ObservableValue`、可观察集合、`MiniEvent`、`AesirTimer`、`AesirPlayerLoop`、能力矩阵 |
| `aesir-decisions` | MVC 还是 MVP、为什么没有 MVVM、四种通知机制怎么选 |
| `aesir-pitfalls` | 十条高频陷阱（现象 + 原因 + 修法） |
| `aesir-project-structure` | PC 独立游戏目录模板、按用途 + Global/Shared/Unique 分层、版本控制边界、docs 文档站与 Zensical 部署（不依赖包） |
| `aesir-boot-scene` | 启动场景 `Bootstrap.unity` + `Bootstrap.cs` + Build Settings index 0 |
| `aesir-ui` / `aesir-events` / `aesir-audio` / `aesir-scene` | RAM 四模块用法 |

扩展独有增量：`skills/aesir-framework/references/templates-game-skeleton.md`（完整可玩小游戏骨架）与 `templates-migration-from-mono.md`（原生 MonoBehaviour → Aesir 迁移表）；脚本 `scripts/install.py`（批量装到项目级/全局技能目录）、`scripts/validate.py`（manifest / frontmatter / 相对链接校验）。

**入库策略**（`.gitignore` 白名单）：入库 `settings.json`、`memory.md` + `memory/`、`skills/`、`RESTORE.md`、`extensions/AesirFrameworkGame/`；其余扩展（TJGenerators 115M、codely-unity-lsp-server、TMPChineseFont、vtsls）、`UnityInsight/`、`auto-saves/`、`tool-outputs/`、`tmp/` 一律不入库，按 `RESTORE.md` 一条命令重装。凭证按文件名模式兜底排除（`*token*.json` / `*secret*` / `*oauth*` / `*credential*` / `.env*` / `*.pem` / `*.p12` / `*.key`）。

**维护约定**：技能只在扩展目录内改（包里不再有副本）；改完跑 `python3 .codely-cli/extensions/AesirFrameworkGame/scripts/validate.py`（期望"0 个错误，0 个警告"）。拆分公司原则是**一个技能只回答一类问题**——入口不写模板、模板不进核心 API、决策与陷阱各自独立，AI 按意图只加载需要的那一篇。

### 公开产品站（同仓，2026-11-23 并入）

- 内容在主仓根 `docs/` + `zensical.toml`，由 `.github/workflows/docs.yml` 构建发布到 GitHub Pages（`https://yuumixcode.github.io/AesirFramework/`）；仅 `docs/**` / `zensical.toml` / workflow 自身变更触发。本地门禁 `zensical build --strict`（0 issue）。
- 站点工程约定（设计系统 / 半角标点 / 中文标题英文锚点 / `edit_uri` 置空 / API 页增量重生成流程）权威源在 `tools/docs-site/README.md`；本地工具 `serve-preview.py` / `shoot.mjs` 与品牌源图同目录。
- `docs/architecture/`、`docs/modules/` 物理路径被 SDG 重生成与 `package.json` 的 `documentationUrl` 依赖，**不可移动**。
- 旧仓 `AesirFramework-Site` 保留为全路径跳转壳 + 历史留档（旧深链跳新站对应页）；本地克隆清理后不再存在。
- `site/`（构建产物）与 `.cache/`（zensical 增量缓存）gitignored。

### 外部关联目录（被 `.gitignore` 排除）

- `Aesir-Notebook/` — 私有开发笔记（Zensical 站点，含 `docs/standards/iteration-workflow.md`、`unit-testing-standards.md`、`design-changelog.md` 权威源）。

### 迭代流程（新增功能 / 行为变更 / Bug 修复）

> 本节点是速查版；带"为什么"的完整 SOP + 文档更新矩阵逐项说明在 `Aesir-Notebook/docs/standards/iteration-workflow.md`。**发版当天的版本号同步不在这里**，走 `.codely-cli/skills/aesir-version-sync/SKILL.md` 的 11 步（发版清单只有这一份权威，别在别处另抄）。

一轮迭代七步，前六步在功能分支内完成，第七步发版时统一收口：

1. **立项与边界** — 一句话写清做什么 / **明确不做什么**；先问"能否用现有能力组合出来"（RAA 极简优先）；判定属 RAA 还是 RAM。
2. **建分支** — 从 `main` 拉 `feat/<name>` / `fix/<name>` / `docs/<name>` / `refactor/<name>`。本机 git 命令一律先 `export PATH=/opt/homebrew/bin:$PATH`。
3. **落代码** — 玩家逻辑进 `Runtime/<模块>/`，编辑器工具进 `Editor/<模块>/`，第三方增强走 `OdinInspector/` + asmref；跨模块协作用 `IService`，不互相 `new`。新增跨 Play 残留的静态状态必须 `SubsystemRegistration` 归位（泛型走 `ResetStaticsAssistant.Register()`）。
4. **补示例与测试** — 公开 API 必配能 Play 的最小示例 + `package.json.samples[]` 登记（`path` 指向 `Samples~`）；测试按三问门槛（可失败性 / 受害者 / 不可替代性），新增公开行为 1–3 例、纯重构 0 新增，需真 `Destroy`/帧/场景加载的进 PlayMode。
5. **更新文档**（易漏，按下表逐项勾）：
   | 改了什么 | 必更新 |
   |---|---|
   | 公开 API / 行为变更 | `Documentation~/<module>.md` + 包根中英 README 特性表 + 示例 + 两包 CHANGELOG `[Unreleased]` + 产品站模块/API 页 |
   | 新增示例 | `package.json.samples[]` + `Samples~/` 镜像 + 产品站示例总览 |
   | 新增模块 | 新 `Documentation~` 文档 + 包根 README + 本文件程序集/关键类型表 + 产品站 |
   | Bug 修复 | CHANGELOG `[Unreleased]` Fixed + 回归测试 |
   | 设计裁决 / 删机制 | `Aesir-Notebook` 的 `design-changelog.md` + `decisions/` + 本文件设计边界 |
   | 改 asmdef / 静态状态 | 无文档变更，但改 asmdef **必须重跑门禁**；改静态状态手动关 Domain Reload 连按两次 Play |
   用户写法变了还要同步 AI 扩展技能（唯一主位 `.codely-cli/extensions/AesirFrameworkGame/skills/`）。产品文档只写当前行为，历史演进写笔记仓。表中「产品站」指同仓主仓根 `docs/`（2026-11-23 起）；站点版本落点随主仓发版提交。
6. **门禁** — 依次：`sync_samples.py --check`（零漂移）→ 编译预热 0 error → EditMode → PlayMode（改运行时行为才需要）；`docs/` / `zensical.toml` 变更另跑主仓根 `zensical build --strict`（0 issue）。改测试/asmdef 全跑，纯文档补丁可豁免 PlayMode；全绿 exit 0、失败 exit 2 需解析 XML。
7. **提交与合并** — 一个功能一个 commit，Conventional Commits + message 写"为什么"；普通提交的主题/正文禁含 `[skip ci]` 等跳过标记字面量（会跳过 GitHub 同步流水线），确需跳过才用；精确路径 `git add`（勿 `-A`）。PR 提 GitHub 并正常 Merge；CNB AI 修复分支经反推按钮进 GitHub 后同样走 PR。功能迭代期只维护各包 CHANGELOG 的 `[Unreleased]`。

**真源与镜像**：`Samples/` 主位 → `Samples~/` 镜像（逐文件 diff 判方向，勿凭主位硬覆盖）；技能正文只在扩展目录；发版清单只认 `sync-to-cnb.yml` / `auto-release.yml` + version-sync skill。

---

## 发布与分支

- **`main`** — 开发主线（GitHub 主仓）。
- **常驻分支 `AesirArchitecture-latest` / `AesirModules-latest`** — GitHub `main` 推送时由 `sync-to-cnb.yml` 按包目录 `git subtree split` 滚动更新（包内容为分支根），**并进一步裁掉开发侧 `Samples/` 与 `Documentation/`，只保留 `~` 版本**（流水线内含残留断言：残留即 `exit 1`），随后 force 同步到 CNB。分支名永久固定：Git URL 一次输入持续更新，升级 = Package Manager 移除后用同一 URL 重新 Add。钉旧版本用 Release tag（`?path=<包路径>#v<版本>`；**版本 tag 一旦发布即不可变、永久保留**，版本号 bump 后的首次推送才创建新 tag）。流水线自动清理两边废弃的 `Aesir*-v*` 版本分支。
- **`.github/workflows/sync-to-cnb.yml`（主仓流水线，唯一跨仓同步入口）** — main push：split 包分支 → `update-info.json` 回写（`[skip ci]` 提交推回 GitHub main）→ 版本 tag（远端已存在即跳过）→ 新 tag 时 `gh workflow run` 派发 auto-release → **main / 两包分支 / tags 全部 force 推 CNB**（需 Secrets `CNB_USERNAME` / `CNB_TOKEN`），并对齐 CNB 侧多余 `v*` tag。
- **`.github/workflows/auto-release.yml`（GitHub Release）** — 主触发路径是 sync 流水线的 `workflow_dispatch` 派发（`GITHUB_TOKEN` 推的 tag 不触发 push workflow）；保留 push tags 触发给维护者手动推 tag 的场景。校验 tag 与两包 `package.json` 版本一致 → 纯 Python 构建 `AesirArchitecture` / `AesirModules` / `AesirFramework`（两包合并）→ 从根 CHANGELOG 抽该版本段落 → 滚动重建 GitHub Release（删旧 Release 保 tag）。
- **`.github/workflows/docs.yml`（文档站）** — main push 且 `docs/**` / `zensical.toml` / workflow 自身变更触发（另含 `workflow_dispatch`）：锁 zensical 0.1.1 → `build --strict` → GitHub Pages（deploy-pages）。与 sync-to-cnb 并存：docs-only 提交照常走 sync（CNB 全量镜像含 `docs/`），包分支 split 不受影响。
- **`.cnb.yml`（CNB 下载仓流水线）** — 只剩两条：`v*` tag push 构建三份 unitypackage 并发 CNB Release（国内加速，两边资产同源）；`web_trigger_push_github` 手动按钮把 CNB AI 修复分支反推 GitHub（按钮定义在 `.cnb/web_trigger.yml`，禁止反推 main / 常驻包分支）。
- **远端 CI 不跑 Unity 编译/测试**（不配 `UNITY_LICENSE`）；编译与 EditMode/PlayMode 验证在本地 batchmode 全绿后再推送。
- 发版前须全仓摸底旧版本号：两包 `package.json`、根 `CHANGELOG.md`（当前版本行 + 固定分支名 + 新段落）、两包中英 `README`、根 `README`/`README_EN`、`CODELY.md`、文档站。改版本号时注意：根 CHANGELOG 插入若用 `%` 格式化会被正文 `%` 撞格式符（改用占位符 `.replace()`）；`git add -A` 会带上测试产物；改名文件要连 `.meta` 一起 add。
- 提交信息中文；`git` 命令在本机需先 `export PATH=/opt/homebrew/bin:$PATH`（git-lfs 在该目录，否则 LFS filter 报 `git-lfs: command not found`）。

---

## 当前状态与待办（2026-11-23）

- 已发布 **0.35.0**（2026-11-23，事件模块大版本）。上一稳定版 0.34.1（2026-11-20）。
- **0.35.0 落地内容**：
  1. **（RAM）事件模块可观测性** —— 取消传播（`Cancel(canceller)` / `NonCancellable()`，First/Last 始终执行）+ 公开诊断 API `AesirEventDiagnostics`（订阅树查询 + 环形缓冲链路快照，零 Odin 依赖、关闭时零分配）+ 三个调试窗口（Event Monitor / Log / Tester，UI Toolkit 版 + Odin 版）；新增示例 `04_Cancellation` / `05_Diagnostics`。
  2. **（RAM）事件模块分发性能与结构精简** —— Type 键注册表 + 注册期有序插入（分发零排序）、去 `object[]` 间接层、非泛型订阅去反射、重入缓冲按深度复用、负缓存、空间过滤器每趟缓存。
  3. **（RAM·破坏性）** 移除 `[AesirListener]` 特性反射订阅（只保留 Script 订阅）；同一参数实例重入发布改 fail-fast；低频公开 API 收窄（`Clone()` / 字符串绑定键删除、`BindingInfo` 转 internal）。
  4. **（RAM）SDG 开放泛型成员默认值读取崩溃修复**。
  5. **（RAA）代码格式化与 XML 注释排版统一**（无运行时行为变更）。
- **2026-11-23 双仓架构反转**：GitHub 升为开发主仓、CNB 降为国内下载仓（原因：CNB AI Issue/PR 效果不佳）。新增 `.github/workflows/sync-to-cnb.yml`（split / update-info / 版本 tag / force 同步 CNB），`.cnb.yml` 瘦身为 CNB Release + 反推按钮，`auto-release.yml` 改由 sync 派发；tag 语义沿承 PR #7「一旦发布即不可变」。Secrets `CNB_USERNAME` / `CNB_TOKEN` 已配置，首跑已验证（run 45140383615）。
- **2026-11-23 文档站并入主仓**（提案 `openspec/changes/merge-docs-site/`）：`docs/` + `zensical.toml` + `docs.yml` 就位，站点 URL 改 `…/AesirFramework/`，旧仓 `AesirFramework-Site` 切全路径跳转壳；两包 `documentationUrl` / `changelogUrl` 与 Getting Started 窗口 404 死链（旧 `AesirFramework-Docs` 地址）已修，随下次发版到达用户。门禁实测：编译 0 error、EditMode 881/878/0/3、sync_samples 零漂移、zensical --strict 0 issue。
- **0.34.1 落地内容**（纯测试基础设施修复，无运行时代码变更）：
  1. **恢复测试程序集的 Odin 预编译引用** —— `fdc5724b` 误删后导致 `CS0012`（`AesirMonoBehaviour` 基类链在 Odin 环境指向 `SerializedMonoBehaviour`）；RAA PlayMode + RAM `Tests.Editor` / `Tests` / `Tests.UniTask` 四个 asmdef 补回 `Sirenix.*.dll`。测试只在本仓开发用、不随包分发，不受"运行时零 Odin 依赖"约束。
  2. **`ScriptDocGeneratorApiTests` 临时夹具移出包内只读目录** —— 改在可写 `Assets/` 下创建，规避 UPM 安装时 `Packages/` immutable folder 拿不到 `.meta` 的问题。
- **0.34.0 落地内容**：
  1. **（RAA）编辑器工具去 Odin 化 + UI Toolkit 复刻** —— Getting Started / Timer Debugger / Update / Scene Settings 四个窗口与 ObservableValue / 可观察集合 / AesirTimer 三个 Drawer 改用 UI Toolkit；新增 `Preferences → Aesir` 偏好页与 Editor 专属样式 SO（只 Editor 读取、不进构建），Odin 降级为纯样式开关；SceneAssetWrapper 绘制改 UI Toolkit 优先 + IMGUI 兜底、零 Odin 依赖。运行时代码一律不依赖 Odin（有 / 无 Odin 编译均 0 error）。
  2. **（RAA）`AesirTimer` 分层重构（无破坏性）** —— 单一类拆为 `AbstractTimer` 抽象基类 + 六种具体计时器（`Countdown` / `NextFrame` / `Repeating` / `Interval` / `Frequency` / `Stopwatch`），既有公开 API 零改动，示例补三种新形态，新增 9 例 EditMode 用例。
  3. **（RAM）模块配置资产改为显式创建** —— 移除导入即自动创建，改菜单 `Tools/Aesir/Modules/Create Module Config Assets` + 运行时内存兜底，不再写用户工程；配套更新两包文档与启动脚本模板。
  4. **（RAM）UI 实例化简化** —— 移除停用模板机制改 `Instantiate → SetActive(false)`，实例名去重复 `(Clone)`；`Resources.Load` 资产语义实证与文档决策复盘。
  5. **移除 Binder / SDG 的 UI Toolkit 窗口** —— 两个工具只保留原 Odin 窗口，数据层零 Odin 依赖不改。
  6. **（AI 扩展）`AesirFrameworkGame` 1.2.0 → 1.3.0** —— 新增 `aesir-project-structure` 技能（12 技能），统一资产命名去类型前缀与工程目录约定；按 PlaneWar 三篇复盘优化 boot-scene / framework / ui / router 技能。
- **门禁基线（0.35.0 实测）**：EditMode batchmode **881 total / 878 pass / 0 fail / 3 skip**（skip = Addressables 自适应 ×2 + batchmode 未保存场景下的预期 Ignore）；PlayMode batchmode **36/36**（RAA 20 / RAM 8 / UniTask 8）；编译预热 0 error。**改测试或 asmdef 必须重跑。**
- 门禁前置校验：`sync-samples --check` 零漂移；扩展 `validate.py` 0 错 0 警；主仓根文档站与 `Aesir-Notebook` 各自 `zensical build --strict` 无 issue。**改测试或 asmdef 必须重跑。**
- **待办 / 未决**：
  - PlaneWar 的 RAA 版案例（`Assets/PlaneWar/Scripts/Raa/`，程序集 `PlaneWar.Raa`）待编写（当前只有 Mono 版；README 已列出规划）。
  - `Aesir-Notebook/talks/dual-package/` 的演讲逐字稿与 deck 口径停在 0.32.0（831 条用例、包内 `Documentation/` 三件套），录制前需按 0.35.0 复核。
  - `.codely-cli/memory.md` 的整理收尾（ARCHIVE 已存在）。
- **UniTask 已通过 `manifest.json` 的 Git URL 安装** → `AESIR_MODULES_UNITASK` 生效，`SceneModuleUniTask` 与对应测试参与编译。新增依赖 UniTask 的代码须确认该宏；新增需要该宏的程序集须**自己再声明一份** `versionDefines` + `defineConstraints`。
- 无 Addressables 配置（UI 走 `ResourcesUILoader`，`IUIAssetLoader` 已预留接口）。

---

## 本仓实测坑位（改代码前先看）

- **编译冻结 / 域重载卡死**：任一程序集编译失败（含 `Assembly-CSharp` 里的生成文件）→ 域不重载，全部程序集停在旧版本（`isCompiling` 长期不落、磁盘 DLL 已新但反射找不到新成员、测试永远跑旧代码）。解法：`grep "error CS" Editor.log` 找到阻塞源修掉 → 重新触发编译 → 等域重载。
- **条件编译**：`versionDefines` 宏只对声明它的程序集可见；`defineConstraints` 不满足时先排除程序集再解析 references；name 引用缺失程序集被静默忽略。RAM 核心 asmdef（`Runestone.AesirModules.asmdef`）无条件 name 引用 `UniTask` 即依赖此行为：未装 UniTask 时该引用被静默忽略、`AESIR_MODULES_UNITASK` 宏不定义，`SceneModule` 自动回退协程驱动——可选集成成立，但属引擎隐式行为（asmdef 为严格 JSON 无法内嵌注释，故记于此），改名 / 移除该引用前须知此耦合。
- **manifest 本地路径依赖**：`cn.tuanjie.ai.generators` 指向 `.codely-cli/extensions/TJGenerators/`（gitignored）。新克隆未装扩展就打开 Unity → 包解析失败、相关脚本不编译。
- **Samples 示例场景污染**：出现 `BinderTag` / `BinderCodeGenerator` / `MenuProbe` 等 RAM 专有类型即污染（RAA 无 RAM 依赖）；本仓示例场景 `GraphicRaycaster` 的 `m_Bits` 惯例值 55。
- **PlayMode 门禁必须走 batchmode**：编辑器内跑 Test Framework 1.1.33 会误入 PlayerLauncher。
- **unitypackage 导出必须整目录剔除 `Tests/`**（含目录 `.meta`）：只删 asmdef 会让残留 `.cs` 并入 `Assembly-CSharp` 报 `CS0246`，比不删更糟。**边界仅 unitypackage 导出线；UPM 形态有意保留 `Tests/`，不要顺手删**——已实测：使用者在自己工程 `Packages/manifest.json` 的 `testables` 数组里加入包 ID（`cn.runestone.aesir.architecture` / `cn.runestone.aesir.modules`），Package Manager 就会编译包内测试程序集，可在 Test Runner 直接跑框架用例；不加则默认不编译，不污染消费者工程。包内测试一律假定自己可能跑在 `Packages/` 只读目录里：临时夹具建在可写的 `Assets/` 或系统 `Temp/`，别用 `AssetDatabase.CreateFolder` 往测试文件所在的包目录里塞东西（immutable folder 新建文件夹拿不到 `.meta`，Unity 报 "has no meta file, but it's in an immutable folder. The asset will be ignored."）。
- **`GITHUB_TOKEN` 推的 tag / 分支不触发 push 触发的 workflow（GitHub 平台限制）**：`sync-to-cnb.yml` 用默认 `GITHUB_TOKEN` 推 tag，`auto-release.yml` 的 push tags 触发**不会**运行——所以 sync 流水线推完新 tag 后用 `gh workflow run auto-release.yml --ref main -f tag=v<版本>` 显式派发（`workflow_dispatch` / `repository_dispatch` 是该限制的豁免事件，需 `actions: write` 权限）。维护者用自己凭证手动推 tag 则正常触发。**手动补发 GitHub Release**：`gh workflow run auto-release.yml --ref main -f tag=v<版本>`，或本地构建三包后 `gh release create`。**验收**：`gh release view <tag> --json assets` 应有 RAA/RAM/RAF 三份。历史注：CNB 主仓时代 git-sync 推的 tag 同样不可靠触发，曾用 CNB 流水线 REST API 直发（`publish_github_release.py`），2026-11-23 反转后该 stage 已删除。
- **版本 tag 不可变：发版后补内容一律 bump patch，不滚 tag（2026-11-20 铁律 + PR #7）**：两边 Release 正文都 checkout **tag 指向的 commit** 的 CHANGELOG 生成，与 main HEAD 无关——发版后再补 CHANGELOG 仅 push main 不会更新已发布 Release。tag 一旦推出即不可变（CNB 侧删 tag 会被后台自动 tag 规则立即重建、导致后续 push 被拒），**禁止重推同名 tag**；再小的更新也 bump patch 版本走完整发版流程，由流水线创建新 tag。GitHub 侧同 tag 重建 Release 走 `auto-release.yml` 的 `workflow_dispatch`（正文仍取自 tag commit）；CNB 侧正文只能随新 tag 更新。本地 CNB token 只有 code 权限，`PATCH /{repo}/-/releases/{id}` 需 `repo-release:rw` 会 403，别走 API 改正文。**验收**：CNB API `https://api.cnb.cool/<owner>/<repo>/-/releases`（带 `Accept: application/json`）比对 `body`，与 `gh release view` 的 GitHub 正文对齐。
- **OAuth 令牌无 workflow scope 时推不了 workflow 文件**：GitHub 拒绝「OAuth App 在未授予 `workflow` scope 时创建/更新 `.github/workflows/*`」。本地 `origin` 已是 SSH（不受此限）；仍受影响的是 CNB 反推按钮用的 `GIT_ACCESS_TOKEN`——修复分支若改了 workflow 文件，反推会被拒（`remote rejected ... without workflow scope`），这类修复改走本地 SSH 直推 GitHub。
- **Odin 面板**：`InspectorProperty.Draw` 对只读属性会 `PushGUIEnabled(false)` → 只读展示成员加 `[EnableGUI]`；多个 `ShowIf` 是 OR 语义；类级 `DetailedInfoBox` 只能一个；窗口 `PropertyTree` 必须在 `OnDisable` 里 `Dispose()`；**绘制回调禁止解析资产库单例**（会触发全项目重扫并报 `GUIStateObj is deleted`），单例收敛到 `OnEnable` 按域缓存。
- **UI 卸载时机**：`OnClose` 只在受控销毁路径调用，外部销毁只有 `OnDestroy` → 解绑放 `OnDestroy`。
- **引擎细节**：两个 Trigger Collider2D 之间至少一个 Kinematic Rigidbody2D 才会触发 `OnTriggerEnter2D`；`PlayerSettings.SetScriptingDefineSymbols` 值未变时不落盘（写完 grep 验证）；`SetScriptingDefineSymbols` 对废弃 `BuildTargetGroup` 抛异常，枚举清理需 try/catch。
- **Codely Bridge 脚本上下文硬限制**：裸 `Object` 报 CS0104（写 `UnityEngine.Object.*`）；无 `UNITY_EDITOR` 宏（`[Conditional("UNITY_EDITOR")]` 方法体被整体剔除）；禁 top-level `await`（会永久冻结）；`TestRunnerApi` 在该上下文不推进测试（走 batchmode）；`Camera` 不是 MonoBehaviour（`StartCoroutine` 反射调用报类型不匹配）；Play 中改脚本不即时编译，须 stop Play + `ImportAsset` + `Refresh` 另一次调用核对；编辑器无聚焦窗口时 `unscaledDeltaTime` 可达秒级。
- **Editor 生命周期**：EditMode 下 `Destroy` 是 no-op（销毁类用例必须进 PlayMode）；`exec_editor_script` 内 `new GameObject()` / `DestroyImmediate` 不标脏场景（要显式 `MarkSceneDirty`）；`[InitializeOnLoad*]` 里禁止直接 `AssetDatabase.CreateAsset` / 写脚本宏（推迟到 `EditorApplication.delayCall`，而后台失焦时 `delayCall` 不派发）。