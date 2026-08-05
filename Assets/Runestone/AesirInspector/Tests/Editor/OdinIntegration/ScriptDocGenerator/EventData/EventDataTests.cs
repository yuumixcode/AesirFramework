using Runestone.AesirInspector;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

#pragma warning disable CS0067 // 事件从未使用过

namespace Runestone.AesirInspector.Editor.Tests
{
    /// <summary>
    /// 测试事件类型字段
    /// </summary>
    [SuppressMessage("ReSharper", "EventNeverSubscribedTo.Local")]
    public class EventDataTests
    {
        static readonly EventInfo[] EventInfos = typeof(TestClass).GetRuntimeEvents().ToArray();

        static readonly IEventData[] EventDataArray = EventInfos
            .Select(e => UnitTestAnalysisFactory.Default.CreateEventData(e)).ToArray();

        static readonly Dictionary<string, string> ExpectedSignatureMaps = new Dictionary<string, string>
        {
            { nameof(TestClass.ActionEvent), "public event Action ActionEvent;" },
            {
                nameof(TestClass.ActionWithParamsEvent),
                "public event Action<int, string> ActionWithParamsEvent;"
            },
            {
                nameof(TestClass.FuncWithParamsEvent),
                "public event Func<int, string, bool> FuncWithParamsEvent;"
            },
            { nameof(TestClass.PredicateEvent), "public event Predicate<int> PredicateEvent;" },
            { nameof(TestClass.ComparisonEvent), "public event Comparison<string> ComparisonEvent;" },
            { nameof(TestClass.StaticActionEvent), "public static event Action<bool> StaticActionEvent;" }
        };

        [Test]
        public void TestActionEvent()
        {
            var fieldData = EventDataArray.First(f => ((MemberData)f).Name == "ActionEvent");
            Debug.Log(fieldData.Signature);
            Assert.AreEqual(ExpectedSignatureMaps[nameof(TestClass.ActionEvent)], fieldData.Signature);
        }

        [Test]
        public void TestActionWithParamsEvent()
        {
            var fieldData = EventDataArray.First(f => ((MemberData)f).Name == "ActionWithParamsEvent");
            Debug.Log(fieldData.Signature);
            Assert.AreEqual(ExpectedSignatureMaps[nameof(TestClass.ActionWithParamsEvent)],
                fieldData.Signature);
        }

        [Test]
        public void TestFuncWithParamsEvent()
        {
            var fieldData = EventDataArray.First(f => ((MemberData)f).Name == "FuncWithParamsEvent");
            Debug.Log(fieldData.Signature);
            Assert.AreEqual(ExpectedSignatureMaps[nameof(TestClass.FuncWithParamsEvent)],
                fieldData.Signature);
        }

        [Test]
        public void TestPredicateEvent()
        {
            var fieldData = EventDataArray.First(f => ((MemberData)f).Name == "PredicateEvent");
            Debug.Log(fieldData.Signature);
            Assert.AreEqual(ExpectedSignatureMaps[nameof(TestClass.PredicateEvent)], fieldData.Signature);
        }

        [Test]
        public void TestComparisonEvent()
        {
            var fieldData = EventDataArray.First(f => ((MemberData)f).Name == "ComparisonEvent");
            Debug.Log(fieldData.Signature);
            Assert.AreEqual(ExpectedSignatureMaps[nameof(TestClass.ComparisonEvent)], fieldData.Signature);
        }

        [Test]
        public void TestStaticActionEvent()
        {
            var fieldData = EventDataArray.First(f => ((MemberData)f).Name == "StaticActionEvent");
            Debug.Log(fieldData.Signature);
            Assert.AreEqual(ExpectedSignatureMaps[nameof(TestClass.StaticActionEvent)], fieldData.Signature);
        }

        #region Nested type: TestClass

        class TestClass
        {
            /// <summary>
            /// Action 事件
            /// </summary>
            public event Action ActionEvent;

            /// <summary>
            /// Action 带参数事件
            /// </summary>
            public event Action<int, string> ActionWithParamsEvent;

            /// <summary>
            /// Func 带参数事件
            /// </summary>
            public event Func<int, string, bool> FuncWithParamsEvent;

            /// <summary>
            /// Predicate 事件
            /// </summary>
            public event Predicate<int> PredicateEvent;

            /// <summary>
            /// Comparison 事件
            /// </summary>
            public event Comparison<string> ComparisonEvent;

            /// <summary>
            /// 静态 Action 事件
            /// </summary>
            public static event Action<bool> StaticActionEvent;
        }

        #endregion
    }
}
