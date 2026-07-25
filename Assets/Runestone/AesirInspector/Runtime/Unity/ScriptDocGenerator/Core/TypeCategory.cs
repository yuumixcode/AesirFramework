namespace Runestone.AesirInspector
{
    /// <summary>
    /// 类型种类枚举
    /// </summary>
    [Summary("类型种类枚举")]
    public enum TypeCategory
    {
        /// <summary>
        /// 类
        /// </summary>
        [Summary("类")]
        Class,

        /// <summary>
        /// 结构体
        /// </summary>
        [Summary("结构体")]
        Struct,

        /// <summary>
        /// 接口
        /// </summary>
        [Summary("接口")]
        Interface,

        /// <summary>
        /// 枚举
        /// </summary>
        [Summary("枚举")]
        Enum,

        /// <summary>
        /// 委托
        /// </summary>
        [Summary("委托")]
        Delegate,

        /// <summary>
        /// 记录类型
        /// </summary>
        [Summary("记录类型")]
        Record,

        /// <summary>
        /// 未知类型
        /// </summary>
        [Summary("未知类型")]
        Unknown
    }
}
