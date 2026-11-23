---
description: "AesirFramework 两包共 11 个可导入示例:计数器六档对照、工具类用法与模块演示(另有仓库级完整游戏案例 PlaneWar,不随包分发)。"
---

# 示例总览

两个包合计 **16 个可导入示例**(RAA 11 个 + RAM 5 个),全部经 Package Manager → 对应包 → **Samples** 标签页按需导入。所有示例:

- 位于包内 `Samples/` 目录(仓库浏览时直接可见、场景可直接打开运行)
- 程序集为**运行时程序集 + 整文件 `#if UNITY_EDITOR`**:编辑器内可编译、可挂载、可 Play;玩家构建整体剔除,示例类型 0 入包
- 构建时示例场景会自动从本次构建选项中剔除,不需要手动处理 Build Settings

> 菜单 `Tools → Aesir → Getting Started` 提供一个示例导航窗口:集中浏览两包全部示例,点击卡片定位示例文件夹,带场景的示例经「打开场景」按钮直达。UPM / 嵌入式安装的未导入示例,卡片会引导你经「导入 Sample」按钮确认后导入到 `Assets/Samples/`。

## Aesir Architecture(11 个)

### 计数器六档(MVC / MVP 各三课)

同一个"计数器界面"用六种写法实现,逐课同构对照 —— 这是学习框架的主线教程。每档只有一个核心增量:

| 档位 | 示例 | Model 暴露面 | 读写路径 | View 边界 |
| --- | --- | --- | --- | --- |
| MVC 第一课 | `Counter-Mvc-Quick` | 具体类,可写 `ObservableValue` | View 兼 Controller 直写直读 | `MonoViewController<T>` 单类 |
| MVC 第二课 | `Counter-Mvc-Standard` | 具体类,只读暴露 + 写方法 | Controller 直调写方法 | View 与 Controller 分离,共享 Model |
| MVC 第三课 | `Counter-Mvc-Strict` | 接口注册,只读暴露 + 写方法 | Command 写 + Query 加工读 | View 按业务窄接口持有 Controller |
| MVP 第一课 | `Counter-Mvp-Quick` | 具体类,可写 `ObservableValue` | Presenter 直改值并推送 | 纯 MonoBehaviour,零接口抽象 |
| MVP 第二课 | `Counter-Mvp-Standard` | 具体类,只读暴露 + 写方法 | Presenter 直调写方法 | View 契约 `IXxxView` |
| MVP 第三课 | `Counter-Mvp-Strict` | 接口注册,只读暴露 + 写方法 | Command 写 + Query 读 | View 按窄接口持有 Presenter |

各档要点:

=== "快捷档(Quick)"

    最少概念跑通数据驱动 UI 闭环。MVC 用 `MonoViewController<T>` 一个类兼 View 与 Controller;MVP 零接口抽象,Presenter 直改可写 `ObservableValue` 并推送被动 View。适合原型与教学起步。

=== "标准档(Standard)"

    **推荐起步档位**。Model 只读暴露 + 写方法封装修改入口;MVC 中 View 与 Controller 分离共享 Model;MVP 中 View 实现 `IXxxView` 契约,Presenter 直调写方法 + 读 Model 推送。

=== "严格档(Strict)"

    读写全解耦:Model 接口注册 + Command 写 + Query 读;View 按**业务窄接口**持有 Controller / Presenter —— 类型层面拿不到 `ExecuteCommand` / `GetModel` 等框架能力,读写分离由类型系统闭环。MVC 与 MVP 严格档结构完全对称。

!!! tip "学习路径建议"

    按 Quick → Standard → Strict 顺序对照源码阅读(MVC 与 MVP 各一遍),重点看每档"多出来的那一个概念"。六份示例的场景、UI、表现完全一致,差异只在架构分层。

### 工具类示例

| 示例 | 内容 |
| --- | --- |
| **MiniEvent** | 无参 / 单参事件的订阅、触发与自动退订(多参数载荷建议用 struct 包裹成单参事件) |
| **ObservableValue**(需 Odin) | `ObservableValue<T>` 自定义 Drawer:int / float / string / bool / Vector2 基础类型与 struct / class 复合类型在 Inspector 中的编辑与通知 |
| **ObservableCollections** | `ObservableList<T>` / `ObservableDictionary<TKey, TValue>` / `ObservableHashSet<T>` 变更通知:单轨 `AddListener` 按 Action 分流 Add / Remove / Replace / Move / Reset,经 ContextMenu 触发增删改查 |
| **RuntimeInitializeLoadType** | 五个 `RuntimeInitializeLoadType` 初始化时机的执行顺序与静态重置实践;菜单 `Tools → Aesir → Architecture → Samples → RuntimeInitializeLoadType`。此为纯编辑器工具程序集(Editor-only + ODIN_INSPECTOR 约束) |
| **AesirTimer** | 计时器用法:`Countdown` 延时与冷却、`NextFrame`、`Every` 有限 / 无限重复、`Pause` / `Resume` / `Restart` / `Cancel`;面板只读 `Remaining` / `Progress` / `RemainingExecutions` 画进度条(无驱动代码),另含一个不继承 MonoBehaviour 的纯 C# 冷却模型;动作在自定义 Inspector 里点按钮触发,配 `Tools → Aesir → Architecture → Timer Debugger` 对照运行时快照 |

### 仓库级完整游戏案例(不随包分发)

`Assets/PlaneWar/` —— 纵版射击飞机大战完整小游戏,位于源码仓库(宿主工程)而非包内:不进 unitypackage、不进 Package Manager 的 Samples 列表、也不在 Getting Started 窗口出现,克隆仓库后打开 `Assets/PlaneWar/Scenes/PlaneWar.unity` 即可运行。

- 得分 HUD、三型敌机、子弹命中计分(A=10 / B=20 / C=30)、坠毁重开流程,原生 MonoBehaviour 实现(纯 Mono 版不使用框架类型,是"迁移前的基线")
- 按实际游戏项目分层:`Art/` / `Prefabs/` / `Scenes/` / `Scripts/` / `Materials/`,脚本程序集 `PlaneWar.Mono` 正常编译进玩家构建(不再经示例的 `#if UNITY_EDITOR` 包裹与场景构建剔除)
- RAA 版(`Scripts/Raa/`)规划中,用于对照"同一款游戏在架构版下的写法"
- 素材自包含:精灵来自 [Vertical 2D Shooting BE4](https://www.goldmetal.co.kr)(Copyright ⓒ 2021 Goldmetal;标注 Goldmetal 出处即可自由使用,含商用),案例内为自包含拷贝

## Aesir Modules(7 个)

| 示例 | 场景 | 演示内容 |
| --- | --- | --- |
| **Event · Key Press** | — | 事件模块最小闭环:按键发布事件 + Script 订阅、句柄退订 |
| **Event · Filters** | `SampleForEventFilters` | 订阅者过滤器对照:`WithTag("Player")` + `InsideCollider2D` 双重过滤,`OnlySelf` 家族命令;场景内置圈内 / 圈外 / 无标签多组对照组与 HUD 日志 |
| **Event · SO Asset** | `SampleForEventSOAsset` | SO 资产化:`ScoreEventAsset` 发布 + `UnityEventOnAesirEvent` 桥接组件零代码串联 UnityEvent 回调,同时演示代码订阅的双消费方式 |
| **Event · Cancellation** | `SampleForCancellation` | 取消传播:四档优先级订阅同一伤害事件,Space 观察执行链,Shift 对照不可取消事件 |
| **Event · Diagnostics** | `SampleForDiagnostics` | 诊断 API:订阅 `TraceProduced` 实时展示事件链路快照 |
| **Audio · Basic Usage** | `SampleForAudioBasic` | SFX 轮询播放、BGM 淡入淡出切换、三通道音量 / 静音持久化(OnGUI 面板驱动) |
| **UI · Basic Usage** | `SampleForUIBasic` | 面板与 Canvas 根窗口两种形态协作:Normal 层控制面板 + Top 层日志 HUD、设置 / 叠加窗口的蒙版单遮与叠遮对照、全屏加载窗口 |

## 继续阅读

- [Architecture 快速开始](../architecture/getting-started.md) —— 手写第一个 MVC 计数器
- [架构概念](../architecture/concepts.md) —— 示例背后的能力接口与 Context 机制
- [UI 框架](../modules/ui.md) · [事件模块](../modules/events.md) · [音频模块](../modules/audio.md) · [场景模块](../modules/scene.md)
