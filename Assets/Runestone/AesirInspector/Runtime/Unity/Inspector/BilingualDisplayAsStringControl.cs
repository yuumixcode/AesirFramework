using System;
using UnityEngine;

namespace Runestone.AesirInspector
{
    /// <summary>
    /// 双语字符串显示控件，以字段的形式支持多语言。
    /// </summary>
    [Serializable]
    public class BilingualDisplayAsStringControl
    {
        public int fontSize = 13;
        public TextAlignment alignment = TextAlignment.Left;
        public bool enableRichText = true;
        public string format = "";
        public bool overflow;

        // 使用 public 字段而非自动属性，确保被 Odin/Unity 序列化。
        // 自动属性 { get; set; } 的编译器生成后备字段在 Odin 非 SerializedScriptableObject
        // 嵌套类中不会被序列化，导致 Domain Reload 后值丢失。
        public string ChineseDisplay;
        public string EnglishDisplay;

        public BilingualDisplayAsStringControl() { }

        public BilingualDisplayAsStringControl(string chinese, string english = null)
        {
            ChineseDisplay = chinese;
            EnglishDisplay = english ?? chinese;
        }
    }
}
