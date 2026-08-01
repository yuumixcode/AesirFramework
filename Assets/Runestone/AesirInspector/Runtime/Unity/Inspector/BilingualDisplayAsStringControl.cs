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

        public BilingualDisplayAsStringControl(string chinese, string english = null)
        {
            ChineseDisplay = chinese;
            EnglishDisplay = english ?? chinese;
        }

        public string ChineseDisplay { get; set; }
        public string EnglishDisplay { get; set; }
    }
}
