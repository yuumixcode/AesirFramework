using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// DisableIf 特性的案例 SO。
    /// </summary>
    [AesirExample]
    internal class DisableIfExampleSO : AttributeExampleSO<DisableIfExampleSO>
    {
        [Title("Controls")]
        public bool isToggled;

        [EnumToggleButtons]
        public InfoMessageType someEnum;

        [Title("No Parameters")]
        [DisableIf("isToggled")]
        public int disabledWhenToggled;

        [Title("Parameter: Value")]
        [DisableIf("someEnum", InfoMessageType.Info)]
        public string disabledWhenInfo = "Disabled when someEnum is Info";

        [DisableIf("someEnum", InfoMessageType.Error)]
        public string disabledWhenError = "Disabled when someEnum is Error";

        [Title("Expression (@)")]
        [DisableIf("@this.isToggled || this.someEnum == InfoMessageType.Warning")]
        public string disabledWithExpression = "Complex condition with expression";

        public override void AesirInspectorReset()
        {
            isToggled = false;
            someEnum = InfoMessageType.None;
            disabledWhenToggled = 0;
            disabledWhenInfo = "Disabled when someEnum is Info";
            disabledWhenError = "Disabled when someEnum is Error";
            disabledWithExpression = "Complex condition with expression";
        }
    }
}
