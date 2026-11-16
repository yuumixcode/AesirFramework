# 开发经验文档

## 编写原则
- **通用性**: 避免游戏特定术语，经验应适用于各种游戏开发
- **简洁性**: 描述简洁明了，避免冗余，代码示例只保留关键部分

## 基本信息
- **创建日期**: 2026-03-25
- **相关任务**: Unity 3D相机跟随系统
- **技术栈**: Unity/C#
- **来源Skill**: unity-camera-follow

## 问题描述

### 问题表现
相机跟随目标时出现明显抖动/震颤，尤其在目标快速移动时更明显。或者相机跟随速度在不同帧率下表现不一致。

### 触发条件
- 相机跟随逻辑写在 `Update()` 而非 `LateUpdate()` 中
- `Vector3.Lerp` 的 t 参数直接使用 smoothSpeed 而未乘以 `Time.deltaTime`
- 跟随目标为 null 时未做检查

## 解决方案

### 关键步骤
1. 相机跟随逻辑**必须放在 `LateUpdate()`** 中（在所有 Update 之后执行，确保目标位置已更新）
2. Lerp 的 t 参数乘以 `Time.deltaTime` 消除帧率依赖
3. 开头检查 target 是否为 null

### 关键代码/命令
```csharp
// ❌ 错误：在 Update 中跟随，且 t 参数未乘 deltaTime
void Update()
{
    transform.position = Vector3.Lerp(transform.position, target.position + offset, smoothSpeed);
}

// ✅ 正确：LateUpdate + deltaTime + null 检查
void LateUpdate()
{
    if (target == null) return;
    Vector3 desired = target.position + offset;
    transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
    transform.LookAt(target.position + Vector3.forward * lookAheadDistance);
}
```

### 最终方案
相机跟随必须在 `LateUpdate()` 中执行，Lerp 插值参数乘以 `Time.deltaTime`，并在开头做 null 检查。这三点缺一不可。

---
**文档版本**: v1.0
**维护者**: Experience Manager
**最后更新**: 2026-03-25
