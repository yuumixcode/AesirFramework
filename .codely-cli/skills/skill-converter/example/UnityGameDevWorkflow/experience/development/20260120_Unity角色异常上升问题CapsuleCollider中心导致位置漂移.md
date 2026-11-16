# Unity角色异常上升问题 - CapsuleCollider中心导致位置漂移

## 编写原则
- **通用性**: 避免游戏特定术语，经验应适用于各种游戏开发
- **简洁性**: 描述简洁明了，避免冗余，代码示例只保留关键部分

## 基本信息
- **创建日期**: 2026-01-20
- **相关任务**: Unity角色物理控制优化
- **技术栈**: Unity 2022.3.x, C#, Rigidbody, CapsuleCollider

## 问题描述

### 问题表现
- 角色位置持续缓慢上升（y从1.00→1.91）
- Rigidbody.velocity = (0, 0, 0)，速度为0
- useGravity=True，重力已启用
- 角色没有任何跳跃或上升力作用

### 触发条件
- GameObject包含Rigidbody和CapsuleCollider组件
- CapsuleCollider的center属性动态修改（例如在蹲伏动画中）
- Rigidbody有物理约束（constraints）设置

## 解决方案

### 关键步骤
1. 将CapsuleCollider.center固定为`Vector3.zero`
2. 移除动态修改center的代码
3. 仅修改CapsuleCollider.height属性
4. 配套修复groundCheckDistance和groundLayer

### 关键代码/命令

**修复前（错误）**：
```csharp
private void HandleCrouchAnimation()
{
    if (capsuleCollider.height != targetHeight)
    {
        float newHeight = Mathf.Lerp(capsuleCollider.height, targetHeight, Time.deltaTime * crouchSpeed);
        capsuleCollider.height = newHeight;

        Vector3 newCenter = originalCenter;
        newCenter.y = (newHeight / 2f) - 0.5f;  // ❌ 导致位置漂移
        capsuleCollider.center = newCenter;
    }
}
```

**修复后（正确）**：
```csharp
private void HandleCrouchAnimation()
{
    if (capsuleCollider.height != targetHeight)
    {
        float newHeight = Mathf.Lerp(capsuleCollider.height, targetHeight, Time.deltaTime * crouchSpeed);
        capsuleCollider.height = newHeight;

        // ✅ 保持中心为零，避免物理引擎调整transform位置
        Vector3 newCenter = Vector3.zero;
        capsuleCollider.center = newCenter;
    }
}
```

**groundCheckDistance修复**：
- 从0.1f增加到2.0f
- 确保从角色顶部能检测到地面

**groundLayer修复**：
```csharp
// Auto-fix: If groundLayer is 0, set it to Layer 10 (Ground layer)
if (groundLayer.value == 0)
{
    groundLayer = 1 << 10; // Layer 10
}
```

### 最终方案

**核心修复**：
- 保持CapsuleCollider.center为Vector3.zero
- 避免动态修改collider中心
- 通过调整GameObject的transform.position来控制位置

**原因**：
- Unity物理引擎在计算碰撞时，会考虑collider的相对位置
- 当center变化时，物理引擎会调整transform.position以保持碰撞一致性
- 对于Rigidbody组件，这种调整表现为位置漂移

**配套修复**：
- groundCheckDistance：设置为角色高度或更大（如2.0f）
- groundLayer：使用明确的LayerMask值（如1 << 10）
- 在Awake中添加自动修复逻辑

---
**文档版本**: v2.0
**维护者**: Experience Manager
**最后更新**: 2026-01-20