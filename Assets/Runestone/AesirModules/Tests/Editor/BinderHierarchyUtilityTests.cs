using NUnit.Framework;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.TestTools;

namespace Runestone.AesirModules.Tests.Editor
{
    /// <summary>
    /// <see cref="BinderHierarchyUtility" /> 层级路径计算单元测试。
    /// </summary>
    public class BinderHierarchyUtilityTests
    {
        GameObject _root;

        [TearDown]
        public void TearDown()
        {
            if (_root)
            {
                Object.DestroyImmediate(_root);
            }
        }

        [Test]
        public void GetAbsolutePath_JoinsRootToTarget()
        {
            _root = new GameObject("Root");
            var child = new GameObject("Child");
            child.transform.SetParent(_root.transform);
            var grandChild = new GameObject("GrandChild");
            grandChild.transform.SetParent(child.transform);

            Assert.That(BinderHierarchyUtility.GetAbsolutePath(grandChild.transform), Is.EqualTo("Root/Child/GrandChild"));
        }

        [Test]
        public void GetRelativePath_JoinsSubSegments()
        {
            var relative = BinderHierarchyUtility.GetRelativePath("Root/Panel", "Root/Panel/PlayButton");

            Assert.That(relative, Is.EqualTo("PlayButton"));
        }

        [Test]
        public void GetRelativePath_SameObjectReturnsEmpty()
        {
            var relative = BinderHierarchyUtility.GetRelativePath("Root/Panel", "Root/Panel");

            Assert.That(relative, Is.EqualTo(""));
        }

        [Test]
        public void GetRelativePath_NotChildReturnsNull()
        {
            // 非父子关系会主动输出 Error 日志，预注册预期避免测试失败
            LogAssert.Expect(LogType.Error, new Regex("路径错误"));

            var relative = BinderHierarchyUtility.GetRelativePath("Root/Panel", "Root/Other/Button");

            Assert.That(relative, Is.Null);
        }

        [Test]
        public void GetRelativePath_ChildShorterThanParentReturnsNull()
        {
            // 回归: 旧实现未校验长度差，子路径比父路径短时直接越界
            LogAssert.Expect(LogType.Error, new Regex("路径错误"));

            string relative = null;
            Assert.DoesNotThrow(() => relative = BinderHierarchyUtility.GetRelativePath("Root/Panel", "Root"));
            Assert.That(relative, Is.Null);
        }
    }
}
