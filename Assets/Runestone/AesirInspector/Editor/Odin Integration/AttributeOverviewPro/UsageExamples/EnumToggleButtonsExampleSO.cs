using System;
using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class EnumToggleButtonsExampleSO : AttributeExampleSO<EnumToggleButtonsExampleSO>
    {
        [Flags]
        public enum SomeBitmaskEnum
        {
            A = 1 << 0,
            B = 1 << 1,
            C = 1 << 2,
            All = A | B | C
        }

        public enum SomeEnum
        {
            First,
            Second,
            Third,
            Fourth
        }

        public enum SomeEnumWithIcons
        {
            [LabelText(SdfIconType.TextLeft)]
            Left,

            [LabelText(SdfIconType.TextCenter)]
            Center,

            [LabelText(SdfIconType.TextRight)]
            Right
        }

        [Title("No Parameters")]
        [EnumToggleButtons]
        public SomeEnum someEnumField;

        [EnumToggleButtons]
        [HideLabel]
        public SomeEnum wideEnumField;

        [Title("Bitmask Support")]
        [EnumToggleButtons]
        public SomeBitmaskEnum bitmaskEnumField;

        [Title("Icons Support")]
        [EnumToggleButtons]
        public SomeEnumWithIcons enumWithIcons;

        public override void AesirInspectorReset()
        {
            someEnumField = SomeEnum.First;
            wideEnumField = SomeEnum.First;
            bitmaskEnumField = 0;
            enumWithIcons = SomeEnumWithIcons.Center;
        }
    }
}
