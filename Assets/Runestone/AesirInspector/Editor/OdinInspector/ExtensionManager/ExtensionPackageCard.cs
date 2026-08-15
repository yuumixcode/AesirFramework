using System;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [Serializable]
    public class ExtensionPackageCard
    {
        public enum PackageState
        {
            None = 0,

            Installed = 1,

            NotInstalled = 2
        }

        public static ExtensionPackageCard AesirToolkitCore = new ExtensionPackageCard(
            "com.runestone.aesir-toolkit",
            "Aesir Toolkit 核心，基于 Odin Inspector 的编辑器扩展基础设施，提供双语 Inspector、工具基类等。",
            "https://github.com/yuumixcode/AesirToolkit", "Runestone Yuumix",
            "https://github.com/yuumixcode/AesirToolkit.git?path=/Assets/Runestone/AesirToolkit/Core#main");

        public static ExtensionPackageCard AesirToolkitModules = new ExtensionPackageCard(
            "com.runestone.aesir-toolkit-modules", "Aesir Toolkit 模块，包括多种功能模块，低侵入性模块。",
            "https://github.com/yuumixcode/AesirToolkit", "Runestone Yuumix",
            "https://github.com/yuumixcode/AesirToolkit.git?path=/Assets/Runestone/AesirToolkit/Modules#main");

        public static ExtensionPackageCard GitAmendImprovedTimers = new ExtensionPackageCard(
            "com.gitamend.improvedtimers", "Unity-Improved-Timers",
            "https://github.com/adammyhre/Unity-Improved-Timers", "Git-Amend",
            "https://github.com/adammyhre/Unity-Improved-Timers.git");

        [BoxGroup("A", ShowLabel = false)]
        [PropertyOrder(-900)]
        [HorizontalGroup("A/C")]
        [SerializeField]
        [HideLabel]
        [DisplayAsString(16)]
        string packageName;

        [BoxGroup("A", ShowLabel = false)]
        [PropertySpace]
        [SerializeField]
        [HideLabel]
        [DisplayAsString(14)]
        string description;

        [BoxGroup("A", ShowLabel = false)]
        [SerializeField]
        [BilingualTitle("Git 链接", "Git URL")]
        [DisplayAsString(13)]
        [HideLabel]
        string gitUrl;

        [BoxGroup("A", ShowLabel = false)]
        [SerializeField]
        [BilingualTitle("仓库链接", "Repository URL")]
        [DisplayAsString(13)]
        [HideLabel]
        string repositoryUrl;

        public ExtensionPackageCard(string packageName,
            string description,
            string repositoryUrl,
            string author,
            string gitUrl)
        {
            this.packageName = packageName;
            this.description = description;
            this.repositoryUrl = repositoryUrl;
            Author = author;
            this.gitUrl = gitUrl;
        }

        public PackageState State { get; set; } = PackageState.NotInstalled;

        public string PackageName => packageName;

        public string Description => description;

        public string RepositoryUrl => repositoryUrl;

        public string Author { get; }

        public string GitUrl => gitUrl;

        bool PackageManagerIsBusy => PackageManagerEditorUtility.IsBusy;
        bool IsInstalled => State == PackageState.Installed;

        [BoxGroup("A", ShowLabel = false)]
        [HorizontalGroup("A/C", 50)]
        [PropertyOrder(-1000)]
        [OnInspectorGUI]
        void DrawPackageState()
        {
            var rect = EditorGUILayout.GetControlRect(false, 20);
            var leftStateRect = rect.AlignLeft(20f);
            var stateColor = GetStateColor(State);
            SirenixEditorGUI.DrawRoundRect(leftStateRect, stateColor, 10f);
        }

        [PropertyOrder(-800)]
        [BoxGroup("A", ShowLabel = false)]
        [HorizontalGroup("A/C")]
        [OnInspectorGUI]
        void DrawAuthor()
        {
            var rect = EditorGUILayout.GetControlRect(false, 20).AlignRight(200);
            EditorGUI.LabelField(rect, "By: " + Author, SirenixGUIStyles.LabelCentered);
        }

        [PropertySpace]
        [BoxGroup("A", ShowLabel = false)]
        [HorizontalGroup("A/B")]
        [BilingualButton("打开仓库", "Open Repository")]
        void OpenRepositoryUrl() => Application.OpenURL(repositoryUrl);

        [PropertySpace]
        [BoxGroup("A", ShowLabel = false)]
        [HorizontalGroup("A/B")]
        [BilingualButton("检测是否安装", "Check Package Installed", drawResult: false)]
        [DisableIf(nameof(PackageManagerIsBusy))]
        public async Task CheckPackageState()
        {
            await PackageManagerEditorUtility.ListPackagesAsyncOffline();
            if (PackageManagerEditorUtility.IsPackageInstalled(packageName))
            {
                AesirInspectorDebug.Info("Package 已安装");
                State = PackageState.Installed;
            }
            else
            {
                AesirInspectorDebug.Info("Package 未安装");
                State = PackageState.NotInstalled;
            }
        }

        [PropertySpace]
        [BoxGroup("A", ShowLabel = false)]
        [HorizontalGroup("A/B")]
        [BilingualButton("安装包", "Install")]
        [HideIf(nameof(IsInstalled))]
        [DisableIf(nameof(PackageManagerIsBusy))]
        void Install()
        {
            AesirInspectorDebug.Info($"尝试安装 {packageName}");
            PackageManagerEditorUtility.InstallPackageAsyncFromCard(this);
        }

        [PropertySpace]
        [BoxGroup("A", ShowLabel = false)]
        [HorizontalGroup("A/B")]
        [BilingualButton("移除包", "Remove")]
        [ShowIf(nameof(IsInstalled))]
        [DisableIf(nameof(PackageManagerIsBusy))]
        [GUIColor("red")]
        void Remove()
        {
            if (EditorUtility.DisplayDialog("删除确认", "确认移除包：" + packageName, "确认", "取消"))
            {
                PackageManagerEditorUtility.RemovePackageAsyncFromCard(this);
            }
        }

        public bool HasValidGitUrl() => !string.IsNullOrEmpty(gitUrl) && gitUrl.Contains(".git");

        #region Internal

        Color GetStateColor(PackageState state)
        {
            return state switch
            {
                PackageState.Installed => new Color(0.2f, 0.7f, 0.3f),
                PackageState.NotInstalled => new Color(0.75f, 0.2f, 0.2f),
                _ => new Color(0.6f, 0.6f, 0.6f)
            };
        }

        #endregion
    }
}
