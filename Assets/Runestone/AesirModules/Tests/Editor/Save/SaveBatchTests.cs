using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Runestone.AesirModules.Tests.Editor.Save
{
    /// <summary>
    /// 批量会话 EditMode 用例（spec「批量原子保存会话」）：提交成功一次性可见、混用类型不串扰、
    /// 提交失败保持旧值、条目缺失 / 未绑定 / 非法参数分支。
    /// </summary>
    /// <remarks>
    /// 测试自建预放置实例，存储经 SetStorage 路由到项目 Temp/ 唯一子目录，TearDown 销毁实例并回收目录。
    /// 条目数据用 [Serializable] POCO（与默认 unity-json 形状约束一致；裸原始类型不在模块能力内，见 spec）。
    /// </remarks>
    public class SaveBatchTests
    {
        string _root;
        GameObject _moduleObject;

        [SetUp]
        public void SetUp()
        {
            SaveModuleTestFixture.ResetStatics();
            var projectTemp = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Temp");
            _root = Path.Combine(projectTemp, "AesirSaveBatchTests", Guid.NewGuid().ToString("N"));

            _moduleObject = new GameObject("SaveModuleBatchTest");
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
        public void SaveBatch_ThenLoadBatch_TwoEntriesRoundTrip()
        {
            var save = SaveModule.SaveBatch("slot0", batch =>
            {
                batch.Set("gold", new BatchValue { value = 100 });
                batch.Set("name", new BatchName { text = "Hero" });
            });

            Assert.IsTrue(save.IsSuccess, save.Error);
            var loaded = SaveModule.LoadBatch("slot0");

            Assert.IsTrue(loaded.IsSuccess, loaded.Error);
            Assert.AreEqual(2, loaded.Value.Count);
            Assert.AreEqual(100, loaded.Value.Get<BatchValue>("gold").value);
            Assert.AreEqual("Hero", loaded.Value.Get<BatchName>("name").text);
        }

        [Test]
        public void SaveBatch_MixedTypes_NoCrossTalk()
        {
            var payload = SaveTestPayload.Sample(score: 77);
            Assert.IsTrue(SaveModule.SaveBatch("mixed", batch =>
            {
                batch.Set("count", new BatchValue { value = 5 });
                batch.Set("payload", payload);
            }).IsSuccess);

            var loaded = SaveModule.LoadBatch("mixed");

            Assert.IsTrue(loaded.IsSuccess, loaded.Error);
            Assert.AreEqual(5, loaded.Value.Get<BatchValue>("count").value);
            var restored = loaded.Value.Get<SaveTestPayload>("payload");
            Assert.IsNotNull(restored);
            Assert.AreEqual(77, restored.Score);
            Assert.AreEqual(payload.Position, restored.Position);
        }

        [Test]
        public void SaveBatch_FailedCommit_KeepsPreviousBatchValues()
        {
            Assert.IsTrue(SaveModule.SaveBatch("slot0", b => b.Set("gold", new BatchValue { value = 100 })).IsSuccess);

            // 换成 Write 抛 IO 异常的存储：旧值已由上一次真实写入落盘，本次提交失败不得改写主存档。
            SaveModule.SetStorage(new ThrowOnWriteStorage());
            // 环境失败经管理器 LogError（错误信息不吞噬），NUnit 需显式预期该日志。
            LogAssert.Expect(LogType.Error, new Regex("保存失败"));
            var failed = SaveModule.SaveBatch("slot0", b => b.Set("gold", new BatchValue { value = 200 }));
            Assert.IsFalse(failed.IsSuccess, "IO 失败应以结果对象表达");

            SaveModule.SetStorage(new FileSaveStorage(_root));
            var loaded = SaveModule.LoadBatch("slot0");
            Assert.IsTrue(loaded.IsSuccess, loaded.Error);
            Assert.AreEqual(100, loaded.Value.Get<BatchValue>("gold").value, "提交失败后应读到提交前的旧值");
        }

        [Test]
        public void Get_MissingEntryOrUnboundBatch_ReturnsDefault()
        {
            Assert.IsTrue(SaveModule.SaveBatch("slot0", b => b.Set("gold", new BatchValue { value = 100 })).IsSuccess);
            var loaded = SaveModule.LoadBatch("slot0");
            Assert.IsTrue(loaded.IsSuccess, loaded.Error);

            Assert.IsNull(loaded.Value.Get<BatchValue>("missing"), "条目缺失返回 default");
            Assert.IsNull(loaded.Value.Get<SaveTestPayload>("missing"), "引用类型条目缺失返回 null");

            var unbound = new SaveBatch();
            Assert.IsNull(unbound.Get<BatchValue>("gold"), "未绑定注册表返回 default");
        }

        [Test]
        public void Set_DuplicateName_OverwritesEntry()
        {
            Assert.IsTrue(SaveModule.SaveBatch("slot0", batch =>
            {
                batch.Set("gold", new BatchValue { value = 100 });
                batch.Set("gold", new BatchValue { value = 200 });
            }).IsSuccess);

            var loaded = SaveModule.LoadBatch("slot0");
            Assert.IsTrue(loaded.IsSuccess, loaded.Error);
            Assert.AreEqual(1, loaded.Value.Count);
            Assert.AreEqual(200, loaded.Value.Get<BatchValue>("gold").value);
        }

        [Test]
        public void Set_InvalidNameOrNullData_Throws()
        {
            Assert.IsTrue(SaveModule.SaveBatch("slot0", b => b.Set("gold", new BatchValue { value = 100 })).IsSuccess);
            var loaded = SaveModule.LoadBatch("slot0");
            Assert.IsTrue(loaded.IsSuccess, loaded.Error);

            Assert.Throws<ArgumentException>(() => loaded.Value.Set("", new BatchValue()));
            Assert.Throws<ArgumentNullException>(() => loaded.Value.Set<SaveTestPayload>("p", null));
        }

        /// <summary>Write 恒抛 IOException 的存储；旧值由前一次真实写入留在磁盘上，本存储只需让提交失败。</summary>
        sealed class ThrowOnWriteStorage : ISaveStorage
        {
            readonly Dictionary<string, byte[]> _data = new Dictionary<string, byte[]>();

            public byte[] Read(string key) => _data.TryGetValue(key, out var bytes) ? bytes : null;

            public byte[] ReadBackup(string key) => null;

            public void Write(string key, byte[] data) => throw new IOException("模拟写入失败");

            public void Delete(string key) => _data.Remove(key);

            public bool Exists(string key) => _data.ContainsKey(key);

            public IReadOnlyList<string> GetKeys() => new List<string>(_data.Keys);
        }
    }

    /// <summary>SaveModule 静态状态重置的共享工具（避免各测试类重复反射代码）。</summary>
    static class SaveModuleTestFixture
    {
        public static void ResetStatics()
        {
            var reset = typeof(SaveModule).GetMethod("ResetStatics",
                BindingFlags.NonPublic | BindingFlags.Static);
            reset.Invoke(null, null);

            var configReset = typeof(SaveConfigSO).GetMethod("ResetStatics",
                BindingFlags.NonPublic | BindingFlags.Static);
            configReset.Invoke(null, null);
        }
    }
}
