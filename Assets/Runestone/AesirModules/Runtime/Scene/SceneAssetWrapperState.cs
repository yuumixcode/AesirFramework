namespace Runestone.AesirModules
{
    /// <summary>
    /// <see cref="SceneAssetWrapper" /> 的可用状态。
    /// <list type="number">
    /// <item><see cref="Unsafe" />：引用不安全（空引用，或场景既不在 BuildSettings 也不可 Addressable）</item>
    /// <item><see cref="Regular" />：引用安全，指向 BuildSettings 中的常规场景</item>
    /// <item><see cref="Addressable" />：引用安全，指向 Addressable 场景</item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// 对位 Eflatun.SceneReference 的 SceneReferenceState：空引用没有独立状态值，
    /// 表现为 <see cref="Unsafe" />，具体原因用 <see cref="SceneAssetWrapperUnsafeReason" /> 区分。
    /// 场景同时存在于 BuildSettings 与 Addressables 时以 <see cref="Regular" /> 优先
    /// （BuildSettings 加载途径不依赖 Addressables 包，兼容性最好）。
    /// </remarks>
    public enum SceneAssetWrapperState
    {
        /// <summary>引用不安全：空引用，或场景未加入 BuildSettings（或被禁用）且不可 Addressable。</summary>
        Unsafe = 0,

        /// <summary>引用安全：场景已加入 BuildSettings 并启用。</summary>
        Regular = 1,

        /// <summary>引用安全：场景在 Addressables 组中（地址数据经编辑器同步缓存，运行时纯数据判定）。</summary>
        Addressable = 2,
    }
}
