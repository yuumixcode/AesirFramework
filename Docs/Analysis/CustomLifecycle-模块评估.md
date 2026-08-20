# CustomLifecycle 模块评估

> 分析日期：2026-08-20
> 分析对象：`Runtime/Modules/CustomLifecycle/`（4 个文件：`ICustomLifecycle.cs`、`MonoLifecycleEvent.cs`、`MonoLifecycleProxy.cs`、`MonoLifecycleProxyExtensions.cs`）

## 一、模块定位与设计目标

**目标**：替代 MonoBehaviour 原生生命周期方法。场景中 MonoBehaviour 过多时，Unity 引擎对每个实例逐一调用 Update/FixedUpdate/LateUpdate 会产生显著的调用开销（特别是空回调），Unity 官方建议开发者自行管理生命周期、删除无效冗余的 MonoBehaviour 回调。

**设计思路**：全局单例 `MonoLifecycleProxy` 挂载在 `[Aesir Architecture]` GameObject 上，把 Unity 原生回调（Update/FixedUpdate/LateUpdate/OnApplicationXxx）+ PlayerLoop 注入（BeforeUpdate/AfterUpdate）统一成**可订阅的有序事件**。业务对象经 `Register(mono)` / `Register(obj)` 注册，销毁时自动取消订阅。

## 二、明显缺陷（按严重程度排序）

### 缺陷 1（中等）：`InvokeEvent` 在遍历时修改列表会抛异常

```csharp
void InvokeEvent(MonoLifecycleEvent evt)
{
    if (!_sortedListeners.TryGetValue(evt, out var list) || list.Count == 0) return;
    EnsureSorted();
    for (var i = 0; i < list.Count; i++)
    {
        list[i].Callback(); // ← 若回调中 RemoveListener → list.Count 变化 → i 越界
    }
}
```

**触发条件**：回调中调 `RemoveListener`（比如"执行一次后取消订阅"的自注销模式）。`list.Count` 在遍历中减小，`i++` 后 `i < list.Count` 的判断虽然不会越界（因为 `RemoveAt` 后 Count 变小），但**会跳过一个回调**（当前索引后的元素前移）。

**概率**：低-中。"执行一次后自注销"是合法且常见的用法。

**修法**：遍历时快照（`var snapshot = list.ToArray()`）或倒序遍历。但快照有分配，倒序遍历对"有序"语义有微妙影响（同 order 的先注册后执行）。**建议**：注释约定"回调中不要 RemoveListener 当前事件"（极简），或接受跳过的语义（当前行为）。

### 缺陷 2（中等）：`EnsureSorted` 每次事件触发都遍历全部字典

```csharp
void EnsureSorted()
{
    if (!_sortDirty) return;
    foreach (var kvp in _sortedListeners) // ← 遍历所有事件，而非仅当前事件
    {
        kvp.Value.Sort(...);
    }
    _sortDirty = false;
}
```

每次 `AddListener` 都置 `_sortDirty = true`，下次任意事件触发时**对全部 8 个事件的列表排序**——即使只有 1 个事件有监听。

**概率**：每次注册后首次触发都会触发。监听越多浪费越大。

**修法**：`_sortDirty` 改为按事件标记（`HashSet<MonoLifecycleEvent>` 或 `long` 位掩码），只排脏的事件。

### 缺陷 3（轻）：`RemoveListener` 的线性查找

```csharp
for (var i = list.Count - 1; i >= 0; i--)
{
    if (list[i].Callback == callback) { list.RemoveAt(i); return; }
}
```

监听多时 O(n)。但对生命周期事件（通常 < 几十个监听）完全够用，**不修**。

### 缺陷 4（轻）：`RegisterAuto` 的接口扫描依赖硬编码 if 链

```csharp
if (obj is ICustomFixedUpdate fu) handles.Add(...);
if (obj is ICustomBeforeUpdate bu) handles.Add(...);
// ... 8 个 if
```

新增生命周期接口时要改两处（接口定义 + if 链）。但对 8 个固定事件来说可控，**不修**。

### 缺陷 5（轻）：`Register(object)` 非 MonoBehaviour 对象的句柄管理靠自觉

注释已写明"调用方负责 Dispose"，但无强制。这是极简原则的有意选择，**不修**。

## 三、值得补充的能力

### 补充 1：`OnCustomDestroy` / `OnCustomDisable` 事件（可选）

当前 `MonoLifecycleEvent` 明确排除了 Awake/OnEnable/OnDisable/OnDestroy/Start（注释说"代理自身创建时这些回调仅在自身触发，外部无法有效订阅"）。这个判断对**代理自身**成立，但对**订阅者**不成立——订阅者关心的是"**我**什么时候销毁"，而不是代理什么时候销毁。

如果业务想订阅"当前场景卸载时清理"，现在只能用 `RemoveListenerWhenOnSceneUnloaded`（事件系统的触发器），和生命周期代理是两套机制。**可以考虑**补一个 `OnSceneUnloaded` 事件（经 `SceneManager.sceneUnloaded` 转发），统一入口。但按极简原则，现有 `RemoveListenerWhenOnSceneUnloaded` 已够用，**不急需**。

### 补充 2：暂停/恢复语义（`OnApplicationPause` 已有，但无 `Time.timeScale` 感知）

游戏暂停（`Time.timeScale = 0`）时，Update/FixedUpdate 仍会触发。如果业务想"暂停时不执行某逻辑"，需要自己在回调里判断 `Time.timeScale`。可以在 `MonoLifecycleEvent` 补一个 `UpdateWhenPaused` 变体，但这属于业务层封装，**框架不做**。

## 四、性能定位评估

| 维度 | 现状 | 评估 |
|------|------|------|
| 每帧开销 | 1 个 MonoBehaviour 的 Update/FixedUpdate/LateUpdate（代理自身）+ 字典查找 + 排序检查 | 远优于"场景中 N 个 MonoBehaviour 各自 Update" |
| 注册开销 | 接口扫描（8 次 is 判断）+ List.Add | 一次性，可忽略 |
| 注销开销 | O(n) 线性查找 | 监听少时可忽略 |
| GC 分配 | 排序时 `List.Sort` 无分配（原地）；快照若加则有 | 当前零分配（除排序） |

**结论**：模块达成了设计目标——把 N 个 MonoBehaviour 的生命周期调用收敛为 1 个代理的调用 + 字典分发。对"场景中 MonoBehaviour 过多导致卡顿"的场景是有效的优化。

## 五、总评

| 维度 | 判定 |
|------|------|
| 设计目标达成 | ✅ 有效收敛生命周期调用开销 |
| 明显缺陷 | 2 个中等（遍历时修改、全量排序）+ 3 个轻量 |
| 建议优先修 | 缺陷 1（遍历时修改）——补注释约定或接受跳过语义 |
| 建议可选修 | 缺陷 2（按事件标记脏）——监听多时有收益 |
| 建议补充 | 不急需（现有事件系统触发器已覆盖场景卸载） |

模块是**合格的性能优化组件**，缺陷都在"约定可杜绝"的范围内，不需要大改。
