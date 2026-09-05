using UnityEngine;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 面板加载器。默认实现 <see cref="ResourcesUILoader" />，可替换为 Addressables 等。
    /// </summary>
    public interface IUIAssetLoader
    {
        /// <summary>
        /// 按路径加载面板预制体。
        /// </summary>
        /// <param name="path">资源路径。</param>
        /// <returns>加载到的预制体，未找到返回 null。</returns>
        GameObject Load(string path);

        /// <summary>
        /// 释放预制体资源。
        /// </summary>
        /// <param name="prefab">需要释放的预制体引用。</param>
        void Unload(GameObject prefab);
    }
}
