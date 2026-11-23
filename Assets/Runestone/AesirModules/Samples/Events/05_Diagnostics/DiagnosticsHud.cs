#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Runestone.AesirModules.Samples.Events.Diagnostics
{
    /// <summary>
    /// 诊断示例 HUD：运行时自建 Screen Space Canvas + uGUI Text，实时展示公开诊断 API 的数据。
    /// </summary>
    [AddComponentMenu("")]
    public class DiagnosticsHud : MonoBehaviour
    {
        Text _text;

        /// <summary>全局单例入口（场景中预放置一个即可）。</summary>
        public static DiagnosticsHud Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            var canvasGo = new GameObject("DiagnosticsHudCanvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            var textGo = new GameObject("LogText");
            textGo.transform.SetParent(canvasGo.transform, false);
            _text = textGo.AddComponent<Text>();
            _text.font = SampleFont.Create();
            _text.fontSize = 20;
            _text.color = Color.white;
            _text.alignment = TextAnchor.UpperLeft;
            _text.horizontalOverflow = HorizontalWrapMode.Overflow;
            _text.verticalOverflow = VerticalWrapMode.Overflow;

            var rect = _text.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(16f, -16f);
            rect.sizeDelta = new Vector2(1400f, 900f);
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>用诊断公开 API 的快照数据刷新 HUD 文本。</summary>
        public static void Render(string content)
        {
            var hud = Instance;
            if (hud != null && hud._text != null)
            {
                hud._text.text = content;
            }
        }
    }

    /// <summary>示例用跨平台中文字体探测。</summary>
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
                        return Font.CreateDynamicFontFromOSFont(installedName, 20);
                    }
                }
            }

            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
    }
}
#endif
