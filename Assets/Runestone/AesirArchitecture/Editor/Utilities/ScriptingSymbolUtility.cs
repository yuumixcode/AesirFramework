using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// 脚本宏定义工具，用于管理 <see cref="PlayerSettings" /> 中的 Scripting Define Symbols。
    /// <para>
    /// 参考 Odin Inspector 的 <c>EnsureOdinInspectorDefine</c> 实现，
    /// 遍历所有构建目标（排除 Unknown 和 Dedicated Server），提供幂等的宏定义符号添加/移除能力。
    /// </para>
    /// </summary>
    public static class ScriptingSymbolUtility
    {
        static NamedBuildTarget[] _validTargets;

        /// <summary>
        /// 获取所有有效构建目标（排除 Unknown 和 Dedicated Server），延迟初始化并缓存。
        /// </summary>
        static NamedBuildTarget[] ValidTargets
        {
            get
            {
                if (_validTargets != null)
                {
                    return _validTargets;
                }

                var list = new List<NamedBuildTarget>();
                var fields = typeof(NamedBuildTarget).GetFields(BindingFlags.Public | BindingFlags.Static);
                foreach (var field in fields)
                {
                    if (field.Name == "Unknown" || field.Name == "Server")
                    {
                        continue;
                    }

                    list.Add((NamedBuildTarget)field.GetValue(null));
                }

                _validTargets = list.ToArray();
                return _validTargets;
            }
        }

        /// <summary>
        /// 确保指定的宏定义符号存在于所有有效构建目标中（排除 Unknown 和 Dedicated Server）。若已存在则不重复添加。
        /// </summary>
        /// <param name="symbol">要添加的宏定义符号（如 <c>"AESIR_ARCHITECTURE"</c>）</param>
        public static void EnsureScriptingDefineSymbol(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
            {
                AesirArchitectureDebug.LogWarning(nameof(ScriptingSymbolUtility), "symbol 不能为空");
                return;
            }

            var added = false;
            foreach (var target in ValidTargets)
            {
                if (EnsureSymbolForTarget(target, symbol))
                {
                    added = true;
                }
            }

            if (added)
            {
                AesirArchitectureDebug.Log(nameof(ScriptingSymbolUtility), $"已添加宏定义符号: {symbol}");
            }
        }

        /// <summary>
        /// 确保指定的宏定义符号不存在于所有有效构建目标中。若不存在则不做任何操作。
        /// </summary>
        /// <param name="symbol">要移除的宏定义符号</param>
        public static void RemoveScriptingDefineSymbol(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
            {
                AesirArchitectureDebug.LogWarning(nameof(ScriptingSymbolUtility), "symbol 不能为空");
                return;
            }

            var removed = false;
            foreach (var target in ValidTargets)
            {
                if (RemoveSymbolForTarget(target, symbol))
                {
                    removed = true;
                }
            }

            if (removed)
            {
                AesirArchitectureDebug.Log(nameof(ScriptingSymbolUtility), $"已移除宏定义符号: {symbol}");
            }
        }

        /// <summary>
        /// 检查指定的宏定义符号是否已存在于当前构建目标中。
        /// </summary>
        public static bool HasScriptingDefineSymbol(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
            {
                return false;
            }

            var target = NamedBuildTarget.FromBuildTargetGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup);
            return ContainsSymbol(PlayerSettings.GetScriptingDefineSymbols(target), symbol);
        }

        static bool EnsureSymbolForTarget(NamedBuildTarget target, string symbol)
        {
            var current = PlayerSettings.GetScriptingDefineSymbols(target);
            if (ContainsSymbol(current, symbol))
            {
                return false;
            }

            var newSymbols = string.IsNullOrEmpty(current) ? symbol : current + ";" + symbol;

            PlayerSettings.SetScriptingDefineSymbols(target, newSymbols);
            return true;
        }

        static bool RemoveSymbolForTarget(NamedBuildTarget target, string symbol)
        {
            var current = PlayerSettings.GetScriptingDefineSymbols(target);
            if (!ContainsSymbol(current, symbol))
            {
                return false;
            }

            var symbols = current.Split(';');
            var result = new List<string>(symbols.Length);
            foreach (var s in symbols)
            {
                var trimmed = s.Trim();
                if (trimmed != symbol)
                {
                    result.Add(trimmed);
                }
            }

            PlayerSettings.SetScriptingDefineSymbols(target, string.Join(";", result.ToArray()));
            return true;
        }

        static bool ContainsSymbol(string symbols, string symbol)
        {
            if (string.IsNullOrEmpty(symbols))
            {
                return false;
            }

            var parts = symbols.Split(';');
            foreach (var part in parts)
            {
                if (part.Trim() == symbol)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
