using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Runestone.AesirModules.ScriptDocGenerator
{
    /// <summary>
    /// 类型名称格式化工具（生成文档使用的"好看名"）。
    /// </summary>
    /// <remarks>
    /// 语义与 <c>Sirenix.Utilities.TypeExtensions.GetNiceName</c> / <c>GetNiceFullName</c> 对齐
    /// （别名表、数组/可空/引用/泛型/嵌套类型的拼接规则逐条对应）。
    /// <para>
    /// <b>为什么要自带一份</b>：本程序集（<c>Runestone.AesirModules.OdinInspector</c>）是全平台程序集，
    /// Player 构建不应依赖 Odin 运行时程序集；而原名格式化是 Odin <c>Sirenix.Utilities</c> 的扩展方法，
    /// 导致 ScriptDocGenerator 的运行时代码把 Odin 拖进玩家构建。移植的是纯格式化逻辑，
    /// 不涉及 Odin 专属能力。
    /// </para>
    /// <para>
    /// 仅主线程使用（框架约定），故缓存不加锁；缓存避免泛型递归拼接的重复分配。
    /// </para>
    /// </remarks>
    internal static class NiceTypeName
    {
        /// <summary>基础类型别名表（与 Sirenix 的 TypeNameAlternatives 一致）。</summary>
        static readonly Dictionary<Type, string> Aliases = new Dictionary<Type, string>
        {
            { typeof(float), "float" },
            { typeof(double), "double" },
            { typeof(sbyte), "sbyte" },
            { typeof(short), "short" },
            { typeof(int), "int" },
            { typeof(long), "long" },
            { typeof(byte), "byte" },
            { typeof(ushort), "ushort" },
            { typeof(uint), "uint" },
            { typeof(ulong), "ulong" },
            { typeof(decimal), "decimal" },
            { typeof(string), "string" },
            { typeof(char), "char" },
            { typeof(bool), "bool" }
        };

        static readonly Dictionary<Type, string> Cache = new Dictionary<Type, string>();

        /// <summary>
        /// 取不含命名空间的类型名（如 <c>Dictionary&lt;string, int&gt;</c>）；
        /// 嵌套类型以 <c>Outer.Inner</c> 形式展开。
        /// </summary>
        /// <param name="type">目标类型</param>
        /// <returns>格式化后的类型名</returns>
        internal static string GetNiceName(Type type)
        {
            if (type.IsNested && !type.IsGenericParameter)
            {
                return GetNiceName(type.DeclaringType) + "." + GetCachedNiceName(type);
            }

            return GetCachedNiceName(type);
        }

        /// <summary>
        /// 取含命名空间的类型名（如 <c>System.Collections.Generic.Dictionary&lt;string, int&gt;</c>）；
        /// 嵌套类型以 <c>Namespace.Outer.Inner</c> 形式展开。
        /// </summary>
        /// <param name="type">目标类型</param>
        /// <returns>格式化后的完整类型名</returns>
        internal static string GetNiceFullName(Type type)
        {
            if (type.IsNested && !type.IsGenericParameter)
            {
                return GetNiceFullName(type.DeclaringType) + "." + GetCachedNiceName(type);
            }

            var result = GetCachedNiceName(type);
            if (type.Namespace != null)
            {
                result = type.Namespace + "." + result;
            }

            return result;
        }

        static string GetCachedNiceName(Type type)
        {
            if (Cache.TryGetValue(type, out var cached))
            {
                return cached;
            }

            var created = CreateNiceName(type);
            Cache[type] = created;
            return created;
        }

        static string CreateNiceName(Type type)
        {
            if (type.IsArray)
            {
                var rank = type.GetArrayRank();
                var elementName = GetNiceName(type.GetElementType());
                switch (rank)
                {
                    case 1:
                        return elementName + "[]";
                    case 2:
                        return elementName + "[,]";
                    case 3:
                        return elementName + "[,,]";
                    case 4:
                        return elementName + "[,,,]";
                    default:
                        var arrayBuilder = new StringBuilder();
                        arrayBuilder.Append(elementName);
                        arrayBuilder.Append('[');
                        while (--rank > 0)
                        {
                            arrayBuilder.Append(',');
                        }

                        arrayBuilder.Append(']');
                        return arrayBuilder.ToString();
                }
            }

            if (Nullable.GetUnderlyingType(type) != null)
            {
                return GetNiceName(type.GetGenericArguments()[0]) + "?";
            }

            if (type.IsByRef)
            {
                return "ref " + GetNiceName(type.GetElementType());
            }

            if (type.IsGenericParameter || !type.IsGenericType)
            {
                return GetMaybeSimplifiedTypeName(type);
            }

            var builder = new StringBuilder();
            var name = type.Name;
            var tick = name.IndexOf('`');
            builder.Append(tick != -1 ? name.Substring(0, tick) : name);
            builder.Append('<');

            var arguments = type.GetGenericArguments();
            for (var i = 0; i < arguments.Length; i++)
            {
                if (i != 0)
                {
                    builder.Append(", ");
                }

                builder.Append(GetNiceName(arguments[i]));
            }

            builder.Append('>');
            return builder.ToString();
        }

        static string GetMaybeSimplifiedTypeName(Type type) =>
            Aliases.TryGetValue(type, out var simplified) ? simplified : type.Name;

        /// <summary>
        /// 取泛型类型定义的全部泛型参数约束字符串（如 <c>where T : class, new() where U : System.IDisposable</c>）。
        /// </summary>
        /// <remarks>
        /// 语义对齐 Sirenix 的 <c>TypeExtensions.GetGenericConstraintsString</c>：
        /// 逐参数拼接各自的约束片段并以空格连接。
        /// </remarks>
        /// <param name="type">目标类型（须为泛型类型定义）</param>
        /// <param name="useFullTypeNames">约束中的类型名是否带命名空间</param>
        /// <returns>约束字符串；无约束时为空串</returns>
        internal static string GetGenericConstraintsString(Type type, bool useFullTypeNames = false)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (!type.IsGenericTypeDefinition)
            {
                throw new ArgumentException($"类型 {GetNiceName(type)} 不是泛型类型定义", nameof(type));
            }

            var parameters = type.GetGenericArguments();
            var parts = new string[parameters.Length];
            for (var i = 0; i < parameters.Length; i++)
            {
                parts[i] = GetGenericParameterConstraintsString(parameters[i], useFullTypeNames);
            }

            return string.Join(" ", parts);
        }

        /// <summary>
        /// 取单个泛型参数的约束字符串（如 <c>where T : class, new()</c>）。
        /// </summary>
        /// <remarks>语义对齐 Sirenix 的 <c>TypeExtensions.GetGenericParameterConstraintsString</c>。</remarks>
        /// <param name="type">目标泛型参数类型</param>
        /// <param name="useFullTypeNames">约束中的类型名是否带命名空间</param>
        /// <returns>约束字符串；无约束时为空串</returns>
        internal static string GetGenericParameterConstraintsString(Type type, bool useFullTypeNames = false)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (!type.IsGenericParameter)
            {
                throw new ArgumentException($"类型 {GetNiceName(type)} 不是泛型参数", nameof(type));
            }

            var builder = new StringBuilder();
            var started = false;
            var special = type.GenericParameterAttributes;

            if ((special & GenericParameterAttributes.NotNullableValueTypeConstraint) ==
                GenericParameterAttributes.NotNullableValueTypeConstraint)
            {
                builder.Append("where ").Append(type.Name).Append(" : struct");
                started = true;
            }
            else if ((special & GenericParameterAttributes.ReferenceTypeConstraint) ==
                     GenericParameterAttributes.ReferenceTypeConstraint)
            {
                builder.Append("where ").Append(type.Name).Append(" : class");
                started = true;
            }

            if ((special & GenericParameterAttributes.DefaultConstructorConstraint) ==
                GenericParameterAttributes.DefaultConstructorConstraint)
            {
                builder.Append(started ? ", new()" : "where " + type.Name + " : new()");
                started = true;
            }

            foreach (var constraint in type.GetGenericParameterConstraints())
            {
                var name = useFullTypeNames ? GetNiceFullName(constraint) : GetNiceName(constraint);
                if (started)
                {
                    builder.Append(", ").Append(name);
                    continue;
                }

                builder.Append("where ").Append(type.Name).Append(" : ").Append(name);
                started = true;
            }

            return builder.ToString();
        }
    }
}
