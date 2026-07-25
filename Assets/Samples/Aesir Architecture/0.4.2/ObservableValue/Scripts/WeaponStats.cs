using System;
using UnityEngine;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// 自定义可序列化复合类型，用于验证 ObservableValue 的递归绘制与 Odin 特性兼容性。
    /// </summary>
    [Serializable]
    public struct WeaponStats
    {
        [SerializeField]
        string weaponName;

        [SerializeField]
        int attackPower;

        [SerializeField]
        float criticalRate;

        public string WeaponName => weaponName;
        public int AttackPower => attackPower;
        public float CriticalRate => criticalRate;

        public WeaponStats(string name, int atk, float crit)
        {
            weaponName = name;
            attackPower = atk;
            criticalRate = crit;
        }
    }
}
