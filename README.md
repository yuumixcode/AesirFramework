# Aesir Modules

Aesir Architecture (RAA) 的功能模块包。当前提供 UI 框架（Manager of Managers 模式）、实验性事件模块与场景管理工具。

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE.md)
[![Version](https://img.shields.io/badge/version-0.16.0-blue.svg)](./CHANGELOG.md)
[![Unity](https://img.shields.io/badge/Unity-2022.3%2B-black.svg)](https://unity.com/)
[![Install via Git URL](https://img.shields.io/badge/UPM-Git%20URL-blueviolet.svg)](#安装)
[![English](https://img.shields.io/badge/README-English-blue.svg)](./Documentation/README_EN.md)

> 📦 **本包是 [AesirFramework](https://github.com/yuumixcode/AesirFramework) monorepo 的一部分**。本包**依赖**：
> - **[Aesir Architecture](https://github.com/yuumixcode/AesirFramework)**（`>= 0.16.0`）

## 模块总览

| 模块 | 状态 | 说明 |
|------|------|------|
| UI | 已实现 | `UIModule` 单例（Manager of Managers）+ `UIRoot` 四层 Canvas + 面板生命周期 + 可插拔资源加载 |
| Event | ⚠️ 实验性 | `EventModule` 双轨订阅（Attribute + Script）+ 5 档优先级 + 表达式树优化。尚未在实际项目中验证 |
| Scene | 已实现 | `SceneModule` 启动/叠加场景管理 + 编辑器工具（SceneManagerWindow / BootstrapSceneHelper） |

> 另有两项可选能力：**Binder 组件绑定**（`Runtime/UI/OdinInspector/Binder/`，需 Odin Inspector）与 **Input System 输入模块适配**（`Runtime/UI/InputSystem/`，独立程序集，启用 Input System 时自动生效）。

## 依赖

- **Aesir Architecture (RAA)** `cn.runestone.aesir.architecture` >= 0.16.0（必需）
- **Odin Inspector**（可选）：仅通过 `#if ODIN_INSPECTOR` 条件编译参与，未导入时自动排除。

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
https://github.com/yuumixcode/AesirFramework.git#AesirModules-v0.16.0
```

或编辑 `Packages/manifest.json`：

```json
{
  "dependencies": {
    "cn.runestone.aesir.modules": "https://github.com/yuumixcode/AesirFramework.git#AesirModules-v0.16.0"
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
| `IUIAssetLoader` / `ResourcesUILoader` | Engine | 可插拔资源加载契约与默认实现（Resources 目录） |
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

需要自定义资源加载（如 Addressables）时替换默认加载器：

```csharp
UIModule.Instance.RegisterAssetLoader(new MyAddressablesLoader());
```

4. 面板生命周期（全部由 `UIModule` 驱动）：

```csharp
public class MainMenuPanel : AesirBasePanel
{
    protected override void OnInit() { }               // 首次实例化后调用一次
    protected override void OnShow(object payload) { } // 每次显示时调用（含首次）
    protected override void OnHide() { }               // 隐藏时调用（默认 SetActive(false)）
    protected override void OnClose() { }              // 销毁前调用
}
```

> **生命周期细节**：面板以停用状态实例化（Awake / OnEnable 推迟到 Show 激活时才触发，保证 OnEnable 可安全访问 OnInit 之后才有值的引用），按 挂层 → `Initialize` → `Show` 顺序驱动；面板注册以实例的**实际类型**为键，以基类类型 Show 后需以实际类型（或面板内 `HideSelf()`）关闭。

### 目录结构

```
Runtime/UI/                        # 汇入核心运行时程序集（层根锚点）
├── UIModule.cs                    # UI 管理器单例
├── UIRoot.cs                      # UI 根节点（四层 Canvas 构建）
├── IUIPanel.cs                    # 面板契约
├── AesirBasePanel.cs              # 面板基类
├── AesirBasePanelView.cs          # MVP 面板视图基类（绑定 Context）
├── UILayer.cs                     # 层级枚举
├── UICanvasConfigSO.cs            # Canvas 配置资产
├── UIAssetLoader/                 # IUIAssetLoader + ResourcesUILoader
├── InputSystem/                   # Input System 输入模块适配（独立可选程序集）
└── OdinInspector/Binder/          # Binder 全家桶（经 asmref 汇入 Odin 程序集）
Editor/UI/                         # 汇入核心编辑器程序集（层根锚点）
├── UIModuleMenuItems.cs           # Create UIRoot / Default UICanvasConfig 菜单项
└── OdinInspector/                 # Odin AttributeProcessors（经 asmref 汇入 Odin 编辑器程序集）
```

## 事件模块

> ⚠️ **实验性模块**：尚未在实际项目中验证，API 可能调整。

基于双轨订阅的事件系统。Attribute 订阅通过 `[AesirListener]` 特性标记方法，Script 订阅通过 `AddListener<T>` 动态注册 Lambda 委托。两种订阅共存于同一分发流程，按 5 档优先级排序执行。

### 核心类型

| 类型 | 说明 |
|------|------|
| `AesirEventArgs` | 事件参数抽象基类。所有自定义事件参数继承此类，作为数据载体在 EventModule 中传递 |
| `AesirListenerAttribute` | 方法特性，标记该方法监听指定事件参数类型 |
| `EventModule` | MonoBehaviour 单例，管理双注册表与事件分发 |
| `BindingInfo` | 绑定信息基类；`StaticBindingInfo` 持有 MethodInfo + 表达式树编译委托；`DynamicBindingInfo<T>` 持有 `Action<T>` 直接委托 |
| `SubscriberPriority` | 订阅优先级枚举（5 档：First/High/Medium/Low/Last） |
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

// 零参数方法订阅（需显式指定事件参数类型）
[AesirListener(typeof(OnKeyPressed))]
private void OnKeyPressed() { ... }
```

### 目录结构

```
Runtime/Events/                    # 汇入核心运行时程序集（层根锚点）
├── AesirEventArgs.cs              # 事件参数基类
├── AesirListenerAttribute.cs      # 订阅者特性
├── AesirEventUtility.cs           # 静态工具
├── BindingInfo.cs                 # 绑定信息基类 + StaticBindingInfo + DynamicBindingInfo<T>
├── Component/
│   └── EventModule.cs             # 事件模块单例
└── SubscriberPriority.cs          # 优先级枚举（5 档）
```

详细文档见 [Documentation/event-module.md](./Documentation/event-module.md)。

## 场景模块

`SceneModule`（MonoBehaviour 单例）负责启动场景与叠加场景管理：

- **启动场景（Bootstrap）** — 按预设名称（`Bootstrap` / `Bootstrapper` 等）或自定义 `SceneAssetWrapper` 引用自动发现启动场景，并确保其在构建场景列表中序号为 0、优先加载
- **场景加载** — `LoadSceneSingle` 单模式加载、`LoadSceneAdditive` 叠加加载（纯叠加追踪，`AddedScenePaths` / `LastLoadedScene` / `GetTotalLoadingProgress`）；均支持路径或 `SceneAssetWrapper` 重载与完成/失败回调
- **场景卸载** — `UnloadScene` 按路径或引用卸载、`UnloadAllAddedScenes` 批量回收、`ReloadScene` 异步重载当前场景
- **`SceneAssetWrapper`** — 可序列化场景引用：GUID 锚点自愈、状态机校验（`State` / `UnsafeReason`）、`TryGet` 安全读取家族；安装 Addressables 时自动扩展地址查询能力（经胶水程序集条件编译，未安装时相关功能自动隐藏）
- **编辑器配套** — `SceneManagerWindow` 场景管理窗口；`BootstrapSceneHelper` 自动/手动搜集 Bootstrapper 场景并注册进 Build Settings

### 目录结构

```
Runtime/Scene/                     # 汇入核心运行时程序集（层根锚点）
├── SceneModule.cs                 # 场景管理单例（Bootstrap / 加载 / 卸载 / 重载）
├── SceneAssetWrapper.cs           # 可序列化场景引用（GUID 锚点 + 状态机）
├── SceneAssetWrapperState.cs      # 引用状态机
├── SceneAssetWrapperUnsafeReason.cs
├── SceneAssetWrapperAddressablesBridge.cs  # Addressables 能力静态桥
└── Exceptions/                    # 专用异常族
Editor/Scene/                      # 汇入核心编辑器程序集（层根锚点）
├── SceneManagerWindow.cs          # 场景管理窗口
├── BootstrapSceneHelper.cs        # Bootstrapper 场景注册工具
├── SceneEditorSettings.cs         # 编辑器持久化设置
├── Tests/                         # EditMode 测试（独立测试程序集）
├── OdinInspector/                 # SceneAssetWrapper Processor（经 asmref 汇入）
└── Addressables/                  # Addressables 胶水实现（经 asmref 汇入）
```

## Binder 组件绑定（Odin 可选）

位于 `Runtime/UI/OdinInspector/Binder/`（经 asmref 汇入 Odin 程序集，需 Odin Inspector）：`BinderAssistant` / `BinderTag` 将面板下 UI 元素（Text、Button 等）按层级自动绑定到脚本字段，减少手工拖引用；配套 `IComponentBinder` 自定义绑定器扩展。

## 示例

- 本仓库直接浏览 / 下载源码：示例位于包内 `Samples/` 文件夹，可直接查看运行。
- Git URL 安装：Package Manager → 选中本包 → `Samples` 标签页按需导入（源在包内 `Samples~/` 隐藏目录，构建时自动剔除）。
- unitypackage 导入：示例随包内含，导入后即可运行。

当前提供：

| 示例 | 说明 |
|------|------|
| `Events/01_KeyPress` | 事件模块基本发布-订阅示例：按键发布事件、`[AesirListener]` 静态订阅 |

## 许可证

MIT

