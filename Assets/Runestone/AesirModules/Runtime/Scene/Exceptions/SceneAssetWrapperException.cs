using System;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 所有 <see cref="SceneAssetWrapper" /> 相关异常的基类，便于调用方统一捕获。
    /// </summary>
    public class SceneAssetWrapperException : InvalidOperationException
    {
        /// <summary>使用默认错误的描述初始化。</summary>
        public SceneAssetWrapperException()
        {
        }

        /// <summary>使用指定错误描述初始化。</summary>
        public SceneAssetWrapperException(string message) : base(message)
        {
        }

        /// <summary>使用指定错误描述与内部异常初始化。</summary>
        public SceneAssetWrapperException(string message, Exception innerException) : base(message,
            innerException)
        {
        }
    }
}
