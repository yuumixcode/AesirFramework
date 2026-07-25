using System;
using System.Reflection;
using UnityEngine;

namespace Runestone.AesirInspector
{
    /// <summary>
    /// 解析数据工厂接口，自定义扩展解析数据工厂
    /// </summary>
    [Summary("解析数据工厂接口，自定义扩展解析数据工厂")]
    public interface IAnalysisDataFactory
    {
        /// <summary>
        /// 创建类型数据
        /// </summary>
        [Summary("创建类型数据")]
        ITypeData CreateTypeData(Type type,
            IAnalysisDataFactory factory = null,
            IAttributeFilter filter = null);

        /// <summary>
        /// 创建构造函数数据
        /// </summary>
        [Summary("创建构造函数数据")]
        IConstructorData CreateConstructorData(ConstructorInfo constructorInfo,
            IAttributeFilter filter = null);

        /// <summary>
        /// 创建事件数据
        /// </summary>
        [Summary("创建事件数据")]
        IEventData CreateEventData(EventInfo eventInfo, IAttributeFilter filter = null);

        /// <summary>
        /// 创建方法数据
        /// </summary>
        [Summary("创建方法数据")]
        IMethodData CreateMethodData(MethodInfo methodInfo, IAttributeFilter filter = null);

        /// <summary>
        /// 创建属性数据
        /// </summary>
        [Summary("创建属性数据")]
        IPropertyData CreatePropertyData(PropertyInfo propertyInfo, IAttributeFilter filter = null);

        /// <summary>
        /// 创建字段数据
        /// </summary>
        [Summary("创建字段数据")]
        IFieldData CreateFieldData(FieldInfo fieldInfo, IAttributeFilter filter = null);
    }

    /// <summary>
    /// Aesir Inspector 默认提供的解析数据工厂实现类
    /// </summary>
    [Summary("Aesir Inspector 默认提供的解析数据工厂实现类")]
    [Serializable]
    public class DefaultAnalysisDataFactory : IAnalysisDataFactory
    {
        /// <summary>
        /// 创建类型数据
        /// </summary>
        [Summary("创建类型数据")]
        public ITypeData CreateTypeData(Type type,
            IAnalysisDataFactory factory = null,
            IAttributeFilter filter = null)
        {
            if (type != null)
            {
                return new TypeData(type, filter, factory ?? this);
            }

            Debug.LogError("Type is null");
            return null;
        }

        /// <summary>
        /// 创建构造函数数据
        /// </summary>
        [Summary("创建构造函数数据")]
        public IConstructorData CreateConstructorData(ConstructorInfo constructorInfo,
            IAttributeFilter filter = null)
        {
            if (constructorInfo != null)
            {
                return new ConstructorData(constructorInfo, filter);
            }

            Debug.LogError("ConstructorInfo is null");
            return null;
        }

        /// <summary>
        /// 创建事件数据
        /// </summary>
        [Summary("创建事件数据")]
        public IEventData CreateEventData(EventInfo eventInfo, IAttributeFilter filter = null)
        {
            if (eventInfo != null)
            {
                return new EventData(eventInfo, filter);
            }

            Debug.LogError("EventInfo is null");
            return null;
        }

        /// <summary>
        /// 创建方法数据
        /// </summary>
        [Summary("创建方法数据")]
        public IMethodData CreateMethodData(MethodInfo methodInfo, IAttributeFilter filter = null)
        {
            if (methodInfo != null)
            {
                return new MethodData(methodInfo, filter);
            }

            Debug.LogError("MethodInfo is null");
            return null;
        }

        /// <summary>
        /// 创建属性数据
        /// </summary>
        [Summary("创建属性数据")]
        public IPropertyData CreatePropertyData(PropertyInfo propertyInfo, IAttributeFilter filter = null)
        {
            if (propertyInfo != null)
            {
                return new PropertyData(propertyInfo, filter);
            }

            Debug.LogError("PropertyInfo is null");
            return null;
        }

        /// <summary>
        /// 创建字段数据
        /// </summary>
        [Summary("创建字段数据")]
        public IFieldData CreateFieldData(FieldInfo fieldInfo, IAttributeFilter filter = null)
        {
            if (fieldInfo != null)
            {
                return new FieldData(fieldInfo, filter);
            }

            Debug.LogError("FieldInfo is null");
            return null;
        }
    }
}
