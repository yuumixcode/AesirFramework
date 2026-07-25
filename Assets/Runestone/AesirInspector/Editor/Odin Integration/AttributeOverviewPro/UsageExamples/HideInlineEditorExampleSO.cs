using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class HideInInlineEditorsExampleSO : AttributeExampleSO<HideInInlineEditorsExampleSO>
    {
        [Title("No Parameters")]
        [InlineEditor]
        [HideInInlineEditors]
        public HideMonoScriptExampleSO hiddenInlineEditor;

        [Title("Without HideInlineEditor")]
        [InlineEditor]
        public HideMonoScriptExampleSO shownInlineEditor;

        public override void AesirInspectorReset()
        {
            hiddenInlineEditor = null;
            shownInlineEditor = null;
        }
    }
}
