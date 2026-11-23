---
title: Scripting API
description: "Runestone.AesirModules 系列命名空间的 Scripting API 参考"
---

# Scripting API

本参考由 Script Doc Generator 基于 C# 反射离线生成，重新生成时自动保留各页面 `## Additional Notes` 之后的手写补充。页面按字母序排列；使用左侧目录或站内搜索定位类型。

## Runestone.AesirModules

命名空间 `Runestone.AesirModules` · 程序集 `Runestone.AesirModules`、`Runestone.AesirModules.Editor.OdinInspector`、`Runestone.AesirModules.OdinInspector`、`Runestone.AesirModules.UniTask` · 共 70 个类型。

### 类

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AddressablesSupportDisabledException`](<Runestone/AesirModules/AddressablesSupportDisabledException.md>) | 当前项目未安装 Addressables 包时访问了 Addressables 相关 API。 |
| [`AesirBasePanel`](<Runestone/AesirModules/AesirBasePanel.md>) | UI 面板基类。子类覆写生命周期虚方法，通过 Context 访问 Model/Service。 |
| [`AesirBasePanelView<T>`](<Runestone/AesirModules/AesirBasePanelView{T}.md>) | 面板视图基类（MVP 模式的 View 层）。 泛型参数 T 指定面板关联的 Context 类型， Context 作为 Model 和 Service 的聚合容器，在面板与… |
| [`AesirBasePanelViewController<T>`](<Runestone/AesirModules/AesirBasePanelViewController{T}.md>) | 面板视图控制器基类（MVC 模式的 Controller 层，兼 View 职责）。 泛型参数 T 指定面板关联的 Context 类型， Context 作为 Model 和… |
| [`AesirBaseWindow`](<Runestone/AesirModules/AesirBaseWindow.md>) | Canvas 根 UI 窗口基类。窗口预制体根节点自带 Canvas（独立渲染根）， 由 UIModule 实例化后直接挂载到 UIRoot 下，按 SortingOrder … |
| [`AesirBaseWindowView<T>`](<Runestone/AesirModules/AesirBaseWindowView{T}.md>) | 窗口视图基类（MVP 模式的 View 层，Canvas 根窗口形态）。 泛型参数 T 指定窗口关联的 Context 类型， Context 作为 Model 和 Servi… |
| [`AesirBaseWindowViewController<T>`](<Runestone/AesirModules/AesirBaseWindowViewController{T}.md>) | 窗口视图控制器基类（MVC 模式的 Controller 层兼 View 职责，Canvas 根窗口形态）。 泛型参数 T 指定窗口关联的 Context 类型， Contex… |
| [`AesirEventArgs`](<Runestone/AesirModules/AesirEventArgs.md>) | 事件参数基类。所有自定义事件参数继承此类，作为事件数据载体在 EventModule 中传递。 注意：AesirEventArgs 本身不持有监听者，仅作为参数实例。 订阅管理… |
| [`AesirEventArgsSO`](<Runestone/AesirModules/AesirEventArgsSO.md>) | AesirEventArgs 的 ScriptableObject 包装，让事件可保存为 .asset 资源， 在 Inspector 中配置载荷并由非程序员触发。 通过 Pr… |
| [`AesirEventDiagnostics`](<Runestone/AesirModules/AesirEventDiagnostics.md>) | 事件模块诊断门面：对外暴露运行时订阅状态查询与事件链路推送，全部为公开 API。 设计定位：数据层与视图层解耦的唯一契约。本类只依赖运行时类型，不引用任何编辑器或 Odin 代… |
| [`AesirEventUtility`](<Runestone/AesirModules/AesirEventUtility.md>) | 事件模块静态工具方法：Unity null 检测与 GameObject 解析， 供订阅者过滤器与诊断门面解析发布者/订阅者。 |
| [`AesirModules`](<Runestone/AesirModules/AesirModules.md>) | Aesir Modules 接入 MonoBehaviour 生命周期的持久化物体对象。 |
| [`AesirModulesDebug`](<Runestone/AesirModules/AesirModulesDebug.md>) | — |
| [`AudioConfigSO`](<Runestone/AesirModules/AudioConfigSO.md>) | 音频模块配置资产。创建路径：Assets → Create → Aesir Modules → Audio → AudioConfig。 配置默认音量、音量持久化开关与 Pla… |
| [`AudioModule`](<Runestone/AesirModules/AudioModule.md>) | 音频管理器（MonoBehaviour 单例）—— 2D 音频极简门面。 负责 SFX 轮询播放、BGM 循环与淡入淡出、三通道音量/静音控制与持久化。 公开 API 全部为静… |
| [`BinderAssistant`](<Runestone/AesirModules/BinderAssistant.md>) | Object Binder 核心组件。挂载在根 UI 物体（面板或 Canvas 根窗口）上，统一配置所有要绑定的子组件并一键生成绑定脚本。 工作流程： 1. 在需要绑定引用的… |
| [`BinderBaseTypeAttribute`](<Runestone/AesirModules/BinderBaseTypeAttribute.md>) | 标记一个 MonoBehaviour 派生类可作为 Binder 生成脚本的基类（用户自定义基类的扩展入口）。 Aesir 面板家族（AesirBasePanel、AesirB… |
| [`BinderCodeGenerator`](<Runestone/AesirModules/BinderCodeGenerator.md>) | Binder 代码生成器。只做纯文本拼装与校验，不接触 Unity 对象， 由 BinderAssistant 收集场景数据后调用，便于单元测试。 |
| [`BinderEditorSettings`](<Runestone/AesirModules/BinderEditorSettings.md>) | Binder 编辑器持久化设置（ScriptableSingleton）：保存 partial 分部类模式的可选文件后缀列表、 默认后缀与最近使用的命名空间，供新建 Binde… |
| [`BinderHierarchyUtility`](<Runestone/AesirModules/BinderHierarchyUtility.md>) | 场景层级路径工具类，用于 Object Binder 计算物体在层级中的路径。 提供绝对路径和相对路径两种计算方式： - 绝对路径：从场景根物体到目标的完整路径，用于 Bind… |
| [`BinderInfo`](<Runestone/AesirModules/BinderInfo.md>) | 绑定单元数据。描述一条要绑定到生成脚本字段的组件引用信息。 在 BinderAssistant 的「绑定单元列表」中以表格形式配置， 由「构建绑定单元」按子物体上的 Binde… |
| [`BinderMenuItems`](<Runestone/AesirModules/BinderMenuItems.md>) | Binder 层级右键菜单快捷入口: 为选中物体快速挂载 BinderAssistant 与 BinderTag， 免去 Add Component 菜单的层层查找。 |
| [`BinderTag`](<Runestone/AesirModules/BinderTag.md>) | Binder 标签组件。挂载在需要自动绑定引用的子物体上，标记该物体可被 BinderAssistant 扫描并生成绑定信息。 一个物体上可绑定多个不同类型的组件，通过 Com… |
| [`BindingInfo`](<Runestone/AesirModules/BindingInfo.md>) | 绑定信息基类：Script 订阅的公共部分（订阅者、优先级、事件类型与诊断成员名）。 internal 实现细节：订阅/退订必须经 EventModule 公开 API 走同一… |
| [`EmptySceneAssetWrapperException`](<Runestone/AesirModules/EmptySceneAssetWrapperException.md>) | 访问了未分配任何场景的 SceneAssetWrapper。 |
| [`EventModule`](<Runestone/AesirModules/EventModule.md>) | 事件模块（MonoBehaviour 单例）。 通过 AddListener{TEventArgs}(object, Action{TEventArgs}) 建立 Script… |
| [`ExcludeSubclassSelectorAttribute`](<Runestone/AesirModules/ExcludeSubclassSelectorAttribute.md>) | 排除特性。标记不想出现在 SubclassSelectorAttribute 下拉中的类型 （如抽象中间层、仅供程序内部使用的事件参数）。 |
| [`InsideCollider2D`](<Runestone/AesirModules/InsideCollider2D.md>) | 仅投递给位于发布者 Collider2D 范围内的订阅者。用于空间局域广播（如爆炸半径）， 订阅者位置取其 Transform.position。发布者挂多个 Collider… |
| [`OnlySelf`](<Runestone/AesirModules/OnlySelf.md>) | 仅投递给发布者自身、其子树或其父级链上的订阅者。用于"只影响自己和亲属"的局域事件 （如组件通知所在层级，不波及场景中其他对象）。 |
| [`ResourcesUILoader`](<Runestone/AesirModules/ResourcesUILoader.md>) | 默认加载器：从 Resources 路径加载面板预制体。 |
| [`SameSceneAsEmitter`](<Runestone/AesirModules/SameSceneAsEmitter.md>) | 仅投递给与发布者同场景的订阅者。适配多场景叠加加载工作流， 防止持久场景中的订阅者收到临时场景的局域事件。 |
| [`SceneAssetWrapper`](<Runestone/AesirModules/SceneAssetWrapper.md>) | 可序列化的场景引用，支持在编辑器中拖拽 SceneAsset 赋值，等价于 Eflatun.SceneReference（SceneReference 类型）+ Odin In… |
| [`SceneAssetWrapperAddressablesBridge`](<Runestone/AesirModules/SceneAssetWrapperAddressablesBridge.md>) | Addressables 编辑器能力的静态桥。 核心程序集不引用任何 Addressables 程序集；由可选程序集 Runestone.AesirModules.Editor… |
| [`SceneAssetWrapperCreationException`](<Runestone/AesirModules/SceneAssetWrapperCreationException.md>) | 通过工厂或构造方法创建 SceneAssetWrapper 时入参无效。 |
| [`SceneAssetWrapperException`](<Runestone/AesirModules/SceneAssetWrapperException.md>) | 所有 SceneAssetWrapper 相关异常的基类，便于调用方统一捕获。 |
| [`SceneModule`](<Runestone/AesirModules/SceneModule.md>) | 场景加载与叠加管理模块（MonoBehaviour 单例）—— 公开 API 全部为静态成员，经 Instance 单例转发。 语义对齐 Unity 原生 LoadSceneM… |
| [`SceneModuleConfigSO`](<Runestone/AesirModules/SceneModuleConfigSO.md>) | 场景模块全局配置（单例资产）。承载无需预放置 [SceneModule] 即可调整的模块级配置： 在 Project 窗口直接编辑本资产即可生效，不再要求预放置 SceneMo… |
| [`SceneModuleUniTask`](<Runestone/AesirModules/SceneModuleUniTask.md>) | SceneModule 的 UniTask 适配 API——游戏工程包含 UniTask 时（宏 AESIR_MODULES_UNITASK 自动维护）， 提供可 await … |
| [`SceneNotAddressableException`](<Runestone/AesirModules/SceneNotAddressableException.md>) | 对非 Addressable 场景的 SceneAssetWrapper 访问了 Address。 |
| [`ScriptBindingInfo<TEventArgs>`](<Runestone/AesirModules/ScriptBindingInfo{TEventArgs}.md>) | Script 订阅绑定。持有 Action{T} 委托，注册后直接调用—— 无反射、无表达式树、无参数数组，分发热路径只是一次虚调用加一次委托调用。 |
| [`SubclassSelectorAttribute`](<Runestone/AesirModules/SubclassSelectorAttribute.md>) | 标记 [SerializeReference] 字段在 Inspector 中使用子类下拉选择器。 点击下拉按钮弹出字段声明类型的全部可选子类（按命名空间分组）， 选择后自动创… |
| [`UICanvasConfigSO`](<Runestone/AesirModules/UICanvasConfigSO.md>) | UI Canvas 配置资产。创建路径：Assets → Create → Aesir Modules → UI → Default UICanvasConfig。 |
| [`UIModule`](<Runestone/AesirModules/UIModule.md>) | UI 管理器（MonoBehaviour 单例）。 负责面板生命周期管理，UI 根节点构建委托给 UIRoot。 |
| [`UIModuleConfigSO`](<Runestone/AesirModules/UIModuleConfigSO.md>) | UI 模块全局配置（单例资产）。承载无需预放置 [UIModule] 即可调整的模块级配置： 在 Project 窗口直接编辑本资产即可生效，不再要求预放置 UIModule … |
| [`UIRoot`](<Runestone/AesirModules/UIRoot.md>) | UI 根节点组件。 负责创建 UICamera、EventSystem、分层 Canvas 以及应用 Canvas 统一配置。 |
| [`UnityEventOnAesirEvent`](<Runestone/AesirModules/UnityEventOnAesirEvent.md>) | UnityEvent 桥接组件。监听指定类型的 AesirEventArgs， 事件触发时调用 Inspector 中配置的 UnityEvent 回调， 供非程序员在 Ins… |
| [`WithPriority`](<Runestone/AesirModules/WithPriority.md>) | 按优先级档位过滤。仅绑定在指定档位的订阅者收到事件， 用于"同一事件类型只通知某一档"的定向分发。 |
| [`WithTag`](<Runestone/AesirModules/WithTag.md>) | 按 Unity Tag 过滤订阅者。仅 Tag 匹配的订阅者收到事件。 用法：new OnExplosion().WithFilter(new WithTag("Enemy")… |
| [`WorldSpaceUICanvas`](<Runestone/AesirModules/WorldSpaceUICanvas.md>) | 世界空间 UI Canvas 预设组件。挂在场景物体（如角色头顶、交互点）上即可获得一个 WorldSpace 渲染根，UI 随该物体在世界空间中的位置移动； 也可在运行时由 … |

</div>

### 结构体

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirEventSubscriptionSnapshot`](<Runestone/AesirModules/AesirEventSubscriptionSnapshot.md>) | 单个事件类型的订阅汇总快照（供监视面板展示"某事件有多少订阅者"）。 |
| [`AesirEventTraceSnapshot`](<Runestone/AesirModules/AesirEventTraceSnapshot.md>) | 事件链路快照。诊断门面 AesirEventDiagnostics 推送与查询的最小不可变数据单元。 用 struct 存于固定容量环形缓冲，记录路径无托管分配；字段刻意保持精… |
| [`AesirSubscriberSnapshot`](<Runestone/AesirModules/AesirSubscriberSnapshot.md>) | 单条订阅绑定快照（供监视面板展开订阅者列表）。 |
| [`AudioChannel`](<Runestone/AesirModules/AudioChannel.md>) | 音频通道描述符（不可变值对象）。音量/静音的存取、PlayerPrefs 持久化与配置默认值载入 均以通道列表（Channels）为唯一数据源遍历。 |
| [`BinderCodeGenerator.BindUnit`](<Runestone/AesirModules/BinderCodeGenerator.BindUnit.md>) | 单个绑定单元在代码生成阶段的只读描述。 HierarchyPath 为空字符串表示绑定 BinderAssistant 所在物体自身， 生成代码将直接调用 GetCompone… |
| [`BinderCodeGenerator.CodeGenConfig`](<Runestone/AesirModules/BinderCodeGenerator.CodeGenConfig.md>) | 一次代码生成的完整配置。 |
| [`UIRoot.LayerCanvasEntry`](<Runestone/AesirModules/UIRoot.LayerCanvasEntry.md>) | 层级 Canvas 的序列化引用条目。Unity 无法序列化字典，改以列表存储（条目数恒等于层数，运行时线性查找即可）。 |

</div>

### 接口

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`IComponentBinder`](<Runestone/AesirModules/IComponentBinder.md>) | 绑定引用接口。由 BinderAssistant 生成的脚本实现，用于自动绑定场景中的组件引用。 生成脚本在「绑定字段（自动生成）」region 之后实现 BindCompon… |
| [`ISubscriberFilter`](<Runestone/AesirModules/ISubscriberFilter.md>) | 订阅者过滤器策略接口。发布者通过 WithFilter 声明过滤器， EventModule 分发时对每个订阅者逐个检查，全部过滤器通过才投递。 约定：过滤器无法解析对象（如发… |
| [`IUIAssetLoader`](<Runestone/AesirModules/IUIAssetLoader.md>) | 面板加载器契约。加载语义为同步：适用于 Resources、同步缓存等管线； Addressables 等异步管线需自行预加载后同步返回，无法在接口内表达等待。 |
| [`IUIPanel`](<Runestone/AesirModules/IUIPanel.md>) | UI 面板契约。生命周期：Initialize → Show → Hide → DestroyPanel。 |
| [`IUIWindow`](<Runestone/AesirModules/IUIWindow.md>) | Canvas 根 UI 窗口契约。生命周期：Initialize → Show → Hide → DestroyWindow，与 IUIPanel 平行。 窗口预制体根节点自带… |

</div>

### 枚举

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirEventTraceKind`](<Runestone/AesirModules/AesirEventTraceKind.md>) | 事件链路快照的种类。 |
| [`AesirModulesDebug.Tags`](<Runestone/AesirModules/AesirModulesDebug.Tags.md>) | — |
| [`BinderScriptMode`](<Runestone/AesirModules/BinderScriptMode.md>) | Binder 脚本生成模式。 |
| [`SceneAssetWrapperState`](<Runestone/AesirModules/SceneAssetWrapperState.md>) | SceneAssetWrapper 的可用状态。 Unsafe：引用不安全（空引用，或场景既不在 BuildSettings 也不可 Addressable） Regular：… |
| [`SceneAssetWrapperUnsafeReason`](<Runestone/AesirModules/SceneAssetWrapperUnsafeReason.md>) | 描述 SceneAssetWrapper 不安全的具体原因。 |
| [`SubscriberPriority`](<Runestone/AesirModules/SubscriberPriority.md>) | 事件订阅者优先级。4 档排序：First → High → Medium → Last。 Medium 为 Script 订阅（AddListener<T>）默认值；High … |
| [`UILayer`](<Runestone/AesirModules/UILayer.md>) | UI 层级。Background < Normal < Popup < Top。 |
| [`UIMaskMode`](<Runestone/AesirModules/UIMaskMode.md>) | 窗口蒙版调度模式，初始值配置于 UIModuleConfigSO（Resources 兜底资产， 可显式创建；缺资产时用内存默认配置），运行时可经 MaskMode 切换。 |
| [`WorldSpaceUICanvas.WorldSpaceBillboardMode`](<Runestone/AesirModules/WorldSpaceUICanvas.WorldSpaceBillboardMode.md>) | 世界空间 UI 的朝向模式。 |

</div>

## Runestone.AesirModules.Editor

命名空间 `Runestone.AesirModules.Editor` · 程序集 `Runestone.AesirModules.Editor`、`Runestone.AesirModules.Editor.OdinInspector` · 共 21 个类型。

### 类

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirEventArgsSOEditor`](<Runestone/AesirModules/Editor/AesirEventArgsSOEditor.md>) | AesirEventArgsSO 自定义 Inspector： 运行模式（Play Mode）限定的事件触发按钮 + 事件参数配置字段。 |
| [`AesirEventLogWindow`](<Runestone/AesirModules/Editor/AesirEventLogWindow.md>) | 事件日志窗口（UI Toolkit 版）：实时展示事件链路快照（触发 / 绑定 / 解绑）。 只消费公开门面 AesirEventDiagnostics 的环形缓冲与 Trac… |
| [`AesirEventLogWindowOdin`](<Runestone/AesirModules/Editor/AesirEventLogWindowOdin.md>) | 事件日志窗口（Odin 版）：用 Odin 表格展示公开门面 AesirEventDiagnostics 的链路快照。 经 OdinWindowOpener 静态委托路由打开；… |
| [`AesirEventMonitorWindow`](<Runestone/AesirModules/Editor/AesirEventMonitorWindow.md>) | 事件监视窗口（UI Toolkit 版）：以只读方式展示"哪些事件当前有哪些订阅者"。 数据全部来自公开门面 AesirEventDiagnostics，本窗口无任何内部注册表… |
| [`AesirEventMonitorWindowOdin`](<Runestone/AesirModules/Editor/AesirEventMonitorWindowOdin.md>) | 事件监视窗口（Odin 版）：用 Odin 表格展示公开 API AesirEventDiagnostics 提供的订阅快照。 经 OdinWindowOpener 静态委托由… |
| [`AesirEventTesterWindow`](<Runestone/AesirModules/Editor/AesirEventTesterWindow.md>) | 事件测试窗口（UI Toolkit 版）：在编辑器中选择事件类型、配置载荷并手动触发， 用于不写触发代码即可复现事件、验证订阅链路。 触发经由公开 API Raise，发布者为… |
| [`AesirEventTesterWindowOdin`](<Runestone/AesirModules/Editor/AesirEventTesterWindowOdin.md>) | 事件测试窗口（Odin 版）：以内联 Inspector 编辑临时事件资产并手动触发。 经 OdinWindowOpener 静态委托路由打开； 未安装 Odin 或关闭 Ae… |
| [`AesirSingletonAssetInitializer`](<Runestone/AesirModules/Editor/AesirSingletonAssetInitializer.md>) | 模块配置资产（单例 ScriptableObject）的显式创建共用实现。 |
| [`AesirUniTaskDefineKeeper`](<Runestone/AesirModules/Editor/AesirUniTaskDefineKeeper.md>) | 自动维护 AESIR_MODULES_UNITASK 脚本宏定义符号——SceneModule 的 UniTask 驱动分支与 UniTask 适配程序集（Runestone.… |
| [`AudioModuleMenuItems`](<Runestone/AesirModules/Editor/AudioModuleMenuItems.md>) | 音频模块编辑器菜单项。提供在场景中预放置 AudioModule 的快捷入口。 |
| [`BootstrapSceneHelper`](<Runestone/AesirModules/Editor/BootstrapSceneHelper.md>) | 启动场景帮助类，自动查找项目中的启动场景，并将其设置为第一个加载的场景 |
| [`EventDiagnosticsWindowHelper`](<Runestone/AesirModules/Editor/EventDiagnosticsWindowHelper.md>) | 事件诊断窗口的 UI 构件helper。集中标题、行文本、空态等零散样式， 供三个 UI Toolkit 窗口复用，避免各自复制样式代码。 |
| [`ModuleConfigAssetsMenuItem`](<Runestone/AesirModules/Editor/ModuleConfigAssetsMenuItem.md>) | 模块配置资产的一键初始化入口：显式在 Assets/Resources/ 下创建 UI / 场景两份配置 SO。 |
| [`SceneAssetWrapperDrawer`](<Runestone/AesirModules/Editor/SceneAssetWrapperDrawer.md>) | SceneAssetWrapper 的属性绘制器 —— UI Toolkit 优先，IMGUI 兜底，零 Odin 依赖。 |
| [`SceneEditorSettings`](<Runestone/AesirModules/Editor/SceneEditorSettings.md>) | Scene 模块编辑器设置（ScriptableSingleton）：Bootstrapper 场景搜集注册与启动流转的编辑器侧开关。 |
| [`SceneModuleConfigAssetInitializer`](<Runestone/AesirModules/Editor/SceneModuleConfigAssetInitializer.md>) | 场景模块配置资产的显式创建入口：在 Resources 兜底路径缺失且项目中无同类型资产时， 创建 SceneModuleConfigSO 至 Assets/Resources… |
| [`SceneModuleSettingsWindow`](<Runestone/AesirModules/Editor/SceneModuleSettingsWindow.md>) | Scene 模块设置窗口（UI Toolkit 版）：展示并编辑 SceneEditorSettings 单例。 |
| [`SceneModuleSettingsWindowOdin`](<Runestone/AesirModules/Editor/SceneModuleSettingsWindowOdin.md>) | Scene 模块设置窗口（Odin 版）：InlineEditor 展示 SceneEditorSettings 单例。 |
| [`SubclassSelectorDrawer`](<Runestone/AesirModules/Editor/SubclassSelectorDrawer.md>) | SubclassSelectorAttribute 的 UI Toolkit 属性绘制器。 为 [SerializeReference] 字段提供子类下拉选择： 点击按钮弹出字… |
| [`UIModuleConfigAssetInitializer`](<Runestone/AesirModules/Editor/UIModuleConfigAssetInitializer.md>) | UI 模块配置资产的显式创建入口：在 Resources 兜底路径缺失且项目中无同类型资产时， 创建 UIModuleConfigSO 至 Assets/Resources/U… |
| [`UIModuleMenuItems`](<Runestone/AesirModules/Editor/UIModuleMenuItems.md>) | UI 模块编辑器菜单项。提供快捷创建 UIRoot 和默认 Canvas 配置资产的入口。 |

</div>

## Runestone.AesirModules.Editor.Bootstrap

命名空间 `Runestone.AesirModules.Editor.Bootstrap` · 程序集 `Runestone.AesirModules.Editor.Bootstrap` · 共 2 个类型。

### 类

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirDependencyInstaller`](<Runestone/AesirModules/Editor/Bootstrap/AesirDependencyInstaller.md>) | Aesir Modules 依赖补全器 —— 检测 Aesir Architecture (RAA) 缺失并经 Git URL 引导安装。 |
| [`AesirDependencyInstaller.PackageManifest`](<Runestone/AesirModules/Editor/Bootstrap/AesirDependencyInstaller.PackageManifest.md>) | 包 package.json 的最小解析模型（JsonUtility 忽略未声明字段）。 |

</div>

## Runestone.AesirModules.Editor.OdinInspector

命名空间 `Runestone.AesirModules.Editor.OdinInspector` · 程序集 `Runestone.AesirModules.Editor.OdinInspector` · 共 8 个类型。

### 类

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AesirBasePanelAttributeProcessor`](<Runestone/AesirModules/Editor/OdinInspector/AesirBasePanelAttributeProcessor.md>) | 为 AesirBasePanel 提供 Odin Inspector 属性处理器， 动态注入 Inspector 显示特性，使 Runtime 程序集零 Odin 依赖。 |
| [`AesirModulesAttributeProcessor`](<Runestone/AesirModules/Editor/OdinInspector/AesirModulesAttributeProcessor.md>) | 为 AesirModules 提供 Odin Inspector 属性处理器， 动态注入 DDOL 开关的说明与警告信息框，使 Runtime 程序集零 Odin 依赖。 |
| [`AudioModuleAttributeProcessor`](<Runestone/AesirModules/Editor/OdinInspector/AudioModuleAttributeProcessor.md>) | 为 AudioModule 提供 Odin Inspector 属性处理器， 动态注入 DDOL 开关的说明信息框，使 Runtime 程序集零 Odin 依赖。 |
| [`SceneAssetWrapperAttributeProcessor`](<Runestone/AesirModules/Editor/OdinInspector/SceneAssetWrapperAttributeProcessor.md>) | 为 SceneAssetWrapper 提供 Odin Inspector 属性处理器， 动态注入 Inspector 显示特性，使 Runtime 程序集零 Odin 依赖。… |
| [`UICanvasConfigSOAttributeProcessor`](<Runestone/AesirModules/Editor/OdinInspector/UICanvasConfigSOAttributeProcessor.md>) | 为 UICanvasConfigSO 提供 Odin Inspector 属性处理器， 动态注入 Odin 专属显示特性（分组、条件显示等），使 Runtime 程序集零 Od… |
| [`UIModuleAttributeProcessor`](<Runestone/AesirModules/Editor/OdinInspector/UIModuleAttributeProcessor.md>) | 为 UIModule 提供 Odin Inspector 属性处理器， 动态注入 DDOL 开关的说明与警告信息框，使 Runtime 程序集零 Odin 依赖。 |
| [`UIRootAttributeProcessor`](<Runestone/AesirModules/Editor/OdinInspector/UIRootAttributeProcessor.md>) | 为 UIRoot 提供 Odin Inspector 属性处理器， 动态注入 Inspector 显示特性，使 Runtime 程序集零 Odin 依赖。 |
| [`WorldSpaceUICanvasAttributeProcessor`](<Runestone/AesirModules/Editor/OdinInspector/WorldSpaceUICanvasAttributeProcessor.md>) | 为 WorldSpaceUICanvas 提供 Odin Inspector 属性处理器： 动态注入分组与条件显示特性，使 Runtime 程序集零 Odin 依赖。 |

</div>

## Runestone.AesirModules.ScriptDocGenerator

命名空间 `Runestone.AesirModules.ScriptDocGenerator` · 程序集 `Runestone.AesirModules.OdinInspector` · 共 33 个类型。

### 类

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AccessModifierTypeExtensions`](<Runestone/AesirModules/ScriptDocGenerator/AccessModifierTypeExtensions.md>) | — |
| [`ConstructorData`](<Runestone/AesirModules/ScriptDocGenerator/ConstructorData.md>) | 构造方法解析数据 |
| [`DefaultAnalysisDataFactory`](<Runestone/AesirModules/ScriptDocGenerator/DefaultAnalysisDataFactory.md>) | Aesir Modules 默认提供的解析数据工厂实现类 |
| [`DefaultAttributeFilter`](<Runestone/AesirModules/ScriptDocGenerator/DefaultAttributeFilter.md>) | 默认特性过滤器，构造函数中传入需要排除的 Attribute 类型 |
| [`DerivedMemberDataComparer`](<Runestone/AesirModules/ScriptDocGenerator/DerivedMemberDataComparer.md>) | IDerivedMemberData 比较类 |
| [`EventData`](<Runestone/AesirModules/ScriptDocGenerator/EventData.md>) | 事件解析数据类，用于存储事件的解析数据 |
| [`FieldData`](<Runestone/AesirModules/ScriptDocGenerator/FieldData.md>) | 字段解析数据类，用于存储字段的解析数据 |
| [`MemberData`](<Runestone/AesirModules/ScriptDocGenerator/MemberData.md>) | 解析成员数据的基类 |
| [`MethodData`](<Runestone/AesirModules/ScriptDocGenerator/MethodData.md>) | 方法解析数据类，用于存储 MethodInfo 的解析结果 |
| [`NiceTypeName`](<Runestone/AesirModules/ScriptDocGenerator/NiceTypeName.md>) | 类型名称格式化工具（生成文档使用的"好看名"）。 |
| [`ParameterData`](<Runestone/AesirModules/ScriptDocGenerator/ParameterData.md>) | 参数信息解析数据 |
| [`PropertyData`](<Runestone/AesirModules/ScriptDocGenerator/PropertyData.md>) | 属性解析数据类，用于存储属性的解析数据 |
| [`ReferenceLinkURLAttribute`](<Runestone/AesirModules/ScriptDocGenerator/ReferenceLinkURLAttribute.md>) | — |
| [`ReflectionUtility`](<Runestone/AesirModules/ScriptDocGenerator/ReflectionUtility.md>) | 反射工具类，提供程序集、命名空间及成员的反射操作方法 |
| [`SourceFileEntry`](<Runestone/AesirModules/ScriptDocGenerator/SourceFileEntry.md>) | 源代码文件路径与内容的绑定容器。 |
| [`SummaryAttribute`](<Runestone/AesirModules/ScriptDocGenerator/SummaryAttribute.md>) | 提供类似于 XML 文档 summary 部分的描述性元数据。 |
| [`TypeAnalyzerStaticExtensions`](<Runestone/AesirModules/ScriptDocGenerator/TypeAnalyzerStaticExtensions.md>) | 类型分析器静态扩展类，统一管理类型分析器有关的静态扩展方法 |
| [`TypeAnalyzerUtility`](<Runestone/AesirModules/ScriptDocGenerator/TypeAnalyzerUtility.md>) | 类型分析器工具类 |
| [`TypeData`](<Runestone/AesirModules/ScriptDocGenerator/TypeData.md>) | 类型解析数据类，存储类型的各种成员的解析数据 |

</div>

### 接口

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`IAnalysisDataFactory`](<Runestone/AesirModules/ScriptDocGenerator/IAnalysisDataFactory.md>) | 解析数据工厂接口，自定义扩展解析数据工厂 |
| [`IAttributeFilter`](<Runestone/AesirModules/ScriptDocGenerator/IAttributeFilter.md>) | 特性过滤器接口，用于过滤掉不需要的特性 |
| [`IConstructorData`](<Runestone/AesirModules/ScriptDocGenerator/IConstructorData.md>) | 构造方法数据接口，继承自 IDerivedMemberData |
| [`IDerivedMemberData`](<Runestone/AesirModules/ScriptDocGenerator/IDerivedMemberData.md>) | 派生成员数据接口，不同的派生类有不同的表现形式，MemberData 无法直接准确获取的信息，需要派生类自己实现 |
| [`IEventData`](<Runestone/AesirModules/ScriptDocGenerator/IEventData.md>) | 事件数据接口，继承自 IDerivedMemberData |
| [`IFieldData`](<Runestone/AesirModules/ScriptDocGenerator/IFieldData.md>) | 字段数据接口，继承自 IDerivedMemberData |
| [`IMemberData`](<Runestone/AesirModules/ScriptDocGenerator/IMemberData.md>) | 成员数据接口 |
| [`IMethodData`](<Runestone/AesirModules/ScriptDocGenerator/IMethodData.md>) | 方法数据接口，继承自 IDerivedMemberData |
| [`IParameterData`](<Runestone/AesirModules/ScriptDocGenerator/IParameterData.md>) | 参数信息解析数据接口 |
| [`IPropertyData`](<Runestone/AesirModules/ScriptDocGenerator/IPropertyData.md>) | 属性数据接口，继承自 IDerivedMemberData，包含属性特有的数据信息和方法，派生类的通用数据信息和方法 |
| [`ITypeData`](<Runestone/AesirModules/ScriptDocGenerator/ITypeData.md>) | 类型解析数据接口，继承自 IDerivedMemberData 接口 |

</div>

### 枚举

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`AccessModifierType`](<Runestone/AesirModules/ScriptDocGenerator/AccessModifierType.md>) | — |
| [`ParameterDirection`](<Runestone/AesirModules/ScriptDocGenerator/ParameterDirection.md>) | 参数方向枚举 |
| [`TypeCategory`](<Runestone/AesirModules/ScriptDocGenerator/TypeCategory.md>) | 类型种类枚举 |

</div>

## Runestone.AesirModules.ScriptDocGenerator.Editor

命名空间 `Runestone.AesirModules.ScriptDocGenerator.Editor` · 程序集 `Runestone.AesirModules.Editor.OdinInspector` · 共 34 个类型。

### 类

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`DefaultScriptingAPISettingsSO`](<Runestone/AesirModules/ScriptDocGenerator/Editor/DefaultScriptingAPISettingsSO.md>) | 默认中文 API 文档生成设置 |
| [`DocGeneratorSettingsSO`](<Runestone/AesirModules/ScriptDocGenerator/Editor/DocGeneratorSettingsSO.md>) | 文档生成器设置抽象类 |
| [`MemberGrouper`](<Runestone/AesirModules/ScriptDocGenerator/Editor/MemberGrouper.md>) | 成员分组引擎：Default 与 Zensical 两生成器共享的分组核心—— API 成员过滤 → 按选择器分组 → 按固定顺序（常量 → 声明 → 继承 → 运算符）输出非… |
| [`ParsedSourceDoc`](<Runestone/AesirModules/ScriptDocGenerator/Editor/ParsedSourceDoc.md>) | 单个源文件（或合并后的类型级视图）的结构化 XML 文档注释解析结果。 键为全限定键（无程序集前缀，合并时按需添加）： 类型级 Namespace.TypeName；成员级 N… |
| [`ProjectScriptIndex`](<Runestone/AesirModules/ScriptDocGenerator/Editor/ProjectScriptIndex.md>) | 项目级类型声明索引：类型名 → 声明所在文件路径列表，附带 文件 → 命名空间集合。 惰性构建，每个域重载周期至多一次全项目扫描（历史实现按"每个找不到源文件的类型" 全项目扫… |
| [`ScriptAssemblyFilter`](<Runestone/AesirModules/ScriptDocGenerator/Editor/ScriptAssemblyFilter.md>) | 脚本程序集过滤器。通过 CompilationPipeline 缓存本项目的脚本程序集名集合， 用于在查找源文件前拦截引擎模块、预编译 DLL 等不可能存在项目源码的类型， 避… |
| [`ScriptDocGenerationResult`](<Runestone/AesirModules/ScriptDocGenerator/Editor/ScriptDocGenerationResult.md>) | 一次脚本文档生成调用的结果：写入的文档文件清单、使用的设置与输出根目录， 以及未能映射到编译产物的源码类型名（文件夹模式）。 |
| [`ScriptDocGeneratorAPI`](<Runestone/AesirModules/ScriptDocGenerator/Editor/ScriptDocGeneratorAPI.md>) | 脚本文档生成器静态 API —— 面板（Tools → Aesir → Modules → Script Doc Generator）的无 UI 等价入口， 面向自动化脚本与 … |
| [`ScriptDocGeneratorAssetMarkerSO`](<Runestone/AesirModules/ScriptDocGenerator/Editor/ScriptDocGeneratorAssetMarkerSO.md>) | Script Doc Generator 模块资产初始化完成标识。首次初始化后创建标识资产， 后续打开工具时通过检查标识是否存在来判断是否已完成初始化。 |
| [`ScriptDocGeneratorEditorUtility`](<Runestone/AesirModules/ScriptDocGenerator/Editor/ScriptDocGeneratorEditorUtility.md>) | Script Doc Generator 的编辑器工具方法，仅供编辑器程序集内部使用。 |
| [`ScriptDocGeneratorMenuItems`](<Runestone/AesirModules/ScriptDocGenerator/Editor/ScriptDocGeneratorMenuItems.md>) | — |
| [`ScriptDocGeneratorMenuPaths`](<Runestone/AesirModules/ScriptDocGenerator/Editor/ScriptDocGeneratorMenuPaths.md>) | Script Doc Generator 所有 MenuItem 菜单路径和优先级的统一管理。 Unity 中 MenuItem 的顺序由 priority 参数（一个整数）决… |
| [`ScriptDocGeneratorPanelSO`](<Runestone/AesirModules/ScriptDocGenerator/Editor/ScriptDocGeneratorPanelSO.md>) | ScriptDocGenerator 可视化操作面板类 |
| [`ScriptDocGeneratorPanelSO.ScriptDocGeneratorPanelAttributeProcessor`](<Runestone/AesirModules/ScriptDocGenerator/Editor/ScriptDocGeneratorPanelSO.ScriptDocGeneratorPanelAttributeProcessor.md>) | — |
| [`ScriptDocGeneratorPaths`](<Runestone/AesirModules/ScriptDocGenerator/Editor/ScriptDocGeneratorPaths.md>) | — |
| [`ScriptDocGeneratorUtility`](<Runestone/AesirModules/ScriptDocGenerator/Editor/ScriptDocGeneratorUtility.md>) | 脚本文档生成器逻辑控制类，负责处理文档生成的核心逻辑 |
| [`ScriptDocGeneratorWindow`](<Runestone/AesirModules/ScriptDocGenerator/Editor/ScriptDocGeneratorWindow.md>) | 脚本文档生成器窗口，直接展示 ScriptDocGeneratorSO 单面板。 |
| [`SourceFileAnalyzerUtility`](<Runestone/AesirModules/ScriptDocGenerator/Editor/SourceFileAnalyzerUtility.md>) | 源文件查找与成员名提取工具。 查找链路：ScriptAssemblyFilter 程序集过滤 → AssetDatabase 按名搜索 + GetClass() 验证（单遍，p… |
| [`SourceScanner`](<Runestone/AesirModules/ScriptDocGenerator/Editor/SourceScanner.md>) | 单遍字符级状态机源码扫描器： 第一遍净化——字符串（普通/逐字）、字符字面量、行注释、块注释内容置空，产出净化行， 使后续正则天然免疫字符串/注释里的假类型声明、假命名空间与假… |
| [`SourceScanner.Frame`](<Runestone/AesirModules/ScriptDocGenerator/Editor/SourceScanner.Frame.md>) | — |
| [`SourceScanner.SanitizerState`](<Runestone/AesirModules/ScriptDocGenerator/Editor/SourceScanner.SanitizerState.md>) | — |
| [`SourceScanner.SourceDocText`](<Runestone/AesirModules/ScriptDocGenerator/Editor/SourceScanner.SourceDocText.md>) | — |
| [`SourceSummaryInitializer`](<Runestone/AesirModules/ScriptDocGenerator/Editor/SourceSummaryInitializer.md>) | 在编辑器程序集（Runestone.AesirModules.ScriptDocGenerator.Editor）加载时注入 XML 文档注释解析器： summary / pa… |
| [`SourceSummaryParser`](<Runestone/AesirModules/ScriptDocGenerator/Editor/SourceSummaryParser.md>) | 源码 XML 文档注释解析门面。实际解析由 SourceScanner 单遍状态机完成： 字符串/逐字字符串/注释感知净化 + 命名空间栈 + 类型栈，支持全限定键（含嵌套类型… |
| [`SummaryToolMenuItems`](<Runestone/AesirModules/ScriptDocGenerator/Editor/SummaryToolMenuItems.md>) | 右键快捷处理 Summary 特性。批量处理多选脚本时仅触发一次 AssetDatabase.Refresh。 |
| [`TypeDataProcessor`](<Runestone/AesirModules/ScriptDocGenerator/Editor/TypeDataProcessor.md>) | — |
| [`TypesCacheSO`](<Runestone/AesirModules/ScriptDocGenerator/Editor/TypesCacheSO.md>) | 存储 Type 的资源文件，提供给脚本文档生成工具复用，用户无需每次重新选择 Type |
| [`XmlCodePart`](<Runestone/AesirModules/ScriptDocGenerator/Editor/XmlCodePart.md>) | XML 注释部分和代码块的组合。 |
| [`XmlSummaryTool`](<Runestone/AesirModules/ScriptDocGenerator/Editor/XmlSummaryTool.md>) | C# 脚本的 XML 中的 Summary 注释的处理器。 内容对齐方向（Sync/Replace）：[Summary] 特性优先——已有可解析特性时以特性文本为准（必要时回写… |
| [`ZensicalScriptingAPISettingsSO`](<Runestone/AesirModules/ScriptDocGenerator/Editor/ZensicalScriptingAPISettingsSO.md>) | Zensical 静态站点专用的 API 文档生成设置 |

</div>

### 枚举

<div class="api-summary-table" markdown="1">

| 名称 | 描述 |
| :--- | :--- |
| [`MemberGroup`](<Runestone/AesirModules/ScriptDocGenerator/Editor/MemberGroup.md>) | 成员分组（文档生成共享）：常量 → 声明 → 继承 → 运算符。 |
| [`ScriptDocGeneratorPanelSO.TypeSource`](<Runestone/AesirModules/ScriptDocGenerator/Editor/ScriptDocGeneratorPanelSO.TypeSource.md>) | — |
| [`XmlCodePart.SummaryAttribution`](<Runestone/AesirModules/ScriptDocGenerator/Editor/XmlCodePart.SummaryAttribution.md>) | [Summary] 特性的归属分析结果。 |
| [`XmlSummaryTool.ProcessMode`](<Runestone/AesirModules/ScriptDocGenerator/Editor/XmlSummaryTool.ProcessMode.md>) | — |

</div>
