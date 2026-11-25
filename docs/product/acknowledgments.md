---
description: "AesirFramework 学习、参考与提取功能来源的开源项目与收费插件集中索引。"
---

# 推荐项目与参考致谢

AesirFramework 站在许多优秀项目的肩膀上。本页是**唯一的参考来源索引**：每个模块「参考了谁、原版是什么性质、Aesir 与原版是什么关系」都在这里集中说明，其他页面不再重复维护。

!!! important "Aesir 与原版的关系：提取与优化，不是复刻"

    本页列出的所有「原版 / 上游」项目，Aesir 均遵循同一条原则：**提取其极简核心功能，再按自己的语义做自定义优化——不是复刻，也不是移植代码**。因此 Aesir 的能力是收敛的：如果你需要更强的完整功能，**请直接使用原版**，多数场景两者还能在同一工程共存。表中标注了原版性质：开源（可自由使用与商用）或**收费插件**（需购买授权）。

## 项目一览 {#project-list}

| 项目 | 性质 | 官方链接 | 对应 Aesir 能力 | 与原版的关系 |
| --- | --- | --- | --- | --- |
| [QFramework](https://github.com/liangxiegame/QFramework) | 开源（MIT） | GitHub | Aesir Architecture（RAA）整体 | 最主要的设计参考；RAA 属 QFramework 宇宙中的同类架构 |
| [Cysharp/ObservableCollections](https://github.com/Cysharp/ObservableCollections) | 开源（MIT） | GitHub | 可观察值与集合（ObservableValue / List / Dictionary / HashSet） | 从原版提取极简集合形态并自定义通知语义 |
| [GameEventHub](https://assetstore.u3d.cn/packages/p/game-event-hub-303196) | **收费插件** | Unity Asset Store（中国区） | 事件模块（MiniEvent / EventModule） | 同类功能对照；Aesir 为自有极简实现 |
| [Easy Save 3](https://assetstore.unity.com/packages/tools/utilities/easy-save-the-complete-save-game-data-serializer-system-768) | **收费插件** | Unity Asset Store | 保存模块（SaveModule） | 从原版高频能力中提取事务写 / 批量 / 默认值 / 时间戳 |
| [Unity-Improved-Timers](https://github.com/adammyhre/Unity-Improved-Timers) | 开源（MIT） | GitHub | 计时器（AbstractTimer 及六种计时器） | 参考分层思路；结算引擎与状态语义为自有实现 |
| [Eflatun.SceneReference](https://github.com/starikcetin/Eflatun.SceneReference) | 开源（MIT） | GitHub | 场景模块（SceneAssetWrapper） | 功能设计参考；打包与重名处理为自有实现 |
| [Sirenix Odin Inspector](https://odininspector.com/) | **收费插件** | odininspector.com | Inspector 增强 / Binder / Script Doc Generator | 可选依赖，不随包分发 |
| [YokiFrame](https://github.com/HinataYoki/YokiFrame) | 开源（MIT） | GitHub | 编辑器窗口 UI Toolkit 样式 | 学习其样式组织方式（设计令牌 / BEM） |
| [StellarFramework](https://github.com/StarrDream/StellarFramework) | 开源（MIT） | GitHub | — | 独立开源项目，相互推荐 |

GameEventHub 亦可见其[官方网站](https://criminal-makers.com/)。

## 各模块的提取关系 {#extracted-from}

### Aesir Architecture × QFramework

**QFramework 是 RAA 最主要的设计参考**。Aesir Architecture 属于 [QFramework](https://github.com/liangxiegame/QFramework) 宇宙中同类型的架构——共享「分层 + 命令/查询 + 事件 + 工具模块」这一脉的设计语言，代码为 Aesir 自有实现。具体启发点：

- **架构分层**：Utility / Module 分层与包内工具的组织思路。
- **包内更新器**：「版本记录随包走」的策略参考 QFramework PackageKit，Aesir 在此基础上增加了**精确差集清理**（详见[包内更新器](../architecture/updater.md)）。
- 与 QFramework 的关系是**同类而非衍生**：RAA 不依赖 QFramework 的任何程序集，可独立安装。

### 可观察值与集合 × Cysharp/ObservableCollections

`ObservableValue<T>`、`ObservableList<T>`、`ObservableDictionary<TKey, TValue>`、`ObservableHashSet<T>` 是从 [Cysharp/ObservableCollections](https://github.com/Cysharp/ObservableCollections)（MIT）**提取的精简内容**：类型命名与集合形态参考原版，但通知语义是 Aesir 自有约定（单轨通知、无变更不通知、批量逐项、监听句柄可绑定 Unity 生命周期）。

需要队列、同步视图、R3 响应式、环形缓冲等**更强功能时请直接使用原版**——程序集、包名、命名空间三层完全隔离，可在同一项目共存，详见[可观察集合](../architecture/observable-collections.md#upstream)。

### 事件模块 × GameEventHub（收费）

MiniEvent / EventModule 是自有极简实现，只参考 [GameEventHub](https://assetstore.u3d.cn/packages/p/game-event-hub-303196)（**收费插件**）的功能设计作同类对照，不复制其类型名或代码结构。需要全局类型化事件总线、事件追踪面板等完整能力时，可购买使用原版。

### 保存模块 × Easy Save 3（收费）

Aesir 从 [Easy Save 3](https://assetstore.unity.com/packages/tools/utilities/easy-save-the-complete-save-game-data-serializer-system-768)（**收费插件**，设计思路参考其 `ES3File`）中提取了最高频的几项能力做极简实现：事务式原子写入 + 备份回退、`SaveBatch` / `LoadBatch` 批量会话、`LoadOrDefault`、`GetTimestamp`。需要多平台存储、可视化配置、加密生态等完整功能时，请购买使用原版。

### 计时器 × Unity-Improved-Timers

`AbstractTimer` 抽象基类 + 六种具体计时器的分层思路参考自 [Unity-Improved-Timers](https://github.com/adammyhre/Unity-Improved-Timers)（MIT）；结算引擎、状态语义与调试工具均为 Aesir 自有实现，并非移植或复刻。

### 编辑器 UI × YokiFrame

Aesir 编辑器窗口的 UI Toolkit 样式组织方式（设计令牌分层 / BEM 命名 / 微交互）借鉴自 [YokiFrame](https://github.com/HinataYoki/YokiFrame)（MIT），仅借鉴组织方式，配色保持 Aesir 自有风格。

## 参考依赖与素材出处 {#reference-deps}

以下为 Aesir 依赖或使用的工具上游与素材出处，与上表「同类项目 / 设计渊源」性质不同，单列一节：

| 项目 | 性质 | 与 Aesir 的关联 |
| --- | --- | --- |
| [Sirenix Odin Inspector](https://odininspector.com/) | **收费插件**（可选依赖，不随包分发） | Inspector 增强、Binder、脚本文档生成 |
| [yuumixcode/AesirInspector](https://github.com/yuumixcode/AesirInspector) | 独立公开仓库 | 面向 Odin Inspector 开发者的学习工具包 |
| [Vertical 2D Shooting BE4](https://www.goldmetal.co.kr) | Copyright ⓒ 2021 Goldmetal，标注出处即可使用（含商用） | 完整游戏案例 `Assets/PlaneWar` 的精灵素材（案例内自包含拷贝，不随包分发） |

完整授权说明见主仓 `Assets/Runestone/AesirModules/Third Party Notices.md` 与各包 `LICENSE.md`。
