using NUnit.Framework;
using TJGenerators.Utils;

namespace TJGenerators.Editor.Tests
{
    public class AudioAssetPathUtilityTests
    {
        [Test]
        public void NormalizeImportedAudioFileExtension_TrimsAndLowercases()
        {
            Assert.AreEqual("wav", TJGeneratorsAudioAssetPathUtility.NormalizeImportedAudioFileExtension(null));
            Assert.AreEqual("wav", TJGeneratorsAudioAssetPathUtility.NormalizeImportedAudioFileExtension(""));
            Assert.AreEqual("mp3", TJGeneratorsAudioAssetPathUtility.NormalizeImportedAudioFileExtension(".MP3"));
            Assert.AreEqual("ogg", TJGeneratorsAudioAssetPathUtility.NormalizeImportedAudioFileExtension(" OGG "));
        }

        [Test]
        public void NormalizeImportedAudioFileExtension_MapsFalEnums()
        {
            Assert.AreEqual("mp3", TJGeneratorsAudioAssetPathUtility.NormalizeImportedAudioFileExtension("mp3_44100_128"));
            Assert.AreEqual("mp3", TJGeneratorsAudioAssetPathUtility.NormalizeImportedAudioFileExtension("mp3_22050_32"));
            Assert.AreEqual("wav", TJGeneratorsAudioAssetPathUtility.NormalizeImportedAudioFileExtension("pcm_44100"));
            Assert.AreEqual("ogg", TJGeneratorsAudioAssetPathUtility.NormalizeImportedAudioFileExtension("opus_48000_128"));
        }

        [Test]
        public void GenerateUniquePlaceholderWavPath_Empty_UsesDefaultStem()
        {
            var path = TJGeneratorsAudioAssetPathUtility.GenerateUniquePlaceholderWavPath(null);
            Assert.IsTrue(path.EndsWith(".wav"), path);
            StringAssert.Contains("Assets/", path.Replace('\\', '/'));
        }
    }
}
