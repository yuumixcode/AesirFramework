# 架构概念

本页深入 Aesir Architecture 的核心概念:能力接口组合、Context 生命周期、注册约定与 Domain Reload 安全。

## 能力接口组合

框架不定义大而全的基类接口,而是把能力拆成细粒度接口,各角色按需组合:

| 能力接口 | 提供的能力 |
|----------|-----------|
| `ICanGetModel` | 读取已注册的 Model |
| `ICanGetService` | 读取已注册的 Service |
| `ICanExecuteCommand` | 分发 Command(写操作) |
| `ICanExecuteQuery` | 分发 Query(读操作) |
| `ICanSetContext` / `IContextHolder` | 上下文注入与持有 |
| `ICanInitialize` | 初始化契约(`Initialize()` / `Initialized`) |

角色与能力的组合关系:

| 角色 | 组合的能力 | 写入路径 |
|------|-----------|---------|
| `IModel` | GetModel | 自身持有 `ObservableValue`,仅通过写方法 / Command 修改 |
| `IService` | GetModel, GetService | 可直写 Model;**故意不能**执行 Command/Query(命令入口应由 Controller/Presenter 触发) |
| `IView` | GetModel, GetService | 只读 |
| `IView<T>` | GetModel, GetService | 只读;声明泛型参数即自动绑定 `AbstractContext<T>.Instance` 单例(默认接口实现) |
| `IController` | GetModel, GetService, ExecuteCommand, ExecuteQuery | MVC 入口 |
| `IPresenter` | 同 Controller + `IDisposable` | MVP 中介 |
| `ICommand` / `IQuery<TResult>` | GetModel, GetService + 自身执行能力 | 写 / 读分发单元 |

对应抽象基类:`AbstractModel` / `AbstractService` / `AbstractCommand` / `AbstractQuery<TResult>`(共同继承 `AbstractSubmodule`,提供 `Initialize()` / `Dispose()` 模板方法与 `OnInitialize()` / `OnDispose()` 钩子)。

## Context 生命周期

`AbstractContext<T>`(CRTP 泛型静态单例)是架构根:

```csharp
public class GameContext : AbstractContext<GameContext>
{
    protected override void Configure()
    {
        RegisterModel<IScoreModel>(new ScoreModel());
        RegisterService<ISaveService>(new SaveService());
    }
}

// 首次访问 Instance 触发懒加载初始化
var model = GameContext.Instance.GetModel<IScoreModel>();
```

执行顺序:

1. **首次访问 `Instance`** → 创建实例并调用 `Initialize()`(幂等)
2. `Configure()` —— 在此注册全部 Model / Service
3. 按**注册顺序**初始化所有 Model → 再按注册顺序初始化所有 Service
4. `Dispose()` 时**逆序**销毁:Service → Model(容器内同序逆序)

!!! warning "初始化期的两条铁律"

    - **`Configure()` 与 `OnInitialize()` 中禁止访问 `Instance`** —— 会递归创建第二个上下文实例。模块间互相访问放到 `OnInitialize()` 之后的阶段(此时全部模块已注册完毕)
    - **`Register` 与 `Get` 必须使用相同类型参数** —— 按实现类注册、按接口获取(或反过来)会命中"未注册"异常;异常消息含近失识别提示

## 注册与获取的失败语义

框架采用 **fail-fast**,不返回 null、不静默兜底:

| 场景 | 行为 |
|------|------|
| `GetModel<T>()` 未注册 | 抛 `InvalidOperationException`,消息含修复提示与近失识别 |
| 获取已注册但尚未初始化的模块 | 抛异常,提示注册顺序错误或循环依赖 |
| 同类型重复 `RegisterModel`(动态替换) | 输出 Warning,Dispose 旧实例;**旧实例上的事件订阅不会迁移** |
| 初始化失败 | 不缓存半成品实例,根因异常每次抛出(`_instance` 仅在成功后赋值) |

## Domain Reload 安全(铁律)

静态变量全部显式重置,反复进出 Play Mode(含禁用 Domain Reload)无残留:

- **非泛型类**:类内 `[RuntimeInitializeOnLoadMethod(SubsystemRegistration)]` 自重置
- **泛型类**:静态构造函数经 `ResetStaticsAssistant.Register()` 注册重置回调(泛型类中的 RIOLM 会被 Unity 静默跳过,助手补位)

`AesirPlayerLoop` 同样在 SubsystemRegistration 阶段自动注入 PlayerLoop;第三方 SDK 覆盖 PlayerLoop 后由 `EnsureInjected()` 自愈(域加载时与每次注册时自动检测;检测按帧节流,同一帧内至多复查一次,也可手动调用)。

## DDOL 显式决策

根单例 `AesirArchitecture` 的 `dontDestroyOnLoad` 序列化字段统一控制预放置 / 运行时两种来源:

- 默认勾选:加入 DontDestroyOnLoad 场景,跨场景持久
- 取消勾选:随所在场景卸载销毁(Inspector 显示警告信息框),多场景叠加加载自行处理

单例 `Instance` 获取优先 `FindAnyObjectByType(FindObjectsInactive.Include)` 搜索场景中预放置的实例(含未激活对象——"先禁用、用到时再启用"式预放置同样被发现),未找到才运行时创建。

## 纯 C# 核心 + MonoBehaviour 适配层

Engine 层(Context、角色、Command/Query)零 MonoBehaviour 依赖;表现层按需选适配基类:

| 基类 | 角色 | Odin | 用途 |
|------|------|------|------|
| `MonoView<T>` | `IView<T>` | 否 | MVC Standard / Strict 的 View |
| `MonoViewController<T>` | `IView<T>` + `IController<T>` | 否 | MVC Quick 的 View 兼 Controller |
| `AesirView<T>` | `IView<T>` | 是 | 同 MonoView,Odin 序列化增强 |
| `AesirViewController<T>` | `IView<T>` + `IController<T>` | 是 | 同 MonoViewController,Odin 增强 |

泛型接口 `IView<T>` / `IController<T>` / `IPresenter<T>` 经默认接口实现(DIM)把 `IContextHolder.Context` 绑定到 `AbstractContext<T>.Instance`:声明泛型参数即完成绑定,无需手写显式实现;View 角色保持非泛型 `IView` 声明,VC 适配对同时实现 `IView<T>` 与 `IController<T>`。

另有 `AesirMonoBehaviour` / `AesirScriptableObject` 架构感知基类:Odin 存在时继承 `SerializedMonoBehaviour` / 对应 SO 版本,否则继承原生基类(条件编译,核心程序集不引用 Odin 程序集)。

## InternalContext 特性

`[InternalContext]` 标记框架内部 Context(示例 / 测试等非用户工作流),Aesir Modules Binder 的「Context 类型」选择器会跳过被标记的类型,避免示例 Context 污染业务项目的下拉列表。

## 设计边界(极简原则)

框架明确**不做**的事:

- **事件总线 / EventChannel** —— 跨模块通信用互相 `GetModel` + `ObservableValue` 订阅,或直接引用 MiniEvent
- **Context 多实例** —— CRTP 泛型单例;多存档 / 多房间在业务层建模
- **Command/Query 池化、async、队列、Undo/Redo** —— 保持同步、无缓存
- **View 生命周期脚手架** —— 面板生命周期由 Aesir Modules 的 UIModule 负责
- **线程安全** —— 仅保证主线程使用
- **低概率问题的运行时防御** —— 用文档约定与编辑期提示(InfoBox)杜绝,不加防御性代码(防御本身有吞异常、分配、时好时坏等隐性代价)

## 继续阅读

- [响应式与事件](observable.md) —— ObservableValue / MiniEvent / 生命周期机制
- [示例总览](../product/samples.md) —— 从六档计数器示例看这些概念的落地
- [FAQ](../faq.md) —— 高频陷阱与修法
