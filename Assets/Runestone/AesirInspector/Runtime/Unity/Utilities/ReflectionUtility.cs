using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Runestone.AesirInspector
{
    /// <summary>
    /// 反射工具类，提供程序集、命名空间及成员的反射操作方法
    /// </summary>
    public static class ReflectionUtility
    {
        /// <summary>
        /// 获取名称中包含指定字符串的所有程序集
        /// </summary>
        public static Assembly[] GetAssembliesOfNameContainString(string partOfAssemblyName)
        {
            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(assembly => assembly.FullName.Contains(partOfAssemblyName)).ToArray();
                return assemblies.Length > 0 ? assemblies : Array.Empty<Assembly>();
            }
            catch (Exception ex)
            {
                throw new Exception($"发生错误: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取指定程序集中的所有命名空间
        /// </summary>
        public static List<string> GetNamespacesInAssembly(Assembly assembly)
        {
            var types = assembly.GetTypes();
            var namespaces = types.Select(type => type.Namespace).Where(ns => ns != null).Distinct().ToList();
            return namespaces;
        }

        /// <summary>
        /// 获取成员的值（支持字段和属性）。
        /// </summary>
        public static object GetMemberValue(MemberInfo member, object obj)
        {
            if ((object)(member as FieldInfo) != null)
            {
                return ((FieldInfo)member).GetValue(obj);
            }

            return (object)(member as PropertyInfo) != null
                ? ((PropertyInfo)member).GetGetMethod(true).Invoke(obj, null)
                : throw new ArgumentException($"Can't get the value of a {member.GetType().Name}");
        }

        /// <summary>
        /// 获取成员的返回类型（支持字段、属性、方法和事件）。
        /// </summary>
        public static Type GetReturnType(MemberInfo memberInfo)
        {
            var fieldInfo = memberInfo as FieldInfo;
            if (fieldInfo != null)
            {
                return fieldInfo.FieldType;
            }

            var propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo != null)
            {
                return propertyInfo.PropertyType;
            }

            var methodInfo = memberInfo as MethodInfo;
            if (methodInfo != null)
            {
                return methodInfo.ReturnType;
            }

            var eventInfo = memberInfo as EventInfo;
            return eventInfo != null ? eventInfo.EventHandlerType : null;
        }

        /// <summary>
        /// 判断成员是否为静态成员。
        /// </summary>
        public static bool IsStatic(MemberInfo member)
        {
            var fieldInfo = member as FieldInfo;
            if (fieldInfo != null)
            {
                return fieldInfo.IsStatic;
            }

            var propertyInfo = member as PropertyInfo;
            if (propertyInfo != null)
            {
                return !propertyInfo.CanRead
                    ? propertyInfo.GetSetMethod(true).IsStatic
                    : propertyInfo.GetGetMethod(true).IsStatic;
            }

            var methodBase = member as MethodBase;
            if (methodBase != null)
            {
                return methodBase.IsStatic;
            }

            var eventInfo = member as EventInfo;
            if (eventInfo != null)
            {
                return eventInfo.GetRaiseMethod(true).IsStatic;
            }

            var type = member as Type;
            if (!(type != null))
            {
                throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture,
                    "Unable to determine IsStatic for member {0}.{1}MemberType was {2} but only fields, properties, methods, events and types are supported.",
                    member.DeclaringType?.FullName, member.Name, member.GetType().FullName));
            }

            return type.IsSealed && type.IsAbstract;
        }

        /// <summary>
        /// 获取成员上的指定类型特性。
        /// </summary>
        public static IEnumerable<T> GetAttributes<T>(ICustomAttributeProvider member) where T : Attribute =>
            GetAttributes<T>(member, false);

        /// <summary>
        /// 获取成员上的指定类型特性。
        /// </summary>
        public static IEnumerable<T> GetAttributes<T>(ICustomAttributeProvider member, bool inherit)
            where T : Attribute
        {
            try
            {
                return member.GetCustomAttributes(typeof(T), inherit).Cast<T>();
            }
            catch
            {
                return Array.Empty<T>();
            }
        }

        /// <summary>
        /// 判断方法是否为扩展方法。
        /// </summary>
        public static bool IsExtensionMethod(MethodBase method)
        {
            var declaringType = method.DeclaringType;
            return declaringType != null && declaringType.IsSealed && !declaringType.IsGenericType &&
                   !declaringType.IsNested && method.IsDefined(typeof(ExtensionAttribute), false);
        }

        /// <summary>
        /// 获取类型的所有基类和接口。
        /// </summary>
        public static IEnumerable<Type> GetBaseTypes(Type type, bool includeSelf = false)
        {
            var first = GetBaseClasses(type, includeSelf).Concat(type.GetInterfaces());
            if (includeSelf && type.IsInterface)
            {
                first = first.Concat(new[]
                {
                    type
                });
            }

            return first;
        }

        /// <summary>
        /// 获取类型的所有基类。
        /// </summary>
        public static IEnumerable<Type> GetBaseClasses(Type type, bool includeSelf = false)
        {
            if (type?.BaseType != null)
            {
                if (includeSelf)
                {
                    yield return type;
                }

                for (var current = type.BaseType; current != null; current = current.BaseType)
                    yield return current;
            }
        }
    }
}
