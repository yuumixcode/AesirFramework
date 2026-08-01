using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Runestone.AesirInspector.Editor
{
    /// <summary>
    /// Aesir Inspector 的安装方式枚举。
    /// </summary>
    public enum AesirInstallMode
    {
        /// <summary>
        /// 检测结果未知（检测过程中发生了异常）。
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// 通过 Unity Package Manager (UPM) 安装，文件位于 Packages/ 目录。
        /// </summary>
        Upm = 1,

        /// <summary>
        /// 通过 Asset Store 导入或手动放置，文件位于 Assets/ 目录（含 Git 子模块方式）。
        /// </summary>
        AssetFolder = 2
    }

    /// <summary>
    /// 在编辑器加载时检测 Aesir Inspector 的安装方式，并将结果缓存为公开静态属性。
    /// </summary>
    [InitializeOnLoad]
    public static class AesirInspectorInstallationChecker
    {
        static AesirInspectorInstallationChecker() => Detect();

        /// <summary>
        /// 当前的安装方式。
        /// </summary>
        public static AesirInstallMode InstallMode { get; private set; } = AesirInstallMode.Unknown;

        /// <summary>
        /// UPM 安装时的具体来源（Registry、Git、LocalPath 等）；非 UPM 时为 PackageSource.Unknown。
        /// </summary>
        public static PackageSource UpmPackageSource { get; private set; } = PackageSource.Unknown;

        /// <summary>
        /// 是否通过 UPM 安装（文件位于 Packages/ 目录）。
        /// </summary>
        public static bool IsUpm => InstallMode == AesirInstallMode.Upm;

        /// <summary>
        /// 是否安装在 Assets/ 目录中（Asset Store 导入或 Git 子模块）。
        /// </summary>
        public static bool IsAssetFolder => InstallMode == AesirInstallMode.AssetFolder;

        #region Internal

        static void Detect()
        {
            try
            {
                PerformDetection();
            }
            catch (Exception ex)
            {
                InstallMode = AesirInstallMode.Unknown;
                UpmPackageSource = PackageSource.Unknown;
                Debug.LogWarning($"[Aesir Inspector] 安装方式检测失败：{ex.Message}");
            }
        }

        static void PerformDetection()
        {
            // PackageInfo.FindForAssembly returns non-null only when the assembly belongs to a package registered with UPM (files under Packages/).
            // A null result means the assembly lives in Assets/ — Asset Store or submodule.
            var info = PackageInfo.FindForAssembly(Assembly.GetExecutingAssembly());
            if (info == null)
            {
                InstallMode = AesirInstallMode.AssetFolder;
                // Debug.Log("[Aesir Inspector] 安装方式：AssetFolder（Assets/ 目录）");
                return;
            }

            InstallMode = AesirInstallMode.Upm;
            UpmPackageSource = info.source;
            // Debug.Log($"[Aesir Inspector] 安装方式：UPM | 来源：{info.source} | 版本：{info.version} | 路径：{info.resolvedPath}");
        }

        #endregion
    }
}
