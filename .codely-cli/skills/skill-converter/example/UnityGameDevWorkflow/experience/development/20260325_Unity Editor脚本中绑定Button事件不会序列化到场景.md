# 开发经验文档

## 编写原则
- **通用性**: 避免游戏特定术语，经验应适用于各种游戏开发
- **简洁性**: 描述简洁明了，避免冗余，代码示例只保留关键部分

## 基本信息
- **创建日期**: 2026-03-25
- **相关任务**: Unity UI 按钮事件绑定
- **技术栈**: Unity/C#/UGUI/Editor
- **来源Skill**: unity-game-ui-panels

## 问题描述

### 问题表现
在 Editor 脚本（如场景构建器）中通过 `button.onClick.AddListener()` 绑定的事件，Play 模式下按钮点击无响应。

### 触发条件
- 在 Editor 脚本（`Assets/Editor/` 下的脚本）中调用 `onClick.AddListener()`
- UnityEvent 的运行时监听器不会被序列化到场景中

## 解决方案

### 关键步骤
1. **不要在 Editor 脚本中绑定按钮事件**
2. 在运行时 MonoBehaviour 的 `Start()` 方法中绑定

### 关键代码/命令
```csharp
// ❌ 错误：在 Editor 脚本中绑定（不会序列化）
// GameSceneBuilder.cs (Editor 脚本)
restartBtn.onClick.AddListener(() => GameManager.Instance.RestartGame());

// ✅ 正确：在运行时脚本的 Start() 中绑定
// GameUI.cs (MonoBehaviour)
void Start()
{
    restartButton.onClick.AddListener(OnRestartClicked);
}

void OnRestartClicked()
{
    GameManager.Instance.RestartGame();
}
```

### 最终方案
按钮事件必须在运行时 MonoBehaviour 的 `Start()` 中通过 `onClick.AddListener()` 绑定。Editor 脚本只负责创建 UI 元素和连接引用（public 字段赋值），不负责绑定事件。

---
**文档版本**: v1.0
**维护者**: Experience Manager
**最后更新**: 2026-03-25
