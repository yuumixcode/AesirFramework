using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// TypeFilter 特性案例。
    /// </summary>
    [AesirExample]
    internal class TypeFilterExampleSO : AttributeExampleSO<TypeFilterExampleSO>
    {
        [Title("Parameter: FilterMethod")]
        [TypeFilter("GetFilteredTypeList")]
        public IMyInterface FilteredInstance;

        public IEnumerable<Type> GetFilteredTypeList()
        {
            var q = typeof(IMyInterface).Assembly.GetTypes().Where(x => !x.IsAbstract)
                .Where(x => !x.IsInterface).Where(x => typeof(IMyInterface).IsAssignableFrom(x));
            return q;
        }

        public override void AesirInspectorReset()
        {
            FilteredInstance = null;
        }
    }

    public interface IMyInterface
    {
        void DoSomething();
    }

    public class MyImplementationA : IMyInterface
    {
        public int A;
        public void DoSomething() => Debug.Log("A");
    }

    public class MyImplementationB : IMyInterface
    {
        public string B;
        public void DoSomething() => Debug.Log("B");
    }
}
