namespace Runestone.AesirModules
{
    /// <summary>
    /// 当前项目未安装 Addressables 包时访问了 Addressables 相关 API。
    /// </summary>
    /// <remarks>
    /// 最小惊讶原则：Addressables 相关 API 在未安装包时依旧可见、可编译（卸载包不会导致任何编译错误），
    /// 但运行期访问会抛出本异常。安装 com.unity.addressables 包后无需任何代码改动即可直接生效。
    /// </remarks>
    public class AddressablesSupportDisabledException : SceneAssetWrapperException
    {
        /// <summary>使用固定的修复指引初始化。</summary>
        public AddressablesSupportDisabledException()
            : base("当前项目未安装 Addressables 包（com.unity.addressables），无法使用 Address 相关功能。" +
                   "\n安装后无需任何代码改动即可直接生效（相关 API 始终可见，遵循最小惊讶原则）。")
        {
        }
    }
}
