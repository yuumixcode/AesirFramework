namespace Runestone.AesirModules
{
    /// <summary>
    /// 保存管理器契约：保存 / 加载的完整编排。策略扩展点——<b>用户自定义保存管理方案的总入口</b>：
    /// 实现本接口并经 <see cref="SaveModule.SetManager" /> 替换，即可整体接入自定义方案
    /// （云存档同步、自定义事务策略、多槽合并等），默认 <see cref="TransactionalSaveManager" /> 不再参与。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 接口只含保存与加载两方法（接口隔离）：删除 / 存在性 / 枚举是存储直通操作，
    /// 由门面直接转发 <see cref="ISaveStorage" />，不经管理器。
    /// </para>
    /// <para>
    /// <b>结果语义契约：</b>可预期的环境失败（IO 错误、存档损坏、版本不匹配）以结果对象表达，
    /// 不抛异常；编程错误（非法键、null 数据）抛异常（fail-fast）。自定义实现应遵守同一边界，
    /// 以便游戏侧用统一的结果处理逻辑。
    /// </para>
    /// </remarks>
    public interface ISaveManager
    {
        /// <summary>编排一次保存：序列化 → 变换 → 封装 → 提交存储。</summary>
        /// <typeparam name="T">数据类型。</typeparam>
        /// <param name="key">存档键。</param>
        /// <param name="data">待保存数据（非 null）。</param>
        /// <param name="dataVersion">调用方声明的数据版本号（写入存档，加载时用于迁移判定）。</param>
        /// <returns>保存结果（成败与错误描述）。</returns>
        SaveResult Save<T>(string key, T data, int dataVersion);

        /// <summary>编排一次加载：读取 → 校验 → 还原 → 版本迁移；主存档损坏时按实现策略回退备份。</summary>
        /// <typeparam name="T">数据类型。</typeparam>
        /// <param name="key">存档键。</param>
        /// <param name="currentVersion">调用方声明的当前数据版本号。</param>
        /// <param name="migrator">版本不匹配时的迁移器；为 null 时版本不匹配返回 VersionMismatch 状态。</param>
        /// <returns>加载结果（状态、值、来源与错误描述）。</returns>
        LoadResult<T> Load<T>(string key, int currentVersion, ISaveMigrator<T> migrator);
    }
}