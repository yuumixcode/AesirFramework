# Aesir Modules API 速查（UI / Scene / Event / Audio）

> RAM 四个功能模块的 API 白名单与使用要点。签名均为源码实证（0.31.x）。**默认档口径**：demo/快速开发直接照抄本文写法，零配置可用。

## UI 模块（Runestone.AesirModules）

### 两种形态——默认用 Panel

| | **Panel（默认形态）** | Window（进阶形态） |
|---|---|---|
| 预制体根节点 | **不带 Canvas**（RectTransform 挂脚本即可） | 自带 `Canvas + CanvasScaler + GraphicRaycaster` |
| 挂载位置 | UIRoot 预设四层 Canvas 之下（Background=100 / Normal=200 / Popup=300 / Top=400） | 直挂 UIRoot，`sortingOrder` 单一真源（默认 500，恒在面板之上） |
| 适用 | 常驻 HUD、列表、信息面板、背景层 UI | 模态弹窗（设置/暂停/结算）、全屏流转页（加载/开场） |
| API 动词 | `Show` / `Hide` | `Open` / `Close` |

**简单/默认情况一律 Panel：用预设 Canvas，单个 Prefab 不要以 Canvas 作为根节点。** Window 只在"需要蒙版挡输入 / 独立排序 / 渲染隔离"时用。

### 从零显示第一个面板（最少步骤，实测路径）

1. 写一个类继承 `AesirBasePanel`（生命周期虚方法可全部不覆写）。
2. 做预制体：根节点挂该脚本（**根不带 Canvas**），存入 `Assets/Resources/UI/`。
3. 任意运行时代码调用一行：

```csharp
UIModule.Show<MainMenuPanel>(path: "UI/MainMenuPanel");   // 注意必须用 path: 命名参数！
```

该行执行时**自动完成全部装配**：创建 `[Aesir Modules]`（DDOL）→ UIModule → `[UIRoot]`（DDOL）→ UICamera + EventSystem + 四层 Canvas → 停用态克隆预制体 → 挂 Normal 层 → `OnInit` → `OnShow`。**场景可以全空，零预放置零配置。**

### API 白名单

```csharp
// 面板（Panel）
UIModule.Show<T>(object payload = null, string path = null);   UIModule.Hide<T>();
UIModule.Get<T>();                                              UIModule.Prewarm<T>(string path = null);
UIModule.RegisterPrefab<T>(GameObject prefab);

// 窗口（Window）
UIModule.Open<T>(object payload = null, string path = null);    UIModule.Close<T>();
UIModule.GetWindow<T>();                                        UIModule.PrewarmWindow<T>(string path = null);
UIModule.RegisterWindowPrefab<T>(GameObject prefab);           UIModule.ContainWindowPrefab<T>();

// 生命周期可覆写：OnInit / OnShow(payload) / OnHide / OnClose / OnDestroy(必须调 base)
//   AesirBasePanel 序列化字段：UILayer layer（Background/Normal/Popup/Top）、bool destroyOnHide
//   AesirBaseWindow 序列化字段：int sortingOrder（默认 500）、bool destroyOnHide、bool closeOnMaskClick（默认 false）
//   窗口可覆写 OnMaskClicked()；便捷自关 CloseSelf()
```

- `path` 传入后经 `ResourcesUILoader` 加载并**自动注册**，后续同类型直接 `Show<T>()` 不必再给 path。
- 实例键 = 预制体**根组件类型**：`Show<HudPanel>()` 的 `HudPanel` 必须与根节点上的组件类型完全一致。
- **未注册且无 path 不抛异常**：只 `LogError` 并返回 null——所以路径常量要过自检脚本（见 jam-playbook.md 第 5 节）。

### 三个必知陷阱

1. **`Show<T>("UI/Xxx")` 位置参数陷阱**：第一参数是 `object payload`，字符串会被当成 payload 吞掉，随后报"未注册预制体且未提供 path"且**不揭示真实原因**——**必须写 `Show<T>(path: "UI/Xxx")` 命名参数**。
2. **主相机 cullingMask 必须排除 UI 层(5)与 TransparentFX(1)**：UICamera 只渲染 UI，主相机含 UI 层会重复渲染（蒙版加深）。UIRoot 的相机已自动配好，只需管你自己的主相机。
3. **`Get<T>()` / `ContainPrefab<T>()` 等查询静态 API 有懒创建副作用**：纯查询也会自动创建 `[Aesir Modules]` 宿主与 UIModule。编辑器期探测"面板是否在场"别用它（用 `FindObjectsByType` 或场景扫描）。

### 进阶自定义（可选，不碰就不存在）

- 替换资源加载器：`UIModule.RegisterAssetLoader(IUIAssetLoader)`（同步契约——Addressables 需预加载后同步返回）。
- 蒙版模式（窗口）：`UIModule.MaskMode`（Single 单遮 / Stacked 叠遮，运行时切换即重算）；配置在 `UIModuleConfigSO`（Resources 兜底 + `RegisterConfigLoader` 可替换）。
- 面板层级 `layer` 与窗口 `sortingOrder` 在各自脚本序列化字段上设置。

## Scene 模块（Runestone.AesirModules）

### 一行切换场景（最简方式，实测路径）

```csharp
// 主菜单 → 玩法场景，一行：
SceneModule.LoadSceneSingle("Assets/Scenes/Gameplay.unity",
    onCompleted: () => Debug.Log("玩法场景已加载"),
    onFailed: e => Debug.LogError($"切换失败：{e}"));
```

前置与事实（全部经源码核查）：

- **零预放置零配置**：首次调用自动创建 SceneModule 于 `[Aesir Modules]`（DDOL）宿主下；`SceneModuleConfigSO` 自动创建且默认值即可用（progressCap=0.9）；bootstrap 体系与普通切换无关。
- **唯一硬前置**：目标场景必须登记进 Build Settings（Unity 硬约束）——启动场景用 `AesirBootScene-Skill` 的 BootSceneBuilder 登记 index 0；玩法场景在 Build Settings 面板拖入，或用 Odin 环境下 `SceneAssetWrapper` 的一键按钮。
- **传完整资产路径**（含 `.unity`）：传裸名虽然 `LoadSceneAsync` 能加载，但模块记账/激活/事件全走样（伪错误日志 + LastLoadedScene 失效）——**永远传 `Assets/.../Xxx.unity` 完整路径**。
- **禁止混用原生 `SceneManager.LoadScene/UnloadSceneAsync`**：绕过模块记账，`AddedScenePaths` / `LastLoadedScene` 会失真。

### API 白名单

```csharp
SceneModule.LoadSceneSingle(string path, Action onCompleted = null, Action onFailed = null, Action<float> onProgress = null);
SceneModule.LoadSceneAdditive(string path, Action onCompleted = null, Action onFailed = null, Action<float> onProgress = null);
SceneModule.UnloadScene(string path, Action onUnloaded = null, Action onFailed = null);
SceneModule.UnloadAllAddedScenes(Action onAllUnloaded = null, Action onFailed = null);
SceneModule.ReloadScene(Action onCompleted = null, Action onFailed = null);   // 重开（先 GameContext.Instance.Dispose()，见 jam-playbook）
SceneModule.SetActiveScene(string path);        // 多场景工作流：指定激活场景（光照/新增物体归属）
SceneModule.LastLoadedScene;                    // 最近一次 Single/Reload 加载的场景路径
SceneModule.AddedScenePaths;                   // 叠加加载追踪中的路径列表
SceneModule.SceneLoadedEvent / SceneUnloadedEvent;   // MiniEvent<string>，参数 = 场景路径
```

- 进度回调已按 0.9 上限归一化（平滑走到 1.0）：`onProgress: p => slider.value = p`。
- 加载/卸载事件先广播、后回调 `onCompleted`——事件监听者抛异常会中断后续（fail-fast），监听回调不应抛异常。

### SceneAssetWrapper（可选的场景资源封装）

对标 Eflatun.SceneReference 的 `[SerializeField]` 场景引用：GUID 锚点自愈（资产移动/改名后自动回写路径）、Build Settings 登记状态着色、Addressables 缓存。**demo 场景不需要它**——`LoadSceneSingle` 直传路径字符串就是最简形态；wrapper 只在"想要 Inspector 拖拽场景资产 + 状态可视化"时引入（其完整 Inspector 体验依赖 Odin；无 Odin 时四字段不可见，纯代码层仍可用）。

## Event 模块（Runestone.AesirModules）——受限使用

事件模块只用于**跨系统、发送方不该知道接收方**的场合（成就/播报/全局音效）。**不要**用它替代 Model 状态通知（状态一律 `ObservableValue`）。

```csharp
// 发布：事件载荷 = AesirEventArgs 子类，命名 XxxEventArgs（仅作数据载体）
public sealed class EnemyKilledEventArgs : AesirEventArgs { public int Score; }

new EnemyKilledEventArgs { Score = 10 }.WithFilter(new WithTag("Enemy")).Invoke(this);

// 订阅：属性式声明 + 配对注册（OnEnable/OnDisable 必须对称）
[AesirListener] void OnEnemyKilled(EnemyKilledEventArgs e) { ... }

void OnEnable()  => EventModule.AddListener(this);      // 绑定本对象全部 [AesirListener]
void OnDisable() => EventModule.RemoveListener(this);   // 必须配对
```

内建过滤器：`WithTag` / `WithPriority` / `SameSceneAsEmitter` / `OnlySelf` / `InsideCollider2D`（全部 fail-closed）。`[AesirListener]` 方法只做转发入口，调到 Controller，不做重逻辑。

## Audio 模块（Runestone.AesirModules）

全静态门面，首次调用自动创建宿主。

```csharp
AudioModule.PlaySfx(AudioClip clip, float volume = 1f, float pitch = 1f, float pitchJitter = 0f);
AudioModule.PlayBgm(AudioClip clip, float fadeSeconds = 0f);   // 独立 loop 源 + 淡入淡出
AudioModule.StopBgm(float fadeSeconds = 0f);
AudioModule.PauseAll(); / AudioModule.ResumeAll();            // 暂停/恢复（配 GameFlowDriver）

AudioModule.MasterVolume / BgmVolume / SfxVolume;             // 三通道音量（PlayerPrefs 持久化）
AudioModule.MasterMute / BgmMute / SfxMute;
AudioModule.IsBgmPlaying; / AudioModule.CurrentBgm;
```

SFX 用 N 个独占 AudioSource 轮询（默认 8，每播音调/音量独立）；`pitchJitter` 做音效抖动。设计边界：不做 3D（原生 `Audio.PlayClipAtPoint` 替代）、不做 AudioMixer、不做每音效 Stop。
