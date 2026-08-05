using System;
using UnityEngine;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// 自定义可序列化复合类型（struct），用于验证 ObservableValue 的递归绘制与 Odin 特性兼容性。
    /// </summary>
    /// <remarks>
    /// 作为值类型（struct），WeaponStats 在赋值时按值复制，
    /// 因此配合 ObservableValue 使用时需要构造新实例整体赋值给 <c>Value</c>，
    /// 而非直接修改内部字段（修改内部字段无法触发变更通知）。
    /// <para>所有字段通过 [SerializeField] 标记，使 Unity Inspector 可以直接编辑，
    /// 配合 ObservableValue 的自定义 Drawer 实现可视化编辑。</para>
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.ObservableValue{T}"/>
    /// <seealso cref="CharacterData"/>
    [Serializable]
    public struct WeaponStats
    {
        [SerializeField]
        string weaponName;

        [SerializeField]
        int attackPower;

        [SerializeField]
        float criticalRate;

        /// <summary>武器名称。</summary>
        public string WeaponName => weaponName;

        /// <summary>攻击力数值。</summary>
        public int AttackPower => attackPower;

        /// <summary>暴击率（0~1 之间的浮点数）。</summary>
        public float CriticalRate => criticalRate;

        /// <summary>
        /// 构造一个武器属性实例。
        /// </summary>
        /// <param name="name">武器名称。</param>
        /// <param name="atk">攻击力数值。</param>
        /// <param name="crit">暴击率（0~1）。</param>
        public WeaponStats(string name, int atk, float crit)
        {
            weaponName = name;
            attackPower = atk;
            criticalRate = crit;
        }
    }
}
