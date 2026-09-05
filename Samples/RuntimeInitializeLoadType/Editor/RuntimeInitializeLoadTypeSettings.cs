using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using FilePathAttribute = UnityEditor.FilePathAttribute;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// RuntimeInitializeLoadType 示例设置 — 控制五个初始化时机回调是否输出日志，并提供 Configurable Enter Play Mode 快捷配置。
    /// </summary>
    [FilePath(ProjectFilePath + "/RuntimeInitializeLoadType/LoadTypeSettings.asset",
        FilePathAttribute.Location.ProjectFolder)]
    public class RuntimeInitializeLoadTypeSettings : ScriptableSingleton<RuntimeInitializeLoadTypeSettings>
    {
        const string ProjectFilePath = "ScriptableSingleton/AesirArchitecture/Samples";

        bool _executeOnSubsystemRegistration;
        bool _executeOnAfterAssembliesLoaded;
        bool _executeOnBeforeSceneLoad;
        bool _executeOnBeforeSplashScreen;

        bool _executeOnAfterSceneLoad;

        [Title("是否输出对应时机的日志")]
        [ShowInInspector]
        [LabelWidth(400)]
        [Tooltip("Sample 执行在 SubsystemRegistration 时机的方法")]
        public bool ExecuteOnSubsystemRegistration
        {
            get => _executeOnSubsystemRegistration;
            set
            {
                _executeOnSubsystemRegistration = value;
                Save(true);
            }
        }

        [ShowInInspector]
        [LabelWidth(400)]
        [Tooltip("Sample 执行在 AfterAssembliesLoaded 时机的方法")]
        public bool ExecuteOnAfterAssembliesLoaded
        {
            get => _executeOnAfterAssembliesLoaded;
            set
            {
                _executeOnAfterAssembliesLoaded = value;
                Save(true);
            }
        }

        [ShowInInspector]
        [LabelWidth(400)]
        [Tooltip("Sample 执行在 BeforeSplashScreen 时机的方法")]
        public bool ExecuteOnBeforeSplashScreen
        {
            get => _executeOnBeforeSplashScreen;
            set
            {
                _executeOnBeforeSplashScreen = value;
                Save(true);
            }
        }

        [ShowInInspector]
        [LabelWidth(400)]
        [Tooltip("Sample 执行在 BeforeSceneLoad 时机的方法")]
        public bool ExecuteOnBeforeSceneLoad
        {
            get => _executeOnBeforeSceneLoad;
            set
            {
                _executeOnBeforeSceneLoad = value;
                Save(true);
            }
        }

        [ShowInInspector]
        [LabelWidth(400)]
        [Tooltip("Sample 执行在 AfterSceneLoad 时机的方法")]
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
        [Title("Configurable Enter Play Mode 设置")]
        [ShowInInspector]
        [LabelWidth(400)]
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
        [Button("启用所有时机的执行选项")]
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
        [Button("禁用所有时机的执行选项")]
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
