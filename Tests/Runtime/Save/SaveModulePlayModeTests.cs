using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Runestone.AesirModules.Tests
{
    /// <summary>
    /// SaveModule 的 PlayMode 用例：自动保存周期触发（AesirTimer 帧驱动）、切后台钩子与配置开关、
    /// 清除后不触发、预放置实例优先（spec「自动保存与生命周期钩子」「模块生命周期惯例」）。
    /// <para>
    /// 销毁类与帧驱动行为必须 PlayMode 真执行（EditMode 下 Destroy 是 no-op、PlayerLoop 不推进）。
    /// TearDown 立即销毁实例与宿主并反射重置静态字段，避免污染后续用例。
    /// </para>
    /// </summary>
    public class SaveModulePlayModeTests
    {
        GameObject _moduleObject;
        int _autoSaveCount;

        [SetUp]
        public void SetUp()
        {
            _autoSaveCount = 0;
            ResetSaveStatics();
        }

        [TearDown]
        public void TearDown()
        {
            SaveModule.ClearAutoSave();
            if (_moduleObject != null)
            {
                Object.DestroyImmediate(_moduleObject);
            }

            var host = GameObject.Find("[Aesir Modules]");
            if (host != null)
            {
                Object.DestroyImmediate(host);
            }

            ResetSaveStatics();
        }

        static void ResetSaveStatics()
        {
            typeof(SaveModule).GetMethod("ResetStatics", BindingFlags.NonPublic | BindingFlags.Static)
                ?.Invoke(null, null);
            typeof(SaveConfigSO).GetMethod("ResetStatics", BindingFlags.NonPublic | BindingFlags.Static)
                ?.Invoke(null, null);
        }

        GameObject CreateModule()
        {
            _moduleObject = new GameObject("SaveModuleTest");
            _moduleObject.AddComponent<SaveModule>();
            return _moduleObject;
        }

        [UnityTest]
        public IEnumerator AutoSave_PeriodicInterval_FiresCallback()
        {
            CreateModule();
            SaveModule.SetAutoSave(() => _autoSaveCount++, 0.1f);

            yield return new WaitForSecondsRealtime(0.35f);

            Assert.GreaterOrEqual(_autoSaveCount, 1, "超过一个间隔后自动保存回调应至少触发一次");
        }

        [UnityTest]
        public IEnumerator OnApplicationPause_WithHookEnabled_FiresCallback()
        {
            var module = CreateModule();
            SaveModule.SetAutoSave(() => _autoSaveCount++, 60f); // 长间隔排除周期触发干扰

            module.SendMessage("OnApplicationPause", true);
            yield return null;

            Assert.AreEqual(1, _autoSaveCount, "切后台应触发一次保存回调");
        }

        [UnityTest]
        public IEnumerator OnApplicationPause_DisabledByConfig_DoesNotFire()
        {
            var module = CreateModule();
            var config = SaveConfigSO.CreateDefault();
            config.saveOnApplicationPause = false;
            SaveModule.ApplyConfig(config);
            SaveModule.SetAutoSave(() => _autoSaveCount++, 60f);

            module.SendMessage("OnApplicationPause", true);
            yield return null;

            Assert.AreEqual(0, _autoSaveCount, "配置关闭切后台保存后钩子不得触发");
        }

        [UnityTest]
        public IEnumerator ClearAutoSave_StopsPeriodicAndPauseHooks()
        {
            var module = CreateModule();
            SaveModule.SetAutoSave(() => _autoSaveCount++, 0.05f);
            yield return new WaitForSecondsRealtime(0.15f);
            var fired = _autoSaveCount;
            Assert.GreaterOrEqual(fired, 1, "清除前应已周期触发");

            SaveModule.ClearAutoSave();
            module.SendMessage("OnApplicationPause", true);
            yield return new WaitForSecondsRealtime(0.15f);

            Assert.AreEqual(fired, _autoSaveCount, "清除后周期与切后台均不得再触发");
        }

        [Test]
        public void PrePlacedInstance_IsPreferredOverRuntimeCreation()
        {
            _moduleObject = new GameObject("SaveModulePrePlaced");
            var module = _moduleObject.AddComponent<SaveModule>();

            Assert.AreSame(module, SaveModule.Instance, "预放置实例应优先于运行时创建");
            Assert.IsNull(GameObject.Find("[Aesir Modules]"), "存在预放置实例时不得创建宿主");
        }
    }
}