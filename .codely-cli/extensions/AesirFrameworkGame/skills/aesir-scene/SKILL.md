---
name: aesir-scene
description: Aesir Modules 场景模块（SceneModule）编码指南。场景切换时使用——LoadSceneSingle / LoadSceneAdditive / ReloadScene、进度归一化、叠加追踪、SceneAssetWrapper 自愈引用、UniTask awaitable。触发词：加载场景、切换场景、进度条、叠加场景、从主菜单进游戏、打完一关回大厅。
---

# Aesir 场景模块（SceneModule）

场景加载、叠加追踪与卸载回收，**语义对齐 Unity 原生 `LoadSceneMode`**：Single 卸载全部场景并重设激活场景；Additive 纯叠加、不改激活场景，叠加场景统一记账。公开 API 全为静态成员，`SceneModule.xxx` 直接调用。

> 前置：本包依赖 Aesir Architecture（RAA）。跨场景传数据用 `MiniEvent` 或共享 Model，本模块不提供场景间传参。
>
> **缺包先过依赖门禁**：工程里没有 `cn.runestone.aesir.modules`（或连带缺 RAA）时，不要改用 `SceneManager.LoadScene` 直切顶替——先按 `aesir-router` 的「依赖门禁」探测 → 读记忆 → 询问安装（Git URL，CNB 优先、GitHub 兜底）。

## 默认路径

```csharp
using Runestone.AesirModules;

// 切场景（Single：卸掉旧的、重设激活场景）
SceneModule.LoadSceneSingle("Assets/_Game/Scenes/Gameplay.unity",
    onCompleted: () => Debug.Log("loaded"),
    onFailed:    () => Debug.LogError("failed"),
    onProgress:  p => progressBar.value = p);   // 逐帧 0–1，已按 progressCap 归一化

// 叠加（Additive：不改激活场景，进追踪列表）
SceneModule.LoadSceneAdditive("Assets/_Game/Scenes/HUD.unity");
SceneModule.UnloadScene("Assets/_Game/Scenes/HUD.unity");
SceneModule.UnloadAllAddedScenes();

// 激活场景（决定光照设置来源与 Instantiate 默认落点）
SceneModule.SetActiveScene("Assets/_Game/Scenes/Gameplay.unity");

// 重载当前激活场景（异步 Single 语义）
SceneModule.ReloadScene();
```

路径 / `SceneAssetWrapper` 双重重载都支持；回调全部可选。

## 硬前置与硬约束（踩了不会静默）

1. **目标场景必须登记进 Build Settings**（Unity 硬约束）——启动场景用 `aesir-boot-scene` Skill 的 BootSceneBuilder 登记 index 0；玩法场景在 Build Settings 面板拖入，或用 Odin 环境下 `SceneAssetWrapper` 的"添加到 BuildSettings"按钮。
2. **传完整资产路径**（含 `.unity`）。传裸名虽然 `LoadSceneAsync` 能加载，但模块记账/激活/事件全走样（伪错误日志 + `LastLoadedScene` 失效）——**永远传 `Assets/.../Xxx.unity`**。
3. **禁止混用原生 `SceneManager.LoadScene` / `UnloadSceneAsync`**：绕过记账，`AddedScenePaths` / `LastLoadedScene` 失真。原生卸载会被对账订阅自动移出记账（只覆盖卸载方向），外部 Single 加载则连 `LastLoadedScene` 都不会更新。

## 进度与并发

- `onProgress` 按 `SceneModuleConfigSO.progressCap`（默认 0.9）归一化：Unity 的 `AsyncOperation.progress` 在激活前停在 0.9、激活瞬间跳 1，归一化后进度条能平滑走到 100%。
- **Single 并发互斥**：进行中收到同路径请求幂等忽略（告警留痕，回调交给进行中的那次）；异路径请求拒绝并触发 `onFailed`。
- **不要对同一路径重复 `LoadSceneAdditive`**：Unity 会加载两个实例但追踪只记一条，卸载只移除其一。

## 生命周期事件与查询

```csharp
SceneModule.SceneLoadedEvent.AddListener(path => Debug.Log($"已加载 {path}"));    // MiniEvent<string>
SceneModule.SceneUnloadedEvent.AddListener(path => Debug.Log($"已卸载 {path}"));

SceneModule.AddedScenePaths;          // 叠加追踪（只读）
SceneModule.LastLoadedScene;          // 最后一次加载的场景
SceneModule.BootstrapSceneAssetWrapper; // 启动场景：实例字段优先，未赋值回退配置资产
```

句柄绑生命周期可用 `RemoveListenerOnDestroyTrigger` / `OnDisable` / `OnSceneUnloaded` 系列触发器。

## SceneAssetWrapper（可序列化场景引用）

```csharp
[SerializeField] SceneAssetWrapper nextScene;      // Inspector 拖拽赋值（Odin / 原生 Drawer 双环境等价）

var w = SceneAssetWrapper.FromScenePath("Assets/_Game/Scenes/Gameplay.unity");
SceneModule.LoadSceneSingle(w);

if (w.TryGetScenePath(out var path)) { }            // 安全读取
if (w.State == SceneAssetWrapperState.Regular) { }  // 加载前校验
var name = w.SceneName;                             // fail-fast 读取（专用异常带修复指引）
```

- 状态：`Regular`（BuildSettings 途径）/ `Addressable` / `Unsafe`（`Empty` 或 `NotInBuild`）；Inspector 有状态着色与一键修复按钮。
- **GUID 锚点自愈**：场景被移动/重命名后引用断链但 GUID 能回到路径、资产仍可定位 → **不算悬空**，加载完全可用（建议重新拖拽一次恢复对象引用）；只有资产真正消失才红色提示。
- Addressable 场景**不经本模块加载**：wrapper 只提供 `Address` / `TryGetAddress`，加载卸载直接用 Addressables API。

## 装了 UniTask 时的 awaitable API

工程包含 UniTask 时模块自动切换内部驱动（无需配置），并额外提供 `SceneModuleUniTask`（独立程序集 `Runestone.AesirModules.UniTask`，无 UniTask 时整体不编译）：

```csharp
await SceneModuleUniTask.LoadSceneSingleAsync(path, onProgress: p => progressBar.value = p);
await SceneModuleUniTask.LoadSceneAdditiveAsync(wrapper);
await SceneModuleUniTask.UnloadSceneAsync(path);
await SceneModuleUniTask.ReloadSceneAsync();
await SceneModuleUniTask.UnloadAllAddedScenesAsync();
```

失败时 await 侧抛 `InvalidOperationException`（真因已由 SceneModule 输出 Console）；`CancellationToken` 取消**只中止等待**，底层流程继续完成（Unity 场景操作不支持中途取消）；宿主销毁时内部流程静默中止，等待方以取消收场，不会悬挂。

## 配置与 DDOL

- `SceneModuleConfigSO` 需显式创建：在 Project 窗口建 `Assets/Resources/SceneModuleConfig/SceneModuleConfig.asset`，或点菜单 `Tools/Aesir/Modules/Create Module Config Assets` 一键生成（包导入时不会自动写工程）。可编辑 `bootstrapScene`（全局启动场景兜底）、`progressCap`（默认 0.9）。解析顺序：注册的加载器 → Resources 兜底 → 内存默认（缺资产也能跑）。
- 预放置实例为根物体时受 `dontDestroyOnLoad`（默认开）控制；**关闭 DDOL 会在 Single 加载时连实例一起销毁并中断回调**。
- **预放置实例必须处于激活状态**：失活物体起不了加载协程，模块会跳过它并新建实例 + 告警。
- 模块实例被重建时叠加追踪归零（`AddedScenePaths` / `LastLoadedScene` 随旧实例消亡），静态事件对象不受影响。

## 设计边界

不做自动流转（`bootstrapScene` 只是给用户代码读的引用，BuildSettings 序号 0 与强制打开 Boot 场景由编辑器 `BootstrapSceneHelper` 负责，默认关闭）、不做场景间传参、不做 Addressables 集成、不内置除 UniTask 适配外的 async 抽象。

## 详情与示例

- 完整 API、测试与维护说明：包内 `Documentation~/scene-module.md`。
- 编辑器工具：菜单 `Tools → Aesir → Modules → Scene Module Settings`（Odin / IMGUI 双版本），开关 `BootstrapSceneHelper` 的启动场景搜集。
