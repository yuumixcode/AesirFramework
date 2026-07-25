using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class HideInEditorModeExampleSO : AttributeExampleSO<HideInEditorModeExampleSO>
    {
        [Title("No Parameters")]
        [HideInEditorMode]
        public string hiddenInEditor;

        public override void AesirInspectorReset()
        {
            hiddenInEditor = "This is visible in play mode but hidden in editor mode";
        }
    }
}
