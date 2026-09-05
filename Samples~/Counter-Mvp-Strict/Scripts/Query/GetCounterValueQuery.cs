namespace Runestone.AesirArchitecture.Samples.MvpStrict
{
    /// <summary>
    /// MVP-3 严格档示例 —— 查询当前计数值。
    /// </summary>
    /// <remarks>
    /// 严格档：Presenter 读取值经 Query 拉取（替代 Model.Count.Value 直读）。
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.AbstractQuery{TResult}" />
    public class GetCounterValueQuery : AbstractQuery<int>
    {
        /// <summary>
        /// 执行查询：从 Context 获取 Model 并返回当前计数值。
        /// </summary>
        protected override int OnExecute() => this.GetModel<ISampleMvpStrictCounterModel>().Count.Value;
    }
}
