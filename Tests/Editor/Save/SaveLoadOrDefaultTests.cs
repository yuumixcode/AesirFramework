using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Runestone.AesirModules.Tests.Editor.Save
{
    /// <summary>
    /// LoadOrDefault EditMode 用例（spec「加载默认值」）：Success 返回值，NoSave / Corrupted /
    /// SerializerMissing / VersionMismatch 返回默认值，且加载不改写磁盘。
    /// </summary>
    public class SaveLoadOrDefaultTests
    {
        string _root;
        GameObject _moduleObject;

        [SetUp]
        public void SetUp()
        {
            SaveModuleTestFixture.ResetStatics();
            var projectTemp = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Temp");
            _root = Path.Combine(projectTemp, "AesirSaveDefaultTests", Guid.NewGuid().ToString("N"));

            _moduleObject = new GameObject("SaveModuleDefaultTest");
            _moduleObject.AddComponent<SaveModule>();
            SaveModule.SetStorage(new FileSaveStorage(_root));
        }

        [TearDown]
        public void TearDown()
        {
            if (_moduleObject != null)
            {
                UnityEngine.Object.DestroyImmediate(_moduleObject);
            }

            SaveModuleTestFixture.ResetStatics();
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }

        [Test]
        public void LoadOrDefault_NoSave_ReturnsDefaultValue()
        {
            var value = SaveModule.LoadOrDefault("missing", 999);

            Assert.AreEqual(999, value);
            Assert.IsFalse(File.Exists(Path.Combine(_root, "missing.sav")));
        }

        [Test]
        public void LoadOrDefault_Success_ReturnsSavedValueNotDefault()
        {
            SaveModule.Save("slot0", SaveTestPayload.Sample(score: 37));

            var value = SaveModule.LoadOrDefault("slot0", new SaveTestPayload { Score = -1 });

            Assert.IsNotNull(value);
            Assert.AreEqual(37, value.Score, "成功加载应返回存档值而非默认值");
        }

        [Test]
        public void LoadOrDefault_Corrupted_ReturnsDefaultAndKeepsDisk()
        {
            SaveModule.Save("slot0", SaveTestPayload.Sample());
            var path = Path.Combine(_root, "slot0.sav");
            var bytes = File.ReadAllBytes(path);
            bytes[bytes.Length - 1] ^= 0xFF; // 位翻转 payload → CRC 校验失败
            File.WriteAllBytes(path, bytes);

            var value = SaveModule.LoadOrDefault("slot0", new SaveTestPayload { Score = -5 });

            Assert.IsNotNull(value);
            Assert.AreEqual(-5, value.Score);
            Assert.AreEqual(bytes, File.ReadAllBytes(path), "加载路径不得改写磁盘内容");
        }

        [Test]
        public void LoadOrDefault_SerializerMissing_ReturnsDefaultValue()
        {
            var path = Path.Combine(_root, "unknown-serializer.sav");
            Directory.CreateDirectory(_root);
            // 构造一份记录未注册序列化器 Id 的合法信封（校验通过但还原阶段查不到序列化器）。
            var file = SaveEnvelope.Wrap("never-registered", Array.Empty<string>(), 0, new byte[] { 1, 2, 3 });
            File.WriteAllBytes(path, file);

            var value = SaveModule.LoadOrDefault("unknown-serializer", 404);

            Assert.AreEqual(404, value);
        }

        [Test]
        public void LoadOrDefault_VersionMismatch_ReturnsDefaultValue()
        {
            SaveModule.Save("slot0", SaveTestPayload.Sample(score: 9), dataVersion: 1);

            // 当前版本 2，无迁移器 → VersionMismatch → 默认值。
            var value = SaveModule.LoadOrDefault("slot0", new SaveTestPayload { Score = -1 }, currentVersion: 2);

            Assert.IsNotNull(value);
            Assert.AreEqual(-1, value.Score);
        }
    }
}
