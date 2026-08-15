using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Runestone.AesirInspector
{
    /// <summary>
    /// 检查器语言枚举。
    /// </summary>
    public enum InspectorLanguage
    {
        Chinese = 0,

        English = 1
    }

    /// <summary>
    /// Aesir Inspector 检查器语言设置。
    /// </summary>
    public class AesirInspectorLanguageSettingsSO : AesirInspectorSettings<AesirInspectorLanguageSettingsSO>,
        ILanguageProvider
    {
        [SerializeField]
        InspectorLanguage currentLanguage = InspectorLanguage.Chinese;

        public static bool CurrentIsChinese => Instance != null && Instance.IsChinese;

        public static bool CurrentIsEnglish => Instance != null && Instance.IsEnglish;

        public bool IsChinese => currentLanguage == InspectorLanguage.Chinese;
        public bool IsEnglish => currentLanguage == InspectorLanguage.English;
        public InspectorLanguage CurrentLanguage => currentLanguage;

        public static event Action LanguageChanged;

        public static void SetChinese()
        {
            Instance.currentLanguage = InspectorLanguage.Chinese;
            LanguageChanged?.Invoke();
        }

        public static void SetEnglish()
        {
            Instance.currentLanguage = InspectorLanguage.English;
            LanguageChanged?.Invoke();
        }

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        static void RegisterPlayModeChangeHandler()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        static void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            if (obj == PlayModeStateChange.EnteredPlayMode)
            {
                LanguageChanged = null;
            }
        }
#endif
    }
}
