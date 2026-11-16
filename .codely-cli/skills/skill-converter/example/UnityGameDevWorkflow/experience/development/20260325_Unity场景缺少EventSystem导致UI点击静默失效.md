# 开发经验文档

## 编写原则
- **通用性**: 避免游戏特定术语，经验应适用于各种游戏开发
- **简洁性**: 描述简洁明了，避免冗余，代码示例只保留关键部分

## 基本信息
- **创建日期**: 2026-03-25
- **相关任务**: Unity UI 系统配置
- **技术栈**: Unity/C#/UGUI
- **来源Skill**: unity-game-ui-panels

## 问题描述

### 问题表现
Canvas 上的 Button、Toggle 等 UI 元素点击完全无响应，onClick 事件不触发，**且 Console 无任何报错**。

### 触发条件
- 通过代码创建 Canvas（而非 Unity 编辑器拖拽创建）
- 场景中没有 EventSystem 和 StandaloneInputModule
- Unity 编辑器手动创建 Canvas 时会自动添加 EventSystem，但代码创建不会

## 解决方案

### 关键步骤
1. 创建 Canvas 时**必须同时创建 EventSystem**
2. EventSystem 需要挂载 `StandaloneInputModule`（旧输入系统）或 `InputSystemUIInputModule`（新输入系统）

### 关键代码/命令
```csharp
// 创建 Canvas 后，必须创建 EventSystem
var es = new GameObject("EventSystem");
es.AddComponent<UnityEngine.EventSystems.EventSystem>();
es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
```

### 最终方案
通过代码创建 Canvas 时，必须手动创建 EventSystem + StandaloneInputModule。这是最隐蔽的 UI 陷阱之一，因为完全没有报错提示。如果使用新输入系统（Input System Package），需要改用 `InputSystemUIInputModule`。

---
**文档版本**: v1.0
**维护者**: Experience Manager
**最后更新**: 2026-03-25
