using System;

namespace Runestone.AesirInspector
{
    [Serializable]
    [Summary("访问修饰符类型")]
    public enum AccessModifierType
    {
        [Summary("公共访问修饰符")]
        Public = 0,

        [Summary("受保护的内部访问修饰符")]
        ProtectedInternal = 1,

        [Summary("受保护访问修饰符")]
        Protected = 2,

        [Summary("内部访问修饰符")]
        Internal = 4,

        [Summary("私有受保护访问修饰符")]
        PrivateProtected = 8,

        [Summary("私有访问修饰符")]
        Private = 16,

        [Summary("无访问修饰符")]
        None = 32
    }

    [Summary("访问修饰符扩展方法")]
    public static class AccessModifierTypeExtensions
    {
        [Summary("将访问修饰符类型转换为对应的字符串表示")]
        public static string ConvertToString(this AccessModifierType modifier)
        {
            return modifier switch
            {
                AccessModifierType.Public => "public",
                AccessModifierType.Private => "private",
                AccessModifierType.Protected => "protected",
                AccessModifierType.Internal => "internal",
                AccessModifierType.ProtectedInternal => "protected internal",
                AccessModifierType.PrivateProtected => "private protected",
                _ => ""
            };
        }
    }
}
