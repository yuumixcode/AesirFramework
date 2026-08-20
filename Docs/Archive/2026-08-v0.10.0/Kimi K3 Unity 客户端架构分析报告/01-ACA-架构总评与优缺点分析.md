# 01 · ACA 架构总评与优缺点分析

> 本文基于对 Aesir Architecture 0.9.0 全部 45 个运行时源码文件、README、Counter-MVC / Counter-MVP 示例的逐文件阅读，所有论断均有代码事实支撑。

---

## 一、架构盘点：ACA 到底是什么

先用一张表说清这套框架的实际体量与构成：

| 维度 | 事实 |
|------|------|
| 运行时规模 | 45 个 .cs 文件，三层结构（Engine 纯 C# 核心 / Component 适配层 / Modules 辅助模块） |
| 核心模式 | MVC 优先（`IController` 入口）+ MVP 可选（`IPresenter`），CQRS 轻量版（Command 写 / Query 读） |
| 根对象 | `AbstractContext<T>`：CRTP 泛型单例，纯 C#，懒加载，两阶段初始化 |
| 权限模型 | 能力接口组合（`ICanGetModel` 等 4 个标记接口 + 扩展方法），编译期访问控制 |
| 数据流 | `ObservableValue<T>` 响应式属性 + `IReadOnlyObservableValue<T>` 只读协变视图 |
| 事件 | `MiniEvent` / `MiniEvent<T>` 零分配直调 + `AutoRemoveListenerHandle` 自动注销 |
| 引擎结合 | PlayerLoop 注入（BeforeUpdate/AfterUpdate）、MonoBehaviour 适配层、Domain Reload 安全 |
| 设计边界 | 明确不做：事件总线、Context 多实例、Command 池化/async/Undo、线程安全、View 脚手架 |

一句话概括：**ACA 是一个"权限矩阵 + 单向数据流"的教学型 MVC 框架，用极少的代码把 QFramework 的核心思想重新做了一遍，并且工程细节（Domain Reload、异常消息、内存泄漏防护）做得比同类教学框架更扎实。**

---

## 二、核心优点

### 优点 1：体量真实可信的"极简"，不是口号

45 个运行时文件，核心 Engine 层只有十几个文件。一个初学者花一个下午可以通读全部框架源码——**这对"第一个框架"定位是决定性优势**。教学框架最大的失败不是功能少，而是学生永远不知道框架"里面还有什么没讲"。ACA 的边界是看得完的。

更难得的是"设计边界"一节把**不做的事**（事件总线、多 Context、池化、Undo、线程安全）显式写成契约。这给初学者传递了一个极有价值的信号：*架构不是功能堆叠，而是取舍*。这本身就是课程内容。

### 优点 2：能力接口组合是绝佳的教学载体

```
IView       = ICanGetModel + ICanGetService
IController = ICanGetModel + ICanGetService + ICanExecuteCommand + ICanExecuteQuery
```

这套设计把"每个角色能做什么"变成了**可以打印出来贴在墙上的权限矩阵**（README 已经这么做了）。它的教学价值在于：

- **编译期强制**：View 没有 `ExecuteCommand` 扩展方法可用，写操作的入口收窄是物理的，不是口头的；
- **概念可组合**：初学者先学 4 个原子能力，再学角色 = 能力组合，认知路径是递进的；
- **扩展方法 + 显式接口实现**：框架管道（`IContextHolder.Context`）从 IntelliSense 里隐藏，学生敲 `this.` 只看到 4 个能力方法——**干净的自动补全就是干净的心智模型**。

### 优点 3：fail-fast 的异常消息是"会教人的"

`CapabilityExtensions.GetModel` 在目标未注册/未初始化时抛出的异常包含：调用者类型、目标类型、**原因猜测**（"注册顺序错误或存在循环依赖"）、**修复指引**（"请检查 Configure() 中 RegisterModel<T>() 的调用顺序"）——而且是中文的。

对目标用户（刚做完教程 demo、第一次接触框架的中文初学者）来说，**异常消息就是隐形的课程助教**。这一点上 ACA 明显优于静默返回 null 的框架，也优于只有英文框架级堆栈的框架。

### 优点 4：把初学者第一大通病——"忘了退订"——做成了基础设施

`MiniEvent.AddListener` 返回 `AutoRemoveListenerHandle`（struct，幂等 Dispose），配合 `RemoveListenerExtensions` 一行绑定 Unity 生命周期：

```csharp
Model.Count.AddListener(UpdateCountText)
           .RemoveListenerWhenGameObjectOnDestroyed(gameObject);
```

场景卸载还按 `Scene.handle` 分桶处理 additive 多场景。**内存泄漏防护不再是"记得在 OnDestroy 里退订"的口头纪律，而是 API 形态本身**。这是整个框架里对初学者最友好的一处设计，值得在课程里单独成章。

### 优点 5：纯 C# 核心 + 薄适配层，可测试性是真实的

Engine 层不依赖 MonoBehaviour：Model、Service、Command、Query、Context 都是纯 C# 对象。这意味着：

- 游戏逻辑可以脱离引擎跑 EditMode 单元测试（框架自身 34 个测试就是这么做的）；
- "架构根不是 MonoBehaviour"这个进阶概念，学生可以在框架源码里亲眼看到；
- `MonoView<T>` / `MonoViewController<T>` 只做一件事：把泛型 Context 绑到 MonoBehaviour 上——适配层薄到一行代码。

### 优点 6：Unity 原生结合的脏活框架自己扛了

- **Domain Reload 安全**：泛型类的 `[RuntimeInitializeOnLoadMethod]` 会被 Unity 静默跳过（2022.3 实测），ACA 用 `ResetStaticsAssistant` 中心化处理；非泛型类用类内 RIOLM 自重置。学生完全不需要知道这件事的存在；
- **PlayerLoop 注入**：`EnsureInjected` + Register 期自动检测补插，第三方 SDK 破坏 PlayerLoop 时也有明确的手动恢复路径；
- **初始化时序**：两阶段初始化（先注册全部，再按序初始化）+ 反向销毁（先 Service 后 Model），把"模块互相依赖时谁先谁后"的问题收口到了 Configure 一个方法里。

这些是典型的"初学者会踩一晚上、但根本不该由初学者解决"的坑。框架把它们消灭在无声处，方向正确。

### 优点 7：MVC 优先、MVP 可选的渐进结构符合学习曲线

没有把 MVP/MVVM/ECS 一股脑塞给学生，而是明确"`IController` 是推荐入口"。`MonoViewController<T>`（View+Controller 双角色）的存在，意味着**第一课可以用最少概念跑通闭环**，之后再拆 Controller、再讲 MVP——渐进式在 API 形态上兑现了，而不只是 README 里的形容词。

---

## 三、主要缺点与风险

> 以下按"对初学者伤害程度"排序，而非按工程量排序。

### 缺点 1（严重）：两个示例的写入口径不一致，直接动摇框架的核心承诺

框架在 `AbstractModel` 文档里承诺："**状态仅通过 Command 写入**"。但实际示例：

- **Counter-MVC**：View → Controller → `ExecuteCommand` → Model ✅ 符合承诺；
- **Counter-MVP**：Presenter **直接调用** `Model.Increase()`，然后**手动** `_view.UpdateCount(Model.Count.Value)` 推刷新 ❌ 绕过 Command，也绕过 ObservableValue 的响应式通知。

问题在于：**初学者唯一会照抄的东西就是示例**。两个示例给出两条不同的写入路径和两条不同的刷新路径，学生第一课就会问"到底哪种是对的"——而这个问题在课程里很难给出不尴尬的回答。这是开课前**必须**修复的问题（修法见 03 文档 P0-1）。

### 缺点 2（严重）：写入保护是约定不是结构——View 其实可以直接改 Model

`IView` 继承 `ICanGetModel`，意味着 View 里这句代码**完全可以编译通过**：

```csharp
this.GetModel<ISampleMvcCounterModel>().Increase(); // View 直接写 Model，编译器不拦
```

能力接口挡住了 `ExecuteCommand`，但挡不住 Model 接口上暴露的公共写方法。也就是说"View 不能写 Model"目前靠自觉。这是 QFramework 同款漏洞，属于教学框架的通病，不算致命，但**必须在课程和文档里显式声明为约定**，否则学生会在第 N 课"发现捷径"并全线塌方式误用。（可选的结构化解法：Model 拆读/写双接口，View 只见只读接口——见 03 文档 P1。）

### 缺点 3（中等）：README 快速开始与真实示例不匹配

README 快速开始里 View 继承 `AesirView<CounterContext>` 并以 `new CounterController(_model)` 构造注入；真实 Counter-MVC 示例继承 `MonoView<SampleMvcCounterContext>`、无参 `new SampleMvcCounterController()`。文档与代码两张皮，会让照着 README 敲代码的学生直接编译失败或困惑。发布前需要对齐到**同一种**推荐写法。

### 缺点 4（中等）：最小闭环的样板量对初学者仍显厚重

一个计数器，完整 MVC 路径需要：Context + IModel + Model + IController + Controller + 3 个 Command + View ≈ **8 个文件**。对一个刚写完单文件 MonoBehaviour demo 的学生，这是不小的第一眼冲击。

缓解路径其实已经存在（`MonoViewController<T>` 双角色可把 View/Controller 合并，省到 5 个文件），但它目前**没有作为"第一步"被显式文档化**，学生大概率直接撞上 8 文件版本。这是呈现问题，不是设计问题。

### 缺点 5（较轻）：`Register` 与 `Get` 类型参数必须一致的陷阱

按实现类注册、按接口获取（或反过来）→ 精确键匹配失败 → 抛"未注册"异常。异常消息目前是通用模板，**没有识别"你注册的实现类其实能赋值给你查的接口"这种近失情况**。初学者的典型错法恰恰是这个。属于体验级问题，改异常消息或在文档"常见陷阱"一节收录即可。

### 缺点 6（较轻）：双基类体系（`AesirView` / `MonoView`） doubling 了文档面

为兼容无 Odin 场景，每个表现层基类都有 Odin/非 Odin 两个版本。设计上是合理的（Odin 可选），但对教学意味着每次介绍基类都要解释"为什么有两个"。建议在课程中统一只讲 `Mono*` 系列，`Aesir*` 系列作为"装了 Odin 后的增强版"一句话带过。

### 缺点 7（观察项）：框架缺乏"运行时可视性"

Context 里注册了什么、每个 ObservableValue 当前值是多少——目前没有任何可视化手段。对教学来说，"让学生**看见**架构"的调试窗口价值很高（这也与 README 路线图中的 Editor 工具链吻合）。不阻塞开课，但值得排期。

---

## 四、定位匹配度结论

| 评估项 | 结论 |
|--------|------|
| 足够简单吗？ | **接近达标**。源码体量、概念数量、异常引导都达标；缺的是"最小路径"的显式呈现和示例口径统一 |
| 足够灵活吗？ | **达标**。能力接口组合 + 可选 MVP + 可替换 Loader 思路，天花板对目标用户足够高 |
| 理念自洽吗？ | **基本自洽，但示例没有完全执行自己的理念**（缺点 1/2）——框架的问题不在设计，在执行的一致性 |
| 适合作为第一个框架吗？ | **适合**。体量可读、错误会教人、泄漏有防护、原生坑已填平。修复 P0 三项后具备课程化条件 |

**总评：ACA 的设计判断力（什么该做、什么不该做）是成熟的；当前最大的短板不是架构能力，而是"示例与文档对设计意图的执行一致性"。这类问题修复成本低、收益极高，恰好在课程化之前是修复的最佳时机。**
