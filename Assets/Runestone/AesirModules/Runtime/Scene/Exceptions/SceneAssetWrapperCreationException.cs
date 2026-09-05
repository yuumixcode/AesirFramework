using System;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 通过工厂或构造方法创建 <see cref="SceneAssetWrapper" /> 时入参无效。
    /// </summary>
    /// <remarks>错误描述中包含具体的无效原因与修复建议。</remarks>
    public class SceneAssetWrapperCreationException : SceneAssetWrapperException
    {
        /// <summary>使用指定错误描述初始化。</summary>
        public SceneAssetWrapperCreationException(string message) : base(message)
        {
        }

        /// <summary>使用指定错误描述与内部异常初始化。</summary>
        public SceneAssetWrapperCreationException(string message, Exception innerException) : base(message,
            innerException)
        {
        }
    }
}
