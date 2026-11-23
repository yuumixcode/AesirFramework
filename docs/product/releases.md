---
description: "AesirFramework 当前版本、版本策略、发布产物与更新路径。"
---

# 版本与下载

## 当前版本

| 包 | 包名 | 版本 | 发布 |
| --- | --- | --- | --- |
| Aesir Architecture | `cn.runestone.aesir.architecture` | **0.35.0** | 2026-11-23 |
| Aesir Modules | `cn.runestone.aesir.modules` | **0.35.0** | 2026-11-23 |

两包**同号发版**(CI 校验一致),推荐始终安装同版本。完整变更见[更新日志](../changelog.md)。

## 版本策略

- 版本号遵循[语义化版本](https://semver.org/lang/zh-CN/),变更记录遵循 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/)
- `0.x` 阶段:破坏性 API 变更会在 CHANGELOG 的 **Changed / Removed** 段明确标注,来龙去脉写在提交说明与内部笔记仓的过程记录里(包内不保留设计变更记录)
- 版本 tag 一旦发布即不可变、永久保留;版本号 bump 后的首次推送才创建新 tag。钉住旧版本请用 tag 而不是分支名

## 获取方式

| 方式 | 地址 | 适用 |
| --- | --- | --- |
| **常驻 latest 分支**(推荐) | `https://github.com/yuumixcode/AesirFramework.git#AesirArchitecture-latest`<br>`https://github.com/yuumixcode/AesirFramework.git#AesirModules-latest` | 日常开发;分支名永久固定,升级=移除后重新添加 |
| **Release unitypackage** | [Releases 页面](https://github.com/yuumixcode/AesirFramework/releases) | 大陆网络受限 / 需要改源码;可用包内更新器一键升级 |
| **钉住某个版本** | `...?path=Assets/Runestone/AesirArchitecture#v<版本>` | 复现问题、锁定依赖 |
| **跟踪 main** | `...?path=Assets/Runestone/AesirArchitecture` | 想要未发版的改动 |

安装细节与依赖补全见[安装与升级](../start/installation.md)。

## 每次 Release 的产物

| 资产 | 内容 |
| --- | --- |
| `AesirArchitecture-v<版本>.unitypackage` | 只含 RAA |
| `AesirModules-v<版本>.unitypackage` | 只含 RAM |
| `AesirFramework-v<版本>.unitypackage` | 两包合并(推荐) |

unitypackage 由仓库内的纯 Python 脚本生成(不依赖第三方 CLI),跳过 `~` 后缀与 `.` 开头的目录 —— 包内的 `Samples~` / `Documentation~` 不进包,`.meta` 与主位同 GUID,保证 UPM 与 Assets 两种形态不冲突。

## 更新路径

| 你的安装形态 | 怎么升级 |
| --- | --- |
| Git URL(latest / main) | Package Manager 不会提示更新:移除旧包后用同一 URL 重新添加,或删掉 `packages-lock.json` 对应条目重新解析 |
| unitypackage(Assets 形态) | 菜单 `Tools → Aesir → Check for Updates` 一键完成检测 / 导入 / 残留清理 |
| 开发仓库(有 `.git`) | 直接 `git pull`;**不要**用更新器,Release 内容会覆盖本地源码 |

## latest 分支是怎么来的

主仓是 monorepo(主仓在 CNB,GitHub 为镜像),两个包位于 `Assets/Runestone/` 下。每次推送 `main`,CNB 流水线 `.cnb.yml`:

1. `git subtree split` 把每个包滚动发布到 `AesirArchitecture-latest` / `AesirModules-latest`(分支根即包根,并裁掉开发侧 `Samples/` 与 `Documentation/`),同时清理废弃的 `<包>-v<旧版本>` 分支
2. 回写 `.github/update-info.json`(供更新器直连检测)
3. 滚动打 `v<版本>` tag,并经 git-sync 把 main / 两包分支 / tag 单向同步到 GitHub

tag 推送后,CNB 与 GitHub 各自构建并发布三个 unitypackage(CNB Release 供国内下载,GitHub Release 供国际用户),两边资产同源。

这也解释了为什么 Git URL 里要带 `#<包>-latest`:包内容在分支根部,不需要 `?path=`。

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
