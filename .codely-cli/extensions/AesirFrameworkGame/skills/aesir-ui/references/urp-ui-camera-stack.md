# URP 下 UICamera 接线（游戏画面被清的修复）

> 适用：工程用 **URP** 且由 `UIRoot` 自动创建 UICamera（绝大多数 Aesir UI 工程）。Built-in 管线**不需要**本组件。

## 症状

进入游戏后 HUD / 菜单 / 暂停界面全部正常渲染，但玩家、敌人、子弹等**游戏精灵一个都看不见**；运行时数据证明战斗在正常进行（计分、扣血都在变）。"看得见的都不对，看不见的都在动"。打包 APK 后真机"UI 在但画面黑/消失"也是同一根因的第一嫌疑。

## 根因

`UIRoot` 创建的 UICamera 按**内置渲染管线**语义配置：`clearFlags = Depth`、`depth = 1`——在 Built-in 里这表示"在主相机之上叠加渲染 UI"。

URP 的相机合成规则不同：多个 **Base** 相机是**整屏替换**，不是叠加。UICamera 作为 Base 相机在主相机之后渲染时，把主相机画好的游戏画面整个清掉，只剩 UI 与它自己的清屏色。框架核心程序集**不引用 URP**（避免对 Built-in 用户产生硬依赖），因此没有内建的 URP 适配路径，需业务侧接线。

## 修复：反射版 UrpUiStackBinder（零硬依赖）

把下面组件挂到**主相机**物体上（每个含主相机的场景都挂，或随场景根节点生成）。它用反射识别 URP 数据类型，Built-in 下自动 no-op，**不引用 `UnityEngine.Rendering.Universal`**，因此框架包无需改动、Built-in 工程也能安全共存同一份脚本。

```csharp
// Assets/_Game/Scripts/UI/UrpUiStackBinder.cs
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using Runestone.AesirModules;

/// <summary>
/// URP 下把 UIRoot 的 UICamera 改为 Overlay 并加入主相机 cameraStack。
/// 反射实现，不引用 URP 程序集；Built-in 管线自动 no-op。
/// </summary>
[DisallowMultipleComponent]
public sealed class UrpUiStackBinder : MonoBehaviour
{
    [SerializeField, Tooltip("等待 UIRoot 实例就绪的最大帧数")]
    int waitFrames = 120;

    Camera _mainCamera;

    void Start() => StartCoroutine(BindWhenReady());

    IEnumerator BindWhenReady()
    {
        _mainCamera = GetComponent<Camera>();
        if (_mainCamera == null)
        {
            Debug.LogError("[UrpUiStackBinder] 必须挂在主相机物体上");
            yield break;
        }

        // 等 UIRoot 建好 UICamera（首次 Show<T> 才创建，可能晚于本组件 Start）
        Camera uiCamera = null;
        for (var i = 0; i < waitFrames; i++)
        {
            uiCamera = UIRoot.TryGetExisting()?.UICamera;
            if (uiCamera != null)
            {
                break;
            }

            yield return null;
        }

        if (uiCamera == null)
        {
            yield break; // 本场景没有 UI，无需接线
        }

        TryConfigureOverlay(_mainCamera, uiCamera);
    }

    static void TryConfigureOverlay(Camera mainCamera, Camera uiCamera)
    {
        // 反射类型：UnityEngine.Rendering.Universal.UniversalAdditionalCameraData
        // 与 CameraRenderType.Overlay（枚举底层 int = 1）
        var dataType = Type.GetType(
            "UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime");
        if (dataType == null)
        {
            return; // Built-in 管线：无此类型，no-op
        }

        var getMainData = mainCamera.GetType().GetMethod("GetUniversalAdditionalCameraData");
        var getUiData = uiCamera.GetType().GetMethod("GetUniversalAdditionalCameraData");
        if (getMainData == null || getUiData == null)
        {
            return;
        }

        object mainData = getMainData.Invoke(mainCamera, null);
        object uiData = getUiData.Invoke(uiCamera, null);
        if (mainData == null || uiData == null)
        {
            return;
        }

        // uiData.renderType = CameraRenderType.Overlay (1)
        var renderTypeProp = dataType.GetProperty("renderType", BindingFlags.Public | BindingFlags.Instance);
        var overlayValue = Enum.ToObject(
            Type.GetType("UnityEngine.Rendering.Universal.CameraRenderType, Unity.RenderPipelines.Universal.Runtime"),
            1);
        renderTypeProp?.SetValue(uiData, overlayValue);

        // mainData.cameraStack.Add(uiCamera)
        var stackProp = dataType.GetProperty("cameraStack", BindingFlags.Public | BindingFlags.Instance);
        if (stackProp?.GetValue(mainData) is System.Collections.IList stack && !stack.Contains(uiCamera))
        {
            stack.Add(uiCamera);
        }
    }
}
```

要点：

- **挂主相机、不要挂 UICamera**：cameraStack 属于 Base 相机，只有主相机能把自己的 stack 追加 Overlay 相机。
- **等 UIRoot 就绪**：UICamera 在首次 `UIModule.Show<T>()` 时才创建，可能晚于主相机的 `Start`——所以协程轮询等待，不要假设它已存在。
- **零硬依赖**：类型名 + 程序集名字符串 `Type.GetType`，Built-in 下取不到类型直接 no-op；同一份脚本能同时用于 Built-in 与 URP 工程。若嫌反射 name 不匹配，可在 URP 工程把 `GetUniversalAdditionalCameraData()` 改为直接调用（此时脚本依赖 URP 程序集，仅用于 URP 工程）。
- **UIRoot 层也要排除 UI(5)**：UICamera 只渲染 UI(5) + TransparentFX(1)；主相机应排除 UI 层，避免重复渲染（见 SKILL 坑位 5）。

## 验证

1. 进 Play Mode，确认所有游戏精灵与 UI 同时可见（正向可见事实用截图确认）。
2. 若疑似"没渲染"，**以运行时数据为准**：VLM/单帧截图对"没看到某事"的否定结论不可靠，慢放录屏尤其容易误判——用确定性断言（如活动子弹峰值、相机 stack 计数）交叉验证。
3. 真机 APK 若"UI 在但画面黑"，第一嫌疑就是本接线：确认两个场景的相机都挂了组件、`UIRoot` 的 UICamera 已进入主相机 cameraStack。

> 本方案源自 PlaneWar 项目实战（`场景UI与战斗阶段问题总结.md` 问题 1），当时为业务侧绕过、框架包零改动。此处通用化为反射版本，供后续工程直接复用。
