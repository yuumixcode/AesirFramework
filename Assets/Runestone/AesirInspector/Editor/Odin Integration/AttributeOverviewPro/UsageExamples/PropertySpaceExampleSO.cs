using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// PropertySpace 特性的案例 SO。
    /// </summary>
    [AesirExample]
    internal class PropertySpaceExampleSO : AttributeExampleSO<PropertySpaceExampleSO>
    {
        [Title("Parameter: SpaceBefore")]
        [PropertySpace(20)]
        public int spaceBefore;

        [Title("Parameter: SpaceBefore, SpaceAfter")]
        [PropertySpace(20, 20)]
        public int spaceBeforeAndAfter;

        [Title("No Parameters")]
        public int noSpace;

        public override void AesirInspectorReset()
        {
            spaceBefore = 0;
            spaceBeforeAndAfter = 0;
            noSpace = 0;
        }
    }
}
