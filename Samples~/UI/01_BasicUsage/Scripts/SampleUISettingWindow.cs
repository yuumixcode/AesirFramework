#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
using UnityEngine;
using UnityEngine.UI;

namespace Runestone.AesirModules.Samples.UI.BasicUsage
{
    /// <summary>
    /// 设置窗口（Canvas 根窗口形态，预制体根节点自带 Canvas + Mask 蒙版 + Content 内容区）。
    /// 演示：蒙版点击关闭（closeOnMaskClick）、payload 数据传入、关闭按钮与蒙版点击双通道关闭。
    /// </summary>
    [AddComponentMenu("")]
    public class SampleUISettingWindow : AesirBaseWindow
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

        protected override void OnMaskClicked()
        {
            SampleUIHud.Log("蒙版被点击");
            base.OnMaskClicked();
        }

        protected override void OnHide()
        {
            base.OnHide();
            SampleUIHud.Log("设置窗口已关闭");
        }
    }
}
#endif
