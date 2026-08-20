# Context Debugger 三版对比（T5.4）

> 三版试做全部完成并可编译打开。功能目标一致：观察 Context/Model/Service + 编辑 ObservableValue。
> 本文档记录实测数据，供用户三选一。

## 代码量与依赖

| 版本 | 窗口代码行数 | 共享数据层 | 外部依赖 | 无 Odin 可编译 |
|------|------------|-----------|---------|--------------|
| V1 · IMGUI | 234 | 复用 | 无 | ✅ |
| V2 · Odin | 118 | 复用 | Odin Inspector | ❌（`#if ODIN_INSPECTOR`） |
| V3 · UI Toolkit | 347 | 复用 | 无 | ✅ |
| 共享层（Scanner + Reader） | 117 + 172 | — | 无 | ✅ |

**关键观察**：V2 Odin 代码量最少（Odin 序列化协议自动处理 ObservableValue/非公有字段/多态），V3 UI Toolkit 最多（样式手写 USS）。

## 实测对比

| 维度 | V1 · IMGUI | V2 · Odin | V3 · UI Toolkit |
|------|-----------|-----------|-----------------|
| **样式** | 朴素（EditorStyles + Box 分组 + 状态徽标）| 最丰富（Inspector 富样式开箱即用）| 现代（卡片式 + 徽标配色 + 圆角，手写 USS）|
| **性能** | 即时模式，手动刷新控制开销 | 同 IMGUI | retained mode 最优（仅变更重绘） |
| **ObservableValue 编辑** | 反射读 value + Value setter 写回 | Odin 协议自动（OnValueChanged → InvokeEvent）| 反射读 value + 类型化字段控件绑定 |
| **维护感** | 直白但啰嗦 | 声明式最省心 | 纯代码可 diff/AI 可维护（G10 最契合） |
| **教学价值** | 反射编辑的直白示范 | Odin 工作流示范 | 现代 Unity 编辑器开发示范（官方推荐方向） |
| **类型覆盖** | int/float/string/bool/Vector2/3/枚举 | Odin 协议全覆盖（含多态/集合）| int/float/string/bool/Vector2/3/枚举 |

## 验收记录

- **编译**：V1/V3 无 Odin 可编译通过；V2 有 Odin 可编译通过（`#if ODIN_INSPECTOR` 保护完整）
- **编辑通知链**：Play 模式中修改计数器 Model 的 Count（0 → 42），View 文本实时刷新（T5.3 验收通过）
- **三版窗口均可打开**：菜单路径分别为 `Debugger (IMGUI)` / `Debugger (Odin)` / `Debugger (UI Toolkit)`

## 推荐意见（供用户参考）

| 若你的优先级是 | 推荐 |
|--------------|------|
| 样式开箱即用、维护最省心，且团队已装 Odin | **V2 Odin** |
| 性能最优、现代感最强、AI 可维护性最高（G10 最契合）、不依赖 Odin | **V3 UI Toolkit** |
| 朴素可靠、教学直白、零依赖基线 | V1 IMGUI |

**综合建议**：考虑到课程定位（学生大量不装 Odin）、G10 AI 优先理念、以及 Unity 官方编辑器开发的推荐方向，**V3 UI Toolkit** 是长期最稳的选择；若短期样式丰富度优先且接受 Odin 依赖，**V2 Odin** 见效最快。

## 待用户拍板

- [ ] 用户三选一：V1 IMGUI / V2 Odin / V3 UI Toolkit
- [ ] 选定后：菜单归一为 `Tools → Aesir → Architecture → Context Debugger`（去技术后缀）
- [ ] 落选两版删除（不留死代码）
