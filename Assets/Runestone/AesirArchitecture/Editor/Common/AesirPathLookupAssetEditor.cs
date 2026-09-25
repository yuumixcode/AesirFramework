using System;
using UnityEditor;

namespace Runestone.AesirArchitecture.Editor
{
    /// <summary>
    /// <see cref="AesirPathLookup" /> 锚点资产的 Inspector — 说明资产用途并比对期望 / 实际 GUID
    /// （参照 Odin 的 SirenixPathLookupScriptableObjectEditor）：锚点 GUID 被外部工具改写会导致
    /// Runestone 移动后的定位失效，此处给出可视化核对面。
    /// </summary>
    [CustomEditor(typeof(AesirPathLookup))]
    internal class AesirPathLookupAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("请勿删除此文件！", EditorStyles.boldLabel);
            EditorGUILayout.Space(4f);
            EditorGUILayout.HelpBox(
                "Aesir 通过这个资产定位包的安装位置——因此你可以把 Runestone 文件夹移动到项目任意位置，" +
                "包内更新器、Getting Started 窗口与示例场景的构建剔除都会照常找到它。\n" + "删除本资产不影响默认位置（Assets/Runestone）的使用，但移动后将无法定位。",
                MessageType.Info);
            EditorGUILayout.Space(8f);

            var assetPath = AssetDatabase.GetAssetPath(target);
            var expected = AesirAssetPaths.GetExpectedLookupAssetGuid(assetPath);
            var actual = string.IsNullOrEmpty(assetPath) ? "非项目资产" : AssetDatabase.AssetPathToGUID(assetPath);

            EditorGUILayout.LabelField("期望资产 GUID：", expected ?? "未知（锚点不在已知包根下）");
            EditorGUILayout.LabelField("实际资产 GUID：", actual);

            if (expected != null && !string.Equals(expected, actual, StringComparison.Ordinal))
            {
                EditorGUILayout.HelpBox(
                    "实际 GUID 与期望不一致：锚点资产可能被外部工具改写，Runestone 移动后将无法定位。" + "重新导入 Aesir 包可恢复。",
                    MessageType.Warning);
            }
        }
    }
}
