using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// 简单类型 ObservableValue Inspector 演示组件。
    /// <para>每种类型同时展示原始字段与 ObservableValue 字段，方便对比 Inspector 绘制效果。</para>
    /// <para>进入 PlayMode 后，在 Inspector 中拖拽/修改 ObservableValue 字段值会触发订阅回调并打印日志。</para>
    /// <para>通过 Button 可用代码修改值，验证事件触发。</para>
    /// </summary>
    public class ObservableValueSimpleSample : MonoBehaviour
    {
        [Header("int — 对比")]
        public int plainHp = 100;

        [SerializeField]
        ObservableValue<int> observableHp = new ObservableValue<int>(100);

        [Header("float — 对比")]
        public float plainSpeed = 5.5f;

        [SerializeField]
        ObservableValue<float> observableSpeed = new ObservableValue<float>(5.5f);

        [Header("string — 对比")]
        public string plainName = "冒险者";

        [SerializeField]
        ObservableValue<string> observableName = new ObservableValue<string>("冒险者");

        [Header("bool — 对比")]
        public bool plainIsAlive = true;

        [SerializeField]
        ObservableValue<bool> observableIsAlive = new ObservableValue<bool>(true);

        [Header("enum — 对比")]
        public SampleAlignment plainAlignment = SampleAlignment.Neutral;

        [SerializeField]
        ObservableValue<SampleAlignment> observableAlignment =
            new ObservableValue<SampleAlignment>(SampleAlignment.Neutral);

        [Header("Vector2 — 对比")]
        public Vector2 plainPosition = Vector2.zero;

        [SerializeField]
        ObservableValue<Vector2> observablePosition = new ObservableValue<Vector2>(Vector2.zero);

        [Header("Vector3 — 对比")]
        public Vector3 plainVelocity = Vector3.zero;

        [SerializeField]
        ObservableValue<Vector3> observableVelocity = new ObservableValue<Vector3>(Vector3.zero);

        AutoRemoveListenerHandle _hpSub, _speedSub, _nameSub, _aliveSub, _alignSub, _posSub, _velSub;

        public IReadOnlyObservableValue<int> ObservableHp => observableHp;
        public IReadOnlyObservableValue<float> ObservableSpeed => observableSpeed;
        public IReadOnlyObservableValue<string> ObservableName => observableName;
        public IReadOnlyObservableValue<bool> ObservableIsAlive => observableIsAlive;
        public IReadOnlyObservableValue<SampleAlignment> ObservableAlignment => observableAlignment;
        public IReadOnlyObservableValue<Vector2> ObservablePosition => observablePosition;
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

        [Title("代码修改测试")]
        [Button("HP +10", ButtonSizes.Medium)]
        void AddHp() => observableHp.Value += 10;

        [Button("Speed +1", ButtonSizes.Medium)]
        void AddSpeed() => observableSpeed.Value += 1f;

        [Button("Name 加后缀", ButtonSizes.Medium)]
        void AppendName() => observableName.Value += "_X";

        [Button("Toggle IsAlive", ButtonSizes.Medium)]
        void ToggleAlive() => observableIsAlive.Value = !observableIsAlive.Value;

        [Button("Alignment 切换", ButtonSizes.Medium)]
        void CycleAlignment() => observableAlignment.Value =
            (SampleAlignment)(((int)observableAlignment.Value + 1) % 4);

        [Button("Position X+1", ButtonSizes.Medium)]
        void AddPosX() => observablePosition.Value += new Vector2(1, 0);

        [Button("Velocity Y+1", ButtonSizes.Medium)]
        void AddVelY() => observableVelocity.Value += new Vector3(0, 1, 0);
    }

    /// <summary>
    /// 示例用枚举，验证 enum 类型在 Drawer 中的绘制。
    /// </summary>
    public enum SampleAlignment
    {
        Neutral,
        Good,
        Evil,
        Chaotic
    }
}
