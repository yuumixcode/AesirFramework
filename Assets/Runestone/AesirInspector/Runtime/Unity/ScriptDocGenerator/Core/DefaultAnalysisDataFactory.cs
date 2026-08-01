using System;
using System.Reflection;
using UnityEngine;

namespace Runestone.AesirInspector
{
    /// <summary>
    /// 解析数据工厂接口，自定义扩展解析数据工厂
    /// </summary>
    public interface IAnalysisDataFactory
    {
        /// <summary>
        /// 创建类型数据
        /// </summary>
        ITypeData CreateTypeData(Type type,
            IAnalysisDataFactory factory = null,
            IAttributeFilter filter = null);

        /// <summary>
        /// 创建构造函数数据
        /// </summary>
        IConstructorData CreateConstructorData(ConstructorInfo constructorInfo,
            IAttributeFilter filter = null);

        /// <summary>
        /// 创建事件数据
        /// </summary>
        IEventData CreateEventData(EventInfo eventInfo, IAttributeFilter filter = null);

        /// <summary>
        /// 创建方法数据
        /// </summary>
        IMethodData CreateMethodData(MethodInfo methodInfo, IAttributeFilter filter = null);

        /// <summary>
        /// 创建属性数据
        /// </summary>
        IPropertyData CreatePropertyData(PropertyInfo propertyInfo, IAttributeFilter filter = null);

        /// <summary>
        /// 创建字段数据
        /// </summary>
        IFieldData CreateFieldData(FieldInfo fieldInfo, IAttributeFilter filter = null);
    }

    /// <summary>
    /// Aesir Inspector 默认提供的解析数据工厂实现类
    /// </summary>
    [Serializable]
    public class DefaultAnalysisDataFactory : IAnalysisDataFactory
    {
        /// <summary>
        /// 创建类型数据
        /// </summary>
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
