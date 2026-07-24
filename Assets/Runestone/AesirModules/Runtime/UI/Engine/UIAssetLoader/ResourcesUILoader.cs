using Runestone.AesirArchitecture;
using UnityEngine;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 默认加载器：从 Resources 路径加载面板预制体。
    /// </summary>
    public sealed class ResourcesUILoader : IUIAssetLoader
    {
        /// <summary>
        /// 从 Resources 路径加载面板预制体。
        /// </summary>
        /// <param name="path">Resources 下的相对路径。</param>
        /// <returns>加载到的预制体，未找到时记录错误并返回 null。</returns>
        public GameObject Load(string path)
        {
            var prefab = Resources.Load<GameObject>(path);
            if (prefab == null)
            {
                AesirModulesDebug.LogError(AesirModulesDebug.UIModuleTag, $"Resources 中未找到面板预制体: {path}");
            }

            return prefab;
        }

        /// <summary>
        /// 释放通过 Resources 加载的预制体资源。
        /// </summary>
        /// <param name="prefab">需要释放的预制体引用。</param>
        public void Unload(GameObject prefab)
        {
            if (prefab != null)
            {
                Resources.UnloadAsset(prefab);
            }
        }
    }
}
