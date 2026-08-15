using System;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 用于标记 BinderAssistant 可以选择的基类。
    /// <para>
    /// 预留特性：设计意图是将此特性应用到自定义类上后，
    /// 该类会出现在 <see cref="BinderAssistant"/> 的基类下拉列表中。
    /// 但当前 <see cref="BinderAssistant.GetBaseTypes"/> 仅返回 <see cref="UnityEngine.MonoBehaviour"/>，
    /// 尚未实现特性扫描逻辑。
    /// </para>
    /// </summary>
    public class BinderBaseTypeAttribute : Attribute { }
}
