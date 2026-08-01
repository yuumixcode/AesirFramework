using System;

namespace Runestone.AesirInspector
{
    [Serializable]
    public enum AccessModifierType
    {
        Public = 0,

        ProtectedInternal = 1,

        Protected = 2,

        Internal = 4,

        PrivateProtected = 8,

        Private = 16,

        None = 32
    }

    public static class AccessModifierTypeExtensions
    {
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
