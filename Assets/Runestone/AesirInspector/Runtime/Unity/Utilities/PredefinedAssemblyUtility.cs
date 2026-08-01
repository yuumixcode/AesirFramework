using System;
using System.Collections.Generic;

namespace Runestone.AesirInspector
{
    /// <summary>
    /// 预定义程序集类型
    /// </summary>
    public enum PredefinedAssemblyType
    {
        None = 0,

        /// <summary>
        /// 程序集 CSharp
        /// </summary>
        AssemblyCSharp = 1,

        /// <summary>
        /// 程序集 CSharp-Editor
        /// </summary>
        AssemblyCSharpEditor = 2,

        /// <summary>
        /// 程序集 CSharp-Editor-firstpass
        /// </summary>
        AssemblyCSharpEditorFirstPass = 3,

        /// <summary>
        /// 程序集 CSharp-firstpass
        /// </summary>
        AssemblyCSharpFirstPass = 4
    }

    /// <summary>
    /// 预定义程序集工具类，提供获取程序集类型及运行时类型的方法
    /// </summary>
    public static class PredefinedAssemblyUtility
    {
        /// <summary>
        /// 根据程序集名称获取对应的预定义程序集类型
        /// </summary>
        public static PredefinedAssemblyType? GetAssemblyType(string assemblyName)
        {
            return assemblyName switch
            {
                "Assembly-CSharp" => PredefinedAssemblyType.AssemblyCSharp,
                "Assembly-CSharp-Editor" => PredefinedAssemblyType.AssemblyCSharpEditor,
                "Assembly-CSharp-Editor-firstpass" => PredefinedAssemblyType.AssemblyCSharpEditorFirstPass,
                "Assembly-CSharp-firstpass" => PredefinedAssemblyType.AssemblyCSharpFirstPass,
                _ => null
            };
        }

        /// <summary>
        /// 获取实现了指定接口的所有运行时类型
        /// </summary>
        public static List<Type> GetRuntimeTypesWithInterface(Type interfaceType)
        {
            var targetTypes = new List<Type>();
            var assemblyTypes = GetRuntimeTypesMap();
            assemblyTypes.TryGetValue(PredefinedAssemblyType.AssemblyCSharp, out var assemblyCSharpTypes);
            AddTypesFromAssembly(assemblyCSharpTypes, interfaceType, targetTypes);
            assemblyTypes.TryGetValue(PredefinedAssemblyType.AssemblyCSharpFirstPass,
                out var assemblyCSharpFirstPassTypes);
            AddTypesFromAssembly(assemblyCSharpFirstPassTypes, interfaceType, targetTypes);
            return targetTypes;
        }

        #region Internal

        static void AddTypesFromAssembly(Type[] assemblyTypes, Type interfaceType, ICollection<Type> results)
        {
            if (assemblyTypes == null)
            {
                return;
            }

            for (var i = 0; i < assemblyTypes.Length; ++i)
            {
                var type = assemblyTypes[i];
                if (type != interfaceType && interfaceType.IsAssignableFrom(type))
                {
                    results.Add(type);
                }
            }
        }

        static Dictionary<PredefinedAssemblyType, Type[]> GetRuntimeTypesMap()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var assemblyTypes = new Dictionary<PredefinedAssemblyType, Type[]>();
            for (var i = 0; i < assemblies.Length; ++i)
            {
                var assemblyType = GetAssemblyType(assemblies[i].GetName().Name);
                if (assemblyType != null)
                {
                    assemblyTypes.Add((PredefinedAssemblyType)assemblyType, assemblies[i].GetTypes());
                }
            }

            return assemblyTypes;
        }

        #endregion
    }
}
