using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class
        CustomContextMenuExampleWithActionSO : AttributeExampleSO<CustomContextMenuExampleWithActionSO>
    {
        [Title("Member Reference ($)")]
        [InfoBox("Right click the field label to execute the context menu item")]
        [CustomContextMenu("Log", "LogValue")]
        public string methodNameExample = "Peace";

        [Title("Expression (@)")]
        [InfoBox("Right click the field label to execute the context menu item")]
        [CustomContextMenu("Log Expression", "@Debug.Log($value)")]
        public string expressionExample = "Love";

        void LogValue(string value)
        {
            Debug.Log(value);
        }

        public override void AesirInspectorReset()
        {
            methodNameExample = "Peace";
            expressionExample = "Love";
        }
    }
}
