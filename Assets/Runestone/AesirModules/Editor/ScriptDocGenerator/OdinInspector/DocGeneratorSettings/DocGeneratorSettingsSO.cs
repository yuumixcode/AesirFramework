using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirModules.ScriptDocGenerator.Editor
{
    /// <summary>
    /// 文档生成器设置抽象类
    /// </summary>
    public abstract class DocGeneratorSettingsSO : ScriptableObject
    {
        /// <summary>
        /// 重置文档生成器设置
        /// </summary>
        [Button("重置为默认值")]
        public void ResetToDefault()
        {
            generateNamespaceFolder = true;
            customizeDocFileExtensionName = false;
            docFileExtensionName = ".md";
            generateIdentifier = true;
        }

        /// <summary>
        /// 通过 TypeData 实例对象，生成文档内容。注意：不要在此方法中添加增量生成标识符
        /// </summary>
        public abstract string GetGeneratedDocumentation(ITypeData data);

        #region Serialized Fields

        /// <summary>
        /// 是否按命名空间生成文件夹
        /// </summary>
        [LabelText("按命名空间生成文件夹")]
        public bool generateNamespaceFolder = true;

        /// <summary>
        /// 是否自定义文档扩展名
        /// </summary>
        [LabelText("自定义文档扩展名")]
        public bool customizeDocFileExtensionName;

        /// <summary>
        /// 设置的文档扩展名
        /// </summary>
        [EnableIf("customizeDocFileExtensionName")]
        [LabelText("文档扩展名")]
        public string docFileExtensionName = ".md";

        /// <summary>
        /// 是否生成增量标识符
        /// </summary>
        [LabelText("是否生成增量标识符")]
        public bool generateIdentifier = true;

        #endregion
    }
}
