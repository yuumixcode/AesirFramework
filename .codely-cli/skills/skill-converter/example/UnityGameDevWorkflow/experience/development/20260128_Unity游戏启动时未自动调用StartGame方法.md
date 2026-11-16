# 开发经验文档

## 编写原则
- **通用性**: 避免游戏特定术语，经验应适用于各种游戏开发
- **简洁性**: 描述简洁明了，避免冗余，代码示例只保留关键部分

## 基本信息
- **创建日期**: 2026-01-28
- **相关任务**: Snake3D游戏开发 - 点play没有自动移动
- **技术栈**: Unity 2022.3, C#, Game Lifecycle

## 问题描述

### 问题表现
游戏进入Play Mode后，玩家无法开始游戏：
- 蛇不移动
- 按键盘无响应
- 游戏处于等待状态，需要手动触发才能开始
- `GameManager.StartGame()`方法未被调用

### 触发条件
- GameManager使用Singleton模式
- `Start()`方法中只初始化基本信息，未调用`StartGame()`
- 游戏逻辑依赖显式调用`StartGame()`才能开始
- 没有UI按钮或按键触发游戏开始

## 解决方案

### 关键步骤
1. 在GameManager的`Start()`方法中添加`StartGame()`调用
2. 确保所有依赖组件在`Start()`之前已初始化（在`Awake()`中）
3. 验证游戏启动流程：Awake → Start → StartGame
4. 添加调试日志确认启动流程正确执行

### 关键代码/命令

```csharp
public class GameManager : MonoBehaviour
{
    private void Awake()
    {
        // 初始化Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // 初始化游戏状态
        InitializeGameState();
    }

    private void Start()
    {
        // 自动启动游戏
        StartGame();
    }

    public void StartGame()
    {
        // 重置游戏状态
        ResetGame();

        // 通知游戏开始
        OnGameStart?.Invoke();

        Debug.Log("Game Started");
    }
}
```

### 最终方案
Unity的生命周期执行顺序为：`Awake()` → `Start()` → `Update()`。

**游戏自动启动的最佳实践**：
1. **在`Start()`中调用`StartGame()`**：确保所有`Awake()`方法都执行完毕
2. **使用`Awake()`初始化依赖**：确保单例、引用等在`Start()`之前就绪
3. **添加调试日志**：验证启动流程按预期执行
4. **考虑游戏暂停功能**：如果需要暂停，可以在`StartGame()`中设置初始状态

**生命周期注意事项**：
- `Awake()`：在脚本实例被加载时调用，适合初始化单例、依赖引用
- `Start()`：在第一帧Update之前调用，适合需要其他组件就绪后的初始化
- `OnEnable()`：在对象启用时调用，可能多次调用（禁用再启用）
- 不要在`Awake()`中调用依赖其他组件`Awake()`的方法
- `Start()`可以在`Awake()`之后安全地访问其他组件

**常见错误**：
- 在`Awake()`中调用需要其他组件就绪的方法
- 忘记调用`StartGame()`导致游戏处于等待状态
- 在`Start()`中尝试访问未初始化的Singleton

---
**文档版本**: v1.0
**维护者**: Experience Manager
**最后更新**: 2026-01-28