using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Runestone.AesirInspector.Editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    public class ExtensionPackageManagerWindow : OdinEditorWindow
    {
        static readonly BilingualData WindowName = new BilingualData("扩展包管理器", "Extension Package Manager");

        [PropertyOrder(-1000)]
        public BilingualHeaderControl bilingualHeaderControl;

        [PropertyOrder(-800)]
        [ListDrawerSettings(ShowFoldout = false, HideAddButton = true, HideRemoveButton = true,
            OnTitleBarGUI = nameof(DrawRefreshCardListButton))]
        [BilingualText("卡片列表", "Card List")]
        public List<ExtensionPackageCard> cardList;

        Texture2D _refreshIcon;

        [MenuItem(AesirInspectorMenuItems.ExtensionPackageManager, false,
            AesirInspectorMenuItems.ExtensionPackageManagerOrder)]
        public static void Open()
        {
            var window = GetWindow<ExtensionPackageManagerWindow>();
            window.position = GUIHelper.GetEditorWindowRect().AlignCenterXY(800f, 600f);
            window.minSize = new Vector2(720f, 520f);
            window.Show();
            _ = window.UpdatePackageInstallationStates();
            AesirInspectorDebug.Info("打开 Extension Package Manager 窗口，检测预设包的安装状态。");
        }

        #region Event Functions

        protected override void OnEnable()
        {
            base.OnEnable();
            bilingualHeaderControl = new BilingualHeaderControl("扩展包管理器 [待修复中]", "Extension Package Manager [WIP]",
                "快捷安装推荐的 Aesir 系列和其他常用开源 Unity Packages，基于 Git URL 方式",
                "Install recommended Aesir series and open-source Unity Packages quickly, Based On Git URL.");
            WindowPadding = new Vector4(10f, 10f, 10f, 10f);
            if (_refreshIcon == null)
            {
                _refreshIcon =
                    SdfIcons.CreateTransparentIconTexture(SdfIconType.ArrowClockwise, Color.white, 24, 24, 0);
            }

            UpdateTitle();
            AesirInspectorLanguageSettingsSO.LanguageChanged -= LanguageChanged;
            AesirInspectorLanguageSettingsSO.LanguageChanged += LanguageChanged;
            PackageManagerEditorUtility.OnPackagesChanged -= OnPackagesChanged;
            PackageManagerEditorUtility.OnPackagesChanged += OnPackagesChanged;
            Events.registeredPackages -= OnRegisteredPackagesEditor;
            Events.registeredPackages += OnRegisteredPackagesEditor;

            cardList = new List<ExtensionPackageCard>
            {
                ExtensionPackageCard.AesirToolkitCore,
                ExtensionPackageCard.AesirToolkitModules,
                ExtensionPackageCard.GitAmendImprovedTimers
            };
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            AesirInspectorLanguageSettingsSO.LanguageChanged -= LanguageChanged;
            PackageManagerEditorUtility.OnPackagesChanged -= OnPackagesChanged;
            Events.registeredPackages -= OnRegisteredPackagesEditor;
        }

        #endregion

        #region Internal

        void DrawRefreshCardListButton()
        {
            var text = new BilingualData("检测安装状态", "Check Packages Installed");
            var content = new GUIContent(text, _refreshIcon);
            if (SirenixEditorGUI.ToolbarButton(content))
            {
                _ = UpdatePackageInstallationStates();
            }
        }

        async Task UpdatePackageInstallationStates()
        {
            await PackageManagerEditorUtility.ListPackagesAsyncOffline();
            if (cardList == null)
            {
                return;
            }

            foreach (var card in cardList)
            {
                var isInstalled = PackageManagerEditorUtility.IsPackageInstalled(card.PackageName);
                card.State = isInstalled
                    ? ExtensionPackageCard.PackageState.Installed
                    : ExtensionPackageCard.PackageState.NotInstalled;
            }
        }

        void OnPackagesChanged()
        {
            _ = UpdatePackageInstallationStates();
            Repaint();
        }

        void OnRegisteredPackagesEditor(PackageRegistrationEventArgs args)
        {
            _ = HandleRegisteredPackagesEditorAsync(args);
        }

        async Task HandleRegisteredPackagesEditorAsync(PackageRegistrationEventArgs args)
        {
            try
            {
                while (EditorApplication.isCompiling || EditorApplication.isUpdating)
                {
                    await Task.Delay(100);
                }

                _ = UpdatePackageInstallationStates();
                Repaint();
            }
            catch (Exception e)
            {
                AesirInspectorDebug.Error($"处理包注册变更回调时发生异常：{e}");
            }
        }

        void LanguageChanged()
        {
            UpdateTitle();
            Repaint();
        }

        void UpdateTitle()
        {
            titleContent = new GUIContent(WindowName);
        }

        #endregion
    }
}
