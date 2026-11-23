#if UNITY_EDITOR // PlayMode 用例仅在编辑器内运行：序列化字段改写（sortingOrder/destroyOnHide）需要 UnityEditor
using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Runestone.AesirModules.Tests
{
    /// <summary>
    /// <see cref="UIModule" /> 窗口销毁链路的 PlayMode 守护用例：非受控外部销毁的反清理 + 蒙版回落，
    /// 以及受控销毁分叉（destroyOnHide=true）的真实物体销毁。
    /// </summary>
    /// <remarks>
    /// 必须在运行期验证：EditMode 下 <c>Destroy</c> 被引擎拒绝（no-op），<c>OnDestroy</c> 反清理链路不可触发——
    /// 既有 EditMode 用例（<c>UIModuleWindowTests.RemoveWindowRecord_ExternalDestroy_CleansRegistry</c>）只能手动调用
    /// internal <c>RemoveWindowRecord</c> 入口替代真实回调。本套件走真实
    /// <c>
    /// Destroy → AesirBaseWindow.OnDestroy →
    /// UIModule.RemoveWindowRecord → RefreshWindowMasks
    /// </c>
    /// 端到端链路，并锁定 <c>RefreshWindowMasks</c>
    /// 复用缓冲（_destroyedWindowKeys）在销毁后重算蒙版的回落行为。
    /// </remarks>
    public class UIModuleWindowPlayModeTests
    {
        /// <summary>
        /// 摘除本套件建立的 UIRoot / UIModule 单例宿主。二者默认 DDOL 跨用例存活，残留单例会让
        /// 依赖「无预存 UIRoot」前提的后续用例（如 <c>UIRootPlayModeTests</c>）新建实例被判重销毁而失败。
        /// 用 TearDown 而非用例内联清理：断言中途失败也能保证清理。FindAnyObjectByType 不触发懒重建。
        /// </summary>
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            var root = Object.FindAnyObjectByType<UIRoot>(FindObjectsInactive.Include);
            if (root != null)
            {
                Object.Destroy(root.gameObject);
            }

            var module = Object.FindAnyObjectByType<UIModule>(FindObjectsInactive.Include);
            if (module != null)
            {
                Object.Destroy(module.transform.root.gameObject);
            }

            yield return null; // 等待 OnDestroy 清空静态单例
        }

        /// <summary>
        /// 非受控外部销毁最高层窗口：经真实 OnDestroy 反清理注册表，单遮蒙版回落到次高层窗口。
        /// </summary>
        [UnityTest]
        public IEnumerator ExternalDestroy_TopWindow_CleansRegistryAndFallsBackMask()
        {
            var module = UIModule.Instance;
            var prefabA = CreateInactiveWindowPrefab<PlayModeWindowA>("PMWindowA", 500);
            var prefabB = CreateInactiveWindowPrefab<PlayModeWindowB>("PMWindowB", 600);
            UIModule.RegisterWindowPrefab<PlayModeWindowA>(prefabA);
            UIModule.RegisterWindowPrefab<PlayModeWindowB>(prefabB);
            module.MaskMode = UIMaskMode.Single;

            var a = module.OpenWindow<PlayModeWindowA>();
            var b = module.OpenWindow<PlayModeWindowB>();
            yield return null;

            Assert.IsNotNull(a, "前置：窗口 A 应打开成功");
            Assert.IsNotNull(b, "前置：窗口 B 应打开成功");
            Assert.IsFalse(IsMaskActive(a.gameObject), "单遮模式最高层为 B(600)，A 的蒙版应关闭");
            Assert.IsTrue(IsMaskActive(b.gameObject), "单遮模式最高层可见窗口 B 的蒙版应开启");

            // 非受控外部销毁（模拟场景卸载 / 用户代码 Destroy），不走 CloseWindow 受控路径
            Object.Destroy(b.gameObject);
            yield return null; // 等待 OnDestroy → RemoveWindowRecord → RefreshWindowMasks

            Assert.IsTrue(UIModule.GetWindow<PlayModeWindowB>() == null,
                "外部销毁应经真实 OnDestroy 反清理注册表（Unity null 语义）");
            Assert.IsTrue(IsMaskActive(a.gameObject), "B 销毁后蒙版应回落到 A（RefreshWindowMasks 复用缓冲重算）");

            Object.Destroy(a.gameObject);
            Object.Destroy(prefabA);
            Object.Destroy(prefabB);
            yield return null;
        }

        /// <summary>
        /// 受控销毁分叉：destroyOnHide=true 时 CloseWindow 在 PlayMode 下真实销毁窗口物体并清理注册表。
        /// </summary>
        [UnityTest]
        public IEnumerator CloseWindow_DestroyOnHide_ActuallyDestroysAndClearsRegistry()
        {
            var module = UIModule.Instance;
            var prefab = CreateInactiveWindowPrefab<PlayModeWindowA>("PMWindowDestroy", 500);
            UIModule.RegisterWindowPrefab<PlayModeWindowA>(prefab);

            var window = module.OpenWindow<PlayModeWindowA>();
            yield return null;
            Assert.IsNotNull(window, "前置：窗口应打开成功");

            SetDestroyOnHide(window, true);
            var windowGo = window.gameObject;

            module.CloseWindow<PlayModeWindowA>();
            yield return null; // Destroy 延迟到帧末执行

            Assert.IsTrue(windowGo == null,
                "destroyOnHide=true：PlayMode 下 Close 应真实销毁窗口物体（EditMode 中 Destroy 为 no-op）");
            Assert.IsTrue(UIModule.GetWindow<PlayModeWindowA>() == null, "销毁分叉应清理注册表");

            Object.Destroy(prefab);
            yield return null;
        }

        /// <summary>
        /// 按 Canvas 根约定创建停用窗口"预制体"（停用态建具避免预制体自身触发 Awake/OnEnable）。
        /// 根节点自带 Canvas + CanvasScaler + GraphicRaycaster，挂窗口脚本与全屏蒙版子物体。
        /// </summary>
        static GameObject CreateInactiveWindowPrefab<T>(string name, int sortingOrder)
            where T : AesirBaseWindow
        {
            var prefabGo = new GameObject(name);
            prefabGo.SetActive(false);
            prefabGo.AddComponent<Canvas>();
            prefabGo.AddComponent<CanvasScaler>();
            prefabGo.AddComponent<GraphicRaycaster>();
            var window = prefabGo.AddComponent<T>();
            SetSortingOrder(window, sortingOrder);
            AddMaskChild(prefabGo);
            return prefabGo;
        }

        /// <summary>添加约定名 Mask 的全屏拉伸蒙版子物体（Image + Button）。</summary>
        static void AddMaskChild(GameObject parent)
        {
            var maskGo = new GameObject("Mask", typeof(RectTransform));
            maskGo.transform.SetParent(parent.transform, false);
            var rectTransform = (RectTransform)maskGo.transform;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            maskGo.AddComponent<Image>();
            maskGo.AddComponent<Button>();
        }

        /// <summary>读取窗口蒙版子物体激活状态（断言蒙版子物体存在）。</summary>
        static bool IsMaskActive(GameObject windowGo)
        {
            var maskTransform = windowGo.transform.Find("Mask");
            Assert.IsNotNull(maskTransform, "窗口应带 Mask 子物体");
            return maskTransform.gameObject.activeSelf;
        }

        /// <summary>经 SerializedObject 修改窗口 sortingOrder 序列化字段（与 EditMode 用例同款）。</summary>
        static void SetSortingOrder(AesirBaseWindow window, int value)
        {
            var so = new SerializedObject(window);
            so.FindProperty("sortingOrder").intValue = value;
            so.ApplyModifiedProperties();
        }

        /// <summary>经 SerializedObject 修改窗口 destroyOnHide 序列化字段（与 EditMode 用例同款）。</summary>
        static void SetDestroyOnHide(AesirBaseWindow window, bool value)
        {
            var so = new SerializedObject(window);
            so.FindProperty("destroyOnHide").boolValue = value;
            so.ApplyModifiedProperties();
        }

        /// <summary>测试窗口 A（基类 OnDestroy 已驱动反清理，无需覆写）。</summary>
        class PlayModeWindowA : AesirBaseWindow { }

        /// <summary>测试窗口 B（与 A 分桶注册，蒙版叠加/回落的第二窗口）。</summary>
        class PlayModeWindowB : AesirBaseWindow { }
    }
}
#endif
