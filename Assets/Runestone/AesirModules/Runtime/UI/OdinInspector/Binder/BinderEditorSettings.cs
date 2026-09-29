#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using FilePathAttribute = UnityEditor.FilePathAttribute;

namespace Runestone.AesirModules
{
    /// <summary>
    /// Binder 编辑器持久化设置（ScriptableSingleton）：保存 partial 分部类模式的可选文件后缀列表、
    /// 默认后缀与最近使用的命名空间，供新建 BinderAssistant 的默认值与后缀下拉共用。
    /// </summary>
    /// <remarks>
    /// 落盘路径为项目根 <c>ScriptableSingleton/AesirModules/BinderEditorSettings.asset</c>（已被 .gitignore 覆盖）。
    /// <c>ScriptableSingleton</c> 走 Unity 原生序列化，故除 <see cref="FilePathAttribute" /> 外，
    /// 每个持久字段还需 <c>[SerializeField]</c>，且写盘必须调用基类 <c>Save(true)</c>——
    /// 仅 <c>EditorUtility.SetDirty</c> / <c>AssetDatabase.SaveAssets</c> 对本单例无效（它不是资产）。
    /// </remarks>
    // 遵循项目约定：ScriptableSingleton 设置资产统一放 ScriptableSingleton/ 前缀目录
    [FilePath("ScriptableSingleton/AesirModules/BinderEditorSettings.asset",
        FilePathAttribute.Location.ProjectFolder)]
    public class BinderEditorSettings : ScriptableSingleton<BinderEditorSettings>
    {
        const string FallbackNamespace = "Game";
        const string FallbackSuffix = ".designer.cs";
        static BinderEditorSettings _settings;

        [SerializeField]
        List<string> partialSuffixes = new List<string> { ".generated.cs", ".designer.cs" };

        [SerializeField]
        string defaultPartialSuffix = FallbackSuffix;

        [SerializeField]
        string lastNamespace = FallbackNamespace;

        /// <summary>
        /// 实例访问器。Unity 的 <c>ScriptableSingleton&lt;T&gt;</c> 暴露小写 <c>instance</c> 属性
        /// （双引擎一致），此处仍按大写/小写双探测并缓存，以免依赖单一拼写。
        /// </summary>
        public static BinderEditorSettings Settings
        {
            get
            {
                if (_settings == null)
                {
                    var baseType = typeof(ScriptableSingleton<BinderEditorSettings>);
                    var property =
                        baseType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static) ??
                        baseType.GetProperty("instance",
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                    _settings = (BinderEditorSettings)property.GetValue(null);
                }

                return _settings;
            }
        }

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
        /// 更新最近使用的命名空间（由生成流程调用，随即写盘）。
        /// </summary>
        public void SetLastNamespace(string targetNamespace)
        {
            if (string.IsNullOrEmpty(targetNamespace) || lastNamespace == targetNamespace)
            {
                return;
            }

            lastNamespace = targetNamespace;
            Save(true);
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
        /// 立即落盘（后缀列表编辑为低频操作，直接写入设置文件保证持久化）。
        /// </summary>
        public void Save()
        {
            EditorUtility.SetDirty(this);
            Save(true);
        }
    }
}
#endif
