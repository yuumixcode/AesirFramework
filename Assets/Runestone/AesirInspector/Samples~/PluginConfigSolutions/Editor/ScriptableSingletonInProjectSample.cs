using System.IO;
using Runestone.AesirInspector.OdinIntegration;
using Sirenix.OdinInspector;
using UnityEditor;
using FilePathAttribute = UnityEditor.FilePathAttribute;

namespace Runestone.AesirInspector.Samples.PluginConfig.Editor
{
    [Summary("ScriptableSingleton 的示例，资源文件路径枚举值为 ProjectFolder。")]
    [FilePath(ProjectFilePath + "/PluginConfigSolutions/ScriptableSingletonInProjectSample.asset",
        FilePathAttribute.Location.ProjectFolder)]
    public class ScriptableSingletonInProjectSample : ScriptableSingleton<ScriptableSingletonInProjectSample>
    {
        const string ProjectFilePath = "Aesir Inspector/Samples";

        string _configName = "Project Config";

        [BilingualTitle("可配置数据", "Configurable Data")]
        [Summary("自定义 String 类型配置")]
        [ShowInInspector]
        public string ConfigName
        {
            get => _configName;
            set
            {
                if (_configName == value)
                {
                    return;
                }

                _configName = value;
                // 必须调用 Save 方法写入磁盘。
                // true 表示资源是以文本格式存储，false 表示以二进制格式存储
                Save(true);
            }
        }

        [BilingualTitle("资产文件相对路径", "Asset Relative File Path")]
        [PropertyOrder(120)]
        [HideLabel]
        [ShowInInspector]
        public string RelativeFilePath => GetFilePath();

        [BilingualTitle("资产文件绝对路径", "Asset Absolute File Path")]
        [PropertyOrder(130)]
        [HideLabel]
        [ShowInInspector]
        public string AbsoluteFilePath => Path.GetFullPath(GetFilePath());

        public string AbsoluteFolderPath => Path.GetDirectoryName(AbsoluteFilePath);

        [Summary("重置配置")]
        [BilingualButton("重置配置", "Reset Config")]
        public void ResetConfig()
        {
            ConfigName = "Project Config";
            Save(true);
        }

        [BilingualTitle("调试", "Debug")]
        [Summary("打开资产所在文件夹")]
        [BilingualButton("打开资产所在文件夹", "Open Asset Folder")]
        [PropertyOrder(100)]
        public void OpenFolder()
        {
            if (!Directory.Exists(AbsoluteFolderPath))
            {
                Directory.CreateDirectory(AbsoluteFolderPath);
            }

            if (!File.Exists(AbsoluteFilePath))
            {
                // instance 是公共静态单例，其他类通过 ScriptableSingletonInProjectSample.instance 调用。
                _ = instance;
                Save(true);
            }

            EditorUtility.RevealInFinder(GetFilePath());
        }
    }
}
