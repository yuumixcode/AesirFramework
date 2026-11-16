# 开发经验文档

## 编写原则
- **通用性**: 避免游戏特定术语，经验应适用于各种游戏开发
- **简洁性**: 描述简洁明了，避免冗余，代码示例只保留关键部分

## 基本信息
- **创建日期**: 2026-03-25
- **相关任务**: Unity 素材加载架构
- **技术栈**: Unity/C#/AssetDatabase
- **来源Skill**: unity-asset-loader

## 问题描述

### 问题表现
1. 编译报错：`The name 'AssetDatabase' does not exist in the current context`
2. `AssetDatabase.LoadAssetAtPath` 路径正确但返回 null

### 触发条件
1. 在非 Editor 脚本（`Assets/Scripts/` 下）中使用 `UnityEditor.AssetDatabase`
2. `LoadAssetAtPath` 的路径大小写与磁盘文件不一致（Windows 不敏感但其他平台敏感）

## 解决方案

### 关键步骤
1. `AssetDatabase` **只能在 Editor 脚本中使用**（`Assets/Editor/` 下或 `#if UNITY_EDITOR` 包裹）
2. 运行时脚本加载素材用 public 字段（由 Editor 脚本赋值）或 `Resources.Load`
3. `LoadAssetAtPath` 路径必须与磁盘文件完全一致

### 关键代码/命令
```csharp
// ❌ 错误：运行时脚本中直接用 AssetDatabase
// Assets/Scripts/MyScript.cs
using UnityEditor;  // 编译报错！
Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Models/Player.asset");

// ✅ 正确方案1：public 字段由 Editor 脚本赋值
// Assets/Scripts/MyScript.cs
public class MyScript : MonoBehaviour
{
    [Header("由 SceneBuilder 赋值")]
    public Mesh playerMesh;  // Editor 脚本中赋值，序列化到场景
}

// Assets/Editor/SceneBuilder.cs
var script = obj.AddComponent<MyScript>();
script.playerMesh = AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Models/Player.asset");

// ✅ 正确方案2：Resources.Load（素材需放在 Assets/Resources/ 下）
Mesh mesh = Resources.Load<Mesh>("Models/Player");  // 路径不含扩展名
```

### 最终方案
严格区分 Editor 脚本和运行时脚本的职责：Editor 脚本负责加载素材并赋值给运行时组件的 public 字段；运行时脚本通过 public 字段使用素材，null 时 fallback 到程序化生成。路径大小写必须与磁盘一致。

---
**文档版本**: v1.0
**维护者**: Experience Manager
**最后更新**: 2026-03-25
