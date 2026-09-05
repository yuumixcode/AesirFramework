namespace Runestone.AesirModules
{
    /// <summary>
    /// 访问了未分配任何场景的 <see cref="SceneAssetWrapper" />。
    /// </summary>
    /// <remarks>
    /// 修复：在 Inspector 中给字段拖入 SceneAsset，或使用 <see cref="SceneAssetWrapper.FromScenePath" /> 构造。
    /// 规避：先检查 <see cref="SceneAssetWrapper.State" /> 是否安全，或改用对应的 TryGet 方法。
    /// </remarks>
    public class EmptySceneAssetWrapperException : SceneAssetWrapperException
    {
        /// <summary>使用固定的修复指引初始化。</summary>
        public EmptySceneAssetWrapperException()
            : base("SceneAssetWrapper 是空引用，未分配任何场景。" +
                   "\n修复：在 Inspector 中拖入 SceneAsset，或使用 SceneAssetWrapper.FromScenePath() 构造。" +
                   "\n规避：先检查 State 属性是否为 Unsafe，或改用对应的 TryGet 方法。")
        {
        }
    }
}
