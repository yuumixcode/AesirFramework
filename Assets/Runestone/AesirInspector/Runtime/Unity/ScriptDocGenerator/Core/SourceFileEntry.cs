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
        public string filePath;

        /// <summary>
        /// 按行分割的源代码内容。
        /// </summary>
        public string[] sourceLines;

        public SourceFileEntry(string filePath, string[] sourceLines)
        {
            this.filePath = filePath;
            this.sourceLines = sourceLines;
        }
    }
}
