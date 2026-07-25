using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class PreviewFieldExampleSO : AttributeExampleSO<PreviewFieldExampleSO>
    {
        [Title("No Parameters")]
        [PreviewField]
        public Texture regularPreviewField;

        [Title("Parameter: ObjectFieldAlignment")]
        [PreviewField(ObjectFieldAlignment.Center)]
        public Texture previewField2;

        [Title("Parameter: Height")]
        [PreviewField(Height = 70)]
        public Texture2D texture2D;

        [Title("Parameter: FilterMode")]
        [PreviewField(FilterMode = FilterMode.Point)]
        public Texture2D texture2D2;

        public override void AesirInspectorReset()
        {
            regularPreviewField = null;
            previewField2 = null;
            texture2D = null;
            texture2D2 = null;
        }
    }
}
