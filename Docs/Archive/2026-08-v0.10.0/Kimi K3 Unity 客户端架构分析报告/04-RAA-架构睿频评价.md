# 04 · RAA 架构睿频评价（锐评）

> 本轮评价不复读 01 文档的温和结论。每一条论断均经过源码二次核查，附文件级证据。
> 评价标准只有一条：**这个框架挂在嘴上的承诺，代码里兑现了没有。**

---

## 一、先给结论

RAA 是一个**"框架本体 A-，示范执行 C+"**的项目。

引擎层的功力（Domain Reload、PlayerLoop、零分配事件、生命周期收口）是商业插件级别的；但**示例、文档、接口设计对框架核心承诺的执行，处处在拆自己的台**。框架教给学生最重要的三句话——"写入必经 Command""View 只读""状态单向流动"——每一句都能在 RAA 自己的示例里找到反例。

这类问题不修复就开课，等于教练一边教"开车必须系安全带"一边自己不系。

---

## 二、优点（锐评版：好在哪里，好到什么程度）

### 1. 引擎脏活的处理是商业级的，不是教学级的

泛型类 `[RuntimeInitializeOnLoadMethod]` 被 Unity 静默跳过这种坑，绝大多数框架作者根本不知道存在。RAA 不仅知道，还用 `ResetStaticsAssistant` 把泛型/非泛型两条重置路径拆得干干净净，注释里写明"2022.3.62 实测"。**这是踩过真坑的人才写得出来的代码。**

### 2. 能力接口是近五年国产教学框架里少见的"真设计"

`ICanGetModel` 等标记接口 + 扩展方法，把访问控制压进编译期，同时用显式接口实现把框架管道从 IntelliSense 里藏起来。学生敲 `this.` 只看到四个方法。**框架的存在感被精确地控制在"需要时出现，不需要时隐身"**——这是设计品味，不是功能堆叠。

### 3. 自动注销句柄直接命中初学者死亡率最高的坑

`AddListener(...).RemoveListenerWhenGameObjectOnDestroyed(gameObject)` 一行解决"忘退订"。这个设计值得单独一课。

### 4. 克制是真的，不是营销话术

"不做事件总线、不做池化、不做 async Command、不做线程安全"逐条写进设计边界，并且代码里真的找不到这些功能的尸体。**框架作者忍住了**，这一点比任何功能都难能可贵。

---

## 三、重大问题（一定需要修改）

> 判定标准：直接违反框架自己公开承诺、或示范了会导致学生项目出事故的模式。

### ⚠️ 重大问题 1：示例 Model 接口直接暴露**可写** `ObservableValue`，框架的"只读视图"承诺被自己的第一个示例击穿

**证据**（`Samples~/Counter-MVC/Scripts/Model/ISampleMvcCounterModel.cs`）：

```csharp
public interface ISampleMvcCounterModel : IModel
{
    ObservableValue<int> Count { get; }   // ← 可写类型，不是 IReadOnlyObservableValue<int>
    void Increase();
    ...
}
```

而框架自己在三个地方承诺了完全相反的东西：

- `AbstractModel` 注释："Model 通过 `IReadOnlyObservableValue<T>` 向 View 暴露只读订阅……**View 只能观察变化，不能回写**"；
- README 核心特性："View 通过 `IReadOnlyObservableValue<out T>` 协变只读访问，保障层级安全"；
- README 快速开始的示例代码写的恰恰是 `IReadOnlyObservableValue<int> Count { get; }`（**对的**）。

但实际 Counter-MVC 示例（学生真正照抄的那份）暴露的是可写类型。后果：`this.GetModel<ISampleMvcCounterModel>().Count.Value = 999;` —— **View 一行代码直接回写 Model，连 Increase() 都不用调，Command、单向数据流、只读视图全部形同虚设。**

README 写的和示例做的不一样，而错的那一份是学生会照抄的。这是全套代码里**最严重的一处自相矛盾**，没有之一。

**修法**：示例接口改 `IReadOnlyObservableValue<int> Count { get; }`，实现类内部持有可写实例。五分钟的事，但必须改。

### ⚠️ 重大问题 2：MVP 示例的"被动视图"接口继承了 `IView`——而 `IView` 自带 `GetModel` 能力

**证据**（`ISampleMvpCounterView.cs`）：

```csharp
public interface ISampleMvpCounterView : IView   // IView = ICanGetModel + ICanGetService
{
    Action IncreaseClicked { get; set; }
    void UpdateCount(int count);
}
```

接口注释写着"被动视图（Passive View）：**不直接访问 Model**"，但 `: IView` 继承让 Presenter 手里的 `_view` 引用天然携带 `GetModel<T>()` / `GetService<T>()` 扩展方法。**"View 不访问 Model"从接口层面就不成立**——这是 MVP 模式边界的结构性破坏，不是写法问题。

叠加之前已指出的：MVP Presenter 直写 `Model.Increase()` 不走 Command。MVP 示例从"模式变体"实际沦为了"规则豁免区"。

**修法**：`ISampleMvpCounterView` 不继承 `IView`（或框架提供无能力的 `IPassiveView` 标记接口）；Presenter 写入改走 `ExecuteCommand`。

### ⚠️ 重大问题 3：两个示例的 `OnDisable` 都在用 `RemoveAllListeners()`——教学示例在示范反模式

**证据**（`SampleMvcCounterMainPanel.cs` / `SampleMvpCounterMainPanel.cs`）：

```csharp
void OnEnable()  => increaseButton.onClick.AddListener(_ctrl.Increase);   // 精确添加
void OnDisable() => increaseButton.onClick.RemoveAllListeners();          // 全部清空？
```

`RemoveAllListeners` 会清掉**所有**监听者，包括其他系统、其他框架、甚至 Unity 内部挂上去的。正确写法是 `RemoveListener(_ctrl.Increase)` 对称移除。框架自己的事件系统都以"精确注销"为核心卖点（`AutoRemoveListenerHandle`），示例却在按钮事件上教"一刀切"——学生照抄进真实项目，就是"我的某个监听莫名失效"这类灵异 bug 的源头。

**教法错误的代码比没有教程更危险**，因为学生默认示例即最佳实践。

### ⚠️ 重大问题 4：MVP 示例用 `Action` 属性而非 `event`，公开 `set` 访问器

**证据**（`ISampleMvpCounterView` / `SampleMvpCounterMainPanel`）：

```csharp
public Action IncreaseClicked { get; set; }   // 任何人可以 = 整体替换、= null 清空、外部 Invoke
```

这是 C# 事件规范的经典反面教材：`event` 关键字存在的全部意义就是限制外部只能 `+=`/`-=`。用 public setter 的 Action 属性，等于把委托链的管理权开放给所有能拿到 View 引用的人。**在一个教"角色与边界"的框架里，示例自己先把边界拆了。**

**修法**：接口改 `event Action IncreaseClicked;`（接口事件合法且实现简单）。

### ⚠️ 重大问题 5："状态仅通过 Command 写入"的承诺，在能力矩阵层面就不成立

把框架自己的文档连起来看：

| 来源 | 说法 |
|------|------|
| `AbstractModel` 注释 | "状态**仅通过 Command** 写入" |
| `IService` 注释 | "Service **能读写 Model**、调用其他 Service" |
| MVP 示例 | Presenter 直接 `Model.Increase()` |
| 能力矩阵 | View 有 `GetModel` → 拿得到 Model 写方法 |

也就是说"Command 是唯一写入入口"这句话，框架**自己就有三个官方通道可以绕过它**。Command 的真实定位其实是"**表现层**写入 Model 的推荐入口"，而不是"Model 的唯一写入入口"。这不是文字游戏：单向数据流的全部教学价值都建立在"入口唯一"上，入口不唯一，学生就会问出那句没法回答的话——"那我为什么不直接调？"

**修法**（二选一，但必须选）：
- **改叙事**：文档统一改为"Command 是 Controller/Presenter 写入 Model 的入口；Service 作为跨模块协调层可直写 Model"，并把理由讲透；
- **改结构**：Model 拆读/写双接口，写接口仅 Command 层可见（成本高，1.0 前不做就必须改叙事）。

---

## 四、明显问题（不改不致命，改了显著更好）

### 问题 6：框架根单例 `AesirArchitecture` 违反了自己的静态重置铁律

`ResetStaticsAssistant` 注释白纸黑字："**非泛型类不要使用本助手，直接在类内声明 `[RuntimeInitializeOnLoadMethod]` 重置方法即可**"。包内 `MonoLifecycleProxy`、`RemoveListenerOnSceneUnloadedTrigger`、`AesirArchitecturePlayerLoop` 都老老实实写了 `ResetStatics()`——**唯独最重要的框架根 `AesirArchitecture` 没有**。

它现在能正常工作，全靠 Unity fake-null 机制隐式救援（退出 Play 时对象销毁，`_instance != null` 自然变 false）。"碰巧正确"和"按原则正确"是两回事——铁律如果是铁律，框架自己必须先无一处例外；否则学生问"为什么这个类不用重置"时，答案只能是"因为它运气好"。

### 问题 7：初始化顺序承诺建立在 `Dictionary` 枚举顺序这个**无契约保证**的行为上

`AbstractContext.Initialize()` 承诺"按注册顺序初始化"，但 `GenericLocator` 底层是 `Dictionary<Type, T>`。字典枚举顺序不是 .NET 契约——现代运行时**碰巧**在无删除时保持插入序，但 `Unregister` 后再 `Register` 会填洞变序，且这始终是实现细节而非规范。框架把"注册顺序 = 依赖顺序"的核心时序语义押在一个随时可能变化的行为上。

**修法**：`List<T>` 保序 + `Dictionary` 做索引，十行代码，语义从此有结构保证。

### 问题 8：~~触发器组件污染学生 GameObject 的 Inspector~~（已撤销，用户裁决）

> **撤销说明**：原建议为 `HideInInspector` 隐藏触发器组件（"基础设施对业务透明"的库设计惯例）。用户否决，裁决理由成立——
> ①**可见性是教学资产**：学生亲眼看到框架自动挂载的组件，才有契机理解"自动退订"的实现机制；藏起来后"为什么这个物体不手动退订也不泄漏"就成了黑魔法；
> ②调试时需要直接在 Inspector 确认触发器挂载状态；
> ③与框架自身"fail-fast、行为显形"的哲学一致。
>
> **修正方向**：保持 Inspector 可见。组件使用语义为「**编辑器预添加为优化，运行时动态添加为兜底**」——预挂载后订阅时 `GetComponent` 直接复用，避免运行时 `AddComponent` 开销（用户二次裁决）；未预挂载时框架自动挂载仅是方便性兜底。在 `RemoveListenerOnDestroyTrigger` / `RemoveListenerOnDisableTrigger` 的类注释中按此语义说明（不写"请勿手动添加"）。对应执行计划 T1.3。

### 问题 9：三份文档三个口径，学生不知道信哪份

- CODELY.md 角色表：Model 能力是 "GetModel, **GetService**" → 实际 `IModel` 接口**没有**继承 `ICanGetService`（README 能力矩阵是对的）；
- README 快速开始：`IReadOnlyObservableValue`（对）→ 实际示例：`ObservableValue`（错）；
- README 快速开始：`AesirView` + 构造注入 Controller → 实际示例：`MonoView` + 无参构造。

框架教别人"契约与一致性"，自己的文档体系却先不一致。

### 问题 10：`Interface` 是全套框架最差的一个命名

框架最核心的入口叫 `AbstractContext<T>.Interface`——和 C# 关键字 `interface` 同词。初学者第一反应必然是"接口？哪个接口？"。且它返回 `IContext` 而非 `T`，访问子类成员还要 `((T)Interface)` 强转。一个词同时制造"术语混淆"和"类型困惑"两种成本。**1.0 API 冻结前，这是最后一个改名的窗口期**（如 `Instance` / `Context` / `It`）。

### 问题 11：示例推广"属性 getter 里做服务定位"的模式

```csharp
ISampleMvcCounterModel Model => this.GetModel<ISampleMvcCounterModel>();
```

注释的理由（支持运行时热替换）是成立的，但代价是每次访问 = 字典查找 + 初始化检查。学生极易把这个模式照抄进 `Update` 或循环体。框架应当在示例注释里**显式警告**"此写法禁止用于每帧路径"，否则等于默认分发了一个性能反模式。

### 问题 12（轻微）：`[Serializable]` + auto-property 的序列化注释误导

`SampleMvcCounterModel` 注释称 `[Serializable]` 使 Count "可在 Unity Inspector 中序列化显示"——Unity 原生**不序列化 auto-property**，能显示是因为装了 Odin。在无 Odin 环境照抄的学生会对着空白的 Inspector 发愣。注释应注明"Inspector 可见性依赖 Odin 序列化"。

---

## 五、总评

| 维度 | 评分 | 一句话 |
|------|:----:|--------|
| 引擎层功力（PlayerLoop / Domain Reload / 生命周期） | A | 商业级，踩过真坑的代码 |
| 核心设计（能力接口 / 自动注销 / 设计边界） | A- | 有品味，且真的克制 |
| 核心承诺的执行一致性 | C+ | 三句核心承诺，句句有自己的反例 |
| 示例质量 | C | 五处反模式/矛盾，且错的全是学生必抄的部分 |
| 文档一致性 | B- | README 主体优秀，但三份文档三个口径 |

**最后一句话**：RAA 的框架本体已经够格开课，但示例和文档还没够格。**学生学不到框架的 A，只会照抄示例的 C。** 五个重大问题全部是"改起来以小时计、不改则每节课都在被打脸"的类型——这正是开课前修复窗口存在的意义。
