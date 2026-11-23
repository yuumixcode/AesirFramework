---
title: AesirUniTaskDefineKeeper
description: "Runestone.AesirModules.Editor.AesirUniTaskDefineKeeper 的 API 文档"
---

# `AesirUniTaskDefineKeeper`

!!! note ""

    - **种类:** `static class`
    - **命名空间:** `Runestone.AesirModules.Editor`
    - **程序集:** `Runestone.AesirModules.Editor`

**继承链:** `System.Object` → `AesirUniTaskDefineKeeper`

## 声明

``` csharp
[InitializeOnLoad]
internal static class AesirUniTaskDefineKeeper
```

自动维护 AESIR_MODULES_UNITASK 脚本宏定义符号——SceneModule 的 UniTask 驱动分支与 UniTask 适配程序集（Runestone.AesirModules.UniTask）均以该宏为编译开关。
维护规则：游戏工程以 UPM 包（com.cysharp.unitask）安装 UniTask 时， 宏由核心程序集与适配程序集的 versionDefines 全权管理（装/卸自动生效），本维护器不干预全局符号； 已知边界：UPM 判定依赖 Package Manager 的已注册包列表，若 delayCall 触发时列表尚未就绪会误判为 非 UPM 形态而写入全局宏，此后 UPM 形态只返回"不干预"、不会回收该残留（功能无害，属 ProjectSettings 残留）； 其他安装形态（unitypackage / DLL 导入）按「域内是否存在 UniTask 程序集」 增删全局符号——存在则补齐，不存在则移除。

**备注**

写入宏会触发脚本重编译，而静态构造函数运行在程序集注册 / 域重载期间—— 此时直接发起重编译属于重入，可能使 Unity 走到程序集注册的致命分支。 故实际同步推迟到 delayCall（编辑器空闲首帧）执行； ScriptingSymbolEditorUtility 本身按构建目标逐一比对、仅在符号确实变化时才写入 （无变化则零写入、不触发重编译），此行为不因推迟而改变。
边界：工程移除 unitypackage 形态的 UniTask 后，全局宏在下一次域重载才被移除——若先于维护器 运行触发编译，核心程序集的 UniTask 分支会出现 CS0246，重装 UniTask 或在 Project Settings 手动移除该宏即可恢复。

## 方法

<div class="api-summary-table" markdown="1">

| 名称 | 描述 | 声明类型 |
| :--- | :--- | :--- |
| `GetType()` | — | `object` |
| `Equals(object)` | — | `object` |
| `GetHashCode()` | — | `object` |
| `ToString()` | — | `object` |
| `MemberwiseClone()` | — | `object` |
| `Finalize()` | — | `object` |

</div>

## Additional Notes

> 首个 `## Additional Notes` 是增量生成文档标识符，请勿修改标题级别和内容！本文档由 [`Script Doc Generator`](https://github.com/yuumixcode/AesirFramework) 辅助生成。
