#if UNITY_EDITOR // 示例仅编辑器内参与编译（运行时程序集保证场景可挂载，#if 保证构建剔除）
using System.Collections.Specialized;
using UnityEngine;

namespace Runestone.AesirArchitecture.Samples.ObservableCollections
{
    /// <summary>
    /// ObservableHashSet&lt;T&gt; 演示组件（在线玩家场景）。
    /// <para>单轨订阅 <c>AddListener</c>：按 <see cref="CollectionChangedEventArgs{T}.Action" /> 区分
    /// 上线（Add）/ 下线（Remove）/ 清空（Reset）；集合无索引，载荷索引固定 -1。</para>
    /// <para>无变更的操作不触发通知：Add 重复元素、Remove 不存在的元素、Clear 空集合。</para>
    /// <para>AddRange / RemoveRange 逐项复用 Add / Remove，仅对实际变更的元素逐项触发通知。</para>
    /// </summary>
    public sealed class ObservableHashSetSample : MonoBehaviour
    {
        readonly ObservableHashSet<string> _onlinePlayers = new ObservableHashSet<string>();

        AutoRemoveListenerHandle _subscription;

        string _lastNameAdded;

        int _playerCounter;

        void Start()
        {
            PlayerOnline();
            PlayerOnline();
            PlayerOnline();
        }

        void OnEnable()
        {
            _subscription = _onlinePlayers.AddListener(OnPlayersChanged);
        }

        void OnDisable()
        {
            _subscription.Dispose();
        }

        /// <summary>
        /// 单轨回调：按 <see cref="CollectionChangedEventArgs{T}.Action" /> 区分变更类型。
        /// </summary>
        void OnPlayersChanged(CollectionChangedEventArgs<string> e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Debug.Log($"[HashSet] Add → {e.NewItem} 上线（当前 {_onlinePlayers.Count} 人）");
                    break;
                case NotifyCollectionChangedAction.Remove:
                    Debug.Log($"[HashSet] Remove → {e.OldItem} 下线（当前 {_onlinePlayers.Count} 人）");
                    break;
                case NotifyCollectionChangedAction.Reset:
                    Debug.Log("[HashSet] Reset → 在线列表已清空");
                    break;
            }
        }

        [ContextMenu("Add：玩家上线")]
        void PlayerOnline()
        {
            _lastNameAdded = $"玩家{_playerCounter++}";
            _onlinePlayers.Add(_lastNameAdded);
            DumpPlayers();
        }

        [ContextMenu("Add 重复玩家（不触发通知）")]
        void AddDuplicatePlayer()
        {
            if (_lastNameAdded == null)
            {
                Debug.LogWarning("[HashSet] 在线列表为空，先运行 Play Mode 或「Add：玩家上线」");
                return;
            }

            var added = _onlinePlayers.Add(_lastNameAdded);
            Debug.Log($"[HashSet] Add(\"{_lastNameAdded}\")（重复）→ 返回 {added}，通知未触发");
        }

        [ContextMenu("Remove：刚上线的玩家下线")]
        void PlayerOffline()
        {
            if (_lastNameAdded == null || !_onlinePlayers.Remove(_lastNameAdded))
            {
                Debug.LogWarning("[HashSet] 没有可下线的玩家，先运行 Play Mode 或「Add：玩家上线」");
                return;
            }

            DumpPlayers();
        }

        [ContextMenu("Remove 不存在的玩家（不触发通知）")]
        void RemoveMissingPlayer()
        {
            var removed = _onlinePlayers.Remove("不在线的玩家");
            Debug.Log($"[HashSet] Remove(\"不在线的玩家\") → 返回 {removed}，通知未触发");
        }

        [ContextMenu("AddRange：批量上线（逐项通知）")]
        void UnionPlayers()
        {
            _onlinePlayers.AddRange(new[] { "Alice", "Bob" });
            DumpPlayers();
        }

        [ContextMenu("RemoveRange：批量下线（逐项通知）")]
        void ExceptPlayers()
        {
            _onlinePlayers.RemoveRange(new[] { "Alice", "Bob" });
            DumpPlayers();
        }

        [ContextMenu("Clear：清空在线列表（Reset 通知）")]
        void ClearPlayers()
        {
            _onlinePlayers.Clear();
            Debug.Log($"[HashSet] Clear 完成，当前在线 {_onlinePlayers.Count} 人");
        }

        void DumpPlayers() =>
            Debug.Log($"[HashSet] 当前在线：[{string.Join(", ", _onlinePlayers)}]");
    }
}
#endif
