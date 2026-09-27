using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirModules.Editor
{
    /// <summary>
    /// Scene 模块设置窗口（Odin 版）：InlineEditor 展示 <see cref="SceneEditorSettings" /> 单例。
    /// </summary>
    /// <remarks>
    /// 本窗口经 <see cref="SceneModuleSettingsWindow.OdinWindowOpener" /> 静态委托路由打开（与包内更新器同款模式）：
    /// Odin 程序集在域加载期经 [InitializeOnLoadMethod] 注册打开方式，未安装 Odin 时菜单落回原生 IMGUI 兜底窗口
    /// <see cref="SceneModuleSettingsWindow" />。菜单项由兜底窗口持有，本类不注册菜单。
    /// </remarks>
    public class SceneModuleSettingsWindowOdin : OdinEditorWindow
    {
        [InlineEditor(InlineEditorObjectFieldModes.CompletelyHidden)]
        [SerializeField]
        SceneEditorSettings settings;

        protected override void OnEnable()
        {
            base.OnEnable();
            settings = SceneEditorSettings.instance;
        }

        // Odin 版窗口的注册入口：域重载清空静态委托后由本程序集重新注册
        [InitializeOnLoadMethod]
        static void RegisterOpener()
        {
            SceneModuleSettingsWindow.RegisterOdinWindowOpener(() =>
            {
                var window = GetWindow<SceneModuleSettingsWindowOdin>();
                window.position = GUIHelper.GetEditorWindowRect().AlignCenterXY(500f, 600f);
                window.titleContent = new GUIContent("Scene Module Settings");
                window.Show();
            });
        }
    }
}
