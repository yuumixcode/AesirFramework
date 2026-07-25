// ----------------------------------------------------------------------------
// MIT License
// 
// Copyright (c) 2026 Runestone - Yuumix
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Runestone.AesirInspector
{
    /// <summary>
    /// 预定义程序集类型
    /// </summary>
    [Summary("预定义程序集类型")]
    public enum PredefinedAssemblyType
    {
        None = 0,

        /// <summary>
        /// 程序集 CSharp
        /// </summary>
        [Summary("程序集 CSharp")]
        AssemblyCSharp = 1,

        /// <summary>
        /// 程序集 CSharp-Editor
        /// </summary>
        [Summary("程序集 CSharp-Editor")]
        AssemblyCSharpEditor = 2,

        /// <summary>
        /// 程序集 CSharp-Editor-firstpass
        /// </summary>
        [Summary("程序集 CSharp-Editor-firstpass")]
        AssemblyCSharpEditorFirstPass = 3,

        /// <summary>
        /// 程序集 CSharp-firstpass
        /// </summary>
        [Summary("程序集 CSharp-firstpass")]
        AssemblyCSharpFirstPass = 4
    }

    /// <summary>
    /// 预定义程序集工具类，提供获取程序集类型及运行时类型的方法
    /// </summary>
    [Summary("预定义程序集工具类，提供获取程序集类型及运行时类型的方法")]
    public static class PredefinedAssemblyUtility
    {
        /// <summary>
        /// 根据程序集名称获取对应的预定义程序集类型
        /// </summary>
        [Summary("根据程序集名称获取对应的预定义程序集类型")]
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
        [Summary("获取实现了指定接口的所有运行时类型")]
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
