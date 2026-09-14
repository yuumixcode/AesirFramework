using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    /// <summary>
    /// View / ViewController 适配层基类与 RemoveListener 触发器族的 EditMode 测试
    /// （两轮锐评点名的测试盲区）：泛型基类的 Context 绑定与 Command/Query 链路、
    /// RemoveListenerOnDestroyTrigger / RemoveListenerOnDisableTrigger 的批量移除语义。
    /// </summary>
    public class ViewAndTriggerTests
    {
        #region 测试角色

        class TestModel : AbstractModel
        {
            public int Value;

            public void SetValue(int value) => Value = value;
        }

        class TestContext : AbstractContext<TestContext>
        {
            protected override void Configure()
            {
                RegisterModel<TestModel>(new TestModel());
            }
        }

        class TestView : MonoView<TestContext>
        {
            public TestModel GetTestModel() => this.GetModel<TestModel>();
        }

        class TestViewController : MonoViewController<TestContext>
        {
            public void ExecuteSetValue(int value) => this.ExecuteCommand(new SetValueCommand(value));
            public int ExecuteGetValue() => this.ExecuteQuery<GetValueQuery, int>();
        }

        class SetValueCommand : AbstractCommand
        {
            readonly int _value;

            public SetValueCommand(int value) => _value = value;

            protected override void OnExecute() => this.GetModel<TestModel>().SetValue(_value);
        }

        class GetValueQuery : AbstractQuery<int>
        {
            protected override int OnExecute() => this.GetModel<TestModel>().Value;
        }

        #endregion

        #region 环境管理

        readonly List<GameObject> _createdObjects = new List<GameObject>();

        [SetUp]
        public void SetUp()
        {
            // 同域重复运行测试时清空静态单例（AbstractContext<T>._instance 等）
            ResetStaticsAssistant.ResetForTests();
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var created in _createdObjects)
            {
                if (created != null)
                {
                    Object.DestroyImmediate(created);
                }
            }

            _createdObjects.Clear();
            ResetStaticsAssistant.ResetForTests();
        }

        GameObject NewGameObject(string name)
        {
            var gameObject = new GameObject(name);
            _createdObjects.Add(gameObject);
            return gameObject;
        }

        static void InvokePrivate(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(method, "未找到私有方法：" + methodName);
            method.Invoke(target, null);
        }

        #endregion

        #region View / ViewController 基类

        [Test]
        public void MonoView_ContextBinding_CanGetModel()
        {
            var view = NewGameObject("TestView").AddComponent<TestView>();

            var model = view.GetTestModel();

            Assert.IsNotNull(model);
            Assert.AreSame(TestContext.Instance.GetModel<TestModel>(), model,
                "MonoView<T> 应经泛型绑定访问同一 Context 单例");
        }

        [Test]
        public void MonoViewController_CanExecuteCommandAndQuery()
        {
            var controller = NewGameObject("TestViewController").AddComponent<TestViewController>();

            controller.ExecuteSetValue(42);

            Assert.AreEqual(42, controller.ExecuteGetValue(),
                "MonoViewController<T> 的 Command 写入与 Query 读取链路应闭环");
        }

        #endregion

        #region RemoveListener 触发器族

        [Test]
        public void RemoveListenerWhenGameObjectOnDestroyed_RemovesOnDestroy()
        {
            var gameObject = NewGameObject("DestroyTriggerHost");
            var miniEvent = new MiniEvent();
            var fired = 0;
            miniEvent.AddListener(() => fired++).RemoveListenerWhenGameObjectOnDestroyed(gameObject);

            var trigger = gameObject.GetComponent<RemoveListenerOnDestroyTrigger>();
            Assert.IsNotNull(trigger, "扩展方法应自动挂载触发器组件");

            InvokePrivate(trigger, "OnDestroy");
            miniEvent.Invoke();

            Assert.AreEqual(0, fired, "OnDestroy 触发后监听应已批量移除");
        }

        [Test]
        public void RemoveListenerWhenGameObjectOnDisable_RemovesOnDisable()
        {
            var gameObject = NewGameObject("DisableTriggerHost");
            var miniEvent = new MiniEvent();
            var fired = 0;
            miniEvent.AddListener(() => fired++).RemoveListenerWhenGameObjectOnDisable(gameObject);

            var trigger = gameObject.GetComponent<RemoveListenerOnDisableTrigger>();
            Assert.IsNotNull(trigger, "扩展方法应自动挂载触发器组件");

            InvokePrivate(trigger, "OnDisable");
            miniEvent.Invoke();

            Assert.AreEqual(0, fired, "OnDisable 触发后监听应已批量移除");
        }

        [Test]
        public void RemoveListenerTrigger_CollectionClearsAfterTrigger()
        {
            var gameObject = NewGameObject("CollectionHost");
            var miniEvent = new MiniEvent();
            miniEvent.AddListener(() => { }).RemoveListenerWhenGameObjectOnDestroyed(gameObject);
            var trigger = gameObject.GetComponent<RemoveListenerOnDestroyTrigger>();

            InvokePrivate(trigger, "OnDestroy");

            // 触发后集合已清空：再次触发不应重复 Dispose（句柄幂等，此处锁定集合语义）
            Assert.DoesNotThrow(() => InvokePrivate(trigger, "OnDestroy"));
        }

        #endregion
    }
}
