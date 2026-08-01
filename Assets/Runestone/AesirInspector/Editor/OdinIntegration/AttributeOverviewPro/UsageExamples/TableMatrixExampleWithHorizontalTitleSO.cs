using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class
        TableMatrixExampleWithHorizontalTitleSO : OdinAttributeExampleSO<
        TableMatrixExampleWithHorizontalTitleSO>
    {
        public string Title = "Peace, Love & Ducks";
        public string AlternativeTitle = "Peace, Love & Fenrir";
        public bool UseAlternativeTitle;

        [Title("Field Name Example")]
        [TableMatrix(HorizontalTitle = "$Title")]
        public bool[,] fieldNameExample = new bool[5, 5];

        [Title("Attribute Expression Example")]
        [TableMatrix(HorizontalTitle = "@UseAlternativeTitle ? AlternativeTitle : Title")]
        public bool[,] attributeExpressionExample = new bool[5, 5];

        [Title("Property Name Example")]
        [TableMatrix(HorizontalTitle = "$TitleProperty")]
        public bool[,] propertyNameExample = new bool[5, 5];

        [Title("Method Name Example")]
        [TableMatrix(HorizontalTitle = "$GetTitle")]
        public bool[,] methodNameExample = new bool[5, 5];

        public string TitleProperty => UseAlternativeTitle ? AlternativeTitle : Title;

        string GetTitle() => UseAlternativeTitle ? AlternativeTitle : Title;

        public override void AesirInspectorReset()
        {
            Title = "Peace, Love & Ducks";
            AlternativeTitle = "Peace, Love & Fenrir";
            UseAlternativeTitle = false;
            fieldNameExample = new bool[5, 5];
            attributeExpressionExample = new bool[5, 5];
            propertyNameExample = new bool[5, 5];
            methodNameExample = new bool[5, 5];
        }
    }
}
