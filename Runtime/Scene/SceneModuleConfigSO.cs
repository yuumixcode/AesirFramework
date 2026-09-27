using System;
using Runestone.AesirArchitecture;
using UnityEngine;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 场景模块全局配置（单例资产）。承载无需预放置 [SceneModule] 即可调整的模块级配置：
    /// 在 Project 窗口直接编辑本资产即可生效，不再要求预放置 SceneModule 物体。
    /// </summary>
    /// <remarks>
    /// 单例 <see cref="Instance" /> 的解析顺序：
    /// <list type="number">
    /// <item><see cref="RegisterConfigLoader" /> 注册的加载器——注册后 Resources 兜底不再执行，
    /// 用于彻底放弃 Resources 的项目（如 Addressables 或代码内构造配置）；</item>
    /// <item>Resources 兜底（<see cref="ResourcePath" />，编辑器在编辑模式域加载时自动创建该资产，
    /// 见编辑器程序集 <c>SceneModuleConfigAssetInitializer</c>）；</item>
    /// <item>内存默认实例——前两级均未命中时以代码默认值创建，仅存在于内存、不持久化。</item>
    /// </list>
    /// 加载进度上限（<see cref="progressCap" />）在每次加载时读取，运行时修改本资产对后续加载立即生效。
    /// </remarks>
    public class SceneModuleConfigSO : AesirScriptableObject
    {
        /// <summary>Resources 兜底路径（相对任意 Resources 根，不含扩展名）。资产固定位于 Assets/Resources/SceneModuleConfig/SceneModuleConfig.asset。</summary>
        public const string ResourcePath = "SceneModuleConfig/SceneModuleConfig";

        /// <summary>
        /// 自定义启动场景（全局兜底）。预放置 <see cref="SceneModule" /> 的序列化字段未赋值（null）时，
        /// <see cref="SceneModule.BootstrapSceneAssetWrapper" /> 回退读取本值；
        /// 两者均未配置时返回 null，启动流程由用户代码自行编排。
        /// </summary>
        [Tooltip("自定义启动场景（全局兜底）——预放置 SceneModule 未赋值启动场景时，BootstrapSceneAssetWrapper 回退读取本值")]
        [SerializeField]
        internal SceneAssetWrapper bootstrapScene;

        /// <summary>
        /// 场景加载进度的归一化上限。Unity 的 AsyncOperation.progress 在场景激活前停在 0.9、激活瞬间跳 1，
        /// onProgress 回调按本值归一化使进度条可平滑走到 100%；消费端钳制到 (0, 1] 区间。
        /// </summary>
        [Tooltip("场景加载进度的归一化上限（0-1，默认 0.9）——Unity 场景激活前进度停在 0.9，onProgress 按本值归一化")]
        [SerializeField]
        internal float progressCap = 0.9f;

        static SceneModuleConfigSO _instance;
        static Func<SceneModuleConfigSO> _configLoader;

        /// <summary>
        /// 全局单例。按类 remarks 声明的顺序解析并缓存；缓存失效（资产被销毁的 Unity 假 null）时自动重新解析。
        /// </summary>
        public static SceneModuleConfigSO Instance
        {
            get
            {
                if (_instance != null)
                {
                    return _instance;
                }

                if (_configLoader != null)
                {
                    _instance = _configLoader();
                }
                else
                {
                    _instance = Resources.Load<SceneModuleConfigSO>(ResourcePath);
                }

                if (_instance == null)
                {
                    _instance = CreateInstance<SceneModuleConfigSO>();
                }

                return _instance;
            }
        }

        /// <summary>
        /// 注册配置加载器，替代 Resources 兜底（注册后 <see cref="Instance" /> 只经加载器解析）。
        /// 须在首次消费配置（场景加载或读取 <see cref="SceneModule.BootstrapSceneAssetWrapper" />）之前调用，
        /// 例如 <c>[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]</c> 或首个场景的引导脚本中。
        /// </summary>
        /// <param name="loader">同步加载器；返回 null 时按内存默认配置兜底。</param>
        public static void RegisterConfigLoader(Func<SceneModuleConfigSO> loader)
        {
            if (loader == null)
            {
                throw new ArgumentNullException(nameof(loader), "配置加载器不可为空");
            }

            if (_configLoader != null)
            {
                throw new InvalidOperationException("配置加载器已注册，重复注册视为初始化错误；如需替换请先 UnregisterConfigLoader");
            }

            _configLoader = loader;
        }

        /// <summary>
        /// 注销配置加载器（幂等）。注销后 <see cref="Instance" /> 重新按 Resources 兜底解析，
        /// 已按加载器路径缓存的实例一并失效。
        /// </summary>
        public static void UnregisterConfigLoader()
        {
            _configLoader = null;
            _instance = null;
        }

        /// <summary>
        /// 创建一份运行时默认配置实例（不持久化为资产）。
        /// </summary>
        /// <returns>默认配置的 <see cref="SceneModuleConfigSO" /> 实例。</returns>
        public static SceneModuleConfigSO CreateDefault() => CreateInstance<SceneModuleConfigSO>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            _instance = null;
            _configLoader = null;
        }
    }
}
