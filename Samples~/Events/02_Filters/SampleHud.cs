#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Runestone.AesirModules.Samples.Events.Filters
{
    /// <summary>
    /// 示例 HUD。运行时自建 Screen Space Canvas + uGUI Text，显示操作说明与事件接收日志。
    /// 中文使用跨平台动态字体（系统字体探测），全不可用时回退内置字体。
    /// </summary>
    [AddComponentMenu("")]
    public class SampleHud : MonoBehaviour
    {
        const int MaxLines = 12;

        /// <summary>全局单例入口（场景中预放置一个即可）。</summary>
        public static SampleHud Instance { get; private set; }

        readonly Queue<string> _lines = new Queue<string>(MaxLines);
        Text _text;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            var canvasGo = new GameObject("SampleHudCanvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<UnityEngine.UI.CanvasScaler>().uiScaleMode =
                CanvasScaler.ScaleMode.ScaleWithScreenSize;

            var textGo = new GameObject("LogText");
            textGo.transform.SetParent(canvasGo.transform, false);
            _text = textGo.AddComponent<Text>();
            _text.font = CreateChineseFont();
            _text.fontSize = 22;
            _text.color = Color.white;
            _text.alignment = TextAnchor.UpperLeft;
            _text.horizontalOverflow = HorizontalWrapMode.Overflow;
            _text.verticalOverflow = VerticalWrapMode.Overflow;

            var rect = _text.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(16f, -16f);
            rect.sizeDelta = new Vector2(1200f, 800f);

            Report("HUD", "示例启动。Space = 警报（Tag+圈内），R = 家族命令（OnlySelf）");
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// 向 HUD 追加一行日志（超出容量时移除最旧行）。
        /// </summary>
        public static void Report(string source, string message)
        {
            var hud = Instance;
            if (hud == null)
            {
                return;
            }

            hud._lines.Enqueue($"[{source}] {message}");
            while (hud._lines.Count > MaxLines)
            {
                hud._lines.Dequeue();
            }

            hud._text.text = string.Join("\n", hud._lines);
        }

        /// <summary>
        /// 跨平台中文字体：按候选优先级探测系统字体生成动态字体，
        /// 全部不可用时回退内置字体（无中文字形但至少可见）。
        /// </summary>
        static Font CreateChineseFont()
        {
            var candidates = new[] { "YaHei", "PingFang", "Noto Sans CJK", "WenQuanYi" };
            var installedFonts = Font.GetOSInstalledFontNames();
            foreach (var candidate in candidates)
            {
                foreach (var installedName in installedFonts)
                {
                    if (installedName.IndexOf(candidate, System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return Font.CreateDynamicFontFromOSFont(installedName, 22);
                    }
                }
            }

            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
    }
}
#endif
