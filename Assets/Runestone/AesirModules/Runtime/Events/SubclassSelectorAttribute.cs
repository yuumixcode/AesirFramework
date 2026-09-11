using System;
using UnityEngine;

namespace Runestone.AesirModules
{
    /// <summary>
    /// 标记 <c>[SerializeReference]</c> 字段在 Inspector 中使用子类下拉选择器。
    /// <para>
    /// 点击下拉按钮弹出字段声明类型的全部可选子类（按命名空间分组），
    /// 选择后自动创建实例并显示其序列化字段。供 <see cref="AesirEventArgsSO" />、
    /// <see cref="UnityEventOnAesirEvent" /> 等需要 Inspector 配置
    /// <see cref="AesirEventArgs" /> 具体子类的场景使用。
    /// </para>
    /// <para>
    /// 候选类型要求：非抽象、非泛型定义、标记 <c>[Serializable]</c>、
    /// 非 <see cref="UnityEngine.Object" /> 派生；标有
    /// <see cref="ExcludeSubclassSelectorAttribute" /> 的类型不出现在下拉中。
    /// </para>
    /// </summary>
    public class SubclassSelectorAttribute : PropertyAttribute { }

    /// <summary>
    /// 排除特性。标记不想出现在 <see cref="SubclassSelectorAttribute" /> 下拉中的类型
    /// （如抽象中间层、仅供程序内部使用的事件参数）。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    public class ExcludeSubclassSelectorAttribute : Attribute { }
}
