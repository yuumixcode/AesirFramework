using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class CustomValueDrawerExampleSO : AttributeExampleSO<CustomValueDrawerExampleSO>
    {
        [Title("Controls")]
        public float min;

        public float max = 10f;

        [Title("Parameter: Action (float value, GUIContent label)")]
        [CustomValueDrawer("DrawSlider")]
        public float customSlider = 5f;

        [Title("Parameter: Action (Color value, GUIContent label)")]
        [CustomValueDrawer("DrawColorBox")]
        public Color customColor = Color.red;

        public override void AesirInspectorReset()
        {
            customSlider = 5f;
            customColor = Color.red;
            min = 0f;
            max = 10f;
        }

        float DrawSlider(float value, GUIContent label) => EditorGUILayout.Slider(label, value, min, max);

        Color DrawColorBox(Color value, GUIContent label)
        {
            var rect = EditorGUILayout.GetControlRect();
            if (label != null)
            {
                rect = EditorGUI.PrefixLabel(rect, label);
            }

            var newColor = EditorGUI.ColorField(rect, value);
            return newColor;
        }
    }
}
