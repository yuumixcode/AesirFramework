#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;

namespace Runestone.AesirArchitecture.Samples.ObservableCollections
{
    /// <summary>
    /// ObservableDictionary&lt;TKey, TValue&gt; 演示组件（角色属性表场景）。
    /// <para>单轨订阅 <c>AddListener</c>：载荷为 <see cref="CollectionChangedEventArgs{T}" />（<c>T</c> = 键值对），
    /// 按 <c>Action</c> 区分新增（Add）/ 移除（Remove）/ 值更新（Replace，旧值在 OldItem）/ 清空（Reset）。</para>
    /// <para>索引器语义：为不存在的键赋值触发 Add；为已有键赋新值触发 Replace；赋相同值不触发通知。</para>
    /// <para>Add 语义：键已存在时抛 <see cref="System.ArgumentException" />（fail-fast），示例中重复触发该菜单可直接观察到异常。</para>
    /// </summary>
    public sealed class ObservableDictionarySample : MonoBehaviour
    {
        readonly ObservableDictionary<string, int> _stats = new ObservableDictionary<string, int>();

        AutoRemoveListenerHandle _subscription;

        void Start()
        {
            _stats["攻击力"] = 10;
            _stats["生命上限"] = 100;
            _stats["暴击率(%)"] = 5;
        }

        void OnEnable()
        {
            _subscription = _stats.AddListener(OnStatsChanged);
        }

        void OnDisable()
        {
            _subscription.Dispose();
        }

        /// <summary>
        /// 单轨回调：按 <see cref="CollectionChangedEventArgs{T}.Action" /> 区分变更类型，
        /// 载荷 <c>NewItem</c> / <c>OldItem</c> 为变更后的 / 变更前的键值对（字典无索引，载荷索引固定 -1）。
        /// </summary>
        void OnStatsChanged(CollectionChangedEventArgs<KeyValuePair<string, int>> e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Debug.Log($"[Dictionary] Add → [{e.NewItem.Key}] = {e.NewItem.Value}（当前 {_stats.Count} 项）");
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Debug.Log($"[Dictionary] Remove → [{e.OldItem.Key}] = {e.OldItem.Value}（当前 {_stats.Count} 项）");
                    break;
                case NotifyCollectionChangedAction.Replace:
                    Debug.Log($"[Dictionary] Replace → [{e.NewItem.Key}]：{e.OldItem.Value} → {e.NewItem.Value}");
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Debug.Log("[Dictionary] Reset → 属性表已清空");
                    break;
            }
        }

        [ContextMenu("索引器：新增键（触发 Add）")]
        void AddKeyViaIndexer()
        {
            _stats["防御力"] = 5;
            DumpStats();
        }

        [ContextMenu("索引器：更新已有键（触发 Replace）")]
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

        [ContextMenu("索引器：赋相同值（不触发通知）")]
        void AssignSameValue()
        {
            if (!_stats.TryGetValue("攻击力", out var attack))
            {
                Debug.LogWarning("[Dictionary] 键「攻击力」不存在，先运行 Play Mode 或「索引器：新增键」");
                return;
            }

            _stats["攻击力"] = attack;
            Debug.Log("[Dictionary] 索引器赋相同值 → 值未变化，通知未触发");
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
            var removed = _stats.Remove("生命上限");
            Debug.Log($"[Dictionary] Remove(\"生命上限\") → 返回 {removed}");
            DumpStats();
        }

        [ContextMenu("TryGetValue：查询键")]
        void QueryKey()
        {
            if (_stats.TryGetValue("攻击力", out var value))
            {
                Debug.Log($"[Dictionary] TryGetValue(\"攻击力\") → {value}");
            }
            else
            {
                Debug.Log("[Dictionary] TryGetValue(\"攻击力\") → 键不存在");
            }
        }

        [ContextMenu("Clear：清空属性表（Reset 通知）")]
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
