# Aesir Modules

Aesir Architecture (RAA) 的功能模块包。当前提供极简 UI 框架，采用 Unity "Manager of Managers" 模式，按 Engine / Component 两层组织。

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE.md)
[![Version](https://img.shields.io/badge/version-0.4.0-blue.svg)](./CHANGELOG.md)
[![Unity](https://img.shields.io/badge/Unity-2022.3%2B-black.svg)](https://unity.com/)
[![Install via Git URL](https://img.shields.io/badge/UPM-Git%20URL-blueviolet.svg)](#安装)
[![English](https://img.shields.io/badge/README-English-blue.svg)](./README.en.md)

> 📦 **本包是 [Unity-Aesir-Packages](https://github.com/yuumixcode/Unity-Aesir-Packages) monorepo 的一部分**。本包**同时依赖**：
> - **[Aesir Architecture](https://github.com/yuumixcode/Unity-Aesir-Packages)**（`>= 0.4.0`）
> - **[Aesir Inspector](https://github.com/yuumixcode/Unity-Aesir-Packages)**（`>= 0.4.0`）

## 模块

| 模块 | 状态 | 说明 |
|------|------|------|
| UI | 已实现 | UIManager 单例；四层 Canvas + 面板生命周期 |
| Scene | 规划中 | 场景加载、SceneReference |

## 依赖

- **Aesir Architecture (RAA)** `cn.runestone.aesir.architecture` >= 0.4.0（必需）
- **Aesir Inspector** `cn.runestone.aesir.inspector` >= 0.4.0（必需）
- **Odin Inspector**（可选）：仅通过 `#if ODIN_INSPECTOR` 条件编译参与，未导入时自动排除。

## 安装

通过 Unity Package Manager 添加包：

```
Packages/manifest.json -> dependencies:
"cn.runestone.aesir.modules": "https://github.com/yuumixcode/Unity-Aesir-Packages.git"
```

UPM 会自动解析 `package.json` 的 `dependencies` 字段，拉取 Aesir Architecture + Aesir Inspector。

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

## 许可证

MIT
