namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// FolderPath 特性的介绍数据。
    /// </summary>
    internal class FolderPathAttributeData : AbstractAttributeData
    {
        public override BilingualHeaderControl BilingualHeaderControl { get; set; } =
            new BilingualHeaderControl("FolderPath", "FolderPath",
                "FolderPath 特性在字符串属性上绘制一个文件夹选择器，方便用户选择文件夹路径。",
                "The FolderPath attribute draws a folder picker for string properties, making it easy for users to select folder paths.",
                OdinInspectorDocumentationLinks.FolderPathUrl);

        public override BilingualData[] UsageTips { get; set; } =
        {
            new BilingualData("可以配置为绝对路径或相对于项目根目录/指定文件夹的相对路径。",
                "Can be configured to use absolute paths or relative paths from the project root or a specified folder."),
            new BilingualData("通过 RequireExistingPath 参数可以强制要求路径必须存在。",
                "The RequireExistingPath parameter can force the selected path to exist."),
            new BilingualData("与 FilePath 特性类似，但专门用于选择文件夹。",
                "Similar to the FilePath attribute, but specifically for selecting folders.")
        };

        public override ParameterValue[] AttributeParameters { get; set; } =
        {
            new ParameterValue(typeof(bool).FullName, "AbsolutePath",
                new BilingualData("是否使用绝对路径。", "Whether to use absolute paths.")),
            new ParameterValue(typeof(string).FullName, "ParentFolder",
                new BilingualData("相对路径的父文件夹。", "The parent folder for relative paths.")),
            new ParameterValue(typeof(bool).FullName, "RequireExistingPath",
                new BilingualData("选定的路径是否必须存在。", "Whether the selected path must exist.")),
            new ParameterValue(typeof(bool).FullName, "UseBackslashes",
                new BilingualData("是否使用反斜杠。", "Whether to use backslashes."))
        };

        public override ResolvedStringParameterValue[] ResolvedStringParameters { get; set; } = { };

        public override AttributeExamplePreviewItem[] ExamplePreviewItems { get; set; } =
        {
            new AttributeExamplePreviewItem().InitializeUnitySerializedExample("Basic Usage",
                FolderPathExampleSO.Instance)
        };
    }
}
