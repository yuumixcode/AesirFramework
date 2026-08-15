using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class RequiredListLengthExampleSO : AttributeExampleSO<RequiredListLengthExampleSO>
    {
        [Title("Fixed Minimum Length")]
        [RequiredListLength(3)]
        public List<int> fixedMinLengthList = new List<int>();

        [Title("Min/Max Length Range")]
        [RequiredListLength(3, 5)]
        public List<int> rangeLengthList = new List<int>();

        public override void AesirInspectorReset()
        {
            fixedMinLengthList = new List<int>();
            rangeLengthList = new List<int>();
        }
    }
}
