---
description: "AesirFramework 周边的仓库、AI 技能文档集、工具链与第三方依赖出处。"
---

# 工具链与生态

## 官方仓库

| 仓库 | 定位 | 是否随包分发 |
| --- | --- | --- |
| [AesirFramework](https://github.com/yuumixcode/AesirFramework) | 框架源码(两个包 + 示例 + 构建脚本) | — |
| [AesirInspector](https://github.com/yuumixcode/AesirInspector) | 面向 Odin Inspector 开发者的学习工具包(已从主仓迁出) | 独立仓库 |
| [AesirFramework docs/](https://github.com/yuumixcode/AesirFramework/tree/main/docs) | 本站(产品介绍 + 使用文档 + Scripting API,与主仓同仓) | 随主仓 |
| `Aesir-Notebook` | 开发过程文档:设计变更、模块锐评、全仓审查、课程规划 | 内部,不公开 |

## AI 技能:整包扩展 `AesirFrameworkGame`

AI 编码指南不随包分发,统一放在源码仓库根的 `.codely-cli/extensions/AesirFrameworkGame/`——一个扩展同时覆盖两个包,可直接被团结 Cowork / Codely CLI / Copilot / Cursor / Claude 等工具加载:

| 技能 | 回答什么 |
| --- | --- |
| `aesir-router` | 入口与分派:依赖门禁、意图 → 技能、组合路径 |
| `aesir-framework` / `aesir-tiers` / `aesir-core-api` / `aesir-decisions` / `aesir-pitfalls` / `aesir-boot-scene` | 架构底座、六档分级与模板、核心 API、模式与通知机制决策、高频陷阱、启动场景 |
| `aesir-ui` / `aesir-events` / `aesir-audio` / `aesir-scene` | Aesir Modules 四个功能模块 |
| 2 篇 templates | 完整小游戏骨架、原生 MonoBehaviour → Aesir 迁移表 |

安装:团结 Cowork 用户稀疏克隆扩展目录后 `codely extensions install`;其他工具取 `skills/` 拆进自己的技能目录。下载与安装说明见包内 `TuanjieCowork~/README.md`。

包内只保留三处**产品文档**:包根 `README.md` / `README_EN.md`、`Documentation~/`(使用文档)、`TuanjieCowork~/README.md`(技能获取入口)。迭代过程与内部结论写进内部笔记仓,不随 `unitypackage` 分发;`.codely-cli/` 与 `~` 结尾目录对 AssetDatabase 不可见,AI 内容不进入任何 Unity 工程。

## 编辑器工具

| 工具 | 入口 | 说明 |
| --- | --- | --- |
| Getting Started | `Tools → Aesir → Getting Started` | 以各包 `package.json` 的 samples 清单为唯一真源,集中浏览 / 定位 / 一键导入示例 |
| Check for Updates | `Tools → Aesir → Check for Updates` | 仅 Assets 形态可见;三层源检测(直连 GitHub → 镜像站 → CDN 中转),见[包内更新器](../architecture/updater.md) |
| Install Dependencies | `Tools → Aesir → Modules → Install Dependencies` | 只装 RAM 时补装 RAA |
| Scene Module Settings | `Tools → Aesir → Modules → Scene Module Settings` | 启动场景与进度上限配置 |
| Script Doc Generator | `Tools → Aesir → Modules → Script Doc Generator` | 反射生成结构化 API 文档(需 Odin);本站 API 页即由它产出 |

## 交付链路

| 环节 | 机制 |
| --- | --- |
| 版本发布 | 推送 `main` 后 GitHub Actions 构建三个 unitypackage 并发布 Release,同时回写 `update-info.json` 供更新器直连检测 |
| 分支分发 | `git subtree split` 把每个包滚动发布到常驻 `<包>-latest` 分支,分支名永久固定 |
| 文档站 | 本站由 Zensical 构建,GitHub Pages 部署;API 页由 Script Doc Generator 生成后同步 |
| 示例同步 | 包内 `Samples/` 是编写主位,`Samples~/` 是 UPM 镜像,由同步脚本逐文件比对方向 |

## 参考依赖与出处

设计参考、工具上游与素材出处统一收录在[推荐项目与参考致谢](acknowledgments.md),避免在多个页面重复维护。完整授权说明见主仓 `Assets/Runestone/AesirModules/Third Party Notices.md` 与各包 `LICENSE.md`。
