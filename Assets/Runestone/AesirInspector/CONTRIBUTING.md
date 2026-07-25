# 贡献指南

感谢你对 Aesir Inspector 的关注！我们欢迎任何形式的贡献，包括但不限于 Bug 报告、功能建议、文档改进和代码贡献。

[English](./CONTRIBUTING.en.md)

---

## 目录

- [行为准则](#行为准则)
- [如何贡献](#如何贡献)
- [开发环境搭建](#开发环境搭建)
- [项目结构](#项目结构)
- [编码规范](#编码规范)
- [提交 Pull Request](#提交-pull-request)
- [问题与帮助](#问题与帮助)

## 行为准则

本项目采用 [Contributor Covenant](https://www.contributor-covenant.org/version/2/1/code_of_conduct/) 行为准则。参与本项目即表示你同意遵守其条款。请以尊重和建设性的方式对待每一位社区成员。

## 如何贡献

### 报告 Bug

1. 在 [Issues](https://github.com/yuumixcode/Unity-Aesir-Packages/issues) 中搜索是否已有相同问题。
2. 如果没有，[创建新 Issue](https://github.com/yuumixcode/Unity-Aesir-Packages/issues/new)，包含以下信息：
   - **复现步骤**：详细描述如何触发该 Bug。
   - **预期行为**：你期望发生什么。
   - **实际行为**：实际发生了什么。
   - **环境信息**：Unity/Tuanjie 版本、Odin Inspector 版本（如适用）、操作系统。
   - **截图/日志**：如有可能，附上截图或错误日志。

### 建议功能

1. 在 [Issues](https://github.com/yuumixcode/Unity-Aesir-Packages/issues) 中搜索是否已有类似建议。
2. 如果没有，创建新 Issue 并使用 `Feature Request` 标签，包含以下信息：
   - **使用场景**：描述你希望解决的具体问题。
   - **建议方案**：描述你期望的解决方案。
   - **备选方案**：你考虑过的其他方案。

### 贡献代码

1. Fork 本仓库。
2. 基于仓库根目录下的 `Assets/aesir-inspector/` 进行开发。
3. 遵循下方 [编码规范](#编码规范)。
4. 提交 Pull Request 到 `main` 分支。

## 开发环境搭建

### 前置要求

- **Tuanjie Editor** 2022.3 或更高版本（Unity 2022.3 fork）
- **Git**：用于版本控制
- **Odin Inspector** 3.3.x 或更高（可选依赖，用于开发 OdinIntegration 增强功能）

### 克隆项目

```bash
git clone https://github.com/yuumixcode/Unity-Aesir-Packages.git
```

将克隆的仓库放入 Tuanjie 项目的 `Assets/` 目录下，或通过 Package Manager 以本地包方式引用。

### Odin Inspector 集成

Odin Inspector 是可选依赖：

- **不安装 Odin**：核心功能正常编译运行，OdinIntegration 程序集会被自动跳过。
- **安装 Odin**：导入后会自动添加 `ODIN_INSPECTOR` 编译符号，启用 OdinIntegration 增强程序集。

## 项目结构

```
Aesir Inspector/
├── Runtime/
│   ├── Unity/                     # 核心运行时 (Runestone.AesirInspector)
│   │   ├── Attributes/            # [Summary] 等自定义特性
│   │   ├── Core/                   # Version, Paths, WebLinks
│   │   ├── Inspector/              # Inspector 显示模型
│   │   ├── Localization/           # 本地化数据与语言设置
│   │   ├── Logging/               # 日志系统
│   │   ├── OdinBridge/            # IOdinBridge 桥接层
│   │   ├── ScriptDocGenerator/    # 文档生成器运行时模型
│   │   └── Utilities/             # 安全编辑器工具集
│   └── Odin Integration/          # Odin 运行时 (ODIN_INSPECTOR)
│       ├── Attributes/            # 双语特性
│       └── OdinCodeHighlighter.cs
├── Editor/
│   ├── Unity/                     # 核心编辑器 (Runestone.AesirInspector.Editor)
│   │   ├── Core/                  # 安装检测、菜单管理
│   │   ├── MiniTools/             # QuickCreateSO
│   │   └── SummaryTool/           # XML Summary 同步
│   └── Odin Integration/          # Odin 编辑器 (ODIN_INSPECTOR)
│       ├── AttributeOverviewPro/  # 特性总览窗口
│       ├── AttributeProcessors/   # OdinAttributeProcessor
│       ├── Bridge/                # OdinInspectorBridge
│       ├── Drawers/               # 双语 Drawer
│       ├── ExtensionManager/      # 扩展包管理器
│       ├── MiniTools/             # MenuItem Viewer, Syntax Highlighter
│       ├── ScriptDocGenerator/    # 文档生成器编辑器逻辑
│       └── Windows/               # Getting Started, Preferences
├── Tests/
│   ├── Editor/                    # 编辑器模式测试
│   └── Runtime/                   # 运行时模式测试
├── Samples~/                      # 使用示例
└── Documentation~/                # 用户文档与开发者指南
```

### 程序集说明

| 程序集 | Odin 依赖 | 说明 |
|--------|-----------|------|
| `Runestone.AesirInspector` | 无 | 核心运行时，不允许引用 Odin API |
| `Runestone.AesirInspector.Editor` | 无 | 核心编辑器 |
| `Runestone.AesirInspector.OdinIntegration` | `ODIN_INSPECTOR` | Odin 运行时桥接 |
| `Runestone.AesirInspector.OdinIntegration.Editor` | `ODIN_INSPECTOR` | Odin 编辑器增强 |

## 编码规范

请务必在提交代码前阅读并遵循以下规范。详细规范参见 `Runtime/Unity/CodeStyle/AesirInspectorCodeStyle.cs` 和 `Documentation~/development.md`。

### 注释规范

本项目采用**自文档化代码**和**无注释范式**：

- **禁止 XML 注释**：不使用 `/// <summary>`、`/// <param>` 等 XML 文档注释。
- **命名即文档**：通过清晰的命名传达意图，无需额外注释。
- **`[Summary]` 仅用于复杂逻辑**：只在命名无法充分表达意图时使用 `[Summary("...")]`，解释"为什么"而非"做了什么"。

```csharp
// ✅ 自文档化：命名清晰，无需注释
public int MaxRetryCount { get; }
public void ApplyDamage(float amount) { }

// ✅ [Summary] 解释"为什么"
[Summary("后者覆盖前者，用于多配置源优先级合并")]
public void MergeConfigSources(IReadOnlyList<ConfigSource> sources) { }

// ❌ 禁止 XML 注释
/// <summary>
/// 应用伤害值
/// </summary>
public void ApplyDamage(float amount) { }
```

### 命名规范

| 标识符 | 规则 | 示例 |
|--------|------|------|
| 类、接口 | `PascalCase`，接口 `I` 前缀 | `PlayerManager`, `IDamageable` |
| 私有非序列化字段 | `_camelCase` | `_health` |
| 序列化字段 `[SerializeField]` | `camelCase` | `moveSpeed` |
| 常量 / 静态只读 | `PascalCase` | `MaxScore` |

### Unity/C# 关键规则

- **严禁**对 `UnityEngine.Object` 派生类使用 `?.` / `??`。
- 私有方法对应公开方法时，增加 `Internal_` 前缀。
- `#if UNITY_EDITOR` 包裹编辑器专用代码。
- Odin 依赖代码**必须**放在 `Odin Integration/` 子目录。
- 核心程序集**不允许**直接引用 Odin API — 通过 `IOdinBridge` 桥接。

### 事件规范

| 角色 | 命名 | 示例 |
|------|------|------|
| 事件 | 无 `On` 前缀 | `DoorOpened` |
| 订阅方法 | `On` + 事件名 | `OnDoorOpened` |
| 触发方法 | `Raise` + 事件名 | `RaiseDoorOpened` |

### Enum 规范

- 普通：含 `None = 0`，显式赋值。
- Flags：`[Flags]`，值为 `1 << n`，复合用 `|`。

### Utility 命名

| 类别 | 命名规则 | 目录 |
|------|----------|------|
| Runtime | `XxxUtility` | `Runtime/Unity/Utilities/` |
| Editor 安全封装 | `XxxSafeEditorUtility` | `Runtime/Unity/Utilities/` |
| Editor-Only | `XxxEditorUtility` | `Editor/Unity/` |

## 提交 Pull Request

### 流程

1. 确保存在对应的 Issue（Bug 修复或功能建议），如没有请先创建。
2. Fork 仓库并从 `main` 创建特性分支：
   ```bash
   git checkout -b feature/your-feature-name
   # 或
   git checkout -b fix/your-bug-fix-name
   ```
3. 编写代码并确保遵循编码规范。
4. 添加必要的单元测试（测试位于 `Tests/Editor/` 和 `Tests/Runtime/`）。
5. 确保所有现有测试通过。
6. 提交更改，提交信息使用现在时态，简洁描述变更：
   ```
   Add bilingual attribute processor for Button
   Fix null reference in OdinBridgeLocator
   ```
7. 推送到你的 Fork 并创建 Pull Request。
8. PR 描述中引用相关 Issue（如 `Closes #123`）。

### 分支命名

| 类型 | 格式 | 示例 |
|------|------|------|
| 功能 | `feature/<name>` | `feature/bilingual-toggle` |
| 修复 | `fix/<name>` | `fix/odin-bridge-null-ref` |
| 文档 | `docs/<name>` | `docs/update-contributing-guide` |

### PR 检查清单

提交 PR 前，请确认：

- [ ] 代码遵循项目编码规范
- [ ] 未引入 XML 文档注释（使用 `[Summary]` 或自文档化命名替代）
- [ ] 未对 `UnityEngine.Object` 派生类使用 `?.` / `??`
- [ ] 编辑器专用代码已用 `#if UNITY_EDITOR` 包裹
- [ ] Odin 依赖代码放置在 `Odin Integration/` 子目录
- [ ] 核心程序集未直接引用 Odin API
- [ ] 已添加必要的单元测试
- [ ] 所有测试通过
- [ ] 提交信息简洁且使用现在时态

## 问题与帮助

- **Bug 报告 & 功能建议**：[GitHub Issues](https://github.com/yuumixcode/Unity-Aesir-Packages/issues)
- **讨论 & 提问**：[GitHub Discussions](https://github.com/yuumixcode/Unity-Aesir-Packages/discussions)
- **邮件**：zeriying@gmail.com

---

感谢你的贡献！每一次提交都让 Aesir Inspector 变得更好。
