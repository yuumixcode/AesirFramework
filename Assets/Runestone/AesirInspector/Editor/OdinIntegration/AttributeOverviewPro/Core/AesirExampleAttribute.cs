using System;
using System.Runtime.CompilerServices;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// 标记一个类为 Aesir Inspector 特性示例类，编译时自动捕获源文件绝对路径。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AesirExampleAttribute : Attribute
    {
        public AesirExampleAttribute([CallerFilePath] string filePath = "unknown") =>
            FilePath = filePath;

        /// <summary>
        /// 示例类的源文件绝对路径。
        /// </summary>
        public string FilePath { get; }
    }
}
