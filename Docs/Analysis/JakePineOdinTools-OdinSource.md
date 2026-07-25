# OdinSource 详细分析

> 源文件: `JakePineOdinTools/OdinSource/Editor/OdinSourceFileHelper.cs`
> 角色: **共享底座**——所有 JakePine 插件都依赖它。给 Aesir 引入 `JakePineOdinTools` 时**必须**带上。

---

## 1. 类签名

```csharp
public static class OdinSourceFileHelper
```

- **静态类**,无实例
- 全部代码包在 `#if UNITY_EDITOR` 内 → 不会进 Runtime 构建
- **零外部依赖**(只用 `UnityEditor` + `System.IO` / `System.Text.RegularExpressions` / `System.Reflection`)

---

## 2. 公共 API 速览

按用途分组。所有方法都是 `public static` 可直接调用。

### 2.1 文件查找与缓存

| 方法 | 返回 | 作用 |
|------|------|------|
| `FindSourceFile(Type type)` | `string` (绝对路径) 或 `null` | 找类型对应的 `.cs` 源文件 |
| `GetSourceLines(Type type)` | `string[]` (按行) 或 `null` | 取源文件的所有行,自动缓存 |
| `ClearCache()` | `void` | 手动清缓存(默认 assembly reload 自动清) |

### 2.2 类型与成员结构识别

| 方法 | 返回 | 作用 |
|------|------|------|
| `GetTypeKey(Type type)` | `string`(如 `"Outer.Inner"`) | 把嵌套类型组装成点分键,用于在源里找类体 |
| `TryGetTypeBodyRange(lines, typeKey, out start, out end)` | `bool` | 在已读入的行里定位类型体的 `{` 和 `}`,返回 0-based 行号 |
| `ExtractMemberName(declarationLine)` | `string` | 从成员声明行提取标识符(自动剥 `[]` 特性、剥 `//` 注释、剥字符串字面量) |
| `IsFieldDeclarationLine(line)` | `bool` | 判断一行是不是字段声明(能区分 `x => x*2` 的 lambda 和 expression-bodied property) |
| `IsPropertyOrMethodDeclarationLine(line)` | `bool` | 判断一行是不是属性/方法声明(覆盖完整 property、`=>` 属性、`[Button]` 方法) |
| `FindMemberEndLine(lines, declStart)` | `int` (行号) | 定位成员声明的结束行(扫花括号平衡,处理跨行 method/property body) |

### 2.3 字符串与注释处理

| 方法 | 作用 |
|------|------|
| `SplitCodeAndComment(line, out code, out comment)` | 拆一行成代码部分 + 行尾 `//` 注释,**字符串字面量内的 `//` 不算注释** |
| `StripStringsAndComment(line)` | 把字符串/字符字面量内容和行尾 `//` 注释全删,只剩结构字符(用于安全扫描 `{ } [ ] ( ) ; = ,`) |
| `GetNetBraceDepthChange(line)` | 一行内 `{` 和 `}` 的净深度变化(剥掉字符串和注释后再算) |

### 2.4 大括号配对

| 方法 | 作用 |
|------|------|
| `FindOpenBraceLine(lines, declLine)` | 找类/方法声明后的 `{` 所在行(可能在同行,可能在下一行) |
| `FindMatchingCloseBrace(lines, openLine)` | 找匹配的 `}` 所在行(深度匹配) |

---

## 3. 关键实现细节

### 3.1 文件查找的回退链(从快到慢)

`FindSourceFile(Type type)` 用了 **4 级回退**,每级都比上一级更慢但更宽松:

```
1. AssetDatabase.FindAssets("TypeName t:MonoScript") + MonoScript.GetClass() 严格匹配
   ↓ 失败
2. 同样的 GUID 集合,但忽略类加载失败(用 MonoScript.name 比对)
   ↓ 失败
3. 读 .cs 文件全文,用正则扫 "class|struct|enum|interface TypeName" 匹配
   ↓ 失败
4. 整个 Project 所有 MonoScript 建索引(只建一次,缓存) → O(1) 查表
```

> 设计精髓: **频繁命中的类型走最快路径,罕见类型(generated types / partial classes 拆多文件) 走最慢兜底**。

**对 Aesir 的价值**: Aesir Inspector 的 Script Doc Generator 现在是反射取 `Type`,**完全拿不到** `[Summary]` 特性 XML 之外的源注释;以后想做"按源文件路径生成 doc",可以直接复用 `FindSourceFile`。

### 3.2 缓存策略

两层缓存:

| 缓存 | Key | Value | 失效时机 |
|------|-----|-------|----------|
| `sourceLinesCache` | `Type` | `string[]` (全文件行) | Assembly Reload |
| `typeToFileIndex` | 类型名(无命名空间) | 绝对路径 | Assembly Reload |

- **没有文件变更监听**(不用 `FileSystemWatcher`)。
- 编辑源文件后**不重读**,需要等 Unity 自动重编译 → 触发 `afterAssemblyReload` 事件 → 缓存清空。
- 编译一触发,整个缓存重置。**简单可靠**。

### 3.3 字符串与注释剥离的正确性

作者写了一个非常仔细的 `StripStringsAndComment`:

```csharp
// 简化示意
for (int i = 0; i < line.Length; i++) {
    char c = line[i];
    if (inString) {
        if (c == '\\' && i+1 < line.Length) { i++; continue; }   // 处理转义
        if (c == stringChar) inString = false;
        continue;   // 字符串内的字符不输出
    }
    if (c == '"' || c == '\'') { inString = true; stringChar = c; continue; }
    if (c == '/' && i+1 < line.Length && line[i+1] == '/') break;   // 行尾注释截断
    builder.Append(c);
}
```

**已知限制**(README 里没明说,看代码可知):

- ❌ **不处理跨行 `/* */` 块注释**——单行扫描,块注释会被当成代码
- ❌ **不处理跨行 `@"..."` 逐字字符串**——单行扫描,跨行字符串里的 `{` `}` 会被算深度
- ⚠️ `IsFieldDeclarationLine` 的注释里也提到 "keep braces balanced inside those if you use them"

**对 Aesir 的启示**: 写工具时,如果代码里大量使用 `/* ... */` 或 `@"..."` 跨行,得自己加预处理(可考虑在 `OdinSourceFileHelper` 外层包装)。

### 3.4 字段 vs 属性 vs 方法的判定

`IsFieldDeclarationLine` 和 `IsPropertyOrMethodDeclarationLine` 是**两条互斥的路径**,有非常细的边界判定:

```csharp
// IsFieldDeclarationLine 的关键检查(伪代码)
1. 剥掉 [Attribute] 前缀
2. 找 '(' 和 '=' 的位置
   - '(' 在 '=' 之前 → 这是方法签名,不是字段
3. 找 '=>' 的位置
   - '=>' 之前**没有**真实 '=' → 这是 expression-bodied 属性,不是字段
4. 找 ' get;' / ' set;' 等访问器标记 → 是 property
5. 剩下能提取出名字的 → 是字段
```

**经典坑都覆盖到了**:
- `public Func<int,int> f = x => x*2;` —— 字段初始化里的 lambda(有 `=`,保留为字段)
- `public int X => 5;` —— expression-bodied 属性(没 `=`,识别为属性)
- `public string s = "x = 5; get; set;";` —— 字符串里有关键字符,但 `StripStringsAndComment` 提前剥掉

### 3.5 跨行成员体扫描

```csharp
public static int FindMemberEndLine(string[] lines, int declStart)
{
    int depth = 0;
    bool seenBrace = false;
    for (int i = declStart; i < lines.Length; i++) {
        string code = StripStringsAndComment(lines[i]);
        for (int c = 0; c < code.Length; c++) {
            char ch = code[c];
            if (ch == '{') { depth++; seenBrace = true; }
            else if (ch == '}') {
                depth--;
                if (seenBrace && depth <= 0) return i;
            }
            else if (ch == ';' && !seenBrace && depth == 0) return i;
        }
    }
    return lines.Length - 1;
}
```

**三个状态**:
- `seenBrace = false` & `depth = 0`:还在声明部分,遇到 `;` 就停(单行字段、auto-property、expression-bodied)
- `seenBrace = true` & `depth > 0`:进入 body,等 `}` 配对
- `seenBrace = true` & `depth = 0`:body 结束,返回当前行

**这保证**:
- 完整 property(有 `get { ... }`)—— 不会把 `get` 体里的 `;` 误当成字段结束
- 块体方法(`public void M() { Debug.Log(";"); }`)—— 字符串里的 `;` 不会断
- 长字段初始化(`public List<int> x = new() { 1, 2, 3 };`)—— 不会把内部 `;` 误断

### 3.6 嵌套类型 body 范围

`TryGetTypeBodyRange` 处理 `Outer.Inner` 这样的嵌套类型:

```csharp
// 伪代码
foreach (part in typeKey.Split('.')) {
    // 从 searchLine 开始,找包含 "class/struct/enum/interface part" 的行
    // 找它的 { 和匹配 } 配对
    // 如果这是最后一段,返回 [openBraceLine, closeBraceLine]
    // 否则,下次从 openBraceLine+1 开始搜下一段
}
```

**这保证**:
- 内层类的字段不会拾取外层类的 summary
- 内层类的特性不会传播到外层类
- `OdinAutoTooltip` 和 `OdinBatch` 都依赖这个把作用域正确限定到目标类型体

---

## 4. 性能特性

| 维度 | 表现 |
|------|------|
| **首次访问某类型** | 1 次 `AssetDatabase.FindAssets` + 1 次 `File.ReadAllLines` + 1 次正则扫描 → ~ms 级 |
| **同类型后续访问** | 直接字典查表 → μs 级 |
| **Assembly Reload** | 缓存全清,下次访问重建 |
| **正则编译** | 所有 `Regex` 都标了 `RegexOptions.Compiled` → 启动时 JIT 一次 |
| **FindSourceFile 失败兜底** | 整个项目建索引,只建一次,后续 O(1) 查表 |

**和 Aesir Inspector 的 Script Doc Generator 关系**:
- ScriptDocGenerator 是反射取 `MemberInfo`,**不读源**
- `OdinSourceFileHelper` 读源,**不反射**
- 两者正交,可以并存甚至配合(用反射找类型,用 OdinSource 读该类型的源做注释/标记解析)

---

## 5. Aesir 引入时需要注意的点

### 5.1 必须保留

- 文件头版权注释 `Copyright (c) 2026 Jake Pine` —— MIT 要求
- `LICENSE.txt` 复制到 Aesir 自己的 `Third Party Notices.md` 引用区
- **不要修改源码**,原样引入便于上游升级

### 5.2 可以做的本地化(不修改源码)

Aesir 引入后,**可以**在自己的 editor 代码里**包一层**提供更友好的 API,而不是改 `OdinSourceFileHelper` 本身:

```csharp
// Aesir/Wrappers/JP/AesirJakePineSourceHelper.cs
public static class AesirJakePineSourceHelper
{
    public static string[] GetSourceLinesFor(Type type) =>
        OdinSourceFileHelper.GetSourceLines(type);

    // 未来可以在这里加:批量预处理 /* */ 块注释、跨行 @"..." 等
}
```

这样上游更新时 merge 零冲突。

### 5.3 asmdef 规划

`OdinSourceFileHelper.cs` 自身**没 asmdef**(作者按文件夹约定),放入 Aesir 后需要:

- 放到 `Editor/` 子文件夹 → 编译进 Editor asmdef
- 不能进 Runtime(它用了 `UnityEditor` API,会编译失败)

Aesir 的 `Odin Integration/...Editor.asmdef` 已经满足这个条件,直接放进对应子文件夹即可。

---

## 6. 测试建议(Aesir 引入时补)

仓库**没有自带测试**,Aesir 引入时建议在 `Tests/Editor/JakePineOdinTools/OdinSource/` 下补:

| 测试点 | 覆盖方法 |
|-------|----------|
| `FindSourceFile` 在嵌套类型/泛型/部分类(多文件)的查找 | `FindSourceFile` |
| `GetSourceLines` 缓存正确性 | `GetSourceLines` + `ClearCache` |
| `StripStringsAndComment` 对转义、混合字符串/注释、行尾注释的处理 | `StripStringsAndComment` |
| `IsFieldDeclarationLine` 对 lambda initializer / expression-bodied property / 字符串内 `;` 的判定 | `IsFieldDeclarationLine` |
| `FindMemberEndLine` 对单行字段、auto-property、块体方法、跨行字段初始化的处理 | `FindMemberEndLine` |
| `TryGetTypeBodyRange` 对嵌套类的范围正确性 | `TryGetTypeBodyRange` |
| `GetNetBraceDepthChange` 字符串里 `{` `}` 不计数 | `GetNetBraceDepthChange` |

---

## 7. 一句话总结

`OdinSource` 是个**写得非常细心的 C# 源码行扫描器**,把 Unity 项目里"按类型找源文件 + 解析类体范围 + 剥字符串和注释"这套脏活封装干净。两个上层插件(OdinAutoTooltip / OdinBatch)都靠它,**它是这套工具的核心价值所在**。
