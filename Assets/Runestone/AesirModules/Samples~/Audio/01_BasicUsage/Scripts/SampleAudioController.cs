#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
using System;
using Runestone.AesirModules;
using UnityEngine;

namespace Runestone.AesirModules.Samples.Audio.BasicUsage
{
    /// <summary>
    /// Audio 模块基础用法示例 —— OnGUI 面板驱动全部公开 API：
    /// SFX 播放（含音调抖动）、BGM 播放与淡入淡出切换、停止、暂停恢复、三通道音量与静音。
    /// </summary>
    /// <remarks>
    /// 本示例演示运行时自动创建路径：场景中无需预放置 AudioModule，首次调用静态 API 时
    /// 自动挂载到 [Aesir Modules] 宿主下。面板文字使用系统动态中文字体（Windows / macOS / Linux 兼容）。
    /// </remarks>
    [AddComponentMenu("")]
    public class SampleAudioController : MonoBehaviour
    {
        /// <summary>背景音乐 A（程序化生成的琶音循环）。</summary>
        [SerializeField]
        AudioClip bgmA;

        /// <summary>背景音乐 B（与 A 不同的琶音循环，用于演示淡入淡出切歌）。</summary>
        [SerializeField]
        AudioClip bgmB;

        /// <summary>UI 点击音效。</summary>
        [SerializeField]
        AudioClip sfxClick;

        /// <summary>演示音调抖动的音效（连续播放可听到每次音调略有差异）。</summary>
        [SerializeField]
        AudioClip sfxJitter;

        static readonly string[] FontCandidates =
        {
            "Microsoft YaHei", // Windows 简体中文
            "Microsoft YaHei UI",
            "PingFang SC", // macOS 简体中文
            "Noto Sans CJK SC", // Linux / 跨平台
            "WenQuanYi Zen Hei" // 部分 Linux 发行版
        };

        Rect _panelRect = new Rect(16, 16, 380, 560);
        Font _dynamicFont;

        void Awake()
        {
            _dynamicFont = CreateSystemFont();
        }

        void OnGUI()
        {
            GUI.skin.font = _dynamicFont;
            _panelRect = GUILayout.Window(0, _panelRect, DrawPanel, "Aesir Audio 模块示例");
        }

        static void DrawTitle(string title)
        {
            GUILayout.Space(6);
            GUILayout.Label(title, GUI.skin.box, GUILayout.ExpandWidth(true));
        }

        void DrawPanel(int windowId)
        {
            DrawTitle("SFX");

            if (GUILayout.Button("播放点击音效"))
            {
                AudioModule.PlaySfx(sfxClick);
            }

            if (GUILayout.Button("播放音效（音调抖动 ±0.15，连点可听差异）"))
            {
                AudioModule.PlaySfx(sfxJitter, pitch: 1f, pitchJitter: 0.15f);
            }

            DrawTitle("BGM");

            if (GUILayout.Button("播放 BGM A（立即）"))
            {
                AudioModule.PlayBgm(bgmA);
            }

            if (GUILayout.Button("切到 BGM B（1.5 秒淡入淡出）"))
            {
                AudioModule.PlayBgm(bgmB, fadeSeconds: 1.5f);
            }

            if (GUILayout.Button("停止 BGM（0.8 秒淡出）"))
            {
                AudioModule.StopBgm(fadeSeconds: 0.8f);
            }

            GUILayout.Label($"当前 BGM：{(AudioModule.CurrentBgm ? AudioModule.CurrentBgm.name : "无")}（正在播放：{AudioModule.IsBgmPlaying}）");

            DrawTitle("暂停");

            if (GUILayout.Button("暂停全部"))
            {
                AudioModule.PauseAll();
            }

            if (GUILayout.Button("恢复全部"))
            {
                AudioModule.ResumeAll();
            }

            DrawTitle("音量");

            GUILayout.Label($"总音量：{AudioModule.MasterVolume:F2}");
            AudioModule.MasterVolume = GUILayout.HorizontalSlider(AudioModule.MasterVolume, 0f, 1f);

            GUILayout.Label($"BGM 音量：{AudioModule.BgmVolume:F2}");
            AudioModule.BgmVolume = GUILayout.HorizontalSlider(AudioModule.BgmVolume, 0f, 1f);

            GUILayout.Label($"音效音量：{AudioModule.SfxVolume:F2}");
            AudioModule.SfxVolume = GUILayout.HorizontalSlider(AudioModule.SfxVolume, 0f, 1f);

            DrawTitle("静音");

            AudioModule.MasterMute = GUILayout.Toggle(AudioModule.MasterMute, "总静音");
            AudioModule.BgmMute = GUILayout.Toggle(AudioModule.BgmMute, "BGM 静音");
            AudioModule.SfxMute = GUILayout.Toggle(AudioModule.SfxMute, "音效静音");

            GUILayout.Label("音量与静音自动经 PlayerPrefs 持久化，重新 Play 后仍保留。");

            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        /// <summary>
        /// 从候选列表中选出第一个在当前系统上可用的中文字体并动态生成。
        /// </summary>
        static Font CreateSystemFont()
        {
            var available = Font.GetOSInstalledFontNames();
            foreach (var candidate in FontCandidates)
            {
                if (Array.Exists(available,
                        name => string.Equals(name, candidate, StringComparison.OrdinalIgnoreCase)))
                {
                    return Font.CreateDynamicFontFromOSFont(candidate, 32);
                }
            }

            // 所有候选均不可用时回退到 Unity 内置字体（无中文，但至少可见）
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
    }
}
#endif
