# 开发经验文档

## 编写原则
- **通用性**: 避免游戏特定术语，经验应适用于各种游戏开发
- **简洁性**: 描述简洁明了，避免冗余，代码示例只保留关键部分

## 基本信息
- **创建日期**: 2026-02-03
- **相关任务**: Unity场景灯光配置
- **技术栈**: Unity 2022.3

## 问题描述

### 问题表现
场景中的Directional Light在Inspector中启用时报错：
```
Quaternion To Matrix conversion failed because input Quaternion is invalid {x, y, z, w} l=0.997199
UnityEditor.Rendering.Universal.UniversalRenderPipelineLightEditor:OnSceneGUI ()
```

### 触发条件
- Directional Light的旋转四元数长度不是精确的1.0（单位四元数要求）
- 四元数长度为0.997199或1.003221等偏离1.0的值
- 在URP渲染管线中使用Directional Light

## 解决方案

### 关键步骤
1. 检查场景文件中Directional Light的m_Rotation值
2. 将四元数归一化为单位四元数（长度=1.0）
3. 或使用EulerAngles设置旋转，让Unity自动生成正确的四元数

### 关键代码/命令
```csharp
// 错误的四元数（长度≠1.0）
Quaternion invalidRotation = new Quaternion(0.462921f, -0.191891f, 0.099334f, 0.861531f);

// 正确的单位四元数（长度=1.0）
Quaternion validRotation = Quaternion.Euler(50, -30, 0); // 自动归一化

// 或手动归一化
Quaternion normalizedRotation = invalidRotation.normalized;
```

### 最终方案
使用EulerAngles设置Directional Light的旋转，避免直接操作四元数：
- EulerAngles: {x: 50, y: -30, z: 0}
- Unity自动计算正确的单位四元数：{x: 0.1093817, y: 0.4082179, z: -0.2345697, w: 0.8754261}

---
**文档版本**: v1.0
**维护者**: Experience Manager
**最后更新**: 2026-02-03