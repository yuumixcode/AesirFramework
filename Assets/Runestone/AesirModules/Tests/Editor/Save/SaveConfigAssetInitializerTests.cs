using NUnit.Framework;
using Runestone.AesirModules.Editor;
using UnityEditor;

namespace Runestone.AesirModules.Tests.Editor.Save
{
    /// <summary>
    /// SaveModuleConfigAssetInitializer 的 EditMode 用例：显式创建入口幂等
    /// （spec「配置资产与内存兜底」菜单幂等场景；创建路径与 UI/Scene 共用 AesirSingletonAssetInitializer）。
    /// <para>
    /// 用例前项目无 SaveConfigSO 资产时：首次调用创建、第二次跳过，断言只一份；
    /// 用例内删除本用例创建的资产与空目录，不在 Assets/ 留残留。
    /// 用例前已有资产（如仓库示例基线）时：两次调用均跳过，同样断言只一份。
    /// </para>
    /// </summary>
    public class SaveConfigAssetInitializerTests
    {
        [Test]
        public void EnsureConfigAsset_CalledTwice_LeavesSingleAsset()
        {
            var preExisting = AssetDatabase.FindAssets("t:SaveConfigSO").Length > 0;

            SaveModuleConfigAssetInitializer.EnsureConfigAsset();
            SaveModuleConfigAssetInitializer.EnsureConfigAsset();

            var guids = AssetDatabase.FindAssets("t:SaveConfigSO");
            Assert.AreEqual(1, guids.Length, "连续两次显式创建应只留一份配置资产");

            if (!preExisting)
            {
                AssetDatabase.DeleteAsset(SaveModuleConfigAssetInitializer.AssetPath);
                var directory = SaveModuleConfigAssetInitializer.AssetDirectory;
                if (AssetDatabase.IsValidFolder(directory) &&
                    AssetDatabase.FindAssets("", new[] { directory }).Length == 0)
                {
                    AssetDatabase.DeleteAsset(directory);
                }
            }
        }
    }
}