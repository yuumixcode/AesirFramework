# Script Doc Generator（脚本文档生成模块）

> 本模块是 **Aesir Modules** 的功能模块，整体强依赖 [Odin Inspector](https://odininspector.com/)（3.3.x+）：代码经 asmref 汇入 `Runestone.AesirModules.OdinInspector` / `Runestone.AesirModules.Editor.OdinInspector` 程序集，未安装 Odin 时自动排除、不参与编译。

通过反射分析 C# 类型信息，生成结构化的 API 文档，支持增量生成与个性化扩展；并附带 Summary 工具，在 XML `<summary>` 注释与 `[Summary]` 特性之间同步（`[Summary]` 特性为权威内容源，特性优先、XML 回退）。

- **Unity**: 2022.3 或更高版本
- **命名空间**：Runtime 为 `Runestone.AesirModules.ScriptDocGenerator`，Editor 为 `Runestone.AesirModules.ScriptDocGenerator.Editor`
- **入口**：`Tools → Aesir → Script Doc Generator` 打开主窗口；Project 窗口右键脚本（`Assets → Script Doc Generator`）可使用全部快捷功能

## 1. 脚本文档生成器 (Script Doc Generator)

### 使用场景

当你的团队、开源项目或个人项目需要一份 API 文档时——不仅需要自动生成的类型签名，还需要针对 API 补充个性化说明（使用示例、注意事项、业务上下文等）——Script Doc Generator 正是为此而设计的。它负责生成准确的 API 签名部分，你来补充那部分只有人才写得出的内容，两者互不干扰，增量更新。

### 核心优势

- **🔒 完全离线**：基于 C# 反射机制运行，无需网络连接、无需外部 API、无需第三方服务。断网环境、内网开发、机密项目——随时随地可用。
- **⚡ 单类型即点即生成**：单类型/多类型模式的分析在毫秒到秒级完成；程序集模式会遍历整个程序集（数百类型），生成阶段带进度条提示。
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
| 文档输出路径 | 自定义文档生成的目标文件夹（支持 Assets 外绝对路径，默认输出到项目根 `ScriptDocGenerator/`，不产生 .meta） |
| 按命名空间生成文件夹 | 开启后按类型的命名空间自动创建子目录，如 `Runestone.AesirModules` → `Runestone/AesirModules/` |
| 自定义文档扩展名 | 默认 `.md`，可切换为 `.mdx`、`.txt` 等任意扩展名 |
| 增量生成标识符 | 开启后自动在文档末尾插入 `## Additional Notes` 段落，重新生成时保留该段落之后的手写内容 |
| 类型来源模式 | 单类型 / 多类型 / 单程序集 / 多程序集，四种粒度按需选择（程序集模式下拉仅列出项目脚本程序集） |
| 调试检查模式 | 默认关闭。开启后在窗口内渲染类型分析的中间结果（完整成员树），供检查分析数据；程序集模式下开启时有性能警告提示 |
| TypesCacheSO | 将 Type 列表保存为可复用的资源文件，避免每次重新选择 |

### 可扩展接口

| 接口 | 说明 |
|------|------|
| `DocGeneratorSettingsSO` | 继承此抽象类并实现 `GetGeneratedDocumentation(ITypeData)` 方法，即可自定义文档的格式与内容。内置两个参考实现：`DefaultScriptingAPISettingsSO`（中文 API Markdown 文档）与 `ZensicalScriptingAPISettingsSO`（静态站点 API 文档：YAML Front Matter、DocFX 风格表格、参数/返回值/备注/类型参数说明列、锚点跳转） |
| `IAnalysisDataFactory` | 替换整个类型分析工厂，自定义成员数据的解析逻辑 |
| `IAttributeFilter` | 自定义特性过滤器，控制哪些特性出现在生成的文档中 |

### 单元测试覆盖

本模块测试位于包根 `Tests/Editor/ScriptDocGenerator/`（汇入 `Runestone.AesirModules.Tests` 程序集），覆盖签名生成、源码解析与工具行为。测试用例会随着功能迭代持续补充：

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
| **SourceParsing** | 48 | SourceScanner 单遍状态机：字符串/注释净化、命名空间与类型栈、嵌套类型键、方法参数键、构造函数 `#ctor` 键、param/remarks 等六种 XML 标签解析 |
| **XmlSummaryTool** | 25 | Summary 工具三模式输出：特性优先双向对齐、引号转义、CRLF 行尾保持、`////` 判定、预处理指令内插入 |
| **Misc** | 13 | 文档文件名转换、内部类型过滤、生成器输出回归（单事件类/单方法接口章节不丢失、常量表过滤、空继承属性章节） |

## 2. Summary 工具 (Summary Tool)

通过右键菜单（`Assets → Script Doc Generator → Process Summary`）快捷处理 C# 脚本中的 XML `<summary>` 注释与 `[Summary]` 特性同步。内容以 `[Summary]` 特性为权威源——已存在可解析的特性时以特性文本为准（XML 与特性不一致时回写 XML 对齐），无特性时回退 XML 内容生成特性。

### 使用场景

当你的团队要求公共成员同时具备 XML 文档注释和 `[Summary]` 特性时——手动维护两份内容相同但格式不同的注释既繁琐又容易遗漏。Summary Tool 正是为此而设计的：特性已存在时它把 XML 注释对齐到特性内容，特性缺失时它从 XML 注释提取摘要自动生成特性，确保两者始终一致。

### 核心优势

- **⚡ 右键即用**：在 Project 窗口选中脚本，右键即可执行，无需打开额外窗口。
- **🔄 三种模式**：同步（Sync）、替换（Replace）、移除（Remove），覆盖日常维护的全部需求；Remove 为破坏性批量操作，执行前有确认对话框。
- **📦 批量处理**：支持多选脚本同时处理，批量操作仅触发一次资产刷新；无 XML 注释或无变化的文件跳过写盘，不做无关重写。
- **🧠 智能导入**：写入特性的模式（Sync/Replace）自动添加 `using Runestone.AesirModules.ScriptDocGenerator;`，无需手动补引用。
- **🏗️ 宏定义感知**：自动识别 `#if` 等预处理指令，确保 `[Summary]` 特性插入在条件编译块内部。
- **🛡️ 源码安全**：特性文本中的双引号自动转义（生成合法 C#）；保留原文件行尾风格（CRLF/LF）；四斜杠普通注释（`////`）不会被误判为 XML 文档注释。

### 工作原理

`XmlSummaryTool` 的处理流程分为三个阶段：**解析 → 分组 → 输出**。

**1. 解析阶段**：将源代码按行扫描，定位第一个 XML 文档注释（恰好三斜杠 `///`），将其之前的所有行标记为 **Header**（using、namespace 等），之后的部分进入分组阶段。

**2. 分组阶段**：从第一个 `///` 开始，交替提取 **XML 注释块**（连续的 `///` 行）和 **代码块**（非 `///` 行），生成 `XmlCodePart` 列表。每个 `XmlCodePart` 由 `xml`（注释部分）和 `code`（代码部分）组成。

**3. 输出阶段**：根据选择的模式，对每个 `XmlCodePart` 执行不同的操作（内容均为特性优先）：

| 模式 | 行为 | 说明 |
|------|---------|------|
| **Sync** | 双向对齐，保持双份 | 已有可解析 `[Summary]`：以特性文本为准，XML summary 与之不一致时把 XML 对齐为特性内容（代码不动）；无特性：以 XML 内容生成特性插入在前导预处理之后。两者都保留 |
| **Replace** | 收敛为单份特性 | 内容首选 `[Summary]` 特性文本（无特性时取 XML）；移除 XML `<summary>` 标签，写入单行 `[Summary]` 特性 |
| **Remove** | 仅移除特性 | 移除所有 `[Summary]` 特性，保留 XML 注释；无特性的文件原样跳过不重写 |

**宏定义感知**：当代码块以 `#if`、`#elif`、`#else` 等预处理指令开头时，`[Summary]` 特性会插入在这些指令之后（即条件编译块内部），而非之前。例如：

```csharp
// 输入（特性与 XML 不一致——特性是权威内容源）
/// <summary>编辑器方法</summary>
#if UNITY_EDITOR
[Summary("旧内容")]
public void Reset() { }
#endif

// Sync 输出 — XML 被对齐为特性内容
/// <summary>旧内容</summary>
#if UNITY_EDITOR
[Summary("旧内容")]
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
├── AnalysisData/                           # 成员数据模型（TypeData/FieldData/MethodData 等，含 IParameterData[] 参数链与 remarks/value/typeparam 注释属性）
├── Attributes/                             # [Summary] / [ReferenceLinkURL] 特性
├── Core/                                   # 反射分析引擎与过滤器
└── Utilities/                              # 反射工具
Editor/ScriptDocGenerator/OdinInspector/    # 经 asmref 汇入 Runestone.AesirModules.Editor.OdinInspector
├── AttributeProcessors/                    # Odin AttributeProcessor
├── DocGeneratorSettings/                   # 文档生成配置（中文 API 生成器 + Zensical 站点生成器）
├── SourceFileTool/                         # 源码扫描与 XML 文档注释解析（六种标签全链路）
├── SummaryAttributeTool/                   # XmlSummaryTool（特性优先同步）
└── ...                                     # 窗口、面板、菜单、路径常量等
Tests/Editor/ScriptDocGenerator/            # 单元测试（汇入 Runestone.AesirModules.Tests）
```
