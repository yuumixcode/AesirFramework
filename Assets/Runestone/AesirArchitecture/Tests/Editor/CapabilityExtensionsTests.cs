using System;
using NUnit.Framework;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    /// <summary>
    /// 验证 <see cref="CapabilityExtensions" /> 的命令 / 查询执行链与能力注入契约——
    /// 框架核心卖点 CQRS 闭环（Context 注册 → 初始化 → Command 写 / Query 读）的执行链路测试。
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     覆盖：带参与无参 <c>ExecuteCommand</c> / <c>ExecuteQuery</c> 的实例创建与上下文注入、
    ///     <see cref="IController{T}" /> 默认接口实现的 Context 自动绑定、命令链与查询组合能力
    ///     （<see cref="ICommand" /> / <see cref="IQuery{TResult}" /> 的交叉执行能力）、
    ///     Model 初始化阶段获取 Service 的两阶段约束异常语义，以及 Dispose 后单例重建。
    ///     </para>
    ///     <para>纯 C# 逻辑，EditMode 即可运行。</para>
    /// </remarks>
    /// <seealso cref="AbstractCommand" />
    /// <seealso cref="AbstractQuery{TResult}" />
    /// <seealso cref="AbstractContext{T}.Dispose" />
    public class CapabilityExtensionsTests
    {
        [SetUp]
        public void SetUp()
        {
            ResetStaticsAssistant.ResetForTests();
        }

        /// <summary>
        /// 验证带参命令：命令实例接收上下文注入并经 Model 写方法修改状态。
        /// </summary>
        [Test]
        public void ExecuteCommand_WithInstance_WritesModelThroughContext()
        {
            var controller = new TestController();
            controller.ExecuteCommand(new AddCountCommand { Delta = 5 });

            Assert.AreEqual(5, controller.GetModel<ICounterModel>().Count,
                "命令应经上下文获取 Model 并完成写入");
            AesirArchitectureDebug.LogTestInfo("ExecuteCommand(带参): 上下文注入 + Model 写入");
        }

        /// <summary>
        /// 验证无参命令：框架经 <c>new()</c> 创建命令实例并执行。
        /// </summary>
        [Test]
        public void ExecuteCommand_Parameterless_CreatesInstanceAndExecutes()
        {
            var controller = new TestController();
            controller.ExecuteCommand<IncrementCommand>();
            controller.ExecuteCommand<IncrementCommand>();

            Assert.AreEqual(2, controller.GetModel<ICounterModel>().Count,
                "两次无参命令各 +1");
            AesirArchitectureDebug.LogTestInfo("ExecuteCommand(无参): new() 实例化并执行");
        }

        /// <summary>
        /// 验证无参查询：读取 Model 状态并返回。
        /// </summary>
        [Test]
        public void ExecuteQuery_Parameterless_ReturnsModelDerivedResult()
        {
            var controller = new TestController();
            controller.ExecuteCommand(new AddCountCommand { Delta = 21 });

            Assert.AreEqual(21, controller.ExecuteQuery<GetCountQuery, int>());
            AesirArchitectureDebug.LogTestInfo("ExecuteQuery(无参): 返回 Model 派生结果");
        }

        /// <summary>
        /// 验证带参查询：查询实例的字段参与计算并返回。
        /// </summary>
        [Test]
        public void ExecuteQuery_WithInstance_PassesThroughParameter()
        {
            var controller = new TestController();
            controller.ExecuteCommand(new AddCountCommand { Delta = 3 });

            Assert.AreEqual(30, controller.ExecuteQuery(new MultipliedCountQuery { Multiplier = 10 }),
                "查询实例字段应参与结果计算");
            AesirArchitectureDebug.LogTestInfo("ExecuteQuery(带参): 实例字段参与计算");
        }

        /// <summary>
        /// 验证命令链：<see cref="ICommand" /> 继承 <see cref="ICanExecuteCommand" />，
        /// 命令内部可再发布其他命令。
        /// </summary>
        [Test]
        public void ExecuteCommand_CanChainAnotherCommand()
        {
            var controller = new TestController();
            controller.ExecuteCommand<ChainCommand>();

            Assert.AreEqual(11, controller.GetModel<ICounterModel>().Count,
                "ChainCommand 自身 +10 并链式执行 IncrementCommand +1");
            AesirArchitectureDebug.LogTestInfo("ExecuteCommand(命令链): 命令内再发命令");
        }

        /// <summary>
        /// 验证查询组合：<see cref="IQuery{TResult}" /> 继承 <see cref="ICanExecuteQuery" />，
        /// 查询内部可组合其他查询。
        /// </summary>
        [Test]
        public void ExecuteQuery_CanComposeAnotherQuery()
        {
            var controller = new TestController();
            controller.ExecuteCommand(new AddCountCommand { Delta = 7 });

            Assert.AreEqual(107, controller.ExecuteQuery<ComposedQuery, int>(),
                "ComposedQuery 应组合 GetCountQuery（7）并 +100");
            AesirArchitectureDebug.LogTestInfo("ExecuteQuery(查询组合): 查询内组合其他查询");
        }

        /// <summary>
        /// 验证 <see cref="IController{T}" /> 默认接口实现：Controller 实例无需任何代码即持有上下文单例。
        /// </summary>
        [Test]
        public void Controller_BindsContextSingletonViaDefaultInterfaceMember()
        {
            var controller = new TestController();
            IContextHolder holder = controller;

            Assert.AreSame(CqrsContext.Instance, holder.Context,
                "IController<T> 的 DIM 应把 Context 绑定到 AbstractContext<T>.Instance");
            AesirArchitectureDebug.LogTestInfo("IController<T> DIM: Context 自动绑定到单例");
        }

        /// <summary>
        /// 验证 Model 初始化阶段获取 Service 抛出的异常包含两阶段初始化提示：
        /// "先全部 Model、后全部 Service"，并指引延迟到运行期获取。
        /// </summary>
        /// <remarks>
        /// 注意：<see cref="IModel" /> 本身不继承 <see cref="ICanGetService" />——标准 Model 在
        /// 编译期就拿不到 GetService 能力（见 <see cref="StandardModel_DoesNotExposeGetServiceCapability" />）；
        /// 本用例使用显式声明了 <see cref="ICanGetService" /> 的非标准 Model 触发运行期防线。
        /// </remarks>
        [Test]
        public void ModelWithServiceCapability_DuringInit_ThrowsWithTwoPhaseHint()
        {
            var ex = Assert.Throws<InvalidOperationException>(() => _ = HungryContext.Instance);

            StringAssert.Contains("先全部 Model", ex.Message, "应提示两阶段初始化约束");
            StringAssert.Contains("延迟到运行期", ex.Message, "应指引延迟获取的修复方式");
            AesirArchitectureDebug.LogTestInfo("Model 初始化期 GetService: 异常含两阶段约束提示");
        }

        /// <summary>
        /// 验证标准 Model 的能力面：不暴露 <see cref="ICanGetService" />——
        /// "Model 不依赖 Service"在类型系统上由编译期阻断。
        /// </summary>
        [Test]
        public void StandardModel_DoesNotExposeGetServiceCapability()
        {
            var model = new CounterModel();

            Assert.IsFalse(model is ICanGetService,
                "IModel 不继承 ICanGetService——Model→Service 依赖由编译期阻断");
            AesirArchitectureDebug.LogTestInfo("标准 Model 能力面: 无 GetService（编译期阻断）");
        }

        /// <summary>
        /// 验证 Dispose 后单例缓存解除：再次访问 <see cref="AbstractContext{T}.Instance" />
        /// 重建并重新初始化全新上下文，而非返回已释放的空壳实例。
        /// </summary>
        [Test]
        public void Dispose_ThenAccessInstance_RebuildsFreshContext()
        {
            var controller = new TestController();
            controller.ExecuteCommand(new AddCountCommand { Delta = 5 });

            var first = CqrsContext.Instance;
            first.Dispose();
            Assert.IsFalse(first.Initialized, "释放后原实例应处于未初始化态");

            var second = CqrsContext.Instance;
            Assert.AreNotSame(first, second, "Dispose 后 Instance 应重建新实例，而非返回僵尸上下文");
            Assert.IsTrue(second.Initialized, "重建的上下文应完成初始化");
            Assert.AreEqual(0, second.GetModel<ICounterModel>().Count,
                "重建后的 Model 应为全新状态（计数归零）");
            AesirArchitectureDebug.LogTestInfo("Dispose 后 Instance: 重建全新上下文");
        }

        // ──────────────────────────── 测试用具 ────────────────────────────

        interface ICounterModel : IModel
        {
            int Count { get; }

            void Add(int delta);
        }

        class CounterModel : AbstractModel, ICounterModel
        {
            public int Count { get; private set; }

            public void Add(int delta) => Count += delta;
        }

        class AddCountCommand : AbstractCommand
        {
            public int Delta = 1;

            protected override void OnExecute() => this.GetModel<ICounterModel>().Add(Delta);
        }

        class IncrementCommand : AbstractCommand
        {
            protected override void OnExecute() => this.GetModel<ICounterModel>().Add(1);
        }

        class ChainCommand : AbstractCommand
        {
            protected override void OnExecute()
            {
                this.GetModel<ICounterModel>().Add(10);
                this.ExecuteCommand<IncrementCommand>();
            }
        }

        class GetCountQuery : AbstractQuery<int>
        {
            protected override int OnExecute() => this.GetModel<ICounterModel>().Count;
        }

        class MultipliedCountQuery : AbstractQuery<int>
        {
            public int Multiplier = 2;

            protected override int OnExecute() => this.GetModel<ICounterModel>().Count * Multiplier;
        }

        class ComposedQuery : AbstractQuery<int>
        {
            protected override int OnExecute() => this.ExecuteQuery<GetCountQuery, int>() + 100;
        }

        class TestController : IController<CqrsContext> { }

        class NeverInitializedService : AbstractService { }

        class ServiceHungryModel : AbstractModel, ICanGetService
        {
            protected override void OnInitialize() => this.GetService<NeverInitializedService>();
        }

        [InternalContext]
        class CqrsContext : AbstractContext<CqrsContext>
        {
            protected override void Configure()
            {
                RegisterModel<ICounterModel>(new CounterModel());
            }
        }

        [InternalContext]
        class HungryContext : AbstractContext<HungryContext>
        {
            protected override void Configure()
            {
                RegisterModel(new ServiceHungryModel());
                RegisterService(new NeverInitializedService());
            }
        }
    }
}
