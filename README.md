# Unity-Aesir-Packages · Aesir 系列 Unity 包集合

> 面向 [团结引擎](https://www.tuanjieengine.com/) / Unity 的渐进式架构框架 + UI 框架 + 编辑器工具集。三个子包可分别通过 Git URL 直接安装，按需选用。

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE)
[![Unity](https://img.shields.io/badge/Unity-2022.3%2B-black.svg)](https://unity.com/)
[![Tuanjie](https://img.shields.io/badge/Tuanjie-2022.3%2B-blueviolet.svg)](https://www.tuanjieengine.com/)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](./CONTRIBUTING.md)
[![Code of Conduct](https://img.shields.io/badge/Contributor%20Covenant-2.1-4baaaa.svg)](./CODE_OF_CONDUCT.md)
[![English](https://img.shields.io/badge/README-English-blue.svg)](./README.en.md)

---

## ✨ 三个子包，独立可装

> **关键点：所有子包都通过同一 Git 仓库发布。** Aesir Architecture 和 Aesir Inspector **互相独立**；Aesir Modules **同时依赖** Architecture 和 Inspector。

| 子包 | 用途 | 包名 | 版本 |
|---|---|---|---|
| **Aesir Architecture** | 渐进式 MVP / MVC 架构框架（能力接口组合、命令模式、事件总线、PlayerLoop） | `cn.runestone.aesir.architecture` | `0.4.2` |
| **Aesir Modules** | 极简 UI 框架（Manager of Managers、四层 Canvas、面板生命周期、可替换资源加载器） | `cn.runestone.aesir.modules` | `0.4.2` |
| **Aesir Inspector** | 编辑器扩展库（双语 Inspector、安全编辑器工具、脚本文档生成器、Summary 同步工具） | `cn.runestone.aesir.inspector` | `0.4.2` |

> 📝 **命名空间**：所有子包统一使用 `Runestone.*` 命名空间（品牌名"符文石"）。

---

## 🧩 依赖关系

```
            ┌──────────────────────────────────────┐
            │      Aesir Inspector（独立工具集）      │
            │  双语 UI / 文档生成器 / Summary 工具     │
            └──────────────┬───────────────────────┘
                           │ 依赖
                           ▼
┌──────────────────────┐    │    ┌──────────────────────────┐
│  Aesir Architecture  │◄───┴───►│  Aesir Inspector（独立）   │
│  MVP/MVC 基础框架     │   互相独立 │  编辑器扩展              │
│  能力接口 / 命令 / 事件 │         └──────────────────────────┘
└──────────────────────┘
            ▲
            │ 依赖
            │
┌──────────────────────┐
│   Aesir Modules      │ ─── 依赖 Architecture + Inspector
│   UI 框架             │
└──────────────────────┘
```

**关键约束**：

- **Aesir Architecture** — 不依赖任何 Aesir 子包，可独立安装
- **Aesir Inspector** — 不依赖任何 Aesir 子包，可独立安装
- **Aesir Modules** — 同时依赖 `cn.runestone.aesir.architecture` 和 `cn.runestone.aesir.inspector`

---

## 📦 安装方式

### 方式 1：通过 UPM Git URL 安装（推荐）

在 Unity Package Manager 窗口点击左上角 `+` → `Add package from git URL...`，填入：

```
https://github.com/yuumixcode/Unity-Aesir-Packages.git
```

按需安装——UPM 会自动识别你要用哪个子包（通过 `package.json` 的 `name` 字段）。

**只安装 Aesir Modules 时**，UPM 会自动解析依赖并拉取 Aesir Architecture + Aesir Inspector（因为 `package.json` 的 `dependencies` 字段里声明了）。

### 方式 2：通过 `manifest.json` 编辑安装

在项目的 `Packages/manifest.json` 文件中添加：

```json
{
  "dependencies": {
    "cn.runestone.aesir.architecture": "https://github.com/yuumixcode/Unity-Aesir-Packages.git",
    "cn.runestone.aesir.modules": "https://github.com/yuumixcode/Unity-Aesir-Packages.git",
    "cn.runestone.aesir.inspector": "https://github.com/yuumixcode/Unity-Aesir-Packages.git"
  }
}
```

只添加你需要的子包——三个包互相独立（除 Aesir Modules 自动依赖其他两个外）。

---

## 🚀 快速开始

> 选一个你感兴趣的子包深入看，下面只是"嗅觉测试"。

### Aesir Architecture — 3 行起一个 Context

```csharp
using Runestone.AesirArchitecture;

public class CounterContext : AbstractContext<CounterContext>
{
    protected override void Configure() => RegisterModel<ICounterModel>(new CounterModel());
}
```

完整指南见 [`Assets/Runestone/AesirArchitecture/README.md`](./Assets/Runestone/AesirArchitecture/README.md)（中文）/ [`Documentation~/README_EN.md`](./Assets/Runestone/AesirArchitecture/Documentation~/README_EN.md)（English）。

### Aesir Modules — 一个 `Open<T>()` 打开面板

```csharp
UIManager.RegisterPrefab<MainMenuPanel>(prefab);
UIManager.Open<MainMenuPanel>();
UIManager.Open<ConfirmDialogPanel>(new ConfirmData { message = "确定？" });
```

完整指南见 [`Assets/Runestone/AesirModules/README.md`](./Assets/Runestone/AesirModules/README.md)（中文）/ [`Documentation~/README_EN.md`](./Assets/Runestone/AesirModules/Documentation~/README_EN.md)（English）。

### Aesir Inspector — 右键即可同步 XML Summary

在 Project 窗口选中脚本 → 右键 → `Aesir → Summary Tool` → 选择 Sync / Replace / Remove。

完整指南见 [`Assets/Runestone/AesirInspector/README.md`](./Assets/Runestone/AesirInspector/README.md)（中文）/ [`Documentation~/README_EN.md`](./Assets/Runestone/AesirInspector/Documentation~/README_EN.md)（English）。

---

## 🗂️ 仓库目录结构

> 这是一个**多包 monorepo**——三个子包的源都在这里，但每个子包都能独立通过 Git URL 安装。

```
Unity-Aesir-Packages/                       # 你现在看到的仓库
├── README.md                              # 本文件（中文）
├── README.en.md                           # English version
├── LICENSE                                # MIT
├── CHANGELOG.md                           # 仓库级别变更日志（聚合三个子包）
├── CONTRIBUTING.md                        # 贡献指南
├── CODE_OF_CONDUCT.md                     # 社区准则
├── AGENTS.md                              # Agent 协作说明
├── CODELY.md                              # 架构详细文档
└── Assets/Runestone/
    ├── AesirArchitecture/                 # 不依赖其他 Aesir 子包
    │   ├── Runtime/  Editor/  Tests/  Samples~/  Documentation~/
    │   ├── README.md  CHANGELOG.md  LICENSE.md  package.json
    ├── AesirModules/                      # 依赖 Architecture + Inspector
    │   ├── Runtime/  Editor/  Documentation~/
    │   ├── README.md  CHANGELOG.md  LICENSE.md  package.json
    └── AesirInspector/                    # 不依赖其他 Aesir 子包
        ├── Runtime/  Editor/  Tests/  Samples~/  Documentation~/
        ├── README.md  CHANGELOG.md  CONTRIBUTING.md  LICENSE.md  package.json
```

---

## 🛠️ 开发环境

> - **Unity / Tuanjie**: 2022.3.62f3c1（或等价 LTS 版本）
> - **渲染管线**: URP 14.0.12
> - **依赖（可选）**: [Odin Inspector](https://odininspector.com/) 3.3.x+ — Aesir Architecture / Modules / Inspector 都通过 `#if ODIN_INSPECTOR` 条件编译，未安装时自动跳过 Odin 相关功能

首次打开项目时，Unity 会自动从 `Packages/manifest.json` 解析依赖。

### 不想开 GUI 预热包缓存

```bash
Unity -batchmode -quit -projectPath . -nographics -logFile /dev/null
```

### CLI 测试

```bash
# Edit 模式
Unity -batchmode -quit -projectPath . \
       -testPlatform editmode -runTests \
       -testResults TestResults.xml -logFile test.log

# Play 模式 — 把 editmode 换成 playmode
```

详细构建与测试流程见 [`AGENTS.md`](./AGENTS.md)。

---

## 🤝 贡献

欢迎任何形式的贡献——Bug 报告、功能建议、文档改进、代码提交。

- 阅读 [`CONTRIBUTING.md`](./CONTRIBUTING.md) 了解提交流程
- 遵循 [`CODE_OF_CONDUCT.md`](./CODE_OF_CONDUCT.md)
- 推荐从 `main` 拉分支；提交信息遵循 [Conventional Commits](https://www.conventionalcommits.org/)

子包 Aesir Inspector 单独有一份更详细的 `CONTRIBUTING.md`（含编码规范细节）：
[`Assets/Runestone/AesirInspector/CONTRIBUTING.md`](./Assets/Runestone/AesirInspector/CONTRIBUTING.md)。

---

## 📄 许可证

本仓库及三个子包均采用 **MIT License** 开源。

```
MIT License

Copyright (c) 2026 Yuumix
```

详见根目录 [`LICENSE`](./LICENSE) 与各子包 `LICENSE.md`。

---

## 🔗 链接

> - **作者主页**: [yuumixcode](https://github.com/yuumixcode)
> - **团结引擎**: [tuanjieengine.com](https://www.tuanjieengine.com/)
