using System.IO;
using System.Linq;
using Runestone.AesirInspector.Editor;
using UnityEditor;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// 右键快捷处理 Summary 特性。
    /// </summary>
    public static class SummaryToolMenuItems
    {
        [MenuItem(AesirInspectorMenuItems.ProcessSummarySync, false,
            AesirInspectorMenuItems.ProcessSummarySyncOrder)]
        public static void QuickSyncSummary()
        {
            if (Selection.objects.Length == 1)
            {
                WriteSyncSummary(AssetDatabase.GetAssetPath(Selection.activeObject));
            }
            else
            {
                foreach (var obj in Selection.objects)
                {
                    WriteSyncSummary(AssetDatabase.GetAssetPath(obj));
                }
            }
        }

        [MenuItem(AesirInspectorMenuItems.ProcessSummaryReplace, false,
            AesirInspectorMenuItems.ProcessSummaryReplaceOrder)]
        public static void QuickReplaceSummary()
        {
            if (Selection.objects.Length == 1)
            {
                WriteReplaceSummary(AssetDatabase.GetAssetPath(Selection.activeObject));
            }
            else
            {
                foreach (var obj in Selection.objects)
                {
                    WriteReplaceSummary(AssetDatabase.GetAssetPath(obj));
                }
            }
        }

        [MenuItem(AesirInspectorMenuItems.ProcessSummaryRemove, false,
            AesirInspectorMenuItems.ProcessSummaryRemoveOrder)]
        public static void QuickRemoveSummary()
        {
            if (Selection.objects.Length == 1)
            {
                WriteRemoveSummary(AssetDatabase.GetAssetPath(Selection.activeObject));
            }
            else
            {
                foreach (var obj in Selection.objects)
                {
                    WriteRemoveSummary(AssetDatabase.GetAssetPath(obj));
                }
            }
        }

        [MenuItem(AesirInspectorMenuItems.ProcessSummarySync, true)]
        static bool CanSyncSummary() => IsScriptAsset();

        [MenuItem(AesirInspectorMenuItems.ProcessSummaryReplace, true)]
        static bool CanReplaceSummary() => IsScriptAsset();

        [MenuItem(AesirInspectorMenuItems.ProcessSummaryRemove, true)]
        static bool CanRemoveSummary() => IsScriptAsset();

        static bool IsScriptAsset()
        {
            var selectedObject = Selection.activeObject;
            return selectedObject && Selection.objects.All(obj => obj is MonoScript);
        }

        static void WriteSyncSummary(string filePath)
        {
            var sourceCode = File.ReadAllText(filePath);
            var processor = new XmlSummaryTool(sourceCode).ParseSourceScript();
            sourceCode = processor.GetProcessedSourceScript(XmlSummaryTool.ProcessMode.SyncSummary);
            File.WriteAllText(filePath, sourceCode);
            AssetDatabase.Refresh();
        }

        static void WriteReplaceSummary(string filePath)
        {
            var sourceCode = File.ReadAllText(filePath);
            var processor = new XmlSummaryTool(sourceCode).ParseSourceScript();
            sourceCode = processor.GetProcessedSourceScript(XmlSummaryTool.ProcessMode.ReplaceSummary);
            File.WriteAllText(filePath, sourceCode);
            AssetDatabase.Refresh();
        }

        static void WriteRemoveSummary(string filePath)
        {
            var sourceCode = File.ReadAllText(filePath);
            var processor = new XmlSummaryTool(sourceCode).ParseSourceScript();
            sourceCode = processor.GetProcessedSourceScript(XmlSummaryTool.ProcessMode.RemoveSummary);
            File.WriteAllText(filePath, sourceCode);
            AssetDatabase.Refresh();
        }
    }
}
