using System;

namespace Runestone.AesirModules.ScriptDocGenerator
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Enum | AttributeTargets.Struct,
        AllowMultiple = true, Inherited = false)]
    public class ReferenceLinkURLAttribute : Attribute
    {
        public readonly string WebUrl;

        public ReferenceLinkURLAttribute(string webUrl) => WebUrl = webUrl;
    }
}
