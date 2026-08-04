using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirPackageExporter
{
    /// <summary>
    /// Aesir 包导出工具，支持单独导出或一次性导出全部三个包。
    /// </summary>
    public static class AesirPackageExporter
    {
        const string MenuRoot = "Tools/Aesir/Export Packages";

        static readonly PackageInfo[] Packages =
        {
            new("Aesir Architecture", "Assets/Runestone/AesirArchitecture"),
            new("Aesir Modules", "Assets/Runestone/AesirModules"),
            new("Aesir Inspector", "Assets/Runestone/AesirInspector")
        };

        /// <summary>
        /// 导出全部三个包到指定输出目录。
        /// </summary>
        [MenuItem(MenuRoot + "/Export All Packages", false, 0)]
        static void ExportAll()
        {
            var outputDir = GetOutputDirectory();
            if (string.IsNullOrEmpty(outputDir))
                return;

            var exported = new List<string>();
            foreach (var pkg in Packages)
            {
                if (!Directory.Exists(pkg.Path))
                {
                    Debug.LogWarning($"[Aesir Export] 跳过 {pkg.Name}：路径不存在 {pkg.Path}");
                    continue;
                }

                var packagePath = ExportPackage(pkg, outputDir);
                if (!string.IsNullOrEmpty(packagePath))
                    exported.Add(packagePath);
            }

            if (exported.Count > 0)
            {
                Debug.Log($"[Aesir Export] 导出完成，共 {exported.Count} 个包：\n{string.Join("\n", exported)}");
                EditorUtility.RevealInFinder(outputDir);
            }
        }

        /// <summary>
        /// 导出 Aesir Architecture 包。
        /// </summary>
        [MenuItem(MenuRoot + "/Export Aesir Architecture", false, 1)]
        static void ExportArchitecture()
        {
            ExportSingle(Packages[0]);
        }

        /// <summary>
        /// 导出 Aesir Modules 包。
        /// </summary>
        [MenuItem(MenuRoot + "/Export Aesir Modules", false, 2)]
        static void ExportModules()
        {
            ExportSingle(Packages[1]);
        }

        /// <summary>
        /// 导出 Aesir Inspector 包。
        /// </summary>
        [MenuItem(MenuRoot + "/Export Aesir Inspector", false, 3)]
        static void ExportInspector()
        {
            ExportSingle(Packages[2]);
        }

        static void ExportSingle(PackageInfo pkg)
        {
            if (!Directory.Exists(pkg.Path))
            {
                Debug.LogError($"[Aesir Export] 路径不存在：{pkg.Path}");
                return;
            }

            var outputDir = GetOutputDirectory();
            if (string.IsNullOrEmpty(outputDir))
                return;

            var packagePath = ExportPackage(pkg, outputDir);
            if (!string.IsNullOrEmpty(packagePath))
            {
                Debug.Log($"[Aesir Export] 导出完成：{packagePath}");
                EditorUtility.RevealInFinder(outputDir);
            }
        }

        static string ExportPackage(PackageInfo pkg, string outputDir)
        {
            try
            {
                var version = GetPackageVersion(pkg.Path);
                var fileName = $"{pkg.Name}-v{version}.unitypackage";
                var outputPath = Path.Combine(outputDir, fileName);

                var assets = CollectAssets(pkg.Path);
                if (assets.Count == 0)
                {
                    Debug.LogWarning($"[Aesir Export] {pkg.Name} 未找到可导出的资产");
                    return null;
                }

                EditorUtility.DisplayProgressBar("Exporting", $"Exporting {pkg.Name}...", 0.5f);
                AssetDatabase.ExportPackage(assets.ToArray(), outputPath,
                    ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);
                EditorUtility.ClearProgressBar();

                Debug.Log($"[Aesir Export] {pkg.Name} v{version} → {outputPath} ({assets.Count} assets)");
                return outputPath;
            }
            catch (Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError($"[Aesir Export] 导出 {pkg.Name} 失败：{e.Message}");
                return null;
            }
        }

        static List<string> CollectAssets(string packagePath)
        {
            var assets = new List<string>();
            var guids = AssetDatabase.FindAssets("", new[] { packagePath });
            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);

                // 排除 Library、Temp 等非包内容
                if (assetPath.StartsWith("Assets/Runestone/", StringComparison.Ordinal))
                    assets.Add(assetPath);
            }
            return assets.Distinct().OrderBy(p => p).ToList();
        }

        static string GetPackageVersion(string packagePath)
        {
            var pkgJsonPath = Path.Combine(packagePath, "package.json");
            if (!File.Exists(pkgJsonPath))
                return "unknown";

            var json = File.ReadAllText(pkgJsonPath);
            var start = json.IndexOf("\"version\"", StringComparison.Ordinal);
            if (start < 0)
                return "unknown";

            var colonIndex = json.IndexOf(':', start);
            var quoteStart = json.IndexOf('"', colonIndex + 1);
            var quoteEnd = json.IndexOf('"', quoteStart + 1);
            return quoteStart >= 0 && quoteEnd > quoteStart
                ? json.Substring(quoteStart + 1, quoteEnd - quoteStart - 1)
                : "unknown";
        }

        static string GetOutputDirectory()
        {
            var defaultDir = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "ExportedPackages");
            var outputPath = EditorUtility.SaveFolderPanel("选择导出目录", defaultDir, "");
            if (string.IsNullOrEmpty(outputPath))
                return null;

            Directory.CreateDirectory(outputPath);
            return outputPath;
        }

        readonly struct PackageInfo
        {
            public readonly string Name;
            public readonly string Path;

            public PackageInfo(string name, string path)
            {
                Name = name;
                Path = path;
            }
        }
    }
}
