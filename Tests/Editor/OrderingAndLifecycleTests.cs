using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    /// <summary>
    /// 验证 <see cref="GenericLocator{T}" /> 的保序枚举、<see cref="AbstractContext{T}" /> 的
    /// 初始化正序 / 销毁逆序、未注册异常的近失识别提示，以及框架根单例的静态重置。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     保序与逆序是"注册顺序 = 依赖顺序"这一核心时序语义的结构保证：
    ///     底层不再依赖 <see cref="Dictionary{TKey,TValue}" /> 枚举顺序（无 .NET 契约保证）。
    ///     </para>
    ///     <para>
    ///     近失识别锁定"Register 与 Get 必须使用相同类型参数"的异常体验：
    ///     按实现类注册、按接口查询时，异常消息应识别兼容实例并提示。
    ///     </para>
    ///     <para>纯 C# 逻辑，EditMode 即可运行。</para>
    /// </remarks>
    /// <seealso cref="GenericLocator{T}.GetAll" />
    /// <seealso cref="AbstractContext{T}.Dispose" />
    public class OrderingAndLifecycleTests
    {
        static readonly List<string> OrderLog = new List<string>();

        [SetUp]
        public void SetUp()
        {
            ResetStaticsAssistant.ResetForTests();
            OrderLog.Clear();
        }

        /// <summary>
        /// 验证 GetAll 按注册顺序枚举。
        /// </summary>
        [Test]
        public void GetAll_ReturnsInInsertionOrder()
        {
            var locator = new GenericLocator<IItem>();
            var a = new ItemA();
            var b = new ItemB();
            var c = new ItemC();

            locator.Register(a);
            locator.Register(b);
            locator.Register(c);

            var order = new List<IItem>(locator.GetAll());
            Assert.AreSame(a, order[0]);
            Assert.AreSame(b, order[1]);
            Assert.AreSame(c, order[2]);
            AesirArchitectureDebug.LogTestInfo("GetAll: 按注册顺序枚举");
        }

        /// <summary>
        /// 验证 Unregister 后再 Register 按新插入语义追加到末尾（不填补原位置）。
        /// </summary>
        [Test]
        public void GetAll_UnregisterThenRegister_AppendsToEnd()
        {
            var locator = new GenericLocator<IItem>();
            var a = new ItemA();
            var b = new ItemB();
            var c = new ItemC();

            locator.Register(a);
            locator.Register(b);
            locator.Register(c);
            locator.Unregister<ItemB>();
            locator.Register(b); // 再注册：追加到末尾

            var order = new List<IItem>(locator.GetAll());
            Assert.AreEqual(3, order.Count);
            Assert.AreSame(a, order[0], "A 保持原位");
            Assert.AreSame(c, order[1], "C 上移到第二位");
            Assert.AreSame(b, order[2], "B 再注册后追加到末尾");
            AesirArchitectureDebug.LogTestInfo("GetAll(注销再注册): 追加到末尾");
        }

        /// <summary>
        /// 验证覆盖注册（同键重复注册）不改变插入顺序位置。
        /// </summary>
        [Test]
        public void GetAll_OverwriteRegister_KeepsOriginalPosition()
        {
            var locator = new GenericLocator<IItem>();
            var a1 = new ItemA();
            var b = new ItemB();
            var a2 = new ItemA();

            locator.Register(a1);
            locator.Register(b);
            locator.Register(a2); // 覆盖 A，位置不变

            var order = new List<IItem>(locator.GetAll());
            Assert.AreEqual(2, order.Count);
            Assert.AreSame(a2, order[0], "覆盖后 A 保持原位");
            Assert.AreSame(b, order[1]);
            AesirArchitectureDebug.LogTestInfo("GetAll(覆盖注册): 位置不变");
        }

        /// <summary>
        /// 验证 Context 初始化按注册顺序、销毁按注册逆序。
        /// </summary>
        [Test]
        public void Context_InitializeAndDispose_FollowRegistrationOrder()
        {
            var context = new OrderedContext();
            context.Initialize();

            Assert.AreEqual("M1.Init,M2.Init,S1.Init,S2.Init", string.Join(",", OrderLog),
                "初始化：Model 先（按注册序），Service 后（按注册序）");

            OrderLog.Clear();
            context.Dispose();

            Assert.AreEqual("S2.Dispose,S1.Dispose,M2.Dispose,M1.Dispose", string.Join(",", OrderLog),
                "销毁：Service 先（按注册逆序），Model 后（按注册逆序）");
            AesirArchitectureDebug.LogTestInfo("Context: 初始化正序、销毁逆序");
        }

        /// <summary>
        /// 验证近失识别：按实现类注册、按接口查询时，异常消息提示"相同类型参数"。
        /// </summary>
        [Test]
        public void GetModel_ImplementationKey_InterfaceQuery_NearMissHint()
        {
            var context = new NearMissContext();
            context.Initialize();

            var ex = Assert.Throws<InvalidOperationException>(() => context.GetModel<INearMissModel>());

            StringAssert.Contains("相同类型参数", ex.Message, "按实现类注册、按接口查询时应提示 Register 与 Get 必须使用相同类型参数");
            StringAssert.Contains("NearMissModel", ex.Message, "异常消息应包含注册键类型名");
            AesirArchitectureDebug.LogTestInfo("近失识别: 实现类注册、接口查询时提示类型参数一致");
        }

        /// <summary>
        /// 验证无近失时（完全不兼容类型）异常消息不含近失提示。
        /// </summary>
        [Test]
        public void GetModel_NoCompatibleRegistration_NoNearMissHint()
        {
            var context = new NearMissContext();
            context.Initialize();

            var ex = Assert.Throws<InvalidOperationException>(() => context.GetModel<UnrelatedModel>());

            StringAssert.DoesNotContain("相同类型参数", ex.Message, "无兼容注册时异常消息不应含近失提示");
            AesirArchitectureDebug.LogTestInfo("近失识别(无兼容): 不含提示");
        }

        /// <summary>
        /// 验证框架根单例的静态重置方法存在且能清空静态字段。
        /// </summary>
        /// <remarks>
        /// 直接经反射调用类内 <c>ResetStatics</c>（非经 ResetForTests 间接路径），
        /// 避免 <c>Instance</c> getter 的创建副作用干扰断言。
        /// </remarks>
        [Test]
        public void AesirArchitecture_ResetStatics_ClearsStaticFields()
        {
            var type = typeof(AesirArchitecture);
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Static;

            var instanceField = type.GetField("_instance", flags);
            var resetMethod = type.GetMethod("ResetStatics", flags);

            Assert.IsNotNull(instanceField, "_instance 字段应存在");
            Assert.IsNotNull(resetMethod, "ResetStatics 方法应存在（非泛型类按铁律类内自重置）");

            // 预置非默认值后调用重置（EditMode 下 AddComponent 不触发 Awake，实例字段不会写入 _instance）
            // 使用非泛型 AddComponent(Type)：本测试程序集未引用 Sirenix，
            // 泛型 AddComponent<T> 需编译期展开 AesirMonoBehaviour 的 Odin 基类链
            var go = new GameObject();
            var fakeInstance = go.AddComponent(typeof(AesirArchitecture));
            instanceField.SetValue(null, fakeInstance);
            resetMethod.Invoke(null, null);

            Assert.IsNull(instanceField.GetValue(null), "ResetStatics 后 _instance 应为 null");
            Object.DestroyImmediate(go);
            AesirArchitectureDebug.LogTestInfo("AesirArchitecture.ResetStatics: 静态字段正确清空");
        }

        /// <summary>
        /// 验证 <c>dontDestroyOnLoad</c> 序列化字段存在且默认值为 true（默认加入 DDOL 场景）。
        /// </summary>
        /// <remarks>
        /// DDOL 开关机制的核心契约：预放置与运行时创建两种来源共用字段默认值 true——
        /// 运行时创建的实例依赖该默认值自动进入 DDOL 场景（AddComponent 同步触发 Awake，无法在创建后修改）。
        /// </remarks>
        [Test]
        public void AesirArchitecture_DontDestroyOnLoad_DefaultsToTrue()
        {
            // 同上：非泛型 AddComponent(Type) 规避 Sirenix 基类链的编译期引用
            var go = new GameObject();
            var component = go.AddComponent(typeof(AesirArchitecture));
            var so = new SerializedObject(component);
            var prop = so.FindProperty(AesirArchitecture.DontDestroyOnLoadFieldName);

            Assert.IsNotNull(prop, "dontDestroyOnLoad 字段应存在且可序列化");
            Assert.IsTrue(prop.boolValue, "dontDestroyOnLoad 默认值应为 true（默认加入 DDOL 场景）");
            Object.DestroyImmediate(go);
            AesirArchitectureDebug.LogTestInfo("AesirArchitecture.dontDestroyOnLoad 默认值为 true");
        }

        // ──────────────────────────── 测试用具 ────────────────────────────

        interface IItem { }

        class ItemA : IItem { }

        class ItemB : IItem { }

        class ItemC : IItem { }

        class OrderedModel1 : AbstractModel
        {
            protected override void OnInitialize() => OrderLog.Add("M1.Init");
            protected override void OnDispose() => OrderLog.Add("M1.Dispose");
        }

        class OrderedModel2 : AbstractModel
        {
            protected override void OnInitialize() => OrderLog.Add("M2.Init");
            protected override void OnDispose() => OrderLog.Add("M2.Dispose");
        }

        class OrderedService1 : AbstractService
        {
            protected override void OnInitialize() => OrderLog.Add("S1.Init");
            protected override void OnDispose() => OrderLog.Add("S1.Dispose");
        }

        class OrderedService2 : AbstractService
        {
            protected override void OnInitialize() => OrderLog.Add("S2.Init");
            protected override void OnDispose() => OrderLog.Add("S2.Dispose");
        }

        [InternalContext]
        class OrderedContext : AbstractContext<OrderedContext>
        {
            protected override void Configure()
            {
                RegisterModel(new OrderedModel1());
                RegisterModel(new OrderedModel2());
                RegisterService(new OrderedService1());
                RegisterService(new OrderedService2());
            }
        }

        interface INearMissModel : IModel { }

        class NearMissModel : AbstractModel, INearMissModel { }

        class UnrelatedModel : AbstractModel { }

        [InternalContext]
        class NearMissContext : AbstractContext<NearMissContext>
        {
            protected override void Configure()
            {
                // 按实现类注册（而非接口）——制造"实现类注册、接口查询"的近失场景
                RegisterModel(new NearMissModel());
            }
        }
    }
}
