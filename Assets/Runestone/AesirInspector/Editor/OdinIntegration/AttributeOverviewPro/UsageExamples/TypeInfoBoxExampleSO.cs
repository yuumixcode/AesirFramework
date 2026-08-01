using System;
using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class TypeInfoBoxExampleSO : AttributeExampleSO<TypeInfoBoxExampleSO>
    {
        [Title("No Parameters")]
        public MyType myObject = new MyType();

        public override void AesirInspectorReset()
        {
            myObject = new MyType();
        }

        [Serializable]
        [TypeInfoBox("This InfoBox is displayed at the top of the class.")]
        public class MyType
        {
            public int value;
        }
    }
}
