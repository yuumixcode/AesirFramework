# 开发经验文档

## 编写原则
- **通用性**: 避免游戏特定术语，经验应适用于各种游戏开发
- **简洁性**: 描述简洁明了，避免冗余，代码示例只保留关键部分

## 基本信息
- **创建日期**: 2026-01-17
- **相关任务**: 修复Unity游戏未自动开始问题
- **技术栈**: Unity, C#, GameManager, Coroutine

## 问题描述

### 问题表现
Unity游戏进入Play模式后，游戏没有自动开始。游戏状态停留在初始状态（如Waiting），游戏进程未启动。障碍物/敌人生成器等依赖游戏状态的功能无法工作。控制台没有游戏开始的日志输出。根本原因：GameManager的Start()方法没有调用StartGame()方法，或直接调用时其他组件未初始化完成。

### 触发条件
- 游戏使用GameManager管理游戏生命周期
- 游戏需要自动开始（无需UI按钮触发）
- GameManager有StartGame()方法，但Start()中未调用或调用时机不当

## 解决方案

### 关键步骤
1. 检查GameManager.cs的Start()方法
2. 确认是否需要自动开始功能
3. 添加自动开始游戏的协程（推荐）
4. 在协程中延迟调用StartGame()方法
5. 编译并测试游戏

### 关键代码/命令
```csharp
// 在GameManager.cs中添加using语句
using System.Collections;

// 修改Start()方法
private void Start()
{
    // 初始化游戏状态
    SetGameState(GameState.Waiting);
    
    // 自动开始游戏（用于测试，无需UI）
    StartCoroutine(AutoStartGameCoroutine());
}

// 添加自动开始协程
private IEnumerator AutoStartGameCoroutine()
{
    yield return new WaitForSeconds(0.5f);
    StartGame();
}
```

### 最终方案
使用协程（Coroutine）延迟0.5秒后自动调用StartGame()，确保所有组件初始化完成后再开始游戏。Unity的初始化顺序无法保证，某些组件可能在GameManager.Start()之后才初始化。直接调用可能导致依赖组件（如Spawner、UI等）还未初始化完成。协程延迟确保所有组件的Awake()和Start()都已执行完毕。

---
**文档版本**: v4.0
**维护者**: Experience Manager
**最后更新**: 2026-01-20