using Runestone.AesirInspector.OdinIntegration;
using Sirenix.OdinInspector;
using UnityEditor;
using FilePathAttribute = UnityEditor.FilePathAttribute;

namespace Runestone.AesirInspector.Samples.LoadType.Editor
{
    [FilePath(ProjectFilePath + "/RuntimeInitializeLoadType/LoadTypeSettings.asset",
        FilePathAttribute.Location.ProjectFolder)]
    public class RuntimeInitializeLoadTypeSettings : ScriptableSingleton<RuntimeInitializeLoadTypeSettings>
    {
        const string ProjectFilePath = "Aesir Inspector/Samples";
        bool _executeOnAfterAssembliesLoaded;
        bool _executeOnAfterSceneLoad;
        bool _executeOnBeforeSceneLoad;
        bool _executeOnBeforeSplashScreen;

        bool _executeOnSubsystemRegistration;

        [BilingualTitle("是否输出对应时机的日志", "Enable Log Output For Each Timing")]
        [ShowInInspector]
        [LabelWidth(400)]
        [BilingualText("Sample 执行在 SubsystemRegistration 时机的方法",
            "Sample method executed at SubsystemRegistration timing")]
        public bool ExecuteOnSubsystemRegistration
        {
            get => _executeOnSubsystemRegistration;
            set
            {
                _executeOnSubsystemRegistration = value;
                Save(true);
            }
        }

        [LabelWidth(400)]
        [ShowInInspector]
        [BilingualText("Sample 执行在 AfterAssembliesLoaded 时机的方法",
            "Sample method executed at AfterAssembliesLoaded timing")]
        public bool ExecuteOnAfterAssembliesLoaded
        {
            get => _executeOnAfterAssembliesLoaded;
            set
            {
                _executeOnAfterAssembliesLoaded = value;
                Save(true);
            }
        }

        [LabelWidth(400)]
        [ShowInInspector]
        [BilingualText("Sample 执行在 BeforeSplashScreen 时机的方法",
            "Sample method executed at BeforeSplashScreen timing")]
        public bool ExecuteOnBeforeSplashScreen
        {
            get => _executeOnBeforeSplashScreen;
            set
            {
                _executeOnBeforeSplashScreen = value;
                Save(true);
            }
        }

        [LabelWidth(400)]
        [ShowInInspector]
        [BilingualText("Sample 执行在 BeforeSceneLoad 时机的方法",
            "Sample method executed at BeforeSceneLoad timing")]
        public bool ExecuteOnBeforeSceneLoad
        {
            get => _executeOnBeforeSceneLoad;
            set
            {
                _executeOnBeforeSceneLoad = value;
                Save(true);
            }
        }

        [LabelWidth(400)]
        [ShowInInspector]
        [BilingualText("Sample 执行在 AfterSceneLoad 时机的方法", "Sample method executed at AfterSceneLoad timing")]
        public bool ExecuteOnAfterSceneLoad
        {
            get => _executeOnAfterSceneLoad;
            set
            {
                _executeOnAfterSceneLoad = value;
                Save(true);
            }
        }

        [PropertyOrder(10)]
        [BilingualTitle("Configurable Enter Play Mode 设置", "Configurable Enter Play Mode Settings")]
        [BilingualText("开启 Enter Play Mode Options", "Enable Enter Play Mode Options")]
        [LabelWidth(400)]
        [ShowInInspector]
        public bool IsEnterPlayMode
        {
            get => EditorSettings.enterPlayModeOptionsEnabled;
            set => EditorSettings.enterPlayModeOptionsEnabled = value;
        }

        [PropertyOrder(10)]
        [ShowInInspector]
        [LabelWidth(400)]
        [ShowIf("IsEnterPlayMode")]
        public bool ReloadDomain
        {
            get => (EditorSettings.enterPlayModeOptions & EnterPlayModeOptions.DisableDomainReload) == 0;
            set
            {
                if (value)
                {
                    EditorSettings.enterPlayModeOptions &= ~EnterPlayModeOptions.DisableDomainReload;
                }
                else
                {
                    EditorSettings.enterPlayModeOptions |= EnterPlayModeOptions.DisableDomainReload;
                }
            }
        }

        [PropertyOrder(10)]
        [LabelWidth(400)]
        [ShowInInspector]
        [ShowIf("IsEnterPlayMode")]
        public bool ReloadScene
        {
            get => (EditorSettings.enterPlayModeOptions & EnterPlayModeOptions.DisableSceneReload) == 0;
            set
            {
                if (value)
                {
                    EditorSettings.enterPlayModeOptions &= ~EnterPlayModeOptions.DisableSceneReload;
                }
                else
                {
                    EditorSettings.enterPlayModeOptions |= EnterPlayModeOptions.DisableSceneReload;
                }
            }
        }

        [PropertySpace]
        [BilingualButton("启用所有时机的执行选项", "Enable All Execution Options")]
        public void EnableAll()
        {
            ExecuteOnSubsystemRegistration = true;
            ExecuteOnAfterAssembliesLoaded = true;
            ExecuteOnBeforeSplashScreen = true;
            ExecuteOnBeforeSceneLoad = true;
            ExecuteOnAfterSceneLoad = true;
            Save(true);
        }

        [PropertySpace]
        [BilingualButton("禁用所有时机的执行选项", "Disable All Execution Options")]
        public void DisableAll()
        {
            ExecuteOnSubsystemRegistration = false;
            ExecuteOnAfterAssembliesLoaded = false;
            ExecuteOnBeforeSplashScreen = false;
            ExecuteOnBeforeSceneLoad = false;
            ExecuteOnAfterSceneLoad = false;
            Save(true);
        }
    }
}
