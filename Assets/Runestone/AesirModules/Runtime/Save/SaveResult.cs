namespace Runestone.AesirModules
{
    /// <summary>
    /// 保存结果：成败与错误描述。可预期环境失败（磁盘满 / 权限等 IO 错误）以本结果表达而非异常，
    /// 错误信息完整保留并经日志门面输出（不是吞噬）；编程错误（非法键 / null 数据）仍抛异常。
    /// </summary>
    public readonly struct SaveResult
    {
        /// <summary>是否保存成功。</summary>
        public bool IsSuccess { get; }

        /// <summary>失败描述；成功时为 null。</summary>
        public string Error { get; }

        SaveResult(bool isSuccess, string error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        /// <summary>成功结果。</summary>
        public static SaveResult Success() => new SaveResult(true, null);

        /// <summary>失败结果。</summary>
        /// <param name="error">失败描述（非空）。</param>
        public static SaveResult Failure(string error) => new SaveResult(false, error);
    }

    /// <summary>加载结果状态。</summary>
    public enum LoadStatus
    {
        /// <summary>加载成功（值有效，来源见 <see cref="LoadResult{T}.Source" />）。</summary>
        Success,

        /// <summary>存档不存在（主存档与备份均无）。</summary>
        NoSave,

        /// <summary>存档损坏且无法从备份恢复（校验失败 / 解析失败 / 变换缺失，主备均不可用）。</summary>
        Corrupted,

        /// <summary>存档版本与当前版本不一致且未提供迁移器（版本信息见结果字段）。</summary>
        VersionMismatch,

        /// <summary>存档记录的序列化器未注册（缺失标识见 <see cref="LoadResult{T}.Error" />）。</summary>
        SerializerMissing
    }

    /// <summary>加载成功时的数据来源。</summary>
    public enum LoadSource
    {
        /// <summary>主存档。</summary>
        Primary,

        /// <summary>备份存档（主存档缺失或损坏时的回退来源）。</summary>
        Backup
    }

    /// <summary>
    /// 加载结果：状态、值、来源与错误描述。损坏 / 缺失 / 版本不匹配是存档系统的正常业务分支，
    /// 以本结果表达而非异常，游戏侧据 <see cref="Status" /> 决定新档 / 重试 / 提示。
    /// </summary>
    /// <typeparam name="T">数据类型。</typeparam>
    public readonly struct LoadResult<T>
    {
        /// <summary>结果状态。</summary>
        public LoadStatus Status { get; }

        /// <summary>加载到的数据；仅 <see cref="LoadStatus.Success" /> 时有效，其余状态为类型默认值。</summary>
        public T Value { get; }

        /// <summary>数据来源；仅成功时有意义。</summary>
        public LoadSource Source { get; }

        /// <summary>失败描述；成功时为 null。</summary>
        public string Error { get; }

        /// <summary>存档记录的数据版本号；仅 <see cref="LoadStatus.VersionMismatch" /> 时有意义。</summary>
        public int SavedVersion { get; }

        /// <summary>加载声明的当前数据版本号；仅 <see cref="LoadStatus.VersionMismatch" /> 时有意义。</summary>
        public int CurrentVersion { get; }

        /// <summary>是否加载成功。</summary>
        public bool IsSuccess => Status == LoadStatus.Success;

        LoadResult(LoadStatus status, T value, LoadSource source, string error,
            int savedVersion = 0, int currentVersion = 0)
        {
            Status = status;
            Value = value;
            Source = source;
            Error = error;
            SavedVersion = savedVersion;
            CurrentVersion = currentVersion;
        }

        /// <summary>成功结果。</summary>
        /// <param name="value">加载到的数据。</param>
        /// <param name="source">数据来源（主存档 / 备份）。</param>
        public static LoadResult<T> Success(T value, LoadSource source) =>
            new LoadResult<T>(LoadStatus.Success, value, source, null);

        /// <summary>存档不存在结果。</summary>
        public static LoadResult<T> NoSave() =>
            new LoadResult<T>(LoadStatus.NoSave, default, LoadSource.Primary, null);

        /// <summary>损坏且无法恢复结果。</summary>
        /// <param name="error">损坏描述。</param>
        public static LoadResult<T> Corrupted(string error) =>
            new LoadResult<T>(LoadStatus.Corrupted, default, LoadSource.Primary, error);

        /// <summary>版本不匹配结果（未提供迁移器）。</summary>
        /// <param name="savedVersion">存档记录的版本。</param>
        /// <param name="currentVersion">加载声明的当前版本。</param>
        public static LoadResult<T> VersionMismatch(int savedVersion, int currentVersion) =>
            new LoadResult<T>(LoadStatus.VersionMismatch, default, LoadSource.Primary,
                $"存档版本 {savedVersion} 与当前版本 {currentVersion} 不一致，且未提供迁移器",
                savedVersion, currentVersion);

        /// <summary>序列化器未注册结果。</summary>
        /// <param name="serializerId">存档记录的序列化器标识。</param>
        public static LoadResult<T> SerializerMissing(string serializerId) =>
            new LoadResult<T>(LoadStatus.SerializerMissing, default, LoadSource.Primary,
                $"存档记录的序列化器 '{serializerId}' 未注册，无法还原");
    }
}