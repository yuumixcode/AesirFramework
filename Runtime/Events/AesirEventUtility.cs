using UnityEngine;
using Object = UnityEngine.Object;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 事件模块静态工具方法：Unity null 检测与 GameObject 解析，
    /// 供订阅者过滤器与诊断门面解析发布者/订阅者。
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
        /// 尝试将对象解析为 <see cref="GameObject" />。支持 GameObject 本身与任意
        /// <see cref="Component" />（含 MonoBehaviour）。供订阅者过滤器解析发布者/订阅者。
        /// </summary>
        /// <param name="obj">待解析对象。</param>
        /// <param name="gameObject">解析出的 GameObject；解析失败时为 null。</param>
        /// <returns>成功解析返回 true。已销毁的 Unity 对象（假 null）与纯 C# 对象均返回 false。</returns>
        public static bool TryGetGameObject(object obj, out GameObject gameObject)
        {
            // 显式做 Unity null 判断：已销毁对象的托管包装仍能通过 is 模式匹配
            if (obj is GameObject go && go != null)
            {
                gameObject = go;
                return true;
            }

            if (obj is Component component && component != null)
            {
                gameObject = component.gameObject;
                return true;
            }

            gameObject = null;
            return false;
        }
    }
}
