using System.IO;
using System.Linq;
using UnityEditor;

namespace Runestone.AesirModules.ScriptDocGenerator.Editor
{
    /// <summary>
    /// 右键快捷处理 Summary 特性。批量处理多选脚本时仅触发一次 AssetDatabase.Refresh。
    /// </summary>
    public static class SummaryToolMenuItems
    {
        [MenuItem(ScriptDocGeneratorMenuPaths.ProcessSummarySync, false,
            ScriptDocGeneratorMenuPaths.ProcessSummarySyncOrder)]
        public static void QuickSyncSummary()
        {
            ProcessSelection(XmlSummaryTool.ProcessMode.SyncSummary, null);
        }

        [MenuItem(ScriptDocGeneratorMenuPaths.ProcessSummaryReplace, false,
            ScriptDocGeneratorMenuPaths.ProcessSummaryReplaceOrder)]
        public static void QuickReplaceSummary()
        {
            ProcessSelection(XmlSummaryTool.ProcessMode.ReplaceSummary, null);
        }

        [MenuItem(ScriptDocGeneratorMenuPaths.ProcessSummaryRemove, false,
            ScriptDocGeneratorMenuPaths.ProcessSummaryRemoveOrder)]
        public static void QuickRemoveSummary()
        {
            // Remove 是破坏性批量删除 [Summary] 特性，执行前确认
            var fileCount = Selection.objects.Length;
            if (!EditorUtility.DisplayDialog("移除 Summary 特性",
                    $"即将从 {fileCount} 个脚本中移除所有 [Summary] 特性（保留 XML 注释），该操作会直接改写源文件，是否继续？", "移除", "取消"))
            {
                return;
            }

            ProcessSelection(XmlSummaryTool.ProcessMode.RemoveSummary, null);
        }

        [MenuItem(ScriptDocGeneratorMenuPaths.ProcessSummarySync, true)]
        static bool CanProcessSummary() => IsScriptAsset();

        [MenuItem(ScriptDocGeneratorMenuPaths.ProcessSummaryReplace, true)]
        static bool CanProcessSummaryReplace() => IsScriptAsset();

        [MenuItem(ScriptDocGeneratorMenuPaths.ProcessSummaryRemove, true)]
        static bool CanProcessSummaryRemove() => IsScriptAsset();

        static bool IsScriptAsset()
        {
            var selectedObject = Selection.activeObject;
            return selectedObject && Selection.objects.All(obj => obj is MonoScript);
        }

        static void ProcessSelection(XmlSummaryTool.ProcessMode mode, string _)
        {
            var processed = 0;
            foreach (var obj in Selection.objects)
            {
                if (WriteProcessedScript(AssetDatabase.GetAssetPath(obj), mode))
                {
                    processed++;
                }
            }

            if (processed > 0)
            {
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// 处理单个脚本并写回磁盘。无 XML 文档注释或处理结果无变化时跳过写盘，避免无关全文件重写。
        /// </summary>
        static bool WriteProcessedScript(string filePath, XmlSummaryTool.ProcessMode mode)
        {
            var sourceCode = File.ReadAllText(filePath);
            var processor = new XmlSummaryTool(sourceCode).ParseSourceScript();
            if (processor.firstXmlCommentLineIndex == -1)
            {
                return false;
            }

            var processed = processor.GetProcessedSourceScript(mode);
            if (processed == sourceCode)
            {
                return false;
            }

            File.WriteAllText(filePath, processed);
            return true;
        }
    }
}
