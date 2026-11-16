using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Runestone.AesirModules.Tests.Editor.UI
{
    /// <summary>
    /// 世界空间 UI Canvas 预设的 EditMode 守护用例：
    /// 配置预设（渲染模式 / 缩放 / 密度 / 屏幕空间配置跳过 Scaler）、组件接线（Canvas / RectTransform /
    /// Raycaster / sortingOrder / UI 层）、跟随目标挂接与偏移、运行时创建入口与空目标防呆。
    /// </summary>
    /// <remarks>
    /// EditMode 中 AddComponent 不触发 Awake，故组件接线由显式调用 <see cref="WorldSpaceUICanvas.Build" /> 驱动；
    /// UIRoot 的层级构建由反射驱动 <c>Initialize</c>（与 UIModuleTests 同款模式）。
    /// </remarks>
    public class WorldSpaceUICanvasTests
    {
        readonly Object[] _created = new Object[8];
        int _createdCount;

        [TearDown]
        public void TearDown()
        {
            for (var i = 0; i < _createdCount; i++)
            {
                if (_created[i] != null)
                {
                    Object.DestroyImmediate(_created[i]);
                }
            }

            _createdCount = 0;
            ResetUIRootStatics();
        }

        T Track<T>(T obj) where T : Object
        {
            _created[_createdCount++] = obj;
            return obj;
        }

        static void ResetUIRootStatics()
        {
            typeof(UIRoot).GetMethod("ResetStatics", BindingFlags.NonPublic | BindingFlags.Static)
                ?.Invoke(null, null);
            typeof(UIModule).GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static)
                ?.SetValue(null, null);
        }

        [Test]
        public void CreateWorldSpaceDefault_UsesWorldSpaceAndConstantPixelSize()
        {
            var config = UICanvasConfigSO.CreateWorldSpaceDefault();

            Assert.AreEqual(RenderMode.WorldSpace, config.renderMode);
            Assert.AreEqual(CanvasScaler.ScaleMode.ConstantPixelSize, config.scaleMode);
        }

        [Test]
        public void ApplyToWorldSpaceCanvas_WiresRenderModeScaleAndRaycaster()
        {
            var go = Track(new GameObject("WorldUI"));
            var rect = go.AddComponent<RectTransform>();
            var canvas = go.AddComponent<Canvas>();
            var scaler = go.AddComponent<CanvasScaler>();
            var raycaster = go.AddComponent<GraphicRaycaster>();
            var config = UICanvasConfigSO.CreateWorldSpaceDefault();

            config.ApplyToWorldSpaceCanvas(canvas);

            Assert.AreEqual(RenderMode.WorldSpace, canvas.renderMode);
            Assert.AreEqual(CanvasScaler.ScaleMode.ConstantPixelSize, scaler.uiScaleMode);
            Assert.AreEqual(0.01f, canvas.transform.localScale.x, 0.0001f);
            Assert.AreEqual(100f, rect.sizeDelta.x, 0.0001f);
            Assert.IsNotNull(raycaster);
        }

        [Test]
        public void ApplyToCanvas_InWorldSpaceConfig_SkipsScreenScaleMode()
        {
            var go = Track(new GameObject("WorldUI"));
            var canvas = go.AddComponent<Canvas>();
            var scaler = go.AddComponent<CanvasScaler>();
            var config = UICanvasConfigSO.CreateWorldSpaceDefault();

            config.ApplyToCanvas(canvas);

            Assert.AreEqual(RenderMode.WorldSpace, canvas.renderMode);
            Assert.AreEqual(CanvasScaler.ScaleMode.ConstantPixelSize, scaler.uiScaleMode,
                "世界空间配置不得把 Scaler 改写为屏幕空间缩放模式");
        }

        [Test]
        public void Build_WiresCanvasComponentsSortingOrderAndUILayer()
        {
            var go = Track(new GameObject("WorldUI"));
            var component = go.AddComponent<WorldSpaceUICanvas>();

            component.Build();

            Assert.IsNotNull(component.Canvas);
            Assert.AreEqual(RenderMode.WorldSpace, component.Canvas.renderMode);
            Assert.AreEqual(450, component.Canvas.sortingOrder);
            Assert.AreEqual(5, go.layer);
            Assert.IsNotNull(go.GetComponent<CanvasScaler>());
            Assert.IsNotNull(go.GetComponent<GraphicRaycaster>());
        }

        [Test]
        public void Build_WithExplicitFollowTarget_AttachesUnderTargetAndAppliesOffset()
        {
            var target = Track(new GameObject("Character")).transform;
            var go = Track(new GameObject("WorldUI"));
            var component = go.AddComponent<WorldSpaceUICanvas>();
            component.FollowTarget = target;
            component.PositionOffset = new Vector3(0f, 2f, 0f);

            component.Build();

            Assert.AreSame(target, go.transform.parent);
            Assert.AreEqual(new Vector3(0f, 2f, 0f), go.transform.localPosition);
        }

        [Test]
        public void UIRoot_CreateWorldSpaceCanvas_AttachesUnderTargetAndUsesExplicitConfig()
        {
            var root = Track(new GameObject("[UIRoot]")).AddComponent<UIRoot>();
            if (root.transform.childCount == 0)
            {
                typeof(UIRoot).GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.Invoke(root, null);
            }

            var target = Track(new GameObject("Character")).transform;
            var config = UICanvasConfigSO.CreateWorldSpaceDefault();
            var worldCanvas = root.CreateWorldSpaceCanvas(target, "Nameplate", config);

            Assert.IsNotNull(worldCanvas);
            Assert.AreSame(target, worldCanvas.transform.parent);
            Assert.AreEqual("Nameplate", worldCanvas.gameObject.name);
            Assert.AreSame(config, worldCanvas.CanvasConfig);
            Assert.AreEqual(RenderMode.WorldSpace, worldCanvas.Canvas.renderMode);
            Assert.AreEqual(5, worldCanvas.gameObject.layer);
        }

        [Test]
        public void UIRoot_CreateWorldSpaceCanvas_NullTarget_ReturnsNull()
        {
            var root = Track(new GameObject("[UIRoot]")).AddComponent<UIRoot>();

            Assert.IsNull(root.CreateWorldSpaceCanvas(null));
        }
    }
}
