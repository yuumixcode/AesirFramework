using System;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 标记一个 <see cref="AbstractContext{T}" /> 派生类为框架内部 Context（示例 / 测试等非用户工作流用途）。
    /// <para>
    /// 被标记的 Context 不会出现在用户工作流的 Context 选择器中
    /// （如 AesirModules Binder 的「Context 类型」下拉会跳过被标记的类型）。
    /// 框架自带的示例与测试 Context 均已标注。
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class InternalContextAttribute : Attribute { }
}
