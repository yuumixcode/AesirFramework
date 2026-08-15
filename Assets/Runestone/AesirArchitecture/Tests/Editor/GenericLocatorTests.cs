using NUnit.Framework;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    /// <summary>
    /// 验证 <see cref="GenericLocator{T}" /> 的注册、查询、注销与清空行为。
    /// </summary>
    /// <remarks>
    /// <para>
    /// GenericLocator 是 <see cref="AbstractContext{T}" /> 内部 Model / Service 容器的底层实现，
    /// 其注册键语义（以 <c>typeof(TItem)</c> 为键、注册与查询必须使用相同类型参数）是框架的核心约定。
    /// </para>
    /// <para>纯 C# 逻辑，EditMode 即可运行。</para>
    /// </remarks>
    /// <seealso cref="GenericLocator{T}"/>
    public class GenericLocatorTests
    {
        /// <summary>
        /// 测试用实现类型，与接口键区分以验证键语义
        /// </summary>
        interface IItem
        {
        }

        class ItemA : IItem
        {
        }

        class ItemB : IItem
        {
        }

        /// <summary>
        /// 验证注册后可按同一类型参数获取，未注册类型返回 null。
        /// </summary>
        [Test]
        public void Register_ThenGet_ReturnsInstance_UnregisteredReturnsNull()
        {
            var locator = new GenericLocator<IItem>();
            var a = new ItemA();

            locator.Register(a);

            Assert.AreSame(a, locator.Get<ItemA>(), "注册后按同一类型参数应取回同一实例");
            Assert.IsNull(locator.Get<ItemB>(), "未注册类型应返回 null");
            AesirArchitectureDebug.LogTestInfo("Register/Get: 同键取回实例，未注册返回 null");
        }

        /// <summary>
        /// 验证重复注册同类型键时覆盖旧实例。
        /// </summary>
        [Test]
        public void Register_SameKeyTwice_Overwrites()
        {
            var locator = new GenericLocator<IItem>();
            var first = new ItemA();
            var second = new ItemA();

            locator.Register(first);
            locator.Register(second);

            Assert.AreSame(second, locator.Get<ItemA>(), "重复注册同键应覆盖为最新实例");
            AesirArchitectureDebug.LogTestInfo("Register(重复): 同键覆盖");
        }

        /// <summary>
        /// 验证 TryGet 与 IsRegistered 的查询行为。
        /// </summary>
        [Test]
        public void TryGet_And_IsRegistered_ReflectRegistry()
        {
            var locator = new GenericLocator<IItem>();
            var a = new ItemA();

            Assert.IsFalse(locator.IsRegistered<ItemA>(), "未注册时 IsRegistered 为 false");
            Assert.IsFalse(locator.TryGet<ItemA>(out var none), "未注册时 TryGet 返回 false");
            Assert.IsNull(none);

            locator.Register(a);

            Assert.IsTrue(locator.IsRegistered<ItemA>());
            Assert.IsTrue(locator.TryGet<ItemA>(out var got));
            Assert.AreSame(a, got);
            AesirArchitectureDebug.LogTestInfo("TryGet/IsRegistered: 正确反映注册状态");
        }

        /// <summary>
        /// 验证接口键注册：以接口类型为键时，需用相同接口类型查询，用实现类查询取不到（键语义约定）。
        /// </summary>
        /// <remarks>
        /// 此测试锁定"Register 与 Get 必须使用相同类型参数"的框架约定，防止未来有人"修复"为按兼容类型查找。
        /// </remarks>
        [Test]
        public void Register_InterfaceKey_GetByImplementation_DoesNotMatch()
        {
            var locator = new GenericLocator<IItem>();
            var a = new ItemA();

            locator.Register<IItem>(a);

            Assert.AreSame(a, locator.Get<IItem>(), "按注册时的接口键查询应取回实例");
            Assert.IsNull(locator.Get<ItemA>(), "以实现类查询接口键注册的实例应返回 null（键精确匹配约定）");
            AesirArchitectureDebug.LogTestInfo("键语义: 注册与查询必须使用相同类型参数");
        }

        /// <summary>
        /// 验证非泛型注册入口 <see cref="GenericLocator{T}.Register(System.Type, T)"/> 的类型匹配校验。
        /// </summary>
        [Test]
        public void Register_ByType_TypeMismatch_Throws()
        {
            var locator = new GenericLocator<IItem>();

            Assert.Throws<System.ArgumentException>(() => locator.Register(typeof(ItemB), new ItemA()),
                "实例类型与注册键不匹配时应抛出 ArgumentException");
            AesirArchitectureDebug.LogTestInfo("Register(Type): 实例与键类型不匹配时抛异常");
        }

        /// <summary>
        /// 验证 Unregister 移除指定键，不影响其他键。
        /// </summary>
        [Test]
        public void Unregister_RemovesOnlyTargetKey()
        {
            var locator = new GenericLocator<IItem>();
            locator.Register(new ItemA());
            locator.Register(new ItemB());

            locator.Unregister<ItemA>();

            Assert.IsFalse(locator.IsRegistered<ItemA>(), "注销后目标键应被移除");
            Assert.IsTrue(locator.IsRegistered<ItemB>(), "其他键不受影响");
            AesirArchitectureDebug.LogTestInfo("Unregister: 仅移除目标键");
        }

        /// <summary>
        /// 验证 Clear 清空所有注册。
        /// </summary>
        [Test]
        public void Clear_RemovesAll()
        {
            var locator = new GenericLocator<IItem>();
            locator.Register(new ItemA());
            locator.Register(new ItemB());

            locator.Clear();

            var count = 0;
            foreach (var _ in locator.GetAll())
            {
                count++;
            }

            Assert.AreEqual(0, count, "清空后 GetAll 应为空集合");
            AesirArchitectureDebug.LogTestInfo("Clear: 清空所有注册");
        }

        /// <summary>
        /// 验证 GetByType 按 <see cref="System.Type" /> 查询的行为。
        /// </summary>
        [Test]
        public void GetByType_ReturnsRegisteredInstance()
        {
            var locator = new GenericLocator<IItem>();
            var a = new ItemA();

            locator.Register(a);

            Assert.AreSame(a, locator.GetByType(typeof(ItemA)));
            Assert.IsNull(locator.GetByType(typeof(ItemB)));
            AesirArchitectureDebug.LogTestInfo("GetByType: 按 Type 正确查询");
        }
    }
}
