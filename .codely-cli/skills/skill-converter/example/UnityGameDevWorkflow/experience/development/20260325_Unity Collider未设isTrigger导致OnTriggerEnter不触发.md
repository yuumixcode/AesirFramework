# 开发经验文档

## 编写原则
- **通用性**: 避免游戏特定术语，经验应适用于各种游戏开发
- **简洁性**: 描述简洁明了，避免冗余，代码示例只保留关键部分

## 基本信息
- **创建日期**: 2026-03-25
- **相关任务**: Unity 收集物/触发器碰撞检测
- **技术栈**: Unity/C#/Physics
- **来源Skill**: unity-collectible-system

## 问题描述

### 问题表现
玩家碰到收集物（金币、道具等）时 `OnTriggerEnter` 回调不触发，收集物无法被拾取，且无任何报错。

### 触发条件
- 收集物的 Collider 的 `isTrigger` 未勾选（默认为 false）
- 代码中使用 `OnTriggerEnter` 而非 `OnCollisionEnter`

## 解决方案

### 关键步骤
1. 收集物的 Collider 必须设置 `isTrigger = true`
2. 玩家必须有 CharacterController 或 Rigidbody + Collider
3. 使用 `OnTriggerEnter(Collider other)` 检测

### 关键代码/命令
```csharp
// 创建收集物时设置 isTrigger
var collider = obj.AddComponent<SphereCollider>();
collider.isTrigger = true;  // ← 关键！
collider.radius = 0.5f;

// 玩家脚本中检测
void OnTriggerEnter(Collider other)
{
    if (other.CompareTag("Coin"))
    {
        other.gameObject.SetActive(false);  // 用 SetActive 而非 Destroy（便于对象池复用）
    }
}
```

### 最终方案
收集物使用 Trigger 碰撞模式：Collider.isTrigger = true + OnTriggerEnter 检测。隐藏收集物时用 `SetActive(false)` 而非 `Destroy`，便于后续对象池优化。

---
**文档版本**: v1.0
**维护者**: Experience Manager
**最后更新**: 2026-03-25
