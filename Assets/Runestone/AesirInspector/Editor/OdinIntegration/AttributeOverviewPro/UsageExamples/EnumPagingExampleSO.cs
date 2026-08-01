using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class EnumPagingExampleSO : AttributeExampleSO<EnumPagingExampleSO>
    {
        public enum SomeEnum
        {
            A,
            B,
            C
        }

        [Title("No Parameters")]
        [EnumPaging]
        public SomeEnum someEnumField;

        public override void AesirInspectorReset()
        {
            someEnumField = SomeEnum.A;
        }
    }
}
