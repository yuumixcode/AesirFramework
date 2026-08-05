using System;

namespace Runestone.AesirInspector
{
    /// <summary>
    /// 提供类似于 XML 文档 summary 部分的描述性元数据。
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class SummaryAttribute : Attribute
    {
        readonly string _summaryText;
        public SummaryAttribute(string summaryText) => _summaryText = summaryText;
        public string GetSummary() => _summaryText;
    }
}
