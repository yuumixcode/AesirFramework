using Sirenix.OdinInspector;

namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 带描述信息的 ScriptableObject 基类，使用 Odin 增强 Inspector 展示
    /// </summary>
    public abstract class DescriptionSO : AesirScriptableObject
    {
        /// <summary>
        /// 资产的详细描述信息
        /// </summary>
        [MultiLineProperty]
        [HideLabel]
        [Title("资产描述信息")]
        public string description;
    }
}
