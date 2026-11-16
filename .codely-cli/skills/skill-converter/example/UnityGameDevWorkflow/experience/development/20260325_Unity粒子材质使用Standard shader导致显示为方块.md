# 开发经验文档

## 编写原则
- **通用性**: 避免游戏特定术语，经验应适用于各种游戏开发
- **简洁性**: 描述简洁明了，避免冗余，代码示例只保留关键部分

## 基本信息
- **创建日期**: 2026-03-25
- **相关任务**: Unity 粒子系统材质配置
- **技术栈**: Unity/C#/ParticleSystem
- **来源Skill**: unity-collectible-system

## 问题描述

### 问题表现
通过代码创建的粒子系统，粒子显示为不透明的白色/彩色方块，而非预期的柔和圆形粒子。

### 触发条件
- ParticleSystemRenderer 的材质使用了 `Standard` shader
- 未使用粒子专用 shader

## 解决方案

### 关键步骤
1. 粒子材质必须使用 `Particles/Standard Unlit` shader
2. 设置材质颜色与粒子 startColor 一致

### 关键代码/命令
```csharp
// ❌ 错误：使用 Standard shader
renderer.material = new Material(Shader.Find("Standard"));

// ✅ 正确：使用粒子专用 shader
var renderer = go.GetComponent<ParticleSystemRenderer>();
renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
renderer.material.color = new Color(1f, 0.85f, 0.2f); // 粒子颜色
```

### 最终方案
代码创建粒子系统时，ParticleSystemRenderer 的材质必须使用 `Particles/Standard Unlit` shader，不能用 `Standard`。

---
**文档版本**: v1.0
**维护者**: Experience Manager
**最后更新**: 2026-03-25
