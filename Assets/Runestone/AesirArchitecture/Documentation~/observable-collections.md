# 可观察集合（ObservableCollections 轻量内置模块）

> **归属声明**：本模块的 `CollectionChanged` 通知语义与通知载荷结构**移植自** [Cysharp/ObservableCollections](https://github.com/Cysharp/ObservableCollections)（MIT）——按上游代码逻辑改写为本项目命名空间与命名规范，并适配 Unity 2022.3（C# 9）。本模块是上游的**高频子集**，不是全量复刻；需要完整能力时请直接使用上游库（见文末「与上游的关系」）。归属与许可见包根 `Third Party Notices.md`。

---

## 一、模块定位

框架面向**独立游戏**与**渐进式架构**，因此可观察集合模块只内置**最高频**的部分：

| 保留 | 理由 |
|------|------|
| `ObservableList<T>` / `ObservableDictionary<TKey, TValue>` / `ObservableHashSet<T>` / `ObservableQueue<T>` | 背包、配置表、状态集合、消息队列——独立游戏最常用的四种集合 |
| `CollectionChanged` 全语义通知 | 与上游一致的统一通知模型（含 Move / Sort / Reverse / 批量），是后续扩展的基础 |
| 既有轻量事件（`AddAddedListener` 等） | 既有代码零迁移，初学者从「四个事件」入手更易理解 |
| Odin Inspector 内联调试面板 | 初学者能直接在 Inspector 里看到集合内容与订阅数量 |

**不做**（需要时用上游库）：同步视图与过滤器（列表驱动 GameObject / UI）、R3 响应式集成、环形缓冲区 / 栈 / 交替索引列表、`INotifyCollectionChanged`（WPF/XAML）绑定层、可写视图回写、`ToViewList` / `ToNotifyCollectionChanged`。

---

## 二、集合家族

| 类型 | 说明 |
|------|------|
| `ObservableList<T>` | 可观察列表；支持 `AddRange` / `InsertRange` / `RemoveRange` / `Move` / `Sort` / `Reverse` |
| `ObservableDictionary<TKey, TValue>` | 可观察字典（键值对无索引，通知索引参数固定为 -1） |
| `ObservableHashSet<T>` | 可观察集合；保留 `ISet<T>` 集合代数操作（逐项通知） |
| `ObservableQueue<T>` | 可观察队列（FIFO） |

四者统一实现 `IObservableCollection<T>`：

```csharp
public interface IObservableCollection<T> : IReadOnlyCollection<T>
{
    event NotifyCollectionChangedEventHandler<T> CollectionChanged;  // in 参数，分发零分配
    object SyncRoot { get; }
}
```

---

## 三、两轨通知

同一集合同时提供两轨变更通知，按场景选用，互不干扰：

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
- **`Sort` / `Reverse` 也能通知**：以 `Reset` + `SortOperation<T>` 表达，消费方据此局部重排而非全量重建。
- 载荷是 `readonly ref struct` + `in` 参数传递——**分发路径零分配**。

### 3.2 轻量事件（既有 API，保持兼容）

`AddAddedListener` / `AddRemovedListener` / `AddReplacedListener` / `AddUpdatedListener` / `AddClearedListener`（返回 `AutoRemoveListenerHandle`）语义不变：**无变化的操作不通知**、`AddRange` 逐项通知。

> 两轨差异是有意的：轻量事件面向「值没变就别通知我」的常见 UI 场景，也是初学者最容易上手的一层；`CollectionChanged` 面向需要与上游严格对齐的场景。

---

## 四、Odin Inspector 内联调试面板（可选）

安装 Odin Inspector 后（`ODIN_INSPECTOR` 宏），任意 Inspector 中的可观察集合字段上方会出现一块摘要面板：**元素数 / `CollectionChanged` 订阅数 / 轻量事件监听数 / 元素预览**；其下保留 Odin 默认列表绘制（元素照常可编辑）。

- 调试信息由编辑器侧反射读取（`ObservableCollectionInspectorUtility`），集合的公开 API 面不为此增加任何接口。
- 未安装 Odin 时该面板整体不参与编译，纯代码 API 照常可用。

---

## 五、与上游的关系

| 需求 | 建议 |
|------|------|
| 列表 / 字典 / 集合 / 队列的变更通知 | 直接用本模块 |
| 同步视图与过滤器（列表驱动 GameObject / UI） | 使用上游 |
| R3 响应式（`ObserveAdd` / `ObserveSort` …） | 使用上游 `ObservableCollections` + `ObservableCollections.R3`（NuGet / UPM） |
| 环形缓冲区（定长日志、滚动窗口）、栈、交替索引列表 | 使用上游 |
| WPF / Avalonia / WinUI 的 `INotifyCollectionChanged` 绑定 | 使用上游 |
| 可写视图回写（视图侧改值回写源集合） | 使用上游 |

> 两套体系**不混用**：本模块是独立实现的轻量子集（命名空间 `Runestone.AesirArchitecture`），上游包的扩展方法作用于上游类型。

### 与上游的差异（本子集内）

| 项目 | 上游 | 本项目 |
|------|------|--------|
| 命名空间 | `ObservableCollections` | `Runestone.AesirArchitecture`（内部工具在 `.Internal`） |
| 集合数量 | 9 种（含队列 / 栈 / 环形缓冲 / 交替索引） | 4 种（List / Dictionary / HashSet / Queue） |
| 同步视图 | `IObservableCollection<T>.CreateView` 全套视图体系 | 不实现（需要时用上游） |
| `List<T>` 的 span 批量操作 | `Unsafe.As` 改写内部数组 | 逐项追加 / 先物化再插入（`Internal/ListExtensions.cs`）——Unity netstandard2.1 无 `Unsafe` / `CollectionsMarshal`，语义一致，批量插入多一次数组分配 |
| 语言版本 | C# 12（`record struct` / 主构造器 / file-scoped namespace） | C# 9 等价写法 |
| `IReadOnlySet<T>` | netstandard2.0 内部 shim | 保留项目既有 `IObservableHashSet<T> : ISet<T>` |
| `notnull` 约束 | `where TKey : notnull` / `where T : notnull` | 不添加（避免破坏既有可空元素用法） |
| 序列化字段 | `readonly List<T> list` | `[SerializeField]`（字段名不变，保留 Inspector 初始元素编辑） |

**语义完全对齐的部分**：通知载荷契约（`NotifyCollectionChangedEventArgs<T>` 的 Action / IsSingleItem / NewItem / OldItems / SortOperation）。

---

## 六、注意事项

- **线程**：集合内部以 `SyncRoot` 加锁；仅约定**主线程使用**。
- **锁与回调**：`CollectionChanged` 在持有集合锁的上下文内分发；回调中再写同一集合是同线程重入（合法），但会递归通知，请避免。
- **性能**：分发热路径零分配；批量操作优于逐项操作（单次通知 + 单次拷贝）。

---

## 七、测试

| 测试文件 | 覆盖 |
|----------|------|
| `Tests/Editor/ObservableCollectionChangedTests.cs` | `CollectionChanged` 上游语义（每次写操作通知 / 批量单次通知 / Move / Sort / Reverse / Clear / 字典与集合的 -1 索引） |
| `Tests/Editor/ObservableListParityTests.cs` | 列表写操作结果与 BCL `ObservableCollection<T>` 对齐（上游测试移植） |
| `Tests/Editor/ObservableListTests.cs` / `ObservableDictionaryTests.cs` / `ObservableHashSetTests.cs` | 项目既有轻量事件语义回归 |
| `Tests/Editor/ObservableValueTests.cs` | `ObservableValue<T>` 回归 |
