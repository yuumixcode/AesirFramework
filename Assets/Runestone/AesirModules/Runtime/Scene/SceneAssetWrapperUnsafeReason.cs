namespace Runestone.AesirModules
{
    /// <summary>
    /// 描述 <see cref="SceneAssetWrapper" /> 不安全的具体原因。
    /// </summary>
    /// <remarks>
    /// <see cref="Empty" /> 优先级最高。对位 Eflatun.SceneReference 的 SceneReferenceUnsafeReason：
    /// NotInMaps 一项在本实现中不存在——运行时直接使用序列化的路径/地址数据，没有映射表的概念。
    /// </remarks>
    public enum SceneAssetWrapperUnsafeReason
    {
        /// <summary>引用安全可用。</summary>
        None,

        /// <summary>空引用，未分配任何场景。</summary>
        Empty,

        /// <summary>场景未加入 BuildSettings（或被禁用），且没有 Addressable 备选加载途径。</summary>
        NotInBuild,
    }
}
