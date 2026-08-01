using System;
using System.Linq;

namespace Runestone.AesirInspector
{
    /// <summary>
    /// 特性过滤器接口，用于过滤掉不需要的特性
    /// </summary>
    public interface IAttributeFilter
    {
        /// <summary>
        /// 排除的特性类型
        /// </summary>
        Type[] ExcludeTypes { get; }

        /// <summary>
        /// 判断传入的特性类型是否应该被过滤掉
        /// </summary>
        bool ShouldFilterOut(Type type);
    }

    /// <summary>
    /// 默认特性过滤器，构造函数中传入需要排除的 Attribute 类型
    /// </summary>
    public class DefaultAttributeFilter : IAttributeFilter
    {
        /// <summary>
        /// 创建默认特性过滤器
        /// </summary>
        public DefaultAttributeFilter(Type[] excludeTypes)
        {
            if (excludeTypes != null)
            {
                ExcludeTypes = excludeTypes;
            }
        }

        #region IAttributeFilter Members

        /// <summary>
        /// 排除的特性类型
        /// </summary>
        public Type[] ExcludeTypes { get; }

        /// <summary>
        /// 判断传入的特性类型是否应该被过滤掉
        /// </summary>
        public bool ShouldFilterOut(Type type) => ExcludeTypes.Contains(type);

        #endregion
    }
}
