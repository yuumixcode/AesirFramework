using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector
{
    /// <summary>
    /// Aesir Inspector 项目级别配置。
    /// </summary>
    public class AesirInspectorProjectSettingsSO : AesirInspectorSettings<AesirInspectorProjectSettingsSO>
    {
        [SerializeField]
        bool isInitialized;

        public bool IsInitialized
        {
            get => isInitialized;
            set
            {
                isInitialized = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(this);
                AssetDatabase.SaveAssets();
#endif
            }
        }
    }
}
