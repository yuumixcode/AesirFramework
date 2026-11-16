# 开发经验文档

## 编写原则
- **通用性**: 避免游戏特定术语，经验应适用于各种游戏开发
- **简洁性**: 描述简洁明了，避免冗余，代码示例只保留关键部分

## 基本信息
- **创建日期**: 2026-01-17
- **相关任务**: 修复EndlessRunner3D游戏UI遮挡Camera问题
- **技术栈**: Unity, UGUI, UI Toolkit

## 问题描述

### 问题表现
Game窗口里Camera完全被UGUI Canvas挡住了，看不到场景。UI使用UGUI（Canvas、Image、Text等组件），场景中的游戏对象（玩家、地面、障碍物等）无法正常显示。

### 触发条件
- 场景中存在UGUI Canvas对象
- Canvas的Render Mode设置为Screen Space - Overlay
- Camera的Depth值低于Canvas的渲染层级

## 解决方案

### 关键步骤
1. 禁用或删除场景中的UGUI Canvas对象
2. 确保PRD中不包含UI相关工作
3. 如需UI功能，使用UI Toolkit（UXML/USS）或仅在脚本中预留接口

### 关键代码/命令
```bash
# 禁用Canvas对象（在Unity编辑器中）
# 选中Canvas对象，取消勾选Inspector中的Active复选框

# 或者删除Canvas对象
# 在Hierarchy中选择Canvas，按Delete键
```

### 最终方案
绝对禁止使用UGUI。不要创建Canvas、Image、Text、Button等UGUI组件。UI相关工作不在任务范围内，不需要为UI创建PRD或实现相关功能。如需UI，使用UI Toolkit（UXML/USS）或仅在脚本中预留UI接口。

---
**文档版本**: v4.0
**维护者**: Experience Manager
**最后更新**: 2026-01-17