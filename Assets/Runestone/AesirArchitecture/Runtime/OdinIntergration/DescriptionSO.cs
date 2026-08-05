using Sirenix.OdinInspector;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 带描述信息的 ScriptableObject 基类，使用 Odin 增强 Inspector 展示
    /// </summary>
    /// <remarks>
    /// 继承自 <see cref="AesirScriptableObject"/>，后者通过条件编译在引入 Odin Inspector 时启用 Odin 序列化支持，
    /// 使得即使在未安装 Odin 的环境中也能正常编译运行。
    /// 通过 <c>[MultiLineProperty]</c>、<c>[HideLabel]</c> 和 <c>[Title]</c> 的组合，
    /// 为子类提供多行文本编辑体验，使描述信息的填写更加直观便捷。
    /// </remarks>
    public abstract class DescriptionSO : AesirScriptableObject
    {
        /// <summary>
        /// 资产的详细描述信息
        /// </summary>
        /// <remarks>
        /// 使用 <c>[MultiLineProperty]</c> 提供多行文本输入区域，
        /// <c>[HideLabel]</c> 隐藏默认字段标签以获得更大的编辑空间，
        /// <c>[Title]</c> 添加自定义标题以区分其他字段，
        /// 三者共同在 Inspector 中呈现简洁的多行描述编辑器。
        /// </remarks>
        [MultiLineProperty]
        [HideLabel]
        [Title("资产描述信息")]
        public string description;
    }
}
