using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirModules.Editor
{
    /// <summary>
    /// 事件测试窗口（Odin 版）：以内联 Inspector 编辑临时事件资产并手动触发。
    /// <para>
    /// 经 <see cref="AesirEventTesterWindow.OdinWindowOpener" /> 静态委托路由打开；
    /// 未安装 Odin 或关闭 Aesir Odin 样式时落回 UI Toolkit 版 <see cref="AesirEventTesterWindow" />。
    /// 触发走公开 API <see cref="AesirEventArgsSO.Raise" />，与 UI Toolkit 版等价。
    /// </para>
    /// </summary>
    public class AesirEventTesterWindowOdin : OdinEditorWindow
    {
        [InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)]
        [SerializeField]
        AesirEventArgsSO testAsset;

        [ShowInInspector]
        [ReadOnly]
        [PropertyOrder(0)]
        string Hint => Application.isPlaying ? "已就绪：配置事件参数后点击下方按钮触发。" : "需进入 Play Mode 才能触发事件（分发走非创建式单例）。";

        bool CanRaise => Application.isPlaying && testAsset != null;

        protected override void OnEnable()
        {
            base.OnEnable();
            titleContent = new GUIContent("Event Tester (Odin)");
            if (testAsset == null)
            {
                testAsset = CreateInstance<AesirEventArgsSO>();
                testAsset.name = "Aesir Event Tester";
            }
        }

        protected override void OnDisable()
        {
            if (testAsset != null)
            {
                DestroyImmediate(testAsset);
                testAsset = null;
            }

            base.OnDisable();
        }

        [Button("触发事件（Raise）", ButtonSizes.Large)]
        [PropertyOrder(1)]
        [EnableIf("CanRaise")]
        void Raise() => testAsset.Raise();

        [InitializeOnLoadMethod]
        static void RegisterOpener()
        {
            AesirEventTesterWindow.RegisterOdinWindowOpener(() =>
            {
                var window = GetWindow<AesirEventTesterWindowOdin>();
                window.position = GUIHelper.GetEditorWindowRect().AlignCenterXY(480f, 420f);
                window.Show();
            });
        }
    }
}
