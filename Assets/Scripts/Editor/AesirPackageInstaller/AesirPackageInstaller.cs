using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirPackageInstaller
{
    /// <summary>
    /// Aesir 包安装器 — 将两个 Aesir 包安装到 Assets/ 目录（非 Unity Package Manager）。
    /// <para>使用方式：将此文件放入目标项目的 Assets/Editor/ 目录，然后通过 Tools → Aesir → Package Installer 打开。</para>
    /// <para>支持本地目录安装（测试用）和 Git 远程安装（正式）。</para>
    /// </summary>
    public class AesirPackageInstallerWindow : EditorWindow
    {
        #region 常量

        const string MenuPath = "Tools/Aesir/Package Installer";
        const string GitRepoUrl = "https://github.com/yuumixcode/AesirFramework.git";
        const string DefaultInstallRoot = "Assets/Runestone";
        const string TempStagingDir = "Temp/AesirInstallTest";

        /// <summary>复制时排除的目录名（不区分大小写）</summary>
        static readonly HashSet<string> ExcludeDirNames = new(StringComparer.OrdinalIgnoreCase)
        {
            ".git", "Library", "Temp", "obj", "Logs", "UserSettings",
            ".codely", ".codely-cli", ".codely.packages", ".idea", ".vs",
            ".vscode", ".junie", ".github", "Build", "Builds"
        };

        /// <summary>复制时排除的文件名（不区分大小写）</summary>
        static readonly HashSet<string> ExcludeFileNames = new(StringComparer.OrdinalIgnoreCase)
        {
            ".DS_Store", ".gitignore", ".gitattributes"
        };

        /// <summary>复制时排除的文件扩展名（不区分大小写）</summary>
        static readonly HashSet<string> ExcludeFileExts = new(StringComparer.OrdinalIgnoreCase)
        {
            ".csproj", ".sln", ".unitypackage", ".apk", ".aab", ".app",
            ".tmp", ".bak"
        };

        #endregion

        #region 包定义

        readonly struct PackageDef
        {
            public readonly string DisplayName;
            public readonly string PackageId;
            public readonly string Version;
            public readonly string DirName;
            public readonly string[] AsmdefRelPaths;
            public readonly string KeyAssemblyName;
            public readonly string KeyTypeFullName;
            public readonly string[] DepPackageIds;
            public readonly bool RequiresOdin;

            public PackageDef(string displayName, string packageId, string version,
                string dirName, string[] asmdefRelPaths, string keyAssemblyName,
                string keyTypeFullName, string[] depPackageIds, bool requiresOdin)
            {
                DisplayName = displayName;
                PackageId = packageId;
                Version = version;
                DirName = dirName;
                AsmdefRelPaths = asmdefRelPaths;
                KeyAssemblyName = keyAssemblyName;
                KeyTypeFullName = keyTypeFullName;
                DepPackageIds = depPackageIds;
                RequiresOdin = requiresOdin;
            }
        }

        static readonly PackageDef[] s_packages =
        {
            new("Aesir Architecture", "cn.runestone.aesir.architecture", "0.14.0",
                "AesirArchitecture",
                new[] { "Runtime/Runestone.AesirArchitecture.asmdef" },
                "Runestone.AesirArchitecture",
                "Runestone.AesirArchitecture.AesirArchitecture",
                Array.Empty<string>(), false),

            new("Aesir Modules", "cn.runestone.aesir.modules", "0.14.0",
                "AesirModules",
                new[] { "Runtime/Runestone.AesirModules.asmdef" },
                "Runestone.AesirModules",
                "Runestone.AesirModules.AesirModules",
                new[] { "cn.runestone.aesir.architecture" }, false),
        };

        #endregion

        #region UI 状态

        enum SourceMode { LocalDirectory, GitRemote }
        enum PkgStatus { NotChecked, NotInstalled, Installed, Failed }

        SourceMode _sourceMode = SourceMode.LocalDirectory;
        string _localPath = "";
        string _gitUrl = GitRepoUrl;
        string _installRoot = DefaultInstallRoot;
        PkgStatus[] _statuses;
        string[] _statusDetails;
        Vector2 _logScroll;
        readonly StringBuilder _log = new();
        bool _busy;

        #endregion

        #region 菜单与初始化

        [MenuItem(MenuPath)]
        static void Open()
        {
            var window = GetWindow<AesirPackageInstallerWindow>("Aesir Package Installer");
            window.minSize = new Vector2(580, 600);
        }

        void OnEnable()
        {
            _statuses = new PkgStatus[s_packages.Length];
            _statusDetails = new string[s_packages.Length];
            for (int i = 0; i < _statuses.Length; i++)
            {
                _statuses[i] = PkgStatus.NotChecked;
                _statusDetails[i] = "";
            }
            _localPath = AutoDetectLocalSource();
            if (!string.IsNullOrEmpty(_localPath))
                Log($"[自动检测] 本地源路径: {_localPath}");
        }

        #endregion

        #region UI

        void OnGUI()
        {
            DrawSourceSection();
            DrawPackageList();
            DrawButtons();
            DrawLog();
        }

        void DrawSourceSection()
        {
            EditorGUILayout.LabelField("安装源", EditorStyles.boldLabel);

            _sourceMode = (SourceMode)GUILayout.SelectionGrid(
                (int)_sourceMode, new[] { "本地目录（测试用）", "Git 远程" }, 2);

            if (_sourceMode == SourceMode.LocalDirectory)
            {
                EditorGUILayout.BeginHorizontal();
                _localPath = EditorGUILayout.TextField("本地路径", _localPath);
                if (GUILayout.Button("浏览", GUILayout.Width(50)))
                {
                    var path = EditorUtility.OpenFolderPanel("选择 Aesir 仓库根目录", _localPath, "");
                    if (!string.IsNullOrEmpty(path))
                        _localPath = path;
                }
                EditorGUILayout.EndHorizontal();

                if (!string.IsNullOrEmpty(_localPath) && !Directory.Exists(_localPath))
                    EditorGUILayout.HelpBox("路径不存在", MessageType.Warning);
            }
            else
            {
                _gitUrl = EditorGUILayout.TextField("Git URL", _gitUrl);
                EditorGUILayout.HelpBox(
                    "Git 模式将执行 git clone 到临时目录后复制包文件。\n" +
                    "当前为测试阶段，建议使用本地目录模式。", MessageType.Info);
            }

            _installRoot = EditorGUILayout.TextField("安装目标", _installRoot);
            EditorGUILayout.Space();
        }

        void DrawPackageList()
        {
            EditorGUILayout.LabelField("包状态", EditorStyles.boldLabel);

            for (int i = 0; i < s_packages.Length; i++)
            {
                var pkg = s_packages[i];
                var status = _statuses[i];
                var detail = _statusDetails[i];

                EditorGUILayout.BeginHorizontal("box");

                var (icon, color) = status switch
                {
                    PkgStatus.Installed => ("✓", Color.green),
                    PkgStatus.Failed => ("✗", Color.red),
                    PkgStatus.NotInstalled => ("○", Color.yellow),
                    _ => ("?", Color.gray)
                };

                var oldColor = GUI.color;
                GUI.color = color;
                EditorGUILayout.LabelField(icon, GUI.skin.label, GUILayout.Width(24));
                GUI.color = oldColor;

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField($"{pkg.DisplayName}  v{pkg.Version}", EditorStyles.boldLabel);
                if (!string.IsNullOrEmpty(detail))
                    EditorGUILayout.LabelField(detail, EditorStyles.miniLabel);
                EditorGUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
        }

        void DrawButtons()
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(_busy);
            if (GUILayout.Button("验证安装", GUILayout.Height(32)))
                VerifyAll();
            if (GUILayout.Button("测试安装", GUILayout.Height(32)))
                TestInstall();
            if (GUILayout.Button("安装全部", GUILayout.Height(32)))
                InstallAll();
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();

            if (_busy)
            {
                EditorGUILayout.Space();
                var rect = GUILayoutUtility.GetRect(0, 24);
                EditorGUI.ProgressBar(rect, 0.5f, "正在执行...");
            }

            EditorGUILayout.Space();
        }

        void DrawLog()
        {
            EditorGUILayout.LabelField("日志", EditorStyles.boldLabel);

            _logScroll = EditorGUILayout.BeginScrollView(_logScroll);
            var style = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = true,
                richText = false,
                stretchHeight = true
            };
            EditorGUILayout.TextArea(_log.ToString(), style, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        #endregion

        #region 验证安装

        void VerifyAll()
        {
            _busy = true;
            Log("════════════════════════════════════════");
            Log("开始验证安装状态");
            Log("════════════════════════════════════════");

            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? "";
            var installPath = Path.Combine(projectRoot, _installRoot);

            bool allOk = true;

            for (int i = 0; i < s_packages.Length; i++)
            {
                var pkg = s_packages[i];
                Log($"--- {pkg.DisplayName} ---");

                var (status, detail) = VerifyPackage(pkg, installPath);
                _statuses[i] = status;
                _statusDetails[i] = detail;

                if (status != PkgStatus.Installed)
                    allOk = false;

                Log($"  结果: {status} — {detail}");
            }

            Log(allOk ? "✅ 全部验证通过" : "❌ 存在未通过项，详见上方日志");
            Log("════════════════════════════════════════\n");
            _busy = false;
        }

        (PkgStatus, string) VerifyPackage(PackageDef pkg, string installPath)
        {
            var pkgPath = Path.Combine(installPath, pkg.DirName);
            var checks = new List<string>();

            // 1. 目录存在
            if (!Directory.Exists(pkgPath))
                return (PkgStatus.NotInstalled, "目录不存在");

            // 2. package.json
            var pkgJsonPath = Path.Combine(pkgPath, "package.json");
            if (!File.Exists(pkgJsonPath))
                return (PkgStatus.Failed, "package.json 缺失");

            var (pkgName, pkgVersion) = ParsePackageJson(pkgJsonPath);
            if (pkgName != pkg.PackageId)
                checks.Add($"包名不符 (期望 {pkg.PackageId}, 实际 {pkgName})");
            if (pkgVersion != pkg.Version)
                checks.Add($"版本不符 (期望 {pkg.Version}, 实际 {pkgVersion})");

            // 3. asmdef 文件
            foreach (var asmdefRel in pkg.AsmdefRelPaths)
            {
                var asmdefPath = Path.Combine(pkgPath, asmdefRel);
                if (!File.Exists(asmdefPath))
                    checks.Add($"缺失 asmdef: {asmdefRel}");
            }

            // 4. 程序集已加载
            var asm = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == pkg.KeyAssemblyName);
            if (asm == null)
                checks.Add($"程序集未加载: {pkg.KeyAssemblyName}");

            // 5. 关键类型
            if (asm != null && !string.IsNullOrEmpty(pkg.KeyTypeFullName))
            {
                var type = asm.GetType(pkg.KeyTypeFullName);
                if (type == null)
                    checks.Add($"关键类型未找到: {pkg.KeyTypeFullName}");
            }

            // 6. 依赖检查
            foreach (var depId in pkg.DepPackageIds)
            {
                int depIdx = Array.FindIndex(s_packages, p => p.PackageId == depId);
                if (depIdx >= 0)
                {
                    var depPkg = s_packages[depIdx];
                    var depPath = Path.Combine(installPath, depPkg.DirName);
                    if (!Directory.Exists(depPath))
                        checks.Add($"依赖未安装: {depPkg.DisplayName}");
                }
            }

            // 7. Odin 依赖
            if (pkg.RequiresOdin)
            {
                bool odinLoaded = AppDomain.CurrentDomain.GetAssemblies()
                    .Any(a => a.GetName().Name.StartsWith("Sirenix", StringComparison.Ordinal));
                if (!odinLoaded)
                    checks.Add("Odin Inspector 未检测到（Inspector 包需要 Odin）");
            }

            if (checks.Count == 0)
                return (PkgStatus.Installed, $"v{pkgVersion} — 文件/程序集/类型 全部通过");

            return (PkgStatus.Failed, string.Join("; ", checks));
        }

        #endregion

        #region 测试安装

        /// <summary>
        /// 测试安装 — 使用本地源复制到临时目录，验证复制和验证逻辑的正确性。
        /// 不触碰 Assets/ 目录，不执行 Git 操作。
        /// </summary>
        void TestInstall()
        {
            _busy = true;
            Log("════════════════════════════════════════");
            Log("开始测试安装（本地源 → 临时目录）");
            Log("════════════════════════════════════════");

            // 1. 确定源路径
            string sourceRoot = _localPath;
            if (string.IsNullOrEmpty(sourceRoot))
                sourceRoot = AutoDetectLocalSource();

            if (string.IsNullOrEmpty(sourceRoot))
            {
                LogError("无法确定本地源路径，请手动指定");
                _busy = false;
                return;
            }

            if (_sourceMode == SourceMode.GitRemote)
            {
                LogError("Git 模式不支持测试安装，请切换到本地目录模式");
                _busy = false;
                return;
            }

            if (!Directory.Exists(sourceRoot))
            {
                LogError($"源路径不存在: {sourceRoot}");
                _busy = false;
                return;
            }

            Log($"源路径: {sourceRoot}");

            // 2. 验证源结构
            var sourcePackagesPath = Path.Combine(sourceRoot, "Assets/Runestone");
            if (!Directory.Exists(sourcePackagesPath))
            {
                LogError($"源中未找到 Assets/Runestone 目录: {sourcePackagesPath}");
                _busy = false;
                return;
            }

            bool sourceValid = true;
            foreach (var pkg in s_packages)
            {
                var pkgSourcePath = Path.Combine(sourcePackagesPath, pkg.DirName);
                if (!Directory.Exists(pkgSourcePath))
                {
                    LogError($"源中缺少包目录: {pkg.DirName}");
                    sourceValid = false;
                    continue;
                }

                var pkgJson = Path.Combine(pkgSourcePath, "package.json");
                if (!File.Exists(pkgJson))
                {
                    LogError($"源中 {pkg.DirName} 缺少 package.json");
                    sourceValid = false;
                    continue;
                }

                var (name, ver) = ParsePackageJson(pkgJson);
                if (name != pkg.PackageId || ver != pkg.Version)
                {
                    LogError($"源中 {pkg.DirName} 元数据不符: 期望 {pkg.PackageId}@{pkg.Version}, 实际 {name}@{ver}");
                    sourceValid = false;
                }
                else
                {
                    Log($"  源验证通过: {pkg.DisplayName} ({name}@{ver})");
                }
            }

            if (!sourceValid)
            {
                LogError("源验证失败，终止测试");
                _busy = false;
                return;
            }

            Log("✅ 源结构验证通过");

            // 3. 准备临时目录
            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? "";
            var tempDir = Path.Combine(projectRoot, TempStagingDir);
            var tempRunestone = Path.Combine(tempDir, "Runestone");

            if (Directory.Exists(tempDir))
            {
                Log($"清理旧临时目录: {tempDir}");
                Directory.Delete(tempDir, true);
            }
            Directory.CreateDirectory(tempRunestone);

            // 4. 执行复制（使用与真实安装完全相同的方法）
            Log("开始复制包文件到临时目录...");
            var copyResults = new List<(PackageDef pkg, int fileCount, bool success)>();

            foreach (var pkg in s_packages)
            {
                var srcPath = Path.Combine(sourcePackagesPath, pkg.DirName);
                var dstPath = Path.Combine(tempRunestone, pkg.DirName);

                try
                {
                    int count = CopyDirectoryFiltered(srcPath, dstPath);
                    copyResults.Add((pkg, count, true));
                    Log($"  ✓ {pkg.DisplayName}: {count} 文件已复制");
                }
                catch (Exception e)
                {
                    copyResults.Add((pkg, 0, false));
                    LogError($"  ✗ {pkg.DisplayName} 复制失败: {e.Message}");
                }
            }

            // 5. 验证临时目录中的包结构
            Log("验证临时目录中的包结构...");
            bool tempValid = true;

            foreach (var (pkg, fileCount, success) in copyResults)
            {
                if (!success)
                {
                    tempValid = false;
                    continue;
                }

                var tempPkgPath = Path.Combine(tempRunestone, pkg.DirName);
                var checks = new List<string>();

                // package.json
                var tempPkgJson = Path.Combine(tempPkgPath, "package.json");
                if (!File.Exists(tempPkgJson))
                {
                    checks.Add("package.json 缺失");
                }
                else
                {
                    var (name, ver) = ParsePackageJson(tempPkgJson);
                    if (name != pkg.PackageId) checks.Add($"包名不符: {name}");
                    if (ver != pkg.Version) checks.Add($"版本不符: {ver}");
                }

                // asmdef 文件
                foreach (var asmdefRel in pkg.AsmdefRelPaths)
                {
                    var tempAsmdef = Path.Combine(tempPkgPath, asmdefRel);
                    if (!File.Exists(tempAsmdef))
                        checks.Add($"缺失 asmdef: {asmdefRel}");
                }

                // 文件数对比（源 vs 目标，均使用过滤后的计数）
                var srcPkgPath = Path.Combine(sourcePackagesPath, pkg.DirName);
                int srcCount = CountFilesFiltered(srcPkgPath);
                int dstCount = CountFilesFiltered(tempPkgPath);

                if (srcCount != dstCount)
                    checks.Add($"文件数不匹配: 源 {srcCount}, 目标 {dstCount}");

                // 检查是否有不该包含的文件
                var junkFiles = FindJunkFiles(tempPkgPath);
                if (junkFiles.Count > 0)
                    checks.Add($"包含垃圾文件: {string.Join(", ", junkFiles)}");

                if (checks.Count == 0)
                    Log($"  ✓ {pkg.DisplayName}: 结构验证通过 (源 {srcCount} = 目标 {dstCount} 文件)");
                else
                {
                    Log($"  ✗ {pkg.DisplayName}: {string.Join("; ", checks)}");
                    tempValid = false;
                }
            }

            // 6. 清理临时目录
            Log("清理临时目录...");
            try
            {
                Directory.Delete(tempDir, true);
                Log("  ✓ 临时目录已清理");
            }
            catch (Exception e)
            {
                LogError($"  ✗ 清理失败: {e.Message}（请手动删除 {tempDir}）");
            }

            // 7. 同时验证当前安装
            Log("\n附加验证: 检查当前项目中的安装状态...");
            VerifyAll();

            // 8. 总结
            Log("════════════════════════════════════════");
            if (tempValid && copyResults.All(r => r.success))
                Log("✅ 测试安装通过 — 安装器复制和验证逻辑正常");
            else
                Log("❌ 测试安装存在问题，详见上方日志");
            Log("════════════════════════════════════════\n");

            _busy = false;
        }

        #endregion

        #region 实际安装

        void InstallAll()
        {
            _busy = true;
            Log("════════════════════════════════════════");
            Log("开始安装全部包");
            Log("════════════════════════════════════════");

            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? "";
            var installPath = Path.Combine(projectRoot, _installRoot);
            Directory.CreateDirectory(installPath);

            if (_sourceMode == SourceMode.LocalDirectory)
            {
                string sourceRoot = _localPath;
                if (string.IsNullOrEmpty(sourceRoot))
                    sourceRoot = AutoDetectLocalSource();

                if (string.IsNullOrEmpty(sourceRoot) || !Directory.Exists(sourceRoot))
                {
                    LogError("无法确定本地源路径");
                    _busy = false;
                    return;
                }

                var sourcePackagesPath = Path.Combine(sourceRoot, "Assets/Runestone");

                for (int i = 0; i < s_packages.Length; i++)
                {
                    var pkg = s_packages[i];
                    var src = Path.Combine(sourcePackagesPath, pkg.DirName);
                    var dst = Path.Combine(installPath, pkg.DirName);

                    if (Directory.Exists(dst))
                    {
                        Log($"  {pkg.DisplayName}: 已存在，跳过（如需重装请先删除 {dst}）");
                        _statuses[i] = PkgStatus.Installed;
                        _statusDetails[i] = "已存在（跳过）";
                        continue;
                    }

                    try
                    {
                        int count = CopyDirectoryFiltered(src, dst);
                        Log($"  ✓ {pkg.DisplayName}: {count} 文件已安装");
                    }
                    catch (Exception e)
                    {
                        LogError($"  ✗ {pkg.DisplayName} 安装失败: {e.Message}");
                    }
                }

                Log("刷新 AssetDatabase...");
                AssetDatabase.Refresh();
            }
            else
            {
                Log("Git 模式暂未实现（测试阶段）");
            }

            Log("等待编译完成后请点击「验证安装」确认结果");
            Log("════════════════════════════════════════\n");
            _busy = false;
        }

        #endregion

        #region 文件操作

        /// <summary>
        /// 递归复制目录，排除垃圾文件和目录。返回复制的文件数。
        /// </summary>
        int CopyDirectoryFiltered(string source, string target)
        {
            if (!Directory.Exists(source))
                throw new DirectoryNotFoundException($"源目录不存在: {source}");

            Directory.CreateDirectory(target);
            int count = 0;

            // 复制文件
            foreach (var file in Directory.GetFiles(source))
            {
                var fileName = Path.GetFileName(file);
                var ext = Path.GetExtension(fileName);

                if (ExcludeFileNames.Contains(fileName))
                    continue;
                if (ExcludeFileExts.Contains(ext))
                    continue;

                var targetFile = Path.Combine(target, fileName);
                File.Copy(file, targetFile, true);
                count++;
            }

            // 复制子目录
            foreach (var dir in Directory.GetDirectories(source))
            {
                var dirName = Path.GetFileName(dir);
                if (ExcludeDirNames.Contains(dirName))
                    continue;

                var targetDir = Path.Combine(target, dirName);
                count += CopyDirectoryFiltered(dir, targetDir);
            }

            return count;
        }

        /// <summary>
        /// 按与复制时相同的过滤规则统计文件数。
        /// </summary>
        int CountFilesFiltered(string dir)
        {
            int count = 0;
            foreach (var file in Directory.GetFiles(dir))
            {
                var fileName = Path.GetFileName(file);
                var ext = Path.GetExtension(file);
                if (ExcludeFileNames.Contains(fileName)) continue;
                if (ExcludeFileExts.Contains(ext)) continue;
                count++;
            }
            foreach (var subDir in Directory.GetDirectories(dir))
            {
                var dirName = Path.GetFileName(subDir);
                if (ExcludeDirNames.Contains(dirName)) continue;
                count += CountFilesFiltered(subDir);
            }
            return count;
        }

        /// <summary>
        /// 在已复制的目录中查找不应包含的垃圾文件。
        /// </summary>
        List<string> FindJunkFiles(string dir)
        {
            var junk = new List<string>();
            foreach (var file in Directory.GetFiles(dir, "*", SearchOption.AllDirectories))
            {
                var fileName = Path.GetFileName(file);
                var ext = Path.GetExtension(fileName);
                if (ExcludeFileNames.Contains(fileName) || ExcludeFileExts.Contains(ext))
                    junk.Add(fileName);
            }
            foreach (var subDir in Directory.GetDirectories(dir, "*", SearchOption.AllDirectories))
            {
                var dirName = Path.GetFileName(subDir);
                if (ExcludeDirNames.Contains(dirName))
                    junk.Add(dirName + "/");
            }
            return junk;
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 自动检测本地源路径（当运行在 Aesir 开发项目时）。
        /// </summary>
        string AutoDetectLocalSource()
        {
            var dataPath = Application.dataPath;
            var projectRoot = Directory.GetParent(dataPath)?.FullName;
            if (projectRoot == null) return null;

            var runestonePath = Path.Combine(projectRoot, "Assets/Runestone");
            if (!Directory.Exists(runestonePath)) return null;

            foreach (var pkg in s_packages)
            {
                if (!Directory.Exists(Path.Combine(runestonePath, pkg.DirName)))
                    return null;
            }

            return projectRoot;
        }

        /// <summary>
        /// 简易 JSON 字段提取（不依赖 Newtonsoft 等 JSON 库）。
        /// </summary>
        (string name, string version) ParsePackageJson(string path)
        {
            try
            {
                var json = File.ReadAllText(path);
                return (ExtractJsonField(json, "name"), ExtractJsonField(json, "version"));
            }
            catch
            {
                return ("", "");
            }
        }

        static string ExtractJsonField(string json, string fieldName)
        {
            var key = "\"" + fieldName + "\"";
            var idx = json.IndexOf(key, StringComparison.Ordinal);
            if (idx < 0) return "";

            var colonIdx = json.IndexOf(':', idx + key.Length);
            if (colonIdx < 0) return "";

            var quoteStart = json.IndexOf('"', colonIdx + 1);
            if (quoteStart < 0) return "";

            var quoteEnd = json.IndexOf('"', quoteStart + 1);
            if (quoteEnd < 0) return "";

            return json.Substring(quoteStart + 1, quoteEnd - quoteStart - 1);
        }

        #endregion

        #region 日志

        void Log(string message)
        {
            var ts = DateTime.Now.ToString("HH:mm:ss");
            _log.AppendLine("[" + ts + "] " + message);
            Debug.Log("[Aesir Installer] " + message);
            Repaint();
        }

        void LogError(string message)
        {
            var ts = DateTime.Now.ToString("HH:mm:ss");
            _log.AppendLine("[" + ts + "] ❌ " + message);
            Debug.LogError("[Aesir Installer] " + message);
            Repaint();
        }

        #endregion
    }
}
