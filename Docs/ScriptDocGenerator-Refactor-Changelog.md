# ScriptDocGenerator 重构变更记录

> 创建日期：2026-08-05
> 用途：记录 2026-08-04 至 2026-08-05 期间 ScriptDocGenerator 模块的全部重构和修复内容，用于后续填充 CHANGELOG。

---

## Added

- **源代码文件查找与内容缓存**：新增 `SourceFileEntry` 数据容器，将 `.cs` 文件路径与代码内容绑定，支持缓存避免重复读取
- **块注释内的假 XML 注释过滤**：解析源代码时逐行跟踪 `/* */` 块注释状态，块注释内以 `///` 开头的行不会被误判为 XML 文档注释
- **跨程序集同名类型区分**：summary 缓存键加入程序集名前缀（`AssemblyName.Namespace.TypeName.MemberName`），避免不同程序集中同名命名空间+类型名的键冲突
- **重载方法 summary 区分**：方法成员的 summary 键附加参数类型列表（如 `MethodName(int, string)`），不同重载方法各自独立解析 summary
- **嵌套类型 summary 解析**：支持嵌套类型（如 `OuterClass.NestedStruct`）的 summary 查询，不再错误返回外层类的 summary
- **泛型类型 summary 解析**：支持泛型类型（如 `AbstractContext<T>`）的 summary 查询
- **文件名与类型名不匹配时的源文件查找**：当一个 `.cs` 文件中定义了多个类型且文件名不与任何类型名匹配时（如 `Capabilities.cs` 中定义 7 个接口），通过全局内容扫描找到源文件
- **多程序集批量分析模式**：`ScriptDocGeneratorSO.TypeSource` 枚举新增 `MultipleAssemblies` 模式，支持同时分析多个程序集的所有类型
- **源码解析单元测试**：新增 33 个测试覆盖块注释、全限定键、命名空间、单行/多行 summary、多文件合并、多行属性声明、泛型方法、表达式体泛型方法、重载方法、嵌套类型等场景
- **重载前缀单元测试**：新增 4 个测试覆盖 2/3/4 个重载方法和非重载方法的 `[Overload]` 前缀验证

## Changed

- **移除 OdinBridge 桥接层**：不再通过 `IOdinBridge` 接口间接调用 Odin，改为 `#if ODIN_INSPECTOR` 条件编译直接使用 `Sirenix.Utilities` API
- **模块整合**：将 `ReflectionAnalyzer`、`SummaryTool`、`OdinSourceFileHelper` 全部整合到 `ScriptDocGenerator` 模块下，减少跨层碎片化
- **回归单面板设计**：从 4 个独立 Panel SO 回归为单个 `ScriptDocGeneratorSO` + `TypeSource` 枚举切换模式
- **OdinSourceFileHelper 精简**：移除花括号跟踪、类型体定位、字符串净化等复杂逻辑，仅保留源文件查找与成员名提取
- **Summary 解析优先级**：优先检查 `[Summary]` 特性，有则直接返回；无则回退到源代码 XML `/// <summary>` 注释解析
- **测试程序集依赖更新**：新增 OdinIntegration 程序集引用和 `ODIN_INSPECTOR` 定义约束

## Removed

- **OdinAutoTooltip 自动 Tooltip 功能**：移除从源代码 XML 注释自动生成 Inspector Tooltip 的功能
- **OdinBridge 桥接模式**：删除 `IOdinBridge`、`DefaultOdinBridge`、`OdinBridgeLocator`、`OdinInspectorBridge` 共 4 个文件
- **多 Panel 设计**：删除 `ScriptDocGeneratorPanelBase` 及 4 个 PanelSO 共 5 个文件

## Fixed

- **块注释内的 XML 注释被误解析**：当 `/* */` 块注释跨行且某行以 `///` 开头时，该行会被误判为 XML 文档注释并提取到错误的 summary。修复后，块注释内的 `///` 行被正确忽略
- **泛型类型的 summary 无法解析**：分析泛型类型（如 `AbstractContext<T>`）时，summary 为空。原因是 `Type.FullName` 返回的泛型类型名包含反引号（如 `` AbstractContext`1 ``），与源码中的类型名不匹配。修复后泛型类型的 summary 可正常解析
- **Type 自身的 summary 无法解析**：分析类型自身时，summary 为空。原因是查询键错误地追加了类型名作为成员名后缀，产生重复键。修复后类型自身的 summary 可正常解析
- **嵌套类型的 summary 返回外层类的注释**：分析嵌套类型（如 `AesirArchitecturePlayerLoop.AesirArchitectureScriptRunBeforeUpdate`）时，返回的是外层类的 summary。修复后嵌套类型返回各自的 summary
- **多行属性声明的成员名提取失败**：当属性声明跨多行（如 `public static IContext Interface` 后跟换行的 `{ get { ... } }`）时，成员名无法提取，导致 summary 丢失。修复后可正确提取成员名
- **泛型方法的成员名提取错误**：当方法声明包含泛型参数（如 `public void RegisterModel<TModel>(TModel model)`）时，成员名被错误提取为约束类型名而非方法名。修复后可正确提取泛型方法名
- **表达式体泛型方法的成员名提取错误**：当泛型方法使用表达式体（如 `public TModel GetModel<TModel>() where TModel : class, IModel => ...`）时，成员名被错误提取为 `IModel` 而非 `GetModel`。修复后可正确提取方法名
- **重载方法的 summary 互相覆盖**：同名重载方法共享同一个缓存键，后解析的 summary 覆盖先前的。修复后每个重载方法通过参数类型列表区分，各自独立解析 summary
- **重载方法的 `[Overload]` 前缀重复追加**：当方法有 N 个重载时，`[Overload]` 前缀被追加 N-1 次（如 `[Overload] [Overload] [Overload] public void Run()`）。修复后每个重载方法只追加一次 `[Overload]` 前缀
- **`ReferenceLinkURL` 特性在文档中显示不全**：`[ReferenceLinkURL("https://...")]` 特性在生成的文档中仅显示为 `[ReferenceLinkURL]`（不含 URL 参数）。修复后完整显示特性及其参数
- **文件名与类型名不匹配时源文件无法找到**：当一个 `.cs` 文件中定义了多个类型且文件名不与任何类型名匹配时（如 `Capabilities.cs` 中定义 7 个接口），所有类型的 summary 均为空。修复后通过全局内容扫描找到源文件，所有类型的 summary 可正常解析
- **`null` 关键字被误提取为成员名**：源代码中的 `return null;` 语句，`null` 被误提取为成员名。修复后 `null` 被加入关键字过滤集合，不再被提取

## 统计

| 指标 | 数值 |
|------|------|
| 删除文件 | 9 个（OdinBridge 4 + Panel 5） |
| 迁移文件 | 21 个（ReflectionAnalyzer 18 + SummaryTool 3） |
| 新增文件 | 3 个（SourceFileEntry、SourceParsingTests、OverloadPrefixTests） |
| 修复问题 | 12 个 |
| 新增测试 | 37 个（SourceParsingTests 33 + OverloadPrefixTests 4） |
| 总测试数 | 106 个全部通过 |
