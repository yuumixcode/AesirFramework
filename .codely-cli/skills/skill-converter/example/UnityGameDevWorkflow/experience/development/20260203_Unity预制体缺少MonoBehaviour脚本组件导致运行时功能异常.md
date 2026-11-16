# 开发经验文档

## 编写原则
- **通用性**: 避免游戏特定术语，经验应适用于各种游戏开发
- **简洁性**: 描述简洁明了，避免冗余，代码示例只保留关键部分

## 基本信息
- **创建日期**: 2026-02-03
- **相关任务**: Unity游戏运行时验证与修复
- **技术栈**: Unity, C#

## 问题描述

### 问题表现
预制体文件存在但缺少必要的MonoBehaviour脚本组件，导致：
- 预制体实例化后无法正常工作
- GameObject.FindObjectOfType<T>()无法找到对应的脚本组件
- 运行时行为异常，无法实现预期功能

### 触发条件
- 预制体通过编辑器脚本创建时，未添加必要的脚本组件
- 预制体在创建时脚本尚未编译完成
- 预制体修改后未正确保存脚本组件引用

## 解决方案

### 关键步骤
1. 加载预制体资产：AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath)
2. 检查是否已有脚本组件：gameObject.GetComponent<T>()
3. 添加缺失的脚本组件：gameObject.AddComponent<T>()
4. 配置脚本参数：使用SerializedObject设置字段值
5. 标记资产为脏并保存：EditorUtility.SetDirty() + AssetDatabase.SaveAssets()

### 关键代码/命令
```csharp
// 加载预制体
GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

// 检查并添加脚本组件
if (prefab.GetComponent<MyScript>() == null)
{
    MyScript script = prefab.AddComponent<MyScript>();
    
    // 配置脚本参数
    SerializedObject serialized = new SerializedObject(script);
    SerializedProperty property = serialized.FindProperty("myField");
    property.intValue = 10;
    serialized.ApplyModifiedProperties();
    
    // 保存预制体
    EditorUtility.SetDirty(prefab);
    AssetDatabase.SaveAssets();
}
```

### 最终方案
通过编辑器脚本批量检查和修复预制体的脚本组件缺失问题。对于需要运行时行为的预制体，必须确保在预制体创建阶段正确添加所有必要的脚本组件，并配置好初始参数。使用SerializedObject而非直接访问私有字段来配置脚本参数，确保配置正确应用。

---
**文档版本**: v1.0
**维护者**: Experience Manager
**最后更新**: 2026-02-03