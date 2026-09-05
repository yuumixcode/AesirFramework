using System;

namespace Runestone.AesirModules
{
    /// <summary>
    /// Addressables 编辑器能力的静态桥。
    /// <para>
    /// 核心程序集不引用任何 Addressables 程序集；由可选程序集
    /// Runestone.AesirModules.Editor.Addressables（仅当项目安装了 Addressables 包时才参与编译）
    /// 在编辑器加载时把能力注册进来。未注册时所有 Addressables 编辑器功能自动隐藏，
    /// 不产生任何编译错误——这是"项目中没有 Addressables 时相关代码整体不编译"约束的运行时侧落点。
    /// </para>
    /// </summary>
    public static class SceneAssetWrapperAddressablesBridge
    {
        /// <summary>
        /// 地址查询委托：入参为场景资产路径，返回其在 Addressables 中的地址；不可寻址或失败时返回 null。
        /// </summary>
        public static Func<string, string> GetAddressHandler { get; private set; }

        /// <summary>
        /// 加入默认组委托：入参为场景资产路径，成功时返回新地址（Addressables 默认寻址下通常等于资产路径），失败返回 null。
        /// </summary>
        public static Func<string, string> MakeAddressableHandler { get; private set; }

        /// <summary>桥是否已注册（即项目安装了 Addressables 包且胶水程序集已参与编译）。</summary>
        public static bool IsAvailable => GetAddressHandler != null && MakeAddressableHandler != null;

        /// <summary>
        /// 注册桥接能力。由 Runestone.AesirModules.Editor.Addressables 程序集的 [InitializeOnLoad] 调用。
        /// </summary>
        public static void Register(Func<string, string> getAddress, Func<string, string> makeAddressable)
        {
            GetAddressHandler = getAddress;
            MakeAddressableHandler = makeAddressable;
        }

        /// <summary>
        /// 注销桥接能力。域重载后静态委托会被清空，胶水程序集会随 [InitializeOnLoad] 重新注册；
        /// 单测用它来隔离用例间的桥状态。
        /// </summary>
        public static void Unregister()
        {
            GetAddressHandler = null;
            MakeAddressableHandler = null;
        }
    }
}
