using System.Linq;
using NUnit.Framework;
using Runestone.AesirModules.ScriptDocGenerator.Editor;

namespace Runestone.AesirModules.Tests.Editor.ScriptDocGenerator
{
    /// <summary>
    /// 增量生成 Front Matter 合并回归（双 Front Matter 缺陷）：
    /// Zensical 生成器自产 YAML 头后，旧文件头部不得再拼接一份；
    /// 不产 Front Matter 的生成器（如中文 API 生成器）仍保留旧文件头部。
    /// </summary>
    public class FrontMatterMergeTests
    {
        const string NewDocWithFrontMatter = "---\ntitle: T\ndescription: \"N.T 的 API 文档\"\n---\n\n# `T`\n";

        const string NewDocWithoutFrontMatter = "# `T`\n\n正文\n";

        static string[] Lines(string text) =>
            text.Split('\n').Select(line => line.TrimEnd('\r')).ToArray();

        [Test]
        public void NewContentWithFrontMatter_OldHeaderNotPrepended()
        {
            var existing = Lines("---\ntitle: 旧标题\n---\n\n# `T`\n");

            var merged = ScriptDocGeneratorUtility.MergeFrontMatterWhenMissing(existing, NewDocWithFrontMatter);

            Assert.That(merged, Is.EqualTo(NewDocWithFrontMatter), "新内容自带 Front Matter 时应以新生成的为准，不得拼出双重头部");
        }

        [Test]
        public void NewContentWithoutFrontMatter_OldHeaderPrepended()
        {
            var existing = Lines("---\ntitle: 旧标题\n---\n\n# `T`\n");

            var merged = ScriptDocGeneratorUtility.MergeFrontMatterWhenMissing(existing, NewDocWithoutFrontMatter);

            Assert.That(merged, Does.StartWith("---\ntitle: 旧标题\n---\n\n"), "不产 Front Matter 的生成器必须保留旧文件头部");
            Assert.That(merged, Does.EndWith(NewDocWithoutFrontMatter));
        }

        [Test]
        public void OldFileWithoutFrontMatter_ContentUnchanged()
        {
            var existing = Lines("# `T`\n\n正文\n");

            var merged = ScriptDocGeneratorUtility.MergeFrontMatterWhenMissing(existing, NewDocWithFrontMatter);

            Assert.That(merged, Is.EqualTo(NewDocWithFrontMatter));
        }

        [Test]
        public void UnclosedFrontMatter_NotMerged()
        {
            var existing = Lines("---\ntitle: 无闭合头部\n# `T`\n");

            var merged = ScriptDocGeneratorUtility.MergeFrontMatterWhenMissing(existing, NewDocWithoutFrontMatter);

            Assert.That(merged, Is.EqualTo(NewDocWithoutFrontMatter), "无闭合分隔符的旧文件不得整体拼回（回归既有行为）");
        }
    }
}
