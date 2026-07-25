using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class DetailInfoBoxBilingualExampleSO : AttributeExampleSO<DetailInfoBoxBilingualExampleSO>
    {
        [Title("Bilingual DetailInfoBox")]
        [DetailInfoBox("这是中文消息", "This is English message", "这是中文详细内容", "This is English detailed content")]
        public int bilingualExample;

        [Title("Dynamic Bilingual Details")]
        [DetailInfoBox("动态详细内容", "Dynamic Details", "$dynamicDetailsChinese", "$dynamicDetailsEnglish")]
        public int dynamicExample;

        public string dynamicDetailsChinese = "来自字段的中文详细内容";
        public string dynamicDetailsEnglish = "Detailed content from field";

        public override void AesirInspectorReset()
        {
            bilingualExample = 0;
            dynamicExample = 0;
            dynamicDetailsChinese = "来自字段的中文详细内容";
            dynamicDetailsEnglish = "Detailed content from field";
        }
    }
}
