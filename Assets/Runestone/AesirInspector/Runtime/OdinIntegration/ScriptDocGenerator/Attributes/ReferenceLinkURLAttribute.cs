using System;

namespace Runestone.AesirInspector
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Struct,
        AllowMultiple = true, Inherited = false)]
    public class ReferenceLinkURLAttribute : Attribute
    {
        public readonly string WebUrl;

        public ReferenceLinkURLAttribute(string webUrl) => WebUrl = webUrl;
    }
}
