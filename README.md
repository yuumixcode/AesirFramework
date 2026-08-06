# Aesir Modules

Aesir Architecture (RAA) 的功能模块包。当前提供 UI 框架（Manager of Managers 模式）和事件模块（反射绑定 + 特性标记静态订阅 + 动态订阅 + 表达式树优化）。

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE.md)
[![Version](https://img.shields.io/badge/version-0.8.0-blue.svg)](./CHANGELOG.md)
[![Unity](https://img.shields.io/badge/Unity-2022.3%2B-black.svg)](https://unity.com/)
[![Install via Git URL](https://img.shields.io/badge/UPM-Git%20URL-blueviolet.svg)](#安装)
[![English](https://img.shields.io/badge/README-English-blue.svg)](./Documentation~/README_EN.md)

> 📦 **本包是 [Unity-Aesir-Packages](https://github.com/yuumixcode/Unity-Aesir-Packages) monorepo 的一部分**。本包**依赖**：
> - **[Aesir Architecture](https://github.com/yuumixcode/Unity-Aesir-Packages)**（`>= 0.8.0`）

## 模块

| 模块 | 状态 | 说明 |
|------|------|------|
| UI | 已实现 | UIManager 单例；四层 Canvas + 面板生命周期 |
| Event | ⚠️ 实验性 | EventModule 单例；双轨订阅（Attribute + Script）+ 5 档优先级 + 表达式树优化。尚未在实际项目中验证 |
| Scene | 规划中 | 场景加载、SceneReference |

## 依赖

- **Aesir Architecture (RAA)** `cn.runestone.aesir.architecture` >= 0.8.0（必需）
- **Odin Inspector**（可选）：仅通过 `#if ODIN_INSPECTOR` 条件编译参与，未导入时自动排除。

## 安装

通过 Unity Package Manager 添加包：

```
Packages/manifest.json -> dependencies:
"cn.runestone.aesir.modules": "https://github.com/yuumixcode/Unity-Aesir-Packages.git?path=Assets/Runestone/AesirModules"
```

UPM 会自动解析 `package.json` 的 `dependencies` 字段，拉取 Aesir Architecture。

## UI 模块

### 核心类型

| 类型 | 层 | 说明 |
|------|----|------|
| `UIManager` | Component | MonoBehaviour 单例，管理面板加载、生命周期、层级；内含四层 Canvas、UICamera、EventSystem |
| `IUIPanel` | Engine | 面板契约接口 |
| `IUIAssetLoader` | Engine | 资源加载器契约接口 |
| `IUICanvasConfig` | Engine | Canvas 配置契约接口 |
| `PanelConfig` | Engine | 面板配置（layer / destroyOnClose） |
| `UILayer` | Engine | 层级枚举：Background=0, Normal=1, Popup=2, Top=3 |
| `ResourcesUILoader` | Engine | 默认加载器（Resources 目录） |
| `AesirUIPanel` | Component | 面板抽象基类，继承 AesirMonoBehaviour |
| `UICanvasConfigSO` | Component | ScriptableObject 配置资产（CreateAssetMenu） |

### 快速开始

1. 创建 UI 预制体，根节点挂脚本继承 `AesirUIPanel`。
2. 注册预制体并打开面板：

```csharp
// 启动时配置（必须在首次面板操作前调用）
UIManager.Instance.SetLoader(new ResourcesUILoader());
UIManager.Instance.SetCanvasConfig(configSO);   // 可选

// 注册预制体
UIManager.RegisterPrefab<MainMenuPanel>(prefab);

// 打开面板
UIManager.Open<MainMenuPanel>();

// 带参数打开
UIManager.Open<ConfirmDialogPanel>(new ConfirmData { message = "确定？" });

// 关闭面板
UIManager.Close<ConfirmDialogPanel>();
```

3. 面板生命周期：

```csharp
public class MainMenuPanel : AesirUIPanel
{
    protected override void OnInit() { }           // 首次创建时调用
    protected override void OnShow(object payload) { } // 每次显示时调用
    protected override void OnHide() { }            // 隐藏时调用
    protected override void OnClose() { }           // 销毁时调用
}
```

### 目录结构

```
Runtime/
├── Engine/UI/
│   ├── Interfaces/
│   │   ├── IUIPanel.cs
│   │   ├── IUIAssetLoader.cs
│   │   └── IUICanvasConfig.cs
│   ├── PanelConfig.cs
│   ├── UILayer.cs
│   ├── ResourcesUILoader.cs
│   └── UILog.cs
├── Component/UI/
│   ├── UIManager.cs
│   ├── AesirUIPanel.cs
│   └── UICanvasConfigSO.cs
Editor/
└── Odin Integration/UI/
    └── UICanvasConfigSOAttributeProcessor.cs
```

详细文档见 [Documentation~/ui-module-manual.md](Documentation~/ui-module-manual.md)。

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
Runtime/Events/
├── AesirEventArgs.cs              # 事件参数基类
├── AesirListenerAttribute.cs      # 订阅者特性
├── AesirEventUtility.cs           # 静态工具
├── BindingInfo.cs                 # 绑定信息基类 + StaticBindingInfo + DynamicBindingInfo<T>
├── Component/
│   └── EventModule.cs             # 事件模块单例
└── SubscriberPriority.cs          # 优先级枚举（5 档）
```

详细文档见 [Documentation~/event-module.md](Documentation~/event-module.md)。

## 许可证

MIT
