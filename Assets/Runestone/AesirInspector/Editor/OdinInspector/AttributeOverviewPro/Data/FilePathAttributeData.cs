namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// FilePath 特性的介绍数据。
    /// </summary>
    internal class FilePathAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("FilePath", "FilePath", "FilePath 特性在字符串属性上绘制一个文件选择器，方便用户选择文件路径。",
                "The FilePath attribute draws a file picker for string properties, making it easy for users to select file paths.",
                OdinInspectorDocumentationLinks.FilePathUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("支持限制扩展名，通过 Extensions 参数设置（如 'cs, asset'）。",
                "Supports restricting file extensions via the Extensions parameter (e.g., 'cs, asset')."),
            new BilingualData("可以配置为绝对路径或相对于项目根目录/指定文件夹的相对路径。",
                "Can be configured to use absolute paths or relative paths from the project root or a specified folder."),
            new BilingualData("通过 RequireExistingPath 参数可以强制要求路径必须存在。",
                "The RequireExistingPath parameter can force the selected path to exist.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(bool).FullName, "AbsolutePath",
                new BilingualData("是否使用绝对路径。", "Whether to use absolute paths.")),
            new ParameterValue(typeof(string).FullName, "Extensions",
                new BilingualData("允许的文件扩展名（用逗号分隔）。", "Allowed file extensions (comma separated).")),
            new ParameterValue(typeof(string).FullName, "ParentFolder",
                new BilingualData("相对路径的父文件夹。", "The parent folder for relative paths.")),
            new ParameterValue(typeof(bool).FullName, "RequireExistingPath",
                new BilingualData("选定的路径是否必须存在。", "Whether the selected path must exist.")),
            new ParameterValue(typeof(bool).FullName, "IncludeFileExtension",
                new BilingualData("选定的路径是否包含扩展名。",
                    "Whether the selected path should include the file extension.")),
            new ParameterValue(typeof(bool).FullName, "UseBackslashes",
                new BilingualData("是否使用反斜杠。", "Whether to use backslashes."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                FilePathExampleSO.Instance)
        };
    }
}
