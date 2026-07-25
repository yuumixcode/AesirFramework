using System;
using System.Reflection;

namespace Runestone.AesirInspector
{
    /// <summary>
    /// 事件数据接口，继承自 IDerivedMemberData
    /// </summary>
    [Summary("事件数据接口，继承自 IDerivedMemberData")]
    public interface IEventData : IDerivedMemberData
    {
        /// <summary>
        /// 事件类型
        /// </summary>
        [Summary("事件类型")]
        Type EventType { get; }

        /// <summary>
        /// 事件类型名称
        /// </summary>
        [Summary("事件类型名称")]
        string EventTypeName { get; }

        /// <summary>
        /// 事件类型的完整名称，包括命名空间
        /// </summary>
        [Summary("事件类型的完整名称，包括命名空间")]
        string EventTypeFullName { get; }
    }

    /// <summary>
    /// 事件解析数据类，用于存储事件的解析数据
    /// </summary>
    [Summary("事件解析数据类，用于存储事件的解析数据")]
    [Serializable]
    public class EventData : MemberData, IEventData
    {
        /// <summary>
        /// 创建事件解析数据实例
        /// </summary>
        public EventData(EventInfo eventInfo, IAttributeFilter filter = null) : base(eventInfo, filter)
        {
            EventType = eventInfo.EventHandlerType;
            EventTypeName = EventType.GetReadableTypeName();
            EventTypeFullName = EventType.GetReadableTypeName(true);
            IsStatic = eventInfo.GetAddMethod(true).IsStatic;
            MemberType = eventInfo.MemberType;
            MemberTypeName = MemberType.ToString();
            AccessModifier = eventInfo.GetEventAccessModifierType();
            AccessModifierName = AccessModifier.ConvertToString();
            Signature = GetEventSignature(AccessModifierName, IsStatic, EventTypeName, Name);
            FullDeclarationWithAttributes = AttributesDeclaration + Signature;
        }

        static string GetEventSignature(string accessModifier,
            bool isStatic,
            string eventType,
            string eventName)
        {
            var signature = accessModifier + " ";
            if (isStatic)
            {
                signature += "static ";
            }

            signature += $"event {eventType} {eventName};";
            return signature;
        }

        #region IDerivedMemberData

        /// <summary>
        /// 是否为静态事件
        /// </summary>
        [Summary("是否为静态事件")]
        public bool IsStatic { get; }

        /// <summary>
        /// 成员类型
        /// </summary>
        [Summary("成员类型")]
        public MemberTypes MemberType { get; }

        /// <summary>
        /// 成员类型名称
        /// </summary>
        [Summary("成员类型名称")]
        public string MemberTypeName { get; }

        /// <summary>
        /// 访问修饰符类型
        /// </summary>
        [Summary("访问修饰符类型")]
        public AccessModifierType AccessModifier { get; }

        /// <summary>
        /// 访问修饰符名称
        /// </summary>
        [Summary("访问修饰符名称")]
        public string AccessModifierName { get; }

        /// <summary>
        /// 事件的完整签名
        /// </summary>
        [Summary("事件的完整签名")]
        public string Signature { get; private set; }

        /// <summary>
        /// 包含特性和签名的完整事件声明
        /// </summary>

        [Summary("包含特性和签名的完整事件声明")]
        public string FullDeclarationWithAttributes { get; }

        #endregion

        #region IEventData

        /// <summary>
        /// 事件类型
        /// </summary>
        [Summary("事件类型")]
        public Type EventType { get; }

        /// <summary>
        /// 事件类型名称
        /// </summary>
        [Summary("事件类型名称")]
        public string EventTypeName { get; }

        /// <summary>
        /// 事件类型的完整名称，包括命名空间
        /// </summary>
        [Summary("事件类型的完整名称，包括命名空间")]
        public string EventTypeFullName { get; }

        #endregion
    }
}
