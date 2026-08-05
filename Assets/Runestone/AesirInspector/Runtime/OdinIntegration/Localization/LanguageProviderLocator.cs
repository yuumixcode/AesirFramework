namespace Runestone.AesirInspector
{
    /// <summary>
    /// 语言定位器。用于解耦 BilingualData 对具体实现的依赖。
    /// </summary>
    public static class LanguageProviderLocator
    {
        static ILanguageProvider _provider;

        /// <summary>
        /// 获取或设置语言提供者。默认返回 AesirInspectorLanguageSettingsSO.Instance。
        /// </summary>
        public static ILanguageProvider Provider
        {
            get => _provider ?? AesirInspectorLanguageSettingsSO.Instance;
            set => _provider = value;
        }
    }
}
