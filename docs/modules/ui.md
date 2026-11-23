# UI 框架

Aesir Modules 的 UI 框架采用 Manager-of-Managers 单例模式:`UIRoot` 负责层级与画布,`UIModule` 负责面板与窗口生命周期。UI 有三种并列形态 —— **Panel(面板)** 经 `AesirBasePanel` 家族接入、挂载于共享层 Canvas;**Window(窗口, Canvas 根 UI)** 经 `AesirBaseWindow` 家族接入、预制体自带 Canvas、直接挂载于 UIRoot;**World Space Canvas(世界空间 UI)** 以 `WorldSpaceUICanvas` 为渲染根、挂在场景物体上随世界对象移动(血条 / 名字牌 / 交互提示),不进注册表。

## UIRoot —— UI 根节点

菜单 `GameObject → Aesir Modules → Create UIRoot` 一键构建:

- **四层 Canvas**:Background(sortingOrder 100)/ Normal(200)/ Popup(300)/ Top(400),每层为 ScreenSpaceCamera 画布 + CanvasScaler + GraphicRaycaster
- **UICamera**:正交、depth 1、cullingMask 为 UI 与 TransparentFX 层
- **EventSystem**:全场景去重检查;默认挂 `StandaloneInputModule`(安装 Input System 包时自动替换为 `InputSystemUIInputModule`)

层级 Canvas / UICamera / EventSystem 均为**序列化引用持久化** —— 存在性判定只看引用非空,子物体重命名不会破坏引用;构建后可自由调整结构。

画布参数(分辨率 1920×1080、ScaleWithScreenSize 等)由 `UICanvasConfigSO` 统一配置;菜单 `Assets → Create → Aesir Modules → UI → Default UICanvasConfig` 生成默认配置资产,未配置时使用内置默认值。

## UIModule —— 面板管理器

静态快捷 API(全部转发到单例):

| API | 说明 |
|-----|------|
| `UIModule.Show<T>(payload, path)` | 显示面板(可带强类型 payload 与 Resources 路径) |
| `UIModule.Hide<T>()` | 隐藏面板(按 `DestroyOnHide` 决定销毁或缓存) |
| `UIModule.Get<T>()` | 获取已注册面板实例 |
| `UIModule.RegisterPrefab<T>(prefab)` | 注册面板预制体 |
| `UIModule.ContainPrefab<T>()` | 查询预制体是否已注册 |
| `UIModule.Prewarm<T>(path)` | 预实例化并隐藏,首次 Show 直接复用 |

`PrewarmAll()` 通过协程**逐帧**预实例化,分摊首次打开的实例化卡顿。

!!! warning "注册键 = 面板实例的实际类型"
    面板注册表以实例的**实际类型**为键。预制体上挂载的脚本是注册类型的派生类时:

    - 以基类类型重复 `Show`:**报错拒绝**,不会重复实例化
    - 以基类类型 `Hide` / `Get`:**警告提示**实际类型后静默返回
    - 面板内 `HideSelf()` 始终以实际类型调用,永远安全

    最佳实践:**注册、显示、关闭、获取统一使用同一类型**。

## 面板生命周期

```
实例化(停用状态,Awake/OnEnable 不触发)
  → 挂到对应层 Canvas → Initialize → Show(payload)
  → Hide ── DestroyOnHide=false → 停用缓存(SetActive(false),下次 Show 复用)
         └ DestroyOnHide=true  → DestroyPanel(OnHide → OnClose → Destroy)
```

- 面板以**停用状态**实例化:`Awake` / `OnEnable` 推迟到 Show 激活时才触发,保证 `OnEnable` 可安全访问 `OnInit` 之后才有值的引用
- 面板被外部销毁 / 场景卸载时,`AesirBasePanel.OnDestroy` 自动反向清理 UIModule 注册表,无残留

### OnClose 与 OnDestroy 的职责分界

| 销毁路径 | 触发的回调 |
|---------|-----------|
| `Hide` 且 `DestroyOnHide=true`(受控销毁) | `OnHide` → `OnClose` → `Destroy` → `OnDestroy` |
| `Hide` 且 `DestroyOnHide=false` | 仅 `OnHide`(实例缓存复用) |
| 场景卸载 / 外部 `Destroy(gameObject)` | 仅 `OnDestroy` |

!!! warning "事件解绑必须放 OnDestroy"
    `OnClose` 只在**受控销毁路径**调用;场景卸载、外部 `Destroy` 等非受控销毁只触发 `OnDestroy`。事件解绑与订阅释放请放在 `OnDestroy`(或 `OnClose` + `OnDestroy` 两处) —— 仅写在 `OnClose` 会在场景切换时泄漏(MiniEvent / ObservableValue 的订阅没有死引用清理兜底)。

面板基类家族:

| 基类 | 角色 | 用途 |
|------|------|------|
| `AesirBasePanel` | `IUIPanel` | 通用面板基类,虚方法 `OnInit` / `OnShow(payload)` / `OnHide` / `OnClose`,便捷方法 `HideSelf()` |
| `AesirBasePanelView<T>` | + RAA `IView` | MVP 模式面板:按 Context 类型绑定,只读访问 Model / Service |
| `AesirBasePanelViewController<T>` | + RAA `IController` | MVC 模式面板:可执行 Command / Query |

## 窗口(Canvas 根 UI)

与 Panel 并列的第二种 UI 形态: 预制体**根节点自带 Canvas**(独立渲染根), 打开时直接挂载到 UIRoot 下(不经四层 Canvas), 默认 `sortingOrder` 500 恒在面板四层之上 —— 适合模态弹窗、全屏流转页(设置 / 暂停 / 结算 / 加载)与需要独立排序或渲染隔离的浮层。

预制体结构约定:

```
XxxWindow(根: Canvas + CanvasScaler + GraphicRaycaster + 窗口脚本; 类名 = 脚本名 = 预制体名, 以 Window 结尾)
├── Mask        蒙版: Image 全屏拉伸(拦截其下一切 UI 的射线) + 可选 Button(承接点击)
└── Content     实际 UI 元素容器(框架不触碰)
```

静态快捷 API:

| API | 说明 |
|-----|------|
| `UIModule.Open<T>(payload, path)` | 打开窗口(已存在则置顶并重新 OnShow) |
| `UIModule.Close<T>()` | 关闭窗口(按 `DestroyOnHide` 决定销毁或缓存) |
| `UIModule.GetWindow<T>()` | 获取已注册窗口实例 |
| `UIModule.RegisterWindowPrefab<T>(prefab)` | 注册窗口预制体(与面板共用注册表, 类型系统天然分桶) |
| `UIModule.PrewarmWindow<T>(path)` | 预实例化并隐藏, 首次 Open 直接复用 |

生命周期与面板完全同构(`AesirBaseWindow` 虚方法 `OnInit` / `OnShow(payload)` / `OnHide` / `OnClose`, 便捷方法 `CloseSelf()`; MVP / MVC 变体 `AesirBaseWindowView<T>` / `AesirBaseWindowViewController<T>` 同款)。差异仅在根节点与挂载: 根 Canvas 挂载时统一接线 UICamera / 渲染模式 / Canvas 缩放配置, 按窗口声明的 `sortingOrder` 排序, 并递归设置 UI 层; 注册键 = 窗口实例的实际类型, 键语义与面板同款; 面板与窗口入口互斥, 类型误入会报错并指向正确入口。

!!! note "命名后缀取 Window 而非 Canvas"
    按 .NET 命名惯例(派生类以基类名结尾是阅读预期), `XxxCanvas` 会被误读为 `UnityEngine.Canvas` 派生类; Unity 运行时不存在 `Window` 类型, 零误导。

### 蒙版机制(单遮 / 叠遮)

蒙版不是独立物体, 而是窗口预制体内的 `Mask` 子物体: 位于自身 Canvas 内、`Content` 之下、其余一切 UI 之上 —— 天然挡住本窗口以下的面板与其他窗口的射线与视觉。`UIModule` 按全局配置资产 `UIModuleConfigSO` 的 `maskMode` 统一调度, 每次窗口 Open / Close / 销毁后重算。配置资产由编辑器自动创建于 `Assets/Resources/UIModuleConfig/`, 在 Project 窗口直接编辑即可, 无需预放置 `[UIModule]`:

| 模式 | 语义 |
|------|------|
| 单遮(默认) | 全局仅最高层可见窗口的蒙版生效(sortingOrder 最大者, 同值取后开者), 多窗口叠加透明度不叠加 |
| 叠遮 | 各窗口蒙版独立跟随自身打开状态, 透明度逐层叠加 |

蒙版点击经 `Mask` 子物体的 Button 在 `OnInit` 自动接线, 回调虚方法 `OnMaskClicked()` —— 默认按 `closeOnMaskClick`(默认关)决定是否关闭本窗口, 子类可覆写自定义行为; 运行时经 `UIModule.MaskMode` 切换(初值取自 `UIModuleConfigSO`, 运行时切换只覆盖内存值并立即重算)。无 `Mask` 子物体的窗口(如全屏不透明加载页)不参与遮挡。

运行示例见包内 `Samples/UI/01_BasicUsage`(面板与窗口协作、蒙版单遮 / 叠遮运行时切换对照、点击蒙版关闭、全屏加载窗口 payload 自动关闭)。

## 世界空间 UI(World Space Canvas)

第三种并列形态:需要 Canvas 随**世界空间物体**移动时(角色头顶血条 / 名字牌、场景交互提示、VR / AR 标记),使用 `WorldSpaceUICanvas` —— 挂在场景物体上即获得 `RenderMode.WorldSpace` 渲染根,不进入 `UIModule` 注册表。

入口为 `Add Component → Aesir Modules → UI → World Space UI Canvas`;组件在 `Awake` 自动补齐并接线:

- `Canvas`(`RenderMode.WorldSpace`)+ `RectTransform`(方形)+ `CanvasScaler`(`ConstantPixelSize`)+ `GraphicRaycaster`
- 子树归入 UI 层(5),默认 `sortingOrder` 450 —— 落在面板层(100–400)与窗口(≥500)之间
- **跟随**:`FollowTarget` 留空即随父级变换移动(推荐把 Canvas 挂在目标子节点);填目标则由组件每帧同步父级变换
- **朝向**:`FaceCamera` 与 `Billboard`(`None` / `CameraFacingYLocked` / `CameraFacingFull`),`FlipFacing` 反向
- **偏移**:`PositionOffset`(局部偏移)

运行时创建:

```csharp
var nameplate = UIRoot.Instance.CreateWorldSpaceCanvas(character.transform, "Nameplate");
nameplate.Billboard = WorldSpaceUICanvas.WorldSpaceBillboardMode.CameraFacingYLocked;
```

配置来源优先级:**组件上显式指定的配置 → `UIRoot.WorldSpaceCanvasConfig` → 内存默认 `UICanvasConfigSO.CreateWorldSpaceDefault()`**。世界空间配置与屏幕空间配置(层 Canvas / 窗口根)是两条独立链,互不覆盖:世界空间专用字段为 `Dynamic Pixels Per Unit`(默认 100,决定 UI 在世界中的物理大小)与 `Reference Pixels Per Unit`(默认 100,影响字体 / 精灵基准尺寸),屏幕缩放字段不参与世界空间配置。

!!! warning "世界空间 UI 不进注册表"
    `WorldSpaceUICanvas` 只提供渲染根:生命周期随宿主场景物体(目标销毁 / 场景卸载),内容由业务代码直接驱动子节点;**不参与** `UIModule` 的 Show / Hide / 蒙版调度。需要面板 / 窗口那种生命周期与注册表管理时,按 Panel 或 Window 形态接入。

!!! note "接线期参数需重建"
    `Awake` 会自动执行一次 `Build()` 完成接线;`CreateWorldSpaceCanvas` 创建时也带配置执行一次。运行时改跟随目标 / 偏移 / 朝向字段由 `LateUpdate` 每帧读取,立即生效;而**配置与 `sortingOrder` 属于接线期参数**,换配置后需再调用 `Build(overrideConfig)` 才会重新应用到 Canvas。

## Panel 与 Window 的选型对比

一个项目通常只选一种主形态; 两种形态 API 与生命周期完全对称, 按需混用也成立(窗口恒在面板之上):

| 维度 | Panel(面板) | Window(窗口) |
|------|--------------|---------------|
| 根节点 | RectTransform(无 Canvas) | Canvas(独立渲染根) |
| 挂载 | 四层 Canvas 之下(100–400 由层决定) | UIRoot 直下(默认 500, 恒在面板之上) |
| 渲染 | 同层共享层 Canvas, 同图集合批友好 | 独立 Canvas, 打断合批但隔离重绘 |
| 排序 | 层内由 Show 顺序决定 | sortingOrder 自治(声明任意值) |
| 蒙版 | 无 | Mask 子物体 + 单遮/叠遮调度 |
| API | `Show` / `Hide` / `Get` / `Prewarm` | `Open` / `Close` / `GetWindow` / `PrewarmWindow` |

- **推荐 Panel**: 常驻 HUD、非模态并存的信息 / 列表面板、同层批量渲染优先 —— 教学与中小项目的默认主形态
- **推荐 Window**: 模态弹窗(需要挡住下面一切的输入)、全屏流转页(设置 / 暂停 / 结算 / 加载)、需要独立排序或渲染隔离的浮层
- 全 Window 化(每个界面一个独立 Canvas)也成立, 代价是放弃共享层 Canvas 的合批友好 —— 适合弹窗密集的模态化项目
- **推荐 World Space Canvas**: 血条 / 名字牌 / 场景交互提示等必须跟随世界物体的 UI —— 只提供渲染根, 没有 Show / Hide 生命周期与注册表, 内容与生命周期由业务代码直接持有

## 可插拔资源加载

默认 `ResourcesUILoader`(预制体路径约定为**面板类型名**)。实现 `IUIAssetLoader` 可替换为其他同步可达方案:

```csharp
public interface IUIAssetLoader
{
    GameObject Load(string path);
}

// 注入自定义加载器
UIModule.Instance.RegisterAssetLoader(new MyAddressablesLoader());
```

!!! warning "加载契约为同步语义"
    `Load` 需同步返回预制体。Addressables 等异步管线无法在接口内表达等待 —— `Handle.Result` 同步等待在 WebGL 会死锁、在其他平台阻塞主线程。推荐做法:**预加载完成后经自定义 loader 查缓存同步返回**。预制体引用由 UIModule 注册表持有,契约不设释放方法。

## DDOL 机制

`AesirModules` / `UIRoot` / `UIModule` / `EventModule` 均有 `[SerializeField] bool dontDestroyOnLoad = true`:

- `AesirModules` 宿主:运行时创建恒为 DDOL
- `UIRoot`:预放置与运行时创建统一由该字段控制
- `UIModule`:字段仅在**预放置为根物体**时生效;运行时自动创建时挂载于 `[Aesir Modules]` 宿主下,跟随宿主决策
- `EventModule`:预放置实例同样由该字段控制(0.32.0 起补上字段)——此前预放置在场景根的事件模块会随场景卸载被销毁,注册表整体消失

## 设计边界

- **层级体系是封闭集** —— `UILayer` 固定四层(Background / Normal / Popup / Top),层序基准硬编码为 100 / 200 / 300 / 400 且每次初始化强制覆盖;新增层级需修改框架源码
- **无 per-panel Canvas** —— 同层多面板共享层 Canvas(同图集合批友好);需要动画隔离 / 独立渲染请在面板预制体内自行添加子 Canvas。同层内渲染顺序仅由 Show 顺序决定;需要独立 Canvas、独立排序或蒙版挡输入的**完整界面**请升格为窗口形态
- **蒙版只服务窗口形态** —— 单遮 / 叠遮调度只作用于窗口的 `Mask` 子物体,面板不提供蒙版(需要遮挡输入的面板应改为窗口)
- **世界空间 UI 不进注册表** —— `WorldSpaceUICanvas` 只提供随世界对象移动的渲染根,生命周期随宿主场景物体,不经 `UIModule` 的 Show / Hide / 蒙版调度;需要生命周期管理时请按面板 / 窗口形态接入
- **主相机需自行排除 UI 层** —— UICamera 的 cullingMask 只含 UI 与 TransparentFX 层,主游戏相机若也包含 UI 层会重复渲染
- **不做** —— 面板 / 窗口导航栈与返回、异步加载接口、层扩展配置;**SmartShowHide 伪隐藏**(全屏窗口弹出时自动伪隐藏被遮挡面板)为下期候选(见包 CHANGELOG 的 Planned 节)

## Binder 组件绑定(需 Odin Inspector)

将 UI 元素自动绑定到面板脚本,免去手写 `transform.Find` / `GetComponent`:

1. `BinderTag` 挂在待绑定子物体上做标记(右键菜单 `GameObject → Aesir → 添加 BinderTag 标记`)
2. `BinderAssistant` 挂在面板根(右键菜单 `GameObject → Aesir → 挂载 BinderAssistant`),「构建绑定单元」按标记增量维护绑定列表
3. 「生成脚本」产出绑定代码;编译完成后自动挂载组件并执行一次绑定

生成配置(后缀列表 / 默认后缀 / 最近命名空间)经 `BinderEditorSettings` 持久化到 `ScriptableSingleton/AesirModules/BinderEditorSettings.asset`,重启编辑器后保留。

两种生成模式:

| 模式 | 行为 |
|------|------|
| **同一脚本增量**(默认) | 只替换 `*.cs` 内「绑定字段(自动生成)」region,region 外归开发者 |
| **Partial 分部类** | 自动维护文件(后缀默认 `.designer.cs`)+ 手写 partial 仅首次生成;Rider 用户推荐 |

生成脚本基类可下拉选择:`MonoBehaviour`、Aesir 面板 / 窗口家族(`AesirBasePanel` / `AesirBasePanelView<T>` / `AesirBasePanelViewController<T>` / `AesirBaseWindow` / `AesirBaseWindowView<T>` / `AesirBaseWindowViewController<T>`;Canvas 根物体上默认基类直指 `AesirBaseWindow`、默认脚本名取 `Window` 后缀,面板根保持 `Panel`,已带对应后缀的物体名不重复拼接)或用户以 `[BinderBaseType]` 标记的自定义基类;选择 Aesir 泛型基类时,「Context 类型」下拉扫描项目内 `AbstractContext` 派生类(自动排除 `[InternalContext]` 标记的示例 / 测试 Context)。

!!! note "Odin 依赖"
    Binder 的类型选择器(组件 / 基类下拉)强依赖 Odin Inspector:运行时可见部分在 `Runestone.AesirModules.OdinInspector`(编辑器实现整文件 `#if UNITY_EDITOR` 剔除,不随 Player 构建打包),编辑器入口在 `Runestone.AesirModules.Editor.OdinInspector`;未安装 Odin 时整体排除,不影响 UI 框架其余功能。

## 继续阅读

- [快速开始](getting-started.md) —— 第一个面板的最小流程
- [场景模块](scene.md) —— SceneModule 与 SceneAssetWrapper
