#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
using UnityEngine;

namespace Runestone.AesirArchitecture.Samples.ObservableCollections
{
    /// <summary>
    /// ObservableList&lt;T&gt; 演示组件（背包场景）。
    /// <para>订阅 Added / Removed / Replaced / Cleared 四类变更事件，通过 ContextMenu 触发增删改，在 Console 观察事件日志。</para>
    /// <para>进入 Play Mode 后 Start 会经 AddRange 批量添加初始道具，三条 Added 事件立即可见。</para>
    /// <para>事件语义：写操作完成后才触发事件——回调中读到的已是变更后的集合；无变更的操作不触发事件（Remove 不存在的元素、Clear 空列表、索引器赋相同值）。</para>
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
            _clearedSub = _inventory.AddClearedListener(
                () => Debug.Log("[List] Cleared → 背包已清空"));
        }

        void OnDisable()
        {
            _addedSub.Dispose();
            _removedSub.Dispose();
            _replacedSub.Dispose();
            _clearedSub.Dispose();
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

        [ContextMenu("索引器赋相同值（不触发事件）")]
        void AssignSameValue()
        {
            if (_inventory.Count == 0)
            {
                Debug.LogWarning("[List] 背包为空，先运行 Play Mode 或「Add：末尾添加道具」");
                return;
            }

            _inventory[0] = _inventory[0];
            Debug.Log("[List] 索引器赋相同值 → 值未变化，事件未触发");
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
            bool removed = _inventory.Remove("不存在的道具");
            Debug.Log($"[List] Remove(\"不存在的道具\") → 返回 {removed}，事件未触发");
        }

        [ContextMenu("Clear：清空背包")]
        void ClearItems()
        {
            _inventory.Clear();
            Debug.Log($"[List] Clear 完成，当前背包 {_inventory.Count} 件");
        }

        void DumpItems() =>
            Debug.Log($"[List] 当前背包：[{string.Join(", ", _inventory)}]");
    }
}
#endif
