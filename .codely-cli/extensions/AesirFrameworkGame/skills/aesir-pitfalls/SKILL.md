---
name: aesir-pitfalls
description: Aesir 的十条高频陷阱清单。写 Aesir 代码报错、行为诡异、订阅收不到或提交前自查时加载——Register/Get 类型不一致、Configure 里访问 Instance、回调抛异常、忘绑自动注销句柄、RemoveAllListeners 一刀切、每帧 GetModel、运行时替换 Model。触发词：Aesir 报错、取不到 Model、订阅没反应、空引用、内存泄漏、帧率掉了、自查 Aesir 写法。
---

# Aesir 高频陷阱（10 条）

每条 = 现象 → 原因 → 修法。框架的 fail-fast 消息本身含修复指引，先读 Console 原文再对照本表。

## 1. 按实现类注册、按接口获取

- **现象**：`GetModel<T>()` 抛 `InvalidOperationException`「该 Model 未在 Context 中注册」
- **原因**：`RegisterModel<CounterModel>(...)` 按实现类注册，却 `GetModel<ICounterModel>()` 按接口查询——键精确匹配失败
- **修法**：Register 与 Get 用**相同类型参数**（默认档全用具体类；要接口就两边都接口）。异常消息含近失识别提示

## 2. 在 Configure() 或模块 OnInitialize() 中访问 Instance

- **现象**：`NullReferenceException`，或拿到第二个 Context 的实例
- **原因**：`Instance` 懒加载，`Configure` 执行期间单例尚未发布，访问会递归创建第二个上下文
- **修法**：`Configure()` 里只做 `RegisterModel/RegisterService`；模块间互相访问放 `OnInitialize()`

## 3. 注册顺序错误 / 模块循环依赖

- **现象**：`GetModel<T>()` 抛「该 Model 尚未初始化。这通常表示注册顺序错误或存在循环依赖」
- **原因**：被依赖的模块还没注册完，依赖方就先初始化了
- **修法**：`Configure()` 中**被依赖的模块先注册**（框架按注册顺序初始化 Model → Service）

## 4. 监听回调里抛异常

- **现象**：一个监听者抛异常后，同事件的后续监听者全部不执行
- **原因**：`MiniEvent` 零分配直调，异常语义 = 原生 C# 事件（fail-fast）
- **修法**：回调内不抛异常；业务异常自行 try-catch

## 5. 忘记绑定自动注销句柄

- **现象**：GameObject 销毁后仍收到通知 / 内存泄漏
- **原因**：`AddListener` 返回的 `AutoRemoveListenerHandle` 未绑生命周期
- **修法**：`.AddListener(cb).RemoveListenerWhenGameObjectOnDestroyed(gameObject)`；频繁启停的 UI 面板用 `...OnDisable(gameObject)`

## 6. OnDisable 里用 RemoveAllListeners()

- **现象**：自己的监听没了，但**别的系统**挂在同一按钮上的监听也莫名失效
- **原因**：`RemoveAllListeners` 一刀切清空所有监听者（含其他框架/插件挂的）
- **修法**：与 `OnEnable` **逐一对称**的 `RemoveListener(...)`——加什么就精确移除什么

## 7. 在 Update 等每帧路径调用 GetModel<T>()

- **现象**：帧率下降、GC 分配
- **原因**：每次访问 = 字典查找 + 初始化检查
- **修法**：`Awake`/`Start` 缓存字段 `_model = this.GetModel<T>()`；属性 getter 形式（`Model => this.GetModel<T>()`）不要用在每帧路径

## 8. 把核心配置只放在 Inspector 面板上

- **现象**：配置不可 diff、不可搜索，AI 辅助开发时读不到也改不了
- **原因**：面板点击操作难驱动、易出错
- **修法**：核心配置首选代码与资产文件（`ScriptableObject` 资产、代码常量）；Inspector 面板是调试加成

## 9. 用 Action 属性代替 event

- **现象**：外部把事件链整体替换（`view.XxxClicked = null`）或直接触发（`view.XxxClicked()`）
- **原因**：`public Action Xxx { get; set; }` 的 public setter 把委托链管理权开放给所有引用持有者
- **修法**：声明 `event Action Xxx;`——编译期限制外部只能 `+=`/`-=`

## 10. 运行时替换 Model 后期望旧订阅自动迁移

- **现象**：`RegisterModel` 替换实例后，View 收不到新实例的通知
- **原因**：替换时旧实例被 Dispose，其上的订阅**不会迁移**
- **修法**：运行时替换仅用于测试/调试；已订阅的 View 需自行重新订阅（`Model => this.GetModel<T>()` 属性形式保证始终拿到最新实例）

---

自查顺序建议：**能编译但跑不对** → 先看 1/2/3（注册与获取）；**行为时好时坏或泄漏** → 5/6/10（订阅生命周期）；**性能** → 7；**结构性** → 8/9。
