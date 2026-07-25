using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class TitleExampleSO : AttributeExampleSO<TitleExampleSO>
    {
        [Title("No Parameters")]
        public int defaultTitle;

        [Title("Parameter: Subtitle", "This is a subtitle")]
        public int subtitleTitle;

        [Title("Parameter: TitleAlignment (Centered)", TitleAlignment = TitleAlignments.Centered)]
        public int centeredTitle;

        [Title("Parameter: TitleAlignment (Right)", TitleAlignment = TitleAlignments.Right)]
        public int rightTitle;

        [Title("Parameter: TitleAlignment (Split)", "Subtitle to the right",
            TitleAlignment = TitleAlignments.Split)]
        public int splitTitle;

        [Title("Parameter: HorizontalLine (False)", HorizontalLine = false)]
        public int noLineTitle;

        [Title("Parameter: Bold (False)", Bold = false)]
        public int notBoldTitle;

        public override void AesirInspectorReset()
        {
            defaultTitle = 0;
            subtitleTitle = 0;
            centeredTitle = 0;
            rightTitle = 0;
            splitTitle = 0;
            noLineTitle = 0;
            notBoldTitle = 0;
        }
    }
}
