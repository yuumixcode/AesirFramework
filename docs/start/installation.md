---
description: "AesirFramework 的三种安装方式、升级与卸载路径,以及安装后的自检清单。"
---

# 安装与升级

AesirFramework 由两个同号发布的包组成,可以只装其中一个,也可以两个都装。

| 包 | 包 ID | 说明 |
| --- | --- | --- |
| **Aesir Architecture**(RAA) | `cn.runestone.aesir.architecture` | 渐进式 MVC / MVP 架构核心,不依赖任何 Aesir 子包 |
| **Aesir Modules**(RAM) | `cn.runestone.aesir.modules` | 功能模块集合(UI / 事件 / 音频 / 场景 / 文档生成),**依赖 RAA** |

## 前置要求

- Unity 或团结引擎 **2022.3+**(开发与验证环境:2022.3.62f3c1)
- 渲染管线无关。架构层是纯 C#,Modules 的 UI 部分使用 uGUI
- 可选依赖:Odin Inspector 3.3.x+(Inspector 增强 / Binder / 脚本文档生成)、Addressables、Input System —— 未安装时对应集成自动排除,不影响基础功能

## 选择安装方式

| 方式 | 适合 | 更新方式 | 代码位置 |
| --- | --- | --- | --- |
| **Git URL(常驻 latest 分支)** | 日常开发,推荐 | 移除后重新添加 | UPM 缓存,只读 |
| **unitypackage + 包内更新器** | 大陆网络受限 / 需要改源码 | 菜单一键更新 | `Assets/Runestone/` |
| **Git URL 跟踪 main** | 想第一时间用未发版的改动 | 同 latest | UPM 缓存,只读 |

### 方式 1:常驻 latest 分支(推荐)

Package Manager 左上角 `+` → **Add package from git URL...**,填入:

=== "只装 Architecture"

    ```text
    https://github.com/yuumixcode/AesirFramework.git#AesirArchitecture-latest
    ```

=== "同时装两个包"

    ```text
    https://github.com/yuumixcode/AesirFramework.git#AesirArchitecture-latest
    https://github.com/yuumixcode/AesirFramework.git#AesirModules-latest
    ```

`latest` 分支由 CI 在每次推送 `main` 时按包目录 subtree split 滚动更新,**分支名永久固定** —— Git URL 只输入一次,后续升级无需改地址。

**钉住旧版本**:改用 Release tag 形式 `?path=Assets/Runestone/AesirArchitecture#v<版本>`(tag 永久保留)。

!!! note "两个包都要装"

    RAM 依赖 RAA。包内 `package.json` 不声明任何依赖(Unity Package Manager 不接受包内的 Git URL 依赖),所以**同时安装两个包是使用者的责任**。只装 RAM 时会编译失败 —— 按下面的「依赖补全」处理。

### 方式 2:unitypackage + 包内更新器

从 [GitHub Releases](https://github.com/yuumixcode/AesirFramework/releases) 下载并导入:

| 资产 | 内容 |
| --- | --- |
| `AesirArchitecture-v<版本>.unitypackage` | 只含 RAA |
| `AesirModules-v<版本>.unitypackage` | 只含 RAM |
| `AesirFramework-v<版本>.unitypackage` | 两包合并(推荐) |

导入后代码位于 `Assets/Runestone/`,**可以直接修改**。更新无需重新下载:菜单 `Tools → Aesir → Check for Updates` 一键完成「检测 → 导入 → 差集清理残留」。详见[包内更新器](../architecture/updater.md)。

!!! warning "更新器只管 Assets 形态"

    经 Git URL 安装的副本不在更新器管辖内 —— 纯 UPM 安装形态下该菜单整体隐藏。开发仓库(目录里有 `.git`)切勿点更新,Release 内容会覆盖本地源码。

### 方式 3:跟踪 main(开发预览)

```text
https://github.com/yuumixcode/AesirFramework.git?path=Assets/Runestone/AesirArchitecture
```

不指定分支即跟随默认分支,能拿到未发版的改动,也意味着可能遇到中间态问题。

## 依赖补全(只装 RAM 时)

RAM 的 `package.json` 不能声明对 RAA 的 Git URL 依赖。若你只添加了 RAM,打开菜单:

```text
Tools → Aesir → Modules → Install Dependencies
```

它会检测 RAA 是否存在,缺失时确认后自动添加常驻 `latest` 分支固定地址。

## 安装后自检

| 检查项 | 预期 |
| --- | --- |
| Console 无编译错误 | 两包同时安装时无 `CS0246`(缺 RAA) |
| 菜单项存在 | `Tools → Aesir → Getting Started` / `Check for Updates` |
| 示例可导入 | Package Manager → 对应包 → **Samples** 标签页 |
| Context 可用 | 新建 `AbstractContext<T>` 子类后能正常初始化 |

## 升级与卸载

- **升级(Git URL)**:Package Manager 不会对 Git URL 包显示更新提示。移除旧包后用同一 URL 重新添加,或删除 `Packages/packages-lock.json` 中对应条目后重新解析
- **升级(unitypackage)**:直接用包内更新器;或从 Releases 下载新版重新导入
- **卸载**:Package Manager 中 Remove;unitypackage 形态直接删除 `Assets/Runestone/<包目录>`
- **两个包同版本**:两包同号发版,推荐始终安装同版本。混装不同版本时更新器会给出配套版本警告(不阻止)

## 下一步

- [Architecture 快速开始](../architecture/getting-started.md) —— 三个脚本跑通第一个 MVC 计数器
- [Modules 快速开始](../modules/getting-started.md) —— 创建 `UIRoot` 与第一个面板
- [兼容性矩阵](compatibility.md) —— 引擎版本、可选集成与线程约定
