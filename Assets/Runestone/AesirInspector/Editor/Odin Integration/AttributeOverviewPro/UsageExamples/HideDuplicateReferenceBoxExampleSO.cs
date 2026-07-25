using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class HideDuplicateReferenceBoxExampleSO : AttributeExampleSO<HideDuplicateReferenceBoxExampleSO>
    {
        [Title("No Parameters")]
        public List<int> list1 = new List<int> { 1, 2, 3 };

        [HideDuplicateReferenceBox]
        public List<int> list2 = new List<int> { 1, 2, 3 };

        public override void AesirInspectorReset()
        {
            list1 = new List<int> { 1, 2, 3 };
            list2 = new List<int> { 1, 2, 3 };
        }
    }
}
