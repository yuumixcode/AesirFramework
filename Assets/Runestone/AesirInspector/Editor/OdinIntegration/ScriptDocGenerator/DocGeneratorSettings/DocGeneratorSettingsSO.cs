using Sirenix.OdinInspector;
using UnityEngine;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// 文档生成器设置抽象类
    /// </summary>
    public abstract class DocGeneratorSettingsSO : ScriptableObject, IAesirInspectorReset
    {
        /// <summary>
        /// 重置文档生成器设置
        /// </summary>
        public void AesirInspectorReset()
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
        [BilingualText("按命名空间生成文件夹", "Generate Namespace Folder")]
        public bool generateNamespaceFolder = true;

        /// <summary>
        /// 是否自定义文档扩展名
        /// </summary>
        [BilingualText("自定义文档扩展名", "Customize Doc Extension Name")]
        public bool customizeDocFileExtensionName;

        /// <summary>
        /// 设置的文档扩展名
        /// </summary>
        [EnableIf("customizeDocFileExtensionName")]
        [BilingualText("文档扩展名", "Doc Extension Name")]
        public string docFileExtensionName = ".md";

        /// <summary>
        /// 是否生成增量标识符
        /// </summary>
        [BilingualText("是否生成增量标识符", "Generate Identifier")]
        public bool generateIdentifier = true;

        #endregion
    }
}
