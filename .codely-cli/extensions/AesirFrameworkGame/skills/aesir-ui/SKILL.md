---
name: aesir-ui
description: Aesir Modules UI 框架（UIModule / UIRoot）编码指南。用 Aesir 做 UI 界面时使用——Panel 与 Window 选型、生命周期、蒙版单遮与叠遮、世界空间血条。触发词：UIModule、UIRoot、AesirBasePanel、AesirBaseWindow、WorldSpaceUICanvas、面板、弹窗、蒙版、四层 Canvas、血条、名字牌、加个开始界面/暂停界面/设置面板。
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

组件 `WorldSpaceUICanvas`（Add Component → Aesir Modules → UI）或运行时 `UIRoot.Instance.CreateWorldSpaceCanvas(target, name)`。`Awake` 自动补齐 Canvas(WorldSpace) + RectTransform + CanvasScaler + GraphicRaycaster，子树归 UI 层；`sortingOrder` 默认 **450**（面板 ≤400 与窗口 ≥500 之间）。跟随/偏移/朝向每帧读取即时生效，但**配置与 sortingOrder 属接线期参数——改完要再调 `Build()`**。它不进 UIModule 注册表、不参与蒙版调度。

两种创建方式、配置优先级链与接线期参数清单见 [references/world-space-ui.md](references/world-space-ui.md)。

## 坑位速查（12 条）

命中某条编号就去 [references/ui-pitfalls.md](references/ui-pitfalls.md) 读该条的现象 / 原因 / 修法全文。

| # | 触发 | 后果 |
|---|---|---|
| 1 | 解绑只写在 `OnClose` | 切场景或外部 Destroy 时**订阅泄漏**（只有 OnDestroy 必调） |
| 2 | 手调层 Canvas 的 sortingOrder | **无效**，Awake/Build 强制覆盖回 100/200/300/400 |
| 3 | 注册用基类、预制体挂派生类 | 重复 Show **报错**；Hide/Get 只 Warning 返回 null |
| 4 | 路径当 payload 传 `Show<T>("UI/X")` | 被 payload 吞掉，报"未注册且未提供 path"**不揭示真因** |
| 5 | 主相机没排除 UI 层(5) | UI **重复渲染** |
| 6 | 加载器里同步等 Addressables Handle | WebGL **死锁**（同步契约，须预加载） |
| 7 | 运行时 AddComponent 出的面板 | Binder **不自动绑定**（绑定只在编辑期） |
| 8 | 覆写 OnShow/OnHide 没调 `base` | 数据全对但**永远不可见**，Get 拿得到实例，极难定位 |
| 9 | 以为面板随场景卸载销毁 | DDOL 下**孤儿面板残留**；需主动 Hide/Close + 负向断言 |
| 10 | 组件节点以 `SetActive(false)` 起始 | 不执行 Start，**订阅建立不了、永远不点亮** |
| 11 | URP 工程用 UIRoot 默认相机 | **游戏画面被整屏清掉**（HUD 在、精灵全消失），需改 Overlay 入 cameraStack |
| 12 | 报"未受管理的 XxxPanel" | 先查是否绕过 Bootstrap 直接 LoadScene，**可能是误报** |

## UI 独立场景 + 叠加加载（非默认，需主动配置）

框架 `dontDestroyOnLoad` 默认全为 true（零配置开箱即用）。中大型项目若要 UI 生命周期随场景回收：把 UIModule + UIRoot **预放置为独立 UI 场景根物体并关掉各自 DDOL**，`LoadSceneAdditive` 载入、`UnloadScene` 卸载即整套回收；进程级单例（AesirArchitecture / AesirModules / AudioModule / SceneModule / EventModule）保持默认 true。**运行时自动创建的实例仍跟随宿主 DDOL**——只有预放置为根物体时该字段才生效。

DDOL 分类表与数据留存注意点见 [references/ui-scene-ddol.md](references/ui-scene-ddol.md)。

## 设计边界（不要尝试）

无面板/窗口导航栈与返回、无 SmartShowHide 伪隐藏、无异步加载接口、无 per-panel Canvas（同层多面板共享层 Canvas——需要独立 Canvas/独立排序/蒙版挡输入就升格为 Window）、面板不提供蒙版、世界空间 UI 不进注册表且不参与蒙版调度。层级体系封闭；UI 虚拟列表与滚动复用不做。

## 详情与示例

- 完整实现细节与蒙版/配置两条链：包内 `Documentation~/ui-module.md`（中文）/ 包根 `README_EN.md` 的 UI 章节。
- 可运行示例：`Samples/UI/01_BasicUsage`（Package Manager → Samples 导入），含 Panel 与 Window 协作、蒙版单遮/叠遮切换、点击蒙版关闭、全屏加载窗口。
- 绑定脚本生成（Odin 可选）：`BinderTag` / `BinderAssistant`，菜单在同一程序集内，见包 README 的 Binder 章节。
