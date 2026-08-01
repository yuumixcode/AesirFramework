using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class
        TableMatrixExampleWithDrawElementMethodSO : OdinAttributeExampleSO<
        TableMatrixExampleWithDrawElementMethodSO>
    {
        [Title("DrawElementMethod: Simple")]
        [TableMatrix(DrawElementMethod = "DrawAsLabel")]
        public string[,] simpleDrawElement = new string[2, 3];

        [Title("DrawElementMethod: Colored Rect")]
        [TableMatrix(DrawElementMethod = "DrawAsColoredRect")]
        public bool[,] coloredDrawElement = new bool[5, 5];

        public Color TrueColor = new Color(0.11f, 0.77f, 0.5f, 1f);
        public Color FalseColor = new Color(1f, 0.4f, 0.14f, 1f);

        string DrawAsLabel(Rect rect, string value)
        {
            EditorGUI.LabelField(rect, value);
            return value;
        }

        bool DrawAsColoredRect(Rect rect, bool[,] table, int x, int y)
        {
            var value = table[x, y];
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                table[x, y] = !value;
            }

            EditorGUI.DrawRect(rect, value ? TrueColor : FalseColor);
            return value;
        }

        public override void AesirInspectorReset()
        {
            simpleDrawElement = new string[2, 3];
            coloredDrawElement = new bool[5, 5];
            TrueColor = new Color(0.11f, 0.77f, 0.5f, 1f);
            FalseColor = new Color(1f, 0.4f, 0.14f, 1f);
        }
    }
}
