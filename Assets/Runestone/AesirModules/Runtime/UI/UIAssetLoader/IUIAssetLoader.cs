using UnityEngine;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 面板加载器契约。加载语义为同步：适用于 Resources、同步缓存等管线；
    /// Addressables 等异步管线需自行预加载后同步返回，无法在接口内表达等待。
    /// </summary>
    /// <remarks>
    /// 预制体引用由 <see cref="UIModule" /> 的注册表持有，生命周期与模块一致，契约不设释放方法。
    /// </remarks>
    public interface IUIAssetLoader
    {
        /// <summary>
        /// 按路径加载面板预制体。
        /// </summary>
        /// <param name="path">资源路径。</param>
        /// <returns>加载到的预制体，未找到返回 null。</returns>
        GameObject Load(string path);
    }
}
