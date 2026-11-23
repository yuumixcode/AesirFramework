using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 事务式保存管理器（默认实现）：编排「序列化 → 变换链 → 信封封装 → 提交存储」的保存流程，
    /// 与「读取 → 校验 → 按信封标识还原 → 版本迁移 → 损坏回退备份」的加载流程。
    /// 经 <see cref="SaveModule.SetManager" /> 可整体替换为自定义保存管理方案。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 依赖经构造注入（依赖倒置）：序列化器注册表、存储、变换链均为抽象；注册表与变换链以引用共享，
    /// 门面后续的注册 / 链变更对本管理器立即可见。
    /// </para>
    /// <para>
    /// 失败边界：IO 异常转为失败结果并 LogError（错误信息完整保留，不是吞噬）；
    /// 加载路径的任何解析失败（信封校验 / 变换还原 / 反序列化）判为该份存档损坏；
    /// 编程错误（非法键 / null 数据 / 默认序列化器未注册）抛异常（fail-fast）；
    /// 迁移器内异常向上传播（游戏代码 bug 不兜底）。
    /// </para>
    /// <para>
    /// 回退策略：主存档缺失或损坏时尝试备份；备份成功则结果标明 <see cref="LoadSource.Backup" />。
    /// 版本不匹配与序列化器缺失是数据语义问题（备份同样存在），不回退。加载路径无磁盘写副作用。
    /// </para>
    /// </remarks>
    public class TransactionalSaveManager : ISaveManager
    {
        readonly IReadOnlyDictionary<string, ISaveSerializer> _serializers;
        readonly ISaveStorage _storage;
        readonly IReadOnlyList<ISaveTransform> _transforms;
        readonly string _defaultSerializerId;

        /// <summary>构造事务式保存管理器。</summary>
        /// <param name="serializers">序列化器注册表（键为 <see cref="ISaveSerializer.Id" />，引用共享）。</param>
        /// <param name="storage">存储实现。</param>
        /// <param name="transforms">变换链（按施加顺序，引用共享）。</param>
        /// <param name="defaultSerializerId">保存时使用的默认序列化器标识。</param>
        public TransactionalSaveManager(IReadOnlyDictionary<string, ISaveSerializer> serializers,
            ISaveStorage storage, IReadOnlyList<ISaveTransform> transforms, string defaultSerializerId)
        {
            _serializers = serializers ?? throw new ArgumentNullException(nameof(serializers));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _transforms = transforms ?? throw new ArgumentNullException(nameof(transforms));
            if (string.IsNullOrEmpty(defaultSerializerId))
            {
                throw new ArgumentException("默认序列化器标识不可为空", nameof(defaultSerializerId));
            }

            _defaultSerializerId = defaultSerializerId;
        }

        /// <inheritdoc />
        public SaveResult Save<T>(string key, T data, int dataVersion)
        {
            SaveKey.Validate(key);
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), "待保存数据不可为空");
            }

            if (!_serializers.TryGetValue(_defaultSerializerId, out var serializer))
            {
                throw new InvalidOperationException(
                    $"默认序列化器 '{_defaultSerializerId}' 未注册，无法保存；" +
                    "请先 SaveModule.RegisterSerializer 注册，或经配置 / ApplyConfig 改用已注册的序列化器");
            }

            try
            {
                var payload = serializer.Serialize(data);
                var transformIds = new string[_transforms.Count];
                for (var i = 0; i < _transforms.Count; i++)
                {
                    payload = _transforms[i].Encode(payload);
                    transformIds[i] = _transforms[i].Id;
                }

                var file = SaveEnvelope.Wrap(serializer.Id, transformIds, dataVersion, payload);
                _storage.Write(key, file);
                return SaveResult.Success();
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
            {
                AesirModulesDebug.LogError(AesirModulesDebug.SaveModuleTag,
                    $"保存失败（键='{key}'）：{e.Message}");
                return SaveResult.Failure(e.Message);
            }
        }

        /// <inheritdoc />
        public LoadResult<T> Load<T>(string key, int currentVersion, ISaveMigrator<T> migrator)
        {
            SaveKey.Validate(key);

            var primary = TryLoadFrom<T>(_storage.Read(key), currentVersion, migrator, LoadSource.Primary);
            if (primary.Status == LoadStatus.Success ||
                primary.Status == LoadStatus.VersionMismatch ||
                primary.Status == LoadStatus.SerializerMissing)
            {
                return primary;
            }

            // 主存档缺失或损坏 → 尝试备份（崩溃点「主缺 bak 在」与「主坏 bak 好」的恢复路径）
            var backupBytes = _storage.ReadBackup(key);
            if (backupBytes == null)
            {
                return primary.Status == LoadStatus.NoSave
                    ? LoadResult<T>.NoSave()
                    : LoadResult<T>.Corrupted(primary.Error + "；备份缺失");
            }

            var backup = TryLoadFrom<T>(backupBytes, currentVersion, migrator, LoadSource.Backup);
            if (backup.Status == LoadStatus.Success)
            {
                AesirModulesDebug.LogWarning(AesirModulesDebug.SaveModuleTag,
                    $"主存档不可用（{primary.Error ?? "缺失"}），已从备份恢复（键='{key}'）");
                return backup;
            }

            return primary.Status == LoadStatus.NoSave
                ? backup
                : LoadResult<T>.Corrupted($"主存档与备份均不可用：主={primary.Error}；备份={backup.Error}");
        }

        /// <summary>
        /// 从单份存档字节走完整加载链。解析类失败一律判损坏（错误信息保留）；
        /// 版本迁移在 try 外执行，迁移器异常向上传播（fail-fast）。
        /// </summary>
        LoadResult<T> TryLoadFrom<T>(byte[] file, int currentVersion, ISaveMigrator<T> migrator,
            LoadSource source)
        {
            if (file == null)
            {
                return LoadResult<T>.NoSave();
            }

            if (!SaveEnvelope.TryUnwrap(file, out var envelope, out var error))
            {
                return LoadResult<T>.Corrupted(error);
            }

            if (!_serializers.TryGetValue(envelope.SerializerId, out var serializer))
            {
                return LoadResult<T>.SerializerMissing(envelope.SerializerId);
            }

            T value;
            try
            {
                var payload = envelope.Payload;
                for (var i = envelope.TransformIds.Length - 1; i >= 0; i--)
                {
                    var transform = FindTransform(envelope.TransformIds[i]);
                    if (transform == null)
                    {
                        return LoadResult<T>.Corrupted(
                            $"存档记录的变换 '{envelope.TransformIds[i]}' 未注册，无法还原");
                    }

                    payload = transform.Decode(payload);
                }

                value = serializer.Deserialize<T>(payload);
            }
            catch (Exception e) when (e is IOException || e is CryptographicException ||
                                      e is ArgumentException || e is FormatException ||
                                      e is System.Runtime.Serialization.SerializationException)
            {
                return LoadResult<T>.Corrupted($"存档还原失败：{e.Message}");
            }

            if (envelope.DataVersion != currentVersion)
            {
                if (migrator == null)
                {
                    return LoadResult<T>.VersionMismatch(envelope.DataVersion, currentVersion);
                }

                value = migrator.Migrate(value, envelope.DataVersion);
            }

            return LoadResult<T>.Success(value, source);
        }

        ISaveTransform FindTransform(string id)
        {
            for (var i = 0; i < _transforms.Count; i++)
            {
                if (_transforms[i].Id == id)
                {
                    return _transforms[i];
                }
            }

            return null;
        }
    }
}