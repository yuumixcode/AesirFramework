# OdinBatch 详细分析

> 源文件:
> - `OdinBatch/Runtime/BatchAttributes.cs` —— 4 个标记特性
> - `OdinBatch/Editor/OdinBatchAttributeProcessor.cs` —— 核心处理器
> - `OdinBatch/Editor/OdinAttributeInstanceFactory.cs` —— attribute 实例克隆工厂
>
> 角色: **新增功能**——Aesir Inspector 目前没有对应能力,定位为 Odin 用户的高频痛点解决方案。
> 依赖: `Odin Inspector` + `OdinSource`

---

## 1. 一句话做什么

在源里用 4 个标记特性(`BatchBegin` / `BatchEnd` / `BatchDefine` / `BatchApply`)声明**哪些 attribute 应该被复制到一批成员上**。编辑器编译后打开 inspector,处理器从源里读标记,从**已编译字段上**克隆 attribute 实例,挂到目标字段上。

**示例**:

```csharp
[FoldoutGroup("Stats"), GUIColor(0.6f, 1f, 0.7f), ReadOnly, BatchBegin("stats")]
public float statOne = 10f;
public float statTwo = 20f;

[BatchEnd("stats")]
public float statThree = 30f;

public float afterStats;   // 不在批次内
```

→ `statOne` / `statTwo` / `statThree` 全部自动拥有 `FoldoutGroup("Stats")` + `GUIColor(...)` + `ReadOnly`。

---

## 2. 4 个标记特性

源码 `BatchAttributes.cs`,**全部 Runtime**(不包 `#if UNITY_EDITOR`),因为它们是用户写在源码上的 `[Attribute]`:

| 特性 | 可放位置 | 作用 |
|------|---------|------|
| `[BatchBegin("name")]` | Field / Property | **开批**。同一 `[ ]` 内的其他 attribute 是批次的集合 |
| `[BatchEnd("name")]` | Field / Property | **关批**(包含此字段)。无 name = 关闭所有 |
| `[BatchDefine("name")]` | Field / Property | **定义**一份可复用集合(只挂当前字段) |
| `[BatchApply("name")]` | Field / Property | **应用**一份已定义集合(只挂当前字段) |

**所有标记都有 `AttributeUsage.Field | Property, AllowMultiple, Inherited = true`**。

`[AttributeUsage]` 里**没有 Method**——但实际上**方法也支持**,因为处理器的字段/属性/方法识别走的是 `OdinSourceFileHelper.IsPropertyOrMethodDeclarationLine` 而不是 `AttributeUsage` 检查。

---

## 3. 处理器的核心循环

```csharp
public override void ProcessChildMemberAttributes(
    InspectorProperty property, MemberInfo member, List<Attribute> attributes)
{
    if (!ENABLED) return;
    if (member == null || member.DeclaringType == null) return;

    // 1. 永远移除 Batch 标记(它们只是"源指令",不应出现在 inspector 上)
    attributes.RemoveAll(IsBatchMarkerAttribute);

    // 2. 从源里拿这个成员应有的 attribute 列表
    List<Attribute> sourceAttributes = GetAttributesFromSource(member);
    if (sourceAttributes == null || sourceAttributes.Count == 0) return;

    // 3. 把 attribute 克隆,挂到 attributes 列表(已存在的不同类型才挂)
    for (int i = 0; i < sourceAttributes.Count; i++)
    {
        Type sourceAttributeType = sourceAttributes[i].GetType();
        bool alreadyPresent = attributes.Any(a => a?.GetType() == sourceAttributeType);
        if (!alreadyPresent && IsAttributeCompatibleWithMember(sourceAttributes[i], member))
        {
            // 注意:每次都克隆,不共享实例(Odin 会 mutate 群组 attribute)
            attributes.Add(OdinAttributeInstanceFactory.CreateFromCompiledTemplate(sourceAttributes[i]) 
                          ?? sourceAttributes[i]);
        }
    }
}
```

**三层保证**:
1. **不挂重复 type 的 attribute**——已经手动写的就保留手写的,不覆盖
2. **不挂不兼容 type 的 attribute**——比如 `RangeAttribute` 不能挂在 `string` 字段上
3. **每次都克隆**——Odin 在 group resolution 时会 mutate group 实例,共享就崩

---

## 4. 状态机:OdinBatchState

源扫描时维护一个**批次元数据字典**:

```csharp
private sealed class OdinBatchState {
    private readonly Dictionary<string, OdinAttributeBatchTemplate> defined   = new();  // 定义/开批留下的可复用模板
    private readonly Dictionary<string, OdinAttributeBatchTemplate> open      = new();  // 当前开着的批
    private readonly List<string> openOrder = new();                                    // 开的顺序(后开 = 内层)
}
```

**关键操作**:
- `Open(name, template)`:开批,同时也存入 `defined`(隐式 Define)
- `Reopen(name)`:空 payload 重开,复用之前 defined 的模板
- `Define(name, template)`:只存 defined,不开
- `End(name) / EndAll()`:关批
- `CollectOpenAttributes(list)`:把当前所有 open 的批次的 attribute 按"**内层优先**"顺序加入结果

**内层覆盖外层**:
```csharp
public void CollectOpenAttributes(List<Attribute> destination)
{
    // 倒序遍历 openOrder,最后开的 = 最内层 = 第一个加入 = 同类型时覆盖外层
    for (int i = openOrder.Count - 1; i >= 0; i--)
        AddTemplateCompiled(destination, open[openOrder[i]]);
}
```

`AddTemplateCompiled` 内有"**同 type 不重复**"的检查,所以内层的同名 attribute 加进 list 后,外层的就被自然跳过。

---

## 5. attribute 克隆:OdinAttributeInstanceFactory

源里**只识别类型名**,**所有 attribute 的"值"都从已编译字段拿**。但编译器生成的 attribute 实例**不能直接共享**(Odin 会 mutate),所以要克隆。

`CreateFromCompiledTemplate(compiledTemplate)` 三步走:

```
1. 找 attribute 的公有构造函数,按参数个数从少到多
   对每个参数,在 compiledTemplate 上找同名/同名+Name 的 property/field
   用 reflection 拿值,当 constructor 参数
   用 Activator.CreateInstance 构造
   构造成功 → 进 step 3

2. step 1 全失败 → 退到 FormatterServices.GetUninitializedObject 绕过构造

3. 不管哪一步成功,都把 compiledTemplate 的所有 instance field
   (含 private + 沿继承链往上)用 reflection 写到新实例上
```

**这套设计保证**:
- `PropertyGroupAttribute`(`FoldoutGroup`、`BoxGroup` 等)有正确的构造初始化(group 内部状态)
- `GUIColor` 的 `Color` 字段、Unity `Range` 的 readonly min/max 都被**精确复制**
- 失败的 attribute 拿不到值也不崩,直接返回 null(外层兜底用原 compiled)

**测试用例覆盖**:
- `FoldoutGroup("$" + nameof(GetDynamicGroupName))` —— Odin 的 `"$"` resolver 字符串被保留,Odin 每次重新求值
- 表达式 body 的成员:`public int Add(int a, int b) => a + b;` 也能拿到 attribute
- 字符串里有关键字符:`public string tricky = "x = 5; get; set;";` 不被误判

---

## 6. 标记规则详解

| 规则 | 说明 |
|------|------|
| **同名 batch 大小写敏感** | `"stats"` ≠ `"Stats"` |
| **空名 `""` 是合法 batch** | `[BatchBegin]` 等价于 `[BatchBegin("")]` |
| **同一 `[ ]` 内多标记** | `BatchBegin` > `BatchDefine`,**第一个**同类型胜出 |
| **`[A, B, C, BatchBegin("x")]` 才是批次集合** | 分开写 `[A] [B] [BatchBegin("x")]` 不行 |
| **`[BatchEnd]` 无名 = 关闭所有** | 包括 `""` |
| **无 `BatchEnd` = 跑到类体末尾** | 不递归进 nested type |
| **内层覆盖外层同名 attribute** | inner 优先级最高 |
| **同名 attribute 不重复挂** | 自动去重 |

---

## 7. 完整示例覆盖矩阵(来自 README)

| 场景 | 写法 |
|------|------|
| **Named batch** | `[BatchBegin("stats")]` ... `[BatchEnd("stats")]` |
| **Empty-name batch** | `[BatchBegin]` ... `[BatchEnd]` |
| **Overlapping batches** | 两个不同名的 batch 嵌套 |
| **Close all at once** | `[BatchEnd]`(无名)关所有 |
| **Define + Apply** | `[BatchDefine("ro")]` ... `[BatchApply("ro")]` |
| **Apply across region** | 用 `BatchBegin` 隐式 define,`BatchApply` 在别处用 |
| **Re-open a batch** | 单独 `[BatchBegin("stats")]`(无 payload) |
| **Overwrite inner/outer** | 同 attribute type,内层覆盖 |
| **Odin `$` resolver** | `[FoldoutGroup("$" + nameof(M))]` 保留 |
| **ToggleGroup** | `[ToggleGroup(nameof(toggleGroupEnabled), "My Toggle Group"), BatchBegin("toggle")]` |
| **Mix per-field with batch** | 字段已有自己的 attribute 跟批次叠加 |
| **No end → run to class end** | 没有 `BatchEnd` |
| **Different member types** | 字段、auto-prop、full prop、`[Button]` 方法、表达式方法都识别 |
| **Marker precedence** | 同一 `[ ]` 里 `BatchBegin` 胜 `BatchDefine` |

---

## 8. 兼容性检查

`IsAttributeCompatibleWithMember(attribute, member)` 决定一个 attribute 能不能挂到某个成员上:

```csharp
// 1. Odin attribute 例外:它用 AttributeTargets.Property/Method 不是 Field
//    因为 Odin 通过自己的 inspector property 系统处理,不靠 C# field 目标
bool isOdinAttribute = attribute.GetType().Assembly.GetName().Name.StartsWith("Sirenix.");
if (!isOdinAttribute) {
    // 非 Odin attribute 才检查 AttributeUsage
    AttributeUsageAttribute usage = attribute.GetType().GetCustomAttribute<AttributeUsageAttribute>(true);
    if (usage != null) {
        if (member is FieldInfo && !usage.ValidOn.HasFlag(AttributeTargets.Field)) return false;
        if (member is PropertyInfo && !usage.ValidOn.HasFlag(AttributeTargets.Property)) return false;
    }
}

// 2. RangeAttribute 只能挂数值类型
if (attribute is RangeAttribute) {
    return valueType != null && IsNumericType(valueType);
}

return true;
```

**对 Aesir 的启示**:
- 如果 Aesir Inspector 自定义 attribute(比如 `BilingualTitle`)要参与 batch 传播,需要在 Aesir 自己的 attribute 上**正确标记 `[AttributeUsage]`**
- `BilingualTitle` 当前标的是 `AttributeTargets.All`,自动兼容;但要小心

---

## 9. 已知限制(代码注释 + README)

1. **类/结构体本身不能标记**——`AttributeUsage` 只有 `Field | Property`,类型声明拿不到 batch attribute
2. **标记本身会被强制移除**——用户写 `[BatchBegin("x")]` 也不会在 inspector 看到
3. **不支持 Method 标 `AttributeUsage`**——但实际上 method 也能拿到(因为 `IsMemberDeclaration` 包含 method 判定)
4. **没监听文件变化**——改源后需要重编译
5. **不支持 `partial class` 跨文件**——`OdinSourceFileHelper` 只读第一个匹配的 `.cs`

---

## 10. 与 Aesir Inspector 的关系

### 10.1 Aesir 目前有没有等价能力?

**没有**。Aesir Inspector 的双语 attribute(`BilingualTitle`、`BilingualButton` 等)和 Odin 自己的 `FoldoutGroup`、`GUIColor` 都只能**手写**到每个字段上。

如果项目里有大量类似这样的代码:

```csharp
[BilingualTitle("玩家属性", "Player Stats")]
public int health;

[BilingualTitle("玩家属性", "Player Stats")]
public int mana;

[BilingualTitle("玩家属性", "Player Stats")]
public int stamina;
```

引入 `OdinBatch` 后可以:

```csharp
[BilingualTitle("玩家属性", "Player Stats"), BatchBegin("player")]
public int health;
public int mana;
public int stamina;
[BatchEnd("player")]
```

### 10.2 Aesir 引入后的潜在扩展

1. **Aesir 自家双语 attribute 自动成 batch** —— `[BilingualTitle] [BilingualButton] [BatchBegin("ui")]` 三件套
2. **Aesir Inspector 的 `Mini Tools` 可以加 "Batch Helper"** —— 可视化生成 batch 标记
3. **Aesir Architecture 的 `View/Controller/Model` 基类** 可以给派生类自动加 inspector 装饰

### 10.3 引入成本

- **代码量**:`OdinBatch` 比 `OdinAutoTooltip` 大约多 1.5 倍
- **测试**:`OdinBatch` 的状态机逻辑值得补 Edit Mode 测试(覆盖每种 batch 规则)
- **风险**:`OdinAttributeInstanceFactory` 用反射克隆 attribute,理论上有些 attribute(自定义 `IProperty` 的)可能克隆失败,需要监控

---

## 11. Aesir 引入时需要新增/调整

### 11.1 必须新增

- **Runtime asmdef 引用**:`OdinBatch/Runtime/BatchAttributes.cs` 是 Runtime,需要被 Runtime asmdef 引用
  - 现有 `AesirInspector.OdinIntegration.Runtime.asmdef` 如果有就直接放进去
  - 没有就新建 `JakePineOdinTools.OdinIntegration.Runtime.asmdef`(只 4 个 attribute 类,代价小)
- **Editor asmdef 引用**: 把 `OdinBatch/Editor/*.cs` 放进 `AesirInspector.OdinIntegration.Editor.asmdef`

### 11.2 文件放置

```
Assets/Runestone/AesirInspector/
└── (按现有结构放置)
    ├── Runtime/
    │   └── Odin Integration/
    │       └── JakePineOdinTools/
    │           └── OdinBatch/
    │               └── Runtime/
    │                   └── BatchAttributes.cs
    └── Editor/
        └── Odin Integration/
            └── JakePineOdinTools/
                ├── OdinSource/
                │   └── Editor/
                │       └── OdinSourceFileHelper.cs
                └── OdinBatch/
                    └── Editor/
                        ├── OdinBatchAttributeProcessor.cs
                        └── OdinAttributeInstanceFactory.cs
```

> **注意**: `OdinSource` 和 `OdinBatch.Editor` 是纯 Editor,`OdinBatch.Runtime` 必须在 Runtime asmdef。

### 11.3 配置建议

`OdinBatchAttributeProcessor.ENABLED` 默认 `true`,**保留默认即可**。Aesir 引入时不需要改这个值。

---

## 12. 一句话总结

**OdinBatch 是 Odin 用户的"属性复用神器"**——把多个字段共享的 inspector 装饰从 N 份复制粘贴变成 1 份 + 几个标记。Aesir Inspector 引入后,自家双语 attribute 也能立刻享受到这个能力。**和 OdinAutoTooltip 一起引入,等于给 Aesir Inspector 一次加了两个重量级 Odin 增强。**
