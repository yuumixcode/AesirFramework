using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// 模块字段读取器。反射遍历 Model/Service 的可序列化字段，供调试窗口展示与编辑。
    /// </summary>
    /// <remarks>
    /// 供 Context Debugger 三版窗口共享使用，不依赖 Odin。
    /// <para><b>G10 精简原则</b>：默认只暴露 <c>ObservableValue&lt;T&gt;</c> 与 <c>[SerializeField]</c> 字段（调试必需项），
    /// 内部实现字段（<c>_valueChangedEvent</c>、缓存、句柄集合等）过滤，提供开关显示全部。</para>
    /// <para><b>ObservableValue 特判</b>：经 <c>ObservableValue{T}.PrivateValueFieldName</c> 常量读 <c>value</c> 字段，
    /// 写回经 <c>Value</c> setter 或 <c>InvokeMethodName</c> 触发通知（复用常量，不硬编码字符串）。</para>
    /// </remarks>
    public static class ModuleFieldReader
    {
        /// <summary>
        /// 字段条目
        /// </summary>
        public sealed class FieldEntry
        {
            /// <summary>字段信息</summary>
            public FieldInfo Field;
            /// <summary>所属对象（Model/Service 实例）</summary>
            public object Owner;
            /// <summary>是否为 ObservableValue&lt;T&gt; 类型</summary>
            public bool IsObservableValue;
            /// <summary>ObservableValue 的内部 value 字段（IsObservableValue 时非 null）</summary>
            public FieldInfo ObservableInnerValueField;
            /// <summary>ObservableValue 实例本身（IsObservableValue 时非 null）</summary>
            public object ObservableInstance;
            /// <summary>ObservableValue 包装的值类型（IsObservableValue 时非 null）</summary>
            public Type ObservableValueType;
            /// <summary>字段显示名</summary>
            public string DisplayName => Field.Name;

            /// <summary>读取当前值</summary>
            public object ReadValue()
            {
                if (IsObservableValue)
                {
                    return ObservableInnerValueField.GetValue(ObservableInstance);
                }

                return Field.GetValue(Owner);
            }

            /// <summary>写回值（ObservableValue 经 Value setter 触发通知）</summary>
            public void WriteValue(object newValue)
            {
                if (IsObservableValue)
                {
                    // 经 Value setter 写回，自动触发变更通知
                    var valueProp = ObservableInstance.GetType().GetProperty("Value");
                    valueProp?.SetValue(ObservableInstance, newValue);
                    return;
                }

                Field.SetValue(Owner, newValue);
            }
        }

        /// <summary>
        /// 读取模块的可展示字段（应用 G10 精简）
        /// </summary>
        /// <param name="module">Model/Service 实例</param>
        /// <param name="includeInternal">是否包含内部实现字段（默认 false）</param>
        public static List<FieldEntry> ReadFields(object module, bool includeInternal = false)
        {
            var result = new List<FieldEntry>();
            if (module == null)
            {
                return result;
            }

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (var field in module.GetType().GetFields(flags))
            {
                // G10 精简：默认只暴露 [SerializeField] 字段与 ObservableValue 字段
                if (!includeInternal && !IsVisibleByDefault(field))
                {
                    continue;
                }

                var entry = CreateEntry(module, field);
                if (entry != null)
                {
                    result.Add(entry);
                }
            }

            return result;
        }

        static bool IsVisibleByDefault(FieldInfo field)
        {
            if (field.GetCustomAttribute<SerializeField>() != null)
            {
                return true;
            }

            if (field.IsPublic)
            {
                return true;
            }

            return IsObservableValueType(field.FieldType);
        }

        static bool IsObservableValueType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ObservableValue<>);
        }

        static FieldEntry CreateEntry(object owner, FieldInfo field)
        {
            if (IsObservableValueType(field.FieldType))
            {
                var observableInstance = field.GetValue(owner);
                if (observableInstance == null)
                {
                    return null;
                }

                var valueType = field.FieldType.GetGenericArguments()[0];
                // 复用常量，避免硬编码字段名字符串
                var innerFieldName = GetConst<string>(field.FieldType, "PrivateValueFieldName") ?? "value";
                var innerField = field.FieldType.GetField(innerFieldName,
                    BindingFlags.Instance | BindingFlags.NonPublic);

                if (innerField == null)
                {
                    return null;
                }

                return new FieldEntry
                {
                    Field = field,
                    Owner = owner,
                    IsObservableValue = true,
                    ObservableInstance = observableInstance,
                    ObservableInnerValueField = innerField,
                    ObservableValueType = valueType,
                };
            }

            return new FieldEntry
            {
                Field = field,
                Owner = owner,
                IsObservableValue = false,
            };
        }

        static T GetConst<T>(Type type, string constName)
        {
            var field = type.GetField(constName, BindingFlags.Public | BindingFlags.Static);
            return field != null ? (T)field.GetValue(null) : default;
        }

        /// <summary>
        /// 读取 ObservableValue 包装的值类型（供编辑控件分发）
        /// </summary>
        public static Type GetEditableValueType(FieldEntry entry)
        {
            return entry.IsObservableValue ? entry.ObservableValueType : entry.Field.FieldType;
        }
    }
}
