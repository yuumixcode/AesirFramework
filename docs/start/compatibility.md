---
description: "AesirFramework 的引擎版本、渲染管线、可选依赖(Odin / Addressables / Input System)与运行时约定。"
---

# 兼容性

## 引擎与运行时

| 项目 | 支持范围 |
| --- | --- |
| Unity / 团结引擎 | **2022.3+**(开发与验证环境:2022.3.62f3c1) |
| 团结引擎 | 一等公民支持,与 Unity 同版本号等效验证 |
| .NET | Unity 2022.3 默认 profile(.NET Standard 2.1 / 4.x API 兼容级) |
| 渲染管线 | RAA 与管线无关(纯 C# + MonoBehaviour 适配层);RAM 的 UI 使用 uGUI。内置管线 / URP / HDRP 均可 |
| 平台 | 不涉及平台特定 API;IL2CPP 与 Mono 均可 |

## 包依赖

| 依赖 | 必需性 | 说明 |
| --- | --- | --- |
| `cn.runestone.aesir.architecture` | RAM **必需** | 两包同号发版,推荐同版本 |
| `com.unity.test-framework` | 可选 | 只有运行包内测试才需要(本仓 1.1.33);包不再声明该依赖 |
| 第三方库 | 无 | 核心架构零第三方依赖 |

!!! note "包内不声明依赖"

    Unity Package Manager 不接受 **包内** `package.json` 的 Git URL 依赖,只有项目 `manifest.json` 可以声明。因此 RAM 不声明对 RAA 的依赖,由使用者的安装动作或 `Tools → Aesir → Modules → Install Dependencies` 补全。

## 可选集成

三个可选能力都遵循同一条原则:**未安装时不报错、自动隐藏**。

<div class="aesir-package-grid" markdown>

<div class="aesir-package-card" markdown>

<span class="aesir-card__eyebrow">可选 · ODIN</span>

### Odin Inspector 3.3.x+

- **不装 Odin**:核心流程完整可用 —— Context 注册 → Model/Service 初始化 → Command/Query → ObservableValue 通知。使用 `MonoView<T>` / `MonoViewController<T>` 系列
- **装了 Odin**:自动启用 `ODIN_INSPECTOR` 集成 —— DDOL 开关信息框、`ObservableValue` Drawer、面板与模块的 AttributeProcessor 样式注入、Binder 组件绑定、脚本文档生成模块
- 集成代码隔离在独立 `OdinInspector/` 程序集 + 条件编译,不影响核心程序集

</div>

<div class="aesir-package-card" markdown>

<span class="aesir-card__eyebrow">可选 · ADDRESSABLES / INPUT</span>

### Addressables 与 Input System

- **Addressables**(`AESIR_MODULES_ADDRESSABLES`):`SceneAssetWrapper` 的地址查询能力经胶水程序集 + defineConstraints 条件编译,装包即启用,卸包后 API 自动隐藏。UI 资源加载默认走 Resources,需要时实现 `IUIAssetLoader` 替换
- **Input System**(`ENABLE_INPUT_SYSTEM`):独立程序集,启用后自动以 `InputSystemUIInputModule` 替换 `UIRoot` 的默认输入模块

</div>

</div>

## 运行时约定

- **主线程**:所有框架类型只保证主线程使用。`Task.Run` 等异步回调请先调度回主线程再访问框架
- **Domain Reload 安全**:静态变量全部显式重置 —— 非泛型单例在类内 `[RuntimeInitializeOnLoadMethod]` 自重置,泛型类经 `ResetStaticsAssistant` 注册。`Enter Play Mode Options`(关闭 Domain Reload)可安全启用
- **异常语义**:框架 fail-fast,异常消息内含修复指引,不吞噬监听回调异常

## 示例与构建

- 示例程序集是**运行时程序集 + 整文件 `#if UNITY_EDITOR`**:编辑器内可编译、可挂载、可 Play;玩家构建时整体剔除,示例类型 0 入包
- 构建时 `AesirSamplesBuildFilter` 会把示例场景从**本次构建选项**中剔除(不改 Build Settings 持久数据)
- 包内 `Documentation~/`(使用文档)与 `TuanjieCowork~/`(技能获取入口)以 `~` 结尾,对 AssetDatabase 不可见、不进 unitypackage;`Samples~` 是 Package Manager 导入示例的唯一来源

## 相关

- [安装与升级](installation.md)
- [常见问题](../faq.md)
