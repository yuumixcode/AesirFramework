# 开发经验文档

## 编写原则
- **通用性**: 避免游戏特定术语，经验应适用于各种游戏开发
- **简洁性**: 描述简洁明了，避免冗余，代码示例只保留关键部分

## 基本信息
- **创建日期**: 2026-02-09
- **相关任务**: FlappyBird3D游戏物理设置阶段
- **技术栈**: Unity, C#, PowerShell

## 问题描述

### 问题表现
Unity编辑器报告编译错误：`error CS2001: Source file 'D:\Project\mwaisamples\Assets/Temp/FlappyBird3D_PhysicsSettings_Execute_20250209.cs' could not be found.`

即使脚本文件已被删除，Unity仍然报告找不到该文件的编译错误。删除Library/ScriptAssemblies目录和Library中相关文件后，错误仍然存在。

### 触发条件
- 创建并删除Unity编辑器脚本文件
- Unity的Library缓存中保留了对已删除文件的引用
- 重新编译或刷新Unity编辑器时触发

## 解决方案

### 关键步骤
1. 尝试删除Library/ScriptAssemblies目录强制重新编译
2. 尝试删除Library中所有与已删除脚本相关的文件
3. 当上述方法无效时，选择直接操作Unity资源文件

### 关键代码/命令
```powershell
# 删除脚本程序集缓存
Remove-Item "D:\Project\mwaisamples\Library\ScriptAssemblies" -Recurse -Force

# 删除Library中与特定脚本相关的所有文件
Get-ChildItem "D:\Project\mwaisamples\Library" -Recurse | Where-Object { $_.Name -like "*FlappyBird3D_PhysicsSettings_Execute*" } | Remove-Item -Force -Recurse
```

### 最终方案
当Unity编辑器Library缓存问题导致编译错误无法清除时，采取以下策略：

1. **直接操作Unity资源文件**：通过文件系统直接创建和修改Unity资源文件（.physicMaterial、.prefab等），而不是通过Unity编辑器API

2. **使用YAML格式创建物理材质**：
```yaml
%YAML 1.1
%TAG !u! tag:yousandi.cn,2023:
--- !u!134 &13400000
PhysicMaterial:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_Name: Default
  dynamicFriction: 0.6
  staticFriction: 0.6
  bounciness: 0
  frictionCombine: 0
  bounceCombine: 0
```

3. **手动创建.meta文件**：为每个资源文件创建对应的.meta文件，包含GUID信息

4. **直接修改Prefab文件**：通过文本编辑器修改Prefab的YAML文件，配置物理组件属性

### 校验方式
- 检查Unity编辑器Console是否还有编译错误
- 在Unity编辑器中打开资源文件，确认配置是否正确
- 运行游戏测试物理效果是否符合预期

### 经验总结
Unity编辑器的Library缓存机制有时会导致已删除文件的编译错误无法清除。当遇到此类问题时，如果常规的清除缓存方法无效，可以选择直接操作Unity资源文件作为替代方案。这种方法虽然不够优雅，但能够有效绕过编译错误，完成资源配置任务。建议在Unity编辑器重启后验证配置是否正确。

---
**文档版本**: v1.0
**维护者**: Experience Manager
**最后更新**: 2026-02-09