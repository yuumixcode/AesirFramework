# 可观察集合

Aesir Architecture 为独立游戏内置的三种高频集合 —— `ObservableList<T>`、`ObservableDictionary<TKey, TValue>`、`ObservableHashSet<T>`,组合 BCL 集合存储 + MiniEvent 零分配事件,与 [ObservableValue](observable.md) 同一套读写分离与句柄模式。

> 类型命名参考 Cysharp/ObservableCollections(MIT),通知语义为本项目自有约定(单轨、无变更不通知、批量逐项)。队列等其他集合形态与同步视图 / R3 / 环形缓冲等高级能力可直接使用上游库 —— 两者可在同一项目**共存**,见文末[与上游的关系](#upstream)。

## 集合家族

| 集合 | 适用场景 | 专属能力 |
|------|---------|---------|
| [ObservableList\<T\>](scripting-api/Runestone/AesirArchitecture/ObservableList{T}.md) | 背包、任务列表、排行榜 | `AddRange` / `InsertRange` / `RemoveRange` / `Move` / `Sort` / `Reverse` |
| [ObservableDictionary\<TKey, TValue\>](scripting-api/Runestone/AesirArchitecture/ObservableDictionary{TKey, TValue}.md) | 配置表、属性表、名称索引 | 索引器分流(新键 Add / 已有键 Replace 含旧值) |
| [ObservableHashSet\<T\>](scripting-api/Runestone/AesirArchitecture/ObservableHashSet{T}.md) | 在线玩家、去重标记 | Add / Remove / Contains 与批量增删(0.23.0 起不再继承 `ISet<T>`,不含集合代数——需要时用内部 `HashSet<T>` 或上游) |

三者统一经 `AddListener` / `RemoveListener` 订阅变更,并各自提供 `ClearListeners()` 一次清空全部监听(已声明进 `IObservableCollection<T>` / `IReadOnlyObservableValue<T>` 接口面,只持有只读接口的 View / Presenter 同样可调)。

## 单轨变更通知

每个集合只有一个变更事件,经 `MiniEvent<T>` 分发(Invoke 路径零分配),载荷是普通只读结构体 [`CollectionChangedEventArgs<T>`](scripting-api/Runestone/AesirArchitecture/CollectionChangedEventArgs{T}.md) —— 可自由存入集合与闭包:

```csharp
// 订阅:返回 AutoRemoveListenerHandle
var handle = list.AddListener(e =>
{
    switch (e.Action)
    {
        case NotifyCollectionChangedAction.Add:     // e.NewItem / e.NewStartingIndex
        case NotifyCollectionChangedAction.Remove:  // e.OldItem / e.OldStartingIndex(变更前索引)
        case NotifyCollectionChangedAction.Replace: // e.NewItem + e.OldItem(旧值)
        case NotifyCollectionChangedAction.Move:    // 被移动元素 + 移动前后两个索引
        case NotifyCollectionChangedAction.Reset:   // 无附加字段(Clear / Sort / Reverse 共用)
            break;
    }
});
```

语义要点:

- **无变更的写操作不通知** —— 索引器赋相同值、Remove 不存在的元素、Clear 空集合、HashSet 添加重复元素,一律静默
- **批量操作逐项通知** —— `AddRange` / `InsertRange` / `RemoveRange`,每个实际变更的元素触发一次事件(零中间集合);需要整批合并处理时在回调内自行缓冲
- **写操作完成后才通知** —— 回调中读取集合已是变更后的状态;监听回调不应抛异常(fail-fast 与原生事件一致)
- **Sort / Reverse / Clear 走 Reset** —— 无附加字段,监听方按「重建视图」处理;少于 2 个元素的排序 / 反转视为无变化,不通知
- **无索引概念的集合索引固定 -1** —— 字典与 HashSet 的载荷索引恒为 -1;列表的 Remove / Replace 携带变更前索引

只读接口(`IReadOnlyObservableList<T>` 等)为**不变型**(无 `out`):结构体事件参数与协变冲突(CS1961),这是有意设计。

## 句柄生命周期

`AddListener` 返回 [AutoRemoveListenerHandle](scripting-api/Runestone/AesirArchitecture/AutoRemoveListenerHandle.md),三种清理方式按场景选用:

```csharp
// ① using 作用域结束自动移除(临时监听 / 单元测试)
using (list.AddListener(e => { })) { /* ... */ }

// ② 手动 Dispose(存句柄字段,OnDisable 里释放)
var handle = list.AddListener(OnChanged);
handle.Dispose();

// ③ 绑定 Unity 生命周期:OnDestroy / OnDisable / 场景卸载时自动移除
list.AddListener(OnChanged).RemoveListenerWhenGameObjectOnDisable(this);
```

`RemoveListenerExtensions` 家族与 [MiniEvent](observable.md) 共用同一套句柄体系 —— 一次学习,全框架适用。

## 设计边界

- **不加锁** —— 集合内部无线程同步,仅约定主线程使用;跨线程访问在调用方自行同步
- **回调中重入合法但会递归通知** —— 在变更回调里再写同一集合会再次触发通知,请避免
- **批量能力收敛** —— 仅提供列表的 Range 操作与 Move / Sort / Reverse,不做同步视图、过滤器、环形缓冲、可写视图
- **Odin 面板为可选增强** —— 安装 Odin Inspector 后,集合字段上方显示内联摘要(元素数 / 变更监听数 / 元素预览);未安装时该面板不参与编译,纯代码 API 照常可用

## 与上游的关系 {#upstream}

需要高级功能时建议使用 [Cysharp/ObservableCollections](https://github.com/Cysharp/ObservableCollections)(MIT),它提供本模块刻意不做的完整能力:

| 需求 | 建议 |
|------|------|
| 三种高频集合 + 单轨通知 | 用本模块 |
| 队列等其他集合形态 | 使用上游 |
| 同步视图与过滤器(列表驱动 GameObject / UI) | 使用上游 |
| R3 响应式(`ObserveAdd` / `ObserveSort` …) | 使用上游 + `ObservableCollections.R3` |
| 环形缓冲区 / 栈 / 交替索引列表 | 使用上游 |
| `INotifyCollectionChanged`(WPF/XAML)绑定 | 使用上游 |

**共存保证**:程序集(`ObservableCollections` vs `Runestone.AesirArchitecture`)、UPM 包名、命名空间三层完全隔离,互不引用 —— 同一项目可同时安装两库。跨库同名类型共 6 个(三种集合与 `IObservableCollection<T>` / `IReadOnlyObservableList<T>` / `IReadOnlyObservableDictionary<TKey, TValue>`);同一源文件同时 `using` 两个命名空间并裸引用同名类型时会产生 CS0104 二义性,用命名空间别名(`using AesirList = Runestone.AesirArchitecture.ObservableList<T>;`)或完全限定名解决。推荐按模块划分文件,同一文件只 `using` 一侧;两套体系不混用(上游扩展方法作用于上游类型)。

## 继续阅读

- [响应式与事件](observable.md) —— ObservableValue / MiniEvent / 生命周期机制与事件机制决策表
- [示例总览](../product/samples.md) —— ObservableCollections 示例(ContextMenu 驱动增删改查与集合运算)
- [Scripting API](scripting-api/index.md) —— 集合与接口的类型参考
