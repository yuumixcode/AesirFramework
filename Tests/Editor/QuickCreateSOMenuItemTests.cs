#if !AESIR_INSPECTOR
using NUnit.Framework;
using Runestone.AesirArchitecture.Editor;
using UnityEngine;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    /// <summary>
    /// QuickCreateSOMenuItem 可测逻辑的 EditMode 测试：
    /// SO 类型过滤（非抽象 SO 子类）与默认资源名裁剪（"SO" 后缀边界）。
    /// </summary>
    public class QuickCreateSOMenuItemTests
    {
        class NormalSO : ScriptableObject { }

        abstract class AbstractSO : ScriptableObject { }

        class NotSO : MonoBehaviour { }

        [Test]
        public void IsCreatableSoClass_FiltersCorrectly()
        {
            Assert.IsTrue(QuickCreateSOMenuItem.IsCreatableSoClass(typeof(NormalSO)),
                "非抽象 SO 子类应可创建");
            Assert.IsFalse(QuickCreateSOMenuItem.IsCreatableSoClass(typeof(AbstractSO)),
                "抽象 SO 不可实例化，应被过滤");
            Assert.IsFalse(QuickCreateSOMenuItem.IsCreatableSoClass(typeof(NotSO)),
                "非 SO 类型应被过滤");
            Assert.IsFalse(QuickCreateSOMenuItem.IsCreatableSoClass(null),
                "null 类型应被过滤");
        }

        [Test]
        public void GetDefaultAssetName_TrimsSoSuffix()
        {
            Assert.AreEqual("Config", QuickCreateSOMenuItem.GetDefaultAssetName("ConfigSO"));
            Assert.AreEqual("SO", QuickCreateSOMenuItem.GetDefaultAssetName("SO"),
                "脚本名恰好为 SO 时保留原名（不产生空资源名）");
            Assert.AreEqual("So", QuickCreateSOMenuItem.GetDefaultAssetName("So"),
                "小写 so 不裁剪");
            Assert.AreEqual("SomeSOX", QuickCreateSOMenuItem.GetDefaultAssetName("SomeSOX"),
                "SO 不在词尾不裁剪");
        }
    }
}
#endif
