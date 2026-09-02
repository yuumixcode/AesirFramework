namespace Runestone.AesirArchitecture
{
    /// <summary>
    /// 集合添加事件参数。包含被添加项及其索引。
    /// </summary>
    /// <typeparam name="T">集合元素类型</typeparam>
    /// <remarks>只读结构体，事件回调时零分配传递变更细节。</remarks>
    public readonly struct CollectionAddEventArgs<T>
    {
        /// <summary>
        /// 被添加项在列表中的索引。
        /// </summary>
        public readonly int Index;

        /// <summary>
        /// 被添加的项。
        /// </summary>
        public readonly T Item;

        /// <summary>
        /// 构造添加事件参数。
        /// </summary>
        /// <param name="index">被添加项的索引</param>
        /// <param name="item">被添加的项</param>
        public CollectionAddEventArgs(int index, T item)
        {
            Index = index;
            Item = item;
        }
    }

    /// <summary>
    /// 集合移除事件参数。包含被移除项及其移除前所在索引。
    /// </summary>
    /// <typeparam name="T">集合元素类型</typeparam>
    /// <remarks>只读结构体，事件回调时零分配传递变更细节。</remarks>
    public readonly struct CollectionRemoveEventArgs<T>
    {
        /// <summary>
        /// 被移除项移除前在列表中的索引。
        /// </summary>
        public readonly int Index;

        /// <summary>
        /// 被移除的项。
        /// </summary>
        public readonly T Item;

        /// <summary>
        /// 构造移除事件参数。
        /// </summary>
        /// <param name="index">被移除项移除前的索引</param>
        /// <param name="item">被移除的项</param>
        public CollectionRemoveEventArgs(int index, T item)
        {
            Index = index;
            Item = item;
        }
    }

    /// <summary>
    /// 集合替换事件参数。包含替换位置索引、旧项与新项。
    /// </summary>
    /// <typeparam name="T">集合元素类型</typeparam>
    /// <remarks>只读结构体，事件回调时零分配传递变更细节。</remarks>
    public readonly struct CollectionReplaceEventArgs<T>
    {
        /// <summary>
        /// 发生替换的索引。
        /// </summary>
        public readonly int Index;

        /// <summary>
        /// 替换前的旧项。
        /// </summary>
        public readonly T OldItem;

        /// <summary>
        /// 替换后的新项。
        /// </summary>
        public readonly T NewItem;

        /// <summary>
        /// 构造替换事件参数。
        /// </summary>
        /// <param name="index">发生替换的索引</param>
        /// <param name="oldItem">替换前的旧项</param>
        /// <param name="newItem">替换后的新项</param>
        public CollectionReplaceEventArgs(int index, T oldItem, T newItem)
        {
            Index = index;
            OldItem = oldItem;
            NewItem = newItem;
        }
    }

    /// <summary>
    /// 字典更新事件参数。包含键、旧值与新值。
    /// </summary>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <typeparam name="TValue">值类型</typeparam>
    /// <remarks>
    /// 仅在通过索引器为已存在的键赋新值时触发；新增键触发的是 Added 事件（参数为 <see cref="System.Collections.Generic.KeyValuePair{TKey, TValue}" />）。
    /// </remarks>
    public readonly struct DictionaryUpdateEventArgs<TKey, TValue>
    {
        /// <summary>
        /// 被更新的键。
        /// </summary>
        public readonly TKey Key;

        /// <summary>
        /// 更新前的旧值。
        /// </summary>
        public readonly TValue OldValue;

        /// <summary>
        /// 更新后的新值。
        /// </summary>
        public readonly TValue NewValue;

        /// <summary>
        /// 构造更新事件参数。
        /// </summary>
        /// <param name="key">被更新的键</param>
        /// <param name="oldValue">更新前的旧值</param>
        /// <param name="newValue">更新后的新值</param>
        public DictionaryUpdateEventArgs(TKey key, TValue oldValue, TValue newValue)
        {
            Key = key;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }
}
