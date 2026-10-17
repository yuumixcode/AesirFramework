using NUnit.Framework;
using TJGenerators.Config;

namespace TJGenerators.Editor.Tests
{
    public class SpriteSheetSequenceImageOrderHintTests
    {
        [Test]
        public void AppendToPrompt_ZeroImages_ReturnsOriginal()
        {
            Assert.AreEqual("hello", SpriteSheetSequenceImageOrderHint.AppendToPrompt("hello", 0, 0));
            Assert.AreEqual("", SpriteSheetSequenceImageOrderHint.AppendToPrompt(null, 0, 1));
        }

        [Test]
        public void AppendToPrompt_UserAndKnowledge_ContainsBothRanges()
        {
            var result = SpriteSheetSequenceImageOrderHint.AppendToPrompt("base", 4, 2);

            StringAssert.Contains("base", result);
            StringAssert.Contains("下标 0～1", result);
            StringAssert.Contains("下标 2～3", result);
            StringAssert.Contains("用户上传参考图", result);
            StringAssert.Contains("布局参考图", result);
        }
    }
}
