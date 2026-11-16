# 开发经验文档

## 编写原则
- **通用性**: 避免游戏特定术语，经验应适用于各种游戏开发
- **简洁性**: 描述简洁明了，避免冗余，代码示例只保留关键部分

## 基本信息
- **创建日期**: 2026-03-25
- **相关任务**: Unity C# 事件系统
- **技术栈**: Unity/C#
- **来源Skill**: unity-game-ui-panels

## 问题描述

### 问题表现
场景重载（如 `SceneManager.LoadScene`）后出现 `NullReferenceException` 或 `MissingReferenceException`，指向已销毁对象的事件回调。

### 触发条件
- MonoBehaviour 在 Start 中订阅了单例/静态对象的事件（如 `GameManager.Instance.OnGameOver += ...`）
- 场景重载时 MonoBehaviour 被销毁，但事件订阅未取消
- 单例对象触发事件时尝试调用已销毁对象的方法

## 解决方案

### 关键步骤
1. 在 `Start()` 中订阅事件
2. **必须在 `OnDestroy()` 中取消订阅**

### 关键代码/命令
```csharp
void Start()
{
    GameManager.Instance.OnGameOver += OnGameOver;
    GameManager.Instance.OnCoinCollected += OnCoinCollected;
}

// 【关键】必须取消订阅
void OnDestroy()
{
    if (GameManager.Instance != null)
    {
        GameManager.Instance.OnGameOver -= OnGameOver;
        GameManager.Instance.OnCoinCollected -= OnCoinCollected;
    }
}
```

### 最终方案
所有在 Start/Awake/OnEnable 中订阅的事件，必须在 OnDestroy/OnDisable 中对称取消订阅。取消前检查事件源是否为 null（单例可能已被销毁）。

---
**文档版本**: v1.0
**维护者**: Experience Manager
**最后更新**: 2026-03-25
