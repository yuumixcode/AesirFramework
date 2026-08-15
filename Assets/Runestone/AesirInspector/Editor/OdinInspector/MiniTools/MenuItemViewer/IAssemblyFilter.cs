using System.Reflection;

namespace Runestone.AesirInspector.OdinIntegration.Editor
{
    /// <summary>
    /// Assembly 过滤器接口，判断 Assembly 是否应该剔除
    /// </summary>
    public interface IAssemblyFilter
    {
        /// <summary>
        /// 判断指定程序集是否应该被过滤掉
        /// </summary>
        bool ShouldFilterOut(Assembly assembly);
    }
}
