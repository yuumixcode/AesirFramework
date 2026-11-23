namespace Runestone.AesirModules
{
    /// <summary>
    /// 保存数据迁移器契约：把旧版本数据升级到当前版本。作为 <see cref="SaveModule.Load{T}" /> 的可选参数传入
    /// （迁移器与数据类型强绑定，故不做全局注册表）。
    /// </summary>
    /// <typeparam name="T">数据类型。</typeparam>
    /// <remarks>
    /// 仅当存档记录的版本与加载声明的当前版本不一致时被调用；实现内可按 <paramref name="fromVersion" />
    /// 分支逐版本升级。迁移结果不会在加载路径自动写回磁盘（读路径无副作用），下次保存时自然落盘。
    /// 迁移器内抛出的异常向上传播（fail-fast，视为游戏代码 bug）。
    /// </remarks>
    public interface ISaveMigrator<T>
    {
        /// <summary>将旧版本数据升级为当前版本。</summary>
        /// <param name="data">按存档版本反序列化出的旧数据。</param>
        /// <param name="fromVersion">存档记录的数据版本号。</param>
        /// <returns>升级到当前版本的数据。</returns>
        T Migrate(T data, int fromVersion);
    }
}