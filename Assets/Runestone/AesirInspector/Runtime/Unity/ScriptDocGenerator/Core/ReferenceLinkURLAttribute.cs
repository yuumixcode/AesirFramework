using System;

namespace Runestone.AesirInspector
{
    [Summary("参考链接特性")]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Struct,
        AllowMultiple = true, Inherited = false)]
    public class ReferenceLinkURLAttribute : Attribute
    {
        [Summary("网页链接")]
        public readonly string WebUrl;

        public ReferenceLinkURLAttribute(string webUrl) => WebUrl = webUrl;
    }
}
