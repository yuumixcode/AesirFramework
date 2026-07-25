using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// ValidateInput 特性的案例 SO。
    /// </summary>
    [AesirExample]
    internal class ValidateInputExampleSO : AttributeExampleSO<ValidateInputExampleSO>
    {
        [Title("Expression (@)")]
        [ValidateInput("@!string.IsNullOrEmpty($value)", "The string cannot be empty!")]
        public string notEmptyString = "Hello";

        [Title("Parameter: Condition (Method)")]
        [ValidateInput("ValidateGreaterThanZero", "Value must be greater than zero")]
        public int greaterThanZero = 10;

        [Title("Advanced Validation (ref string message)")]
        [ValidateInput("ValidateWithDynamicMessage")]
        public int dynamicMessageValue = 5;

        bool ValidateGreaterThanZero(int value) => value > 0;

        bool ValidateWithDynamicMessage(int value, ref string message, ref InfoMessageType messageType)
        {
            if (value < 0)
            {
                message = "Value is negative!";
                messageType = InfoMessageType.Error;
                return false;
            }

            if (value < 10)
            {
                message = "Value is small, but acceptable.";
                messageType = InfoMessageType.Warning;
                return true; // Still returns true, but shows a warning
            }

            return true;
        }

        public override void AesirInspectorReset()
        {
            notEmptyString = "Hello";
            greaterThanZero = 10;
            dynamicMessageValue = 5;
        }
    }
}
