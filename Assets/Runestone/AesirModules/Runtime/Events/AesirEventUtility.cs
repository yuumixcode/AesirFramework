using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 事件模块静态工具方法。
    /// </summary>
    public static class AesirEventUtility
    {
        /// <summary>
        /// 事件类型 → 绑定键缓存。
        /// <para>
        /// 为什么需要缓存：<see cref="Type.AssemblyQualifiedName" /> 每次访问都会重新拼接
        /// 并分配一个新字符串（约 100-200 字节），而绑定键查询位于事件分发热路径上——
        /// 不缓存时每次发布都产生一次字符串分配。缓存后稳态发布零分配。
        /// </para>
        /// <para>
        /// 框架约定主线程使用，不加锁。
        /// </para>
        /// </summary>
        static readonly Dictionary<Type, string> KeyCache = new Dictionary<Type, string>();

        /// <summary>
        /// 检测对象是否为 Unity 假 null（已销毁但引用未置空）。
        /// </summary>
        /// <param name="obj">待检测的对象。</param>
        /// <returns>如果对象为 null 或已销毁的 Unity 对象，返回 true。</returns>
        public static bool IsObjectUnityNull(object obj) =>
            obj == null || (obj is Object unityObj && unityObj == null);

        /// <summary>
        /// 获取事件的绑定键（事件类型的 AssemblyQualifiedName）。
        /// 按事件类型缓存，同类型重复发布复用同一字符串实例（热路径零分配）。
        /// </summary>
        /// <param name="eventArgs">事件参数实例。</param>
        /// <returns>事件类型的 AssemblyQualifiedName。</returns>
        public static string GetEventBindingKey(AesirEventArgs eventArgs) =>
            GetOrCreateBindingKey(eventArgs.GetType());

        /// <summary>
        /// 获取事件类型的绑定键（事件类型的 AssemblyQualifiedName）。
        /// 按事件类型缓存，同类型重复发布复用同一字符串实例（热路径零分配）。
        /// </summary>
        /// <typeparam name="TEventArgs">事件参数类型。</typeparam>
        /// <returns>事件类型的 AssemblyQualifiedName。</returns>
        public static string GetEventBindingKey<TEventArgs>() where TEventArgs : AesirEventArgs =>
            GetOrCreateBindingKey(typeof(TEventArgs));

        /// <summary>
        /// 查询或创建事件类型的绑定键。
        /// </summary>
        static string GetOrCreateBindingKey(Type eventType)
        {
            if (!KeyCache.TryGetValue(eventType, out var key))
            {
                key = eventType.AssemblyQualifiedName;
                KeyCache[eventType] = key;
            }

            return key;
        }

        /// <summary>
        /// 获取事件类型的简短名称。
        /// </summary>
        /// <typeparam name="TEventArgs">事件参数类型。</typeparam>
        /// <returns>事件类型的 Type.Name。</returns>
        public static string GetEventName<TEventArgs>() where TEventArgs : AesirEventArgs =>
            typeof(TEventArgs).Name;

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
