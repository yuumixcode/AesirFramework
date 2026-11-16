# 开发经验文档

## 编写原则
- **通用性**: 避免游戏特定术语，经验应适用于各种游戏开发
- **简洁性**: 描述简洁明了，避免冗余，代码示例只保留关键部分

## 基本信息
- **创建日期**: 2026-03-25
- **相关任务**: Unity 材质属性设置
- **技术栈**: Unity/C#/Built-In Render Pipeline
- **来源Skill**: unity-asset-loader

## 问题描述

### 问题表现
通过代码设置材质光泽度为 0，但材质仍然有明显的高光/光泽效果，`SetFloat` 调用无报错但无效果。

### 触发条件
- 使用 Built-In Render Pipeline 的 Standard shader
- 代码中使用 `mat.SetFloat("_Smoothness", 0f)` 设置光泽度
- Standard shader 的实际属性名是 `_Glossiness`，不是 `_Smoothness`

## 解决方案

### 关键步骤
1. Standard shader（Built-In RP）的光泽度属性名是 **`_Glossiness`**
2. URP/HDRP 的 Lit shader 才用 `_Smoothness`

### 关键代码/命令
```csharp
// ❌ 错误：属性名写错，设置无效（无报错！）
mat.SetFloat("_Smoothness", 0f);

// ✅ 正确：Built-In Standard shader
mat.SetFloat("_Glossiness", 0f);
mat.SetFloat("_Metallic", 0f);

// ✅ 如果是 URP Lit shader
mat.SetFloat("_Smoothness", 0f);
```

### 最终方案
Built-In Render Pipeline 的 Standard shader 光泽度属性名是 `_Glossiness`（Unity 2022）。`SetFloat` 对不存在的属性名不会报错，只是静默无效，这是一个非常隐蔽的陷阱。

---
**文档版本**: v1.0
**维护者**: Experience Manager
**最后更新**: 2026-03-25
