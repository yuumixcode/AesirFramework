using Runestone.AesirArchitecture;
using UnityEngine;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 保存模块配置资产。创建路径：Assets → Create → Aesir Modules → Save → SaveConfig，
    /// 或菜单 <c>Tools/Aesir/Modules/Create Module Config Assets</c>（幂等）。
    /// <para>配置存储目录、默认序列化器、备份开关与切后台 / 退出自动保存开关。</para>
    /// </summary>
    /// <remarks>
    /// 单例 <see cref="Instance" /> 解析顺序：Resources 兜底（<see cref="ResourcePath" />）→ 内存默认实例
    /// （仅存在于内存、不持久化）。包导入时<b>不</b>自动创建资产——落盘是开发者的显式选择。
    /// 压缩 / 加密不在本资产配置：变换链与密钥是代码侧组合（密钥入资产即泄密），
    /// 见 <see cref="SaveModule.SetTransformChain" />。
    /// </remarks>
    [CreateAssetMenu(menuName = "Aesir Modules/Save/SaveConfig", fileName = "SaveConfig")]
    public class SaveConfigSO : AesirScriptableObject
    {
        /// <summary>Resources 兜底路径（相对任意 Resources 根，不含扩展名）。</summary>
        public const string ResourcePath = "SaveModuleConfig/SaveModuleConfig";

        [Tooltip("存储目录名 — 位于 Application.persistentDataPath 下；首次写入时懒创建")]
        [SerializeField]
        internal string saveDirectory = "Saves";

        [Tooltip("默认序列化器标识 — 保存未指定时使用；须已注册（unity-json 默认注册，odin/newtonsoft 需显式注册）")]
        [SerializeField]
        internal string defaultSerializerId = "unity-json";

        [Tooltip("保留备份 — 主存档损坏时自动回退上一份完整存档；关闭后提交仍原子，但失去损坏回退能力")]
        [SerializeField]
        internal bool backupEnabled = true;

        [Tooltip("切后台自动保存 — OnApplicationPause(true) 时触发已注册的自动保存回调")]
        [SerializeField]
        internal bool saveOnApplicationPause = true;

        [Tooltip("退出自动保存 — OnApplicationQuit 时触发已注册的自动保存回调")]
        [SerializeField]
        internal bool saveOnApplicationQuit = true;

        static SaveConfigSO _instance;

        /// <summary>
        /// 全局单例：Resources 兜底命中则用资产，否则以代码默认值创建内存实例（不落盘）。
        /// 缓存失效（资产被销毁的 Unity 假 null）时自动重新解析。
        /// </summary>
        public static SaveConfigSO Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                _instance = Resources.Load<SaveConfigSO>(ResourcePath);
                if (_instance == null)
                {
                    _instance = CreateInstance<SaveConfigSO>();
                }

                return _instance;
            }
        }

        /// <summary>创建一份运行时默认配置实例（不持久化为资产）。</summary>
        public static SaveConfigSO CreateDefault() => CreateInstance<SaveConfigSO>();

        /// <summary>兼容 Enter Play Mode（跳过域重载）的静态状态重置。</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            _instance = null;
        }
    }
}