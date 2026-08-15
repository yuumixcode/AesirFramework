using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class OnValueChangedExampleSO : AttributeExampleSO<OnValueChangedExampleSO>
    {
        [Title("No Parameters")]
        [OnValueChanged("OnValueChange")]
        public int value;

        [Title("Usage Examples")]
        [OnValueChanged("OnShaderChange")]
        public Shader shader;

        [ReadOnly]
        [InlineEditor(InlineEditorModes.LargePreview)]
        public Material material;

        void OnValueChange()
        {
            Debug.Log("Value changed to: " + value);
        }

        void OnShaderChange()
        {
            if (material != null)
            {
                DestroyImmediate(material);
                material = null;
            }

            if (shader != null)
            {
                material = new Material(shader);
            }
        }

        public override void AesirInspectorReset()
        {
            value = 0;
            shader = null;
            if (material != null)
            {
                DestroyImmediate(material);
                material = null;
            }
        }
    }
}
