# AesirArchitecture 极简化分析与改进计划

> 分析对象：`Assets/Runestone/AesirArchitecture`（v0.8.0，2026-08-15 当前代码状态，即缺陷分析 #1–#7、#10 修复与程序集重命名之后）
> 前置文档：`Docs/AesirArchitecture-缺陷分析.md`（20 项问题清单）
> 文档日期：2026-08-15
> 核心诉求：**保持极简**。对低概率问题、或因不推荐编写方式造成的问题，应在项目前期用约定与文档直接杜绝，而不是靠额外的防御性代码兜底。

---

## 一、分析原则

评估每一段防御性代码时，依次问三个问题：

1. **它防的问题真实存在吗？** —— 概率极低（如第三方 SDK 恰好覆盖 PlayerLoop、初始化中途重入单例）的，不配拥有运行时代码。
2. **触发它需要用户写错代码吗？** —— 需要（如监听回调里抛异常、Configure 里重入 Interface），那就用文档约定禁止，让错误 fail-fast 地暴露，而不是框架替用户吞掉。
3. **它有隐性代价吗？** —— 防御代码的代价不只是行数：try-catch 吞异常掩盖根因、快照遍历引入每帧分配、自愈轮询制造"时好时坏"的不可预期行为。**防御本身可以是缺陷。**

按此原则，裁决分四档：

| 档位 | 含义 |
|------|------|
| **保留** | 修复真实缺陷、成本≈0、或属于用户明确要求的功能 |
| **简化** | 功能方向对，但实现超出必要复杂度 |
| **撤销** | 防御低概率/自找的问题，删代码、改约定 |
| **文档化** | 不写代码，用约定杜绝 |

---

## 二、现状防护层盘点与逐项裁决

### 2.1 上一轮修复引入的防护（本轮重点复查对象）

| # | 机制 | 位置 | 现状成本 | 裁决 | 理由 |
|---|------|------|---------|------|------|
| A1 | `MiniEvent.Invoke` 逐监听者 try-catch + `GetInvocationList` 快照 | `Engine/Event/MiniEvent.cs` | **每次 Invoke 分配一个委托数组，破坏了框架"零分配事件"的核心卖点**；~35 行 | **撤销** | 监听者抛异常是业务代码 bug，应 fail-fast 暴露而非吞掉。恢复 `_eventListeners?.Invoke()` 后语义 = 原生 C# 事件（异常中断后续监听者），这是所有 C# 开发者的已有心智模型，无需文档即可预期 |
| A2 | `MonoLifecycleProxy.InvokeEvent` 逐回调 try-catch | `Component/CustomLifecycle/MonoLifecycleProxy.cs` | ~8 行；Unity 本身会捕获 MonoBehaviour 回调异常并记日志，帧循环不会中断 | **撤销** | 同 A1。回调抛异常 = bug，让它自然炸出来 |
| A3 | `MonoLifecycleProxy` 每 120 帧自愈轮询 | 同上（`_selfHealFrameCounter` + Update 分支） | ~15 行 + 每 2 秒两次树遍历；更糟的是引入"注入点被覆盖后延迟约 2 秒才恢复"的**时好时坏行为**，比直接失效更难排查 | **撤销** | 第三方覆盖 PlayerLoop 是低概率事件。保留公开的 `EnsureInjected()`（幂等、~20 行），在文档中写明"若第三方 SDK 重写 PlayerLoop，请手动调用一次"即可 |
| A4 | `Register` 时懒自愈检测 | `Engine/Common/AesirArchitecturePlayerLoop.cs` | 1 行 + 注册期两次树遍历（注册发生在初始化期，非每帧） | **保留** | 一行成本换取消除绝大多数人工介入场景，且行为确定（注册即恢复，无延迟）。这是 A3 轮询的极简替代 |
| A5 | `AbstractContext.Initialize` 失败回滚（快照 + 逆序 Dispose + 逐模块 try-catch） | `Engine/Context/AbstractContext.cs` | ~55 行，含 40 行纯防御 | **简化** | 回滚 Dispose 防的是"初始化半途失败还要优雅善后"——初始化失败属于启动期编程错误，此时进程本就不该继续。**保留**半成品不缓存的核心修复，但用更简单的实现：先 `Initialize()` 成功再赋值 `_instance`（3 行 try 无需，天然不缓存）。撤销回滚机制 |
| A6 | `ObservableValue` setter 异常隔离 | `Engine/Observable/ObservableValue.cs` | 0 行（经 MiniEvent 自动获得） | 随 A1 撤销自动回退 | 值已写入、通知中断 —— 与原生 C# 属性事件语义一致 |
| A7 | `Interface` getter 失败不缓存半成品 | `Engine/Context/AbstractContext.cs` | 随 A5 简化为 0 额外行 | **保留（简化实现）** | 这是 #5 的正确修复：`var ctx = new T(); ctx.Initialize(); _instance = ctx;` —— 异常时 `_instance` 从未赋值，无需 try/catch。代价是初始化期间重入 `Interface` 会创建第二个实例并随后抛"已初始化"类异常 —— 重入属不推荐写法，文档约定禁止 |
| A8 | `ModelReplaced` / `ServiceReplaced` 替换通知 | `IContext` + `ModuleReplacedEventArgs.cs` | ~60 行（含 XML 注释） | **撤销（用户裁决）** | 原则上游戏正常运行不需要动态替换 Model/Service，该场景属测试环节——测试应自行处理订阅迁移，不徒增框架事件面。`RegisterModel`/`RegisterService` 覆盖注册仍 Dispose 旧实例 |
| A9 | `GetModel`/`GetService` 未注册抛清晰异常 | `IContext` / `AbstractContext` | ~20 行 | **保留** | 修复的是"返回 null → NRE 延迟爆发"的真实体验缺陷，且异常消息即文档 |
| A10 | `RemoveListenerOnSceneUnloadedTrigger` 按 `Scene.handle` 分桶 | `Component/Event/` | 与旧实现行数持平 | **保留** | 修复同名场景误清的真实缺陷，零额外成本 |
| A11 | `RemoveListenerExtensions` 的 `(Scene)` / `(MonoBehaviour)` 重载 | `Engine/Event/` | ~35 行 | **保留** | 解决 additive 归桶的真实易用性问题；`MonoBehaviour` 重载 5 行，是高频场景的人体工学 |
| A12 | `ResetForTests()` 测试钩子 | `Engine/Common/ResetStaticsAssistant.cs` | 8 行，`[Conditional("UNITY_INCLUDE_TESTS")]` 编译剔除 | **保留** | 测试基建必需，不进构建 |
| A13 | 预放置实例风险 Warning InfoBox ×3 | `Editor/OdinInspector/AttributeProcessors/` | 编辑期注入，0 运行时成本 | **保留** | 这正是"文档化杜绝"的正确形态：在用户写代码的地方提示，而非运行时防御 |
| A14 | `AesirArchitecture` 双模式单例（预放置优先 + 运行时创建 DDOL） | `Component/Common/` | ~30 行状态机 | **保留** | 多场景叠加是既定工作流（预放置优先），运行时创建是向后兼容。属功能而非防御 |

### 2.2 原始设计中已存在的防护

| # | 机制 | 位置 | 裁决 | 理由 |
|---|------|------|------|------|
| B1 | `AesirArchitecturePlayerLoop.InvokeHooks` 逐回调 try-catch | `Engine/Common/AesirArchitecturePlayerLoop.cs` | **撤销（可选，需确认）** | 与 A1/A2 同类：回调抛异常应 fail-fast。Unity 会捕获 PlayerLoop 系统内异常并记日志，不会崩溃。撤销后三处事件系统行为统一为"原生 C# 事件语义"。*若担心引擎行为差异，保留此一处也可接受（仅 8 行）* |
| B2 | PlayerLoop 遍历期注册的延迟命令队列（`_invoking` + `DelayedCommands`） | 同上 | **保留** | 防的是"回调内注销自己"这一**高频合法模式**（一次性监听），不是低概率防御；且实现零分配。删掉它反而逼迫用户绕路 |
| B3 | `MiniEvent` / `MonoLifecycleProxy` / `PlayerLoop` 三处排序（Order + InsertionIndex 稳定排序） | 三文件 | **保留** | 功能性契约（执行顺序可预期），有测试锁定 |
| B4 | `ResetStaticsAssistant`（原缺陷 #18 的快照修复项） | `Engine/Common/` | **撤销助手中心化（用户裁决，终版拆分）** | 非泛型单例（MonoLifecycleProxy、RemoveListenerOnSceneUnloadedTrigger）类内声明 `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]` 自重置；助手保留但收窄为仅服务泛型类——泛型类中的 RIOLM 被 Unity 静默跳过（2022.3 实测），`AbstractContext<T>` 维持经助手注册重置回调；`ContextSingletonStore` 字典存储方案一度实施后被否决 |

### 2.3 遗留未修项的极简处置（原缺陷 #8–#20）

| 原编号 | 问题 | 极简处置 | 说明 |
|--------|------|---------|------|
| #8 | 泛型静态单例无法多实例化 | **文档化** | CRTP 单例是极简选择；多存档/多房间属进阶需求，README 声明边界即可。引入 Context 容器层违背极简 |
| #9 | package.json description 仍写"类型化事件总线" | **文字修正** | 改为"轻量事件（MiniEvent）与响应式属性（ObservableValue）"，与实际能力对齐。事件总线已在 0.3.2 后删除且不回归 |
| #11 | Command/Query 无池化/async/Undo | **文档化** | 极简定位下明确"不做"：`new T()` 的分配在中小项目可忽略；需要池化/队列时用户在业务层包装。写入 README 的"设计边界"节 |
| #12 | View 层是空壳 | **文档化** | 空壳即设计：View 保持 1 行属性，脚手架交给 Aesir Modules 的 UIModule。不往 Architecture 加生命周期支架 |
| #15 | `GenericLocator.GetRegistry()` 泄漏底层字典 | **删除方法** | 公开 API 泄漏封装，注释还明说可改。删（若有外部使用再/internal 化） |
| #16 | 注册键精确匹配无保护 | **文档化** | "Register 与 Get 必须使用同一类型参数"写入 README 约定；接口级抛异常（A9）已兜住失败路径的报错体验 |
| #17 | 测试覆盖薄弱 | **补测（限定范围）** | 只补 **EditMode 纯 C# 单测**：GenericLocator（注册/覆盖/注销/清空）、ObservableValue（值比较/静默设置/AddListenerAndInvoke/Clear）、MiniEvent 原生语义。**不补**防御行为测试（防御已按本计划删除） |
| #19 | CHANGELOG `[Unreleased]` 位置不合惯例 | **文字整理** | 移至版本列表顶部 |
| #20 | 线程策略未声明 | **文档化** | README 加一句："所有框架类型仅保证主线程使用；Service 中 `Task.Run` 回调后请调度回主线程再访问框架" |

### 2.4 明确不做的事（防再次膨胀清单）

- ❌ 不做事件总线 / EventChannel 回归（#9 已裁决删除）
- ❌ 不做 Context 多实例容器
- ❌ 不做 Command 池化 / async 队列 / Undo
- ❌ 不做 View 生命周期脚手架（归 Modules）
- ❌ 不做线程安全包装
- ❌ 不做 PlayerLoop 覆盖自动轮询检测
- ❌ 不做初始化失败优雅回滚
- ❌ 不做事件回调异常吞噬

---

## 三、改进计划

> 所有撤销项在实施前需确认——其中 A1/A2/A5 撤销的是**上一轮按当时要求添加的防护**，本轮立场反转为极简优先。

### 阶段 P0：纯文字（半天）

| 任务 | 文件 |
|------|------|
| 修 description（去"类型化事件总线"） | `package.json` |
| README 增"设计边界"节：多实例边界、Command/Query 不做池化/async、Register/Get 同键约定、仅主线程、回调不抛异常约定、第三方覆盖 PlayerLoop 时手动 `EnsureInjected()`、Configure 内禁重入 `Interface` | `README.md` + `Documentation~/README_EN.md` |
| CHANGELOG `[Unreleased]` 移顶部、记录本轮极简化变更 | `CHANGELOG.md` |

### 阶段 P1：代码精简（约 −150 行运行时代码，一次提交）

| 任务 | 文件 | 预计变化 |
|------|------|---------|
| A1/A6：`MiniEvent.Invoke`/`Invoke(T)` 恢复 `_eventListeners?.Invoke()`（恢复零分配） | `Engine/Event/MiniEvent.cs` | −35 行 |
| A2：`MonoLifecycleProxy.InvokeEvent` 撤销 try-catch | `Component/CustomLifecycle/MonoLifecycleProxy.cs` | −8 行 |
| A3：删除 120 帧自愈轮询（保留 `EnsureInjected` 公开 API 与 A4 注册期检测） | 同上 | −15 行 |
| B1：`InvokeHooks` 撤销 try-catch（*待确认*） | `Engine/Common/AesirArchitecturePlayerLoop.cs` | −8 行 |
| A5/A7：`Interface` 改为"成功后赋值"；`Initialize` 撤销快照回滚，恢复顺序 foreach | `Engine/Context/AbstractContext.cs` | −55 行 |
| 扩展方法死代码清理：`GetModel`/`GetService` 的 null 分支已不可达（接口层已抛异常），仅保留 `Initialized` 校验 | `Engine/Capabilities/CapabilityExtensions.cs` | −16 行 |
| #15：删除 `GetRegistry()` | `Engine/Locator/GenericLocator.cs` | −10 行 |
| B4：`ResetStaticsAll` 快照遍历 | `Engine/Common/ResetStaticsAssistant.cs` | +1 行 |

**同步测试调整**：

- `MiniEventTests`：异常隔离 3 测试改写为原生语义断言（抛异常监听者中断后续、`Invoke` 零分配语义），或删 2 留 1
- `MonoLifecycleProxyTests`：删除 `AddListener_Update_ThrowingCallback_DoesNotPreventSubsequentCallbacks`
- `AbstractContextInitializationTests`：保留"失败不缓存半成品 + 每次重抛"，删除"回滚 Dispose 计数"断言（回滚机制撤销后 Dispose 不再被调用——未初始化完成的模块随 GC 回收）

### 阶段 P2：补测与收尾（EditMode，约 +300 行测试）

- `GenericLocatorTests`：注册/覆盖/`TryGet`/`IsRegistered`/`Unregister`/`Clear`/`Global` 与 Dispose 联动
- `ObservableValueTests`：构造/值比较跳过通知/`SetValueSilently`/`AddListenerAndInvoke`/`InvokeEvent`/`Clear`
- `MiniEventTests`（既有文件扩充）：原生事件语义、句柄 Dispose 幂等
- 运行两轮 EditMode 全量验证同域重跑稳定（沿用 `ResetForTests` 模式）

### 预期效果

| 指标 | 现状 | 目标 |
|------|------|------|
| Runtime 代码量 | 4129 行 | ~3980 行（−150） |
| `MiniEvent.Invoke` 分配 | 每次调用分配数组 | **零分配（恢复卖点）** |
| 事件系统异常语义 | 三处不一致的 try-catch 行为 | 统一 = 原生 C# 事件语义（fail-fast） |
| 防御性机制 | 回滚/轮询/异常吞噬共存 | 无轮询、无回滚、无吞噬；约定 + 1 个手动 API（`EnsureInjected`） |

---

## 四、实施前确认点（已全部确认，2026-08-15）

1. **A1/A2/B1 异常处理全撤** — ✅ 确认。监听回调抛异常中断同事件后续回调（原生 C# 事件语义，fail-fast）
2. **A5 回滚机制撤销** — ✅ 确认。初始化失败时已初始化模块不做 Dispose，随 GC 回收
3. **A3 自愈轮询撤销** — ✅ 确认。保留 `EnsureInjected()` 公开 API + Register 期自动检测
4. **#15 `GetRegistry()` 直接删除** — ✅ 确认删除（全仓库无调用方）

用户追加裁决：**A8 替换通知事件删除**（测试场景自行处理）；**B4 改为"助手收窄为仅服务泛型类"**（泛型类 RIOLM 静默失效是助手存在的根本原因；`ContextSingletonStore` 字典方案否决）。

## 五、实施结果（2026-08-15）

全部按本计划执行完毕，验证：编译 0 错误 0 警告；EditMode 测试同域两轮 **34/34 通过**；PlayMode 冒烟（proxy 事件触发、Context 单例缓存）通过。测试变化：新增 `GenericLocatorTests`（8）+ `ObservableValueTests`（7）、`MiniEventTests` 重写为原生语义（5）、`AbstractContextInitializationTests` 扩充未注册校验（6）、删除 `AbstractContextModuleReplacementTests` 与 proxy 异常隔离 PlayMode 测试。

*本文档替代"缺陷分析"作为下一轮迭代的执行依据；原 20 项清单中未在此出现的项均已在 0.8.0 修复或裁决完毕。*
