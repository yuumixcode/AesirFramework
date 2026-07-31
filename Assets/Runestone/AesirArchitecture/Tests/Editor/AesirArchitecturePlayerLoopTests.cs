using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace Runestone.AesirArchitecture.Tests.Editor
{
    /// <summary>
    /// 验证 AesirArchitecturePlayerLoop 与 PlayerLoopUtility 在自定义系统插入和生命周期回调方面的正确性测试类
    /// </summary>
    public class AesirArchitecturePlayerLoopTests
    {
        PlayerLoopSystem _originalLoop;

        [SetUp]
        public void SetUp()
        {
            _originalLoop = PlayerLoop.GetCurrentPlayerLoop();
            AesirArchitecturePlayerLoop.Reset();
        }

        [TearDown]
        public void TearDown()
        {
            PlayerLoop.SetPlayerLoop(_originalLoop);
            AesirArchitecturePlayerLoop.Reset();
        }

        /// <summary>
        /// 验证 InsertSystemBefore 将自定义系统插入到目标系统之前
        /// </summary>
        [Test]
        public void InsertSystemBefore_TargetExists_InsertsBefore()
        {
            var markerType = typeof(TestMarkerBeforeUpdate);
            var inserted = PlayerLoopUtility.InsertSystemBefore<Update>(
                new PlayerLoopSystem { type = markerType });
            Assert.IsTrue(inserted);
            AssertIsBeforeInLoop(markerType, typeof(Update));
            AesirArchitectureDebug.LogTestInfo("InsertSystemBefore: 成功在 Update 之前插入了自定义系统");
        }

        /// <summary>
        /// 验证 InsertSystemAfter 将自定义系统插入到目标系统之后
        /// </summary>
        [Test]
        public void InsertSystemAfter_TargetExists_InsertsAfter()
        {
            var markerType = typeof(TestMarkerAfterFixedUpdate);
            var inserted = PlayerLoopUtility.InsertSystemAfter<FixedUpdate>(
                new PlayerLoopSystem { type = markerType });

            Assert.IsTrue(inserted);
            AssertIsAfterInLoop(markerType, typeof(FixedUpdate));
            AesirArchitectureDebug.LogTestInfo("InsertSystemAfter: 成功在 FixedUpdate 之后插入了自定义系统");
        }

        /// <summary>
        /// 验证对同一目标系统连续两次 InsertSystemBefore 时，两次插入均生效
        /// </summary>
        [Test]
        public void InsertSystemBefore_SameTargetTwice_InsertsTwoSystems()
        {
            var marker1 = typeof(TestMarkerTwice1);
            var marker2 = typeof(TestMarkerTwice2);

            PlayerLoopUtility.InsertSystemBefore<Update>(new PlayerLoopSystem { type = marker1 });
            PlayerLoopUtility.InsertSystemBefore<Update>(new PlayerLoopSystem { type = marker2 });

            var loop = PlayerLoop.GetCurrentPlayerLoop();
            Assert.IsTrue(ContainsType(ref loop, marker1), "First marker should exist");
            Assert.IsTrue(ContainsType(ref loop, marker2), "Second marker should exist");
            AesirArchitectureDebug.LogTestInfo("InsertSystemBefore(连续两次): 成功在 Update 中插入了两个自定义系统");
        }

        /// <summary>
        /// 验证 GetCurrentPlayerLoopDescription 在默认 PlayerLoop 下包含核心系统名称
        /// </summary>
        [Test]
        public void GetCurrentPlayerLoopDescription_DefaultLoop_ContainsCoreSystems()
        {
            var dump = PlayerLoopUtility.GetCurrentPlayerLoopDescription();

            Assert.IsTrue(dump.Contains("Update"), "Should contain Update");
            Assert.IsTrue(dump.Contains("FixedUpdate"), "Should contain FixedUpdate");
            Assert.IsTrue(dump.Contains("PostLateUpdate"), "Should contain PostLateUpdate");
            AesirArchitectureDebug.LogTestInfo(
                "GetCurrentPlayerLoopDescription(默认循环): 输出中包含 Update、FixedUpdate、PostLateUpdate 等核心系统");
        }

        /// <summary>
        /// 验证 GetCurrentPlayerLoopDescription 在插入自定义系统后能输出该系统名称
        /// </summary>
        [Test]
        public void GetCurrentPlayerLoopDescription_AfterInsert_ShowsInsertedSystem()
        {
            var markerType = typeof(TestMarkerForDump);
            PlayerLoopUtility.InsertSystemBefore<Update>(new PlayerLoopSystem { type = markerType });

            var dump = PlayerLoopUtility.GetCurrentPlayerLoopDescription();

            Assert.IsTrue(dump.Contains(markerType.Name),
                $"Dump should contain inserted system '{markerType.Name}'");
            AesirArchitectureDebug.LogTestInfo(
                $"GetCurrentPlayerLoopDescription(插入后): 输出中包含插入的系统 '{markerType.Name}'");
        }

        /// <summary>
        /// 验证 BeforeUpdate 和 AfterUpdate 两个阶段的注册互不干扰
        /// </summary>
        [Test]
        public void Register_BeforeAndAfterUpdate_AreDistinct()
        {
            AesirArchitecturePlayerLoop.Reset();

            AesirArchitecturePlayerLoop.Register(AesirArchitectureLifeCyclePhase.BeforeUpdate, () => { });
            AesirArchitecturePlayerLoop.Register(AesirArchitectureLifeCyclePhase.AfterUpdate, () => { });

            Assert.AreEqual(1,
                AesirArchitecturePlayerLoop.GetHookCount(AesirArchitectureLifeCyclePhase.BeforeUpdate));
            Assert.AreEqual(1,
                AesirArchitecturePlayerLoop.GetHookCount(AesirArchitectureLifeCyclePhase.AfterUpdate));
            AesirArchitectureDebug.LogTestInfo("Register(BeforeUpdate/AfterUpdate): 两个阶段注册互不干扰，各自计数为 1");
        }

        /// <summary>
        /// 验证 Clear 清除之前注册的所有回调
        /// </summary>
        [Test]
        public void Clear_RemovesAllRegisteredHooks()
        {
            AesirArchitecturePlayerLoop.Register(AesirArchitectureLifeCyclePhase.BeforeUpdate, () => { });
            Assert.AreEqual(1,
                AesirArchitecturePlayerLoop.GetHookCount(AesirArchitectureLifeCyclePhase.BeforeUpdate));

            AesirArchitecturePlayerLoop.Reset();

            Assert.AreEqual(0,
                AesirArchitecturePlayerLoop.GetHookCount(AesirArchitectureLifeCyclePhase.BeforeUpdate));
            AesirArchitectureDebug.LogTestInfo("Clear: 成功清除之前注册的所有回调");
        }

        /// <summary>
        /// 验证相同 order 的回调按注册顺序执行，不同 order 按 order 升序排列
        /// </summary>
        [Test]
        public void Register_SameOrder_ExecutesInRegistrationOrder()
        {
            var executionOrder = new List<int>();
            AesirArchitecturePlayerLoop.Register(AesirArchitectureLifeCyclePhase.BeforeUpdate,
                () => executionOrder.Add(1));
            AesirArchitecturePlayerLoop.Register(AesirArchitectureLifeCyclePhase.BeforeUpdate,
                () => executionOrder.Add(2));
            AesirArchitecturePlayerLoop.Register(AesirArchitectureLifeCyclePhase.BeforeUpdate,
                () => executionOrder.Add(3), -1);
            AesirArchitecturePlayerLoop.Register(AesirArchitectureLifeCyclePhase.BeforeUpdate,
                () => executionOrder.Add(4), 1);
            AesirArchitecturePlayerLoop.Register(AesirArchitectureLifeCyclePhase.BeforeUpdate,
                () => executionOrder.Add(5));

            // 直接调用 internal 方法（通过 InternalsVisibleTo），替代反射
            AesirArchitecturePlayerLoop.OnBeforeUpdate();

            // 预期顺序: Order -1 → Order 0(按注册) → Order 1
            var expected = new[] { 3, 1, 2, 5, 4 };
            CollectionAssert.AreEqual(expected, executionOrder);
            AesirArchitectureDebug.LogTestInfo("Register(同order): 同 order 按注册顺序执行，不同 order 按升序排列");
        }

        /// <summary>
        /// 断言 markerType 在 PlayerLoop 中与 targetType 处于同一父级，且排在 targetType 之前
        /// </summary>
        static void AssertIsBeforeInLoop(Type markerType, Type targetType)
        {
            var loop = PlayerLoop.GetCurrentPlayerLoop();
            var found = TryFindSiblingIndices(ref loop, markerType, targetType, out var markerIdx,
                out var targetIdx);
            Assert.IsTrue(found, $"{markerType.Name} and {targetType.Name} should be siblings in the loop");
            Assert.Less(markerIdx, targetIdx, $"{markerType.Name} should be before {targetType.Name}");
        }

        /// <summary>
        /// 断言 markerType 在 PlayerLoop 中与 targetType 处于同一父级，且排在 targetType 之后
        /// </summary>
        static void AssertIsAfterInLoop(Type markerType, Type targetType)
        {
            var loop = PlayerLoop.GetCurrentPlayerLoop();
            var found = TryFindSiblingIndices(ref loop, markerType, targetType, out var markerIdx,
                out var targetIdx);
            Assert.IsTrue(found, $"{markerType.Name} and {targetType.Name} should be siblings in the loop");
            Assert.Greater(markerIdx, targetIdx, $"{markerType.Name} should be after {targetType.Name}");
        }

        static bool TryFindSiblingIndices(ref PlayerLoopSystem system,
            Type typeA,
            Type typeB,
            out int indexA,
            out int indexB)
        {
            indexA = -1;
            indexB = -1;

            if (system.subSystemList == null)
            {
                return false;
            }

            for (var i = 0; i < system.subSystemList.Length; i++)
            {
                if (system.subSystemList[i].type == typeA)
                {
                    indexA = i;
                }

                if (system.subSystemList[i].type == typeB)
                {
                    indexB = i;
                }
            }

            if (indexA >= 0 && indexB >= 0)
            {
                return true;
            }

            for (var i = 0; i < system.subSystemList.Length; i++)
            {
                if (TryFindSiblingIndices(ref system.subSystemList[i], typeA, typeB, out indexA, out indexB))
                {
                    return true;
                }
            }

            return false;
        }

        static bool ContainsType(ref PlayerLoopSystem system, Type targetType)
        {
            if (system.type == targetType)
            {
                return true;
            }

            if (system.subSystemList == null)
            {
                return false;
            }

            for (var i = 0; i < system.subSystemList.Length; i++)
            {
                if (ContainsType(ref system.subSystemList[i], targetType))
                {
                    return true;
                }
            }

            return false;
        }

        struct TestMarkerBeforeUpdate { }

        struct TestMarkerAfterFixedUpdate { }

        struct TestMarkerTwice1 { }

        struct TestMarkerTwice2 { }

        struct TestMarkerForDump { }
    }
}
