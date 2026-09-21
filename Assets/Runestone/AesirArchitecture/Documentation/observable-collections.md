# 可观察集合（ObservableCollections 全量复刻）

> **归属声明**：本模块的可观察集合家族是 [Cysharp/ObservableCollections](https://github.com/Cysharp/ObservableCollections)（MIT）的**全量复刻**——集合语义、同步视图体系、通知载荷结构、R3 扩展方法集均按其代码逻辑移植，并按本项目规范改写命名空间与部分命名、适配 Unity 2022.3（C# 9）。上游以 NuGet 包分发，本包以源码形式内置（见包根 `Third Party Notices.md`）。
> 与上游的差异见本文末「与上游的差异」一节。

---

## 一、模块定位

框架早期只提供 `ObservableList<T>` / `ObservableDictionary<TKey, TValue>` / `ObservableHashSet<T>` 三个「轻量可观察集合」（仅 Added / Removed / Replaced / Updated / Cleared 五种变更通知）。
自本版本起，该模块升级为**上游全量能力的本地实现**：新增队列 / 栈 / 环形缓冲区 / 固定长环形缓冲区 / 交替索引列表、同步视图（含过滤器与可写视图）、`INotifyCollectionChanged` 绑定层、R3 响应式扩展，以及 Odin Inspector 调试面板。

**核心价值**：`CollectionChanged` + `SynchronizedView` 让「数据（Model）」与「表现（GameObject / UI 元素）」保持自动同步——视图只在元素增删时创建 / 销毁一次，不必每帧重建。

---

## 二、集合家族

| 类型 | 说明 |
|------|------|
| `ObservableList<T>` | 可观察列表；支持 `AddRange` / `InsertRange` / `RemoveRange` / `Move` / `Sort` / `Reverse` 的批量与排序通知 |
| `ObservableDictionary<TKey, TValue>` | 可观察字典（键值对无索引，通知索引参数固定为 -1） |
| `ObservableHashSet<T>` | 可观察集合；保留项目既有的 `ISet<T>` 集合代数操作（UnionWith 等逐项通知） |
| `ObservableQueue<T>` | 可观察队列（FIFO） |
| `ObservableStack<T>` | 可观察栈（LIFO） |
| `ObservableRingBuffer<T>` | 可观察环形缓冲区（首尾双向增删） |
| `ObservableFixedSizeRingBuffer<T>` | 定长可观察环形缓冲区（写满后自动覆盖最旧元素，适合「最多保留 N 条日志」类场景） |
| `RingBuffer<T>` | 非可观察的环形缓冲区（纯数据结构，视图侧复用） |
| `AlternateIndexList<T>` | 交替索引列表（按业务索引维护有序序列，供同步视图的过滤器索引换算使用） |

所有可观察集合统一实现 `IObservableCollection<T>`：

```csharp
public interface IObservableCollection<T> : IReadOnlyCollection<T>
{
    event NotifyCollectionChangedEventHandler<T> CollectionChanged;  // in 参数，零分配
    object SyncRoot { get; }                                          // 同步根
    ISynchronizedView<T, TView> CreateView<TView>(Func<T, TView> transform);
}
```

---

## 三、两轨通知

同一集合同时提供**两轨**变更通知，可按场景自由选用，两者互不干扰：

### 3.1 `CollectionChanged`（上游语义，推荐）

```csharp
list.CollectionChanged += (in NotifyCollectionChangedEventArgs<int> e) =>
{
    switch (e.Action)
    {
        case NotifyCollectionChangedAction.Add:
            // e.IsSingleItem ? e.NewItem : 遍历 e.NewItems（ReadOnlySpan<T>）
            break;
        case NotifyCollectionChangedAction.Remove: ...
        case NotifyCollectionChangedAction.Replace: ...
        case NotifyCollectionChangedAction.Move: ...
        case NotifyCollectionChangedAction.Reset:   // Clear / Sort / Reverse
            // e.SortOperation.IsClear / IsSort / IsReverse
            break;
    }
};
```

语义要点：

- **每次写操作都通知**——包括为已存在的键赋相同值、空集合的 `Clear`。
- **批量操作通知单次事件**：`AddRange` / `InsertRange` / `RemoveRange` 只触发一次，载荷是 `ReadOnlySpan<T>`（无逐项装箱、无中间集合）。
- **`Sort` / `Reverse` 也能通知**：以 `Reset` + `SortOperation<T>` 表达（`IsSort` / `IsReverse` / `IsClear`），视图据此做局部重排而非全量重建。
- 载荷是 `readonly ref struct` + `in` 参数传递——**分发路径零分配**。

### 3.2 轻量事件（项目既有 API，保持兼容）

`AddAddedListener` / `AddRemovedListener` / `AddReplacedListener` / `AddUpdatedListener` / `AddClearedListener`（返回 `AutoRemoveListenerHandle`）保留原有语义不变：**无变化的操作不通知**、`AddRange` 逐项通知。

> 两轨语义差异是有意的：轻量事件面向「值没变就别通知我」的常见 UI 场景；`CollectionChanged` 面向需要与上游 / 视图机制严格对齐的场景。

---

## 四、同步视图（SynchronizedView）

视图把集合元素**转换一次**为表现对象（GameObject、UI 元素、ViewModel…）并长期持有，随集合增删自动同步：

```csharp
var view = list.CreateView(x => x.ToString() + "$");   // 变换只调用一次
list.Add(10);
list.AddRange(new[] { 20, 30 });
list[0] = 99;
list.Reverse();
foreach (var v in view) { /* 99$, 30$, 20$ */ }
view.Dispose();                                        // 取消订阅
```

### 4.1 过滤器

```csharp
view.AttachFilter(x => x % 2 == 0);   // 只显示偶数（Count 返回过滤后的数量）
view.ResetFilter();                   // 移除过滤器
```

- `ISynchronizedViewFilter<T, TView>` 可自定义；`AttachFilter(Func<T, bool>)` / `AttachFilter(Func<T, TView, bool>)` 为便捷重载。
- 视图的 `Count` 是**过滤后**数量，`UnfilteredCount` 是全部数量。
- 枚举视图只得到 `TView`；需要同时拿到原值用 `Filtered` / `Unfiltered`（返回 `(T Value, TView View)`）。

### 4.2 视图事件

- `ViewChanged`（`SynchronizedViewChangedEventArgs<T, TView>`）——增删改移与 Reset。
- `RejectedViewChanged`——被过滤器拒绝的增删移（可用于「有数据但被隐藏」的提示）。
- `CollectionStateChanged`——集合整体状态变化（Blazor 类场景刷新用）。

### 4.3 可写视图与绑定层

- `CreateWritableView` / `IWritableSynchronizedView<T, TView>`——允许把视图侧修改回写到源集合。
- `ToViewList()`——把视图转成带索引的 `ISynchronizedViewList<TView>`。
- `ToNotifyCollectionChanged()`——转换为实现 `INotifyCollectionChanged` 的集合（XAML / Avalonia / WinUI 等平台的绑定入口）；可传 `ICollectionEventDispatcher` 把通知派发到 UI 线程。
- `ToNotifyCollectionChangedSlim()`（仅 `ObservableList<T>`）——共享底层数据、最省内存，但不支持 Range 操作。

---

## 五、R3 响应式集成（可选）

上游 `ObservableCollections.R3` 的扩展方法集已一并复刻，位于独立程序集 `Runestone.AesirArchitecture.R3`。

### 5.1 安装

1. 项目中存在 `R3` 程序集（推荐经 [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity) 安装 `R3`，或把 `R3.dll` 及其依赖放入 `Assets/Plugins/`）。
2. 无需手工配置宏：`EnsureAesirR3Define` 在编辑器加载时检测 `R3` 程序集，自动设置 / 移除 `AESIR_R3` 宏。
3. `AESIR_R3` 未定义时，R3 集成程序集整体不参与编译，**主包与纯代码 API 不受任何影响**。

### 5.2 集合侧扩展

```csharp
list.ObserveChanged().Subscribe(e => { });
list.ObserveAdd().Subscribe(e => { });       // CollectionAddEvent<T>(Index, Value)
list.ObserveRemove().Subscribe(e => { });
list.ObserveReplace().Subscribe(e => { });
list.ObserveMove().Subscribe(e => { });
list.ObserveReset().Subscribe(e => { });     // Clear / Sort / Reverse 汇总
list.ObserveClear().Subscribe(_ => { });
list.ObserveReverse().Subscribe(e => { });   // (Index, Count)
list.ObserveSort().Subscribe(e => { });      // (Index, Count, Comparer)
list.ObserveCountChanged().Subscribe(c => { });
dictionary.ObserveDictionaryAdd().Subscribe(e => { });
```

### 5.3 视图侧扩展

`ISynchronizedView<T, TView>` 上提供同名 `ObserveChanged` / `ObserveAdd` / `ObserveRemove` / `ObserveReplace` / `ObserveMove` / `ObserveReset` / `ObserveClear` / `ObserveReverse` / `ObserveSort` / `ObserveCountChanged`，另有 `ObserveRejected()` 订阅被过滤器拒绝的变更。

所有扩展方法均接受 `CancellationToken`。

---

## 六、Odin Inspector 调试面板（可选）

安装 Odin Inspector 后（`ODIN_INSPECTOR` 宏），本模块额外提供两个调试入口：

| 入口 | 位置 | 内容 |
|------|------|------|
| 集合浏览器窗口 | 菜单 `Tools/Aesir/Observable Collections` | 列出编辑器下存活的集合与同步视图：类型、元素数、`CollectionChanged` 订阅者数、轻量事件监听者数、关联视图数、元素预览；支持过滤与自动刷新 |
| 内联调试面板 | 任意 Inspector 中的集合字段 | 集合摘要 + 元素预览，其下保留 Odin 默认列表绘制（元素照常可编辑） |

实现方式：

- 运行时侧 `ObservableCollectionRegistry` 维护**弱引用**登记表——集合构造函数与 `CreateView` 中登记（调用点标注 `[Conditional("UNITY_EDITOR")]`，**玩家构建时调用与实参求值被编译器整体移除**，运行时零开销、零静态状态）。
- 调试信息由编辑器侧反射读取（`ObservableCollectionInspectorUtility`），因此集合的公开 API 面与上游保持一致，不为调试引入额外接口。
- 未安装 Odin 时两个入口整体不参与编译，纯代码 API 照常可用。

---

## 七、与上游的差异

| 项目 | 上游 | 本项目 | 原因 |
|------|------|--------|------|
| 命名空间 | `ObservableCollections` | `Runestone.AesirArchitecture`（内部工具在 `.Internal`，R3 扩展在 `.R3`） | 项目命名规范 |
| 抽象类命名 | `NotifyCollectionChangedSynchronizedViewList<TView>` | `AbstractNotifyCollectionChangedSynchronizedViewList<TView>` | 项目规范：抽象类加 `Abstract` 前缀 |
| `List<T>` 的 span 批量操作 | `Unsafe.As` 改写内部数组（.NET 8 走原生重载） | 逐项追加 / 先物化再插入（`Internal/ListExtensions.cs`） | Unity netstandard2.1 参考程序集不含 `System.Runtime.CompilerServices.Unsafe`；语义一致，仅批量插入多一次数组分配 |
| `CollectionsMarshal.AsSpan` | 直接取 `List<T>` 内部数组（4 处） | 索引器读改写 / 专用 `CloneCollection` 重载 | 同上 |
| `ObservableList.RemoveRange` 区间拷贝 | `CollectionsMarshal.AsSpan(list).Slice(...)` | `new CloneCollection<T>(items, index, count)` 逐项拷贝 | 同上 |
| 语言版本 | C# 12（`record struct`、主构造器、file-scoped namespace） | C# 9 等价写法（`readonly struct` + 显式构造器；R3 载荷结构体不带值相等语义） | Unity 2022.3 编译器上限 |
| `IReadOnlySet<T>` | netstandard2.0 提供内部 shim | 项目既有 `IObservableHashSet<T> : ISet<T>` 保留，另加 `IObservableCollection<T>` | 保留项目既有能力（上游的 HashSet 不实现 `ISet<T>`） |
| `ObservableList<T>` / `ObservableDictionary<,>` / `ObservableHashSet<>` | 每次写操作都通知；`AddRange` 单次批量通知 | **两轨并存**：`CollectionChanged` 同上游语义；轻量事件保留项目既有语义 | 既有用户代码零迁移 |
| `notnull` 约束 | `where TKey : notnull` / `where T : notnull` | 不添加 | 避免破坏既有可空元素类型用法 |
| 序列化字段 | `readonly List<T> list` 等 | `[SerializeField]` + Unity 原生/Odin 序列化（字段名不变） | 保留项目既有的 Inspector 初始元素编辑能力 |
| 调试面板 | 无 | 新增 Odin 集合浏览器 + 内联面板 + 弱引用注册表 | 本项目定位：ObservableCollections 与 Odin Inspector 融合 |

**语义完全对齐的部分**：通知载荷结构（`NotifyCollectionChangedEventArgs<T>` 的 Action / IsSingleItem / NewItem / OldItems / SortOperation 契约）、`SynchronizedView` 的过滤与重排行为、`INotifyCollectionChanged` 绑定层、R3 扩展方法名与载荷类型。

---

## 八、注意事项

- **线程**：集合与视图内部以 `SyncRoot` 加锁，保证「视图与集合一致」；但仅约定**主线程使用**，跨线程通知请经 `ICollectionEventDispatcher` 派发。
- **视图必须 Dispose**：视图与集合之间是事件订阅关系，不 `Dispose` 会导致集合持有视图引用（内存泄漏）。
- **锁与回调**：`CollectionChanged` 在持有集合锁的上下文内分发；回调中再写同一集合是同线程重入（合法），但会递归通知，请避免。
- **性能**：分发热路径零分配（`in` + `ref struct` + 复用缓冲）；批量操作优于逐项操作（单次通知 + 单次拷贝）。

---

## 九、测试

| 测试文件 | 覆盖 |
|----------|------|
| `Tests/Editor/ObservableListParityTests.cs` 等 8 个 `*ParityTests` | 上游测试套件移植（列表 / 字典 / 集合 / 队列 / 栈 / 环形缓冲区 / 交替索引 / `INotifyCollectionChanged` 绑定） |
| `Tests/Editor/ObservableListTests.cs` / `ObservableDictionaryTests.cs` / `ObservableHashSetTests.cs` | 项目既有轻量事件语义回归 |
| `Tests/R3/ObservableCollectionR3ExtensionsParityTests.cs` | 上游 R3 扩展测试移植（仅在 `AESIR_R3` 时编译） |
| `Tests/Editor/ObservableCollectionRegistryTests.cs` | 调试注册表登记 / 注销 / 弱引用清理 |
