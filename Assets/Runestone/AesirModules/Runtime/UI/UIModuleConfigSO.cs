using System;
using Runestone.AesirArchitecture;
using UnityEngine;

namespace Runestone.AesirModules
{
    /// <summary>
    /// UI 模块全局配置（单例资产）。承载无需预放置 [UIModule] 即可调整的模块级配置：
    /// 在 Project 窗口直接编辑本资产即可生效，不再要求预放置 UIModule 物体。
    /// </summary>
    /// <remarks>
    /// 单例 <see cref="Instance" /> 的解析顺序：
    /// <list type="number">
    /// <item><see cref="RegisterConfigLoader" /> 注册的加载器——注册后 Resources 兜底不再执行，
    /// 用于彻底放弃 Resources 的项目（如 Addressables 或代码内构造配置）；</item>
    /// <item>Resources 兜底（<see cref="ResourcePath" />，编辑器在编辑模式域加载时自动创建该资产，
    /// 见编辑器程序集 <c>UIModuleConfigAssetInitializer</c>）；</item>
    /// <item>内存默认实例——前两级均未命中时以代码默认值创建，仅存在于内存、不持久化。</item>
    /// </list>
    /// 运行时经 <see cref="UIModule.MaskMode" /> 的切换只覆盖 UIModule 的内存值，不改写本资产。
    /// </remarks>
    public class UIModuleConfigSO : AesirScriptableObject
    {
        /// <summary>Resources 兜底路径（相对任意 Resources 根，不含扩展名）。资产固定位于 Assets/Resources/UIModuleConfig/UIModuleConfig.asset。</summary>
        public const string ResourcePath = "UIModuleConfig/UIModuleConfig";

        /// <summary>
        /// 窗口蒙版调度模式：单遮 = 仅最高层可见窗口的蒙版生效；叠遮 = 各窗口蒙版独立生效。
        /// 运行时可经 <see cref="UIModule.MaskMode" /> 临时切换。
        /// </summary>
        [Tooltip("窗口蒙版调度模式：单遮 = 仅最高层可见窗口的蒙版生效；叠遮 = 各窗口蒙版独立生效。运行时可经 UIModule.MaskMode 临时切换")]
        [SerializeField]
        internal UIMaskMode maskMode = UIMaskMode.Single;

        static UIModuleConfigSO _instance;
        static Func<UIModuleConfigSO> _configLoader;

        /// <summary>
        /// 全局单例。按类 remarks 声明的顺序解析并缓存；缓存失效（资产被销毁的 Unity 假 null）时自动重新解析。
        /// </summary>
        public static UIModuleConfigSO Instance
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
                    _instance = Resources.Load<UIModuleConfigSO>(ResourcePath);
                }

                if (_instance == null)
                {
                    _instance = CreateInstance<UIModuleConfigSO>();
                }

                return _instance;
            }
        }

        /// <summary>
        /// 注册配置加载器，替代 Resources 兜底（注册后 <see cref="Instance" /> 只经加载器解析）。
        /// 须在 <see cref="UIModule" /> 首次<b>消费配置</b>（读取 <c>MaskMode</c>，其惰性解析首次访问）之前调用，
        /// 例如 <c>[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]</c> 或首个场景的引导脚本中。
        /// </summary>
        /// <param name="loader">同步加载器（加载契约与 <see cref="IUIAssetLoader" /> 同为同步语义）；返回 null 时按内存默认配置兜底。</param>
        public static void RegisterConfigLoader(Func<UIModuleConfigSO> loader)
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
        /// <returns>默认配置的 <see cref="UIModuleConfigSO" /> 实例。</returns>
        public static UIModuleConfigSO CreateDefault() => CreateInstance<UIModuleConfigSO>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            _instance = null;
            _configLoader = null;
        }
    }
}
