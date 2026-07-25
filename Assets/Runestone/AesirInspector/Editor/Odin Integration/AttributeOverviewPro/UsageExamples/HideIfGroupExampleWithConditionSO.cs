using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class HideIfGroupExampleWithConditionSO : AttributeExampleSO<HideIfGroupExampleWithConditionSO>
    {
        [Title("Controls")]
        public bool hideGroup = true;

        [FoldoutGroup("Field Name Example")]
        [HideIfGroup("Hidden", Condition = "hideGroup")]
        [FoldoutGroup("Hidden/Field Name Example")]
        public string fieldNameExample;

        [FoldoutGroup("Property Name Example")]
        [HideIfGroup("Hidden", Condition = "HideGroupProperty")]
        [FoldoutGroup("Hidden/Property Name Example")]
        public string propertyNameExample;

        [FoldoutGroup("Method Name Example")]
        [HideIfGroup("Hidden", Condition = "GetHiddenState")]
        [FoldoutGroup("Hidden/Method Name Example")]
        public string methodNameExample;

        [FoldoutGroup("Expression (@)")]
        [HideIfGroup("Hidden", Condition = "@hideGroup")]
        [FoldoutGroup("Hidden/Attribute Expression Example")]
        public string attributeExpressionExample;

        public bool HideGroupProperty => hideGroup;

        bool GetHiddenState() => hideGroup;

        public override void AesirInspectorReset()
        {
            hideGroup = true;
            fieldNameExample = string.Empty;
            propertyNameExample = string.Empty;
            methodNameExample = string.Empty;
            attributeExpressionExample = string.Empty;
        }
    }
}
