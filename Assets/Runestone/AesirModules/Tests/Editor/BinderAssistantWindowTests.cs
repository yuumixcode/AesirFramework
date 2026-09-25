using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Runestone.AesirModules.Tests.Editor
{
    /// <summary>
    /// BinderAssistant 的 Canvas 根窗口感知测试：
    /// 默认脚本名后缀（Canvas 根 → Window，面板根 → Panel，已有后缀去重）、
    /// 默认基类（Canvas 根 → <see cref="AesirBaseWindow" />）、基类下拉包含窗口家族。
    /// </summary>
    public class BinderAssistantWindowTests
    {
        /// <summary>AddComponent 触发 Reset 后，读取助手上的公开配置字段做断言。</summary>
        static BinderAssistant CreateAssistant(GameObject host)
        {
            return host.AddComponent<BinderAssistant>();
        }

        [Test]
        public void DefaultScriptName_CanvasRoot_AppendsWindowSuffixAndWindowBaseType()
        {
            var host = new GameObject("Setting");
            host.AddComponent<Canvas>();

            var assistant = CreateAssistant(host);

            Assert.AreEqual("SettingWindow", assistant.ScriptName,
                "Canvas 根物体上的默认脚本名应追加 Window 后缀");
            Assert.AreEqual(typeof(AesirBaseWindow).FullName, assistant.BaseType,
                "Canvas 根物体上的默认基类应指向 AesirBaseWindow");
            Object.DestroyImmediate(host);
        }

        [Test]
        public void DefaultScriptName_PanelRoot_AppendsPanelSuffixAndMonoBehaviourBaseType()
        {
            var host = new GameObject("Setting");

            var assistant = CreateAssistant(host);

            Assert.AreEqual("SettingPanel", assistant.ScriptName,
                "非 Canvas 根物体上的默认脚本名应追加 Panel 后缀");
            Assert.AreEqual(typeof(MonoBehaviour).FullName, assistant.BaseType,
                "非 Canvas 根物体上的默认基类保持 MonoBehaviour");
            Object.DestroyImmediate(host);
        }

        [Test]
        public void DefaultScriptName_AlreadySuffixedObjectName_DoesNotDuplicateSuffix()
        {
            var panelHost = new GameObject("ScorePanel");
            Assert.AreEqual("ScorePanel", CreateAssistant(panelHost).ScriptName,
                "物体名已以 Panel 结尾时不应重复拼接");
            Object.DestroyImmediate(panelHost);

            var windowHost = new GameObject("LoadingWindow");
            windowHost.AddComponent<Canvas>();
            Assert.AreEqual("LoadingWindow", CreateAssistant(windowHost).ScriptName,
                "物体名已以 Window 结尾时不应重复拼接");
            Object.DestroyImmediate(windowHost);
        }

        [Test]
        public void GetBaseTypes_ContainsAesirWindowFamily()
        {
            var host = new GameObject("BaseTypeHost");
            var assistant = CreateAssistant(host);
            var values = new HashSet<string>(assistant.GetBaseTypes().Select(item => item.Value));

            Assert.IsTrue(values.Contains(typeof(AesirBaseWindow).FullName), "基类下拉应包含 AesirBaseWindow");
            Assert.IsTrue(values.Contains("Runestone.AesirModules.AesirBaseWindowView<T>"),
                "基类下拉应包含 AesirBaseWindowView<T> 占位");
            Assert.IsTrue(values.Contains("Runestone.AesirModules.AesirBaseWindowViewController<T>"),
                "基类下拉应包含 AesirBaseWindowViewController<T> 占位");
            Assert.IsTrue(values.Contains(typeof(AesirBasePanel).FullName), "面板家族应保持存在");
            Object.DestroyImmediate(host);
        }

        [Test]
        public void GetBaseTypes_WindowGenericBase_TriggersContextDropdownFlag()
        {
            // 经反射驱动 IsAesirGenericUiBase：窗口泛型基类应与面板泛型基类同样触发「Context 类型」下拉
            var host = new GameObject("GenericHost");
            var assistant = CreateAssistant(host);
            assistant.BaseType = "Runestone.AesirModules.AesirBaseWindowView<T>";

            var property = typeof(BinderAssistant)
                .GetProperty("IsAesirGenericUiBase",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(property, "IsAesirGenericUiBase 应存在（私有属性）");
            Assert.IsTrue((bool)property.GetValue(assistant), "窗口泛型基类应触发 Context 下拉");

            assistant.BaseType = "Runestone.AesirModules.AesirBaseWindowViewController<T>";
            Assert.IsTrue((bool)property.GetValue(assistant), "窗口 ViewController 泛型基类同样应触发");

            assistant.BaseType = typeof(MonoBehaviour).FullName;
            Assert.IsFalse((bool)property.GetValue(assistant), "非泛型基类不应触发");
            Object.DestroyImmediate(host);
        }
    }
}
