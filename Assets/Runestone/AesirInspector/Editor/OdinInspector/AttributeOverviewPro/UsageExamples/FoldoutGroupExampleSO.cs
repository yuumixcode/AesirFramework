using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class FoldoutGroupExampleSO : AttributeExampleSO<FoldoutGroupExampleSO>
    {
        [Title("No Parameters")]
        [FoldoutGroup("My Foldout")]
        public int a;

        [FoldoutGroup("My Foldout")]
        public int b;

        [Title("Parameter: Expanded")]
        [FoldoutGroup("Initially Expanded", true)]
        public int c;

        [FoldoutGroup("Initially Expanded")]
        public int d;

        [Title("Usage with Nesting")]
        [FoldoutGroup("Nested")]
        [BoxGroup("Nested/Inside Box")]
        public int e;

        public override void AesirInspectorReset()
        {
            a = 0;
            b = 0;
            c = 0;
            d = 0;
            e = 0;
        }
    }
}
