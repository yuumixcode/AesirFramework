# 开发经验文档

## 编写原则
- **通用性**: 避免游戏特定术语，经验应适用于各种游戏开发
- **简洁性**: 描述简洁明了，避免冗余，代码示例只保留关键部分

## 基本信息
- **创建日期**: 2026-01-22
- **相关任务**: 修复Unity游戏首次按键无效问题
- **技术栈**: Unity, C#, Input System, GameManager, Game State

## 问题描述

### 问题表现
Unity游戏进入Play模式后，玩家按键盘（如空格键）无任何响应。PlayerController的输入回调方法（如OnJumpPerformed）被调用，但Jump()方法中的逻辑未执行，因为游戏状态不是Playing。控制台日志显示："Cannot jump - game is not active"。GameManager.Instance.IsGameActive返回false。

### 触发条件
- 游戏使用GameManager管理游戏状态
- PlayerController只在Playing状态下才允许执行动作（如Jump）
- 游戏默认状态是Waiting或类似非Playing状态
- 没有自动启动游戏的机制

## 解决方案

### 关键步骤
1. 检查PlayerController的Jump()方法
2. 在执行动作之前，检查游戏状态
3. 如果游戏未开始，自动调用GameManager.Instance.StartGame()
4. 编译并测试游戏

### 关键代码/命令
**方案1：在Jump()方法中自动启动游戏（推荐）**
```csharp
public class PlayerController : MonoBehaviour
{
    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        Jump();
    }

    public void Jump()
    {
        // 如果游戏未开始，自动启动游戏
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive)
        {
            GameManager.Instance.StartGame();
            Debug.Log("[PlayerController] Game started on first jump.");
        }

        // 只有在游戏中才能跳跃
        if (GameManager.Instance != null && !GameManager.Instance.IsGameActive)
        {
            Debug.Log("[PlayerController] Cannot jump - game is not active.");
            return;
        }

        // 执行跳跃逻辑
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
}
```

### 最终方案
在PlayerController的Jump()方法中，第一次按键时自动调用GameManager.Instance.StartGame()。符合玩家直觉，按空格键就能开始游戏。不需要额外的UI按钮或自动启动机制。简单直接，易于实现和理解。

---
**文档版本**: v4.0
**维护者**: Experience Manager
**最后更新**: 2026-01-22