using System.Reflection;

namespace Runestone.AesirInspector
{
    /// <summary>
    /// 派生成员数据接口，不同的派生类有不同的表现形式，MemberData 无法直接准确获取的信息，需要派生类自己实现
    /// </summary>
    public interface IDerivedMemberData
    {
        /// <summary>
        /// 是否为静态
        /// </summary>
        bool IsStatic { get; }

        /// <summary>
        /// 成员类型（字段、属性、方法等）
        /// </summary>
        MemberTypes MemberType { get; }

        /// <summary>
        /// MemberType 类型的字符串表示形式
        /// </summary>
        string MemberTypeName { get; }

        /// <summary>
        /// 访问修饰符类型，表示成员的访问级别（public、private、protected等）
        /// </summary>
        AccessModifierType AccessModifier { get; }

        /// <summary>
        /// 访问修饰符的字符串表示形式
        /// </summary>
        string AccessModifierName { get; }

        /// <summary>
        /// 成员签名字符串，包含访问修饰符、字段修饰符（static/readonly/const）、类型名称和成员名称
        /// </summary>
        string Signature { get; }

        /// <summary>
        /// 包含特性的完整声明字符串，包含特性声明和成员签名
        /// </summary>
        string FullDeclarationWithAttributes { get; }
    }
}
