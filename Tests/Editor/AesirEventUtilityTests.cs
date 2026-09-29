using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;

namespace Runestone.AesirModules.Tests.Editor
{
    /// <summary>
    /// <see cref="AesirEventUtility" /> 绑定键缓存的静态重置守护用例。
    /// </summary>
    public class AesirEventUtilityTests
    {
        [Test]
        public void ResetStatics_ClearsBindingKeyCache()
        {
            AesirEventUtility.GetEventBindingKey<AesirEventArgs>();

            var cache = GetKeyCache();
            Assert.Greater(cache.Count, 0, "前置：查询绑定键后缓存应有条目");

            typeof(AesirEventUtility).GetMethod("ResetStatics", BindingFlags.NonPublic | BindingFlags.Static)
                .Invoke(null, null);

            Assert.AreEqual(0, cache.Count,
                "域加载期重置必须清空绑定键缓存——缓存是纯派生数据（AssemblyQualifiedName），清空后下次查询按需重建");
        }

        static Dictionary<Type, string> GetKeyCache() =>
            (Dictionary<Type, string>)typeof(AesirEventUtility)
                .GetField("KeyCache", BindingFlags.NonPublic | BindingFlags.Static)
                .GetValue(null);
    }
}
