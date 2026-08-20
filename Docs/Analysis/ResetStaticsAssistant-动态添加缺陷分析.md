# ResetStaticsAssistant「动态添加导致遍历问题」说法分析

> 分析日期：2026-08-20
> 分析对象：`Runtime/Common/ResetStaticsAssistant.cs` 的 `ResetStaticsAll()` 遍历逻辑

## 一、说法内容

有 AI 提出：`ResetStaticsAll()` 用 `foreach` 遍历 `ResetStaticsCallbacks` 时，若回调中**动态添加**新回调（`Register()`），会导致 `InvalidOperationException: Collection was modified`。

## 二、正确性判定：**技术上成立，实际概率趋近于零**

### 技术上成立

`foreach` 遍历 `List<T>` 时，若列表被修改（`Add`/`Remove`），枚举器会在下一次 `MoveNext()` 时抛出：

```csharp
foreach (var callback in ResetStaticsCallbacks)
{
    callback?.Invoke(); // 若 callback 内部调 Register() → 列表被修改 → 下次迭代抛异常
}
```

这是 .NET `List<T>` 枚举器的标准行为（版本号校验），不是误报。

### 实际概率：**趋近于零**

这个说法成立的前提是"回调执行期间调用了 `Register()`"。但看全部注册点：

| 注册方 | 注册时机 | 注册内容 |
|--------|---------|---------|
| `AbstractContext<T>` | **静态构造函数** | `_instance?.Dispose(); _instance = null;` |

**关键事实**：`ResetStaticsCallbacks` 的全部注册都发生在**静态构造函数**中——这是类型首次被引用时由运行时一次性执行的，**不是游戏运行时的动态行为**。

`ResetStaticsAll()` 的调用时机只有两处：
1. `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]` —— Unity 域加载/Play 模式入口（此时业务代码还没跑）；
2. `ResetForTests()` —— EditMode 测试的 `SetUp` 中。

在这两个时机，**静态构造函数早已执行完毕**，回调列表已稳定。回调内部（`_instance?.Dispose(); _instance = null;`）也**不会调用 `Register()`**——注册只在静态构造函数里，不在重置回调里。

**结论**：该缺陷在技术上存在，但在 ACA 的实际使用模式下（静态构造函数注册、重置回调不注册）**不会被触发**。你的判断"应该不会出现动态添加的情况"是正确的。

## 三、防御价值评估

按 ACA 极简原则（"低概率问题用约定杜绝，不加防御性代码"），**不值得为此加防御**。

但如果未来出现新的注册方（比如某个泛型模块想在重置回调里注册另一个模块的重置），这个陷阱就会从"理论"变成"实际"。当前注释里写清约定即可：

> 重置回调中禁止调用 `Register()`（会修改正在遍历的列表）。

## 四、关于「null 回调直接跳过并删除」

`callback?.Invoke()` 已经处理了 null 回调（跳过执行）。但"删除" null 回调没有必要——null 回调出现的唯一路径是 `Register(null)`，而当前没有任何代码会传 null。按极简原则，**不处理**。

## 五、结论

| 维度 | 判定 |
|------|------|
| 说法正确性 | ✅ 技术上成立（List 遍历时修改会抛异常） |
| 实际触发概率 | ❌ 趋近于零（注册在静态构造函数，回调内不注册） |
| 是否需要修复 | ❌ 不需要（极简原则：低概率问题用约定杜绝） |
| 建议 | 在 `Register()` 注释中补一条约定：「重置回调中禁止调用 Register」 |
