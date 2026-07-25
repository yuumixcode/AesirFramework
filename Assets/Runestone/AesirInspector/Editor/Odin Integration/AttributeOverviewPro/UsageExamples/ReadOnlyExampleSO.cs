using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// ReadOnly 特性的案例 SO。
    /// </summary>
    [AesirExample]
    internal class ReadOnlyExampleSO : AttributeExampleSO<ReadOnlyExampleSO>
    {
        [Title("No Parameters")]
        [ReadOnly]
        public string readOnlyField = "This field is read-only in the inspector";

        [ReadOnly]
        public List<int> readOnlyList = new List<int> { 1, 2, 3 };

        [Title("Usage with Properties")]
        [ShowInInspector]
        [ReadOnly]
        public int ReadOnlyProperty => 42;

        public override void AesirInspectorReset()
        {
            readOnlyField = "This field is read-only in the inspector";
            readOnlyList = new List<int> { 1, 2, 3 };
        }
    }
}
