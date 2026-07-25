using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class GUIColorExampleWithColorSO : AttributeExampleSO<GUIColorExampleWithColorSO>
    {
        [Title("Expression (@)")]
        public bool useRed;

        [GUIColor("@useRed ? UnityEngine.Color.red : UnityEngine.Color.green")]
        public string attributeExpressionExample;

        [Title("Member Reference ($)")]
        public Color color = Color.green;

        [GUIColor("$color")]
        public string fieldNameExample;

        [Title("Parameter: Color (Method)")]
        [GUIColor("$GetColor")]
        public string methodNameExample;

        [Title("Parameter: Color (Property)")]
        [GUIColor("$ColorProperty")]
        public string propertyNameExample;

        [Title("Usage Example: Dynamic Color")]
        [GUIColor("$GetDynamicColor")]
        public int dynamicColorExample;

        public Color ColorProperty => color;

        public override void AesirInspectorReset()
        {
            useRed = false;
            color = Color.green;
            attributeExpressionExample = string.Empty;
            fieldNameExample = string.Empty;
            methodNameExample = string.Empty;
            propertyNameExample = string.Empty;
            dynamicColorExample = 0;
        }

        Color GetColor() => useRed ? Color.red : Color.green;

        static Color GetDynamicColor()
        {
            GUIHelper.RequestRepaint();
            return Color.HSVToRGB(Mathf.Cos((float)EditorApplication.timeSinceStartup) * 0.225f + 0.325f, 1,
                1);
        }
    }
}
