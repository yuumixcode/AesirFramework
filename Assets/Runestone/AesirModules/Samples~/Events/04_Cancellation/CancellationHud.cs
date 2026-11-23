#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Runestone.AesirModules.Samples.Events.Cancellation
{
    /// <summary>
    /// 取消传播示例 HUD：运行时自建 Screen Space Canvas + uGUI Text，显示操作说明与事件链日志。
    /// </summary>
    [AddComponentMenu("")]
    public class CancellationHud : MonoBehaviour
    {
        const int MaxLines = 12;

        readonly Queue<string> _lines = new Queue<string>(MaxLines);
        Text _text;

        /// <summary>全局单例入口（场景中预放置一个即可）。</summary>
        public static CancellationHud Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            var canvasGo = new GameObject("CancellationHudCanvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            var textGo = new GameObject("LogText");
            textGo.transform.SetParent(canvasGo.transform, false);
            _text = textGo.AddComponent<Text>();
            _text.font = SampleFont.Create();
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

            Report("HUD", "Space = 取消传播；Shift+Space = 不可取消（四档全执行）");
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>向 HUD 追加一行日志（超出容量时移除最旧行）。</summary>
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
    }

    /// <summary>示例用跨平台中文字体探测（系统字体优先，回退内置字体）。</summary>
    internal static class SampleFont
    {
        public static Font Create()
        {
            var candidates = new[] { "YaHei", "PingFang", "Noto Sans CJK", "WenQuanYi" };
            var installedFonts = Font.GetOSInstalledFontNames();
            foreach (var candidate in candidates)
            {
                foreach (var installedName in installedFonts)
                {
                    if (installedName.IndexOf(candidate, StringComparison.OrdinalIgnoreCase) >= 0)
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
