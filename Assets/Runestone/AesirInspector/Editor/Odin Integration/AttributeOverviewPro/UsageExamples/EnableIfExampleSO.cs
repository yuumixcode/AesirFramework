using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// EnableIf 特性的案例 SO。
    /// </summary>
    [AesirExample]
    internal class EnableIfExampleSO : AttributeExampleSO<EnableIfExampleSO>
    {
        [Title("Controls")]
        public bool isToggled;

        [EnumToggleButtons]
        public InfoMessageType someEnum;

        [Title("No Parameters")]
        [EnableIf("isToggled")]
        public int enabledWhenToggled;

        [Title("Parameter: Value")]
        [EnableIf("someEnum", InfoMessageType.Info)]
        public string enabledWhenInfo = "Only editable when someEnum is Info";

        [EnableIf("someEnum", InfoMessageType.Error)]
        public string enabledWhenError = "Only editable when someEnum is Error";

        [Title("Expression (@)")]
        [EnableIf("@this.isToggled && this.someEnum == InfoMessageType.Warning")]
        public string enabledWithExpression = "Complex condition with expression";

        public override void AesirInspectorReset()
        {
            isToggled = false;
            someEnum = InfoMessageType.None;
            enabledWhenToggled = 0;
            enabledWhenInfo = "Only editable when someEnum is Info";
            enabledWhenError = "Only editable when someEnum is Error";
            enabledWithExpression = "Complex condition with expression";
        }
    }
}
