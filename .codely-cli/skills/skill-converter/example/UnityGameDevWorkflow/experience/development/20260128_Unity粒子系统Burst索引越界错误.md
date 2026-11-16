# 开发经验文档

## 编写原则
- **通用性**: 避免游戏特定术语，经验应适用于各种游戏开发
- **简洁性**: 描述简洁明了，避免冗余，代码示例只保留关键部分

## 基本信息
- **创建日期**: 2026-01-28
- **相关任务**: Unity粒子系统配置
- **技术栈**: Unity 2022.3.x, C#, ParticleSystem

## 问题描述

### 问题表现
Unity控制台显示粒子系统错误：
- `Burst index passed to SetBurst is too high (0, max 0)`
- 粒子系统发射的Burst设置被忽略

### 触发条件
- 使用`ParticleSystem.EmissionModule.SetBurst()`方法时传入的索引超出范围
- 粒子系统未正确设置burstCount
- 代码中直接使用索引0而没有先添加Burst

## 解决方案

### 关键步骤
1. 使用`ParticleSystem.Burst`数组初始化emission.bursts
2. 或者先设置`emission.burstCount`再使用`SetBurst()`
3. 确保索引值小于burstCount

### 关键代码/命令
**错误代码示例**：
```csharp
var emission = ps.emission;
emission.rateOverTime = 0;
// 错误：直接使用SetBurst(0, ...)，但burstCount为0
emission.SetBurst(0, new ParticleSystem.Burst(0, 50));
```

**正确代码示例1（推荐）**：
```csharp
var emission = ps.emission;
emission.rateOverTime = 0;
// 直接设置bursts数组
emission.bursts = new ParticleSystem.Burst[]
{
    new ParticleSystem.Burst(0.0f, 50)
};
```

**正确代码示例2**：
```csharp
var emission = ps.emission;
emission.rateOverTime = 0;
// 先设置burstCount
emission.burstCount = 1;
// 再设置Burst
emission.SetBurst(0, new ParticleSystem.Burst(0.0f, 50));
```

### 最终方案
配置粒子系统Burst时，避免直接使用`SetBurst(index, burst)`方法，因为需要先确保`burstCount`大于索引值。推荐直接使用`emission.bursts`数组属性进行初始化，这样更简洁且不会出现索引越界问题。

**注意事项**：
- `ParticleSystem.Burst`构造函数：`Burst(float time, float count)`或`Burst(float time, float minCount, float maxCount)`
- time是Burst触发的相对时间（秒）
- count是发射的粒子数量

---
**文档版本**: v1.0
**维护者**: Experience Manager
**最后更新**: 2026-01-28