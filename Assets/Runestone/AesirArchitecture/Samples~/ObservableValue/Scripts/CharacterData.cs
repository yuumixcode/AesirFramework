using System;
using UnityEngine;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// 引用类型复合数据，用于验证 ObservableValue 对 class 的递归绘制与变更检测。
    /// </summary>
    /// <remarks>
    /// <see cref="ObservableValue{T}"/> 对引用类型（class）的变更检测依赖值比较而非引用比较。
    /// 本类实现 <see cref="IEquatable{T}"/> 以提供逐字段比较，
    /// 确保修改子字段后调用 <c>InvokeEvent()</c> 时能正确判断值是否变化。
    /// <para>所有字段通过 [SerializeField] 标记，使 Unity Inspector 可以直接编辑，
    /// 配合 ObservableValue 的自定义 Drawer 实现可视化编辑。</para>
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.ObservableValue{T}"/>
    /// <seealso cref="WeaponStats"/>
    [Serializable]
    public class CharacterData : IEquatable<CharacterData>
    {
        [SerializeField]
        string displayName;

        [SerializeField]
        int level;

        [SerializeField]
        Vector2 position;

        /// <summary>
        /// 使用默认值构造角色数据。
        /// </summary>
        /// <remarks>
        /// 默认名称为"无名英雄"，等级为 1，位置为原点。
        /// </remarks>
        public CharacterData()
        {
            displayName = "无名英雄";
            level = 1;
            position = Vector2.zero;
        }

        /// <summary>角色显示名称。</summary>
        public string DisplayName => displayName;

        /// <summary>角色当前等级。</summary>
        public int Level => level;

        /// <summary>角色在世界空间中的二维坐标。</summary>
        public Vector2 Position => position;

        /// <summary>
        /// 逐字段比较两个 CharacterData 是否相等。
        /// </summary>
        /// <param name="other">需要比较的另一个 CharacterData 实例。</param>
        /// <returns>所有字段值均相等则返回 <c>true</c>，否则返回 <c>false</c>。</returns>
        /// <remarks>
        /// 此方法供 ObservableValue 的变更检测使用：只有值确实变化时才触发监听回调，
        /// 避免设置相同值时产生无效刷新。
        /// </remarks>
        public bool Equals(CharacterData other)
        {
            if (other is null)
            {
                return false;
            }

            return displayName == other.displayName && level == other.level &&
                   position.Equals(other.position);
        }

        /// <summary>
        /// 设置角色显示名称。
        /// </summary>
        /// <param name="name">新的显示名称。</param>
        public void SetDisplayName(string name) => displayName = name;

        /// <summary>
        /// 设置角色等级。
        /// </summary>
        /// <param name="lv">新的等级值。</param>
        public void SetLevel(int lv) => level = lv;

        /// <summary>
        /// 设置角色位置坐标。
        /// </summary>
        /// <param name="pos">新的二维坐标。</param>
        public void SetPosition(Vector2 pos) => position = pos;
    }
}
