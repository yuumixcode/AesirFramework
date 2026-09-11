using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    /// <summary>
    /// 验证 <see cref="AbstractContext{T}" /> 的 <c>UnregisterModel</c> / <c>UnregisterService</c>：
    /// 摘除后 Get 抛未注册异常、被摘除实例被 Dispose（含 Initialized 重置）、未注册幂等、
    /// 注销后再注册按新插入语义追加到注册顺序末尾。
    /// </summary>
    /// <remarks>
    /// 摘除语义与动态替换对齐：被摘除实例经 Dispose 释放（初始化状态随之复位），
    /// 其上的订阅不会迁移；未注册时静默无操作（幂等）。
    /// <para>纯 C# 逻辑，EditMode 即可运行，无需 PlayMode。</para>
    /// </remarks>
    /// <seealso cref="AbstractContext{T}" />
    public class AbstractContextUnregisterTests
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
            CountingService.InitializeCount = 0;
            CountingService.DisposeCount = 0;
        }

        /// <summary>
        /// 验证注销已注册的 Model：实例被 Dispose（Initialized 复位），摘除后 Get 抛未注册异常。
        /// </summary>
        [Test]
        public void UnregisterModel_Registered_DisposesInstanceAndRemovesFromGet()
        {
            var context = UnregisterContext.Instance;
            var model = context.GetModel<CountingModel>();
            Assert.IsTrue(model.Initialized, "前置：注册并初始化后应为已初始化状态");

            context.UnregisterModel<CountingModel>();

            Assert.AreEqual(1, CountingModel.DisposeCount, "被摘除实例应被 Dispose 恰好 1 次");
            Assert.IsFalse(model.Initialized, "Dispose 后 Initialized 应重置为 false");
            var ex = Assert.Throws<InvalidOperationException>(() => context.GetModel<CountingModel>(),
                "摘除后 GetModel 应抛未注册异常");
            StringAssert.Contains("CountingModel", ex.Message, "异常消息应包含目标类型名");
            AesirArchitectureDebug.LogTestInfo("UnregisterModel: 摘除即释放 + Initialized 复位 + Get 抛未注册");
        }

        /// <summary>
        /// 验证注销未注册的 Model / Service：静默无操作（幂等），不抛异常。
        /// </summary>
        [Test]
        public void Unregister_NotRegistered_IsSilentNoOp()
        {
            var context = UnregisterContext.Instance;

            Assert.DoesNotThrow(() => context.UnregisterModel<NeverRegisteredModel>(), "未注册类型注销应为幂等无操作");
            Assert.DoesNotThrow(() => context.UnregisterService<NeverRegisteredService>(), "未注册类型注销应为幂等无操作");
            Assert.AreEqual(0, CountingModel.DisposeCount, "幂等无操作不应触发任何 Dispose");
            Assert.IsTrue(context.Initialized, "无操作不应影响上下文状态");
            AesirArchitectureDebug.LogTestInfo("Unregister: 未注册类型静默无操作（幂等）");
        }

        /// <summary>
        /// 验证注销后再次注册：按新插入语义追加到注册顺序末尾（其余模块相对顺序不变）。
        /// </summary>
        [Test]
        public void UnregisterModel_ThenReRegister_AppendsToEndOfRegistrationOrder()
        {
            var context = UnregisterContext.Instance;
            context.UnregisterModel<CountingModel>();
            context.RegisterModel(new CountingModel());

            var order = new List<Type>();
            foreach (var m in context.GetAllModels())
            {
                order.Add(m.GetType());
            }

            // 初始注册顺序 CountingModel → OtherModel；摘除 CountingModel 后再注册追加到末尾
            CollectionAssert.AreEqual(new[] { typeof(OtherModel), typeof(CountingModel) }, order,
                "注销后再注册应追加到注册顺序末尾，OtherModel 相对顺序保持不变");
            AesirArchitectureDebug.LogTestInfo("UnregisterModel: 再注册按新插入语义追加到末尾");
        }

        /// <summary>
        /// 验证注销已注册的 Service：实例被 Dispose，摘除后 Get 抛未注册异常，其余模块不受影响。
        /// </summary>
        [Test]
        public void UnregisterService_Registered_DisposesInstanceAndOthersUnaffected()
        {
            var context = UnregisterContext.Instance;

            context.UnregisterService<CountingService>();

            Assert.AreEqual(1, CountingService.DisposeCount, "被摘除的 Service 应被 Dispose 恰好 1 次");
            Assert.Throws<InvalidOperationException>(() => context.GetService<CountingService>(),
                "摘除后 GetService 应抛未注册异常");
            Assert.DoesNotThrow(() => _ = context.GetModel<CountingModel>(), "注销 Service 不应影响其余模块的注册与获取");
            AesirArchitectureDebug.LogTestInfo("UnregisterService: 摘除即释放 + 其余模块不受影响");
        }

        /// <summary>
        /// 验证注销在 <c>Configure</c> 阶段注册的类型后，上下文整体 Dispose 不再释放已摘除实例。
        /// </summary>
        [Test]
        public void UnregisterModel_RemovedFromContextDispose()
        {
            var context = UnregisterContext.Instance;
            context.UnregisterModel<CountingModel>();
            CountingModel.DisposeCount = 0;

            context.Dispose();

            Assert.AreEqual(0, CountingModel.DisposeCount, "已摘除的实例不应在上下文 Dispose 时被二次释放");
            AesirArchitectureDebug.LogTestInfo("UnregisterModel: 上下文 Dispose 不二次释放已摘除实例");
        }

        #region 测试夹具

        [InternalContext]
        class UnregisterContext : AbstractContext<UnregisterContext>
        {
            protected override void Configure()
            {
                RegisterModel(new CountingModel());
                RegisterModel(new OtherModel());
                RegisterService(new CountingService());
            }
        }

        class CountingModel : AbstractModel
        {
            public static int InitializeCount;
            public static int DisposeCount;

            protected override void OnInitialize() => InitializeCount++;

            protected override void OnDispose() => DisposeCount++;
        }

        class OtherModel : AbstractModel { }

        class NeverRegisteredModel : AbstractModel { }

        class CountingService : AbstractService
        {
            public static int InitializeCount;
            public static int DisposeCount;

            protected override void OnInitialize() => InitializeCount++;

            protected override void OnDispose() => DisposeCount++;
        }

        class NeverRegisteredService : AbstractService { }

        #endregion
    }
}
