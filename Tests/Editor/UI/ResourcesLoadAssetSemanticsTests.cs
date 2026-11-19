using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Runestone.AesirModules;
using Object = UnityEngine.Object;

namespace Runestone.AesirModules.Tests.Editor.UI
{
    /// <summary>
    /// 实证 `Resources.Load` 返回对象与磁盘资产的关系（编辑器环境）。
    /// <para>
    /// 结论（与直觉相反、需实测确认）：
    /// ① 编辑器里 `Resources.Load` 与 `AssetDatabase.LoadAssetAtPath` 返回**同一对象引用**（缓存的资产对象本身），不是隔离副本；
    /// ② 修改这个对象，经同一引用读回立即见新值——但这只是内存态；
    /// ③ 仅修改字段（不 SetDirty / SaveAssets）**不会写回磁盘 .asset 文件**；
    /// ④ 一旦某处调用 SetDirty（Inspector 改动、SaveAssets、显式 API），改动才落盘。
    /// </para>
    /// 自包含：SetUp 在测试专用 Resources 路径创建临时 SO 资产，TearDown 删除，
    /// 不依赖工程是否已有 UIModuleConfig 资产，也不污染既有配置。
    /// </summary>
    public class ResourcesLoadAssetSemanticsTests
    {
        const string TestDir = "Assets/Resources/AesirResourcesLoadProbe";
        const string AssetPath = TestDir + "/Probe.asset";
        const string ResourcePath = "AesirResourcesLoadProbe/Probe";
        static readonly FieldInfo MaskModeField = typeof(UIModuleConfigSO).GetField(
            "maskMode", BindingFlags.Instance | BindingFlags.NonPublic);

        UIModuleConfigSO _cfg;
        UIMaskMode _baseline;

        [SetUp]
        public void SetUp()
        {
            // 自包含：确保测试专用目录与资产存在（不复用工程正式配置资产）
            if (!AssetDatabase.IsValidFolder(TestDir))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }
                AssetDatabase.CreateFolder("Assets/Resources", "AesirResourcesLoadProbe");
            }

            _cfg = AssetDatabase.LoadAssetAtPath<UIModuleConfigSO>(AssetPath);
            if (_cfg == null)
            {
                _cfg = ScriptableObject.CreateInstance<UIModuleConfigSO>();
                AssetDatabase.CreateAsset(_cfg, AssetPath);
                AssetDatabase.SaveAssets();
            }

            // 资源系统可能缓存旧引用：强制从磁盘重导后经 Resources 取，保证与磁盘一致
            AssetDatabase.ImportAsset(AssetPath, ImportAssetOptions.ForceUpdate);
            _cfg = Resources.Load<UIModuleConfigSO>(ResourcePath);
            Assert.IsNotNull(_cfg, "测试资产应能从 Resources 加载");
            _baseline = (UIMaskMode)MaskModeField.GetValue(_cfg);
        }

        [TearDown]
        public void TearDown()
        {
            // 还原内存态再删除资产，避免残留脏对象影响其它用例
            if (_cfg != null)
            {
                MaskModeField.SetValue(_cfg, _baseline);
                EditorUtility.SetDirty(_cfg);
                AssetDatabase.SaveAssets();
            }

            if (AssetDatabase.LoadAssetAtPath<UIModuleConfigSO>(AssetPath) != null)
            {
                AssetDatabase.DeleteAsset(AssetPath);
            }
            if (AssetDatabase.IsValidFolder(TestDir))
            {
                AssetDatabase.DeleteAsset(TestDir);
            }
        }

        [Test]
        public void ResourcesLoad_And_AssetDatabaseLoad_ReturnSameReference()
        {
            var byAssetDatabase = AssetDatabase.LoadAssetAtPath<UIModuleConfigSO>(AssetPath);

            // 关键：两个入口是同一对象，不是"加载出一份隔离副本"
            Assert.IsTrue(ReferenceEquals(_cfg, byAssetDatabase),
                "编辑器里 Resources.Load 与 AssetDatabase.LoadAssetAtPath 应返回同一对象引用");
        }

        [Test]
        public void ModifyLoadedObject_SameReferenceSeesNewValueImmediately()
        {
            var byAssetDatabase = AssetDatabase.LoadAssetAtPath<UIModuleConfigSO>(AssetPath);
            var changed = (UIMaskMode)(((int)_baseline + 1) % 3);
            Assume.That(changed != _baseline);

            MaskModeField.SetValue(_cfg, changed);

            // 因同一引用，AssetDatabase 入口立刻读到新值——这是"内存态一致"，不是"磁盘已改"
            Assert.AreEqual(changed, (UIMaskMode)MaskModeField.GetValue(byAssetDatabase),
                "同一对象引用：改一处，另一入口立即读到新值");
        }

        [Test]
        public void ModifyLoadedObject_WithoutSave_DoesNotWriteDiskAsset()
        {
            var changed = (UIMaskMode)(((int)_baseline + 1) % 3);
            Assume.That(changed != _baseline);

            MaskModeField.SetValue(_cfg, changed); // 只改内存，不 SetDirty / SaveAssets
            AssetDatabase.Refresh();

            // 绕过 Unity 对象缓存，直接读磁盘 YAML 文本
            var diskText = File.ReadAllText(AssetPath);
            var expectedOnDisk = "maskMode: " + (int)_baseline;
            StringAssert.Contains(expectedOnDisk, diskText,
                "未 SetDirty / SaveAssets 时，内存修改不应写回磁盘 .asset 文件");

            // 且此时对象未被标脏（脚本直接改字段不自动 dirty）
            Assert.IsFalse(EditorUtility.IsDirty(_cfg),
                "脚本直接修改字段不会自动把资产标记为 dirty");
        }
    }
}
