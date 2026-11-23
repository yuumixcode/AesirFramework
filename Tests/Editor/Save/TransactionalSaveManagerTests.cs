using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Runestone.AesirModules.Tests.Editor.Save
{
    /// <summary>
    /// TransactionalSaveManager 的 EditMode 用例：按档 Id 还原、序列化器缺失、版本迁移、
    /// 损坏回退备份、加载无磁盘写副作用（spec「序列化器标识与按档还原」「数据版本与迁移」「完整性校验与损坏回退」）。
    /// </summary>
    public class TransactionalSaveManagerTests
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

        TransactionalSaveManager CreateManager(string defaultSerializerId = "unity-json",
            params ISaveSerializer[] extraSerializers)
        {
            var serializers = new Dictionary<string, ISaveSerializer>
            {
                ["unity-json"] = new UnityJsonSerializer()
            };
            foreach (var extra in extraSerializers)
            {
                serializers[extra.Id] = extra;
            }

            return new TransactionalSaveManager(serializers, _storage, new List<ISaveTransform>(),
                defaultSerializerId);
        }

        [Test]
        public void Save_ThenLoad_RoundTripsFromPrimary()
        {
            var manager = CreateManager();
            var payload = SaveTestPayload.Sample();

            Assert.IsTrue(manager.Save("slot0", payload, 0).IsSuccess);
            var result = manager.Load<SaveTestPayload>("slot0", 0, null);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(LoadSource.Primary, result.Source);
            Assert.AreEqual(payload.Score, result.Value.Score);
            Assert.AreEqual(payload.Inventory, result.Value.Inventory);
        }

        [Test]
        public void Load_AfterDefaultSerializerSwitch_UsesRecordedSerializer()
        {
            var base64 = new Base64TestSerializer();
            var oldManager = CreateManager("test-base64", base64);
            Assert.IsTrue(oldManager.Save("slot0", SaveTestPayload.Sample(), 0).IsSuccess);

            // 切换默认为 unity-json 的新管理器：旧档仍按信封记录的 test-base64 还原
            var newManager = CreateManager("unity-json", base64);
            var result = newManager.Load<SaveTestPayload>("slot0", 0, null);

            Assert.IsTrue(result.IsSuccess, result.Error);
            Assert.AreEqual(42, result.Value.Score);
        }

        [Test]
        public void Load_UnregisteredSerializer_ReturnsSerializerMissing()
        {
            var base64 = new Base64TestSerializer();
            var oldManager = CreateManager("test-base64", base64);
            oldManager.Save("slot0", SaveTestPayload.Sample(), 0);

            // 新管理器不注册 test-base64
            var result = CreateManager().Load<SaveTestPayload>("slot0", 0, null);

            Assert.AreEqual(LoadStatus.SerializerMissing, result.Status);
            StringAssert.Contains("test-base64", result.Error);
        }

        [Test]
        public void Load_VersionMismatchWithoutMigrator_ReturnsVersionMismatch()
        {
            var manager = CreateManager();
            manager.Save("slot0", SaveTestPayload.Sample(), 1);

            var result = manager.Load<SaveTestPayload>("slot0", 2, null);

            Assert.AreEqual(LoadStatus.VersionMismatch, result.Status);
            Assert.AreEqual(1, result.SavedVersion);
            Assert.AreEqual(2, result.CurrentVersion);
        }

        [Test]
        public void Load_VersionMismatchWithMigrator_UpgradesWithoutDiskWriteBack()
        {
            var manager = CreateManager();
            manager.Save("slot0", SaveTestPayload.Sample(10), 1);
            var before = File.ReadAllBytes(Path.Combine(_root, "slot0.sav"));

            var result = manager.Load("slot0", 2, new AddTenMigrator());

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(110, result.Value.Score, "迁移器应把旧版 Score 加 100");
            Assert.AreEqual(before, File.ReadAllBytes(Path.Combine(_root, "slot0.sav")),
                "迁移结果不得在加载路径写回磁盘");
        }

        [Test]
        public void Load_CorruptedPrimary_FallsBackToBackup()
        {
            var manager = CreateManager();
            manager.Save("slot0", SaveTestPayload.Sample(1), 0);
            manager.Save("slot0", SaveTestPayload.Sample(2), 0); // bak = 第一份

            var mainPath = Path.Combine(_root, "slot0.sav");
            var corrupted = File.ReadAllBytes(mainPath);
            corrupted[corrupted.Length - 1] ^= 0xFF;
            File.WriteAllBytes(mainPath, corrupted);

            var result = manager.Load<SaveTestPayload>("slot0", 0, null);

            Assert.IsTrue(result.IsSuccess, result.Error);
            Assert.AreEqual(LoadSource.Backup, result.Source);
            Assert.AreEqual(1, result.Value.Score, "应回退到上一份完整存档");
        }

        [Test]
        public void Load_BothCorrupted_ReturnsCorrupted()
        {
            var manager = CreateManager();
            manager.Save("slot0", SaveTestPayload.Sample(1), 0);
            manager.Save("slot0", SaveTestPayload.Sample(2), 0);

            CorruptFile(Path.Combine(_root, "slot0.sav"));
            CorruptFile(Path.Combine(_root, "slot0.sav.bak"));

            var result = manager.Load<SaveTestPayload>("slot0", 0, null);

            Assert.AreEqual(LoadStatus.Corrupted, result.Status);
            Assert.IsNotNull(result.Error);
        }

        [Test]
        public void Load_NeverSaved_ReturnsNoSave()
        {
            var result = CreateManager().Load<SaveTestPayload>("never-saved", 0, null);

            Assert.AreEqual(LoadStatus.NoSave, result.Status);
        }

        [Test]
        public void Save_WithoutTransforms_PayloadIsRawSerializerOutput()
        {
            var manager = CreateManager();
            var payload = SaveTestPayload.Sample();

            Assert.IsTrue(manager.Save("slot0", payload, 0).IsSuccess);

            var file = File.ReadAllBytes(Path.Combine(_root, "slot0.sav"));
            Assert.IsTrue(SaveEnvelope.TryUnwrap(file, out var envelope, out var error), error);
            Assert.AreEqual(0, envelope.TransformIds.Length);
            Assert.AreEqual(new UnityJsonSerializer().Serialize(payload), envelope.Payload,
                "默认无变换链时 payload 应为序列化器原始输出");
        }

        [Test]
        public void Load_FallbackPath_DoesNotModifyDisk()
        {
            var manager = CreateManager();
            manager.Save("slot0", SaveTestPayload.Sample(1), 0);
            manager.Save("slot0", SaveTestPayload.Sample(2), 0);
            CorruptFile(Path.Combine(_root, "slot0.sav"));

            // 损坏后快照：加载（含回退路径）不得改写任何文件
            var snapshot = Directory.GetFiles(_root)
                .ToDictionary(p => p, p => (bytes: File.ReadAllBytes(p), mtime: File.GetLastWriteTimeUtc(p)));

            var result = manager.Load<SaveTestPayload>("slot0", 0, null);
            Assert.IsTrue(result.IsSuccess);

            foreach (var pair in snapshot)
            {
                Assert.AreEqual(pair.Value.bytes, File.ReadAllBytes(pair.Key), $"文件 {pair.Key} 内容被加载改写");
                Assert.AreEqual(pair.Value.mtime, File.GetLastWriteTimeUtc(pair.Key),
                    $"文件 {pair.Key} 修改时间被加载改写");
            }
        }

        [Test]
        public void Save_DefaultSerializerUnregistered_ThrowsInvalidOperation()
        {
            var manager = CreateManager("missing-serializer");

            Assert.Throws<InvalidOperationException>(
                () => manager.Save("slot0", SaveTestPayload.Sample(), 0));
        }

        static void CorruptFile(string path)
        {
            var bytes = File.ReadAllBytes(path);
            bytes[bytes.Length - 1] ^= 0xFF;
            File.WriteAllBytes(path, bytes);
        }
    }
}