using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// Indent 特性案例。
    /// </summary>
    [AesirExample]
    internal class IndentExampleSO : AttributeExampleSO<IndentExampleSO>
    {
        [Title("No Parameters")]
        [Indent]
        public int A;

        [Title("Parameter: IndentLevel")]
        [Indent(2)]
        public int B;

        [Indent(3)]
        public int C;

        [Indent(4)]
        public int D;

        [Title("Parameter: IndentLevel (Negative)")]
        [Indent(-1)]
        public int G;

        public override void AesirInspectorReset()
        {
            A = 0;
            B = 0;
            C = 0;
            D = 0;
            G = 0;
        }
    }
}
