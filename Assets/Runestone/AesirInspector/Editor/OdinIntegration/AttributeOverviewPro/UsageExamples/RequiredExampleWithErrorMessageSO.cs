using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class RequiredExampleWithErrorMessageSO : AttributeExampleSO<RequiredExampleWithErrorMessageSO>
    {
        [Title("Member Reference ($)")]
        public string customError = "My custom error message from field";

        [Required("$customError")]
        public string referenceExample;

        [Title("Expression (@)")]
        [Required("@$property.NiceName + \" is required!\"")]
        public string expressionExample;

        public override void AesirInspectorReset()
        {
            customError = "My custom error message from field";
            referenceExample = "";
            expressionExample = "";
        }
    }
}
