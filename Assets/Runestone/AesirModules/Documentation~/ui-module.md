# UI 模块详细文档

UI 框架（Manager of Managers 模式）：`UIModule` 单例管理面板生命周期，`UIRoot` 负责四层 Canvas 层级构建，面板以 `AesirBasePanel` 家族为根节点。本文覆盖快速开始之外的实现细节与设计边界；基础用法见包 [README](../README.md#ui-模块)。

## 架构总览

```
[Aesir Modules]（宿主，DDOL）
└── UIModule                     # 面板注册表 + 生命周期驱动（单例）
UIRoot（预放置或运行时创建）
├── UICamera                     # 正交、depth=1、cullingMask=UI|TransparentFX
├── EventSystem                  # 自建时挂 StandaloneInputModule 或 InputSystemUIInputModule
├── BackgroundLayer              # Canvas，sortingOrder 100
├── NormalLayer                  # Canvas，sortingOrder 200
├── PopupLayer                   # Canvas，sortingOrder 300
└── TopLayer                     # Canvas，sortingOrder 400
        ↑ 面板实例挂载于所属层的 Canvas 下
```

依赖方向为单向拉取：`UIModule.EnsureReady` 在每次面板操作时经 `UIRoot.Instance` 惰性发现 UI 根节点（未预放置时自动创建全层级）；`UIRoot` 不反向注册 UIModule——这保证编辑器菜单 `Create UIRoot` 只创建层级本身，不会连带触发运行时单例的创建链。

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

预制体引用由 `UIModule` 注册表持有（生命周期与模块一致），契约不设释放方法。

## Canvas 配置

`UICanvasConfigSO` 统一配置所有**层 Canvas**（渲染模式固定 ScreenSpaceCamera）：

- 创建：`Assets → Create → Aesir Modules → UI → Default UICanvasConfig`（与 UIRoot Inspector 的"创建默认资产"按钮共用同一幂等实现）；
- 未创建资产时使用内存默认值（字段默认值与资产一致）；
- 每次初始化（Awake / `Build`）重新应用全量配置，**层 sortingOrder 被强制覆盖为基准值**（100/200/300/400）——手动微调层序会被打回；
- 配置不管理面板预制体内自行添加的子 Canvas。

## 设计边界

- **层级体系封闭**：`UILayer` 固定四层，层序基准硬编码；加层需改框架源码。四层对教学/中小项目已足够。
- **无 per-panel Canvas**：同层多面板共享层 Canvas（同图集合批友好）；代价是同层穿插打断 batch、无独立 sortingOrder。需要动画隔离/独立渲染请在面板预制体内加子 Canvas。同层内顺序仅由 Show 顺序决定。
- **主相机需自行排除 UI 层**：UICamera 只渲染 UI(5)+TransparentFX(1)，主游戏相机若含 UI 层会重复渲染。
- **不做**：面板导航栈/返回、全局模态遮罩管理、异步加载接口、层扩展配置、per-panel Canvas 选项。

## 测试与维护

- 面板生命周期状态机由 `Tests/Editor/UI/UIModuleTests.cs` 锁定（13 用例：三路 Show、Hide 双分叉、Prewarm 幂等、键语义诊断、RemovePanelRecord 反清理、缺层中止、生命周期顺序）；
- Binder 代码生成器另有 18 用例（`BinderCodeGeneratorTests` 等）；
- 修改 UIModule 状态机或 UIRoot 层级构建逻辑时，先跑对应 EditMode 测试。
