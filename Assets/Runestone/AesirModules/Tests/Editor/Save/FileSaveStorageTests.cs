using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Runestone.AesirModules.Tests.Editor.Save
{
    /// <summary>
    /// FileSaveStorage 的 EditMode 用例：提交式写入、备份轮换、崩溃点磁盘状态恢复、
    /// 临时残留处理、枚举与删除（spec「事务式写入的崩溃安全」）。
    /// 文件 IO 走项目 Temp/ 下唯一子目录，TearDown 回收。
    /// </summary>
    public class FileSaveStorageTests
    {
        string _root;
        FileSaveStorage _storage;

        [SetUp]
        public void SetUp()
        {
            var projectTemp = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Temp");
            _root = Path.Combine(projectTemp, "AesirSaveTests", Guid.NewGuid().ToString("N"));
            _storage = new FileSaveStorage(_root);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }

        [Test]
        public void Write_ThenRead_ReturnsCommittedBytes()
        {
            var data = new byte[] { 1, 2, 3 };
            _storage.Write("slot0", data);

            Assert.AreEqual(data, _storage.Read("slot0"));
            Assert.IsTrue(_storage.Exists("slot0"));
        }

        [Test]
        public void Write_OverExisting_RotatesPreviousToBackup()
        {
            _storage.Write("slot0", new byte[] { 1 });
            _storage.Write("slot0", new byte[] { 2 });

            Assert.AreEqual(new byte[] { 2 }, _storage.Read("slot0"));
            Assert.AreEqual(new byte[] { 1 }, _storage.ReadBackup("slot0"), "上一份提交数据应轮转为备份");
        }

        [Test]
        public void Write_WithTempResidue_CommitsAndCleansResidue()
        {
            Directory.CreateDirectory(_root);
            File.WriteAllBytes(Path.Combine(_root, "slot0.sav.tmp"), new byte[] { 9, 9 });
            _storage.Write("slot0", new byte[] { 1 });

            Assert.AreEqual(new byte[] { 1 }, _storage.Read("slot0"), "残留临时文件不得影响加载");
            Assert.IsFalse(File.Exists(Path.Combine(_root, "slot0.sav.tmp")), "提交后临时文件应被复用清除");
        }

        [Test]
        public void Read_MainMissingBackupPresent_ReturnsBackupBytes()
        {
            // 模拟崩溃点④之后：主已轮转为备份、temp 未提交
            _storage.Write("slot0", new byte[] { 1 });
            File.Move(Path.Combine(_root, "slot0.sav"), Path.Combine(_root, "slot0.sav.bak"));

            Assert.IsNull(_storage.Read("slot0"));
            Assert.AreEqual(new byte[] { 1 }, _storage.ReadBackup("slot0"));
        }

        [Test]
        public void ReadBackup_BackupDisabled_ReturnsNull()
        {
            var storage = new FileSaveStorage(_root, backupEnabled: false);
            storage.Write("slot0", new byte[] { 1 });
            storage.Write("slot0", new byte[] { 2 });

            Assert.AreEqual(new byte[] { 2 }, storage.Read("slot0"), "关闭备份仍应原子提交新数据");
            Assert.IsNull(storage.ReadBackup("slot0"));
            Assert.IsFalse(File.Exists(Path.Combine(_root, "slot0.sav.bak")), "关闭备份提交后不应残留 bak");
        }

        [Test]
        public void Delete_RemovesMainBackupAndTemp()
        {
            _storage.Write("slot0", new byte[] { 1 });
            _storage.Write("slot0", new byte[] { 2 });
            File.WriteAllBytes(Path.Combine(_root, "slot0.sav.tmp"), new byte[] { 9 });

            _storage.Delete("slot0");

            Assert.IsFalse(_storage.Exists("slot0"));
            Assert.IsNull(_storage.ReadBackup("slot0"));
            Assert.IsFalse(File.Exists(Path.Combine(_root, "slot0.sav.tmp")));
        }

        [Test]
        public void Delete_NonExistingKey_NoSideEffect()
        {
            Assert.DoesNotThrow(() => _storage.Delete("never-saved"));
            Assert.IsFalse(_storage.Exists("never-saved"));
        }

        [Test]
        public void GetKeys_ExcludesTempAndBackupFiles()
        {
            _storage.Write("slot0", new byte[] { 1 });
            _storage.Write("slot0", new byte[] { 2 }); // 产生 bak
            _storage.Write("slot1", new byte[] { 3 });
            File.WriteAllBytes(Path.Combine(_root, "slot2.sav.tmp"), new byte[] { 9 });

            var keys = _storage.GetKeys();

            Assert.AreEqual(new[] { "slot0", "slot1" }, keys.OrderBy(k => k).ToArray());
        }
    }
}