#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
using UnityEngine;
using UnityEngine.UI;

namespace Runestone.AesirModules.Samples.UI.BasicUsage
{
    /// <summary>
    /// 控制面板（Normal 层 Panel 形态）。按钮驱动窗口的打开/关闭与蒙版模式切换，
    /// 演示 Panel 与 Window（Canvas 根 UI）两种形态的协作：面板常驻 Normal 层，窗口恒在面板之上。
    /// </summary>
    [AddComponentMenu("")]
    public class SampleUIControlPanel : AesirBasePanel
    {
        [SerializeField] Button settingButton;

        [SerializeField] Button stackedButton;

        [SerializeField] Button loadingButton;

        [SerializeField] Button maskModeButton;

        [SerializeField] Button closeAllButton;

        protected override void OnInit()
        {
            // 预制体静态 Text 的默认字体对中文字形不可靠，运行时统一挂系统中文动态字体
            SampleUIFonts.Apply(transform);

            // 打开设置窗口：带蒙版 + 点击蒙版关闭，payload 传入标题文本
            settingButton.onClick.AddListener(() =>
            {
                UIModule.Open<SampleUISettingWindow>("设置窗口（payload 演示）");
                SampleUIHud.Log("打开设置窗口：单块蒙版垫底，点击蒙版即可关闭");
            });

            // 打开叠加窗口：与设置窗口同时在场时直观对照单遮/叠遮差异
            stackedButton.onClick.AddListener(() =>
            {
                UIModule.Open<SampleUIStackedWindow>("叠加窗口（蒙版叠加演示）");
                SampleUIHud.Log("打开叠加窗口：两块蒙版并存，切换单遮/叠遮对照效果");
            });

            // 打开全屏加载窗口：无蒙版窗口形态 + payload 数据 + 自动关闭
            loadingButton.onClick.AddListener(() =>
            {
                UIModule.Open<SampleUILoadingWindow>("加载中…（2 秒后自动关闭）");
                SampleUIHud.Log("打开加载窗口：全屏无蒙版，payload 传入加载文案");
            });

            maskModeButton.onClick.AddListener(SwitchMaskMode);
            closeAllButton.onClick.AddListener(() =>
            {
                UIModule.Close<SampleUIStackedWindow>();
                UIModule.Close<SampleUISettingWindow>();
                UIModule.Close<SampleUILoadingWindow>();
                SampleUIHud.Log("全部窗口已关闭");
            });
        }

        void SwitchMaskMode()
        {
            var module = UIModule.Instance;
            module.MaskMode = module.MaskMode == UIMaskMode.Single ? UIMaskMode.Stacked : UIMaskMode.Single;
            SampleUIHud.Log($"蒙版模式切换为：{(module.MaskMode == UIMaskMode.Single ? "单遮（仅最高层窗口持有蒙版）" : "叠遮（各窗口蒙版独立生效）")}");
        }
    }
}
#endif
