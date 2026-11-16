# 开发经验文档

## 编写原则
- **通用性**: 避免游戏特定术语，经验应适用于各种游戏开发
- **简洁性**: 描述简洁明了，避免冗余，代码示例只保留关键部分

## 基本信息
- **创建日期**: 2026-01-28
- **相关任务**: Unity 3D物理碰撞检测
- **技术栈**: Unity/C#/Physics

## 问题描述

### 问题表现
游戏对象之间发生物理碰撞，但OnCollisionEnter(Collision collision)回调方法未被触发，或者触发了但在检查collision.collider.CompareTag()时返回false，导致碰撞逻辑未执行。

### 触发条件
- 碰撞体的Tag设置为默认值"Untagged"
- 代码中使用CompareTag检查特定Tag（如"Ball"、"Brick"等）
- 预制体或场景对象的Tag在创建时未正确设置

## 解决方案

### 关键步骤
1. 在场景搭建阶段正确设置关键对象的Tag
2. 在预制体创建时设置预制体的Tag
3. 使用GameObject.Find()或直接引用对象后设置其Tag
4. 使用unity_gameobject工具或编辑器脚本批量设置Tag

### 关键代码/命令
```csharp
// 方法1：编辑器脚本设置Tag
GameObject ball = GameObject.Find("Ball");
if (ball != null)
{
    ball.tag = "Ball";
}

// 方法2：预制体设置
GameObject brickPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Brick.prefab");
if (brickPrefab != null)
{
    brickPrefab.tag = "Brick";
    EditorUtility.SetDirty(brickPrefab);
    AssetDatabase.SaveAssets();
}

// 方法3：碰撞检测代码
private void OnCollisionEnter(Collision collision)
{
    if (collision.collider.CompareTag("Ball"))
    {
        // 处理球体碰撞
    }
}
```

### 最终方案
在场景搭建和预制体创建阶段，务必正确设置关键游戏对象的Tag。建议在PRD文档的"Tag、Layer配置"阶段明确列出所有需要的Tag，并在场景搭建阶段统一设置。对于预制体，修改Tag后需要保存预制体资产。使用CompareTag()方法而非直接比较tag字符串，因为CompareTag()性能更优。

---
**文档版本**: v1.0
**维护者**: Experience Manager
**最后更新**: 2026-01-28