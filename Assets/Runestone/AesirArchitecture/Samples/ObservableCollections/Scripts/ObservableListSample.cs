#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
using System.Collections.Specialized;
using UnityEngine;

namespace Runestone.AesirArchitecture.Samples.ObservableCollections
{
    /// <summary>
    /// ObservableList&lt;T&gt; 演示组件（背包场景）。
    /// <para>同时演示两轨通知：轻量事件（Added / Removed / Replaced / Cleared）与 <c>CollectionChanged</c>
    /// （含 Move / Sort / Reverse / 批量操作）。</para>
    /// <para>轻量事件语义：写操作完成后才触发事件——回调中读到的已是变更后的集合；无变更的操作不触发事件
    /// （Remove 不存在的元素、Clear 空列表、索引器赋相同值）。<c>CollectionChanged</c> 则每次写操作都通知。</para>
    /// </summary>
    public sealed class ObservableListSample : MonoBehaviour
    {
        readonly ObservableList<string> _inventory = new ObservableList<string>();

        AutoRemoveListenerHandle _addedSub, _removedSub, _replacedSub, _clearedSub;

        int _itemCounter;

        void Start()
        {
            _inventory.AddRange(new[] { "木剑", "皮甲", "红药水" });
        }

        void OnEnable()
        {
            _addedSub = _inventory.AddAddedListener(evt =>
                Debug.Log($"[List] Added → 索引 {evt.Index}：{evt.Item}（当前 {_inventory.Count} 件）"));
            _removedSub = _inventory.AddRemovedListener(evt =>
                Debug.Log($"[List] Removed → 原索引 {evt.Index}：{evt.Item}（当前 {_inventory.Count} 件）"));
            _replacedSub = _inventory.AddReplacedListener(evt =>
                Debug.Log($"[List] Replaced → 索引 {evt.Index}：{evt.OldItem} → {evt.NewItem}"));
            _clearedSub = _inventory.AddClearedListener(() => Debug.Log("[List] Cleared → 背包已清空"));

            _inventory.CollectionChanged += OnCollectionChanged;
        }

        void OnDisable()
        {
            _addedSub.Dispose();
            _removedSub.Dispose();
            _replacedSub.Dispose();
            _clearedSub.Dispose();

            _inventory.CollectionChanged -= OnCollectionChanged;
        }

        /// <summary>
        /// 第二轨通知：<c>CollectionChanged</c> 对齐 Cysharp.ObservableCollections 语义，
        /// 批量操作只通知一次、Sort / Reverse 走 Reset + SortOperation。
        /// </summary>
        void OnCollectionChanged(in NotifyCollectionChangedEventArgs<string> e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Debug.Log(e.IsSingleItem
                        ? $"[CollectionChanged] Add → {e.NewItem} @ {e.NewStartingIndex}"
                        : $"[CollectionChanged] Add × {e.NewItems.Length} @ {e.NewStartingIndex}（批量单次通知）");
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Debug.Log(e.IsSingleItem
                        ? $"[CollectionChanged] Remove → {e.OldItem} @ {e.OldStartingIndex}"
                        : $"[CollectionChanged] Remove × {e.OldItems.Length} @ {e.OldStartingIndex}（批量单次通知）");
                    break;
                case NotifyCollectionChangedAction.Replace:
                    Debug.Log($"[CollectionChanged] Replace → {e.OldItem} → {e.NewItem} @ {e.NewStartingIndex}");
                    break;
                case NotifyCollectionChangedAction.Move:
                    Debug.Log($"[CollectionChanged] Move → {e.NewStartingIndex} ← {e.OldStartingIndex}");
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Debug.Log(e.SortOperation.IsClear
                        ? "[CollectionChanged] Reset → Clear"
                        : e.SortOperation.IsReverse
                            ? $"[CollectionChanged] Reset → Reverse({e.SortOperation.Index}, {e.SortOperation.Count})"
                            : $"[CollectionChanged] Reset → Sort({e.SortOperation.Index}, {e.SortOperation.Count})");
                    break;
            }
        }

        [ContextMenu("Add：末尾添加道具")]
        void AddItem()
        {
            _inventory.Add($"道具{_itemCounter++}");
            DumpItems();
        }

        [ContextMenu("Insert：头部插入道具")]
        void InsertItem()
        {
            _inventory.Insert(0, $"道具{_itemCounter++}");
            DumpItems();
        }

        [ContextMenu("AddRange：批量添加（单次批量通知）")]
        void AddRangeItems()
        {
            _inventory.AddRange(new[] { $"道具{_itemCounter++}", $"道具{_itemCounter++}", $"道具{_itemCounter++}" });
            DumpItems();
        }

        [ContextMenu("InsertRange：索引 1 起插入 2 件")]
        void InsertRangeItems()
        {
            _inventory.InsertRange(1, new[] { $"道具{_itemCounter++}", $"道具{_itemCounter++}" });
            DumpItems();
        }

        [ContextMenu("RemoveRange：移除索引 1 起 2 件")]
        void RemoveRangeItems()
        {
            if (_inventory.Count < 3)
            {
                Debug.LogWarning("[List] 道具不足 3 件，先添加");
                return;
            }

            _inventory.RemoveRange(1, 2);
            DumpItems();
        }

        [ContextMenu("Move：把首个道具移到末尾")]
        void MoveFirstItem()
        {
            if (_inventory.Count < 2)
            {
                Debug.LogWarning("[List] 道具不足 2 件，先添加");
                return;
            }

            _inventory.Move(0, _inventory.Count - 1);
            DumpItems();
        }

        [ContextMenu("Sort：按名称排序")]
        void SortItems()
        {
            _inventory.Sort();
            DumpItems();
        }

        [ContextMenu("Reverse：反转")]
        void ReverseItems()
        {
            _inventory.Reverse();
            DumpItems();
        }

        [ContextMenu("替换首个道具（触发 Replaced）")]
        void ReplaceFirstItem()
        {
            if (_inventory.Count == 0)
            {
                Debug.LogWarning("[List] 背包为空，先运行 Play Mode 或「Add：末尾添加道具」");
                return;
            }

            _inventory[0] = "精炼石";
            DumpItems();
        }

        [ContextMenu("索引器赋相同值（轻量事件不触发，CollectionChanged 仍通知）")]
        void AssignSameValue()
        {
            if (_inventory.Count == 0)
            {
                Debug.LogWarning("[List] 背包为空，先运行 Play Mode 或「Add：末尾添加道具」");
                return;
            }

            _inventory[0] = _inventory[0];
            Debug.Log("[List] 索引器赋相同值 → 轻量事件未触发；CollectionChanged 按上游语义仍然通知");
        }

        [ContextMenu("RemoveAt：移除首个道具")]
        void RemoveFirstItem()
        {
            if (_inventory.Count == 0)
            {
                Debug.LogWarning("[List] 背包为空，先运行 Play Mode 或「Add：末尾添加道具」");
                return;
            }

            _inventory.RemoveAt(0);
            DumpItems();
        }

        [ContextMenu("Remove：移除不存在的元素（不触发事件）")]
        void RemoveMissingItem()
        {
            var removed = _inventory.Remove("不存在的道具");
            Debug.Log($"[List] Remove(\"不存在的道具\") → 返回 {removed}，事件未触发");
        }

        [ContextMenu("Clear：清空背包")]
        void ClearItems()
        {
            _inventory.Clear();
            Debug.Log($"[List] Clear 完成，当前背包 {_inventory.Count} 件");
        }

        void DumpItems()
        {
            Debug.Log($"[List] 当前背包：[{string.Join(", ", _inventory)}]");
        }
    }
}
#endif
