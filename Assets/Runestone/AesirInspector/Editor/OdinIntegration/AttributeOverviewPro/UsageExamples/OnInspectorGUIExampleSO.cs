using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// OnInspectorGUI 特性案例。
    /// </summary>
    [AesirExample]
    internal class OnInspectorGUIExampleSO : AttributeExampleSO<OnInspectorGUIExampleSO>
    {
        [Title("Parameter: Action (Before Field)")]
        [OnInspectorGUI("DrawLabelBefore", false)]
        public string FieldWithLabel;

        [Title("Parameter: Action (After Field)")]
        [OnInspectorGUI("DrawButtonAfter")]
        public int FieldWithButton;

        void DrawLabelBefore()
        {
            GUILayout.Label("This label is drawn before the field.", EditorStyles.boldLabel);
        }

        void DrawButtonAfter()
        {
            if (GUILayout.Button("Click Me!"))
            {
                Debug.Log("Button clicked!");
            }
        }

        [Title("No Parameters (On Method)")]
        [OnInspectorGUI]
        void DrawCustomGUI()
        {
            var rect = EditorGUILayout.GetControlRect(false, 2);
            EditorGUI.DrawRect(rect, Color.grey);
        }

        public override void AesirInspectorReset()
        {
            FieldWithLabel = "Hello";
            FieldWithButton = 0;
        }
    }
}
