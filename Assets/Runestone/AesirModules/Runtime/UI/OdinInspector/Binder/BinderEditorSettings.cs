#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirModules
{
    /// <summary>
    /// Binder 编辑器持久化设置（ScriptableSingleton，随编辑器会话持久存储）。
    /// <para>
    /// 保存 partial 分部类模式的可选文件后缀列表、默认后缀与最近使用的命名空间，
    /// 供新建 BinderAssistant 的默认值与后缀下拉共用。
    /// </para>
    /// </summary>
    public class BinderEditorSettings : ScriptableSingleton<BinderEditorSettings>
    {
        static BinderEditorSettings _settings;

        /// <summary>
        /// 实例访问器。标准 Unity 的 ScriptableSingleton 暴露大写 <c>Instance</c>，
        /// 团结引擎为小写 <c>instance</c>，此处经反射做双引擎兼容并缓存。
        /// </summary>
        public static BinderEditorSettings Settings
        {
            get
            {
                if (_settings == null)
                {
                    var baseType = typeof(ScriptableSingleton<BinderEditorSettings>);
                    var property = baseType.GetProperty("Instance",
                                       BindingFlags.Public | BindingFlags.Static)
                                   ?? baseType.GetProperty("instance",
                                       BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    _settings = (BinderEditorSettings)property.GetValue(null);
                }

                return _settings;
            }
        }

        const string FallbackNamespace = "Game";
        const string FallbackSuffix = ".designer.cs";

        [SerializeField]
        List<string> partialSuffixes = new List<string> { ".generated.cs", ".designer.cs" };

        [SerializeField]
        string defaultPartialSuffix = FallbackSuffix;

        [SerializeField]
        string lastNamespace = FallbackNamespace;

        /// <summary>
        /// partial 分部类模式下自动维护文件的可选后缀列表（含扩展名，如 <c>.designer.cs</c>）。
        /// </summary>
        public List<string> PartialSuffixes => partialSuffixes;

        /// <summary>
        /// 新建 BinderAssistant 时默认选中的自动维护文件后缀。
        /// </summary>
        public string DefaultPartialSuffix => defaultPartialSuffix;

        /// <summary>
        /// 最近一次成功生成脚本时使用的命名空间，作为新建 BinderAssistant 的命名空间默认值。
        /// </summary>
        public string LastNamespace => lastNamespace;

        /// <summary>
        /// 更新最近使用的命名空间（由生成流程调用，随后统一 SaveAssets 落盘）。
        /// </summary>
        public void SetLastNamespace(string targetNamespace)
        {
            if (string.IsNullOrEmpty(targetNamespace) || lastNamespace == targetNamespace)
            {
                return;
            }

            lastNamespace = targetNamespace;
            EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// 更新默认选中的自动维护文件后缀。
        /// </summary>
        public void SetDefaultPartialSuffix(string suffix)
        {
            if (string.IsNullOrEmpty(suffix) || defaultPartialSuffix == suffix)
            {
                return;
            }

            defaultPartialSuffix = suffix;
            Save();
        }

        /// <summary>
        /// 用编辑器内编辑过的列表替换可选后缀列表并落盘。
        /// </summary>
        public void SetPartialSuffixes(List<string> suffixes)
        {
            if (suffixes == null || suffixes.Count == 0)
            {
                return;
            }

            partialSuffixes = suffixes;
            Save();
        }

        /// <summary>
        /// 立即落盘（后缀列表编辑为低频操作，直接 SaveAssets 保证持久化）。
        /// </summary>
        public void Save()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    }
}
#endif
