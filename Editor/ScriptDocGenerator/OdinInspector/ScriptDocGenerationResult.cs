using System.Collections.Generic;

namespace Runestone.AesirModules.ScriptDocGenerator.Editor
{
    /// <summary>
    /// 一次脚本文档生成调用的结果：写入的文档文件清单、使用的设置与输出根目录，
    /// 以及未能映射到编译产物的源码类型名（文件夹模式）。
    /// </summary>
    public sealed class ScriptDocGenerationResult
    {
        /// <summary>使用的文档生成器设置显示名（资产名 + 类型名）。</summary>
        public string SettingsName { get; internal set; }

        /// <summary>本次写入的输出根目录（绝对路径）。</summary>
        public string OutputFolder { get; internal set; }

        /// <summary>成功写入的文档文件绝对路径（按生成顺序排列）。</summary>
        public List<string> GeneratedFiles { get; } = new List<string>();

        /// <summary>
        /// 文件夹模式中源码声明了、但当前编译域内不存在的类型名（典型成因：被条件编译剔除）。
        /// 其他模式恒为空。
        /// </summary>
        public List<string> UnresolvedTypeNames { get; } = new List<string>();

        /// <summary>成功写入的文档数量。</summary>
        public int GeneratedCount => GeneratedFiles.Count;

        /// <summary>是否至少写入了一份文档。</summary>
        public bool Success => GeneratedFiles.Count > 0;

        /// <summary>单行摘要（含未解析类型提示），便于日志与 AI 助手回显。</summary>
        public override string ToString()
        {
            var summary = "脚本文档生成结果：" + GeneratedFiles.Count + " 个文档 → " + OutputFolder +
                          "（设置：" + SettingsName + "）";
            if (UnresolvedTypeNames.Count > 0)
            {
                summary += "；未解析类型 " + UnresolvedTypeNames.Count + " 个：" +
                           string.Join(", ", UnresolvedTypeNames);
            }

            return summary;
        }
    }
}
