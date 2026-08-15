using System;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// 为特性面板 SO 标记所属的 Odin 特性分类。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AttributeCategoryAttribute : Attribute
    {
        public AttributeCategoryAttribute(AesirAttributeCategory category) => Category = category;

        /// <summary>
        /// 所属特性分类。
        /// </summary>
        public AesirAttributeCategory Category { get; }
    }
}
