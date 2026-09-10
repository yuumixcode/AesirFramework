# Script Doc Generator（脚本文档生成模块）

> 本模块是 **Aesir Modules** 的功能模块，整体强依赖 [Odin Inspector](https://odininspector.com/)（3.3.x+）：代码经 asmref 汇入 `Runestone.AesirModules.OdinInspector` / `Runestone.AesirModules.Editor.OdinInspector` 程序集，未安装 Odin 时自动排除、不参与编译。

通过反射分析 C# 类型信息，生成结构化的 API 文档，支持增量生成与个性化扩展；并附带 Summary 工具，快捷处理 XML `<summary>` 注释与 `[Summary]` 特性之间的双向同步。

- **Unity**: 2022.3 或更高版本
- **命名空间**：Runtime 为 `Runestone.AesirModules.ScriptDocGenerator`，Editor 为 `Runestone.AesirModules.ScriptDocGenerator.Editor`
- **入口**：`Tools → Aesir → Script Doc Generator` 打开主窗口；Project 窗口右键脚本（`Assets → Script Doc Generator`）可使用全部快捷功能

## 1. 脚本文档生成器 (Script Doc Generator)

### 使用场景

当你的团队、开源项目或个人项目需要一份 API 文档时——不仅需要自动生成的类型签名，还需要针对 API 补充个性化说明（使用示例、注意事项、业务上下文等）——Script Doc Generator 正是为此而设计的。它负责生成准确的 API 签名部分，你来补充那部分只有人才写得出的内容，两者互不干扰，增量更新。

### 核心优势

- **🔒 完全离线**：基于 C# 反射机制运行，无需网络连接、无需外部 API、无需第三方服务。断网环境、内网开发、机密项目——随时随地可用。
- **⚡ 零等待**：反射分析在毫秒级完成。选中类型，文档即现。没有进度条，没有等待。
- **🎮 与 Unity 一体**：作为 Editor 原生扩展运行，直接在 Inspector 中操作。无需切换窗口、无需外部工具链——文档就在你编写代码的地方。
- **✏️ 增量生成**：重新生成文档时，自动保留 `## Additional Notes` 标识符之后的手写内容，已有 Front Matter（YAML/TOML 头部）也会保留。自动生成的签名与人工补充的说明互不覆盖。
- **🤖 AI 友好**：默认生成 Markdown 格式文档，结构清晰、语义明确，可直接用于构建 AI API 问答知识库（RAG、Embedding 等），让 AI 助手精准回答项目 API 相关问题。
- **🔧 可配置与可扩展**：提供多种配置项与扩展接口，适配不同项目的文档需求（详见下方）。

### 功能详情

- **类型分析**：支持类、结构体、接口、枚举、委托、Record 等类型的完整签名与特性解析，包含泛型约束、基类继承与接口实现。
- **字段解析**：覆盖全部 C# 原始类型、集合类型、委托类型、特殊类型（abstract/dynamic/interface/nullable），以及 const/static 默认值、访问修饰符、复合关键字（const/static readonly/readonly）、Unity 内置类型与特性标注。
- **属性解析**：支持 getter/setter 不对称访问修饰符（如 `public get / private set`）、静态属性、默认值初始化。
- **方法解析**：支持泛型方法、参数默认值、`params` 可变参数、`async` 异步方法、运算符重载、扩展方法。
- **继承分析**：识别 virtual/abstract/override 方法与接口实现，追踪继承链来源。
- **内部类型过滤**：自动跳过编译器合成类型（`<PrivateImplementationDetails>`、匿名类型、闭包类）与 Unity 源生成类型（`UnitySourceGenerated*`），不为它们生成文档。
- **辅助功能**：成员排序（`DerivedMemberDataComparer`）、方法重载标记、构造方法签名生成、事件签名生成。

### 可配置项

| 配置项 | 说明 |
|-------|------|
| 文档输出路径 | 自定义文档生成的目标文件夹，支持拖拽设置 |
| 按命名空间生成文件夹 | 开启后按类型的命名空间自动创建子目录，如 `Runestone.AesirModules` → `Runestone/AesirModules/` |
| 自定义文档扩展名 | 默认 `.md`，可切换为 `.mdx`、`.txt` 等任意扩展名 |
| 增量生成标识符 | 开启后自动在文档末尾插入 `## Additional Notes` 段落，重新生成时保留该段落之后的手写内容 |
| 类型来源模式 | 单类型 / 多类型 / 整个程序集，三种粒度按需选择 |
| TypesCacheSO | 将 Type 列表保存为可复用的资源文件，避免每次重新选择 |

### 可扩展接口

| 接口 | 说明 |
|------|------|
| `DocGeneratorSettingsSO` | 继承此抽象类并实现 `GetGeneratedDocumentation(ITypeData)` 方法，即可自定义文档的格式与内容。内置了 `DefaultScriptingAPISettingsSO`（中文 API Markdown 文档生成器）作为参考实现 |
| `IAnalysisDataFactory` | 替换整个类型分析工厂，自定义成员数据的解析逻辑 |
| `IAttributeFilter` | 自定义特性过滤器，控制哪些特性出现在生成的文档中 |

### 单元测试覆盖

本模块包含 **153 个单元测试**（位于包根 `Tests/Editor/ScriptDocGenerator/`，汇入 `Runestone.AesirModules.Tests` 程序集），覆盖各数据类型的签名生成功能。测试用例会随着功能迭代持续补充：

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

## 2. Summary 工具 (Summary Tool)

通过右键菜单（`Assets → Script Doc Generator → Process Summary`）快捷处理 C# 脚本中的 XML `<summary>` 注释与 `[Summary]` 特性之间的双向同步。

### 使用场景

当你的团队要求公共成员同时具备 XML 文档注释和 `[Summary]` 特性时——手动维护两份内容相同但格式不同的注释既繁琐又容易遗漏。Summary Tool 正是为此而设计的。它从 XML 注释中提取摘要，自动生成对应的 `[Summary]` 特性，确保两者始终保持同步。

### 核心优势

- **⚡ 右键即用**：在 Project 窗口选中脚本，右键即可执行，无需打开额外窗口。
- **🔄 三种模式**：同步（Sync）、替换（Replace）、移除（Remove），覆盖日常维护的全部需求。
- **📦 批量处理**：支持多选脚本同时处理，批量同步或清理。
- **🧠 智能导入**：处理完成后自动添加 `using Runestone.AesirModules.ScriptDocGenerator;`，无需手动补引用。
- **🏗️ 宏定义感知**：自动识别 `#if` 等预处理指令，确保 `[Summary]` 特性插入在条件编译块内部。

### 工作原理

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

最后，输出阶段会自动检测 Header 中是否已包含 `using Runestone.AesirModules.ScriptDocGenerator;`，若未包含则自动添加。

## 3. 自定义特性 (Custom Attributes)

| 特性 | 说明 |
|------|------|
| `[Summary]` | 注释特性，等同于 XML 注释的 `<summary>` 部分，可在运行时通过 `GetSummary()` 获取摘要文本 |
| `[ReferenceLinkURL]` | 参考链接特性，为类型附加文档参考链接，生成的文档中会包含这些链接 |

## 目录结构

模块遵循 Aesir Modules 的标准模块结构：整体为 Odin 专属代码，经 asmref 汇入对应 Odin 程序集。

```
Runtime/ScriptDocGenerator/OdinInspector/   # 经 asmref 汇入 Runestone.AesirModules.OdinInspector
├── AnalysisData/                           # 成员数据模型（TypeData/FieldData/MethodData 等）
├── Attributes/                             # [Summary] / [ReferenceLinkURL] 特性
├── Core/                                   # 反射分析引擎与过滤器
└── Utilities/                              # 反射工具
Editor/ScriptDocGenerator/OdinInspector/    # 经 asmref 汇入 Runestone.AesirModules.Editor.OdinInspector
├── AttributeProcessors/                    # Odin AttributeProcessor
├── DocGeneratorSettings/                   # 文档生成配置（含默认中文 API 生成器）
├── SourceFileTool/                         # 源码扫描与 Summary 解析
├── SummaryAttributeTool/                   # XmlSummaryTool（双向同步）
├── UIToolkitWindow/                        # UI Toolkit 版窗口
└── ...                                     # 窗口、面板、菜单、路径常量等
Tests/Editor/ScriptDocGenerator/            # 153 个单元测试（汇入 Runestone.AesirModules.Tests）
```
