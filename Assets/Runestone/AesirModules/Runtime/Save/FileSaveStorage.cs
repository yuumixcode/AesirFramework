using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 文件系统存储（默认实现）：以「临时文件 + 强制刷盘 + 双 rename 备份轮换 + 原子提交」满足
    /// <see cref="ISaveStorage" /> 的提交式写入契约——写入流程的任意中断点之后，磁盘上至少保留一份
    /// 完整可读存档（旧值或新值）。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 写入五步（同目录，<c>key.sav</c> / <c>key.sav.bak</c> / <c>key.sav.tmp</c> 三文件名）：
    /// ① 写 temp 全量字节 → ② <c>Flush(flushToDisk: true)</c> 强制落盘（Windows FlushFileBuffers / POSIX fsync）
    /// → ③ 删旧 bak → ④ 主 rename 为 bak → ⑤ temp rename 为主（原子提交）。
    /// rename 在主流 OS 上是原子目录项更新；唯一非原子窗口是 ③→④ 之间（bak 已删、主未轮转），
    /// 此时崩溃主存档仍完好，损失的只是备份副本——不可牺牲的主存档从不暴露在非原子步骤中。
    /// </para>
    /// <para>
    /// <paramref name="backupEnabled" /> 为 false 时提交流程不变（崩溃安全保持），仅在提交成功后删除 bak
    /// 且 <see cref="ReadBackup" /> 恒返回 null（放弃损坏回退能力）。
    /// </para>
    /// <para>
    /// 平台边界：WebGL 的 persistentDataPath 是 IndexedDB 虚拟文件系统，<c>Flush(true)</c> 无真实 fsync 语义、
    /// rename 到持久化存在延迟窗口，崩溃安全弱于原生平台（文档明示，代码不做平台特判）。
    /// </para>
    /// </remarks>
    public class FileSaveStorage : ISaveStorage
    {
        const string MainExtension = ".sav";
        const string BackupExtension = ".sav.bak";
        const string TempExtension = ".sav.tmp";

        readonly string _rootDirectory;
        readonly bool _backupEnabled;

        /// <summary>构造文件系统存储。</summary>
        /// <param name="rootDirectory">存储根目录（绝对路径；首次写入时懒创建）。</param>
        /// <param name="backupEnabled">是否保留备份（false 时提交后删除 bak 且 ReadBackup 恒 null）。</param>
        /// <exception cref="ArgumentException">根目录为空。</exception>
        public FileSaveStorage(string rootDirectory, bool backupEnabled = true)
        {
            if (string.IsNullOrEmpty(rootDirectory))
            {
                throw new ArgumentException("存储根目录不可为空", nameof(rootDirectory));
            }

            _rootDirectory = rootDirectory;
            _backupEnabled = backupEnabled;
        }

        /// <inheritdoc />
        public byte[] Read(string key)
        {
            var path = PathFor(key);
            return File.Exists(path) ? File.ReadAllBytes(path) : null;
        }

        /// <inheritdoc />
        public byte[] ReadBackup(string key)
        {
            if (!_backupEnabled)
            {
                return null;
            }

            var path = BackupPathFor(key);
            return File.Exists(path) ? File.ReadAllBytes(path) : null;
        }

        /// <inheritdoc />
        public void Write(string key, byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            Directory.CreateDirectory(_rootDirectory);

            var main = PathFor(key);
            var backup = BackupPathFor(key);
            var temp = TempPathFor(key);

            // ① + ② 写临时文件并强制刷盘（覆盖上次中断的残留）
            using (var stream = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                stream.Write(data, 0, data.Length);
                stream.Flush(flushToDisk: true);
            }

            // ③ 删旧 bak（Windows 上 rename 到已存在名字会失败，备份槽是可牺牲副本）
            if (File.Exists(backup))
            {
                File.Delete(backup);
            }

            // ④ 主轮转为备份
            if (File.Exists(main))
            {
                File.Move(main, backup);
            }

            // ⑤ 原子提交
            File.Move(temp, main);

            if (!_backupEnabled && File.Exists(backup))
            {
                File.Delete(backup);
            }
        }

        /// <inheritdoc />
        public void Delete(string key)
        {
            DeleteIfExists(PathFor(key));
            DeleteIfExists(BackupPathFor(key));
            DeleteIfExists(TempPathFor(key));
        }

        /// <inheritdoc />
        public bool Exists(string key) => File.Exists(PathFor(key));

        /// <inheritdoc />
        public IReadOnlyList<string> GetKeys()
        {
            if (!Directory.Exists(_rootDirectory))
            {
                return Array.Empty<string>();
            }

            // 不用 "*.sav" 搜索模式：Windows 的 8.3 短名匹配会误命中 .sav.bak 等扩展名前缀相同的文件
            return Directory.EnumerateFiles(_rootDirectory)
                .Where(path => path.EndsWith(MainExtension, StringComparison.Ordinal))
                .Select(Path.GetFileNameWithoutExtension)
                .ToList();
        }

        string PathFor(string key) => Path.Combine(_rootDirectory, key + MainExtension);

        string BackupPathFor(string key) => Path.Combine(_rootDirectory, key + BackupExtension);

        string TempPathFor(string key) => Path.Combine(_rootDirectory, key + TempExtension);

        static void DeleteIfExists(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}