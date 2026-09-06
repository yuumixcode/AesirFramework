using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    /// <summary>
    /// 验证 <see cref="AbstractContext{T}" /> 动态替换已注册 Model / Service 时的警告日志行为。
    /// </summary>
    /// <remarks>
    ///     <para>关键契约：</para>
    ///     <list type="bullet">
    ///         <item>首次注册（键未命中已有实例）不输出任何警告；</item>
    ///         <item>替换已注册实例时输出一条 Warning，提示旧实例上的事件订阅关系不会自动迁移；</item>
    ///         <item>警告仅提示、不影响执行——旧实例仍被 Dispose，新实例正常注册并初始化。</item>
    ///     </list>
    ///     <para>纯 C# 逻辑，EditMode 即可运行，无需 PlayMode。</para>
    /// </remarks>
    /// <seealso cref="AbstractContext{T}.RegisterModel{TModel}" />
    /// <seealso cref="AbstractContext{T}.RegisterService{TService}" />
    public class ContextReplacementWarningTests
    {
        /// <summary>
        /// 重置静态单例与计数器，确保测试间及同域重复运行间隔离
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            // EditMode 测试同一域内重复运行不触发域重载，需手动重置静态单例
            ResetStaticsAssistant.ResetForTests();
            CountingModel.DisposeCount = 0;
            CountingService.DisposeCount = 0;
        }

        /// <summary>
        /// 验证首次注册 Model / Service 时不输出动态替换警告。
        /// </summary>
        /// <remarks>
        /// 通过 <see cref="Application.logMessageReceived" /> 捕获测试期间全部 Warning，
        /// 断言其中不含动态替换提示（替换警告属误报——首次注册无旧实例可替换）。
        /// </remarks>
        [Test]
        public void Register_FirstRegistration_DoesNotLogReplacementWarning()
        {
            var warnings = new List<string>();
            Application.logMessageReceived += Capture;

            try
            {
                var context = ReplaceableContext.Instance;
                Assert.NotNull(context.GetModel<CountingModel>());
                Assert.NotNull(context.GetService<CountingService>());
            }
            finally
            {
                Application.logMessageReceived -= Capture;
            }

            Assert.IsEmpty(warnings, "首次注册不应输出动态替换警告");
            AesirArchitectureDebug.LogTestInfo("Register(首次注册): Model/Service 均无动态替换警告");

            void Capture(string condition, string stackTrace, LogType type)
            {
                if (type == LogType.Warning && condition.Contains("动态替换"))
                {
                    warnings.Add(condition);
                }
            }
        }

        /// <summary>
        /// 验证替换已注册的 Model 时输出警告，且替换行为不变（旧实例 Dispose、新实例初始化）。
        /// </summary>
        [Test]
        public void RegisterModel_ReplaceExisting_LogsWarningAndKeepsBehavior()
        {
            var context = ReplaceableContext.Instance;

            LogAssert.Expect(LogType.Warning, new Regex("动态替换 Model"));
            context.RegisterModel(new CountingModel());

            Assert.AreEqual(1, CountingModel.DisposeCount, "旧 Model 实例应被 Dispose");
            Assert.NotNull(context.GetModel<CountingModel>(), "替换后应能正常获取新实例");
            AesirArchitectureDebug.LogTestInfo("RegisterModel(动态替换): 输出警告，旧实例 Dispose，新实例正常注册");
        }

        /// <summary>
        /// 验证替换已注册的 Service 时输出警告，且替换行为不变（旧实例 Dispose、新实例初始化）。
        /// </summary>
        [Test]
        public void RegisterService_ReplaceExisting_LogsWarningAndKeepsBehavior()
        {
            var context = ReplaceableContext.Instance;

            LogAssert.Expect(LogType.Warning, new Regex("动态替换 Service"));
            context.RegisterService(new CountingService());

            Assert.AreEqual(1, CountingService.DisposeCount, "旧 Service 实例应被 Dispose");
            Assert.NotNull(context.GetService<CountingService>(), "替换后应能正常获取新实例");
            AesirArchitectureDebug.LogTestInfo("RegisterService(动态替换): 输出警告，旧实例 Dispose，新实例正常注册");
        }

        /// <summary>
        /// 统计 Dispose 次数的 Model，用于验证替换时旧实例被正确释放
        /// </summary>
        class CountingModel : AbstractModel
        {
            public static int DisposeCount;

            protected override void OnDispose() => DisposeCount++;
        }

        /// <summary>
        /// 统计 Dispose 次数的 Service，用于验证替换时旧实例被正确释放
        /// </summary>
        class CountingService : AbstractService
        {
            public static int DisposeCount;

            protected override void OnDispose() => DisposeCount++;
        }

        /// <summary>
        /// 预注册一个 Model 和一个 Service 的测试上下文，用于触发替换路径
        /// </summary>
        [InternalContext]
        class ReplaceableContext : AbstractContext<ReplaceableContext>
        {
            protected override void Configure()
            {
                RegisterModel(new CountingModel());
                RegisterService(new CountingService());
            }
        }
    }
}
