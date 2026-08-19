using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// Context 注册表静态扫描器。扫描 AppDomain 中全部 AbstractContext&lt;T&gt; 子类并读取其单例状态。
    /// </summary>
    /// <remarks>
    /// 供 Context Debugger 三版窗口（IMGUI / Odin / UI Toolkit）共享使用，不依赖 Odin。
    /// <para><b>不主动触发初始化</b>：仅读取静态 <c>_instance</c> 字段（可能为 null），
    /// 是否初始化由窗口提供按钮显式触发（避免编辑期意外创建上下文）。</para>
    /// </remarks>
    public static class ContextRegistryScanner
    {
        /// <summary>
        /// Context 扫描结果条目
        /// </summary>
        public sealed class Entry
        {
            /// <summary>Context 具体类型</summary>
            public Type ContextType;
            /// <summary>已初始化的实例（未初始化时为 null）</summary>
            public IContext Instance;
            /// <summary>是否已初始化</summary>
            public bool Initialized => Instance != null && Instance.Initialized;
            /// <summary>显示名（类型名）</summary>
            public string DisplayName => ContextType.Name;
        }

        /// <summary>
        /// 扫描全部 AbstractContext&lt;T&gt; 子类并返回条目列表
        /// </summary>
        public static List<Entry> Scan()
        {
            var result = new List<Entry>();
            var openGeneric = typeof(AbstractContext<>);

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    types = e.Types.Where(t => t != null).ToArray();
                }

                foreach (var type in types)
                {
                    if (type == null || type.IsAbstract || !InheritsFromOpenGeneric(type, openGeneric))
                    {
                        continue;
                    }

                    var instance = ReadStaticInstance(type);
                    result.Add(new Entry
                    {
                        ContextType = type,
                        Instance = instance,
                    });
                }
            }

            return result.OrderBy(e => e.DisplayName).ToList();
        }

        /// <summary>
        /// 触发指定 Context 的初始化（经 Instance getter）
        /// </summary>
        public static IContext EnsureInitialized(Type contextType)
        {
            var prop = contextType.GetProperty("Instance",
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            return prop?.GetValue(null) as IContext;
        }

        static bool InheritsFromOpenGeneric(Type type, Type openGeneric)
        {
            var current = type.BaseType;
            while (current != null)
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition() == openGeneric)
                {
                    return true;
                }

                current = current.BaseType;
            }

            return false;
        }

        static IContext ReadStaticInstance(Type contextType)
        {
            // _instance 是 AbstractContext<T> 的私有静态字段
            var current = contextType.BaseType;
            while (current != null)
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(AbstractContext<>))
                {
                    var field = current.GetField("_instance",
                        BindingFlags.NonPublic | BindingFlags.Static);
                    return field?.GetValue(null) as IContext;
                }

                current = current.BaseType;
            }

            return null;
        }
    }
}
