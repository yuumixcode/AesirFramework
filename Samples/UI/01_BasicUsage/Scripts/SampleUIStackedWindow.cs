#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
using UnityEngine;
using UnityEngine.UI;

namespace Runestone.AesirModules.Samples.UI.BasicUsage
{
    /// <summary>
    /// 叠加窗口（Canvas 根窗口形态，sortingOrder 550 略高于设置窗口的 500）。
    /// 与设置窗口同时在场时直观对照单遮（仅叠加窗口持有蒙版）/叠遮（两块蒙版并存）的差异。
    /// </summary>
    [AddComponentMenu("")]
    public class SampleUIStackedWindow : AesirBaseWindow
    {
        [SerializeField] Text titleText;

        [SerializeField] Button closeButton;

        protected override void OnInit()
        {
            SampleUIFonts.Apply(transform);
            closeButton.onClick.AddListener(CloseSelf);
        }

        protected override void OnShow(object payload)
        {
            base.OnShow(payload);
            if (payload != null && titleText != null)
            {
                titleText.text = payload.ToString();
            }
        }

        protected override void OnHide()
        {
            base.OnHide();
            SampleUIHud.Log("叠加窗口已关闭");
        }
    }
}
#endif
