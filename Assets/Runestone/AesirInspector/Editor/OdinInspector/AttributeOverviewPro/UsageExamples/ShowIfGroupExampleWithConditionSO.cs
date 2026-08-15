using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class ShowIfGroupExampleWithConditionSO : AttributeExampleSO<ShowIfGroupExampleWithConditionSO>
    {
        [Title("Controls")]
        public bool showGroup = true;

        [FoldoutGroup("Field Name Example")]
        [ShowIfGroup("Show", Condition = "showGroup")]
        [FoldoutGroup("Show/Field Name Example")]
        public string fieldNameExample;

        [FoldoutGroup("Property Name Example")]
        [ShowIfGroup("Show", Condition = "ShowGroupProperty")]
        [FoldoutGroup("Show/Property Name Example")]
        public string propertyNameExample;

        [FoldoutGroup("Method Name Example")]
        [ShowIfGroup("Show", Condition = "GetShowState")]
        [FoldoutGroup("Show/Method Name Example")]
        public string methodNameExample;

        [FoldoutGroup("Expression (@)")]
        [ShowIfGroup("Show", Condition = "@showGroup")]
        [FoldoutGroup("Show/Attribute Expression Example")]
        public string attributeExpressionExample;

        public bool ShowGroupProperty => showGroup;

        bool GetShowState() => showGroup;

        public override void AesirInspectorReset()
        {
            showGroup = true;
            fieldNameExample = string.Empty;
            propertyNameExample = string.Empty;
            methodNameExample = string.Empty;
            attributeExpressionExample = string.Empty;
        }
    }
}
