using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class ShowDrawerChainExampleSO : AttributeExampleSO<ShowDrawerChainExampleSO>
    {
        [Title("No Parameters")]
        [ShowDrawerChain]
        public int intValue;

        [ShowDrawerChain]
        public float floatValue;

        [ShowDrawerChain]
        public bool boolValue;

        [ShowDrawerChain]
        public string stringValue = "Unity Built-in";

        [ShowDrawerChain]
        public Vector2 vector2Value;

        [ShowDrawerChain]
        public LayerMask layerMask;

        [ShowDrawerChain]
        public Color color;

        public override void AesirInspectorReset()
        {
            intValue = 0;
            floatValue = 0f;
            boolValue = false;
            stringValue = "Unity Built-in";
            vector2Value = Vector2.zero;
            layerMask = 0;
            color = Color.white;
        }
    }
}
