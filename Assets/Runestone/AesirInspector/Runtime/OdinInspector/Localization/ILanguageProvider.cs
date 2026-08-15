namespace Runestone.AesirInspector
{
    /// <summary>
    /// 语言提供者接口，用于解耦本地化数据与具体设置实现。
    /// </summary>
    public interface ILanguageProvider
    {
        /// <summary>
        /// 当前是否为中文。
        /// </summary>
        bool IsChinese { get; }

        /// <summary>
        /// 当前是否为英文。
        /// </summary>
        bool IsEnglish { get; }

        /// <summary>
        /// 当前语言。
        /// </summary>
        InspectorLanguage CurrentLanguage { get; }
    }
}
