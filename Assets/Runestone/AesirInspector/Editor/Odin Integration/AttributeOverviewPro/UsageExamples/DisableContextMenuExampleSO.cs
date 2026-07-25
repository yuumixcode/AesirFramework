using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class DisableContextMenuExampleSO : AttributeExampleSO<DisableContextMenuExampleSO>
    {
        [Title("No Parameters")]
        [DisableContextMenu]
        public int[] noRightClickList = { 2, 3, 5 };

        [DisableContextMenu]
        public int noRightClickField = 19;

        [Title("Parameter: DisableForProperty = true, DisableForChildren = true")]
        [DisableContextMenu(true, true)]
        public int[] disableRightClickCompletely = { 13, 17 };

        public override void AesirInspectorReset()
        {
            noRightClickList = new[] { 2, 3, 5 };
            noRightClickField = 19;
            disableRightClickCompletely = new[] { 13, 17 };
        }
    }
}
