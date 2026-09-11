using Runestone.AesirArchitecture;
using UnityEngine;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 音频模块配置资产。创建路径：Assets → Create → Aesir Modules → Audio → AudioConfig。
    /// <para>配置默认音量、音量持久化开关与 PlayerPrefs 键前缀；留空键前缀时回退 AesirAudio。</para>
    /// </summary>
    [CreateAssetMenu(menuName = "Aesir Modules/Audio/AudioConfig", fileName = "AudioConfig")]
    public class AudioConfigSO : AesirScriptableObject
    {
        [Tooltip("默认总音量（0-1）— 初始化时使用；玩家已持久化的音量优先于本默认值")]
        [SerializeField]
        internal float masterVolume = 1f;

        [Tooltip("默认背景音乐通道音量（0-1）— 与总音量相乘生效")]
        [SerializeField]
        internal float bgmVolume = 1f;

        [Tooltip("默认音效通道音量（0-1）— 与总音量相乘生效")]
        [SerializeField]
        internal float sfxVolume = 1f;

        [Tooltip("持久化音量与静音 — 玩家调整的值经 PlayerPrefs 保存，重启后自动恢复")]
        [SerializeField]
        internal bool persistVolumes = true;

        [Tooltip("PlayerPrefs 键前缀 — 实际键为 前缀.MasterVolume 等；留空时回退 AesirAudio")]
        [SerializeField]
        internal string prefsKey = "AesirAudio";
    }
}
