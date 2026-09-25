#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
using UnityEngine;

namespace Runestone.AesirModules.Samples.UI.BasicUsage
{
    /// <summary>
    /// UI 模块基础用法示例引导脚本。场景内仅放置本脚本与主相机，全部 UI 经预制体注册后运行时创建
    /// （UIRoot / UIModule 未预放置，首次调用静态 API 时自动创建，演示运行时自动创建路径）。
    /// </summary>
    [AddComponentMenu("")]
    public class SampleUIBoot : MonoBehaviour
    {
        [Header("面板预制体")]
        [SerializeField] GameObject controlPanelPrefab;

        [SerializeField] GameObject hudPrefab;

        [Header("窗口预制体（Canvas 根）")]
        [SerializeField] GameObject settingWindowPrefab;

        [SerializeField] GameObject stackedWindowPrefab;

        [SerializeField] GameObject loadingWindowPrefab;

        void Start()
        {
            // 面板与窗口的预制体注册（实际项目可经 RegisterPrefab/RegisterWindowPrefab 或 Resources path 完成）
            UIModule.RegisterPrefab<SampleUIControlPanel>(controlPanelPrefab);
            UIModule.RegisterPrefab<SampleUIHud>(hudPrefab);
            UIModule.RegisterWindowPrefab<SampleUISettingWindow>(settingWindowPrefab);
            UIModule.RegisterWindowPrefab<SampleUIStackedWindow>(stackedWindowPrefab);
            UIModule.RegisterWindowPrefab<SampleUILoadingWindow>(loadingWindowPrefab);

            UIModule.Show<SampleUIControlPanel>();
            UIModule.Show<SampleUIHud>();
            SampleUIHud.Log("场景引导完成：控制面板与 HUD 已显示");
        }
    }
}
#endif
