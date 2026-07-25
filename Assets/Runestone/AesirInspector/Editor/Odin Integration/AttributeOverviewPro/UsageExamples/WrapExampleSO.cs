using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class WrapExampleSO : AttributeExampleSO<WrapExampleSO>
    {
        [Title("No Parameters")]
        [Wrap(0f, 30f)]
        public int wrappedInt;

        [Wrap(0f, 30f)]
        public float wrappedFloat;

        [Wrap(0f, 30f)]
        public Vector3 wrappedVector3;

        [Title("Special Values")]
        [Wrap(0f, 360f)]
        public float angleWrap;

        [Wrap(0f, 6.28318548f)]
        public float radianWrap;

        public override void AesirInspectorReset()
        {
            wrappedInt = 0;
            wrappedFloat = 0f;
            wrappedVector3 = Vector3.zero;
            angleWrap = 0f;
            radianWrap = 0f;
        }
    }
}
