using System.Collections.Generic;
using Runestone.AesirArchitecture;
using UnityEngine;
using UnityEngine.UI;

namespace Runestone.AesirModules
{
    /// <summary>
    /// UI Canvas 配置资产。创建路径：Create → Aesir Modules → UI Canvas Config。
    /// </summary>
    public class UICanvasConfigSO : AesirScriptableObject
    {
        [Tooltip("渲染模式 — 项目固定为 ScreenSpaceCamera，所有 UI Canvas 以相机投影方式渲染")]
        [SerializeField]
        internal RenderMode renderMode = RenderMode.ScreenSpaceCamera;

        [Tooltip("像素完美 — 启用后 UI 元素自动对齐像素边界，避免边缘模糊")]
        [SerializeField]
        internal bool pixelPerfect;

        [Tooltip("平面距离 — Canvas 与相机的距离，仅在 Screen Space - Camera 模式下生效")]
        [SerializeField]
        internal float planeDistance = 100f;

        [Tooltip("排序层名称 — 决定 Canvas 与其他渲染器之间的前后绘制顺序")]
        [SerializeField]
        internal string sortingLayerName = "Default";

        [Tooltip("附加着色器通道 — 默认启用 TexCoord1、Normal、Tangent，供自定义 Lit UI Shader 使用")]
        [SerializeField]
        internal AdditionalCanvasShaderChannels additionalShaderChannels =
            AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.Normal |
            AdditionalCanvasShaderChannels.Tangent;

        [Tooltip("顶点颜色 Gamma 空间 — 启用后顶点颜色始终在 Gamma 色彩空间解释，不受项目 Linear/Gamma 设置影响")]
        [SerializeField]
        internal bool vertexColorAlwaysGammaSpace;

        [Tooltip("缩放模式 — 默认为 ScaleWithScreenSize，UI 按参考分辨率随屏幕尺寸等比缩放")]
        [SerializeField]
        internal CanvasScaler.ScaleMode scaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        [Tooltip("参考分辨率 — UI 设计的基准尺寸，默认 1920×1080，应设置为美术出图分辨率")]
        [SerializeField]
        internal Vector2 referenceResolution = new Vector2(1920, 1080);

        [Tooltip("屏幕匹配模式 — 宽高不一致时的匹配策略，可选匹配宽度、高度或两者加权")]
        [SerializeField]
        internal CanvasScaler.ScreenMatchMode screenMatchMode =
            CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        [Tooltip("缩放因子 — 恒定像素大小模式下的全局缩放倍数，值越大 UI 元素显示越大")]
        [SerializeField]
        internal float scaleFactor = 1f;

        [Tooltip("宽高匹配权重 — 默认 0.5 即宽高等权插值，0 纯匹配宽度，1 纯匹配高度")]
        [Range(0f, 1f)]
        [SerializeField]
        internal float matchWidthOrHeight = 0.5f;

        [Tooltip("参考像素/单位 — 默认 100，即世界空间 1 单位 = 100 像素，影响 UI 在世界空间中的物理大小")]
        [SerializeField]
        internal float referencePixelsPerUnit = 100f;

        [Tooltip("物理单位 — 恒定物理尺寸模式下的度量单位，如厘米、毫米、英寸等")]
        [SerializeField]
        internal CanvasScaler.Unit physicalUnit = CanvasScaler.Unit.Points;

        [Tooltip("回退屏幕 DPI — 无法获取真实屏幕 DPI 时使用的默认值，影响物理尺寸的像素换算精度")]
        [SerializeField]
        internal float fallbackScreenDPI = 96f;

        [Tooltip("默认精灵 DPI — 精灵未设置 PPU 时的回退值，用于物理尺寸缩放计算")]
        [SerializeField]
        internal float defaultSpriteDPI = 96f;

        [Tooltip("忽略反转图形 — 默认启用，背面朝向摄像机的 UI 不参与射线检测，避免误触")]
        [SerializeField]
        internal bool ignoreReversedGraphics = true;

        [Tooltip("阻挡对象 — 指定哪些类型的 2D/3D 物体会阻挡 UI 射线，被遮挡的 UI 不可点击")]
        [SerializeField]
        internal GraphicRaycaster.BlockingObjects blockingObjects = GraphicRaycaster.BlockingObjects.None;

        [Tooltip("阻挡遮罩 — 通过 LayerMask 精确控制哪些层的物体参与阻挡 UI 射线检测")]
        [SerializeField]
        internal LayerMask blockingMask = -1;

        internal bool ScaleModeIsConstantPixelSize => scaleMode == CanvasScaler.ScaleMode.ConstantPixelSize;

        internal bool ScaleModeIsScaleWithScreenSize =>
            scaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize;

        internal bool ScaleModeIsConstantPhysicalSize =>
            scaleMode == CanvasScaler.ScaleMode.ConstantPhysicalSize;

        internal bool ScaleModeIsScaleWithScreenSizeAndScreenMatchModeIsMatchWidthOrHeight =>
            scaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize &&
            screenMatchMode == CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        /// <summary>
        /// 将本资产的所有配置统一应用到目标 Canvas 及其关联的 CanvasScaler 和 GraphicRaycaster。
        /// </summary>
        /// <param name="canvas">目标 Canvas。</param>
        public void ApplyToCanvas(Canvas canvas)
        {
            if (canvas == null)
            {
                return;
            }

            canvas.renderMode = renderMode;
            canvas.planeDistance = planeDistance;
            canvas.pixelPerfect = pixelPerfect;
            canvas.sortingLayerName = sortingLayerName;
            canvas.additionalShaderChannels = additionalShaderChannels;
            canvas.vertexColorAlwaysGammaSpace = vertexColorAlwaysGammaSpace;

            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = scaleMode;
                if (scaleMode == CanvasScaler.ScaleMode.ConstantPixelSize)
                {
                    scaler.scaleFactor = scaleFactor;
                }

                if (scaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
                {
                    scaler.referenceResolution = referenceResolution;
                    scaler.screenMatchMode = screenMatchMode;
                    if (screenMatchMode == CanvasScaler.ScreenMatchMode.MatchWidthOrHeight)
                    {
                        scaler.matchWidthOrHeight = matchWidthOrHeight;
                    }
                }

                if (scaleMode == CanvasScaler.ScaleMode.ConstantPhysicalSize)
                {
                    scaler.physicalUnit = physicalUnit;
                    scaler.fallbackScreenDPI = fallbackScreenDPI;
                    scaler.defaultSpriteDPI = defaultSpriteDPI;
                }

                scaler.referencePixelsPerUnit = referencePixelsPerUnit;
            }

            var raycaster = canvas.GetComponent<GraphicRaycaster>();
            if (raycaster != null)
            {
                raycaster.ignoreReversedGraphics = ignoreReversedGraphics;
                raycaster.blockingObjects = blockingObjects;
                raycaster.blockingMask = blockingMask;
            }
        }

        /// <summary>
        /// 获取项目中所有可用的 Sorting Layer 名称，供 Inspector 下拉选择。
        /// </summary>
        internal static IEnumerable<string> GetSortingLayerNames()
        {
            foreach (var layer in SortingLayer.layers)
            {
                yield return layer.name;
            }
        }

        /// <summary>
        /// 创建一份运行时默认配置实例（不持久化为资产）。
        /// </summary>
        /// <returns>默认配置的 <see cref="UICanvasConfigSO" /> 实例。</returns>
        public static UICanvasConfigSO CreateDefault() => CreateInstance<UICanvasConfigSO>();
    }
}
