using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// 脚本宏定义工具，用于管理 <see cref="PlayerSettings" /> 中的 Scripting Define Symbols。
    /// <para>
    /// 遍历所有构建目标（排除 Unknown 和 Dedicated Server），提供幂等的宏定义符号添加/移除能力。
    /// </para>
    /// </summary>
    public static class ScriptingSymbolEditorUtility
    {
        static NamedBuildTarget[] _validTargets;

        /// <summary>
        /// 获取所有有效构建目标（排除 Unknown 和 Dedicated Server），延迟初始化并缓存。
        /// </summary>
        /// <remarks>
        /// 通过反射获取 <see cref="NamedBuildTarget" /> 类型的所有公共静态字段，
        /// 排除 <c>Unknown</c> 和 <c>Server</c>（即 Dedicated Server），
        /// 因为这两者不适用于常规的构建目标宏管理场景。
        /// 结果在首次访问后缓存，避免重复反射开销。
        /// </remarks>
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
        /// <remarks>
        /// 此方法是幂等的：若符号在某个构建目标中已存在，则跳过该目标不会重复添加，
        /// 避免产生重复的分号分隔条目。
        /// </remarks>
        public static void EnsureScriptingDefineSymbol(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
            {
                AesirArchitectureDebug.LogWarning(nameof(ScriptingSymbolEditorUtility), "symbol 不能为空");
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
                AesirArchitectureDebug.Log(nameof(ScriptingSymbolEditorUtility), $"已添加宏定义符号: {symbol}");
            }
        }

        /// <summary>
        /// 确保指定的宏定义符号不存在于所有有效构建目标中。若不存在则不做任何操作。
        /// </summary>
        /// <param name="symbol">要移除的宏定义符号</param>
        /// <remarks>
        /// 此方法是幂等的：若符号在某个构建目标中不存在，则跳过该目标不会产生错误，
        /// 仅在实际移除了符号时才记录日志。
        /// </remarks>
        public static void RemoveScriptingDefineSymbol(string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
            {
                AesirArchitectureDebug.LogWarning(nameof(ScriptingSymbolEditorUtility), "symbol 不能为空");
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
                AesirArchitectureDebug.Log(nameof(ScriptingSymbolEditorUtility), $"已移除宏定义符号: {symbol}");
            }
        }

        /// <summary>
        /// 检查指定的宏定义符号是否已存在于当前构建目标中。
        /// </summary>
        /// <param name="symbol">要检查的宏定义符号</param>
        /// <returns>若符号存在于当前选中的构建目标中则返回 <c>true</c>，否则返回 <c>false</c></returns>
        /// <remarks>
        /// 仅检查当前在 Unity Editor 中选中的构建目标组（<see cref="EditorUserBuildSettings.selectedBuildTargetGroup" />），
        /// 不遍历所有构建目标。如需检查全部目标，请遍历调用各目标的查询方法。
        /// </remarks>
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

        /// <summary>
        /// 确保指定构建目标中存在指定宏定义符号，若已存在则不重复添加。
        /// </summary>
        /// <returns>若实际添加了符号则返回 <c>true</c>，若已存在则返回 <c>false</c></returns>
        static bool EnsureSymbolForTarget(NamedBuildTarget target, string symbol)
        {
            var current = PlayerSettings.GetScriptingDefineSymbols(target);
            var updated = AddSymbol(current, symbol);
            if (updated == current)
            {
                return false;
            }

            PlayerSettings.SetScriptingDefineSymbols(target, updated);
            return true;
        }

        /// <summary>
        /// 从指定构建目标中移除宏定义符号，若不存在则不做任何操作。
        /// </summary>
        /// <returns>若实际移除了符号则返回 <c>true</c>，若不存在则返回 <c>false</c></returns>
        static bool RemoveSymbolForTarget(NamedBuildTarget target, string symbol)
        {
            var current = PlayerSettings.GetScriptingDefineSymbols(target);
            var updated = RemoveSymbol(current, symbol);
            if (updated == current)
            {
                return false;
            }

            PlayerSettings.SetScriptingDefineSymbols(target, updated);
            return true;
        }

        /// <summary>
        /// 检查分号分隔的符号字符串中是否包含指定宏定义符号。
        /// </summary>
        /// <remarks>
        /// 纯字符串判定，不触碰 <see cref="PlayerSettings" /> —— 写入宏定义符号会触发
        /// "Define symbols changed" 全量脚本重编译与域重载，测试期间发生会中断 Test Runner，
        /// 故与 <see cref="PlayerSettings" /> 解耦的这段语义由纯函数承载。
        /// </remarks>
        /// <returns>若包含则返回 <c>true</c>，否则返回 <c>false</c></returns>
        internal static bool ContainsSymbol(string symbols, string symbol)
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

        /// <summary>
        /// 在分号分隔的符号字符串末尾追加宏定义符号（幂等）。
        /// </summary>
        /// <remarks>
        /// 与此前内联实现等价：已存在（忽略两侧空白）时原样返回 <paramref name="symbols" />，
        /// 调用方据此判断"是否需要写回 <see cref="PlayerSettings" />"。
        /// </remarks>
        /// <returns>追加后的符号字符串；符号已存在时返回原字符串</returns>
        internal static string AddSymbol(string symbols, string symbol)
        {
            if (ContainsSymbol(symbols, symbol))
            {
                return symbols;
            }

            return string.IsNullOrEmpty(symbols) ? symbol : symbols + ";" + symbol;
        }

        /// <summary>
        /// 从分号分隔的符号字符串中移除宏定义符号，其余条目两侧空白一并清理。
        /// </summary>
        /// <remarks>
        /// 符号不存在时原样返回 <paramref name="symbols" />（调用方据此跳过写回）；
        /// 移除最后一项时返回空字符串（与"全部移除"语义一致，而非 <c>null</c>）。
        /// </remarks>
        /// <returns>移除后的符号字符串；符号不存在时返回原字符串</returns>
        internal static string RemoveSymbol(string symbols, string symbol)
        {
            if (!ContainsSymbol(symbols, symbol))
            {
                return symbols;
            }

            var parts = symbols.Split(';');
            var result = new List<string>(parts.Length);
            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                if (trimmed != symbol)
                {
                    result.Add(trimmed);
                }
            }

            return string.Join(";", result.ToArray());
        }
    }
}
