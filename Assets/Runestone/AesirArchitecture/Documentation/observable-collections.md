# 可观察集合（ObservableCollections 轻量内置模块）

> **参考声明**：本模块的集合类型命名参考 [Cysharp/ObservableCollections](https://github.com/Cysharp/ObservableCollections)（MIT），便于对照上游文档。本模块是上游的**高频子集**而非复刻；变更通知为本项目自有约定（单轨事件、无变更不通知、批量逐项），**与上游语义不一致**。两者可在同一项目**共存**（见「与上游共存」）；需要完整能力时请直接使用上游库（见文末「与上游的关系」）。归属与许可见包根 `Third Party Notices.md`。

---

## 一、模块定位

框架面向**独立游戏**与**渐进式架构**，因此可观察集合模块只内置**最高频**的部分：

| 保留 | 理由 |
|------|------|
| `ObservableList<T>` / `ObservableDictionary<TKey, TValue>` / `ObservableHashSet<T>` / `ObservableQueue<T>` | 背包、配置表、状态集合、消息队列——独立游戏最常用的四种集合 |
| 单轨变更通知 | 一套事件语义覆盖全部集合（含 Move / Sort / Reverse），监听句柄可绑定 Unity 生命周期自动移除 |
| Odin Inspector 内联调试面板 | 初学者能直接在 Inspector 里看到集合内容与监听数量 |

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
    // 订阅返回 AutoRemoveListenerHandle，支持 using 作用域清理与 Unity 生命周期绑定
    AutoRemoveListenerHandle AddListener(Action<CollectionChangedEventArgs<T>> callback);
    void RemoveListener(Action<CollectionChangedEventArgs<T>> callback);
}
```

---

## 三、单轨变更通知

每个集合只有**一个**变更事件，经 `MiniEvent<T>` 分发（Invoke 路径零分配），载荷是普通只读结构体 `CollectionChangedEventArgs<T>`，可自由存入集合与闭包：

```csharp
var subscription = list.AddListener(e =>
{
    switch (e.Action)
    {
        case NotifyCollectionChangedAction.Add:    // e.NewItem / e.NewStartingIndex
            break;
        case NotifyCollectionChangedAction.Remove: // e.OldItem / e.OldStartingIndex
            break;
        case NotifyCollectionChangedAction.Replace:// e.NewItem + e.OldItem（旧值）
            break;
        case NotifyCollectionChangedAction.Move:   // e.NewItem = e.OldItem = 被移动元素 + 两个索引
            break;
        case NotifyCollectionChangedAction.Reset:  // 无附加字段（Clear / Sort / Reverse）
            break;
    }
});
```

语义要点：

- **无变更的写操作不通知**：Remove 不存在的元素、Clear 空集合、索引器赋相同值、Add 重复元素（HashSet）一律静默。
- **批量操作逐项通知**：`AddRange` / `InsertRange` / `RemoveRange` 每个变更元素触发一次单件事件（零中间集合、零逐项装箱）；回调次数与批量元素数成正比。
- **字典的值更新以 Replace 表达**：为已有键赋新值触发 Replace，旧值在 `OldItem`（键值对形式）。
- **`Move` 是单次 Move 事件**：`NewItem` 与 `OldItem` 均为被移动元素，两个索引分别为移动前 / 移动后位置。
- **`Sort` / `Reverse` / `Clear` 以 `Reset` 通知**：无附加字段，监听方按「重建视图」处理；少于 2 个元素的排序 / 反转视为无变化，不通知。
- **索引无概念的集合（字典 / HashSet）索引固定 -1**；列表的 Remove / Replace 携带的是**变更前**索引。
- **通知在写操作完成后发出**：回调中读取集合已是变更后的状态；监听回调不应抛异常（fail-fast 与原生事件一致）。

### 订阅生命周期

`AddListener` 返回 `AutoRemoveListenerHandle`，三种清理方式：

```csharp
// ① using 作用域结束自动移除
using (list.AddListener(e => { })) { /* ... */ }

// ② 手动 Dispose
var handle = list.AddListener(OnChanged);
handle.Dispose();

// ③ 绑定 Unity 生命周期：OnDestroy / OnDisable / 场景卸载时自动移除
list.AddListener(OnChanged).RemoveListenerWhenGameObjectOnDisable(this);
```

---

## 四、Odin Inspector 内联调试面板（可选）

安装 Odin Inspector 后（`ODIN_INSPECTOR` 宏），任意 Inspector 中的可观察集合字段上方会出现一块摘要面板：**元素数 / 变更监听数 / 元素预览**；其下保留 Odin 默认列表绘制（元素照常可编辑）。

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

### 与上游共存

两者可以在**同一项目**中同时使用：上游经 UPM（Git URL `…/ObservableCollections.git?path=src/ObservableCollections/UPM~`）或 NuGet DLL 安装，本模块随 Aesir Architecture 包分发——程序集（`ObservableCollections` vs `Runestone.AesirArchitecture`）、UPM 包名、命名空间三层完全隔离，互不引用、互不影响。

跨库同名类型（命名空间不同，共 7 个）：

| 同名类型 | 本模块命名空间 | 上游命名空间 |
|------|------|------|
| `ObservableList<T>` / `ObservableDictionary<TKey, TValue>` / `ObservableHashSet<T>` / `ObservableQueue<T>` | `Runestone.AesirArchitecture` | `ObservableCollections` |
| `IObservableCollection<T>` / `IReadOnlyObservableList<T>` / `IReadOnlyObservableDictionary<TKey, TValue>` | `Runestone.AesirArchitecture` | `ObservableCollections` |

- 本模块另有上游没有的类型：`IObservableList<T>` / `IObservableDictionary<TKey, TValue>` / `IObservableHashSet<T>` / `IReadOnlyObservableHashSet<T>` / `CollectionChangedEventArgs<T>`。
- 同一源文件同时 `using` 两个命名空间并**裸引用同名类型**时会产生 CS0104 二义性——用命名空间别名（`using AesirList = Runestone.AesirArchitecture.ObservableList<T>;`）或完全限定名解决；更推荐按模块划分文件，同一文件只 `using` 一侧。
- 其余潜在撞名点已核查无碍：上游的 `Shims/CollectionExtensions`（`AddRange(ReadOnlySpan)` / `TryGetNonEnumeratedCount`）与双方各自的 `CloneCollection<T>` 均为 **internal**，跨程序集不可见；`NotifyCollectionChangedAction` 是双方共享的 BCL 枚举（同一类型，无冲突）；上游无 Editor 程序集，本模块的 Odin 抽屉（internal）仅作用于本模块类型。

### 与上游的差异（本子集内）

| 项目 | 上游 | 本项目 |
|------|------|--------|
| 命名空间 | `ObservableCollections` | `Runestone.AesirArchitecture`（内部工具在 `.Internal`） |
| 集合数量 | 9 种（含队列 / 栈 / 环形缓冲 / 交替索引） | 4 种（List / Dictionary / HashSet / Queue） |
| 通知模型 | `CollectionChanged`（`readonly ref struct` 载荷 + `in` 参数）+ 各集合另有一套 `IEvent<T>` 轻量事件 | 单轨 `AddListener`（`MiniEvent<T>` 承载，普通 struct 载荷，返回句柄） |
| 通知语义 | 每次写操作都通知（含无变更写入）、批量操作单次事件、Sort / Reverse 携带 `SortOperation` | 无变更不通知、批量操作逐项通知、Sort / Reverse / Clear 统一 Reset 无附加字段 |
| 订阅清理 | 事件退订（`-=`）+ `IEvent<T>` 手动管理 | `AutoRemoveListenerHandle`：using / Dispose / Unity 生命周期自动移除 |
| 同步视图 | `IObservableCollection<T>.CreateView` 全套视图体系 | 不实现（需要时用上游） |
| `List<T>` 的 span 批量操作 | `Unsafe.As` 改写内部数组 | 逐项追加 / 先物化再插入（`Internal/ListExtensions.cs`）——Unity netstandard2.1 无 `Unsafe` / `CollectionsMarshal`，语义一致，批量插入多一次数组分配 |
| 语言版本 | C# 12（`record struct` / 主构造器 / file-scoped namespace） | C# 9 等价写法 |
| `IReadOnlySet<T>` | netstandard2.0 内部 shim | 保留项目既有 `IObservableHashSet<T> : ISet<T>` |
| `notnull` 约束 | `where TKey : notnull` / `where T : notnull` | 不添加（避免破坏既有可空元素用法） |
| 序列化字段 | `readonly List<T> list` | `[SerializeField]`（字段名不变，保留 Inspector 初始元素编辑） |
| 线程同步 | 内部以 `SyncRoot` 加锁 | 不加锁（仅主线程约定，见「注意事项」） |

---

## 六、注意事项

- **线程**：集合内部**不加锁**——与框架整体边界一致，仅约定**主线程使用**；需要跨线程访问时请在外部自行同步。
- **回调中重入**：在变更回调里再写同一集合是合法的，但会递归通知，请避免。
- **批量性能**：批量操作本身仍是一次内存写入 + 逐条通知；若监听方需要「整批合并处理」（如一次性刷新 UI 列表），请在回调内自行缓冲聚合。

---

## 七、测试

| 测试文件 | 覆盖 |
|----------|------|
| `Tests/Editor/ObservableCollectionChangedTests.cs` | 单轨通知语义（无变更不通知 / 批量逐项 / Move / Reset / 字典与集合的 -1 索引 / 句柄与 ClearListeners / 句柄绑定 GameObject OnDisable 自动移除） |
| `Tests/Editor/ObservableListParityTests.cs` | 列表写操作结果与 BCL `ObservableCollection<T>` 对齐（上游测试移植） |
| `Tests/Editor/ObservableListTests.cs` / `ObservableDictionaryTests.cs` / `ObservableHashSetTests.cs` | 单轨通知回归（含集合代数操作逐项通知与自差集短路） |
| `Tests/Editor/ObservableValueTests.cs` | `ObservableValue<T>` 回归 |
