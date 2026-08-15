using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// 存储 Type 的资源文件，提供给脚本文档生成工具复用，用户无需每次重新选择 Type
    /// </summary>
    public class TypesCacheSO : SerializedScriptableObject
    {
        /// <summary>
        /// 存储 Type 的列表
        /// </summary>
        public List<Type> Types;
    }
}
