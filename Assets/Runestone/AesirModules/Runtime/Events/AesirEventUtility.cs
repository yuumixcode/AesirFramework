using UnityEngine;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 事件模块静态工具方法。
    /// </summary>
    public static class AesirEventUtility
    {
        /// <summary>
        /// 检测对象是否为 Unity 假 null（已销毁但引用未置空）。
        /// </summary>
        /// <param name="obj">待检测的对象。</param>
        /// <returns>如果对象为 null 或已销毁的 Unity 对象，返回 true。</returns>
        public static bool IsObjectUnityNull(object obj) =>
            obj == null || (obj is Object unityObj && unityObj == null);

        /// <summary>
        /// 获取事件的绑定键（事件类型的 AssemblyQualifiedName）。
        /// </summary>
        /// <param name="eventArgs">事件参数实例。</param>
        /// <returns>事件类型的 AssemblyQualifiedName。</returns>
        public static string GetEventBindingKey(AesirEventArgs eventArgs) =>
            eventArgs.GetType().AssemblyQualifiedName;

        /// <summary>
        /// 获取事件类型的绑定键（事件类型的 AssemblyQualifiedName）。
        /// </summary>
        /// <typeparam name="TEventArgs">事件参数类型。</typeparam>
        /// <returns>事件类型的 AssemblyQualifiedName。</returns>
        public static string GetEventBindingKey<TEventArgs>() where TEventArgs : AesirEventArgs =>
            typeof(TEventArgs).AssemblyQualifiedName;

        /// <summary>
        /// 获取事件类型的简短名称。
        /// </summary>
        /// <typeparam name="TEventArgs">事件参数类型。</typeparam>
        /// <returns>事件类型的 Type.Name。</returns>
        public static string GetEventName<TEventArgs>() where TEventArgs : AesirEventArgs =>
            typeof(TEventArgs).Name;
    }
}
