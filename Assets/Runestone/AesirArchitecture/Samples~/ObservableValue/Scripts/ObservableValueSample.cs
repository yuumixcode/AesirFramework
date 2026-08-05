using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// 简单类型 ObservableValue Inspector 演示组件。
    /// </summary>
    /// <remarks>
    /// 展示 <see cref="ObservableValue{T}"/> 对常见基础类型（int、float、string、bool、enum、Vector2、Vector3）的支持。
    /// 每种类型同时展示"原始字段"与"ObservableValue 字段"进行对比，
    /// 方便直观地观察 ObservableValue 在 Inspector 中的绘制效果与事件触发行为。
    /// <para>进入 PlayMode 后，在 Inspector 中修改 ObservableValue 字段值会触发订阅回调并打印日志；
    /// 也可以通过下方的 Button 按钮以代码方式修改值，验证事件触发。</para>
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.ObservableValue{T}"/>
    /// <seealso cref="Runestone.AesirArchitecture.IReadOnlyObservableValue{T}"/>
    /// <seealso cref="SampleAlignment"/>
    public class ObservableValueSimpleSample : MonoBehaviour
    {
        /// <summary>
        /// 原始 int 字段，用于对比 Inspector 绘制效果（无事件通知）。
        /// </summary>
        [Header("int — 对比")]
        public int plainHp = 100;

        /// <summary>
        /// ObservableValue 包装的 HP 字段，修改 Value 时自动触发事件通知。
        /// </summary>
        [SerializeField]
        ObservableValue<int> observableHp = new ObservableValue<int>(100);

        /// <summary>
        /// 原始 float 字段，用于对比 Inspector 绘制效果（无事件通知）。
        /// </summary>
        [Header("float — 对比")]
        public float plainSpeed = 5.5f;

        /// <summary>
        /// ObservableValue 包装的速度字段，修改 Value 时自动触发事件通知。
        /// </summary>
        [SerializeField]
        ObservableValue<float> observableSpeed = new ObservableValue<float>(5.5f);

        /// <summary>
        /// 原始 string 字段，用于对比 Inspector 绘制效果（无事件通知）。
        /// </summary>
        [Header("string — 对比")]
        public string plainName = "冒险者";

        /// <summary>
        /// ObservableValue 包装的名称字段，修改 Value 时自动触发事件通知。
        /// </summary>
        [SerializeField]
        ObservableValue<string> observableName = new ObservableValue<string>("冒险者");

        /// <summary>
        /// 原始 bool 字段，用于对比 Inspector 绘制效果（无事件通知）。
        /// </summary>
        [Header("bool — 对比")]
        public bool plainIsAlive = true;

        /// <summary>
        /// ObservableValue 包装的存活状态字段，修改 Value 时自动触发事件通知。
        /// </summary>
        [SerializeField]
        ObservableValue<bool> observableIsAlive = new ObservableValue<bool>(true);

        /// <summary>
        /// 原始 enum 字段，用于对比 Inspector 绘制效果（无事件通知）。
        /// </summary>
        [Header("enum — 对比")]
        public SampleAlignment plainAlignment = SampleAlignment.Neutral;

        /// <summary>
        /// ObservableValue 包装的阵营字段，修改 Value 时自动触发事件通知。
        /// </summary>
        [SerializeField]
        ObservableValue<SampleAlignment> observableAlignment =
            new ObservableValue<SampleAlignment>(SampleAlignment.Neutral);

        /// <summary>
        /// 原始 Vector2 字段，用于对比 Inspector 绘制效果（无事件通知）。
        /// </summary>
        [Header("Vector2 — 对比")]
        public Vector2 plainPosition = Vector2.zero;

        /// <summary>
        /// ObservableValue 包装的位置字段，修改 Value 时自动触发事件通知。
        /// </summary>
        [SerializeField]
        ObservableValue<Vector2> observablePosition = new ObservableValue<Vector2>(Vector2.zero);

        /// <summary>
        /// 原始 Vector3 字段，用于对比 Inspector 绘制效果（无事件通知）。
        /// </summary>
        [Header("Vector3 — 对比")]
        public Vector3 plainVelocity = Vector3.zero;

        /// <summary>
        /// ObservableValue 包装的速度向量字段，修改 Value 时自动触发事件通知。
        /// </summary>
        [SerializeField]
        ObservableValue<Vector3> observableVelocity = new ObservableValue<Vector3>(Vector3.zero);

        AutoRemoveListenerHandle _hpSub, _speedSub, _nameSub, _aliveSub, _alignSub, _posSub, _velSub;

        /// <summary>HP 可观察值（只读视图），供外部监听但不允许修改。</summary>
        public IReadOnlyObservableValue<int> ObservableHp => observableHp;

        /// <summary>速度可观察值（只读视图），供外部监听但不允许修改。</summary>
        public IReadOnlyObservableValue<float> ObservableSpeed => observableSpeed;

        /// <summary>名称可观察值（只读视图），供外部监听但不允许修改。</summary>
        public IReadOnlyObservableValue<string> ObservableName => observableName;

        /// <summary>存活状态可观察值（只读视图），供外部监听但不允许修改。</summary>
        public IReadOnlyObservableValue<bool> ObservableIsAlive => observableIsAlive;

        /// <summary>阵营可观察值（只读视图），供外部监听但不允许修改。</summary>
        public IReadOnlyObservableValue<SampleAlignment> ObservableAlignment => observableAlignment;

        /// <summary>位置可观察值（只读视图），供外部监听但不允许修改。</summary>
        public IReadOnlyObservableValue<Vector2> ObservablePosition => observablePosition;

        /// <summary>速度向量可观察值（只读视图），供外部监听但不允许修改。</summary>
        public IReadOnlyObservableValue<Vector3> ObservableVelocity => observableVelocity;

        void OnEnable()
        {
            _hpSub = observableHp.AddListener(v => Debug.Log($"[Simple] HP → {v}"));
            _speedSub = observableSpeed.AddListener(v => Debug.Log($"[Simple] Speed → {v}"));
            _nameSub = observableName.AddListener(v => Debug.Log($"[Simple] Name → {v}"));
            _aliveSub = observableIsAlive.AddListener(v => Debug.Log($"[Simple] IsAlive → {v}"));
            _alignSub = observableAlignment.AddListener(v => Debug.Log($"[Simple] Alignment → {v}"));
            _posSub = observablePosition.AddListener(v => Debug.Log($"[Simple] Position → {v}"));
            _velSub = observableVelocity.AddListener(v => Debug.Log($"[Simple] Velocity → {v}"));
        }

        void OnDisable()
        {
            _hpSub.Dispose();
            _speedSub.Dispose();
            _nameSub.Dispose();
            _aliveSub.Dispose();
            _alignSub.Dispose();
            _posSub.Dispose();
            _velSub.Dispose();
        }

        /// <summary>通过代码将 HP 增加 10，验证 ObservableValue 事件触发。</summary>
        [Title("代码修改测试")]
        [Button("HP +10", ButtonSizes.Medium)]
        void AddHp() => observableHp.Value += 10;

        /// <summary>通过代码将速度增加 1，验证 ObservableValue 事件触发。</summary>
        [Button("Speed +1", ButtonSizes.Medium)]
        void AddSpeed() => observableSpeed.Value += 1f;

        /// <summary>通过代码在名称后追加后缀，验证 ObservableValue 事件触发。</summary>
        [Button("Name 加后缀", ButtonSizes.Medium)]
        void AppendName() => observableName.Value += "_X";

        /// <summary>通过代码切换存活状态，验证 ObservableValue 事件触发。</summary>
        [Button("Toggle IsAlive", ButtonSizes.Medium)]
        void ToggleAlive() => observableIsAlive.Value = !observableIsAlive.Value;

        /// <summary>通过代码循环切换阵营，验证 ObservableValue 对 enum 类型的事件触发。</summary>
        [Button("Alignment 切换", ButtonSizes.Medium)]
        void CycleAlignment() => observableAlignment.Value =
            (SampleAlignment)(((int)observableAlignment.Value + 1) % 4);

        /// <summary>通过代码将位置 X 轴 +1，验证 ObservableValue 对 Vector2 类型的事件触发。</summary>
        [Button("Position X+1", ButtonSizes.Medium)]
        void AddPosX() => observablePosition.Value += new Vector2(1, 0);

        /// <summary>通过代码将速度 Y 轴 +1，验证 ObservableValue 对 Vector3 类型的事件触发。</summary>
        [Button("Velocity Y+1", ButtonSizes.Medium)]
        void AddVelY() => observableVelocity.Value += new Vector3(0, 1, 0);
    }

    /// <summary>
    /// 示例用枚举，验证 enum 类型在 ObservableValue Drawer 中的绘制与事件触发。
    /// </summary>
    /// <remarks>
    /// 用于演示 ObservableValue 对枚举类型的完整支持，
    /// 包括 Inspector 下拉选择和值变更时的自动事件通知。
    /// </remarks>
    /// <seealso cref="ObservableValueSimpleSample"/>
    public enum SampleAlignment
    {
        Neutral,
        Good,
        Evil,
        Chaotic
    }
}
