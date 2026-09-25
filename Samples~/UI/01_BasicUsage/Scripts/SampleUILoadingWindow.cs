#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Runestone.AesirModules.Samples.UI.BasicUsage
{
    /// <summary>
    /// 加载窗口（Canvas 根窗口形态，预制体无 Mask 子物体——全屏不透明窗口天然遮挡，不需要蒙版）。
    /// 演示 payload 数据传入 + 协程自动关闭 + DestroyOnHide 销毁回收（每次打开重新实例化）。
    /// </summary>
    [AddComponentMenu("")]
    public class SampleUILoadingWindow : AesirBaseWindow
    {
        [SerializeField] Text loadingText;

        Coroutine _autoCloseRoutine;

        protected override void OnInit()
        {
            SampleUIFonts.Apply(transform);
        }

        protected override void OnShow(object payload)
        {
            base.OnShow(payload);
            if (loadingText != null)
            {
                loadingText.text = payload as string ?? "加载中…";
            }

            if (_autoCloseRoutine != null)
            {
                StopCoroutine(_autoCloseRoutine);
            }

            _autoCloseRoutine = StartCoroutine(AutoCloseRoutine(2f));
        }

        protected override void OnHide()
        {
            base.OnHide();
            if (_autoCloseRoutine != null)
            {
                StopCoroutine(_autoCloseRoutine);
                _autoCloseRoutine = null;
            }
        }

        IEnumerator AutoCloseRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            SampleUIHud.Log("加载完成，窗口自动关闭");
            CloseSelf();
        }
    }
}
#endif
