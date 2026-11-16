# CODELY.md — AesirFramework

本文件是本仓库的项目指令上下文：说明项目用途、架构约定、目录归属、构建/测试命令与开发纪律。修改代码前先读对应章节。

---

## 项目概览

**AesirFramework** 是 **Runestone Yuumix** 开发的 Unity / 团结引擎框架集合，以单一 monorepo 发布两个可独立安装的 UPM 包，面向**独立游戏**的初学者与中级程序员，主打**精简 + 渐进式**。

| 项 | 值 |
|---|---|
| Unity 版本 | 2022.3.62f3c1（Unity 中国版） |
| 渲染管线 | URP 14.0.12 |
| C# 版本 | C# 9 |
| 许可证 | MIT |
| 作者 | [yuumixcode](https://github.com/yuumixcode) |
| 命名空间 | `Runestone.AesirArchitecture` / `Runestone.AesirModules`（品牌名"符文石"） |
| 代码注释 | 中文 XML 文档注释，标识符英文 |
| 规模 | 两个包内共 426 个 `.cs`（含 `Samples~/` 镜像；排除镜像为 352 个） |

### 两个包

| 包 | 包 ID | 版本 | 说明 |
|---|---|---|---|
| **Aesir Architecture（RAA）** | `cn.runestone.aesir.architecture` | 0.32.0 | 渐进式 MVC 架构：能力接口组合、Command/Query（CQRS 风格）、MiniEvent、ObservableValue / 可观察集合、AesirPlayerLoop、AesirTimer、纯 C# Context + MonoBehaviour 适配层 |
| **Aesir Modules（RAM）** | `cn.runestone.aesir.modules` | 0.32.0 | 功能模块：UI 框架（Panel + Canvas 根 Window）、事件模块、音频模块、场景模块、脚本文档生成（需 Odin） |

依赖关系：**RAA 不依赖任何 Aesir 包**；**RAM 依赖 RAA**（`package.json` 中**不声明**该依赖，见下方"依赖补全"）。

Aesir Inspector 已迁出为独立公开仓库，本仓不再包含。根目录残留的 `Runestone.AesirInspector.*.csproj`、`Runestone.ScriptDocGenerator.*.csproj`、`Runestone.AesirArchitecture.R3.*.csproj`、`Runestone.AesirModules.*Binder*.csproj` 等均为 **gitignored 的历史 IDE 生成物**（`.csproj` / `.sln` 全在 `.gitignore` 内），不代表现存程序集，勿据此判断项目结构。

### 设计基调（最高优先级裁决）

1. **RAA 极简是第一排序标准**：低频公开 API 一律删或收窄；新增能力前先问"能否由既有类型组合出来"。
2. **RAM 功能性优先于极简**：功能模块的面（UI / 事件 / 音频 / 场景）是价值，不是负担。
3. **核心架构默认最低档 Quick**：不建 Model/Service 接口、不做只读 property 包装、直接持具体类、`Register*` 注册实现类型；只有确需多实现/替换时才引入接口。
4. **Model = 运行时数据的底层数据源**，不是 UI 附属。
5. **低概率问题用文档约定杜绝，不加防御性代码兜底**；异常不吞噬（fail-fast）；不做初始化回滚、不做自愈轮询。
6. **暂不提供 MVVM（2026-11-16 裁决）**：快速开发首推 MVC；MVP 是 Unity 官方最佳实践所教授的严格分层范式（可选）。MVVM 的核心增量是数据绑定引擎，与"Unity 原生优先、不构建平行自建体系"基调相悖，状态通知已由 `ObservableValue<T>` 覆盖；业务侧可自行按 MVVM 风格组织代码，框架包不提供一等公民支持（无 IViewModel / 绑定引擎 / 官方模板）。重评触发：UI Toolkit 运行时绑定成为主力 UI 方案 / 大型数据驱动 UI 消除订阅胶水成为普遍诉求 / 社区明确广泛需求。完整论述见 `Aesir-Notebook/docs/decisions/architecture-core.md` 第十一章与 `AesirArchitecture/Documentation/AesirFramework-Skill/references/mode-decisions.md`。

---

## 目录结构

```
AesirFramework/
├── README.md / README_EN.md / CHANGELOG.md / CONTRIBUTING.md / CODE_OF_CONDUCT*.md
├── CODELY.md                      # 本文件
├── CodeStyle/AesirCodeStyle.cs    # 统一代码风格指南（不参与编译）
├── .github/
│   ├── workflows/auto-release.yml           # 发版：Build 三份 unitypackage + Release + 回写 update-info.json
│   ├── workflows/auto-publish-branches.yml  # main 推送时 subtree split 滚动 *-latest 常驻分支
│   └── scripts/build_unitypackage.py        # 打包脚本（纯 Python，禁换第三方 CLI）
├── Assets/
│   ├── Runestone/
│   │   ├── AesirArchitecture/     # RAA 包根
│   │   └── AesirModules/          # RAM 包根
│   ├── Scripts/                   # 宿主工程自用脚本（不属于两个包）
│   │   ├── HUDContext.cs / HUDPanel.cs      # Binder 生成的示范面板与 Context
│   │   └── Editor/AesirPackageExporter/     # Tools/Aesir/Export Packages 导出工具
│   ├── Scenes/SampleScene.unity
│   ├── Settings/                  # URP 资源（UniversalRP / Renderer2D / Lit2DSceneTemplate）
│   └── Plugins/Sirenix/           # Odin Inspector（gitignored，独立授权）
└── Packages/manifest.json
```

**gitignored 的本地产物（不要提交、也不要当作事实来源）**：`Library/`、`Temp/`、`obj/`、`Logs/`、`UserSettings/`、`Exports/`（历史 unitypackage）、`ScriptDocGenerator/`（默认文档输出根）、`ScriptableSingleton/`、`TestResults*.xml`、`*.log`、`screenshots/`、`Assets/Resources/`（模块配置资产，运行时生成）、`Assets/Editor Default Resources/`、`*.csproj`、`*.sln`。

`.codely-cli/` 大多忽略，但 **`.codely-cli/extensions/TJGenerators` 与其 Bridge 包缓存已入库**（见 `.gitignore` 注释），`.codely-cli/skills/` 仍是本地工具链、不入库。

### 两个包的标准结构

```
<包根>/
├── package.json / README.md / CHANGELOG.md / LICENSE.md / Third Party Notices.md
├── AesirPathLookup.asset        # 安装位置锚点（GUID 固定，勿删/勿改）
├── Runtime/                     # 运行时锚点 asmdef 在层根，模块以子目录汇入
├── Editor/                      # 编辑器锚点 asmdef 在层根
├── Tests/                       # Editor（EditMode）/ Runtime（PlayMode）
├── Samples/                     # 编写主位（Assets 内可见、可运行、构建剔除）
├── Samples~/                    # 发布镜像（Package Manager 按需导入）
├── Documentation/               # 文档主位（Assets 内可见、随 unitypackage 导出、不进构建）
└── Documentation~/              # 文档镜像（无 .meta）
```

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
| `AesirTimer` | 唯一计时器原语（`Runtime/Modules/Timer`）：`Countdown(seconds, onFinished, unscaled)` / `NextFrame(onFinished)` / `Every(interval, onTick, repeatCount = -1, unscaled)`；读 `Duration`/`Remaining`/`Progress`/`RemainingExecutions`/`IsFinished`/`IsCancelled`/`IsPaused`；`Pause()`/`Resume()`/`Restart()`/`Cancel()`（`Dispose()` 等价取消）。复用 `AesirPlayerLoop`（BeforeUpdate + `order: int.MinValue`），结算期与执行期分离，回调内新建/取消延后一帧；重复形态一次结算最多回调一次（卡顿不补发）。调试面板 `Tools/Aesir/Architecture/Timer Debugger`（Odin + IMGUI 双版本，数据源/样式分层，拉取节奏可调，编辑模式恒为零）与字段检视（原生需 `[SerializeReference]`，Odin 需 Odin 序列化宿主或 `[ShowInInspector]`；普通字段看示例的 `AesirTimerSampleInspector` 写法） |
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

其余示例：`ObservableValue`（Odin Drawer）、`ObservableCollections`、`MiniEvent`、`AesirTimer`（延时 / 冷却 / 重复 / 暂停 + 只读查询面板 + 自定义 Inspector 状态表与动作按钮，配 `Timer Debugger` 窗口）、`PlaneWar`（Mono 版纵版射击实战）、`RuntimeInitializeLoadType`（五个阶段顺序，Editor-only asmdef，开关在 `Tools → Aesir → Architecture → Samples/RuntimeInitializeLoadType`）。

### RAA 编辑器工具（`Editor/`）

- **Getting Started 窗口**（0.24.0）— `Tools/Aesir/Getting Started`，priority **-980**（居 Tools/Aesir 顶部）。以各包 `package.json` 的 `samples` 清单为唯一真源；覆盖 Assets 安装 / 嵌入式 / UPM 三种发现；未导入示例可窗口内一键导入（`PackageManager.UI.Sample` API）。Odin 版与 IMGUI 兜底版经 `OdinWindowOpener` 静态委托路由。
- **包内更新器** — `Tools/Aesir/Check for Updates`，priority **1100**（置底）。面向"代码导入 `Assets/Runestone`"的用户：锚点定位安装根 → 扫描 `*/package.json` → 三层版本检测（GitHub 直连 → 镜像站 `ghproxy.net`/`gh-proxy.com` → jsDelivr）→ 下载 `<包目录名>-v<版本>.unitypackage` → 差集删除残留条目 → 静默导入 → 登记 `.aesir/installed-manifest.json`。整段更新用 `EditorApplication.LockReloadAssemblies()` 防域重载打断；超时双判据（总时长 + 无进展）。纯 UPM 安装时菜单隐藏（无管辖对象）。
- **安装位置锚点** — 每个包根 `AesirPathLookup.asset`（`ScriptableObject` 空壳，`.meta` GUID 固定写在 `AesirAssetPaths` 常量里）。解析顺序：默认根 `Directory.Exists` → 锚点 GUID 查询 → `FindAssets("t:AesirPathLookup")` 兜底；域重载后重解析。**勿删、勿改 GUID。**
- **构建剔除** — `AesirSamplesBuildFilter` 经 `BuildPlayerWindow.RegisterBuildPlayerHandler` 把 Aesir 示例场景从本次构建选项剔除并打印 `[Aesir Build]` 日志（不改 Build Settings 持久数据）。仅拦截窗口构建入口，CI 直接调 `BuildPipeline.BuildPlayer` 不经过钩子。
- **导出工具** — 宿主工程 `Assets/Scripts/Editor/AesirPackageExporter/`：`Tools/Aesir/Export Packages/Export All Packages | Export Aesir Architecture | Export Aesir Modules`（priority 1000/1001/1002）。
- **计时器调试面板** — `Tools/Aesir/Architecture/Timer Debugger`，priority **996**。Odin 版（`AesirTimerDebugWindowOdin`，经 `OdinWindowOpener` 路由）+ IMGUI 兜底版。**数据源与样式分层**：`AesirTimerSnapshot` / `AesirTimerStatistics` 只搬数据（零文案零颜色）→ `AesirTimerDebugSource` 管抓取节奏与汇总 → `AesirTimerText` 文案/颜色样式 + `AesirTimerSummaryView` 单行摘要外观 → 两个窗口只做排布。换面板样式只换视图。**拉取与重绘分离**：工具栏配拉取模式（按时间 0.05–10s / 按帧 1–600，`EditorPrefs` 持久化，默认 0.5s），到点才枚举注册表并重建快照，其余重绘只读缓存行。**编辑模式无数据源**：非 Play 模式不枚举注册表、不读累计计数，统计与表格恒为零，Play 状态切换那一刻立即重抓。列：状态 / 形态 / 时间源 / 剩余 / 进度 / 已执行 / 还剩次数 / 存活 / 回调；排序为"运行中在前 + 剩余时间升序"，超 200 行截断。`Editor` 程序集经 `InternalsVisibleTo` 读运行时内部状态。
- 菜单分组惯例：父组 priority 取子项最小值，差 > 10 自动插分割线。新增 RAA 工具进 `Tools/Aesir/Architecture/`，RAM 工具进 `Tools/Aesir/Modules/`。

### 设计边界（RAA）

不做：事件总线 / EventChannel、Context 多实例、Command/Query 池化与 async 与 Undo/Redo、`AesirTimer` 之外的调度原语（`Start`、秒表、对象池）、View 生命周期脚手架（交给 RAM 的 UIModule）、线程安全（仅主线程）、集合高级能力（队列/过滤视图/环形缓冲等用上游 Cysharp.ObservableCollections）。

时间调度的四条路径边界：MonoBehaviour 内本地 float 字段 / `MonoLifecycleProxy` 帧代理 / `AesirTimer` / 协程。`AesirTimer` 内部的两层暂停：全局靠 `Time.timeScale`（`unscaled` 不受影响），单实例靠 `Pause()`/`Resume()`。

---

## Aesir Modules（RAM）

模块间零依赖，删除模块 = 删 `Runtime/<模块>/` + `Editor/<模块>/`。

### UI 模块（`Runtime/UI`）

- **`UIModule`** — `AesirMonoBehaviour` 单例（`dontDestroyOnLoad` 序列化字段控制 DDOL），管理面板与窗口生命周期。
  - 静态门面：`Show<T>` / `Hide<T>` / `Get<T>` / `Prewarm<T>` / `RegisterPrefab<T>` / `ContainPrefab<T>`；窗口：`Open<T>` / `Close<T>` / `GetWindow<T>` / `PrewarmWindow<T>` / `RegisterWindowPrefab<T>` / `ContainWindowPrefab<T>`。实例侧为长名（`ShowPanel` / `OpenWindow` …）。
- **`IUIPanel`** — `Initialize → Show(payload) → Hide → DestroyPanel`；`Layer`（`UILayer`）、`DestroyOnHide`、`IsOpen`。
  - `AesirBasePanel` / `AesirBasePanelView<T>` / `AesirBasePanelViewController<T>`；面板实例化到 `UIRoot` 四层 Canvas 下。
- **Canvas 根窗口形态** — `IUIWindow`：`OnInit → OnShow → OnHide → OnClose`；`AesirBaseWindow` / `AesirBaseWindowView<T>` / `AesirBaseWindowViewController<T>`；预制体根节点自带 Canvas，直接挂 `UIRoot` 下，不经四层层级；`sortingOrder` 为单一真源（默认 500）；预制体约定 `Mask` + `Content` 子物体。
  - 蒙版：初值在 `UIModuleConfigSO` 单例资产，运行时经 `MaskMode` 切换；蒙版点击走 `AesirBaseWindow.OnMaskClicked()` 虚方法，`closeOnMaskClick`（默认 false）。层级枚举 `UIMaskMode`。
  - 命名规范：窗口类/脚本/预制体以 `Window` 结尾（不用 `XxxCanvas`，会被误读为 Canvas 派生类）。
- **`UIRoot`** — 构建四层 Canvas 层级；层 Canvas / UICamera / EventSystem 为序列化引用持久化（`UICanvasConfigSO`）。
  - **世界空间 UI Canvas 预设** — `WorldSpaceUICanvas`（Add Component → Aesir Modules → UI）：`RenderMode.WorldSpace` + RectTransform/CanvasScaler/GraphicRaycaster 自动接线，默认 sortingOrder 450（面板层 100–400 与窗口 ≥500 之间）；`FollowTarget`（空=跟随父级）+ `PositionOffset` + `FaceCamera`/`Billboard` 控制跟随与朝向，子树归入 UI 层(5)；`UIRoot.CreateWorldSpaceCanvas(target, name, config)` 运行时创建。世界空间配置走独立链（组件 → `UIRoot.WorldSpaceCanvasConfig` → `UICanvasConfigSO.CreateWorldSpaceDefault()`），**不进 UIModule 注册表 / 蒙版调度**，生命周期随场景宿主。
- **`IUIAssetLoader` / `ResourcesUILoader`** — 可插拔资源加载，**同步契约**（Addressables 需预加载后同步返回）；`RegisterAssetLoader` 替换。
- **`BinderAssistant` / `BinderTag`**（Odin 程序集）— UI 元素绑定系统 + 代码生成；窗口感知，根节点带 Canvas 时默认 `Window` 后缀与 `AesirBaseWindow` 基类。生成脚本为 partial + 「同一脚本增量」模式，只替换「绑定字段」region。
- 已知契约：`OnClose` 只在受控销毁路径（Hide + DestroyOnHide）调用，**外部销毁只有 `OnDestroy`** → 解绑逻辑放 `OnDestroy`。同基类类型重复 Show 报错；Hide/Get 未命中给 Warning；缺层 fail-fast。

### 事件模块（`Runtime/Events`）

- `EventModule` 直接继承 `AesirMonoBehaviour`；双注册表分离：AttributeBindings（`[AesirListener]` 静态订阅，返回 `AutoRemoveListenerHandle`）+ DynamicBindings（脚本订阅）。
- `BindingInfo`（`StaticBindingInfo` / `DynamicBindingInfo<T>`）+ `SubscriberPriority`（First / High / Medium / Last；Attribute 默认 High，Script 默认 Medium）。
- 事件参数继承 `AesirEventArgs`，命名 `XxxEventArgs`；`WithFilter/WithFilters` 链式过滤器（`ISubscriberFilter`：`WithTag` / `WithPriority` / `SameSceneAsEmitter` / `OnlySelf` / `InsideCollider2D`，**fail-closed**，未过滤零开销）。
- 分发期自动清理已销毁订阅者（编辑器告警、玩家构建静默）；`executionMsLimit` 毫秒阈值性能监控（默认 0 关闭）。
- 绑定键按事件类型缓存（分发热路径稳态零分配）。
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

`Runtime/ScriptDocGenerator/OdinInspector` + `Editor/ScriptDocGenerator/OdinInspector`（经 asmref 汇入 Odin 程序集）。反射生成结构化 API 文档（增量保留 `## Additional Notes`）+ Summary 双向同步工具。静态入口 `ScriptDocGeneratorAPI`（`GenerateDocsForType/ForTypes/ForAssembly/ForFolder`，`settings == null` 用 Default，`outputFolder == null` 用项目根 `ScriptDocGenerator/`；全程无弹窗）。菜单：`Tools → Aesir → Modules → Script Doc Generator`。Summary 工具的权威源是 `[Summary]` 特性。

### 依赖补全（`Editor/Bootstrap`）

`package.json` **不声明任何 Aesir 依赖**：UPM 不支持包内 Git URL 依赖（保留会让单装直接失败），`com.unity.test-framework` 也非硬依赖。RAA 缺席由 `Tools/Aesir/Modules/Install Dependencies`（priority **998**）补装：确认框 → `Client.Add`（固定常量 URL，指向常驻 `AesirArchitecture-latest` 分支）→ `SessionState` + `[InitializeOnLoadMethod]` 跨域重载收尾。

**铁律**：`Runestone.AesirModules.Editor.Bootstrap` 是**零引用程序集**（`references: []`），不得加 RAM 核心 / RAA / Odin 引用，否则缺依赖时菜单自身不编译。

### 设计边界（RAM）

不做：Addressables 默认集成（UI 默认 `ResourcesUILoader`，接口已预留）、AudioMixer、事件系统元事件 / DefaultChannel（编辑器工具链依赖，暂缓）、UI 虚拟列表与滚动复用。

---

## 程序集定义

### RAA（17 个 asmdef，`Samples~/` 按同名镜像一份）

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
| `Runestone.AesirArchitecture.Samples.PlaneWarMono` | `Samples/PlaneWar/Scripts/Mono/` |
| `Runestone.AesirArchitecture.Samples.RuntimeInitializeLoadType` | `Samples/RuntimeInitializeLoadType/Editor/`（Editor-only） |

示例程序集为**运行时程序集 + 整文件 `#if UNITY_EDITOR` 包裹**：编辑器内正常编译/挂载/Play，玩家构建整体剔除。**不要改回 Editor-only asmdef**——其 MonoBehaviour 会被判定为编辑器脚本而禁止挂场景（Missing Script）。

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

### 现状（2026-11-12 实测）

- **6 个测试程序集 / 72 个测试文件**；属性层面 `[Test]` 690、`[TestCase]` 67、`[UnityTest]` 37（参数化展开后约 800+ 条）。
- 最密、触碰时优先收敛：`BinderCodeGeneratorTests`、`AesirUpdateServiceTests`、`SourceParsingTests`、`AudioModuleTests`、`EventModuleTests`、`XmlSummaryToolTests`。
- 改测试或 asmdef **必须重跑**；纯文档补丁可豁免 PlayMode。

---

## 构建与运行

### 编辑器

1. 用 Unity 2022.3.62f3c1 打开项目（首次导入会编译两包 + 示例）。
2. 主推示例：`Assets/Runestone/AesirArchitecture/Samples/Counter-Mvc-Quick/Scene/SampleForCounterMvcQuick.unity`。
3. Odin Inspector 可选：装于 `Assets/Plugins/Sirenix/`（gitignored）。未安装时 `ODIN_INSPECTOR` 相关内容整体编译剔除，功能可用但 Binder / SDG / Odin 窗口退化。

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

Unity 侧无自定义构建脚本；示例脚本整文件 `#if UNITY_EDITOR`，玩家构建自动排除。unitypackage 由 CI 的 `.github/scripts/build_unitypackage.py` 产出（纯 Python，**禁止换第三方 CLI**——实测会产出重复 GUID / 全零 meta / 丢文件夹条目；跳过 `~` 后缀与 `.` 开头目录；包根 `Tests/` 整目录剔除，含目录 `.meta`）。

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

### Samples / Documentation 双目录

- `Samples/`、`Documentation/` 是**编写主位**；`Samples~/`、`Documentation~/` 是 UPM 镜像（`~` 目录对 AssetDatabase 不可见、不进 unitypackage）。
- 同步方向：先改主位，再同步镜像（含 `.meta`，GUID 与 UPM 导入链路一致；`Documentation~/` 不含 `.meta`）。
- 本机同步脚本：`.codely-cli/skills/sync-samples/scripts/sync_samples.py --check`。**逐文件 diff 判断方向，不要凭"主位为准"硬覆盖**（历史上主位反而是错的一侧）；脚本会校验 `package.json` 的 `samples` 清单。`rsync --exclude='*.meta'` 会误配 `.md.meta`，应写 `--exclude='*.md.meta'`。
- 文档三类：`Documentation/` 只放 ①英文 `README_EN` ②使用文档 ③AI Skill。迭代过程与内部设计文档写私有笔记仓 `Aesir-Notebook/`，不随包分发；包内不保留设计变更记录。

### 外部关联目录（均被 `.gitignore` 排除）

- `Aesir-Notebook/` — 私有开发笔记（Zensical 站点，含 `docs/standards/unit-testing-standards.md` 与 `docs/standards/design-changelog.md` 权威源）。
- `AesirFramework-Site/` — 公开产品站（Zensical + GitHub Pages），`docs/architecture/`、`docs/modules/` 物理路径被 SDG 与 `package.json` 的 `documentationUrl` 依赖，**不可移动**。

---

## 发布与分支

- **`main`** — 开发主线。
- **常驻分支 `AesirArchitecture-latest` / `AesirModules-latest`** — CI 在 main 推送时按包目录 subtree split 滚动更新（包内容为分支根）。分支名永久固定：Git URL 一次输入持续更新，升级 = Package Manager 移除后用同一 URL 重新 Add。钉旧版本用 Release tag（`?path=<包路径>#v<版本>`；**历史已发布版本 tag 永久保留**，当前在研版本 tag 随 push main 滚动刷新、版本号 bump 后冻结——钉版本钉历史稳定版）。CI 自动清理废弃的 `Aesir*-v*` 版本分支。
- **`auto-release.yml`** — 每次 push main 滚动构建三份 unitypackage（`AesirArchitecture` / `AesirModules` / `AesirFramework`，纯 Python `build_unitypackage.py`，CI 无需 Unity 编辑器）+ 创建 Release + 把 `update-info.json`（版本/tag + 各包文件清单）以 `[skip ci]` 提交回 main。**远端 CI 不跑 Unity 编译/测试**（不配 `UNITY_LICENSE`）；编译与 EditMode/PlayMode 验证在本地 batchmode 全绿后再推送（见测试规范）。`auto-publish-branches.yml` 仅镜像裁剪双向结构校验，同样无 Unity 测试。
- 发版前须全仓摸底旧版本号：两包 `package.json`、根 `CHANGELOG.md`（当前版本行 + 固定分支名 + 新段落）、两包中英 `README`、根 `README`/`README_EN`、`CODELY.md`、文档站。改版本号时注意：根 CHANGELOG 插入若用 `%` 格式化会被正文 `%` 撞格式符（改用占位符 `.replace()`）；`git add -A` 会带上测试产物；改名文件要连 `.meta` 一起 add。
- 提交信息中文；`git` 命令在本机需先 `export PATH=/opt/homebrew/bin:$PATH`（git-lfs 在该目录，否则 LFS filter 报 `git-lfs: command not found`）。

---

## 当前状态与待办（2026-11-12）

- 已发布 **0.32.0**；工作区在 `[Unreleased]` 上继续开发，当前未提交的增量主要是 **`AesirTimer`**：`Runtime/Modules/Timer/`（原 `Runtime/Common/`，已迁为内置功能模块）+ `Tests/Editor/AesirTimerTests.cs`（15 例）+ `Tests/Runtime/AesirTimerTests.cs`（3 例）+ `Editor/Timer/`（调试面板、设置、字段绘制器）+ `Editor/OdinInspector/Timer/`（Odin 版窗口与 Drawer）与配套 CHANGELOG / 文档 / README 改动。
- **UniTask 已通过 `manifest.json` 的 Git URL 安装** → `AESIR_MODULES_UNITASK` 生效，`SceneModuleUniTask` 与对应测试参与编译。新增依赖 UniTask 的代码须确认该宏；新增需要该宏的程序集须**自己再声明一份** `versionDefines` + `defineConstraints`。
- PlaneWar 的 RAA 版示例（`Samples/PlaneWar/Scripts/Raa/`）待编写（当前只有 Mono 版）。
- 无 Addressables 配置（UI 走 `ResourcesUILoader`，`IUIAssetLoader` 已预留接口）。

---

## 本仓实测坑位（改代码前先看）

- **编译冻结 / 域重载卡死**：任一程序集编译失败（含 `Assembly-CSharp` 里的生成文件）→ 域不重载，全部程序集停在旧版本（`isCompiling` 长期不落、磁盘 DLL 已新但反射找不到新成员、测试永远跑旧代码）。解法：`grep "error CS" Editor.log` 找到阻塞源修掉 → 重新触发编译 → 等域重载。
- **条件编译**：`versionDefines` 宏只对声明它的程序集可见；`defineConstraints` 不满足时先排除程序集再解析 references；name 引用缺失程序集被静默忽略。RAM 核心 asmdef（`Runestone.AesirModules.asmdef`）无条件 name 引用 `UniTask` 即依赖此行为：未装 UniTask 时该引用被静默忽略、`AESIR_MODULES_UNITASK` 宏不定义，`SceneModule` 自动回退协程驱动——可选集成成立，但属引擎隐式行为（asmdef 为严格 JSON 无法内嵌注释，故记于此），改名 / 移除该引用前须知此耦合。
- **Samples 示例场景污染**：出现 `BinderTag` / `BinderCodeGenerator` / `MenuProbe` 等 RAM 专有类型即污染（RAA 无 RAM 依赖）；本仓示例场景 `GraphicRaycaster` 的 `m_Bits` 惯例值 55。
- **PlayMode 门禁必须走 batchmode**：编辑器内跑 Test Framework 1.1.33 会误入 PlayerLauncher。
- **unitypackage 导出必须整目录剔除 `Tests/`**（含目录 `.meta`）：只删 asmdef 会让残留 `.cs` 并入 `Assembly-CSharp` 报 `CS0246`，比不删更糟。边界仅 unitypackage；UPM 形态维持现状（Package Manager 默认不编译包内测试程序集，需 Testables 主动 opt-in）。
- **Odin 面板**：`InspectorProperty.Draw` 对只读属性会 `PushGUIEnabled(false)` → 只读展示成员加 `[EnableGUI]`；多个 `ShowIf` 是 OR 语义；类级 `DetailedInfoBox` 只能一个；窗口 `PropertyTree` 必须在 `OnDisable` 里 `Dispose()`；**绘制回调禁止解析资产库单例**（会触发全项目重扫并报 `GUIStateObj is deleted`），单例收敛到 `OnEnable` 按域缓存。
- **UI 卸载时机**：`OnClose` 只在受控销毁路径调用，外部销毁只有 `OnDestroy` → 解绑放 `OnDestroy`。
- **引擎细节**：两个 Trigger Collider2D 之间至少一个 Kinematic Rigidbody2D 才会触发 `OnTriggerEnter2D`；`PlayerSettings.SetScriptingDefineSymbols` 值未变时不落盘（写完 grep 验证）；`SetScriptingDefineSymbols` 对废弃 `BuildTargetGroup` 抛异常，枚举清理需 try/catch。