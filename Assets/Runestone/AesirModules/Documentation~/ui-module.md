# UI 模块详细文档

UI 框架（Manager of Managers 模式）：`UIModule` 单例管理面板与窗口的生命周期，`UIRoot` 负责四层 Canvas 层级构建。UI 有两种并列形态——**Panel（面板）** 以 `AesirBasePanel` 家族为根节点，挂载于共享层 Canvas；**Window（窗口，Canvas 根 UI）** 以 `AesirBaseWindow` 家族为根节点，预制体自带 Canvas、直接挂载于 UIRoot。本文覆盖快速开始之外的实现细节与设计边界；基础用法见包 [README](../README.md#ui-模块)。

## 架构总览

```
[Aesir Modules]（宿主，DDOL）
└── UIModule                     # 面板/窗口注册表 + 生命周期驱动 + 蒙版调度（单例）
UIRoot（预放置或运行时创建）
├── UICamera                     # 正交、depth=1、cullingMask=UI|TransparentFX
├── EventSystem                  # 自建时挂 StandaloneInputModule 或 InputSystemUIInputModule
├── BackgroundLayer              # Canvas，sortingOrder 100
├── NormalLayer                  # Canvas，sortingOrder 200
├── PopupLayer                   # Canvas，sortingOrder 300
└── TopLayer                     # Canvas，sortingOrder 400
        ↑ 面板实例挂载于所属层的 Canvas 下
SettingWindow / LoadingWindow …  # 窗口实例（根节点自带 Canvas，sortingOrder ≥ 500）
        ↑ 窗口实例直接挂载于 UIRoot 下，恒在全部面板层之上
```

依赖方向为单向拉取：`UIModule.EnsureReady` 在每次面板/窗口操作时经 `UIRoot.Instance` 惰性发现 UI 根节点（未预放置时自动创建全层级）；`UIRoot` 不反向注册 UIModule——这保证编辑器菜单 `Create UIRoot` 只创建层级本身，不会连带触发运行时单例的创建链。

## 面板生命周期

### 驱动顺序

新面板首次 `ShowPanel` 的完整时序：

```
ResolvePrefab → InstantiateInactive（停用克隆，不触发 Awake/OnEnable）
→ 挂载到所属层 Canvas → Initialize（OnInit，此时组件仍停用）
→ Show（OnShow → 默认 SetActive(true) → 此刻 Awake/OnEnable 才触发）
```

该顺序保证：`OnEnable` 可以安全访问 `OnInit` 之后才有值的引用（基类成员、绑定数据等在组件激活前已就绪）。代价是 `OnInit` 语义上**取代**了 Awake 的初始化角色——子类应把传统 Awake 逻辑放进 `OnInit`，覆写 `Awake` 的执行时机会晚于 `OnInit`。

再次 Show（激活或停用面板）不会重走 Initialize，只做置顶（`SetAsLastSibling`）+ 重新 `OnShow(payload)`。

### OnClose 与 OnDestroy 的职责分界

| 路径 | 触发的回调 |
|------|-----------|
| `HidePanel` 且 `DestroyOnHide=true` | `OnHide` → `OnClose` → `Destroy` → `OnDestroy` |
| `HidePanel` 且 `DestroyOnHide=false` | `OnHide`（实例缓存复用） |
| 场景卸载 / 外部 `Destroy(gameObject)` | 仅 `OnDestroy` |

**事件解绑、订阅释放必须放在 `OnDestroy`（或 `OnClose` + `OnDestroy` 两处）**。`OnClose` 只在受控销毁路径调用，仅写 `OnClose` 的清理逻辑会在场景切换时泄漏（MiniEvent / ObservableValue 的订阅没有死引用清理兜底）。

### 注册键语义

面板实例注册表以**实例的实际类型**为键（`uiPanel.GetType()`）。当预制体根节点挂载的脚本恰好是注册类型的派生类时（如 `RegisterPrefab<BasePanel>` 但预制体挂的是 `DerivedPanel`）：

- 再次以基类类型 `ShowPanel`：**报错拒绝**，不会重复实例化；
- 以基类类型 `HidePanel` / `GetPanel`：**警告提示**键语义后静默返回 null；
- 以实际类型操作一切正常。

最佳实践：注册、显示、关闭、获取统一使用同一类型。面板内部 `HideSelf()` 始终以 `GetType()`（实际类型）调用，永远安全。类型不匹配的调用配合 Odin 与否均会得到 Console 诊断日志。

## 窗口（Canvas 根 UI）

与 Panel 并列的第二种 UI 形态：预制体根节点自带 `Canvas + CanvasScaler + GraphicRaycaster`（独立渲染根），`UIModule` 实例化后直接挂载到 UIRoot 下。适合模态弹窗、全屏流转页（设置 / 暂停 / 结算 / 加载）与需要独立 sortingOrder 或渲染隔离的浮层。

### 预制体结构约定

```
XxxWindow（根：Canvas + CanvasScaler + GraphicRaycaster + 窗口脚本；类名 = 脚本名 = 预制体名，以 Window 结尾）
├── Mask        蒙版：Image 全屏拉伸（raycastTarget 拦截其下一切 UI 的射线）+ 可选 Button（承接点击）
└── Content     实际 UI 元素容器（框架不触碰，仅作为结构约定）
```

命名后缀取 `Window` 而非 `Canvas`：按 .NET 命名惯例（派生类以基类名结尾是阅读预期，`ArgumentOutOfRangeException` is a kind of `Exception` 式），`XxxCanvas` 会被误读为 `UnityEngine.Canvas` 派生类，而 Unity 运行时不存在 `Window` 类型，零误导。

### 基类与生命周期

`AesirBaseWindow`（+ MVP / MVC 变体 `AesirBaseWindowView<T>` / `AesirBaseWindowViewController<T>`）与 `AesirBasePanel` 同构：`OnInit → OnShow(payload) → OnHide → OnClose`，Awake/OnEnable 推迟到首次激活，`DestroyOnHide` 决定关闭销毁或隐藏复用，`CloseSelf()` 便捷关闭自身。与面板的差异点：

- **层级自声明**：`[SerializeField] int sortingOrder = 500`——挂载时应用到根 Canvas，默认恒在面板四层（≤400）之上；多窗口按此值自治排序，同值时后开者居上（sibling 位置）；
- **挂载接线**（`UIModule` 在实例化时统一执行）：挂载 UIRoot 直下 → 应用 `UICanvasConfigSO`（渲染模式 ScreenSpaceCamera / CanvasScaler / GraphicRaycaster）→ 接线 `UICamera` → 应用窗口声明的 sortingOrder → 递归设置 UI 层（layer 5，保证从零新建、默认 layer 0 的预制体内容能被 UICamera 渲染）。根节点缺 Canvas 组件时报错中止，不保留半挂载实例。

### API（静态快捷为用户主入口）

```csharp
UIModule.Open<SettingWindow>(payload);       // 打开窗口（已存在则置顶并重新 OnShow）
UIModule.Close<SettingWindow>();              // 关闭窗口（按 DestroyOnHide 销毁或隐藏）
UIModule.GetWindow<SettingWindow>();          // 获取窗口实例
UIModule.PrewarmWindow<SettingWindow>(path);  // 预热（实例化并隐藏，首次 Open 直接复用）
UIModule.RegisterWindowPrefab<SettingWindow>(prefab);
```

实例侧对应 `OpenWindow` / `CloseWindow` / `GetWindow` / `PrewarmWindow` / `RegisterWindowPrefab`（Type 签名长名）。注册表语义与面板一致：实例实际类型为键、键语义诊断同款；窗口与面板**共用同一份预制体注册表与资源加载器**（类型系统天然分桶），但两套入口跨契约互斥——Panel 类型误入窗口入口（或反之）会报错并指向正确入口，不会误报「未挂载组件」或产生歧义实例。

### 蒙版机制（单遮 / 叠遮）

蒙版不是独立物体，而是每个窗口预制体内的 `Mask` 子物体：位于自身 Canvas 内、`Content` 之下、其余一切 UI 之上——天然挡住本窗口以下的面板与其他窗口的射线与视觉。`UIModule` 按序列化配置 `maskMode` 统一调度，每次窗口 Open / Close / 销毁后重算：

| 模式 | 语义 |
|------|------|
| 单遮（默认） | 全局仅最高层可见窗口的蒙版生效（sortingOrder 最大者，同值取后开者），多窗口叠加透明度不叠加 |
| 叠遮 | 每个窗口的蒙版独立跟随自身打开状态，透明度逐层叠加 |

蒙版点击：`Mask` 子物体挂 `Button` 时由基类在 `OnInit` 自动接线，回调虚方法 `OnMaskClicked()`——默认按 `closeOnMaskClick`（默认 false）决定是否关闭本窗口，子类可覆写自定义行为（如提示「先完成当前操作」）。运行时经 `UIModule.Instance.MaskMode` 切换，切换立即重算。无 `Mask` 子物体的窗口（如全屏不透明加载页）对蒙版调度为无操作，天然不参与遮挡。

## Panel 与 Window 的选型对比

一个项目通常只选一种主形态；两种形态的 API 与生命周期完全对称，按需混用也成立（窗口恒在面板之上）：

| 维度 | Panel（面板） | Window（窗口） |
|------|--------------|---------------|
| 根节点 | RectTransform（无 Canvas） | Canvas（独立渲染根） |
| 挂载 | 四层 Canvas 之下（100–400 由层决定） | UIRoot 直下（默认 500，恒在面板之上） |
| 渲染 | 同层共享层 Canvas，同图集合批友好 | 独立 Canvas，打断合批但隔离重绘 |
| 排序 | 层内由 Show 顺序决定 | sortingOrder 自治（声明任意值） |
| 蒙版 | 无 | Mask 子物体 + 单遮/叠遮调度 |
| API | `Show` / `Hide` / `Get` / `Prewarm` | `Open` / `Close` / `GetWindow` / `PrewarmWindow` |

- **推荐 Panel 的场景**：常驻 HUD、非模态并存的信息/列表面板、同层批量渲染优先的界面——教学与中小项目的默认主形态。
- **推荐 Window 的场景**：模态弹窗（需要挡住下面一切的输入）、全屏流转页（设置 / 暂停 / 结算 / 加载）、需要独立 sortingOrder 或希望把高频变动 UI 从层 Canvas 的重绘中隔离出来的浮层。
- 全 Window 化（每个界面一个独立 Canvas）也成立，代价是放弃共享层 Canvas 的合批友好——适合弹窗密集的模态化项目。

## 加载器

`IUIAssetLoader` 契约为**同步语义**：

```csharp
public interface IUIAssetLoader
{
    GameObject Load(string path);
}
```

- 默认实现 `ResourcesUILoader` 从 Resources 目录加载；
- 自定义实现适用于同步缓存、Addressables 预加载查表等同步可达管线；
- 异步管线（Addressables 直接 `LoadAssetAsync`）无法经本接口表达等待——`Handle.Result` 同步等待在 WebGL 会死锁、在其他平台阻塞主线程，接入前请评估。预加载完成后经自定义 loader 同步返回缓存是推荐做法。

预制体引用由 `UIModule` 注册表持有（生命周期与模块一致），契约不设释放方法。面板与窗口共用同一份注册表与加载器。

## Canvas 配置

`UICanvasConfigSO` 统一配置所有**层 Canvas** 与**窗口根 Canvas**（渲染模式固定 ScreenSpaceCamera）：

- 创建：`Assets → Create → Aesir Modules → UI → Default UICanvasConfig`（与 UIRoot Inspector 的"创建默认资产"按钮共用同一幂等实现）；
- 未创建资产时使用内存默认值（字段默认值与资产一致）；
- 每次初始化（Awake / `Build`）重新应用全量配置，**层 sortingOrder 被强制覆盖为基准值**（100/200/300/400）——手动微调层序会被打回；
- 窗口挂载时同样应用全量配置（渲染模式 / CanvasScaler / GraphicRaycaster），但**窗口 sortingOrder 不被覆盖**——取窗口声明的序列化值（默认 500，恒在面板层之上）；
- 配置不管理预制体内自行添加的子 Canvas。

## 设计边界

- **层级体系封闭**：`UILayer` 固定四层，层序基准硬编码；加层需改框架源码。四层对教学/中小项目已足够。
- **无 per-panel Canvas**：同层多面板共享层 Canvas（同图集合批友好）；代价是同层穿插打断 batch、无独立 sortingOrder。需要动画隔离/独立渲染请在面板预制体内加子 Canvas。同层内顺序仅由 Show 顺序决定。需要独立 Canvas、独立排序或蒙版挡输入的界面请升格为**窗口形态**——窗口是与面板并列的概念，不是 per-panel 选项。
- **窗口蒙版只服务窗口**：单遮/叠遮调度只作用于窗口的 `Mask` 子物体；面板不提供蒙版（需要遮挡输入的面板应改为窗口形态）。
- **主相机需自行排除 UI 层**：UICamera 只渲染 UI(5)+TransparentFX(1)，主游戏相机若含 UI 层会重复渲染。
- **Binder 仅服务编辑期构建**：绑定在「生成脚本 → 编译后自动挂载」时经 `BindComponents()` 完成并写入 prefab 序列化数据；运行时 `AddComponent` 动态创建的面板/窗口不做自动绑定（无运行时重绑兜底）。
- **Binder 生成产物使用 Odin 特性**：生成字段的分组展示（`TitleGroup`）与 `IComponentBinder` 接口位于 Odin 程序集——使用 Binder 的前提是项目已安装 Odin Inspector（与 UI 模块核心「Odin 可选」的边界不同，Binder 全家桶整体在 Odin 程序集内）。
- **不做**：面板/窗口导航栈与返回、SmartShowHide 伪隐藏（下期候选，见 CHANGELOG「Planned」节）、异步加载接口、层扩展配置、per-panel Canvas 选项。

## 测试与维护

- 面板生命周期状态机由 `Tests/Editor/UI/UIModuleTests.cs` 锁定（17 用例：三路 Show、Hide 双分叉、Prewarm 幂等、键语义诊断、RemovePanelRecord 反清理、缺层中止、生命周期顺序、注册时序（OnShow 内递归 Show 不重复实例化、OnShow 抛异常不泄漏）、Awake/OnEnable 推迟到 Show 激活的生命周期契约）；
- 窗口生命周期与蒙版机制由 `Tests/Editor/UI/UIModuleWindowTests.cs` 锁定（17 用例：挂载接线、sortingOrder 应用、UI 层递归、生命周期契约、Close 双分叉、键语义、Panel↔Window 跨契约互斥、根缺 Canvas 中止、反清理，蒙版单遮重算 / 同序 tie / 叠遮独立 / 点击蒙版 / 无 Mask 子物体无操作）；Binder 窗口感知由 `Tests/Editor/BinderAssistantWindowTests.cs` 锁定（5 用例：默认脚本名后缀、默认基类、后缀去重、基类下拉窗口家族、Context 下拉触发）；
- Binder 代码生成器另有 20 用例（`BinderCodeGeneratorTests` 14 + `BinderContextSelectorTests` 1 + `BinderHierarchyUtilityTests` 5）；
- 修改 UIModule 状态机或 UIRoot 层级构建逻辑时，先跑对应 EditMode 测试。
