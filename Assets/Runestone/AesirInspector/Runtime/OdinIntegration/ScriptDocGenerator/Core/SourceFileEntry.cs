using System;

namespace Runestone.AesirInspector
{
    /// <summary>
    /// 源代码文件路径与内容的绑定容器。
    /// </summary>
    [Serializable]
    public class SourceFileEntry
    {
        /// <summary>
        /// 相对路径（Assets/ 开头）。
        /// </summary>
        public string FilePath;

        /// <summary>
        /// 按行分割的源代码内容。
        /// </summary>
        public string[] SourceLines;

        public SourceFileEntry(string filePath, string[] sourceLines)
        {
            FilePath = filePath;
            SourceLines = sourceLines;
        }
    }
}
