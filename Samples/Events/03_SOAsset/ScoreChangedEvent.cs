#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
namespace Runestone.AesirModules.Samples.Events.SOAsset
{
    /// <summary>
    /// 分数变更事件。<see cref="AesirEventArgsSO" /> 资产在 Inspector 中经
    /// SubclassSelector 下拉选择此类型作为事件参数，并配置 <see cref="NewScore" /> 字段。
    /// </summary>
    public class ScoreChangedEvent : AesirEventArgs
    {
        /// <summary>
        /// 新分数（在 SO 资产 Inspector 中配置）。
        /// </summary>
        public int NewScore;
    }
}
#endif
