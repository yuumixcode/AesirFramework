using System;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 标记一个 <see cref="UnityEngine.MonoBehaviour" /> 派生类可作为 Binder 生成脚本的基类（用户自定义基类的扩展入口）。
    /// <para>
    /// Aesir 面板家族（<see cref="AesirBasePanel" />、<c>AesirBasePanelView&lt;T&gt;</c>、
    /// <c>AesirBasePanelViewController&lt;T&gt;</c>）由 BinderAssistant 内置预选，无需标注。
    /// </para>
    /// <para>
    /// 本特性与整个 Binder 功能一同收录于 Odin 程序集（<c>Runtime/UI/OdinInspector/Binder/</c>）：
    /// Binder 的类型选择器（组件/基类的 ValueDropdown）强依赖 Odin Inspector。
    /// 用户程序集引用 <c>Runestone.AesirModules.OdinInspector</c> 后即可标注
    /// （Assembly-CSharp 经 autoReferenced 自动引用；自定义 asmdef 需显式引用；
    /// 卸载 Odin Inspector 后相关标注代码需自行条件编译）。
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class BinderBaseTypeAttribute : Attribute { }
}
