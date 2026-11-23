---
description: "AesirFramework 的已知缺口、暂缓决策与后续计划方向。"
---

# 路线图

这里记录**已知缺口**与**暂缓决策**,不承诺时间表。已完成的变更见[更新日志](../changelog.md)。

## 已登记缺口

| 方向 | 缺口 | 现状 |
| --- | --- | --- |
| 示例 | PlaneWar 的 RAA 版 | 案例已迁到仓库根 `Assets/PlaneWar/`(不随包分发),当前只有 Mono 版;RAA 版(`Scripts/Raa/`)用于对照"同一款游戏在架构版下的写法" |
| UI | Addressables 资源加载 | `IUIAssetLoader` 契约已预留,只提供 `ResourcesUILoader`。需要时自行实现(契约是同步语义,Addressables 需预加载后同步返回) |
| 事件 | 系统事件(元事件)与 DefaultChannel | 暂缓:无频道分发机制时频道标签是死元数据(类型键已是完整寻址轴);等频道机制真正立项再一并设计 |
| 事件 | 单实例合并 | 不实施:`Awake` 已销毁重复实例,订阅统一走 `Instance` 单一通道 |
| 文档 | Scripting API 页 | 由包内 Script Doc Generator 反射生成,目前是发版后手动重生成流程,未接入 CI |

## 明确不做

极简优先,以下方向已判定为"不做",不要在后续版本里期待:

- 事件总线 / EventChannel、Context 多实例、Command/Query 池化 / async / Undo
- 框架级时间调度原语(需要延时用 `MonoLifecycleProxy` + 计时字段,或 GameObject 协程)
- 线程安全(仅主线程约定)
- 框架级对象池、DI 容器
- 3D 音频 / AudioMixer / 每音效 Stop / 播完回调
- 窗口导航栈、per-panel Canvas

RAM 的边界比 RAA 宽(功能模块的面是价值),但同样只收高频能力;低频能力请直接用上游库或引擎原生 API。

## 演进原则

任何新增能力都要先过这三问:

1. **是否为高频需求?** 低频项写进设计边界,引导上游方案,而不是塞进框架
2. **是否能用文档约定代替运行时代码?** 低概率问题、误用问题优先用文档 + 编辑期提示解决,不加防御性兜底
3. **RAA 是否还能保持极简?** RAA 以极简为第一排序标准;RAM 更看重功能性,但同样不做重复造轮子

## 相关

- [特性总览](features.md) —— 现有能力全景与设计边界
- [更新日志](../changelog.md) —— 已经发生了什么
- [支持](../support.md) —— 提需求与反馈的渠道
