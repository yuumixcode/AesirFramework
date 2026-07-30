# Aesir Inspector

Aesir Inspector 是一个 Unity/Tuanjie 编辑器扩展库，提供双语 Inspector UI、安全编辑器工具集、脚本文档生成器、XML Summary 同步工具等功能。可选集成 Odin Inspector 以获得增强的 Inspector 渲染和样式优化。

## 安装

### 通过 Package Manager (推荐)

1. 打开 Package Manager
2. 点击 `+` → `Add package from git URL...`
3. 输入：`https://github.com/yuumixcode/Unity-Aesir-Packages.git`

### 通过 Assets 目录

将 `Aesir Inspector` 文件夹放置到 `Assets/` 目录下任意位置。

## 快速开始

安装后，通过菜单栏 `Aesir Inspector` 访问所有功能：

| 菜单 | 功能 |
|------|------|
| Getting Started | 欢迎窗口 |
| Attribute Overview Pro | 特性总览（需 Odin Inspector） |
| Mini Tools | 迷你工具集 |
| Extension Package Manager | 扩展包管理器（需 Odin Inspector） |

## 核心功能

### 双语 Inspector

支持中英双语 Inspector 显示，通过 `AesirInspectorLanguageSettingsSO` 切换语言。提供双语 Odin 特性（如 `[BilingualTitle]`、`[BilingualButton]`、`[BilingualText]`）。

### 安全编辑器工具

所有 Runtime 工具类使用 `SafeEditorUtility` 模式，确保构建时自动剔除 Editor 代码。

### Script Doc Generator

反射生成 API 文档，增量更新，AI 友好 Markdown 输出。支持自定义文档格式策略。

### Summary Tool

右键菜单 XML `<summary>` ↔ `[Summary]` 双向同步，保持代码注释与 XML 文档一致。

### Odin Inspector 集成

安装 Odin Inspector 后自动启用增强功能：双语特性、Drawer、AttributeProcessor、Attribute Overview Pro、扩展包管理器等。

## 程序集

| 程序集 | 说明 | Odin 依赖 |
|--------|------|-----------|
| Runestone.AesirInspector | 核心运行时 | 无 |
| Runestone.AesirInspector.Editor | 核心编辑器 | 无 |
| Runestone.AesirInspector.OdinIntegration | Odin 运行时 | 需要 |
| Runestone.AesirInspector.OdinIntegration.Editor | Odin 编辑器 | 需要 |

## 系统要求

- Unity 2022.3 或 Tuanjie 引擎
- 可选：Odin Inspector

## 许可证

MIT License — 详见 [LICENSE.md](../LICENSE.md)
