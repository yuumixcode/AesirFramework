#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Runestone.AesirModules.Samples.UI.BasicUsage
{
    /// <summary>
    /// 操作日志 HUD（Top 层 Panel 形态）。窗口与蒙版的每次操作经 <see cref="Log" /> 打到屏上，
    /// 文字使用系统动态中文字体（<see cref="SampleUIFonts" /> 统一提供）。
    /// </summary>
    [AddComponentMenu("")]
    public class SampleUIHud : AesirBasePanel
    {
        [SerializeField] Text logText;

        static readonly List<string> Logs = new List<string>();

        static SampleUIHud _instance;

        void OnEnable()
        {
            _instance = this;
            SampleUIFonts.Apply(transform);
            RefreshLog();
        }

        void OnDisable()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        /// <summary>记录一条操作日志（窗口/蒙版行为均会调用），超过 7 条滚动丢弃。</summary>
        public static void Log(string message)
        {
            Logs.Add(DateTime.Now.ToString("HH:mm:ss") + "  " + message);
            if (Logs.Count > 7)
            {
                Logs.RemoveAt(0);
            }

            if (_instance != null)
            {
                _instance.RefreshLog();
            }
        }

        void RefreshLog()
        {
            if (logText != null)
            {
                logText.text = string.Join("\n", Logs);
            }
        }
    }
}
#endif
