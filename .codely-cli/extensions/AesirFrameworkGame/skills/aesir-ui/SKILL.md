---
name: aesir-ui
description: Aesir Modules 的 UI 框架（UIModule / UIRoot）编码指南。为 Unity/团结引擎项目做界面时使用——Panel（四层 Canvas 下的常驻界面）与 Window（自带 Canvas 的模态弹窗/全屏页）的选型与写法、面板生命周期 OnInit/OnShow/OnHide/OnClose、蒙版单遮与叠遮、世界空间 UI Canvas（血条/名字牌）、可替换资源加载器。触发场景：用户提到 UIModule、UIRoot、AesirBasePanel、AesirBaseWindow、AesirBasePanelView、面板、弹窗、蒙版、四层 Canvas、血条、名字牌、WorldSpaceUICanvas，或要求"用 Aesir 做 UI 界面"、"加个开始界面/暂停界面/设置面板"、"UI 框架怎么用"。
---

# Aesir UI 模块（UIModule + UIRoot）

RAM 的界面框架：**单门面 + 多注册表**——`UIModule` 管生命周期，`UIRoot` 管四层 Canvas 层级。两种并列形态：**Panel**（默认，共享层 Canvas）与 **Window**（自带 Canvas，模态/全屏）。第三种形态 **WorldSpaceUICanvas** 只做世界空间渲染根，不进注册表。

> 前置：本包依赖 Aesir Architecture（RAA）。Context / Model / Controller 的组织方式默认按 **Quick 档**，见 `aesir-framework` Skill；本 Skill 只管 UI 本身。
>
> **缺包先过依赖门禁**：工程里没有 `cn.runestone.aesir.modules`（或连带缺 RAA）时，不要改用原生 Canvas + 自建面板管理器顶替——先按 `aesir-router` 的「依赖门禁」探测 → 读记忆 → 询问安装（Git URL，CNB 优先、GitHub 兜底）。

## 选型：先定形态，再写脚本

| 你要做的界面 | 选什么 |
|---|---|
| 常驻 HUD、血条栏、背包/列表、非模态并存的信息面板 | **Panel**（项目默认主形态，同图集合批友好） |
| 模态弹窗、设置/暂停/结算/加载全屏页、需要独立 sortingOrder 或挡输入 | **Window** |
| 头顶血条、名字牌、交互提示，随世界物体移动 | **WorldSpaceUICanvas**（无 Show/Hide 生命周期） |

一个项目通常只选一种主形态；两者 API 与生命周期对称，混用成立（窗口恒在面板之上）。

## Panel：默认路径

```csharp
// 1. 面板脚本：根节点挂载，继承 AesirBasePanel
public sealed class HudPanel : AesirBasePanel
{
    protected override void OnInit()   { /* 初始化引用（取代 Awake） */ }
    protected override void OnShow(object payload) { /* 每次显示 */ }
    protected override void OnHide()   { }
    protected override void OnDestroy() { /* 订阅解绑放这里 */ }
}

// 2. 显示 / 隐藏（静态门面是主入口）
UIModule.Show<HudPanel>(payload);
UIModule.Hide<HudPanel>();                 // DestroyOnHide 决定销毁还是缓存复用
UIModule.Get<HudPanel>();                  // 纯查询：无模块实例时返回 null，不创建宿主
UIModule.Show<ShopPanel>("UI/ShopPanel");  // 单参路径重载：加载后自动注册，之后可省略 path
```

生命周期实际时序（首次 Show）：

```
ResolvePrefab → InstantiateInactive（不触发 Awake/OnEnable）
→ 挂到所属层 Canvas → 子树递归归入 UI 层(5)
→ OnInit（组件仍停用）→ OnShow → SetActive(true)（此刻才 Awake/OnEnable）
```

因此 **`OnInit` 取代了 Awake 的初始化角色**：需要在 OnEnable 里安全访问的引用，都应在 `OnInit` 里就绪。

- 层级：`UILayer` 四层固定（Background 100 / Normal 200 / Popup 300 / Top 400），层序硬编码，**加层要改框架源码**。
- 面板基类家族：`AesirBasePanel`、`AesirBasePanelView<T>`（MVP）、`AesirBasePanelViewController<T>`（MVC）。
- 注册表以**实例实际类型**为键：注册 `<Base>` 但预制体挂 `<Derived>` 时，用基类再 Show 会被报错拒绝，Hide/Get 只警告返回 null。**注册、显示、关闭、获取统一用同一类型**。

## Window：自带 Canvas 的模态/全屏页

```csharp
// 根节点：Canvas + CanvasScaler + GraphicRaycaster + 窗口脚本（类名 = 脚本名 = 预制体名，以 Window 结尾）
// 子物体约定：Mask（全屏 Image，承接点击）/ Content（实际内容，框架不碰）
public sealed class SettingWindow : AesirBaseWindow
{
    protected override void OnShow(object payload) { }
}

UIModule.Open<SettingWindow>(payload);        // 已存在则置顶并重新 OnShow
UIModule.Close<SettingWindow>();
UIModule.GetWindow<SettingWindow>();
UIModule.PrewarmWindow<SettingWindow>("UI/SettingWindow");
```

- `sortingOrder` 由窗口自身 `[SerializeField] int sortingOrder = 500` 声明并应用到根 Canvas，**恒在面板四层（≤400）之上**；同值时后开者居上。
- 挂载时框架统一接线：挂 UIRoot 直下 → 应用 `UICanvasConfigSO`（渲染模式 / CanvasScaler / GraphicRaycaster）→ 接线 UICamera → 应用 sortingOrder → 递归设 UI 层。**根节点缺 Canvas 直接报错中止**，不会留半挂载实例。
- 命名必须 `XxxWindow`，不要 `XxxCanvas`（会被误读为 `UnityEngine.Canvas` 派生类）。
- 蒙版是每个窗口预制体内的 `Mask` 子物体，由 `UIModuleConfigSO.maskMode` 统一调度：**单遮（默认）**只让最高层窗口的蒙版生效，**叠遮**逐层叠加。点击走虚方法 `OnMaskClicked()`，默认按 `closeOnMaskClick`（默认 false）关闭，子类可覆写。
- `UIModuleConfigSO` 需显式创建：在 Project 窗口建 `Assets/Resources/UIModuleConfig/UIModuleConfig.asset`，或点菜单 `Tools/Aesir/Modules/Create Module Config Assets` 一键生成（包导入时不会自动写工程）；也可 `UIModuleConfigSO.RegisterConfigLoader(...)` 走自定义路径。三者都没做时运行时用内存默认配置（单遮），不落盘。
- 注册表、预制体注册表与资源加载器**与面板共用**；Panel 类型误入窗口入口（或反之）会报错并指向正确入口。

## 世界空间 UI（血条 / 名字牌）

```csharp
// 方式一：组件直接挂在目标节点下（FollowTarget 留空，随父级移动）
// Add Component → Aesir Modules → UI → World Space UI Canvas

// 方式二：运行时创建
var popup = UIRoot.Instance.CreateWorldSpaceCanvas(interactable.transform, "InteractHint");
popup.PositionOffset = new Vector3(0f, 2f, 0f);
popup.Billboard = WorldSpaceUICanvas.WorldSpaceBillboardMode.CameraFacingFull; // LateUpdate 每帧读取，立即生效
```

- `Awake` 自动补齐 `Canvas(WorldSpace)` + `RectTransform`（正方形像素网格，避免非等比拉伸）+ `CanvasScaler` + `GraphicRaycaster`，子树归 UI 层。
- `sortingOrder` 默认 **450**（介于面板 ≤400 与窗口 ≥500 之间）。
- 配置来源优先级：组件上显式配置 → `UIRoot.WorldSpaceCanvasConfig` → 内存默认 `UICanvasConfigSO.CreateWorldSpaceDefault()`。世界空间链与屏幕空间链**互不覆盖**。
- 跟随目标 / `PositionOffset` / `FaceCamera` / `Billboard` / `FlipFacing` 由 `LateUpdate` 每帧读取，运行时改动即时生效；**配置与 `sortingOrder` 属接线期参数**——换配置需再调 `Build(overrideConfig)`，`sortingOrder` 为序列化字段（`SortingOrder` 属性无 setter），Inspector 改动后同样要 `Build()` 才刷新 Canvas。

## 坑位（违反即报错或静默失效）

1. **解绑/退订必须放 `OnDestroy`**。`OnClose` 只在受控销毁路径（Hide + `DestroyOnHide=true`）调用；场景卸载或外部 `Destroy` 时**只有 `OnDestroy`**——只写 OnClose 的清理会在切场景时泄漏。
2. **层 Canvas 的 sortingOrder 会被强制覆盖**为 100/200/300/400（每次 Awake / Build 重新应用），手动微调层序无效；窗口的 sortingOrder 不被覆盖。
3. **同一基类类型重复 Show 会报错**；`Hide` / `Get` 未命中只给 Warning。类型要一路用同一个。
4. **路径别当成 payload 传**：`Show<T>("UI/Xxx")` 会命中专门的单参路径重载；要同时带参数就写 `Show<T>(payload, "UI/Xxx")`——否则字符串会被 `object payload` 吞掉、随后报"未注册且未提供 path"而不揭示真因。经**引用注册**的条目带 path 调用会警告"path 被忽略"。
5. **主相机要自行排除 UI 层(5)**：UICamera 只渲染 UI + TransparentFX，主相机含 UI 层会重复渲染。
6. **加载器是同步契约**：`IUIAssetLoader.Load(path)` 无 await。Addressables 直接 `LoadAssetAsync` 的 `Handle.Result` 同步等待在 WebGL 会死锁——先预加载、再经自定义 loader 同步返回缓存。
7. **运行时 `AddComponent` 出来的面板/窗口不做 Binder 自动绑定**（绑定只在编辑期生成脚本 → 编译后挂载时写入 prefab 序列化数据）。
8. **覆写 `OnShow`/`OnHide` 第一行必须调 `base`**。基类默认实现是 `gameObject.SetActive(true/false)`——面板/窗口以**停用状态**实例化，激活动作就藏在 `base.OnShow` 里。只写自己的刷新逻辑而不调 base，实例数据全对但**永远不可见**，且 `Get<T>` 拿得到实例、注册表正常，极难定位。可见性断言要带 `gameObject.activeInHierarchy == true`。
9. **DDOL 下离开场景要主动收尾本场景的面板/窗口**。UIRoot 常驻，挂在层 Canvas 下的面板**不随场景卸载销毁**——"面板交给场景卸载处理"的假设永远不成立。切场景前 `HideSelf()`，并在场景根的 `OnDestroy` 里统一 `Hide`/`Close` 本场景打开的面板窗口。验证必须有**负向断言**（"不该在的不在"），只验"该在的在"抓不到泄漏。
10. **需要订阅/初始化的组件节点不能以 `SetActive(false)` 起始**。禁用节点不执行 `Start`，`AddListenerAndInvoke` 订阅建立不了，后续永远不会点亮（血条/光圈/护盾这类"默认隐藏"组件高发）。用组件级 `spriteRenderer.enabled = false` 控制显隐，节点保持激活。多个驱动源（订阅回调 + 生命周期事件）并存的显隐状态，强制关闭要用带 `_forced` 标志的 `ForceHide()` **压过**所有驱动源，否则下一帧回调又点亮。
11. **URP 工程 UICamera 会清掉全部游戏画面**（HUD 正常、精灵全消失，但运行时数据在动）。UIRoot 的 UICamera 按内置管线语义配成 Base + `clearFlags=Depth`；URP 的多个 Base 相机是**整屏替换**而非叠加，主相机画面被它整个清掉。需在业务侧把 UICamera 改成 Overlay 并加入主相机 `cameraStack`——反射零硬依赖的通用接线组件见 `references/urp-ui-camera-stack.md`。真机"UI 在但画面黑"第一嫌疑就是此接线。
12. **"未受管理的 XxxPanel" 告警先查是否直接 `LoadScene` 跳场景**。验证脚本绕过 Boot→Start→Bootstrap 正常流程直接切场景时，模块实例与面板注册表错位会误报泄漏；编辑模式清点激活实例若为 0 则无残留，不必"修"。实例名已复位为预制体名（无 `(Clone)` 后缀），报警文本里的名字可直接对照预制体。

## 推荐方案：UI 独立场景 + 叠加加载（非默认，需主动配置）

框架的 `dontDestroyOnLoad` **默认全部为 true**，保证零配置开箱即用（UIModule / UIRoot 常驻跨场景）。中大型项目若希望 UI 生命周期随场景回收，采用如下**推荐**结构——这是主动配置，不是框架默认：

| 类别 | 典型类型 | DDOL 设置 | 理由 |
|---|---|---|---|
| 全局物体 | `AesirArchitecture` / `AesirModules` / `AudioModule` / `SceneModule` / `EventModule` | **保持默认 true** | 它们是进程级单例（配置、事件总线、音频、场景调度），本就该跨场景常驻 |
| UI 宿主 | `UIModule` + `UIRoot` | 预放置为**独立 UI 场景根物体**，关掉各自的 `dontDestroyOnLoad` | UI 场景可 `UnloadScene` 卸载；UI 子树随场景一次性回收，不留孤儿面板 |

要点：

- **UI 独立场景**（如 `UIScene.unity`）只放 UIModule / UIRoot 与其层级 Canvas，作为根物体预放置；两个组件的 `dontDestroyOnLoad` 取消勾选。以 `LoadSceneAdditive` 叠加载入，`UnloadScene` 卸载即完成整套 UI 回收。
- **运行时自动创建的实例仍跟随 `[Aesir Modules]` 宿主 DDOL**（此时字段不参与判断）——只有预放置为根物体时该字段才生效。所以"UI 场景可卸载"要求**主动预放置并关字段**，不是改框架默认。
- 面板/窗口实例挂在 UIRoot 层 Canvas 下，随 UI 场景卸载一并销毁；注册表随 UIModule 实例重建而清零，天然规避"旧场景孤儿面板"问题（见坑位 9/12）。卸载 UI 场景前若需保留数据，把状态放在 DDOL 的 Model（`AbstractContext` 纯 C# 单例）里，不放在面板字段。

## 设计边界（不要尝试）

无面板/窗口导航栈与返回、无 SmartShowHide 伪隐藏、无异步加载接口、无 per-panel Canvas（同层多面板共享层 Canvas——需要独立 Canvas/独立排序/蒙版挡输入就升格为 Window）、面板不提供蒙版、世界空间 UI 不进注册表且不参与蒙版调度。层级体系封闭；UI 虚拟列表与滚动复用不做。

## 详情与示例

- 完整实现细节与蒙版/配置两条链：包内 `Documentation~/ui-module.md`（中文）/ 包根 `README_EN.md` 的 UI 章节。
- 可运行示例：`Samples/UI/01_BasicUsage`（Package Manager → Samples 导入），含 Panel 与 Window 协作、蒙版单遮/叠遮切换、点击蒙版关闭、全屏加载窗口。
- 绑定脚本生成（Odin 可选）：`BinderTag` / `BinderAssistant`，菜单在同一程序集内，见包 README 的 Binder 章节。
