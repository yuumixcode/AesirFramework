using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Runestone.AesirModules.Tests.Editor.Save
{
    /// <summary>
    /// SaveModule 门面的 EditMode 用例：无配置资产往返、扩展点替换（管理器 / 序列化器 / 存储）、
    /// 静态重置（spec「保存管理扩展点」「配置资产与内存兜底」「模块生命周期惯例」）。
    /// <para>
    /// 测试自建预放置实例（EditMode 下 AddComponent 不触发 Awake，由 Ready 懒初始化兜底），
    /// 存储经 SetStorage 路由到项目 Temp/ 唯一子目录，TearDown 销毁实例并回收目录。
    /// </para>
    /// </summary>
    public class SaveModuleTests
    {
        string _root;
        GameObject _moduleObject;

        [SetUp]
        public void SetUp()
        {
            ResetSaveModuleStatics();
            var projectTemp = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Temp");
            _root = Path.Combine(projectTemp, "AesirSaveTests", Guid.NewGuid().ToString("N"));

            _moduleObject = new GameObject("SaveModuleTest");
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

            ResetSaveModuleStatics();
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }

        static void ResetSaveModuleStatics()
        {
            var reset = typeof(SaveModule).GetMethod("ResetStatics",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(reset, "SaveModule 应按框架铁律在类内声明 RIOLM ResetStatics");
            reset.Invoke(null, null);

            var configReset = typeof(SaveConfigSO).GetMethod("ResetStatics",
                BindingFlags.NonPublic | BindingFlags.Static);
            configReset.Invoke(null, null);
        }

        [Test]
        public void SaveLoad_WithoutConfigAsset_RoundTrips()
        {
            var payload = SaveTestPayload.Sample();

            Assert.IsTrue(SaveModule.Save("slot0", payload).IsSuccess);
            var result = SaveModule.Load<SaveTestPayload>("slot0");

            Assert.IsTrue(result.IsSuccess, result.Error);
            Assert.AreEqual(payload.Score, result.Value.Score);
            Assert.AreEqual(payload.Position, result.Value.Position);
        }

        [Test]
        public void SetManager_CustomManager_TakesOverSaveAndLoad()
        {
            var recorder = new RecordingTestManager();
            SaveModule.SetManager(recorder);

            SaveModule.Save("slot0", SaveTestPayload.Sample());
            SaveModule.Load<SaveTestPayload>("slot0");

            Assert.AreEqual(1, recorder.SaveCalls, "保存应进入自定义管理器");
            Assert.AreEqual(1, recorder.LoadCalls, "加载应进入自定义管理器");
        }

        [Test]
        public void RegisterSerializer_WithApplyConfig_RoundTripsCustomSerializer()
        {
            SaveModule.RegisterSerializer(new Base64TestSerializer());
            var config = SaveConfigSO.CreateDefault();
            config.defaultSerializerId = "test-base64";
            SaveModule.ApplyConfig(config);

            Assert.IsTrue(SaveModule.Save("slot0", SaveTestPayload.Sample()).IsSuccess);
            var result = SaveModule.Load<SaveTestPayload>("slot0");

            Assert.IsTrue(result.IsSuccess, result.Error);
            Assert.AreEqual(42, result.Value.Score);
        }

        [Test]
        public void SetStorage_MemoryStorage_ProducesNoFileSystemOutput()
        {
            var memory = new MemoryTestStorage();
            SaveModule.SetStorage(memory);

            Assert.IsTrue(SaveModule.Save("memory-only", SaveTestPayload.Sample()).IsSuccess);
            var result = SaveModule.Load<SaveTestPayload>("memory-only");

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, memory.WriteCount);
            Assert.IsFalse(File.Exists(Path.Combine(_root, "memory-only.sav")),
                "替换内存存储后文件系统不得产出存档");
        }

        [Test]
        public void Delete_ThenLoad_ReturnsNoSave()
        {
            SaveModule.Save("slot0", SaveTestPayload.Sample());

            SaveModule.Delete("slot0");
            var result = SaveModule.Load<SaveTestPayload>("slot0");

            Assert.AreEqual(LoadStatus.NoSave, result.Status);
            Assert.IsFalse(SaveModule.Exists("slot0"));
        }

        [Test]
        public void ResetStatics_ClearsSingletonInstance()
        {
            var module = _moduleObject.GetComponent<SaveModule>();
            Assert.AreSame(module, SaveModule.Instance);

            var reset = typeof(SaveModule).GetMethod("ResetStatics",
                BindingFlags.NonPublic | BindingFlags.Static);
            reset.Invoke(null, null);

            var field = typeof(SaveModule).GetField("_instance",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNull(field.GetValue(null), "ResetStatics 应清空静态单例引用");

            // 场景中的预放置实例仍在，Instance 经 FindAnyObjectByType 兜底重发现
            Assert.AreSame(module, SaveModule.Instance);
        }
    }
}