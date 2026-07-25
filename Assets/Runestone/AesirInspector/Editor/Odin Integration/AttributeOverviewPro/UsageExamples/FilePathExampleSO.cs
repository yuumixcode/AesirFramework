using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    [AesirExample]
    public class FilePathExampleSO : AttributeExampleSO<FilePathExampleSO>
    {
        [Title("No Parameters")]
        [FilePath]
        public string path;

        [Title("Parameter: Extensions")]
        [FilePath(Extensions = "cs, lua")]
        public string scriptPath;

        [Title("Parameter: AbsolutePath")]
        [FilePath(AbsolutePath = true)]
        public string absolutePath;

        [Title("Parameter: ParentFolder")]
        [FilePath(ParentFolder = "Assets/Runestone")]
        public string relativePath;

        [Title("Parameter: RequireExistingPath")]
        [FilePath(RequireExistingPath = true)]
        public string existingPath;

        [Title("Parameter: IncludeFileExtension (False)")]
        [FilePath(IncludeFileExtension = false)]
        public string noExtensionPath;

        public override void AesirInspectorReset()
        {
            path = "";
            scriptPath = "";
            absolutePath = "";
            relativePath = "";
            existingPath = "";
            noExtensionPath = "";
        }
    }
}
