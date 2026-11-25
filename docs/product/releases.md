---
description: "AesirFramework 当前版本、版本策略、发布产物与更新路径。"
---

# 版本与下载

## 当前版本

| 包 | 包名 | 版本 | 发布 |
| --- | --- | --- | --- |
| Aesir Architecture | `cn.runestone.aesir.architecture` | **0.36.0** | 2026-11-24 |
| Aesir Modules | `cn.runestone.aesir.modules` | **0.36.0** | 2026-11-24 |

两包**同号发版**(CI 校验一致),推荐始终安装同版本。完整变更见[更新日志](../changelog.md)。

## 版本策略

- 版本号遵循[语义化版本](https://semver.org/lang/zh-CN/),变更记录遵循 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/)
- `0.x` 阶段:破坏性 API 变更会在 CHANGELOG 的 **Changed / Removed** 段明确标注,来龙去脉写在提交说明与内部笔记仓的过程记录里(包内不保留设计变更记录)
- 版本 tag 一旦发布即不可变、永久保留;版本号 bump 后的首次推送才创建新 tag。钉住旧版本请用 tag 而不是分支名

## 如何获取

三种安装方式(Git URL 常驻 `latest` 分支 / Release unitypackage / 钉版本)、依赖补全与升级卸载见[安装与升级](../start/installation.md)。本节只讲版本策略与发布产物。

## 每次 Release 的产物

| 资产 | 内容 |
| --- | --- |
| `AesirArchitecture-v<版本>.unitypackage` | 只含 RAA |
| `AesirModules-v<版本>.unitypackage` | 只含 RAM |
| `AesirFramework-v<版本>.unitypackage` | 两包合并(推荐) |

unitypackage 由仓库内的纯 Python 脚本生成(不依赖第三方 CLI),跳过 `~` 后缀与 `.` 开头的目录 —— 包内的 `Samples~` / `Documentation~` 不进包,`.meta` 与主位同 GUID,保证 UPM 与 Assets 两种形态不冲突。

## 更新路径

升级方式按安装形态区分(Git URL 移除后重新添加 / unitypackage 用包内更新器 / 开发仓库 `git pull`),见[安装与升级](../start/installation.md)。`latest` 分支由 CI 在每次推送 `main` 时按包目录 subtree split 滚动更新、分支名永久固定,故 Git URL 一次输入持续可用。

## 确认自己装的是哪版

| 形态 | 查看位置 |
| --- | --- |
| UPM | Package Manager → 对应包(或 `Packages/packages-lock.json`) |
| Assets / unitypackage | `Assets/Runestone/<包目录>/package.json` 的 `version` 字段 |
| 任一形态 | 菜单 `Tools → Aesir → Check for Updates`(仅 Assets 形态可见)会显示本地与远程版本 |

## 历史版本

- [更新日志](../changelog.md) —— 站点摘要视图,按版本倒序
- [根 CHANGELOG.md](https://github.com/yuumixcode/AesirFramework/blob/main/CHANGELOG.md) —— 完整历史(含 0.14.0 之前)
- [Releases](https://github.com/yuumixcode/AesirFramework/releases) —— 每个版本的 unitypackage 与发布说明
