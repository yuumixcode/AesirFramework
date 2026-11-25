using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Runestone.AesirModules.Tests.Editor.Save
{
    /// <summary>
    /// GetTimestamp EditMode 用例（spec「存档时间戳查询」）：写后读到 UTC 时间、无存档返回 null、
    /// 存储不支持时间戳能力时返回 null 且不抛异常。
    /// </summary>
    public class SaveTimestampTests
    {
        string _root;
        GameObject _moduleObject;

        [SetUp]
        public void SetUp()
        {
            SaveModuleTestFixture.ResetStatics();
            var projectTemp = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Temp");
            _root = Path.Combine(projectTemp, "AesirSaveTimestampTests", Guid.NewGuid().ToString("N"));

            _moduleObject = new GameObject("SaveModuleTimestampTest");
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
        public void GetTimestamp_NoSave_ReturnsNull()
        {
            Assert.IsNull(SaveModule.GetTimestamp("missing"));
        }

        [Test]
        public void GetTimestamp_AfterSave_ReturnsUtcTime()
        {
            SaveModule.Save("slot0", SaveTestPayload.Sample());

            var timestamp = SaveModule.GetTimestamp("slot0");

            Assert.IsTrue(timestamp.HasValue, "已保存存档应返回时间戳");
            Assert.AreEqual(DateTimeKind.Utc, timestamp.Value.Kind);
            var now = DateTime.UtcNow;
            Assert.LessOrEqual(timestamp.Value, now.AddMinutes(1), "时间戳不应晚于当前时刻");
            Assert.GreaterOrEqual(timestamp.Value, now.AddMinutes(-1), "时间戳应为近期写入时刻");
        }

        [Test]
        public void GetTimestamp_StorageWithoutCapability_ReturnsNull()
        {
            // MemoryTestStorage 未实现 ISaveStorageTimestamp：能力缺失返回 null，不抛异常。
            SaveModule.SetStorage(new MemoryTestStorage());
            SaveModule.Save("slot0", SaveTestPayload.Sample());

            Assert.IsNull(SaveModule.GetTimestamp("slot0"));
        }
    }
}
