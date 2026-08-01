using System.Reflection;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// 示例实现 IAssemblyFilter 接口，用于过滤名称以 Runestone.AesirInspector 开头的程序集。
    /// </summary>
    public class FilterOutAesirInspectorAssembly : IAssemblyFilter
    {
        #region IAssemblyFilter Members

        /// <summary>
        /// 判断指定程序集是否应该被过滤掉
        /// </summary>
        public bool ShouldFilterOut(Assembly assembly) =>
            assembly.FullName.StartsWith("Runestone.AesirInspector");

        #endregion
    }
}
