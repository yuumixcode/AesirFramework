# Summary 特性替换方案

> 目标:用 `JakePineOdinTools` 仓库的 `OdinAutoTooltip` 替代 Aesir Inspector 现有的 `Summary` 工具。
> 状态:**规划中,待确认方案后实施**。本阶段不修改任何代码。
> 创建日期: 2026-07-24

---

## 1. 背景:为什么要替换?

| 现状痛点 | 影响 |
|---------|------|
| XML `<summary>` 和 `[Summary("...")]` 维护两份 | 容易不同步、容易遗漏、commit diff 噪音大 |
| 必须跑右键菜单才能同步 | 自动化缺失,新人不知道有这个工具 |
| `[Summary]` 写满每个公共成员 | 代码膨胀,Aesir 自家代码里 `XmlSummaryTool.cs` 头部已经看到一堆 |
| 三个处理模式(Sync/Replace/Remove)用户搞不清区别 | 文档要专门解释 |
| `[Summary]` 进了 Runtime 程序集 | 真正被用到的只有编辑器场景,运行时无意义 |

**新方案(JakePineOdinTools)**: **只维护 XML,editor 自动读源 + 自动转 Tooltip**。零操作、零冗余、零运行时负担。

---

## 2. 现有 Summary 体系全貌

### 2.1 文件清单

| 路径 | 角色 | 运行时必需? |
|------|------|------------|
| `Runtime/Unity/Attributes/SummaryAttribute.cs` | `[Summary("text")]` 特性 + `GetSummary()` | ✅ 被 ScriptDocGenerator 反射取 |
| `Editor/Unity/SummaryTool/XmlSummaryTool.cs` | 核心解析 + 三种处理模式 | ❌ Editor only |
| `Editor/Unity/SummaryTool/XmlCodePart.cs` | 数据模型 | ❌ Editor only |
| `Editor/Unity/SummaryTool/SummaryToolMenuItems.cs` | 右键菜单入口 | ❌ Editor only |
| `Editor/Unity/Core/AesirInspectorMenuItems.cs` (3 个 MenuItem 路径常量) | 菜单定义 | ❌ Editor only |
| `Tests/Editor/SummaryTool/XmlSummaryToolTests.cs` | 单元测试 | ❌ Test only |
| `Runtime/Unity/ScriptDocGenerator/AnalysisData/MemberData.cs` (line 70, 93, 111) | **运行时反射取 Summary** | ✅ **关键调用方** |
| `Editor/Odin Integration/ScriptDocGenerator/CnScriptingAPISettingsSO.cs` (12+ 处) | **生成中文 API 文档用** | ❌ Editor only,但**消费 Summary 的输出** |
| `Editor/Odin Integration/Windows/AesirInspectorGettingStartedWindow.cs` (line 131, 141) | UI 介绍 Summary Attribute | ❌ Editor only |
| `Documentation~/en/README.md` (line 256) | 英文文档 | — |
| `README.md` (line 264) | 中文文档 | — |
| `CHANGELOG.md` (line 141) | 历史变更 | — |

### 2.2 关键调用方:`ScriptDocGenerator` 链路

**`MemberData.cs:111`**:
```csharp
SummaryAttributeValue = memberInfo.GetCustomAttribute<SummaryAttribute>()?.GetSummary();
```

**`CnScriptingAPISettingsSO.cs`** 在 12+ 处直接读 `memberData.SummaryAttributeValue`:
- `Line 78`:作为文档主标题
- `Line 89`:列表项
- `Line 118`:参数说明
- `Line 192/216/315/334/354/422/446/532/552/573`:各种 markdown 表格的"描述"列

**含义**:
> 删掉 `SummaryAttribute` = 删掉 `ScriptDocGenerator` 生成的 API 文档里**所有"成员说明"列的文本**。
> 这不是小改动,这是**核心功能受损**。

---

## 3. 替换策略:三选一

### 方案 A:**完全等价替换**——放弃运行时 Summary 能力,文档生成器读 XML 源

| 维度 | 状态 |
|------|------|
| 运行时 `SummaryAttribute` | **完全删除** |
| ScriptDocGenerator 拿 summary 方式 | 改用 `OdinSourceFileHelper` 读源,反射回退 |
| 工作量 | 🔴 大:ScriptDocGenerator 7 处 `CnScriptingAPISettingsSO` + 1 处 `MemberData` 需要改写 |
| 用户感知 | 🔴 破坏性:已有 `[Summary]` 全部失效,需重跑或接受文档空 |
| 优点 | 简洁,源码唯一真相,运行时无冗余 |
| 缺点 | 风险高,文档生成器 153 个测试可能要重写 |

### 方案 B:**保留运行时,新增自动源同步**——`SummaryAttribute` 保留,加 `OdinAutoTooltip`,改 `MemberData` 优先用源回退

| 维度 | 状态 |
|------|------|
| 运行时 `SummaryAttribute` | **保留**(标 `[Obsolete]`,提示"未来可能删除") |
| ScriptDocGenerator 拿 summary 方式 | 优先用源(XML `<summary>`),回退到 `SummaryAttribute` |
| 工作量 | 🟡 中:`MemberData` 一处改写,新增 1 个源读取 helper |
| 用户感知 | 🟢 渐进:已有 `[Summary]` 继续工作,但**不再需要** |
| 优点 | 完全向后兼容,旧代码/旧文档生成结果零影响 |
| 缺点 | `SummaryAttribute` 仍占 Runtime 体积,长期没移除 |
| 推荐 | ✅ **本方案** |

### 方案 C:**并行引入,不删任何东西**——`OdinAutoTooltip` 作为新功能,Summary 工具保留

| 维度 | 状态 |
|------|------|
| 运行时 `SummaryAttribute` | **完全保留** |
| ScriptDocGenerator 拿 summary 方式 | **不变** |
| 工作量 | 🟢 最小:只引入新文件夹,改 README |
| 用户感知 | 🟢 加法,无破坏性 |
| 优点 | 完全零风险,新旧工作流并存 |
| 缺点 | 治标不治本,Summary 工具的维护负担还在 |
| 推荐 | ⚠️ 作为**过渡期**方案 |

**我的建议**:**方案 B 作为目标 + 方案 C 作为过渡**。
- **第一阶段(方案 C)**:只引入 `OdinSource` + `OdinAutoTooltip`,**不动**现有 Summary 工具
- **第二阶段(方案 B)**:`MemberData` 增加源回退 + `SummaryAttribute` 标 `[Obsolete]`
- **第三阶段(可选,远期)**:验证 N 个版本无影响后,在某次 major bump 时彻底移除 `SummaryAttribute` + 三个右键菜单

---

## 4. 推荐实施计划:方案 B + C 组合

### 阶段 1:基础设施(方案 C,零破坏)

#### 1.1 引入文件

```
Assets/Runestone/AesirInspector/
└── Editor/
    └── Odin Integration/
        └── JakePineOdinTools/        ← 新建
            ├── OdinSource/            ← 必需
            │   ├── README.md
            │   └── Editor/
            │       └── OdinSourceFileHelper.cs
            └── OdinAutoTooltip/       ← 替代 Summary 的核心
                ├── README.md
                └── Editor/
                    └── OdinAutoTooltipAttributeProcessor.cs
```

> **`OdinBatch` 不在第一阶段引入**。等 Summary 替换稳定后,作为独立 PR 引入,降低单次改动风险。

#### 1.2 asmdef 调整

- `AesirInspector.OdinIntegration.Editor.asmdef` 添加 `Sirenix.OdinInspector.Editor` 引用(可能已有,需检查)
- 验证 `OdinAutoTooltipAttributeProcessor` 编译通过

#### 1.3 文档更新

- 在 Aesir Inspector 的 `README.md` 增加章节:
  > **5.6 Odin 工具集**(JakePineOdinTools)
  > 自动从 XML 注释生成 Tooltip(Odin 渲染时)
  > 共享源解析底座
- 在 `CHANGELOG.md` 加一条 `feat:`:
  > 新增 Odin 工具集成,提供 Odin Inspector 渲染时的自动 Tooltip 功能
- 在 `Documentation~/en/README.md` 同步英文版
- 在 `Third Party Notices.md` 增加 JakePineOdinTools MIT 许可记录

#### 1.4 测试

- 验证在装了 Odin 的 Aesir 测试工程里,选中带 `/// <summary>` 的 MonoBehaviour,hover 字段能看到 tooltip
- 验证没装 Odin 时(走 Aesir 现有非 Odin 路径)不受影响

#### 1.5 第一阶段交付清单

- [ ] 引入 `OdinSource` + `OdinAutoTooltip` 文件夹
- [ ] asmdef 引用就绪
- [ ] Aesir Inspector 的 README/CHANGELOG/EN_README 更新
- [ ] `Third Party Notices.md` 更新
- [ ] Edit Mode 测试:验证装了 Odin 时 tooltip 自动出现
- [ ] Edit Mode 测试:验证没装 Odin 时编译通过、不报错
- [ ] **不改任何现有 Summary 代码**

---

### 阶段 2:渐进迁移(方案 B,向后兼容)

#### 2.1 `MemberData` 增加源回退

修改 `Runtime/Unity/ScriptDocGenerator/AnalysisData/MemberData.cs:111`:

```csharp
// 改前
SummaryAttributeValue = memberInfo.GetCustomAttribute<SummaryAttribute>()?.GetSummary();

// 改后:优先用 [Summary](向后兼容),回退读 XML 源
SummaryAttributeValue = ResolveSummaryValue(memberInfo);

private static string ResolveSummaryValue(MemberInfo member)
{
    // 1. 优先:已有 [Summary] 特性(向后兼容,旧代码继续工作)
    var summaryAttr = member.GetCustomAttribute<SummaryAttribute>();
    if (summaryAttr != null && !string.IsNullOrEmpty(summaryAttr.GetSummary()))
        return summaryAttr.GetSummary();

    // 2. 回退:读源里的 /// <summary>(阶段 3 也可以删除这条)
    return SourceSummaryReader.TryReadSummary(member);
}
```

**新增 helper**: `Editor/Unity/SummaryTool/SourceSummaryReader.cs`

```csharp
// 伪代码
internal static class SourceSummaryReader
{
    private static readonly Dictionary<Type, Dictionary<string, string>> cache = new();

    public static string TryReadSummary(MemberInfo member)
    {
        var declaringType = member.DeclaringType;
        if (declaringType == null) return null;

        if (!cache.TryGetValue(declaringType, out var map))
        {
            map = ParseSummariesForType(declaringType);  // 调用 OdinSourceFileHelper
            cache[declaringType] = map;
        }

        return map != null && map.TryGetValue(member.Name, out var s) ? s : null;
    }

    // 解析逻辑可以**直接复用** JakePineOdinTools/OdinAutoTooltipAttributeProcessor 的
    // ExtractSummaries,或者用 Unity 反射的 MemberInfo.GetXMLDoc() 走标准 doc 解析
}
```

**注意**:`SourceSummaryReader` 必须在 **Editor** 程序集,因为它用 `OdinSourceFileHelper`(用了 `UnityEditor.AssetDatabase`)。但 `MemberData` 在 Runtime 调用,这是**跨程序集**的——需要 `SourceSummaryReader` 留一个 Runtime 兼容的接口 + Editor 实现,类似 Aesir 现有的 `OdinBridge` 模式。

**两种实现路径**:

| 路径 | 做法 | 优点 | 缺点 |
|------|------|------|------|
| **B1. 仅 Editor 实现** | `SourceSummaryReader` 是 Editor-only 类,Runtime 里 `MemberData.ResolveSummaryValue` 调用 `OdinBridgeLocator.Resolve<ISourceSummaryReader>()` | 简单,无 Runtime 依赖 | 运行时拿不到源 summary,纯 Unity Build 出来脚本的 `[Summary]` 也拿不到(本来就这样,无 regression) |
| **B2. 双实现** | Runtime 用 `Mono.Cecil` 读 DLL 的 XML doc,Editor 用 OdinSource 读源 | Runtime 也能工作 | 引入 `Mono.Cecil`,包体积变大 |

**建议 B1**。理由:
- 现状 Runtime 也只能拿 `[Summary]`(读源根本不在 Runtime 走)
- ScriptDocGenerator 本身就是 Editor 工具,生成的文档是给开发者看的,不是给运行时的
- 用户如果有 Build 出来的 DLL 需要文档,**应该用生成的 md 文档**而不是运行时反射

#### 2.2 `SummaryAttribute` 标 `[Obsolete]`

```csharp
[Obsolete("SummaryAttribute 已被 OdinAutoTooltip 取代。新代码请只写 /// <summary>,运行时会自动转 Tooltip。该特性将在 v1.0 移除。")]
[AttributeUsage(AttributeTargets.All)]
public class SummaryAttribute : Attribute
{
    // ... 保持原样,旧代码继续编译
}
```

#### 2.3 右键菜单 + 文档

- `SummaryToolMenuItems` 的三个菜单项**保留**——给老用户提供"批量回填"功能
  - Sync 模式:把 XML 写进 `[Summary]`(反向)
  - Replace/Remove 模式:已经不需要,但不删,给愿意手动管理的人
- 在菜单项上加个提示:"⚠️ 已被 OdinAutoTooltip 取代,不再需要手动同步"

#### 2.4 第二阶段交付清单

- [ ] 新增 `Editor/Unity/SummaryTool/SourceSummaryReader.cs`
- [ ] 修改 `MemberData.cs:111` 调用 `SourceSummaryReader`
- [ ] `SummaryAttribute` 加 `[Obsolete]`
- [ ] 153 个 ScriptDocGenerator 测试全部通过(不能 break)
- [ ] 新增测试:验证源 XML 优先于 `SummaryAttribute`
- [ ] 新增测试:验证都没有时返回 null
- [ ] Aesir 文档更新:说明推荐只写 XML summary

---

### 阶段 3(可选,远期):完全删除

只在以下条件都满足时做:
- 至少发布过 2 个 minor 版本带 `[Obsolete]`
- 用户调研没有"必须保留 `[Summary]` 拿运行时值"的场景
- Aesir 全家桶(Aesir Architecture / Aesir Modules)都没有 `GetSummary()` 的调用方

**删除内容**:
- `Runtime/Unity/Attributes/SummaryAttribute.cs` —— 整文件
- `Editor/Unity/SummaryTool/` 整个文件夹
- `Editor/Unity/Core/AesirInspectorMenuItems.cs` 里的 3 个 MenuItem 常量
- `MemberData.cs` 里的 `SummaryAttributeValue` 改为只读源(或保留字段名但仅取源)
- 所有 `MemberData` 上的 `[Summary("...")]` 装饰

**保留内容**:
- `OdinSource` + `OdinAutoTooltip` 继续提供自动 Tooltip
- `CnScriptingAPISettingsSO` 不变(接口签名不变,只是数据源从 attribute 变 source)

---

## 5. 工作量估算

| 阶段 | 工作量 | 风险 | 价值 |
|------|--------|------|------|
| **阶段 1**(零破坏引入) | 1~2 小时(纯文件复制 + asmdef + 文档) | 极低 | 拿到 Odin 用户的 Tooltip 自动化能力 |
| **阶段 2**(渐进迁移) | 1~2 个工作日(主要在测试) | 低 | ScriptDocGenerator 不再依赖双写 |
| **阶段 3**(彻底删除) | 半天 | 中(老用户升级) | 运行时减一个 attribute,代码更干净 |

**总成本**: 2~3 个工作日。
**总收益**:
- 开发者**永远**只维护 XML summary
- 153 个测试**不破坏**
- Aesir 自家代码可以批量去掉 `[Summary("...")]` 装饰
- Runtime 包更干净

---

## 6. 需要用户确认的关键决策

> 在开始实施前,**先确认以下选择**:

1. **方案选择**:B(保留兼容,渐进)还是 A(破坏性,直接干净)?
   - 我推荐 **B**。

2. **OdinBatch 一起引入吗**?阶段 1 还是单独?
   - 我推荐**阶段 1 不引入**,作为独立 PR 处理,降低 review 压力。

3. **`SummaryAttribute` 何时加 `[Obsolete]`**?
   - 阶段 1 引入新工具时(激进,**马上提示**)
   - 阶段 2 改 `MemberData` 时(温和,**与代码改一起提示**)
   - **永不**(永远共存,留给用户选)
   - 我推荐**阶段 2 时一起加**。

4. **运行时是否需要拿到 summary**?
   - 答:**不需要**。ScriptDocGenerator 是 Editor 工具,生成 markdown 文档给开发者看。运行时拿 summary 没用户场景。
   - 如果用户答"需要",**方案 B 失败,要选 A 或保留 C**。

5. **是否在 Aesir 自己的代码里去 `[Summary("...")]` 装饰**?
   - 推荐:**阶段 2 之后**开一个独立的 `chore: remove obsolete [Summary] decorations` PR,跑测试,确认无问题。
   - 现在的 `XmlSummaryTool.cs` 头部那一堆 `[Summary("...")]` 装饰是主要清理目标。

---

## 7. 实施前置清单(开始写代码前确认)

- [ ] 用户确认方案 B
- [ ] 用户确认 OdinBatch 独立 PR
- [ ] 用户确认 `[Obsolete]` 时机
- [ ] 检查 Aesir Architecture / Aesir Modules 是否有 `GetSummary()` 调用(目前我看到的**只有** Aesir Inspector 内部)
- [ ] 检查 Aesir 现有 `AesirInspector.OdinIntegration.Editor.asmdef` 是否有 `Sirenix.OdinInspector.Editor` 引用
- [ ] 准备一个装了 Odin 的测试工程,跑通整个流程

---

## 8. 文档更新 checklist

- [ ] `Assets/Runestone/AesirInspector/README.md`
  - 删掉 "3. Summary 工具" 整章节(阶段 3 时)
  - 加 "5.6 Odin 工具集成" 章节
  - 删掉 "9. 自定义特性" 表格里的 `[Summary]` 行(阶段 3 时)
- [ ] `Assets/Runestone/AesirInspector/CHANGELOG.md`
  - 阶段 1:`feat: 引入 OdinAutoTooltip,自动从 XML summary 生成 Tooltip`
  - 阶段 2:`refactor: ScriptDocGenerator 优先使用源 XML,SummaryAttribute 标记 [Obsolete]`
  - 阶段 3:`feat!: 移除 SummaryAttribute 和 SummaryTool,OdinAutoTooltip 完全替代`
- [ ] `Assets/Runestone/AesirInspector/Documentation~/en/README.md`
  - 同步英文版更新
- [ ] `Assets/Runestone/AesirInspector/Third Party Notices.md`
  - 增加 JakePineOdinTools MIT 许可记录
- [ ] `Assets/Runestone/AesirInspector/Documentation~/development.md`
  - 删 `SummaryAttribute` 条目(阶段 3 时)

---

## 9. 一句话总结

**保留 ScriptDocGenerator 的 Summary 输出口不变(向后兼容),让运行时 `[Summary]` 变成"可选"——XML 是新的唯一真相,`[Summary]` 只作为旧代码的"兼容 shim"继续工作。**
