#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Runestone.AesirModules.Samples.Events.SOAsset
{
    /// <summary>
    /// 分数灯泡。<see cref="Flash" /> 绑定到同物体 <see cref="UnityEventOnAesirEvent" /> 的
    /// On Raised（Inspector 中以 UnityEvent 配置）——演示非程序员零代码串联事件回调：
    /// 事件到达 → UnityEvent 自动调用灯泡反馈。本组件自身不写任何订阅代码。
    /// </summary>
    [AddComponentMenu("")]
    public class ScoreBulb : MonoBehaviour
    {
        static readonly int ColorId = Shader.PropertyToID("_BaseColor");

        /// <summary>收到事件后的反馈颜色。</summary>
        [SerializeField]
        Color flashColor = Color.yellow;

        Vector3 _baseScale;
        float _bounceTimer;
        Text _counterLabel;
        MaterialPropertyBlock _propertyBlock;
        int _receiveCount;

        Renderer _renderer;

        void Awake()
        {
            _renderer = GetComponentInChildren<Renderer>();
            _propertyBlock = new MaterialPropertyBlock();
            _baseScale = transform.localScale;
            CreateCounterLabel();
        }

        void Update()
        {
            if (_bounceTimer <= 0f)
            {
                return;
            }

            // 弹跳动画：0.3 秒内放大后回落
            _bounceTimer -= Time.deltaTime;
            var t = Mathf.Clamp01(_bounceTimer / 0.3f);
            transform.localScale = _baseScale * (1f + 0.4f * Mathf.Sin(t * Mathf.PI));
        }

        /// <summary>
        /// UnityEvent 桥接回调：由 <see cref="UnityEventOnAesirEvent" /> 的 On Raised 调用，无参数。
        /// </summary>
        public void Flash()
        {
            _receiveCount++;
            _bounceTimer = 0.3f;

            if (_renderer != null)
            {
                _renderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor(ColorId, flashColor);
                _renderer.SetPropertyBlock(_propertyBlock);
            }

            if (_counterLabel != null)
            {
                _counterLabel.text = $"灯泡收到 ×{_receiveCount}";
            }

            Debug.Log($"[ScoreBulb] 第 {_receiveCount} 次经 UnityEvent 收到事件（零订阅代码）", this);
        }

        /// <summary>
        /// 运行时自建计数标签（Screen Space Canvas + uGUI Text + 跨平台中文字体）。
        /// </summary>
        void CreateCounterLabel()
        {
            var canvasGo = new GameObject("ScoreBulbCanvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var textGo = new GameObject("CounterText");
            textGo.transform.SetParent(canvasGo.transform, false);
            _counterLabel = textGo.AddComponent<Text>();
            _counterLabel.font = CreateChineseFont();
            _counterLabel.fontSize = 24;
            _counterLabel.color = Color.white;
            _counterLabel.alignment = TextAnchor.MiddleCenter;
            _counterLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
            _counterLabel.verticalOverflow = VerticalWrapMode.Overflow;

            var rect = _counterLabel.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -16f);
            rect.sizeDelta = new Vector2(600f, 40f);
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
                    if (installedName.IndexOf(candidate, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return Font.CreateDynamicFontFromOSFont(installedName, 24);
                    }
                }
            }

            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
    }
}
#endif
