# 启动脚本与场景构建脚本（完整代码）

> `../SKILL.md` 的展开参考：步骤 1、2 落地时读本文，两段代码可直接抄。设计约束与验证清单在 SKILL.md，抄完必须回去逐项验证。

## 步骤 1：生成启动脚本 Bootstrap.cs

```csharp
// Assets/_Game/Scripts/Bootstrap.cs
using UnityEngine;
using Runestone.AesirArchitecture;
using Runestone.AesirModules;   // UIModule.Show / 模块配置加载器注册时需要

/// <summary>
/// 启动场景唯一引导脚本：初始化 Context，并显示首个 UI。
/// 场景内除本脚本（与 AudioListener）外不放任何东西。
/// </summary>
public sealed class Bootstrap : MonoBehaviour
{
    void Start()
    {
        // 触发 Context 初始化（执行 Configure，注册全部 Model / Service）
        _ = GameContext.Instance;

        // 【推荐】显式决定模块配置资产怎么来——框架不会在导入包时往你的工程写文件：
        // A. 菜单 Tools/Aesir/Modules/Create Module Config Assets 一键在 Assets/Resources 下生成两份配置 SO，
        //    之后在 Project 窗口编辑：UIModuleConfig 管窗口蒙版的单遮 / 叠遮，SceneModuleConfig 管启动场景与加载进度上限；
        // B. 自定义路径或 Addressables：在首次消费配置前注册加载器（须早于下方 UIModule.Show）：
        //    UIModuleConfigSO.RegisterConfigLoader(() => Resources.Load<UIModuleConfigSO>("UIModuleConfig/UIModuleConfig"));
        //    SceneModuleConfigSO.RegisterConfigLoader(() => Resources.Load<SceneModuleConfigSO>("SceneModuleConfig/SceneModuleConfig"));
        // C. 什么都不做也能跑：Resources 未命中时各建一份内存默认实例（不落盘），蒙版默认单遮、进度上限默认 0.9。

        // 首个 UI——主菜单面板写好后启用（path 填预制体在 Resources 下的相对路径；首次 Show 会自动创建 UIRoot）
        // UIModule.Show<MenuPanel>(path: "UI/MenuPanel");
    }
}
```

要点：

- 启动脚本放 `Scripts/` 根目录、**全局命名空间**（角色是框架级全局概念，不套业务域命名空间），因此步骤 2 的编辑器脚本无需 `using` 即可引用。
- `_ = GameContext.Instance;` 用丢弃赋值触发懒加载初始化（属性访问不能单独作语句）。
- 配置块保持注释不影响编译；A / B / C 三选一，不要同时注册加载器又依赖菜单资产。框架导入时不写 Resources 资产，缺配置时运行时用内存默认值兜底。
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

namespace EditorTools
{
    /// <summary>
    /// 创建 Aesir 启动场景：空场景 + 唯一物体 Bootstrap（挂启动脚本与 AudioListener），
    /// 登记 Build Settings index 0。幂等，可重复执行。
    /// </summary>
    public static class BootSceneBuilder
    {
        const string ScenePath = "Assets/_Game/Scenes/Bootstrap.unity";

        [MenuItem("Tools/Game/Create Boot Scene")]
        public static void CreateBootScene()
        {
            // 尽力保存当前已打开场景（未命名场景不弹窗、自动跳过）
            EditorSceneManager.SaveOpenScenes();

            // 空场景：无 Main Camera、无 AudioListener、无灯光
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 唯一物体：启动脚本 + AudioListener（场景无相机，音频监听必须手动补，且恰好一个）
            var bootstrap = new GameObject("Bootstrap");
            bootstrap.AddComponent<Bootstrap>();
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
            // EditorBuildSettings.scenes 只改编辑器内存，不调用 SaveAssets 不写磁盘：
            // 协作/构建机上 Boot 未登记会导致构建入口错误，是典型协作事故源
            AssetDatabase.SaveAssets();

            Debug.Log($"[BootSceneBuilder] 启动场景已创建并置为 Build Settings index 0：{ScenePath}");
        }
    }
}
#endif
```

可按工程习惯调整的项（改前确认不与既有菜单冲突）：

- 菜单路径 `Tools/Game/Create Boot Scene`—— jam 工程可改 `Tools/GameJam/...`；不要挂进 `Tools/Aesir/`（那是框架包菜单空间）。
- 场景名与路径 `Assets/_Game/Scenes/Bootstrap.unity`——直接放 `Scenes/` 下、文件名不带类型前缀（`Bootstrap` 而非 `scn_Bootstrap`）；如工程约定用 `MainMenu.unity` 或别的名字，同步改常量即可。

