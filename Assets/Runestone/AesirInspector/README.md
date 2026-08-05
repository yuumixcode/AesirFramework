# Aesir Inspector

[English](Documentation~/README_EN.md) | [![license](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE.md)
[![Version](https://img.shields.io/badge/version-0.6.0-blue.svg)](CHANGELOG.md)
[![Install via Git URL](https://img.shields.io/badge/UPM-Git%20URL-blueviolet.svg)](#安装说明)

> 📦 **本包是 [Unity-Aesir-Packages](https://github.com/yuumixcode/Unity-Aesir-Packages) monorepo 的一部分**。本包**不依赖**其他 Aesir 子包（独立可装）。
>
> ⚠️ **强依赖 [Odin Inspector](https://odininspector.com/)**：本包需要 Odin Inspector 才能正常编译和运行。请确保项目中已安装 Odin Inspector 3.3.x+。
>
> 关联包：
> - **[Aesir Architecture](https://github.com/yuumixcode/Unity-Aesir-Packages)**（独立）
> - **[Aesir Modules](https://github.com/yuumixcode/Unity-Aesir-Packages)**（依赖 Architecture）

`Aesir Inspector` 是一个 Unity 编辑器扩展库，旨在提供双语 Inspector UI、安全编辑器工具集、脚本文档生成器等功能。基于 Odin Inspector 提供增强的 Inspector 渲染和样式优化。

## 适用人群

- **编辑器工具开发者**：正在开发自定义 Inspector 工具，需要双语（中/英）UI 显示支持的开发者。
- **跨国/跨地区协作团队**：团队成员语言背景不同，需要在 Inspector 面板中同时展示中英文信息以降低沟通成本。
- **Unity 编辑器用户**：希望获得安全编辑器工具、文档生成器、Summary 同步工具等实用功能的开发者，无需安装 Odin Inspector。
- **Odin Inspector 用户**：已有 Odin Inspector 并希望获得更丰富的属性装饰器与增强 Inspector 体验的开发者。
- **代码规范倡导者**：希望团队遵循统一的代码风格与注释标准，提升项目可维护性。

## 安装说明

### 通过 Git URL 安装

1. 打开 Unity Package Manager 窗口。
2. 点击左上角的 `+` 按钮，选择 `Add package from git URL...`。
3. 输入以下地址：
   ```
   https://github.com/yuumixcode/Unity-Aesir-Packages.git?path=Assets/Runestone/AesirInspector
   ```

### 通过 manifest.json 安装

在项目的 `Packages/manifest.json` 文件中添加：

```json
{
  "dependencies": {
    "cn.runestone.aesir-inspector": "https://github.com/yuumixcode/Unity-Aesir-Packages.git?path=Assets/Runestone/AesirInspector"
  }
}
```

### 安装方式检测

Aesir Inspector 会在编辑器加载时自动检测安装方式（UPM / Assets 目录），并通过 `AesirInspectorInstallationChecker` 暴露静态属性：

- `InstallMode`：当前安装方式（`Upm` / `AssetFolder` / `Unknown`）。
- `IsUpm`：是否通过 UPM 安装。
- `IsAssetFolder`：是否安装在 Assets 目录中（Asset Store 导入或 Git 子模块）。

## 环境依赖

- **Unity**: 2022.3.2t3 (Tuanjie) 或更高版本。
- **Odin Inspector**: 3.3.x 或更高版本（可选依赖；导入后会自动添加 `ODIN_INSPECTOR` 编译符号，启用 OdinIntegration 增强程序集）。

## 核心功能

> **📌 提示**：标注 ⚡ 的功能需要安装 Odin Inspector。

### 1. 特性总览 (Attribute Overview Pro) ⚡

以可搜索的树形菜单展示所有已注册的 Odin Inspector 与 Aesir Inspector 特性面板，每个特性提供实时预览与示例代码。

- **分类浏览**：按 Essentials / Buttons / Collections / Groups / Conditionals 等分类浏览特性。
- **搜索定位**：支持模糊搜索，快速找到目标特性。
- **实时预览**：选中特性即可在右侧面板查看效果与参数配置。
- **代码预览**：选中特性即可查看对应的示例源代码，快速了解用法。
- 通过 `Tools → Aesir → Inspector → Attribute Overview Pro` 菜单打开。

### 2. 脚本文档生成器 (Script Doc Generator) ⚡

通过反射分析 C# 类型信息，生成结构化的 API 文档，支持增量生成与个性化扩展。

#### 使用场景

当你的团队、开源项目或个人项目需要一份 API 文档时——不仅需要自动生成的类型签名，还需要针对 API 补充个性化说明（使用示例、注意事项、业务上下文等）——Script Doc Generator 正是为此而设计的。它负责生成准确的 API 签名部分，你来补充那部分只有人才写得出的内容，两者互不干扰，增量更新。

#### 核心优势

- **🔒 完全离线**：基于 C# 反射机制运行，无需网络连接、无需外部 API、无需第三方服务。断网环境、内网开发、机密项目——随时随地可用。
- **⚡ 零等待**：反射分析在毫秒级完成。选中类型，文档即现。没有进度条，没有等待。
- **🎮 与 Unity 一体**：作为 Editor 原生扩展运行，直接在 Inspector 中操作。无需切换窗口、无需外部工具链——文档就在你编写代码的地方。
- **✏️ 增量生成**：重新生成文档时，自动保留 `## 额外说明` 标识符之后的手写内容，已有 Front Matter（YAML/TOML 头部）也会保留。自动生成的签名与人工补充的说明互不覆盖。
- **🤖 AI 友好**：默认生成 Markdown 格式文档，结构清晰、语义明确，可直接用于构建 AI API 问答知识库（RAG、Embedding 等），让 AI 助手精准回答项目 API 相关问题。
- **🔧 可配置与可扩展**：提供多种配置项与扩展接口，适配不同项目的文档需求（详见下方）。

#### 功能详情

- **类型分析**：支持类、结构体、接口、枚举、委托、Record 等类型的完整签名与特性解析，包含泛型约束、基类继承与接口实现。
- **字段解析**：覆盖全部 C# 原始类型、集合类型、委托类型、特殊类型（abstract/dynamic/interface/nullable），以及 const/static 默认值、访问修饰符、复合关键字（const/static readonly/readonly）、Unity 内置类型与特性标注。
- **属性解析**：支持 getter/setter 不对称访问修饰符（如 `public get / private set`）、静态属性、默认值初始化。
- **方法解析**：支持泛型方法、参数默认值、`params` 可变参数、`async` 异步方法、运算符重载、扩展方法。
- **继承分析**：识别 virtual/abstract/override 方法与接口实现，追踪继承链来源。
- **辅助功能**：成员排序（`DerivedMemberDataComparer`）、方法重载标记、构造方法签名生成、事件签名生成。

#### 可配置项

| 配置项 | 说明 |
|-------|------|
| 文档输出路径 | 自定义文档生成的目标文件夹，支持拖拽设置 |
| 按命名空间生成文件夹 | 开启后按类型的命名空间自动创建子目录，如 `Runestone.AesirInspector` → `Runestone/AesirInspector/` |
| 自定义文档扩展名 | 默认 `.md`，可切换为 `.mdx`、`.txt` 等任意扩展名 |
| 增量生成标识符 | 开启后自动在文档末尾插入 `## 额外说明` 段落，重新生成时保留该段落之后的手写内容 |
| 类型来源模式 | 单类型 / 多类型 / 整个程序集，三种粒度按需选择 |
| TypesCacheSO | 将 Type 列表保存为可复用的资源文件，避免每次重新选择 |

#### 可扩展接口

| 接口 | 说明 |
|------|------|
| `DocGeneratorSettingsSO` | 继承此抽象类并实现 `GetGeneratedDoc(ITypeData)` 方法，即可自定义文档的格式与内容。内置了 `CnScriptingAPISettingsSO`（中文 API Markdown 文档生成器）作为参考实现 |
| `IAnalysisDataFactory` | 替换整个类型分析工厂，自定义成员数据的解析逻辑 |
| `IAttributeFilter` | 自定义特性过滤器，控制哪些特性出现在生成的文档中 |

#### 单元测试覆盖

Script Doc Generator 目前已包含 **153 个单元测试**，覆盖各数据类型的签名生成功能。测试用例会随着功能迭代持续补充：

| 测试模块 | 测试数 | 说明 |
|---------|-------|------|
| **FieldData** · 签名 | 41 | 原始类型、集合类型、委托类型、特殊类型（abstract/dynamic/interface/nullable）的 Signature 生成 |
| **FieldData** · 默认值 | 32 | const 常量与 static 静态字段的默认值生成，含 decimal 边界情况验证 |
| **FieldData** · 修饰符 | 10 | 复合关键字（const/static readonly/readonly）与全部 6 种访问修饰符 |
| **FieldData** · Unity | 7 | Unity 内置类型（GameObject/Transform/Rigidbody 等）与特性标注（SerializeField/Range/ColorUsage/Obsolete） |
| **PropertyData** | 13 | 静态属性默认值、getter/setter 不对称访问修饰符组合 |
| **MethodData** · 通用 | 11 | 泛型方法、默认参数、params 可变参数、async 异步方法、静态方法 |
| **MethodData** · 继承 | 5 | virtual/abstract/override 方法与接口实现的继承分析 |
| **MethodData** · 运算符 | 8 | 算术运算符重载、隐式/显式类型转换运算符 |
| **MethodData** · 扩展 | 1 | 扩展方法的签名生成与 `[Ext]` 标记 |
| **ConstructorData** | 1 | 构造方法签名生成，含基类构造调用 |
| **EventData** | 6 | Action/Func/Predicate/Comparison 等委托类型事件及静态事件 |
| **TypeData** | 14 | class/struct/interface/enum/delegate/record/static/sealed/generic 等类型声明，含特性与泛型约束 |
| **MemberData** · 继承 | 4 | 字段/属性/事件/方法从基类继承的 `IsFromInheritance` 标记 |

### 3. Summary 工具 (Summary Tool)

通过右键菜单快捷处理 C# 脚本中的 XML `<summary>` 注释与 `[Summary]` 特性之间的双向同步。

#### 使用场景

当你的团队要求公共成员同时具备 XML 文档注释和 `[Summary]` 特性时——手动维护两份内容相同但格式不同的注释既繁琐又容易遗漏。Summary Tool 正是为此而设计的。它从 XML 注释中提取摘要，自动生成对应的 `[Summary]` 特性，确保两者始终保持同步。

#### 核心优势

- **⚡ 右键即用**：在 Project 窗口选中脚本，右键即可执行，无需打开额外窗口。
- **🔄 三种模式**：同步（Sync）、替换（Replace）、移除（Remove），覆盖日常维护的全部需求。
- **📦 批量处理**：支持多选脚本同时处理，批量同步或清理。
- **🧠 智能导入**：处理完成后自动添加 `using Runestone.AesirInspector;`，无需手动补引用。
- **🏗️ 宏定义感知**：自动识别 `#if` 等预处理指令，确保 `[Summary]` 特性插入在条件编译块内部。

#### 工作原理

`XmlSummaryTool` 的处理流程分为三个阶段：**解析 → 分组 → 输出**。

**1. 解析阶段**：将源代码按行扫描，定位第一个 `///` 注释，将其之前的所有行标记为 **Header**（using、namespace 等），之后的部分进入分组阶段。

**2. 分组阶段**：从第一个 `///` 开始，交替提取 **XML 注释块**（连续的 `///` 行）和 **代码块**（非 `///` 行），生成 `XmlCodePart` 列表。每个 `XmlCodePart` 由 `xml`（注释部分）和 `code`（代码部分）组成。

**3. 输出阶段**：根据选择的模式，对每个 `XmlCodePart` 执行不同的操作：

| 模式 | 输出组合 | 说明 |
|------|---------|------|
| **Sync** | `xml` + `前导预处理` + `[Summary]` + `删除首个[Summary]后的code` | 保留 XML 注释，在预处理指令之后添加/更新 `[Summary]`；若已有 `[Summary]` 则替换为 XML 中的内容 |
| **Replace** | `移除summary后的xml` + `前导预处理` + `[Summary]` + `删除首个[Summary]后的code` | 移除 `<summary>` 标签，用 `[Summary]` 特性替代；已有 `[Summary]` 的内容同步为 XML 中的文本 |
| **Remove** | `xml` + `前导预处理` + `删除所有[Summary]后的code` | 仅移除所有 `[Summary]` 特性，保留 XML 注释 |

**宏定义感知**：当代码块以 `#if`、`#elif`、`#else` 等预处理指令开头时，`[Summary]` 特性会插入在这些指令之后（即条件编译块内部），而非之前。例如：

```csharp
// 输入
/// <summary>编辑器方法</summary>
#if UNITY_EDITOR
[Summary("旧内容")]
public void Reset() { }
#endif

// Sync 输出 — [Summary] 在 #if 内部
/// <summary>编辑器方法</summary>
#if UNITY_EDITOR
[Summary("编辑器方法")]
public void Reset() { }
#endif
```

最后，输出阶段会自动检测 Header 中是否已包含 `using Runestone.AesirInspector;`，若未包含则自动添加。

### 4. 迷你工具集 (Mini Tools) ⚡

整合常用编辑器小工具，通过 `Tools → Aesir → Inspector → Mini Tools` 菜单打开统一窗口。

| 工具 | 说明 |
|------|------|
| **MenuItem Viewer** | 搜集并展示项目中所有 `[MenuItem]` 菜单项信息，支持按程序集过滤、搜索，便于规划菜单结构 |
| **Syntax Highlighter** | 基于 Odin 内置语法高亮处理器的可视化面板，输入源码即可测试高亮效果并输出富文本标记 |
| **Quick Create SO** | 在 Project 窗口右键 MonoScript 即可快速生成 ScriptableObject 资源文件，支持多选批量创建 |

### 5. 扩展包管理器 (Extension Package Manager) ⚡

快捷安装推荐的 Aesir 系列和其他常用开源 Unity Packages，基于 Git URL 方式。

- **一键安装/移除**：卡片式 UI 展示推荐包的安装状态，点击即可安装或移除。
- **自动检测**：打开窗口时自动检测已安装包的状态，安装/移除后实时刷新。
- 通过 `Tools → Aesir → Inspector → Extension Package Manager` 菜单打开。

## 基础设施

### 6. 双语 UI 特性 (Bilingual Attributes) ⚡

提供了一套完整的双语属性装饰器与 Inspector Control，支持在 Inspector 面板中同时显示中文和英文信息。主要面向以下场景：

- **编辑器工具开发**：当你在开发其他编辑器工具时，希望 Inspector 界面支持中英双语显示，让不同语言背景的用户都能直观理解各项参数与操作。
- **团队协作**：跨地区、跨语言的团队在共享项目时，双语显示可有效降低沟通成本，避免因语言差异导致的误操作。

可用装饰器与 Control：

- `[BilingualTitle]`
- `[BilingualButton]`
- `[BilingualInfoBox]`
- `[BilingualText]`
- `BilingualDisplayAsStringControl` 双语只读文本显示控件
- `BilingualHeaderControl` 双语头部信息控件
- `HorizontalSeparateControl` 水平分隔线控件

### 7. OdinBridge 桥接层

提供 Odin Inspector 可选集成机制，使核心程序集不依赖 Odin，同时允许 OdinIntegration 程序集在 Odin 可用时提供增强功能：

| 类 | 说明 |
|----|------|
| `IOdinBridge` | Odin 可用性查询接口，定义 `IsOdinPresent` 等能力 |
| `DefaultOdinBridge` | 无 Odin 时的默认桥接实现 |
| `OdinBridgeLocator` | 运行时自动定位 Odin 桥接，无 Odin 时回退至 `DefaultOdinBridge` |
| `OdinInspectorBridge` | Odin 可用时提供的编辑器侧增强桥接实现 |

### 8. 安全编辑器工具 (Safe Editor Utilities)

针对 Unity Editor API 进行了安全封装，确保编辑器专用代码在打包后自动剔除。

| 工具类 | 说明 |
|-------|------|
| `ScriptableObjectSafeEditorUtility` | 提供更可靠的 ScriptableObject 资产创建与管理 |
| `MonoScriptSafeEditorUtility` | 根据脚本名称查找、选择 MonoScript 资源 |
| `PathUtility` | 路径字符串工具：Unity 路径规范化、子路径提取、路径合并 |
| `PathSafeEditorUtility` | 确保 Assets 目录下文件夹存在的安全创建工具 |
| `HierarchySafeEditorUtility` | 获取 GameObject 在 Hierarchy 中的绝对路径 |
| `HierarchyUtility` | Transform 层级路径操作：完整路径、相对路径、深层子物体查找 |
| `ProjectSafeEditorUtility` | Ping 并选中项目中任意资源（支持文件夹路径） |
| `UrlUtility` | 便捷的 URL 打开与外部链接处理 |
| `ReflectionUtility` | 程序集与命名空间的反射操作工具 |
| `PredefinedAssemblyUtility` | 预定义程序集类型识别与接口实现类型查找 |
| `PlayerLoopUtility` | 自定义 Unity PlayerLoop：插入、移除子系统，打印 PlayerLoop 结构 |
| `RegexUtility` | 正则表达式工具：命名空间/类名规范化、邮箱/URL 校验 |
| `AesirInspectorLogger` | 统一日志输出，带彩色前缀，编译后自动剔除，双击可跳转调用方；可通过 `AesirInspectorLoggerSettings` 配置日志级别 |

### 9. 自定义特性 (Custom Attributes)

| 特性 | 说明 |
|------|------|
| `[Summary]` | 注释特性，等同于 XML 注释的 `<summary>` 部分，可在运行时通过 `GetSummary()` 获取摘要文本 |

### 10. 代码风格与规范

本项目将代码风格视为与功能同等重要的组成部分。内置严格的代码编写标准与示例，确保团队协作中的代码一致性与可维护性：

- **风格指南**：详情请参阅 `Runtime/CodeStyle/AESIR_INSPECTOR_CODE_STYLE.cs`。
- **设计理念**：良好的代码风格不是可选项，而是项目质量的基石。所有贡献者均需遵循本规范。

## 使用示例

```csharp
using Runestone.AesirInspector;
using Sirenix.OdinInspector;
using UnityEngine;

public class ExampleMonoBehaviour : MonoBehaviour
{
    [BilingualTitle("玩家属性", "Player Stats")]
    [SerializeField]
    private int health;

    [BilingualButton("重置属性", "Reset Stats")]
    private void ResetStats()
    {
        health = 100;
    }
}
```

## 许可协议

本项目采用 MIT 协议开源。详情请参阅 [LICENSE.md](LICENSE.md)。
