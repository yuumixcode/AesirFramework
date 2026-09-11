# Scene 模块

场景加载、叠加追踪与卸载回收模块，语义对齐 Unity 原生 `LoadSceneMode`：Single 卸载全部场景并重设激活场景；Additive 纯叠加、不改变激活场景，叠加场景统一记入追踪列表。

## 核心类型

| 类型 | 说明 |
|------|------|
| `SceneModule` | 场景管理单例（MonoBehaviour，预放置优先/运行时自动创建于 `[Aesir Modules]` 宿主下） |
| `SceneAssetWrapper` | 可序列化场景引用：GUID 锚点自愈、状态机校验、TryGet 安全读取家族、Addressables 地址缓存 |
| `SceneAssetWrapperState` | 引用状态枚举：`Regular`（BuildSettings 途径）/ `Addressable` / `Unsafe` |
| `SceneAssetWrapperUnsafeReason` | 不安全原因：`Empty` / `NotInBuild` |
| `SceneAssetWrapperException` 异常族 | 空引用 / 创建失败 / Addressables 未装包 / 不可寻址四类，消息带"修复 / 规避"双指引 |
| `BootstrapSceneHelper` | 编辑器 Bootstrapper 场景搜集注册工具（默认关闭，`Tools → Aesir → Scene Editor Settings` 开启） |

## SceneModule API

```csharp
// 加载：path / SceneAssetWrapper 双重重载；完成/失败/进度回调全部可选
SceneModule.Instance.LoadSceneSingle(scenePath,
    onCompleted: () => { },
    onFailed:    () => { },
    onProgress:  p => { });   // 逐帧 0-1，已按 Unity 激活上限 0.9 归一化
SceneModule.Instance.LoadSceneAdditive(scenePath);

// 卸载：经本模块叠加加载的场景自动移出追踪；批量卸载单个失败跳过并告警
SceneModule.Instance.UnloadScene(scenePath);
SceneModule.Instance.UnloadAllAddedScenes();

// 激活场景切换（决定光照设置来源与 Instantiate 默认落点），返回是否成功
SceneModule.Instance.SetActiveScene(scenePath);

// 重载当前激活场景（异步 Single 语义）
SceneModule.Instance.ReloadScene();

// 场景生命周期广播（MiniEvent<string>，参数为场景路径，AddListener 返回自动清理句柄）
SceneModule.Instance.SceneLoadedEvent.AddListener(path => Debug.Log($"已加载 {path}"));
SceneModule.Instance.SceneUnloadedEvent.AddListener(path => Debug.Log($"已卸载 {path}"));
```

## SceneAssetWrapper

```csharp
// Inspector 拖拽赋值（需 Odin），或代码构造：
var wrapper = SceneAssetWrapper.FromScenePath("Assets/Scenes/Gameplay.unity");

// 状态校验（加载前判断）
if (wrapper.State == SceneAssetWrapperState.Regular) { /* 可经 BuildSettings 加载 */ }

// 安全读取（不抛异常）
if (wrapper.TryGetScenePath(out var path)) { /* 使用路径 */ }

// fail-fast 读取（空引用/未装包/不可寻址抛专用异常，消息带修复指引）
var name = wrapper.SceneName;
var address = wrapper.Address; // Addressable 场景
```

Inspector 三态着色与一键修复（需 Odin）：Addressable 场景青色；引用悬空（SceneAsset 丢失但路径残留）红色 + Error 信息框；未加入 BuildSettings 红色 + "添加到 BuildSettings"按钮；已加入但被禁用黄色 + "启用"按钮。

## 生命周期与 DDOL

- 预放置为根物体时受 `dontDestroyOnLoad` 字段（默认开）控制 DDOL；关闭会输出警告——Single 加载会卸载所有旧场景，关闭 DDOL 的实例将随场景销毁并中断进行中的加载回调。
- 运行时自动创建的实例挂在 `[Aesir Modules]` 宿主下，跟随宿主的 DDOL 决策。
- 重复实例只销毁自身组件（`Destroy(this)`），不连带销毁宿主物体。

## 设计边界

- **Odin Inspector 边界** — `SceneAssetWrapper` 的 Inspector 面板效果（拖拽赋值、着色、一键修复按钮）依赖 Odin Inspector；未安装 Odin 时仅保证 API 可用（`FromScenePath` 构造、编辑器下 `SceneAsset` 属性代码赋值、TryGet 家族），面板不支持。
- **Addressable 场景不经 SceneModule 加载** — wrapper 提供地址缓存（`Address` / `TryGetAddress`），加载/卸载请直接调用 Addressables API（如 `Addressables.LoadSceneAsync(wrapper.Address)`）。
- **重复叠加同一路径后果自负** — Unity 会加载两个实例而追踪只记一条，卸载只移除其一；请勿对同一路径重复 `LoadSceneAdditive`。
- **启动场景分工** — 运行时 `SceneModule` 只持有 `bootstrapScene` 引用供用户代码读取（`BootstrapSceneAssetWrapper`），不做自动流转；BuildSettings 序号 0 与进 Play 强制打开 Bootstrap 由编辑器 `BootstrapSceneHelper` 负责（默认关闭）。
- **不做场景间传参 / async 化** — 跨场景传数据用框架 MiniEvent 或共享 Model。
