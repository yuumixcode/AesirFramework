# Unity编辑器脚本ExecuteCSharpScript日志显示为Exception问题

## 问题描述
在使用 `execute_csharp_script` 工具执行 Unity 编辑器脚本时，脚本中通过 `Debug.Log` 输出的正常日志信息，在工具返回的 JSON 结果中，其 `type` 字段可能被错误标记为 `Exception`，尽管脚本执行成功且 `success` 字段为 `true`。

## 触发条件
- 使用 `execute_csharp_script` 工具执行包含 `Debug.Log` 的 C# 脚本。
- 工具捕获日志时可能存在的解析或分类逻辑问题。

## 解决方案
1. **检查 `success` 字段**：首先检查工具返回的 `success` 字段是否为 `true`。
2. **检查日志内容**：如果 `success` 为 `true`，即使日志类型标记为 `Exception`，也应检查 `message` 内容。如果内容是预期的日志信息而非错误堆栈，则可忽略 `Exception` 类型标记。
3. **使用 `Debug.LogError` 明确错误**：在脚本中明确使用 `Debug.LogError` 来输出真正的错误信息，以便区分。

## 校验方式
- 查看工具返回的 JSON 数据。
- 确认 `success: true`。
- 确认 `logs` 数组中的 `message` 内容符合预期输出。

## 关联文档
- 无

## 经验总结
在使用自动化工具执行脚本时，工具的日志分类可能不准确。应综合判断执行结果状态 (`success`) 和日志内容 (`message`)，而不仅仅依赖日志类型标签 (`type`)。对于关键操作，建议在脚本执行完毕后通过其他方式（如文件检查、组件检查）进行二次验证。
