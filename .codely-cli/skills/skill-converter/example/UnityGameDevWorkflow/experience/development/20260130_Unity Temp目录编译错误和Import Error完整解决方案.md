# 开发经验文档

## 编写原则
- **通用性**: 避免游戏特定术语，经验应适用于各种游戏开发
- **简洁性**: 描述简洁明了，避免冗余，代码示例只保留关键部分

## 基本信息
- **创建日期**: 2026-01-30
- **相关任务**: Unity编辑器脚本开发 - 解决编译错误和Import Error
- **技术栈**: Unity 2022.3, C#, PowerShell, AssetDatabase

## 问题描述

### 问题表现1：编译错误
在Unity中通过编辑器脚本创建Prefab时，遇到以下问题：
1. 编译错误：`error CS2001: Source file 'Assets/Temp/XXX.cs' could not be found`
2. 多个临时脚本文件被删除后，Unity编译系统仍尝试编译这些文件
3. 编译失败导致无法执行新的编辑器脚本

### 问题表现2：Import Error
修改脚本文件后Unity出现Import Error，导致脚本无法加载，编译失败：
1. Build asset version error - 文件时间戳不匹配
2. 资源导入失败，无法继续开发

### 触发条件
- 在`Assets/Temp/`目录创建临时编辑器脚本
- 临时脚本被删除后，对应的.meta文件可能仍然存在
- Unity的编译系统缓存了已删除文件的引用
- 修改脚本文件后Unity缓存冲突

## 解决方案

### 关键步骤

#### 方案1：删除无效的meta文件（推荐）

**PowerShell 命令 - 删除无效的meta文件：**
```powershell
# 删除Temp目录下所有不存在.cs文件的.meta文件
$tempDir = "E:\Projects\mwaisamples\Assets\Temp"
Get-ChildItem $tempDir -Include "*.meta" -Recurse | ForEach-Object {
    $csFile = $_.FullName -replace '\.meta$', ''
    if (-not (Test-Path $csFile)) {
        Remove-Item $_.FullName -Force
        Write-Host "已删除无效的meta文件: $($_.Name)"
    }
}
```

**PowerShell 命令 - 删除冲突的meta文件：**
```powershell
# 删除冲突的meta文件解决Import Error
Remove-Item "Assets\Temp.meta" -ErrorAction SilentlyContinue
Remove-Item "Assets\Temp\ScriptName.cs.meta" -ErrorAction SilentlyContinue
```

#### 方案2：刷新AssetDatabase

**C#脚本 - 刷新AssetDatabase：**
```csharp
using UnityEngine;
using UnityEditor;

namespace EditorAutomation
{
    public class RefreshAssetDatabase
    {
        public static string execute()
        {
            try
            {
                AssetDatabase.Refresh();
                Debug.Log("AssetDatabase refreshed successfully!");
                return "success";
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to refresh AssetDatabase: {e.Message}");
                return $"failed: {e.Message}";
            }
        }
    }
}
```

#### 方案3：等待Unity编译完成

**Unity Editor 命令：**
```bash
# 等待Unity完成编译
unity_editor wait_for_compile

# 等待Unity编辑器进入空闲状态
unity_editor wait_for_idle
```

### 关键代码/命令

**完整的清理流程：**
```powershell
# 1. 删除Temp目录下所有无效的meta文件
$tempDir = "E:\Projects\mwaisamples\Assets\Temp"
if (Test-Path $tempDir) {
    Get-ChildItem $tempDir -Include "*.meta" -Recurse | ForEach-Object {
        $csFile = $_.FullName -replace '\.meta$', ''
        if (-not (Test-Path $csFile)) {
            Remove-Item $_.FullName -Force
            Write-Host "已删除无效的meta文件: $($_.Name)"
        }
    }
}

# 2. 删除Temp.meta文件（如果存在）
Remove-Item "Assets\Temp.meta" -ErrorAction SilentlyContinue

# 3. 使用Unity工具刷新AssetDatabase
# (通过execute_custom_tool执行RefreshAssetDatabase脚本)

# 4. 等待Unity完成编译
# (通过unity_editor wait_for_compile等待编译完成)
```

### 最终方案

**Unity编辑器脚本清理编译错误的完整流程：**

1. **识别问题文件**：
   - 查看编译错误信息，找到所有缺失的.cs文件路径
   - 检查`Assets/Temp/`目录，确认哪些.meta文件对应的.cs文件已不存在

2. **清理无效meta文件**：
   - 使用PowerShell脚本删除所有无效的.meta文件
   - 确保Temp.meta文件格式正确或删除

3. **刷新AssetDatabase**：
   - 创建刷新脚本并执行
   - 使用`AssetDatabase.Refresh()`强制Unity刷新资源数据库
   - 等待Unity完成资源数据库刷新

4. **验证编译**：
   - 请求Unity重新编译
   - 检查控制台确认没有编译错误
   - 继续执行后续任务

**最佳实践：**
- 在Unity中删除临时脚本文件时，务必同时删除对应的.meta文件
- 使用`AssetDatabase.Refresh()`可以强制Unity刷新资源数据库
- 避免在`Assets/Temp/`目录下积累过多的临时文件
- 建议定期清理Temp目录，保持项目整洁
- 修改脚本后等待Unity编译完成再创建新脚本
- 遇到编译缓存问题时使用新文件名（如`_v2`后缀）避免缓存冲突

---
**文档版本**: v1.0
**维护者**: Experience Manager
**最后更新**: 2026-01-30