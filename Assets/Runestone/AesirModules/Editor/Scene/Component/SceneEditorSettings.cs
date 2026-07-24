using Sirenix.OdinInspector;
using UnityEditor;
using FilePathAttribute = UnityEditor.FilePathAttribute;

namespace Runestone.AesirModules.Editor
{
    [FilePath("ProjectEditorSettings/SceneEditorSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class SceneEditorSettings : ScriptableSingleton<SceneEditorSettings>
    {
        string _bootstrapperScenePath;
        bool _firstLoadBootstrapScene;
        string _previousScenePath;
        bool _setupBootstrapper;

        [LabelWidth(300)]
        [LabelText("是否自动搜集项目中的 Bootstrapper 场景并注册")]
        [ShowInInspector]
        public bool SetupBootstrapper
        {
            get => _setupBootstrapper;
            set
            {
                _setupBootstrapper = value;
                Save(true);
            }
        }

        [LabelWidth(300)]
        [LabelText("是否强制优先加载 Bootstrapper 场景")]
        [ShowInInspector]
        public bool FirstLoadBootstrapScene
        {
            get => _firstLoadBootstrapScene;
            set
            {
                _firstLoadBootstrapScene = value;
                Save(true);
            }
        }

        [PropertyOrder(10)]
        [ReadOnly]
        [ShowInInspector]
        public string BootstrapperScenePath
        {
            get => _bootstrapperScenePath;
            set
            {
                _bootstrapperScenePath = value;
                Save(true);
            }
        }

        [PropertyOrder(10)]
        [ReadOnly]
        [ShowInInspector]
        public string PreviousScenePath
        {
            get => _previousScenePath;
            set
            {
                _previousScenePath = value;
                Save(true);
            }
        }

        [Button("手动搜集 Bootstrapper 场景并注册", ButtonSizes.Medium)]
        public void ManualSetupBootstrapper()
        {
            BootstrapSceneHelper.SetupBootstrapScene();
        }
    }
}
