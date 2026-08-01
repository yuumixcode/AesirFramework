using System;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// Odin Inspector 特性分类枚举，用于将特性面板归入对应分类。
    /// </summary>
    [Flags]
    public enum AesirAttributeCategory
    {
        None = 0,
        Essentials = 1 << 0,
        Buttons = 1 << 1,
        Collections = 1 << 2,
        Groups = 1 << 3,
        Conditionals = 1 << 4,
        Numbers = 1 << 5,
        TypeSpecifics = 1 << 6,
        Validation = 1 << 7,
        Misc = 1 << 8,
        Meta = 1 << 9,
        Unity = 1 << 10,
        Debug = 1 << 11
    }

    /// <summary>
    /// AesirAttributeCategory 枚举扩展方法。
    /// </summary>
    public static class AesirAttributeCategoryExtensions
    {
        /// <summary>
        /// 高性能 HasFlag 实现，避免装箱操作。
        /// </summary>
        public static bool HasFlagFast(this AesirAttributeCategory value, AesirAttributeCategory flag) =>
            (value & flag) != 0;
    }
}
