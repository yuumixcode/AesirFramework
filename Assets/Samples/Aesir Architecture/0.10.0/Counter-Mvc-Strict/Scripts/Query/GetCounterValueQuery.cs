namespace Runestone.AesirArchitecture.Samples
{
    /// <summary>
    /// MVC-3 严格档示例 —— 查询当前计数值。
    /// </summary>
    /// <remarks>
    /// 严格档：View 不持有 Model，读取值经 Query 拉取——
    /// 牺牲响应式推送（订阅刷新），换来 View 对 Model 的零持有。
    /// <para>对照：标准档（Counter-MVC）Controller 持有 Model + 订阅刷新。</para>
    /// </remarks>
    /// <seealso cref="Runestone.AesirArchitecture.AbstractQuery{TResult}"/>
    public class GetCounterValueQuery : AbstractQuery<int>
    {
        /// <summary>
        /// 执行查询：从 Context 获取 Model 并返回当前计数值。
        /// </summary>
        protected override int OnExecute()
        {
            return this.GetModel<ISampleMvcStrictCounterModel>().Count.Value;
        }
    }
}
