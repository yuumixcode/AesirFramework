using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class TitleGroupExampleSO : AttributeExampleSO<TitleGroupExampleSO>
    {
        [FoldoutGroup("No Parameters")]
        [TitleGroup("Main Title")]
        public int noParamsField;

        [FoldoutGroup("Parameter: Subtitle")]
        [TitleGroup("Main Title", "This is a subtitle")]
        public int withSubtitle;

        [FoldoutGroup("Parameter: Alignment")]
        [TitleGroup("Centered Title", "Subtitle", TitleAlignments.Centered)]
        public int centered;

        [FoldoutGroup("Parameter: Alignment")]
        [TitleGroup("Split Title", "Subtitle", TitleAlignments.Split)]
        public int split;

        [FoldoutGroup("Parameter: HorizontalLine")]
        [TitleGroup("No Horizontal Line", horizontalLine: false)]
        public int noHorizontalLine;

        [FoldoutGroup("Parameter: BoldTitle")]
        [TitleGroup("Not Bold", boldTitle: false)]
        public int notBold;

        [FoldoutGroup("Parameter: Indent")]
        [TitleGroup("Indented Title", indent: true)]
        public int indented;

        [FoldoutGroup("Parameter: Indent")]
        [TitleGroup("Nested/Deeply Indented", indent: true)]
        public int nestedIndented;

        [FoldoutGroup("Parameter: Order")]
        [TitleGroup("Order 5", order: 5)]
        public int order5;

        [FoldoutGroup("Parameter: Order")]
        [TitleGroup("Order 2", order: 2)]
        public int order2;

        public override void AesirInspectorReset()
        {
            noParamsField = 0;
            withSubtitle = 0;
            centered = 0;
            split = 0;
            noHorizontalLine = 0;
            notBold = 0;
            indented = 0;
            nestedIndented = 0;
            order5 = 0;
            order2 = 0;
        }
    }
}
