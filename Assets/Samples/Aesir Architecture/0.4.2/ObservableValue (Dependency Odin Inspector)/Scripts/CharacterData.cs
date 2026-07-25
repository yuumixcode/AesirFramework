using System;
using UnityEngine;

namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// 引用类型复合数据，用于验证 ObservableValue 对 class 的递归绘制。
    /// </summary>
    [Serializable]
    public class CharacterData : IEquatable<CharacterData>
    {
        [SerializeField]
        string displayName;

        [SerializeField]
        int level;

        [SerializeField]
        Vector2 position;

        public CharacterData()
        {
            displayName = "无名英雄";
            level = 1;
            position = Vector2.zero;
        }

        public string DisplayName => displayName;
        public int Level => level;
        public Vector2 Position => position;

        public bool Equals(CharacterData other)
        {
            if (other is null)
            {
                return false;
            }

            return displayName == other.displayName && level == other.level &&
                   position.Equals(other.position);
        }

        public void SetDisplayName(string name) => displayName = name;
        public void SetLevel(int lv) => level = lv;
        public void SetPosition(Vector2 pos) => position = pos;
    }
}
