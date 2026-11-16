# 开发经验文档

## 编写原则
- **通用性**: 避免游戏特定术语，经验应适用于各种游戏开发
- **简洁性**: 描述简洁明了，避免冗余，代码示例只保留关键部分

## 基本信息
- **创建日期**: 2026-03-25
- **相关任务**: Unity 物体浮动/悬浮动画
- **技术栈**: Unity/C#
- **来源Skill**: unity-collectible-system

## 问题描述

### 问题表现
使用 Sin 波实现物体上下浮动动画时，物体逐渐偏离初始位置，越飘越远。

### 触发条件
- 在 Update 中直接对 `transform.position.y` 做 Sin 偏移累加
- 未记录初始位置作为基准

## 解决方案

### 关键步骤
1. 在 Start 中记录初始位置 `startPos`
2. 每帧基于 `startPos` 计算偏移，而非在当前 position 上累加

### 关键代码/命令
```csharp
// ❌ 错误：在当前位置上累加偏移
void Update()
{
    transform.position += Vector3.up * Mathf.Sin(Time.time * speed) * height * Time.deltaTime;
}

// ✅ 正确：基于初始位置偏移
Vector3 startPos;
void Start() { startPos = transform.position; }
void Update()
{
    Vector3 pos = startPos;
    pos.y += Mathf.Sin(Time.time * bobSpeed) * bobHeight;
    transform.position = pos;
}
```

### 最终方案
浮动动画必须在 Start 中记录 `startPos`，Update 中基于 `startPos` 加 Sin 偏移赋值，不能在当前 position 上累加。

---
**文档版本**: v1.0
**维护者**: Experience Manager
**最后更新**: 2026-03-25
