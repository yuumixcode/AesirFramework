using UnityEngine;

namespace Runestone.AesirInspector.Samples.LoadType.Editor
{
    public static class RuntimeInitializeLoadTypeSample
    {
        public static RuntimeInitializeLoadTypeSettings Settings =>
            RuntimeInitializeLoadTypeSettings.instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void OnSubsystemRegistration()
        {
            if (!Settings.ExecuteOnSubsystemRegistration)
            {
                return;
            }

            Debug.Log(AesirInspectorLanguageSettingsSO.CurrentIsEnglish
                ? "Aesir Inspector Sample: RuntimeInitializeLoadType.SubsystemRegistration triggered"
                : "Aesir Inspector 示例：RuntimeInitializeLoadType.SubsystemRegistration 触发");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public static void OnAfterAssembliesLoaded()
        {
            if (!Settings.ExecuteOnAfterAssembliesLoaded)
            {
                return;
            }

            Debug.Log(AesirInspectorLanguageSettingsSO.CurrentIsEnglish
                ? "Aesir Inspector Sample: RuntimeInitializeLoadType.AfterAssembliesLoaded triggered"
                : "Aesir Inspector 示例：RuntimeInitializeLoadType.AfterAssembliesLoaded 触发");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        public static void OnBeforeSplashScreen()
        {
            if (!Settings.ExecuteOnBeforeSplashScreen)
            {
                return;
            }

            Debug.Log(AesirInspectorLanguageSettingsSO.CurrentIsEnglish
                ? "Aesir Inspector Sample: RuntimeInitializeLoadType.BeforeSplashScreen triggered"
                : "Aesir Inspector 示例：RuntimeInitializeLoadType.BeforeSplashScreen 触发");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void OnBeforeSceneLoad()
        {
            if (!Settings.ExecuteOnBeforeSceneLoad)
            {
                return;
            }

            Debug.Log(AesirInspectorLanguageSettingsSO.CurrentIsEnglish
                ? "Aesir Inspector Sample: RuntimeInitializeLoadType.BeforeSceneLoad triggered"
                : "Aesir Inspector 示例：RuntimeInitializeLoadType.BeforeSceneLoad 触发");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void OnAfterSceneLoad()
        {
            if (!Settings.ExecuteOnAfterSceneLoad)
            {
                return;
            }

            Debug.Log(AesirInspectorLanguageSettingsSO.CurrentIsEnglish
                ? "Aesir Inspector Sample: RuntimeInitializeLoadType.AfterSceneLoad triggered"
                : "Aesir Inspector 示例：RuntimeInitializeLoadType.AfterSceneLoad 触发");
        }
    }
}
