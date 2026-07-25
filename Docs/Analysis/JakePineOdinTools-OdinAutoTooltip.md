# OdinAutoTooltip 详细分析

> 源文件: `JakePineOdinTools/OdinAutoTooltip/Editor/OdinAutoTooltipAttributeProcessor.cs`
> 角色: **替代 Aesir Inspector 现有 Summary 特性的核心插件**。
> 依赖: `Odin Inspector` + `OdinSource`

---

## 1. 一句话做什么

读源文件里的 `/// <summary>` 注释,在 Odin 渲染 inspector 成员时,自动给该成员加一个 `TooltipAttribute`(内存里挂,不改源文件)。

**对比 Aesir 现有 Summary Tool**:
- Aesir 现状:开发者**手动**右键菜单,在源码里插入 `[Summary("...")]`,运行期也能读到
- 新方式:开发者**只**写 XML summary,**不动手**,editor 渲染时自动转 tooltip,**运行期无 Summary**

---

## 2. 类签名与扩展点

```csharp
public class OdinAutoTooltipAttributeProcessor : OdinAttributeProcessor<object>
```

- 继承 Sirenix 的 `OdinAttributeProcessor<T>` —— Odin 的标准扩展点
- `T = object` 表示对**所有 inspector 化对象**生效
- 静态成员,Odin 内部用反射 new 一个实例,无状态可保留

**Odin 的扩展机制**:
- Unity Inspector 选中或绘制某个对象时,Odin 遍历它的每个成员(field/property/method)
- 对每个成员调一次 `ProcessChildMemberAttributes(prop, member, attributes)`
- `attributes` 列表是该成员已经收集到的所有 attribute,**直接 Add 到列表里就会被 Odin 当作真实 attribute 处理**
- 列表里的 attribute 在 inspector 关闭/重编译时**自动释放**,**不写回源、不进运行时**

---

## 3. 三个配置开关

```csharp
public static readonly bool ENABLED       = true;   // 编译期 readonly
public static bool USE_SUMMARIES          = true;   // 用 /// <summary>
public static bool USE_MEMBER_NAME        = true;   // 没 summary 时用成员名
```

| 开关 | 默认 | 控制粒度 | 关闭后效果 |
|------|------|---------|----------|
| `ENABLED` | `true` | **整个插件** | `ProcessChildMemberAttributes` 立即 return,**零开销** |
| `USE_SUMMARIES` | `true` | 摘要来源 | 关闭后**不读任何源文件**,只走 `USE_MEMBER_NAME` 路径 |
| `USE_MEMBER_NAME` | `true` | 兜底 | 关闭后无 summary 的成员**完全不挂 tooltip** |

**还有一个** `ShouldSkipMember` 钩子(可选,用于项目级特殊规则):

```csharp
public static Func<InspectorProperty, MemberInfo, List<Attribute>, bool> ShouldSkipMember;
```

注册示例:

```csharp
[InitializeOnLoad]
public static class MyTooltipFilter {
    static MyTooltipFilter() {
        OdinAutoTooltipAttributeProcessor.ShouldSkipMember = (prop, member, attrs) => {
            // 比如:某个自定义 drawer 内的字段已经有自己的 hover label
            return member.Name == "dontWantTooltip";
        };
    }
}
```

---

## 4. 主流程

```csharp
public override void ProcessChildMemberAttributes(
    InspectorProperty property, MemberInfo member, List<Attribute> attributes)
{
    // 1. 总闸 + 双开关检查
    if (!ENABLED || (!USE_SUMMARIES && !USE_MEMBER_NAME)) return;

    // 2. 已经有 Tooltip 或 PropertyTooltip → 不动它
    if (!attributes.OfType<TooltipAttribute>().Any()
        && !attributes.OfType<PropertyTooltipAttribute>().Any())
    {
        // 3. 项目级过滤
        if (ShouldSkipMember != null && ShouldSkipMember(property, member, attributes))
            return;

        // 4. 拿 tooltip 文本
        string tooltip = ResolveTooltipText(member);
        if (string.IsNullOrEmpty(tooltip)) return;

        // 5. 挂上去
        attributes.Add(new TooltipAttribute(tooltip));
    }
}
```

**关键设计**:
- **不覆盖用户已经写的 tooltip**——这是"非侵入式"的核心承诺
- **`[Tooltip(null)]` 也算"已设置"**——可以**主动关闭**某个成员的自动 tooltip
- **property 级别用 `PropertyTooltipAttribute`**(Odin 的),field 级别用 `TooltipAttribute`(Unity 的)

---

## 5. 摘要解析流程

`ResolveTooltipText` 走"先 summary,后 member name"的两级回退:

```
USE_SUMMARIES = true
  └─ GetSummaryFromSource(member)
        └─ 从 declaringType 的缓存拿
              └─ 缓存没有 → ParseSummariesForType(declaringType) 读源
                    └─ 缓存结果(哪怕是 null)
        └─ 命中 → 用 summary
        └─ 不命中 → 走 member name

USE_SUMMARIES = false
  └─ 直接走 member name

USE_MEMBER_NAME = false 且 USE_SUMMARIES = false
  └─ 主入口直接 return,什么也不做
```

---

## 6. 源文件解析的细节

### 6.1 类型范围限定

```csharp
private static Dictionary<string, string> ParseSummariesForType(Type type)
{
    string[] lines = OdinSourceFileHelper.GetSourceLines(type);
    string typeKey = OdinSourceFileHelper.GetTypeKey(type);

    if (OdinSourceFileHelper.TryGetTypeBodyRange(lines, typeKey, out int start, out int end))
    {
        // 只在类型体范围内搜,避免外层/内层类的污染
        string[] scoped = new string[end - start + 1];
        Array.Copy(lines, start, scoped, 0, scoped.Length);
        return ExtractSummaries(scoped);
    }
    // 兜底:拿不到 body 范围就在全文件搜
    return ExtractSummaries(lines);
}
```

### 6.2 Summary 块识别

`ExtractSummaries` 的核心循环:

```csharp
while (lineIndex < lines.Length)
{
    string trimmed = lines[lineIndex].TrimStart();

    if (!StartsSummaryDocComment(trimmed))   // 是否 /// 开头且以 <summary> 开头
    {
        lineIndex++;
        continue;
    }

    // 1. 连续收 /// 行,直到遇到非 /// 行 或 看到 </summary>
    List<string> summaryLines = CollectSummaryDocLines(lines, ref lineIndex);

    // 2. 跳过预处理指令、单行注释、特性 [ ]
    lineIndex = SkipMemberPreambleLines(lines, lineIndex);

    // 3. 遇到空行就跳
    while (lineIndex < lines.Length && string.IsNullOrWhiteSpace(lines[lineIndex]))
        lineIndex++;

    // 4. 提取成员名
    if (lineIndex >= lines.Length) break;
    string memberName = OdinSourceFileHelper.ExtractMemberName(lines[lineIndex].Trim());
    if (memberName == null) continue;

    // 5. 入字典
    string summary = ParseSummaryText(summaryLines);
    if (!string.IsNullOrWhiteSpace(summary))
        result[memberName] = summary;
}
```

**关键宽松性**:
- `SkipMemberPreambleLines` 跳过的内容:`#` 预处理指令、`//` 行注释、`[...]` 特性
- 也就是说 summary 在 member 之前的位置很自由:
  - `summary → attributes → member` ✅
  - `attributes → summary → member` ✅
  - summary 前后有空行 ✅
- 多个 summary 块最近一个生效(顺序扫描,后写覆盖前写)

### 6.3 文本清洗

`ParseSummaryText` + `StripXmlTags`:

- 提取 `<summary>...</summary>` 中间的内容
- 处理 `<see cref="TypeName.Member"/>` → 简化成 `Member`
- 剥其他 XML 标签(`<param>`、`<returns>`、`<typeparam>` 等)
- 多行合并成一行(用空格 join)
- 多个连续空格压成一个

**例**:
```csharp
/// <summary>
/// Maximum health for this unit. Clamped at runtime by
/// <see cref="minHealth"/> and upgrade modifiers.
/// </summary>
```
→
```
Maximum health for this unit. Clamped at runtime by minHealth and upgrade modifiers.
```

### 6.4 缓存策略

| 缓存 | Key | Value | 失效 |
|------|-----|-------|------|
| `summaryCache` | `Type`(declaring type) | `Dictionary<memberName, summary>` 或 `null` | Assembly Reload |

**关键细节** —— **null 也缓存**:

```csharp
if (!summaryCache.TryGetValue(declaringType, out var memberSummaries))
{
    memberSummaries = ParseSummariesForType(declaringType);
    summaryCache[declaringType] = memberSummaries;   // 即使是 null 也存
}
```

注释里写明原因:
> Cache the result even when null. ParseSummariesForType returns null whenever the source file cannot be found — which is the case for every compiled-only type (Unity built-in components, third-party DLLs, etc.). Without caching null, each repaint re-runs AssetDatabase.FindAssets and file reads for those types, which is very costly in windows that draw many components at once (e.g. PinnedInspectorWindow, which renders all components on a GameObject).

**对 Aesir 的启示**: **缓存要 cache miss 也算结果**。这是非常实用的性能优化经验。

---

## 7. 已知限制(README 明确说明 + 代码补充)

### 7.1 README 列出的限制

1. **类/结构体/枚举上的 summary 不会传播给该类型的字段**
   - `public Outer nestedOuter;` 的 tooltip 用的是字段名 `nestedOuter`,不是 `Outer` 类的 summary
2. **枚举值不支持**——必须在每个枚举成员上手动写 `[Tooltip]`
3. **`[Tooltip(null)]` 完全关闭**,但 IDE 文档仍能用 XML summary

### 7.2 代码层面的隐含限制

- **跨行块注释 `/* */` 没处理**(`SkipMemberPreambleLines` 里的 `//` 检查)—— 块注释会被当代码扫
- **跨行逐字字符串 `@"..."` 没处理** —— 同 `OdinSource` 限制
- **没有文件变更监听** —— 改源后需重编译
- **不支持 `partial class` 跨文件**(`OdinSourceFileHelper` 拿到的是第一个匹配的 `.cs`,其他 partial 段里的 summary 抓不到)

---

## 8. 与 Aesir 现有 Summary 特性的逐项对比

| 维度 | Aesir `[Summary]` 现状 | JakePine `OdinAutoTooltip` 替换后 |
|------|---------------------|--------------------------------|
| **触发** | 手动右键菜单 | 打开 Inspector 自动 |
| **落点** | 写回源 `[Summary("...")]` | editor 内存里挂 `TooltipAttribute`,**不写源** |
| **运行时可读** | ✅ `SummaryAttribute.GetSummary()` | ❌ 运行时无 |
| **依赖 Odin** | 否(不依赖 Odin) | ✅ 必须 |
| **运行期构建** | Runtime 程序集有 `SummaryAttribute` | Editor-only,Runtime 干净 |
| **IDE 同步** | 容易双写不同步 | XML 是唯一真相,自洽 |
| **菜单** | 有 | 无(不需要) |
| **同步/替换/移除三模式** | 有 | 不需要 |
| **预处理感知** | `#if` 内放 `[Summary]`(已实现) | Odin inspector 自己管 |
| **多行 summary** | 整段保留 | 合并成一行 |
| **`<see cref="X.Y"/>` 处理** | 不处理(原文) | 简化成 `Y` |
| **枚举值支持** | 不涉及 | **不支持** ❌ |
| **类 summary → 字段 tooltip** | 不涉及 | **不支持** ❌ |
| **缓存** | 无(每次跑菜单重新解析) | 类型级缓存,assembly reload 失效 |
| **性能** | 菜单触发,几秒(改写源) | 一次性 ~ms,后续 μs |
| **运行时反射拿 Summary** | 支持 | **破坏性变更** ❌ |

### 关键 break change

1. **`SummaryAttribute` 整个类需要被废弃/移除**
2. **所有 `attribute.GetSummary()` 调用方失效**
   - 需在 Aesir 全家桶(`Aesir Inspector` + `Aesir Architecture` + `Aesir Modules`)全局 grep 确认有没有调用方
3. **`XmlSummaryTool` 三个处理模式(Sync/Replace/Remove)无意义**
4. **右键菜单 `SummaryToolMenuItems` 需要移除**
5. **测试 `XmlSummaryToolTests` 需要重写/删除**

### 关键 gain

1. **不再需要双写**——开发者只维护 XML summary
2. **不修改源码**——版本控制 diff 干净
3. **Odin inspector 渲染时始终最新**
4. **运行时省一个 attribute**——Release 包更小
5. **多选脚本批量处理的概念不再适用**——零操作

---

## 9. Aesir 引入时的迁移细节

### 9.1 文件放置

```
Assets/Runestone/AesirInspector/Editor/Odin Integration/JakePineOdinTools/
├── OdinSource/                ← 必需底座
│   ├── README.md
│   └── Editor/
│       └── OdinSourceFileHelper.cs
└── OdinAutoTooltip/           ← 替代 Summary 的核心
    ├── README.md
    └── Editor/
        └── OdinAutoTooltipAttributeProcessor.cs
```

### 9.2 asmdef

- 放到 `Editor/` 子文件夹,自动编入 `AesirInspector.OdinIntegration.Editor.asmdef`
- 该 asmdef 应该已经引用了 `Sirenix.OdinInspector.Editor` —— 否则 Odin 的 `OdinAttributeProcessor<>` 找不到
- 需要在 Aesir 现有 asmdef 里加 `Odin Inspector` 引用(检查一下,可能已经有了)

### 9.3 配置默认值建议

Aesir 引入时,**配置文件默认值**:
- `ENABLED = true` —— 默认开,符合"零配置"原则
- `USE_SUMMARIES = true` —— 默认读 XML summary
- `USE_MEMBER_NAME = true` —— 兜底,符合"任何字段都至少有个 tooltip"原则

如果项目里**有大量字段没有 XML summary** 且觉得 `memberName` 兜底太啰嗦,可以在 Aesir 的初始化里改成 `USE_MEMBER_NAME = false`。

### 9.4 引入后,Aesir 自家要不要禁用?

Aesir Inspector 自己的代码(比如 `XmlSummaryTool.cs`)目前**对每个公共成员都加了 `[Summary("...")]`**。引入新工具后:
- 这些 `[Summary]` 变成**纯运行时残留**(editor 看不到,因为 `OdinAutoTooltip` 看到已有 Tooltip 会跳过)
- 运行期仍可读(只要不删 `SummaryAttribute`)
- **建议**:分阶段移除,先禁用 Aesir 的 `[Summary]` 装饰,跑一两个版本确认无 regression 后再彻底删除

---

## 10. 文档与上游引用

- 仓库 README 章节: <https://github.com/JakePineGames/JakePineOdinTools/tree/master#plugins>
- OdinAutoTooltip 自带 README: <https://github.com/JakePineGames/JakePineOdinTools/blob/master/OdinAutoTooltip/README.md>
- 源码(Apache/MIT 兼容): `OdinAutoTooltip/Editor/OdinAutoTooltipAttributeProcessor.cs`

---

## 11. 一句话总结

**OdinAutoTooltip 是 Aesir 现有 Summary Tool 的现代替身**——同样的"XML summary → 显示文本"价值,但**全自动、零操作、不污染源码**。代价是强依赖 Odin + 放弃运行时 `GetSummary()`,需要在 Aesir 全家桶确认没有运行时调用方。
