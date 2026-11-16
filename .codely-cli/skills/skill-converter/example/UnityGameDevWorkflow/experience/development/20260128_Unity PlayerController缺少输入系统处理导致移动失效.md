# 开发经验文档

## 编写原则
- **通用性**: 避免游戏特定术语，经验应适用于各种游戏开发
- **简洁性**: 描述简洁明了，避免冗余，代码示例只保留关键部分

## 基本信息
- **创建日期**: 2026-01-28
- **相关任务**: SpaceShooter游戏开发 - PlayerController WASD移动失效
- **技术栈**: Unity 2022.3, C#, Input System

## 问题描述

### 问题表现
游戏运行时，玩家按键盘（WASD/方向键）无法移动：
- 玩家对象位置没有变化
- 输入回调方法（如OnMove()）未被调用
- 控制台没有输入相关的日志输出
- PlayerController代码存在但Move()方法从未执行

### 触发条件
- PlayerController类定义了Move()方法但没有在Update()中调用
- 缺少InputActionAsset引用和输入回调注册
- 缺少IGameplayActions接口实现
- 输入系统配置正确，但未在代码中处理输入

## 解决方案

### 关键步骤
1. 在PlayerController类中添加SpaceShooterInputActions引用和IGameplayActions接口
2. 实现OnMove()回调处理WASD输入
3. 在OnEnable()/OnDisable()中注册/注销输入回调
4. 在Update()中添加HandleMovement()方法调用Move()
5. 在Inspector中配置InputActionAsset引用

### 关键代码/命令
```csharp
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, IGameplayActions
{
    private SpaceShooterInputActions _inputActions;
    private Vector2 _moveInput;
    [SerializeField] private float _moveSpeed = 8f;

    private void Awake()
    {
        _inputActions = new SpaceShooterInputActions();
    }

    private void OnEnable()
    {
        _inputActions.Gameplay.SetCallbacks(this);
        _inputActions.Gameplay.Enable();
    }

    private void OnDisable()
    {
        _inputActions.Gameplay.Disable();
        _inputActions.Gameplay.SetCallbacks(null);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (_moveInput != Vector2.zero)
        {
            Vector3 movement = new Vector3(_moveInput.x, 0, 0);
            transform.position += movement * _moveSpeed * Time.deltaTime;
        }
    }
}
```

### 最终方案
PlayerController必须正确实现Input System的完整流程：
1. **添加InputActionAsset引用**：在类中定义`SpaceShooterInputActions`实例
2. **实现回调接口**：实现`IGameplayActions`接口的`OnMove()`方法
3. **注册生命周期回调**：在`OnEnable()`/`OnDisable()`中注册/注销输入回调
4. **在Update()中处理移动**：将输入值转换为移动向量并应用到transform

**最佳实践**：
- 使用`[SerializeField]`标记可配置的移动速度
- 在Move()方法中添加边界限制，防止玩家移出屏幕
- 使用`Time.deltaTime`确保移动速度与帧率无关
- 在Inspector中验证InputActionAsset引用是否正确

**常见错误**：
- 只定义了Move()方法但没有在Update()中调用
- 缺少OnEnable()/OnDisable()中的输入回调注册
- InputActionAsset未在Inspector中赋值导致_inputActions为null
- 忘记在OnDisable()中注销回调导致内存泄漏

---
**文档版本**: v1.0
**维护者**: Experience Manager
**最后更新**: 2026-01-28