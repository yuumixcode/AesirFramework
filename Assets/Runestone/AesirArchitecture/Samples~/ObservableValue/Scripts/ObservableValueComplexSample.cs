using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// 复合类型 ObservableValue Inspector 演示组件。
    /// </summary>
    /// <remarks>
    /// 展示 <see cref="ObservableValue{T}"/> 对复杂类型（struct 和 class）的支持。
    /// 每种类型同时展示"原始字段"与"ObservableValue 字段"进行对比，
    /// 方便直观地观察 ObservableValue 在 Inspector 中的绘制效果与事件触发行为。
    /// <para>struct 类型（<see cref="WeaponStats"/>）：因 struct 为值类型，修改子字段需整体替换 Value，
    /// 否则 ObservableValue 无法检测到内部字段变化。</para>
    /// <para>class 类型（<see cref="CharacterData"/>）：因 class 为引用类型，可直接修改子字段后调用
    /// <c>InvokeEvent()</c> 手动触发变更通知。</para>
    /// <para>进入 PlayMode 后，修改子字段值或点击 Button 会触发订阅回调并打印日志。</para>
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.ObservableValue{T}"/>
    /// <seealso cref="Runestone.AesirArchitecture.IReadOnlyObservableValue{T}"/>
    /// <seealso cref="WeaponStats"/>
    /// <seealso cref="CharacterData"/>
    public class ObservableValueComplexSample : MonoBehaviour
    {
        /// <summary>
        /// 原始 struct 字段，用于对比 Inspector 绘制效果（无事件通知）。
        /// </summary>
        [Header("struct (WeaponStats) — 对比")]
        public WeaponStats plainWeapon = new WeaponStats("铁剑", 15, 0.1f);

        /// <summary>
        /// ObservableValue 包装的 struct 字段，修改 Value 时自动触发事件通知。
        /// </summary>
        [SerializeField]
        ObservableValue<WeaponStats> observableWeapon =
            new ObservableValue<WeaponStats>(new WeaponStats("铁剑", 15, 0.1f));

        /// <summary>
        /// 原始 class 字段，用于对比 Inspector 绘制效果（无事件通知）。
        /// </summary>
        [Header("class (CharacterData) — 对比")]
        public CharacterData plainCharacter = new CharacterData();

        /// <summary>
        /// ObservableValue 包装的 class 字段，修改子字段后需调用 <c>InvokeEvent()</c> 触发通知。
        /// </summary>
        [SerializeField]
        ObservableValue<CharacterData> observableCharacter =
            new ObservableValue<CharacterData>(new CharacterData());

        AutoRemoveListenerHandle _weaponSub, _charSub;

        /// <summary>
        /// 武器属性的可观察值（只读视图），供外部监听但不允许修改。
        /// </summary>
        public IReadOnlyObservableValue<WeaponStats> ObservableWeapon => observableWeapon;

        /// <summary>
        /// 角色数据的可观察值（只读视图），供外部监听但不允许修改。
        /// </summary>
        public IReadOnlyObservableValue<CharacterData> ObservableCharacter => observableCharacter;

        void OnEnable()
        {
            _weaponSub = observableWeapon.AddListener(v =>
                Debug.Log($"[Complex] Weapon → {v.WeaponName} (ATK {v.AttackPower})"));
            _charSub = observableCharacter.AddListener(v =>
                Debug.Log($"[Complex] Character → {v.DisplayName} Lv.{v.Level}"));
        }

        void OnDisable()
        {
            _weaponSub.Dispose();
            _charSub.Dispose();
        }

        /// <summary>
        /// 通过代码修改武器攻击力（+5），验证 ObservableValue 事件触发。
        /// </summary>
        /// <remarks>
        /// struct 为值类型，必须构造新实例整体赋值给 <c>Value</c>，
        /// 否则 ObservableValue 无法感知内部字段的变化。
        /// </remarks>
        [Title("代码修改测试")]
        [Button("Weapon ATK +5", ButtonSizes.Medium)]
        void AddWeaponAtk() => observableWeapon.Value = new WeaponStats(observableWeapon.Value.WeaponName,
            observableWeapon.Value.AttackPower + 5, observableWeapon.Value.CriticalRate);

        /// <summary>
        /// 通过代码修改角色等级（+1），验证 ObservableValue 事件触发。
        /// </summary>
        /// <remarks>
        /// class 为引用类型，可以直接修改子字段后调用 <c>InvokeEvent()</c> 手动触发通知。
        /// 与 struct 不同，无需构造新实例整体替换。
        /// </remarks>
        [Button("Character Level +1", ButtonSizes.Medium)]
        void AddCharLevel()
        {
            observableCharacter.Value.SetLevel(observableCharacter.Value.Level + 1);
            observableCharacter.InvokeEvent();
        }
    }
}
