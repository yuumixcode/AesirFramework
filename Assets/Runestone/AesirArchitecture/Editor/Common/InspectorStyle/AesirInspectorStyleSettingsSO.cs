using UnityEditor;
using UnityEngine;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// Aesir 编辑器偏好设置的 Editor 专属 ScriptableObject —— 只在 Editor 阶段读取，不进玩家构建。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 与场景模块的 <c>SceneModuleConfigSO</c>（<c>Resources</c> 下、随构建分发的运行时配置）成对：
    /// 本资产保存"编辑器怎么显示"的偏好，运行时配置保存"游戏怎么加载场景"的数据，两者职责不交叉。
    /// </para>
    /// <para>
    /// 用 <see cref="ScriptableSingleton{T}" /> 而非 <c>EditorPrefs</c>：偏好随工程走（shell 的
    /// <c>ScriptableSingleton/</c> 目录已 gitignore），换机 / 多人协作时不会各持一套显示样式。
    /// </para>
    /// </remarks>
    [FilePath("ScriptableSingleton/AesirFramework/AesirInspectorStyleSettings.asset",
        FilePathAttribute.Location.ProjectFolder)]
    internal sealed class AesirInspectorStyleSettingsSO : ScriptableSingleton<AesirInspectorStyleSettingsSO>
    {
        [SerializeField] bool useOdinInspectorStyle = true;

        /// <summary>是否使用 Odin 样式（Aesir 自己的 Odin 集成是否生效）。</summary>
        internal bool UseOdin
        {
            get => useOdinInspectorStyle;
            set
            {
                if (useOdinInspectorStyle == value)
                {
                    return;
                }

                useOdinInspectorStyle = value;
                Save(true);
            }
        }
    }
}
