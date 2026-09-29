#if ODIN_INSPECTOR // Odin 展示特性仅装饰 Inspector（样式与逻辑分离）；未安装 Odin 时整体剔除，数据层零依赖
using Sirenix.OdinInspector;
#endif
using UnityEditor;
using UnityEngine;
using FilePathAttribute = UnityEditor.FilePathAttribute;

namespace Runestone.AesirModules.Editor
{
    /// <summary>
    /// Scene 模块编辑器设置（ScriptableSingleton）：Bootstrapper 场景搜集注册与启动流转的编辑器侧开关。
    /// </summary>
    /// <remarks>
    /// 本类是纯数据层，不依赖 Odin：展示与交互由窗口层承担——安装 Odin 时为
    /// <see cref="SceneModuleSettingsWindowOdin" />（InlineEditor 展示本单例），未安装时为原生 IMGUI 兜底
    /// <see cref="SceneModuleSettingsWindow" />，两窗口经同一菜单入口按 Odin 可用性路由（更新器双窗口同款模式）。
    /// Odin 展示特性（LabelText / LabelWidth / ShowInInspector 等）经 <c>#if ODIN_INSPECTOR</c> 包裹，
    /// 未安装 Odin 的环境整体编译剔除，不影响数据读写。
    /// </remarks>
    // 遵循项目约定：ScriptableSingleton 设置资产统一放 ScriptableSingleton/ 前缀目录（已被 .gitignore 覆盖）
    [FilePath("ScriptableSingleton/AesirModules/SceneEditorSettings.asset",
        FilePathAttribute.Location.ProjectFolder)]
    public class SceneEditorSettings : ScriptableSingleton<SceneEditorSettings>
    {
        // ScriptableSingleton 的 Save 走 Unity 原生序列化（InternalEditorUtility.SaveToSerializedFileAndForget），
        // 私有字段必须显式标记才会写入设置文件；漏标则文件里没有对应键，值只活在本进程内，
        // 新会话/重启等重建单例的路径会静默回落到默认值。
        [SerializeField]
        string _bootstrapperScenePath;

        [SerializeField]
        bool _firstLoadBootstrapScene;

        [SerializeField]
        string _previousScenePath;

        [SerializeField]
        bool _setupBootstrapper;

#if ODIN_INSPECTOR
        [LabelWidth(300)]
        [LabelText("是否自动搜集项目中的 Bootstrapper 场景并注册")]
        [ShowInInspector]
#endif
        public bool SetupBootstrapper
        {
            get => _setupBootstrapper;
            set
            {
                if (_setupBootstrapper == value)
                {
                    return;
                }

                _setupBootstrapper = value;
                Save(true);
            }
        }

#if ODIN_INSPECTOR
        [LabelWidth(300)]
        [LabelText("是否强制优先加载 Bootstrapper 场景")]
        [ShowInInspector]
#endif
        public bool FirstLoadBootstrapScene
        {
            get => _firstLoadBootstrapScene;
            set
            {
                if (_firstLoadBootstrapScene == value)
                {
                    return;
                }

                _firstLoadBootstrapScene = value;
                Save(true);
            }
        }

#if ODIN_INSPECTOR
        [PropertyOrder(10)]
        [ReadOnly]
        [ShowInInspector]
#endif
        public string BootstrapperScenePath
        {
            get => _bootstrapperScenePath;
            set
            {
                if (_bootstrapperScenePath == value)
                {
                    return;
                }

                _bootstrapperScenePath = value;
                Save(true);
            }
        }

#if ODIN_INSPECTOR
        [PropertyOrder(10)]
        [ReadOnly]
        [ShowInInspector]
#endif
        public string PreviousScenePath
        {
            get => _previousScenePath;
            set
            {
                if (_previousScenePath == value)
                {
                    return;
                }

                _previousScenePath = value;
                Save(true);
            }
        }

#if ODIN_INSPECTOR
        [Button("手动搜集 Bootstrapper 场景并注册", ButtonSizes.Medium)]
#endif
        public void ManualSetupBootstrapper()
        {
            BootstrapSceneHelper.SetupBootstrapScene();
        }
    }
}
