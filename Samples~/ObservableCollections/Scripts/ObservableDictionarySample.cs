#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
using System.Linq;
using UnityEngine;

namespace Runestone.AesirArchitecture.Samples.ObservableCollections
{
    /// <summary>
    /// ObservableDictionary&lt;TKey, TValue&gt; 演示组件（角色属性表场景）。
    /// <para>订阅 Added / Removed / Updated / Cleared 四类变更事件，通过 ContextMenu 触发读写，在 Console 观察事件日志。</para>
    /// <para>索引器语义：为不存在的键赋值触发 Added；为已有键赋新值触发 Updated（参数含旧值）；赋相同值不触发事件。</para>
    /// <para>Add 语义：键已存在时抛 <see cref="System.ArgumentException" />（fail-fast），示例中重复触发该菜单可直接观察到异常。</para>
    /// </summary>
    public sealed class ObservableDictionarySample : MonoBehaviour
    {
        readonly ObservableDictionary<string, int> _stats = new ObservableDictionary<string, int>();

        AutoRemoveListenerHandle _addedSub, _removedSub, _updatedSub, _clearedSub;

        void Start()
        {
            _stats["攻击力"] = 10;
            _stats["生命上限"] = 100;
            _stats["暴击率(%)"] = 5;
        }

        void OnEnable()
        {
            _addedSub = _stats.AddAddedListener(pair =>
                Debug.Log($"[Dictionary] Added → [{pair.Key}] = {pair.Value}（当前 {_stats.Count} 项）"));
            _removedSub = _stats.AddRemovedListener(pair =>
                Debug.Log($"[Dictionary] Removed → [{pair.Key}] = {pair.Value}（当前 {_stats.Count} 项）"));
            _updatedSub = _stats.AddUpdatedListener(evt =>
                Debug.Log($"[Dictionary] Updated → [{evt.Key}]：{evt.OldValue} → {evt.NewValue}"));
            _clearedSub = _stats.AddClearedListener(
                () => Debug.Log("[Dictionary] Cleared → 属性表已清空"));
        }

        void OnDisable()
        {
            _addedSub.Dispose();
            _removedSub.Dispose();
            _updatedSub.Dispose();
            _clearedSub.Dispose();
        }

        [ContextMenu("索引器：新增键（触发 Added）")]
        void AddKeyViaIndexer()
        {
            _stats["防御力"] = 5;
            DumpStats();
        }

        [ContextMenu("索引器：更新已有键（触发 Updated）")]
        void UpdateExistingKey()
        {
            if (!_stats.ContainsKey("攻击力"))
            {
                Debug.LogWarning("[Dictionary] 键「攻击力」不存在，先运行 Play Mode 或「索引器：新增键」");
                return;
            }

            _stats["攻击力"] += 5;
            DumpStats();
        }

        [ContextMenu("索引器：赋相同值（不触发事件）")]
        void AssignSameValue()
        {
            if (!_stats.TryGetValue("攻击力", out int attack))
            {
                Debug.LogWarning("[Dictionary] 键「攻击力」不存在，先运行 Play Mode 或「索引器：新增键」");
                return;
            }

            _stats["攻击力"] = attack;
            Debug.Log("[Dictionary] 索引器赋相同值 → 值未变化，事件未触发");
        }

        [ContextMenu("Add：新增键（重复键抛异常）")]
        void AddKey()
        {
            _stats.Add("敏捷", 7);
            DumpStats();
        }

        [ContextMenu("Remove：移除键")]
        void RemoveKey()
        {
            bool removed = _stats.Remove("生命上限");
            Debug.Log($"[Dictionary] Remove(\"生命上限\") → 返回 {removed}");
            DumpStats();
        }

        [ContextMenu("TryGetValue：查询键")]
        void QueryKey()
        {
            if (_stats.TryGetValue("攻击力", out int value))
            {
                Debug.Log($"[Dictionary] TryGetValue(\"攻击力\") → {value}");
            }
            else
            {
                Debug.Log("[Dictionary] TryGetValue(\"攻击力\") → 键不存在");
            }
        }

        [ContextMenu("Clear：清空属性表")]
        void ClearStats()
        {
            _stats.Clear();
            Debug.Log($"[Dictionary] Clear 完成，当前属性 {_stats.Count} 项");
        }

        void DumpStats() => Debug.Log(
            $"[Dictionary] 当前属性：[{string.Join(", ", _stats.Select(pair => $"{pair.Key}={pair.Value}"))}]");
    }
}
#endif
