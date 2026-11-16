# 开发经验文档

## 编写原则
- **通用性**: 避免游戏特定术语，经验应适用于各种游戏开发
- **简洁性**: 描述简洁明了，避免冗余，代码示例只保留关键部分

## 基本信息
- **创建日期**: 2026-01-22
- **相关任务**: 修复Unity游戏场景缺少GameManager导致无法响应输入问题
- **技术栈**: Unity, C#, GameObject, Singleton Pattern

## 问题描述

### 问题表现
Unity游戏进入Play模式后，玩家对象（如Player）无法响应键盘输入。PlayerController的输入回调方法（如OnJumpPerformed）未被调用。PlayerController代码中检查GameManager.Instance.IsGameActive返回false。场景中找不到GameManager GameObject。

### 触发条件
- 游戏使用GameManager单例模式管理游戏状态
- PlayerController或其他脚本依赖GameManager.Instance
- 场景中没有GameManager GameObject

## 解决方案

### 关键步骤
1. 使用GameObject.CreatePrimitive()或unity_gameobject工具创建GameManager GameObject
2. 添加GameManager组件到GameObject
3. 保存场景
4. 编译并测试游戏

### 关键代码/命令
**方案1：使用Unity Codely CLI工具（推荐）**
```bash
# 使用unity_gameobject工具创建GameManager
unity_gameobject create --name "GameManager"
unity_gameobject add_component --target "GameManager" --componentName "YourNamespace.GameManager"
```

**方案2：在Unity编辑器中手动创建**
1. 在Hierarchy窗口右键 -> Create Empty
2. 重命名为"GameManager"
3. 在Inspector窗口点击Add Component
4. 搜索并添加GameManager组件

### 最终方案
在场景中创建GameManager GameObject并添加GameManager组件。GameManager是单例模式，应该由场景统一管理。确保游戏状态管理器的存在和正确初始化。便于在Unity编辑器中配置GameManager的参数。

---
**文档版本**: v4.0
**维护者**: Experience Manager
**最后更新**: 2026-01-22