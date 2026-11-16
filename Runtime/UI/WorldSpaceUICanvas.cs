using Runestone.AesirArchitecture;
using UnityEngine;
using UnityEngine.UI;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 世界空间 UI Canvas 预设组件。挂在场景物体（如角色头顶、交互点）上即可获得一个
    /// <see cref="RenderMode.WorldSpace" /> 渲染根，UI 随该物体在世界空间中的位置移动；
    /// 也可在运行时由 <see cref="UIRoot.CreateWorldSpaceCanvas" /> 挂到任意 <see cref="Transform" /> 上。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 与世界空间跟随目标的两种关系：物体是 Canvas 的<b>父级</b>时直接随父变换移动（如角色头顶血条——
    /// Canvas 挂在角色子节点，不指定 <see cref="FollowTarget" />）；物体是 Canvas 的<b>兄弟节点</b>时
    /// 指定 <see cref="FollowTarget" />，由本组件每帧把 Canvas 的父级变换同步到目标。
    /// 两种情形都支持 <see cref="PositionOffset" />（局部偏移）与 <see cref="FaceCamera" />（面向相机）。
    /// </para>
    /// <para>
    /// 本组件只提供渲染根，不参与 <see cref="UIModule" /> 的面板 / 窗口注册表与生命周期：
    /// 世界空间 UI 的生命周期与宿主场景物体绑定，由业务代码直接驱动子节点内容。
    /// </para>
    /// </remarks>
    [AddComponentMenu("Aesir Modules/UI/World Space UI Canvas")]
    [DisallowMultipleComponent]
    public sealed class WorldSpaceUICanvas : AesirMonoBehaviour
    {
        internal const string FollowTargetFieldName = nameof(followTarget);
        internal const string PositionOffsetFieldName = nameof(positionOffset);
        internal const string FaceCameraFieldName = nameof(faceCamera);
        internal const string BillboardModeFieldName = nameof(billboardMode);
        internal const string FlipFacingFieldName = nameof(flipFacing);
        internal const string CanvasConfigFieldName = nameof(canvasConfig);
        internal const string SortingOrderFieldName = nameof(sortingOrder);

        /// <summary>世界空间 UI 的朝向模式。</summary>
        public enum WorldSpaceBillboardMode
        {
            /// <summary>不自动调整朝向，完全尊重 Canvas 父级的旋转。</summary>
            None = 0,

            /// <summary>面向 UI 相机，忽略 X / Y 轴旋转，保持 Y 轴竖直（适合头顶信息牌）。</summary>
            CameraFacingYLocked = 1,

            /// <summary>完整面向 UI 相机（平面广告牌，适合始终正对屏幕的标记）。</summary>
            CameraFacingFull = 2
        }

        [Tooltip("跟随的世界空间对象。为空时假定本组件已成为该目标的子节点（随父变换移动）")]
        [SerializeField]
        Transform followTarget;

        [Tooltip("相对跟随目标的局部位置偏移（跟随父级模式与显式目标模式均生效）")]
        [SerializeField]
        Vector3 positionOffset;

        [Tooltip("是否每帧面向 UI 相机。具体朝向由 Billboard Mode 决定")]
        [SerializeField]
        bool faceCamera = true;

        [Tooltip("面向 UI 相机的具体模式（Face Camera 开启时生效）")]
        [SerializeField]
        WorldSpaceBillboardMode billboardMode = WorldSpaceBillboardMode.CameraFacingYLocked;

        [Tooltip("朝向翻转：默认开启，使 UI 正面朝向相机；关闭时 Canvas 的 +Z 指向远离相机的一侧（Unity 广告牌惯例）")]
        [SerializeField]
        bool flipFacing = true;

        [Tooltip("世界空间 Canvas 配置。为空时回退 UIRoot 的世界空间配置，再回退内存默认预设")]
        [SerializeField]
        UICanvasConfigSO canvasConfig;

        [Tooltip("Canvas 的 sortingOrder。默认 450，位于面板四层（≤400）与窗口（≥500）之间")]
        [SerializeField]
        int sortingOrder = 450;

        Canvas _canvas;

        /// <summary>根节点上的 Canvas（懒缓存）。</summary>
        public Canvas Canvas => _canvas != null ? _canvas : _canvas = GetComponent<Canvas>();

        /// <summary>当前跟随的世界空间对象（可运行时赋值切换目标，null 表示跟随父变换）。</summary>
        public Transform FollowTarget
        {
            get => followTarget;
            set => followTarget = value;
        }

        /// <summary>相对跟随目标的局部位置偏移。</summary>
        public Vector3 PositionOffset
        {
            get => positionOffset;
            set => positionOffset = value;
        }

        /// <summary>是否每帧面向 UI 相机。</summary>
        public bool FaceCamera
        {
            get => faceCamera;
            set => faceCamera = value;
        }

        /// <summary>面向 UI 相机的具体模式。</summary>
        public WorldSpaceBillboardMode Billboard
        {
            get => billboardMode;
            set => billboardMode = value;
        }

        /// <summary>是否翻转朝向（默认 true = UI 正面朝向相机；false = Canvas +Z 沿相机→Canvas 的 Unity 广告牌惯例）。</summary>
        public bool FlipFacing
        {
            get => flipFacing;
            set => flipFacing = value;
        }

        /// <summary>本组件使用的世界空间 Canvas 配置。</summary>
        public UICanvasConfigSO CanvasConfig => canvasConfig;

        /// <summary>Canvas 的 sortingOrder。</summary>
        public int SortingOrder => sortingOrder;

        /// <summary>
        /// 用当前配置重新构建并接线 Canvas（渲染模式 / 缩放密度 / 射线检测 / sortingOrder / UI 层）。
        /// 通常在 Awake 中自动调用；运行时修改配置字段后可手动调用本方法即时生效。
        /// </summary>
        /// <param name="overrideConfig">可选的配置覆盖。非 null 时写入序列化字段并作为本次配置来源，便于运行时换配置。</param>
        public void Build(UICanvasConfigSO overrideConfig = null)
        {
            EnsureCanvasComponents();

            if (overrideConfig != null)
            {
                canvasConfig = overrideConfig;
            }

            var config = ResolveConfig();
            config.ApplyToWorldSpaceCanvas(Canvas);
            Canvas.sortingOrder = sortingOrder;
            UIRoot.SetLayerRecursively(transform, UIRoot.UILayerIndex);
            ApplyTransform(computeBillboard: false);
        }

        void Awake() => Build();

        void LateUpdate() => ApplyTransform(computeBillboard: true);

        UICanvasConfigSO ResolveConfig()
        {
            if (canvasConfig != null)
            {
                return canvasConfig;
            }

            var root = UIRoot.TryGetExisting();
            if (root != null)
            {
                var rootConfig = root.WorldSpaceCanvasConfig;
                if (rootConfig != null)
                {
                    return rootConfig;
                }
            }

            canvasConfig = UICanvasConfigSO.CreateWorldSpaceDefault();
            return canvasConfig;
        }

        void EnsureCanvasComponents()
        {
            if (GetComponent<RectTransform>() == null)
            {
                gameObject.AddComponent<RectTransform>();
            }

            if (Canvas == null)
            {
                _canvas = gameObject.AddComponent<Canvas>();
            }

            if (GetComponent<CanvasScaler>() == null)
            {
                gameObject.AddComponent<CanvasScaler>();
            }

            if (GetComponent<GraphicRaycaster>() == null)
            {
                gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        void ApplyTransform(bool computeBillboard)
        {
            var target = followTarget != null ? followTarget : transform.parent;
            if (target != null && transform.parent != target)
            {
                transform.SetParent(target, false);
            }

            if (transform.localPosition != positionOffset)
            {
                transform.localPosition = positionOffset;
            }

            if (!computeBillboard || !faceCamera || billboardMode == WorldSpaceBillboardMode.None)
            {
                return;
            }

            var camera = GetWorldUICamera();
            if (camera == null)
            {
                return;
            }

            // FlipFacing=true（默认）：Canvas +Z 指向相机（UI 正面朝相机）；false：+Z 沿相机→Canvas 方向（Unity 广告牌惯例）。
            var direction = flipFacing
                ? camera.transform.position - transform.position
                : transform.position - camera.transform.position;
            if (billboardMode == WorldSpaceBillboardMode.CameraFacingYLocked)
            {
                direction.y = 0f;
                if (direction.sqrMagnitude > 0.0001f)
                {
                    transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
                }

                return;
            }

            transform.rotation = Quaternion.LookRotation(direction, camera.transform.up);
        }

        Camera GetWorldUICamera()
        {
            var root = UIRoot.TryGetExisting();
            if (root != null && root.UICamera != null)
            {
                return root.UICamera;
            }

            return Camera.main;
        }
    }
}
