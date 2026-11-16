---
name: tutorial-path-router
description: 将用户的游戏教程需求路由到本地教程注册表中的唯一教程路径。适用于用户提到 Unity 2D 肉鸽、Unity 肉鸽游戏教程、2D 游戏开发、Unity 动画系统、Unity 敌人 AI、Unity 战斗系统、roguelite 游戏或 Unity 2D 教程等场景。
---

# 教程路径路由器

## 目标

将用户的高层需求映射为 `.codely-cli/skills/planner/tutorials/` 下唯一一个最匹配的教程文件路径。

## 输入

自然语言需求，例如：
- 一个2d的肉鸽游戏
- 我想做Unity敌人AI和战斗系统

## 输出

仅返回一行：

`PATH: .codely-cli/skills/planner/tutorials/<file>.md`

除非用户明确要求细节，否则不要附加额外解释。

## 匹配流程

1. 从用户输入中提取需求关键词。
2. 与下方教程注册表进行对比。
3. 按关键词重合度和主题意图对教程打分。
4. 只返回得分最高的教程路径。
5. 如果分数相同，优先选择覆盖“从入门到完成”的完整教程。

## 教程注册表

### 1) Unity 2D 肉鸽完整流程
- File: `.codely-cli/skills/planner/tutorials/unity-2d-roguelite.md`
- Covers:
  - unity 2d 肉鸽
  - unity 肉鸽游戏教程
  - unity敌人ai
  - roguelite游戏
- Typical triggers:
  - 2D + 肉鸽 / roguelite
  - 角色动画 + 敌人AI + 战斗流程
  - 从场景到脚本的完整实战教程

## 回退规则

如果没有明显匹配项，默认返回：

`PATH: .codely-cli/skills/planner/tutorials/unity-2d-roguelite.md`

## 维护规则

当该目录新增教程 Markdown 文件时，需要把它追加到**教程注册表**，并登记：
- 文件路径
- 覆盖主题
- 触发关键词
