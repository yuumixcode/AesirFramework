---
name: aesir-boot-scene
description: 为 Aesir 框架 Unity 工程创建启动场景（主菜单场景）与启动脚本。项目初始化阶段使用——生成 GameBootstrap 启动脚本与场景构建编辑器脚本，产出只含「启动脚本物体 + AudioListener」的启动场景（不放 Main Camera，杜绝编辑器警告），并登记 Build Settings index 0。触发场景：用户说"创建启动场景"、"初始化工程"、"搭工程骨架"、"建主菜单场景"、"jam 开工"、"Boot Scene"。
---

# 创建 Aesir 启动场景（主菜单场景）

## 目标产物

一次执行后工程获得：

| 产物 | 位置 | 说明 |
|------|------|------|
| `Boot.unity` 启动场景 | `Assets/Scenes/` | 场景内**恰好一个** `GameBootstrap` 物体（挂启动脚本 + AudioListener），**无 Main Camera** |
| `GameBootstrap.cs` 启动脚本 | `Assets/Game/Core/` | 场景唯一引导：初始化 Context，显示首个 UI |
| `BootSceneBuilder.cs` 编辑器工具 | `Assets/Editor/` | 菜单一键重建启动场景（幂等），Build Settings 登记 index 0 |

**设计约束（硬规则，不得变通）**：

1. 启动场景**只放**启动脚本物体 + AudioListener——不放 Main Camera（UI 由 UIRoot 自动创建的 UICamera 渲染）。
2. AudioListener 全场景**恰好一个**：多于一个触发编辑器警告 `There are 2 audio listeners in the scene`；一个都没有则音频无声。它挂在 `GameBootstrap` 物体上。
3. 场景资产**必须经编辑器脚本创建**（本 skill 的 `BootSceneBuilder`），**禁止手写 `.unity` YAML**——GUID/fileID 错误要打开场景才炸，排查成本极高。
4. 构建脚本幂等：重复执行覆盖同名场景、Build Settings 不产生重复条目。

## 前置检查

执行前确认（不满足先补齐）：

- [ ] 工程已安装 Aesir 两个包（`cn.runestone.aesir.architecture` + `cn.runestone.aesir.modules`，缺 Modules 时经菜单 `Tools/Aesir/Modules/Install Dependencies` 补装）。
- [ ] 业务代码放 `Assembly-CSharp`（**不新建任何 asmdef**——少一层编译上下文，AI 出错面减半）。
- [ ] 工程已有 `GameContext`（RAA Context）；没有则先用下面的最小模板创建一个，再回来继续。

<details>
<summary>最小 Context 模板（工程还没有 GameContext 时用）</summary>

```csharp
// Assets/Game/Context/GameContext.cs
using Runestone.AesirArchitecture;

public sealed class GameContext : AbstractContext<GameContext>
{
    protected override void Configure()
    {
        // 注册 Model——默认按最低档（Quick）：直接注册具体类，不建接口
        // RegisterModel(new PlayerModel());
    }
}
```

</details>

## 步骤 1：生成启动脚本 GameBootstrap.cs

```csharp
// Assets/Game/Core/GameBootstrap.cs
using UnityEngine;
using Runestone.AesirArchitecture;

namespace Game.Core
{
    /// <summary>
    /// 启动场景唯一引导脚本：初始化 Context，并显示首个 UI。
    /// 场景内除本脚本（与 AudioListener）外不放任何东西。
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        void Start()
        {
            // 触发 Context 初始化（执行 Configure，注册全部 Model / Service）
            _ = GameContext.Instance;

            // 首个 UI——主菜单面板写好后启用（路径常量集中在 UIPaths；首次 Show 会自动创建 UIRoot）
            // UIModule.Show<MenuPanel>(path: UIPaths.MenuPanel);
        }
    }
}
```

要点：

- `_ = GameContext.Instance;` 用丢弃赋值触发懒加载初始化（属性访问不能单独作语句）。
- UI 显示行先注释保留：面板类与预制体尚未生成时保持注释可保证开箱编译通过；接入 UI 后取消注释。
- 如需显示面板：`UIModule.Show<T>(path:...)` 首次调用会**自动创建** UIRoot（四层 Canvas + UICamera + EventSystem），启动场景不需要预放置任何 UI 基础设施。

## 步骤 2：生成场景构建脚本 BootSceneBuilder.cs

```csharp
// Assets/Editor/BootSceneBuilder.cs
#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Game.EditorTools
{
    /// <summary>
    /// 创建 Aesir 启动场景：空场景 + 唯一物体 GameBootstrap（挂启动脚本与 AudioListener），
    /// 登记 Build Settings index 0。幂等，可重复执行。
    /// </summary>
    public static class BootSceneBuilder
    {
        const string ScenePath = "Assets/Scenes/Boot.unity";

        [MenuItem("Tools/Game/Create Boot Scene")]
        public static void CreateBootScene()
        {
            // 尽力保存当前已打开场景（未命名场景不弹窗、自动跳过）
            EditorSceneManager.SaveOpenScenes();

            // 空场景：无 Main Camera、无 AudioListener、无灯光
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 唯一物体：启动脚本 + AudioListener（场景无相机，音频监听必须手动补，且恰好一个）
            var bootstrap = new GameObject("GameBootstrap");
            bootstrap.AddComponent<GameBootstrap>();
            bootstrap.AddComponent<AudioListener>();

            // 保存场景（SaveScene 不自动建父目录，须先建目录并刷新）
            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
            AssetDatabase.Refresh();
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
            {
                Debug.LogError($"[BootSceneBuilder] 启动场景保存失败：{ScenePath}");
                return;
            }

            // Build Settings 登记为 index 0（幂等：先移除同路径旧条目再前插）
            var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            list.RemoveAll(s => s.path == ScenePath);
            list.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = list.ToArray();

            Debug.Log($"[BootSceneBuilder] 启动场景已创建并置为 Build Settings index 0：{ScenePath}");
        }
    }
}
#endif
```

可按工程习惯调整的项（改前确认不与既有菜单冲突）：

- 菜单路径 `Tools/Game/Create Boot Scene`—— jam 工程可改 `Tools/GameJam/...`；不要挂进 `Tools/Aesir/`（那是框架包菜单空间）。
- 场景名与路径 `Assets/Scenes/Boot.unity`——如工程约定用 `MainMenu.unity` 或别的目录，同步改常量即可。

## 步骤 3：执行构建

按 AI 环境二选一：

- **有编辑器桥**（如 Codely Unity Bridge / 其它可执行编辑器脚本的环境）：直接调用 `Game.EditorTools.BootSceneBuilder.CreateBootScene()`（反射调用亦可）。
- **无编辑器桥**：让用户点击菜单 `Tools/Game/Create Boot Scene`，然后回来继续验证。

## 步骤 4：验证清单（执行后必须逐项确认）

| # | 检查项 | 判定 |
|---|--------|------|
| 1 | 打开 `Boot.unity`，场景根物体**恰好一个** `GameBootstrap` | 物体数 = 1 |
| 2 | `GameBootstrap` 物体组件 = `GameBootstrap` 脚本 + `AudioListener`，**无 Camera 组件** | 组件数 = 2 |
| 3 | `Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None)` 计数 | = 1 |
| 4 | Build Settings 首条 | `Assets/Scenes/Boot.unity`（enabled） |
| 5 | Console | 无新增警告/错误（重点：无 "2 audio listeners"、无 Missing Script） |
| 6 | 进 Play Mode 一次后退出 | Console 仍零警告；`GameContext` 初始化无异常 |

验证失败时的常见原因：

- 物体多于一个/组件多于两个 → 生成时手加了相机或灯光：对照步骤 2 模板修正，重跑构建。
- Missing Script → `GameBootstrap.cs` 与 `BootSceneBuilder.cs` 不在同一工程的编译域（如给业务代码建了 asmdef 导致 Editor 程序集引用不到）：删掉 asmdef，回到 `Assembly-CSharp` 约定。

## 为什么这样设计（AI 不要自作聪明改掉）

| 设计 | 理由 |
|------|------|
| 不放 Main Camera | 主菜单是纯 UI 场景，UI 由 UIRoot 的 UICamera 渲染；多一台主相机只会重复渲染、多一份 AudioListener 警告源 |
| AudioListener 挂在 GameBootstrap 物体上 | 场景无相机 → 引擎不再自动带 listener，必须手动补；挂启动物体上保证"场景自含音频能力"，不依赖其它物体存活 |
| 场景经编辑器脚本生成而非手写 YAML | AI 手写场景 YAML 的 GUID/fileID 错误无法在编译期暴露，现场是打开场景一片 None；编辑器脚本语法/类型错全部编译期可见 |
| Build Settings index 0 | 构建从启动场景进；`SceneModule.ReloadScene` 等场景 API 依赖 Build Settings 登记 |

## 与多场景切换的衔接

- 启动场景 → 玩法场景：`SceneModule.LoadSceneSingle("Assets/Scenes/Main.unity", ...)`（Single 卸载启动场景，AudioListener 随之移除，玩法场景相机自带 listener，全程恰好一个）。
- **若玩法场景以 Additive 叠加**在启动场景之上：两个场景会各有一个 AudioListener（编辑器警告）。处理任选其一——玩法场景相机不挂 AudioListener（推荐，启动场景常驻）；或叠加前禁用 Bootstrap 上的 listener。
