# Aesir Modules

Aesir Architecture (RAA) 的功能模块包。当前提供 UI 框架（Manager of Managers 模式）、实验性事件模块、音频管理、场景管理工具与脚本文档生成工具。

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE.md)
[![Version](https://img.shields.io/badge/version-0.19.0-blue.svg)](./CHANGELOG.md)
[![Unity](https://img.shields.io/badge/Unity-2022.3%2B-black.svg)](https://unity.com/)
[![Install via Git URL](https://img.shields.io/badge/UPM-Git%20URL-blueviolet.svg)](#安装)
[![English](https://img.shields.io/badge/README-English-blue.svg)](./Documentation/README_EN.md)

> 📦 **本包是 [AesirFramework](https://github.com/yuumixcode/AesirFramework) monorepo 的一部分**。本包**依赖**：
> - **[Aesir Architecture](https://github.com/yuumixcode/AesirFramework)**（`>= 0.19.0`）

## 模块总览

| 模块 | 状态 | 说明 |
|------|------|------|
| UI | 已实现 | `UIModule` 单例（Manager of Managers）+ `UIRoot` 四层 Canvas + 面板生命周期 + 可插拔资源加载 |
| Event | ⚠️ 实验性 | `EventModule` 双轨订阅（Attribute + Script）+ 4 档优先级 + 表达式树优化 + 订阅者过滤器（精确投递）+ 死引用清理 + SO 资产化。尚未在实际项目中验证 |
| Audio | 已实现 | `AudioModule` 单例（2D 音频极简门面）+ SFX 独占音源轮询 + BGM 淡入淡出 + 三通道音量/静音持久化 |
| Scene | 已实现 | `SceneModule` 场景加载/叠加/卸载/激活场景切换 + 场景事件广播 + `SceneAssetWrapper` 可序列化场景引用 + 编辑器工具（BootstrapSceneHelper / Scene Editor Settings） |
| ScriptDocGenerator | 已实现（需 Odin） | 反射分析 C# 类型生成结构化 API 文档（增量保留手写内容）+ Summary 工具（XML `<summary>` 与 `[Summary]` 特性同步，特性优先） |

> 另有两项可选能力：**Binder 组件绑定**（`Runtime/UI/OdinInspector/Binder/`，需 Odin Inspector）与 **Input System 输入模块适配**（`Runtime/UI/InputSystem/`，独立程序集，启用 Input System 时自动生效）。

## 依赖

- **Aesir Architecture (RAA)** `cn.runestone.aesir.architecture` >= 0.19.0（必需）
- **Odin Inspector**（可选）：仅通过 `#if ODIN_INSPECTOR` 条件编译参与，未导入时自动排除。注意 **Scene 模块的 `SceneAssetWrapper` Inspector 面板效果（拖拽赋值、着色、一键修复按钮）依赖 Odin**；未安装 Odin 时仅保证 API 可用（`FromScenePath` 构造 / `SceneAsset` 代码赋值 / TryGet 家族），面板不支持。

## 目录组织

包采用标准 Unity 自定义包根结构：`Runtime/` 与 `Editor/` 为包根两级目录，功能模块以子目录形式存在于对应层级（如 `Runtime/UI/` 与 `Editor/UI/`）；共享基础位于 `Runtime/Common/`。

程序集组织：

- **核心程序集**锚点在层根（`Runtime/Runestone.AesirModules.asmdef`、`Editor/Runestone.AesirModules.Editor.asmdef`）——层内模块主代码默认汇入对应核心程序集，无需 asmref
- **细分程序集**锚点在 `Common/` 下（Odin 运行时 `Runtime/Common/OdinInspector/`、Odin 编辑器 `Editor/Common/OdinInspector/`、Addressables 胶水 `Editor/Common/Addressables/`）；模块的对应专属代码放在自己的 `OdinInspector/`、`Addressables/` 子目录，经 **Assembly Definition Reference（asmref）** 汇入
- **删除模块** = 删除 `Runtime/<模块>/` 与 `Editor/<模块>/`（如存在），不会影响其余模块编译

## 安装

### UPM（Git URL，推荐）

在 Unity Package Manager 窗口 `+` → `Add package from git URL...`：

```
https://github.com/yuumixcode/AesirFramework.git#AesirModules-v0.19.0
```

或编辑 `Packages/manifest.json`：

```json
{
  "dependencies": {
    "cn.runestone.aesir.modules": "https://github.com/yuumixcode/AesirFramework.git#AesirModules-v0.19.0"
  }
}
```

跟踪 main 最新开发版：把 URL 换成 `https://github.com/yuumixcode/AesirFramework.git?path=Assets/Runestone/AesirModules`。

UPM 会自动解析 `package.json` 的 `dependencies` 字段，拉取 Aesir Architecture。

### unitypackage 导入

从 [GitHub Releases](https://github.com/yuumixcode/AesirFramework/releases) 下载 `AesirModules-v<版本>.unitypackage`（或两包合并的 `AesirFramework-v<版本>.unitypackage`）导入。以此方式安装在 `Assets/Runestone/` 下的包，可通过 Unity 菜单 `Tools → Aesir → Check for Updates`（随 Aesir Architecture 分发）一键检查并更新。

## UI 模块

### 核心类型

| 类型 | 层 | 说明 |
|------|----|------|
| `UIModule` | Component | UI 管理器单例：面板注册、显示、隐藏、预热与注册表维护；提供静态快捷 API 供全局调用 |
| `UIRoot` | Component | UI 根节点：构建四层 Canvas（Background / Normal / Popup / Top）+ UICamera + EventSystem，应用 Canvas 统一配置 |
| `IUIPanel` | Engine | 面板契约：生命周期 `Initialize → Show(payload) → Hide → DestroyPanel`；属性 `Layer` / `DestroyOnHide` / `IsOpen` |
| `AesirBasePanel` | Component | 面板抽象基类：虚方法 `OnInit` / `OnShow` / `OnHide` / `OnClose`，序列化字段 `layer` / `destroyOnHide`，便捷方法 `HideSelf()` |
| `AesirBasePanelView<T>` | Component | MVP 模式面板视图基类：继承 `AesirBasePanel` 并按 Context 类型绑定（`IView`），经 Context 访问 Model / Service |
| `AesirBasePanelViewController<T>` | Component | MVC 模式面板控制器基类：继承 `AesirBasePanel` 并按 Context 类型绑定（`IController`），经 Context 访问 Model / Service 并可执行 Command / Query |
| `IUIAssetLoader` / `ResourcesUILoader` | Engine | 可插拔资源加载契约与默认实现（Resources 目录）。加载契约为**同步语义**：适用于 Resources、同步缓存等管线；Addressables 等异步管线需自行预加载后同步返回 |
| `UICanvasConfigSO` | Component | Canvas 统一配置资产（可经 Create 菜单创建默认资产） |
| `UILayer` | Engine | 层级枚举：Background / Normal / Popup / Top |

### 快速开始

1. 菜单 `GameObject → Aesir Modules → Create UIRoot` 创建带完整层级结构的 UI 根节点（或在场景中预放置挂载 `UIRoot` 的物体）。
2. 创建面板预制体，根节点挂脚本继承 `AesirBasePanel`（MVP 模式继承 `AesirBasePanelView<TContext>`）。
3. 注册预制体并显示面板：

```csharp
// 注册面板预制体
UIModule.RegisterPrefab<MainMenuPanel>(prefab);

// 显示面板
UIModule.Show<MainMenuPanel>();

// 带参数显示（强类型 payload）
UIModule.Show<ConfirmDialogPanel, ConfirmData>(new ConfirmData { message = "确定？" });

// 关闭面板（按面板的 DestroyOnHide 决定销毁或缓存复用）
UIModule.Hide<ConfirmDialogPanel>();

// 预热：预实例化并隐藏，首次 Show 直接复用，避免卡顿
UIModule.Prewarm<MainMenuPanel>();
```

需要自定义资源加载时替换默认加载器：

```csharp
UIModule.Instance.RegisterAssetLoader(new MyAddressablesLoader());
```

> **加载契约**：`IUIAssetLoader.Load` 为同步语义，适用于 Resources、同步缓存等管线。Addressables 等异步管线无法在接口内表达等待——需自行预加载后同步返回（`Handle.Result` 存在 WebGL 死锁与主线程阻塞风险，请自行评估）。

4. 面板生命周期（全部由 `UIModule` 驱动）：

```csharp
public class MainMenuPanel : AesirBasePanel
{
    protected override void OnInit() { }               // 首次实例化后调用一次
    protected override void OnShow(object payload) { } // 每次显示时调用（含首次）
    protected override void OnHide() { }               // 隐藏时调用（默认 SetActive(false)）
    protected override void OnClose() { }              // 受控销毁（HidePanel + DestroyOnHide=true）前调用
}
```

> **生命周期细节**：面板以停用状态实例化（Awake / OnEnable 推迟到 Show 激活时才触发，保证 OnEnable 可安全访问 OnInit 之后才有值的引用），按 挂层 → `Initialize` → `Show` 顺序驱动；面板注册以实例的**实际类型**为键——预制体上挂载的脚本是注册类型的派生类时，以基类类型操作会命中键语义诊断（重复 Show 报错拒绝、Hide/Get 警告提示），**注册、显示、关闭、获取请统一使用同一类型**（面板内 `HideSelf()` 始终使用实际类型，安全）。`OnClose` 仅在受控销毁路径（`HidePanel` 且 `DestroyOnHide=true`）调用；**场景卸载、外部 `Destroy` 等非受控销毁只触发 `OnDestroy`**，事件解绑与订阅释放请放在 `OnDestroy`（或两处都写），仅写在 `OnClose` 会在场景切换时泄漏。

### 目录结构

```
Runtime/UI/                        # 汇入核心运行时程序集（层根锚点）
├── UIModule.cs                    # UI 管理器单例
├── UIRoot.cs                      # UI 根节点（四层 Canvas 构建）
├── IUIPanel.cs                    # 面板契约
├── AesirBasePanel.cs              # 面板基类
├── AesirBasePanelView.cs          # MVP 面板视图基类（绑定 Context）
├── AesirBasePanelViewController.cs # MVC 面板控制器基类（绑定 Context + Command/Query 能力）
├── UILayer.cs                     # 层级枚举
├── UICanvasConfigSO.cs            # Canvas 配置资产
├── UIAssetLoader/                 # IUIAssetLoader + ResourcesUILoader
├── InputSystem/                   # Input System 输入模块适配（独立可选程序集）
└── OdinInspector/Binder/          # Binder 全家桶（经 asmref 汇入 Odin 程序集）
Editor/UI/                         # 汇入核心编辑器程序集（层根锚点）
├── UIModuleMenuItems.cs           # Create UIRoot / Default UICanvasConfig 菜单项
└── OdinInspector/                 # Odin AttributeProcessors（经 asmref 汇入 Odin 编辑器程序集）
```

> **设计边界**：
> - **层级体系是封闭集** — `UILayer` 固定四层（Background / Normal / Popup / Top），层序基准硬编码为 100 / 200 / 300 / 400 且每次 Awake 强制覆盖。新增层级或调整层序需修改框架源码，不提供配置位。四层对教学与中小项目已足够。
> - **面板根节点直接挂层 Canvas 下（无独立 Canvas）** — 同层多面板共享层 Canvas：同图集合批友好，代价是同层穿插打断 batch、无 per-panel Canvas / 独立 sortingOrder 配置位。需要独立 Canvas（动画隔离、渲染特效）请在面板预制体内自行添加子 Canvas（不归 UICanvasConfigSO 统一配置管理）。同层内渲染顺序仅由 Show 顺序（`SetAsLastSibling`）决定。
> - **加载契约为同步语义** — 见上方"加载契约"说明；不做 async 接口。
> - **`OnClose` 仅受控销毁路径调用** — 见上方生命周期细节；非受控销毁（场景卸载 / 外部 Destroy）只触发 `OnDestroy`。
> - **主相机需自行排除 UI 层** — UICamera 的 cullingMask 只含 UI(5) 与 TransparentFX(1) 层，但主游戏相机若也包含 UI 层会重复渲染，请在项目相机配置中自行排除。

详细文档见 [Documentation/ui-module.md](./Documentation/ui-module.md)。

## 事件模块

> ⚠️ **实验性模块**：尚未在实际项目中验证，API 可能调整。

基于双轨订阅的事件系统。Attribute 订阅通过 `[AesirListener]` 特性标记方法，Script 订阅通过 `AddListener<T>` 动态注册 Lambda 委托。两种订阅共存于同一分发流程，按 4 档优先级排序执行。

分发期内置订阅者过滤器（精确投递：Tag / 优先级 / 同场景 / Collider2D 范围 / 同家族）、死引用自动清理与可选的耗时告警；支持 SO 资产化（`AesirEventArgsSO`）与 UnityEvent 桥接组件，非程序员可在 Inspector 配置事件。

### 核心类型

| 类型 | 说明 |
|------|------|
| `AesirEventArgs` | 事件参数抽象基类。所有自定义事件参数继承此类，作为数据载体在 EventModule 中传递；支持 `WithFilter` 链式声明投递过滤器 |
| `AesirListenerAttribute` | 方法特性，标记该方法监听指定事件参数类型（AllowMultiple：一个方法可监听多种事件） |
| `EventModule` | MonoBehaviour 单例，管理双注册表与事件分发；分发期自动清理已销毁订阅者，可选 `executionMsLimit` 耗时告警 |
| `BindingInfo` | 绑定信息基类；`StaticBindingInfo` 持有 MethodInfo + 表达式树编译委托；`DynamicBindingInfo<T>` 持有 `Action<T>` 直接委托 |
| `ISubscriberFilter` | 订阅者过滤器策略接口；内建 `WithTag` / `WithPriority` / `SameSceneAsEmitter` / `OnlySelf` / `InsideCollider2D` |
| `SubscriberPriority` | 订阅优先级枚举（4 档：First/High/Medium/Last） |
| `AesirEventArgsSO` | 事件参数的 ScriptableObject 包装，事件可保存为 .asset 资源并在 Inspector 配置/触发 |
| `UnityEventOnAesirEvent` | UnityEvent 桥接组件，非程序员在 Inspector 串联事件回调 |
| `SubclassSelector` | `[SerializeReference]` 字段的子类下拉 PropertyDrawer（UI Toolkit） |
| `AesirEventUtility` | 事件模块静态工具方法 |

### 快速开始

1. 定义事件参数：

```csharp
using Runestone.AesirModules;

public class OnPlayerScored : AesirEventArgs
{
    public int points;
    public string playerName;
}
```

2. Attribute 订阅（静态绑定）：

```csharp
using UnityEngine;
using Runestone.AesirModules;

public class ScoreUI : MonoBehaviour
{
    void OnEnable()  => EventModule.AddListener(this);
    void OnDisable() => EventModule.RemoveListener(this);

    [AesirListener]
    private void OnPlayerScored(OnPlayerScored e)
    {
        Debug.Log($"[ScoreUI] {e.playerName} scored {e.points}");
    }
}
```

3. Script 订阅（动态绑定）：

```csharp
using UnityEngine;
using Runestone.AesirModules;

public class ScoreController : MonoBehaviour
{
    AutoRemoveListenerHandle _handle;

    void OnEnable() =>
        _handle = EventModule.AddListener<OnPlayerScored>(this, e =>
            Debug.Log($"Score: {e.points}"));

    void OnDisable() => _handle.Dispose();
}
```

4. 发布事件：

```csharp
new OnPlayerScored { points = 10, playerName = "Player1" }.Invoke(this);
```

5. 订阅者过滤器（精确投递，发布时链式声明）：

```csharp
// 只有 Tag 为 "Enemy" 的订阅者收到
new OnExplosion().WithFilter(new WithTag("Enemy")).Invoke(this);

// 组合过滤：爆炸半径内的敌人
new OnExplosion()
    .WithFilter(new InsideCollider2D())
    .WithFilter(new WithTag("Enemy"))
    .Invoke(this);
```

内建过滤器：`WithTag` / `WithPriority` / `SameSceneAsEmitter`（多场景叠加加载）/ `OnlySelf`（自身/子树/父级链）/ `InsideCollider2D`（空间局域广播）。实现 `ISubscriberFilter` 即可自定义。

6. SO 资产化（非程序员在 Inspector 配置事件）：

- Project 右键 `Create → Aesir → Event Module → AesirEventArgsSO` 创建事件资产，经 `SubclassSelector` 下拉选择参数子类并配置载荷；运行时在 Inspector 点击「触发事件（Raise）」或代码调用 `asset.Raise()`
- 挂载 `UnityEventOnAesirEvent` 组件选择监听的事件类型，在 On Raised 中绑定 UnityEvent 回调

### API 速查

```csharp
// Attribute 订阅 / 退订
EventModule.AddListener(this);    // OnEnable 中调用
EventModule.RemoveListener(this); // OnDisable 中调用

// Script 订阅（返回 AutoRemoveListenerHandle）
var handle = EventModule.AddListener<MyEventArgs>(this, e => { ... });
handle.Dispose();  // 退订

// 指定优先级
EventModule.AddListener<MyEventArgs>(this, e => { ... }, SubscriberPriority.First);

// 发布
new MyEventArgs().Invoke(this);              // 链式调用
EventModule.InvokeEvent(sender, eventArgs);   // 直接调用

// 订阅者过滤器（链式叠加，全部通过才投递）
new MyEventArgs().WithFilter(new WithTag("Enemy")).Invoke(this);

// SO 资产触发（发布者 = SO 资产本身）
myEventArgsSO.Raise();

// 零参数方法订阅（需显式指定事件参数类型）
[AesirListener(typeof(OnKeyPressed))]
private void OnKeyPressed() { ... }
```

> **设计边界**：分发为同步非重入设计（约定不在订阅者回调内同步发布事件）；遗漏退订的已销毁订阅者由分发期死引用清理自动兜底；系统事件（元事件）与频道标签依赖编辑器工具链，待工具链立项后一并设计。详见 [Documentation/event-module.md](./Documentation/event-module.md)。

### 目录结构

```
Runtime/Events/                    # 汇入核心运行时程序集（层根锚点）
├── AesirEventArgs.cs              # 事件参数基类（Sender + WithFilter 链式 API）
├── AesirEventArgsSO.cs            # SO 资产包装（CreateAssetMenu + Raise）
├── AesirListenerAttribute.cs      # 订阅者特性（AllowMultiple）
├── AesirEventUtility.cs           # 静态工具
├── BindingInfo.cs                 # 绑定信息基类 + StaticBindingInfo + DynamicBindingInfo<T>
├── ISubscriberFilter.cs           # 过滤器策略接口
├── SubscriberFilters.cs          # 内建过滤器（WithTag/WithPriority/SameSceneAsEmitter/OnlySelf/InsideCollider2D）
├── SubclassSelectorAttribute.cs   # [SerializeReference] 子类下拉特性
├── Component/
│   ├── EventModule.cs             # 事件模块单例（死引用清理 + 过滤器检查 + 性能监控）
│   └── UnityEventOnAesirEvent.cs  # UnityEvent 桥接组件
└── SubscriberPriority.cs          # 优先级枚举（4 档）
```

详细文档见 [Documentation/event-module.md](./Documentation/event-module.md)。

## 音频模块

2D 音频极简门面（`AudioModule` 单例，公开 API 全为静态成员，调用即用）：

- **SFX** — 固定数量独占音源轮询（默认 8，可配）：无每播实例化开销，每次播放的局部音量/音调独立生效，源全忙时抢占最旧；`pitchJitter` 音调随机抖动防止机械感
- **BGM** — 专用循环音源；同曲在播幂等返回（跨场景重复触发不打断音乐）；切换支持协程淡入淡出（`unscaledDeltaTime`，slow motion 不变调）
- **音量与静音** — Master / BGM / SFX 三通道乘法链 + 三通道静音（Master 总闸），设置即时生效并经 PlayerPrefs 持久化（重启自动恢复，键前缀可配）
- **暂停** — `PauseAll` / `ResumeAll` 一对，适合暂停菜单与切后台
- **配置** — 零配置可用；可选 `AudioConfigSO` 管默认音量、持久化开关与 PlayerPrefs 键前缀

```csharp
using Runestone.AesirModules;

// SFX：一击即走，音调 ±0.1 抖动
AudioModule.PlaySfx(clickClip, pitchJitter: 0.1f);

// BGM：立即播放；同曲在播时幂等返回
AudioModule.PlayBgm(bgmClip);

// 切歌：淡出旧曲 1.5 秒 → 淡入新曲 1.5 秒
AudioModule.PlayBgm(sceneB, fadeSeconds: 1.5f);

// 音量：设置即生效、即持久化
AudioModule.SfxVolume = 0.5f;
AudioModule.MasterMute = true;
```

> **设计边界**：仅负责 2D 音频——3D 空间音效用原生 `AudioSource.PlayClipAtPoint`；不集成 AudioMixer；`PlaySfx` 为 fire-and-forget，无每音效停止与播完回调（回调需求用 MiniEvent）。详见 [Documentation/audio-module.md](./Documentation/audio-module.md)。

### 目录结构

```
Runtime/Audio/                    # 汇入核心运行时程序集（层根锚点）
├── AudioModule.cs                # 音频管理器单例（SFX 轮询 / BGM 淡入淡出 / 音量持久化）
└── AudioConfigSO.cs              # 配置资产（默认音量 / 持久化开关 / PlayerPrefs 键前缀）
Editor/Audio/                     # 汇入核心编辑器程序集（层根锚点）
├── AudioModuleMenuItems.cs       # GameObject 预放置菜单
└── OdinInspector/                # AudioModule Processor（经 asmref 汇入 Odin 编辑器程序集）
```

## 场景模块

`SceneModule`（MonoBehaviour 单例）负责场景加载、叠加追踪与卸载回收，语义对齐 Unity 原生 LoadSceneMode：

### 核心类型

| 类型 | 说明 |
|------|------|
| `SceneModule` | 场景管理单例：Single/Additive 加载（完成/失败/进度回调）、卸载、重载、激活场景切换、`SceneLoadedEvent` / `SceneUnloadedEvent` 场景事件广播、DDOL 序列化配置 |
| `SceneAssetWrapper` | 可序列化场景引用：GUID 锚点自愈（移动/重命名免疫、断链自恢复）、状态机校验（`State` / `UnsafeReason`）、`TryGet` 安全读取家族；安装 Addressables 时自动扩展地址查询能力。Inspector 面板效果（拖拽/着色/一键修复）需 Odin Inspector，未安装时仅保证 API 可用。功能设计参考 [Eflatun.SceneReference](https://github.com/starikcetin/Eflatun.SceneReference) |
| `SceneAssetWrapperState` / `SceneAssetWrapperUnsafeReason` | 引用状态（Regular/Addressable/Unsafe）与不安全原因枚举 |
| `SceneAssetWrapperAddressablesBridge` | Addressables 编辑器能力静态桥（核心程序集零 Addressables 依赖，未装包自动隐藏） |
| `SceneAssetWrapperException` 异常族 | 空引用 / 创建失败 / 未装包 / 不可寻址四类专用异常，消息均带"修复 / 规避"双指引 |

`SceneModule` 主要 API：

```csharp
// 加载（path 与 SceneAssetWrapper 双重重载；onProgress 为逐帧 0-1 进度，
// 已按 Unity 场景激活上限 0.9 归一化，进度条可平滑走到 100%）
SceneModule.Instance.LoadSceneSingle(scenePath,
    onCompleted: () => { },
    onFailed:    () => { },
    onProgress:  p => { });
SceneModule.Instance.LoadSceneAdditive(scenePath);

// 卸载（经本模块叠加加载的场景自动移出追踪；批量卸载单个失败跳过并告警）
SceneModule.Instance.UnloadScene(scenePath);
SceneModule.Instance.UnloadAllAddedScenes();

// 激活场景切换（多场景叠加工作流：决定光照设置来源与 Instantiate 默认落点）
SceneModule.Instance.SetActiveScene(scenePath);

// 重载当前激活场景（异步 Single 语义）
SceneModule.Instance.ReloadScene();

// 场景生命周期广播（MiniEvent，参数为场景路径；AddListener 返回句柄自动清理）
SceneModule.Instance.SceneLoadedEvent.AddListener(path => Debug.Log($"已加载 {path}"));
SceneModule.Instance.SceneUnloadedEvent.AddListener(path => Debug.Log($"已卸载 {path}"));

// 查询：AddedScenePaths（叠加追踪）/ LastLoadedScene / BootstrapSceneAssetWrapper
```

### 快速开始

1. 预放置（推荐）：把 `SceneModule` 挂到启动场景物体上（或运行时直接 `SceneModule.Instance` 自动创建于 `[Aesir Modules]` 宿主下）。预放置为根物体时受 `dontDestroyOnLoad` 字段（默认开）控制 DDOL——**保持开启**，Single 加载会卸载所有旧场景，关闭 DDOL 的实例将随场景销毁并中断加载回调。

2. 用 `SceneAssetWrapper` 声明场景引用并在 Inspector 拖拽赋值（需 Odin）：

```csharp
using Runestone.AesirModules;
using UnityEngine;

public class LevelFlow : MonoBehaviour
{
    [SerializeField] SceneAssetWrapper gameplayScene; // Inspector 拖拽（需 Odin）

    void Start()
    {
        // 引用无效（空/不在 BuildSettings）或 Addressable 场景走 onFailed，不抛异常
        SceneModule.Instance.LoadSceneSingle(gameplayScene,
            onCompleted: () => Debug.Log("进入关卡"),
            onProgress: p => Debug.Log($"加载中 {p:P0}"));

        // 无 Odin 环境：代码构造引用同样可用
        // var sceneRef = SceneAssetWrapper.FromScenePath("Assets/Scenes/Gameplay.unity");
    }
}
```

3. 编辑器校验：wrapper 字段自带三态着色（Addressable 青 / 悬空与缺 Build 红 / 禁用黄 / 正常白）与一键修复按钮（添加到 BuildSettings / 启用 / 加入 Addressables 默认组）。

### 设计边界

- **Odin Inspector 边界** — `SceneAssetWrapper` 的 Inspector 面板效果依赖 Odin（经 AttributeProcessor 注入）；未安装 Odin 时仅保证 API 可用：`SceneAssetWrapper.FromScenePath(...)` 构造、编辑器下 `SceneAsset` 属性代码赋值、TryGet 家族读取，面板不支持。
- **Addressable 场景不经 SceneModule 加载** — 安装 Addressables 后 wrapper 提供地址（`Address` / `TryGetAddress`），加载请直接调用 `Addressables.LoadSceneAsync(wrapper.Address)`，卸载同理走 Addressables API。
- **重复叠加同一路径后果自负** — Unity 会加载两个场景实例而追踪列表按路径只记一条，`UnloadScene` 按路径只卸载其一，剩余实例脱离追踪；请勿对同一路径重复 `LoadSceneAdditive`。
- **启动场景（Bootstrap）分工** — 运行时 `SceneModule` 只持有 `bootstrapScene` 引用供用户代码读取（`BootstrapSceneAssetWrapper`），不做自动流转；BuildSettings 序号 0 与进 Play 强制打开 Bootstrap 场景由编辑器 `BootstrapSceneHelper` 负责（`Tools → Aesir → Scene Editor Settings` 中开启，默认关闭）。
- **不做场景间传参 / async 化** — 跨场景传数据用框架 MiniEvent 或共享 Model；async 支持待框架统一裁决。

### 目录结构

```
Runtime/Scene/                     # 汇入核心运行时程序集（层根锚点）
├── SceneModule.cs                 # 场景管理单例（加载 / 卸载 / 重载 / 激活切换 / 事件 / DDOL）
├── SceneAssetWrapper.cs           # 可序列化场景引用（GUID 锚点 + 状态机）
├── SceneAssetWrapperState.cs      # 引用状态机
├── SceneAssetWrapperUnsafeReason.cs
├── SceneAssetWrapperAddressablesBridge.cs  # Addressables 能力静态桥
└── Exceptions/                    # 专用异常族
Editor/Scene/                      # 汇入核心编辑器程序集（层根锚点）
├── SceneManagerWindow.cs          # Scene Editor Settings 设置窗口（Tools/Aesir/Scene Editor Settings）
├── BootstrapSceneHelper.cs        # Bootstrapper 场景搜集注册工具（默认关闭）
├── SceneEditorSettings.cs         # 编辑器持久化设置
├── Tests/                         # EditMode 测试（SceneAssetWrapper 27 用例 + SceneModule 20 用例）
├── OdinInspector/                 # SceneAssetWrapper Processor（经 asmref 汇入）
└── Addressables/                  # Addressables 胶水实现（经 asmref 汇入）
```

## Binder 组件绑定（Odin 可选）

位于 `Runtime/UI/OdinInspector/Binder/`（经 asmref 汇入 Odin 程序集，需 Odin Inspector）。Binder 全部收录于 Odin 程序集（含 `[BinderBaseType]`）：其类型选择器（组件 / 基类的 ValueDropdown）强依赖 Odin Inspector。

- `BinderTag` 挂在需要绑定引用的子物体上做标记（默认绑定 1 个组件），用「绑定组件数量」声明要绑定的组件个数；层级右键菜单 `GameObject/Aesir/` 可为选中物体一键挂载 `BinderAssistant` / `BinderTag`；
- `BinderAssistant` 挂在根面板上，「构建绑定单元」按标记增量维护绑定列表（每条记录组件类型、字段名、绑定路径），支持两种生成模式（默认「同一脚本增量」）：「同一脚本增量」只替换目标 `*.cs` 内「绑定字段（自动生成）」region 的内容（字段 + `BindComponents` 方法，类型与特性全限定、自包含），region 外内容归开发者所有，文件不存在时自动创建脚手架；「Partial 分部类」产出手写 partial `*.cs`（仅生成一次）与自动维护文件（后缀可选，默认 `.designer.cs`——Rider 中该后缀默认折叠，Rider 用户推荐）；生成脚本的绑定字段以 `TitleGroup`（「绑定字段（自动生成）」）分组标注自动生成；两种模式编译完成后都会自动挂载组件并执行一次绑定；
- 生成脚本的基类可下拉选择：内置 `MonoBehaviour`、由 Binder 预选的 Aesir 面板家族（`AesirBasePanel`、`AesirBasePanelView<T>`、`AesirBasePanelViewController<T>`——核心程序集无法反向引用 Odin 程序集标注特性，故由 Binder 经 typeof 内置），以及用户以 `[BinderBaseType]` 标记的类（需引用 `Runestone.AesirModules.OdinInspector`）；选择 Aesir 泛型面板基类后在「Context 类型」下拉中选择项目内 AbstractContext 派生类（占位不会写进生成代码）；
- 命名空间默认值与 partial 后缀候选列表经 ScriptableSingleton 在编辑器阶段持久化；
- 生成逻辑为纯文本拼装，配套 EditMode 测试程序集 `Runestone.AesirModules.Tests`（包根 `Tests/`）；`IComponentBinder` 保留为自定义绑定器扩展点。

## 脚本文档生成模块（需 Odin）

位于 `Runtime/ScriptDocGenerator/OdinInspector/` 与 `Editor/ScriptDocGenerator/OdinInspector/`（经 asmref 汇入 Odin 程序集，**强依赖 Odin Inspector**，未安装时自动排除）。命名空间 `Runestone.AesirModules.ScriptDocGenerator`（.Editor）。

- **Script Doc Generator** — 反射分析 C# 类型信息生成结构化 API 文档：全离线、单类型毫秒级、增量生成（保留 `## Additional Notes` 之后的手写内容与 Front Matter）、Markdown 输出可直接用于 AI 知识库；参数/返回值/备注/类型参数说明列全链路输出（Zensical 生成器）；支持自定义输出路径（默认项目根 `ScriptDocGenerator/`，Assets 外无 .meta）/ 命名空间子目录 / 扩展名 / 单类型·多类型·单程序集·多程序集四种来源粒度（程序集下拉仅列脚本程序集），可经 `DocGeneratorSettingsSO`、`IAnalysisDataFactory`、`IAttributeFilter` 扩展。入口 `Tools → Aesir → Script Doc Generator`。
- **Summary 工具** — Project 窗口右键（`Assets → Script Doc Generator → Process Summary`）在 XML `<summary>` 注释与 `[Summary]` 特性之间同步（特性为权威内容源，特性优先、XML 回退）：Sync（双向对齐保留双份）/ Replace（收敛为单份特性）/ Remove（移除特性，带确认）三种模式、批量单次刷新、引号转义、行尾保持、宏定义感知、自动补 `using`。
- **自定义特性** — `[Summary]`（运行时可经 `GetSummary()` 读取）、`[ReferenceLinkURL]`（为类型附加文档链接）。

详细文档见 [Documentation/script-doc-generator.md](./Documentation/script-doc-generator.md)。

## 示例

- 本仓库直接浏览 / 下载源码：示例位于包内 `Samples/` 文件夹，可直接查看运行。
- Git URL 安装：Package Manager → 选中本包 → `Samples` 标签页按需导入（源在包内 `Samples~/` 隐藏目录，构建时自动剔除）。
- unitypackage 导入：示例随包内含，导入后即可运行。

当前提供：

| 示例 | 说明 |
|------|------|
| `Events/01_KeyPress` | 事件模块基本发布-订阅示例：按键发布事件、`[AesirListener]` 静态订阅 |
| `Events/02_Filters` | 订阅者过滤器示例：Space 发布 `WithTag`+`InsideCollider2D` 双重过滤警报、R 发布 `OnlySelf` 家族命令，场景内置圈内/圈外/无标签、家族/无关多组对照，直观呈现精确投递 |
| `Events/03_SOAsset` | SO 资产化示例：`ScoreEventAsset.asset` 配置事件载荷（SubclassSelector 下拉选型），`UnityEventOnAesirEvent` 桥接组件在 Inspector 零代码串联 UnityEvent 回调 |
| `Audio/01_BasicUsage` | 音频模块基础用法示例：SFX 播放（含音调抖动）、BGM 淡入淡出切歌、三通道音量与静音持久化 |

## 许可证

MIT

