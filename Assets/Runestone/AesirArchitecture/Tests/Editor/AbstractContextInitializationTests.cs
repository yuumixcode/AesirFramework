using System;
using NUnit.Framework;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    /// <summary>
    /// 验证 <see cref="AbstractContext{T}" /> 初始化失败不缓存单例、成功时的单例缓存行为，
    /// 以及 <see cref="IContext.GetModel{T}" /> / <see cref="IContext.GetService{T}" /> 的未注册校验。
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="AbstractContext{T}.Instance" /> 的关键契约：初始化失败不得缓存单例——
    /// 否则后续所有访问都拿到 <c>Initialized == false</c> 的坏上下文，且根因异常只在首次访问抛出一次，
    /// 报错点与根因分离，排查体验极差。
    /// </para>
    /// <para>
    /// 初始化失败不做回滚 Dispose（极简约定）：初始化失败属启动期编程错误，应修复根因；
    /// 已初始化到一半的模块随被丢弃的实例交由 GC 回收。
    /// </para>
    /// <para>
    /// GetModel / GetService 的关键契约：未注册时抛出 <see cref="InvalidOperationException" />
    /// 而非返回 null——返回 null 会延迟到使用点爆发 NRE，且报错点与根因分离。
    /// </para>
    /// <para>纯 C# 逻辑，EditMode 即可运行，无需 PlayMode。</para>
    /// </remarks>
    /// <seealso cref="AbstractContext{T}.Instance"/>
    /// <seealso cref="AbstractContext{T}.Initialize"/>
    public class AbstractContextInitializationTests
    {
        /// <summary>
        /// 重置静态单例与计数器，确保测试间及同域重复运行间隔离
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            // EditMode 测试同一域内重复运行不触发域重载，需手动重置静态单例
            ResetStaticsAssistant.ResetForTests();
            CountingModel.InitializeCount = 0;
            CountingModel.DisposeCount = 0;
            HealthyContext.ConfigureCount = 0;
        }

        /// <summary>
        /// 验证初始化失败时不缓存单例：每次访问都重新创建并重新初始化，根因异常每次抛出。
        /// </summary>
        /// <remarks>
        /// <c>ThrowingModelContext</c> 注册顺序为 CountingModel（正常）→ ThrowingModel（初始化抛异常）：
        /// <list type="number">
        /// <item>两次访问均抛出 <see cref="InvalidOperationException"/>（根因异常每次都抛出，而非只抛一次）；</item>
        /// <item>CountingModel 每次访问被初始化 1 次，共 2 次（单例未缓存，每次都重新创建）；</item>
        /// <item>失败路径不做回滚 Dispose，Dispose 计数为 0。</item>
        /// </list>
        /// </remarks>
        [Test]
        public void Interface_InitializeThrows_DoesNotCacheSingleton()
        {
            Assert.Throws<InvalidOperationException>(() => _ = ThrowingModelContext.Instance,
                "首次访问应抛出初始化的根因异常");
            Assert.Throws<InvalidOperationException>(() => _ = ThrowingModelContext.Instance,
                "第二次访问应再次抛出同一根因异常（未缓存半成品单例）");

            Assert.AreEqual(2, CountingModel.InitializeCount,
                "每次访问都重新创建上下文并初始化 CountingModel，共 2 次");
            Assert.AreEqual(0, CountingModel.DisposeCount,
                "失败路径不做回滚 Dispose，半成品模块随实例交由 GC 回收");
            AesirArchitectureDebug.LogTestInfo(
                "Interface(初始化失败): 不缓存单例，根因异常每次抛出，无回滚");
        }

        /// <summary>
        /// 验证初始化成功时的正常单例行为：缓存实例，重复访问不重复初始化。
        /// </summary>
        [Test]
        public void Interface_InitializeSucceeds_CachesSingletonAndInitializesOnce()
        {
            var first = HealthyContext.Instance;

            Assert.NotNull(first);
            Assert.IsTrue(first.Initialized, "首次访问后上下文应为已初始化状态");

            var second = HealthyContext.Instance;
            Assert.AreSame(first, second, "重复访问应返回同一单例实例");

            Assert.AreEqual(1, HealthyContext.ConfigureCount, "Configure 只应执行 1 次");
            Assert.AreEqual(1, CountingModel.InitializeCount, "模块只应初始化 1 次");
            Assert.AreEqual(0, CountingModel.DisposeCount, "成功路径不应触发 Dispose");
            AesirArchitectureDebug.LogTestInfo("Interface(初始化成功): 单例正确缓存，初始化只执行一次");
        }

        /// <summary>
        /// 验证 GetModel 在目标 Model 未注册时抛出包含类型名与修复提示的 <see cref="InvalidOperationException" />。
        /// </summary>
        /// <remarks>
        /// 锁定"接口方法自身即抛清晰异常"的防护语义，消除 Command/Query 直接调用
        /// <c>Context.GetModel</c> 绕过扩展方法防护的路径。
        /// </remarks>
        [Test]
        public void GetModel_NotRegistered_ThrowsInvalidOperationException()
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => EmptyContext.Instance.GetModel<CountingModel>());

            StringAssert.Contains("CountingModel", ex.Message, "异常消息应包含目标类型名");
            StringAssert.Contains("RegisterModel", ex.Message, "异常消息应包含修复提示");
            AesirArchitectureDebug.LogTestInfo("GetModel(未注册): 抛出含类型名与修复提示的清晰异常");
        }

        /// <summary>
        /// 验证 GetService 在目标 Service 未注册时抛出包含类型名与修复提示的 <see cref="InvalidOperationException" />。
        /// </summary>
        [Test]
        public void GetService_NotRegistered_ThrowsInvalidOperationException()
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => EmptyContext.Instance.GetService<CountingService>());

            StringAssert.Contains("CountingService", ex.Message, "异常消息应包含目标类型名");
            StringAssert.Contains("RegisterService", ex.Message, "异常消息应包含修复提示");
            AesirArchitectureDebug.LogTestInfo("GetService(未注册): 抛出含类型名与修复提示的清晰异常");
        }

        /// <summary>
        /// 正常 Model，统计初始化与释放次数
        /// </summary>
        class CountingModel : AbstractModel
        {
            public static int InitializeCount;
            public static int DisposeCount;

            protected override void OnInitialize() => InitializeCount++;
            protected override void OnDispose() => DisposeCount++;
        }

        /// <summary>
        /// 正常 Service，用于 GetService 未注册校验测试
        /// </summary>
        class CountingService : AbstractService
        {
        }

        /// <summary>
        /// 初始化必然抛异常的 Model，用于触发初始化失败路径
        /// </summary>
        class ThrowingModel : AbstractModel
        {
            protected override void OnInitialize() =>
                throw new InvalidOperationException("模拟模块初始化失败");
        }

        /// <summary>
        /// 初始化失败的测试上下文：先注册正常的 CountingModel，再注册抛异常的 ThrowingModel
        /// </summary>
        class ThrowingModelContext : AbstractContext<ThrowingModelContext>
        {
            protected override void Configure()
            {
                RegisterModel<CountingModel>(new CountingModel());
                RegisterModel<ThrowingModel>(new ThrowingModel());
            }
        }

        /// <summary>
        /// 初始化成功的测试上下文（对照组）
        /// </summary>
        class HealthyContext : AbstractContext<HealthyContext>
        {
            public static int ConfigureCount;

            protected override void Configure()
            {
                ConfigureCount++;
                RegisterModel<CountingModel>(new CountingModel());
            }
        }

        /// <summary>
        /// 不注册任何模块的测试上下文，用于未注册校验测试
        /// </summary>
        class EmptyContext : AbstractContext<EmptyContext>
        {
            protected override void Configure() { }
        }
    }
}
