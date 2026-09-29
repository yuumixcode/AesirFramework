#if UNITY_EDITOR // 测试脚本仅编辑器内参与编译：PlayMode 用例的物体清理与宿主场景操作需要 UnityEditor
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Runestone.AesirModules.Tests
{
    /// <summary>
    /// <see cref="UIRoot" /> 的 PlayMode 守护用例：重复实例只销毁自身组件，
    /// 不连带销毁用户物体与其上的其它组件。
    /// </summary>
    /// <remarks>
    /// 必须在运行期验证：EditMode 下 <c>Destroy</c> 被引擎拒绝（"Destroy may not be called from edit mode"，
    /// <c>Destroy(this)</c> 与 <c>Destroy(gameObject)</c> 都是 no-op），两种销毁粒度在该模式下不可区分。
    /// </remarks>
    public class UIRootPlayModeTests
    {
        [UnityTest]
        public IEnumerator DuplicateInstance_DestroysOnlyComponent_KeepsGameObjectAndSiblings()
        {
            var parent = new GameObject("UIRootPlayModeTests");

            // 既有实例挂非根物体：DDOL 分支只对根物体生效，避免测试触发场景迁移
            var existingHost = new GameObject("Existing");
            existingHost.transform.SetParent(parent.transform, false);
            var existing = existingHost.AddComponent<UIRoot>();
            yield return null;

            Assert.IsTrue(existing != null, "前置：既有实例应已建立");

            // 重复实例与业务组件同挂一个物体
            var duplicateHost = new GameObject("Duplicate", typeof(BoxCollider));
            duplicateHost.transform.SetParent(parent.transform, false);
            duplicateHost.AddComponent<UIRoot>();
            yield return null;

            Assert.IsTrue(existing != null && existing == UIRoot.Instance, "重复实例不得覆盖既有单例");
            Assert.IsTrue(duplicateHost != null, "重复实例所在物体必须存活（对齐 RAA 先例：不连带销毁用户物体）");
            Assert.IsNotNull(duplicateHost.GetComponent<BoxCollider>(), "同物体上的业务组件必须存活");
            Assert.IsTrue(duplicateHost.GetComponent<UIRoot>() == null, "重复实例自身组件应被销毁");

            Object.Destroy(parent);
        }
    }
}
#endif
