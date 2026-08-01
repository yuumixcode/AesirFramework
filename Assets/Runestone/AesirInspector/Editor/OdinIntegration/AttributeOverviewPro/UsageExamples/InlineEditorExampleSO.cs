using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class InlineEditorExampleSO : AttributeExampleSO<InlineEditorExampleSO>
    {
        [FoldoutGroup("No Parameters")]
        [InlineEditor]
        public Material material;

        [FoldoutGroup("Parameter: InlineEditorModes")]
        [Title("FullEditor")]
        [InlineEditor(InlineEditorModes.FullEditor)]
        public Material fullEditor;

        [FoldoutGroup("Parameter: InlineEditorModes")]
        [Title("GUIAndHeader")]
        [InlineEditor(InlineEditorModes.GUIAndHeader)]
        public Material guiAndHeader;

        [FoldoutGroup("Parameter: InlineEditorModes")]
        [Title("LargePreview")]
        [InlineEditor(InlineEditorModes.LargePreview)]
        public Mesh mesh;

        [FoldoutGroup("Parameter: InlineEditorObjectFieldModes")]
        [Title("Foldout")]
        [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
        public Material foldoutMode;

        [FoldoutGroup("Parameter: InlineEditorObjectFieldModes")]
        [Title("Hidden")]
        [InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        public Material hiddenMode;

        public override void AesirInspectorReset()
        {
            material = null;
            fullEditor = null;
            guiAndHeader = null;
            mesh = null;
            foldoutMode = null;
            hiddenMode = null;
        }
    }
}
