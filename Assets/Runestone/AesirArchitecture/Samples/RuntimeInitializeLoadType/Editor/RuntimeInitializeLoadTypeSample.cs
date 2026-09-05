using UnityEngine;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// RuntimeInitializeLoadType 示例 — 在五个不同的初始化时机输出日志，演示执行顺序。
    /// </summary>
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

            Debug.Log("AesirArchitecture 示例：RuntimeInitializeLoadType.SubsystemRegistration 触发");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public static void OnAfterAssembliesLoaded()
        {
            if (!Settings.ExecuteOnAfterAssembliesLoaded)
            {
                return;
            }

            Debug.Log("AesirArchitecture 示例：RuntimeInitializeLoadType.AfterAssembliesLoaded 触发");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        public static void OnBeforeSplashScreen()
        {
            if (!Settings.ExecuteOnBeforeSplashScreen)
            {
                return;
            }

            Debug.Log("AesirArchitecture 示例：RuntimeInitializeLoadType.BeforeSplashScreen 触发");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void OnBeforeSceneLoad()
        {
            if (!Settings.ExecuteOnBeforeSceneLoad)
            {
                return;
            }

            Debug.Log("AesirArchitecture 示例：RuntimeInitializeLoadType.BeforeSceneLoad 触发");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void OnAfterSceneLoad()
        {
            if (!Settings.ExecuteOnAfterSceneLoad)
            {
                return;
            }

            Debug.Log("AesirArchitecture 示例：RuntimeInitializeLoadType.AfterSceneLoad 触发");
        }
    }
}
