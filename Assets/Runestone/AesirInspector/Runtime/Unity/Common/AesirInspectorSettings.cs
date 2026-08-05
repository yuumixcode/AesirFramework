using UnityEngine;

namespace Runestone.AesirInspector
{
    /// <summary>
    /// Aesir Inspector 的编辑器单例设置基类。
    /// 自动处理资源的获取、创建以及在 EditorBuildSettings 中的注册。
    /// </summary>
    /// <typeparam name="T">设置项类型</typeparam>
    public abstract class AesirInspectorSettings<T> : ScriptableObject where T : AesirInspectorSettings<T>
    {
        static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                var type = typeof(T);
                var configName = type.FullName;
                var assetName = type.Name;

                _instance = ScriptableObjectSafeEditorUtility.GetOrCreateEditorScriptableObject<T>(configName,
                    AesirInspectorPaths.PreferencesAssetsFolderPath, assetName);

                return _instance;
            }
        }
    }
}
