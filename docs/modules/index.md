# Aesir Modules 概览

**Aesir Modules(RAM)** 是 Aesir Architecture 之上的功能模块集合,采用标准 Unity 自定义包根结构(`Runtime/` / `Editor/` 两级,模块以子目录存在,模块间零依赖 —— 删除模块 = 删除对应目录)。

## 模块总览

| 模块 | 状态 | 说明 |
|------|------|------|
| **UI** | 已实现 | `UIModule` 单例(Manager of Managers)+ `UIRoot` 四层 Canvas + 面板生命周期 + 可插拔资源加载 |
| **Event** | 已实现 | `EventModule` Script 订阅 + 4 档优先级稳定执行(注册期有序)+ 快照与跨实例重入安全分发 + 订阅者过滤器(精确投递)+ 取消传播 + 死引用清理 + SO 资产化 + 公开诊断 API |
| **Audio** | 已实现 | `AudioModule` 单例(2D 音频极简门面)+ SFX 独占音源轮询 + BGM 淡入淡出 + 三通道音量 / 静音持久化 |
| **Scene** | 已实现 | `SceneModule` 场景加载 / 叠加 / 卸载 / 激活场景切换 + 场景事件广播 + `SceneAssetWrapper` 可序列化场景引用 + 编辑器工具 |
| **ScriptDocGenerator** | 已实现(需 Odin) | 反射分析 C# 类型生成结构化 API 文档(增量保留手写内容)+ Summary 工具(XML `<summary>` 与 `[Summary]` 特性同步,特性优先) |

另有两项**可选能力**:

- **Binder 组件绑定**(需 Odin Inspector)—— `BinderAssistant` / `BinderTag` 将 UI 元素自动绑定到面板脚本,支持代码生成
- **Input System 适配**(独立程序集)—— 启用 Input System 包时自动替换 UIRoot 的输入模块

## 依赖

- **Aesir Architecture** `cn.runestone.aesir.architecture`(必需;两包同号发版,推荐同版本安装)
- **Odin Inspector**(可选):仅经 `#if ODIN_INSPECTOR` 条件编译参与,未导入时自动排除

## 继续阅读

- [快速开始](getting-started.md) — 安装、UIRoot 与第一个面板
- [特性一览](features.md) — 五模块速览
- [UI 框架](ui.md) — UIRoot / UIModule / 面板生命周期 / Binder
- [事件模块](events.md) — Script 订阅 / 过滤器 / 取消传播 / SO 资产化 / 诊断
- [音频模块](audio.md) — SFX 轮询 / BGM 淡入淡出 / 音量持久化
- [场景模块](scene.md) — SceneModule / SceneAssetWrapper / Addressables
- [兼容性](../start/compatibility.md) — 可选依赖(Odin / Addressables / Input System)