#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Runestone.AesirArchitecture.Samples.PlaneWarMono
{
    /// <summary>
    /// 飞机大战原生版示例 —— 游戏 HUD（得分 + 当前时间 + 游戏结束提示）。
    /// </summary>
    /// <remarks>
    /// 原生写法：HUD 在 <c>Update</c> 里每帧轮询 GameManager 的得分并重建字符串；
    /// 当前时间直接取系统时钟。游戏结束时切换为结束文案，并允许按 Space 重开。
    /// <para>
    /// 字体使用系统动态生成（Windows / macOS / Linux 均兼容），避免引入全局字体资产。
    /// </para>
    /// </remarks>
    public class SamplePlaneWarMonoGameHUD : MonoBehaviour
    {
        /// <summary>
        /// 跨平台中文系统字体候选（按优先级从左到右探测）。
        /// </summary>
        static readonly string[] FontCandidates =
        {
            "Microsoft YaHei",  // Windows 简体中文
            "Microsoft YaHei UI",
            "PingFang SC",      // macOS 简体中文
            "Noto Sans CJK SC", // Linux / 跨平台
            "WenQuanYi Zen Hei" // 部分 Linux 发行版
        };

        /// <summary>
        /// 得分文本。
        /// </summary>
        [SerializeField]
        Text scoreText;

        /// <summary>
        /// 当前时间文本。
        /// </summary>
        [SerializeField]
        Text timeText;

        /// <summary>
        /// 游戏结束提示文本。
        /// </summary>
        [SerializeField]
        Text gameOverText;

        Font _dynamicFont;

        void Awake()
        {
            _dynamicFont = CreateSystemFont();
            scoreText.font = _dynamicFont;
            timeText.font = _dynamicFont;
            gameOverText.font = _dynamicFont;
        }

        void Update()
        {
            if (SamplePlaneWarMonoGameManager.Instance.IsGameOver)
            {
                scoreText.text = "游戏结束";
                timeText.text = $"得分: {SamplePlaneWarMonoGameManager.Instance.Score}";
                gameOverText.text = "[按 Space 继续]";
            }
            else
            {
                scoreText.text = $"得分: {SamplePlaneWarMonoGameManager.Instance.Score}";
                timeText.text = DateTime.Now.ToString("HH:mm:ss");
                gameOverText.text = "";
            }
        }

        /// <summary>
        /// 从候选列表中选出第一个在当前系统上可用的中文字体并动态生成。
        /// </summary>
        static Font CreateSystemFont()
        {
            string[] available = Font.GetOSInstalledFontNames();
            foreach (string candidate in FontCandidates)
            {
                if (Array.Exists(available,
                        name => string.Equals(name, candidate, StringComparison.OrdinalIgnoreCase)))
                {
                    return Font.CreateDynamicFontFromOSFont(candidate, 48);
                }
            }

            // 所有候选均不可用时回退到 Unity 内置字体（无中文，但至少可见）
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
    }
}
#endif
