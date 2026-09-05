namespace Runestone.AesirModules
{
    /// <summary>
    /// 对非 Addressable 场景的 <see cref="SceneAssetWrapper" /> 访问了 Address。
    /// </summary>
    /// <remarks>
    /// 修复：把场景加入 Addressables 组（可在 Inspector 字段的工具按钮中一键加入），或改用 BuildSettings 加载途径。
    /// </remarks>
    public class SceneNotAddressableException : SceneAssetWrapperException
    {
        /// <summary>使用固定的修复指引初始化。</summary>
        public SceneNotAddressableException()
            : base("此 SceneAssetWrapper 引用的场景不是 Addressable 场景，无法获取 Address。" +
                   "\n修复：把场景加入 Addressables 组（Inspector 字段旁的\"加入 Addressables\"按钮可一键完成）。")
        {
        }
    }
}
