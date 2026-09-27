using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Runestone.AesirArchitecture.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    /// <summary>
    /// <see cref="AesirUpdateController" /> 忙碌标记簿记与域重载兜底收尾（
    /// <c>RecoverFromInterruptedRun</c>）的 EditMode 测试。
    /// </summary>
    /// <remarks>
    /// <para>
    /// 「AesirUpdater.Busy」是控制器内的私有 SessionState 键，本类以字面量锁定键名——
    /// 控制器侧一旦改名，兜底判断与标记写入会静默失配，本测试据字面量即刻变红。
    /// </para>
    /// <para>
    /// 更新执行的完整链路涉及网络下载与 unitypackage 导入，不在单测范围；
    /// 收尾阶段「先解锁再清标记」的顺序与异常路径下的锁配平由代码结构保证，
    /// 另经编辑器内注毒探针验证（收尾异常时 UnlockReloadAssemblies 仍执行、标记时序正确）。
    /// </para>
    /// </remarks>
    public class AesirUpdateControllerTests
    {
        /// <summary>忙碌标记的 SessionState 键（与控制器内私有常量字面量对齐）。</summary>
        const string BusySessionKey = "AesirUpdater.Busy";

        AesirUpdateController.UpdateState _state;

        AesirUpdateController _controller;

        [SetUp]
        public void SetUp()
        {
            _state = new AesirUpdateController.UpdateState();
            _controller = new AesirUpdateController(_state, "测试", null, null);
            SessionState.SetBool(BusySessionKey, false);
        }

        [TearDown]
        public void TearDown()
        {
            SessionState.SetBool(BusySessionKey, false);
        }

        [Test]
        public void BeginBusy_SetsBusyFlag_ReentryRejected_EndBusyClears()
        {
            Assert.IsTrue(InvokeBeginBusy());
            Assert.IsTrue(_state.Busy);
            Assert.IsTrue(SessionState.GetBool(BusySessionKey, false), "进入忙碌应写入 SessionState 标记");

            Assert.IsFalse(InvokeBeginBusy(), "忙碌中重复进入应被拒绝");

            InvokeEndBusy();
            Assert.IsFalse(_state.Busy);
            Assert.IsFalse(SessionState.GetBool(BusySessionKey, false), "收尾应清除 SessionState 标记");
        }

        [Test]
        public void RecoverFromInterruptedRun_WhenBusy_ClearsFlag_Unlocks_AndWarns()
        {
            // 先于加锁解析方法：方法缺失时断言失败，不会走到下面的 Lock，避免测试自身泄漏重载锁
            var method = GetRecoverMethod();
            SessionState.SetBool(BusySessionKey, true);

            // 先持一把锁，与兜底收尾的 Unlock 配平——测试会话不留下未配对的解锁
            EditorApplication.LockReloadAssemblies();
            LogAssert.Expect(LogType.Warning, new Regex(
                "上一次更新/检测流程未正常收尾（域重载或编辑器中断），已清理残留进度条与程序集重载锁"));
            method.Invoke(null, null);

            Assert.IsFalse(SessionState.GetBool(BusySessionKey, false), "兜底收尾后忙碌标记应被清除");
        }

        [Test]
        public void RecoverFromInterruptedRun_WhenNotBusy_IsNoOp()
        {
            SessionState.SetBool(BusySessionKey, false);
            GetRecoverMethod().Invoke(null, null);
            Assert.IsFalse(SessionState.GetBool(BusySessionKey, false));
            LogAssert.NoUnexpectedReceived();
        }

        bool InvokeBeginBusy()
        {
            return (bool)InvokePrivate("BeginBusy");
        }

        void InvokeEndBusy()
        {
            InvokePrivate("EndBusy");
        }

        object InvokePrivate(string methodName)
        {
            var method = typeof(AesirUpdateController).GetMethod(methodName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(method, $"未找到私有方法 {methodName}——控制器重构后请同步更新本测试");
            return method.Invoke(_controller, null);
        }

        static MethodInfo GetRecoverMethod()
        {
            var method = typeof(AesirUpdateController).GetMethod("RecoverFromInterruptedRun",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(method, "未找到 RecoverFromInterruptedRun——控制器重构后请同步更新本测试");
            return method;
        }
    }
}
