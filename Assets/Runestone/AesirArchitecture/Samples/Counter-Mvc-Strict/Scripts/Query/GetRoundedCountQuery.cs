using System;

namespace Runestone.AesirArchitecture.Samples.MvcStrict
{
    /// <summary>
    /// MVC-3 严格档示例 —— 查询十位四舍五入后的计数近似值。
    /// </summary>
    /// <remarks>
    /// Query 的定位：对原始值做<b>加工修饰</b>后返回（如四舍五入、去小数位），
    /// 不改变原有值。若只是返回原始值，直接用只读订阅即可，无需 Query。
    /// <para>本查询将计数值四舍五入到最近的 10 的倍数（43 → 40，45 → 50）。</para>
    /// <para>对照：标准档（Counter-Mvc-Standard）无 Query；快捷档（Counter-Mvc-Quick）无 Query。</para>
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.AbstractQuery{TResult}" />
    public class GetRoundedCountQuery : AbstractQuery<int>
    {
        /// <summary>
        /// 执行查询：读取当前计数并四舍五入到十位，返回加工值（原始值不变）。
        /// </summary>
        protected override int OnExecute()
        {
            var count = this.GetModel<ISampleMvcStrictCounterModel>().Count.Value;
            return (int)Math.Round(count / 10.0, MidpointRounding.AwayFromZero) * 10;
        }
    }
}
