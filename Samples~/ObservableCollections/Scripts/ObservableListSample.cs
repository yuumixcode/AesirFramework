#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
using System.Collections.Specialized;
using UnityEngine;

namespace Runestone.AesirArchitecture.Samples.ObservableCollections
{
    /// <summary>
    /// ObservableList&lt;T&gt; 演示组件（背包场景）。
    /// <para>单轨订阅 <c>AddListener</c>：所有变更（含 Move / Sort / Reverse / 批量操作）都经这一个回调通知，
    /// 靠 <see cref="CollectionChangedEventArgs{T}" /> 的 <c>Action</c> 区分变更类型。</para>
    /// <para>通知语义：写操作完成后才触发——回调中读到的已是变更后的集合；无变更的操作不触发通知
    /// （Remove 不存在的元素、Clear 空列表、索引器赋相同值）；批量操作逐项通知；
    /// Sort / Reverse / Clear 以 Reset 通知（监听方按"重建视图"处理）。</para>
    /// <para>订阅演示用 <see cref="AesirArchitecture.RemoveListenerExtensions.RemoveListenerWhenGameObjectOnDisable" />
    /// 把监听句柄绑定到 GameObject 生命周期——OnDisable 时自动移除，无需手动管理。</para>
    /// </summary>
    public sealed class ObservableListSample : MonoBehaviour
    {
        readonly ObservableList<string> _inventory = new ObservableList<string>();

        int _itemCounter;

        void Start()
        {
            _inventory.AddRange(new[] { "木剑", "皮甲", "红药水" });
        }

        void OnEnable()
        {
            // 句柄绑定到 GameObject OnDisable，禁用时自动移除监听（也可改存句柄并在 OnDisable 中 Dispose）
            _inventory.AddListener(OnInventoryChanged).RemoveListenerWhenGameObjectOnDisable(this);
        }

        /// <summary>
        /// 单轨回调：按 <see cref="CollectionChangedEventArgs{T}.Action" /> 区分变更类型，
        /// 每次变更携带单个变更项（批量操作会连续收到多条 Add / Remove）。
        /// </summary>
        void OnInventoryChanged(CollectionChangedEventArgs<string> e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Debug.Log($"[List] Add → {e.NewItem} @ {e.NewStartingIndex}（当前 {_inventory.Count} 件）");
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Debug.Log($"[List] Remove → {e.OldItem} @ {e.OldStartingIndex}（当前 {_inventory.Count} 件）");
                    break;
                case NotifyCollectionChangedAction.Replace:
                    Debug.Log($"[List] Replace → {e.OldItem} → {e.NewItem} @ {e.NewStartingIndex}");
                    break;
                case NotifyCollectionChangedAction.Move:
                    Debug.Log($"[List] Move → {e.NewItem}：{e.OldStartingIndex} → {e.NewStartingIndex}");
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Debug.Log("[List] Reset → 顺序或内容整体变化，按重建视图处理（当前 " + _inventory.Count + " 件）");
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

        [ContextMenu("AddRange：批量添加（逐项通知）")]
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

        [ContextMenu("Sort：按名称排序（Reset 通知）")]
        void SortItems()
        {
            _inventory.Sort();
            DumpItems();
        }

        [ContextMenu("Reverse：反转（Reset 通知）")]
        void ReverseItems()
        {
            _inventory.Reverse();
            DumpItems();
        }

        [ContextMenu("替换首个道具（触发 Replace）")]
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

        [ContextMenu("索引器赋相同值（不触发通知）")]
        void AssignSameValue()
        {
            if (_inventory.Count == 0)
            {
                Debug.LogWarning("[List] 背包为空，先运行 Play Mode 或「Add：末尾添加道具」");
                return;
            }

            _inventory[0] = _inventory[0];
            Debug.Log("[List] 索引器赋相同值 → 值未变化，通知未触发");
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

        [ContextMenu("Remove：移除不存在的元素（不触发通知）")]
        void RemoveMissingItem()
        {
            var removed = _inventory.Remove("不存在的道具");
            Debug.Log($"[List] Remove(\"不存在的道具\") → 返回 {removed}，通知未触发");
        }

        [ContextMenu("Clear：清空背包（Reset 通知）")]
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
